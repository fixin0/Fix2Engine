using Fix2Engine.Components.Animation;
using Raylib_cs;

namespace Fix2Engine.Components;

public class AnimatedSprite2D : SpriteObject2D
{
    public AnimationPlayer Animator { get; } = new();
    protected override Rectangle? FrameRectangle => Animator.CurrentFrame ?? base.FrameRectangle;

    public AnimatedSprite2D(string name = "AnimatedSprite2D") : base(name) { }

    // Runs automatically after the user OnUpdate hook, even if a subclass doesn't call base.OnUpdate.
    internal override void AdvanceAnimation(float dt) => Animator.Update(dt);
}
