namespace Fix2Console;

public static class EngineRootFinder
{
    public static string? FindEngineRoot()
    {
        string? project = ProjectSettings.FindProjectDirectory(Environment.CurrentDirectory);
        if (project is not null && ProjectSettings.ReadEngineDirectory(project) is { } configured)
        {
            Validate(configured);
            return configured;
        }

        foreach (string start in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
            for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
                if (File.Exists(Path.Combine(dir.FullName, "Fix2Engine.sln")))
                {
                    Validate(dir.FullName);
                    return dir.FullName;
                }
        return null;
    }

    public static void Validate(string directory)
    {
        if (!File.Exists(Path.Combine(directory, "Fix2Engine.sln")) ||
            !File.Exists(Path.Combine(directory, "Input", "Input.csproj")))
            throw new DirectoryNotFoundException($"Invalid Fix2Engine directory: {directory}");
    }
}
