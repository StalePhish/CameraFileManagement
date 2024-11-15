using System.CommandLine;
using System.Reflection;

namespace CAndrews.CameraFileManagement.CLI;

internal static partial class Commands
{
    /// <summary>
    /// Creates a command for managing settings
    /// </summary>
    internal static Command SettingsCommand()
    {
        Argument<string> pathArgument = new(
            name: "path",
            description: "Absolute file path",
            parse: arg =>
            {
                string path = arg.Tokens.Single().Value;
                if (!System.IO.File.Exists(path))
                {
                    arg.ErrorMessage = "File does not exist";
                    return null;
                }
                return path;
            });

        // settings list
        Command displayCommand = new(
            name: "list",
            description: "Displays settings");
        displayCommand.SetHandler(DisplaySettings);

        // settings import <path>
        Command importCommand = new(
            name: "import",
            description: "Import settings from file")
        {
            pathArgument
        };
        importCommand.SetHandler((path) =>
        {
            Settings.Instance.Import(path);
            Console.WriteLine($"Imported settings from {path}");
            DisplaySettings();
        },
        pathArgument);

        // settings export <path>
        Command exportCommand = new(
            name: "export",
            description: "Export settings to file")
        {
            pathArgument
        };
        exportCommand.SetHandler((path) =>
        {
            Settings.Instance.Export(path);
            Console.WriteLine($"Exported settings to {path}");
            DisplaySettings();
        },
        pathArgument);

        // <value>
        Argument<string> valueArgument = new(
            name: "value",
            description: "The value to set",
            parse: arg =>
            {
                return arg.Tokens.Single().Value;
            });

        // settings set-format <value>
        Command setDateTimeFormatCommand = new(
            name: "set-format",
            description: "Sets the Date/Time format")
        {
            valueArgument
        };
        setDateTimeFormatCommand.SetHandler((value) =>
        {
            Settings.Instance.DateTimeFormat = value;
            Settings.Instance.Save();
            DisplaySettings();
        },
        valueArgument);

        // settings reset-format
        Command resetDateTimeFormatCommand = new(
            name: "reset-format",
            description: "Resets the Date/Time format to factory default");
        resetDateTimeFormatCommand.SetHandler(() =>
        {
            Settings.Instance.DateTimeFormat = Settings.Instance.DefaultValue(nameof(Settings.DateTimeFormat));
            Settings.Instance.Save();
            DisplaySettings();
        });

        // settings set-extensions <value>
        Command setExtensionsCommand = new(
            name: "set-extensions",
            description: "Sets the Date/Time format")
        {
            valueArgument
        };
        setExtensionsCommand.SetHandler((value) =>
        {
            Settings.Instance.Extensions = value;
            Settings.Instance.Save();
            DisplaySettings();
        },
        valueArgument);

        // settings reset-extensions
        Command resetExtensionsCommand = new(
            name: "reset-extensions",
            description: "Resets the supported file extensions to factory default");
        resetExtensionsCommand.SetHandler(() =>
        {
            Settings.Instance.Extensions = Settings.Instance.DefaultValue(nameof(Settings.Extensions));
            Settings.Instance.Save();
            DisplaySettings();
        });

        // settings set-default-source-directory <value>
        Command setDefaultSourceDirectoryCommand = new(
            name: "set-default-source-directory",
            description: "Sets the default source directory")
        {
            valueArgument
        };
        setDefaultSourceDirectoryCommand.SetHandler((value) =>
        {
            Settings.Instance.DefaultSourceDirectory = value;
            Settings.Instance.Save();
            DisplaySettings();
        },
        valueArgument);

        // settings reset-extensions
        Command resetDefaultSourceDirectoryCommand = new(
            name: "reset-default-source-directory",
            description: "Resets the default source directory to factory default");
        resetDefaultSourceDirectoryCommand.SetHandler(() =>
        {
            Settings.Instance.DefaultSourceDirectory = Settings.Instance.DefaultValue(nameof(Settings.DefaultSourceDirectory));
            Settings.Instance.Save();
            DisplaySettings();
        });

        // settings add <path>
        Command addCommand = new(
            name: "add",
            description: "Adds a new camera from a sample file")
        {
            pathArgument
        };
        addCommand.SetHandler((path) =>
        {
            Settings.Instance.CameraSettings.TryAddCamera(path);
            Settings.Instance.Save();
            DisplaySettings();
        },
        pathArgument);

        Argument<CameraSetting> cameraArgument = new(
            name: "camera|index",
            description: "The camera or index",
            parse: arg =>
            {
                if (int.TryParse(arg.Tokens.Single().Value, out int index))
                {
                    return Settings.Instance.CameraSettings[index - 1];
                }
                else
                {
                    return Settings.Instance.CameraSettings.Single(cs => cs.Alias == arg.Tokens.Single().Value);
                }
            });

        // settings delete <camera>|<index>
        Command deleteCommand = new(
            name: "delete",
            description: "Deletes a camera")
        {
            cameraArgument
        };
        deleteCommand.SetHandler((camera) =>
        {
            Settings.Instance.CameraSettings.Remove(camera);
            Settings.Instance.Save();
            DisplaySettings();
        },
        cameraArgument);

        // settings set <camera>|<index> <property> <value>
        Argument<PropertyInfo> propertyArgument = new(
            name: "property",
            description: "The property name",
            parse: arg =>
            {
                var props = typeof(CameraSetting).GetProperties();
                var prop = props.SingleOrDefault(prop => prop.Name == arg.Tokens.Single().Value);
                if (prop is null)
                {
                    Console.WriteLine($"Invalid property: {arg.Tokens.Single().Value}");
                    Console.WriteLine("Available properties:");
                    props.ToList().ForEach(p => Console.WriteLine($"- {p.Name}"));
                    return null;
                }
                return prop;
            });
        Command setCommand = new(
            name: "set",
            description: "Sets a property's value")
        {
            cameraArgument,
            propertyArgument,
            valueArgument
        };
        setCommand.SetHandler((camera, property, value) =>
        {
            if (property is not null)
            {
                if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                {
                    property.SetValue(camera, bool.Parse(value));
                }
                else
                {
                    property.SetValue(camera, value);
                }
                Settings.Instance.Save();
                DisplaySettings();
            }
        },
        cameraArgument,
        propertyArgument,
        valueArgument);

        // settings
        Command settingsCommand = new(
            name: "settings",
            description: "Manages settings");
        settingsCommand.AddCommand(displayCommand);
        settingsCommand.AddCommand(importCommand);
        settingsCommand.AddCommand(exportCommand);
        settingsCommand.AddCommand(setDateTimeFormatCommand);
        settingsCommand.AddCommand(resetDateTimeFormatCommand);
        settingsCommand.AddCommand(setExtensionsCommand);
        settingsCommand.AddCommand(resetExtensionsCommand);
        settingsCommand.AddCommand(setDefaultSourceDirectoryCommand);
        settingsCommand.AddCommand(resetDefaultSourceDirectoryCommand);
        settingsCommand.AddCommand(addCommand);
        settingsCommand.AddCommand(deleteCommand);
        settingsCommand.AddCommand(setCommand);

        return settingsCommand;
    }

    /// <summary>
    /// Helper to display settings state
    /// </summary>
    private static void DisplaySettings()
    {
        // Set up the columns and widths
        List<(string column, int size)> columns =
        [
            ("Index", 0),
            ("Enabled", 0),
            ("Camera Type", Settings.Instance.CameraSettings.Max(cs => cs.Type.ToString()?.Length) ?? 0),
            ("Camera Alias", Settings.Instance.CameraSettings.Max(cs => cs.Alias?.Length) ?? 0),
            ("Make", Settings.Instance.CameraSettings.Max(cs => cs.Make?.Length) ?? 0),
            ("Model", Settings.Instance.CameraSettings.Max(cs => cs.Model?.Length) ?? 0),
            ("Destination Directory", Settings.Instance.CameraSettings.Max(cs => cs.Destination?.Length) ?? 0),
            ("Format", Settings.Instance.CameraSettings.Max(cs => cs.Format?.Length) ?? 0),
            ("Date/Time Priority", Settings.Instance.CameraSettings.Max(cs => cs.DateTimePriority?.Length) ?? 0),
            ("Move (or Copy)", 0),
        ];
        columns = columns.Select(col => (col.column, int.Max(col.column.Length, col.size))).ToList();

        // Generate the table format and flower boxes
        string tableFormat = $"| {string.Join(" | ", columns.Select((col, index) => $"{{{index},-{col.size}}}"))} |";
        string header = string.Format(tableFormat, columns.Select(c => c.column).ToArray());
        string seperator(string left, string middle, string right) => $"{left}{string.Join(middle, columns.Select(col => new string('─', col.size + 2)))}{right}";
        string info(string info) => $"| {info}{new string(' ', header.Length - info.Length - 3)}|";

        // Extra information
        string userSettingsLine = $"User Settings File: {Settings.Instance.UserSettingsPath()}";
        string dateTimeFormatLine = $"Date/Time Format: {Settings.Instance.DateTimeFormat} ({DateTime.Now.ToString(Settings.Instance.DateTimeFormat)})";
        string dateTimePriorityLine = $"Date/Time Priority: {Settings.Instance.DateTimePriority}";
        string extensionsLine = $"Supported Extensions: {Settings.Instance.Extensions}";
        string defaultSourceDirectoryLine = $"Default Source Directory: {Settings.Instance.DefaultSourceDirectory}";
        string versionLine = $"Version: {Settings.Instance.Version}";

        // Write out the about information to console
        Console.WriteLine(seperator("┌", "─", "┐"));
        Console.WriteLine(info(userSettingsLine));
        Console.WriteLine(info(dateTimeFormatLine));
        Console.WriteLine(info(dateTimePriorityLine));
        Console.WriteLine(info(extensionsLine));
        Console.WriteLine(info(defaultSourceDirectoryLine));
        Console.WriteLine(info(versionLine));
        Console.WriteLine(seperator("├", "┬", "┤"));
        Console.WriteLine(header);
        Console.WriteLine(seperator("|", "┼", "|"));
        int index = 0;
        Settings.Instance.CameraSettings.ToList().ForEach(cs => Console.WriteLine(string.Format(tableFormat,
            ++index, cs.Enabled, cs.Type.ToString(), cs.Alias, cs.Make, cs.Model, cs.Destination, cs.Format, cs.DateTimePriority,
            cs.Move ? "Move" : "Copy")));
        Console.WriteLine(seperator("└", "┴", "┘"));
    }
}
