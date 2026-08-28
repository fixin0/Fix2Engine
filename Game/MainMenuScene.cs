using System;
using System.Numerics;
using Fix2Engine.Components;
using Fix2Engine.Components.Scene;
using ImGuiNET;
using Raylib_cs;

namespace Fix2Engine
{
    public class MainMenuScene : IFixScene
    {
        private readonly Vector2 _menuSize = new Vector2(360, 360);
        private readonly Vector2 _buttonSize = new Vector2(280, 48);

        private bool _musicEnabled = true;

        public void Start() { }
        public void Update(float dt) { }

        public void Render()
        {
            Raylib.ClearBackground(new Color(15, 15, 20, 255));
        }

        public void RenderUI()
        {
            var viewport = ImGui.GetMainViewport();
            ImGui.SetNextWindowPos(viewport.GetCenter(), ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(_menuSize, ImGuiCond.Always);

            ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings;
            if (ImGui.Begin("MainMenu", flags))
            {
                float winW = ImGui.GetWindowSize().X;
                string header = "MAIN MENU";
                float tw = ImGui.CalcTextSize(header).X;
                ImGui.SetCursorPosX((winW - tw) * 0.5f);
                ImGui.TextColored(new Vector4(0.90f, 0.92f, 0.98f, 1.00f), header);
                ImGui.Separator();
                ImGui.Dummy(new Vector2(0, 10));

                ImGui.SetCursorPosX((winW - _buttonSize.X) * 0.5f);
                if (ImGui.Button("3D Engine Showcase", _buttonSize))
                    SceneManager.LoadScene<Debug3DScene>();

                ImGui.SetCursorPosX((winW - _buttonSize.X) * 0.5f);
                if (ImGui.Button("2D Engine Showcase", _buttonSize))
                    SceneManager.LoadScene<Debug2DPixelScene>();

                ImGui.SetCursorPosX((winW - _buttonSize.X) * 0.5f);
                if (ImGui.Button("Settings", _buttonSize))
                    ImGui.OpenPopup("SettingsModal");

                ImGui.Dummy(new Vector2(0, 4));

                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.45f, 0.15f, 0.18f, 0.80f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.65f, 0.20f, 0.22f, 1.00f));
                ImGui.SetCursorPosX((winW - _buttonSize.X) * 0.5f);
                bool exitClicked = ImGui.Button("Exit Game", _buttonSize);
                ImGui.PopStyleColor(2);
                if (exitClicked)
                    ImGui.OpenPopup("ExitConfirm");

                ImGui.End();
            }

            if (ImGui.BeginPopupModal("ExitConfirm", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings))
            {
                ImGui.Text("Oyundan cikmak istedigine emin misin?");
                ImGui.Separator();
                float btnW = 120f;
                float spacing = ImGui.GetStyle().ItemSpacing.X;
                float totalW = btnW * 2 + spacing;
                ImGui.SetCursorPosX((ImGui.GetWindowSize().X - totalW) * 0.5f);
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.45f, 0.15f, 0.18f, 0.80f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.65f, 0.20f, 0.22f, 1.00f));
                if (ImGui.Button("Cik", new Vector2(btnW, 0)))
                {
                    ImGui.CloseCurrentPopup();
                    Environment.Exit(0);
                }
                ImGui.PopStyleColor(2);
                ImGui.SameLine();
                if (ImGui.Button("Iptal", new Vector2(btnW, 0)))
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }

            RenderSettingsModal();
        }

        private void RenderSettingsModal()
        {
            ImGui.SetNextWindowSize(new Vector2(320, 180), ImGuiCond.Appearing);
            if (ImGui.BeginPopupModal("SettingsModal", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings))
            {
                string header = "AYARLAR";
                float winW = ImGui.GetWindowSize().X;
                float tw = ImGui.CalcTextSize(header).X;
                ImGui.SetCursorPosX((winW - tw) * 0.5f);
                ImGui.TextColored(new Vector4(0.90f, 0.92f, 0.98f, 1.00f), header);
                ImGui.Separator();
                ImGui.Dummy(new Vector2(0, 10));

                ImGui.Checkbox("Muzik", ref _musicEnabled);
                ImGui.Dummy(new Vector2(0, 12));
                float btnW = 120f;
                ImGui.SetCursorPosX((winW - btnW) * 0.5f);
                if (ImGui.Button("Kapat", new Vector2(btnW, 36)))
                    ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
            }
        }

        public void Unload() { }
        public void Dispose() { }
    }
}
