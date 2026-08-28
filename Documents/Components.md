# Components

Sources: `Fix2Engine.Components/*`

## PineObject2D

`PineObject2D.cs` — namespace `Fix2Engine.Components` — lightweight 2D scene-graph node.

```csharp
public class PineObject2D
{
    public Guid Guid { get; }
    public string Name { get; set; }
    public bool IsActive { get; set; } = true;

    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Rotation { get; set; } = 0.0f;
    public Vector2 Scale { get; set; } = Vector2.One;
    public Vector2 Origin { get; set; } = Vector2.Zero;
    public Color Tint { get; set; } = Color.White;
    public string TexturePath { get; set; }

    public PineObject2D? Parent { get; }
    public IReadOnlyList<PineObject2D> Children { get; }
    public Vector2 GlobalPosition { get; } // recursive: Parent.GlobalPosition + Position

    public PineObject2D(string name = "PineObject2D");
    public void AddChild(PineObject2D child);
    public void RemoveChild(PineObject2D child);
    public virtual void Update(float deltaTime);
    public virtual void Render();
}
```

Usage:

```csharp
var player = new PineObject2D("Player")
{
    Position = new Vector2(100, 100),
    TexturePath = "assets/player.png"
};

var weapon = new PineObject2D("Weapon")
{
    Position = new Vector2(10, 0),
    TexturePath = "assets/weapon.png"
};

player.AddChild(weapon);

// each frame:
player.Update(dt);
player.Render(); // draws self (if TexturePath set) then children

// world position of child:
Vector2 worldPos = weapon.GlobalPosition; // player.Position + weapon.Position
```

Notes:
- `AddChild` automatically detaches the child from its previous parent.
- `Update` propagates to children only if `IsActive` is true.
- `Render` currently creates a `new Sprite2D(TexturePath)` each call — suitable for prototyping; for production, cache the `Sprite2D` or `Texture2D` yourself.

---

## Character3D

`Character3D.cs` — namespace `Fix2Engine.Components` — **stub / prototype**.

```csharp
public class Character3D
{
    public Guid _guid { get; }
    public string Name { get; set; }

    public Character3D();
}
```

Currently only holds an identity (`Guid` + `Name`). No transform, physics, or rendering. Intended as a base for a future 3D entity. For now, use `Camera` + `Model3D` or Raylib primitives directly for 3D objects.

---

## Adding Your Own Component

Follow the `PineObject2D` pattern:

```csharp
public class HealthComponent : PineObject2D
{
    public float MaxHealth { get; set; } = 100.0f;
    public float CurrentHealth { get; private set; }

    public HealthComponent(string name) : base(name)
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(float amount)
    {
        CurrentHealth = Math.Max(0, CurrentHealth - amount);
        if (CurrentHealth == 0) IsActive = false;
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);
        // custom logic
    }
}
```
