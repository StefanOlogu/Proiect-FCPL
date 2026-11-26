namespace Capstone.Core.Json;

/// <summary>
/// The record: what the save file holds. The marker says "this file is ours", so a JSON file
/// from somewhere else is caught as corrupt instead of loading as an empty bar.
/// <see cref="Barred"/> is who was thrown out; a save from before barring existed simply has none.
/// </summary>
public sealed record SaveFile(
    string? Format,
    IReadOnlyList<PersonalityDto?>? Personalities,
    IReadOnlyList<PersonalityDto?>? Barred = null)
{
    public const string CurrentFormat = "la-faleza/1";
}
