using System.Numerics;
using Fix2Engine.Core;

namespace Fix2Engine.Graphics;

/// <summary>Frame-wide 3D settings and safe entry points for 3D rendering.</summary>
public static class Graphics3D
{
    private static float _time;
    private static Vector3 _activeCameraPosition;
    public static float Time => _time;
    public static Vector3 ActiveCameraPosition => _activeCameraPosition;
    public static Vector3 AmbientColor { get; set; } = new(0.12f, 0.15f, 0.22f);
    public static Vector3 LightDirection { get; set; } = Vector3.Normalize(new(-0.45f, -1f, -0.25f));
    public static Vector3 LightColor { get; set; } = new(1.0f, 0.94f, 0.82f);

    public static IDisposable Begin(Camera3D camera)
    {
        ArgumentNullException.ThrowIfNull(camera);
        _activeCameraPosition = camera.Position;
        return EngineBackend.Current.Begin3D(camera.ToState());
    }

    public static void DrawModel(Model3D model, Material3D material, Matrix4x4 transform)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(material);
        ObjectDisposedException.ThrowIf(model.IsDisposed, model);
        material.Apply(model);
        EngineBackend.Current.DrawModel3D(model.Resource, transform, material.Tint);
    }

    internal static void AdvanceFrame(float deltaTime) => _time += MathF.Max(0, deltaTime);
}
