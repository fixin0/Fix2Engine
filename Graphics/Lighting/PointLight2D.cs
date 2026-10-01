using Fix2Engine.Core;

namespace Fix2Engine.Graphics.Lighting;

/// <summary>A radial light with a smooth, additive falloff.</summary>
public sealed class PointLight2D : Light2D
{
    public float Range { get; set; } = 128f;
    /// <summary>Number of circles used to approximate the radial falloff.</summary>
    public int FalloffSteps { get; set; } = 12;

    protected override void DrawCore(RenderContext graphics)
    {
        ValidateRange(Range, nameof(Range));
        if (FalloffSteps < 1) throw new ArgumentOutOfRangeException(nameof(FalloffSteps));
        using var blend = graphics.PushAdditiveBlend();
        for (int step = FalloffSteps; step >= 1; step--)
        {
            float progress = step / (float)FalloffSteps;
            graphics.DrawCircle(Position, Range * progress, ColorAt((1f - progress) * .16f + .015f));
        }
    }
}
