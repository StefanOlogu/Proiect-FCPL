using SharpQuest.Contracts;

namespace Capstone.Core.Model;

/// <summary>The item: one animatronic of the pizzeria.</summary>
public sealed class Animatronic : IItem, IDescribable
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
            ? throw new ArgumentException("An animatronic needs an Id.", nameof(Id))
            : value;
    }

    public required string Name
    {
        get => _name;
        init => _name = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("An animatronic needs a name.", nameof(Name))
            : value;
    }

    public string DisplayName => Name;

    public required Approach Approach { get; init; }

    /// <summary>0 = never moves, 20 = moves every chance it gets.</summary>
    public required int Aggression
    {
        get => _aggression;
        init => _aggression = value is < 0 or > MaxAggression
            ? throw new ArgumentOutOfRangeException(nameof(Aggression), value, $"Aggression goes from 0 to {MaxAggression}.")
            : value;
    }

    /// <summary>The first night this animatronic is switched on.</summary>
    public int FirstNight
    {
        get => _firstNight;
        init => _firstNight = value is < Night.FirstNight or > Night.LastNight
            ? throw new ArgumentOutOfRangeException(nameof(FirstNight), value, $"Nights go from {Night.FirstNight} to {Night.LastNight}.")
            : value;
    }

    /// <summary>The nullable that means something: null = silent, the player hears no warning.</summary>
    public string? Catchphrase { get; init; }

    /// <summary>Does this animatronic move on the given night? Only once it's switched on, and only with some aggression.</summary>
    public bool IsActiveOn(int night)
    {
        if (night is < Night.FirstNight or > Night.LastNight)
            throw new ArgumentOutOfRangeException(nameof(night), night, $"Nights go from {Night.FirstNight} to {Night.LastNight}.");
        return night >= FirstNight && Aggression > 0;
    }

    public string Describe() =>
        $"{Name} · {Approach} · aggression {Aggression} · from night {FirstNight} · {Catchphrase ?? "(silent)"}";
}
