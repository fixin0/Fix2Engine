# Parallax Arka Plan

Parallax, uzaktaki katmanları yakındaki katmanlardan daha yavaş hareket ettirerek
derinlik hissi oluşturur. Bu örnek, arka planın boşluk bırakmadan tekrarlanması için
1280 piksel genişliğindeki her görselden iki kopya kullanır.

`assets/background-far.png`, `assets/background-mid.png` ve
`assets/background-near.png` dosyalarını ekleyin. [Merhaba Sprite2D](../HelloSprite2D.md)
bölümündeki asset kopyalama kuralını kullanın.

```csharp
using System.Numerics;
using Fix2Engine.Components;

namespace MyGame;

public sealed class ParallaxLayer : Object2D
{
    private readonly SpriteObject2D _first;
    private readonly SpriteObject2D _second;
    private readonly float _factor;
    private readonly float _layerWidth;

    public float CameraX { get; set; }

    public ParallaxLayer(string texturePath, float factor, int zIndex,
        float layerWidth = 1280) : base("Parallax Layer")
    {
        _factor = factor;
        _layerWidth = layerWidth;
        ZIndex = zIndex;

        _first = AddChild(new SpriteObject2D { TexturePath = texturePath });
        _second = AddChild(new SpriteObject2D { TexturePath = texturePath });
    }

    protected override void OnUpdate(float dt)
    {
        float rawOffset = CameraX * _factor;
        float offset = ((rawOffset % _layerWidth) + _layerWidth) % _layerWidth;
        _first.Position = new Vector2(-offset, 0);
        _second.Position = new Vector2(_layerWidth - offset, 0);
    }
}
```

Tüm katmanları tek bir sanal kamera konumuyla hareket ettirin. Varsayılan giriş
haritasında `MoveLeft` ve `MoveRight` zaten bulunur.

```csharp
using Fix2Engine.Components.Scene;
using Fix2Engine.Input;

namespace MyGame;

public sealed class ParallaxScene : FixScene
{
    private ParallaxLayer[] _layers = [];
    private float _cameraX;

    protected override void OnStart()
    {
        _layers =
        [
            Add(new ParallaxLayer("assets/background-far.png", 0.15f, -30)),
            Add(new ParallaxLayer("assets/background-mid.png", 0.40f, -20)),
            Add(new ParallaxLayer("assets/background-near.png", 0.75f, -10))
        ];
    }

    protected override void OnUpdate(float dt)
    {
        float direction = (InputManager.IsDown("MoveRight") ? 1f : 0f)
                        - (InputManager.IsDown("MoveLeft") ? 1f : 0f);
        _cameraX += direction * 300f * dt;

        foreach (ParallaxLayer layer in _layers)
            layer.CameraX = _cameraX;
    }
}
```

`Game.Start` içinden `ParallaxScene` sahnesini yükleyin. Kaynak görselleriniz 1280
piksel değilse `layerWidth` değerini değiştirin. Kesintisiz dokular en iyi sonucu
verir.
