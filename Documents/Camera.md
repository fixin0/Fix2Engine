# Camera

Source: `Components/Cameras/Camera.cs` — namespace `Fix2Engine.Components.Cameras`

## Enums

```csharp
public enum CameraType { FirstPerson, ThirdPerson, Free, Custom }
public enum ProjectionType { Perspective, Orthographic }
```

## Class

```csharp
public class Camera
{
    public Vector3 Position { get; set; }
    public Vector3 Target { get; set; }
    public Vector3 Up { get; set; } = new Vector3(0, 1, 0);

    public float FOV { get; set; } = 60.0f;
    public CameraType Type { get; set; } = CameraType.FirstPerson;
    public ProjectionType Projection { get; set; } = ProjectionType.Perspective;

    public Vector3 TargetOffset { get; set; } = new Vector3(0, 3, 5); // ThirdPerson only
    public float MoveSpeed { get; set; } = 6.0f;
    public float MouseSensitivity { get; set; } = 0.003f;
    public double NearPlane { get; set; } = 0.05;
    public double FarPlane { get; set; } = 10000.0;

    public Camera();
    public Camera(Vector3 position, Vector3 target, float fov = 60.0f, CameraType type = CameraType.FirstPerson);

    public void Follow(Vector3 targetPosition);
    public void Update();
    public Camera3D GetRaylibCamera();
    public void Begin();
    public void End();
}
```

## Usage

```csharp
using Fix2Engine.Components.Cameras;

var camera = new Camera(
    position: new Vector3(0, 1.8f, 0),
    target:   new Vector3(0, 1.8f, -1),
    fov: 75.0f,
    type: CameraType.FirstPerson
);

// each frame:
camera.Update(); // handles WASD + Space/Ctrl + mouse via Raylib UpdateCameraPro

camera.Begin();
DrawGrid(50, 2.0f);
DrawSphere(enemyPos, 0.8f, Color.Red);
camera.End();
```

`Update()` only runs for `FirstPerson` and `Free`. It reads `IsKeyDown(W/A/S/D/Space/LeftControl)` and `GetMouseDelta()`, then calls `UpdateCameraPro`.

### Third-Person

```csharp
using Fix2Engine.Components.Cameras;

var camera = new Camera(Vector3.Zero, Vector3.Zero, 60.0f, CameraType.ThirdPerson);
camera.TargetOffset = new Vector3(0, 3, 5);

// each frame:
camera.Follow(playerPosition);

camera.Begin();
// draw world
camera.End();
```

`Follow` sets `Target = targetPosition` and, for `ThirdPerson`, `Position = targetPosition + TargetOffset`.

### Custom / Manual

Use `CameraType.Custom` and set `Position`/`Target` yourself. `Update()` will not overwrite them. You can also modify `Position` after `Update()` (e.g., view bob):

```csharp
camera.Update();
camera.Position += viewBob; // additive offset
```

### Clip Planes

`NearPlane` / `FarPlane` are applied in `Begin()` via `Rlgl.SetClipPlanes`. Defaults `0.05` / `10000.0` cover large worlds; tighten them to reduce z-fighting.

### Getting the Forward Vector

There is no `Forward` property — derive it from the Raylib camera:

```csharp
var rc = camera.GetRaylibCamera();
var forward = Vector3.Normalize(rc.Target - rc.Position);
var right   = Vector3.Normalize(Vector3.Cross(forward, rc.Up));
```

Use `forward` for raycasts, gun placement, etc.
