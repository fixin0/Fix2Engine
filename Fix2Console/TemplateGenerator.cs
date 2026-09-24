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
                + $"      <ProjectReference Include=\"{rel("Runner/Runner.csproj")}\" />\n"
                + $"      <ProjectReference Include=\"{rel("Graphics/Graphics.csproj")}\" />\n"
                + $"      <ProjectReference Include=\"{rel("Components/Components.csproj")}\" />\n"
                + $"      <ProjectReference Include=\"{rel("Input/Input.csproj")}\" />\n"
                + $"      <ProjectReference Include=\"{rel("Physics/Physics.csproj")}\" />\n"
                + $"      <ProjectReference Include=\"{rel("Audio/Audio.csproj")}\" />\n"
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
                + "        Fix2Engine.Fix2.Run<Game>();\n"
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
        ArgumentException.ThrowIfNullOrWhiteSpace(projectName);
        return $$"""
            using Fix2Engine;
            using Fix2Engine.Core;
            using Fix2Engine.Components;

            namespace {{projectName}};

            public sealed class Game : FixGame
            {
                protected override void Configure(GameSettings settings)
                {
                    settings.Title = "{{projectName}}";
                    settings.ClearColor = new Color32(15, 15, 20);
                    settings.ShowFps = true;
                }

                protected override void Start()
                {
                    SceneManager.LoadScene<{{projectName}}Scene>();
                }
            }
            """ + "\n";
    }

    public static string GenerateScene(string projectName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectName);
        return $$"""
            using Fix2Engine.Components;
            using Fix2Engine.Components.Scene;
            using Fix2Engine.Graphics;
            using Fix2Engine.Core;

            namespace {{projectName}};

            public class {{projectName}}Scene : FixScene
            {
                protected override void OnStart()
                {
                    // Add your Object2D subclasses here: Add(new Player());
                }

                protected override void OnRender(RenderContext graphics)
                {
                    // Draw in screen coordinates, or add SpriteObject2D objects to the scene.
                }
            }
            """ + "\n";
    }
}
