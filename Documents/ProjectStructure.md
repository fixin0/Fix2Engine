# Project Structure

## Solution

`Fix2Engine.sln` (Format 12.00) — 6 projects:

```
Fix2Engine.sln
├── Fix2Engine.Graphics   (class library)
├── Fix2Engine.Components (class library)
├── Fix2Engine.Input      (class library)
├── Fix2Engine.Physics    (class library)  — stub (PhysicsWorld)
├── Fix2Engine.Audio      (class library)  — stub
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
│   └── ProjectStructure.md
├── Fix2Engine.Graphics/
│   ├── Fix2Engine.Graphics.csproj
│   ├── Windowing.cs
│   ├── Model3D.cs
│   ├── Sprite2D.cs
│   └── Skybox.cs
├── Fix2Engine.Components/
│   ├── Fix2Engine.Components.csproj
│   ├── Cameras/Camera.cs       (namespace Fix2Engine.Components.Cameras)
│   ├── Scene/IFixScene.cs
│   ├── Scene/SceneLoader.cs   (SceneManager)
│   ├── Node2D.cs               (was PineObject2D)
│   └── Node3D.cs               (was Character3D)
├── Fix2Engine.Input/
│   ├── Fix2Engine.Input.csproj
│   ├── InputBackend/Input.cs           (class InputManager, namespace Fix2Engine.Input)
│   ├── InputBackend/WindowsInputBackend.cs (was InputBackend_Windows.cs)
│   ├── InputBackend/Keys.cs
│   ├── InputBackend/KeyState.cs
│   ├── InputConfig.cs
│   └── ConfigReader.cs
├── Fix2Engine.Physics/
│   ├── Fix2Engine.Physics.csproj
│   └── PhysicsWorld.cs         (was Class1.cs)
├── Fix2Engine.Audio/
│   ├── Fix2Engine.Audio.csproj
│   └── OpenAL/SoundManager.cs (stub)
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
    ├── Game.cs                (Windowing subclass, ShowPerformanceMonitor property)
    ├── MainMenuScene.cs
    ├── Debug3DScene.cs
    └── Debug2DPixelScene.cs
```

## Project References

```
Game ──→ Fix2Engine.Components ──→ Fix2Engine.Graphics ──→ Fix2Engine.Input
     ──→ Fix2Engine.Graphics
     ──→ Fix2Engine.Input

Fix2Engine.Graphics ──→ Fix2Engine.Input
Fix2Engine.Components ──→ Fix2Engine.Graphics
```

`Physics` and `Audio` are not yet referenced by `Game` (stubs). `Fix2Engine.IMGUI` was removed — UI now uses native `ImGui.NET` + `rlImGui-cs` directly.

## Package References

| Project | Packages |
|---------|----------|
| `Graphics` | `Raylib-cs 8.0.0`, `rlImgui-cs 3.2.0` |
| `Components` | `Raylib-cs 8.0.0` |
| `Input` | `Tomlyn 2.10.1` |
| `Audio` | `Silk.NET.OpenAL 2.23.0` |
| `Physics` | — |
| `Game` | `rlImgui-cs 3.2.0`, `ImGui.NET 1.91.6.1`, `System.Diagnostics.PerformanceCounter 8.0.0` |

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
- **Input:** poll via `InputManager.IsDown/IsPressed/IsReleased(Keys.X)` (or `Fix2Engine.Input.InputManager`) inside `Update`; do not call `InputManager.Update()` yourself (handled by `Windowing`).
- **Performance:** `Game.ShowPerformanceMonitor { get; set; }` controls the ImGui performance overlay (FPS, frame time, CPU/GPU graphs).
- **Unsafe code:** `Model3D.SetTexture` and `Skybox` use `unsafe` to assign Raylib material maps — project must allow unsafe blocks.
