# Project Structure

## Solution

`Fix2Engine.sln` (Format 12.00) — 8 projects:

```
Fix2Engine.sln
├── Graphics   (class library)
├── Components (class library)
├── Input      (class library)
├── User       (class library)  — platform info (Platform)
├── Physics    (class library)  — stub (PhysicsWorld)
├── Audio      (class library)  — stub
├── Monitoring (class library)  — performance monitor overlay
└── Fix2Console (CLI executable)
```

`Fix2Console` creates standalone application projects and is not part of their runtime.

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
│   ├── SceneManagement.md
│   ├── IMGUI.md                (legacy — now native ImGui, see below)
│   ├── Components.md
│   ├── PhysicsAndAudio.md
│   ├── Monitoring.md
│   └── ProjectStructure.md
├── Graphics/
│   ├── Graphics.csproj
│   ├── Windowing.cs
│   └── Sprite2D.cs
├── Components/
│   ├── Components.csproj
│   ├── Scene/IFixScene.cs
│   ├── Scene/SceneLoader.cs   (SceneManager)
│   └── Node2D.cs
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
└── Tests/InputSystem/
    ├── InputSystem.Tests.csproj
    ├── InputMap.toml           (independent test fixture)
    └── Program.cs
```

## Project References

```
Components ──→ Graphics ──→ Input ──→ User
Monitoring ──→ Graphics
Fix2Console ──→ Input
```

`Physics` and `Audio` are stubs referenced by generated applications. `Fix2Engine.IMGUI` was removed — UI now uses native `ImGui.NET` + `rlImGui-cs` directly.

## Package References

| Project | Packages |
|---------|----------|
| `Graphics` | `Raylib-cs 8.0.0`, `rlImgui-cs 3.2.0` |
| `Components` | `Raylib-cs 8.0.0` |
| `Input` | `Tomlyn 2.10.1`, `Raylib-cs 8.0.0` |
| `Audio` | `Silk.NET.OpenAL 2.23.0` |
| `Physics` | — |
| `Monitoring` | `ImGui.NET 1.91.6.1`, `rlImgui-cs 3.2.0`, `System.Diagnostics.PerformanceCounter 8.0.0` |

All projects: `<TargetFramework>net10.0</TargetFramework>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<Nullable>enable</Nullable>`.

## Build

```bash
dotnet build Fix2Engine.sln -m:1
dotnet run --project Tests/InputSystem/InputSystem.Tests.csproj
```

## Conventions

- **Namespaces:** `Fix2Engine.*` consistently. `InputManager` in `Fix2Engine.Input`, `WindowsInputBackend` in `Fix2Engine.Input.InputBackend`.
- **Files:** `PascalCase.cs` without underscores (`WindowsInputBackend.cs` not `InputBackend_Windows.cs`).
- **Scene graph:** `Node2D` is the base scene-graph node.
- **Scenes:** implement `IFixScene` + `IDisposable`; register via `SceneManager.LoadScene<T>()`.
- **UI:** native `ImGuiNET` inside `rlImGui.Begin()`/`End()` — wrap `SceneManager.RenderUI()` in this block in your application's `Render()` override. No wrapper library.
- **Input:** poll via `InputManager.IsDown/IsPressed/IsReleased(Keys.X)` (or `Fix2Engine.Input.InputManager`) inside `Update`; do not call `InputManager.Update()` yourself (handled by `Windowing`). Backend is auto-selected by OS — Win32 on Windows, Raylib on Linux/macOS.
- **Performance:** `PerformanceMonitor` (`Fix2Engine.Monitoring`) draws the ImGui overlay (FPS, frame time, CPU/GPU graphs). Call `PerformanceMonitor.Update(dt)` in `Update` and `PerformanceMonitor.Draw()` (optionally with a context line) inside `rlImGui.Begin()`/`End()`.
