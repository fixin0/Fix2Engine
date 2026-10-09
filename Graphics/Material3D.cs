using System.Numerics;
using System.Text.Json;
using Fix2Engine.Core;

namespace Fix2Engine.Graphics;

/// <summary>Shader settings and texture bindings shared by 3D render components. It is not tied to one lighting model.</summary>
public sealed class Material3D : IDisposable
{
    private readonly Dictionary<string, object> _parameters = new(StringComparer.Ordinal);
    private readonly Dictionary<MaterialTextureSlot, Texture> _maps = new();
    private readonly Dictionary<string, Texture> _textures = new(StringComparer.Ordinal);
    private readonly HashSet<Texture> _ownedTextures = new();
    private readonly bool _ownsShader;
    private bool _disposed;

    public ShaderProgram Shader { get; }
    public Color32 Tint { get; set; } = Color32.White;
    public bool IsDisposed => _disposed;

    public Material3D(ShaderProgram shader, bool ownsShader = false)
    {
        Shader = shader ?? throw new ArgumentNullException(nameof(shader));
        _ownsShader = ownsShader;
    }

    public static Material3D FromPreset(ShaderSet set) => new(ShaderProgram.FromPreset(set), ownsShader: true);

    /// <summary>Loads a JSON material descriptor. Paths inside it are relative to its own directory.</summary>
    public static Material3D Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        var directory = Path.GetDirectoryName(Path.GetFullPath(path)) ?? Environment.CurrentDirectory;
        ShaderProgram shader;
        if (root.TryGetProperty("preset", out var presetNode))
        {
            if (!Enum.TryParse<ShaderSet>(presetNode.GetString(), true, out var preset))
                throw new FormatException("Material preset must be Realistic, Toon, Water, or Unlit.");
            shader = ShaderProgram.FromPreset(preset);
        }
        else
        {
            var vertex = Resolve(directory, root.GetProperty("vertex").GetString());
            var fragment = Resolve(directory, root.GetProperty("fragment").GetString());
            shader = ShaderProgram.FromFiles(vertex, fragment);
        }
        var material = new Material3D(shader, ownsShader: true);
        if (root.TryGetProperty("parameters", out var parameters))
            foreach (var property in parameters.EnumerateObject()) material.ReadParameter(property);
        if (root.TryGetProperty("maps", out var maps))
            foreach (var property in maps.EnumerateObject())
            {
                if (!Enum.TryParse<MaterialTextureSlot>(property.Name, true, out var slot))
                    throw new FormatException($"Unknown material texture slot '{property.Name}'.");
                material.SetMap(slot, Content.LoadTexture(Resolve(directory, property.Value.GetString())), ownsTexture: true);
            }
        if (root.TryGetProperty("textures", out var textures))
            foreach (var property in textures.EnumerateObject())
                material.SetTexture(property.Name, Content.LoadTexture(Resolve(directory, property.Value.GetString())), ownsTexture: true);
        return material;
    }

    public Material3D SetFloat(string name, float value) { Set(name, value); return this; }
    public Material3D SetVector2(string name, Vector2 value) { Set(name, value); return this; }
    public Material3D SetVector3(string name, Vector3 value) { Set(name, value); return this; }
    public Material3D SetVector4(string name, Vector4 value) { Set(name, value); return this; }
    public Material3D SetMap(MaterialTextureSlot slot, Texture texture, bool ownsTexture = false)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(texture);
        ObjectDisposedException.ThrowIf(texture.IsDisposed, texture);
        ReleaseOwned(_maps.TryGetValue(slot, out var previous) ? previous : null);
        _maps[slot] = texture;
        if (ownsTexture) _ownedTextures.Add(texture);
        return this;
    }
    public Material3D SetTexture(string uniformName, Texture texture, bool ownsTexture = false)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(uniformName);
        ArgumentNullException.ThrowIfNull(texture);
        ObjectDisposedException.ThrowIf(texture.IsDisposed, texture);
        ReleaseOwned(_textures.TryGetValue(uniformName, out var previous) ? previous : null);
        _textures[uniformName] = texture;
        if (ownsTexture) _ownedTextures.Add(texture);
        return this;
    }

    internal void Apply(Model3D model)
    {
        ThrowIfDisposed();
        ObjectDisposedException.ThrowIf(Shader.IsDisposed, Shader);
        EngineBackend.Current.SetModelShader3D(model.Resource, Shader.Resource);
        foreach (var (slot, texture) in _maps)
        {
            EngineBackend.Current.SetModelTexture3D(model.Resource, slot, texture.Resource);
            ApplyMap(slot, texture);
        }
        foreach (var (name, texture) in _textures) Shader.SetTexture(name, texture);
        Shader.SetFloat("time", Graphics3D.Time);
        Shader.SetVector3("lightDirection", Graphics3D.LightDirection);
        Shader.SetVector3("lightColor", Graphics3D.LightColor);
        Shader.SetVector3("ambientColor", Graphics3D.AmbientColor);
        Shader.SetVector3("viewPos", Graphics3D.ActiveCameraPosition);
        foreach (var (name, value) in _parameters) ApplyParameter(name, value);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var texture in _ownedTextures) texture.Dispose();
        _ownedTextures.Clear();
        if (_ownsShader) Shader.Dispose();
        GC.SuppressFinalize(this);
    }

    private void Set(string name, object value)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _parameters[name] = value;
    }
    private void ApplyParameter(string name, object value)
    {
        switch (value)
        {
            case float number: Shader.SetFloat(name, number); break;
            case Vector2 vector2: Shader.SetVector2(name, vector2); break;
            case Vector3 vector3: Shader.SetVector3(name, vector3); break;
            case Vector4 vector4: Shader.SetVector4(name, vector4); break;
            default: throw new InvalidOperationException($"Unsupported shader parameter '{name}'.");
        }
    }
    private void ApplyMap(MaterialTextureSlot slot, Texture texture)
    {
        var (sampler, availability) = slot switch
        {
            MaterialTextureSlot.Albedo => ("albedoMap", "hasAlbedoMap"),
            MaterialTextureSlot.Normal => ("normalMap", "hasNormalMap"),
            MaterialTextureSlot.Metallic => ("metallicMap", "hasMetallicMap"),
            MaterialTextureSlot.Roughness => ("roughnessMap", "hasRoughnessMap"),
            MaterialTextureSlot.Occlusion => ("occlusionMap", "hasOcclusionMap"),
            MaterialTextureSlot.Emission => ("emissionMap", "hasEmissionMap"),
            _ => (string.Empty, string.Empty)
        };
        if (sampler.Length == 0) return;
        Shader.SetTexture(sampler, texture);
        Shader.SetFloat(availability, 1f);
    }
    private void ReadParameter(JsonProperty property)
    {
        if (property.Value.ValueKind == JsonValueKind.Number) { SetFloat(property.Name, property.Value.GetSingle()); return; }
        if (property.Value.ValueKind != JsonValueKind.Array) throw new FormatException($"Parameter '{property.Name}' must be a number or vector array.");
        var values = property.Value.EnumerateArray().Select(value => value.GetSingle()).ToArray();
        switch (values.Length)
        {
            case 2: SetVector2(property.Name, new(values[0], values[1])); break;
            case 3: SetVector3(property.Name, new(values[0], values[1], values[2])); break;
            case 4: SetVector4(property.Name, new(values[0], values[1], values[2], values[3])); break;
            default: throw new FormatException($"Vector parameter '{property.Name}' must have 2, 3, or 4 values.");
        }
    }
    private void ReleaseOwned(Texture? texture)
    {
        if (texture != null && _ownedTextures.Remove(texture)) texture.Dispose();
    }
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
    private static string Resolve(string directory, string? path) => string.IsNullOrWhiteSpace(path)
        ? throw new FormatException("Shader and texture paths must be strings.")
        : Path.GetFullPath(Path.Combine(directory, path));
}
