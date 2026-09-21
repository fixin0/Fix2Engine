using System;
using System.IO;

namespace Fix2Console;

public static class ProjectGenerator
{
    public static void CreateNewProject(string rawName)
    {
        try
        {
            string projectName = rawName.Trim();

            if (!ProjectValidator.IsValidProjectName(projectName))
            {
                Console.WriteLine($"Error: Invalid project name '{projectName}'.");
                Console.WriteLine("  - Must start with a letter or underscore");
                Console.WriteLine("  - Only letters, digits and underscore allowed");
                return;
            }

            if (projectName.Length > 64)
            {
                Console.WriteLine($"Error: Project name too long (max 64 chars): '{projectName}'");
                return;
            }

            string targetDir = Path.Combine(Environment.CurrentDirectory, projectName);

            try
            {
                if (Directory.Exists(targetDir))
                {
                    Console.WriteLine($"Error: Directory already exists: {targetDir}");
                    return;
                }

                if (File.Exists(targetDir))
                {
                    Console.WriteLine($"Error: A file with the same name already exists: {targetDir}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking target path: {ex.Message}");
                return;
            }

            string? engineRoot = null;
            try
            {
                engineRoot = EngineRootFinder.FindEngineRoot();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error locating engine root: {ex.Message}");
                return;
            }

            if (engineRoot == null)
            {
                Console.WriteLine("Error: Could not locate Fix2Engine root (Fix2Engine.sln not found).");
                Console.WriteLine("  Make sure Fix2Console is built from the Fix2Engine solution.");
                return;
            }

            try
            {
                if (!Directory.Exists(Path.Combine(engineRoot, "Graphics")))
                {
                    Console.WriteLine($"Error: Engine root is incomplete, missing Graphics at: {engineRoot}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating engine root: {ex.Message}");
                return;
            }

            Console.WriteLine($"Creating project '{projectName}' at {targetDir} ...");

            try
            {
                Directory.CreateDirectory(targetDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating directory '{targetDir}': {ex.Message}");
                return;
            }

            string csprojPath = Path.Combine(targetDir, $"{projectName}.csproj");
            string programPath = Path.Combine(targetDir, "Program.cs");
            string gamePath = Path.Combine(targetDir, "Game.cs");
            string scenePath = Path.Combine(targetDir, $"{projectName}Scene.cs");

            try
            {
                File.WriteAllText(csprojPath, TemplateGenerator.GenerateCsproj(projectName, targetDir, engineRoot));
                VerifyFile(csprojPath);
                Console.WriteLine($"  Created {Path.GetFileName(csprojPath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating {csprojPath}: {ex.Message}");
                CleanupOnFailure(targetDir);
                return;
            }

            try
            {
                File.WriteAllText(programPath, TemplateGenerator.GenerateProgram(projectName));
                VerifyFile(programPath);
                Console.WriteLine($"  Created Program.cs");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Program.cs: {ex.Message}");
                CleanupOnFailure(targetDir);
                return;
            }

            try
            {
                File.WriteAllText(gamePath, TemplateGenerator.GenerateGame(projectName));
                VerifyFile(gamePath);
                Console.WriteLine($"  Created Game.cs");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Game.cs: {ex.Message}");
                CleanupOnFailure(targetDir);
                return;
            }

            try
            {
                File.WriteAllText(scenePath, TemplateGenerator.GenerateScene(projectName));
                VerifyFile(scenePath);
                Console.WriteLine($"  Created {projectName}Scene.cs");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating {projectName}Scene.cs: {ex.Message}");
                CleanupOnFailure(targetDir);
                return;
            }

            try
            {
                ProjectSettings.CreateFiles(targetDir, engineRoot);
                VerifyProjectIntegrity(targetDir, projectName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error completing project setup: {ex.Message}");
                CleanupOnFailure(targetDir);
                Environment.ExitCode = 1;
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"Project '{projectName}' created successfully.");
            Console.WriteLine($"  cd {projectName} && dotnet run");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error creating project '{rawName}': {ex.Message}");
#if DEBUG
            Console.WriteLine(ex.ToString());
#endif
        }
    }

    private static void VerifyFile(string path)
    {
        try
        {
            if (!File.Exists(path))
                throw new IOException($"File was not created: {path}");

            var info = new FileInfo(path);
            if (info.Length == 0)
                throw new IOException($"File is empty: {path}");
        }
        catch (Exception ex) when (ex is IOException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to verify file '{path}': {ex.Message}", ex);
        }
    }

    private static void VerifyProjectIntegrity(string targetDir, string projectName)
    {
        string[] required = {
            Path.Combine(targetDir, $"{projectName}.csproj"),
            Path.Combine(targetDir, "Program.cs"),
            Path.Combine(targetDir, "Game.cs"),
            Path.Combine(targetDir, $"{projectName}Scene.cs"),
            Path.Combine(targetDir, "InputMap.toml"),
            Path.Combine(targetDir, "Fix2Engine.toml")
        };

        foreach (var file in required)
        {
            if (!File.Exists(file))
                throw new FileNotFoundException($"Missing required file: {file}");

            string content = File.ReadAllText(file);
            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidDataException($"File is empty: {file}");

            if (!content.Contains(projectName) && file.EndsWith(".cs"))
                Console.WriteLine($"Warning: {Path.GetFileName(file)} does not contain project name");
        }

        string csproj = File.ReadAllText(required[0]);
        string[] requiredRefs = { "Graphics", "Components", "Input" };
        foreach (var r in requiredRefs)
        {
            if (!csproj.Contains(r))
                Console.WriteLine($"Warning: csproj missing reference: {r}");
        }
    }

    private static void CleanupOnFailure(string targetDir)
    {
        try
        {
            Console.WriteLine("  Cleaning up incomplete project...");

            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    if (Directory.Exists(targetDir))
                    {
                        foreach (var file in Directory.GetFiles(targetDir))
                        {
                            try { File.Delete(file); } catch { }
                        }
                        Directory.Delete(targetDir, true);
                    }
                    break;
                }
                catch when (attempt < 2)
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  Warning: Cleanup failed: {ex.Message}");
            Console.WriteLine($"  Please manually remove: {targetDir}");
        }
    }
}
