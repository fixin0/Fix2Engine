# Physics & Audio

Both modules are currently **stubs** — the projects exist and are referenced, but contain no logic. This page explains the current state and how to extend them.

---

## Physics — `Fix2Engine.Physics`

**Current content:** `PhysicsWorld.cs`

```csharp
namespace Fix2Engine.Physics;
public class PhysicsWorld { }
```

**Project:** `Physics.csproj` — `net10.0`, no package references.

**Intended use** (per `README`): collision detection, spatial checks, physical movement logic.

### How to Extend

Add your systems and call them from `Windowing.FixedUpdate` (deterministic 60 Hz):

```csharp
// Physics/Collision.cs
public static class Collision
{
    public static bool CircleVsCircle(Vector2 a, float ra, Vector2 b, float rb)
        => Vector2.Distance(a, b) < ra + rb;

    public static bool RectangleVsRectangle(Rectangle a, Rectangle b)
        => Raylib.CheckCollisionRecs(a, b);
}

// In your Game.FixedUpdate:
protected override void FixedUpdate(float fixedDt)
{
    // Update movement and run 2D collision checks here.
}
```

Consider adding a `FixedUpdate` method to `IFixScene`; `Windowing` already provides the accumulator loop for it.

Raylib's built-in `CheckCollision*` helpers cover basic 2D collision checks.

---

## Audio — `Fix2Engine.Audio`

**Current content:** `OpenAL/SoundManager.cs`

```csharp
using Silk.NET.OpenAL;
namespace Fix2Engine.Audio.OpenAL;
public class SoundManager { }
```

**Project:** `Audio.csproj` — references `Silk.NET.OpenAL 2.23.0`.

**Intended use:** OpenAL-based sound playback (buffers, sources, listener).

### How to Extend

Wrap Silk.NET.OpenAL context and expose a simple API:

```csharp
public class SoundManager : IDisposable
{
    private readonly AL _al;
    private readonly ALContext _alc;
    private unsafe void* _device;
    private unsafe void* _context;

    public SoundManager()
    {
        _al = AL.GetApi();
        _alc = ALContext.GetApi();
        // _device = _alc.OpenDevice("");
        // _context = _alc.CreateContext(_device, null);
        // _alc.MakeContextCurrent(_context);
    }

    public uint LoadSound(string path) { /* GenBuffer, BufferData */ return 0; }
    public void Play(uint buffer) { /* GenSource, SourcePlay */ }
    public void Dispose() { /* cleanup */ }
}
```

Then instantiate it in `Game.Start()` and play sounds from scenes:

```csharp
// Game.cs
private SoundManager _audio;
protected override void Start()
{
    _audio = new SoundManager();
    SceneManager.LoadScene<MyScene>();
}
```

Alternatively, for a simpler path, use Raylib's own audio (`InitAudioDevice`, `LoadSound`, `PlaySound`) which is already available via `Raylib-cs` without needing OpenAL directly.

---

## Summary

| Module | Status | Next Step |
|--------|--------|-----------|
| `Physics` | Empty | Add 2D collision helpers; hook into `FixedUpdate` |
| `Audio` | Empty | Wrap `Silk.NET.OpenAL` or use Raylib audio (`Raylib.InitAudioDevice`) |

Both are safe to ignore for prototyping with primitives; add them when you need real physics or sound.
