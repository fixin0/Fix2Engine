using System.Numerics;
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
        InitWindow(320, 240, "Object2D rendering checks");
        if (!IsWindowReady()) throw new InvalidOperationException("A graphics display is required for --graphics.");
        string path = Path.Combine(Path.GetTempPath(), $"fix2-sprite-{Guid.NewGuid():N}.png");
        try
        {
            var image = GenImageColor(32, 16, Color.Red);
            ImageDrawRectangle(ref image, 16, 0, 16, 16, Color.Blue);
            ExportImage(image, path);
            Texture2D texture = LoadTextureFromImage(image);
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
                var child = parent.AddChild(new SpriteObject2D { Position = new(10, 0), SourceRect = new Rectangle(0, 0, 16, 16) });
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
            finally { UnloadTexture(texture); }
        }
        finally
        {
            CloseWindow();
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
    }

    private sealed class CleanupWindow(bool fail) : Windowing(64, 64, "Cleanup check")
    {
        public int Unloads;
        public bool WasWindowReady;
        public Sprite2D? Resource;
        protected override void Start()
        {
            var image = GenImageColor(4, 4, Color.White);
            Resource = new Sprite2D(LoadTextureFromImage(image), ownsTexture: true);
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
