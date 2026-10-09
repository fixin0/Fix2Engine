# İlk 3D Sahnen

3D sistem, mevcut 2D sahne sisteminden bağımsız çalışır. Aktif `Scene3D`,
`Scene3DManager` tarafından önce çizilir; ardından `FixScene`, `RenderContext` ve
arayüz çizimi devam eder. Oyun kodunda yalnızca Fix2Engine türleri kullanılır.

`Start` içinde 3D sahneyi oluşturun, kamerayı ayarlayın ve yükleyin:

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

`Object3D`; `Position`, derece cinsinden `Rotation`, `Scale`, alt nesneler ve
bileşenler içerir. Dönüşüm hiyerarşisi için `AddChild`, davranış veya çizim için
`AddComponent` kullanın. `ModelRenderer3D`, modelini sahibinin global dönüşümüyle
çizer.

## Model ve ilkel geometri

Harici bir modeli aktif çizim altyapısı üzerinden yükleyin:

```csharp
var imported = Model3D.Load("Assets/robot.glb");
```

Model dosyası gerekmediğinde `Model3D.Cube`, `Plane`, `Sphere` ve `Cylinder`
kullanın. Desteklenen model biçimlerini aktif altyapı belirler. Dahili Raylib
altyapısı, kendi model yükleyicisinin desteklediği biçimleri açar.

Kaynaklar varsayılan olarak ödünç alınır. Bileşenin kaynakları sahibi `Object3D`
ile birlikte temizlemesi gerekiyorsa `ModelRenderer3D` oluştururken
`ownsModel: true` ve `ownsMaterial: true` verin.

Hazır shader seçimi veya kendi GLSL dosyalarınızı yüklemek için
[3D Shader Setleri](Shaders3D.md) belgesine geçin.
