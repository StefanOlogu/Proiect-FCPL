using Capstone.Core.Json;
using Capstone.Core.Model;

namespace Capstone.Tests;

/// <summary>Tests over our own domain logic (Lab 03 asks for at least 5).</summary>
public class AnimatronicTests
{
    private static Animatronic Make(int aggression = 5, int firstNight = 1, string id = "barnaby", string? catchphrase = null) => new()
    {
        Id = id,
        Name = "Barnaby the Rabbit",
        Approach = Approach.LeftDoor,
        Aggression = aggression,
        FirstNight = firstNight,
        Catchphrase = catchphrase,
    };

    // The theory: one check, several nights.
    [Theory]
    [InlineData(1, false)]   // before its first night
    [InlineData(2, false)]
    [InlineData(3, true)]    // exactly its first night
    [InlineData(7, true)]
    public void Active_only_from_its_first_night(int night, bool expected)
    {
        Assert.Equal(expected, Make(firstNight: 3).IsActiveOn(night));
    }

    [Fact]
    public void Aggression_zero_is_never_active()
    {
        Assert.False(Make(aggression: 0).IsActiveOn(7));
    }

    // Rejecting bad input.
    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void Aggression_out_of_range_is_rejected(int aggression)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Make(aggression: aggression));
    }

    [Fact]
    public void A_blank_id_is_rejected()
    {
        Assert.Throws<ArgumentException>(() => Make(id: "  "));
    }

    [Fact]
    public void A_night_only_counts_its_active_animatronics()
    {
        var night = new Night(2);
        night.Add(Make(id: "awake", firstNight: 1));
        night.Add(Make(id: "later", firstNight: 4));

        Assert.Equal(2, night.Animatronics.Count);
        Assert.Equal("awake", Assert.Single(night.Active).Id);
    }

    [Fact]
    public void A_night_outside_one_to_seven_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Night(8));
    }

    [Fact]
    public void Json_round_trip_keeps_a_silent_animatronic_silent()
    {
        var mapper = new AnimatronicMapper();

        var copy = Assert.IsType<Animatronic>(mapper.FromJson(mapper.ToJson(Make(catchphrase: null))));

        Assert.Null(copy.Catchphrase);
        Assert.Equal(Approach.LeftDoor, copy.Approach);
    }
}
