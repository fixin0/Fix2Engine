# Getting Started

## 1. Prerequisites

- Install [.NET 10 SDK](https://dotnet.microsoft.com/download).
- Clone the repository and open `Fix2Engine.sln`.
- Install the native graphics/windowing support required by Raylib for your platform.

## 2. Build the Engine

From the engine checkout:

```bash
dotnet build Fix2Engine.sln -m:1
```

The solution contains the engine libraries and the `Fix2Console` command-line tool.
Your executable lives in a separate project.

## 3. Create Your Project

From the directory where you keep your projects:

```bash
dotnet run --project /path/to/Fix2Engine/Fix2Console -- --new-project MyGame
cd MyGame
dotnet run
```

Replace `/path/to/Fix2Engine` with the engine checkout path. The generated project
contains `Program.cs`, a `Game` class derived from `Windowing`, a scene,
`InputMap.toml`, and `Fix2Engine.toml`. It references `Graphics`, `Components`,
`Input`, `Physics`, and `Audio`, and sets up native ImGui and a simple 2D drawing.

The generated `Game` class belongs to your application. Customize its `Start`,
`Update`, and `Render` methods to wire up your scenes and systems. Its startup loads
`InputMap.toml` from beside the executable before entering the loop.

## 4. Add a Scene

Every scene implements `IFixScene`:

```csharp
using Fix2Engine.Components.Scene;
using Raylib_cs;

public class MyScene : IFixScene
{
    public void Start() { }
    public void Update(float dt) { }
    public void Render()
    {
        Raylib.ClearBackground(Color.Black);
        Raylib.DrawRectangle(100, 100, 160, 160, Color.Red);
    }
    public void RenderUI() { }
    public void Unload() { }
    public void Dispose() { }
}
```

Select it in your application's `Start` method:

```csharp
Fix2Engine.Components.SceneManager.LoadScene<MyScene>();
```

Scene switching is deferred until the next `SceneManager.Update(dt)`, so it can also
be requested from gameplay logic or a UI button. See [Scene Management](SceneManagement.md).

## 5. Configure an Existing Project

Inside an existing C# project, run the CLI with `--init /path/to/Fix2Engine` to save
the engine directory, create an input map, and add its reference and copy rules.
Call `Fix2Engine.Input.InputManager.LoadInputMap()` once at application startup.
See [Input](Input.md) for configuration and action bindings.

## 6. Next Steps

- [Windowing & Game Loop](WindowingAndGameLoop.md) — lifecycle and fixed updates
- [Graphics](Graphics.md) — sprites and 2D drawing
- [Components](Components.md) — `Node2D`
- [UI](IMGUI.md) — native ImGui integration
- [Monitoring](Monitoring.md) — optional performance overlay
