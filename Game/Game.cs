using Raylib_cs;
using rlImGui_cs;
using Fix2Engine.Components;
using Fix2Engine.Components.Scene; // ya da SceneManager namespace'in
using Fix2Engine.Graphics;

namespace Fix2Engine
{
    public class Game : Windowing
    {
        public Game() : base(1024, 768, "Fix2Engine - DEBUGGING") { }

        protected override void Start()
        {
            rlImGui.Setup(true);

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
    }
}