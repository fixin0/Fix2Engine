using System;
using System.Numerics;
using ImGuiNET;

namespace Fix2Engine.IMGUI
{
    
    public readonly struct UIStyleScope : IDisposable
    {
        private readonly int _colorCount;
        private readonly int _varCount;

        public UIStyleScope(int colorCount, int varCount)
        {
            _colorCount = colorCount;
            _varCount = varCount;
        }

        public void Dispose()
        {
            if (_colorCount > 0) ImGui.PopStyleColor(_colorCount);
            if (_varCount > 0) ImGui.PopStyleVar(_varCount);
        }
    }

    
    public readonly struct UIPanelScope : IDisposable
    {
        private readonly bool _isOpen;

        public UIPanelScope(string id, ImGuiWindowFlags flags)
        {
            _isOpen = ImGui.Begin(id, flags);
        }

        public bool IsVisible => _isOpen;

        public void Dispose()
        {
            ImGui.End();
        }
    }
}