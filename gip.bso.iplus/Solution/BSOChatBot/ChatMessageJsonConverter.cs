using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using gip.bso.iplus;

namespace Microsoft.Extensions.AI;

/// <summary>JSON converter for <see cref="ChatMessage"/>.</summary>
public class ChatMessageJsonConverter : JsonConverter<ChatMessage>
{
    public override ChatMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        var message = new ChatMessage();
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return message;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "Role":
                    message.Role = JsonSerializer.Deserialize<ChatRole>(ref reader, options);
                    break;
                case "AuthorName":
                    message.AuthorName = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    break;
                case "Contents":
                    message.Contents = JsonSerializer.Deserialize<IList<AIContent>>(ref reader, options) ?? [];
                    break;
                case "MessageId":
                    message.MessageId = reader.TokenType == JsonTokenType.Null ? null : reader.GetString();
                    break;
                case "AdditionalProperties":
                    message.AdditionalProperties = JsonSerializer.Deserialize<AdditionalPropertiesDictionary>(ref reader, options);
                    break;
                default:
                    // Handle additional properties
                    if (message.AdditionalProperties == null)
                    {
                        message.AdditionalProperties = new AdditionalPropertiesDictionary();
                    }
                    var value = JsonSerializer.Deserialize<object>(ref reader, options);
                    message.AdditionalProperties[propertyName] = value;
                    break;
            }
        }

        throw new JsonException("Unexpected end of JSON input");
    }

    public override void Write(Utf8JsonWriter writer, ChatMessage value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Role");
        JsonSerializer.Serialize(writer, value.Role, options);

        if (value.AuthorName != null)
        {
            writer.WritePropertyName("AuthorName");
            writer.WriteStringValue(value.AuthorName);
        }

        writer.WritePropertyName("Contents");
        JsonSerializer.Serialize(writer, value.Contents, options);

        if (value.MessageId != null)
        {
            writer.WritePropertyName("MessageId");
            writer.WriteStringValue(value.MessageId);
        }

        if (value.AdditionalProperties != null)
        {
            foreach (var kvp in value.AdditionalProperties)
            {
                writer.WritePropertyName(kvp.Key);
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }
        }

        writer.WriteEndObject();
    }
}

/// <summary>JSON converter for <see cref="List{ChatMessage}"/>.</summary>
public class ChatMessageListJsonConverter : JsonConverter<List<ChatMessage>>
{
    private readonly ChatMessageJsonConverter _chatMessageConverter = new();

    public override List<ChatMessage> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token");
        }

        var messages = new List<ChatMessage>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return messages;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var message = _chatMessageConverter.Read(ref reader, typeof(ChatMessage), options);
                messages.Add(message);
            }
            else
            {
                throw new JsonException("Expected StartObject token for ChatMessage");
            }
        }

        throw new JsonException("Unexpected end of JSON input");
    }

    public override void Write(Utf8JsonWriter writer, List<ChatMessage> value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();

        foreach (var message in value)
        {
            if (message != null)
            {
                _chatMessageConverter.Write(writer, message, options);
            }
            else
            {
                writer.WriteNullValue();
            }
        }

        writer.WriteEndArray();
    }
}

/// <summary>JSON converter for <see cref="ChatResponseUpdateWrapper"/>.</summary>
public class ChatResponseUpdateWrapperJsonConverter : JsonConverter<ChatResponseUpdateWrapper>
{
    public override ChatResponseUpdateWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        var wrapper = new ChatResponseUpdateWrapper();
        ChatResponseUpdate update = null;
        DateTime timestamp = DateTime.Now;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (update != null)
                {
                    wrapper.Update = update;
                }
                wrapper.Timestamp = timestamp;
                return wrapper;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "Update":
                    update = JsonSerializer.Deserialize<ChatResponseUpdate>(ref reader, options);
                    break;
                case "Timestamp":
                    timestamp = JsonSerializer.Deserialize<DateTime>(ref reader, options);
                    break;
                default:
                    // Skip unknown properties
                    JsonSerializer.Deserialize<object>(ref reader, options);
                    break;
            }
        }

        throw new JsonException("Unexpected end of JSON input");
    }

    public override void Write(Utf8JsonWriter writer, ChatResponseUpdateWrapper value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        if (value.Update != null)
        {
            writer.WritePropertyName("Update");
            JsonSerializer.Serialize(writer, value.Update, options);
        }

        writer.WritePropertyName("Timestamp");
        JsonSerializer.Serialize(writer, value.Timestamp, options);

        writer.WriteEndObject();
    }
}

/// <summary>JSON converter for <see cref="ChatMessageWrapper"/>.</summary>
public class ChatMessageWrapperJsonConverter : JsonConverter<ChatMessageWrapper>
{
    private readonly ChatResponseUpdateWrapperJsonConverter _updateConverter = new();

    public override ChatMessageWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected StartObject token");
        }

        var wrapper = new ChatMessageWrapper();
        ChatMessage wrappedMessage = null;
        DateTime timestamp = DateTime.Now;
        ObservableCollection<ChatResponseUpdateWrapper> updates = new();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (wrappedMessage != null)
                {
                    wrapper.WrappedChatMessage = wrappedMessage;
                }
                wrapper.Timestamp = timestamp;
                wrapper.Updates = updates;
                return wrapper;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected PropertyName token");
            }

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName)
            {
                case "WrappedChatMessage":
                    wrappedMessage = JsonSerializer.Deserialize<ChatMessage>(ref reader, options);
                    break;
                case "Timestamp":
                    timestamp = JsonSerializer.Deserialize<DateTime>(ref reader, options);
                    break;
                case "Updates":
                    var updatesList = JsonSerializer.Deserialize<List<ChatResponseUpdateWrapper>>(ref reader, options);
                    if (updatesList != null)
                    {
                        updates = new ObservableCollection<ChatResponseUpdateWrapper>(updatesList);
                    }
                    break;
                default:
                    // Skip unknown properties
                    JsonSerializer.Deserialize<object>(ref reader, options);
                    break;
            }
        }

        throw new JsonException("Unexpected end of JSON input");
    }

    public override void Write(Utf8JsonWriter writer, ChatMessageWrapper value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        if (value.WrappedChatMessage != null)
        {
            writer.WritePropertyName("WrappedChatMessage");
            JsonSerializer.Serialize(writer, value.WrappedChatMessage, options);
        }

        writer.WritePropertyName("Timestamp");
        JsonSerializer.Serialize(writer, value.Timestamp, options);

        if (value.Updates != null)
        {
            writer.WritePropertyName("Updates");
            JsonSerializer.Serialize(writer, value.Updates, options);
        }

        writer.WriteEndObject();
    }
}

/// <summary>JSON converter for <see cref="ObservableCollection{ChatMessageWrapper}"/>.</summary>
public class ChatMessageWrapperCollectionJsonConverter : JsonConverter<ObservableCollection<ChatMessageWrapper>>
{
    private readonly ChatMessageWrapperJsonConverter _messageConverter = new();

    public override ObservableCollection<ChatMessageWrapper> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token");
        }

        var messages = new ObservableCollection<ChatMessageWrapper>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return messages;
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var message = _messageConverter.Read(ref reader, typeof(ChatMessageWrapper), options);
                messages.Add(message);
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                // Skip null entries
                continue;
            }
            else
            {
                throw new JsonException("Expected StartObject token for ChatMessageWrapper");
            }
        }

        throw new JsonException("Unexpected end of JSON input");
    }

    public override void Write(Utf8JsonWriter writer, ObservableCollection<ChatMessageWrapper> value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();

        foreach (var message in value)
        {
            if (message != null)
            {
                _messageConverter.Write(writer, message, options);
            }
            else
            {
                writer.WriteNullValue();
            }
        }

        writer.WriteEndArray();
    }
}