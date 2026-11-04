using SharpQuest.Contracts;

namespace Capstone.Core.Model;

/// <summary>
/// The abstract base: one personality that wants "one last drink" after closing time.
/// Nobody is "just a personality": every one is a <see cref="LocalLegend"/> or a <see cref="ViralMeme"/>.
/// </summary>
public abstract class Personality : IItem, IDescribable
{
    public const int MaxAggression = 20;

    private readonly string _id = "";
    private readonly string _name = "";
    private readonly int _aggression;
    private readonly int _firstNight = Night.FirstNight;

    public required string Id
    {
        get => _id;
        init => _id = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A personality needs an Id.", nameof(Id))
            : value;
    }

    public required string Name
    {
        get => _name;
        init => _name = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A personality needs a name.", nameof(Name))
            : value;
    }

    public string DisplayName => Name;

    public required Entrance Entrance { get; init; }

    /// <summary>0 = never moves, 20 = moves every chance it gets.</summary>
    public required int Aggression
    {
        get => _aggression;
        init => _aggression = value is < 0 or > MaxAggression
            ? throw new ArgumentOutOfRangeException(nameof(Aggression), value, $"Aggression goes from 0 to {MaxAggression}.")
            : value;
    }

    /// <summary>The first night this personality shows up.</summary>
    public int FirstNight
    {
        get => _firstNight;
        init => _firstNight = value is < Night.FirstNight or > Night.LastNight
            ? throw new ArgumentOutOfRangeException(nameof(FirstNight), value, $"Nights go from {Night.FirstNight} to {Night.LastNight}.")
            : value;
    }

    /// <summary>The nullable that means something: null = silent, the bartender hears no warning.</summary>
    public string? Catchphrase { get; init; }

    /// <summary>Does this personality move on the given night? Only once it shows up, and only with some aggression.</summary>
    public bool IsActiveOn(int night)
    {
        if (night is < Night.FirstNight or > Night.LastNight)
            throw new ArgumentOutOfRangeException(nameof(night), night, $"Nights go from {Night.FirstNight} to {Night.LastNight}.");
        return night >= FirstNight && Aggression > 0;
    }

    /// <summary>
    /// How aggressive it is at this hour of the shift (0 = 00:00 … 5 = 05:00).
    /// Virtual: the base keeps it flat, each kind bends it its own way.
    /// </summary>
    public virtual int AggressionAt(int hour)
    {
        CheckHour(hour);
        return Aggression;
    }

    /// <summary>The polymorphic one: each kind describes itself in its own words.</summary>
    public abstract string Describe();

    /// <summary>What happens when it reaches the bar through an open entrance.</summary>
    public abstract string Jumpscare();

    /// <summary>The part every description shares, so the kinds only add what is theirs.</summary>
    protected string Basics => $"{Name} · {Entrance} · aggression {Aggression} · from night {FirstNight}";

    protected string Quote => Catchphrase is null ? "(silent)" : $"\"{Catchphrase}\"";

    protected static void CheckHour(int hour)
    {
        if (hour is < 0 or >= NightShiftClock.Hours)
            throw new ArgumentOutOfRangeException(nameof(hour), hour, $"The shift's hours go from 0 to {NightShiftClock.Hours - 1}.");
    }
}
