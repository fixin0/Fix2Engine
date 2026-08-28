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
        public Game() : base(1280, 720, "F2Engine Demo") { }

        protected override void Start()
        {
            rlImGui.Setup(true);
            ApplyImGuiTheme();
            SceneManager.LoadScene<MainMenuScene>();
        }

        protected override void Update(float dt)
        {
            SceneManager.Update(dt);
        }

        protected override void Render()
        {
            SceneManager.Render();

            rlImGui.Begin();
            SceneManager.RenderUI();
            rlImGui.End();

            Raylib.DrawFPS(Width - 90, 10);
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
