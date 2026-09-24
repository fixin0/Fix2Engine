# Nesne Hiyerarşisi

Child nesneler parent nesnelerinin konum, dönüş ve ölçeğini devralır. Bu yapı bağlı
silahlar, tekerlekler, yörüngedeki cisimler ve çok parçalı karakterler için
kullanışlıdır.

```csharp
using System.Numerics;
using Fix2Engine.Components;
using Fix2Engine.Components.Scene;
using Fix2Engine.Core;
using Fix2Engine.Graphics;

namespace MyGame;

public sealed class OrbitBody : Object2D
{
    private readonly float _size;
    private readonly Color32 _color;
    public float AngularSpeed { get; init; }

    public OrbitBody(string name, float size, Color32 color) : base(name)
    {
        _size = size;
        _color = color;
    }

    protected override void OnUpdate(float dt)
    {
        Rotation += AngularSpeed * dt;
    }

    protected override void OnRender(RenderContext graphics)
    {
        graphics.DrawRectangle(
            new RectF(-_size / 2, -_size / 2, _size, _size),
            _color);
    }
}

public sealed class HierarchyScene : FixScene
{
    protected override void OnStart()
    {
        var star = Add(new OrbitBody("Star", 48, new Color32(255, 210, 70))
        {
            Position = new Vector2(640, 360),
            AngularSpeed = 15
        });

        var planet = star.AddChild(new OrbitBody("Planet", 28, Color32.Blue)
        {
            Position = new Vector2(180, 0),
            AngularSpeed = 60
        });

        planet.AddChild(new OrbitBody("Moon", 12, Color32.White)
        {
            Position = new Vector2(55, 0)
        });
    }
}
```

Yıldızı döndürmek gezegeni yıldız çevresinde hareket ettirir. Gezegeni döndürmek
ayı gezegen çevresinde hareket ettirir. Yıldız yok edildiğinde tüm child nesneleri
de otomatik olarak yok edilir.
