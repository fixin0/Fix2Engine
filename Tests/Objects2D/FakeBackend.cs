using System.Numerics;
using Fix2Engine.Core;

sealed class FakeBackend : IGameBackend
{
    public bool IsReady { get; private set; }
    public bool ShouldClose => false;
    public float FrameTime => 1f / 60f;
    public int FramesPerSecond => 60;
    public int BeginCount, EndCount, DisposeCount, DrawCount, Draw3DCount, CircleCount, TriangleCount, AdditiveDepth, TransformDepth;
    public RectF LastSource;
    public Color32 LastTint;
    public Matrix3x2 LastTransform;
    public readonly HashSet<int> Keys = new();
    public readonly List<FakeTexture> Textures = new();
    public readonly List<FakeModel> Models = new();
    public readonly List<FakeShader> Shaders = new();
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
    public void DrawCircle(Vector2 center, float radius, Color32 color) => CircleCount++;
    public void DrawTriangle(Vector2 first, Vector2 second, Vector2 third, Color32 color) => TriangleCount++;
    public void DrawLine(Vector2 start, Vector2 end, float thickness, Color32 color) { }
    public void DrawText(string text, Vector2 position, int fontSize, Color32 color) { }
    public IModel3DResource LoadModel3D(string path) => CreateModel();
    public IModel3DResource CreatePrimitive3D(Primitive3D primitive, Vector3 dimensions, int slices, int rings) => CreateModel();
    public IShaderResource LoadShader3D(string vertexPath, string fragmentPath) => CreateShader();
    public IShaderResource LoadShader3DFromSource(string vertexSource, string fragmentSource) => CreateShader();
    public IDisposable Begin3D(Camera3DState camera) => new Scope(() => { });
    public void DrawModel3D(IModel3DResource model, Matrix4x4 transform, Color32 tint) => Draw3DCount++;
    public void SetModelShader3D(IModel3DResource model, IShaderResource shader) { }
    public void SetModelTexture3D(IModel3DResource model, MaterialTextureSlot slot, ITextureResource texture) { }
    public void SetShaderFloat(IShaderResource shader, string name, float value) { }
    public void SetShaderVector2(IShaderResource shader, string name, Vector2 value) { }
    public void SetShaderVector3(IShaderResource shader, string name, Vector3 value) { }
    public void SetShaderVector4(IShaderResource shader, string name, Vector4 value) { }
    public void SetShaderTexture(IShaderResource shader, string name, ITextureResource texture) { }
    public IDisposable PushTransform(Matrix3x2 transform)
    {
        TransformDepth++; LastTransform = transform;
        return new Scope(() => TransformDepth--);
    }
    public IDisposable PushAdditiveBlend()
    {
        AdditiveDepth++;
        return new Scope(() => AdditiveDepth--);
    }
    public void Dispose()
    {
        DisposeCount++; IsReady = false;
        foreach (var texture in Textures) texture.Dispose();
        foreach (var model in Models) model.Dispose();
        foreach (var shader in Shaders) shader.Dispose();
    }
    private FakeModel CreateModel() { var model = new FakeModel(); Models.Add(model); return model; }
    private FakeShader CreateShader() { var shader = new FakeShader(); Shaders.Add(shader); return shader; }
    private sealed class Scope(Action close) : IDisposable { public void Dispose() => close(); }
}

sealed class FakeModel : IModel3DResource
{
    public bool IsDisposed { get; private set; }
    public void Dispose() => IsDisposed = true;
}

sealed class FakeShader : IShaderResource
{
    public bool IsDisposed { get; private set; }
    public void Dispose() => IsDisposed = true;
}

sealed class FakeTexture : ITextureResource
{
    public int Width => 32;
    public int Height => 16;
    public bool IsDisposed { get; private set; }
    public int Unloads;
    public void Dispose() { if (IsDisposed) return; IsDisposed = true; Unloads++; }
}
