using Newtonsoft.Json;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Extension methods for <see cref="Settings"/>
/// </summary>
[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Singleton does not use extension parameter")]
public static class SettingsExtensions
{
    /// <summary>
    /// Absolute file path to the user settings file
    /// </summary>
    public static string UserSettingsPath(this Settings extension)
    {
        var assemblyInfo = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(Settings)).Location);
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            assemblyInfo.CompanyName, assemblyInfo.ProductName, "user.cfm");
    }

    /// <summary>
    /// Load user settings from file
    /// </summary>
    public static void Load(this Settings extension)
    {
        if (File.Exists(Settings.Instance.UserSettingsPath()))
        {
            Settings.Instance.Import(Settings.Instance.UserSettingsPath());
        }
    }

    /// <summary>
    /// Save user settings to file
    /// </summary>
    public static void Save(this Settings extension) => Settings.Instance.Export(Settings.Instance.UserSettingsPath());

    /// <summary>
    /// Import settings from file
    /// </summary>
    /// <param name="path">Absolute path to read the settings file from</param>
    public static void Import(this Settings extension, string path)
    {
        // Deserialize
        Settings.Instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(path));

        // Sort the camera settings by alias
        List<CameraSetting> cameraSettings = [.. Settings.Instance.CameraSettings];
        Settings.Instance.CameraSettings.Clear();
        cameraSettings.OrderBy(cs => cs.Alias).OrderBy(cs => !cs.Enabled).ToList().ForEach(Settings.Instance.CameraSettings.Add);

        // Re-save the file
        Settings.Instance.Save();
    }

    /// <summary>
    /// Export settings to file
    /// </summary>
    /// <param name="path">Absolute path to save the settings file to</param>
    public static void Export(this Settings extension, string path)
    {
        // Update version
        Settings.Instance.Version = Settings.Instance.DefaultValue(nameof(Settings.Version));

        // Create settings directory if it doesn't exist yet
        if (!File.Exists(Settings.Instance.UserSettingsPath()))
        {
            Directory.CreateDirectory(Directory.GetParent(Settings.Instance.UserSettingsPath()).FullName);
        }

        // Serialize
        var contents = JsonConvert.SerializeObject(Settings.Instance, Formatting.Indented);
        using StreamWriter writer = new(path, false, Encoding.UTF8);
        writer.Write(contents);
    }

    /// <summary>
    /// Resets the specified setting to factory default value
    /// </summary>
    /// <param name="setting">Name of the setting</param>
    /// <returns>Factory default value</returns>
    public static string DefaultValue(this Settings extension, string setting)
    {
        switch (setting)
        {
            case nameof(Settings.Version):
                var assemblyInfo = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(Settings)).Location);
                return assemblyInfo.FileVersion;

            case nameof(Settings.DateTimeFormat):
                return $"yyyy{Path.AltDirectorySeparatorChar}yyyy-MM-dd HH.mm.ss";

            case nameof(Settings.DateTimePriority):
                return $"DateTime DateAcquired CreationTime LastAccessTime LastWriteTime";

            case nameof(Settings.Extensions):
                return "jpg jpeg gif png bmp dng cr2 gcs nrw nef tif webp avi avif mp4 mov mpv wlmp wmv xmp 3gpp";

            case nameof(Settings.DefaultSourceDirectory):
                return string.Empty;
        }

        throw new ArgumentException($"Setting {setting} has no default value.", nameof(setting));
    }

    /// <summary>
    /// List of all camera aliases
    /// </summary>
    /// <param name="setting">Name of the setting</param>
    /// <returns>List of aliases</returns>
    public static List<string> AllCameraAliases(this Settings extension)
    {
        return Settings.Instance.CameraSettings.Select(cs => cs.Alias).ToList();
    }

    /// <summary>
    /// List of camera aliases that are enabled in settings
    /// </summary>
    /// <param name="setting">Name of the setting</param>
    /// <returns>List of aliases</returns>
    public static List<string> AvailableCameraAliases(this Settings extension)
    {
        return Settings.Instance.CameraSettings.Where(cs => cs.Enabled).Select(cs => cs.Alias).ToList();
    }
}
