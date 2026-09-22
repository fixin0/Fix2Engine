# Scene Management

Sources:
- `Components/Scene/IFixScene.cs` — namespace `Fix2Engine.Components.Scene`
- `Components/Scene/SceneLoader.cs` — namespace `Fix2Engine.Components` (class `SceneManager`)

## IFixScene

```csharp
public interface IFixScene : IDisposable
{
    void Start();
    void Update(float dt);
    void Render();
    void RenderUI();
    void Unload();
}
```

All scenes must implement these five methods plus `Dispose()` (from `IDisposable`).

| Method | When | Typical Work |
|--------|------|--------------|
| `Start()` | Once, when scene becomes current | Spawn objects, load textures |
| `Update(float dt)` | Every frame (`dt` = frame time) | Input, gameplay, AI, physics |
| `Render()` | Every frame, inside `BeginDrawing`/`EndDrawing` | 2D drawing |
| `RenderUI()` | Every frame, inside `rlImGui.Begin`/`End` | ImGui panels, HUD, menus |
| `Unload()` | Rarely used (use `Dispose` instead) | Custom cleanup if needed |
| `Dispose()` | On scene switch and on app exit | Unload textures and other assets |

Minimal scene:

```csharp
using Fix2Engine.Components.Scene;
using Raylib_cs;
using static Raylib_cs.Raylib;

public class MyScene : IFixScene
{
    public void Start() { }
    public void Update(float dt) { }

    public void Render()
    {
        ClearBackground(new Color(30, 30, 40, 255));
        DrawRectangle(100, 100, 160, 160, Color.Red);
    }

    public void RenderUI() { }
    public void Unload() { }
    public void Dispose() { }
}
```

## SceneManager

```csharp
public static class SceneManager
{
    public static IFixScene? CurrentScene { get; }
    public static void LoadScene<T>() where T : IFixScene, new();
    public static void LoadScene(IFixScene scene);
    public static void Update(float dt);
    public static void Render();
    public static void RenderUI();
    public static void Unload();
}
```

### Switching Scenes

```csharp
SceneManager.LoadScene<MyScene>();       // generic — creates via new()
SceneManager.LoadScene(new MyScene());   // instance
```

Switching is **deferred**: `LoadScene` only sets `_nextScene`. At the start of the next `SceneManager.Update(dt)`:

```
if (_nextScene != null)
{
    CurrentScene?.Dispose();
    CurrentScene = _nextScene;
    CurrentScene.Start();
    _nextScene = null;
}
CurrentScene?.Update(dt);
```

This makes it safe to call `LoadScene` from inside `Update`, from a button callback, or from `RenderUI`.

### Wiring in Game

Your application's `Windowing` subclass delegates to `SceneManager`. The following
example also uses the optional `Monitoring` project:

```csharp
protected override void Start()
{
    rlImGui.Setup(true);
    SceneManager.LoadScene<MyScene>();
}

protected override void Update(float dt)
{
    PerformanceMonitor.Update(dt);
    SceneManager.Update(dt);
}

protected override void Render()
{
    SceneManager.Render();       // 2D scene

    rlImGui.Begin();
    SceneManager.RenderUI();     // ImGui (native ImGuiNET)
    PerformanceMonitor.Draw();
    rlImGui.End();

    Raylib.DrawFPS(Width - 90, 10);
}
```

Create scene classes in your own application project.
