using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;

namespace Fix2Engine.IMGUI
{
    public static class IMGUI
    {
        // Aktif pencerelerin push ettiği stil sayılarını takip eden stack
        private static readonly Stack<(int colors, int vars)> _styleStack = new();

        // --- PANEL BAŞLANGICI ---
        public static bool BeginCenteredPanel(string id, Vector2 size)
        {
            Vector2 displaySize = ImGui.GetIO().DisplaySize;

            ImGui.SetNextWindowPos(displaySize * 0.5f, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(size, ImGuiCond.Always);

            // Stilleri uygula
            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 12.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 8.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 12));

            ImGui.PushStyleColor(ImGuiCol.WindowBg, Palette.Background);
            ImGui.PushStyleColor(ImGuiCol.Border, Palette.PanelBorder);
            ImGui.PushStyleColor(ImGuiCol.Button, Palette.AccentActive);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Palette.AccentHover);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, Palette.Accent);

            // Uygulanan stil/renk sayılarını kaydet (EndPanel'da temizleyeceğiz)
            _styleStack.Push((colors: 5, vars: 4));

            ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration |
                                     ImGuiWindowFlags.NoMove |
                                     ImGuiWindowFlags.NoResize |
                                     ImGuiWindowFlags.NoSavedSettings;

            return ImGui.Begin(id, flags);
        }

        // Belirli bir ekran konumuna sabitlenmiş panel (HUD elemanları için: köşe göstergeleri vb.)
        public static bool BeginPanelAt(string id, Vector2 pos, Vector2 size, Vector2? pivot = null)
        {
            ImGui.SetNextWindowPos(pos, ImGuiCond.Always, pivot ?? Vector2.Zero);
            ImGui.SetNextWindowSize(size, ImGuiCond.Always);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 10.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1.0f);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, Palette.Background);
            ImGui.PushStyleColor(ImGuiCol.Border, Palette.PanelBorder);

            _styleStack.Push((colors: 2, vars: 2));

            ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration |
                                     ImGuiWindowFlags.NoMove |
                                     ImGuiWindowFlags.NoResize |
                                     ImGuiWindowFlags.NoSavedSettings |
                                     ImGuiWindowFlags.NoFocusOnAppearing;

            return ImGui.Begin(id, flags);
        }

        // --- PANEL BİTİŞİ ---
        public static void EndPanel()
        {
            ImGui.End();

            // Bu panele ait stilleri otomatik Pop et
            if (_styleStack.Count > 0)
            {
                var (colors, vars) = _styleStack.Pop();
                if (colors > 0) ImGui.PopStyleColor(colors);
                if (vars > 0) ImGui.PopStyleVar(vars);
            }
        }

        // --- BİLEŞENLER ---
        public static void Header(string text)
        {
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 10.0f);
            
            float windowWidth = ImGui.GetWindowSize().X;
            float textWidth = ImGui.CalcTextSize(text).X;
            ImGui.SetCursorPosX((windowWidth - textWidth) * 0.5f);

            ImGui.TextColored(Palette.Text, text);
            ImGui.Separator();
            ImGui.Dummy(new Vector2(0, 10));
        }

        public static bool Button(string label, Vector2 size)
        {
            float windowWidth = ImGui.GetWindowSize().X;
            ImGui.SetCursorPosX((windowWidth - size.X) * 0.5f);
            return ImGui.Button(label, size);
        }

        public static bool DangerButton(string label, Vector2 size)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Palette.Danger);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Palette.DangerHover);

            bool clicked = Button(label, size);

            ImGui.PopStyleColor(2);
            return clicked;
        }
    }
}