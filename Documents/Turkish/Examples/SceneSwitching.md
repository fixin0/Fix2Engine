# Sahne Değiştirme

`SceneManager.LoadScene` güvenli bir sahne değişimi sıraya alır. Sonraki update
başladığında eski sahne ve sahip olduğu nesneler otomatik olarak yok edilir.

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
        graphics.DrawText("Oynamak için Space'e bas", new Vector2(400, 330),
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
        graphics.DrawText("Escape: menü", new Vector2(20, 20),
            24, Color32.White);
    }
}
```

`Game` sınıfınızdan menüyle başlayın:

```csharp
protected override void Start()
{
    SceneManager.LoadScene<MenuScene>();
}
```

Oluşturulan giriş haritası `Jump` aksiyonunu Space'e, `Pause` aksiyonunu Escape'e
bağlar. `Player` sınıfı için [Klavye ile Hareket](KeyboardMovement.md) örneğini
kullanın.
