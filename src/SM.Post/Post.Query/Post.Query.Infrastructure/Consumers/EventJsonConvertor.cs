using System.Text.Json;
using System.Text.Json.Serialization;
using CQRS.Core.Events;
using Post.Common.Events;

namespace Post.Query.Infrastructure.Consumers;

public class EventJsonConvertor : JsonConverter<BaseEvent>
{
    public override bool CanConvert(Type typeToConvert) 
        => typeToConvert.IsAssignableFrom(typeof(BaseEvent));

    public override BaseEvent? Read(ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (!JsonDocument.TryParseValue(ref reader, out JsonDocument? document))
        {
            throw new JsonException($"Failed to parse {nameof(JsonDocument)}");
        }

        if (!document.RootElement.TryGetProperty("Type", out JsonElement type))
        {
            throw new JsonException($"Could not detect the type discriminator property");
        }

        string? typeDiscriminator = type.GetString();
        string json = document.RootElement.GetRawText();

        return typeDiscriminator switch
        {
            nameof(PostCreatedEvent) => JsonSerializer.Deserialize<PostCreatedEvent>(json, options),
            nameof(MessageUpdatedEvent) => JsonSerializer.Deserialize<MessageUpdatedEvent>(json, options),
            nameof(PostLikedEvent) => JsonSerializer.Deserialize<PostLikedEvent>(json, options),
            nameof(CommentAddedEvent) => JsonSerializer.Deserialize<CommentAddedEvent>(json, options),
            nameof(CommentUpdatedEvent) => JsonSerializer.Deserialize<CommentUpdatedEvent>(json, options),
            nameof(CommentRemovedEvent) => JsonSerializer.Deserialize<CommentRemovedEvent>(json, options),
            nameof(PostRemovedEvent) => JsonSerializer.Deserialize<PostRemovedEvent>(json, options),
            _ => throw new JsonException($"{typeDiscriminator} is not supported yet!")
        };
    }

    public override void Write(Utf8JsonWriter writer, BaseEvent value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}