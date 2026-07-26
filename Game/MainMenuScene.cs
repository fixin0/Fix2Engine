using Fix2Engine.Components;
using Raylib_cs;
using ImGuiNET;
using Fix2Engine.Components.Scene;

namespace Fix2Engine
{
    public class MainMenuScene : IFixScene
    {
        public void Start()
        {
        }

        public void Update(float dt)
        {
        }

        public void Render()
        {
            Raylib.ClearBackground(Color.Black);
        }

        public void RenderUI()
        {
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(ImGui.GetIO().DisplaySize.X * 0.5f, ImGui.GetIO().DisplaySize.Y * 0.5f), ImGuiCond.FirstUseEver, new System.Numerics.Vector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 150), ImGuiCond.FirstUseEver);

            ImGui.Begin("Main Menu");

            if (ImGui.Button("Go to Debug3DScene", new System.Numerics.Vector2(250, 50)))
            {
                SceneManager.LoadScene<Debug3DScene>();
            }

            ImGui.End();
        }

        public void Unload()
        {
        }

        public void Dispose()
        {
        }
    }
}
