using System.Runtime.InteropServices;
using Raylib_cs;
namespace Fix2Engine.Backends.Raylib;

public sealed partial class RaylibBackend
{
    private static readonly Dictionary<int, KeyboardKey> VkToKey = new()
    {
        { 0x08, KeyboardKey.Back },           // Backspace
        { 0x09, KeyboardKey.Tab },            // Tab
        { 0x0D, KeyboardKey.Enter },          // Enter
        { 0x1B, KeyboardKey.Escape },         // Escape
        { 0x20, KeyboardKey.Space },          // Space
        { 0x21, KeyboardKey.PageUp },         // PageUp
        { 0x22, KeyboardKey.PageDown },       // PageDown
        { 0x23, KeyboardKey.End },            // End
        { 0x24, KeyboardKey.Home },           // Home
        { 0x25, KeyboardKey.Left },           // Left
        { 0x26, KeyboardKey.Up },             // Up
        { 0x27, KeyboardKey.Right },          // Right
        { 0x28, KeyboardKey.Down },           // Down
        { 0x2C, KeyboardKey.PrintScreen },    // PrintScreen
        { 0x2D, KeyboardKey.Insert },         // Insert
        { 0x2E, KeyboardKey.Delete },         // Delete
        { 0x30, KeyboardKey.Zero },           // 0
        { 0x31, KeyboardKey.One },            // 1
        { 0x32, KeyboardKey.Two },            // 2
        { 0x33, KeyboardKey.Three },          // 3
        { 0x34, KeyboardKey.Four },           // 4
        { 0x35, KeyboardKey.Five },           // 5
        { 0x36, KeyboardKey.Six },            // 6
        { 0x37, KeyboardKey.Seven },          // 7
        { 0x38, KeyboardKey.Eight },          // 8
        { 0x39, KeyboardKey.Nine },           // 9
        { 0x41, KeyboardKey.A },              // A
        { 0x42, KeyboardKey.B },              // B
        { 0x43, KeyboardKey.C },              // C
        { 0x44, KeyboardKey.D },              // D
        { 0x45, KeyboardKey.E },              // E
        { 0x46, KeyboardKey.F },              // F
        { 0x47, KeyboardKey.G },              // G
        { 0x48, KeyboardKey.H },              // H
        { 0x49, KeyboardKey.I },              // I
        { 0x4A, KeyboardKey.J },              // J
        { 0x4B, KeyboardKey.K },              // K
        { 0x4C, KeyboardKey.L },              // L
        { 0x4D, KeyboardKey.M },              // M
        { 0x4E, KeyboardKey.N },              // N
        { 0x4F, KeyboardKey.O },              // O
        { 0x50, KeyboardKey.P },              // P
        { 0x51, KeyboardKey.Q },              // Q
        { 0x52, KeyboardKey.R },              // R
        { 0x53, KeyboardKey.S },              // S
        { 0x54, KeyboardKey.T },              // T
        { 0x55, KeyboardKey.U },              // U
        { 0x56, KeyboardKey.V },              // V
        { 0x57, KeyboardKey.W },              // W
        { 0x58, KeyboardKey.X },              // X
        { 0x59, KeyboardKey.Y },              // Y
        { 0x5A, KeyboardKey.Z },              // Z
        { 0x60, KeyboardKey.Kp0 },            // Numpad0
        { 0x61, KeyboardKey.Kp1 },            // Numpad1
        { 0x62, KeyboardKey.Kp2 },            // Numpad2
        { 0x63, KeyboardKey.Kp3 },            // Numpad3
        { 0x64, KeyboardKey.Kp4 },            // Numpad4
        { 0x65, KeyboardKey.Kp5 },            // Numpad5
        { 0x66, KeyboardKey.Kp6 },            // Numpad6
        { 0x67, KeyboardKey.Kp7 },            // Numpad7
        { 0x68, KeyboardKey.Kp8 },            // Numpad8
        { 0x69, KeyboardKey.Kp9 },            // Numpad9
        { 0x6A, KeyboardKey.KpMultiply },     // Multiply
        { 0x6B, KeyboardKey.KpAdd },          // Add
        { 0x6D, KeyboardKey.KpSubtract },     // Subtract
        { 0x6E, KeyboardKey.KpDecimal },      // Decimal
        { 0x6F, KeyboardKey.KpDivide },       // Divide
        { 0x70, KeyboardKey.F1 },             // F1
        { 0x71, KeyboardKey.F2 },             // F2
        { 0x72, KeyboardKey.F3 },             // F3
        { 0x73, KeyboardKey.F4 },             // F4
        { 0x74, KeyboardKey.F5 },             // F5
        { 0x75, KeyboardKey.F6 },             // F6
        { 0x76, KeyboardKey.F7 },             // F7
        { 0x77, KeyboardKey.F8 },             // F8
        { 0x78, KeyboardKey.F9 },             // F9
        { 0x79, KeyboardKey.F10 },            // F10
        { 0x7A, KeyboardKey.F11 },            // F11
        { 0x7B, KeyboardKey.F12 },            // F12
        { 0x90, KeyboardKey.NumLock },        // NumLock
        { 0x91, KeyboardKey.ScrollLock },     // ScrollLock
        { 0xA0, KeyboardKey.LeftShift },      // LeftShift
        { 0xA1, KeyboardKey.RightShift },     // RightShift
        { 0xA2, KeyboardKey.LeftControl },    // LeftControl
        { 0xA3, KeyboardKey.RightControl },   // RightControl
        { 0xA4, KeyboardKey.LeftAlt },        // LeftAlt
        { 0xA5, KeyboardKey.RightAlt },       // RightAlt
        { 0xBA, KeyboardKey.Semicolon },      // Semicolon
        { 0xBB, KeyboardKey.Equal },          // Equal
        { 0xBC, KeyboardKey.Comma },          // Comma
        { 0xBD, KeyboardKey.Minus },          // Minus
        { 0xBE, KeyboardKey.Period },         // Period
        { 0xBF, KeyboardKey.Slash },          // Slash
        { 0xC0, KeyboardKey.Grave },          // Grave
        { 0xDB, KeyboardKey.LeftBracket },    // LeftBracket
        { 0xDC, KeyboardKey.Backslash },      // Backslash
        { 0xDD, KeyboardKey.RightBracket },   // RightBracket
        { 0xDE, KeyboardKey.Apostrophe },     // Apostrophe
    };


    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int key);
    public bool IsKeyDown(int virtualKey)
    {
        if (OperatingSystem.IsWindows()) return (GetAsyncKeyState(virtualKey) & 0x8000) != 0;
        return VkToKey.TryGetValue(virtualKey, out var key) && Raylib_cs.Raylib.IsKeyDown(key);
    }
}
