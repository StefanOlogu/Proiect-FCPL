using Capstone.Core.Search;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab03Wiring : ILab03Wiring
{
    public IItemStore CreateStore() => new AnimatronicRoster();

    public IItem CreateSample(int seed) => Lab02Wiring.Sample(seed);

    public IReadOnlyList<IItem> Search(IItemStore store, string query) => new RosterSearch().Search(store, query);
}
