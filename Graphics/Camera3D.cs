using System.Numerics;
using Fix2Engine.Core;

namespace Fix2Engine.Graphics;

/// <summary>Engine-owned 3D camera. Angles and native camera types do not leak into game code.</summary>
public sealed class Camera3D
{
    public Vector3 Position { get; set; } = new(7, 5, 7);
    public Vector3 Target { get; set; } = Vector3.Zero;
    public Vector3 Up { get; set; } = Vector3.UnitY;
    public float FieldOfView { get; set; } = 60f;
    public Projection3D Projection { get; set; } = Projection3D.Perspective;

    internal Camera3DState ToState() => new(Position, Target, Up, FieldOfView, Projection);
}
