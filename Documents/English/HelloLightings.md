# 2D Lighting

Fix2Engine provides additive, sprite-independent 2D light shapes in
`Fix2Engine.Graphics.Lighting`. Draw lights after the sprites they should light.
They brighten the pixels already on screen; they do not yet use normal maps,
shadow casting, or occlusion.

## Point light

`PointLight2D` is a radial light. Its `Range` controls its radius and `Energy`
controls brightness.

```csharp
using System.Numerics;
using Fix2Engine.Components.Scene;
using Fix2Engine.Core;
using Fix2Engine.Graphics;
using Fix2Engine.Graphics.Lighting;

public sealed class LightingScene : FixScene
{
    private readonly PointLight2D _lamp = new()
    {
        Position = new Vector2(400, 260),
        Range = 160,
        Color = new Color32(255, 210, 90),
        Energy = 1.2f
    };

    protected override void OnRender(RenderContext graphics)
    {
        // Draw your background and sprites first.
        _lamp.Draw(graphics);
    }
}
```

## Directional and spot lights

`DirectionalLight2D` casts a broad rectangular beam. `SpotLight2D` casts a
cone. `Direction` is a screen-space vector; `Vector2.UnitY` points down.

```csharp
private readonly DirectionalLight2D _sun = new()
{
    Position = new Vector2(400, 0),
    Direction = Vector2.UnitY,
    Width = 800,
    Length = 500,
    Color = new Color32(150, 200, 255),
    Energy = 0.45f
};

private readonly SpotLight2D _torch = new()
{
    Position = new Vector2(120, 280),
    Direction = Vector2.UnitX,
    Range = 220,
    AngleDegrees = 38,
    Color = new Color32(255, 236, 190)
};

protected override void OnRender(RenderContext graphics)
{
    // Render world sprites first, then the light overlay.
    _sun.Draw(graphics);
    _torch.Draw(graphics);
}
```

All light types expose `Enabled`, `Position`, `Color`, and `Energy`. Point and
spot lights also expose `FalloffSteps`; increasing it makes the falloff smoother
at a small drawing cost.
