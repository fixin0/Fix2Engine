using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Fix2Console;

public static class ProjectValidator
{
    public static bool IsValidProjectName(string name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            if (name.Length > 64) return false;
            if (!Regex.IsMatch(name, @"^[A-Za-z_][A-Za-z0-9_]*$")) return false;

            string[] reserved = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "LPT1" };
            foreach (var r in reserved)
            {
                if (string.Equals(name, r, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (name.Contains(Path.DirectorySeparatorChar) || name.Contains(Path.AltDirectorySeparatorChar))
                return false;

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Validation error for '{name}': {ex.Message}");
            return false;
        }
    }
}
