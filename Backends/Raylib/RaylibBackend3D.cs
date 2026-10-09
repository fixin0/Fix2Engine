using System.Numerics;
using Fix2Engine.Core;
using Raylib_cs;
using Native = Raylib_cs.Raylib;

namespace Fix2Engine.Backends.Raylib;

/// <summary>Raylib implementation of the engine's opaque 3D resource contracts.</summary>
public sealed partial class RaylibBackend
{
    private readonly HashSet<Model3DResource> _models3D = new();
    private readonly HashSet<ShaderResource> _shaders3D = new();

    public IModel3DResource LoadModel3D(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Ensure3DReady();
        var model = Native.LoadModel(path);
        if (model.MeshCount <= 0) throw new InvalidOperationException($"Could not load 3D model '{path}'.");
        return TrackModel(model);
    }

    public IModel3DResource CreatePrimitive3D(Primitive3D primitive, Vector3 dimensions, int slices, int rings)
    {
        Ensure3DReady();
        if (!float.IsFinite(dimensions.X) || !float.IsFinite(dimensions.Y) || !float.IsFinite(dimensions.Z) ||
            dimensions.X <= 0 || dimensions.Y <= 0 || dimensions.Z <= 0)
            throw new ArgumentOutOfRangeException(nameof(dimensions));
        if (slices < 3) throw new ArgumentOutOfRangeException(nameof(slices));
        if (rings < 2) throw new ArgumentOutOfRangeException(nameof(rings));

        var mesh = primitive switch
        {
            Primitive3D.Cube => Native.GenMeshCube(dimensions.X, dimensions.Y, dimensions.Z),
            Primitive3D.Plane => Native.GenMeshPlane(dimensions.X, dimensions.Z, slices, rings),
            Primitive3D.Sphere => Native.GenMeshSphere(dimensions.X / 2f, slices, rings),
            Primitive3D.Cylinder => Native.GenMeshCylinder(dimensions.X / 2f, dimensions.Y, slices),
            _ => throw new ArgumentOutOfRangeException(nameof(primitive))
        };
        return TrackModel(Native.LoadModelFromMesh(mesh));
    }

    public IShaderResource LoadShader3D(string vertexPath, string fragmentPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vertexPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fragmentPath);
        Ensure3DReady();
        var shader = Native.LoadShader(vertexPath, fragmentPath);
        if (shader.Id == 0) throw new InvalidOperationException($"Could not load shader '{vertexPath}' / '{fragmentPath}'.");
        return TrackShader(shader);
    }

    public IShaderResource LoadShader3DFromSource(string vertexSource, string fragmentSource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(vertexSource);
        ArgumentException.ThrowIfNullOrWhiteSpace(fragmentSource);
        Ensure3DReady();
        var shader = Native.LoadShaderFromMemory(vertexSource, fragmentSource);
        if (shader.Id == 0) throw new InvalidOperationException("Could not compile the supplied 3D shader source.");
        return TrackShader(shader);
    }

    public IDisposable Begin3D(Camera3DState camera)
    {
        Ensure3DReady();
        if (!float.IsFinite(camera.FieldOfView) || camera.FieldOfView <= 0)
            throw new ArgumentOutOfRangeException(nameof(camera));
        if (camera.Position == camera.Target) throw new ArgumentException("Camera Position and Target must differ.", nameof(camera));
        Native.BeginMode3D(new Camera3D
        {
            Position = camera.Position,
            Target = camera.Target,
            Up = camera.Up,
            FovY = camera.FieldOfView,
            Projection = camera.Projection == Projection3D.Orthographic ? CameraProjection.Orthographic : CameraProjection.Perspective
        });
        return new Mode3DScope();
    }

    public void DrawModel3D(IModel3DResource model, Matrix4x4 transform, Color32 tint)
    {
        var resource = GetModel(model);
        var previous = resource.NativeModel.Transform;
        resource.NativeModel.Transform = transform;
        Native.DrawModel(resource.NativeModel, Vector3.Zero, 1f, Convert(tint));
        resource.NativeModel.Transform = previous;
    }

    public unsafe void SetModelShader3D(IModel3DResource model, IShaderResource shader)
    {
        var modelResource = GetModel(model);
        var shaderResource = GetShader(shader);
        for (var i = 0; i < modelResource.NativeModel.MaterialCount; i++)
            modelResource.NativeModel.Materials[i].Shader = shaderResource.NativeShader;
    }

    public unsafe void SetModelTexture3D(IModel3DResource model, MaterialTextureSlot slot, ITextureResource texture)
    {
        var modelResource = GetModel(model);
        var textureResource = GetTexture(texture);
        var nativeSlot = slot switch
        {
            MaterialTextureSlot.Albedo => MaterialMapIndex.Albedo,
            MaterialTextureSlot.Normal => MaterialMapIndex.Normal,
            MaterialTextureSlot.Metallic => MaterialMapIndex.Metalness,
            MaterialTextureSlot.Roughness => MaterialMapIndex.Roughness,
            MaterialTextureSlot.Occlusion => MaterialMapIndex.Occlusion,
            MaterialTextureSlot.Emission => MaterialMapIndex.Emission,
            _ => throw new ArgumentOutOfRangeException(nameof(slot))
        };
        for (var i = 0; i < modelResource.NativeModel.MaterialCount; i++)
            Native.SetMaterialTexture(ref modelResource.NativeModel.Materials[i], nativeSlot, textureResource.NativeTexture);
    }

    public void SetShaderFloat(IShaderResource shader, string name, float value) =>
        Native.SetShaderValue(GetShader(shader).NativeShader, GetShader(shader).Location(name), value, ShaderUniformDataType.Float);
    public void SetShaderVector2(IShaderResource shader, string name, Vector2 value) =>
        Native.SetShaderValue(GetShader(shader).NativeShader, GetShader(shader).Location(name), value, ShaderUniformDataType.Vec2);
    public void SetShaderVector3(IShaderResource shader, string name, Vector3 value) =>
        Native.SetShaderValue(GetShader(shader).NativeShader, GetShader(shader).Location(name), value, ShaderUniformDataType.Vec3);
    public void SetShaderVector4(IShaderResource shader, string name, Vector4 value) =>
        Native.SetShaderValue(GetShader(shader).NativeShader, GetShader(shader).Location(name), value, ShaderUniformDataType.Vec4);
    public void SetShaderTexture(IShaderResource shader, string name, ITextureResource texture)
    {
        var shaderResource = GetShader(shader);
        Native.SetShaderValueTexture(shaderResource.NativeShader, shaderResource.Location(name), GetTexture(texture).NativeTexture);
    }

    private void Ensure3DReady()
    {
        if (!IsReady) throw new InvalidOperationException("Create a window before loading 3D resources.");
    }

    private Model3DResource TrackModel(Model model)
    {
        var resource = new Model3DResource(this, model);
        _models3D.Add(resource);
        return resource;
    }

    private ShaderResource TrackShader(Shader shader)
    {
        var resource = new ShaderResource(this, shader);
        _shaders3D.Add(resource);
        return resource;
    }

    private Model3DResource GetModel(IModel3DResource model)
    {
        if (model is not Model3DResource resource || resource.Owner != this)
            throw new ArgumentException("Model belongs to a different backend.", nameof(model));
        ObjectDisposedException.ThrowIf(resource.IsDisposed, model);
        return resource;
    }

    private ShaderResource GetShader(IShaderResource shader)
    {
        if (shader is not ShaderResource resource || resource.Owner != this)
            throw new ArgumentException("Shader belongs to a different backend.", nameof(shader));
        ObjectDisposedException.ThrowIf(resource.IsDisposed, shader);
        return resource;
    }

    private TextureResource GetTexture(ITextureResource texture)
    {
        if (texture is not TextureResource resource || resource.Owner != this)
            throw new ArgumentException("Texture belongs to a different backend.", nameof(texture));
        ObjectDisposedException.ThrowIf(resource.IsDisposed, texture);
        return resource;
    }

    private sealed class Model3DResource(RaylibBackend owner, Model model) : IModel3DResource
    {
        internal RaylibBackend Owner => owner;
        internal Model NativeModel = model;
        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Native.UnloadModel(NativeModel);
            owner._models3D.Remove(this);
        }
    }

    private sealed class ShaderResource(RaylibBackend owner, Shader shader) : IShaderResource
    {
        private readonly Dictionary<string, int> _locations = new(StringComparer.Ordinal);
        internal RaylibBackend Owner => owner;
        internal Shader NativeShader = shader;
        public bool IsDisposed { get; private set; }
        internal int Location(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return _locations.TryGetValue(name, out var location)
                ? location
                : _locations[name] = Native.GetShaderLocation(NativeShader, name);
        }
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Native.UnloadShader(NativeShader);
            owner._shaders3D.Remove(this);
        }
    }

    private sealed class Mode3DScope : IDisposable
    {
        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Native.EndMode3D();
        }
    }
}
