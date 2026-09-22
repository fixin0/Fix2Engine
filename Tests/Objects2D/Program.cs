using System.Numerics;
using Fix2Engine.Components;
using Fix2Engine.Components.Animation;
using Fix2Engine.Components.Scene;
using Fix2Engine.Graphics;
using Raylib_cs;

int assertions = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    assertions++;
}
void Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { assertions++; return; }
    throw new Exception($"Expected {typeof(T).Name}");
}
bool Near(Vector2 a, Vector2 b) => Vector2.Distance(a, b) < 0.001f;

using (var scene = new ProbeScene())
{
    var parent = scene.Add(new ProbeObject());
    var child = parent.AddChild(new ProbeObject());
    scene.Update(0.1f);
    scene.Update(0.1f);
    scene.FixedUpdate(0.02f);
    Check(parent.Starts == 1 && child.Starts == 1 && child.Updates == 2 && child.FixedUpdates == 1,
        "Scene must automatically traverse object lifecycle exactly once.");
    Check(child.Scene == scene && child.Parent == parent && scene.Objects.Count == 1, "Ownership must propagate through children.");
    parent.IsActive = false;
    scene.Update(0.1f);
    scene.FixedUpdate(0.02f);
    Check(child.Updates == 2 && child.FixedUpdates == 1, "Inactive parent must suppress descendants.");
    parent.IsActive = true;
    parent.IsVisible = false;
    scene.Update(0.1f);
    scene.RenderUI();
    Check(child.Updates == 3 && child.UiDraws == 0, "Hidden objects still update but do not render UI.");
    parent.IsVisible = true;
    scene.RenderUI();
    Check(child.UiDraws == 1, "Visibility should recover.");
    parent.Destroy();
    parent.Destroy();
    Check(parent.Destroys == 1 && child.Destroys == 1 && scene.Objects.Count == 0,
        "Destroy must recursively release once and detach from the scene.");
    Throws<ObjectDisposedException>(() => scene.Add(parent));
}

using (var root = new Object2D { Position = new(100, 50), Rotation = 90, Scale = new(2, 3) })
{
    var child = root.AddChild(new Object2D { Position = new(10, 5), Rotation = 25, Scale = new(1, 2) });
    Check(Near(child.GlobalPosition, new(85, 70)), "Parent rotation and non-uniform scale must transform child position.");
    Check(Near(child.ToLocal(child.ToGlobal(new(4, 7))), new(4, 7)), "Global and local conversion must preserve full affine transforms.");
    Throws<InvalidOperationException>(() => child.AddChild(root));
    Throws<InvalidOperationException>(() => root.AddChild(root));
    child.Scale = Vector2.Zero;
    Throws<InvalidOperationException>(() => child.ToLocal(Vector2.One));
}

using (var first = new ProbeScene())
using (var second = new ProbeScene())
{
    var parent = first.Add(new Object2D());
    var child = parent.AddChild(new ProbeObject());
    second.Add(child);
    first.Dispose();
    Check(!child.IsDestroyed && child.Parent == null && child.Scene == second && parent.Children.Count == 0,
        "Reparenting across scenes must transfer ownership without destroying the object.");
    second.Remove(child);
    second.Dispose();
    Check(!child.IsDestroyed && child.Scene == null, "Remove detaches without destroying.");
    child.Dispose();
}

using (var scene = new ProbeScene())
{
    var first = scene.Add(new ProbeObject());
    var removed = scene.Add(new ProbeObject());
    var last = scene.Add(new ProbeObject());
    var added = new ProbeObject();
    first.UpdateAction = () => { removed.Destroy(); scene.Add(added); first.UpdateAction = null; };
    scene.Update(0.1f);
    Check(removed.Updates == 0 && last.Updates == 1 && added.Updates == 0,
        "Removal must not skip siblings; additions must wait until the next traversal.");
    scene.Update(0.1f);
    Check(added.Starts == 1 && added.Updates == 1, "Dynamically added objects must start and update automatically.");
    first.UpdateAction = () => { first.AddChild(last); first.UpdateAction = null; };
    int previousUpdates = last.Updates;
    scene.Update(0.1f);
    Check(last.Updates == previousUpdates, "Reparenting during traversal must defer the moved subtree.");
    scene.Update(0.1f);
    Check(last.Updates == previousUpdates + 1, "Reparented objects must resume on the next traversal.");
}

using (var scene = new ProbeScene())
{
    var dying = scene.Add(new ProbeObject());
    var child = dying.AddChild(new ProbeObject());
    dying.UpdateAction = dying.Destroy;
    scene.Update(0.1f);
    Check(dying.Destroys == 1 && child.Updates == 0 && child.Destroys == 1, "Self-destruction must stop the subtree immediately.");
}

var order = new List<string>();
using (var scene = new ProbeScene())
{
    scene.Add(new ProbeObject { Name = "front", ZIndex = 3, UiAction = () => order.Add("front") });
    scene.Add(new ProbeObject { Name = "back", ZIndex = -1, UiAction = () => order.Add("back") });
    scene.Add(new ProbeObject { Name = "same", ZIndex = 3, UiAction = () => order.Add("same") });
    scene.RenderUI();
    Check(order.SequenceEqual(["back", "front", "same"]), "Sibling drawing order must use ZIndex and stable insertion order.");
}

var failingScene = new ProbeScene();
var bad = failingScene.Add(new ProbeObject { DestroyAction = () => throw new InvalidOperationException("expected") });
var good = failingScene.Add(new ProbeObject());
Throws<AggregateException>(failingScene.Dispose);
Check(good.IsDestroyed && bad.IsDestroyed && failingScene.Unloads == 1, "A failing destructor must not leak the remaining scene objects.");

using (var source = new ProbeScene())
using (var destination = new ProbeScene())
{
    var trigger = source.Add(new ProbeObject());
    var transferred = source.Add(new ProbeObject());
    trigger.DestroyAction = () => destination.Add(transferred);
    source.Dispose();
    Check(!transferred.IsDestroyed && transferred.Scene == destination,
        "Cleanup callbacks that transfer ownership must not destroy the transferred object.");
}

var frames = new[] { new Rectangle(0, 0, 16, 16), new Rectangle(16, 0, 16, 16) };
var immutable = new SpriteAnimation("copy", frames);
frames[0] = new Rectangle(999, 999, 1, 1);
Check(immutable.Frames[0].X == 0, "Clip frame storage must be a defensive copy.");
var grid = SpriteAnimation.FromGrid("grid", 16, 24, 3, 4, startFrame: 3);
Check(grid.Frames[0].X == 48 && grid.Frames[1].X == 0 && grid.Frames[1].Y == 24,
    "Grid clips must wrap rows and respect the starting frame.");
Throws<ArgumentException>(() => new SpriteAnimation("empty", []));
Throws<ArgumentException>(() => new SpriteAnimation("bad", [new Rectangle(0, 0, 0, 1)]));
Throws<ArgumentOutOfRangeException>(() => new SpriteAnimation("fps", immutable.Frames, 0));
Throws<ArgumentOutOfRangeException>(() => SpriteAnimation.FromGrid("bad", 16, 16, 2, 0));

var player = new AnimationPlayer();
player.Add(SpriteAnimation.FromGrid("run", 16, 16, 4, 4, 8));
player.Play("run");
player.Update(0.25f);
Check(player.FrameIndex == 2, "Animation must advance by delta time.");
player.Play("run");
Check(player.FrameIndex == 2, "Repeated Play must not reset the current animation.");
player.Pause(); player.Update(1);
Check(player.FrameIndex == 2 && !player.IsPlaying, "Pause must hold the current frame.");
player.Resume(); player.Update(0.25f);
Check(player.FrameIndex == 0 && player.IsPlaying, "Loop must wrap to the first frame.");
player.Speed = 2; player.Update(0.125f);
Check(player.FrameIndex == 2, "Speed must multiply playback rate.");
player.Speed = 0; player.Update(4);
Check(player.FrameIndex == 2, "Zero speed must freeze playback.");
player.Speed = 1; player.Play("run", restart: true); player.Update(1000000.25f);
Check(player.FrameIndex == 2, "Large deltas must wrap efficiently without dropping remainder.");
player.Stop();
Check(player.FrameIndex == 0 && !player.IsPlaying, "Stop must rewind.");
Throws<KeyNotFoundException>(() => player.Play("missing"));
Throws<ArgumentException>(() => player.Add(SpriteAnimation.FromGrid("run", 1, 1, 1, 1)));
Throws<ArgumentOutOfRangeException>(() => player.Speed = -1);
Throws<ArgumentOutOfRangeException>(() => player.Update(float.NaN));

player.Add(SpriteAnimation.FromGrid("once", 16, 16, 3, 3, 10, loop: false));
int finished = 0;
player.Finished += clip => { if (clip.Name == "once") finished++; };
player.Play("once"); player.Update(0.21f);
Check(player.FrameIndex == 2 && player.IsPlaying, "Last frame must be held for its full duration.");
player.Update(0.1f); player.Update(20);
Check(player.FrameIndex == 2 && !player.IsPlaying && finished == 1, "One-shot must finish exactly once and retain its last frame.");
player.Play("once");
Check(player.FrameIndex == 0 && player.IsPlaying, "Play after completion must restart.");

using (var scene = new ProbeScene())
{
    var animated = scene.Add(new CustomAnimatedObject());
    animated.Animator.Add(SpriteAnimation.FromGrid("run", 16, 16, 4, 4, 8));
    animated.Animator.Play("run");
    scene.Update(0.25f);
    Check(animated.UserUpdates == 1 && animated.Animator.FrameIndex == 2,
        "Subclass OnUpdate must not disable automatic animation when it omits a base call.");
    animated.IsActive = false;
    scene.Update(0.25f);
    Check(animated.Animator.FrameIndex == 2, "Inactive animated objects must pause advancement.");
    animated.IsActive = true; animated.IsVisible = false;
    scene.Update(0.25f);
    Check(animated.Animator.FrameIndex == 0, "Hidden animated objects must continue updating.");
}

var borrowed = new Sprite2D(new Texture2D { Id = 123, Width = 32, Height = 16 });
var holder = new SpriteObject2D(); holder.SetSprite(borrowed); holder.Destroy();
Check(!borrowed.IsDisposed, "Borrowed sprite wrappers must not be disposed by objects.");
var owner = new SpriteObject2D(); owner.SetSprite(borrowed, ownsSprite: true); owner.Destroy();
Check(borrowed.IsDisposed && !borrowed.OwnsTexture, "Explicit wrapper ownership must clean up without unloading borrowed texture handles.");
using (var clearPath = new SpriteObject2D { TexturePath = "not-loaded.png" })
{
    clearPath.SetSprite(null);
    Check(clearPath.TexturePath == null && clearPath.Sprite == null,
        "Clearing a sprite must also cancel a pending lazy texture load.");
}
Throws<ObjectDisposedException>(() => new SpriteObject2D().SetSprite(borrowed));
Check(new Node2D() is Object2D, "Node2D must use the common object hierarchy.");

SceneManager.Unload();
var superseded = new ProbeScene();
var currentScene = new ProbeScene();
var currentObject = currentScene.Add(new ProbeObject());
SceneManager.LoadScene(superseded);
SceneManager.LoadScene(currentScene);
Check(superseded.IsDisposed, "Superseded pending scenes must be disposed.");
SceneManager.Update(0.1f); SceneManager.FixedUpdate(0.02f);
Check(currentObject.Updates == 1 && currentObject.FixedUpdates == 1, "SceneManager must dispatch both update phases.");
var afterStart = new ProbeScene();
var redirect = new ProbeScene { StartAction = () => SceneManager.LoadScene(afterStart) };
SceneManager.LoadScene(redirect); SceneManager.Update(0.1f);
Check(currentScene.IsDisposed && currentObject.IsDestroyed, "Scene switches must release objects.");
Check(SceneManager.CurrentScene == redirect, "A scene requested in Start must wait for the next update.");
SceneManager.Update(0.1f);
Check(SceneManager.CurrentScene == afterStart && redirect.IsDisposed, "Start-time requests must not be lost.");
var pending = new ProbeScene(); SceneManager.LoadScene(pending); SceneManager.Unload();
Check(afterStart.IsDisposed && pending.IsDisposed && SceneManager.CurrentScene == null,
    "Shutdown must dispose current and pending scenes.");
Throws<ArgumentNullException>(() => SceneManager.LoadScene(null!));
Throws<ObjectDisposedException>(() => SceneManager.LoadScene(pending));

if (args.Contains("--graphics")) GraphicsChecks.Run(Check);
Console.WriteLine($"PASS: {assertions} checks (object ownership, lifecycle, transforms, animation, scene switching" +
    (args.Contains("--graphics") ? ", native rendering and window cleanup)." : ")."));

class ProbeObject : Object2D
{
    public int Starts, Updates, FixedUpdates, UiDraws, Destroys;
    public Action? UpdateAction, DestroyAction, UiAction;
    protected override void OnStart() => Starts++;
    protected override void OnUpdate(float dt) { Updates++; UpdateAction?.Invoke(); }
    protected override void OnFixedUpdate(float dt) => FixedUpdates++;
    protected override void OnRenderUI() { UiDraws++; UiAction?.Invoke(); }
    protected override void OnDestroy() { Destroys++; DestroyAction?.Invoke(); }
}
class ProbeScene : FixScene
{
    public int Unloads;
    public Action? StartAction;
    protected override void OnStart() => StartAction?.Invoke();
    protected override void OnUnload() => Unloads++;
}
class CustomAnimatedObject : AnimatedSprite2D
{
    public int UserUpdates;
    protected override void OnUpdate(float dt) => UserUpdates++;
}
