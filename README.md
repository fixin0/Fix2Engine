# Fix2Engine

Fix2Engine is a modular, code-first game engine built with C# and .NET. It is designed to offer full control over engine logic while keeping core systems separate, lightweight, and easy to maintain.

---

<img width="1277" height="718" alt="image" src="https://github.com/user-attachments/assets/63c42f76-a220-4fa1-a87a-786a0f3ca790" />

_The model shown in the image was obtained from mixamo.com._


## Architecture Overview

The repository is organized into distinct class libraries to isolate responsibilities across the engine pipeline:

* **Fix2Engine.Graphics:** Handles rendering, 3D model loading, skybox drawing, and camera management. This module uses Raylib (`Raylib-cs`) as its rendering backend to provide low-level graphics operations without the overhead of heavy abstractions.
* **Fix2Engine.Components:** Implements a component-based scene and object structure (`IFixScene`, scene camera controls, object properties).
* **Fix2Engine.IMGUI:** Integrates Dear ImGui (`ImGuiNET`) to render in-engine inspector windows, debug overlays, and runtime control widgets.
* **Fix2Engine.Audio:** Manages sound effects, music playback, and audio asset lifecycles.
* **Fix2Engine.Physics:** Encapsulates collision detection, spatial checks, and physical movement logic.
* **Game:** The entry point/sandbox project used to assemble scenes (`Debug3DScene`), test features, and run game code.

---

## Tech Stack

* **Language & Runtime:** C# / .NET
* **Rendering:** Raylib (`Raylib-cs`)
* **GUI / Inspector:** ImGuiNET (`Dear ImGui` bindings)
* **IDE Support:** JetBrains Rider / Visual Studio

---

## Project Structure

```text
Fix2Engine/
├── Fix2Engine.Audio/          # Sound management
├── Fix2Engine.Components/     # Scene and component structures
├── Fix2Engine.Graphics/       # Raylib rendering abstraction
├── Fix2Engine.IMGUI/          # ImGui wrappers and UI widgets
├── Fix2Engine.Physics/        # Collision and physics logic
└── Game/                      # Project runtime and test scenes
``` 

## Getting Started
Clone the repository:


```git clone https://github.com/your-username/Fix2Engine.git```
<br></br>
Open Fix2Engine.sln in Visual Studio or JetBrains Rider.

Set Game as the startup project and run.

Documentation: Coming soon...
