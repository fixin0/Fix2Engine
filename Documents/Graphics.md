# Graphics

Sources: `Graphics/Windowing.cs`, `Graphics/Sprite2D.cs` — namespace `Fix2Engine.Graphics`

## Dependencies

The graphics project uses `Raylib-cs 8.0.0` for windowing and 2D drawing and `rlImgui-cs 3.2.0` for UI integration. Drawing takes place inside the `Windowing.Render()` frame.

## Sprite2D

`Sprite2D` draws a textured rectangle and implements `IDisposable`.

```csharp
using System.Numerics;
using Fix2Engine.Graphics;

using var player = new Sprite2D("assets/sprites/player.png");
player.Position = new Vector2(100, 100);
player.Scale = new Vector2(2, 2);
player.CenterOrigin();
player.Draw();
```

`Position`, `Scale`, `Origin`, `Rotation`, `Tint`, and `SourceRect` control how the sprite is drawn. Dispose a file-backed sprite when its scene exits to release its texture.

## 2D Primitives

Raylib shapes work directly inside a scene's `Render()` method:

```csharp
using static Raylib_cs.Raylib;

ClearBackground(new Color(40, 44, 52, 255));
DrawRectangle(100, 100, 160, 160, Color.Red);
DrawRectangleLines(100, 100, 160, 160, Color.White);
DrawCircle(400, 180, 40, Color.Blue);
```

Use these drawing calls in your application's scene; see [Getting Started](GettingStarted.md).
