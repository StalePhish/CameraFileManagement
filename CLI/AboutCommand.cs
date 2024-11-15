using System.CommandLine;

namespace CAndrews.CameraFileManagement.CLI;

internal static partial class Commands
{
    /// <summary>
    /// Creates a fileCommand for getting Help>About information
    /// </summary>
    internal static Command AboutCommand()
    {
        // about
        Command aboutCommand = new(
            name: "about",
            description: "Displays Help>About information");
        aboutCommand.SetHandler(() =>
        {
            var aboutAttributes = About.GetAboutInfo();

            // Generate the table format and flower boxes
            int maxLength1 = aboutAttributes.Max(a => a.Item1.Length);
            int maxLength2 = aboutAttributes.Max(a => a.Item2.Length);
            string tableFormat = $"| {{0,-{maxLength1}}} | {{1,-{maxLength2}}} |";
            string separator = $"|{new string('-', maxLength1 + 2)}|{new string('-', maxLength2 + 2)}|";

            // Write out the about information to console
            Console.WriteLine(separator);
            aboutAttributes.ForEach(a => Console.WriteLine(string.Format(tableFormat, a.Item1, a.Item2)));
            Console.WriteLine(separator);
        });
        return aboutCommand;
    }
}
