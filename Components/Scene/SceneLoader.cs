using System;
using Fix2Engine.Components.Scene;

namespace Fix2Engine.Components
{
    public static class SceneManager
    {
        public static IFixScene? CurrentScene { get; private set; }
        private static IFixScene? _nextScene;

        public static void LoadScene<T>() where T : IFixScene, new()
        {
            _nextScene = new T();
        }

        public static void LoadScene(IFixScene scene)
        {
            _nextScene = scene;
        }

        public static void Update(float dt)
        {
            // Scene transition is safely performed at the start of the frame (Deferred Switch)
            if (_nextScene != null)
            {
                CurrentScene?.Dispose();
                CurrentScene = _nextScene;
                CurrentScene.Start();
                _nextScene = null;
            }

            CurrentScene?.Update(dt);
        }

        public static void Render()
        {
            CurrentScene?.Render();
        }

        public static void RenderUI()
        {
            CurrentScene?.RenderUI();
        }

        public static void Unload()
        {
            CurrentScene?.Dispose();
            CurrentScene = null;
        }
    }
}