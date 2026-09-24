# Hello Lightings

Fix2Engine does not currently provide real lights, shaders, normal maps, or
additive blending. This object creates a simple translucent square glow. It is a
prototype effect and does not illuminate other sprites.

```csharp
using Fix2Engine.Components;
using Fix2Engine.Core;
using Fix2Engine.Graphics;

namespace MyGame;

public sealed class GlowLight : Object2D
{
    public GlowLight() : base("Glow Light")
    {
        ZIndex = 10;
    }

    protected override void OnRender(RenderContext graphics)
    {
        // Draw the widest, faintest layer first.
        for (int layer = 8; layer >= 1; layer--)
        {
            float size = layer * 28f;
            byte alpha = (byte)(10 + (8 - layer) * 7);
            graphics.DrawRectangle(
                new RectF(-size / 2, -size / 2, size, size),
                new Color32(255, 210, 90, alpha));
        }
    }
}
```

Add and position it in your scene:

```csharp
Add(new GlowLight { Position = new System.Numerics.Vector2(400, 260) });
```
