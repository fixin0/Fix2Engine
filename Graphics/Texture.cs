using Fix2Engine.Core;

namespace Fix2Engine.Graphics;

/// <summary>A GPU resource. Dispose before the game window closes; sprites may borrow it.</summary>
public sealed class Texture : IDisposable
{
    internal ITextureResource Resource { get; }
    public int Width => Resource.Width;
    public int Height => Resource.Height;
    public bool IsDisposed => Resource.IsDisposed;
    internal Texture(ITextureResource resource) => Resource = resource;
    public void Dispose() => Resource.Dispose();
}

public static class Content
{
    public static Texture LoadTexture(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new Texture(EngineBackend.Current.LoadTexture(path));
    }
}
