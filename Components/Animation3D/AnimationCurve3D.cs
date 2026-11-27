using System.Numerics;

namespace Fix2Engine.Components.Animation3D;

/// <summary>A single value at a time on an <see cref="AnimationCurve3D{T}"/>.</summary>
public readonly record struct Keyframe3D<T>(float Time, T Value, EasingKind Easing = EasingKind.Linear) where T : struct;

/// <summary>An immutable, time-indexed curve. Values are clamped before the first and after the last key.</summary>
public sealed class AnimationCurve3D<T> where T : struct
{
    private readonly Keyframe3D<T>[] _keyframes;
    private readonly Func<T, T, float, T> _interpolate;

    public AnimationCurve3D(IEnumerable<Keyframe3D<T>> keyframes, Func<T, T, float, T> interpolate)
    {
        ArgumentNullException.ThrowIfNull(keyframes);
        ArgumentNullException.ThrowIfNull(interpolate);
        var copy = keyframes.OrderBy(key => key.Time).ToArray();
        if (copy.Length == 0) throw new ArgumentException("A curve needs at least one keyframe.", nameof(keyframes));
        for (int i = 0; i < copy.Length; i++)
        {
            var key = copy[i];
            if (!float.IsFinite(key.Time) || key.Time < 0)
                throw new ArgumentException("Keyframe times must be finite and non-negative.", nameof(keyframes));
            if (i > 0 && key.Time <= copy[i - 1].Time)
                throw new ArgumentException("Keyframe times must be strictly increasing.", nameof(keyframes));
            if (!Enum.IsDefined(key.Easing))
                throw new ArgumentOutOfRangeException(nameof(keyframes), $"Unknown easing value '{key.Easing}'.");
        }
        _keyframes = copy;
        _interpolate = interpolate;
    }

    public IReadOnlyList<Keyframe3D<T>> Keyframes => _keyframes;
    public float Duration => _keyframes[^1].Time;

    public T Evaluate(float time)
    {
        if (!float.IsFinite(time)) throw new ArgumentOutOfRangeException(nameof(time));
        if (time <= _keyframes[0].Time) return _keyframes[0].Value;
        if (time >= _keyframes[^1].Time) return _keyframes[^1].Value;
        int low = 0;
        int high = _keyframes.Length - 1;
        while (low + 1 < high)
        {
            int middle = (low + high) / 2;
            if (_keyframes[middle].Time <= time) low = middle;
            else high = middle;
        }
        var from = _keyframes[low];
        var to = _keyframes[high];
        float progress = (time - from.Time) / (to.Time - from.Time);
        return _interpolate(from.Value, to.Value, Easing.Evaluate(to.Easing, progress));
    }
}

/// <summary>Factory helpers for the value types used by built-in 3D animation.</summary>
public static class AnimationCurves3D
{
    public static AnimationCurve3D<float> Float(IEnumerable<Keyframe3D<float>> keyframes) =>
        new(keyframes, static (from, to, t) => from + (to - from) * t);

    public static AnimationCurve3D<Vector3> Vector(IEnumerable<Keyframe3D<Vector3>> keyframes) =>
        new(keyframes, static (from, to, t) => Vector3.Lerp(from, to, t));

    public static AnimationCurve3D<Quaternion> Rotation(IEnumerable<Keyframe3D<Quaternion>> keyframes) =>
        new(keyframes, static (from, to, t) => System.Numerics.Quaternion.Slerp(from, to, t));
}
