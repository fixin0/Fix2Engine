using System.Numerics;
using Fix2Engine.Core;

namespace Fix2Engine.Graphics;

/// <summary>Drawing operations available during a render callback.</summary>
public sealed class RenderContext
{
    public static RenderContext Current { get; } = new();
    private RenderContext() { }
    public int FramesPerSecond => EngineBackend.Current.FramesPerSecond;
    public float FrameTime => EngineBackend.Current.FrameTime;
    public void Clear(Color32 color) => EngineBackend.Current.Clear(color);
    public void DrawRectangle(RectF rectangle, Color32 color) => EngineBackend.Current.DrawRectangle(rectangle, color);
    public void DrawLine(Vector2 start, Vector2 end, Color32 color, float thickness = 1) => EngineBackend.Current.DrawLine(start, end, thickness, color);
    public void DrawText(string text, Vector2 position, int fontSize, Color32 color) => EngineBackend.Current.DrawText(text, position, fontSize, color);
    public void DrawTexture(Texture texture, Vector2 position, Color32? tint = null)
    {
        ArgumentNullException.ThrowIfNull(texture);
        DrawTexture(texture, new(0, 0, texture.Width, texture.Height), new(position.X, position.Y, texture.Width, texture.Height), Vector2.Zero, 0, tint ?? Color32.White);
    }
    public void DrawTexture(Texture texture, RectF source, RectF destination, Vector2 origin, float rotation, Color32 tint)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ObjectDisposedException.ThrowIf(texture.IsDisposed, texture);
        EngineBackend.Current.DrawTexture(texture.Resource, source, destination, origin, rotation, tint);
    }
    public IDisposable PushTransform(Matrix3x2 transform) => EngineBackend.Current.PushTransform(transform);
}
