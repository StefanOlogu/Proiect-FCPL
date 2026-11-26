using System.Text.Json;
using Capstone.Core.Json;
using Capstone.Core.Model;
using SharpQuest.Contracts;

namespace Capstone.Core.Storage;

/// <summary>
/// Saves the roster to disk and loads it back (Lab 08), so the bar remembers its regulars after you close the app.
/// Every item goes through <see cref="PersonalityMapper.ToDto"/>, which writes its kind down:
/// a LocalLegend comes back a LocalLegend, a ViralMeme a ViralMeme, every field intact.
/// </summary>
public static class RosterArchive
{
    /// <summary>Where the game keeps its save when nobody says otherwise: ~/.local/share/LaFaleza/roster.json or %LOCALAPPDATA%.</summary>
    public static string DefaultPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LaFaleza", "roster.json");

    /// <summary>
    /// Writes a snapshot of the store, and of who is <paramref name="barred"/> if anyone.
    /// Missing folders are created; an existing file is replaced whole.
    /// </summary>
    public static async Task SaveAsync(IItemStore store, string path, IEnumerable<IItem>? barred = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var file = new SaveFile(
            SaveFile.CurrentFormat,
            store.All().Select(PersonalityMapper.ToDto).ToList(),
            (barred ?? []).Select(PersonalityMapper.ToDto).ToList());

        var folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);

        // Write next to it, then swap: a crash mid-save leaves the old file, never half of a new one.
        var temp = path + ".tmp";
        await using (var stream = File.Create(temp))
        {
            await JsonSerializer.SerializeAsync(stream, file, PersonalityMapper.Options, cancellationToken);
        }
        File.Move(temp, path, overwrite: true);
    }

    /// <summary>
    /// The roster as it was saved. No file yet is an empty roster (the first run);
    /// a file that isn't a valid save is <see cref="InvalidDataException"/>, never a silent empty roster.
    /// </summary>
    public static async Task<PersonalityRoster> LoadAsync(string path, CancellationToken cancellationToken = default) =>
        (await LoadWithBarredAsync(path, cancellationToken)).Roster;

    /// <summary>The roster as it was saved, and who was barred from the bar. Same rules as <see cref="LoadAsync"/>.</summary>
    public static async Task<(PersonalityRoster Roster, IReadOnlyList<Personality> Barred)> LoadWithBarredAsync(
        string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var roster = new PersonalityRoster();
        if (!File.Exists(path)) return (roster, []);

        SaveFile? file;
        try
        {
            await using var stream = File.OpenRead(path);
            file = await JsonSerializer.DeserializeAsync<SaveFile>(stream, PersonalityMapper.Options, cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"'{path}' is not a readable save file: {ex.Message}", ex);
        }

        if (file is not { Format: SaveFile.CurrentFormat, Personalities: { } saved })
            throw new InvalidDataException($"'{path}' is JSON, but not a Bar \"La Faleză\" save file.");

        foreach (var item in Read(saved, path, "Entry"))
        {
            try
            {
                roster.Add(item);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidDataException($"'{path}' repeats the Id '{item.Id}'.", ex);
            }
        }
        return (roster, Read(file.Barred ?? [], path, "Barred entry").ToList());
    }

    /// <summary>Every entry as a personality; one damaged entry means the whole file is damaged.</summary>
    private static IEnumerable<Personality> Read(IReadOnlyList<PersonalityDto?> entries, string path, string what) =>
        entries.Select((dto, i) => PersonalityMapper.FromDto(dto)
            ?? throw new InvalidDataException($"{what} {i} in '{path}' is damaged or incomplete."));

    /// <summary>
    /// Adds the valid entries of a hand-written JSON array (like data/personalities.json) to the roster,
    /// skipping the broken ones and anyone already there. Returns how many were added.
    /// </summary>
    public static async Task<int> ImportAsync(PersonalityRoster roster, string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(roster);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        List<PersonalityDto?>? entries;
        try
        {
            await using var stream = File.OpenRead(path);
            entries = await JsonSerializer.DeserializeAsync<List<PersonalityDto?>>(stream, PersonalityMapper.Options, cancellationToken);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"'{path}' is not a JSON array of personalities: {ex.Message}", ex);
        }

        var fresh = (entries ?? [])
            .Select(PersonalityMapper.FromDto)
            .OfType<Personality>()
            .Where(p => roster[p.Id] is null)
            .DistinctBy(p => p.Id)
            .ToList();

        foreach (var p in fresh) roster.Add(p);
        return fresh.Count;
    }
}
