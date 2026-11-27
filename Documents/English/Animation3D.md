# 3D Animations

The 3D system ships its own animation layer that runs beside the 2D animation
system. Everything is engine-owned and backend-independent: transform clips,
timelines, camera and material animation use Fix2Engine types only, while skeletal
animation reads clips from GLB/GLTF files through the active rendering backend.

There are three ways to animate a 3D scene:

1. **Transform animation** moves, rotates and scales an `Object3D`.
2. **Timelines** drive camera, material, or any custom value over time.
3. **Skeletal animation** plays bone clips baked into a GLB/GLTF model.

## Transform animation

`TransformAnimator3D` plays `TransformClip3D` keyframes onto the `Object3D` it is
attached to. Build a clip with position, rotation and scale tracks, attach the
animator, then play the clip by name:

```csharp
using System.Numerics;
using Fix2Engine;
using Fix2Engine.Components;
using Fix2Engine.Components.Animation3D;
using Fix2Engine.Components.Scene;
using Fix2Engine.Graphics;

namespace MyGame;

public sealed class Game : FixGame
{
    private Scene3D? _world;

    protected override void Start()
    {
        _world = new Scene3D();
        _world.Camera.Position = new Vector3(7, 5, 7);
        _world.Camera.Target = Vector3.Zero;
        Scene3DManager.Load(_world);

        var beacon = new Object3D("Beacon");
        beacon.AddComponent(new ModelRenderer3D(
            Model3D.Cube(new Vector3(1)), Material3D.FromPreset(ShaderSet.Toon),
            ownsModel: true, ownsMaterial: true));
        _world.Add(beacon);

        var clip = new TransformClip3D("orbit")
            .AddPosition(0f, new Vector3(-3, 0, 0))
            .AddPosition(2f, new Vector3(3, 0, 0), EasingKind.SineInOut)
            .AddRotation(0f, Vector3.Zero)
            .AddRotation(2f, new Vector3(0, 360, 0))
            .AddScale(0f, Vector3.One)
            .AddScale(2f, new Vector3(1.5f), EasingKind.BackOut);

        beacon.AddComponent(new TransformAnimator3D()).Add(clip).Play("orbit");
    }
}
```

Rotation keys use Euler degrees in X pitch, Y yaw, Z roll order. Missing tracks keep
their identity value, so a clip may animate only position, for example.

Playback is automatic once the animator is attached to a scene-owned object; do not
call `Update` yourself. Clips loop by default. Set `clip.Loop = false` for a one-shot
clip.

### Playback control

`TransformAnimator3D` offers `Play`, `Pause`, `Resume`, `Stop`, `Seek` (in seconds)
and `Speed`, plus `Started`, `Finished` and `Looped` events. `Play` keeps its
progress on repeated calls; pass `restart: true` to rewind:

```csharp
var animator = new TransformAnimator3D();
animator.Add(clip);
animator.Started += c => Console.WriteLine($"Started {c.Name}");
animator.Finished += c => Console.WriteLine($"Finished {c.Name}");
animator.Play("orbit");
```

The same clip can be attached to several objects and played independently.

## Easing

Every keyframe accepts an `EasingKind`. `Easing.Evaluate` exposes the same curves for
custom use; both input and output are clamped to the 0..1 range:

```csharp
float eased = Easing.Evaluate(EasingKind.BounceOut, 0.75f);
```

## Timelines

`Timeline3D` runs independent tracks in parallel. Each track receives an eased 0..1
progress value, so it can drive anything, not just transforms:

```csharp
var timeline = new Timeline3D { Loop = true };
timeline.Add(0f, 2f, t => Console.WriteLine($"progress {t}"), EasingKind.CubicInOut);
timeline.Play();
```

Attach a `TimelineComponent3D` to an `Object3D` so the timeline advances with the
scene automatically, or call `Timeline.Update(dt)` yourself.

## Camera animation

`CameraAnimator3D` animates the camera of the `Scene3D` that owns the object it is
attached to:

```csharp
var camera = new CameraAnimator3D()
    .AnimatePosition(0f, 3f, new Vector3(10, 6, 10), new Vector3(0, 2, 8), EasingKind.SineInOut)
    .AnimateTarget(0f, 3f, Vector3.Zero, new Vector3(0, 1, 0))
    .AnimateFieldOfView(0f, 3f, 70f, 50f);

_world.Add(new Object3D("CameraDriver")).AddComponent(camera);
camera.Play();
```

## Material animation

`MaterialAnimator3D` animates named shader parameters. It targets the material given
in its constructor, or the first `ModelRenderer3D` on the same object:

```csharp
var material = Material3D.FromPreset(ShaderSet.Toon).SetFloat("pulse", 0f);
var node = new Object3D();
node.AddComponent(new ModelRenderer3D(Model3D.Cube(Vector3.One), material,
    ownsModel: true, ownsMaterial: true));
var animator = new MaterialAnimator3D(material)
    .AnimateFloat("pulse", 0f, 1f, 0f, 10f);
node.AddComponent(animator);
animator.Play();
_world.Add(node);
```

## Skeletal animation

Models loaded from GLB/GLTF files can carry bone animations. Load the model and its
animation set, then let `ModelAnimator3D` play a clip by name:

```csharp
using var model = Model3D.Load("Assets/robot.glb");
using var animations = model.LoadAnimations();

var node = new Object3D("Robot");
node.AddComponent(new ModelRenderer3D(model, material,
    ownsModel: true, ownsMaterial: true));
var animator = new ModelAnimator3D(model, animations);
node.AddComponent(animator);
animator.Play("walk");
_world.Add(node);
```

`ModelAnimator3D` mirrors the transform animator's API: `Play`, `Pause`, `Resume`,
`Stop`, `Seek`, `Speed`, `Loop`, and the `Started`, `Finished`, `Looped` events.
`Play` validates that the clip's skeleton matches the model. The animation set is
borrowed by default; pass `ownsAnimations: true` to let the component dispose it.

Supported formats depend on the backend. The included Raylib backend loads the
formats its model loader accepts (such as `.glb`, `.gltf`, `.iqm`, `.m3d`); it does
not load `.fbx`. Generated primitives have no animation data, so `LoadAnimations`
throws for them.

Continue with [3D Shader Sets](Shaders3D.md) for materials, or return to
[Your First 3D Scene](Hello3D.md).
