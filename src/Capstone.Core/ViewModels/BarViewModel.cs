namespace Capstone.Core.ViewModels;

/// <summary>The whole window: the roster tab and the night-shift tab, over the same roster.</summary>
public sealed class BarViewModel(RosterViewModel roster, NightShiftViewModel night)
{
    public RosterViewModel Roster { get; } = roster;

    public NightShiftViewModel Night { get; } = night;

    public string Title => "Night Shift at Bar \"La Faleză\" · Galați";
}
