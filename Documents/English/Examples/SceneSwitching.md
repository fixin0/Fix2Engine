# Scene Switching

`SceneManager.LoadScene` queues a safe scene change. The old scene and its owned
objects are disposed automatically when the next update begins.

```csharp
using System.Numerics;
using Fix2Engine.Components;
using Fix2Engine.Components.Scene;
using Fix2Engine.Core;
using Fix2Engine.Graphics;
using Fix2Engine.Input;

namespace MyGame;

public sealed class MenuScene : FixScene
{
    protected override void OnUpdate(float dt)
    {
        if (InputManager.IsPressed("Jump"))
            SceneManager.LoadScene<GameplayScene>();
    }

    protected override void OnRender(RenderContext graphics)
    {
        graphics.DrawText("Press Space to play", new Vector2(420, 330),
            32, Color32.White);
    }
}

public sealed class GameplayScene : FixScene
{
    protected override void OnStart()
    {
        Add(new Player());
    }

    protected override void OnUpdate(float dt)
    {
        if (InputManager.IsPressed("Pause"))
            SceneManager.LoadScene<MenuScene>();
    }

    protected override void OnRender(RenderContext graphics)
    {
        graphics.DrawText("Escape: menu", new Vector2(20, 20),
            24, Color32.White);
    }
}
```

Start at the menu from your `Game` class:

```csharp
protected override void Start()
{
    SceneManager.LoadScene<MenuScene>();
}
```

The generated input map binds `Jump` to Space and `Pause` to Escape. Use the
`Player` class from the [Keyboard Movement](KeyboardMovement.md) example.
