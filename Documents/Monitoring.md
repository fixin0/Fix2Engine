# Monitoring & Performance Monitor

Source: `Monitoring/PerformanceMonitor.cs` — namespace `Fix2Engine.Monitoring`

The engine's built-in performance overlay (FPS, frame time, CPU, GPU, memory) lives in its own project so the game loop stays clean.

```csharp
using Fix2Engine.Monitoring;
```

## Usage

`PerformanceMonitor` is a **static, self-contained** class. It only needs two calls — one per frame in `Update`, one inside the ImGui `Begin`/`End` block in `Render`:

```csharp
protected override void Update(float dt)
{
    PerformanceMonitor.Update(dt);
    // ...your scene/game logic
}

protected override void Render()
{
    SceneManager.Render();

    rlImGui.Begin();
    SceneManager.RenderUI();
    PerformanceMonitor.Draw();                    // simple
    PerformanceMonitor.Draw($"Resolution: {Width}x{Height}   Scene: ..."); // optional context line
    rlImGui.End();
}
```

- `PerformanceMonitor.Update(float dt)` — samples CPU/GPU usage and records the FPS / frame-time / CPU / GPU history arrays. Call once per frame.
- `PerformanceMonitor.Draw(string? contextLine = null)` — renders the ImGui window. Must be called inside `rlImGui.Begin()` / `rlImGui.End()` (which `Windowing.Render()` already wraps, as does `Game.Render()`).
- `PerformanceMonitor.Visible { get; set; }` (default `true`) — toggles the overlay from script at runtime.

## What It Shows

- **FPS** and frame time (ms)
- **CPU** utilization (via `Process.TotalProcessorTime` delta)
- **GPU** utilization (via Windows `PerformanceCounter` "GPU Engine" when available; otherwise an estimated fallback derived from frame time)
- **Memory** (managed heap, MB) and GC collection count
- **Cores / OS**, plus an optional per-app context line (e.g. resolution and current scene)

## Platform Notes

- **GPU counter** is Windows-only (`System.Diagnostics.PerformanceCounter`). On Linux/macOS it gracefully falls back to a frame-time-based estimate — no crash, just a soft number.
- `Fix2Engine.Monitoring` depends on `ImGui.NET`, `rlImgui-cs`, `System.Diagnostics.PerformanceCounter`, and `Fix2Engine.Graphics` (transitively `Raylib-cs`).

## Dependencies

Add a project reference to `Fix2Engine.Monitoring` from your game/executable project:

```xml
<ItemGroup>
  <ProjectReference Include="..\Monitoring\Monitoring.csproj" />
</ItemGroup>
```
