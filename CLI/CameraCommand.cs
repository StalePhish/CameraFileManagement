using CAndrews.CameraFileManagement.Application.View;
using System.CommandLine;
using System.Windows;
using System.Windows.Threading;
using static CAndrews.CameraFileManagement.Enumerations;

namespace CAndrews.CameraFileManagement.CLI;

internal static partial class Commands
{
    /// <summary>
    /// Creates a command for copying camera files
    /// </summary>
    internal static Command CameraCommand()
    {
        Option<bool> subfoldersOption = new(
            name: "subfolders",
            description: "Include subfolders",
            getDefaultValue: () => true);

        Option<bool> openFoldersAfterOption = new(
            name: "open",
            description: "Open folders after",
            getDefaultValue: () => true);

        Argument<string> pathArgument = new(
            name: "path",
            description: "Absolute directory path",
            parse: arg =>
            {
                string path = arg.Tokens.Single().Value;
                if (!System.IO.Directory.Exists(path))
                {
                    arg.ErrorMessage = "Path does not exist";
                    return null;
                }
                return path;
            });

        // camera list <path>
        Command listCommand = new(
            name: "list",
            description: "Display camera files to be copied")
        {
            subfoldersOption,
            pathArgument
        };
        listCommand.SetHandler(DisplayCameraFiles, pathArgument, subfoldersOption);

        // camera run <path>
        Command runCommand = new(
            name: "run",
            description: "Run file transfer using individual move or copy settings per file")
        {
            subfoldersOption,
            openFoldersAfterOption,
            pathArgument
        };
        runCommand.SetHandler((path, subfolders, openFoldersAfter) =>
        {
            Run(CopyType.Run, path, subfolders, openFoldersAfter).Wait();
        },
        pathArgument, subfoldersOption, openFoldersAfterOption);

        // camera move <path>
        Command moveCommand = new(
            name: "move",
            description: "Run file transfer explicitly moving all files")
        {
            subfoldersOption,
            openFoldersAfterOption,
            pathArgument
        };
        moveCommand.SetHandler((path, subfolders, openFoldersAfter) =>
        {
            Run(CopyType.Move, path, subfolders, openFoldersAfter).Wait();
        },
        pathArgument, subfoldersOption, openFoldersAfterOption);

        // camera copy <path>
        Command copyCommand = new(
            name: "copy",
            description: "Run file transfer explicitly copying all files")
        {
            subfoldersOption,
            openFoldersAfterOption,
            pathArgument
        };
        copyCommand.SetHandler((path, subfolders, openFoldersAfter) =>
        {
            Run(CopyType.Copy, path, subfolders, openFoldersAfter).Wait();
        },
        pathArgument, subfoldersOption, openFoldersAfterOption);

        // camera rename <path>
        Command renameCommand = new(
            name: "rename",
            description: "Run file transfer explicitly renaming all files in place")
        {
            subfoldersOption,
            openFoldersAfterOption,
            pathArgument
        };
        copyCommand.SetHandler((path, subfolders, openFoldersAfter) =>
        {
            Run(CopyType.Rename, path, subfolders, openFoldersAfter).Wait();
        },
        pathArgument, subfoldersOption, openFoldersAfterOption);

        // camera demo <path>
        Command demoCommand = new(
            name: "demo",
            description: "Run file transfer in demonstration mode that does not copy and files")
        {
            subfoldersOption,
            openFoldersAfterOption,
            pathArgument
        };
        demoCommand.SetHandler((path, subfolders, openFoldersAfter) =>
        {
            Run(CopyType.Demonstration, path, subfolders, openFoldersAfter).Wait();
        },
        pathArgument, subfoldersOption, openFoldersAfterOption);

        // camera
        Command cameraCommand = new(
            name: "camera",
            description: "Camera copy command");
        cameraCommand.AddCommand(listCommand);
        cameraCommand.AddCommand(runCommand);
        cameraCommand.AddCommand(moveCommand);
        cameraCommand.AddCommand(copyCommand);
        cameraCommand.AddCommand(renameCommand);
        cameraCommand.AddCommand(demoCommand);
        return cameraCommand;
    }

    /// <summary>
    /// Writes out a table of camera files
    /// </summary>
    /// <param name="sourceDirectory">The source directory</param>
    /// <param name="subfolders">Include subfolders</param>
    private static void DisplayCameraFiles(string sourceDirectory, bool subfolders)
    {
        CameraLoad cameraLoad = new();
        cameraLoad.ProgressUpdate += UpdateProgress();
        List<CameraFileInfo> files = cameraLoad.GetCameraFilesFromDirectory(sourceDirectory, subfolders, CancellationToken.None, out bool isRemovableDrive);
        cameraLoad.ProgressUpdate -= UpdateProgress();
        SetConsoleColor();

        // Set up the columns and widths
        List<(string column, int size)> columns =
        [
            ("Index", 0),
            ("Selected", files.Max(f => f.Selected.ToString().Length)),
            ("Status", files.Max(f => f.Status.ToString().Length)),
            ("Camera", files.Max(f => f.Alias?.Length ?? 0)),
            ("Source File Name", files.Max(f => f.SourceFileName?.Length ?? 0)),
            ("Destination Directory", files.Max(f => f.DestinationDirectory?.Length ?? 0)),
            ("Destination File Name", files.Max(f => f.DestinationFileName?.Length ?? 0)),
            ("Destination Exists", 0),
            ("Move (or Copy)", 0)
        ];
        columns = columns.Select(col => (col.column, int.Max(col.column.Length, col.size))).ToList();

        // Generate the table format and flower boxes
        string tableFormat = $"| {string.Join(" | ", columns.Select((col, index) => $"{{{index},-{col.size}}}"))} |";
        string header = string.Format(tableFormat, columns.Select(c => c.column).ToArray());
        string seperator(string left, string middle, string right) => $"{left}{string.Join(middle, columns.Select(col => new string('─', col.size + 2)))}{right}";
        string info(string info) => $"| {info}{new string(' ', header.Length - info.Length - 3)}|";

        // Extra information
        string sourceDirectoryLine = $"Camera Files in Source Directory{(isRemovableDrive ? " (removable drive)" : "")}: {sourceDirectory}";

        // Write out the header and file information to console
        Console.WriteLine(seperator("┌", "─", "┐"));
        Console.WriteLine(info(sourceDirectoryLine));
        Console.WriteLine(seperator("├", "┬", "┤"));
        Console.WriteLine(header);
        Console.WriteLine(seperator("|", "┼", "|"));
        int index = 0;
        files.ForEach(f => Console.WriteLine(string.Format(tableFormat, ++index, f.Selected ? "X" : null, f.Status, f.Alias, f.SourceFileName,
            f.DestinationDirectory, f.DestinationFileName, f.DestinationExists ? "X" : null, f.Move ? "Move" : "Copy")));
        Console.WriteLine(seperator("└", "┴", "┘"));
    }

    /// <summary>
    /// Runs the camera copy
    /// </summary>
    /// <param name="copyType">The copy type, whether it be based on each file's camera setting or override to move or copy all</param>
    /// <param name="sourceDirectory">The source directory</param>
    /// <param name="subfolders">Include subfolders</param>
    /// <param name="openFoldersAfter">Open the output folders automatically, or not</param>
    private async static Task Run(CopyType copyType, string sourceDirectory, bool subfolders, bool openFoldersAfter)
    {
        CameraLoad cameraLoad = new();
        CameraCopy cameraCopy = new();
        //cameraLoad.ProgressUpdate += UpdateProgress();
        cameraCopy.ProgressUpdate += UpdateProgress();

        // Display a quick loading message since we purposely
        // did not attach to cameraLoad.ProgressUpdate to reduct clutter.
        CameraProgress progress = new()
        {
            Value = 0,
            Indeterminate = true,
            Result = StatusType.Ready,
            Text = $"Loading {sourceDirectory}..."
        };
        Action<CameraProgress> progressDelegate = UpdateProgress();
        progressDelegate(progress);

        // TODO: add a comment line argument to optionally display a GUI progress window with a cancel button
        // Experimental code that could be pursued to implement this feature
        /*
        //var app = new Application();
        var progressControl = new ProgressControl();
        var window = new System.Windows.Window() { Content = progressControl };
        app.Run(window);
        window.Show();
        Dispatcher.Run();
        var thread = new Thread(Foo);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        */

        List<CameraFileInfo> files = cameraLoad.GetCameraFilesFromDirectory(sourceDirectory, subfolders, CancellationToken.None, out _);        
        await cameraCopy.Run(files, copyType, openFoldersAfter, CancellationToken.None);

        //cameraLoad.ProgressUpdate -= UpdateProgress();
        cameraCopy.ProgressUpdate -= UpdateProgress();

        //window.Hide();
    }

    /// <summary>
    /// Update the progress notification for the user
    /// </summary>
    private static Action<CameraProgress> UpdateProgress()
    {
        return progress =>
        {
            SetConsoleColor(progress.Result);
            Console.WriteLine(progress.Text);
        };
    }

    /// <summary>
    /// Sets the console foreground color based on the status type
    /// </summary>
    /// <param name="status">Status type</param>
    private static void SetConsoleColor(StatusType status = StatusType.Ready)
    {
        Console.ForegroundColor = status switch
        {
            StatusType.Failed => ConsoleColor.Red,
            StatusType.Canceled => ConsoleColor.Yellow,
            StatusType.Success => ConsoleColor.Green,
            _ => ConsoleColor.Gray,
        };
    }
}
