using System.Numerics;
using ImGuiNET;

namespace Fix2Engine.IMGUI
{
    
    public static class Widgets
    {
        public static void Spacer(float height = 8f) => ImGui.Dummy(new Vector2(0, height));

        public static void SeparatorWithLabel(string label)
        {
            ImGui.Spacing();
            ImGui.TextColored(Palette.TextMuted, label);
            ImGui.Separator();
            ImGui.Spacing();
        }

        public static bool Checkbox(string label, ref bool value) => ImGui.Checkbox(label, ref value);

       
        public static bool Toggle(string label, ref bool value)
        {
            ImGui.PushID(label);
            var draw = ImGui.GetWindowDrawList();
            float height = ImGui.GetFrameHeight();
            float width = height * 1.8f;
            Vector2 pos = ImGui.GetCursorScreenPos();

            bool clicked = ImGui.InvisibleButton("##toggle", new Vector2(width, height));
            if (clicked) value = !value;

            float t = value ? 1f : 0f;
            uint bg = ImGui.ColorConvertFloat4ToU32(value ? Palette.Accent : Palette.FrameBg);
            draw.AddRectFilled(pos, pos + new Vector2(width, height), bg, height * 0.5f);

            float knobRadius = height * 0.5f - 2f;
            Vector2 knobCenter = pos + new Vector2(height * 0.5f + t * (width - height), height * 0.5f);
            draw.AddCircleFilled(knobCenter, knobRadius, ImGui.ColorConvertFloat4ToU32(Palette.Text));

            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(Palette.Text, label);
            ImGui.PopID();

            return clicked;
        }

        public static bool SliderFloat(string label, ref float value, float min, float max, string format = "%.1f")
            => ImGui.SliderFloat(label, ref value, min, max, format);

        public static bool SliderInt(string label, ref int value, int min, int max)
            => ImGui.SliderInt(label, ref value, min, max);

        public static bool InputText(string label, ref string value, uint maxLength = 256)
            => ImGui.InputText(label, ref value, maxLength);

        public static bool Combo(string label, ref int selectedIndex, string[] options)
            => ImGui.Combo(label, ref selectedIndex, options, options.Length);

        public static void ProgressBar(float fraction, string overlay = "")
            => ImGui.ProgressBar(fraction, new Vector2(-1, 0), overlay);

        public static void Tooltip(string text)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(300f);
                ImGui.TextColored(Palette.Text, text);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        public static bool IconButton(string glyphOrLabel, Vector2 size)
        {
            using var scope = new UIStyleScope(colorCount: 0, varCount: 1);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, size.X * 0.5f);
            bool clicked = ImGui.Button(glyphOrLabel, size);
            return clicked;
        }

        public static bool SuccessButton(string label, Vector2 size)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Palette.Success);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Palette.SuccessHover);
            bool clicked = IMGUI.Button(label, size);
            ImGui.PopStyleColor(2);
            return clicked;
        }
        
        public static IDisposableTabBar TabBar(string id) => new(id);

        public readonly struct IDisposableTabBar : IDisposable
        {
            private readonly bool _open;
            public IDisposableTabBar(string id) => _open = ImGui.BeginTabBar(id);
            public bool IsOpen => _open;
            public void Dispose() { if (_open) ImGui.EndTabBar(); }
        }

        public static bool Tab(string label) => ImGui.BeginTabItem(label);
        public static void EndTab() => ImGui.EndTabItem();
    }
}
