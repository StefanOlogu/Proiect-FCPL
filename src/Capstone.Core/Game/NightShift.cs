using Capstone.Core.Collections;
using Capstone.Core.Model;

namespace Capstone.Core.Game;

public enum ShiftOutcome
{
    InProgress,
    Survived,   // 06:00, the bartender goes home
    Caught,     // somebody got in through an open entrance
}

/// <summary>
/// One shift behind the bar, from 00:00 to 06:00, played in ticks.
/// Every tick each active personality rolls against its aggression for that hour; a success is one step closer.
/// From the doorway, one more step is either a slammed door (back to the street) or a jumpscare.
/// Closed entrances drain the old fuse box; when it hits zero, everything swings open.
/// </summary>
public sealed class NightShift
{
    public const int TicksPerHour = 4;
    public const int TotalTicks = NightShiftClock.Hours * TicksPerHour;
    public const int StartDistance = 3;   // 3 = down the street, 0 = in the doorway
    public const int FullPower = 100;
    public const int DrainPerClosedEntrance = 2;   // all four closed runs dry around 03:00
    public const int Die = 20;            // aggression 20 always moves, 0 never does

    private readonly Func<int, int> _roll;
    private readonly IReadOnlyList<Personality> _active;
    private readonly Dictionary<Personality, int> _distance = [];
    private readonly HashSet<Entrance> _closed = [];

    /// <param name="night">Who is around tonight; only the active ones play.</param>
    /// <param name="roll">The dice, passed in as a function: given n, returns 1..n. Tests pass a fixed one.</param>
    public NightShift(Night night, Func<int, int> roll)
    {
        ArgumentNullException.ThrowIfNull(night);
        ArgumentNullException.ThrowIfNull(roll);

        _roll = roll;
        _active = night.Active;
        foreach (var p in _active)
            _distance[p] = StartDistance;
    }

    /// <summary>A personality took a step; the int is how far it still is (0 = in the doorway).</summary>
    public event Action<Personality, int>? Moved;

    /// <summary>A personality hit a closed entrance and went back down the street.</summary>
    public event Action<Personality>? Blocked;

    /// <summary>The fuse box gave out: every entrance is open now.</summary>
    public event Action? Blackout;

    /// <summary>The shift ended, one way or the other.</summary>
    public event Action<ShiftOutcome>? Ended;

    public int Tick { get; private set; }

    public int Hour => Math.Min(Tick / TicksPerHour, NightShiftClock.Hours - 1);

    public int Power { get; private set; } = FullPower;

    public ShiftOutcome Outcome { get; private set; } = ShiftOutcome.InProgress;

    /// <summary>Who got in, once <see cref="Outcome"/> is <see cref="ShiftOutcome.Caught"/>.</summary>
    public Personality? CaughtBy { get; private set; }

    /// <summary>The last three personalities seen on the cameras, newest first.</summary>
    public RecentlySeen<Personality> CameraHistory { get; } = new(3);

    public IReadOnlyList<Personality> Personalities => _active;

    public int DistanceOf(Personality personality) =>
        _distance.TryGetValue(personality, out var d)
            ? d
            : throw new ArgumentException($"{personality.Name} is not playing tonight.", nameof(personality));

    public bool IsClosed(Entrance entrance) => _closed.Contains(entrance);

    public void Close(Entrance entrance)
    {
        if (Power > 0) _closed.Add(entrance);   // no power, no doors
    }

    public void Open(Entrance entrance) => _closed.Remove(entrance);

    /// <summary>Checks the camera on one entrance: the closest personality there, or null if nobody is coming.</summary>
    public Personality? Watch(Entrance entrance)
    {
        var closest = _active
            .Where(p => p.Entrance == entrance)
            .OrderBy(p => _distance[p])
            .FirstOrDefault();

        if (closest is not null) CameraHistory.Push(closest);
        return closest;
    }

    /// <summary>Plays one tick. Calling it after the shift ended is a bug in the caller.</summary>
    public void Advance()
    {
        if (Outcome != ShiftOutcome.InProgress)
            throw new InvalidOperationException($"The shift is over ({Outcome}).");

        foreach (var p in _active)
        {
            if (_roll(Die) > p.AggressionAt(Hour)) continue;

            Step(p);
            if (Outcome == ShiftOutcome.Caught) return;
        }

        DrainPower();

        Tick++;
        if (Tick == TotalTicks) End(ShiftOutcome.Survived);
    }

    private void Step(Personality p)
    {
        var distance = _distance[p];
        if (distance > 0)
        {
            _distance[p] = distance - 1;
            Moved?.Invoke(p, distance - 1);
        }
        else if (_closed.Contains(p.Entrance))
        {
            _distance[p] = StartDistance;
            Blocked?.Invoke(p);
        }
        else
        {
            CaughtBy = p;
            End(ShiftOutcome.Caught);
        }
    }

    private void DrainPower()
    {
        if (_closed.Count == 0) return;

        Power = Math.Max(0, Power - _closed.Count * DrainPerClosedEntrance);
        if (Power > 0) return;

        _closed.Clear();
        Blackout?.Invoke();
    }

    private void End(ShiftOutcome outcome)
    {
        Outcome = outcome;
        Ended?.Invoke(outcome);
    }
}
