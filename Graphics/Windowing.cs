using Fix2Engine.Input;
using Fix2Engine.Core;
using Fix2Engine.Backends.Raylib;

namespace Fix2Engine.Graphics;

/// <summary>Owns a single window and its backend.</summary>
public class Windowing : IDisposable
{
    private readonly IGameBackend _backend;
    private int _width, _height;
    private string _title;
    private int _targetFps = 240;
    public int Width { get => _width; set { ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value); _width = value; if (_backend.IsReady) _backend.SetSize(_width, _height); } }
    public int Height { get => _height; set { ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value); _height = value; if (_backend.IsReady) _backend.SetSize(_width, _height); } }
    public string Title { get => _title; set { ArgumentNullException.ThrowIfNull(value); _title = value; if (_backend.IsReady) _backend.SetTitle(value); } }
    public int TargetFps { get => _targetFps; set { ArgumentOutOfRangeException.ThrowIfNegative(value); _targetFps = value; if (_backend.IsReady) _backend.SetTargetFps(value); } }
    public Color32 ClearColor { get; set; } = Color32.Black;
    public bool ShowFps { get; set; }
    public bool EnableUI { get; set; }
    private const float FixedDeltaTime = 1f / 60f;
    private float _accumulator;
    private bool _started, _disposed, _closeRequested, _inCallback;

    public Windowing(int width, int height, string title, IGameBackend? backend = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentNullException.ThrowIfNull(title);
        _width = width; _height = height; _title = title;
        _backend = backend ?? new RaylibBackend();
        EngineBackend.Attach(_backend);
        try { Init(); _backend.Open(Width, Height, Title); _backend.SetTargetFps(TargetFps); }
        catch
        {
            try { _backend.Dispose(); }
            finally { EngineBackend.Detach(_backend); }
            throw;
        }
    }

    protected virtual void Init() { }
    protected virtual void Start() { }
    protected virtual void Update(float dt) { }
    protected virtual void FixedUpdate(float dt) { }
    protected virtual void Render() { }
    protected virtual void Render(RenderContext graphics) => Render();
    protected virtual void RenderUI() { }
    protected virtual void OnUnload() { }
    public void RequestClose() => _closeRequested = true;

    public void Run()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_started) throw new InvalidOperationException("Run can only be called once.");
        _started = true;
        _inCallback = true;
        try
        {
            Start();
            while (!_closeRequested && !_backend.ShouldClose)
            {
                float dt = _backend.FrameTime;
                InputManager.Update();
                Update(dt);
                if (_closeRequested) break;
                _accumulator += MathF.Min(dt, 0.25f);
                while (_accumulator >= FixedDeltaTime && !_closeRequested)
                {
                    FixedUpdate(FixedDeltaTime);
                    _accumulator -= FixedDeltaTime;
                }
                if (_closeRequested) break;
                _backend.BeginDrawing();
                try
                {
                    _backend.Clear(ClearColor);
                    Render(RenderContext.Current);
                    if (!_closeRequested && EnableUI)
                    {
                        _backend.BeginUI();
                        try { RenderUI(); }
                        finally { _backend.EndUI(); }
                    }
                    if (!_closeRequested && ShowFps)
                        RenderContext.Current.DrawText($"{_backend.FramesPerSecond} FPS", new(Width - 100, 10), 20, Color32.White);
                }
                finally { _backend.EndDrawing(); }
            }
        }
        finally { _inCallback = false; Dispose(); }
    }

    public void Dispose()
    {
        if (_disposed) return;
        if (_inCallback) { RequestClose(); return; }
        _disposed = true;
        try { if (_started) OnUnload(); }
        finally
        {
            try { _backend.Dispose(); }
            finally { EngineBackend.Detach(_backend); GC.SuppressFinalize(this); }
        }
    }
}
