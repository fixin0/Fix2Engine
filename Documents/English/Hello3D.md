# Your First 3D Scene

The 3D system runs beside the existing 2D scene system. `Scene3DManager` renders
its active `Scene3D` first; `FixScene`, `RenderContext`, and UI rendering continue
afterward. Game code uses Fix2Engine types only.

Create a 3D scene in `Start`, assign its camera, then load it:

```csharp
using System.Numerics;
using Fix2Engine;
using Fix2Engine.Components;
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

        var material = Material3D.FromPreset(ShaderSet.Toon)
            .SetFloat("steps", 4)
            .SetFloat("rimStrength", 0.7f);
        var beacon = new Object3D("Beacon") { Position = new Vector3(0, 1, 0) };
        beacon.AddComponent(new ModelRenderer3D(
            Model3D.Cube(new Vector3(2)), material,
            ownsModel: true, ownsMaterial: true));
        _world.Add(beacon);
    }
}
```

`Object3D` has `Position`, `Rotation` in degrees, `Scale`, child objects, and
components. Use `AddChild` for a transform hierarchy and `AddComponent` for
rendering or behaviour. `ModelRenderer3D` draws its model with the owner's global
transform.

## Models and primitives

Load an external model through the active rendering backend:

```csharp
var imported = Model3D.Load("Assets/robot.glb");
```

Use `Model3D.Cube`, `Plane`, `Sphere`, or `Cylinder` when no model file is needed.
The backend decides which imported file formats are supported. The included Raylib
backend handles the formats supported by its model loader.

Resources are borrowed by default. Pass `ownsModel: true` and `ownsMaterial: true`
to `ModelRenderer3D` when that component should release the resources along with
its owning `Object3D`.

Continue with [3D Shader Sets](Shaders3D.md) to select a built-in shader or load
your own GLSL files.
