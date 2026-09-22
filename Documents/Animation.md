# 2D Sprite Animation

Sources: `Components/Animation/*`, `Components/AnimatedSprite2D.cs`.

`AnimatedSprite2D` is an `Object2D` with sprite-sheet animation. Each object has its
own `AnimationPlayer`; immutable `SpriteAnimation` clips can be shared. No separate
animation update is needed for an object owned by `FixScene`.

## An Inheritable Animated Character

This example expects a sheet with 32×32 frames and eight columns. Supply your own
texture and copy it with your application assets.

```csharp
using System.Numerics;
using Fix2Engine.Components;
using Fix2Engine.Components.Animation;
using Fix2Engine.Input;

public class Player : AnimatedSprite2D
{
    public Player() : base("Player")
    {
        TexturePath = "assets/player.png";
        Origin = new Vector2(16, 32);
        Animator.Add(SpriteAnimation.FromGrid("idle", 32, 32, 4, 8, framesPerSecond: 6));
        Animator.Add(SpriteAnimation.FromGrid("run", 32, 32, 6, 8,
            framesPerSecond: 12, startFrame: 8));
        Animator.Play("idle");
    }

    protected override void OnUpdate(float dt)
    {
        float direction = (InputManager.IsDown("MoveRight") ? 1 : 0)
                        - (InputManager.IsDown("MoveLeft") ? 1 : 0);
        Position += new Vector2(direction * 180 * dt, 0);
        if (direction != 0) FlipX = direction < 0;
        Animator.Play(direction == 0 ? "idle" : "run");
    }
}
```

Add `new Player()` to a `FixScene`. Repeated `Play` calls with the same name preserve
frame progress, so animation selection can run every update. Object animation
advances after `OnUpdate`, even when a subclass does not call the base hook.

## Define Clips

`SpriteAnimation.FromGrid(name, frameWidth, frameHeight, frameCount, columns,
framesPerSecond, loop, startFrame)` walks a sheet from left to right, wrapping rows.
The starting frame index is zero-based. Set `loop: false` for a one-shot animation.

For irregular sheets, pass explicit source rectangles:

```csharp
var hit = new SpriteAnimation("hit", new[]
{
    new Raylib_cs.Rectangle(0, 64, 32, 32),
    new Raylib_cs.Rectangle(32, 64, 32, 32),
    new Raylib_cs.Rectangle(64, 64, 32, 32)
}, framesPerSecond: 10, loop: false);
Animator.Add(hit);
Animator.Finished += clip =>
{
    if (clip.Name == "hit") Animator.Play("idle");
};
```

Clips copy their frame data and require a nonempty frame list, positive frame sizes
and a finite positive frame rate. Clip names are case-sensitive; duplicate names
and attempts to play an unknown name are errors. All rectangles refer to the
object's current texture.

## Playback

| API | Behavior |
| --- | --- |
| `Play(name)` | Select a clip, resume a paused clip, or restart a completed clip |
| `Play(name, restart: true)` | Rewind explicitly |
| `Pause()` / `Resume()` | Hold / continue the current frame |
| `Stop()` | Stop and rewind to frame zero |
| `Speed` | Playback multiplier; zero freezes time, negative/nonfinite values are rejected |
| `FrameIndex`, `CurrentFrame`, `CurrentAnimation` | Inspect current playback state |
| `IsPlaying` | Whether playback is enabled |
| `FrameChanged` | Reports the final visible frame index after an update or playback reset |
| `Finished` | Reports a one-shot clip once after its final frame's full duration |

A completed one-shot retains its last frame. Looping clips wrap and preserve the
elapsed remainder, including large deltas. Frame-change events report the final
visible frame, not every skipped frame. Hidden objects continue animation;
inactive objects and inactive ancestors suspend it.

For custom systems outside a scene, an `AnimationPlayer` can also be used directly
by calling `Update(dt)` yourself. Do not additionally update the animator of a
scene-owned `AnimatedSprite2D`.
