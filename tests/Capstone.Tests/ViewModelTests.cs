using Capstone.Core.Game;
using Capstone.Core.Model;
using Capstone.Core.Storage;
using Capstone.Core.ViewModels;
using static Capstone.Tests.PersonalityTests;

namespace Capstone.Tests;

/// <summary>Lab 09: the screens, driven without a screen.</summary>
public class ViewModelTests
{
    private static (PersonalityRoster Roster, RosterViewModel Vm) Open()
    {
        var roster = new PersonalityRoster();
        roster.Add(Local());
        roster.Add(Meme());
        return (roster, new RosterViewModel(roster, savePath: Path.Combine(Path.GetTempPath(), "unused.json")));
    }

    [Fact]
    public void The_search_box_ignores_romanian_diacritics()
    {
        var (_, vm) = Open();

        vm.Filter = "papucul";

        Assert.Equal("mama", Assert.Single(vm.Items).Id);
    }

    [Fact]
    public void Removing_from_the_roster_anywhere_updates_the_list_and_the_selection()
    {
        var (roster, vm) = Open();
        vm.Selected = roster["doge"];

        roster.Remove("doge");

        Assert.Single(vm.Items);
        Assert.Null(vm.Selected);
        Assert.Equal(1, vm.Count);
    }

    [Fact]
    public void A_barred_personality_can_be_let_back_in()
    {
        var (roster, vm) = Open();
        vm.Selected = roster["doge"];

        vm.RemoveCommand.Execute(null);

        var barred = Assert.Single(vm.Barred);
        Assert.False(vm.UnbarCommand.CanExecute(null));   // nobody picked in the barred list yet

        vm.SelectedBarred = barred;
        vm.UnbarCommand.Execute(null);

        Assert.Same(barred, roster["doge"]);   // the same personality, every field intact
        Assert.Empty(vm.Barred);
        Assert.Null(vm.SelectedBarred);
    }

    [Fact]
    public async Task Barring_survives_closing_the_app()
    {
        var path = Path.Combine(Path.GetTempPath(), $"la-faleza-{Guid.NewGuid():N}.json");
        try
        {
            var (roster, vm) = Open();
            vm.Selected = roster["doge"];
            vm.RemoveCommand.Execute(null);
            await RosterArchive.SaveAsync(roster, path, vm.Barred);

            var reopened = new RosterViewModel(new PersonalityRoster(), path);
            await reopened.LoadAsync();

            Assert.Equal("mama", Assert.Single(reopened.Items).Id);
            Assert.Equal("doge", Assert.Single(reopened.Barred).Id);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void The_form_adds_a_meme()
    {
        var (roster, vm) = Open();
        vm.NewName = "Trollface";
        vm.NewIsMeme = true;
        vm.NewOrigin = "2008";

        vm.CreateCommand.Execute(null);

        Assert.Equal(3, roster.Count);
        Assert.Equal(2008, Assert.IsType<ViralMeme>(roster[2]).OriginYear);
        Assert.Equal("", vm.NewName);   // the form clears for the next one
    }

    [Theory]
    [InlineData(true, "not a year")]
    [InlineData(false, "   ")]   // a local legend without a neighbourhood
    public void A_bad_form_is_a_message_not_a_crash(bool isMeme, string origin)
    {
        var (roster, vm) = Open();
        vm.NewName = "Somebody";
        vm.NewIsMeme = isMeme;
        vm.NewOrigin = origin;

        vm.CreateCommand.Execute(null);

        Assert.Equal(2, roster.Count);
        Assert.False(string.IsNullOrWhiteSpace(vm.Status));
    }

    [Fact]
    public void The_game_screen_plays_a_shift()
    {
        var roster = new PersonalityRoster();
        roster.Add(Meme(aggression: 10));
        var game = new NightShiftViewModel(roster, _ => 1);   // the dice always say "move"

        Assert.False(game.WaitCommand.CanExecute(null));      // nothing to wait for yet
        game.StartCommand.Execute(null);
        var terrace = game.Doors.Single(d => d.Entrance == Entrance.Terrace);
        terrace.ToggleCommand.Execute(null);

        for (var i = 0; i < 4; i++) game.WaitCommand.Execute(null);

        Assert.True(terrace.IsClosed);
        Assert.True(game.IsPlaying);
        Assert.Equal("01:00", game.Clock);
        Assert.Contains(game.Log, line => line.Contains("BANG"));
    }

    [Fact]
    public void The_briefing_answers_for_the_chosen_night_and_follows_the_roster()
    {
        var roster = new PersonalityRoster();
        roster.Add(Local(id: "late", firstNight: 4));
        var game = new NightShiftViewModel(roster);
        var announced = new List<string?>();
        game.PropertyChanged += (_, e) => announced.Add(e.PropertyName);

        Assert.Equal("nobody moves tonight", game.Briefing[0].Answer);   // night 1: "late" isn't here yet

        game.NightNumber = 4;
        roster.Add(Meme());

        Assert.Equal("Doge (5), Mama cu Papucul (5)", game.Briefing[0].Answer);   // a tie: alphabetical
        Assert.Equal("Doge (2013)", game.Briefing[3].Answer);
        Assert.Equal(2, announced.Count(name => name == nameof(NightShiftViewModel.Briefing)));
    }

    [Fact]
    public void Leaving_a_door_open_ends_the_game()
    {
        var roster = new PersonalityRoster();
        roster.Add(Meme(aggression: 10));
        var game = new NightShiftViewModel(roster, _ => 1);

        game.StartCommand.Execute(null);
        for (var i = 0; i <= NightShift.StartDistance; i++) game.WaitCommand.Execute(null);

        Assert.False(game.IsPlaying);
        Assert.StartsWith("GAME OVER", game.Headline);
        Assert.True(game.StartCommand.CanExecute(null));   // try again
    }
}
