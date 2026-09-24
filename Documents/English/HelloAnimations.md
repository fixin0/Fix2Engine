# Hello Animations

`AnimatedSprite2D` displays frames from a sprite sheet. This example expects four
32×32 frames in one row in `assets/player-sheet.png`. Use the asset copy rule from
[Hello Sprite2D](HelloSprite2D.md).

```csharp
using System.Numerics;
using Fix2Engine.Components;
using Fix2Engine.Components.Animation;
using Fix2Engine.Components.Scene;

namespace MyGame;

public sealed class MyGameScene : FixScene
{
    protected override void OnStart()
    {
        var player = new AnimatedSprite2D("Player")
        {
            TexturePath = "assets/player-sheet.png",
            Position = new Vector2(320, 180),
            Scale = new Vector2(3, 3),
            Origin = new Vector2(16, 16)
        };

        player.Animator.Add(SpriteAnimation.FromGrid(
            name: "walk",
            frameWidth: 32,
            frameHeight: 32,
            frameCount: 4,
            columns: 4,
            framesPerSecond: 8));

        player.Animator.Play("walk");
        Add(player);
    }
}
```

A scene-owned animated sprite advances automatically; do not call
`Animator.Update` yourself.
