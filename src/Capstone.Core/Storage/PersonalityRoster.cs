using System.Collections;
using SharpQuest.Contracts;

namespace Capstone.Core.Storage;

/// <summary>
/// The store, and since Lab 05 a real collection: foreach works on it, roster[0] and roster["id"] work on it.
/// A list keeps insertion order; a dictionary makes lookups by Id fast.
/// Both have to agree, so every change goes through Add and Remove.
/// </summary>
public sealed class PersonalityRoster : IItemStore, IEnumerable<IItem>
{
    private readonly List<IItem> _ordered = [];
    private readonly Dictionary<string, IItem> _byId = new(StringComparer.Ordinal);   // Ids are case-sensitive

    /// <summary>
    /// The publisher (Lab 06): raised once per item, after it is really on the roster.
    /// An Add that throws raises nothing.
    /// </summary>
    public event Action<IItem>? ItemAdded;

    /// <summary>Raised once, after an item has really left the roster. A Remove that finds nothing raises nothing.</summary>
    public event Action<IItem>? ItemRemoved;

    public int Count => _ordered.Count;

    /// <summary>By position, in the order items were added. Out of range throws ArgumentOutOfRangeException (the list does it).</summary>
    public IItem this[int index] => _ordered[index];

    /// <summary>By Id, or null when nobody has it — same answer as <see cref="FindById"/>.</summary>
    public IItem? this[string id] => FindById(id);

    public void Add(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!_byId.TryAdd(item.Id, item))
            throw new ArgumentException($"A personality with Id '{item.Id}' is already on the roster.", nameof(item));

        _ordered.Add(item);
        ItemAdded?.Invoke(item);   // ?. : nobody subscribed means the event is null, and that's fine
    }

    public IItem? FindById(string id) => _byId.GetValueOrDefault(id);

    // ToArray copies, so callers get a snapshot, not a live view of _ordered.
    public IReadOnlyList<IItem> All() => _ordered.ToArray();

    public bool Remove(string id)
    {
        if (!_byId.Remove(id, out var item))
            return false;

        _ordered.Remove(item);
        ItemRemoved?.Invoke(item);
        return true;
    }

    /// <summary>The iterator: walks the roster in order, one yield at a time.</summary>
    public IEnumerator<IItem> GetEnumerator()
    {
        for (var i = 0; i < _ordered.Count; i++)
            yield return _ordered[i];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// The items whose DisplayName contains <paramref name="text"/> (case-insensitive), in roster order.
    /// Lazy: nothing is checked until somebody enumerates it, so it sees items added in between.
    /// </summary>
    public IEnumerable<IItem> Matching(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        foreach (var item in this)
            if (item.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase))
                yield return item;
    }

    // Functions as parameters: the caller brings the rule, the roster runs it once per item.

    public IReadOnlyList<IItem> Filter(Func<IItem, bool> keep)
    {
        ArgumentNullException.ThrowIfNull(keep);

        var kept = new List<IItem>();
        foreach (var item in this)
            if (keep(item))
                kept.Add(item);
        return kept;
    }

    public IReadOnlyList<string> Map(Func<IItem, string> describe)
    {
        ArgumentNullException.ThrowIfNull(describe);

        var lines = new List<string>(Count);
        foreach (var item in this)
            lines.Add(describe(item));
        return lines;
    }

    public void ForEach(Action<IItem> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        foreach (var item in this)
            action(item);
    }
}
