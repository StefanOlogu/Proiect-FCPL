using Capstone.Core.Model;

namespace Capstone.Core.Json;

/// <summary>
/// The record: how a personality looks in JSON. Everything is nullable on purpose,
/// because JSON from outside can be missing anything; the mapper decides what is acceptable.
/// <see cref="Kind"/> says which class to build: "local" or "meme".
/// </summary>
public sealed record PersonalityDto(
    string? Kind,
    string? Id,
    string? Name,
    Entrance? Entrance,
    int? Aggression,
    int? FirstNight,
    string? Catchphrase,
    string? Neighbourhood,
    int? OriginYear);
