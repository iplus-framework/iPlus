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
                        case nameof(ChatClientSettings.Endpoint):
                            settings.Endpoint = reader.GetString();
                            break;
                        case nameof(ChatClientSettings.ApiKey):
                            settings.ApiKey = reader.GetString();
                            break;
                        case nameof(ChatClientSettings.ModelName):
                            settings.ModelName = reader.GetString();
                            break;
                        case nameof(ChatClientSettings.AIClientType):
                            settings.AIClientType = reader.GetString();
                            break;
                        case nameof(ChatClientSettings.MaxOutputTokens):
                            settings.MaxOutputTokens = reader.TokenType == JsonTokenType.Null ? null : reader.GetInt32();
                            break;
                        case nameof(ChatClientSettings.Temperature):
                            settings.Temperature = reader.TokenType == JsonTokenType.Null ? null : reader.GetSingle();
                            break;
                        case nameof(ChatClientSettings.TopP):
                            settings.TopP = reader.TokenType == JsonTokenType.Null ? null : reader.GetSingle();
                            break;
                        case nameof(ChatClientSettings.FrequencyPenalty):
                            settings.FrequencyPenalty = reader.TokenType == JsonTokenType.Null ? null : reader.GetSingle();
                            break;
                        case nameof(ChatClientSettings.PresencePenalty):
                            settings.PresencePenalty = reader.TokenType == JsonTokenType.Null ? null : reader.GetSingle();
                            break;
                        case nameof(ChatClientSettings.Seed):
                            settings.Seed = reader.TokenType == JsonTokenType.Null ? null : reader.GetInt64();
                            break;
                        case nameof(ChatClientSettings.IncludeApiKeyInSerialization):
                            settings.IncludeApiKeyInSerialization = reader.GetBoolean();
                            break;
                        case nameof(ChatClientSettings.AllowMultipleToolCalls):
                            settings.AllowMultipleToolCalls = reader.TokenType == JsonTokenType.Null ? null : reader.GetBoolean();
                            break;
                        case nameof(ChatClientSettings.IsDefault):
                            settings.IsDefault = reader.GetBoolean();
                            break;
                        case nameof(ChatClientSettings.UseCaching):
                            settings.UseCaching = reader.TokenType == JsonTokenType.Null ? null : reader.GetBoolean();
                            break;
                        case nameof(ChatClientSettings.NoAssistantPrefill):
                            settings.NoAssistantPrefill = reader.TokenType == JsonTokenType.Null ? null : reader.GetBoolean();
                            break;
                        case nameof(ChatClientSettings.AdditionalPropertiesJSON):
                            settings.AdditionalPropertiesJSON = reader.GetString();
                            break;
                        case nameof(ChatClientSettings.ToolMode):
                            if (reader.TokenType == JsonTokenType.Null)
                            {
                                settings.ToolMode = null;
                            }
                            else if (reader.TokenType == JsonTokenType.String)
                            {
                                var toolModeString = reader.GetString();
                                if (string.IsNullOrEmpty(toolModeString) || toolModeString == nameof(ChatToolMode.Auto))
                                    settings.ToolMode = ChatToolMode.Auto;
                                else if (toolModeString == nameof(ChatToolMode.None))
                                    settings.ToolMode = ChatToolMode.None;
                                else if (toolModeString == nameof(ChatToolMode.RequireAny))
                                    settings.ToolMode = ChatToolMode.RequireAny;
                            }
                            break;
                        case nameof(ChatClientSettings.StopSequences):
                            if (reader.TokenType == JsonTokenType.StartArray)
                            {
                                var stopSequences = new List<string>();
                                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                                {
                                    if (reader.TokenType == JsonTokenType.String)
                                    {
                                        stopSequences.Add(reader.GetString());
                                    }
                                }
                                settings.StopSequences = stopSequences.ToArray();
                            }
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

            writer.WriteString(nameof(ChatClientSettings.Endpoint), value.Endpoint);
            
            // Conditionally serialize ApiKey
            if (value.IncludeApiKeyInSerialization)
            {
                writer.WriteString(nameof(ChatClientSettings.ApiKey), value.ApiKey);
            }

            writer.WriteString(nameof(ChatClientSettings.ModelName), value.ModelName);
            writer.WriteString(nameof(ChatClientSettings.AIClientType), value.AIClientType);

            if (value.MaxOutputTokens.HasValue)
                writer.WriteNumber(nameof(ChatClientSettings.MaxOutputTokens), value.MaxOutputTokens.Value);

            if (value.Temperature.HasValue)
                writer.WriteNumber(nameof(ChatClientSettings.Temperature), value.Temperature.Value);

            if (value.TopP.HasValue)
                writer.WriteNumber(nameof(ChatClientSettings.TopP), value.TopP.Value);

            if (value.FrequencyPenalty.HasValue)
                writer.WriteNumber(nameof(ChatClientSettings.FrequencyPenalty), value.FrequencyPenalty.Value);

            if (value.PresencePenalty.HasValue)
                writer.WriteNumber(nameof(ChatClientSettings.PresencePenalty), value.PresencePenalty.Value);

            if (value.Seed.HasValue)
                writer.WriteNumber(nameof(ChatClientSettings.Seed), value.Seed.Value);

            if (value.AllowMultipleToolCalls.HasValue)
                writer.WriteBoolean(nameof(ChatClientSettings.AllowMultipleToolCalls), value.AllowMultipleToolCalls.Value);

            writer.WriteBoolean(nameof(ChatClientSettings.IncludeApiKeyInSerialization), value.IncludeApiKeyInSerialization);
            writer.WriteBoolean(nameof(ChatClientSettings.IsDefault), value.IsDefault);

            if (value.UseCaching.HasValue)
                writer.WriteBoolean(nameof(ChatClientSettings.UseCaching), value.UseCaching.Value);
                
            if (value.NoAssistantPrefill.HasValue)
                writer.WriteBoolean(nameof(ChatClientSettings.NoAssistantPrefill), value.NoAssistantPrefill.Value);

            if (value.ToolMode != null)
            {
                if (value.ToolMode == ChatToolMode.None)
                    writer.WriteString(nameof(ChatClientSettings.ToolMode), nameof(ChatToolMode.None));
                else if (value.ToolMode == ChatToolMode.RequireAny)
                    writer.WriteString(nameof(ChatClientSettings.ToolMode), nameof(ChatToolMode.RequireAny));
                else
                    writer.WriteString(nameof(ChatClientSettings.ToolMode), nameof(ChatToolMode.Auto));
            }

            if (value.StopSequences != null && value.StopSequences.Length > 0)
            {
                writer.WriteStartArray(nameof(ChatClientSettings.StopSequences));
                foreach (var sequence in value.StopSequences)
                {
                    writer.WriteStringValue(sequence);
                }
                writer.WriteEndArray();
            }

            // Serialize AdditionalPropertiesJSON instead of AdditionalProperties
            if (!string.IsNullOrEmpty(value.AdditionalPropertiesJSON))
            {
                writer.WriteString(nameof(ChatClientSettings.AdditionalPropertiesJSON), value.AdditionalPropertiesJSON);
            }

            writer.WriteEndObject();
        }
    }
}