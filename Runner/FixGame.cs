using Fix2Engine.Components;
using Fix2Engine.Core;
using Fix2Engine.Graphics;

namespace Fix2Engine;

public sealed class GameSettings
{
    public int Width { get; set; } = 1280;
    public int Height { get; set; } = 720;
    public string Title { get; set; } = "Fix2Engine";
    public int TargetFps { get; set; } = 240;
    public Color32 ClearColor { get; set; } = Color32.Black;
    public bool ShowFps { get; set; }
    public bool EnableUI { get; set; }
}

/// <summary>Game callbacks. Scene updates, drawing and cleanup are automatic.</summary>
public abstract class FixGame
{
    internal Action? CloseRequested;
    public void RequestClose() => CloseRequested?.Invoke();
    protected internal virtual void Configure(GameSettings settings) { }
    protected internal virtual void Start() { }
    protected internal virtual void Update(float dt) { }
    protected internal virtual void FixedUpdate(float dt) { }
    protected internal virtual void Render(RenderContext graphics) { }
    protected internal virtual void RenderUI() { }
    protected internal virtual void OnUnload() { }
}

public static class Fix2
{
    public static void Run<T>() where T : FixGame, new() => Run(new T());
    public static void Run(FixGame game)
    {
        ArgumentNullException.ThrowIfNull(game);
        var settings = new GameSettings();
        game.Configure(settings);
        ArgumentOutOfRangeException.ThrowIfNegative(settings.TargetFps);
        using var host = new GameHost(game, settings);
        host.Run();
    }
}

internal sealed class GameHost : Windowing
{
    private readonly FixGame _game;
    public GameHost(FixGame game, GameSettings settings) : base(settings.Width, settings.Height, settings.Title)
    {
        _game = game;
        ClearColor = settings.ClearColor;
        ShowFps = settings.ShowFps;
        EnableUI = settings.EnableUI;
        TargetFps = settings.TargetFps;
        game.CloseRequested = RequestClose;
    }
    protected override void Start() => _game.Start();
    protected override void Update(float dt) { _game.Update(dt); SceneManager.Update(dt); }
    protected override void FixedUpdate(float dt) { _game.FixedUpdate(dt); SceneManager.FixedUpdate(dt); }
    protected override void Render(RenderContext graphics) { SceneManager.Render(); _game.Render(graphics); }
    protected override void RenderUI() { SceneManager.RenderUI(); _game.RenderUI(); }
    protected override void OnUnload()
    {
        _game.CloseRequested = null;
        try { _game.OnUnload(); }
        finally { SceneManager.Unload(); }
    }
}
