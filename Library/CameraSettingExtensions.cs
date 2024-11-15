using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Windows.Media.Media3D;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Extension methods for <see cref="CameraSetting"/>
/// </summary>
public static class CameraSettingExtensions
{
    /// <summary>
    /// Attempts to add a new camera given the make and model combination
    /// </summary>
    /// <param name="cameraSettings">Camera settings</param>
    /// <param name="path">Absolute path from which to add a camera based on its file attributes</param>
    [SuppressMessage("Performance", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "Easier maintenance with inline regex")]
    public static void TryAddCamera(this ObservableCollection<CameraSetting> cameraSettings, string path)
    {
        var fileAttributes = FileAttribute.GetFileAttributes(path);
        var (make, model) = fileAttributes.GetMakeAndModel();

        var match = Regex.Match(Path.GetFileNameWithoutExtension(path), "^(?<prefix>\\D*)(?<suffix>\\d*)$");

        if (model is not null && !cameraSettings.Any(c => c.Make == make && c.Model == model))
        {
            cameraSettings.Add(new CameraSetting()
            {
                Enabled = true,
                Type = Enumerations.CameraType.None,
                Alias = CreateCameraAlias(fileAttributes),
                Make = make,
                Model = model,
                Destination = null,
                Format = match.Success ? @$"({match.Groups["prefix"].Value}\d{{{match.Groups["suffix"].Value.Length}}})|.*" : null,
                DateTimePriority = null,
                Move = false,
            });
        }
    }

    /// <summary>
    /// Attempts to create an alias for a camera make and model
    /// </summary>
    /// <param name="fileAttributes">List of file attributes</param>
    /// <returns>Alias for the camera make and model</returns>
    [SuppressMessage("Performance", "SYSLIB1045:Convert to 'GeneratedRegexAttribute'.", Justification = "Easier maintenance with inline regex")]
    private static string CreateCameraAlias(List<FileAttribute> fileAttributes = null)
    {
        // Attempt to get the camera make and model from the file attributes
        var makeAndModel = fileAttributes.GetMakeAndModel();
        string make = makeAndModel.make.Split(' ').FirstOrDefault().ToUpper();
        string model = makeAndModel.model.ToUpper();
        string alias = null;

        // Parse known makes
        switch (make)
        {
            case "CANON":
                var canon = Regex.Match(model, @"SX\d+");
                if (canon.Success)
                {
                    alias = $"{make} {canon.Value}";
                }
                break;
            case "GOOGLE":
                var google = Regex.Matches(model, @"\d+\w");
                if (google.Count > 0)
                {
                    alias = $"{model.Split(' ').FirstOrDefault()} {google.Aggregate("", (current, match) => current + match.Value)}";
                }
                else
                {
                    alias = model;
                }
                break;
        }

        // Pull off the last token if the alias was still undetermined
        return alias ?? $"{make} {model?.Split(' ').LastOrDefault()}";
    }
}
