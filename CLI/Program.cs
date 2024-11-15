using System.CommandLine;

namespace CAndrews.CameraFileManagement.CLI;

internal class Program
{
    /// <summary>
    /// Entry point for the fileCommand line interface
    /// </summary>
    /// <param name="args">Argument values</param>
    /// <exception cref="ArgumentException"></exception>
    static async Task<int> Main(string[] args)
    {
        Console.Title = "CAndrews Camera File Management";
        //Console.ForegroundColor = ConsoleColor.Cyan;

        Settings.Instance.Load();

        var rootCommand = new RootCommand
        {
            Commands.CameraCommand(),
            Commands.FileCommand(),
            Commands.SettingsCommand(),
            Commands.AboutCommand()
        };

        return await rootCommand.InvokeAsync(args);
    }
}
