using Fix2Engine.Core;

namespace Fix2Engine.Graphics;

/// <summary>Metadata for one skeletal animation clip. It does not own GPU resources.</summary>
public sealed class ModelAnimationClip
{
    private float _framesPerSecond = 60f;

    internal ModelAnimationClip(ModelAnimationSet set, int index, string name, int frameCount, int boneCount)
    {
        Set = set;
        Index = index;
        Name = name;
        FrameCount = frameCount;
        BoneCount = boneCount;
    }

    public string Name { get; }
    public int FrameCount { get; }
    public int BoneCount { get; }
    /// <summary>Playback rate used to convert seconds to raylib animation frames.</summary>
    public float FramesPerSecond
    {
        get => _framesPerSecond;
        set
        {
            if (!float.IsFinite(value) || value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            _framesPerSecond = value;
        }
    }
    public double Duration => FrameCount / (double)FramesPerSecond;
    internal ModelAnimationSet Set { get; }
    internal int Index { get; }
}

/// <summary>Owns the skeletal animation clips loaded from a model file.</summary>
public sealed class ModelAnimationSet : IDisposable
{
    private readonly IModelAnimationSetResource _resource;
    private readonly ModelAnimationClip[] _clips;
    private readonly Dictionary<string, ModelAnimationClip> _byName;

    public IReadOnlyList<ModelAnimationClip> Clips { get; }
    public bool IsDisposed => _resource.IsDisposed;

    private ModelAnimationSet(IModelAnimationSetResource resource)
    {
        _resource = resource;
        try
        {
            _clips = new ModelAnimationClip[resource.Count];
            _byName = new Dictionary<string, ModelAnimationClip>(resource.Count, StringComparer.Ordinal);
            for (int i = 0; i < resource.Count; i++)
            {
                var name = resource.GetName(i);
                if (string.IsNullOrWhiteSpace(name)) name = $"Clip{i}";
                var clip = new ModelAnimationClip(this, i, name, resource.GetFrameCount(i), resource.GetBoneCount(i));
                if (!_byName.TryAdd(name, clip))
                    throw new InvalidOperationException($"Animation clip '{name}' appears more than once.");
                _clips[i] = clip;
            }
            Clips = Array.AsReadOnly(_clips);
        }
        catch
        {
            resource.Dispose();
            throw;
        }
    }

    public static ModelAnimationSet Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new ModelAnimationSet(EngineBackend.Current.LoadModelAnimations3D(path));
    }

    public bool Contains(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _byName.ContainsKey(name);
    }

    public ModelAnimationClip? Find(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _byName.GetValueOrDefault(name);
    }

    public ModelAnimationClip Get(string name) =>
        Find(name) ?? throw new KeyNotFoundException($"Animation clip '{name}' does not exist.");

    /// <summary>Checks whether the clip's skeleton matches the model.</summary>
    public bool IsValid(Model3D model, ModelAnimationClip clip)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentNullException.ThrowIfNull(model);
        ObjectDisposedException.ThrowIf(model.IsDisposed, model);
        ValidateClip(clip);
        return EngineBackend.Current.IsModelAnimationValid3D(model.Resource, _resource, clip.Index);
    }

    /// <summary>Applies a frame to the model. Call before the model is drawn.</summary>
    public void Apply(Model3D model, ModelAnimationClip clip, float frame)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentNullException.ThrowIfNull(model);
        ObjectDisposedException.ThrowIf(model.IsDisposed, model);
        ValidateClip(clip);
        if (!float.IsFinite(frame)) throw new ArgumentOutOfRangeException(nameof(frame));
        EngineBackend.Current.UpdateModelAnimation3D(model.Resource, _resource, clip.Index, frame);
    }

    public void Dispose() => _resource.Dispose();

    private void ValidateClip(ModelAnimationClip clip)
    {
        ArgumentNullException.ThrowIfNull(clip);
        if (!ReferenceEquals(clip.Set, this))
            throw new ArgumentException("The animation clip belongs to a different animation set.", nameof(clip));
    }
}
