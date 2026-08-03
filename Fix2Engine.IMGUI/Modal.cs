using System.Numerics;
using ImGuiNET;

namespace Fix2Engine.IMGUI
{
    
    public static class Modal
    {
        public static void Open(string id) => ImGui.OpenPopup(id);

        public static bool Begin(string id, Vector2 size)
        {
            ImGui.SetNextWindowSize(size, ImGuiCond.Appearing);
            return ImGui.BeginPopupModal(id, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings);
        }

        public static void End() => ImGui.EndPopup();

       
        public static bool? Confirm(string id, string message, string confirmLabel = "Evet", string cancelLabel = "İptal")
        {
            bool? result = null;
            if (Begin(id, new Vector2(360, 140)))
            {
                IMGUI.Header(message);

                float btnWidth = 120f;
                float spacing = ImGui.GetStyle().ItemSpacing.X;
                float totalWidth = btnWidth * 2 + spacing;
                ImGui.SetCursorPosX((ImGui.GetWindowSize().X - totalWidth) * 0.5f);

                if (IMGUI.DangerButton(confirmLabel, new Vector2(btnWidth, 0)))
                {
                    result = true;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.SameLine();
                if (ImGui.Button(cancelLabel, new Vector2(btnWidth, 0)))
                {
                    result = false;
                    ImGui.CloseCurrentPopup();
                }
                End();
            }
            return result;
        }
    }
}
