# Parallax Background

Parallax creates depth by moving distant layers more slowly than nearby layers.
This example uses two copies of each 1280-pixel-wide image so the background loops
without a visible gap.

Add `assets/background-far.png`, `assets/background-mid.png`, and
`assets/background-near.png`. Use the asset copy rule from
[Hello Sprite2D](../HelloSprite2D.md).

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

Drive all layers with one virtual camera position. The default input map already
contains `MoveLeft` and `MoveRight`.

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

Load `ParallaxScene` from `Game.Start`. Change `layerWidth` when your source images
are not 1280 pixels wide. Seamless textures produce the best result.
