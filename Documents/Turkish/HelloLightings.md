# 2D Işıklandırma

Fix2Engine, `Fix2Engine.Graphics.Lighting` altında additive harmanlama kullanan,
sprite'lardan bağımsız 2D ışık şekilleri sunar. Işıkları, aydınlatmak istediğiniz
sprite'lardan sonra çizin. Ekrandaki mevcut pikselleri parlaklaştırırlar; normal
map, gölge oluşturma ve engellenme henüz desteklenmez.

## Point Light

`PointLight2D` dairesel bir ışıktır. `Range` yarıçapı, `Energy` parlaklığı
belirler.

```csharp
using System.Numerics;
using Fix2Engine.Components.Scene;
using Fix2Engine.Core;
using Fix2Engine.Graphics;
using Fix2Engine.Graphics.Lighting;

public sealed class LightingScene : FixScene
{
    private readonly PointLight2D _lamba = new()
    {
        Position = new Vector2(400, 260),
        Range = 160,
        Color = new Color32(255, 210, 90),
        Energy = 1.2f
    };

    protected override void OnRender(RenderContext graphics)
    {
        // Önce arka planı ve sprite'ları çizin.
        _lamba.Draw(graphics);
    }
}
```

## Directional and spot light

`DirectionalLight2D` geniş, dikdörtgen bir ışık demeti üretir.
`SpotLight2D` ise koni biçiminde çalışır. `Direction` ekran uzayındaki vektördür;
`Vector2.UnitY` aşağıyı gösterir.

```csharp
private readonly DirectionalLight2D _gunes = new()
{
    Position = new Vector2(400, 0),
    Direction = Vector2.UnitY,
    Width = 800,
    Length = 500,
    Color = new Color32(150, 200, 255),
    Energy = 0.45f
};

private readonly SpotLight2D _fener = new()
{
    Position = new Vector2(120, 280),
    Direction = Vector2.UnitX,
    Range = 220,
    AngleDegrees = 38,
    Color = new Color32(255, 236, 190)
};

protected override void OnRender(RenderContext graphics)
{
    // Dünya sprite'larını önce, ışık katmanını sonra çizin.
    _gunes.Draw(graphics);
    _fener.Draw(graphics);
}
```

Tüm ışık türlerinde `Enabled`, `Position`, `Color` ve `Energy` bulunur. Nokta ve
spot ışıklarında ayrıca `FalloffSteps` vardır; artırmak geçişi yumuşatır, ancak
bir miktar daha fazla çizim gerektirir.
