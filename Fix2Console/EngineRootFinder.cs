using System;
using System.IO;

namespace Fix2Console;

public static class EngineRootFinder
{
    public static string? FindEngineRoot()
    {
        try
        {
            string? dir = AppContext.BaseDirectory;
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (dir == null) break;
                    string candidate = Path.Combine(dir, "Fix2Engine.sln");
                    if (File.Exists(candidate))
                    {
                        var full = Path.GetFullPath(dir);
                        if (Directory.Exists(full) && File.Exists(Path.Combine(full, "Fix2Engine.sln")))
                            return full;
                    }
                    dir = Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Error checking directory '{dir}': {ex.Message}");
                    try { dir = Path.GetDirectoryName(dir!.TrimEnd(Path.DirectorySeparatorChar)); } catch { break; }
                }
            }

            string? alt = null;
            try
            {
                alt = Path.GetDirectoryName(typeof(EngineRootFinder).Assembly.Location);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not get assembly location: {ex.Message}");
            }

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    if (alt == null) break;
                    string candidate = Path.Combine(alt, "Fix2Engine.sln");
                    if (File.Exists(candidate))
                    {
                        var full = Path.GetFullPath(alt);
                        if (Directory.Exists(full))
                            return full;
                    }
                    alt = Path.GetDirectoryName(alt.TrimEnd(Path.DirectorySeparatorChar));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Error checking alt directory '{alt}': {ex.Message}");
                    try { alt = Path.GetDirectoryName(alt!.TrimEnd(Path.DirectorySeparatorChar)); } catch { break; }
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding engine root: {ex.Message}");
            return null;
        }
    }
}
