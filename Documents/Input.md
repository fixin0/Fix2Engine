# Input

Sources:
- `Fix2Engine.Input/InputBackend/Input.cs` — namespace `Fix2Engine.Input` (`InputManager` class, platform dispatch)
- `Fix2Engine.Input/InputBackend/WindowsInputBackend.cs` — Win32 keyboard backend (`user32.dll`)
- `Fix2Engine.Input/InputBackend/Keys.cs` — enum `Keys` (Win32 virtual-key codes)
- `Fix2Engine.Input/InputBackend/KeyState.cs` — enum `KeyState` (reserved)
- `Fix2Engine.Input/InputConfig.cs` + `ConfigReader.cs` — TOML action maps

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

## TOML Action Maps (Optional)

`InputConfig` allows binding named actions to key lists via TOML:

```toml
[Actions]
MoveForward = ["W", "Up"]
Jump        = ["Space"]
Fire        = ["LeftMouse"]
```

```csharp
var config = InputConfig.InputConfigLoader("inputconfig.toml");
foreach (var kv in config.Actions)
    Console.WriteLine($"{kv.Key} = {string.Join(", ", kv.Value)}");
```

`ConfigReader.Reading(path)` is a thin wrapper around the same deserialization. Requires `AppContext.SetSwitch("Tomlyn.TomlSerializer.IsReflectionEnabledByDefault", true)` (already set in `Program.cs` due to `PublishAot`).

## Reserved

`KeyState { Up, Pressed, Held, Released }` is defined but not yet used — intended for a future state-machine layer on top of `IsDown`/`IsPressed`/`IsReleased`.
