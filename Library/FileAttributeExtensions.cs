using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Extension methods for <see cref="FileAttribute"/>
/// </summary>
public static class FileAttributeExtensions
{
    /// <summary>
    /// Extension method to gets an attribute's value given a file's attributes
    /// </summary>
    /// <param name="fileAttributes">List of file attributes</param>
    /// <param name="name">Attribute name</param>
    /// <returns>Attribute value</returns>
    public static string Value(this List<FileAttribute> fileAttributes, string name)
    {
        //return fileAttributes.SingleOrDefault(a => a.Name == name)?.Value;
        return fileAttributes.FirstOrDefault(a => a.Name == name)?.Value;
    }

    /// <summary>
    /// Adds a file attribute to the list
    /// </summary>
    /// <param name="fileAttributes">List of file attributes</param>
    /// <param name="name">Property name</param>
    /// <param name="rawValue">Raw property value</param>
    /// <param name="source">Source of the attribute</param>
    public static void AddFileAttribute(this List<FileAttribute> fileAttributes, string name, object rawValue, string source)
    {
        try
        {
            // Join lists into a comma-separated string
            string value;
            Type type = rawValue?.GetType();
            if (type?.IsArray == true)
            {
                value = string.Join("; ", (object[])rawValue);
            }
            else if (type?.IsGenericType == true)
            {
                value = string.Join("; ", (rawValue as IEnumerable<object>).ToList());
            }
            else
            {
                value = rawValue?.ToString() ?? string.Empty;
            }

            // Skip adding rows with blank values
            if (!string.IsNullOrWhiteSpace(value) && value != double.NaN.ToString())
            {
                fileAttributes.Add(new FileAttribute(name, value, source));
            }
        }
        catch
        {
            // Swallow exception
        }
    }

    /// <summary>
    /// Extension method to get the make and model given a file's attributes
    /// </summary>
    /// <param name="fileAttributes">List of file attributes</param>
    /// <param name="fallback">True to allow fallback calculations. False to only read stock file attributes.</param>
    /// <returns>Make and Model</returns>
    public static (string make, string model) GetMakeAndModel(this List<FileAttribute> fileAttributes, bool fallback = true)
    {
        // Pull the make and model out of the file attributes
        (string make, string model) makeAndModel = new()
        {
            make = fileAttributes.Value("Make") ?? fileAttributes.Value("Android Manufacturer"),
            model = fileAttributes.Value("Model") ?? fileAttributes.Value("Android Model")
        };

        // Fall back to reading an alias out of an existing filename
        if (fallback && string.IsNullOrWhiteSpace(makeAndModel.make) && string.IsNullOrWhiteSpace(makeAndModel.model))
        {
            var camera = Settings.Instance.CameraSettings.FirstOrDefault(cs => !string.IsNullOrWhiteSpace(cs.Alias) && fileAttributes.Value("FullName").Contains(cs.Alias));
            makeAndModel.make = camera?.Make;
            makeAndModel.model = camera?.Model;
        }

        // Fall back to reading them from CFM auxiliary data added during a previous copy
        if (fallback && string.IsNullOrWhiteSpace(makeAndModel.make) && string.IsNullOrWhiteSpace(makeAndModel.model))
        {
            try
            {
                var fileTag = TagLib.File.Create(fileAttributes.Value("FullName"));
                makeAndModel.make = fileTag.Tag.Performers.ElementAtOrDefault(0);
                makeAndModel.model = fileTag.Tag.Performers.ElementAtOrDefault(1);
            }
            catch (Exception ex)
            {
                // Ignore exceptions from TagLib
                if (ex is not TagLib.CorruptFileException && ex is not TagLib.UnsupportedFormatException && ex is not InvalidDataException)
                {
                    throw;
                }
            }
        }

        return makeAndModel;
    }

    /// <summary>
    /// Extension method to get the camera settings given a file's attributes
    /// </summary>
    /// <param name="fileAttributes">List of file attributes</param>
    /// <returns>Camera settings</returns>
    public static CameraSetting GetCamera(this List<FileAttribute> fileAttributes)
    {
        var (make, model) = fileAttributes.GetMakeAndModel();

        // TODO improve so entries with Make=value and Model=null won't return two result cameras for a Model that was never filled out
        return Settings.Instance.CameraSettings.SingleOrDefault(cs => (cs.Make == make || make is null) && cs.Model == model);
    }

    /// <summary>
    /// Extension method to get the destination file name given a file's attributes by looking up the camera settings
    /// </summary>
    /// <param name="fileAttributes">List of file attributes</param>
    /// <param name="camera">Camera settings</param>
    /// <returns>New filename</returns>
    public static string GetDestinationFileName(this List<FileAttribute> fileAttributes, CameraSetting camera)
    {
        if (camera is not null)
        {
            DateTime dateTime = DateTime.Now;
            string fileName = Path.GetFileNameWithoutExtension(fileAttributes.Value("Name"));

            // Strip out any existing date/time from the filename to re-use later.

            // Try to get date/time from Dropbox style filenames: Ring_FrontDoor_20241001_1653
            const string dropboxDatePattern = @"_*\d{4}-\d{2}-\d{2}[- ]\d{2}[-.]\d{2}[-.]\d{2}";
            if (Regex.Match(fileName, dropboxDatePattern) is Match dropboxDateMatch && dropboxDateMatch.Success &&
                DateTime.TryParseExact(dropboxDateMatch.Value, "yyyy-MM-dd HH.mm.ss",
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dateTime))
            {
                fileName = Regex.Replace(fileName, dropboxDatePattern, string.Empty).Trim();
            }

            // Try to get date/time from Ring newer style filenames
            else if (Regex.Match(fileName, @"(?<=_)\d{8}_\d{4}$") is Match ringDateMatch1 && ringDateMatch1.Success &&
                DateTime.TryParseExact(ringDateMatch1.Value, "yyyyMMdd_HHmm",
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dateTime))
            {
                // Success
            }

            // Try to get date/time from Ring older style filenames: [2018-12-06T142145]-Front Door-RingAccepted
            else if (Regex.Match(fileName, @"\d{4}-\d{2}-\d{2}T\d{6}") is Match ringDateMatch2 && ringDateMatch2.Success &&
                DateTime.TryParseExact(ringDateMatch2.Value, "yyyy-MM-ddTHHmmss",
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dateTime))
            {
                // Success
            }

            // Date/Time priority order to use for determining the relevant timestamp. Begin with the camera setting 
            // but if none were specified, fall back on the overall setting. Parse as a space-separated ordered list.
            else
            {   
                List<string> dateTimePriorities = [];

                // Use camera-specific DateTimePriority
                if (!string.IsNullOrWhiteSpace(camera.DateTimePriority))
                {
                    dateTimePriorities.AddRange(camera.DateTimePriority.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                }

                // Use overall DateTimePriority
                if (dateTimePriorities.Count == 0 && !string.IsNullOrWhiteSpace(Settings.Instance.DateTimePriority))
                {
                    dateTimePriorities.AddRange(Settings.Instance.DateTimePriority.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                }

                // Loop through the date/time priorities in order until one resolves
                foreach (string dateTimePriority in dateTimePriorities)
                {
                    if (DateTime.TryParse(fileAttributes.Value(dateTimePriority), out dateTime))
                    {
                        break;
                    }
                }
            }

            // Use the camera filename format to extract the original name
            var match = Regex.Match(fileName, camera?.Format ?? string.Empty);
            if (match.Groups.Count > 1)
            {
                fileName = string.Join(string.Empty, match.Groups.Cast<Group>().Skip(1).Select(g => g.Value));
            }
            else
            {
                fileName = match.Value;
            }

            // Create filename from: date + alias + original name + extension
            List<string> args =
                [
                dateTime.ToString(Settings.Instance.DateTimeFormat),
                camera?.Alias,
                fileName
                ];
            return string.Join(' ', args.Where(a => !string.IsNullOrWhiteSpace(a))).Trim() + fileAttributes.Value("Extension");
        }
        return fileAttributes.Value("Name");
    }

}
