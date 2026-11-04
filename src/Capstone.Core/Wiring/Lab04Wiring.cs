using Capstone.Core.Model;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab04Wiring : ILab04Wiring
{
    public IItemStore CreateStore() => new PersonalityRoster();

    public Type BaseType => typeof(Personality);

    public IReadOnlyList<Type> KindTypes => [typeof(LocalLegend), typeof(ViralMeme)];

    public IItem CreateOfKind(int kindIndex, int seed) => Lab02Wiring.OfKind(kindIndex, seed);

    public string Describe(IItem item) => PersonalityCatalog.AsPersonality(item).Describe();

    public IReadOnlyList<string> DescribeAll(IItemStore store) => PersonalityCatalog.DescribeAll(store.All());

    public string Classify(IItem item) => PersonalityCatalog.Classify(item);

    public bool TryFindByName(IItemStore store, string name, out IItem? found) =>
        PersonalityCatalog.TryFindByName(store, name, out found);

    public int AddAll(IItemStore store, params IItem[] items) => PersonalityCatalog.AddAll(store, items);
}
