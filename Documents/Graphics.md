# Graphics

`Graphics` uses `Raylib-cs 8.0.0` for windowing and drawing and `rlImgui-cs 3.2.0`
for UI integration. `Components` builds scene entities on this rendering layer.

## Scene Objects

Use `SpriteObject2D` for a static image and `AnimatedSprite2D` for sprite-sheet
animation. Both derive from `Object2D` and can be subclassed, parented and owned by
a scene. See [Objects](Components.md) and [Animation](Animation.md).

Object `OnRender()` hooks draw in local coordinates. The engine applies the full
parent transform around the hook, including rotation, scale and translation.
For a custom shape object:

```csharp
public class Marker : Fix2Engine.Components.Object2D
{
    protected override void OnRender()
    {
        Raylib_cs.Raylib.DrawRectangle(0, 0, 32, 32, Raylib_cs.Color.Red);
    }
}
```

Set `Position` on the marker and add it to a `FixScene`. Use `OnRenderUI` for screen
coordinates and ImGui controls.

## Low-Level Sprite Resource

`Fix2Engine.Graphics.Sprite2D` owns or borrows a texture and supports direct drawing.
It is a graphics resource rather than a scene entity.

```csharp
// After a graphics window has been created:
using var resource = new Fix2Engine.Graphics.Sprite2D("assets/player.png");
resource.Position = new System.Numerics.Vector2(100, 100);
resource.Scale = new System.Numerics.Vector2(2, 2);
resource.CenterOrigin();
// Inside a drawing frame:
resource.Draw();
```

`SourceRect`, `Origin`, `Position`, `Scale`, `Rotation` and `Tint` control direct
rendering. Origin is measured in source pixels and is scaled with the sprite.
Negative direct-draw scales flip the texture while keeping its destination size
positive.

File-backed resources own their textures. Existing `Texture2D` handles are borrowed
by default: `new Sprite2D(texture, ownsTexture: true)` explicitly transfers ownership.
A resource can be shared with objects using `obj.SetSprite(resource)`; it must outlive
those objects. An object's `TexturePath` instead creates and owns its own resource,
loads it only on first render, and releases it on replacement or destruction.

Dispose owned textures on the application thread before closing the window.
There is no graphics finalizer: native cleanup must not run on a GC thread.
`Windowing.OnUnload` provides the shutdown hook for this purpose.
