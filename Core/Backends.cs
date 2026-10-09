using System.Numerics;

namespace Fix2Engine.Core;

/// <summary>Backend extension point; implementations own their native resource.</summary>
public interface ITextureResource : IDisposable
{
    int Width { get; }
    int Height { get; }
    bool IsDisposed { get; }
}

/// <summary>Opaque native model handle owned by a rendering backend.</summary>
public interface IModel3DResource : IDisposable
{
    bool IsDisposed { get; }
}

/// <summary>Opaque native shader handle owned by a rendering backend.</summary>
public interface IShaderResource : IDisposable
{
    bool IsDisposed { get; }
}

public enum Projection3D { Perspective, Orthographic }

/// <summary>Camera data shared with 3D-capable backends without exposing native types.</summary>
public readonly record struct Camera3DState(Vector3 Position, Vector3 Target, Vector3 Up, float FieldOfView, Projection3D Projection)
{
    public static Camera3DState Default => new(new(6, 4, 6), Vector3.Zero, Vector3.UnitY, 60, Projection3D.Perspective);
}

public enum Primitive3D { Cube, Plane, Sphere, Cylinder }

/// <summary>Texture slots understood by built-in 3D shaders.</summary>
public enum MaterialTextureSlot { Albedo, Normal, Metallic, Roughness, Occlusion, Emission }

/// <summary>Optional 3D rendering capability implemented by native game backends.</summary>
public interface IRender3DBackend
{
    IModel3DResource LoadModel3D(string path);
    IModel3DResource CreatePrimitive3D(Primitive3D primitive, Vector3 dimensions, int slices, int rings);
    IShaderResource LoadShader3D(string vertexPath, string fragmentPath);
    IShaderResource LoadShader3DFromSource(string vertexSource, string fragmentSource);
    IDisposable Begin3D(Camera3DState camera);
    void DrawModel3D(IModel3DResource model, Matrix4x4 transform, Color32 tint);
    void SetModelShader3D(IModel3DResource model, IShaderResource shader);
    void SetModelTexture3D(IModel3DResource model, MaterialTextureSlot slot, ITextureResource texture);
    void SetShaderFloat(IShaderResource shader, string name, float value);
    void SetShaderVector2(IShaderResource shader, string name, Vector2 value);
    void SetShaderVector3(IShaderResource shader, string name, Vector3 value);
    void SetShaderVector4(IShaderResource shader, string name, Vector4 value);
    void SetShaderTexture(IShaderResource shader, string name, ITextureResource texture);
}

public interface IRenderBackend
{
    ITextureResource LoadTexture(string path);
    void Clear(Color32 color);
    void DrawTexture(ITextureResource texture, RectF source, RectF destination, Vector2 origin, float rotation, Color32 tint);
    void DrawRectangle(RectF rectangle, Color32 color);
    void DrawCircle(Vector2 center, float radius, Color32 color);
    void DrawTriangle(Vector2 first, Vector2 second, Vector2 third, Color32 color);
    void DrawLine(Vector2 start, Vector2 end, float thickness, Color32 color);
    void DrawText(string text, Vector2 position, int fontSize, Color32 color);
    IDisposable PushTransform(Matrix3x2 transform);
    IDisposable PushAdditiveBlend();
    void BeginDrawing();
    void EndDrawing();
}

public interface IWindowBackend : IDisposable
{
    bool IsReady { get; }
    bool ShouldClose { get; }
    float FrameTime { get; }
    int FramesPerSecond { get; }
    void Open(int width, int height, string title);
    void SetSize(int width, int height);
    void SetTitle(string title);
    void SetTargetFps(int fps);
}

public interface IInputBackend
{
    bool IsKeyDown(int virtualKey);
}

public interface IGameBackend : IRenderBackend, IRender3DBackend, IWindowBackend, IInputBackend
{
    void BeginUI();
    void EndUI();
}

/// <summary>The active, single-window backend. The application host owns its lifetime.</summary>
public static class EngineBackend
{
    private static IGameBackend? _current;
    public static IGameBackend Current => _current ?? throw new InvalidOperationException("Create a game window before using graphics or input.");
    public static bool IsAttached => _current != null;
    public static void Attach(IGameBackend backend)
    {
        ArgumentNullException.ThrowIfNull(backend);
        if (_current != null) throw new InvalidOperationException("Only one game window may be active.");
        _current = backend;
    }
    public static void Detach(IGameBackend backend)
    {
        if (ReferenceEquals(_current, backend)) _current = null;
    }
}
