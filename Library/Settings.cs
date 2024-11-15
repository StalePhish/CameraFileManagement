using System.Collections.ObjectModel;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Settings values
/// </summary>
[Serializable]
public sealed class Settings : NotifyPropertyChanged
{
    /// <summary>
    /// Singleton instance
    /// </summary>
    public static Settings Instance { get; internal set; } = new Settings();

    /// <summary>
    /// Constructor
    /// </summary>
    public Settings()
    {
        Version = this.DefaultValue(nameof(Version));
        DateTimeFormat = this.DefaultValue(nameof(DateTimeFormat));
        DateTimePriority = this.DefaultValue(nameof(DateTimePriority));
        Extensions = this.DefaultValue(nameof(Extensions));

        CameraSettings = [];
    }

    /// <summary>
    /// Version number of the app the settings were last loaded in
    /// </summary>
    public string Version
    {
        get => _version;
        set
        {
            _version = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Date/Time format to use while renaming files. See <see cref="DateTime.ToString()"/>
    /// </summary>
    public string DateTimeFormat
    {
        get => _dateTimeFormat;
        set
        {
            _dateTimeFormat = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Date/Time priority order to use for determining the relevant timestamp. Space-separated ordered list.
    /// Can be overridden on a per-camera basis with <see cref="CameraSetting.DateTimePriority"/>.
    /// </summary>
    public string DateTimePriority
    {
        get => _dateTimePriority;
        set
        {
            _dateTimePriority = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Supported file extensions
    /// </summary>
    public string Extensions
    {
        get => _extensions;
        set
        {
            _extensions = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Default source directory
    /// </summary>
    public string DefaultSourceDirectory
    {
        get => _defaultSourceDirectory;
        set
        {
            _defaultSourceDirectory = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Camera settings
    /// </summary>
    public ObservableCollection<CameraSetting> CameraSettings { get; set; }

    #region Private Members

    private string _version;
    private string _dateTimeFormat;
    private string _dateTimePriority;
    private string _extensions;
    private string _defaultSourceDirectory;

    #endregion Private Members
}