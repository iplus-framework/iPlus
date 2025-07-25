// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OllamaSharp;
using System.Text.Json;
using gip.core.media;
using System.IO;

namespace gip.bso.iplus
{
    public partial class BSOChatBot : ACBSO
    {
        #region Properties

        #region Config
        private const string C_DefaultEndpoint = "https://openrouter.ai/api/v1";
        private const string C_DefaultModelName = "google/gemini-2.0-flash-exp:free";

        protected ACPropertyConfigValue<string> _ChatClientConfig;
        [ACPropertyInfo(10, "", "en{'Chat client JSON config'}de{'Chat client JSON config'}")]
        public string ChatClientConfig
        {
            get { return _ChatClientConfig.ValueT; }
            set
            {
                _ChatClientConfig.ValueT = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region LLM Properties

        private bool _LLMPropsChanged = false;

        private List<string> _aiClientTypes;
        [ACPropertyList(5, "AIClientTypes", "en{'AI Client Types'}de{'AI Client Typen'}")]
        public List<string> AIClientTypes
        {
            get
            {
                if (_aiClientTypes == null)
                {
                    _aiClientTypes = new List<string> { "OpenAICompatible", "OpenAI", "Ollama" };
                }
                return _aiClientTypes;
            }
        }

        private string _selectedAIClientType = "OpenAICompatible";
        [ACPropertySelected(6, "AIClientTypes", "en{'Selected AI Client'}de{'Ausgewählter AI Client'}")]
        public string SelectedAIClientType
        {
            get { return _selectedAIClientType; }
            set
            {
                if (_selectedAIClientType != value)
                    _LLMPropsChanged = true;
                _selectedAIClientType = value;
                OnPropertyChanged();
                InitializeSelectedAIClient();
            }
        }

        private string _apiKey = "";
        [ACPropertyInfo(7, "ApiKey", "en{'API Key'}de{'API Schlüssel'}")]
        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                if (_apiKey != value)
                    _LLMPropsChanged = true;
                _apiKey = value;
                OnPropertyChanged();
            }
        }

        private string _Endpoint = C_DefaultEndpoint;
        [ACPropertyInfo(8, "Endpoint", "en{'Endpoint'}de{'Endpunkt'}")]
        public string Endpoint
        {
            get { return _Endpoint; }
            set
            {
                if (_Endpoint != value)
                    _LLMPropsChanged = true;
                _Endpoint = value;
                OnPropertyChanged();
            }
        }

        private string _modelName = C_DefaultModelName;
        [ACPropertyInfo(9, "ModelName", "en{'Model Name'}de{'Modell Name'}")]
        public string ModelName
        {
            get { return _modelName; }
            set
            {
                if (_modelName != value)
                    _LLMPropsChanged = true;
                _modelName = value;
                OnPropertyChanged();
            }
        }


        private List<ChatClientSettings> _ChatClientSettingsList;
        [ACPropertyList(18, "ChatClientSettings", "en{'Settings of Chat Clients'}de{'Settings of Chat Clients'}")]
        public List<ChatClientSettings> ChatClientSettingsList
        {
            get
            {
                if (_ChatClientSettingsList == null)
                {
                    _ChatClientSettingsList = new List<ChatClientSettings>();
                    LoadChatClientSettingsFromConfig();
                }
                return _ChatClientSettingsList;
            }
            set
            {
                _ChatClientSettingsList = value;
                OnPropertyChanged();
            }
        }

        private ChatClientSettings _SelectedChatClientSettings;
        [ACPropertySelected(19, "ChatClientSettings", "en{'Selected Chat Client Settings'}de{'Ausgewählte Chat Client Einstellungen'}")]
        public ChatClientSettings SelectedChatClientSettings
        {
            get { return _SelectedChatClientSettings; }
            set
            {
                _SelectedChatClientSettings = value;
                if (_SelectedChatClientSettings != null)
                {
                    Endpoint = _SelectedChatClientSettings.Endpoint;
                    ApiKey = _SelectedChatClientSettings.ApiKey;
                    ModelName = _SelectedChatClientSettings.ModelName;
                    SelectedAIClientType = _SelectedChatClientSettings.AIClientType;
                }
                else
                {
                    Endpoint = C_DefaultEndpoint;
                    ApiKey = "";
                    ModelName = C_DefaultModelName;
                    SelectedAIClientType = "OpenAICompatible";
                }
                OnPropertyChanged();
            }
        }

        #endregion

        #endregion

        #region Private Fields

        private IChatClient _CurrentChatClient;
        private ServiceProvider _ServiceProvider;
        private ILoggerFactory _LoggerFactory;

        #endregion

        #region Methods

        #region Chat Client Settings Methods

        /// <summary>
        /// Loads the chat client settings from the ChatClientConfig JSON configuration
        /// </summary>
        private void LoadChatClientSettingsFromConfig()
        {
            try
            {
                if (!string.IsNullOrEmpty(ChatClientConfig))
                {
                    try
                    {
                        var converter = new ChatClientSettingsListJsonConverter();
                        var options = new JsonSerializerOptions();
                        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(ChatClientConfig);
                        Utf8JsonReader reader = new Utf8JsonReader(jsonBytes);

                        if (reader.Read())
                        {
                            var settings = converter.Read(ref reader, typeof(List<ChatClientSettings>), options);
                            if (settings != null)
                            {
                                _ChatClientSettingsList = settings;
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Messages.LogException(this.GetACUrl(), "LoadChatClientSettingsFromConfig - JSON parsing", ex);
                    }
                }
                OnPropertyChanged(nameof(ChatClientSettingsList));
                ChatClientSettings defaultLLM = null;
                if (_ChatClientSettingsList != null && _ChatClientSettingsList.Any())
                {
                    defaultLLM = _ChatClientSettingsList.Where(c => c.IsDefault).FirstOrDefault();
                    if (defaultLLM == null)
                        defaultLLM = _ChatClientSettingsList.FirstOrDefault();
                }
                SelectedChatClientSettings = defaultLLM;
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "LoadChatClientSettingsFromConfig", ex);
            }
        }

        /// <summary>
        /// Saves the available chat client settings to the ChatClientConfig JSON configuration
        /// </summary>
        [ACMethodCommand("SaveChatClientSettings", "en{'Save Chat Client Settings'}de{'Chat Client Einstellungen speichern'}", 108)]
        public void SaveChatClientSettings()
        {
            try
            {
                var converter = new ChatClientSettingsListJsonConverter();
                var options = new JsonSerializerOptions { WriteIndented = true };
                ChatClientConfig = JsonSerializer.Serialize(ChatClientSettingsList, options);

                ChatOutput = $"Chat client settings saved. {ChatClientSettingsList.Count} settings saved.";
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "SaveChatClientSettings", ex);
                ChatOutput = $"Error saving chat client settings: {ex.Message}";
            }
        }

        public bool IsEnabledSaveChatClientSettings()
        {
            return ChatClientSettingsList != null && ChatClientSettingsList.Any();
        }

        /// <summary>
        /// Adds a new chat client setting based on current properties
        /// </summary>
        [ACMethodCommand("AddChatClientSetting", "en{'Add Chat Client Setting'}de{'Chat Client Einstellung hinzufügen'}", 109)]
        public void AddChatClientSetting()
        {
            try
            {
                // Create a new setting based on current properties
                var newSetting = new ChatClientSettings
                {
                    Endpoint = Endpoint,
                    ApiKey = ApiKey,
                    ModelName = ModelName,
                    AIClientType = SelectedAIClientType
                };

                // Check if this setting already exists
                bool exists = ChatClientSettingsList.Any(s =>
                    s.Endpoint == newSetting.Endpoint &&
                    s.ModelName == newSetting.ModelName &&
                    s.AIClientType == newSetting.AIClientType);

                if (!exists)
                {
                    ChatClientSettingsList.Add(newSetting);
                    OnPropertyChanged(nameof(ChatClientSettingsList));
                    ChatOutput = $"Added new chat client setting: {newSetting}";
                }
                else
                {
                    ChatOutput = "Chat client setting already exists.";
                }
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "AddChatClientSetting", ex);
                ChatOutput = $"Error adding chat client setting: {ex.Message}";
            }
        }

        public bool IsEnabledAddChatClientSetting()
        {
            return !string.IsNullOrEmpty(Endpoint) &&
                   !string.IsNullOrEmpty(ModelName) &&
                   !string.IsNullOrEmpty(SelectedAIClientType);
        }

        /// <summary>
        /// Removes the selected chat client setting
        /// </summary>
        [ACMethodCommand("RemoveChatClientSetting", "en{'Remove Chat Client Setting'}de{'Chat Client Einstellung entfernen'}", 110)]
        public void RemoveChatClientSetting()
        {
            try
            {
                if (SelectedChatClientSettings != null && ChatClientSettingsList.Contains(SelectedChatClientSettings))
                {
                    ChatClientSettingsList.Remove(SelectedChatClientSettings);
                    SelectedChatClientSettings = null;
                    OnPropertyChanged(nameof(ChatClientSettingsList));
                    ChatOutput = "Chat client setting removed.";
                }
                else
                {
                    ChatOutput = "No chat client setting selected to remove.";
                }
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "RemoveChatClientSetting", ex);
                ChatOutput = $"Error removing chat client setting: {ex.Message}";
            }
        }

        public bool IsEnabledRemoveChatClientSetting()
        {
            return SelectedChatClientSettings != null && ChatClientSettingsList.Contains(SelectedChatClientSettings);
        }

        #endregion


        #region LLM Methods
        public bool EnsureAIClientsInitialized()
        {
            if (_ServiceProvider == null || _CurrentChatClient == null || _LLMPropsChanged)
            {
                InitializeAIClients();
                _LLMPropsChanged = false;
            }
            return _CurrentChatClient != null;
        }

        private void InitializeAIClients()
        {
            var services = new ServiceCollection();

            // Add logging
            services.AddLogging(builder => builder.AddConsole());

            // Register different AI clients based on selection
            services.AddSingleton<IChatClient>(provider =>
            {
                var loggerFactory = provider.GetService<ILoggerFactory>();
                return CreateChatClient(loggerFactory);
            });

            _ServiceProvider = services.BuildServiceProvider();
            _LoggerFactory = _ServiceProvider.GetService<ILoggerFactory>();

            InitializeSelectedAIClient();
        }

        private IChatClient CreateChatClient(ILoggerFactory loggerFactory)
        {
            switch (SelectedAIClientType)
            {
                case "Ollama":
                    return new OllamaApiClient(new Uri(Endpoint), ModelName);
                case "OpenAICompatible":
                    return new OpenAI.Chat.ChatClient(ModelName,
                        new System.ClientModel.ApiKeyCredential(ApiKey),
                        new OpenAI.OpenAIClientOptions() { Endpoint = new Uri(Endpoint) })
                    .AsIChatClient()
                    .AsBuilder()
                    .UseFunctionInvocation()
                    .UseOpenTelemetry(loggerFactory: loggerFactory)
                    .Build();
                case "OpenAI":
                    return new OpenAI.OpenAIClient(ApiKey)
                        .GetChatClient(ModelName)
                        .AsIChatClient()
                        .AsBuilder()
                        .UseFunctionInvocation()
                        .UseOpenTelemetry(loggerFactory: loggerFactory)
                        .Build();
                default:
                    throw new InvalidOperationException($"Unknown AI client type: {SelectedAIClientType}");
            }
        }

        private void InitializeSelectedAIClient()
        {
            try
            {
                _CurrentChatClient = _ServiceProvider?.GetService<IChatClient>();
                IsConnected = _CurrentChatClient != null;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                ChatOutput = $"Error initializing AI client: {ex.Message}";
            }
        }

        [ACMethodInfo("TestConnection", "en{'Test Connection'}de{'Verbindung testen'}", 104)]
        public async Task TestConnection()
        {
            EnsureAIClientsInitialized();

            if (_CurrentChatClient == null)
            {
                ChatOutput = "No AI client initialized";
                return;
            }

            try
            {
                var testMessage = new List<ChatMessage>
                {
                    new ChatMessage(ChatRole.User, "Hello, this is a test message.")
                };

                var response = await _CurrentChatClient.GetResponseAsync(testMessage);
                ChatOutput = $"Connection test successful. Response: {response}";
                IsConnected = true;
            }
            catch (Exception ex)
            {
                ChatOutput = $"Connection test failed: {ex.Message}";
                IsConnected = false;
            }
        }
        #endregion

        #endregion
    }
}