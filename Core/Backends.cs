using System.Numerics;

namespace Fix2Engine.Core;

/// <summary>Backend extension point; implementations own their native resource.</summary>
public interface ITextureResource : IDisposable
{
    int Width { get; }
    int Height { get; }
    bool IsDisposed { get; }
}

public interface IRenderBackend
{
    ITextureResource LoadTexture(string path);
    void Clear(Color32 color);
    void DrawTexture(ITextureResource texture, RectF source, RectF destination, Vector2 origin, float rotation, Color32 tint);
    void DrawRectangle(RectF rectangle, Color32 color);
    void DrawLine(Vector2 start, Vector2 end, float thickness, Color32 color);
    void DrawText(string text, Vector2 position, int fontSize, Color32 color);
    IDisposable PushTransform(Matrix3x2 transform);
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

public interface IGameBackend : IRenderBackend, IWindowBackend, IInputBackend
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
