# Scene Management

Sources:
- `Fix2Engine.Components/Scene/IFixScene.cs` — namespace `Fix2Engine.Components.Scene`
- `Fix2Engine.Components/Scene/SceneLoader.cs` — namespace `Fix2Engine.Components` (class `SceneManager`)

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
| `Start()` | Once, when scene becomes current | Init camera, spawn objects, load models/textures |
| `Update(float dt)` | Every frame (`dt` = frame time) | Input, gameplay, AI, physics |
| `Render()` | Every frame, inside `BeginDrawing`/`EndDrawing` | 3D/2D world drawing (between `Camera.Begin`/`End`) |
| `RenderUI()` | Every frame, inside `rlImGui.Begin`/`End` | ImGui panels, HUD, menus |
| `Unload()` | Rarely used (use `Dispose` instead) | Custom cleanup if needed |
| `Dispose()` | On scene switch and on app exit | Unload models, textures, etc. |

Minimal scene:

```csharp
public class MyScene : IFixScene
{
    private Camera _camera;

    public void Start()
    {
        _camera = new Camera(new Vector3(0, 2, 10), Vector3.Zero, 75f, CameraType.FirstPerson);
    }

    public void Update(float dt)
    {
        if (InputManager.Input.IsPressed(Keys.Escape))
            SceneManager.LoadScene<MainMenuScene>();
        _camera.Update();
    }

    public void Render()
    {
        _camera.Begin();
        ClearBackground(new Color(30, 30, 40, 255));
        DrawGrid(20, 1.0f);
        _camera.End();
    }

    public void RenderUI()
    {
        if (IMGUI.BeginCenteredPanel("HUD", new Vector2(200, 80)))
        {
            IMGUI.Header("MY SCENE");
            ImGui.Text("Hello, world!");
            IMGUI.EndPanel();
        }
    }

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
SceneManager.LoadScene<MainMenuScene>();          // generic — creates via new()
SceneManager.LoadScene(new FpsDemoScene());       // instance
SceneManager.LoadScene<Debug3DScene>();
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

`Game` (inherits `Windowing`) delegates to `SceneManager`:

```csharp
protected override void Start()
{
    rlImGui.Setup(true);
    Theme.ApplyDark();
    SceneManager.LoadScene<MainMenuScene>();
}

protected override void Update(float dt) => SceneManager.Update(dt);

protected override void Render()
{
    SceneManager.Render();       // 3D world

    rlImGui.Begin();
    SceneManager.RenderUI();     // ImGui
    rlImGui.End();

    Raylib.DrawFPS(Width - 90, 10);
}
```

Current scenes in `Game/`: `MainMenuScene`, `Debug3DScene`, `Debug2DPixelScene`, `FpsDemoScene`, `Racing3DScene`.
