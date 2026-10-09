using Fix2Engine.Graphics;

namespace Fix2Engine.Components.Scene;

/// <summary>Independent 3D world. It renders before the established 2D scene and UI layers.</summary>
public sealed class Scene3D : IDisposable
{
    private readonly List<Object3D> _objects = new();
    private readonly IReadOnlyList<Object3D> _objectsView;
    public Camera3D Camera { get; } = new();
    public IReadOnlyList<Object3D> Objects => _objectsView;
    public bool IsDisposed { get; private set; }

    public Scene3D() => _objectsView = _objects.AsReadOnly();
    public T Add<T>(T obj) where T : Object3D
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentNullException.ThrowIfNull(obj);
        if (obj.Scene == this && obj.Parent == null) return obj;
        obj.Detach();
        _objects.Add(obj);
        obj.SetScene(this);
        return obj;
    }
    public bool Remove(Object3D obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        if (obj.Scene != this || obj.Parent != null) return false;
        obj.Detach();
        return true;
    }
    internal void RemoveRoot(Object3D obj) => _objects.Remove(obj);
    internal void Update(float dt) { if (!IsDisposed) foreach (var obj in _objects.ToArray()) obj.Update(dt); }
    internal void FixedUpdate(float dt) { if (!IsDisposed) foreach (var obj in _objects.ToArray()) obj.FixedUpdate(dt); }
    internal void Render()
    {
        if (IsDisposed) return;
        using var scope = Graphics3D.Begin(Camera);
        foreach (var obj in _objects.ToArray()) obj.Render();
    }
    public void Dispose()
    {
        if (IsDisposed) return;
        IsDisposed = true;
        var errors = new List<Exception>();
        foreach (var obj in _objects.ToArray())
        {
            try { obj.Destroy(); }
            catch (Exception exception) { errors.Add(exception); }
        }
        _objects.Clear();
        GC.SuppressFinalize(this);
        if (errors.Count > 0) throw new AggregateException("3D scene cleanup failed.", errors);
    }
}
