using System.Numerics;
using Fix2Engine.Core;

namespace Fix2Engine.Graphics.Lighting;

/// <summary>Base type for additive 2D lights. Call <see cref="Draw"/> from a render callback.</summary>
public abstract class Light2D
{
    /// <summary>Position in the coordinate space used by the render callback.</summary>
    public Vector2 Position { get; set; }
    public Color32 Color { get; set; } = new(255, 244, 214);
    /// <summary>Brightness multiplier. Zero hides the light; values above one are allowed.</summary>
    public float Energy { get; set; } = 1f;
    public bool Enabled { get; set; } = true;

    public void Draw(RenderContext graphics)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        if (!Enabled || Energy <= 0) return;
        if (!float.IsFinite(Energy)) throw new ArgumentOutOfRangeException(nameof(Energy));
        DrawCore(graphics);
    }

    protected abstract void DrawCore(RenderContext graphics);

    protected Color32 ColorAt(float opacity)
    {
        opacity = Math.Clamp(opacity * Energy, 0f, 1f);
        return new Color32(Color.R, Color.G, Color.B, (byte)MathF.Round(Color.A * opacity));
    }

    protected static void ValidateRange(float range, string parameterName)
    {
        if (!float.IsFinite(range) || range <= 0) throw new ArgumentOutOfRangeException(parameterName);
    }

    protected static Vector2 NormalizeDirection(Vector2 direction, string parameterName)
    {
        if (!float.IsFinite(direction.X) || !float.IsFinite(direction.Y) || direction.LengthSquared() < float.Epsilon)
            throw new ArgumentOutOfRangeException(parameterName);
        return Vector2.Normalize(direction);
    }
}
