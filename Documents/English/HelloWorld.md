# Hello World

`FixScene` is where you create objects and draw the current level. Replace the
generated `MyGameScene.cs` with this small text example:

```csharp
using System.Numerics;
using Fix2Engine.Components.Scene;
using Fix2Engine.Core;
using Fix2Engine.Graphics;

namespace MyGame;

public sealed class MyGameScene : FixScene
{
    protected override void OnRender(RenderContext graphics)
    {
        graphics.DrawText(
            "Hello, Fix2Engine!",
            new Vector2(40, 40),
            32,
            Color32.White);
    }
}
```

The engine clears the screen using `GameSettings.ClearColor`, then renders the
active scene every frame.
