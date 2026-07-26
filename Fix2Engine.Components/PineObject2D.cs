using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Fix2Engine.Graphics;
// Kullandığın grafik kütüphanesine göre (Raylib-cs, MonoGame vs.) buradaki türleri ayarlayabilirsin:
 using Raylib_cs;
 using Rectangle = Raylib_cs.Rectangle;

 namespace Fix2Engine.Components;

public class PineObject2D
{
    public Guid Guid { get; private set; }
    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
    
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0f;
    public Vector2 Scale { get; set; } = Vector2.One;
    
    public string TexturePath { get; set; } 
    public Vector2 Origin { get; set; } = Vector2.Zero; 
    
    public Raylib_cs.Color Tint {get; set;} = Raylib_cs.Color.White;
    
    public PineObject2D? Parent { get; private set; }
    private readonly List<PineObject2D> _children = new();
    public IReadOnlyList<PineObject2D> Children => _children.AsReadOnly();

    public Vector2 GlobalPosition => Parent == null ? Position : Parent.GlobalPosition + Position;

    public PineObject2D(string name = "PineObject2D")
    {
        Guid = Guid.NewGuid();
        Name = name;
    }

    public void AddChild(PineObject2D child)
    {
        if (child == null || child == this || _children.Contains(child)) return;
        child.Parent?._children.Remove(child);
        child.Parent = this;
        _children.Add(child);
    }

    public void RemoveChild(PineObject2D child)
    {
        if (child == null || !_children.Contains(child)) return;
        child.Parent = null;
        _children.Remove(child);
    }

    public virtual void Update(float deltaTime)
    {
        if (!IsActive) return;

        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].Update(deltaTime);
        }
    }

    public virtual void Render()
    {
        if (!IsActive) return;

        // Texture varsa burada ekrana çizdirme mantığını çağırırsın:
        if (TexturePath != null)
        {
            // Örn Raylib için:
            // Raylib.DrawTextureEx((Texture2D)Texture, GlobalPosition, Rotation, Scale.X, Color.WHITE)
            Sprite2D _renderTextureForSprite = new Sprite2D(TexturePath);
            _renderTextureForSprite.Scale = Scale;
            _renderTextureForSprite.Rotation = Rotation;
            _renderTextureForSprite.Origin = Origin;
            _renderTextureForSprite.Tint = Tint;
            _renderTextureForSprite.SourceRect = new Rectangle();

        }
        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].Render();
        }
    }
}