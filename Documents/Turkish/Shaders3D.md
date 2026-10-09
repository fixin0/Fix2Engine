# 3D Shader Setleri ve Materyaller

`Material3D`, shader türünden bağımsızdır: bir `ShaderProgram`, isimli parametreler,
doku haritaları ve çizim rengini bir araya getirir. `PbrMaterial` API’si yoktur.
Metal/pürüzlülük tabanlı aydınlatma, gerçekçi shader setinin içindedir.

## Hazır shader setleri

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

| Set | Kullanım | Parametreler |
| --- | --- | --- |
| `Realistic` | GGX metal/pürüzlülük aydınlatması | `metallic`, `roughness` |
| `Toon` | Basamaklı ışık ve kenar parlaması | `steps`, `rimStrength` |
| `Water` | Hareketli dalgalar ve Fresnel tipi parlama | `shallowColor`, `deepColor`, `flowDirection`, `waveHeight`, `waveFrequency`, `transparency` |
| `Unlit` | Işıktan etkilenmeyen yüzeyler | Zorunlu parametre yok |

`Graphics3D.AmbientColor`, `LightDirection` ve `LightColor`; gerçekçi, toon ve
water shader’larının kullandığı kare-geneli ışık değerleridir.

## Gerçekçi shader doku haritaları

Haritaları `MaterialTextureSlot` ile atayın. Materyalin sahip olduğu dokular,
materyal temizlenirken otomatik silinir.

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

Gerçekçi shader metal değerini mavi kanaldan, pürüzlülüğü yeşil kanaldan ve
engellenmeyi kırmızı kanaldan okur. `Normal` slotu uyumlu custom shader’lar için
kullanılabilir; yerleşik gerçekçi shader henüz teğet uzay normal map uygulamaz.

## Kendi GLSL dosyaların

Hazır set yeterli olmadığında vertex ve fragment shader dosyalarını doğrudan
yükleyin:

```csharp
var shader = ShaderProgram.FromFiles("Shaders/crystal.vert", "Shaders/crystal.frag");
var material = new Material3D(shader, ownsShader: true)
    .SetFloat("crystalPulse", 1.4f)
    .SetVector3("crystalColor", new(0.2f, 0.8f, 1.0f));

material.SetTexture("noiseMap", Content.LoadTexture("Textures/noise.png"), ownsTexture: true);
```

Shader bu uniform’ları tanımlarsa motor günceller: `time`, `viewPos`,
`lightDirection`, `lightColor` ve `ambientColor`. Standart vertex shader’lar
`vertexPosition`, `vertexTexCoord`, `vertexNormal`, `vertexColor`, `mvp` ve
`matModel` adlarını kullanabilir. Shader’a özel değerler için `SetFloat`,
`SetVector2`, `SetVector3`, `SetVector4` ve `SetTexture` kullanın.

## Materyal dosyaları

`Material3D.Load("Materials/robot.fixmat")`, JSON dosyasındaki shader ve doku
yollarını dosyanın kendi klasörüne göre yükler. Hazır shader kullanan bir örnek:

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

Custom shader için `preset` yerine `vertex` ve `fragment` yol alanlarını ekleyin.
