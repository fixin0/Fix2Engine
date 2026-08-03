using System.Numerics;
using ImGuiNET;

namespace Fix2Engine.IMGUI
{
    
    public static class Palette
    {
        public static Vector4 Background = new(0.08f, 0.08f, 0.12f, 0.85f);
        public static Vector4 PanelBorder = new(0.25f, 0.27f, 0.38f, 0.50f);

        public static Vector4 Text = new(0.90f, 0.92f, 0.98f, 1.00f);
        public static Vector4 TextMuted = new(0.60f, 0.62f, 0.70f, 1.00f);

        public static Vector4 Accent = new(0.38f, 0.45f, 0.65f, 1.00f);
        public static Vector4 AccentHover = new(0.28f, 0.33f, 0.48f, 1.00f);
        public static Vector4 AccentActive = new(0.18f, 0.20f, 0.28f, 1.00f);

        public static Vector4 Danger = new(0.45f, 0.15f, 0.18f, 0.80f);
        public static Vector4 DangerHover = new(0.65f, 0.20f, 0.22f, 1.00f);

        public static Vector4 Success = new(0.16f, 0.45f, 0.25f, 1.00f);
        public static Vector4 SuccessHover = new(0.20f, 0.62f, 0.34f, 1.00f);

        public static Vector4 FrameBg = new(0.14f, 0.15f, 0.20f, 1.00f);
        public static Vector4 FrameBgHover = new(0.20f, 0.22f, 0.30f, 1.00f);
    }

    
    public static class Theme
    {
        private static bool _applied;

        public static void ApplyDark()
        {
            if (_applied) return;
            _applied = true;

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
            c[(int)ImGuiCol.WindowBg] = Palette.Background;
            c[(int)ImGuiCol.PopupBg] = Palette.Background;
            c[(int)ImGuiCol.Border] = Palette.PanelBorder;
            c[(int)ImGuiCol.Text] = Palette.Text;
            c[(int)ImGuiCol.TextDisabled] = Palette.TextMuted;

            c[(int)ImGuiCol.Button] = Palette.AccentActive;
            c[(int)ImGuiCol.ButtonHovered] = Palette.AccentHover;
            c[(int)ImGuiCol.ButtonActive] = Palette.Accent;

            c[(int)ImGuiCol.FrameBg] = Palette.FrameBg;
            c[(int)ImGuiCol.FrameBgHovered] = Palette.FrameBgHover;
            c[(int)ImGuiCol.FrameBgActive] = Palette.FrameBgHover;

            c[(int)ImGuiCol.CheckMark] = Palette.Accent;
            c[(int)ImGuiCol.SliderGrab] = Palette.Accent;
            c[(int)ImGuiCol.SliderGrabActive] = Palette.AccentHover;

            c[(int)ImGuiCol.Tab] = Palette.FrameBg;
            c[(int)ImGuiCol.TabHovered] = Palette.AccentHover;
            c[(int)ImGuiCol.TabSelected] = Palette.Accent;

            c[(int)ImGuiCol.Header] = Palette.AccentActive;
            c[(int)ImGuiCol.HeaderHovered] = Palette.AccentHover;
            c[(int)ImGuiCol.HeaderActive] = Palette.Accent;

            c[(int)ImGuiCol.ScrollbarBg] = Palette.Background;
            c[(int)ImGuiCol.ScrollbarGrab] = Palette.FrameBg;
            c[(int)ImGuiCol.ScrollbarGrabHovered] = Palette.FrameBgHover;
        }
    }
}
