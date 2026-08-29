# Getting Started

## 1. Prerequisites

- Install [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Clone the repository and open `Fix2Engine.sln`

## 2. Build

```bash
dotnet build
dotnet run --project Game
```

The `Game` project is the executable. All engine projects are class libraries referenced by `Game`.

## 3. Create a New Scene

Every game screen implements `IFixScene` (see [Scene Management](SceneManagement.md)):

```csharp
using Fix2Engine.Components.Scene;

public class MyScene : IFixScene
{
    public void Start() { }
    public void Update(float dt) { }
    public void Render() { }
    public void RenderUI() { }
    public void Unload() { }
    public void Dispose() { }
}
```

Register it from the main menu or from `Game.Start()`:

```csharp
SceneManager.LoadScene<MyScene>();
// or with an instance:
SceneManager.LoadScene(new MyScene());
```

Scene switching is **deferred** — the actual switch happens at the start of the next `SceneManager.Update(dt)` call, so it is safe to call from inside `Update` or from a UI button.

## 4. Wire Up the Game Class

`Game` inherits from `Windowing` (see [Windowing & Game Loop](WindowingAndGameLoop.md)):

```csharp
using Fix2Engine.Components.Scene;
using Fix2Engine.Graphics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace Fix2Engine
{
    public class Game : Windowing
    {
        public bool ShowPerformanceMonitor { get; set; } = true;

        public Game() : base(1280, 720, "My Game") { }

        protected override void Start()
        {
            rlImGui.Setup(true);
            ApplyImGuiTheme();
            SceneManager.LoadScene<MainMenuScene>();
        }

        protected override void Update(float dt) => SceneManager.Update(dt);

        protected override void Render()
        {
            SceneManager.Render();
            rlImGui.Begin();
            SceneManager.RenderUI();
            if (ShowPerformanceMonitor) DrawPerformanceMonitor();
            rlImGui.End();
            Raylib.DrawFPS(Width - 90, 10);
        }

        private static void ApplyImGuiTheme()
        {
            var style = ImGui.GetStyle();
            style.WindowRounding = 12.0f;
            // ... set colors ...
        }
    }
}
```

UI uses native `ImGuiNET` inside `rlImGui.Begin()`/`End()` — no wrapper library.

## 5. Entry Point

`Game/Program.cs` enables Tomlyn reflection (required because `PublishAot=true`) and starts the loop:

```csharp
internal static class Program
{
    static void Main(string[] args)
    {
        AppContext.SetSwitch("Tomlyn.TomlSerializer.IsReflectionEnabledByDefault", true);
        using var game = new Game();
        game.Run();
    }
}
```

Or scaffold a new project:

```bash
dotnet run --project Fix2Console -- --new-project MyGame
cd MyGame && dotnet run
```

The generated project uses native ImGui and references `Fix2Engine.Graphics`, `Fix2Engine.Components`, `Fix2Engine.Input`, `Fix2Engine.Physics`, `Fix2Engine.Audio`.

## 6. Next Steps

- Handle keyboard input → [Input](Input.md) (`InputManager.IsDown` / `IsPressed`)
- Draw something → [Graphics](Graphics.md)
- Control the camera → [Camera](Camera.md) (`Fix2Engine.Components.Cameras.Camera`)
- Build a UI → native `ImGuiNET` inside `RenderUI`
