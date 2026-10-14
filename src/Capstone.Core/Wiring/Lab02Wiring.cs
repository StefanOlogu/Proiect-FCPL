using Capstone.Core.Json;
using Capstone.Core.Model;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab02Wiring : ILab02Wiring
{
    private static readonly string[] Names =
    [
        "Barnaby the Rabbit",
        "Clucky the Hen",
        "Pip the Parrot",
        "Grizzly the Bear",
        "Captain Rusthook",
        "Marlow the Mole",
    ];

    public IItemStore CreateStore() => new AnimatronicRoster();

    public IItemMapper CreateMapper() => new AnimatronicMapper();

    public IItem CreateSample(int seed) => Sample(seed);

    internal static Animatronic Sample(int seed) => new()
    {
        Id = $"anim-{seed:D3}",                       // same seed → same Id, different seeds → different Ids
        Name = Names[Math.Abs(seed - 1) % Names.Length],
        Approach = (Approach)(Math.Abs(seed) % 4),
        Aggression = Math.Abs(seed) % (Animatronic.MaxAggression + 1),
        FirstNight = 1 + Math.Abs(seed) % Night.LastNight,
        Catchphrase = seed % 2 == 0 ? null : "Let's play!",
    };
}
