using Capstone.Core.Model;

namespace Capstone.Core.Json;

/// <summary>
/// The record: how an animatronic looks in JSON. Everything is nullable on purpose,
/// because JSON from outside can be missing anything; the mapper decides what is acceptable.
/// </summary>
public sealed record AnimatronicDto(
    string? Id,
    string? Name,
    Approach? Approach,
    int? Aggression,
    int? FirstNight,
    string? Catchphrase);
