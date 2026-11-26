using System.Text.Json;
using Capstone.Core.Game;
using Capstone.Core.Json;
using Capstone.Core.Model;
using Capstone.Core.Queries;
using Capstone.Core.Storage;

// Night Shift at Bar "La Faleză", Galați. Run it with:
//   dotnet run --project src/Capstone.App            → night 1
//   dotnet run --project src/Capstone.App -- 4       → night 4

Console.OutputEncoding = System.Text.Encoding.UTF8;

var roster = new PersonalityRoster();

// One publisher, two subscribers (Lab 06): a log line, and a tally per entrance.
var perEntrance = new Dictionary<Entrance, int>();
roster.ItemAdded += item => Console.WriteLine($"  + {item.DisplayName} is somewhere in Galați tonight");
roster.ItemAdded += item =>
{
    if (item is Personality p) perEntrance[p.Entrance] = perEntrance.GetValueOrDefault(p.Entrance) + 1;
};

var mapper = new PersonalityMapper();
using (var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "personalities.json"))))
{
    foreach (var element in doc.RootElement.EnumerateArray())
        if (mapper.FromJson(element.GetRawText()) is { } item) roster.Add(item);
        else Console.WriteLine($"  ! skipped invalid entry: {element.GetRawText()}");
}

Console.WriteLine();
Console.WriteLine("Who's coming, and from where:");
foreach (var (entrance, count) in perEntrance) Console.WriteLine($"  {entrance,-12} {count}");

Console.WriteLine();
foreach (var personality in roster.Filter(item => item is Personality).Cast<Personality>())
    Console.WriteLine($"  [{PersonalityCatalog.Classify(personality)}] {personality.Describe()}");

var nightNumber = args.Length > 0 && int.TryParse(args[0], out var n) ? Math.Clamp(n, Night.FirstNight, Night.LastNight) : Night.FirstNight;
var night = new Night(nightNumber);
roster.ForEach(item => { if (item is Personality p) night.Add(p); });

// Five questions a bartender asks before a shift (Lab 07).
Console.WriteLine();
Console.WriteLine($"Most dangerous tonight: {string.Join(", ", roster.ThreatsOn(nightNumber).Take(3).Select(p => $"{p.Name} ({p.Aggression})"))}");
Console.WriteLine($"Busiest entrance:       {roster.TrafficByEntrance().First().Entrance}");
Console.WriteLine($"Locals come from:       {string.Join(", ", roster.Neighbourhoods())}");
Console.WriteLine($"Oldest meme:            {roster.OldestMeme()?.Name ?? "none"} ({roster.OldestMeme()?.OriginYear})");
Console.WriteLine($"No warning from:        {string.Join(", ", roster.SilentOnes().Select(p => p.Name))}");

var shift = new NightShift(night, sides => Random.Shared.Next(1, sides + 1));
shift.Moved += (p, distance) =>
{
    // The silent ones (Catchphrase == null) give no warning. That's the point of them.
    if (distance == 0 && p.Catchphrase is { } line) Console.WriteLine($"  At the {p.Entrance}, someone says: \"{line}\"");
};
shift.Blocked += p => Console.WriteLine($"  BANG. Something hit the closed {p.Entrance} and walked away.");
shift.Blackout += () => Console.WriteLine("  The fuse box dies. Every door swings open...");
shift.Ended += outcome => Console.WriteLine(outcome == ShiftOutcome.Survived
    ? "\n  06:00. The sun rises over the Danube. You survived."
    : $"\n  {shift.CaughtBy?.Jumpscare()}\n  GAME OVER.");

Console.WriteLine();
Console.WriteLine($"{night.Describe()} at Bar \"La Faleză\", on the Danube promenade in Galați.");
Console.WriteLine("Keys: +f close, -f open, ?f camera   (f = front door, t = terrace, k = kitchen, c = cellar)   Enter = wait");

var keys = new Dictionary<char, Entrance>
{
    ['f'] = Entrance.FrontDoor, ['t'] = Entrance.Terrace, ['k'] = Entrance.KitchenDoor, ['c'] = Entrance.CellarHatch,
};

while (shift.Outcome == ShiftOutcome.InProgress)
{
    var closed = string.Join(" ", keys.Where(k => shift.IsClosed(k.Value)).Select(k => k.Key));
    Console.Write($"{NightShiftClock.Format(shift.Hour)}  power {shift.Power,3}%  closed [{closed}] > ");

    var input = Console.ReadLine()?.Trim().ToLowerInvariant();
    if (input is null) { shift.Advance(); Console.WriteLine(); continue; }   // no keyboard: just let the night run
    if (input.Length == 0) { shift.Advance(); continue; }
    if (input.Length != 2 || !keys.TryGetValue(input[1], out var entrance)) { Console.WriteLine("  ?"); continue; }

    switch (input[0])
    {
        case '+': shift.Close(entrance); break;
        case '-': shift.Open(entrance); break;
        case '?':
            var seen = shift.Watch(entrance);
            Console.WriteLine(seen is null ? "  Camera: nobody." : $"  Camera: {seen.Name}, {shift.DistanceOf(seen)} step(s) away.");
            break;
        default: Console.WriteLine("  ?"); break;
    }
}
