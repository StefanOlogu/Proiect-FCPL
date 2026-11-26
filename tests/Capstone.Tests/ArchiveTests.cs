using Capstone.Core.Model;
using Capstone.Core.Storage;
using static Capstone.Tests.PersonalityTests;

namespace Capstone.Tests;

/// <summary>Lab 08: what goes into the save file comes back out — same kinds, same values, same order.</summary>
public sealed class ArchiveTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "la-faleza-tests-" + Guid.NewGuid().ToString("N"));

    private string PathTo(string name) => Path.Combine(_folder, name);

    public void Dispose()
    {
        if (Directory.Exists(_folder)) Directory.Delete(_folder, recursive: true);
    }

    [Fact]
    public async Task Both_kinds_come_back_as_themselves_with_every_field()
    {
        var roster = new PersonalityRoster();
        roster.Add(Local(catchphrase: null, firstNight: 3));
        roster.Add(Meme());
        var path = PathTo("nested/folder/roster.json");   // folders that don't exist yet

        await RosterArchive.SaveAsync(roster, path);
        var loaded = await RosterArchive.LoadAsync(path);

        var local = Assert.IsType<LocalLegend>(loaded[0]);
        var meme = Assert.IsType<ViralMeme>(loaded[1]);
        Assert.Equal(("Mazepa", 3, (string?)null), (local.Neighbourhood, local.FirstNight, local.Catchphrase));
        Assert.Equal((2013, Entrance.Terrace), (meme.OriginYear, meme.Entrance));
    }

    [Fact]
    public async Task No_save_yet_is_an_empty_roster()
    {
        var loaded = await RosterArchive.LoadAsync(PathTo("never-saved.json"));

        Assert.Empty(loaded);
    }

    [Theory]
    [InlineData("{ not json")]
    [InlineData("""{ "hello": "world" }""")]
    [InlineData("""{ "format": "la-faleza/1", "personalities": [ { "kind": "meme", "id": "x" } ] }""")]   // a damaged entry
    public async Task A_file_that_is_not_a_good_save_is_an_error_not_an_empty_bar(string content)
    {
        var path = PathTo("bad.json");
        Directory.CreateDirectory(_folder);
        await File.WriteAllTextAsync(path, content);

        await Assert.ThrowsAsync<InvalidDataException>(() => RosterArchive.LoadAsync(path));
    }

    [Fact]
    public async Task Import_skips_broken_entries_and_people_already_there()
    {
        Directory.CreateDirectory(_folder);
        var path = PathTo("seed.json");
        await File.WriteAllTextAsync(path, """
            [
              { "kind": "meme", "id": "doge", "name": "Doge", "entrance": "Terrace", "aggression": 10, "originYear": 2013 },
              { "kind": "meme", "id": "broken", "name": "No Year", "entrance": "Terrace", "aggression": 1 },
              { "kind": "local", "id": "mama", "name": "Mama cu Papucul", "entrance": "FrontDoor", "aggression": 6, "neighbourhood": "Mazepa" }
            ]
            """);
        var roster = new PersonalityRoster();
        roster.Add(Local());   // "mama" is already here

        var added = await RosterArchive.ImportAsync(roster, path);

        Assert.Equal(1, added);
        Assert.Equal(2, roster.Count);
    }
}
