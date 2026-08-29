using Fix2Engine.Input.InputBackend;

namespace Fix2Engine.Input;
public static class InputManager
{
    private static readonly bool[] CurrentKeys = new bool[256];
    private static readonly bool[] PreviousKeys = new bool[256];

    public static void Update()
    {
        Array.Copy(
            CurrentKeys,
            PreviousKeys,
            CurrentKeys.Length
        );

        for (int i = 0; i < CurrentKeys.Length; i++)
        {
            CurrentKeys[i] =
                WindowsInputBackend.IsDown((Keys)i);
        }
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