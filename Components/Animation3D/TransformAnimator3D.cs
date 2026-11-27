namespace Fix2Engine.Components.Animation3D;

/// <summary>Plays <see cref="TransformClip3D"/> instances on the owning <see cref="Object3D"/>.</summary>
public sealed class TransformAnimator3D : Component3D
{
    private readonly Dictionary<string, TransformClip3D> _clips = new(StringComparer.Ordinal);
    private TransformClip3D? _current;
    private float _time;
    private float _speed = 1;
    private bool _playing;
    private bool _completed;

    public TransformClip3D? CurrentClip => _current;
    public float Time => _time;
    public float NormalizedTime => _current == null || _current.Duration <= 0
        ? 0f
        : Math.Clamp(_time / _current.Duration, 0f, 1f);
    public bool IsPlaying => _playing;
    public IReadOnlyCollection<TransformClip3D> Clips => _clips.Values;
    public float Speed
    {
        get => _speed;
        set
        {
            if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            _speed = value;
        }
    }
    public event Action<TransformClip3D>? Started;
    public event Action<TransformClip3D>? Finished;
    public event Action<TransformClip3D>? Looped;

    public TransformAnimator3D Add(TransformClip3D clip)
    {
        ArgumentNullException.ThrowIfNull(clip);
        if (!_clips.TryAdd(clip.Name, clip))
            throw new ArgumentException($"Clip '{clip.Name}' already exists.", nameof(clip));
        return this;
    }

    public bool Contains(string name) => _clips.ContainsKey(name);

    public bool Remove(string name)
    {
        if (!_clips.Remove(name, out var removed)) return false;
        if (ReferenceEquals(_current, removed))
        {
            _current = null;
            _playing = false;
            _completed = false;
            _time = 0;
        }
        return true;
    }

    /// <summary>Repeated Play calls preserve progress; restart=true explicitly rewinds.</summary>
    public void Play(string name, bool restart = false)
    {
        if (!_clips.TryGetValue(name, out var clip))
            throw new KeyNotFoundException($"Clip '{name}' does not exist.");
        if (ReferenceEquals(_current, clip) && !restart && !_completed)
        {
            _playing = true;
            return;
        }
        _current = clip;
        _time = 0;
        _completed = false;
        _playing = true;
        ApplyPose();
        Started?.Invoke(clip);
    }

    public void Pause() => _playing = false;
    public void Resume() => _playing = _current != null && !_completed;

    public void Stop()
    {
        _playing = false;
        _completed = false;
        _time = 0;
        if (_current != null) ApplyPose();
    }

    /// <summary>Jumps to a time without changing the paused/playing state.</summary>
    public void Seek(float time)
    {
        if (_current == null) throw new InvalidOperationException("Select a clip before seeking.");
        if (!float.IsFinite(time) || time < 0) throw new ArgumentOutOfRangeException(nameof(time));
        _time = WrapOrClamp(_current, time);
        _completed = false;
        ApplyPose();
    }

    /// <summary>Advances playback. Attached components call this automatically from OnUpdate.</summary>
    public new void Update(float dt)
    {
        if (!float.IsFinite(dt) || dt < 0) throw new ArgumentOutOfRangeException(nameof(dt));
        var clip = _current;
        if (!_playing || clip == null || _speed == 0 || dt == 0) return;

        float duration = clip.Duration;
        if (duration <= 0)
        {
            ApplyPose();
            if (!clip.Loop) Complete(clip);
            return;
        }

        float next = _time + dt * _speed;
        if (clip.Loop)
        {
            bool looped = next >= duration;
            if (looped) next %= duration;
            _time = next;
            ApplyPose();
            if (looped) Looped?.Invoke(clip);
            return;
        }

        if (next >= duration)
        {
            _time = duration;
            ApplyPose();
            Complete(clip);
            return;
        }

        _time = next;
        ApplyPose();
    }

    protected override void OnUpdate(float dt) => Update(dt);

    private void Complete(TransformClip3D clip)
    {
        _playing = false;
        _completed = true;
        Finished?.Invoke(clip);
    }

    private void ApplyPose()
    {
        if (_current == null || Owner == null) return;
        var pose = _current.Evaluate(_time);
        Owner.Position = pose.Position;
        Owner.Orientation = pose.Rotation;
        Owner.Scale = pose.Scale;
    }

    private static float WrapOrClamp(TransformClip3D clip, float time)
    {
        if (clip.Duration <= 0) return 0f;
        return clip.Loop ? time % clip.Duration : Math.Min(time, clip.Duration);
    }
}
