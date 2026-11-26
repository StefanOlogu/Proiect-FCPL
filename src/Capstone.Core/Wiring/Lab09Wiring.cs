using Capstone.Core.Storage;
using Capstone.Core.ViewModels;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab09Wiring : ILab09Wiring
{
    public IItemStore CreateStore() => new PersonalityRoster();

    public IItem CreateSample(int seed) => Lab02Wiring.Sample(seed);

    public ICatalogueViewModel CreateViewModel(IItemStore store) => new RosterViewModel(Lab05Wiring.Roster(store));
}
