# Merhaba Sprite2D (Hello Sprite2D)

`SpriteObject2D` bir dokuyu yükler, sahiplenir ve sahnenin parçası olarak çizer.
`assets/player.png` dosyasını oluşturun. Görselin build ve export çıktısına
kopyalanması için `MyGame.csproj` içine şu kuralı ekleyin:

```xml
<ItemGroup>
  <None Update="assets/**/*"
        CopyToOutputDirectory="PreserveNewest"
        CopyToPublishDirectory="PreserveNewest" />
</ItemGroup>
```

Sprite'ı sahnede kullanın:

```csharp
using System.Numerics;
using Fix2Engine.Components;
using Fix2Engine.Components.Scene;

namespace MyGame;

public sealed class MyGameScene : FixScene
{
    protected override void OnStart()
    {
        Add(new SpriteObject2D("Player")
        {
            TexturePath = "assets/player.png",
            Position = new Vector2(320, 180),
            Scale = new Vector2(2, 2),
            Origin = new Vector2(16, 16)
        });
    }
}
```

`Position`, `Scale`, `Rotation`, `Origin`, `Tint`, `FlipX` ve `FlipY` çalışma
sırasında değiştirilebilir. Nesne yok edildiğinde doku otomatik bırakılır.
