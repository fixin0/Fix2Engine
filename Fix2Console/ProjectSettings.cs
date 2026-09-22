using System.Xml.Linq;
using Tomlyn;
using Tomlyn.Serialization;

namespace Fix2Console;

public sealed class ProjectSettings
{
    public EngineSettings Engine { get; set; } = new();

    public static string? FindProjectDirectory(string start)
    {
        for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
            if (dir.EnumerateFiles("*.csproj").Any()) return dir.FullName;
        return null;
    }

    public static string? ReadEngineDirectory(string projectDirectory)
    {
        string path = Path.Combine(projectDirectory, "Fix2Engine.toml");
        if (!File.Exists(path)) return null;
        var settings = TomlSerializer.Deserialize(File.ReadAllText(path), ProjectTomlContext.Default.ProjectSettings);
        if (string.IsNullOrWhiteSpace(settings?.Engine?.Directory))
            throw new InvalidDataException($"'{path}' must contain [Engine] Directory.");
        return Path.GetFullPath(settings.Engine.Directory, projectDirectory);
    }

    public static void CreateFiles(string projectDirectory, string engineDirectory)
    {
        string settingsPath = Path.Combine(projectDirectory, "Fix2Engine.toml");
        if (!File.Exists(settingsPath))
        {
            var settings = new ProjectSettings
            {
                Engine = new EngineSettings
                {
                    Directory = Path.GetRelativePath(projectDirectory, engineDirectory).Replace('\\', '/')
                }
            };
            File.WriteAllText(settingsPath, TomlSerializer.Serialize(settings, ProjectTomlContext.Default.ProjectSettings));
        }

        string inputPath = Path.Combine(projectDirectory, "InputMap.toml");
        if (!File.Exists(inputPath)) File.WriteAllText(inputPath, DefaultInputMap);
    }

    public static void Initialize(string? engineDirectory = null)
    {
        string projectDirectory = FindProjectDirectory(Environment.CurrentDirectory)
            ?? throw new InvalidOperationException("Run --init inside a C# project directory.");
        var projects = Directory.GetFiles(projectDirectory, "*.csproj");
        if (projects.Length != 1)
            throw new InvalidOperationException("The directory must contain exactly one .csproj file.");

        string? savedRoot = ReadEngineDirectory(projectDirectory);
        string root = engineDirectory is null
            ? savedRoot ?? EngineRootFinder.FindEngineRoot()
                ?? throw new InvalidOperationException("Engine not found. Use Fix2Console --init <engine-directory>.")
            : Path.GetFullPath(engineDirectory);
        EngineRootFinder.Validate(root);
        if (savedRoot is not null && !Path.GetFullPath(savedRoot).Equals(Path.GetFullPath(root),
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidOperationException("Fix2Engine.toml already specifies a different engine. Edit [Engine] Directory first.");

        var document = XDocument.Load(projects[0], LoadOptions.PreserveWhitespace);
        var project = document.Root ?? throw new InvalidDataException("Invalid project file.");
        XNamespace ns = project.Name.Namespace;
        string inputProject = Path.Combine(root, "Input", "Input.csproj");
        bool hasReference = Path.GetFullPath(projects[0]) == inputProject || project.Descendants(ns + "ProjectReference").Any(reference =>
        {
            var include = (string?)reference.Attribute("Include");
            return include is not null && !include.Contains('$') &&
                Path.GetFullPath(include.Replace('\\', '/'), projectDirectory) == inputProject;
        });
        if (!hasReference)
            project.Add(new XElement(ns + "ItemGroup", new XElement(ns + "ProjectReference",
                new XAttribute("Include", Path.GetRelativePath(projectDirectory, inputProject).Replace('\\', '/')))));

        var item = project.Descendants().FirstOrDefault(element =>
            (element.Name == ns + "None" || element.Name == ns + "Content") &&
            ((string?)element.Attribute("Update") == "InputMap.toml" || (string?)element.Attribute("Include") == "InputMap.toml"));
        if (item is null)
        {
            item = new XElement(ns + "None", new XAttribute("Update", "InputMap.toml"));
            project.Add(new XElement(ns + "ItemGroup", item));
        }
        item.SetAttributeValue("CopyToOutputDirectory", "PreserveNewest");
        item.SetAttributeValue("CopyToPublishDirectory", "PreserveNewest");
        item.Elements(ns + "CopyToOutputDirectory").Remove();
        item.Elements(ns + "CopyToPublishDirectory").Remove();
        var licenseImport = project.Elements(ns + "Import")
            .FirstOrDefault(element => (string?)element.Attribute("Label") == "Fix2EngineLicenses");
        if (licenseImport is null)
        {
            licenseImport = new XElement(ns + "Import", new XAttribute("Label", "Fix2EngineLicenses"));
            project.Add(licenseImport);
        }
        licenseImport.SetAttributeValue("Project", Path.GetRelativePath(projectDirectory,
            Path.Combine(root, "build", "Fix2Engine.Licenses.targets")).Replace('\\', '/'));
        licenseImport.SetAttributeValue("Condition", "'$(_Fix2EngineLicensesImported)' != 'true'");

        CreateFiles(projectDirectory, root);
        document.Save(projects[0]);
        Console.WriteLine($"Configured project: {projectDirectory}");
        Console.WriteLine($"Engine directory: {root}");
        Console.WriteLine("Edit InputMap.toml, then call InputManager.LoadInputMap() once at startup.");
    }

    public const string DefaultInputMap = """
        # Action names are case-sensitive. Key names come from the Keys enum.
        # Multiple keys are alternatives for the same action.
        [Actions]
        MoveForward = ["W", "Up"]
        MoveBackward = ["S", "Down"]
        MoveLeft = ["A", "Left"]
        MoveRight = ["D", "Right"]
        Jump = ["Space"]
        Pause = ["Escape"]
        ToggleDebug = ["F1"]
        """ + "\n";
}

public sealed class EngineSettings
{
    public string Directory { get; set; } = "";
}

[TomlSerializable(typeof(ProjectSettings))]
internal partial class ProjectTomlContext : TomlSerializerContext { }
