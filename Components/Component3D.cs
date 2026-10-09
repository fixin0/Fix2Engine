namespace Fix2Engine.Components;

/// <summary>Attachable behaviour for an <see cref="Object3D"/>.</summary>
public abstract class Component3D : IDisposable
{
    public Object3D? Owner { get; private set; }
    public bool IsStarted { get; private set; }
    public bool IsDestroyed { get; private set; }

    internal void Attach(Object3D owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (Owner != null) throw new InvalidOperationException("A 3D component is already attached.");
        if (IsDestroyed) throw new ObjectDisposedException(GetType().Name);
        Owner = owner;
    }
    internal void Start()
    {
        if (IsStarted || IsDestroyed) return;
        IsStarted = true;
        OnStart();
    }
    internal void Update(float dt) { if (!IsDestroyed) OnUpdate(dt); }
    internal void FixedUpdate(float dt) { if (!IsDestroyed) OnFixedUpdate(dt); }
    internal void Render() { if (!IsDestroyed) OnRender3D(); }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate(float dt) { }
    protected virtual void OnFixedUpdate(float dt) { }
    protected virtual void OnRender3D() { }
    protected virtual void DisposeResources() { }

    public void Dispose()
    {
        if (IsDestroyed) return;
        IsDestroyed = true;
        DisposeResources();
        Owner = null;
        GC.SuppressFinalize(this);
    }
}
