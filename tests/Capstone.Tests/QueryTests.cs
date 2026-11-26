using Capstone.Core.Model;
using Capstone.Core.Queries;
using SharpQuest.Contracts;
using static Capstone.Tests.PersonalityTests;

namespace Capstone.Tests;

/// <summary>Lab 07: the questions the bar asks about its personalities.</summary>
public class QueryTests
{
    private static readonly IItem[] Bar =
    [
        Local(id: "mama", aggression: 6),
        Local(id: "mama-late", aggression: 9, firstNight: 4, catchphrase: "Vino-ncoace!"),
        Meme(id: "doge", aggression: 10),
        Meme(id: "stonks", aggression: 3, entrance: Entrance.FrontDoor),
    ];

    [Fact]
    public void Threats_tonight_are_only_the_active_ones_most_dangerous_first()
    {
        var ids = Bar.ThreatsOn(night: 1).Select(p => p.Id);

        Assert.Equal(["doge", "mama", "stonks"], ids);   // mama-late doesn't show up until night 4
    }

    [Fact]
    public void Traffic_counts_each_entrance_busiest_first()
    {
        var traffic = Bar.TrafficByEntrance().ToList();

        Assert.Equal((Entrance.FrontDoor, 3), traffic[0]);
        Assert.Equal((Entrance.Terrace, 1), traffic[1]);
        Assert.Equal(2, traffic.Count);
    }

    [Fact]
    public void Neighbourhoods_are_listed_once_each()
    {
        Assert.Equal(["Mazepa"], Bar.Neighbourhoods());
    }

    [Fact]
    public void The_oldest_meme_is_found_and_no_memes_means_null()
    {
        Assert.NotNull(Bar.OldestMeme());
        Assert.Null(Bar.OfType<LocalLegend>().OldestMeme());
    }

    [Fact]
    public void The_silent_ones_have_no_catchphrase()
    {
        Assert.Equal(["mama"], Bar.SilentOnes().Select(p => p.Id));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, 2)]
    [InlineData(10, 4)]   // asking for more than there are gives all of them
    public void Longest_takes_at_most_n(int n, int expected)
    {
        Assert.Equal(expected, Bar.Longest(n).Count());
    }

    [Fact]
    public void A_negative_count_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Bar.Longest(-1));
    }

    [Fact]
    public void Summary_of_nothing_is_zeroes_instead_of_a_crash()
    {
        Assert.Equal(new StoreStats(0, 0, null, 0), Array.Empty<IItem>().Summarise());
    }
}
