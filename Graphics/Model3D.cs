using System.Numerics;
using Fix2Engine.Core;

namespace Fix2Engine.Graphics;

/// <summary>A GPU model that may originate from a file or an engine-generated primitive.</summary>
public sealed class Model3D : IDisposable
{
    internal IModel3DResource Resource { get; }
    public bool IsDisposed => Resource.IsDisposed;
    private Model3D(IModel3DResource resource) => Resource = resource;

    public static Model3D Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new Model3D(EngineBackend.Current.LoadModel3D(path));
    }

    public static Model3D Cube(Vector3 size) => Create(Primitive3D.Cube, size, 3, 2);
    public static Model3D Plane(float width, float depth, int subdivisions = 1) =>
        Create(Primitive3D.Plane, new Vector3(width, 1, depth), Math.Max(3, subdivisions), Math.Max(2, subdivisions));
    public static Model3D Sphere(float diameter, int slices = 32, int rings = 16) =>
        Create(Primitive3D.Sphere, new Vector3(diameter), slices, rings);
    public static Model3D Cylinder(float diameter, float height, int slices = 32) =>
        Create(Primitive3D.Cylinder, new Vector3(diameter, height, diameter), slices, 2);

    private static Model3D Create(Primitive3D primitive, Vector3 size, int slices, int rings) =>
        new(EngineBackend.Current.CreatePrimitive3D(primitive, size, slices, rings));

    public void Dispose() => Resource.Dispose();
}
