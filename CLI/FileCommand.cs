using System.CommandLine;

namespace CAndrews.CameraFileManagement.CLI;

internal static partial class Commands
{
    /// <summary>
    /// Creates a command for getting file attributes
    /// </summary>
    internal static Command FileCommand()
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

        // file <absolute file path>
        Command fileCommand = new(
            name: "file",
            description: "Displays file attributes")
        {
            pathArgument
        };
        fileCommand.SetHandler((path) =>
        {
            // Read the file attributes using the library method
            var fileAttributes = FileAttribute.GetFileAttributes(path);

            // Generate the table format and flower boxes
            int maxNameLength = fileAttributes.Max(a => a.Name.Length);
            int maxValueLength = fileAttributes.Max(a => a.Value?.Length).Value;
            int maxSourceLength = fileAttributes.Max(a => a.Source.Length);
            string tableFormat = $"| {{0,-{maxNameLength}}} | {{1,-{maxValueLength}}} | {{2,-{maxSourceLength}}} |";
            string header = string.Format(tableFormat, "Name", "Value", "Source");
            string separator = $"|{new string('-', maxNameLength + 2)}|{new string('-', maxValueLength + 2)}|{new string('-', maxSourceLength + 2)}|";            

            // Write out the header and file information to console
            Console.WriteLine(separator);
            Console.WriteLine(header);
            Console.WriteLine(separator);
            fileAttributes.ForEach(a => Console.WriteLine(string.Format(tableFormat, a.Name, a.Value, a.Source)));
            Console.WriteLine(separator);
        },
        pathArgument);
        return fileCommand;
    }
}
