using System.Numerics;
using Fix2Engine.Graphics;
using Raylib_cs;

namespace Fix2Engine.Components;

public class SpriteObject2D : Object2D
{
    private Sprite2D? _sprite;
    private bool _ownsSprite;
    private string? _texturePath;

    /// <summary>Loaded once on first render, and released when changed or destroyed.</summary>
    public string? TexturePath
    {
        get => _texturePath;
        set
        {
            ThrowIfDestroyed();
            if (_texturePath == value) return;
            ReleaseSprite();
            _texturePath = value;
        }
    }

    public Sprite2D? Sprite => _sprite;
    public Rectangle? SourceRect { get; set; }
    public Vector2 Origin { get; set; }
    public Color Tint { get; set; } = Color.White;
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }
    protected virtual Rectangle? FrameRectangle => SourceRect;

    public SpriteObject2D(string name = "SpriteObject2D") : base(name) { }

    public void SetSprite(Sprite2D? sprite, bool ownsSprite = false)
    {
        ThrowIfDestroyed();
        if (sprite?.IsDisposed == true) throw new ObjectDisposedException(nameof(sprite));
        if (ReferenceEquals(_sprite, sprite) && _texturePath == null)
        {
            _ownsSprite = ownsSprite;
            return;
        }
        ReleaseSprite();
        _texturePath = null;
        _sprite = sprite;
        _ownsSprite = ownsSprite;
    }

    protected override void OnRender()
    {
        if (_sprite == null && !string.IsNullOrWhiteSpace(_texturePath))
        {
            _sprite = new Sprite2D(_texturePath);
            _ownsSprite = true;
        }
        if (_sprite == null || _sprite.IsDisposed) return;
        Rectangle source = FrameRectangle ?? _sprite.SourceRect;
        float width = MathF.Abs(source.Width);
        float height = MathF.Abs(source.Height);
        if (FlipX) source.Width = -source.Width;
        if (FlipY) source.Height = -source.Height;
        Raylib.DrawTexturePro(_sprite.Texture, source, new Rectangle(0, 0, width, height), Origin, 0, Tint);
    }

    protected sealed override void DisposeResources() => ReleaseSprite();

    private void ReleaseSprite()
    {
        if (_ownsSprite) _sprite?.Dispose();
        _sprite = null;
        _ownsSprite = false;
    }
}
