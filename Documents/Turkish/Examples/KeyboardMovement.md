# Klavye ile Hareket

Kontrollerin `InputMap.toml` üzerinden değiştirilebilmesi için sabit tuşlar yerine
giriş aksiyonlarını kullanın. Hızı `dt` ile çarpmak, hareketi kare hızından bağımsız
yapar.

```csharp
using System.Numerics;
using Fix2Engine.Components;
using Fix2Engine.Input;

namespace MyGame;

public sealed class Player : SpriteObject2D
{
    private const float Speed = 220f;

    public Player() : base("Player")
    {
        TexturePath = "assets/player.png";
        Position = new Vector2(320, 180);
        Origin = new Vector2(16, 16);
    }

    protected override void OnUpdate(float dt)
    {
        float x = (InputManager.IsDown("MoveRight") ? 1f : 0f)
                - (InputManager.IsDown("MoveLeft") ? 1f : 0f);
        float y = (InputManager.IsDown("MoveBackward") ? 1f : 0f)
                - (InputManager.IsDown("MoveForward") ? 1f : 0f);

        var direction = new Vector2(x, y);
        if (direction != Vector2.Zero)
            direction = Vector2.Normalize(direction);

        Position += direction * Speed * dt;
        Position = new Vector2(
            Math.Clamp(Position.X, 16, 1264),
            Math.Clamp(Position.Y, 16, 704));

        if (x != 0) FlipX = x < 0;
    }
}
```

Oyuncuyu sahneye ekleyin:

```csharp
protected override void OnStart()
{
    Add(new Player());
}
```

Sınır değerleri 1280×720 pencere ve merkezlenmiş 32×32 sprite içindir.
