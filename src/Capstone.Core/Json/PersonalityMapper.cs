using System.Text.Json;
using System.Text.Json.Serialization;
using Capstone.Core.Model;
using SharpQuest.Contracts;

namespace Capstone.Core.Json;

public sealed class PersonalityMapper : IItemMapper
{
    public const string LocalKind = "local";
    public const string MemeKind = "meme";

    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },   // "FrontDoor" instead of 0
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string ToJson(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        var dto = item switch
        {
            LocalLegend l => new PersonalityDto(LocalKind, l.Id, l.Name, l.Entrance, l.Aggression, l.FirstNight, l.Catchphrase, l.Neighbourhood, null),
            ViralMeme m => new PersonalityDto(MemeKind, m.Id, m.Name, m.Entrance, m.Aggression, m.FirstNight, m.Catchphrase, null, m.OriginYear),
            _ => throw new ArgumentException($"This mapper only knows personalities, not {item.GetType().Name}.", nameof(item)),
        };
        return JsonSerializer.Serialize(dto, Options);
    }

    public IItem? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        PersonalityDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<PersonalityDto>(json, Options);
        }
        catch (JsonException)
        {
            return null;   // not JSON, not an object, or a value of the wrong type
        }

        // Check the DTO first: missing data is a normal "no", not an exception.
        if (dto is null) return null;
        if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.Name)) return null;
        if (dto.Entrance is not { } entrance || dto.Aggression is not { } aggression) return null;
        var firstNight = dto.FirstNight ?? Night.FirstNight;

        try
        {
            return dto switch
            {
                { Kind: LocalKind, Neighbourhood: { } neighbourhood } => new LocalLegend
                {
                    Id = dto.Id, Name = dto.Name, Entrance = entrance, Aggression = aggression,
                    FirstNight = firstNight, Catchphrase = dto.Catchphrase, Neighbourhood = neighbourhood,
                },
                { Kind: MemeKind, OriginYear: { } year } => new ViralMeme
                {
                    Id = dto.Id, Name = dto.Name, Entrance = entrance, Aggression = aggression,
                    FirstNight = firstNight, Catchphrase = dto.Catchphrase, OriginYear = year,
                },
                _ => null,   // unknown kind, or the field that kind needs is missing
            };
        }
        catch (ArgumentException)   // a blank neighbourhood or a number out of range
        {
            return null;
        }
    }
}
