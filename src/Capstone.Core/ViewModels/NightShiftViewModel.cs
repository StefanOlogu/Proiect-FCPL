using System.Collections.ObjectModel;
using System.Windows.Input;
using Capstone.Core.Game;
using Capstone.Core.Model;
using Capstone.Core.Queries;
using Capstone.Core.Storage;

namespace Capstone.Core.ViewModels;

/// <summary>
/// What the game screen binds to: the clock, the fuse box, one panel per entrance, and the log of
/// what you heard. One click of Wait is one quarter of an hour.
/// </summary>
public sealed class NightShiftViewModel : ObservableObject
{
    private readonly PersonalityRoster _roster;
    private readonly Func<int, int> _roll;
    private readonly RelayCommand _waitCommand;
    private readonly RelayCommand _startCommand;

    private NightShift? _shift;
    private int _nightNumber = Night.FirstNight;
    private string _headline = "Pick a night and open the bar.";

    /// <param name="roll">The dice for every shift this screen starts. Tests pass a fixed one.</param>
    public NightShiftViewModel(PersonalityRoster roster, Func<int, int>? roll = null)
    {
        ArgumentNullException.ThrowIfNull(roster);
        _roster = roster;
        _roll = roll ?? (sides => Random.Shared.Next(1, sides + 1));

        _startCommand = new RelayCommand(_ => Start(), _ => !IsPlaying);
        _waitCommand = new RelayCommand(_ => Wait(), _ => IsPlaying);
        Doors = Enum.GetValues<Entrance>().Select(e => new DoorViewModel(this, e)).ToList();

        // The briefing answers questions about the roster, so it follows the roster.
        _roster.ItemAdded += _ => OnPropertyChanged(nameof(Briefing));
        _roster.ItemRemoved += _ => OnPropertyChanged(nameof(Briefing));
    }

    public IReadOnlyList<int> Nights { get; } = Enumerable.Range(Night.FirstNight, Night.LastNight).ToList();

    public int NightNumber
    {
        get => _nightNumber;
        set
        {
            if (SetField(ref _nightNumber, Math.Clamp(value, Night.FirstNight, Night.LastNight)))
                OnPropertyChanged(nameof(Briefing));
        }
    }

    /// <summary>The five questions a bartender asks before the chosen night (Lab 07's queries), answered.</summary>
    public IReadOnlyList<BriefingLine> Briefing
    {
        get
        {
            var threats = _roster.ThreatsOn(NightNumber).Take(3).Select(p => $"{p.Name} ({p.Aggression})").ToList();
            var traffic = _roster.TrafficByEntrance().FirstOrDefault();
            var oldest = _roster.OldestMeme();
            var silent = _roster.SilentOnes().Select(p => p.Name).ToList();

            return
            [
                new("Most dangerous tonight", threats.Count == 0 ? "nobody moves tonight" : string.Join(", ", threats)),
                new("Busiest entrance", traffic.Count == 0 ? "none" : $"{traffic.Entrance} ({traffic.Count} personalities)"),
                new("Locals come from", string.Join(", ", _roster.Neighbourhoods()).OrIfEmpty("nowhere: no locals")),
                new("Oldest meme", oldest is null ? "no memes" : $"{oldest.Name} ({oldest.OriginYear})"),
                new("No warning from", string.Join(", ", silent).OrIfEmpty("nobody: everyone talks")),
            ];
        }
    }

    public bool IsPlaying => _shift?.Outcome == ShiftOutcome.InProgress;

    /// <summary>Each tick is a quarter of an hour: tick 3 is 00:45, the last tick is 06:00.</summary>
    public string Clock => _shift is null ? "--:--" : $"{_shift.Tick / NightShift.TicksPerHour:D2}:{_shift.Tick % NightShift.TicksPerHour * 15:D2}";

    public int Power => _shift?.Power ?? NightShift.FullPower;

    /// <summary>The big line on the screen: what just happened, or how it ended.</summary>
    public string Headline
    {
        get => _headline;
        private set => SetField(ref _headline, value);
    }

    /// <summary>Everything heard tonight, newest first.</summary>
    public ObservableCollection<string> Log { get; } = [];

    public IReadOnlyList<DoorViewModel> Doors { get; }

    public ICommand StartCommand => _startCommand;

    public ICommand WaitCommand => _waitCommand;

    internal NightShift? Shift => _shift;

    private void Start()
    {
        var night = new Night(NightNumber);
        foreach (var p in _roster.OfType<Personality>()) night.Add(p);

        _shift = new NightShift(night, _roll);
        _shift.Moved += (p, distance) =>
        {
            // The silent ones give no warning — that's the point of a null catchphrase.
            if (distance == 0 && p.Catchphrase is { } line) Say($"At the {p.Entrance}: \"{line}\"");
        };
        _shift.Blocked += p => Say($"BANG on the {p.Entrance}. Footsteps going away.");
        _shift.Blackout += () => Say("The fuse box dies. Every door swings open.");
        _shift.Ended += outcome => Headline = outcome == ShiftOutcome.Survived
            ? "06:00. The sun comes up over the Danube. You survived."
            : $"GAME OVER. {_shift.CaughtBy?.Jumpscare()}";

        Log.Clear();
        Headline = $"{night.Describe()}. Bar \"La Faleză\" is closed. Nobody gets in.";
        Changed();
    }

    private void Wait()
    {
        _shift?.Advance();
        Changed();
    }

    private void Say(string line) => Log.Insert(0, $"{Clock}  {line}");

    /// <summary>Everything on screen depends on the shift; after every move, tell the window to look again.</summary>
    internal void Changed()
    {
        OnPropertyChanged(nameof(IsPlaying));
        OnPropertyChanged(nameof(Clock));
        OnPropertyChanged(nameof(Power));
        _startCommand.RaiseCanExecuteChanged();
        _waitCommand.RaiseCanExecuteChanged();
        foreach (var door in Doors) door.Changed();
    }
}

/// <summary>One question of the briefing and its answer.</summary>
public sealed record BriefingLine(string Question, string Answer);

internal static class StringExtensions
{
    public static string OrIfEmpty(this string text, string fallback) => text.Length == 0 ? fallback : text;
}

/// <summary>One entrance on the game screen: its door, and its camera.</summary>
public sealed class DoorViewModel : ObservableObject
{
    private readonly NightShiftViewModel _game;
    private string _camera = "Camera off.";

    internal DoorViewModel(NightShiftViewModel game, Entrance entrance)
    {
        _game = game;
        Entrance = entrance;
        ToggleCommand = new RelayCommand(_ => Toggle(), _ => game.IsPlaying);
        WatchCommand = new RelayCommand(_ => Watch(), _ => game.IsPlaying);
    }

    public Entrance Entrance { get; }

    public bool IsClosed => _game.Shift?.IsClosed(Entrance) ?? false;

    public string DoorLabel => IsClosed ? "Open the door" : "Close the door";

    public string Camera
    {
        get => _camera;
        private set => SetField(ref _camera, value);
    }

    public RelayCommand ToggleCommand { get; }

    public RelayCommand WatchCommand { get; }

    private void Toggle()
    {
        if (_game.Shift is not { } shift) return;

        if (shift.IsClosed(Entrance)) shift.Open(Entrance);
        else shift.Close(Entrance);
        _game.Changed();
    }

    private void Watch()
    {
        if (_game.Shift is not { } shift) return;

        Camera = shift.Watch(Entrance) is { } seen
            ? $"{seen.Name}, {shift.DistanceOf(seen)} step(s) away"
            : "Nobody. For now.";
    }

    internal void Changed()
    {
        OnPropertyChanged(nameof(IsClosed));
        OnPropertyChanged(nameof(DoorLabel));
        ToggleCommand.RaiseCanExecuteChanged();
        WatchCommand.RaiseCanExecuteChanged();
    }
}
