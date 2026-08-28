# Project Structure

## Solution

`Fix2Engine.sln` (Format 12.00) — 7 projects:

```
Fix2Engine.sln
├── Fix2Engine.Graphics   (class library)
├── Fix2Engine.Components (class library)
├── Fix2Engine.Input      (class library)
├── Fix2Engine.IMGUI      (class library)
├── Fix2Engine.Physics    (class library)  — stub
├── Fix2Engine.Audio      (class library)  — stub
└── Game                  (executable)
```

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
│   ├── IMGUI.md
│   ├── Components.md
│   ├── PhysicsAndAudio.md
│   └── ProjectStructure.md
├── Fix2Engine.Graphics/
│   ├── Fix2Engine.Graphics.csproj
│   ├── Windowing.cs
│   ├── Model3D.cs
│   ├── Sprite2D.cs
│   ├── Skybox.cs
│   └── Shaders/               (empty, reserved)
├── Fix2Engine.Components/
│   ├── Fix2Engine.Components.csproj
│   ├── Cameras/Camera.cs
│   ├── Scene/IFixScene.cs
│   ├── Scene/SceneLoader.cs   (SceneManager)
│   ├── PineObject2D.cs
│   └── Character3D.cs         (stub)
├── Fix2Engine.Input/
│   ├── Fix2Engine.Input.csproj
│   ├── InputBackend/Input.cs
│   ├── InputBackend/InputBackend_Windows.cs
│   ├── InputBackend/Keys.cs
│   ├── InputBackend/KeyState.cs
│   ├── InputConfig.cs
│   └── ConfigReader.cs
├── Fix2Engine.IMGUI/
│   ├── Fix2Engine.IMGUI.csproj
│   ├── IMGUI.cs
│   ├── Widgets.cs
│   ├── Modal.cs
│   ├── Theme.cs
│   ├── UIStyleScope.cs
│   └── Palette (inside Theme.cs)
├── Fix2Engine.Physics/
│   ├── Fix2Engine.Physics.csproj
│   └── Class1.cs              (stub)
├── Fix2Engine.Audio/
│   ├── Fix2Engine.Audio.csproj
│   └── OpenAL/SoundManager.cs (stub)
└── Game/
    ├── Game.csproj
    ├── Program.cs
    ├── Game.cs                (Windowing subclass)
    ├── MainMenuScene.cs
    ├── Debug3DScene.cs
    ├── Debug2DPixelScene.cs
    ├── FpsDemoScene.cs
    └── Racing3DScene.cs
```

## Project References

```
Game ──→ Fix2Engine.Components ──→ Fix2Engine.Graphics ──→ Fix2Engine.Input
     ──→ Fix2Engine.Graphics
     ──→ Fix2Engine.IMGUI
     ──→ Fix2Engine.Input

Fix2Engine.Graphics ──→ Fix2Engine.Input
Fix2Engine.Components ──→ Fix2Engine.Graphics
```

`Physics` and `Audio` are not yet referenced by `Game` (stubs).

## Package References

| Project | Packages |
|---------|----------|
| `Graphics` | `Raylib-cs 8.0.0`, `rlImgui-cs 3.2.0` |
| `Components` | `Raylib-cs 8.0.0` |
| `IMGUI` | `Raylib-cs 8.0.0`, `rlImgui-cs 3.2.0` (→ `ImGui.NET`) |
| `Input` | `Tomlyn 2.10.1` |
| `Audio` | `Silk.NET.OpenAL 2.23.0` |
| `Physics` | — |
| `Game` | `rlImgui-cs 3.2.0` |

All projects: `<TargetFramework>net10.0</TargetFramework>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<Nullable>enable</Nullable>`, `AllowUnsafeBlocks=true` where needed (Graphics, Game).

## Build

```bash
dotnet build          # builds entire solution
dotnet run --project Game
dotnet publish --project Game -c Release
```

## Conventions

- **Namespaces:** mostly `Fix2Engine.*`, but `Input` lives in `InputManager` (global) and `Camera` is in `Fix2Engine.Graphics` despite being under `Components` — be aware when adding `using` directives.
- **Scenes:** implement `IFixScene` + `IDisposable`; register via `SceneManager.LoadScene<T>()`.
- **UI:** must be inside `rlImGui.Begin()`/`End()` — `Game.Render()` already does this around `SceneManager.RenderUI()`.
- **Input:** poll via `InputManager.Input.IsDown/IsPressed/IsReleased(Keys.X)` inside `Update`; do not call `Input.Update()` yourself (handled by `Windowing`).
- **Unsafe code:** `Model3D.SetTexture` and `Skybox` use `unsafe` to assign Raylib material maps — project must allow unsafe blocks.
