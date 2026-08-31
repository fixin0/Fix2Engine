using Tomlyn;

namespace Fix2Engine.Input;

public class InputConfig
{
    public Dictionary<string, string[]> Actions { get; set; } = new();

    public static InputConfig InputConfigLoader(string path)
    {
        return TomlSerializer.Deserialize<InputConfig>(File.ReadAllText(path));

        
    }
    
    
}

