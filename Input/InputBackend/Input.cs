using Fix2Engine.Core;
using Fix2Engine.Input.InputBackend;

namespace Fix2Engine.Input;
public static class InputManager
{
    private static readonly bool[] CurrentKeys = new bool[256];
    private static readonly bool[] PreviousKeys = new bool[256];

    private static Dictionary<string, Keys[]> _actions = new(StringComparer.Ordinal);

    public static string? InputMapPath { get; private set; }

    /// <summary>Loads InputMap.toml from the executable directory, or an explicit path.
    /// Invalid reloads leave the current map intact. Action names are case-sensitive.</summary>
    public static void LoadInputMap(string? path = null)
    {
        string fullPath = Path.GetFullPath(path ?? Path.Combine(AppContext.BaseDirectory, "InputMap.toml"));
        var actions = InputConfig.Load(fullPath).Compile();
        _actions = actions;
        InputMapPath = fullPath;
    }

    public static bool HasAction(string action) => _actions.ContainsKey(action);

    public static bool IsDown(string action) => AnyDown(GetAction(action), CurrentKeys);

    public static bool IsPressed(string action)
    {
        var keys = GetAction(action);
        return AnyDown(keys, CurrentKeys) && !AnyDown(keys, PreviousKeys);
    }

    public static bool IsReleased(string action)
    {
        var keys = GetAction(action);
        return !AnyDown(keys, CurrentKeys) && AnyDown(keys, PreviousKeys);
    }

    private static Keys[] GetAction(string action) => _actions.TryGetValue(action, out var keys)
        ? keys
        : throw new KeyNotFoundException($"Input action '{action}' is not defined. Load InputMap.toml and check the action name.");

    private static bool AnyDown(Keys[] keys, bool[] state)
    {
        foreach (var key in keys)
            if (state[(int)key]) return true;
        return false;
    }

    public static void Update()
    {
        Array.Copy(CurrentKeys, PreviousKeys, CurrentKeys.Length);
        for (int i = 0; i < CurrentKeys.Length; i++)
            CurrentKeys[i] = EngineBackend.Current.IsKeyDown(i);
    }

    public static bool IsDown(Keys key)
    {
        return CurrentKeys[(int)key];
    }

    public static bool IsPressed(Keys key)
    {
        return
            CurrentKeys[(int)key] &&
            !PreviousKeys[(int)key];
    }

    public static bool IsReleased(Keys key)
    {
        return
            !CurrentKeys[(int)key] &&
            PreviousKeys[(int)key];
    }
}
