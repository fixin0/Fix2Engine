using System;
using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Fix2Engine.Graphics;

public class Sprite2D : IDisposable
{
    public Texture2D Texture { get; private set; }
    
    public Vector2 Position { get; set; } = Vector2.Zero;
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Origin { get; set; } = Vector2.Zero; 
    public float Rotation { get; set; } = 0.0f;         
    public Color Tint { get; set; } = Color.White;


    public Rectangle SourceRect { get; set; }

    private bool _isDisposed;


    public Sprite2D(string filePath)
    {
        Texture = LoadTexture(filePath);
        SourceRect = new Rectangle(0, 0, Texture.Width, Texture.Height);
    }
    
    public Sprite2D(Texture2D texture)
    {
        Texture = texture;
        SourceRect = new Rectangle(0, 0, Texture.Width, Texture.Height);
    }

    /// <summary>
    /// Sets the pivot to the center of the texture.
    /// </summary>
    public void CenterOrigin()
    {
        Origin = new Vector2(SourceRect.Width / 2f, SourceRect.Height / 2f);
    }

    /// <summary>
    /// Draws the sprite with current parameters.
    /// </summary>
    public void Draw()
    {
        if (_isDisposed || Texture.Id == 0) return;

        // Calculate destination area and size for rendering
        Rectangle destRect = new Rectangle(
            Position.X,
            Position.Y,
            SourceRect.Width * Scale.X,
            SourceRect.Height * Scale.Y
        );

        
        DrawTexturePro(Texture, SourceRect, destRect, Origin, Rotation, Tint);
    }
    

  
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (Texture.Id != 0)
            {
                UnloadTexture(Texture);
            }
            _isDisposed = true;
        }
    }

    ~Sprite2D()
    {
        Dispose(false);
    }
}