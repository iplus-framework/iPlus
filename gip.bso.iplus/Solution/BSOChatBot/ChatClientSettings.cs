// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace gip.bso.iplus
{
    /// <summary>
    /// JSON serializable wrapper for ChatOptions with additional client configuration
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Chat Client Settings'}de{'Chat Client Settings'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class ChatClientSettings : EntityBase
    {
        [JsonIgnore]
        private string _Endpoint;
        [ACPropertyInfo(1, "", "en{'Endpoint'}de{'Endpoint'}")]
        public string Endpoint
        {
            get
            {
                return _Endpoint;
            }
            set
            {
                SetProperty(ref _Endpoint, value);
            }
        }
            
        [JsonIgnore]
        private string _ApiKey;
        [ACPropertyInfo(2, "", "en{'API Key'}de{'API Key'}")]
        public string ApiKey
        {
            get
            {
                return _ApiKey;
            }
            set
            {
                SetProperty(ref _ApiKey, value);
            }
        }

        [JsonIgnore]
        private string _ModelName;
        [ACPropertyInfo(3, "", "en{'Model Name'}de{'Model Name'}")]
        public string ModelName
        {
            get
            {
                return _ModelName;
            }
            set
            {
                SetProperty(ref _ModelName, value);
            }
        }

        [JsonIgnore]
        private string _AIClientType;
        [ACPropertyInfo(4, "", "en{'AI Client Type'}de{'AI Client Type'}")]
        public string AIClientType
        {
            get
            {
                if (string.IsNullOrEmpty(_AIClientType))
                {
                    _AIClientType = "OpenAICompatible";
                }
                return _AIClientType;
            }
            set
            {
                SetProperty(ref _AIClientType, value);
            }
        }

        [JsonIgnore]
        private int? _MaxOutputTokens;
        [ACPropertyInfo(6, "", "en{'Max Output Tokens'}de{'Max Output Tokens'}")]
        public int? MaxOutputTokens
        {
            get => _MaxOutputTokens;
            set
            {
                SetProperty(ref _MaxOutputTokens, value);
            }
        }

        [JsonIgnore]
        private float? _Temperature;
        [ACPropertyInfo(7, "", "en{'Temperature'}de{'Temperature'}")]
        public float? Temperature
        {
            get => _Temperature;
            set
            {
                SetProperty(ref _Temperature, value);
            }
        }

        [JsonIgnore]
        private float? _TopP;
        [ACPropertyInfo(8, "", "en{'Top P'}de{'Top P'}")]
        public float? TopP
        {
            get => _TopP;
            set
            {
                SetProperty(ref _TopP, value);
            }
        }

        [JsonIgnore]
        private float? _FrequencyPenalty;
        [ACPropertyInfo(9, "", "en{'Frequency Penalty'}de{'Frequency Penalty'}")]
        public float? FrequencyPenalty
        {
            get => _FrequencyPenalty;
            set
            {
                SetProperty(ref _FrequencyPenalty, value);
            }
        }

        [JsonIgnore]
        private float? _PresencePenalty;
        [ACPropertyInfo(10, "", "en{'Presence Penalty'}de{'Presence Penalty'}")]
        public float? PresencePenalty
        {
            get => _PresencePenalty;
            set
            {
                SetProperty(ref _PresencePenalty, value);
            }
        }

        [JsonIgnore]
        private string[] _StopSequences;
        [ACPropertyInfo(11, "", "en{'Stop Sequences'}de{'Stop Sequences'}")]
        public string[] StopSequences
        {
            get => _StopSequences;
            set
            {
                SetProperty(ref _StopSequences, value);
            }
        }

        [JsonIgnore]
        private long? _Seed;
        [ACPropertyInfo(12, "", "en{'Seed'}de{'Seed'}")]
        public long? Seed
        {
            get => _Seed;
            set
            {
                SetProperty(ref _Seed, value, nameof(Seed));
            }
        }

        [JsonIgnore]
        private Dictionary<string, object> _AdditionalProperties;
        [JsonIgnore]
        [ACPropertyInfo(13, "", "en{'Additional Properties'}de{'Zus�tzliche Eigenschaften'}")]
        public Dictionary<string, object> AdditionalProperties
        {
            get => _AdditionalProperties;
            set
            {
                SetProperty(ref _AdditionalProperties, value);
            }
        }

        [JsonIgnore]
        private bool _IncludeApiKeyInSerialization = false;
        [ACPropertyInfo(14, "", "en{'Include API Key in Serialization'}de{'API Key in Serialization einschlie�en'}")]
        public bool IncludeApiKeyInSerialization
        {
            get => _IncludeApiKeyInSerialization;
            set
            {
                SetProperty(ref _IncludeApiKeyInSerialization, value);
            }
        }

        [JsonIgnore]
        private bool _IsDefault = false;
        [ACPropertyInfo(14, "", "en{'Is Default LLM'}de{'Ist Standard LLM'}")]
        public bool IsDefault
        {
            get => _IsDefault;
            set
            {
                SetProperty(ref _IsDefault, value);
            }
        }

        [JsonIgnore]
        private bool? _AllowMultipleToolCalls = false;
        [ACPropertyInfo(15, "", "en{'Allow parallel tool calls'}de{'Erlaube parallele Toolaurufe'}")]
        public bool? AllowMultipleToolCalls
        {
            get => _AllowMultipleToolCalls;
            set
            {
                SetProperty(ref _AllowMultipleToolCalls, value);
            }
        }

        [JsonIgnore]
        private ChatToolMode _ToolMode;
        public ChatToolMode ToolMode
        {
            get => _ToolMode;
            set
            {
                SetProperty(ref _ToolMode, value);
                if (_ToolMode == ChatToolMode.RequireAny)
                    _ToolRequired = true;
                else if (_ToolMode == ChatToolMode.None)
                    _ToolRequired = false;
                else
                    _ToolRequired = null; // Auto mode
                OnPropertyChanged(nameof(ToolRequired));
            }
        }

        [JsonIgnore]
        private bool? _ToolRequired;
        [JsonIgnore]
        [ACPropertyInfo(17, "", "en{'Force Tool Usage'}de{'Erzwinge Toolverwendung'}")]
        public bool? ToolRequired
        {
            get => _ToolRequired;
            set
            {
                SetProperty(ref _ToolRequired, value);
                if (value.HasValue && value.Value)
                    _ToolMode = ChatToolMode.RequireAny;
                else if (value.HasValue && !value.Value)
                    _ToolMode = ChatToolMode.None;
                else
                    _ToolMode = ChatToolMode.Auto; // Default to Auto if null
                OnPropertyChanged(nameof(ToolMode));
            }
        }

        [JsonIgnore]
        private bool? _useCaching = null;
        [ACPropertyInfo(9, "", "en{'Prompt-Caching'}de{'Prompt-Caching'}")]
        public bool? UseCaching
        {
            get { return _useCaching; }
            set
            {
                SetProperty(ref _useCaching, value);
                OnPropertyChanged();
            }
        }

         [JsonIgnore]
        private bool? _noAssistantPrefill = null;
        [ACPropertyInfo(9, "", "en{'No Assistant Prefill'}de{'Kein Assistenten-Vorbefüllen'}")]
        public bool? NoAssistantPrefill
        {
            get { return _noAssistantPrefill; }
            set
            {
                SetProperty(ref _noAssistantPrefill, value);
                OnPropertyChanged();
            }
        }       

        /// <summary>
        /// Example JSON representation of AdditionalProperties:
        /// {
        ///   "reasoning": {
        ///     "effort": "high",
        ///     "max_tokens": 2000,
        ///     "exclude": false,
        ///     "enabled": true
        ///   }
        /// }
        /// </summary>
        [JsonIgnore]
        private string _AdditionalPropertiesJSON;
        [ACPropertyInfo(16, "", "en{'Additional Properties JSON'}de{'Zus�tzliche Eigenschaften JSON'}")]
        public string AdditionalPropertiesJSON
        {
            get => _AdditionalPropertiesJSON;
            set
            {
                SetProperty(ref _AdditionalPropertiesJSON, value);
                // Parse JSON and populate AdditionalProperties when set
                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        var jsonDoc = JsonDocument.Parse(value);
                        AdditionalProperties = ParseJsonElement(jsonDoc.RootElement);
                    }
                    catch (JsonException)
                    {
                        // If JSON is invalid, clear AdditionalProperties
                        AdditionalProperties = new Dictionary<string, object>();
                    }
                }
                else
                {
                    AdditionalProperties = new Dictionary<string, object>();
                }
            }
        }

        /// <summary>
        /// Helper method to parse JsonElement into Dictionary string, object
        /// </summary>
        private Dictionary<string, object> ParseJsonElement(JsonElement element)
        {
            var result = new Dictionary<string, object>();
            
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in element.EnumerateObject())
                {
                    result[property.Name] = ParseJsonValue(property.Value);
                }
            }
            
            return result;
        }

        /// <summary>
        /// Helper method to parse JsonElement values into appropriate .NET types
        /// </summary>
        private object ParseJsonValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt32(out var intVal) ? intVal : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Object => ParseJsonElement(element),
                JsonValueKind.Array => element.EnumerateArray().Select(ParseJsonValue).ToArray(),
                _ => null
            };
        }

        public ChatClientSettings()
        {
            AdditionalProperties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates a ChatOptions instance from this ChatClientSettings
        /// </summary>
        public ChatOptions ToChatOptions()
        {
            var chatOptions = new ChatOptions
            {
                MaxOutputTokens = MaxOutputTokens,
                Temperature = Temperature,
                TopP = TopP,
                FrequencyPenalty = FrequencyPenalty,
                PresencePenalty = PresencePenalty,
                StopSequences = StopSequences,
                Seed = Seed,
                AllowMultipleToolCalls = AllowMultipleToolCalls,
                ToolMode = ToolMode
            };

            if (AdditionalProperties != null)
            {
                foreach (var kvp in AdditionalProperties)
                {
                    chatOptions.AdditionalProperties ??= new AdditionalPropertiesDictionary();
                    chatOptions.AdditionalProperties[kvp.Key] = kvp.Value;
                }
            }

            return chatOptions;
        }

        /// <summary>
        /// Creates a ChatClientSettings from a ChatOptions instance
        /// </summary>
        public static ChatClientSettings FromChatOptions(ChatOptions chatOptions, string endpoint = "", string apiKey = "", string modelName = "", string aiClientType = "OpenAICompatible")
        {
            var settings = new ChatClientSettings
            {
                Endpoint = endpoint,
                ApiKey = apiKey,
                ModelName = modelName,
                AIClientType = aiClientType,
                MaxOutputTokens = chatOptions.MaxOutputTokens,
                Temperature = chatOptions.Temperature,
                TopP = chatOptions.TopP,
                FrequencyPenalty = chatOptions.FrequencyPenalty,
                PresencePenalty = chatOptions.PresencePenalty,
                StopSequences = chatOptions.StopSequences.ToArray(),
                Seed = chatOptions.Seed,
                AllowMultipleToolCalls = chatOptions.AllowMultipleToolCalls,
                ToolMode = chatOptions.ToolMode
            };

            if (chatOptions.AdditionalProperties != null)
            {
                settings.AdditionalProperties = new Dictionary<string, object>(chatOptions.AdditionalProperties);
            }

            return settings;
        }

        /// <summary>
        /// Returns a display-friendly string representation
        /// </summary>
        public override string ToString()
        {
            return $"{AIClientType} - {ModelName} ({Endpoint})";
        }
    }
}