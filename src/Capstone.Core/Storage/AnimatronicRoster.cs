using SharpQuest.Contracts;

namespace Capstone.Core.Storage;

/// <summary>
/// The store. A list keeps insertion order; a dictionary makes lookups by Id fast.
/// Both have to agree, so every change goes through Add and Remove.
/// </summary>
public sealed class AnimatronicRoster : IItemStore
{
    private readonly List<IItem> _ordered = [];
    private readonly Dictionary<string, IItem> _byId = new(StringComparer.Ordinal);   // Ids are case-sensitive

    public int Count => _ordered.Count;

    public void Add(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!_byId.TryAdd(item.Id, item))
            throw new ArgumentException($"An animatronic with Id '{item.Id}' is already on the roster.", nameof(item));

        _ordered.Add(item);
    }

    public IItem? FindById(string id) => _byId.GetValueOrDefault(id);

    // ToArray copies, so callers get a snapshot, not a live view of _ordered.
    public IReadOnlyList<IItem> All() => _ordered.ToArray();

    public bool Remove(string id)
    {
        if (!_byId.Remove(id, out var item))
            return false;

        _ordered.Remove(item);
        return true;
    }
}
