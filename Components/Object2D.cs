using System.Numerics;
using Fix2Engine.Components.Scene;
using Raylib_cs;

namespace Fix2Engine.Components;

/// <summary>Inheritable scene entity. Override the On* hooks, not the traversal methods.</summary>
public class Object2D : IDisposable
{
    private readonly List<Object2D> _children = new();
    private readonly IReadOnlyList<Object2D> _childrenView;
    private long _attachmentVersion;

    public Guid Guid { get; } = Guid.NewGuid();
    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public bool IsStarted { get; private set; }
    public bool IsDestroyed { get; private set; }
    public int ZIndex { get; set; }
    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public Object2D? Parent { get; private set; }
    public FixScene? Scene { get; private set; }
    public IReadOnlyList<Object2D> Children => _childrenView;
    public bool IsActiveInHierarchy => !IsDestroyed && IsActive && (Parent?.IsActiveInHierarchy ?? true);
    public bool IsVisibleInHierarchy => IsVisible && (Parent?.IsVisibleInHierarchy ?? true);
    public Matrix3x2 LocalTransform => Matrix3x2.CreateScale(Scale) *
        Matrix3x2.CreateRotation(Rotation * MathF.PI / 180f) * Matrix3x2.CreateTranslation(Position);
    public Matrix3x2 GlobalTransform => LocalTransform * (Parent?.GlobalTransform ?? Matrix3x2.Identity);
    public Vector2 GlobalPosition => Vector2.Transform(Vector2.Zero, GlobalTransform);

    public Object2D(string name = "Object2D")
    {
        Name = name;
        _childrenView = _children.AsReadOnly();
    }

    public T AddChild<T>(T child) where T : Object2D
    {
        ThrowIfDestroyed();
        ArgumentNullException.ThrowIfNull(child);
        child.ThrowIfDestroyed();
        for (Object2D? ancestor = this; ancestor != null; ancestor = ancestor.Parent)
            if (ReferenceEquals(ancestor, child)) throw new InvalidOperationException("Object parenting cannot form a cycle.");
        if (child.Parent == this) return child;
        child.Detach();
        child.Parent = this;
        _children.Add(child);
        child.SetScene(Scene);
        return child;
    }

    public bool RemoveChild(Object2D child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child.Parent != this) return false;
        child.Detach();
        return true;
    }

    /// <summary>Detach without destroying. Local transforms are retained when reparenting.</summary>
    public void Detach()
    {
        if (Parent != null) Parent._children.Remove(this);
        else Scene?.RemoveRoot(this);
        Parent = null;
        SetScene(null);
    }

    public Vector2 ToGlobal(Vector2 localPoint) => Vector2.Transform(localPoint, GlobalTransform);

    public Vector2 ToLocal(Vector2 globalPoint)
    {
        if (!Matrix3x2.Invert(GlobalTransform, out var inverse))
            throw new InvalidOperationException("A zero scale has no inverse transform.");
        return Vector2.Transform(globalPoint, inverse);
    }

    public void Start() => Dispatch(Capture([this]), Phase.Start);
    public void Update(float dt) { ValidateDelta(dt); Dispatch(Capture([this]), Phase.Update, dt); }
    public void FixedUpdate(float dt) { ValidateDelta(dt); Dispatch(Capture([this]), Phase.FixedUpdate, dt); }
    public void Render() => Dispatch(Capture([this], ordered: true), Phase.Render);
    public void RenderUI() => Dispatch(Capture([this], ordered: true), Phase.RenderUI);

    protected virtual void OnStart() { }
    protected virtual void OnUpdate(float dt) { }
    protected virtual void OnFixedUpdate(float dt) { }
    /// <summary>Draw in local coordinates; the complete parent transform is applied automatically.</summary>
    protected virtual void OnRender() { }
    /// <summary>Called inside the application's ImGui frame, in screen coordinates.</summary>
    protected virtual void OnRenderUI() { }
    protected virtual void OnDestroy() { }
    protected virtual void DisposeResources() { }
    internal virtual void AdvanceAnimation(float dt) { }

    protected void ThrowIfDestroyed() => ObjectDisposedException.ThrowIf(IsDestroyed, this);

    public void Destroy()
    {
        if (IsDestroyed) return;
        IsDestroyed = true;
        Detach();
        var errors = new List<Exception>();
        foreach (var child in _children.ToArray())
            if (child.Parent == this) Attempt(child.Destroy, errors);
        _children.Clear();
        Attempt(OnDestroy, errors);
        Attempt(DisposeResources, errors);
        GC.SuppressFinalize(this);
        if (errors.Count > 0) throw new AggregateException("Object cleanup failed.", errors);
    }

    public void Dispose() => Destroy();

    internal void SetScene(FixScene? scene)
    {
        Scene = scene;
        _attachmentVersion++;
        foreach (var child in _children) child.SetScene(scene);
    }

    internal enum Phase { Start, Update, FixedUpdate, Render, RenderUI }
    internal readonly record struct Entry(Object2D Object, long Version);

    // Snapshot the whole tree once: callback mutations cannot visit an object twice.
    internal static List<Entry> Capture(IEnumerable<Object2D> roots, bool ordered = false)
    {
        var result = new List<Entry>();
        void Visit(IEnumerable<Object2D> objects)
        {
            foreach (var obj in ordered ? objects.OrderBy(obj => obj.ZIndex) : objects)
            {
                result.Add(new Entry(obj, obj._attachmentVersion));
                Visit(obj._children);
            }
        }
        Visit(roots);
        return result;
    }

    internal static void Dispatch(List<Entry> snapshot, Phase phase, float dt = 0)
    {
        foreach (var entry in snapshot)
        {
            var obj = entry.Object;
            bool CanRun() => entry.Version == obj._attachmentVersion && obj.IsActiveInHierarchy &&
                (phase is not (Phase.Render or Phase.RenderUI) || obj.IsVisibleInHierarchy);
            if (!CanRun()) continue;
            if (!obj.IsStarted)
            {
                obj.IsStarted = true;
                obj.OnStart();
            }
            if (!CanRun()) continue;
            switch (phase)
            {
                case Phase.Update:
                    obj.OnUpdate(dt);
                    if (CanRun()) obj.AdvanceAnimation(dt);
                    break;
                case Phase.FixedUpdate: obj.OnFixedUpdate(dt); break;
                case Phase.Render:
                    // Mirrored transforms reverse triangle winding. Flush before changing
                    // culling so sprites already in Raylib's batch retain their render state.
                    bool mirrored = obj.GlobalTransform.GetDeterminant() < 0;
                    if (mirrored)
                    {
                        Rlgl.DrawRenderBatchActive();
                        Rlgl.DisableBackfaceCulling();
                    }
                    Rlgl.PushMatrix();
                    try
                    {
                        // System.Numerics uses row vectors; Raylib's matrix helper uses column vectors.
                        Rlgl.MultMatrixf(Matrix4x4.Transpose(new Matrix4x4(obj.GlobalTransform)));
                        obj.OnRender();
                    }
                    finally
                    {
                        Rlgl.PopMatrix();
                        if (mirrored)
                        {
                            Rlgl.DrawRenderBatchActive();
                            Rlgl.EnableBackfaceCulling();
                        }
                    }
                    break;
                case Phase.RenderUI: obj.OnRenderUI(); break;
            }
        }
    }

    internal static void ValidateDelta(float dt)
    {
        if (!float.IsFinite(dt) || dt < 0) throw new ArgumentOutOfRangeException(nameof(dt));
    }

    internal static void Attempt(Action action, List<Exception> errors)
    {
        try { action(); }
        catch (Exception exception) { errors.Add(exception); }
    }
}
