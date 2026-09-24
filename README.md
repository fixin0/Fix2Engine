
# Fix2Engine

Fix2Engine is a modular, code-first game engine built with C# and .NET, using Raylib through its C# bindings, Raylib-cs, for window management and 2D rendering. It is designed to offer full control over engine logic while keeping core systems separate, lightweight, and easy to maintain.

## Architecture Overview

The repository is organized into distinct class libraries to isolate responsibilities across the engine pipeline:

* **Core:** Engine-owned Color32 / RectF types and backend contracts.
* **Backends/Raylib:** Native rendering, input and window implementation. Native types stay in this project.
* **Graphics:** Public texture, sprite and RenderContext API.
* **Runner:** FixGame lifecycle, automatic scene dispatch and cleanup. Games start with Fix2.Run<Game>().
* **Components:** Provides inheritable `Object2D` entities, static and animated sprites, and `FixScene` ownership and lifecycle management.
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
├── Core/           # Public primitives and backend contracts
├── Backends/Raylib/ # Native implementation
├── Runner/         # Game host and lifecycle
├── Audio/          # Sound management
├── Components/     # Scene and component structures
├── Graphics/       # Raylib rendering/windowing abstraction
├── Input/          # Cross-platform keyboard input (Win32 + Raylib)
├── Monitoring/     # Performance monitor ImGui overlay
├── Physics/        # Collision and physics logic
├── User/           # Platform info helper
└── Fix2Console/    # Project creation and configuration CLI
```

Games use Fix2Engine APIs without importing Raylib or managing native UI frames.

Documentation: choose the [English or Turkish tutorial](Documents/README.md). It
covers setup, game settings, text, sprites, animation, a basic glow effect, and
publishing.

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

Replace `/path/to/Fix2Engine` with your engine checkout. See the
[tutorial index](Documents/README.md) for project setup and scene creation.

## License and third-party software

Fix2Engine's own code is licensed under the [MIT license](LICENSE). Raylib,
Raylib-cs, ImGui, Tomlyn, Silk.NET, and other dependencies retain their respective
licenses. Their license texts and copyright notices are collected in
[THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt).

Builds and publishes include `Fix2Engine-LICENSE.txt` and
`Fix2Engine-THIRD-PARTY-NOTICES.txt`; keep both files with redistributed builds.
Review [LICENSE](LICENSE) and [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt)
before redistribution.

## Contributing

Read [CONTRIBUTOR.MD](CONTRIBUTOR.MD) for setup, testing, contribution licensing,
and third-party code and asset requirements.
