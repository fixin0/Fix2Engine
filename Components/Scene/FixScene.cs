using Fix2Engine.Graphics;

namespace Fix2Engine.Components.Scene;

/// <summary>A scene owns objects but is not itself an Object2D.</summary>
public abstract class FixScene : IFixScene
{
    private readonly List<Object2D> _objects = new();
    private readonly IReadOnlyList<Object2D> _objectsView;
    public IReadOnlyList<Object2D> Objects => _objectsView;
    public bool IsStarted { get; private set; }
    public bool IsDisposed { get; private set; }

    protected FixScene() => _objectsView = _objects.AsReadOnly();

    public T Add<T>(T obj) where T : Object2D
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentNullException.ThrowIfNull(obj);
        ObjectDisposedException.ThrowIf(obj.IsDestroyed, obj);
        if (obj.Scene == this && obj.Parent == null) return obj;
        obj.Detach();
        _objects.Add(obj);
        obj.SetScene(this);
        return obj;
    }

    public bool Remove(Object2D obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        if (obj.Scene != this || obj.Parent != null) return false;
        obj.Detach();
        return true;
    }

    internal void RemoveRoot(Object2D obj) => _objects.Remove(obj);

    public void Start()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (IsStarted) return;
        IsStarted = true;
        OnStart();
        if (!IsDisposed) Object2D.Dispatch(Object2D.Capture(_objects), Object2D.Phase.Start);
    }

    public void Update(float dt)
    {
        Object2D.ValidateDelta(dt);
        Run(Object2D.Phase.Update, () => OnUpdate(dt), dt);
    }

    public void FixedUpdate(float dt)
    {
        Object2D.ValidateDelta(dt);
        Run(Object2D.Phase.FixedUpdate, () => OnFixedUpdate(dt), dt);
    }

    public void Render() => Run(Object2D.Phase.Render, () => OnRender(RenderContext.Current));
    public void RenderUI() => Run(Object2D.Phase.RenderUI, OnRenderUI);

    private void Run(Object2D.Phase phase, Action hook, float dt = 0)
    {
        if (IsDisposed) return;
        Start();
        if (IsDisposed) return;
        var snapshot = Object2D.Capture(_objects, phase is Object2D.Phase.Render or Object2D.Phase.RenderUI);
        hook();
        if (!IsDisposed) Object2D.Dispatch(snapshot, phase, dt);
    }

    protected virtual void OnStart() { }
    protected virtual void OnUpdate(float dt) { }
    protected virtual void OnFixedUpdate(float dt) { }
    protected virtual void OnRender() { }
    protected virtual void OnRender(RenderContext graphics) => OnRender();
    protected virtual void OnRenderUI() { }
    protected virtual void OnUnload() { }

    public void Unload() => Dispose();

    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        var errors = new List<Exception>();
        foreach (var obj in _objects.ToArray())
            if (obj.Scene == this && obj.Parent == null) Object2D.Attempt(obj.Destroy, errors);
        _objects.Clear();
        Object2D.Attempt(OnUnload, errors);
        GC.SuppressFinalize(this);
        if (errors.Count > 0) throw new AggregateException("Scene cleanup failed.", errors);
    }
}
