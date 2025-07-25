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
        [ACPropertyInfo(13, "", "en{'Additional Properties'}de{'Zusätzliche Eigenschaften'}")]
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
        [ACPropertyInfo(14, "", "en{'Include API Key in Serialization'}de{'API Key in Serialization einschließen'}")]
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
                Seed = Seed
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
                Seed = chatOptions.Seed
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