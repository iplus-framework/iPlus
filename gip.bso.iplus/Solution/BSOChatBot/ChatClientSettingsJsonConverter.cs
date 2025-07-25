// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using gip.bso.iplus;

namespace Microsoft.Extensions.AI
{
    /// <summary>
    /// JSON converter for List of ChatClientSettings
    /// </summary>
    public class ChatClientSettingsListJsonConverter : JsonConverter<List<ChatClientSettings>>
    {
        public override List<ChatClientSettings> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected StartArray token");
            }

            var settingsList = new List<ChatClientSettings>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return settingsList;
                }

                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    var settings = JsonSerializer.Deserialize<ChatClientSettings>(ref reader, options);
                    if (settings != null)
                    {
                        settingsList.Add(settings);
                    }
                }
                else if (reader.TokenType == JsonTokenType.Null)
                {
                    // Skip null entries
                    continue;
                }
                else
                {
                    throw new JsonException("Expected StartObject token for ChatClientSettings");
                }
            }

            throw new JsonException("Unexpected end of JSON input");
        }

        public override void Write(Utf8JsonWriter writer, List<ChatClientSettings> value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartArray();

            var settingsConverter = new ChatClientSettingsJsonConverter();
            foreach (var settings in value)
            {
                if (settings != null)
                {
                    settingsConverter.Write(writer, settings, options);
                }
                else
                {
                    writer.WriteNullValue();
                }
            }

            writer.WriteEndArray();
        }
    }

    /// <summary>
    /// JSON converter for ChatClientSettings with conditional ApiKey serialization
    /// </summary>
    public class ChatClientSettingsJsonConverter : JsonConverter<ChatClientSettings>
    {
        public override ChatClientSettings Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token");
            }

            var settings = new ChatClientSettings();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return settings;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "Endpoint":
                            settings.Endpoint = reader.GetString();
                            break;
                        case "ApiKey":
                            settings.ApiKey = reader.GetString();
                            break;
                        case "ModelName":
                            settings.ModelName = reader.GetString();
                            break;
                        case "AIClientType":
                            settings.AIClientType = reader.GetString();
                            break;
                        case "MaxOutputTokens":
                            settings.MaxOutputTokens = reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();
                            break;
                        case "Temperature":
                            settings.Temperature = reader.TokenType == JsonTokenType.Null ? null : reader.GetSingle();
                            break;
                        case "TopP":
                            settings.TopP = reader.TokenType == JsonTokenType.Null ? null : reader.GetSingle();
                            break;
                        case "FrequencyPenalty":
                            settings.FrequencyPenalty = reader.TokenType == JsonTokenType.Null ? null : reader.GetSingle();
                            break;
                        case "PresencePenalty":
                            settings.PresencePenalty = reader.TokenType == JsonTokenType.Null ? null : reader.GetSingle();
                            break;
                        case "Seed":
                            settings.Seed = reader.TokenType == JsonTokenType.Null ? null : reader.GetInt64();
                            break;
                        case "IncludeApiKeyInSerialization":
                            settings.IncludeApiKeyInSerialization = reader.GetBoolean();
                            break;
                        default:
                            reader.Skip();
                            break;
                    }
                }
            }

            throw new JsonException("Unexpected end of JSON input");
        }

        public override void Write(Utf8JsonWriter writer, ChatClientSettings value, JsonSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();

            writer.WriteString("Endpoint", value.Endpoint);
            
            // Conditionally serialize ApiKey
            if (value.IncludeApiKeyInSerialization)
            {
                writer.WriteString("ApiKey", value.ApiKey);
            }

            writer.WriteString("ModelName", value.ModelName);
            writer.WriteString("AIClientType", value.AIClientType);

            if (value.MaxOutputTokens.HasValue)
                writer.WriteNumber("MaxOutputTokens", value.MaxOutputTokens.Value);

            if (value.Temperature.HasValue)
                writer.WriteNumber("Temperature", value.Temperature.Value);

            if (value.TopP.HasValue)
                writer.WriteNumber("TopP", value.TopP.Value);

            if (value.FrequencyPenalty.HasValue)
                writer.WriteNumber("FrequencyPenalty", value.FrequencyPenalty.Value);

            if (value.PresencePenalty.HasValue)
                writer.WriteNumber("PresencePenalty", value.PresencePenalty.Value);

            if (value.Seed.HasValue)
                writer.WriteNumber("Seed", value.Seed.Value);

            writer.WriteBoolean("IncludeApiKeyInSerialization", value.IncludeApiKeyInSerialization);

            writer.WriteEndObject();
        }
    }
}