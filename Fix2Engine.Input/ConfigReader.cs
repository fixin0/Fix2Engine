using System;
using Tomlyn;

namespace Fix2Engine.Input;

public class ConfigReader
{

    public static string Reading(string path)
    {
        string text = File.ReadAllText(path);
        InputConfig config = TomlSerializer.Deserialize<InputConfig>(text);
        return config.ToString();
    }
    
}