using SharpQuest.Contracts;

namespace Capstone.Core.Model;

/// <summary>
/// The outside world's view of the hierarchy (Lab 04): one loop that doesn't care which kind it holds,
/// one switch expression that does, and the two parameter keywords worth knowing (out, params).
/// </summary>
public static class PersonalityCatalog
{
    public const string LocalLabel = "Local legend of Galați";
    public const string MemeLabel = "Viral meme";
    public const string StrangerLabel = "Stranger (not a personality)";

    /// <summary>The polymorphic loop: every item speaks for itself through the override.</summary>
    public static IReadOnlyList<string> DescribeAll(IEnumerable<IItem> items)
    {
        var lines = new List<string>();
        foreach (var item in items)
            lines.Add(AsPersonality(item).Describe());
        return lines;
    }

    /// <summary>The switch expression: sorting an item into a category from the outside.</summary>
    public static string Classify(IItem item) => item switch
    {
        null => throw new ArgumentNullException(nameof(item)),
        LocalLegend => LocalLabel,
        ViralMeme => MemeLabel,
        _ => StrangerLabel,
    };

    /// <summary>The first item whose DisplayName is exactly <paramref name="name"/>. A blank name is simply not found.</summary>
    public static bool TryFindByName(IItemStore store, string? name, out IItem? found)
    {
        ArgumentNullException.ThrowIfNull(store);

        found = null;
        if (string.IsNullOrWhiteSpace(name)) return false;

        foreach (var item in store.All())
        {
            if (item.DisplayName == name)
            {
                found = item;
                return true;
            }
        }
        return false;
    }

    /// <summary>Adds every item in order; the store's own rules still throw, and what came before stays.</summary>
    public static int AddAll(IItemStore store, params IItem[] items)
    {
        ArgumentNullException.ThrowIfNull(store);

        var added = 0;
        foreach (var item in items)
        {
            store.Add(item);
            added++;
        }
        return added;
    }

    public static Personality AsPersonality(IItem item) =>
        item as Personality ?? throw new ArgumentException($"{item.DisplayName} is not a personality.", nameof(item));
}
