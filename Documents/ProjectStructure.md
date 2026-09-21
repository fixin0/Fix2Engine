# Project Structure

## Solution

`Fix2Engine.sln` (Format 12.00) — 7 projects:

```
Fix2Engine.sln
├── Graphics   (class library)
├── Components (class library)
├── Input      (class library)
├── User       (class library)  — platform info (Platform)
├── Physics    (class library)  — stub (PhysicsWorld)
├── Audio      (class library)  — stub
├── Monitoring (class library)  — performance monitor overlay
└── Game                  (executable)
```

Plus tool project `Fix2Console` (CLI for `Fix2Console --new-project` scaffolding, not part of runtime).

## Directory Layout

```
Fix2Engine/
├── Fix2Engine.sln
├── Documents/                  ← you are here
│   ├── README.md
│   ├── GettingStarted.md
│   ├── WindowingAndGameLoop.md
│   ├── Input.md
│   ├── Graphics.md
│   ├── Camera.md
│   ├── SceneManagement.md
│   ├── IMGUI.md                (legacy — now native ImGui, see below)
│   ├── Components.md
│   ├── PhysicsAndAudio.md
│   ├── Monitoring.md
│   └── ProjectStructure.md
├── Graphics/
│   ├── Graphics.csproj
│   ├── Windowing.cs
│   ├── Model3D.cs
│   ├── Sprite2D.cs
│   └── Skybox.cs
├── Components/
│   ├── Components.csproj
│   ├── Cameras/Camera.cs       (namespace Fix2Engine.Components.Cameras)
│   ├── Scene/IFixScene.cs
│   ├── Scene/SceneLoader.cs   (SceneManager)
│   ├── Node2D.cs               (was PineObject2D)
│   └── Node3D.cs               (was Character3D)
├── Input/
│   ├── Input.csproj
│   ├── InputBackend/Input.cs           (class InputManager, namespace Fix2Engine.Input, platform dispatch)
│   ├── InputBackend/WindowsInputBackend.cs (was InputBackend_Windows.cs)
│   ├── InputBackend/Keys.cs
│   ├── InputBackend/KeyState.cs
│   ├── InputConfig.cs
│   └── ConfigReader.cs
├── User/
│   ├── User.csproj
│   └── Platform.cs               (platform info helper)
├── Physics/
│   ├── Physics.csproj
│   └── PhysicsWorld.cs         (was Class1.cs)
├── Audio/
│   ├── Audio.csproj
│   └── OpenAL/SoundManager.cs (stub)
├── Monitoring/
│   ├── Monitoring.csproj
│   └── PerformanceMonitor.cs   (FPS/frame-time/CPU/GPU ImGui overlay)
├── Fix2Console/
│   ├── Fix2Console.csproj
│   ├── Terminal.cs
│   ├── ProjectGenerator.cs
│   ├── TemplateGenerator.cs
│   ├── EngineRootFinder.cs
│   └── ProjectValidator.cs
└── Game/
    ├── Game.csproj
    ├── Program.cs
    ├── Game.cs                (Windowing subclass, uses PerformanceMonitor)
    ├── MainMenuScene.cs
    ├── Debug3DScene.cs
    └── Debug2DPixelScene.cs
```

## Project References

```
Game ──→ Components ──→ Graphics ──→ Input
     ──→ Graphics
     ──→ Input
     ──→ Monitoring ──→ Graphics

Graphics ──→ Input
Components ──→ Graphics
Input ──→ User
```

`Physics` and `Audio` are not yet referenced by `Game` (stubs). `Fix2Engine.IMGUI` was removed — UI now uses native `ImGui.NET` + `rlImGui-cs` directly.

## Package References

| Project | Packages |
|---------|----------|
| `Graphics` | `Raylib-cs 8.0.0`, `rlImgui-cs 3.2.0` |
| `Components` | `Raylib-cs 8.0.0` |
| `Input` | `Tomlyn 2.10.1`, `Raylib-cs 8.0.0` |
| `Audio` | `Silk.NET.OpenAL 2.23.0` |
| `Physics` | — |
| `Monitoring` | `ImGui.NET 1.91.6.1`, `rlImgui-cs 3.2.0`, `System.Diagnostics.PerformanceCounter 8.0.0` |
| `Game` | `rlImgui-cs 3.2.0` |

All projects: `<TargetFramework>net10.0</TargetFramework>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<Nullable>enable</Nullable>`, `AllowUnsafeBlocks=true` where needed (Graphics, Game).

## Build

```bash
dotnet build          # builds entire solution
dotnet run --project Game
dotnet publish --project Game -c Release
```

## Conventions

- **Namespaces:** `Fix2Engine.*` consistently. `Camera` is in `Fix2Engine.Components.Cameras`, `InputManager` in `Fix2Engine.Input`, `WindowsInputBackend` in `Fix2Engine.Input.InputBackend`.
- **Files:** `PascalCase.cs` without underscores (`WindowsInputBackend.cs` not `InputBackend_Windows.cs`).
- **Scene graph:** `Node2D` / `Node3D` are the base scene-graph nodes (replacing `PineObject2D`/`Character3D`).
- **Scenes:** implement `IFixScene` + `IDisposable`; register via `SceneManager.LoadScene<T>()`.
- **UI:** native `ImGuiNET` inside `rlImGui.Begin()`/`End()` — `Game.Render()` already does this around `SceneManager.RenderUI()`. No wrapper library.
- **Input:** poll via `InputManager.IsDown/IsPressed/IsReleased(Keys.X)` (or `Fix2Engine.Input.InputManager`) inside `Update`; do not call `InputManager.Update()` yourself (handled by `Windowing`). Backend is auto-selected by OS — Win32 on Windows, Raylib on Linux/macOS.
- **Performance:** `PerformanceMonitor` (`Fix2Engine.Monitoring`) draws the ImGui overlay (FPS, frame time, CPU/GPU graphs). Call `PerformanceMonitor.Update(dt)` in `Update` and `PerformanceMonitor.Draw()` (optionally with a context line) inside `rlImGui.Begin()`/`End()`.
- **Unsafe code:** `Model3D.SetTexture` and `Skybox` use `unsafe` to assign Raylib material maps — project must allow unsafe blocks.
