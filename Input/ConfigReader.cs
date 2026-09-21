namespace Fix2Engine.Input;

public class ConfigReader
{
    public static InputConfig Load(string path) => InputConfig.Load(path);

    public static string Reading(string path)
    {
        var config = Load(path);
        return string.Join(Environment.NewLine,
            config.Actions.Select(action => $"{action.Key} = {string.Join(", ", action.Value)}"));
    }
}
