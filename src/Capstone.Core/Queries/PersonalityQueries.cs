using Capstone.Core.Model;
using SharpQuest.Contracts;

namespace Capstone.Core.Queries;

/// <summary>
/// The questions the bar asks about its personalities (Lab 07), as extension methods on a sequence —
/// the same shape as every LINQ verb, so they chain with them.
/// </summary>
public static class PersonalityQueries
{
    /// <summary>Name order: by DisplayName ignoring case, ties broken by Id.</summary>
    public static IOrderedEnumerable<IItem> InNameOrder(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        return items
            .OrderBy(i => i.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(i => i.Id, StringComparer.Ordinal);
    }

    /// <summary>The <paramref name="n"/> longest names, longest first, ties broken by Id.</summary>
    public static IEnumerable<IItem> Longest(this IEnumerable<IItem> items, int n)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        return items
            .OrderByDescending(i => i.DisplayName.Length)
            .ThenBy(i => i.Id, StringComparer.Ordinal)
            .Take(n);
    }

    /// <summary>The whole sequence in one line. Average() throws on nothing, so nothing gets its own answer.</summary>
    public static StoreStats Summarise(this IEnumerable<IItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var list = items.ToList();   // read once, then ask it four questions
        if (list.Count == 0) return new StoreStats(0, 0, null, 0);

        return new StoreStats(
            list.Count,
            list.Select(i => char.ToUpperInvariant(i.DisplayName[0])).Distinct().Count(),
            list.Longest(1).First().DisplayName,
            list.Average(i => i.DisplayName.Length));
    }

    // The questions a bartender actually asks.

    /// <summary>Who moves tonight, most dangerous first.</summary>
    public static IEnumerable<Personality> ThreatsOn(this IEnumerable<IItem> items, int night) =>
        items.OfType<Personality>()
             .Where(p => p.IsActiveOn(night))
             .OrderByDescending(p => p.Aggression)
             .ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase);

    /// <summary>How many come through each entrance, busiest first. Entrances nobody uses aren't listed.</summary>
    public static IEnumerable<(Entrance Entrance, int Count)> TrafficByEntrance(this IEnumerable<IItem> items) =>
        items.OfType<Personality>()
             .GroupBy(p => p.Entrance)
             .Select(g => (g.Key, g.Count()))
             .OrderByDescending(t => t.Item2)
             .ThenBy(t => t.Key);

    /// <summary>The Galați neighbourhoods the local legends come from, each once, alphabetically.</summary>
    public static IEnumerable<string> Neighbourhoods(this IEnumerable<IItem> items) =>
        items.OfType<LocalLegend>()
             .Select(l => l.Neighbourhood)
             .Distinct(StringComparer.OrdinalIgnoreCase)
             .Order(StringComparer.CurrentCulture);

    /// <summary>The oldest meme in the building, or null when there are none.</summary>
    public static ViralMeme? OldestMeme(this IEnumerable<IItem> items) =>
        items.OfType<ViralMeme>().MinBy(m => m.OriginYear);

    /// <summary>The ones that give no warning: no catchphrase, so nothing is heard at the door.</summary>
    public static IEnumerable<Personality> SilentOnes(this IEnumerable<IItem> items) =>
        items.OfType<Personality>().Where(p => p.Catchphrase is null);
}
