using Capstone.Core.Collections;
using Capstone.Core.Model;
using Capstone.Core.Storage;
using SharpQuest.Contracts;
using static Capstone.Tests.PersonalityTests;

namespace Capstone.Tests;

/// <summary>The hierarchy from outside (Lab 04), the roster as a collection (Lab 05) and as a publisher (Lab 06).</summary>
public class RosterTests
{
    private static PersonalityRoster RosterOf(params IItem[] items)
    {
        var roster = new PersonalityRoster();
        PersonalityCatalog.AddAll(roster, items);
        return roster;
    }

    [Fact]
    public void Classify_puts_each_kind_in_its_own_box()
    {
        Assert.Equal(PersonalityCatalog.LocalLabel, PersonalityCatalog.Classify(Local()));
        Assert.Equal(PersonalityCatalog.MemeLabel, PersonalityCatalog.Classify(Meme()));
        Assert.Equal(PersonalityCatalog.StrangerLabel, PersonalityCatalog.Classify(new ForeignItem("x", "A tourist")));
        Assert.Throws<ArgumentNullException>(() => PersonalityCatalog.Classify(null!));
    }

    [Fact]
    public void DescribeAll_lets_each_kind_speak_for_itself()
    {
        var lines = PersonalityCatalog.DescribeAll([Local(), Meme()]);

        Assert.Contains("local legend from Mazepa", lines[0]);
        Assert.Contains("viral since 2013", lines[1]);
    }

    [Theory]
    [InlineData("Doge", true)]
    [InlineData("doge", false)]   // exact, case-sensitive
    [InlineData("", false)]
    public void TryFindByName_finds_exact_names_only(string name, bool expected)
    {
        var roster = RosterOf(Local(), Meme());

        Assert.Equal(expected, PersonalityCatalog.TryFindByName(roster, name, out var found));
        Assert.Equal(expected, found is not null);
    }

    [Fact]
    public void Indexers_find_by_position_and_by_id()
    {
        var roster = RosterOf(Local(), Meme());

        Assert.Equal("doge", roster[1].Id);
        Assert.Equal("Mama cu Papucul", roster["mama"]?.DisplayName);
        Assert.Null(roster["nobody"]);
        Assert.Throws<ArgumentOutOfRangeException>(() => roster[2]);
    }

    [Fact]
    public void Matching_is_lazy_and_ignores_case()
    {
        var roster = RosterOf(Local());
        var matches = roster.Matching("DOGE");

        roster.Add(Meme());   // added after asking, before enumerating

        Assert.Equal("doge", Assert.Single(matches).Id);
    }

    [Fact]
    public void ItemAdded_reaches_both_subscribers_after_the_add()
    {
        var roster = new PersonalityRoster();
        var log = new List<string>();
        roster.ItemAdded += item => log.Add($"first:{item.Id}:{roster.Count}");
        roster.ItemAdded += item => log.Add($"second:{item.Id}");

        roster.Add(Meme());
        Record.Exception(() => roster.Add(Meme()));   // duplicate: rejected, announces nothing

        Assert.Equal(["first:doge:1", "second:doge"], log);
    }

    [Fact]
    public void Filter_Map_and_ForEach_run_the_callers_function()
    {
        var roster = RosterOf(Local(), Meme(), Meme(id: "doge-2"));
        var seen = 0;

        var memes = roster.Filter(item => item is ViralMeme);
        var ids = roster.Map(item => item.Id.ToUpperInvariant());
        roster.ForEach(_ => seen++);

        Assert.Equal(2, memes.Count);
        Assert.Equal(["MAMA", "DOGE", "DOGE-2"], ids);
        Assert.Equal(3, seen);
        Assert.Throws<ArgumentNullException>(() => roster.Filter(null!));
    }

    [Fact]
    public void RecentlySeen_keeps_the_newest_first_without_duplicates()
    {
        var cameras = new RecentlySeen<string>(2);

        foreach (var seen in new[] { "Doge", "Trollface", "Doge", "Stonks Man" })
            cameras.Push(seen);

        Assert.Equal(["Stonks Man", "Doge"], cameras.ToList());
        Assert.Equal("Stonks Man", cameras.Latest);
    }

    [Fact]
    public void RecentlySeen_needs_room_for_at_least_one()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RecentlySeen<int>(0));
    }
}
