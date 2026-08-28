using System.Runtime.InteropServices;

namespace Fix2Engine.Input.InputBackend;

public static class InputBackend_Windows
{
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
    public static bool IsDown(Keys key)
    {
        return (GetAsyncKeyState((int)key) & 0x8000) != 0;
    }
    
    
    
    
    
    
    
}