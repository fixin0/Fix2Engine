# Fix2Engine Documentation

Welcome to **Fix2Engine** — a modular C# / .NET 10 game engine built on top of [Raylib-cs](https://github.com/ChrisDill/Raylib-cs) and [rlImGui-cs](https://github.com/raylib-extras/rlImGui).

## Overview

Fix2Engine is split into focused class-library projects plus a `Game` executable that demonstrates usage:

| Project | Purpose | Key Dependencies |
|---------|---------|-----------------|
| `Fix2Engine.Graphics` | Window, rendering, models, sprites, skybox | `Raylib-cs 8.0`, `rlImGui-cs 3.2` |
| `Fix2Engine.Components` | Scene graph (`Node2D`/`Node3D`), camera, scene interface | `Raylib-cs 8.0` |
| `Fix2Engine.Input` | Keyboard polling (`WindowsInputBackend`) + TOML config | `Tomlyn 2.10` |
| `Fix2Engine.Physics` | `PhysicsWorld` placeholder for collision / physics | — |
| `Fix2Engine.Audio` | `SoundManager` placeholder for OpenAL audio | `Silk.NET.OpenAL 2.23` |
| `Game` | Entry point, example scenes, menu, performance monitor | All above + `ImGui.NET` |
| `Fix2Console` | CLI tool (`--new-project`) for scaffolding | — |

All projects target **`net10.0`** with `ImplicitUsings` and `Nullable` enabled. The `Game` project publishes as AOT (`PublishAot=true`). UI uses native `ImGui.NET` + `rlImGui-cs` (the former `Fix2Engine.IMGUI` wrapper was removed).

## Quick Links

1. [Getting Started](GettingStarted.md) — create your first window and scene
2. [Windowing & Game Loop](WindowingAndGameLoop.md) — lifecycle, `FixedUpdate`
3. [Input](Input.md) — polling keys, TOML action maps, adding Linux support
4. [Graphics](Graphics.md) — `Model3D`, `Sprite2D`, `Skybox`, primitives
5. [Camera](Camera.md) — `Camera`, `CameraType`, `Begin`/`End`
6. [Scene Management](SceneManagement.md) — `IFixScene`, `SceneManager`
7. [IMGUI / UI](IMGUI.md) — native ImGui usage (legacy doc)
8. [Components](Components.md) — `Node2D`, `Node3D`
9. [Physics & Audio](PhysicsAndAudio.md) — `PhysicsWorld` / `SoundManager` stubs and how to extend
10. [Project Structure](ProjectStructure.md) — solution layout, build, conventions

## Minimum Example

```csharp
// Program.cs
AppContext.SetSwitch("Tomlyn.TomlSerializer.IsReflectionEnabledByDefault", true);
using var game = new Game();
game.Run();

// Game.cs
public class Game : Windowing
{
    public bool ShowPerformanceMonitor { get; set; } = true;

    public Game() : base(1280, 720, "My Game") { }
    protected override void Start()
    {
        rlImGui.Setup(true);
        ApplyImGuiTheme(); // native ImGui style setup
        SceneManager.LoadScene<MyScene>();
    }
    protected override void Update(float dt) => SceneManager.Update(dt);
    protected override void Render()
    {
        SceneManager.Render();
        rlImGui.Begin();
        SceneManager.RenderUI();
        if (ShowPerformanceMonitor) DrawPerformanceMonitor();
        rlImGui.End();
    }
}
```

## Requirements

- .NET 10 SDK
- Windows (current `WindowsInputBackend` uses `user32.dll`; see [Input](Input.md) for Linux notes)
- No asset pipeline required — primitives work out of the box
