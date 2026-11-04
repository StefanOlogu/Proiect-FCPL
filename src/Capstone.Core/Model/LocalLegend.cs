namespace Capstone.Core.Model;

/// <summary>
/// A kind: a legend of Galați itself — the neighbour with the drill, the mother with the slipper.
/// Locals never leave before closing, so they get bolder as the night goes on.
/// </summary>
public sealed class LocalLegend : Personality
{
    private readonly string _neighbourhood = "";

    /// <summary>Where in Galați they come from: Țiglina, Mazepa, Micro 19…</summary>
    public required string Neighbourhood
    {
        get => _neighbourhood;
        init => _neighbourhood = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("A local legend comes from somewhere in Galați.", nameof(Neighbourhood))
            : value;
    }

    /// <summary>+1 aggression every hour, never above the maximum.</summary>
    public override int AggressionAt(int hour)
    {
        CheckHour(hour);
        return Math.Min(MaxAggression, Aggression + hour);
    }

    public override string Describe() => $"{Basics} · local legend from {Neighbourhood} · {Quote}";

    public override string Jumpscare() =>
        Catchphrase is null
            ? $"{Name} was already behind you. Nobody from {Neighbourhood} ever says a word."
            : $"{Name} storms in from {Neighbourhood}: \"{Catchphrase}\"";
}
