using System.Numerics;
using Fix2Engine.Core;

namespace Fix2Engine.Graphics.Lighting;

/// <summary>A cone-shaped light cast from <see cref="Light2D.Position"/>.</summary>
public sealed class SpotLight2D : Light2D
{
    public Vector2 Direction { get; set; } = Vector2.UnitY;
    public float Range { get; set; } = 240f;
    /// <summary>Full width of the cone, in degrees.</summary>
    public float AngleDegrees { get; set; } = 45f;
    public int FalloffSteps { get; set; } = 10;

    protected override void DrawCore(RenderContext graphics)
    {
        ValidateRange(Range, nameof(Range));
        if (!float.IsFinite(AngleDegrees) || AngleDegrees <= 0 || AngleDegrees >= 360)
            throw new ArgumentOutOfRangeException(nameof(AngleDegrees));
        if (FalloffSteps < 1) throw new ArgumentOutOfRangeException(nameof(FalloffSteps));
        var direction = NormalizeDirection(Direction, nameof(Direction));
        float center = MathF.Atan2(direction.Y, direction.X);
        float halfAngle = AngleDegrees * MathF.PI / 360f;
        using var blend = graphics.PushAdditiveBlend();
        for (int step = FalloffSteps; step >= 1; step--)
        {
            float progress = step / (float)FalloffSteps;
            float distance = Range * progress;
            var left = Position + new Vector2(MathF.Cos(center - halfAngle), MathF.Sin(center - halfAngle)) * distance;
            var right = Position + new Vector2(MathF.Cos(center + halfAngle), MathF.Sin(center + halfAngle)) * distance;
            graphics.DrawTriangle(Position, left, right, ColorAt((1f - progress) * .13f + .015f));
        }
    }
}
