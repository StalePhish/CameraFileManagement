using System.Diagnostics;
using System.Reflection;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Library for Help>About information
/// </summary>
public static class About
{
    /// <summary>
    /// Gets the Help>About information
    /// </summary>
    /// <returns></returns>
    public static List<Tuple<string, string>> GetAboutInfo()
    {
        var assemblyInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        var taglibInfo = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(TagLib.File)).Location);
        var newtonsoftInfo = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(Newtonsoft.Json.JsonConvert)).Location);
        var commandLineInfo = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(System.CommandLine.Command)).Location);
        var usbEjectInfo = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(UsbEject.Volume)).Location);
        var metadataExtractorInfo = FileVersionInfo.GetVersionInfo(Assembly.GetAssembly(typeof(MetadataExtractor.ImageMetadataReader)).Location);

        return
        [
            new("Company", assemblyInfo.CompanyName),
            new("Product", assemblyInfo.FileDescription),
            new("Version", assemblyInfo.FileVersion),
            new(taglibInfo.FileDescription, taglibInfo.FileVersion),
            new(newtonsoftInfo.FileDescription, newtonsoftInfo.FileVersion),
            new(commandLineInfo.FileDescription, commandLineInfo.FileVersion),
            new(usbEjectInfo.FileDescription, usbEjectInfo.FileVersion),
            new(metadataExtractorInfo.FileDescription, metadataExtractorInfo.FileVersion),
        ];
    }
}
