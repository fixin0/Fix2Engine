using System;
using System.Numerics;
using Fix2Engine.Components;
using Fix2Engine.Components.Scene;
using Fix2Engine.IMGUI;
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
            if (IMGUI.IMGUI.BeginCenteredPanel("MainMenu", _menuSize))
            {
                IMGUI.IMGUI.Header("MAIN MENU");

                if (IMGUI.IMGUI.Button("3D Engine Showcase", _buttonSize))
                {
                    SceneManager.LoadScene<Debug3DScene>();
                }
                
                if (IMGUI.IMGUI.Button("2D Engine Showcase", _buttonSize))
                {
                     SceneManager.LoadScene<Debug2DPixelScene>();
                }

                if (IMGUI.IMGUI.Button("Settings", _buttonSize))
                {
                    Modal.Open("SettingsModal");
                }

                Widgets.Spacer(4);

                if (IMGUI.IMGUI.DangerButton("Exit Game", _buttonSize))
                {
                    Modal.Open("ExitConfirm");
                }
            }
            IMGUI.IMGUI.EndPanel(); // Stilleri ve ImGui.End()'i kendisi halleder

            if (Modal.Confirm("ExitConfirm", "Oyundan çıkmak istediğine emin misin?", "Çık", "İptal") == true)
            {
                Environment.Exit(0);
            }

            RenderSettingsModal();
        }

        private void RenderSettingsModal()
        {
            if (Modal.Begin("SettingsModal", new Vector2(320, 180)))
            {
                IMGUI.IMGUI.Header("AYARLAR");

                Widgets.Toggle("Müzik", ref _musicEnabled);

                Widgets.Spacer(12);
                if (IMGUI.IMGUI.Button("Kapat", new Vector2(120, 36)))
                {
                    ImGuiNET.ImGui.CloseCurrentPopup();
                }
                Modal.End();
            }
        }

        public void Unload() { }
        public void Dispose() { }
    }
}
