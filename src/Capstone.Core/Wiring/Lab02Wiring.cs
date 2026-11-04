using Capstone.Core.Json;
using Capstone.Core.Model;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.Wiring;

public sealed class Lab02Wiring : ILab02Wiring
{
    private static readonly (string Name, string Neighbourhood)[] Locals =
    [
        ("Mama cu Papucul", "Mazepa"),
        ("Vecinul cu Bormașina", "Țiglina"),
        ("Bunica cu Sarmale", "Micro 19"),
        ("Nea Taximetristul", "Siderurgiștilor"),
        ("Pescarul de pe Faleză", "Faleza Dunării"),
        ("Curentul", "Bădălan"),
    ];

    private static readonly (string Name, int Year)[] Memes =
    [
        ("Stonks Man", 2017),
        ("This Is Fine Dog", 2013),
        ("Doge", 2013),
        ("Trollface", 2008),
        ("Sigma Grindset", 2021),
        ("The NPC", 2018),
    ];

    public IItemStore CreateStore() => new PersonalityRoster();

    public IItemMapper CreateMapper() => new PersonalityMapper();

    public IItem CreateSample(int seed) => Sample(seed);

    /// <summary>Odd seeds are memes, even seeds are locals, so a store of samples always has both kinds.</summary>
    internal static Personality Sample(int seed) => OfKind(Math.Abs(seed) % 2 == 0 ? 0 : 1, seed);

    /// <summary>Kind 0 = <see cref="LocalLegend"/>, kind 1 = <see cref="ViralMeme"/>. The prefix keeps the kinds' Ids apart.</summary>
    internal static Personality OfKind(int kind, int seed)
    {
        var n = Math.Abs(seed);
        var entrance = (Entrance)(n % 4);
        var aggression = n % (Personality.MaxAggression + 1);
        var firstNight = 1 + n % Night.LastNight;
        var catchphrase = n % 3 == 0 ? null : "Încă o bere și plec!";

        return kind switch
        {
            0 => new LocalLegend
            {
                Id = $"local-{seed:D3}",                    // same seed → same Id, different seeds → different Ids
                Name = Locals[n % Locals.Length].Name,
                Neighbourhood = Locals[n % Locals.Length].Neighbourhood,
                Entrance = entrance, Aggression = aggression, FirstNight = firstNight, Catchphrase = catchphrase,
            },
            1 => new ViralMeme
            {
                Id = $"meme-{seed:D3}",
                Name = Memes[n % Memes.Length].Name,
                OriginYear = Memes[n % Memes.Length].Year,
                Entrance = entrance, Aggression = aggression, FirstNight = firstNight, Catchphrase = catchphrase,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Kind 0 is a local legend, kind 1 a viral meme."),
        };
    }
}
