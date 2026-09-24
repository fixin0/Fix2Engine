using System.Numerics;
using Fix2Engine.Core;

namespace Fix2Engine.Graphics;

/// <summary>Low-level texture resource. Scene entities use Components.SpriteObject2D.</summary>
public class Sprite2D : IDisposable
{
    public Texture Texture { get; private set; }
    public bool IsDisposed { get; private set; }
    public bool OwnsTexture { get; }
    public Vector2 Position { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Origin { get; set; }
    public float Rotation { get; set; }
    public Color32 Tint { get; set; } = Color32.White;
    public RectF SourceRect { get; set; }

    public Sprite2D(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        Texture = Content.LoadTexture(filePath);
        OwnsTexture = true;
        SourceRect = new RectF(0, 0, Texture.Width, Texture.Height);
    }

    /// <summary>Existing texture handles are borrowed unless ownership is explicitly transferred.</summary>
    public Sprite2D(Texture texture, bool ownsTexture = false)
    {
        ArgumentNullException.ThrowIfNull(texture);
        ObjectDisposedException.ThrowIf(texture.IsDisposed, texture);
        Texture = texture;
        OwnsTexture = ownsTexture;
        SourceRect = new RectF(0, 0, texture.Width, texture.Height);
    }

    public void CenterOrigin() => Origin = new Vector2(MathF.Abs(SourceRect.Width), MathF.Abs(SourceRect.Height)) / 2;

    public void Draw()
    {
        if (IsDisposed || Texture.IsDisposed) return;
        var source = SourceRect;
        if (Scale.X < 0) source.Width = -source.Width;
        if (Scale.Y < 0) source.Height = -source.Height;
        var size = new Vector2(MathF.Abs(SourceRect.Width * Scale.X), MathF.Abs(SourceRect.Height * Scale.Y));
        RenderContext.Current.DrawTexture(Texture, source, new RectF(Position.X, Position.Y, size.X, size.Y),
            Origin * Vector2.Abs(Scale), Rotation, Tint);
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        if (OwnsTexture) Texture.Dispose();
        GC.SuppressFinalize(this);
    }
}
