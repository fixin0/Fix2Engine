# Fix2Engine 3D graphics

The 3D layer is independent from the established 2D scene. `Scene3DManager` draws its active world before `FixScene` and 2D UI. Game code only uses Fix2Engine types; Raylib resource handles remain in the backend. The user-facing tutorials are available in [English](../../Documents/English/Hello3D.md) and [Turkish](../../Documents/Turkish/Hello3D.md).

## Start a 3D world

```csharp
var scene = new Scene3D();
scene.Camera.Position = new Vector3(7, 5, 7);
scene.Camera.Target = Vector3.Zero;
Scene3DManager.Load(scene);

var cube = new Object3D("Beacon") { Position = new(0, 1, 0) };
cube.AddComponent(new ModelRenderer3D(
    Model3D.Cube(new Vector3(2)),
    Material3D.FromPreset(ShaderSet.Toon),
    ownsModel: true,
    ownsMaterial: true));
scene.Add(cube);
```

`Model3D.Load(path)` imports models through the selected backend. `Model3D.Cube`, `Plane`, `Sphere`, and `Cylinder` create geometry without model files. `ModelRenderer3D` borrows its model and material by default; pass both ownership flags when the component should dispose them.

## Shader sets

- `ShaderSet.Realistic` uses a metallic/roughness GGX lighting pass.
- `ShaderSet.Toon` uses stepped lighting and an adjustable rim light.
- `ShaderSet.Water` deforms a plane with animated waves and uses a Fresnel reflection response.
- `ShaderSet.Unlit` renders light-independent surfaces.

The material object is shader-neutral. It stores a shader, texture bindings and named uniform values; it is deliberately not a `PbrMaterial` type.

```csharp
var water = Material3D.FromPreset(ShaderSet.Water)
    .SetVector3("shallowColor", new(0.03f, 0.31f, 0.52f))
    .SetFloat("waveHeight", 0.15f);
```

## Custom shader paths

```csharp
var shader = ShaderProgram.FromFiles("Shaders/crystal.vert", "Shaders/crystal.frag");
var material = new Material3D(shader, ownsShader: true)
    .SetFloat("crystalPulse", 1.4f);
```

Custom vertex shaders receive Raylib-compatible `vertexPosition`, `vertexTexCoord`, `vertexNormal`, `vertexColor`, `mvp`, and `matModel` names. Materials automatically update `time`, `lightDirection`, `lightColor`, `ambientColor`, and `viewPos` when those uniforms exist. User parameters may be `float`, `Vector2`, `Vector3`, `Vector4`, or a named `Texture`.

## Realistic maps

`Realistic` provides metallic/roughness GGX direct lighting. Its map slots are `Albedo`, `Metallic`, `Roughness`, `Occlusion`, and `Emission`; the shader samples metallic from blue, roughness from green, and occlusion from red. `Normal` remains available for custom shaders but is not applied by the current built-in realistic shader.

```csharp
var realistic = Material3D.FromPreset(ShaderSet.Realistic)
    .SetFloat("metallic", 0.65f)
    .SetFloat("roughness", 0.28f)
    .SetMap(MaterialTextureSlot.Albedo, Content.LoadTexture("Textures/robot-albedo.png"), ownsTexture: true)
    .SetMap(MaterialTextureSlot.Roughness, Content.LoadTexture("Textures/robot-roughness.png"), ownsTexture: true);
```

## Material descriptor files

`Material3D.Load("Materials/lake.fixmat")` loads JSON relative to the descriptor file. It supports either a preset or custom shader paths:

```json
{
  "preset": "Toon",
  "parameters": { "steps": 4, "rimStrength": 0.7 },
  "maps": {
    "Albedo": "Textures/robot-color.png",
    "Metallic": "Textures/robot-metallic.png",
    "Roughness": "Textures/robot-roughness.png"
  },
  "textures": { "noiseMap": "Textures/noise.png" }
}
```

For custom files, replace `preset` with `vertex` and `fragment` path properties.
