namespace Fix2Engine.Components.Animation3D;

/// <summary>Attaches a <see cref="Timeline3D"/> to an <see cref="Object3D"/> so it advances with the scene.</summary>
public sealed class TimelineComponent3D : Component3D
{
    public Timeline3D Timeline { get; }

    public TimelineComponent3D(Timeline3D? timeline = null) => Timeline = timeline ?? new Timeline3D();

    protected override void OnUpdate(float dt) => Timeline.Update(dt);
}
