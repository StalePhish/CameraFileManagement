using static CAndrews.CameraFileManagement.Enumerations;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Runs the camera load feature
/// </summary>
public class CameraLoad : NotifyPropertyChanged
{
    /// <summary>
    /// Returns a list of <see cref="CameraFileInfo"/> from a given directory
    /// </summary>
    /// <param name="directoryPath">Absolute directory path containing files</param>
    /// <param name="recursive">Recursively process subdirectories in addition to target directory</param>
    /// <param name="cancel">Cancellation token</param>
    /// <param name="isRemovableDrive">Returns if the directory path is a removable drive</param>
    /// <returns>List of files</returns>
    public List<CameraFileInfo> GetCameraFilesFromDirectory(string directoryPath, bool recursive, CancellationToken cancel, out bool isRemovableDrive)
    {
        isRemovableDrive= false;
        List<CameraFileInfo> cameraFiles = [];
        try
        {
            // Default progress to to indeterminate until directory info can be loaded
            _progress.Value = 0;
            _progress.Indeterminate = true;
            _progress.Result = StatusType.Ready;
            _progress.Text = $"Loading {directoryPath}...";
            ProgressUpdate?.Invoke(_progress);
            double filesLoaded = 0;
            double filesFailed = 0;

            // Get list of all files in the directory, recursively if applicable.
            // Use the space-separated extensions setting as an inclusion filter.
            var directory = new DirectoryInfo(directoryPath);
            isRemovableDrive = DriveInfo.GetDrives().SingleOrDefault(d => d.Name == Path.GetPathRoot(directoryPath) && d.DriveType == DriveType.Removable) is not null;
            IEnumerable<FileInfo> files = [];
            Settings.Instance.Extensions.Split(' ').ToList().ForEach(ext => files = files.Union(directory.EnumerateFiles($"*.{ext}",
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)));            
            _progress.Indeterminate = false;

            if (files.Any())
            {
                // Reset progress now that the number of files to load is known
                _progress.Maximum = files.Count();
                _progress.Result = StatusType.Success;

                // Load the persistance data to see if this is a known camera
                CameraSetting camera = null;
                if (isRemovableDrive)
                {
                    camera = Settings.Instance.CameraSettings.SingleOrDefault(cs => cs.Alias == CameraSetting.GetPersistedCamera(directoryPath));
                }

                // Load each file to add it to the list, calculating which camera the file belongs to and the output settings
                foreach (var sourceFile in files)
                {
                    // Check for user cancellation at the beginning of each loop
                    cancel.ThrowIfCancellationRequested();

                    try
                    {
                        // The heavy lifting occurs inside the CameraFileInfo constructor
                        cameraFiles.Add(new(sourceFile, directoryPath, camera));
                        _progress.Text = $"Loaded {_progress.Value++} files from {directoryPath}";
                        filesLoaded++;
                    }
                    catch (Exception ex)
                    {
                        _progress.Text = $"{sourceFile.Name} failed: {ex.Message}";
                        filesFailed++;
                    }                    
                    ProgressUpdate?.Invoke(_progress);
                }

                // Directory is on a removable drive                
                if (isRemovableDrive)
                {
                    // Contains some files not associated with a known camera
                    var unknownFiles = cameraFiles.Where(file => string.IsNullOrWhiteSpace(file.Alias)).ToList();
                    if (unknownFiles.Count != cameraFiles.Count)
                    {
                        // If all the remaining files are from a single camera, assume the unknown files are too
                        var knownCamera = cameraFiles.Where(file => !string.IsNullOrWhiteSpace(file.Alias)).Select(file => file.Camera).Distinct().SingleOrDefault();
                        if (knownCamera is not null)
                        {
                            unknownFiles.ForEach(file => file.Camera = knownCamera);
                        }
                    }
                }
            }

            // Reset progress bar when loading has finished
            _progress.Result = StatusType.Success;
            _progress.Text = $"Loaded {filesLoaded} files {(filesFailed > 0 ? $"and {filesFailed} files failed" : "")} from {directoryPath}";
            _progress.Value = 0;
            _progress.Maximum = 1;
            ProgressUpdate?.Invoke(_progress);
        }

        catch (Exception ex)
        {
            // Ignore exceptions from TagLib
            if (ex.Source != "taglib-sharp")
            {
                _progress.Result = StatusType.Failed;
                _progress.Text = $"Loading failed: {ex.Message}";
            }

            // Catch the cancellation and set a cancel status
            else
            //if (ex is OperationCanceledException)
            {
                _progress.Result = StatusType.Canceled;
                _progress.Text = ex.Message;
            }
            
            _progress.Indeterminate = false;
            ProgressUpdate?.Invoke(_progress);
        }

        // Sort the pre-Selected items to the top
        cameraFiles.Sort((x,y) => y.Selected.CompareTo(x.Selected));

        return cameraFiles;
    }

    /// <summary>
    /// Progress update event
    /// </summary>
    public event Action<CameraProgress> ProgressUpdate;

    /// <summary>
    /// Progress tracking
    /// </summary>
    private readonly CameraProgress _progress = new();
}
