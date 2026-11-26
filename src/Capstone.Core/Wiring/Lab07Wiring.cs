using Capstone.Core.Queries;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab07Wiring : ILab07Wiring
{
    public IItemStore CreateStore() => new PersonalityRoster();

    public IItem CreateSample(int seed) => Lab02Wiring.Sample(seed);

    // Deferred: Select over the roster itself, so it runs each time somebody enumerates it.
    public IEnumerable<string> Names(IItemStore store) => Lab05Wiring.Roster(store).Select(i => i.DisplayName);

    public IReadOnlyList<IItem> Sorted(IEnumerable<IItem> items) => items.InNameOrder().ToList();

    public IReadOnlyList<IItem> Page(IEnumerable<IItem> items, int skip, int take)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(take);

        return items.InNameOrder().Skip(skip).Take(take).ToList();
    }

    public IReadOnlyList<IItem> Longest(IEnumerable<IItem> items, int n) => items.Longest(n).ToList();

    public Type ExtensionHost => typeof(PersonalityQueries);

    public StoreStats Summary(IEnumerable<IItem> items) => items.Summarise();
}
