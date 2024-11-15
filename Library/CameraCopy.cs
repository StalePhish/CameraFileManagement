using System.ComponentModel;
using System.Diagnostics;
using UsbEject;
using static CAndrews.CameraFileManagement.Enumerations;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Runs the camera copy feature
/// </summary>
public class CameraCopy : NotifyPropertyChanged
{
    /// <summary>
    /// Returns a list of <see cref="DriveInfo"/> from the system
    /// </summary>
    /// <returns></returns>
    public static List<DriveInfo> GetRemovableDrives()
    {
        return DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable && d.IsReady).ToList();
    }

    /// <summary>
    /// Run the camera copy
    /// </summary>
    /// <param name="cameraFiles">List of <see cref="CameraFileInfo"/></param>
    /// <param name="copyType">The copy type, whether it be based on each file's camera setting or override to move or copy all</param>
    /// <param name="openFoldersAfter">Open the output folders automatically, or not</param>
    /// <param name="cancel">Cancellation token</param>
    /// <returns></returns>
    public async Task Run(List<CameraFileInfo> cameraFiles, CopyType copyType, bool openFoldersAfter, CancellationToken cancel)
    {
        // Progress initialization
        _progress.Value = 0;
        _progress.Maximum = double.Max(0.001, cameraFiles.Where(cs => cs.Selected).Select(cs => cs.Size).Sum());
        _progress.Result = StatusType.Ready;
        _progress.Text = $"Starting copy from {cameraFiles.FirstOrDefault()?.SourceDirectory}";
        ProgressUpdate?.Invoke(_progress);

        #region Final Copied Text
        int filesMoved = 0;
        int filesCopied = 0;
        int filesRenamed = 0;
        int filesFailed = 0;
        int filesSkipped = 0;
        string finalCopiedText()
        {
            string text = string.Empty;
            if (filesMoved > 0)
            {
                text += $"Moved {filesMoved} files. ";
            }
            if (filesCopied > 0)
            {
                text += $"Copied {filesCopied} files. ";
            }
            if (filesRenamed > 0)
            {
                text += $"Renamed {filesRenamed} files. ";
            }
            if (filesFailed > 0)
            {
                text += $"Failed {filesFailed} files. ";
            }
            if (filesSkipped > 0)
            {
                text += $"Skipped {filesSkipped} files.";
            }
            return text;
        }
        #endregion Final Copied Text

        // Keep a list of successful file copies so we can open the output directories afterwards
        Dictionary<string, HashSet<string>> successfulCopies = [];

        // Get a list of removable drives
        HashSet<DriveInfo> ejectDrives = [];
        var removableDrives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Removable);

        try
        {
            // Process the list of files sequentially
            foreach (CameraFileInfo file in cameraFiles)
            {
                // Check for user cancellation at the beginning of each loop
                cancel.ThrowIfCancellationRequested();

                // Only try to copy files that are selected (Selected checkbox is ticked)
                if (file.Selected)
                {
                    try
                    {
                        file.Status = StatusType.Processing;
                        var (make, model) = file.FileAttributes.GetMakeAndModel();

                        // Destination is either the full destination or a relative path against the source
                        var target = Path.IsPathRooted(file.Destination) ? file.Destination : Path.Combine(file.SourceDirectory, file.Destination);

                        // Check for duplicate to prevent accidentally overwriting another file in the same run
                        if (successfulCopies.TryGetValue(Path.GetDirectoryName(file.Destination), out var copiedFiles) && 
                            copiedFiles.Any(f => f == Path.GetFileName(file.Destination)))
                        {
                            throw new ArgumentException("Duplicate");
                        }

                        // Demonstration copy
                        if (copyType == CopyType.Demonstration)
                        {
                            await Task.Run(() => Thread.Sleep((int)(file.Size * 100)), cancel);
                            filesCopied++;
                            _progress.Text = $"{_progress.MegabyteText} - Pretending to copy {file.SourceFileName} to {file.Destination}";
                        }

                        // Rename
                        else if (copyType == CopyType.Rename)
                        {
                            target = Path.Combine(file.SourceDirectory, file.DestinationFileName);
                            await Task.Run(() => File.Move(file.Source, target, false), cancel);
                            filesRenamed++;
                            _progress.Text = $"{_progress.MegabyteText} - Renamed {file.SourceFileName} to {Path.GetFileName(target)}";
                        }

                        // Move
                        else if ((copyType == CopyType.Run && file.Move) || copyType == CopyType.Move)
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(target));
                            await Task.Run(() => File.Move(file.Source, target, true), cancel);
                            filesMoved++;
                            _progress.Text = $"{_progress.MegabyteText} - Moved {file.SourceFileName} to {target}";
                        }

                        // Copy
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(target));
                            await Task.Run(() => File.Copy(file.Source, target, true), cancel);
                            filesCopied++;
                            _progress.Text = $"{_progress.MegabyteText} - Copied {file.SourceFileName} to {target}";
                        }

                        if (copyType != CopyType.Demonstration)
                        {
                            // Summarize a list of which cameras need to be ejected
                            if (file.Camera.Type is CameraType.Camera or CameraType.Drone)
                            {
                                DriveInfo drive = removableDrives.SingleOrDefault(d => d.Name == Directory.GetDirectoryRoot(file.SourceDirectory));
                                if (drive is not null)
                                {
                                    ejectDrives.Add(drive);
                                }
                            }

                            // Test that the copy succeeded, and then remove any read-only bit
                            FileInfo destinationFile = new(target)
                            {
                                IsReadOnly = false
                            };
                            file.RaisePropertyChanged(nameof(CameraFileInfo.DestinationExists));

                            // Try to modify the file attributes to write the camera make and model into CFM auxiliary data
                            if (string.IsNullOrWhiteSpace(make) && string.IsNullOrWhiteSpace(model))
                            {
                                try
                                {
                                    var fileTag = TagLib.File.Create(target);
                                    fileTag.Tag.Performers = [file.Camera.Make, file.Camera.Model];
                                    fileTag.Save();
                                }
                                catch (Exception ex)
                                {
                                    // Ignore exceptions from TagLib
                                    if (ex is not TagLib.CorruptFileException && ex is not TagLib.UnsupportedFormatException)
                                    {
                                        throw;
                                    }
                                }
                            }
                        }

                        // Update status after successful copy
                        file.Selected = false;
                        file.Status = StatusType.Success;
                        file.Error = null;
                        _progress.Result = StatusType.Success;
                        successfulCopies.TryAdd(Path.GetDirectoryName(target), []);
                        successfulCopies[Path.GetDirectoryName(target)].Add(Path.GetFileName(target));
                    }

                    // Catch the cancellation and break out of the loop
                    catch (OperationCanceledException)
                    {
                        filesSkipped++;
                        file.Status = StatusType.Ready;
                        throw;
                    }

                    // Catch individual file copy fail but to not stop the entire copy
                    catch (Exception ex)
                    {
                        filesFailed++;
                        file.Status = StatusType.Failed;
                        file.Error = ex.Message;
                        _progress.Result = StatusType.Failed;
                        _progress.Text = ex.Message;
                    }

                    // Increment progress even if the copy was a failure
                    _progress.Value += file.Size;
                }

                // Skip updating status if it was already a success
                else if (file.Status != StatusType.Success)
                {
                    filesSkipped++;
                    file.Status = StatusType.Skipped;
                    file.Error = null;
                }

                // Progress event for the end of each file copy
                ProgressUpdate?.Invoke(_progress);
            }

            _progress.Result = StatusType.Ready;

            // Final status
            _progress.Text = $"{_progress.MegabyteText} - {finalCopiedText()}";
            _progress.Value = 0;
        }

        // Catch the cancellation and set a cancel status
        catch (OperationCanceledException ex)
        {
            _progress.Result = StatusType.Canceled;
            _progress.Text = $"{ex.Message} - {finalCopiedText()}";
        }

        // Catch any other fatal error
        catch (Exception ex)
        {
            _progress.Result = StatusType.Failed;
            _progress.Text = $"{ex.Message} - {finalCopiedText()}";
        }

        ProgressUpdate?.Invoke(_progress);

        // Eject drives
        ejectDrives.ToList().ForEach(Eject);

        // Open the output folders automatically
        if (openFoldersAfter && (filesCopied > 0 || filesMoved > 0))
        {
            OpenFoldersAfter(successfulCopies);
        }
    }

    /// <summary>
    /// Eject drives
    /// </summary>
    /// <param name="drive">Drive to eject</param>
    private void Eject(DriveInfo drive)
    {
        using VolumeDeviceClass volumeDevice = new();
        Volume volume = volumeDevice.SingleOrDefault(v => drive.Name[..2] == v.LogicalDrive);
        if (volume is not null)
        {
            _progress.Text = $"Ejected {drive.VolumeLabel} ({drive.Name})";
            volume.Eject(false);
            ProgressUpdate?.Invoke(_progress);
        }
    }

    /// <summary>
    /// Open the output folders automatically
    /// </summary>
    private static void OpenFoldersAfter(Dictionary<string, HashSet<string>> successfullyCopiedFiles)
    {
        // Open each folder separately with all of the files copied in each selected at once
        foreach (var copied in successfullyCopiedFiles)
        {
            try
            {
                Process.Start("OpenFolderAndSelect.exe", $"\"{copied.Key}\" " +
                    $"{string.Join(" ", copied.Value.Select(file => $"\"{file}\""))}");
            }
            catch (Win32Exception ex)
            {
                // Re-try with only the first image if the failure was because too many files were selected
                if (ex.Message.StartsWith("An error occurred trying to start process") && ex.Message.EndsWith("The filename or extension is too long."))
                {
                    Process.Start("OpenFolderAndSelect.exe", $"\"{copied.Key}\" \"{copied.Value.FirstOrDefault()}\"");
                }
            }
        }
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
