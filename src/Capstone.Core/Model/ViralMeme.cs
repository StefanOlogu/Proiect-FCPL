namespace Capstone.Core.Model;

/// <summary>
/// A kind: a meme that walked off the internet and into the bar.
/// Memes go stale fast: strongest at midnight, weaker every hour after.
/// </summary>
public sealed class ViralMeme : Personality
{
    public const int FirstMemeYear = 1990;

    private readonly int _originYear = FirstMemeYear;

    /// <summary>The year it went viral.</summary>
    public required int OriginYear
    {
        get => _originYear;
        init => _originYear = value < FirstMemeYear || value > DateTime.Now.Year
            ? throw new ArgumentOutOfRangeException(nameof(OriginYear), value, $"A meme goes viral between {FirstMemeYear} and this year.")
            : value;
    }

    /// <summary>−2 aggression every hour, never below zero.</summary>
    public override int AggressionAt(int hour)
    {
        CheckHour(hour);
        return Math.Max(0, Aggression - 2 * hour);
    }

    public override string Describe() => $"{Basics} · viral since {OriginYear} · {Quote}";

    public override string Jumpscare() =>
        $"{Name} fills every screen in the bar. {Catchphrase ?? "No caption needed."} ({OriginYear} called, it wants its meme back.)";
}
