
# Fix2Engine

Fix2Engine is a modular, code-first game engine built with C# and .NET, using Raylib through its C# bindings, Raylib-cs, for window management and 2D rendering. It is designed to offer full control over engine logic while keeping core systems separate, lightweight, and easy to maintain.

## Architecture Overview

The repository is organized into distinct class libraries to isolate responsibilities across the engine pipeline:

* **Graphics:** Handles windowing, rendering, 2D sprites and drawing. Uses Raylib (`Raylib-cs`) as its rendering backend to provide low-level operations without heavy abstractions.
* **Components:** Implements a component-based scene and object structure (`IFixScene`, `Node2D`).
* **Input:** Cross-platform keyboard polling — Win32 backend on Windows, Raylib backend on Linux/macOS — plus optional TOML action maps.
* **User:** Platform information helper.
* **Monitoring:** Built-in `PerformanceMonitor` ImGui overlay (FPS, frame time, CPU/GPU).
* **Audio:** Manages sound effects, music playback, and audio asset lifecycles.
* **Physics:** Encapsulates collision detection, spatial checks, and physical movement logic.
* **Fix2Console:** Creates standalone projects and configures their engine references and input maps.

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
└── Fix2Console/    # Project creation and configuration CLI
```

Documentation: see [Documents/](Documents/README.md).

## Build and create a project

Build the engine libraries and CLI:

```bash
dotnet build Fix2Engine.sln -m:1
```

From the directory where you keep your projects, create a standalone application:

```bash
dotnet run --project /path/to/Fix2Engine/Fix2Console -- --new-project MyGame
cd MyGame
dotnet run
```

Replace `/path/to/Fix2Engine` with your engine checkout. See
[Getting Started](Documents/GettingStarted.md) for project setup and scene creation.

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
