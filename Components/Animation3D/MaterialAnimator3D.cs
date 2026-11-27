using System.Numerics;
using Fix2Engine.Graphics;

namespace Fix2Engine.Components.Animation3D;

/// <summary>Animates named <see cref="Material3D"/> parameters over time.</summary>
public sealed class MaterialAnimator3D : Component3D
{
    public Timeline3D Timeline { get; }
    public Material3D? Material { get; set; }

    public MaterialAnimator3D(Material3D? material = null, Timeline3D? timeline = null)
    {
        Material = material;
        Timeline = timeline ?? new Timeline3D();
    }

    public MaterialAnimator3D AnimateFloat(string name, float start, float duration, float from, float to,
        EasingKind easing = EasingKind.Linear)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Timeline.Add(start, duration, t => ResolveMaterial()?.SetFloat(name, from + (to - from) * t), easing);
        return this;
    }

    public MaterialAnimator3D AnimateVector2(string name, float start, float duration, Vector2 from, Vector2 to,
        EasingKind easing = EasingKind.Linear)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Timeline.Add(start, duration, t => ResolveMaterial()?.SetVector2(name, Vector2.Lerp(from, to, t)), easing);
        return this;
    }

    public MaterialAnimator3D AnimateVector3(string name, float start, float duration, Vector3 from, Vector3 to,
        EasingKind easing = EasingKind.Linear)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Timeline.Add(start, duration, t => ResolveMaterial()?.SetVector3(name, Vector3.Lerp(from, to, t)), easing);
        return this;
    }

    public MaterialAnimator3D AnimateVector4(string name, float start, float duration, Vector4 from, Vector4 to,
        EasingKind easing = EasingKind.Linear)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Timeline.Add(start, duration, t => ResolveMaterial()?.SetVector4(name, Vector4.Lerp(from, to, t)), easing);
        return this;
    }

    public void Play(bool restart = false) => Timeline.Play(restart);
    public void Pause() => Timeline.Pause();
    public void Resume() => Timeline.Resume();
    public void Stop() => Timeline.Stop();
    public void Seek(float time) => Timeline.Seek(time);

    protected override void OnUpdate(float dt) => Timeline.Update(dt);

    private Material3D? ResolveMaterial()
    {
        if (Material != null) return Material;
        if (Owner == null) return null;
        foreach (var component in Owner.Components)
            if (component is ModelRenderer3D renderer) return renderer.Material;
        return null;
    }
}
