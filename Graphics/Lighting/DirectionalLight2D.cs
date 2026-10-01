using System.Numerics;
using Fix2Engine.Core;

namespace Fix2Engine.Graphics.Lighting;

/// <summary>A broad rectangular light cast in a direction.</summary>
public sealed class DirectionalLight2D : Light2D
{
    /// <summary>Normalized automatically when rendered. The default points down the screen.</summary>
    public Vector2 Direction { get; set; } = Vector2.UnitY;
    public float Length { get; set; } = 480f;
    public float Width { get; set; } = 960f;

    protected override void DrawCore(RenderContext graphics)
    {
        ValidateRange(Length, nameof(Length));
        ValidateRange(Width, nameof(Width));
        var direction = NormalizeDirection(Direction, nameof(Direction));
        float rotation = MathF.Atan2(direction.Y, direction.X) - MathF.PI / 2f;
        using var transform = graphics.PushTransform(Matrix3x2.CreateRotation(rotation) * Matrix3x2.CreateTranslation(Position));
        using var blend = graphics.PushAdditiveBlend();
        const int bands = 8;
        for (int band = bands; band >= 1; band--)
        {
            float progress = band / (float)bands;
            float start = Length * (1f - progress);
            graphics.DrawRectangle(new RectF(-Width / 2, start, Width, Length * progress),
                ColorAt((1f - progress) * .075f + .012f));
        }
    }
}
