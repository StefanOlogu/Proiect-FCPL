using Capstone.Core.Game;
using Capstone.Core.Model;
using static Capstone.Tests.PersonalityTests;

namespace Capstone.Tests;

/// <summary>The game itself. The dice are a function, so the tests pass ones that never surprise them.</summary>
public class NightShiftTests
{
    private static readonly Func<int, int> AlwaysMoves = _ => 1;
    private static readonly Func<int, int> NeverMoves = sides => sides;   // a 20 beats every aggression below 20

    private static NightShift ShiftWith(Func<int, int> roll, params Personality[] personalities)
    {
        var night = new Night(1);
        foreach (var p in personalities) night.Add(p);
        return new NightShift(night, roll);
    }

    [Fact]
    public void An_empty_bar_survives_till_six()
    {
        var shift = ShiftWith(AlwaysMoves);
        ShiftOutcome? ended = null;
        shift.Ended += outcome => ended = outcome;

        for (var i = 0; i < NightShift.TotalTicks; i++) shift.Advance();

        Assert.Equal(ShiftOutcome.Survived, ended);
        Assert.Throws<InvalidOperationException>(shift.Advance);
    }

    [Fact]
    public void An_open_entrance_lets_them_in()
    {
        var doge = Meme(aggression: 10);
        var shift = ShiftWith(AlwaysMoves, doge);

        for (var i = 0; i <= NightShift.StartDistance; i++) shift.Advance();

        Assert.Equal(ShiftOutcome.Caught, shift.Outcome);
        Assert.Same(doge, shift.CaughtBy);
    }

    [Fact]
    public void A_closed_entrance_sends_them_back_down_the_street()
    {
        var doge = Meme(aggression: 10);
        var shift = ShiftWith(AlwaysMoves, doge);
        var blocked = 0;
        shift.Blocked += _ => blocked++;
        shift.Close(doge.Entrance);

        for (var i = 0; i <= NightShift.StartDistance; i++) shift.Advance();

        Assert.Equal(ShiftOutcome.InProgress, shift.Outcome);
        Assert.Equal(1, blocked);
        Assert.Equal(NightShift.StartDistance, shift.DistanceOf(doge));
    }

    [Fact]
    public void Nobody_moves_when_the_dice_say_no()
    {
        var doge = Meme(aggression: 10);
        var shift = ShiftWith(NeverMoves, doge);

        shift.Advance();

        Assert.Equal(NightShift.StartDistance, shift.DistanceOf(doge));
    }

    [Fact]
    public void Closed_doors_drain_the_power_until_a_blackout_opens_them()
    {
        var shift = ShiftWith(NeverMoves);
        var blackouts = 0;
        shift.Blackout += () => blackouts++;
        foreach (var entrance in Enum.GetValues<Entrance>()) shift.Close(entrance);

        while (shift.Power > 0) shift.Advance();   // four doors at once can't last the night

        Assert.Equal(ShiftOutcome.InProgress, shift.Outcome);
        Assert.Equal(1, blackouts);
        Assert.False(shift.IsClosed(Entrance.FrontDoor));
    }

    [Fact]
    public void The_camera_shows_the_closest_one_and_remembers_it()
    {
        var far = Meme(id: "far");
        var near = Meme(id: "near", aggression: 19);
        var shift = ShiftWith(sides => sides - 1, far, near);   // a 19: only "near" moves

        shift.Advance();

        Assert.Same(near, shift.Watch(Entrance.Terrace));
        Assert.Null(shift.Watch(Entrance.CellarHatch));
        Assert.Same(near, shift.CameraHistory.Latest);
    }
}
