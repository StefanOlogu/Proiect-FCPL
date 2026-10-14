using SharpQuest.Contracts;

namespace Capstone.Core.Search;

/// <summary>Searches a store by DisplayName, using the course's SearchMatcher rules.</summary>
public sealed class RosterSearch
{
    private readonly SearchMatcher _matcher = SearchMatcher.Create();

    /// <summary>The items whose DisplayName matches the query, in store order. Never changes the store.</summary>
    public IReadOnlyList<IItem> Search(IItemStore store, string query)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(query);

        return store.All().Where(item => _matcher.Matches(item.DisplayName, query)).ToArray();
    }
}
