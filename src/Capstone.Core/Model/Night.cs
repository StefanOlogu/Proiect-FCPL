namespace Capstone.Core.Model;

/// <summary>The parent: one night shift and the personalities around the bar that night.</summary>
public sealed class Night : IDescribable
{
    public const int FirstNight = 1;
    public const int LastNight = 7;

    private readonly List<Personality> _personalities = [];

    public Night(int number)
    {
        if (number is < FirstNight or > LastNight)
            throw new ArgumentOutOfRangeException(nameof(number), number, $"Nights go from {FirstNight} to {LastNight}.");
        Number = number;
    }

    public int Number { get; }

    public IReadOnlyList<Personality> Personalities => _personalities.ToArray();

    /// <summary>The personalities that will actually move tonight.</summary>
    public IReadOnlyList<Personality> Active => _personalities.Where(p => p.IsActiveOn(Number)).ToArray();

    public void Add(Personality personality)
    {
        ArgumentNullException.ThrowIfNull(personality);
        _personalities.Add(personality);
    }

    public string Describe() => $"Night {Number} · {Active.Count} of {_personalities.Count} personalities active";
}
