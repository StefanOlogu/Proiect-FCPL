using System.Text.Json;
using System.Text.Json.Serialization;
using Capstone.Core.Model;
using SharpQuest.Contracts;

namespace Capstone.Core.Json;

public sealed class AnimatronicMapper : IItemMapper
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },   // "LeftDoor" instead of 0
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string ToJson(IItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (item is not Animatronic a)
            throw new ArgumentException($"This mapper only knows animatronics, not {item.GetType().Name}.", nameof(item));

        var dto = new AnimatronicDto(a.Id, a.Name, a.Approach, a.Aggression, a.FirstNight, a.Catchphrase);
        return JsonSerializer.Serialize(dto, Options);
    }

    public IItem? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        AnimatronicDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<AnimatronicDto>(json, Options);
        }
        catch (JsonException)
        {
            return null;   // not JSON, not an object, or a value of the wrong type
        }

        // Check the DTO first: missing data is a normal "no", not an exception.
        if (dto is null) return null;
        if (string.IsNullOrWhiteSpace(dto.Id) || string.IsNullOrWhiteSpace(dto.Name)) return null;
        if (dto.Approach is not { } approach || dto.Aggression is not { } aggression) return null;

        try
        {
            return new Animatronic
            {
                Id = dto.Id,
                Name = dto.Name,
                Approach = approach,
                Aggression = aggression,
                FirstNight = dto.FirstNight ?? Night.FirstNight,
                Catchphrase = dto.Catchphrase,
            };
        }
        catch (ArgumentException)   // a number out of range; ArgumentOutOfRangeException is an ArgumentException
        {
            return null;
        }
    }
}
