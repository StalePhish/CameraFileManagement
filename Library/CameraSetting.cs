using static CAndrews.CameraFileManagement.Enumerations;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Represents the settings for a supported camera
/// </summary>
public class CameraSetting() : NotifyPropertyChanged
{
    #region Properties

    /// <summary>
    /// Enabled state of the camera. False implies it is hidden from various menus
    /// </summary>
    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Camera type
    /// </summary>
    public CameraType Type
    {
        get => _type;
        set
        {
            _type = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Camera alias for lookups
    /// </summary>
    public string Alias
    {
        get => _alias;
        set
        {
            _alias = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Camera make / brand name
    /// </summary>
    public string Make
    {
        get => _make;
        set
        {
            _make = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Camera model name
    /// </summary>
    public string Model
    {
        get => _model;
        set
        {
            _model = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Destination directory to output files to
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
    /// Camera file name format
    /// </summary>
    public string Format
    {
        get => _format;
        set
        {
            _format = value;
            RaisePropertyChanged();
        }
    }

    /// <summary>
    /// Date/Time priority order to use for determining the relevant timestamp. Space-separated ordered list.
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

    #endregion Properties

    #region Utilities

    /// <summary>
    /// List of camera types
    /// </summary>
    public static List<string> CameraTypes => new(Enum.GetNames(typeof(CameraType)));

    /// <summary>
    /// Saves the persist file containing the camera alias
    /// </summary>
    /// <param name="camera">Camera settings</param>
    /// <param name="directory">Directory path</param>
    public static void SetPersistedCamera(CameraSetting camera, string directory)
    {
        string cfmFile = Path.Combine(Path.GetPathRoot(directory), "CFM");
        File.WriteAllText(cfmFile, camera.Alias);
    }

    /// <summary>
    /// Load the persist file containing the camera alias
    /// </summary>
    /// <param name="camera">Camera settings</param>
    /// <param name="directory">Directory path</param>
    /// <returns>Persisted camera alias</returns>
    public static string GetPersistedCamera(string directory)
    {
        string cfmFile = Path.Combine(Path.GetPathRoot(directory), "CFM");
        if (File.Exists(cfmFile))
        {
            return File.ReadAllText(cfmFile);
        }
        return null;
    }

    #endregion Utilities

    #region Private Members

    private bool _enabled;
    private CameraType _type;
    private string _alias;
    private string _make;
    private string _model;
    private string _destination;
    private string _dateTimePriority;
    private string _format;
    private bool _move = false;

    #endregion Private Members
}
