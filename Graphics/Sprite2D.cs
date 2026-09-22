using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Fix2Engine.Graphics;

/// <summary>Low-level texture resource. Scene entities use Components.SpriteObject2D.</summary>
public class Sprite2D : IDisposable
{
    public Texture2D Texture { get; private set; }
    public bool IsDisposed { get; private set; }
    public bool OwnsTexture { get; }
    public Vector2 Position { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Origin { get; set; }
    public float Rotation { get; set; }
    public Color Tint { get; set; } = Color.White;
    public Rectangle SourceRect { get; set; }

    public Sprite2D(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        if (!IsWindowReady()) throw new InvalidOperationException("Create a window before loading textures.");
        Texture = LoadTexture(filePath);
        if (Texture.Id == 0 || Texture.Width <= 0 || Texture.Height <= 0)
            throw new InvalidOperationException($"Could not load texture '{filePath}'.");
        OwnsTexture = true;
        SourceRect = new Rectangle(0, 0, Texture.Width, Texture.Height);
    }

    /// <summary>Existing texture handles are borrowed unless ownership is explicitly transferred.</summary>
    public Sprite2D(Texture2D texture, bool ownsTexture = false)
    {
        if (texture.Id == 0 || texture.Width <= 0 || texture.Height <= 0)
            throw new ArgumentException("A valid texture is required.", nameof(texture));
        Texture = texture;
        OwnsTexture = ownsTexture;
        SourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
    }

    public void CenterOrigin() => Origin = new Vector2(MathF.Abs(SourceRect.Width), MathF.Abs(SourceRect.Height)) / 2;

    public void Draw()
    {
        if (IsDisposed) return;
        var source = SourceRect;
        if (Scale.X < 0) source.Width = -source.Width;
        if (Scale.Y < 0) source.Height = -source.Height;
        var size = new Vector2(MathF.Abs(SourceRect.Width * Scale.X), MathF.Abs(SourceRect.Height * Scale.Y));
        DrawTexturePro(Texture, source, new Rectangle(Position.X, Position.Y, size.X, size.Y),
            Origin * Vector2.Abs(Scale), Rotation, Tint);
    }

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        if (OwnsTexture && Texture.Id != 0) UnloadTexture(Texture);
        Texture = default;
        GC.SuppressFinalize(this);
    }
}
