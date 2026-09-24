using System.Numerics;
using Fix2Engine.Core;

sealed class FakeBackend : IGameBackend
{
    public bool IsReady { get; private set; }
    public bool ShouldClose => false;
    public float FrameTime => 1f / 60f;
    public int FramesPerSecond => 60;
    public int BeginCount, EndCount, DisposeCount, DrawCount, TransformDepth;
    public RectF LastSource;
    public Color32 LastTint;
    public Matrix3x2 LastTransform;
    public readonly HashSet<int> Keys = new();
    public readonly List<FakeTexture> Textures = new();
    public void Open(int width, int height, string title) => IsReady = true;
    public void SetSize(int width, int height) { }
    public void SetTitle(string title) { }
    public void SetTargetFps(int fps) { }
    public void BeginDrawing() => BeginCount++;
    public void EndDrawing() { if (!IsReady) throw new Exception("Window closed before EndDrawing."); EndCount++; }
    public void Clear(Color32 color) { }
    public bool IsKeyDown(int virtualKey) => Keys.Contains(virtualKey);
    public void BeginUI() { }
    public void EndUI() { }
    public ITextureResource LoadTexture(string path)
    {
        var texture = new FakeTexture();
        Textures.Add(texture);
        return texture;
    }
    public void DrawTexture(ITextureResource texture, RectF source, RectF destination, Vector2 origin, float rotation, Color32 tint)
    {
        ObjectDisposedException.ThrowIf(texture.IsDisposed, texture);
        DrawCount++; LastSource = source; LastTint = tint;
    }
    public void DrawRectangle(RectF rectangle, Color32 color) { }
    public void DrawLine(Vector2 start, Vector2 end, float thickness, Color32 color) { }
    public void DrawText(string text, Vector2 position, int fontSize, Color32 color) { }
    public IDisposable PushTransform(Matrix3x2 transform)
    {
        TransformDepth++; LastTransform = transform;
        return new Scope(() => TransformDepth--);
    }
    public void Dispose()
    {
        DisposeCount++; IsReady = false;
        foreach (var texture in Textures) texture.Dispose();
    }
    private sealed class Scope(Action close) : IDisposable { public void Dispose() => close(); }
}

sealed class FakeTexture : ITextureResource
{
    public int Width => 32;
    public int Height => 16;
    public bool IsDisposed { get; private set; }
    public int Unloads;
    public void Dispose() { if (IsDisposed) return; IsDisposed = true; Unloads++; }
}
