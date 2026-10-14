namespace Capstone.Core.Model;

/// <summary>The parent: one night shift and the animatronics in the building that night.</summary>
public sealed class Night : IDescribable
{
    public const int FirstNight = 1;
    public const int LastNight = 7;

    private readonly List<Animatronic> _animatronics = [];

    public Night(int number)
    {
        if (number is < FirstNight or > LastNight)
            throw new ArgumentOutOfRangeException(nameof(number), number, $"Nights go from {FirstNight} to {LastNight}.");
        Number = number;
    }

    public int Number { get; }

    public IReadOnlyList<Animatronic> Animatronics => _animatronics.ToArray();

    /// <summary>The animatronics that will actually move tonight.</summary>
    public IReadOnlyList<Animatronic> Active => _animatronics.Where(a => a.IsActiveOn(Number)).ToArray();

    public void Add(Animatronic animatronic)
    {
        ArgumentNullException.ThrowIfNull(animatronic);
        _animatronics.Add(animatronic);
    }

    public string Describe() => $"Night {Number} · {Active.Count} of {_animatronics.Count} animatronics active";
}
