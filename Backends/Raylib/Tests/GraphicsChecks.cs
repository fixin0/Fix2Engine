using System.Numerics;
using Fix2Engine.Core;
using Fix2Engine.Backends.Raylib;
using Fix2Engine.Components;
using Fix2Engine.Components.Animation;
using Fix2Engine.Graphics;
using Raylib_cs;
using static Raylib_cs.Raylib;

static class GraphicsChecks
{
    public static void Run(Action<bool, string> check)
    {
        SetTraceLogLevel(TraceLogLevel.Warning);
        SetConfigFlags(ConfigFlags.HiddenWindow);
        using var backendWindow = new Windowing(320, 240, "Object2D rendering checks");
        if (!IsWindowReady()) throw new InvalidOperationException("A graphics display is required for --graphics.");
        string path = Path.Combine(Path.GetTempPath(), $"fix2-sprite-{Guid.NewGuid():N}.png");
        try
        {
            var image = GenImageColor(32, 16, Color.Red);
            ImageDrawRectangle(ref image, 16, 0, 16, 16, Color.Blue);
            ExportImage(image, path);
            using var texture = Content.LoadTexture(path);
            UnloadImage(image);
            try
            {
                using var shared = new Sprite2D(texture);
                using var scene = new ProbeScene();
                var animated = scene.Add(new AnimatedSprite2D { Position = new(20, 20), Scale = new(2, 2) });
                animated.SetSprite(shared);
                animated.Animator.Add(SpriteAnimation.FromGrid("colors", 16, 16, 2, 2, 4));
                animated.Animator.Play("colors");
                var parent = scene.Add(new Object2D { Position = new(120, 60), Rotation = 90, Scale = new(2, 1) });
                var child = parent.AddChild(new SpriteObject2D { Position = new(10, 0), SourceRect = new RectF(0, 0, 16, 16) });
                child.SetSprite(shared);
                var mirror = scene.Add(new SpriteObject2D { Position = new(220, 20), Scale = new(-2, 2) });
                mirror.SetSprite(shared);
                var flipped = scene.Add(new SpriteObject2D { Position = new(20, 90), Scale = new(2, 2), FlipX = true });
                flipped.SetSprite(shared);
                var lazy = scene.Add(new SpriteObject2D { TexturePath = path, Position = new(100, 160) });
                scene.Update(0);
                check(lazy.Sprite == null, "File-backed textures should load only when first rendered.");

                Image DrawAndRead()
                {
                    BeginDrawing();
                    ClearBackground(Color.Black);
                    scene.Render();
                    EndDrawing();
                    return LoadImageFromScreen();
                }
                bool Pixel(Image result, int x, int y, Color expected)
                {
                    var pixel = GetImageColor(result, x, y);
                    return pixel.R == expected.R && pixel.G == expected.G && pixel.B == expected.B;
                }
                var result = DrawAndRead();
                try
                {
                    check(Pixel(result, 28, 28, Color.Red), "Animated object must draw the first sprite-sheet frame.");
                    check(Pixel(result, 112, 90, Color.Red), "Rendered parent rotation and scale must agree with the world transform.");
                    check(Pixel(result, 212, 28, Color.Red) && Pixel(result, 164, 28, Color.Blue), "Negative object scales must remain visible and mirror the texture.");
                    check(Pixel(result, 28, 98, Color.Blue) && Pixel(result, 76, 98, Color.Red), "FlipX must reverse sprite-sheet UVs.");
                    check(Pixel(result, 104, 164, Color.Red), "A file-backed object must actually draw its texture.");
                }
                finally { UnloadImage(result); }
                var cached = lazy.Sprite;
                scene.Update(0.25f);
                result = DrawAndRead();
                try { check(Pixel(result, 28, 28, Color.Blue), "Animation update must change the rendered source rectangle."); }
                finally { UnloadImage(result); }
                check(ReferenceEquals(cached, lazy.Sprite), "Texture must be cached across frames.");
                scene.Dispose();
                check(cached!.IsDisposed && !shared.IsDisposed, "Scene cleanup must release owned textures while retaining borrowed resources.");
            }
            finally { texture.Dispose(); }
        }
        finally
        {
            backendWindow.Dispose();
            File.Delete(path);
        }

        using (var window = new CleanupWindow(false))
        {
            window.Run();
            window.Dispose();
            check(window.Unloads == 1 && window.WasWindowReady && window.Resource!.IsDisposed && !IsWindowReady(),
                "Window shutdown must dispose the scene before closing the graphics context, exactly once.");
        }
        using (var window = new CleanupWindow(true))
        {
            try { window.Run(); }
            catch (InvalidOperationException exception) when (exception.Message == "expected update failure") { }
            check(window.Unloads == 1 && window.Resource!.IsDisposed && !IsWindowReady(),
                "Update failures must still release scene resources and close the window.");
        }
        var game = new HostedGame();
        var uiDirectory = Directory.CreateTempSubdirectory("fix2-ui-check-");
        string previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = uiDirectory.FullName;
            Fix2Engine.Fix2.Run(game);
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
            uiDirectory.Delete(recursive: true);
        }
        check(game.UiFrames == 1 && game.Scene.Object.Updates == 1 && game.Scene.Object.Draws == 1,
            "FixGame must automatically dispatch scene update, rendering and the optional UI frame.");
        check(game.Scene.IsDisposed && game.Scene.Object.IsDestroyed && !IsWindowReady() && !EngineBackend.IsAttached,
            "FixGame must dispose the scene, UI context and native window on exit.");
    }

    private sealed class HostedGame : Fix2Engine.FixGame
    {
        public readonly HostedScene Scene = new();
        public int UiFrames;
        protected override void Configure(Fix2Engine.GameSettings settings)
        {
            settings.Width = 64; settings.Height = 64; settings.EnableUI = true;
        }
        protected override void Start() => SceneManager.LoadScene(Scene);
        protected override void RenderUI() { UiFrames++; RequestClose(); }
    }
    private sealed class HostedScene : Fix2Engine.Components.Scene.FixScene
    {
        public readonly HostedObject Object = new();
        protected override void OnStart() => Add(Object);
    }
    private sealed class HostedObject : Object2D
    {
        public int Updates, Draws;
        protected override void OnUpdate(float dt) => Updates++;
        protected override void OnRender(RenderContext graphics)
        {
            Draws++;
            graphics.DrawRectangle(new RectF(0, 0, 16, 16), Color32.White);
        }
    }

    private sealed class CleanupWindow(bool fail) : Windowing(64, 64, "Cleanup check")
    {
        public int Unloads;
        public bool WasWindowReady;
        public Sprite2D? Resource;
        protected override void Start()
        {
            var image = GenImageColor(4, 4, Color.White);
            string path = Path.Combine(Path.GetTempPath(), $"fix2-cleanup-{Guid.NewGuid():N}.png");
            try { ExportImage(image, path); Resource = new Sprite2D(path); }
            finally { File.Delete(path); }
            UnloadImage(image);
            var scene = new ProbeScene();
            var obj = scene.Add(new SpriteObject2D());
            obj.SetSprite(Resource, ownsSprite: true);
            SceneManager.LoadScene(scene);
        }
        protected override void Update(float dt)
        {
            SceneManager.Update(dt);
            if (fail) throw new InvalidOperationException("expected update failure");
            Dispose();
        }
        protected override void OnUnload()
        {
            Unloads++;
            WasWindowReady = IsWindowReady();
            SceneManager.Unload();
        }
    }
}
