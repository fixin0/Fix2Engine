# 3D Shader Sets and Materials

`Material3D` is shader-neutral: it combines a `ShaderProgram`, named parameters,
texture maps, and a draw tint. There is no `PbrMaterial` API. The realistic shader
set contains the metallic/roughness lighting implementation itself.

## Ready-made shader sets

```csharp
var realistic = Material3D.FromPreset(ShaderSet.Realistic)
    .SetFloat("metallic", 0.65f)
    .SetFloat("roughness", 0.28f);

var toon = Material3D.FromPreset(ShaderSet.Toon)
    .SetFloat("steps", 4)
    .SetFloat("rimStrength", 0.75f);

var water = Material3D.FromPreset(ShaderSet.Water)
    .SetVector3("shallowColor", new(0.03f, 0.31f, 0.52f))
    .SetVector3("deepColor", new(0.01f, 0.03f, 0.16f))
    .SetVector2("flowDirection", new(0.7f, 0.3f))
    .SetFloat("waveHeight", 0.14f)
    .SetFloat("waveFrequency", 1.6f)
    .SetFloat("transparency", 0.92f);
```

| Set | Use | Parameters |
| --- | --- | --- |
| `Realistic` | GGX metallic/roughness lighting | `metallic`, `roughness` |
| `Toon` | Stepped diffuse lighting and rim light | `steps`, `rimStrength` |
| `Water` | Animated waves and Fresnel-style highlights | `shallowColor`, `deepColor`, `flowDirection`, `waveHeight`, `waveFrequency`, `transparency` |
| `Unlit` | Light-independent surfaces | No required parameters |

`Graphics3D.AmbientColor`, `LightDirection`, and `LightColor` are frame-wide
lighting values used by the built-in realistic, toon, and water shaders.

## Realistic texture maps

Assign maps through `MaterialTextureSlot`; material-owned textures are disposed
with the material.

```csharp
var material = Material3D.FromPreset(ShaderSet.Realistic)
    .SetFloat("metallic", 0.0f)
    .SetFloat("roughness", 0.5f)
    .SetMap(MaterialTextureSlot.Albedo, Content.LoadTexture("Assets/robot-albedo.png"), ownsTexture: true)
    .SetMap(MaterialTextureSlot.Metallic, Content.LoadTexture("Assets/robot-metallic.png"), ownsTexture: true)
    .SetMap(MaterialTextureSlot.Roughness, Content.LoadTexture("Assets/robot-roughness.png"), ownsTexture: true)
    .SetMap(MaterialTextureSlot.Occlusion, Content.LoadTexture("Assets/robot-ao.png"), ownsTexture: true)
    .SetMap(MaterialTextureSlot.Emission, Content.LoadTexture("Assets/robot-emission.png"), ownsTexture: true);
```

The realistic shader samples metallic from the blue channel, roughness from green,
and occlusion from red. `Normal` is available as a material slot for compatible
custom shaders; the current built-in realistic shader does not yet apply tangent-
space normal maps.

## Custom GLSL files

Load shader files directly when a ready-made set does not fit:

```csharp
var shader = ShaderProgram.FromFiles("Shaders/crystal.vert", "Shaders/crystal.frag");
var material = new Material3D(shader, ownsShader: true)
    .SetFloat("crystalPulse", 1.4f)
    .SetVector3("crystalColor", new(0.2f, 0.8f, 1.0f));

material.SetTexture("noiseMap", Content.LoadTexture("Textures/noise.png"), ownsTexture: true);
```

The engine updates these uniforms when a shader declares them: `time`, `viewPos`,
`lightDirection`, `lightColor`, and `ambientColor`. Standard vertex shaders can
use `vertexPosition`, `vertexTexCoord`, `vertexNormal`, `vertexColor`, `mvp`, and
`matModel`. Use `SetFloat`, `SetVector2`, `SetVector3`, `SetVector4`, and
`SetTexture` for shader-specific values.

## Material files

`Material3D.Load("Materials/robot.fixmat")` loads relative shader and texture paths
from JSON. A material file can use a preset:

```json
{
  "preset": "Realistic",
  "parameters": { "metallic": 0.65, "roughness": 0.28 },
  "maps": {
    "Albedo": "Textures/robot-albedo.png",
    "Metallic": "Textures/robot-metallic.png",
    "Roughness": "Textures/robot-roughness.png"
  }
}
```

For a custom shader, replace `preset` with `vertex` and `fragment` path fields.
