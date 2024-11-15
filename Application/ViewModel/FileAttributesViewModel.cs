using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using static CAndrews.CameraFileManagement.Enumerations;

namespace CAndrews.CameraFileManagement.Application.ViewModel;

/// <summary>
/// View Model for File Attributes Control
/// </summary>
internal class FileAttributesViewModel : NotifyPropertyChanged
{
    /// <summary>
    /// The collection of file information bound to the data grid
    /// </summary>
    public ObservableCollection<FileAttribute> FileInformation { get; set; } = [];

    /// <summary>
    /// The selected file path
    /// </summary>
    public string Source
    {
        get => _source;
        set
        {
            _source = value;
            PopulateDataGrid();
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(AllCameraAliases));
        }
    }

    /// <summary>
    /// PopulateDataGrid the file attributes data grid
    /// </summary>
    private void PopulateDataGrid()
    {
        FileInformation.Clear();
        _cameraFileInfo = new(new FileInfo(Source), null, null);
        CameraAlias = _cameraFileInfo?.Alias;
        _cameraFileInfo.FileAttributes.ForEach(FileInformation.Add);
    }

    /// <summary>
    /// List of all camera aliases
    /// </summary>
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Data binding")]
    public List<string> AllCameraAliases => Settings.Instance.AllCameraAliases();

    /// <summary>
    /// The camera alias
    /// </summary>
    public string CameraAlias
    {
        get => _cameraFileInfo?.Alias;
        set
        {
            _cameraFileInfo.Alias = value;
            Destination = _cameraFileInfo.Destination;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// The destination file path to copy the file to
    /// </summary>
    public string Destination
    {
        get => _destination;
        set
        {
            _destination = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Command to browse for a file
    /// </summary>
    public RelayCommand BrowseCommand => _browseCommand ??= new RelayCommand(
        param =>
        {
            if (Browse.BrowserForFilePath("Selected an image file to load camera attributes.", out string path, Source))
            {
                Source = path;
            }
        });

    /// <summary>
    /// Command to copy the file
    /// </summary>
    public RelayCommand CopyCommand => _copyCommand ??= new RelayCommand(
        async param =>
        {
            CameraCopy cameraCopy = new();
            _cameraFileInfo.Selected = true;
            await Task.Run(() => cameraCopy.Run([_cameraFileInfo], CopyType.Copy, true, CancellationToken.None));
        },
        param => File.Exists(Source) && (DriveInfo.GetDrives().SingleOrDefault(d => d.Name == Path.GetPathRoot(Destination))?.IsReady ?? false));

    #region Private Members

    private RelayCommand _browseCommand;
    private RelayCommand _copyCommand;

    private CameraFileInfo _cameraFileInfo;
    private string _source;
    private string _destination;

    #endregion Private Members
}
