
# Fix2Engine

Fix2Engine is a modular, code-first game engine built with C# and .NET, using Raylib through its C# bindings, Raylib-cs, for window management and 2D/3D rendering. It is designed to offer full control over engine logic while keeping core systems separate, lightweight, and easy to maintain.

---

<img width="1276" height="752" alt="f2engine_2demo" src="https://github.com/user-attachments/assets/0d17fe5a-4ca3-4463-b47d-86e44669243b" />
<img width="1277" height="752" alt="image" src="https://github.com/user-attachments/assets/be7ed4f1-0d1a-41a4-8aee-24af9cdc4f4f" />


## Architecture Overview

The repository is organized into distinct class libraries to isolate responsibilities across the engine pipeline:

* **Graphics:** Handles windowing, rendering, 3D model loading, 2D sprites, and skybox drawing. Uses Raylib (`Raylib-cs`) as its rendering backend to provide low-level operations without heavy abstractions.
* **Components:** Implements a component-based scene and object structure (`IFixScene`, `Node2D`/`Node3D`, camera controls).
* **Input:** Cross-platform keyboard polling — Win32 backend on Windows, Raylib backend on Linux/macOS — plus optional TOML action maps.
* **User:** Platform information helper.
* **Monitoring:** Built-in `PerformanceMonitor` ImGui overlay (FPS, frame time, CPU/GPU).
* **Audio:** Manages sound effects, music playback, and audio asset lifecycles.
* **Physics:** Encapsulates collision detection, spatial checks, and physical movement logic.
* **Game:** The entry point/sandbox project used to assemble scenes (`Debug2DPixelScene`, `Debug3DScene`), test features, and run game code.

---

## Tech Stack

* **Language & Runtime:** C# / .NET 10
* **Rendering:** Raylib (`Raylib-cs`)
* **GUI / Inspector:** ImGuiNET (`Dear ImGui` bindings) via `rlImGui-cs`

---

## Project Structure

```text
Fix2Engine/
├── Audio/          # Sound management
├── Components/     # Scene and component structures
├── Graphics/       # Raylib rendering/windowing abstraction
├── Input/          # Cross-platform keyboard input (Win32 + Raylib)
├── Monitoring/     # Performance monitor ImGui overlay
├── Physics/        # Collision and physics logic
├── User/           # Platform info helper
└── Game/                      # Project runtime and test scenes
```

Documentation: see [Documents/](Documents/README.md).

## License and third-party software

Fix2Engine's own code is licensed under the [MIT license](LICENSE). Raylib,
Raylib-cs, ImGui, Tomlyn, Silk.NET, and other dependencies retain their respective
licenses. Their license texts and copyright notices are collected in
[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt).

Builds and publishes include `Fix2Engine-LICENSE.txt` and
`Fix2Engine-THIRD-PARTY-NOTICES.txt`; keep both files with redistributed builds.
See [licensing and release notes](Documents/Licensing.md) for scope and maintenance.

## Contributing

Read [CONTRIBUTOR.MD](CONTRIBUTOR.MD) for setup, testing, contribution licensing,
and third-party code and asset requirements.
