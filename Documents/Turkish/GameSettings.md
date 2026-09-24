# Oyun Ayarları (Game Settings)

`Configure`; başlangıç pencere başlığını ve boyutunu, hedef kare hızını, arka plan
rengini, FPS sayacını ve isteğe bağlı UI desteğini ayarlar. Bu kodu `Game.cs`
dosyasına koyun.

```csharp
using Fix2Engine;
using Fix2Engine.Components;
using Fix2Engine.Core;

namespace MyGame;

public sealed class Game : FixGame
{
    protected override void Configure(GameSettings settings)
    {
        settings.Title = "İlk Fix2 Oyunum";
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

`TargetFps = 0` kare hızı sınırını kapatır. Genişlik ve yükseklik pozitif olmalıdır.
