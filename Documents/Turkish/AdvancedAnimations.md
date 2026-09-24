# Gelişmiş Animasyonlar (Advanced Animations)

Birden fazla klip, karakterin durum değiştirmesini sağlar. `Finished`, tek seferlik
bir klipten geri dönmek için kullanışlıdır. `Speed`, `Pause`, `Resume` ve
`SeekFrame` oynatımı doğrudan kontrol eder.

Bu sheet sekiz sütunludur: idle kare 0'da, run kare 8'de, attack kare 16'da başlar.
Varsayılan giriş haritasında `MoveLeft`, `MoveRight` ve `Jump` zaten bulunur.

```csharp
using System.Numerics;
using Fix2Engine.Components;
using Fix2Engine.Components.Animation;
using Fix2Engine.Input;

namespace MyGame;

public sealed class Player : AnimatedSprite2D
{
    private bool _attacking;

    public Player() : base("Player")
    {
        TexturePath = "assets/player-sheet.png";
        Position = new Vector2(320, 180);
        Origin = new Vector2(16, 16);

        Animator.Add(SpriteAnimation.FromGrid("idle", 32, 32, 4, 8, 6));
        Animator.Add(SpriteAnimation.FromGrid("run", 32, 32, 6, 8, 12,
            startFrame: 8));
        Animator.Add(SpriteAnimation.FromGrid("attack", 32, 32, 5, 8, 14,
            loop: false, startFrame: 16));

        Animator.Finished += clip =>
        {
            if (clip.Name != "attack") return;
            _attacking = false;
            Animator.Play("idle");
        };

        Animator.Play("idle");
    }

    protected override void OnUpdate(float dt)
    {
        if (InputManager.IsPressed("Jump"))
        {
            _attacking = true;
            Animator.Play("attack", restart: true);
        }

        if (_attacking) return;

        float direction = (InputManager.IsDown("MoveRight") ? 1f : 0f)
                        - (InputManager.IsDown("MoveLeft") ? 1f : 0f);

        Position += new Vector2(direction * 180f * dt, 0);
        if (direction != 0) FlipX = direction < 0;
        Animator.Play(direction == 0 ? "idle" : "run");
    }
}
```

Sahnenizin `OnStart` metodunda `new Player()` ekleyin. Aktif klip için yinelenen
`Play` çağrıları, `restart: true` kullanılmadıkça ilerlemeyi korur.
