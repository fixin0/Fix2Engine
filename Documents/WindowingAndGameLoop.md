# Windowing & Game Loop

Source: `Graphics/Windowing.cs` — namespace `Fix2Engine.Graphics`.

Inherit `Windowing` in your application's entry class. The constructor calls `Init`
before creating the Raylib window. `Run` starts the application once and guarantees
cleanup if startup, updates or rendering throw.

## Frame Order

```text
constructor → Init → InitWindow
Run → Start
    each frame:
        InputManager.Update
        Update(dt)                 # applies pending scene switches
        FixedUpdate(1/60)           # zero or more steps
        BeginDrawing
            Render
        EndDrawing
    Dispose → OnUnload → CloseWindow
```

Fixed updates accumulate frame time at 60 Hz. Catch-up is capped at 0.25 seconds per
frame to keep long stalls from causing an unbounded loop. Input is polled before
updates; do not poll it again in your scene. Use key press/release actions in
`OnUpdate` so a pressed key is handled once per frame, not once per fixed step.

## Scene Integration

```csharp
public class Game : Fix2Engine.Graphics.Windowing
{
    public Game() : base(1280, 720, "My Application") { }

    protected override void Start()
    {
        rlImGui.Setup(true);
        SceneManager.LoadScene<MyScene>();
    }

    protected override void Update(float dt) => SceneManager.Update(dt);
    protected override void FixedUpdate(float dt) => SceneManager.FixedUpdate(dt);

    protected override void Render()
    {
        SceneManager.Render();
        rlImGui.Begin();
        SceneManager.RenderUI();
        rlImGui.End();
    }

    protected override void OnUnload()
    {
        try { SceneManager.Unload(); }
        finally { rlImGui.Shutdown(); }
    }
}
```

Use namespaces `Fix2Engine.Components` and `rlImGui_cs` for this example. Define
`MyScene` in your application. `Fix2Console` generates this lifecycle wiring.

`Dispose` is idempotent. `OnUnload` runs once after startup, while the graphics
context is still alive; release scenes, textures and UI resources there. The window
also closes if `OnUnload` throws.

`Width`, `Height` and `Title` store application values. Use Raylib's `SetWindowSize`
and `SetWindowTitle` to change the actual window. `Run` can be called only once.
See [Monitoring](Monitoring.md) to add the optional performance overlay.
