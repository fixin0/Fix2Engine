# Input

Sources:
- `Fix2Engine.Input/InputBackend/Input.cs` — namespace `Fix2Engine.Input` (`InputManager` class)
- `Fix2Engine.Input/InputBackend/WindowsInputBackend.cs` — namespace `Fix2Engine.Input.InputBackend`
- `Fix2Engine.Input/InputBackend/Keys.cs` — enum `Keys`
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
- `Update()` does `Array.Copy(Current → Previous)` then fills `Current` via `WindowsInputBackend.IsDown((Keys)i)` for all 256 virtual-key codes.
- `WindowsInputBackend` P/Invokes `user32.dll!GetAsyncKeyState` and checks bit `0x8000`.

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

## Raylib vs Native — Adding Linux Support

The current backend is **Windows-only** (`user32.dll` via `WindowsInputBackend`). For cross-platform input you have two options:

| Approach | Pros | Cons |
|----------|------|------|
| **Raylib** (`IsKeyDown`, `GetMousePosition`, etc.) | Already a dependency, cross-platform (X11/Wayland/macOS), trivial wrapper | Less low-level control |
| **Native X11/Wayland** | Full control | Must handle X11 + Wayland separately, more code and maintenance |

**Recommendation:** keep Raylib as the base. Abstract the backend:

```csharp
// Fix2Engine.Input/InputBackend/IInputBackend.cs
public interface IInputBackend { bool IsDown(Keys key); }

// Windows: GetAsyncKeyState via WindowsInputBackend
// Linux/macOS: Raylib.IsKeyDown((KeyboardKey)key)
```

The engine already depends on `Raylib-cs` in `Graphics` and `Components`, so Raylib input adds zero new dependencies.

## Reserved

`KeyState { Up, Pressed, Held, Released }` is defined but not yet used — intended for a future state-machine layer on top of `IsDown`/`IsPressed`/`IsReleased`.
