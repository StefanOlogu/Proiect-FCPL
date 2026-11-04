using Capstone.Core.Json;
using Capstone.Core.Model;

namespace Capstone.Tests;

/// <summary>Tests over the personalities themselves: rules, the two kinds, JSON.</summary>
public class PersonalityTests
{
    internal static LocalLegend Local(int aggression = 5, int firstNight = 1, string id = "mama", string? catchphrase = null) => new()
    {
        Id = id,
        Name = "Mama cu Papucul",
        Neighbourhood = "Mazepa",
        Entrance = Entrance.FrontDoor,
        Aggression = aggression,
        FirstNight = firstNight,
        Catchphrase = catchphrase,
    };

    internal static ViralMeme Meme(int aggression = 5, string id = "doge", Entrance entrance = Entrance.Terrace) => new()
    {
        Id = id,
        Name = "Doge",
        OriginYear = 2013,
        Entrance = entrance,
        Aggression = aggression,
        Catchphrase = "Much bar. Very night. Wow.",
    };

    // The theory: one check, several nights.
    [Theory]
    [InlineData(1, false)]   // before its first night
    [InlineData(2, false)]
    [InlineData(3, true)]    // exactly its first night
    [InlineData(7, true)]
    public void Active_only_from_its_first_night(int night, bool expected)
    {
        Assert.Equal(expected, Local(firstNight: 3).IsActiveOn(night));
    }

    [Fact]
    public void Aggression_zero_is_never_active()
    {
        Assert.False(Local(aggression: 0).IsActiveOn(7));
    }

    // Rejecting bad input.
    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void Aggression_out_of_range_is_rejected(int aggression)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Local(aggression: aggression));
    }

    [Fact]
    public void A_blank_id_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => Local(id: "  "));
    }

    [Fact]
    public void A_local_legend_needs_a_neighbourhood()
    {
        Assert.Throws<ArgumentException>(() => new LocalLegend
        {
            Id = "x", Name = "Nobody", Neighbourhood = " ", Entrance = Entrance.Terrace, Aggression = 1,
        });
    }

    [Theory]
    [InlineData(1989)]
    [InlineData(3000)]
    public void A_meme_from_before_the_internet_or_the_future_is_rejected(int year)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ViralMeme
        {
            Id = "x", Name = "Nobody", OriginYear = year, Entrance = Entrance.Terrace, Aggression = 1,
        });
    }

    // The two overrides of AggressionAt pull in opposite directions.
    [Theory]
    [InlineData(0, 5)]
    [InlineData(3, 8)]
    [InlineData(5, 10)]
    public void Locals_get_bolder_every_hour(int hour, int expected)
    {
        Assert.Equal(expected, Local(aggression: 5).AggressionAt(hour));
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(2, 1)]
    [InlineData(3, 0)]   // 5 − 2·3 would be −1: never below zero
    public void Memes_go_stale_every_hour(int hour, int expected)
    {
        Assert.Equal(expected, Meme(aggression: 5).AggressionAt(hour));
    }

    [Fact]
    public void A_local_legend_never_goes_above_the_maximum()
    {
        Assert.Equal(Personality.MaxAggression, Local(aggression: 19).AggressionAt(5));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(6)]
    public void An_hour_outside_the_shift_is_rejected(int hour)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Meme().AggressionAt(hour));
    }

    [Fact]
    public void A_silent_local_gives_no_quote_when_it_jumps_you()
    {
        var silent = Local(catchphrase: null);

        Assert.Contains("(silent)", silent.Describe());
        Assert.DoesNotContain("\"", silent.Jumpscare());
    }

    [Fact]
    public void A_night_only_counts_its_active_personalities()
    {
        var night = new Night(2);
        night.Add(Local(id: "awake", firstNight: 1));
        night.Add(Local(id: "later", firstNight: 4));

        Assert.Equal(2, night.Personalities.Count);
        Assert.Equal("awake", Assert.Single(night.Active).Id);
    }

    [Fact]
    public void A_night_outside_one_to_seven_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Night(8));
    }

    [Fact]
    public void Json_round_trip_keeps_the_kind_and_a_silent_local_silent()
    {
        var mapper = new PersonalityMapper();

        var local = Assert.IsType<LocalLegend>(mapper.FromJson(mapper.ToJson(Local(catchphrase: null))));
        var meme = Assert.IsType<ViralMeme>(mapper.FromJson(mapper.ToJson(Meme())));

        Assert.Null(local.Catchphrase);
        Assert.Equal("Mazepa", local.Neighbourhood);
        Assert.Equal(2013, meme.OriginYear);
    }

    [Theory]
    [InlineData("""{ "kind": "meme", "id": "m", "name": "M", "entrance": "Terrace", "aggression": 3 }""")]       // no year
    [InlineData("""{ "kind": "local", "id": "l", "name": "L", "entrance": "Terrace", "aggression": 3 }""")]      // no neighbourhood
    [InlineData("""{ "kind": "ghost", "id": "g", "name": "G", "entrance": "Terrace", "aggression": 3 }""")]      // unknown kind
    public void Json_missing_what_its_kind_needs_is_rejected(string json)
    {
        Assert.Null(new PersonalityMapper().FromJson(json));
    }
}
