using System.Numerics;

namespace Fix2Engine.Components.Animation3D;

/// <summary>A position, quaternion rotation and scale sampled from a <see cref="TransformClip3D"/>.</summary>
public readonly record struct TransformPose3D(Vector3 Position, Quaternion Rotation, Vector3 Scale)
{
    public static TransformPose3D Identity => new(Vector3.Zero, Quaternion.Identity, Vector3.One);
}

/// <summary>
/// Reusable keyframe data for an object transform. Tracks may have different key times; missing tracks
/// keep their identity value. The clip owns no scene objects or GPU resources.
/// </summary>
public sealed class TransformClip3D
{
    private readonly List<Keyframe3D<Vector3>> _positions = new();
    private readonly List<Keyframe3D<Quaternion>> _rotations = new();
    private readonly List<Keyframe3D<Vector3>> _scales = new();
    private AnimationCurve3D<Vector3>? _positionCurve;
    private AnimationCurve3D<Quaternion>? _rotationCurve;
    private AnimationCurve3D<Vector3>? _scaleCurve;
    private bool _dirty = true;

    public string Name { get; }
    /// <summary>When true, playback wraps to the beginning instead of finishing.</summary>
    public bool Loop { get; set; } = true;
    public float Duration { get; private set; }
    public bool HasPositionKeys => _positions.Count > 0;
    public bool HasRotationKeys => _rotations.Count > 0;
    public bool HasScaleKeys => _scales.Count > 0;

    public TransformClip3D(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public TransformClip3D AddPosition(float time, Vector3 value, EasingKind easing = EasingKind.Linear)
    {
        ValidateTime(time);
        _positions.Add(new Keyframe3D<Vector3>(time, value, ValidateEasing(easing)));
        Touch(time);
        return this;
    }

    /// <summary>Adds a rotation key using Euler degrees in X pitch, Y yaw, Z roll order.</summary>
    public TransformClip3D AddRotation(float time, Vector3 eulerDegrees, EasingKind easing = EasingKind.Linear) =>
        AddRotation(time, FromEuler(eulerDegrees), easing);

    public TransformClip3D AddRotation(float time, Quaternion value, EasingKind easing = EasingKind.Linear)
    {
        ValidateTime(time);
        ValidateRotation(value);
        _rotations.Add(new Keyframe3D<Quaternion>(time, Quaternion.Normalize(value), ValidateEasing(easing)));
        Touch(time);
        return this;
    }

    public TransformClip3D AddScale(float time, Vector3 value, EasingKind easing = EasingKind.Linear)
    {
        ValidateTime(time);
        _scales.Add(new Keyframe3D<Vector3>(time, value, ValidateEasing(easing)));
        Touch(time);
        return this;
    }

    public TransformClip3D AddPose(float time, TransformPose3D pose, EasingKind easing = EasingKind.Linear)
    {
        AddPosition(time, pose.Position, easing);
        AddRotation(time, pose.Rotation, easing);
        AddScale(time, pose.Scale, easing);
        return this;
    }

    public TransformPose3D Evaluate(float time)
    {
        EnsureCurves();
        if (!float.IsFinite(time)) throw new ArgumentOutOfRangeException(nameof(time));
        return new TransformPose3D(
            _positionCurve?.Evaluate(time) ?? Vector3.Zero,
            _rotationCurve?.Evaluate(time) ?? Quaternion.Identity,
            _scaleCurve?.Evaluate(time) ?? Vector3.One);
    }

    private void Touch(float time)
    {
        _dirty = true;
        Duration = MathF.Max(Duration, time);
    }

    private void EnsureCurves()
    {
        if (!_dirty) return;
        if (_positions.Count == 0 && _rotations.Count == 0 && _scales.Count == 0)
            throw new InvalidOperationException("A transform clip needs at least one keyframe.");
        _positionCurve = _positions.Count > 0 ? AnimationCurves3D.Vector(_positions) : null;
        _rotationCurve = _rotations.Count > 0 ? AnimationCurves3D.Rotation(_rotations) : null;
        _scaleCurve = _scales.Count > 0 ? AnimationCurves3D.Vector(_scales) : null;
        Duration = MathF.Max(
            _positionCurve?.Duration ?? 0f,
            MathF.Max(_rotationCurve?.Duration ?? 0f, _scaleCurve?.Duration ?? 0f));
        _dirty = false;
    }

    private static Quaternion FromEuler(Vector3 degrees)
    {
        const float toRadians = MathF.PI / 180f;
        return Quaternion.CreateFromYawPitchRoll(degrees.Y * toRadians, degrees.X * toRadians, degrees.Z * toRadians);
    }

    private static void ValidateTime(float time)
    {
        if (!float.IsFinite(time) || time < 0) throw new ArgumentOutOfRangeException(nameof(time));
    }

    private static EasingKind ValidateEasing(EasingKind easing) =>
        Enum.IsDefined(easing) ? easing : throw new ArgumentOutOfRangeException(nameof(easing));

    private static void ValidateRotation(Quaternion value)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) || !float.IsFinite(value.Z) || !float.IsFinite(value.W) ||
            value.LengthSquared() < 1e-12f)
            throw new ArgumentOutOfRangeException(nameof(value));
    }
}
