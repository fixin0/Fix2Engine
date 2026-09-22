using System.Diagnostics;
using System.Numerics;
using Raylib_cs;
using ImGuiNET;

namespace Fix2Engine.Monitoring;

public static class PerformanceMonitor
{
    public static bool Visible { get; set; } = true;

    private static readonly float[] FpsHistory = new float[100];
    private static readonly float[] FrameTimeHistory = new float[100];
    private static readonly float[] CpuHistory = new float[100];
    private static readonly float[] GpuHistory = new float[100];
    private static int _historyIndex;

    private static TimeSpan _lastCpuTime;
    private static DateTime _lastCpuCheck = DateTime.UtcNow;
    private static float _currentCpu;
    private static float _currentGpu;
    private static PerformanceCounter? _gpuCounter;
    private static bool _gpuCounterTried;

    public static void Update(float dt)
    {
        UpdateCpuUsage();
        UpdateGpuUsage();

        FpsHistory[_historyIndex] = Raylib.GetFPS();
        FrameTimeHistory[_historyIndex] = dt * 1000.0f;
        CpuHistory[_historyIndex] = _currentCpu;
        GpuHistory[_historyIndex] = _currentGpu;
        _historyIndex = (_historyIndex + 1) % FpsHistory.Length;
    }

    public static void Draw(string? contextLine = null)
    {
        if (!Visible)
            return;

        try
        {
            ImGui.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSize(new Vector2(360, 420), ImGuiCond.FirstUseEver);
            ImGuiWindowFlags flags = ImGuiWindowFlags.NoSavedSettings;
            bool show = Visible;
            if (ImGui.Begin("Performance Monitor", ref show, flags))
            {
                float fps = Raylib.GetFPS();
                float frameTime = Raylib.GetFrameTime() * 1000.0f;
                long memory = GC.GetTotalMemory(false) / (1024 * 1024);

                ImGui.Text($"FPS: {fps:0}"); ImGui.SameLine(); ImGui.TextDisabled($"({frameTime:0.00} ms)");
                ImGui.Text($"CPU: {_currentCpu:0.0}%"); ImGui.SameLine(); ImGui.Text($"GPU: {_currentGpu:0.0}%");
                ImGui.Text($"Memory: {memory} MB"); ImGui.SameLine(); ImGui.TextDisabled($"GC: {GC.CollectionCount(0)}");
                ImGui.Separator();
                ImGui.Text("FPS"); ImGui.PlotLines("##fps", ref FpsHistory[0], FpsHistory.Length, _historyIndex, null, 0, 240, new Vector2(330, 45));
                ImGui.Text("Frame Time (ms)"); ImGui.PlotLines("##ms", ref FrameTimeHistory[0], FrameTimeHistory.Length, _historyIndex, null, 0, 33, new Vector2(330, 45));
                Vector4 cpuCol = _currentCpu > 80 ? new Vector4(1, 0.3f, 0.3f, 1) : _currentCpu > 50 ? new Vector4(1, 0.8f, 0.2f, 1) : new Vector4(0.3f, 1, 0.4f, 1);
                ImGui.TextColored(cpuCol, $"CPU { _currentCpu:0.0}%"); ImGui.PlotLines("##cpu", ref CpuHistory[0], CpuHistory.Length, _historyIndex, null, 0, 100, new Vector2(330, 45));
                Vector4 gpuCol = _currentGpu > 80 ? new Vector4(1, 0.3f, 0.3f, 1) : _currentGpu > 50 ? new Vector4(1, 0.8f, 0.2f, 1) : new Vector4(0.4f, 0.6f, 1, 1);
                ImGui.TextColored(gpuCol, $"GPU { _currentGpu:0.0}%"); ImGui.PlotLines("##gpu", ref GpuHistory[0], GpuHistory.Length, _historyIndex, null, 0, 100, new Vector2(330, 45));
                ImGui.Separator();
                if (contextLine != null)
                    ImGui.Text(contextLine);
                ImGui.Text($"Cores: {Environment.ProcessorCount}  OS: {Environment.OSVersion.Platform}");
            }
            ImGui.End();
            Visible = show;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PerformanceMonitor failed: {ex.Message}");
        }
    }

    private static void UpdateCpuUsage()
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

    private static void UpdateGpuUsage()
    {
        try
        {
            if (!_gpuCounterTried)
            {
                _gpuCounterTried = true;
                try
                {
                    var cat = new PerformanceCounterCategory("GPU Engine");
                    // Windows names the GPU graphics queue "3D" even for 2D rendering.
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
                float load = FrameTimeHistory[_historyIndex == 0 ? 99 : _historyIndex - 1];
                _currentGpu = Math.Clamp(load / 16.6f * 30f + _currentCpu * 0.2f, 0, 100);
            }
        }
        catch { _currentGpu = 0; }
    }
}
