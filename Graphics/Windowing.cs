using Fix2Engine.Input;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Fix2Engine.Graphics;

public class Windowing : IDisposable
{
    public int Width { get; set; }
    public int Height { get; set; }
    public string Title { get; set; }
    private const float FixedDeltaTime = 1f / 60f;
    private float _accumulator;
    private bool _started;
    private bool _disposed;

    public Windowing(int width, int height, string title)
    {
        Width = width;
        Height = height;
        Title = title;
        Init();
        InitWindow(Width, Height, Title);
        SetTargetFPS(240);
    }

    protected virtual void Init() { }
    protected virtual void Start() { }
    protected virtual void Update(float dt) { }
    protected virtual void FixedUpdate(float dt) { }
    protected virtual void Render() => ClearBackground(Color.Black);
    /// <summary>Release scene and graphics resources here, before the window closes.</summary>
    protected virtual void OnUnload() { }

    public void Run()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_started) throw new InvalidOperationException("Run can only be called once.");
        _started = true;
        try
        {
            Start();
            while (!_disposed && !WindowShouldClose())
            {
                float dt = GetFrameTime();
                InputManager.Update();
                Update(dt);
                if (_disposed) break;
                _accumulator += MathF.Min(dt, 0.25f);
                while (_accumulator >= FixedDeltaTime && !_disposed)
                {
                    FixedUpdate(FixedDeltaTime);
                    _accumulator -= FixedDeltaTime;
                }
                if (_disposed) break;
                BeginDrawing();
                try { Render(); }
                finally { EndDrawing(); }
            }
        }
        finally { Dispose(); }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        try { if (_started) OnUnload(); }
        finally
        {
            CloseWindow();
            GC.SuppressFinalize(this);
        }
    }
}
