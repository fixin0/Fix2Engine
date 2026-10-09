using System.Diagnostics;

namespace Fix2Console;

/// <summary>Builds the Fix2Engine project that contains the current directory.</summary>
public static class ProjectBuilder
{
    public static void BuildDesktop()
    {
        string projectDirectory = ProjectSettings.FindProjectDirectory(Environment.CurrentDirectory)
            ?? throw new InvalidOperationException("Run this command inside a C# project directory.");
        string[] projects = Directory.GetFiles(projectDirectory, "*.csproj");
        if (projects.Length != 1)
            throw new InvalidOperationException("The directory must contain exactly one .csproj file.");

        string projectFile = projects[0];
        Console.WriteLine($"Starting desktop build for {Path.GetFileNameWithoutExtension(projectFile)}...");

        if (RunProcess(CreateDesktopBuildStartInfo(projectFile, projectDirectory), ".NET SDK") == 0)
        {
            Console.WriteLine("Desktop build completed.");
        }
    }

    /// <summary>Creates the SDK invocation used by the build commands.</summary>
    public static ProcessStartInfo CreateDesktopBuildStartInfo(string projectFile, string workingDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);

        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("build");
        startInfo.ArgumentList.Add(projectFile);
        startInfo.ArgumentList.Add("--configuration");
        startInfo.ArgumentList.Add("Release");
        return startInfo;
    }

    private static int RunProcess(ProcessStartInfo startInfo, string toolName)
    {
        Process? process = Process.Start(startInfo);
        using (process ?? throw new InvalidOperationException($"Could not start {toolName}."))
        {
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                Environment.ExitCode = process.ExitCode;
                Console.WriteLine($"{toolName} failed (exit code {process.ExitCode}).");
            }
            return process.ExitCode;
        }
    }
}
