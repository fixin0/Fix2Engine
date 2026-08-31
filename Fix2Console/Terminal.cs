using System;
using Fix2Engine.User;

namespace Fix2Console;

public class Terminal : IDisposable
{
    public Terminal(string[] args)
    {
        try
        {
            HandleArgs(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
#if DEBUG
            Console.WriteLine(ex.ToString());
#endif
        }
    }

    private void HandleArgs(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            switch (args[0])
            {
                case "--new-project" when args.Length >= 2:
                    ProjectGenerator.CreateNewProject(args[1]);
                    break;
                case "--new-project":
                    
                    Console.WriteLine("Error: --new-project requires a project name.");
                    Console.WriteLine("Usage: Fix2Console --new-project {project name}");
                    break;
                case "--help":
                case "-h":
                case "--h":
                    PrintHelp();
                    break;
                default:
                    Console.WriteLine($"Unknown command: {args[0]}");
                    PrintHelp();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling command '{(args.Length > 0 ? args[0] : "")}': {ex.Message}");
        }
    }

    private void PrintHelp()
    {
        try
        {
            Console.WriteLine("Fix2Console - Fix2Engine Project Tool");
            Console.WriteLine($"Your OS - {Platform.PlatformInfo()}");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  Fix2Console --new-project {project name}   Create a new Fix2Engine project in current directory");
            Console.WriteLine("  Fix2Console --help                         Show this help");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to print help: {ex.Message}");
        }
    }

    public void Dispose() { }
}
