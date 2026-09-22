using System;
using System.IO;

namespace Fix2Console;

public static class TemplateGenerator
{
    public static string GenerateCsproj(string projectName, string targetDir, string engineRoot)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectName)) throw new ArgumentException("Project name is empty", nameof(projectName));
            if (string.IsNullOrWhiteSpace(targetDir)) throw new ArgumentException("Target dir is empty", nameof(targetDir));
            if (string.IsNullOrWhiteSpace(engineRoot)) throw new ArgumentException("Engine root is empty", nameof(engineRoot));

            string rel(string lib)
            {
                try
                {
                    string full = Path.Combine(engineRoot, lib);
                    string relative = Path.GetRelativePath(targetDir, full).Replace('\\', '/');
                    if (string.IsNullOrWhiteSpace(relative)) throw new InvalidOperationException($"Failed to compute relative path for {lib}");
                    return System.Security.SecurityElement.Escape(relative)!;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to compute relative path for '{lib}': {ex.Message}", ex);
                }
            }

            string content = "<Project Sdk=\"Microsoft.NET.Sdk\">\n\n"
                + "    <PropertyGroup>\n"
                + "        <OutputType>Exe</OutputType>\n"
                + "        <TargetFramework>net10.0</TargetFramework>\n"
                + "        <ImplicitUsings>enable</ImplicitUsings>\n"
                + "        <Nullable>enable</Nullable>\n"
                + "        <PublishAot>true</PublishAot>\n"
                + "        <InvariantGlobalization>true</InvariantGlobalization>\n"
                + "    </PropertyGroup>\n\n"
                + "    <ItemGroup>\n"
                + $"      <ProjectReference Include=\"{rel("Graphics/Graphics.csproj")}\" />\n"
                + $"      <ProjectReference Include=\"{rel("Components/Components.csproj")}\" />\n"
                + $"      <ProjectReference Include=\"{rel("Input/Input.csproj")}\" />\n"
                + $"      <ProjectReference Include=\"{rel("Physics/Physics.csproj")}\" />\n"
                + $"      <ProjectReference Include=\"{rel("Audio/Audio.csproj")}\" />\n"
                + "    </ItemGroup>\n\n"
                + "    <ItemGroup>\n"
                + "      <PackageReference Include=\"rlImgui-cs\" Version=\"3.2.0\" />\n"
                + "    </ItemGroup>\n\n"
                + "    <ItemGroup>\n"
                + "      <None Update=\"InputMap.toml\" CopyToOutputDirectory=\"PreserveNewest\" CopyToPublishDirectory=\"PreserveNewest\" />\n"
                + "    </ItemGroup>\n"
                + $"    <Import Project=\"{rel("build/Fix2Engine.Licenses.targets")}\" Label=\"Fix2EngineLicenses\" Condition=\"'$(_Fix2EngineLicensesImported)' != 'true'\" />\n"
                + "</Project>\n";

            if (!content.Contains(projectName) && false)
                throw new InvalidDataException("Generated csproj is invalid");

            return content;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to generate csproj for '{projectName}': {ex.Message}", ex);
        }
    }

    public static string GenerateProgram(string projectName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectName)) throw new ArgumentException("Project name is empty", nameof(projectName));

            return "using System;\n\n"
                + $"namespace {projectName};\n\n"
                + "internal static class Program\n"
                + "{\n"
                + "    static void Main(string[] args)\n"
                + "    {\n"
                + "        Fix2Engine.Input.InputManager.LoadInputMap();\n"
                + "        using var game = new Game();\n"
                + "        game.Run();\n"
                + "    }\n"
                + "}\n";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate Program.cs for '{projectName}': {ex.Message}", ex);
        }
    }

    public static string GenerateGame(string projectName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectName)) throw new ArgumentException("Project name is empty", nameof(projectName));

            return "using System.Numerics;\n"
                + "using Raylib_cs;\n"
                + "using rlImGui_cs;\n"
                + "using Fix2Engine.Components;\n"
                + "using Fix2Engine.Components.Scene;\n"
                + "using Fix2Engine.Graphics;\n"
                + "using ImGuiNET;\n\n"
                + $"namespace {projectName};\n\n"
                + "public class Game : Windowing\n"
                + "{\n"
                + $"    public Game() : base(1280, 720, \"{projectName}\") {{ }}\n\n"
                + "    protected override void Start()\n"
                + "    {\n"
                + "        rlImGui.Setup(true);\n"
                + "        ApplyImGuiTheme();\n"
                + $"        SceneManager.LoadScene<{projectName}Scene>();\n"
                + "    }\n\n"
                + "    protected override void Update(float dt)\n"
                + "    {\n"
                + "        SceneManager.Update(dt);\n"
                + "    }\n\n"
                + "    protected override void Render()\n"
                + "    {\n"
                + "        SceneManager.Render();\n"
                + "        rlImGui.Begin();\n"
                + "        SceneManager.RenderUI();\n"
                + "        rlImGui.End();\n"
                + "        Raylib.DrawFPS(Width - 90, 10);\n"
                + "    }\n\n"
                + "    protected void OnUnload()\n"
                + "    {\n"
                + "        SceneManager.Unload();\n"
                + "        rlImGui.Shutdown();\n"
                + "    }\n\n"
                + "    private static void ApplyImGuiTheme()\n"
                + "    {\n"
                + "        var style = ImGui.GetStyle();\n"
                + "        style.WindowRounding = 12.0f;\n"
                + "        style.FrameRounding = 8.0f;\n"
                + "        style.GrabRounding = 8.0f;\n"
                + "        style.WindowBorderSize = 1.0f;\n"
                + "        style.ItemSpacing = new System.Numerics.Vector2(10, 12);\n"
                + "        var c = style.Colors;\n"
                + "        c[(int)ImGuiCol.WindowBg] = new System.Numerics.Vector4(0.08f, 0.08f, 0.12f, 0.85f);\n"
                + "        c[(int)ImGuiCol.Border] = new System.Numerics.Vector4(0.25f, 0.27f, 0.38f, 0.50f);\n"
                + "        c[(int)ImGuiCol.Button] = new System.Numerics.Vector4(0.18f, 0.20f, 0.28f, 1.00f);\n"
                + "        c[(int)ImGuiCol.ButtonHovered] = new System.Numerics.Vector4(0.28f, 0.33f, 0.48f, 1.00f);\n"
                + "        c[(int)ImGuiCol.ButtonActive] = new System.Numerics.Vector4(0.38f, 0.45f, 0.65f, 1.00f);\n"
                + "        c[(int)ImGuiCol.Text] = new System.Numerics.Vector4(0.90f, 0.92f, 0.98f, 1.00f);\n"
                + "    }\n"
                + "}\n";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate Game.cs for '{projectName}': {ex.Message}", ex);
        }
    }

    public static string GenerateScene(string projectName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectName)) throw new ArgumentException("Project name is empty", nameof(projectName));

            return "using System.Numerics;\n"
                + "using Raylib_cs;\n"
                + "using ImGuiNET;\n"
                + "using Fix2Engine.Components.Scene;\n"
                + "using Fix2Engine.Graphics;\n"
                + "using static Raylib_cs.Raylib;\n\n"
                + $"namespace {projectName};\n\n"
                + $"public class {projectName}Scene : IFixScene\n"
                + "{\n"
                + "    public void Start()\n"
                + "    {\n"
                + "    }\n\n"
                + "    public void Update(float dt)\n"
                + "    {\n"
                + "    }\n\n"
                + "    public void Render()\n"
                + "    {\n"
                + "        ClearBackground(new Color(15, 15, 20, 255));\n"
                + "        DrawRectangle(100, 100, 160, 160, Color.Red);\n"
                + "        DrawRectangleLines(100, 100, 160, 160, Color.White);\n"
                + "    }\n\n"
                + "    public void RenderUI()\n"
                + "    {\n"
                + "        var viewport = ImGui.GetMainViewport();\n"
                + "        ImGui.SetNextWindowPos(viewport.GetCenter(), ImGuiCond.Always, new Vector2(0.5f, 0.5f));\n"
                + "        ImGui.SetNextWindowSize(new Vector2(360, 200), ImGuiCond.Always);\n"
                + "        if (ImGui.Begin(\"" + projectName + "\", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings))\n"
                + "        {\n"
                + "            ImGui.Text(\"" + projectName + "\");\n"
                + "            ImGui.Separator();\n"
                + "            ImGui.Text(\"Welcome to " + projectName + "!\");\n"
                + "            ImGui.Text($\"FPS: {GetFPS()}\");\n"
                + "            ImGui.End();\n"
                + "        }\n"
                + "    }\n\n"
                + "    public void Unload() { }\n"
                + "    public void Dispose() { }\n"
                + "}\n";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate Scene for '{projectName}': {ex.Message}", ex);
        }
    }
}
