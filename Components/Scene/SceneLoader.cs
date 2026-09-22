using Fix2Engine.Components.Scene;

namespace Fix2Engine.Components;

public static class SceneManager
{
    public static IFixScene? CurrentScene { get; private set; }
    private static IFixScene? _nextScene;
    private static bool _unloading;

    public static void LoadScene<T>() where T : IFixScene, new() => LoadScene(new T());

    /// <summary>Transfers ownership; superseded pending scenes are disposed.</summary>
    public static void LoadScene(IFixScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        if (_unloading) throw new InvalidOperationException("Cannot load a scene during shutdown.");
        if (scene is FixScene { IsDisposed: true }) throw new ObjectDisposedException(nameof(scene));
        if (ReferenceEquals(scene, CurrentScene) || ReferenceEquals(scene, _nextScene)) return;
        var pending = _nextScene;
        _nextScene = scene;
        pending?.Dispose();
    }

    public static void Update(float dt)
    {
        Object2D.ValidateDelta(dt);
        if (_nextScene != null)
        {
            var next = _nextScene;
            _nextScene = null; // Requests made during Start are retained for the following update.
            var previous = CurrentScene;
            CurrentScene = null;
            try { previous?.Dispose(); }
            catch
            {
                next.Dispose();
                throw;
            }
            CurrentScene = next;
            try { next.Start(); }
            catch
            {
                CurrentScene = null;
                next.Dispose();
                throw;
            }
        }
        CurrentScene?.Update(dt);
    }

    public static void FixedUpdate(float dt)
    {
        Object2D.ValidateDelta(dt);
        CurrentScene?.FixedUpdate(dt);
    }

    public static void Render() => CurrentScene?.Render();
    public static void RenderUI() => CurrentScene?.RenderUI();

    public static void Unload()
    {
        if (_unloading) return;
        var current = CurrentScene;
        var pending = _nextScene;
        CurrentScene = _nextScene = null;
        _unloading = true;
        var errors = new List<Exception>();
        try
        {
            if (current != null) Object2D.Attempt(current.Dispose, errors);
            if (pending != null && !ReferenceEquals(pending, current)) Object2D.Attempt(pending.Dispose, errors);
        }
        finally { _unloading = false; }
        if (errors.Count > 0) throw new AggregateException("Scene shutdown failed.", errors);
    }
}
