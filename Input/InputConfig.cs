using Fix2Engine.Input.InputBackend;
using Tomlyn;
using Tomlyn.Serialization;

namespace Fix2Engine.Input;

public class InputConfig
{
    public Dictionary<string, string[]> Actions { get; set; } = new();

    public static InputConfig InputConfigLoader(string path) => Load(path);

    public static InputConfig Load(string path)
    {
        try
        {
            var config = TomlSerializer.Deserialize(File.ReadAllText(path), InputTomlContext.Default.InputConfig)
                ?? throw new InvalidDataException("Input map is empty.");
            config.Compile();
            return config;
        }
        catch (Exception ex) when (ex is not IOException && ex is not UnauthorizedAccessException)
        {
            throw new InvalidDataException($"Invalid input map '{path}': {ex.Message}", ex);
        }
    }

    internal Dictionary<string, Keys[]> Compile()
    {
        if (Actions is null || Actions.Count == 0)
            throw new InvalidDataException("Input map must contain an [Actions] table with at least one action.");

        var result = new Dictionary<string, Keys[]>(StringComparer.Ordinal);
        foreach (var (action, names) in Actions)
        {
            if (string.IsNullOrWhiteSpace(action) || names is null || names.Length == 0)
                throw new InvalidDataException($"Action '{action}' must have a name and at least one key.");

            var keys = new List<Keys>();
            foreach (var name in names)
            {
                // Enum.TryParse also accepts numbers and comma-separated values; only named keys are valid.
                if (string.IsNullOrWhiteSpace(name) ||
                    !Enum.GetNames<Keys>().Contains(name, StringComparer.OrdinalIgnoreCase) ||
                    !Enum.TryParse<Keys>(name, true, out var key) || key == Keys.None)
                    throw new InvalidDataException($"Unknown key '{name}' for action '{action}'. Use a Keys enum name, for example Space or LeftShift.");
                keys.Add(key);
            }
            result.Add(action, keys.Distinct().ToArray());
        }
        return result;
    }
}

[TomlSerializable(typeof(InputConfig))]
internal partial class InputTomlContext : TomlSerializerContext { }
