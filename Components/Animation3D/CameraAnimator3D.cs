using System.Numerics;
using Fix2Engine.Components.Scene;

namespace Fix2Engine.Components.Animation3D;

/// <summary>Animates the camera of the <see cref="Scene3D"/> that owns this object's scene.</summary>
public sealed class CameraAnimator3D : Component3D
{
    public Timeline3D Timeline { get; }

    public CameraAnimator3D(Timeline3D? timeline = null) => Timeline = timeline ?? new Timeline3D();

    public CameraAnimator3D AnimatePosition(float start, float duration, Vector3 from, Vector3 to,
        EasingKind easing = EasingKind.Linear)
    {
        Timeline.Add(start, duration, t =>
        {
            var camera = Owner?.Scene?.Camera;
            if (camera != null) camera.Position = Vector3.Lerp(from, to, t);
        }, easing);
        return this;
    }

    public CameraAnimator3D AnimateTarget(float start, float duration, Vector3 from, Vector3 to,
        EasingKind easing = EasingKind.Linear)
    {
        Timeline.Add(start, duration, t =>
        {
            var camera = Owner?.Scene?.Camera;
            if (camera != null) camera.Target = Vector3.Lerp(from, to, t);
        }, easing);
        return this;
    }

    public CameraAnimator3D AnimateFieldOfView(float start, float duration, float from, float to,
        EasingKind easing = EasingKind.Linear)
    {
        if (!float.IsFinite(from) || from <= 0) throw new ArgumentOutOfRangeException(nameof(from));
        if (!float.IsFinite(to) || to <= 0) throw new ArgumentOutOfRangeException(nameof(to));
        Timeline.Add(start, duration, t =>
        {
            var camera = Owner?.Scene?.Camera;
            if (camera != null) camera.FieldOfView = from + (to - from) * t;
        }, easing);
        return this;
    }

    public void Play(bool restart = false) => Timeline.Play(restart);
    public void Pause() => Timeline.Pause();
    public void Resume() => Timeline.Resume();
    public void Stop() => Timeline.Stop();
    public void Seek(float time) => Timeline.Seek(time);

    protected override void OnUpdate(float dt) => Timeline.Update(dt);
}
