using Capstone.Core.Model;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab08Wiring : ILab08Wiring
{
    public IItemStore CreateStore() => new PersonalityRoster();

    public IItem CreateSample(int seed) => Lab02Wiring.Sample(seed);

    public IReadOnlyList<Type> KindTypes => [typeof(LocalLegend), typeof(ViralMeme)];

    public IItem CreateOfKind(int kindIndex, int seed) => Lab02Wiring.OfKind(kindIndex, seed);

    public Task SaveAsync(IItemStore store, string path) => RosterArchive.SaveAsync(store, path);

    public async Task<IItemStore> LoadAsync(string path) => await RosterArchive.LoadAsync(path);
}
