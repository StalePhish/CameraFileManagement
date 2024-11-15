using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using static CAndrews.CameraFileManagement.Enumerations;

namespace CAndrews.CameraFileManagement.Application.ViewModel;

/// <summary>
/// View Model for Camera Control
/// </summary>
internal class CameraViewModel : NotifyPropertyChanged
{
    /// <summary>
    /// Initialize the view model
    /// </summary>
    public void Initialize(ProgressViewModel progressVM)
    {
        // Hook up the progress view model
        _progressVM = progressVM;
        _cameraLoad.ProgressUpdate += _progressVM.UpdateProgress();
        _cameraCopy.ProgressUpdate += _progressVM.UpdateProgress();

        // Attempt to load from a removable drive
        LoadRemovableDrive();

        // Otherwise attempt to load the default source directory
        if (string.IsNullOrWhiteSpace(SourceDirectory))
        {
            SourceDirectory = Settings.Instance.DefaultSourceDirectory;
        }
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~CameraViewModel()
    {
        _cameraLoad.ProgressUpdate -= _progressVM.UpdateProgress();
        _cameraCopy.ProgressUpdate -= _progressVM.UpdateProgress();
    }

    /// <summary>
    /// Loads the removable drive as the default source directory if only one is ready
    /// </summary>
    private void LoadRemovableDrive()
    {
        var removableDrives = CameraCopy.GetRemovableDrives();
        SourceDirectory = removableDrives.FirstOrDefault()?.RootDirectory.FullName;
    }

    /// <summary>
    /// Loads the files from the source directory into the data grid
    /// </summary>
    private async void LoadFromSourceDirectory()
    {
        if (_progressVM is null)
        {
            throw new ArgumentNullException(nameof(_progressVM), "Progress View Model was not set which likely means CameraViewModel.Initialize was not called.");
        }

        IsBusy = true;

        CameraFiles.Clear();
        CameraAlias = null;

        if (SourceDirectory is not null)
        {
            List<CameraFileInfo> files = [];
            await Task.Run(() => files = _cameraLoad.GetCameraFilesFromDirectory(SourceDirectory, Subfolders, _cancellationToken.Token, out _isRemovableDrive));
            files.ForEach(CameraFiles.Add);

            if (IsRemovableDrive)
            {
                CameraAlias = CameraSetting.GetPersistedCamera(SourceDirectory);
                RaisePropertyChanged(nameof(CameraAlias));
            }
        }

        RaisePropertyChanged(nameof(AllCameraAliases));
        RaisePropertyChanged(nameof(AvailableCameraAliases));
        RaisePropertyChanged(nameof(CanPersist));

        IsBusy = false;
    }

    /// <summary>
    /// Runs the camera copy
    /// </summary>
    /// <param name="copyType">The copy type, whether it be based on each file's camera setting or override to move or copy all</param>
    private async void Run(CopyType copyType)
    {
        IsBusy = true;
        await Task.Run(() => _cameraCopy.Run([.. CameraFiles], copyType, OpenFoldersAfter, _cancellationToken.Token));
        IsBusy = false;
    }

    #region Properties

    /// <summary>
    /// The collection of directory information bound to the data grid
    /// </summary>
    public ObservableCollection<CameraFileInfo> CameraFiles { get; set; } = [];

    /// <summary>
    /// The source directory
    /// </summary>
    public string SourceDirectory
    {
        get => _sourceDirectory;
        set
        {
            _sourceDirectory = value;
            RaisePropertyChanged();

            LoadFromSourceDirectory();
        }
    }

    /// <summary>
    /// If the source directory is a removable drive
    /// </summary>
    public bool IsRemovableDrive
    {
        get => _isRemovableDrive;
        set
        {
            _isRemovableDrive = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Can the Persist button be clicked? Only when there is a selected camera, the files are on a removable drive, and it wasn't already saved
    /// </summary>
    public bool CanPersist => !string.IsNullOrEmpty(_selectedCamera?.Alias)
        && IsRemovableDrive && _selectedCamera.Alias != CameraSetting.GetPersistedCamera(SourceDirectory);

    /// <summary>
    /// Append suffix
    /// </summary>
    public string Suffix
    {
        get => _suffix;
        set
        {
            _suffix = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Include subfolders?
    /// </summary>
    public bool Subfolders
    {
        get => _subfolders;
        set
        {
            _subfolders = value;
            RaisePropertyChanged();

            LoadFromSourceDirectory();
        }
    }

    /// <summary>
    /// Open folders after processing?
    /// </summary>
    public bool OpenFoldersAfter { get; set; } = true;

    #endregion Properties

    #region Camera Aliasing

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

    /// <summary>
    /// Camera alias for the files. Setting this updates unknown files to specified camera alias.
    /// </summary>
    public string CameraAlias
    {
        get => _selectedCamera?.Alias;
        set
        {
            _selectedCamera = Settings.Instance.CameraSettings.SingleOrDefault(cs => cs.Alias == value);
            RaisePropertyChanged();
            RaisePropertyChanged(nameof(CanPersist));

            // Update camera files that had no alias to use the selected alias
            CameraFiles.Where(cf => string.IsNullOrWhiteSpace(cf.Camera?.Alias)).ToList().ForEach(cf => cf.Camera = _selectedCamera);
        }
    }

    #endregion Camera Aliasing

    #region Commands

    public RelayCommand LoadRemovableCommand => _loadRemovableCommand ??= new RelayCommand(
        param => LoadRemovableDrive());

    /// <summary>
    /// Command to reload/refresh the source directory
    /// </summary>
    public RelayCommand ReloadSourceCommand => _reloadSourceCommand ??= new RelayCommand(
        param => LoadFromSourceDirectory());

    /// <summary>
    /// Command to browse for a source directory
    /// </summary>
    public RelayCommand BrowseSourceCommand => _browseSourceCommand ??= new RelayCommand(
        param =>
        {
            if (Browse.BrowseForDirectory("Selected a source directory.", out string path, SourceDirectory))
            {
                SourceDirectory = path;
            }
        });

    /// <summary>
    /// Command to update unknown files to the selected camera
    /// </summary>
    public RelayCommand UpdateUnknownCommand => _updateUnknownCommand ??= new RelayCommand(
        param => CameraFiles.Where(cf => string.IsNullOrWhiteSpace(cf.Camera?.Alias)).ToList().ForEach(cf => cf.Camera = _selectedCamera),
        param => _selectedCamera is not null);

    /// <summary>
    /// Command to update all files to the selected camera
    /// </summary>
    public RelayCommand UpdateAllCommand => _updateAllCommand ??= new RelayCommand(
        param => CameraFiles.ToList().ForEach(cf => cf.Camera = _selectedCamera),
        param => _selectedCamera is not null);

    /// <summary>
    /// Command to persist the selected camera to disk
    /// </summary>
    public RelayCommand PersistCameraCommand => _persistCameraCommand ??= new RelayCommand(
        param => CameraSetting.SetPersistedCamera(_selectedCamera, SourceDirectory),
        param => CanPersist);

    /// <summary>
    /// Command to select all camera files
    /// </summary>
    public RelayCommand SelectAllCommand => _selectAllCommand ??= new RelayCommand(
        param => CameraFiles.ToList().ForEach(cf => cf.Selected = true));

    /// <summary>
    /// Command to deselect unknown camera files
    /// </summary>
    public RelayCommand DeselectUnknownCommand => _deselectUnknownCommand ??= new RelayCommand(
        param => CameraFiles.Where(cf => string.IsNullOrWhiteSpace(cf.Camera?.Alias)).ToList().ForEach(cf => cf.Selected = false));

    /// <summary>
    /// Command to append the suffix to the camera files
    /// </summary>
    public RelayCommand AppendSuffixCommand => _appendSuffixCommand ??= new RelayCommand(
        param => CameraFiles.ToList().ForEach(cf => cf.DestinationFileName = $"{string.Join(" ", Regex.Match(cf.DestinationFileName, @".*(?=\.)"), Suffix)}{Path.GetExtension(cf.Destination)}"));

    /// <summary>
    /// Command to run file transfer using individual move or copy settings per file
    /// </summary>
    public RelayCommand RunCommand => _runCommand ??= new RelayCommand(
        param => Run(CopyType.Run),
        param => CanRun);

    /// <summary>
    /// Command to run file transfer explicitly moving the files ignoring move/copy setting
    /// </summary>
    public RelayCommand MoveCommand => _moveCommand ??= new RelayCommand(
        param => Run(CopyType.Move),
        param => CanRun);

    /// <summary>
    /// Command to run file transfer explicitly copying the files ignoring move/copy setting
    /// </summary>
    public RelayCommand CopyCommand => _copyCommand ??= new RelayCommand(
        param => Run(CopyType.Copy),
        param => CanRun);

    /// <summary>
    /// Command to run file transfer explicitly renaming the files in place ignoring move/copy setting
    /// </summary>
    public RelayCommand RenameCommand => _renameCommand ??= new RelayCommand(
        param => Run(CopyType.Rename),
        param => CanRun);

    /// <summary>
    /// Command to run a demonstration of the file copy
    /// </summary>
    public RelayCommand DemoCommand => _demoCommand ??= new RelayCommand(
        param => Run(CopyType.Demonstration),
        param => CanRun);

    /// <summary>
    /// Command to cancel the file copy
    /// </summary>
    public RelayCommand CancelCommand => _cancelCommand ??= new RelayCommand(
        param => _cancellationToken.CancelAsync());

    #endregion Commands

    #region Busy Signaling

    /// <summary>
    /// True if the move/copy is running
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;

            // Reset the cancellation token
            if (value)
            {
                _cancellationToken = new();
            }
            else
            {
                _cancellationToken.Dispose();
            }

            RaisePropertyChanged();
            RaisePropertyChanged(nameof(CanRun));
            RaisePropertyChanged(nameof(IsNotBusy));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// Opposite of <see cref="IsBusy"/>
    /// </summary>
    public bool IsNotBusy => !IsBusy;

    /// <summary>
    /// Is the move/copy available to run?
    /// </summary>
    public bool CanRun => IsNotBusy && Directory.Exists(SourceDirectory);

    #endregion Busy Signaling

    #region Private Members

    private RelayCommand _loadRemovableCommand;
    private RelayCommand _reloadSourceCommand;
    private RelayCommand _browseSourceCommand;
    private RelayCommand _updateUnknownCommand;
    private RelayCommand _updateAllCommand;
    private RelayCommand _selectAllCommand;
    private RelayCommand _persistCameraCommand;
    private RelayCommand _deselectUnknownCommand;
    private RelayCommand _appendSuffixCommand;
    private RelayCommand _runCommand;
    private RelayCommand _moveCommand;
    private RelayCommand _copyCommand;
    private RelayCommand _renameCommand;
    private RelayCommand _demoCommand;
    private RelayCommand _cancelCommand;

    private readonly CameraLoad _cameraLoad = new();
    private readonly CameraCopy _cameraCopy = new();
    private CameraSetting _selectedCamera = new();
    private string _sourceDirectory;
    private bool _isRemovableDrive;
    private string _suffix;
    private bool _subfolders = true;
    private ProgressViewModel _progressVM;
    private CancellationTokenSource _cancellationToken = new();
    private bool _isBusy = false;

    #endregion Private Members
}
