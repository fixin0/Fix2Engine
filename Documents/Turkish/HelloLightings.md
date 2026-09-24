# Merhaba Işıklandırma (Hello Lightings)

Fix2Engine şu anda gerçek ışık, shader, normal map veya additive blending API'si
sunmaz. Bu nesne basit, yarı saydam ve kare biçimli bir parlama üretir. Bir
prototip efektidir; diğer sprite'ları gerçekten aydınlatmaz.

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
        // Önce en geniş ve en soluk katmanı çiz.
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

Nesneyi sahneye ekleyip konumlandırın:

```csharp
Add(new GlowLight { Position = new System.Numerics.Vector2(400, 260) });
```
