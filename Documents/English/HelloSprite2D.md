# Hello Sprite2D

`SpriteObject2D` loads and owns a texture, then draws it as part of the scene.
Create `assets/player.png`, then add this rule to `MyGame.csproj` so assets are
included in builds and exports:

```xml
<ItemGroup>
  <None Update="assets/**/*"
        CopyToOutputDirectory="PreserveNewest"
        CopyToPublishDirectory="PreserveNewest" />
</ItemGroup>
```

Use the sprite in your scene:

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

`Position`, `Scale`, `Rotation`, `Origin`, `Tint`, `FlipX`, and `FlipY` can be
changed at runtime. The texture is released automatically with the object.
