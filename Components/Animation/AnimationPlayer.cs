using Fix2Engine.Core;

namespace Fix2Engine.Components.Animation;

/// <summary>Per-object playback state. Clips may be shared by multiple players.</summary>
public sealed class AnimationPlayer
{
    private readonly Dictionary<string, SpriteAnimation> _animations = new(StringComparer.Ordinal);
    private double _framePosition;
    private float _speed = 1;
    private bool _completed;

    public SpriteAnimation? CurrentAnimation { get; private set; }
    public int FrameIndex { get; private set; }
    public RectF? CurrentFrame => CurrentAnimation?.Frames[FrameIndex];
    public bool IsPlaying { get; private set; }
    public float Speed
    {
        get => _speed;
        set
        {
            if (!float.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            _speed = value;
        }
    }
    public event Action<int>? FrameChanged;
    public event Action<SpriteAnimation>? Finished;

    public void Add(SpriteAnimation animation)
    {
        ArgumentNullException.ThrowIfNull(animation);
        if (!_animations.TryAdd(animation.Name, animation))
            throw new ArgumentException($"Animation '{animation.Name}' already exists.", nameof(animation));
    }

    public bool Contains(string name) => _animations.ContainsKey(name);

    public IReadOnlyCollection<SpriteAnimation> Animations => _animations.Values;

    /// <summary>Jump to a frame without changing playback/paused state.</summary>
    public void SeekFrame(int frame)
    {
        if (CurrentAnimation == null) throw new InvalidOperationException("Select an animation before seeking.");
        if (frame < 0 || frame >= CurrentAnimation.Frames.Count) throw new ArgumentOutOfRangeException(nameof(frame));
        int previous = FrameIndex;
        FrameIndex = frame;
        _framePosition = frame;
        _completed = false;
        if (previous != frame) FrameChanged?.Invoke(frame);
    }

    public bool Remove(string name)
    {
        if (!_animations.Remove(name, out var removed)) return false;
        if (CurrentAnimation == removed)
        {
            Stop();
            CurrentAnimation = null;
        }
        return true;
    }

    /// <summary>Repeated Play calls preserve progress; restart=true explicitly rewinds.</summary>
    public void Play(string name, bool restart = false)
    {
        if (!_animations.TryGetValue(name, out var animation))
            throw new KeyNotFoundException($"Animation '{name}' does not exist.");
        if (CurrentAnimation == animation && !restart && !_completed)
        {
            IsPlaying = true;
            return;
        }
        CurrentAnimation = animation;
        FrameIndex = 0;
        _framePosition = 0;
        _completed = false;
        IsPlaying = true;
        FrameChanged?.Invoke(FrameIndex);
    }

    public void Pause() => IsPlaying = false;
    public void Resume() => IsPlaying = CurrentAnimation != null && !_completed;

    public void Stop()
    {
        IsPlaying = false;
        _completed = false;
        _framePosition = 0;
        int previous = FrameIndex;
        FrameIndex = 0;
        if (previous != 0) FrameChanged?.Invoke(FrameIndex);
    }

    public void Update(float dt)
    {
        Object2D.ValidateDelta(dt);
        var clip = CurrentAnimation;
        if (!IsPlaying || clip == null || Speed == 0 || dt == 0) return;
        double next = _framePosition + (double)dt * Speed * clip.FramesPerSecond;
        bool finished = !clip.Loop && next >= clip.Frames.Count;
        _framePosition = clip.Loop ? next % clip.Frames.Count : Math.Min(next, clip.Frames.Count);
        int previous = FrameIndex;
        FrameIndex = Math.Min((int)_framePosition, clip.Frames.Count - 1);
        if (finished)
        {
            IsPlaying = false;
            _completed = true;
        }
        // Notify once about the final visible frame, even when a slow update skips frames.
        if (previous != FrameIndex) FrameChanged?.Invoke(FrameIndex);
        if (finished) Finished?.Invoke(clip);
    }
}
