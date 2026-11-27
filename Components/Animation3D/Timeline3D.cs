namespace Fix2Engine.Components.Animation3D;

/// <summary>
/// A lightweight, engine-owned timeline for camera, material or arbitrary value tracks.
/// Tracks run in parallel and receive an eased 0..1 progress value.
/// </summary>
public sealed class Timeline3D
{
    private sealed class Track
    {
        public float Start;
        public float Duration;
        public Action<float> Update = null!;
        public EasingKind Easing;
        public bool Completed;
    }

    private readonly List<Track> _tracks = new();
    private float _time;
    private float _speed = 1;
    private bool _playing;

    public float Duration { get; private set; }
    public float Time => _time;
    public bool Loop { get; set; }
    public bool IsPlaying => _playing;
    public float Speed
    {
        get => _speed;
        set
        {
            if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            _speed = value;
        }
    }
    public event Action? Started;
    public event Action? Finished;
    public event Action? Looped;

    public Timeline3D Add(float start, float duration, Action<float> update, EasingKind easing = EasingKind.Linear)
    {
        if (!float.IsFinite(start) || start < 0) throw new ArgumentOutOfRangeException(nameof(start));
        if (!float.IsFinite(duration) || duration < 0) throw new ArgumentOutOfRangeException(nameof(duration));
        ArgumentNullException.ThrowIfNull(update);
        if (!Enum.IsDefined(easing)) throw new ArgumentOutOfRangeException(nameof(easing));
        _tracks.Add(new Track { Start = start, Duration = duration, Update = update, Easing = easing });
        Duration = MathF.Max(Duration, start + duration);
        return this;
    }

    public void Play(bool restart = false)
    {
        if (restart)
        {
            _time = 0;
            ResetTracks();
        }
        if (_playing && !restart) return;
        _playing = true;
        Apply();
        Started?.Invoke();
    }

    public void Pause() => _playing = false;
    public void Resume() => _playing = true;

    public void Stop()
    {
        _playing = false;
        _time = 0;
        ResetTracks();
        Apply();
    }

    /// <summary>Jumps to a time without changing the paused/playing state.</summary>
    public void Seek(float time)
    {
        if (!float.IsFinite(time) || time < 0) throw new ArgumentOutOfRangeException(nameof(time));
        _time = WrapOrClamp(time);
        ResetTracks();
        Apply();
    }

    /// <summary>Advances the timeline. Attach a <see cref="TimelineComponent3D"/> for automatic updates.</summary>
    public void Update(float dt)
    {
        if (!float.IsFinite(dt) || dt < 0) throw new ArgumentOutOfRangeException(nameof(dt));
        if (!_playing || _speed == 0 || dt == 0) return;

        if (Duration <= 0)
        {
            Apply();
            if (!Loop) Complete();
            return;
        }

        float next = _time + dt * _speed;
        if (Loop)
        {
            bool looped = next >= Duration;
            if (looped)
            {
                next %= Duration;
                ResetTracks();
            }
            _time = next;
            Apply();
            if (looped) Looped?.Invoke();
            return;
        }

        if (next >= Duration)
        {
            _time = Duration;
            Apply();
            Complete();
            return;
        }

        _time = next;
        Apply();
    }

    private void Apply()
    {
        foreach (var track in _tracks)
        {
            if (track.Completed || _time < track.Start) continue;
            float progress = track.Duration <= 0
                ? 1f
                : Math.Clamp((_time - track.Start) / track.Duration, 0f, 1f);
            track.Update(Easing.Evaluate(track.Easing, progress));
            if (progress >= 1f) track.Completed = true;
        }
    }

    private void Complete()
    {
        _playing = false;
        Finished?.Invoke();
    }

    private void ResetTracks()
    {
        foreach (var track in _tracks) track.Completed = false;
    }

    private float WrapOrClamp(float time)
    {
        if (Duration <= 0) return 0f;
        return Loop ? time % Duration : Math.Min(time, Duration);
    }
}
