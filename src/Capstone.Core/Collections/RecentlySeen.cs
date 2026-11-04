namespace Capstone.Core.Collections;

/// <summary>
/// The generic type (Lab 05): the last few things seen, newest first, no duplicates.
/// The game uses it as the security camera's history — RecentlySeen&lt;Personality&gt; —
/// but nothing in here knows that, so it works for strings, numbers, anything.
/// </summary>
public sealed class RecentlySeen<T>
{
    private readonly LinkedList<T> _items = [];
    private readonly IEqualityComparer<T> _comparer = EqualityComparer<T>.Default;

    public RecentlySeen(int capacity)
    {
        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "It has to remember at least one thing.");
        Capacity = capacity;
    }

    public int Capacity { get; }

    public int Count => _items.Count;

    /// <summary>The newest one, or default when nothing was seen yet.</summary>
    public T? Latest => _items.First is { } first ? first.Value : default;

    /// <summary>Puts <paramref name="value"/> at the front. Already there: it moves. Full: the oldest drops out.</summary>
    public void Push(T value)
    {
        for (var node = _items.First; node is not null; node = node.Next)
        {
            if (_comparer.Equals(node.Value, value))
            {
                _items.Remove(node);
                break;
            }
        }

        _items.AddFirst(value);

        if (_items.Count > Capacity)
            _items.RemoveLast();
    }

    /// <summary>A snapshot, newest first.</summary>
    public IReadOnlyList<T> ToList() => _items.ToArray();
}
