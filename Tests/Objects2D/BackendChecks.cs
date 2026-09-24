using System.Numerics;
using System.Reflection;
using Fix2Engine.Core;
using Fix2Engine.Graphics;
using Fix2Engine.Components;
using Fix2Engine.Input;
using Fix2Engine.Input.InputBackend;

static class BackendChecks
{
    public static void Run(Action<bool, string> check)
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root != null && !File.Exists(Path.Combine(root.FullName, "Fix2Engine.sln"))) root = root.Parent;
        if (root == null) throw new InvalidOperationException("Engine checkout not found.");
        foreach (var folder in new[] { "Core", "Graphics", "Components", "Input", "Monitoring", "Runner", "Fix2Console" })
        {
            foreach (var path in Directory.EnumerateFiles(Path.Combine(root.FullName, folder), "*", SearchOption.AllDirectories)
                .Where(p => !p.Contains("/obj/") && !p.Contains("/bin/") && (p.EndsWith(".cs") || p.EndsWith(".csproj"))))
            {
                var source = File.ReadAllText(path);
                check(!source.Contains("Raylib_cs") && !source.Contains("rlImGui_cs")
                    && !source.Contains("Include=\"Raylib-cs\"") && !source.Contains("Include=\"rlImgui-cs\""),
                    $"Native backend dependency escaped into {path}.");
            }
        }
        var backend = new FakeBackend();
        using (var window = new RenderCloseWindow(backend))
        {
            window.Run();
            check(backend.BeginCount == 1 && backend.EndCount == 1 && backend.DisposeCount == 1,
                "Dispose during Render must finish drawing before closing the backend.");
            check(window.Unloads == 1 && !EngineBackend.IsAttached, "Host must unload exactly once and detach.");
        }
        backend = new FakeBackend();
        using (var window = new RenderCloseWindow(backend, fail: true))
        {
            try { window.Run(); } catch (InvalidOperationException e) when (e.Message == "render failure") { }
            check(backend.EndCount == 1 && backend.DisposeCount == 1 && window.Unloads == 1,
                "Render exceptions must balance drawing and release the backend.");
        }
        backend = new FakeBackend();
        using (var window = new Windowing(64, 64, "Backend checks", backend))
        {
            backend.Keys.Add((int)Keys.W);
            InputManager.Update();
            check(InputManager.IsPressed(Keys.W), "Input must poll the configured backend.");
            InputManager.Update();
            check(!InputManager.IsPressed(Keys.W) && InputManager.IsDown(Keys.W), "Held keys must not retrigger.");
            backend.Keys.Clear(); InputManager.Update();
            check(InputManager.IsReleased(Keys.W), "Backend key release must propagate.");
            using var texture = Content.LoadTexture("fake.png");
            using var sprite = new Sprite2D(texture);
            using var obj = new SpriteObject2D { Position = new(5, 6), Scale = new(-2, 1), FlipX = true, Tint = new(1, 2, 3, 4) };
            obj.SetSprite(sprite);
            obj.Render();
            check(backend.DrawCount == 1 && backend.LastSource.Width == -32 && backend.LastTint == obj.Tint,
                "Sprite source flips and RGBA must cross the backend boundary intact.");
            check(backend.LastTransform == obj.GlobalTransform && backend.TransformDepth == 0,
                "Object transforms must reach the backend and be restored.");
            using var throwing = new ThrowingObject();
            try { throwing.Render(); } catch (InvalidOperationException) { }
            check(backend.TransformDepth == 0, "Render exceptions must restore transforms.");
            sprite.Dispose();
            check(!texture.IsDisposed, "Borrowing a texture must not transfer ownership.");
            texture.Dispose(); texture.Dispose();
            check(backend.Textures[0].Unloads == 1, "A texture must unload only once.");
        }
        foreach (var assembly in new[] { typeof(Texture).Assembly, typeof(Object2D).Assembly, typeof(Color32).Assembly, typeof(InputManager).Assembly, typeof(Fix2Engine.FixGame).Assembly })
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (member is MethodBase method && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly))
                    {
                        foreach (var parameter in method.GetParameters())
                            check(!Leaks(parameter.ParameterType), $"{type}.{member.Name} exposes a native parameter.");
                        if (method is MethodInfo info) check(!Leaks(info.ReturnType), $"{type}.{member.Name} exposes a native return type.");
                    }
                    if (member is FieldInfo field && (field.IsPublic || field.IsFamily || field.IsFamilyOrAssembly))
                        check(!Leaks(field.FieldType), $"{type}.{member.Name} exposes a native field.");
                }
            }
        }
    }
    private static bool Leaks(Type type) => (type.Namespace?.StartsWith("Raylib_cs") ?? false)
        || (type.HasElementType && Leaks(type.GetElementType()!))
        || (type.IsGenericType && type.GetGenericArguments().Any(Leaks));
    private sealed class RenderCloseWindow(FakeBackend backend, bool fail = false) : Windowing(64, 64, "Test", backend)
    {
        public int Unloads;
        protected override void Render()
        {
            if (fail) throw new InvalidOperationException("render failure");
            Dispose();
        }
        protected override void OnUnload() => Unloads++;
    }
    private sealed class ThrowingObject : Object2D
    {
        protected override void OnRender(RenderContext graphics) => throw new InvalidOperationException();
    }
}
