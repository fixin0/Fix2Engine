namespace Fix2Engine.Core;

public readonly record struct Color32(byte R, byte G, byte B, byte A = 255)
{
    public static Color32 White => new(255, 255, 255);
    public static Color32 Black => new(0, 0, 0);
    public static Color32 Red => new(230, 41, 55);
    public static Color32 Blue => new(0, 121, 241);
    public static Color32 Transparent => new(0, 0, 0, 0);
    public static Color32 FromRgb(byte r, byte g, byte b) => new(r, g, b);
}

public record struct RectF(float X, float Y, float Width, float Height);
