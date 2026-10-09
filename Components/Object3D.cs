using System.Numerics;
using Fix2Engine.Components.Scene;

namespace Fix2Engine.Components;

/// <summary>A transformable 3D scene node with independently attachable render and behaviour components.</summary>
public class Object3D : IDisposable
{
    private readonly List<Object3D> _children = new();
    private readonly List<Component3D> _components = new();
    private readonly IReadOnlyList<Object3D> _childrenView;
    private readonly IReadOnlyList<Component3D> _componentsView;

    public string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public bool IsStarted { get; private set; }
    public bool IsDestroyed { get; private set; }
    public Vector3 Position { get; set; }
    /// <summary>Euler rotation in degrees: X pitch, Y yaw, Z roll.</summary>
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; } = Vector3.One;
    public Object3D? Parent { get; private set; }
    public Scene3D? Scene { get; private set; }
    public IReadOnlyList<Object3D> Children => _childrenView;
    public IReadOnlyList<Component3D> Components => _componentsView;
    public bool IsActiveInHierarchy => !IsDestroyed && IsActive && (Parent?.IsActiveInHierarchy ?? true);
    public bool IsVisibleInHierarchy => IsVisible && (Parent?.IsVisibleInHierarchy ?? true);
    public Matrix4x4 LocalTransform => Matrix4x4.CreateScale(Scale) *
        Matrix4x4.CreateFromYawPitchRoll(ToRadians(Rotation.Y), ToRadians(Rotation.X), ToRadians(Rotation.Z)) *
        Matrix4x4.CreateTranslation(Position);
    public Matrix4x4 GlobalTransform => LocalTransform * (Parent?.GlobalTransform ?? Matrix4x4.Identity);
    public Vector3 GlobalPosition => Vector3.Transform(Vector3.Zero, GlobalTransform);

    public Object3D(string name = "Object3D")
    {
        Name = name;
        _childrenView = _children.AsReadOnly();
        _componentsView = _components.AsReadOnly();
    }

    public T AddChild<T>(T child) where T : Object3D
    {
        ThrowIfDestroyed();
        ArgumentNullException.ThrowIfNull(child);
        child.ThrowIfDestroyed();
        for (Object3D? node = this; node != null; node = node.Parent)
            if (ReferenceEquals(node, child)) throw new InvalidOperationException("Object parenting cannot form a cycle.");
        if (child.Parent == this) return child;
        child.Detach();
        child.Parent = this;
        _children.Add(child);
        child.SetScene(Scene);
        return child;
    }

    public bool RemoveChild(Object3D child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (child.Parent != this) return false;
        child.Detach();
        return true;
    }

    public T AddComponent<T>(T component) where T : Component3D
    {
        ThrowIfDestroyed();
        ArgumentNullException.ThrowIfNull(component);
        component.Attach(this);
        _components.Add(component);
        if (IsStarted) component.Start();
        return component;
    }

    public void Detach()
    {
        if (Parent != null) Parent._children.Remove(this);
        else Scene?.RemoveRoot(this);
        Parent = null;
        SetScene(null);
    }

    public void Destroy()
    {
        if (IsDestroyed) return;
        IsDestroyed = true;
        Detach();
        var errors = new List<Exception>();
        foreach (var child in _children.ToArray()) Attempt(child.Destroy, errors);
        _children.Clear();
        foreach (var component in _components.ToArray()) Attempt(component.Dispose, errors);
        _components.Clear();
        Attempt(OnDestroy, errors);
        GC.SuppressFinalize(this);
        if (errors.Count > 0) throw new AggregateException("3D object cleanup failed.", errors);
    }

    public void Dispose() => Destroy();

    internal void Start()
    {
        if (!IsActiveInHierarchy) return;
        if (!IsStarted)
        {
            IsStarted = true;
            OnStart();
            foreach (var component in _components.ToArray()) component.Start();
        }
        foreach (var child in _children.ToArray()) child.Start();
    }
    internal void Update(float dt)
    {
        if (!IsActiveInHierarchy) return;
        Start();
        OnUpdate(dt);
        foreach (var component in _components.ToArray()) component.Update(dt);
        foreach (var child in _children.ToArray()) child.Update(dt);
    }
    internal void FixedUpdate(float dt)
    {
        if (!IsActiveInHierarchy) return;
        Start();
        OnFixedUpdate(dt);
        foreach (var component in _components.ToArray()) component.FixedUpdate(dt);
        foreach (var child in _children.ToArray()) child.FixedUpdate(dt);
    }
    internal void Render()
    {
        if (!IsActiveInHierarchy || !IsVisibleInHierarchy) return;
        Start();
        OnRender3D();
        foreach (var component in _components.ToArray()) component.Render();
        foreach (var child in _children.ToArray()) child.Render();
    }

    internal void SetScene(Scene3D? scene)
    {
        Scene = scene;
        foreach (var child in _children) child.SetScene(scene);
    }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate(float dt) { }
    protected virtual void OnFixedUpdate(float dt) { }
    protected virtual void OnRender3D() { }
    protected virtual void OnDestroy() { }
    protected void ThrowIfDestroyed() => ObjectDisposedException.ThrowIf(IsDestroyed, this);
    private static float ToRadians(float degrees) => degrees * MathF.PI / 180f;
    private static void Attempt(Action action, ICollection<Exception> errors)
    {
        try { action(); }
        catch (Exception exception) { errors.Add(exception); }
    }
}
