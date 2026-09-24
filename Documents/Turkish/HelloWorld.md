# Merhaba Dünya (Hello World)

`FixScene`, geçerli seviyenin nesnelerini oluşturduğunuz ve çizim yaptığınız
yerdir. Oluşturulan `MyGameScene.cs` dosyasını bu örnekle değiştirin:

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
            "Merhaba, Fix2Engine!",
            new Vector2(40, 40),
            32,
            Color32.White);
    }
}
```

Motor ekranı önce `GameSettings.ClearColor` ile temizler, ardından aktif sahneyi
her karede çizer.
