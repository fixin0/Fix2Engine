namespace Fix2Engine.Components.Scene;

/// <summary>Owns the active 3D scene independently of the legacy 2D scene manager.</summary>
public static class Scene3DManager
{
    public static Scene3D? CurrentScene { get; private set; }
    private static Scene3D? _nextScene;
    public static void Load(Scene3D scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ObjectDisposedException.ThrowIf(scene.IsDisposed, scene);
        if (ReferenceEquals(scene, CurrentScene) || ReferenceEquals(scene, _nextScene)) return;
        _nextScene?.Dispose();
        _nextScene = scene;
    }
    public static void Update(float dt)
    {
        if (_nextScene != null)
        {
            var previous = CurrentScene;
            CurrentScene = _nextScene;
            _nextScene = null;
            previous?.Dispose();
        }
        CurrentScene?.Update(dt);
    }
    public static void FixedUpdate(float dt) => CurrentScene?.FixedUpdate(dt);
    public static void Render() => CurrentScene?.Render();
    public static void Unload()
    {
        var current = CurrentScene;
        var pending = _nextScene;
        CurrentScene = _nextScene = null;
        current?.Dispose();
        if (!ReferenceEquals(current, pending)) pending?.Dispose();
    }
}
