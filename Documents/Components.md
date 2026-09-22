# 2D Objects

Namespace: `Fix2Engine.Components`

Every scene entity derives from `Object2D`. Characters, props, triggers, HUD elements
and custom behavior can share the same hierarchy and lifecycle. `FixScene` is a
separate container that owns these objects. Engine services and resource data, such
as input, textures and animation clips, are not scene entities.

## Class Hierarchy

```text
Object2D
└── SpriteObject2D
    ├── AnimatedSprite2D
    └── Node2D (legacy name)

FixScene (separate scene container)
```

## Create Your Own Object

```csharp
using Fix2Engine.Components;
using Fix2Engine.Input;
using System.Numerics;

public class Player : Object2D
{
    public float Speed { get; set; } = 180;

    public Player() : base("Player") { }

    protected override void OnUpdate(float dt)
    {
        float direction = (InputManager.IsDown("MoveRight") ? 1 : 0)
                        - (InputManager.IsDown("MoveLeft") ? 1 : 0);
        Position += new Vector2(direction * Speed * dt, 0);
    }
}
```

Add it from a `FixScene.OnStart` override:

```csharp
var player = Add(new Player { Position = new Vector2(100, 100) });
player.AddChild(new SpriteObject2D("Visual")
{
    TexturePath = "assets/player.png",
    Origin = new Vector2(16, 16)
});
```

The scene starts, updates, draws and disposes the hierarchy automatically. Do not
manually call a child's `Update` or `Render` from its parent's hooks.

## Lifecycle Hooks

| Override | Behavior |
| --- | --- |
| `OnStart()` | Once, before the first active lifecycle call |
| `OnUpdate(float dt)` | Frame update, then automatic animation advancement |
| `OnFixedUpdate(float dt)` | Fixed update, routed by `SceneManager.FixedUpdate` |
| `OnRender()` | Drawing in local coordinates, with the entire parent transform applied |
| `OnRenderUI()` | UI drawing in screen coordinates, inside the application's ImGui frame |
| `OnDestroy()` | Once, after detachment and child destruction |

The public traversal methods are non-virtual. Override the hooks to customize
behavior without having to call `base` to keep children or animation running.
A sprite subclass that replaces `OnRender` should call `base.OnRender()` when it
also wants the built-in sprite drawing.

## Object Properties and Hierarchy

- `Guid`, `Name`: identity and a readable name.
- `Position`, `Rotation` (degrees), `Scale`: local transform.
- `GlobalTransform`, `GlobalPosition`, `ToGlobal`, `ToLocal`: complete affine parent
  transforms, including rotation and non-uniform scaling. A zero scale cannot be inverted.
- `IsActive`: suspends updates, animation and rendering for the entire subtree.
- `IsVisible`: hides drawing and UI for the subtree while updates continue.
- `ZIndex`: sibling draw order; lower values render first. Equal values preserve
  insertion order. Each subtree is drawn as a group, parent before children.
- `Parent`, `Children`, `Scene`: current ownership. Collections are read-only views.
- `IsStarted`, `IsDestroyed`: lifecycle state.

`AddChild(child)` reparents an existing object and retains its local transform.
Self-parenting and ancestor cycles are rejected. `RemoveChild`, `Detach`, and
`FixScene.Remove` detach without destroying; the caller then owns the object.
`FixScene.Add` moves an object into that scene as a root.

`Destroy` / `Dispose` detach the object and recursively dispose owned children once.
Destroyed objects cannot be attached again. Traversals use a snapshot: objects
added or reparented during a callback join the next traversal, and destroyed
objects stop receiving callbacks immediately. Keep object and graphics operations
on the application thread.

## Sprites and Animation

`SpriteObject2D` adds `TexturePath`, optional `SourceRect`, `Origin`, `Tint`, `FlipX`
and `FlipY`. A texture path loads once on first render and is released on replacement
or object destruction. `Origin` is the pivot in source-frame pixels.

Use `SetSprite(resource, ownsSprite: false)` to share an existing low-level
`Graphics.Sprite2D` resource. Pass `ownsSprite: true` to transfer ownership of the
wrapper to the object. Borrowed resources must outlive all their users.

`AnimatedSprite2D` adds an `Animator` and selects its current sprite-sheet rectangle
for drawing. It is also inheritable. See [2D Animation](Animation.md).

## Migrating from Node2D

`Node2D` is retained as a subclass of `SpriteObject2D`. Existing position, sprite
and child setup can use the new hierarchy. Move custom overrides from `Update` /
`Render` to `OnUpdate` / `OnRender`; traversal now belongs to the engine. Parent and
child references use `Object2D`. `GlobalPosition` now includes parent rotation and
scale, and file-backed sprites are cached, drawn and released automatically.
