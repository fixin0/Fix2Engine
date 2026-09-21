using System;
using System.Numerics;

namespace Fix2Engine.Components;

public class Node3D
{
    public Guid Guid { get; private set; }
    public string Name { get; set; }
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;
    public bool IsActive { get; set; } = true;

    public Node3D(string name = "Node3D")
    {
        Guid = Guid.NewGuid();
        Name = name;
    }
}
