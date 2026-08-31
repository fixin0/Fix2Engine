
# Fix2Engine

Fix2Engine is a modular, code-first game engine built with C# and .NET. It is designed to offer full control over engine logic while keeping core systems separate, lightweight, and easy to maintain.

---

<img width="1276" height="752" alt="f2engine_2demo" src="https://github.com/user-attachments/assets/0d17fe5a-4ca3-4463-b47d-86e44669243b" />
<img width="1277" height="752" alt="image" src="https://github.com/user-attachments/assets/be7ed4f1-0d1a-41a4-8aee-24af9cdc4f4f" />


## Architecture Overview

The repository is organized into distinct class libraries to isolate responsibilities across the engine pipeline:

* **Fix2Engine.Graphics:** Handles windowing, rendering, 3D model loading, 2D sprites, and skybox drawing. Uses Raylib (`Raylib-cs`) as its rendering backend to provide low-level operations without heavy abstractions.
* **Fix2Engine.Components:** Implements a component-based scene and object structure (`IFixScene`, `Node2D`/`Node3D`, camera controls).
* **Fix2Engine.Input:** Cross-platform keyboard polling — Win32 backend on Windows, Raylib backend on Linux/macOS — plus optional TOML action maps.
* **Fix2Engine.User:** Platform information helper.
* **Fix2Engine.Monitoring:** Built-in `PerformanceMonitor` ImGui overlay (FPS, frame time, CPU/GPU).
* **Fix2Engine.Audio:** Manages sound effects, music playback, and audio asset lifecycles.
* **Fix2Engine.Physics:** Encapsulates collision detection, spatial checks, and physical movement logic.
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
├── Fix2Engine.Audio/          # Sound management
├── Fix2Engine.Components/     # Scene and component structures
├── Fix2Engine.Graphics/       # Raylib rendering/windowing abstraction
├── Fix2Engine.Input/          # Cross-platform keyboard input (Win32 + Raylib)
├── Fix2Engine.Monitoring/     # Performance monitor ImGui overlay
├── Fix2Engine.Physics/        # Collision and physics logic
├── Fix2Engine.User/           # Platform info helper
└── Game/                      # Project runtime and test scenes
```

Documentation: see [Documents/](Documents/README.md).

## Getting Started
Clone the repository:


```git clone https://github.com/fixin0/Fix2Engine.git```
<br></br>
Open Fix2Engine.sln in Visual Studio or JetBrains Rider.

Set Game as the startup project and run.

Documentation: Coming soon...
