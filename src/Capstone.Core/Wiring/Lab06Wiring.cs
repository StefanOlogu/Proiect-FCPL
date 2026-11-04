using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab06Wiring : ILab06Wiring
{
    public IItemStore CreateStore() => new PersonalityRoster();

    public IItem CreateSample(int seed) => Lab02Wiring.Sample(seed);

    public Type PublisherType => typeof(PersonalityRoster);

    public Action Subscribe(IItemStore store, Action<IItem> handler)
    {
        var roster = Lab05Wiring.Roster(store);
        roster.ItemAdded += handler;
        return () => roster.ItemAdded -= handler;
    }

    public IReadOnlyList<IItem> Where(IItemStore store, Func<IItem, bool> keep) => Lab05Wiring.Roster(store).Filter(keep);

    public IReadOnlyList<string> Map(IItemStore store, Func<IItem, string> describe) => Lab05Wiring.Roster(store).Map(describe);

    public void ForEach(IItemStore store, Action<IItem> action) => Lab05Wiring.Roster(store).ForEach(action);
}
