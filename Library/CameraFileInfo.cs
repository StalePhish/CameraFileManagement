using System.Diagnostics.CodeAnalysis;
using static CAndrews.CameraFileManagement.Enumerations;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Represents sources and destinations to copy files
/// </summary>
public class CameraFileInfo : NotifyPropertyChanged
{
    /// <summary>
    /// Creates a new instance given a source file
    /// </summary>
    /// <param name="sourceFileInfo">Source file</param>
    /// <param name="sourceDirectoryRoot">Directory path root to exclude for certain display contexts</param>
    /// <param name="camera">Camera settings</param>
    public CameraFileInfo(FileInfo sourceFileInfo, string sourceDirectoryRoot, CameraSetting camera)
    {
        _status = StatusType.Ready;
        _error = null;
        _sourceDirectoryRoot = sourceDirectoryRoot?.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).
            TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        _sourceDirectory = sourceFileInfo.DirectoryName;
        _sourceFileName = sourceFileInfo.Name;
        _size = (double)sourceFileInfo.Length / (1024 * 1024);

        _fileAttributes = FileAttribute.GetFileAttributes(sourceFileInfo.FullName);
        Camera = camera ?? _fileAttributes.GetCamera();

        CalculateDestination(sourceFileInfo);
    }

    /// <summary>
    /// Calculate the destination and selection options given the selected Camera alias
    /// </summary>
    /// <param name="sourceFileInfo">Source file. Null will do a lookup</param>
    public void CalculateDestination(FileInfo sourceFileInfo)
    {
        // Combine camera destination with formatted file name
        string destination = Path.Combine(Camera?.Destination ?? string.Empty, _fileAttributes.GetDestinationFileName(Camera));

        // Deconstruct the path path to directory and file name
        // because the formatted file name could have a directory prefix
        DestinationDirectory = Path.GetDirectoryName(destination);
        DestinationFileName = Path.GetFileName(destination);

        // Calculate default selected state
        CalculateSelectedState(sourceFileInfo);
        Move = Camera?.Move ?? false;
    }

    /// <summary>
    /// Calculate default selected state
    /// </summary>
    public void CalculateSelectedState(FileInfo sourceFileInfo = null)
    {
        sourceFileInfo ??= new FileInfo(Source);
        Selected = DestinationDirectory is not null
            && DestinationFileName is not null
            && !DestinationExists
            && (sourceFileInfo.IsReadOnly || (Camera?.Type != CameraType.Dashcam));
    }

    /// <summary>
    /// File attributes
    /// </summary>
    public List<FileAttribute> FileAttributes
    {
        get => _fileAttributes;
        set
        {
            _fileAttributes = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Camera settings
    /// </summary>
    public CameraSetting Camera
    {
        get => _camera;
        set
        {
            _camera = value;
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Alias));

            CalculateDestination(null);
        }
    }

    /// <summary>
    /// Selected for processing or not
    /// </summary>
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Copy status
    /// </summary>
    public StatusType Status
    {
        get => _status;
        set
        {
            _status = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Error message if applicable
    /// </summary>
    public string Error
    {
        get => _error;
        set
        {
            _error = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Camera alias for lookups
    /// </summary>
    public string Alias
    {
        get => Camera?.Alias;
        set
        {
            Camera = Settings.Instance.CameraSettings.SingleOrDefault(cs => cs.Alias == value);
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Source directory
    /// </summary>
    public string SourceDirectory => _sourceDirectory;

    /// <summary>
    /// Source file name
    /// </summary>
    public string SourceFileName => _sourceFileName;

    /// <summary>
    /// Absolute path to the source file
    /// </summary>
    public string Source => Path.Combine(SourceDirectory ?? string.Empty, SourceFileName);

    /// <summary>
    /// Relative path to the source file without the directory root prefix
    /// </summary>
    public string SourceNoRoot => Source.Replace(_sourceDirectoryRoot, null);

    /// <summary>
    /// Destination directory
    /// </summary>
    public string DestinationDirectory
    {
        get => _destinationDirectory;
        set
        {
            _destinationDirectory = value?.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Selected));
            RaisePropertyChanged(nameof(DestinationExists));
        }
    }

    /// <summary>
    /// Destination file name
    /// </summary>
    public string DestinationFileName
    {
        get => _destinationFileName;
        set
        {
            _destinationFileName = value?.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(Selected));
            RaisePropertyChanged(nameof(DestinationExists));
        }
    }

    /// <summary>
    /// Absolute path to teh destination file
    /// </summary>
    public string Destination => Path.Combine(DestinationDirectory ?? string.Empty, DestinationFileName);

    /// <summary>
    /// Does the destination file already exist
    /// </summary>
    public bool DestinationExists => File.Exists(Destination);

    /// <summary>
    /// Move the file or only copy it
    /// </summary>
    public bool Move
    {
        get => _move;
        set
        {
            _move = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// File size (megabytes)
    /// </summary>
    public double Size
    {
        get => _size;
        set
        {
            _size = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// List of all camera aliases
    /// </summary>
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Data binding")]
    public List<string> AllCameraAliases => Settings.Instance.AllCameraAliases();

    /// <summary>
    /// List of available camera aliases
    /// </summary>
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Data binding")]
    public List<string> AvailableCameraAliases => Settings.Instance.AvailableCameraAliases();

    #region Private Members

    private List<FileAttribute> _fileAttributes;
    private CameraSetting _camera;
    private bool _selected;
    private StatusType _status;
    private string _error;
    private readonly string _sourceDirectoryRoot;
    private readonly string _sourceDirectory;
    private readonly string _sourceFileName;
    private string _destinationDirectory;
    private string _destinationFileName;
    private bool _move;
    private double _size;

    #endregion Private Members
}
