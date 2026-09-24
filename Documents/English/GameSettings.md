# Game Settings

`Configure` controls the initial window title and size, target frame rate,
background color, FPS counter, and optional UI support. Put this in `Game.cs`.

```csharp
using Fix2Engine;
using Fix2Engine.Components;
using Fix2Engine.Core;

namespace MyGame;

public sealed class Game : FixGame
{
    protected override void Configure(GameSettings settings)
    {
        settings.Title = "My First Fix2 Game";
        settings.Width = 1280;
        settings.Height = 720;
        settings.TargetFps = 144;
        settings.ClearColor = new Color32(18, 20, 30);
        settings.ShowFps = true;
        settings.EnableUI = false;
    }

    protected override void Start()
    {
        SceneManager.LoadScene<MyGameScene>();
    }
}
```

`TargetFps = 0` disables the frame-rate limit. Width and height must be positive.
