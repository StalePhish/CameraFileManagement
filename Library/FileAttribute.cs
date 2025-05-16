using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace CAndrews.CameraFileManagement;

/// <summary>
/// Represents attributes of a fileTag
/// </summary>
public class FileAttribute(string name, string value, string source)
{
    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; } = value;

    /// <summary>
    /// Source
    /// </summary>
    public string Source { get; set; } = source;

    /// <summary>
    /// Gets the list of fileTag attributes
    /// </summary>
    /// <param name="path">Absolute path from which to read the fileTag attributes</param>
    /// <returns>List of fileTag attributes</returns>
    public static List<FileAttribute> GetFileAttributes(string path)
    {
        List<FileAttribute> fileAttributes = [];

        // Generic fileTag attributes
        var fileInfo = new FileInfo(path);
        fileInfo.GetType().GetProperties().ToList().ForEach(
            prop => fileAttributes.AddFileAttribute(prop.Name, prop.GetValue(fileInfo), nameof(FileInfo)));

        try
        {
            // Metadata fileTag attributes
            var fileTag = TagLib.File.Create(path);
            fileTag.Properties?.GetType().GetProperties().ToList().ForEach(
                prop => fileAttributes.AddFileAttribute(prop.Name, prop.GetValue(fileTag.Properties), nameof(fileTag.Properties)));

            fileTag.Tag.GetType().GetProperties().ToList().ForEach(
                prop => fileAttributes.AddFileAttribute(prop.Name, prop.GetValue(fileTag.Tag), nameof(fileTag.Tag)));
        }
        catch (Exception ex)
        {
            // Ignore exceptions from TagLib or file compression issues
            if (ex is not TagLib.CorruptFileException && ex is not TagLib.UnsupportedFormatException && ex is not InvalidDataException)
            {
                throw;
            }
        }

        try
        {
            // Metadata Extractor attributes
            var fileMetadata = ImageMetadataReader.ReadMetadata(path);
            //fileMetadata.ToList().ForEach(data => data.Tags.ToList().ForEach(
            //    tag => fileAttributes.AddFileAttribute(tag.Name, tag.Description, data.Name)));
            var quickTimeMetadataHeader = fileMetadata.SingleOrDefault(data => data.GetType() == typeof(QuickTimeMetadataHeaderDirectory));
            quickTimeMetadataHeader?.Tags.ToList().ForEach(
                tag => fileAttributes.AddFileAttribute(tag.Name, tag.Description, quickTimeMetadataHeader.Name));
            var exifIfd0 = fileMetadata.SingleOrDefault(data => data.GetType() == typeof(ExifIfd0Directory));
            exifIfd0?.Tags.ToList().ForEach(
                tag => fileAttributes.AddFileAttribute(tag.Name, tag.Description, exifIfd0.Name));
        }
        catch (ImageProcessingException)
        {
            // Ignore exceptions from MetadataExtractor
        }
        
        // ShellObject
        using (ShellObject so = ShellObject.FromParsingName(path))
        {
            var properties = so.Properties.System.GetType().GetProperties();

            var dateAcquired = properties.SingleOrDefault(p => p.Name == "DateAcquired");
            if (dateAcquired is not null)
            {
                var value = dateAcquired.GetValue(so.Properties.System, null) as IShellProperty;
                fileAttributes.AddFileAttribute(dateAcquired.Name, value?.ValueAsObject, "ShellObject");
            }

            // Too slow to load them all
            /*
            foreach (System.Reflection.PropertyInfo property in properties)
            {
                var value = property.GetValue(so.Properties.System, null) as IShellProperty;
                fileAttributes.AddFileAttribute(property.Name, value?.ValueAsObject, "ShellObject");
            }
            */
        }

        return [.. fileAttributes.OrderBy(p => p.Name)];
    }
}