using Fix2Engine.Graphics;

namespace Fix2Engine.Components.Animation3D;

/// <summary>Plays skeletal animation clips on a <see cref="Model3D"/> through the active backend.</summary>
public sealed class ModelAnimator3D : Component3D
{
    private ModelAnimationClip? _current;
    private float _time;
    private float _speed = 1;
    private bool _playing;
    private bool _completed;

    public ModelAnimator3D(Model3D model, ModelAnimationSet animations, bool ownsAnimations = false)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(animations);
        if (animations.Clips.Count == 0) throw new ArgumentException("The animation set is empty.", nameof(animations));
        Model = model;
        Animations = animations;
        OwnsAnimations = ownsAnimations;
    }

    public Model3D Model { get; }
    public ModelAnimationSet Animations { get; }
    public bool OwnsAnimations { get; }
    public ModelAnimationClip? CurrentClip => _current;
    public float Time => _time;
    public float CurrentFrame => _current == null ? 0f : _time * _current.FramesPerSecond;
    public float NormalizedTime => _current == null || _current.Duration <= 0
        ? 0f
        : Math.Clamp(_time / (float)_current.Duration, 0f, 1f);
    public bool IsPlaying => _playing;
    public bool Loop { get; set; } = true;
    public float Speed
    {
        get => _speed;
        set
        {
            if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            _speed = value;
        }
    }
    public event Action<ModelAnimationClip>? Started;
    public event Action<ModelAnimationClip>? Finished;
    public event Action<ModelAnimationClip>? Looped;

    /// <summary>Repeated Play calls preserve progress; restart=true explicitly rewinds.</summary>
    public void Play(string name, bool restart = false)
    {
        var clip = Animations.Get(name);
        if (ReferenceEquals(_current, clip) && !restart && !_completed)
        {
            _playing = true;
            return;
        }
        if (!Animations.IsValid(Model, clip))
            throw new InvalidOperationException($"Animation '{clip.Name}' does not match the model skeleton.");
        _current = clip;
        _time = 0;
        _completed = false;
        _playing = true;
        ApplyFrame();
        Started?.Invoke(clip);
    }

    public void Pause() => _playing = false;
    public void Resume() => _playing = _current != null && !_completed;

    public void Stop()
    {
        _playing = false;
        _completed = false;
        _time = 0;
        if (_current != null) ApplyFrame();
    }

    /// <summary>Jumps to a time in seconds without changing the paused/playing state.</summary>
    public void Seek(float time)
    {
        if (_current == null) throw new InvalidOperationException("Select a clip before seeking.");
        if (!float.IsFinite(time) || time < 0) throw new ArgumentOutOfRangeException(nameof(time));
        float duration = (float)_current.Duration;
        _time = duration <= 0 ? 0f : Loop ? time % duration : Math.Min(time, duration);
        _completed = false;
        ApplyFrame();
    }

    /// <summary>Advances playback. Attached components call this automatically from OnUpdate.</summary>
    public new void Update(float dt)
    {
        if (!float.IsFinite(dt) || dt < 0) throw new ArgumentOutOfRangeException(nameof(dt));
        var clip = _current;
        if (!_playing || clip == null || _speed == 0 || dt == 0) return;

        float duration = (float)clip.Duration;
        if (duration <= 0)
        {
            ApplyFrame();
            if (!Loop) Complete(clip);
            return;
        }

        float next = _time + dt * _speed;
        if (Loop)
        {
            bool looped = next >= duration;
            if (looped) next %= duration;
            _time = next;
            ApplyFrame();
            if (looped) Looped?.Invoke(clip);
            return;
        }

        if (next >= duration)
        {
            _time = duration;
            ApplyFrame();
            Complete(clip);
            return;
        }

        _time = next;
        ApplyFrame();
    }

    protected override void OnUpdate(float dt) => Update(dt);

    protected override void DisposeResources()
    {
        if (OwnsAnimations) Animations.Dispose();
    }

    private void ApplyFrame()
    {
        if (_current == null) return;
        Animations.Apply(Model, _current, _time * _current.FramesPerSecond);
    }

    private void Complete(ModelAnimationClip clip)
    {
        _playing = false;
        _completed = true;
        Finished?.Invoke(clip);
    }
}
