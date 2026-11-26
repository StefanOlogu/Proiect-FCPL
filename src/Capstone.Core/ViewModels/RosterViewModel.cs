using System.Collections.ObjectModel;
using Capstone.Core.Model;
using Capstone.Core.Storage;
using SharpQuest.Contracts;

namespace Capstone.Core.ViewModels;

/// <summary>
/// What the roster window binds to (Lab 09): the list, the search box, the selection, the buttons.
/// It has never heard of a Button, which is why the tests can drive it without a screen.
/// </summary>
public sealed class RosterViewModel : ObservableObject, ICatalogueViewModel
{
    private readonly PersonalityRoster _roster;
    private readonly string _savePath;
    private readonly ObservableCollection<IItem> _items = [];
    private readonly SearchMatcher _matcher = SearchMatcher.Create();
    private readonly RelayCommand _removeCommand;
    private readonly RelayCommand _unbarCommand;
    private readonly ObservableCollection<IItem> _barred = [];

    private string _filter = "";
    private IItem? _selected;
    private IItem? _selectedBarred;
    private string _status = "";

    // The new-personality form.
    private string _newName = "";
    private bool _newIsMeme;
    private Entrance _newEntrance = Entrance.FrontDoor;
    private int _newAggression = 5;
    private int _newFirstNight = Night.FirstNight;
    private string _newOrigin = "";
    private string _newCatchphrase = "";

    public RosterViewModel(PersonalityRoster roster, string? savePath = null)
    {
        ArgumentNullException.ThrowIfNull(roster);
        _roster = roster;
        _savePath = savePath ?? RosterArchive.DefaultPath;

        AddCommand = new RelayCommand(p => Add(p as IItem), p => p is IItem);
        _removeCommand = new RelayCommand(_ => Remove(), _ => Selected is not null);
        _unbarCommand = new RelayCommand(_ => Unbar(), _ => SelectedBarred is not null);
        CreateCommand = new RelayCommand(_ => Create());
        SaveCommand = new RelayCommand(async _ => await SaveAsync());

        // Lab 06's events doing the job they were written for: the list follows the roster.
        _roster.ItemAdded += OnAdded;
        _roster.ItemRemoved += OnRemoved;
        Refresh();
    }

    public IEnumerable<IItem> Items => _items;   // the same ObservableCollection, always

    public string Filter
    {
        get => _filter;
        set
        {
            if (SetField(ref _filter, value ?? "")) Refresh();
        }
    }

    public IItem? Selected
    {
        get => _selected;
        set
        {
            if (!SetField(ref _selected, value)) return;
            OnPropertyChanged(nameof(Details));
            _removeCommand.RaiseCanExecuteChanged();
        }
    }

    public string Details => Selected switch
    {
        null => "Nobody selected. Pick a personality from the list.",
        Personality p => $"{PersonalityCatalog.Classify(p)}\n\n{p.Describe()}\n\nIf they get in: {p.Jumpscare()}",
        var other => other.DisplayName,
    };

    public int Count => _items.Count;

    /// <summary>Who was thrown out. They're off the roster (so they don't play), but not forgotten.</summary>
    public IEnumerable<IItem> Barred => _barred;

    /// <summary>The row selected in the barred list; it's who <see cref="UnbarCommand"/> lets back in.</summary>
    public IItem? SelectedBarred
    {
        get => _selectedBarred;
        set
        {
            if (SetField(ref _selectedBarred, value)) _unbarCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>The last thing that happened: saved, loaded, a rejected add.</summary>
    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public System.Windows.Input.ICommand AddCommand { get; }

    public System.Windows.Input.ICommand RemoveCommand => _removeCommand;

    /// <summary>Lets <see cref="SelectedBarred"/> back onto the roster. Greyed out when nobody is selected there.</summary>
    public System.Windows.Input.ICommand UnbarCommand => _unbarCommand;

    /// <summary>Builds a personality from the form below and adds it.</summary>
    public System.Windows.Input.ICommand CreateCommand { get; }

    public System.Windows.Input.ICommand SaveCommand { get; }

    public IReadOnlyList<Entrance> Entrances { get; } = Enum.GetValues<Entrance>();

    public string NewName { get => _newName; set => SetField(ref _newName, value ?? ""); }

    /// <summary>False = a local legend (Origin is the neighbourhood), true = a viral meme (Origin is the year).</summary>
    public bool NewIsMeme { get => _newIsMeme; set => SetField(ref _newIsMeme, value); }

    public Entrance NewEntrance { get => _newEntrance; set => SetField(ref _newEntrance, value); }

    public int NewAggression { get => _newAggression; set => SetField(ref _newAggression, value); }

    public int NewFirstNight { get => _newFirstNight; set => SetField(ref _newFirstNight, value); }

    public string NewOrigin { get => _newOrigin; set => SetField(ref _newOrigin, value ?? ""); }

    public string NewCatchphrase { get => _newCatchphrase; set => SetField(ref _newCatchphrase, value ?? ""); }

    /// <summary>Saves the roster in the background; the window stays responsive while the disk works.</summary>
    public async Task SaveAsync()
    {
        try
        {
            await RosterArchive.SaveAsync(_roster, _savePath, _barred);
            Status = $"Saved {_roster.Count} personalities ({_barred.Count} barred) to {_savePath}.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Status = $"Could not save: {ex.Message}";
        }
    }

    /// <summary>
    /// Brings back what was saved last time. Anyone already on the roster stays as they are.
    /// No save yet (the first run): starts from <paramref name="seedPath"/>, the hand-written starting roster.
    /// </summary>
    public async Task LoadAsync(string? seedPath = null)
    {
        try
        {
            var (saved, barred) = await RosterArchive.LoadWithBarredAsync(_savePath);
            var added = saved.Count(item => _roster[item.Id] is null && TryAdd(item));
            foreach (var p in barred.Where(p => _roster[p.Id] is null && !_barred.Any(b => b.Id == p.Id)))
                _barred.Add(p);

            if (saved.Count == 0 && barred.Count == 0 && seedPath is not null && File.Exists(seedPath))
                Status = $"First night at the bar: {await RosterArchive.ImportAsync(_roster, seedPath)} regulars from the starting roster.";
            else
                Status = saved.Count == 0 ? "No save yet: this is the first night." : $"Loaded {added} personalities from {_savePath}.";
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            Status = $"Could not load the save: {ex.Message}";
        }
    }

    private void Add(IItem? item)
    {
        if (item is not null && TryAdd(item)) Status = $"{item.DisplayName} joined the roster.";
    }

    /// <summary>Through the roster, so its rules apply; a rejection becomes a message, not a crash.</summary>
    private bool TryAdd(IItem item)
    {
        try
        {
            _roster.Add(item);
            return true;
        }
        catch (ArgumentException ex)
        {
            Status = ex.Message;
            return false;
        }
    }

    private void Remove()
    {
        if (Selected is not { } doomed) return;

        _roster.Remove(doomed.Id);   // OnRemoved takes it out of the list
        Status = $"{doomed.DisplayName} is barred from the bar.";
    }

    private void Create()
    {
        var id = $"{(NewIsMeme ? "meme" : "local")}-{Guid.NewGuid():N}"[..14];
        var catchphrase = string.IsNullOrWhiteSpace(NewCatchphrase) ? null : NewCatchphrase.Trim();
        try
        {
            Personality created = NewIsMeme
                ? new ViralMeme
                {
                    Id = id, Name = NewName.Trim(), Entrance = NewEntrance, Aggression = NewAggression, FirstNight = NewFirstNight,
                    Catchphrase = catchphrase,
                    OriginYear = int.TryParse(NewOrigin, out var year) ? year : throw new ArgumentException("A meme needs the year it went viral."),
                }
                : new LocalLegend
                {
                    Id = id, Name = NewName.Trim(), Entrance = NewEntrance, Aggression = NewAggression, FirstNight = NewFirstNight,
                    Catchphrase = catchphrase, Neighbourhood = NewOrigin.Trim(),
                };
            Add(created);
            NewName = NewOrigin = NewCatchphrase = "";
        }
        catch (ArgumentException ex)
        {
            Status = ex.Message;
        }
    }

    private bool Matches(IItem item) => _matcher.Matches(item.DisplayName, Filter);

    private void OnAdded(IItem item)
    {
        // Back on the roster, however they got there: no longer barred.
        if (_barred.FirstOrDefault(b => b.Id == item.Id) is { } wasBarred)
        {
            if (ReferenceEquals(wasBarred, SelectedBarred)) SelectedBarred = null;
            _barred.Remove(wasBarred);
        }

        if (!Matches(item)) return;

        _items.Add(item);   // new items go last on the roster, so last in the list keeps roster order
        OnPropertyChanged(nameof(Count));
    }

    private void Unbar()
    {
        if (SelectedBarred is not { } forgiven) return;

        if (TryAdd(forgiven))   // the roster's rules still apply: an Id taken since then is a message
        {
            Status = $"{forgiven.DisplayName} is welcome at the bar again.";
            if (_items.Contains(forgiven)) Selected = forgiven;
        }
    }

    private void OnRemoved(IItem item)
    {
        if (!_barred.Contains(item)) _barred.Add(item);
        if (ReferenceEquals(item, Selected)) Selected = null;
        if (_items.Remove(item)) OnPropertyChanged(nameof(Count));
    }

    /// <summary>
    /// Makes the list show exactly the matching items, in roster order, by removing and inserting rows —
    /// never by Clear(), which would make the window redraw everything and lose its scroll position.
    /// </summary>
    private void Refresh()
    {
        var wanted = _roster.Where(Matches).ToList();
        var before = _items.Count;

        for (var i = _items.Count - 1; i >= 0; i--)
            if (!wanted.Contains(_items[i])) _items.RemoveAt(i);

        for (var i = 0; i < wanted.Count; i++)
            if (i >= _items.Count || !ReferenceEquals(_items[i], wanted[i])) _items.Insert(i, wanted[i]);

        if (Selected is not null && !_items.Contains(Selected)) Selected = null;
        if (_items.Count != before) OnPropertyChanged(nameof(Count));
    }
}
