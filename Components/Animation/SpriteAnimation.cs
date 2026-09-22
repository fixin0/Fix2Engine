using Raylib_cs;

namespace Fix2Engine.Components.Animation;

/// <summary>Immutable, reusable sprite-sheet frame data; it owns no texture.</summary>
public sealed class SpriteAnimation
{
    public string Name { get; }
    public IReadOnlyList<Rectangle> Frames { get; }
    public float FramesPerSecond { get; }
    public bool Loop { get; }
    public double Duration => Frames.Count / (double)FramesPerSecond;

    public SpriteAnimation(string name, IEnumerable<Rectangle> frames, float framesPerSecond = 12, bool loop = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(frames);
        if (!float.IsFinite(framesPerSecond) || framesPerSecond <= 0)
            throw new ArgumentOutOfRangeException(nameof(framesPerSecond));
        var copy = frames.ToArray();
        if (copy.Length == 0) throw new ArgumentException("An animation needs at least one frame.", nameof(frames));
        if (copy.Any(frame => !float.IsFinite(frame.X) || !float.IsFinite(frame.Y) ||
            !float.IsFinite(frame.Width) || !float.IsFinite(frame.Height) ||
            frame.X < 0 || frame.Y < 0 || frame.Width <= 0 || frame.Height <= 0))
            throw new ArgumentException("Frames must have finite non-negative coordinates and positive sizes.", nameof(frames));
        Name = name;
        Frames = Array.AsReadOnly(copy);
        FramesPerSecond = framesPerSecond;
        Loop = loop;
    }

    public static SpriteAnimation FromGrid(string name, int frameWidth, int frameHeight,
        int frameCount, int columns, float framesPerSecond = 12, bool loop = true, int startFrame = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameHeight);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frameCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);
        ArgumentOutOfRangeException.ThrowIfNegative(startFrame);
        var frames = new Rectangle[frameCount];
        for (int i = 0; i < frameCount; i++)
        {
            long frame = (long)startFrame + i;
            frames[i] = new Rectangle((frame % columns) * (float)frameWidth,
                (frame / columns) * (float)frameHeight, frameWidth, frameHeight);
        }
        return new SpriteAnimation(name, frames, framesPerSecond, loop);
    }
}
