# Input

Sources:
- `Input/InputBackend/Input.cs` — namespace `Fix2Engine.Input` (`InputManager` class, platform dispatch)
- `Input/InputBackend/WindowsInputBackend.cs` — Win32 keyboard backend (`user32.dll`)
- `Input/InputBackend/Keys.cs` — enum `Keys` (Win32 virtual-key codes)
- `Input/InputBackend/KeyState.cs` — enum `KeyState` (reserved)
- `Input/InputConfig.cs` + `ConfigReader.cs` — TOML action maps

## Polling API

`InputManager` is a **static** class in `Fix2Engine.Input`. It is pumped once per frame by `Windowing.Run()` before `Update(dt)` — no manual call needed.

```csharp
using Fix2Engine.Input; // for InputManager
using Fix2Engine.Input.InputBackend; // for Keys

bool held     = InputManager.IsDown(Keys.W);
bool pressed  = InputManager.IsPressed(Keys.Space);   // edge: down this frame, up last frame
bool released = InputManager.IsReleased(Keys.Escape); // edge: up this frame, down last frame
```

Or fully qualified: `Fix2Engine.Input.InputManager.IsDown(Keys.W)`.

Call `IsDown` for continuous movement, `IsPressed` for one-shot actions (jump, shoot, menu toggle).

```csharp
public void Update(float dt)
{
    if (InputManager.IsDown(Keys.W))    MoveForward(dt);
    if (InputManager.IsPressed(Keys.R)) Reload();
    if (InputManager.IsPressed(Keys.F1)) ToggleCursor();
}
```

### How It Works

- Two `bool[256]` arrays: `CurrentKeys` and `PreviousKeys`.
- `Update()` does `Array.Copy(Current → Previous)` then fills `Current` for all 256 virtual-key codes.
- The backend is chosen once at startup based on the OS:
  - **Windows** → `WindowsInputBackend.IsDown((Keys)i)` (P/Invokes `user32.dll!GetAsyncKeyState`, checks bit `0x8000`).
  - **Anything else (Linux, macOS)** → maps each `Keys` value to a Raylib `KeyboardKey` and calls `Raylib.IsKeyDown(...)`.

## Cross-Platform Backend

`InputManager` dispatches automatically on the first call using `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)`:

```csharp
if (IsWindows)
    CurrentKeys[i] = WindowsInputBackend.IsDown((Keys)i);
else if (VkToKey.TryGetValue(i, out var raylibKey))
    CurrentKeys[i] = Raylib.IsKeyDown(raylibKey);
```

| OS | Backend | Mechanism |
|----|---------|-----------|
| Windows | `WindowsInputBackend` | `user32.dll!GetAsyncKeyState` |
| Linux / macOS | Raylib | `Raylib.IsKeyDown(KeyboardKey)` via the current window's GLFW context |

The `Keys` enum uses Win32 virtual-key codes, so a static `Dictionary<int, KeyboardKey>` `VkToKey` translates them to Raylib's `KeyboardKey` values (which follow GLFW key codes). Raylib's `KeyboardKey` enum only exposes `F1`–`F12`, so higher function keys (`F13`+) are not supported on non-Windows platforms.

`Fix2Engine.Input` depends on `Raylib-cs 8.0.0` to provide the non-Windows path.

## Keys Enum

`Keys` maps 1:1 to Win32 virtual-key codes (~144 values):

```csharp
Keys.A .. Keys.Z          // 0x41–0x5A
Keys.Key0 .. Keys.Key9     // 0x30–0x39
Keys.F1 .. Keys.F24        // 0x70–0x87
Keys.Space, Escape, Enter, Tab, Backspace
Keys.Left, Right, Up, Down // arrow keys
Keys.LeftShift, RightShift, LeftControl, RightControl, LeftAlt, RightAlt
Keys.Numpad0 .. Numpad9, Multiply, Add, Subtract, Divide
// ... plus media/browser/OEM keys
```

## InputMap.toml

Create `InputMap.toml` at the game project root and edit it as TOML:

```toml
[Actions]
MoveForward = ["W", "Up"]
Jump = ["Space"]
Pause = ["Escape"]
```

Load once before starting the game loop:

```csharp
InputManager.LoadInputMap();
```

Use actions from `Update`:

```csharp
if (InputManager.IsDown("MoveForward")) MoveForward(dt);
if (InputManager.IsPressed("Jump")) Jump();
if (InputManager.IsReleased("Jump")) EndJump();
```

- Action names are case-sensitive; key names are case-insensitive `Keys` enum names.
- Keys in an array are alternatives. Press fires when an action changes from inactive to active; release fires only when all its keys are released. Switching between held alternatives does not retrigger it.
- This version supports keyboard keys. Mouse buttons and gamepad bindings are not supported.
- Undefined actions throw `KeyNotFoundException`. Use `HasAction(name)` for optional actions.
- Invalid keys, empty bindings and malformed TOML produce an error. A failed reload preserves the previously loaded map.
- `LoadInputMap()` reads beside the executable, independent of the working directory. `LoadInputMap(path)` loads an explicit file; calling it again reloads edits. Files are not watched automatically.
- `InputConfig.Load(path)` / `ConfigReader.Load(path)` return parsed configuration without installing it in `InputManager`.
- TOML uses generated serialization metadata, so the loader does not need reflection enabled for NativeAOT.

The project must copy the map for both builds and publishing (Fix2Console sets this up):

```xml
<None Update="InputMap.toml" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="PreserveNewest" />
```

The demo uses actions for movement and debug controls. The built-in camera uses `MoveForward`, `MoveBackward`, `MoveLeft`, `MoveRight`, `MoveUp`, and `MoveDown` when defined, with its original key controls as fallback.

## Fix2Console project setup

```bash
Fix2Console --new-project MyGame
# Or, inside an existing C# project (including a subdirectory):
Fix2Console --init /path/to/Fix2Engine
# Once the engine is discoverable or configured:
Fix2Console --init
```

New projects include `InputMap.toml`, load it at startup, and copy it into build/publish output. `--init` creates missing configuration files, adds the Input project reference and output-copy rules to the existing `.csproj`. For an existing game, add `InputManager.LoadInputMap()` at startup yourself. Repeated initialization preserves existing TOML files and custom bindings. Running Fix2Console without arguments inside a C# project performs the same initialization automatically; outside a project it shows help.

`Fix2Engine.toml` records the engine location:

```toml
[Engine]
Directory = "../Fix2Engine"
```

Relative paths resolve from the project directory. Fix2Console consults this setting first, then looks for the engine above its working directory and executable directory. An invalid saved path produces an error; edit the setting if the engine moves. This development setting is not needed by the published game.

Run the input/configuration regression checks without opening a window:

```bash
dotnet build Tests/InputSystem/InputSystem.Tests.csproj -m:1
dotnet run --project Tests/InputSystem/InputSystem.Tests.csproj --no-build
```

## Reserved

`KeyState { Up, Pressed, Held, Released }` is defined but not yet used — intended for a future state-machine layer on top of `IsDown`/`IsPressed`/`IsReleased`.
