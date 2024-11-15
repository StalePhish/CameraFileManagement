using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using System.Windows.Threading;

namespace CAndrews.CameraFileManagement.Application.ViewModel;

/// <summary>
/// View Model for Settings Control
/// </summary>
internal class SettingsViewModel : NotifyPropertyChanged
{
    /// <summary>
    /// Save settings
    /// </summary>
    public void SaveSettings()
    {
        // Temporarily set the background color of the save button to green
        SaveIconColor = new SolidColorBrush(Colors.Green);

        // Perform the actual save function
        Settings.Instance.Save();

        // Reset the background color of the save button after 1 second
        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (sender, args) =>
        {
            SaveIconColor = null;
            timer.Stop();
        };
        timer.Start();
    }

    /// <summary>
    /// Color of the Save icon
    /// </summary>
    public SolidColorBrush SaveIconColor
    {
        get => _saveIconColor;
        set
        {
            _saveIconColor = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Date/Time format
    /// </summary>
    public string DateTimeFormat
    {
        get => Settings.Instance.DateTimeFormat;
        set
        {
            Settings.Instance.DateTimeFormat = value;
            RaisePropertyChanged();
            SaveSettings();
        }
    }

    /// <summary>
    /// Date/Time priority
    /// </summary>
    public string DateTimePriority
    {
        get => Settings.Instance.DateTimePriority;
        set
        {
            Settings.Instance.DateTimePriority = value;
            RaisePropertyChanged();
            SaveSettings();
        }
    }

    /// <summary>
    /// Supported file extensions
    /// </summary>
    public string Extensions
    {
        get => Settings.Instance.Extensions;
        set
        {
            Settings.Instance.Extensions = value;
            RaisePropertyChanged();
            SaveSettings();
        }
    }

    /// <summary>
    /// Default source directory
    /// </summary>
    public string DefaultSourceDirectory
    {
        get => Settings.Instance.DefaultSourceDirectory;
        set
        {
            Settings.Instance.DefaultSourceDirectory = value;
            RaisePropertyChanged();
            SaveSettings();
        }
    }

    /// <summary>
    /// The collection of camera settings bound to the data grid
    /// </summary>
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Wrapper")]
    public ObservableCollection<CameraSetting> CameraSettings => Settings.Instance.CameraSettings;

    /// <summary>
    /// Command to reset the Date/Time format to factory default
    /// </summary>
    public RelayCommand ResetDateTimeFormatCommand => _resetDateTimeFormatCommand ??= new RelayCommand(
        param =>
        {
            DateTimeFormat = Settings.Instance.DefaultValue(nameof(Settings.DateTimeFormat));
            SaveSettings();
        });

    /// <summary>
    /// Command to reset the Date/Time priority to factory default
    /// </summary>
    public RelayCommand ResetDateTimePriorityCommand => _resetDateTimePriorityCommand ??= new RelayCommand(
        param =>
        {
            DateTimePriority = Settings.Instance.DefaultValue(nameof(Settings.DateTimePriority));
            SaveSettings();
        });

    /// <summary>
    /// Command to reset the supported file extensions to factory default
    /// </summary>
    public RelayCommand ResetExtensionsCommand => _resetExtensionsCommand ??= new RelayCommand(
        param =>
        {
            Extensions = Settings.Instance.DefaultValue(nameof(Settings.Extensions));
            SaveSettings();
        });

    /// <summary>
    /// Command to import a new settings file
    /// </summary>
    public RelayCommand ImportCommand => _importCommand ??= new RelayCommand(
        param =>
        {
            if (Browse.BrowserForFilePath("Import CFM settings", out string filePath, 
                filter: "Camera File Management Settings (*.cfm)|*.cfm"))
            {
                Settings.Instance.Import(filePath);
                RaisePropertyChanged(nameof(DateTimeFormat));
                RaisePropertyChanged(nameof(CameraSettings));
            }
        });

    /// <summary>
    /// Command to export the edited settings file
    /// </summary>
    public RelayCommand ExportCommand => _exportCommand ??= new RelayCommand(
        param =>
        {
            if (Browse.BrowserForFilePath("Export CFM settings", out string filePath, 
                filter: "Camera File Management Settings (*.cfm)|*.cfm", 
                fileName: "settings.cfm", mustExist: false))
            {
                Settings.Instance.Export(filePath);
            }
        });

    /// <summary>
    /// Command to add a new camera
    /// </summary>
    public RelayCommand AddCommand => _addCommand ??= new RelayCommand(
        param =>
        {
            // Add a camera with settings from an existing file
            if (Browse.BrowseForFilePaths("Selected an image file to load camera attributes", out string[] paths))
            {
                paths.ToList().ForEach(path => Settings.Instance.CameraSettings.TryAddCamera(path));
            }

            // Add a blank camera
            else
            {
                CameraSettings.Add(new());
            }

            SaveSettings();
        });

    /// <summary>
    /// Command to reload/refresh the default source directory
    /// </summary>
    public RelayCommand ReloadDefaultSourceDirectoryCommand => _reloadDefaultSourceDirectoryCommand ??= new RelayCommand(
        param => DefaultSourceDirectory = Settings.Instance.DefaultValue(nameof(Settings.DefaultSourceDirectory)));

    /// <summary>
    /// Command to browse for a default source directory
    /// </summary>
    public RelayCommand BrowseDefaultSourceDirectoryCommand => _browseDefaultSourceDirectoryCommand ??= new RelayCommand(
        param =>
        {
            if (Browse.BrowseForDirectory("Selected a default source directory.", out string path, DefaultSourceDirectory))
            {
                DefaultSourceDirectory = path;
            }
        });

    #region Private Members

    private SolidColorBrush _saveIconColor;
    private RelayCommand _resetDateTimeFormatCommand;
    private RelayCommand _resetDateTimePriorityCommand;
    private RelayCommand _resetExtensionsCommand;
    private RelayCommand _importCommand;
    private RelayCommand _exportCommand;
    private RelayCommand _addCommand;
    private RelayCommand _reloadDefaultSourceDirectoryCommand;
    private RelayCommand _browseDefaultSourceDirectoryCommand;

    #endregion Private Members
}
