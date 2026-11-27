namespace Fix2Engine.Components.Animation3D;

/// <summary>Interpolation curves used by 3D animation clips and timelines.</summary>
public enum EasingKind
{
    Linear,
    SineIn,
    SineOut,
    SineInOut,
    QuadIn,
    QuadOut,
    QuadInOut,
    CubicIn,
    CubicOut,
    CubicInOut,
    BackIn,
    BackOut,
    BackInOut,
    BounceOut,
    ElasticOut
}

public static class Easing
{
    private const float BackOvershoot = 1.70158f;
    private const float BackOvershootInOut = BackOvershoot * 1.525f;

    /// <summary>Evaluates an easing curve. Input and output are in the 0..1 range.</summary>
    public static float Evaluate(EasingKind kind, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return kind switch
        {
            EasingKind.Linear => t,
            EasingKind.SineIn => 1f - MathF.Cos(t * MathF.PI / 2f),
            EasingKind.SineOut => MathF.Sin(t * MathF.PI / 2f),
            EasingKind.SineInOut => -(MathF.Cos(MathF.PI * t) - 1f) / 2f,
            EasingKind.QuadIn => t * t,
            EasingKind.QuadOut => 1f - (1f - t) * (1f - t),
            EasingKind.QuadInOut => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f,
            EasingKind.CubicIn => t * t * t,
            EasingKind.CubicOut => 1f - MathF.Pow(1f - t, 3f),
            EasingKind.CubicInOut => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f,
            EasingKind.BackIn => (BackOvershoot + 1f) * t * t * t - BackOvershoot * t * t,
            EasingKind.BackOut => 1f + (BackOvershoot + 1f) * MathF.Pow(t - 1f, 3f) + BackOvershoot * MathF.Pow(t - 1f, 2f),
            EasingKind.BackInOut => t < 0.5f
                ? MathF.Pow(2f * t, 2f) * ((BackOvershootInOut + 1f) * 2f * t - BackOvershootInOut) / 2f
                : (MathF.Pow(2f * t - 2f, 2f) * ((BackOvershootInOut + 1f) * (t * 2f - 2f) + BackOvershootInOut) + 2f) / 2f,
            EasingKind.BounceOut => BounceOut(t),
            EasingKind.ElasticOut => t is 0f or 1f ? t : MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * (2f * MathF.PI / 3f)) + 1f,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
    }

    private static float BounceOut(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;
        if (t < 1f / d1) return n1 * t * t;
        if (t < 2f / d1) return n1 * (t -= 1.5f / d1) * t + 0.75f;
        if (t < 2.5f / d1) return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        return n1 * (t -= 2.625f / d1) * t + 0.984375f;
    }
}
