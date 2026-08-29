using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using rlImGui_cs;
using Fix2Engine.Components;
using Fix2Engine.Components.Scene;
using Fix2Engine.Graphics;
using ImGuiNET;

namespace Fix2Engine
{
    public class Game : Windowing
    {
        public bool ShowPerformanceMonitor { get; set; } = true;

        private readonly float[] _fpsHistory = new float[100];
        private readonly float[] _frameTimeHistory = new float[100];
        private readonly float[] _cpuHistory = new float[100];
        private readonly float[] _gpuHistory = new float[100];
        private int _historyIndex;

        private TimeSpan _lastCpuTime;
        private DateTime _lastCpuCheck = DateTime.UtcNow;
        private float _currentCpu;
        private float _currentGpu;
        private PerformanceCounter? _gpuCounter;
        private bool _gpuCounterTried;

        public Game() : base(1280, 720, "F2Engine Demo") { }

        protected override void Start()
        {
            rlImGui.Setup(true);
            ApplyImGuiTheme();
            SceneManager.LoadScene<MainMenuScene>();
        }

        protected override void Update(float dt)
        {
            try
            {
                UpdateCpuUsage();
                UpdateGpuUsage();

                _fpsHistory[_historyIndex] = Raylib.GetFPS();
                _frameTimeHistory[_historyIndex] = dt * 1000.0f;
                _cpuHistory[_historyIndex] = _currentCpu;
                _gpuHistory[_historyIndex] = _currentGpu;
                _historyIndex = (_historyIndex + 1) % _fpsHistory.Length;
            }
            catch { }

            SceneManager.Update(dt);
        }

        private void UpdateCpuUsage()
        {
            try
            {
                var proc = Process.GetCurrentProcess();
                var now = DateTime.UtcNow;
                var curCpu = proc.TotalProcessorTime;
                double elapsedMs = (now - _lastCpuCheck).TotalMilliseconds;
                if (elapsedMs > 200)
                {
                    double cpuMs = (curCpu - _lastCpuTime).TotalMilliseconds;
                    _currentCpu = (float)(cpuMs / elapsedMs / Environment.ProcessorCount * 100.0);
                    _currentCpu = Math.Clamp(_currentCpu, 0, 100);
                    _lastCpuTime = curCpu;
                    _lastCpuCheck = now;
                }
            }
            catch { _currentCpu = 0; }
        }

        private void UpdateGpuUsage()
        {
            try
            {
                if (!_gpuCounterTried)
                {
                    _gpuCounterTried = true;
                    try
                    {
                        var cat = new PerformanceCounterCategory("GPU Engine");
                        string? instance = cat.GetInstanceNames().FirstOrDefault(n => n.Contains("engtype_3D"));
                        if (instance != null)
                            _gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", instance, true);
                    }
                    catch { _gpuCounter = null; }
                }

                if (_gpuCounter != null)
                {
                    _currentGpu = _gpuCounter.NextValue();
                    _currentGpu = Math.Clamp(_currentGpu, 0, 100);
                }
                else
                {
                    float load = _frameTimeHistory[_historyIndex == 0 ? 99 : _historyIndex - 1];
                    _currentGpu = Math.Clamp(load / 16.6f * 30f + _currentCpu * 0.2f, 0, 100);
                }
            }
            catch { _currentGpu = 0; }
        }

        protected override void Render()
        {
            SceneManager.Render();

            rlImGui.Begin();
            SceneManager.RenderUI();
            if (ShowPerformanceMonitor)
                DrawPerformanceMonitor();
            rlImGui.End();

            Raylib.DrawFPS(Width - 90, 10);
        }

        private void DrawPerformanceMonitor()
        {
            try
            {
                ImGui.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowSize(new Vector2(360, 420), ImGuiCond.FirstUseEver);
                ImGuiWindowFlags flags = ImGuiWindowFlags.NoSavedSettings;
                bool show = ShowPerformanceMonitor;
                if (ImGui.Begin("Performance Monitor", ref show, flags))
                {
                    float fps = Raylib.GetFPS();
                    float frameTime = Raylib.GetFrameTime() * 1000.0f;
                    long memory = GC.GetTotalMemory(false) / (1024 * 1024);

                    ImGui.Text($"FPS: {fps:0}"); ImGui.SameLine(); ImGui.TextDisabled($"({frameTime:0.00} ms)");
                    ImGui.Text($"CPU: {_currentCpu:0.0}%"); ImGui.SameLine(); ImGui.Text($"GPU: {_currentGpu:0.0}%");
                    ImGui.Text($"Memory: {memory} MB"); ImGui.SameLine(); ImGui.TextDisabled($"GC: {GC.CollectionCount(0)}");
                    ImGui.Separator();
                    ImGui.Text("FPS"); ImGui.PlotLines("##fps", ref _fpsHistory[0], _fpsHistory.Length, _historyIndex, null, 0, 240, new Vector2(330, 45));
                    ImGui.Text("Frame Time (ms)"); ImGui.PlotLines("##ms", ref _frameTimeHistory[0], _frameTimeHistory.Length, _historyIndex, null, 0, 33, new Vector2(330, 45));
                    Vector4 cpuCol = _currentCpu > 80 ? new Vector4(1, 0.3f, 0.3f, 1) : _currentCpu > 50 ? new Vector4(1, 0.8f, 0.2f, 1) : new Vector4(0.3f, 1, 0.4f, 1);
                    ImGui.TextColored(cpuCol, $"CPU { _currentCpu:0.0}%"); ImGui.PlotLines("##cpu", ref _cpuHistory[0], _cpuHistory.Length, _historyIndex, null, 0, 100, new Vector2(330, 45));
                    Vector4 gpuCol = _currentGpu > 80 ? new Vector4(1, 0.3f, 0.3f, 1) : _currentGpu > 50 ? new Vector4(1, 0.8f, 0.2f, 1) : new Vector4(0.4f, 0.6f, 1, 1);
                    ImGui.TextColored(gpuCol, $"GPU { _currentGpu:0.0}%"); ImGui.PlotLines("##gpu", ref _gpuHistory[0], _gpuHistory.Length, _historyIndex, null, 0, 100, new Vector2(330, 45));
                    ImGui.Separator();
                    ImGui.Text($"Resolution: {Width}x{Height}");
                    ImGui.Text($"Scene: {SceneManager.CurrentScene?.GetType().Name ?? "None"}");
                    ImGui.Text($"Cores: {Environment.ProcessorCount}  OS: {Environment.OSVersion.Platform}");
                }
                ImGui.End();
                ShowPerformanceMonitor = show;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PerformanceMonitor failed: {ex.Message}");
            }
        }

        protected void OnUnload()
        {
            SceneManager.Unload();
            rlImGui.Shutdown();
        }

        private static void ApplyImGuiTheme()
        {
            var style = ImGui.GetStyle();
            style.WindowRounding = 12.0f;
            style.FrameRounding = 8.0f;
            style.GrabRounding = 8.0f;
            style.TabRounding = 6.0f;
            style.PopupRounding = 8.0f;
            style.ScrollbarRounding = 8.0f;
            style.WindowBorderSize = 1.0f;
            style.FrameBorderSize = 0.0f;
            style.ItemSpacing = new Vector2(10, 12);
            style.FramePadding = new Vector2(10, 6);
            style.WindowPadding = new Vector2(16, 16);

            var c = style.Colors;
            c[(int)ImGuiCol.WindowBg] = new Vector4(0.08f, 0.08f, 0.12f, 0.85f);
            c[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.12f, 0.85f);
            c[(int)ImGuiCol.Border] = new Vector4(0.25f, 0.27f, 0.38f, 0.50f);
            c[(int)ImGuiCol.Text] = new Vector4(0.90f, 0.92f, 0.98f, 1.00f);
            c[(int)ImGuiCol.TextDisabled] = new Vector4(0.60f, 0.62f, 0.70f, 1.00f);
            c[(int)ImGuiCol.Button] = new Vector4(0.18f, 0.20f, 0.28f, 1.00f);
            c[(int)ImGuiCol.ButtonHovered] = new Vector4(0.28f, 0.33f, 0.48f, 1.00f);
            c[(int)ImGuiCol.ButtonActive] = new Vector4(0.38f, 0.45f, 0.65f, 1.00f);
            c[(int)ImGuiCol.FrameBg] = new Vector4(0.14f, 0.15f, 0.20f, 1.00f);
            c[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.20f, 0.22f, 0.30f, 1.00f);
            c[(int)ImGuiCol.FrameBgActive] = new Vector4(0.20f, 0.22f, 0.30f, 1.00f);
            c[(int)ImGuiCol.CheckMark] = new Vector4(0.38f, 0.45f, 0.65f, 1.00f);
            c[(int)ImGuiCol.SliderGrab] = new Vector4(0.38f, 0.45f, 0.65f, 1.00f);
            c[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.28f, 0.33f, 0.48f, 1.00f);
            c[(int)ImGuiCol.Tab] = new Vector4(0.14f, 0.15f, 0.20f, 1.00f);
            c[(int)ImGuiCol.TabHovered] = new Vector4(0.28f, 0.33f, 0.48f, 1.00f);
            c[(int)ImGuiCol.TabSelected] = new Vector4(0.38f, 0.45f, 0.65f, 1.00f);
            c[(int)ImGuiCol.Header] = new Vector4(0.18f, 0.20f, 0.28f, 1.00f);
            c[(int)ImGuiCol.HeaderHovered] = new Vector4(0.28f, 0.33f, 0.48f, 1.00f);
            c[(int)ImGuiCol.HeaderActive] = new Vector4(0.38f, 0.45f, 0.65f, 1.00f);
            c[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.08f, 0.08f, 0.12f, 0.85f);
            c[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.14f, 0.15f, 0.20f, 1.00f);
            c[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.20f, 0.22f, 0.30f, 1.00f);
        }
    }
}
