namespace Fix2Engine.Components;

/// <summary>Legacy name for a sprite-capable object. New code can use Object2D or SpriteObject2D.</summary>
public class Node2D : SpriteObject2D
{
    public Node2D(string name = "Node2D") : base(name) { }
}
