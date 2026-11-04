using Capstone.Core.Collections;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab05Wiring : ILab05Wiring
{
    public IItemStore CreateStore() => new PersonalityRoster();

    public IItem CreateSample(int seed) => Lab02Wiring.Sample(seed);

    public Type CollectionType => typeof(PersonalityRoster);

    public Type GenericDefinition => typeof(RecentlySeen<>);

    public IItem At(IItemStore store, int index) => Roster(store)[index];

    public IItem? ById(IItemStore store, string id) => Roster(store)[id];

    public IReadOnlyList<IItem> EachInOrder(IItemStore store)
    {
        var items = new List<IItem>();
        foreach (var item in Roster(store))
            items.Add(item);
        return items;
    }

    public IEnumerable<IItem> Matching(IItemStore store, string text) => Roster(store).Matching(text);

    public IReadOnlyList<T> Recent<T>(int capacity, params T[] pushed)
    {
        var recent = new RecentlySeen<T>(capacity);
        foreach (var value in pushed)
            recent.Push(value);
        return recent.ToList();
    }

    internal static PersonalityRoster Roster(IItemStore store) =>
        store as PersonalityRoster ?? throw new ArgumentException("Expected the PersonalityRoster.", nameof(store));
}
