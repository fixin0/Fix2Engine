# Keyboard Movement

Use input actions instead of hard-coded keys so controls remain configurable in
`InputMap.toml`. Multiplying speed by `dt` keeps movement frame-rate independent.

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

Add the player in a scene:

```csharp
protected override void OnStart()
{
    Add(new Player());
}
```

The clamp values assume a 1280×720 window and a 32×32 centered sprite.
