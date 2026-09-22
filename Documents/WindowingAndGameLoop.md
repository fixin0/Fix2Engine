# Windowing & Game Loop

Source: `Graphics/Windowing.cs` — namespace `Fix2Engine.Graphics`

## Class

```csharp
public class Windowing : IDisposable
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Title { get; set; }

    public Windowing(int width, int height, string title);
    protected virtual void Init();
    protected virtual void Start();
    protected virtual void Update(float dt);
    protected virtual void FixedUpdate(float fixedDt);
    protected virtual void Render();
    public void Run();
    public void Dispose();
}
```

## Lifecycle

```
ctor(width, height, title)
  → Init()                      // before InitWindow — override for Raylib config
  → InitWindow(width,height,title)
  → SetTargetFPS(240)

Run()
  → Start()                     // once, after window is open
  → loop while (!WindowShouldClose())
      dt = GetFrameTime()
      accumulator += dt
      while (accumulator >= 1/60)
          FixedUpdate(1/60)     // deterministic 60 Hz
          accumulator -= 1/60
      Fix2Engine.Input.InputManager.Update() // polls all keys via OS-appropriate backend
      Update(dt)                // per-frame logic
      BeginDrawing()
        Render()               // your drawing (ClearBackground etc.)
      EndDrawing()
  → Dispose() → CloseWindow()
```

## Overriding

Inherit and override only what you need:

```csharp
public class Game : Windowing
{
    public Game() : base(1280, 720, "My Application") { }

    protected override void Init()
    {
        // e.g. SetConfigFlags(ConfigFlags.Msaa4xHint);
    }

    protected override void Start()
    {
        rlImGui.Setup(true);
        ApplyImGuiTheme();
        SceneManager.LoadScene<MyScene>();
    }

    protected override void Update(float dt)
    {
        PerformanceMonitor.Update(dt);
        SceneManager.Update(dt);
    }

    protected override void FixedUpdate(float fixedDt)
    {
        // physics / collision here — runs at fixed 60 Hz
    }

    protected override void Render()
    {
        Raylib.ClearBackground(new Color(15, 15, 20, 255));
        SceneManager.Render();
        rlImGui.Begin();
        SceneManager.RenderUI();
        PerformanceMonitor.Draw($"Resolution: {Width}x{Height}   Scene: {SceneManager.CurrentScene?.GetType().Name}");
        rlImGui.End();
    }
}
```

`ApplyImGuiTheme()` sets `ImGui.GetStyle()` colors/rounding natively (no `Fix2Engine.IMGUI` wrapper). `PerformanceMonitor` (namespace `Fix2Engine.Monitoring`) owns the performance overlay — see `Monitoring.md`.

## Notes

- `FixedUpdate` is ideal for physics. Use `Update` for input and gameplay.
- `Fix2Engine.Input.InputManager.Update()` is called automatically before `Update` — do not call it yourself.
- `Width`/`Height`/`Title` are mutable but changing them does not resize the window; use Raylib `SetWindowSize` / `SetWindowTitle` if needed.
- `Run()` blocks until the window closes. Call it once from `Program.Main`.
