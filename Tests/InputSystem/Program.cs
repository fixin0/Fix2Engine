using System.Reflection;
using System.Xml.Linq;
using Fix2Console;
using Fix2Engine.Input;
using Fix2Engine.Input.InputBackend;

AppContext.SetSwitch("Tomlyn.TomlSerializer.IsReflectionEnabledByDefault", false);
string originalDirectory = Environment.CurrentDirectory;
string temp = Path.Combine(Path.GetTempPath(), "fix2-input-tests-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temp);
int assertions = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    assertions++;
}
void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { assertions++; return; }
    throw new Exception($"Expected {typeof(T).Name}");
}
try
{
    string map = Path.Combine(temp, "InputMap.toml");
    File.WriteAllText(map, "[Actions]\nMove = [\"w\", \"Up\"]\nJump = [\"Space\"]\n");
    InputManager.LoadInputMap(map);
    Check(InputManager.HasAction("Move"), "Map did not load");
    Check(!InputManager.HasAction("move"), "Action names must be case-sensitive");
    Check(ConfigReader.Load(map).Actions.Count == 2, "ConfigReader must return parsed config");
    var current = (bool[])typeof(InputManager).GetField("CurrentKeys", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
    var previous = (bool[])typeof(InputManager).GetField("PreviousKeys", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
    void Frame(params Keys[] keys)
    {
        Array.Copy(current, previous, current.Length);
        Array.Clear(current);
        foreach (var key in keys) current[(int)key] = true;
    }
    Frame(Keys.W);
    Check(InputManager.IsPressed("Move") && InputManager.IsDown("Move"), "First key press");
    Check(InputManager.IsPressed(Keys.W), "Raw key API must still work");
    Frame(Keys.W, Keys.Up);
    Check(!InputManager.IsPressed("Move"), "Second alternative must not retrigger action");
    Frame(Keys.Up);
    Check(InputManager.IsDown("Move") && !InputManager.IsReleased("Move"), "Releasing one alternative must not release action");
    Frame(Keys.W);
    Check(!InputManager.IsPressed("Move") && !InputManager.IsReleased("Move"), "Switching alternatives must keep action held");
    Frame();
    Check(InputManager.IsReleased("Move") && !InputManager.IsDown("Move"), "Last key releases action");
    Frame();
    Check(!InputManager.IsReleased("Move"), "Release lasts one frame");
    Throws<KeyNotFoundException>(() => InputManager.IsDown("Missing"));
    foreach (string invalid in new[]
    {
        "[Actions]\nMove = [\"NotAKey\"]", "[Actions]\nMove = [\"65\"]",
        "[Actions]\nMove = [\"None\"]", "[Actions]\nMove = []",
        "[Actions]\nMove = [\"W, Up\"]", "[Actions]\nMove = 123",
        "[Actions]\nMove = [", "[Actions]\nMove = [\"W\"]\nMove = [\"S\"]", "[Other]\nValue = 1"
    })
    {
        File.WriteAllText(map, invalid);
        Throws<InvalidDataException>(() => InputManager.LoadInputMap(map));
        Check(InputManager.HasAction("Jump"), "Invalid reload must preserve current map");
    }
    Throws<FileNotFoundException>(() => InputManager.LoadInputMap(Path.Combine(temp, "missing.toml")));
    File.WriteAllText(map, "[Actions]\nNewAction = [\"Enter\"]");
    InputManager.LoadInputMap(map);
    Check(InputManager.HasAction("NewAction") && !InputManager.HasAction("Move"), "Valid reload replaces bindings");
    Environment.CurrentDirectory = temp;
    InputManager.LoadInputMap();
    Check(InputManager.HasAction("Jump"), "Default path must use executable directory regardless of working directory");

    string engine = Path.TrimEndingDirectorySeparator(args.Length > 0
        ? Path.GetFullPath(args[0], originalDirectory)
        : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../")));
    string project = Path.Combine(temp, "Example");
    Directory.CreateDirectory(project);
    string csproj = Path.Combine(project, "Example.csproj");
    File.WriteAllText(csproj, TemplateGenerator.GenerateCsproj("Example", project, engine));
    ProjectSettings.CreateFiles(project, engine);
    Check(ProjectSettings.ReadEngineDirectory(project) == engine.TrimEnd(Path.DirectorySeparatorChar), "Engine directory round trip");
    Check(InputConfig.Load(Path.Combine(project, "InputMap.toml")).Actions.ContainsKey("Jump"), "Generated map must parse");
    string customMap = "# Preserve my comment\n[Actions]\nCustom = [\"F2\"]\n";
    File.WriteAllText(Path.Combine(project, "InputMap.toml"), customMap);
    Directory.CreateDirectory(Path.Combine(project, "Scenes"));
    Environment.CurrentDirectory = Path.Combine(project, "Scenes");
    ProjectSettings.Initialize();
    using (var terminal = new Terminal(Array.Empty<string>())) { }
    Check(Environment.ExitCode == 0, "Running without arguments inside a project should initialize it");
    Check(File.ReadAllText(Path.Combine(project, "InputMap.toml")) == customMap, "Init must preserve custom input maps");
    var doc = XDocument.Load(csproj);
    Check(doc.Descendants("None").Count(x => (string?)x.Attribute("Update") == "InputMap.toml") == 1, "Init must not duplicate copy items");
    Check(doc.Descendants("ProjectReference").Count(x => ((string?)x.Attribute("Include"))!.EndsWith("Input.csproj")) == 1, "Init must not duplicate Input reference");
    Check(doc.Descendants("Import").Count(x => (string?)x.Attribute("Label") == "Fix2EngineLicenses") == 1,
        "Repeated init must preserve exactly one license import");
    Check(EngineRootFinder.FindEngineRoot() == engine.TrimEnd(Path.DirectorySeparatorChar), "Resolve engine from nested project directory");
    Throws<DirectoryNotFoundException>(() => ProjectSettings.Initialize(Path.Combine(temp, "absent")));
    File.WriteAllText(Path.Combine(project, "Fix2Engine.toml"), "[Engine]\nDirectory = \"missing\"\n");
    Throws<DirectoryNotFoundException>(() => EngineRootFinder.FindEngineRoot());
    string bare = Path.Combine(temp, "Bare");
    Directory.CreateDirectory(bare);
    File.WriteAllText(Path.Combine(bare, "Bare.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
    Environment.CurrentDirectory = bare;
    ProjectSettings.Initialize(engine);
    var bareDoc = XDocument.Load(Path.Combine(bare, "Bare.csproj"));
    Check(bareDoc.Descendants("ProjectReference").Count() == 1, "Existing plain project needs Input reference");
    Check(bareDoc.Descendants("None").Any(x => (string?)x.Attribute("CopyToPublishDirectory") == "PreserveNewest"), "Existing project needs publish copy metadata");
    Check(File.Exists(Path.Combine(bare, "InputMap.toml")), "Existing project needs default map");
    Check(bareDoc.Descendants("Import").Any(x => (string?)x.Attribute("Label") == "Fix2EngineLicenses"),
        "Existing plain project needs license distribution import");
    Environment.CurrentDirectory = temp;
    Throws<InvalidOperationException>(() => ProjectSettings.Initialize(engine));
    Console.WriteLine($"PASS: {assertions} assertions (parser, action transitions, reload, config and project initialization).");
}
finally
{
    Environment.CurrentDirectory = originalDirectory;
    Directory.Delete(temp, true);
}
