using System.Numerics;
using Fix2Engine.Core;
using Raylib_cs;
using rlImGui_cs;
using Native = Raylib_cs.Raylib;

namespace Fix2Engine.Backends.Raylib;

public sealed partial class RaylibBackend : IGameBackend
{
    private readonly HashSet<TextureResource> _textures = new();
    private bool _uiReady;
    private bool _disposed;
    public bool IsReady => !_disposed && Native.IsWindowReady();
    public bool ShouldClose => Native.WindowShouldClose();
    public float FrameTime => Native.GetFrameTime();
    public int FramesPerSecond => Native.GetFPS();

    public void Open(int width, int height, string title)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (Native.IsWindowReady()) throw new InvalidOperationException("A native window is already open.");
        Native.InitWindow(width, height, title);
        if (!Native.IsWindowReady()) throw new InvalidOperationException("Could not create the game window.");
    }
    public void SetSize(int width, int height) => Native.SetWindowSize(width, height);
    public void SetTitle(string title) => Native.SetWindowTitle(title);
    public void SetTargetFps(int fps) => Native.SetTargetFPS(fps);
    public void BeginDrawing() => Native.BeginDrawing();
    public void EndDrawing() => Native.EndDrawing();
    public void Clear(Color32 color) => Native.ClearBackground(Convert(color));
    public void DrawRectangle(RectF rectangle, Color32 color) => Native.DrawRectangleRec(Convert(rectangle), Convert(color));
    public void DrawLine(Vector2 start, Vector2 end, float thickness, Color32 color) => Native.DrawLineEx(start, end, thickness, Convert(color));
    public void DrawText(string text, Vector2 position, int fontSize, Color32 color) => Native.DrawText(text, (int)position.X, (int)position.Y, fontSize, Convert(color));
    public ITextureResource LoadTexture(string path)
    {
        if (!IsReady) throw new InvalidOperationException("Create a window before loading textures.");
        var native = Native.LoadTexture(path);
        if (native.Id == 0 || native.Width <= 0 || native.Height <= 0)
            throw new InvalidOperationException($"Could not load texture '{path}'.");
        var resource = new TextureResource(this, native);
        _textures.Add(resource);
        return resource;
    }
    public void DrawTexture(ITextureResource texture, RectF source, RectF destination, Vector2 origin, float rotation, Color32 tint)
    {
        if (texture is not TextureResource resource || resource.Owner != this)
            throw new ArgumentException("Texture belongs to a different backend.", nameof(texture));
        ObjectDisposedException.ThrowIf(resource.IsDisposed, texture);
        Native.DrawTexturePro(resource.NativeTexture, Convert(source), Convert(destination), origin, rotation, Convert(tint));
    }
    public IDisposable PushTransform(Matrix3x2 transform) => new TransformScope(transform);
    public void BeginUI()
    {
        if (!_uiReady) { rlImGui.Setup(true); _uiReady = true; }
        rlImGui.Begin();
    }
    public void EndUI() => rlImGui.End();
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            foreach (var texture in _textures.ToArray()) texture.Dispose();
            if (_uiReady) rlImGui.Shutdown();
        }
        finally { Native.CloseWindow(); }
    }
    private static Color Convert(Color32 c) => new(c.R, c.G, c.B, c.A);
    private static Rectangle Convert(RectF r) => new(r.X, r.Y, r.Width, r.Height);

    private sealed class TextureResource(RaylibBackend owner, Texture2D texture) : ITextureResource
    {
        internal RaylibBackend Owner => owner;
        internal Texture2D NativeTexture => texture;
        public int Width => texture.Width;
        public int Height => texture.Height;
        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Native.UnloadTexture(texture);
            owner._textures.Remove(this);
        }
    }
    private sealed class TransformScope : IDisposable
    {
        private readonly bool _mirrored;
        private bool _disposed;
        public TransformScope(Matrix3x2 transform)
        {
            _mirrored = transform.GetDeterminant() < 0;
            if (_mirrored) { Rlgl.DrawRenderBatchActive(); Rlgl.DisableBackfaceCulling(); }
            Rlgl.PushMatrix();
            Rlgl.MultMatrixf(Matrix4x4.Transpose(new Matrix4x4(transform)));
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Rlgl.PopMatrix();
            if (_mirrored) { Rlgl.DrawRenderBatchActive(); Rlgl.EnableBackfaceCulling(); }
        }
    }
}
