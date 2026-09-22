# Scene Management

`FixScene` (`Fix2Engine.Components.Scene`) owns a collection of root `Object2D`
instances. It is a separate container, not an object subclass. `SceneManager`
(`Fix2Engine.Components`) switches between scenes and routes lifecycle calls.

## Define a Scene

```csharp
using System.Numerics;
using Fix2Engine.Components;
using Fix2Engine.Components.Scene;
using Raylib_cs;

public class MyScene : FixScene
{
    protected override void OnStart()
    {
        Add(new SpriteObject2D("Decoration")
        {
            Position = new Vector2(100, 100),
            TexturePath = "assets/decoration.png"
        });
        // Add(new Player()); // your Object2D subclass
    }

    protected override void OnRender()
    {
        Raylib.ClearBackground(Color.Black);
    }
}
```

Scene hooks are `OnStart`, `OnUpdate`, `OnFixedUpdate`, `OnRender`, `OnRenderUI`, and
`OnUnload`. Start runs once. Update/render hooks run before automatic object
traversal; use `OnRender` for the background. `OnUnload` runs after object cleanup.
Calling `Dispose` or `Unload` releases the entire owned hierarchy once, even if
individual cleanup hooks throw; errors are then reported together.

`Add(obj)` returns the typed object. `Objects` is a read-only root collection.
`Remove(obj)` detaches a root without destroying it; use `obj.Destroy()` to remove
and dispose it. Reparenting transfers ownership. See [Objects](Components.md).

## Switch Scenes

```csharp
SceneManager.LoadScene<MyScene>();
// Or transfer ownership of an existing scene:
SceneManager.LoadScene(new MyScene());
```

Switching is deferred until the next `SceneManager.Update(dt)`. The old scene is
disposed before the new scene starts. Requests made during the new scene's
`OnStart` remain pending for the following update. Replaced pending scenes are
disposed too. `SceneManager.Unload()` cleans up both current and pending scenes.

## Connect to Your Application

In your `Windowing` subclass:

```csharp
protected override void Start()
{
    rlImGui.Setup(true);
    SceneManager.LoadScene<MyScene>();
}

protected override void Update(float dt) => SceneManager.Update(dt);
protected override void FixedUpdate(float dt) => SceneManager.FixedUpdate(dt);

protected override void Render()
{
    SceneManager.Render();
    rlImGui.Begin();
    SceneManager.RenderUI();
    rlImGui.End();
}

protected override void OnUnload()
{
    try { SceneManager.Unload(); }
    finally { rlImGui.Shutdown(); }
}
```

`Windowing.OnUnload` runs before the graphics context closes. This is where scene
textures and ImGui must be released. The CLI-generated application wires up these
calls. Objects are updated once by the scene; do not manually traverse them again.

## Manual Scene Implementations

Existing code can still implement `IFixScene` directly:

```csharp
public interface IFixScene : IDisposable
{
    void Start();
    void Update(float dt);
    void FixedUpdate(float dt) { }
    void Render();
    void RenderUI();
    void Unload();
}
```

The default fixed-update implementation is empty for compatibility. Direct
implementations own their own lifecycle and resources. Inherit `FixScene` to get
automatic object ownership, traversal and cleanup.
