using System.Reflection;
using System.Numerics;
using Fix2Engine.Core;

namespace Fix2Engine.Graphics;

public enum ShaderSet { Realistic, Toon, Water, Unlit }

/// <summary>A compiled GLSL shader. Use a ready-made <see cref="ShaderSet"/> or load vertex/fragment paths directly.</summary>
public sealed class ShaderProgram : IDisposable
{
    internal IShaderResource Resource { get; }
    public bool IsDisposed => Resource.IsDisposed;
    private ShaderProgram(IShaderResource resource) => Resource = resource;

    public static ShaderProgram FromFiles(string vertexPath, string fragmentPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vertexPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fragmentPath);
        return new ShaderProgram(EngineBackend.Current.LoadShader3D(vertexPath, fragmentPath));
    }

    public static ShaderProgram FromPreset(ShaderSet set)
    {
        var vertex = set == ShaderSet.Water ? ReadAsset("Water.vert") : ReadAsset("Standard.vert");
        var fragment = ReadAsset($"{set}.frag");
        return new ShaderProgram(EngineBackend.Current.LoadShader3DFromSource(vertex, fragment));
    }

    public void SetFloat(string name, float value) => EngineBackend.Current.SetShaderFloat(Resource, name, value);
    public void SetVector2(string name, Vector2 value) => EngineBackend.Current.SetShaderVector2(Resource, name, value);
    public void SetVector3(string name, Vector3 value) => EngineBackend.Current.SetShaderVector3(Resource, name, value);
    public void SetVector4(string name, Vector4 value) => EngineBackend.Current.SetShaderVector4(Resource, name, value);
    public void SetTexture(string name, Texture value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ObjectDisposedException.ThrowIf(value.IsDisposed, value);
        EngineBackend.Current.SetShaderTexture(Resource, name, value.Resource);
    }

    public void Dispose() => Resource.Dispose();

    private static string ReadAsset(string fileName)
    {
        var assembly = typeof(ShaderProgram).Assembly;
        var name = assembly.GetManifestResourceNames().SingleOrDefault(name => name.EndsWith($"Shaders.{fileName}", StringComparison.Ordinal));
        if (name == null) throw new InvalidOperationException($"Built-in shader asset '{fileName}' was not embedded.");
        using var stream = assembly.GetManifestResourceStream(name) ?? throw new InvalidOperationException($"Could not read shader asset '{fileName}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
