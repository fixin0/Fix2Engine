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
using Fix2Engine.Monitoring;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace Fix2Engine
{
    public class Game : Windowing
    {
        public Game() : base(1280, 720, "My Game") { }

        protected override void Start()
        {
            rlImGui.Setup(true);
            ApplyImGuiTheme();
            SceneManager.LoadScene<MainMenuScene>();
        }

        protected override void Update(float dt)
        {
            PerformanceMonitor.Update(dt);
            SceneManager.Update(dt);
        }

        protected override void Render()
        {
            SceneManager.Render();
            rlImGui.Begin();
            SceneManager.RenderUI();
            PerformanceMonitor.Draw();
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

UI uses native `ImGuiNET` inside `rlImGui.Begin()`/`End()` — no wrapper library. The FPS/CPU/GPU overlay is provided by `PerformanceMonitor` (`Fix2Engine.Monitoring`) — see [Monitoring](Monitoring.md).

## 5. Entry Point

`Game/Program.cs` loads `InputMap.toml` and starts the loop:

```csharp
internal static class Program
{
    static void Main(string[] args)
    {
        Fix2Engine.Input.InputManager.LoadInputMap();
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

The generated project uses native ImGui and references `Graphics`, `Components`, `Input`, `Physics`, `Audio`.

Existing projects can run `Fix2Console --init /path/to/Fix2Engine` to save the engine directory in `Fix2Engine.toml` and create `InputMap.toml`. See [Input](Input.md) for the action API and setup details.

## 6. Next Steps

- Handle keyboard input → [Input](Input.md) (`InputManager.IsDown` / `IsPressed`)
- Draw something → [Graphics](Graphics.md)
- Control the camera → [Camera](Camera.md) (`Fix2Engine.Components.Cameras.Camera`)
- Build a UI → native `ImGuiNET` inside `RenderUI`
