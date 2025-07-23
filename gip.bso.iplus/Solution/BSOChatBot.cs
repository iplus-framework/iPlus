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
    [ACClassInfo(Const.PackName_VarioSystem, "en{'AI-Chatbot'}de{'AI-Chatbot'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, false, true)]
    public class BSOChatBot : ACBSO
    {
        #region c'tors

        public BSOChatBot(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _MCPServerConfig = new ACPropertyConfigValue<string>(this, nameof(MCPServerConfig), "{\r\n  \"mcpServers\": {\r\n    \"iPlus\": {\r\n      \"command\": \"npx\",\r\n      \"args\": [\r\n        \"mcp-remote\",\r\n        \"http://localhost:8750/sse\",\r\n        \"--allow-http\"\r\n      ]\r\n    },\r\n    \"github\": {\r\n      \"command\": \"npx\",\r\n      \"args\": [\r\n        \"-y\",\r\n        \"@modelcontextprotocol/server-github\"\r\n      ],\r\n      \"env\": {\r\n        \"GITHUB_PERSONAL_ACCESS_TOKEN\": \"<YOUR_TOKEN>\"\r\n      }\r\n    }\r\n  }\r\n}");
            
            _chatMessagesObservable = new ObservableCollection<ChatMessageWrapper>();
            _currentChatUpdates = new ObservableCollection<ChatResponseUpdateWrapper>();
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_mcpClients != null)
            {
                foreach (var mcpClient in _mcpClients.Values)
                {
                    mcpClient?.DisposeAsync().AsTask().Wait();
                }
                _mcpClients.Clear();
            }
            _serviceProvider?.Dispose();
            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion

        #region Properties

        #region Chat Properties
        private string _chatInput = "";
        [ACPropertyInfo(1, "ChatInput", "en{'Chat Input'}de{'Chat Eingabe'}")]
        public string ChatInput
        {
            get { return _chatInput; }
            set
            {
                _chatInput = value;
                OnPropertyChanged();
            }
        }

        private string _chatOutput = "";
        [ACPropertyInfo(2, "ChatOutput", "en{'Chat Output'}de{'Chat Ausgabe'}")]
        public string ChatOutput
        {
            get { return _chatOutput; }
            set
            {
                _chatOutput = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ChatMessageWrapper> _chatMessagesObservable;
        [ACPropertyList(3, "ChatMessagesObservable", "en{'Chat Messages'}de{'Chat Nachrichten'}")]
        public ObservableCollection<ChatMessageWrapper> ChatMessagesObservable
        {
            get { return _chatMessagesObservable; }
            set
            {
                _chatMessagesObservable = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ChatResponseUpdateWrapper> _currentChatUpdates;
        [ACPropertyList(4, "CurrentChatUpdates", "en{'Current Chat Updates'}de{'Aktuelle Chat Updates'}")]
        public ObservableCollection<ChatResponseUpdateWrapper> CurrentChatUpdates
        {
            get { return _currentChatUpdates; }
            set
            {
                _currentChatUpdates = value;
                OnPropertyChanged();
            }
        }

        private List<string> _chatHistory;
        [ACPropertyList(11, "ChatHistory", "en{'Chat History'}de{'Chat Verlauf'}")]
        public List<string> ChatHistory
        {
            get
            {
                if (_chatHistory == null)
                    _chatHistory = new List<string>();
                return _chatHistory;
            }
        }
        #endregion

        #region LLM Properties
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
        [ACPropertyInfo(6, "SelectedAIClientType", "en{'Selected AI Client'}de{'Ausgewählter AI Client'}")]
        public string SelectedAIClientType
        {
            get { return _selectedAIClientType; }
            set
            {
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
                _apiKey = value;
                OnPropertyChanged();
            }
        }

        private string _endpoint = "https://openrouter.ai/api/v1";
        [ACPropertyInfo(8, "Endpoint", "en{'Endpoint'}de{'Endpunkt'}")]
        public string Endpoint
        {
            get { return _endpoint; }
            set
            {
                _endpoint = value;
                OnPropertyChanged();
            }
        }

        private string _modelName = "google/gemini-2.5-flash-lite-preview-06-17";
        [ACPropertyInfo(9, "ModelName", "en{'Model Name'}de{'Modell Name'}")]
        public string ModelName
        {
            get { return _modelName; }
            set
            {
                _modelName = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region MCP Properties
        private bool _isConnected = false;
        [ACPropertyInfo(10, "IsConnected", "en{'Is Connected'}de{'Ist Verbunden'}")]
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }


        protected ACPropertyConfigValue<string> _MCPServerConfig;
        [ACPropertyInfo(12, "McpServerCommand", "en{'MCP Server JSON config'}de{'MCP Server JSON config'}")]
        public string MCPServerConfig
        {
            get { return _MCPServerConfig.ValueT; }
            set
            {
                _MCPServerConfig.ValueT = value;
                OnPropertyChanged();
            }
        }

        private string _MCPServerConfigFromFile;
        [ACPropertyInfo(13, "McpServerCommand", "en{'MCP Server JSON config from file'}de{'MCP Server JSON config Datei'}")]
        public string MCPServerConfigFromFile
        {
            get { return _MCPServerConfigFromFile; }
            set
            {
                _MCPServerConfigFromFile = value;
                OnPropertyChanged();
            }
        }

        private bool _mcpConnected = false;
        [ACPropertyInfo(14, "McpConnected", "en{'MCP Connected'}de{'MCP Verbunden'}")]
        public bool McpConnected
        {
            get { return _mcpConnected; }
            set
            {
                _mcpConnected = value;
                OnPropertyChanged();
            }
        }

        private List<AITool> _availableTools;
        public List<AITool> AvailableTools
        {
            get
            {
                if (_availableTools == null)
                    _availableTools = new List<AITool>();
                return _availableTools;
            }
        }

        [ACPropertyList(15, "AvailableTools", "en{'Available Tools'}de{'Verfügbare Tools'}")]
        public List<String> AvailableToolsNames
        {
            get { return AvailableTools.Select(t => t.Name).ToList(); }
        }
        #endregion

        #region Private Fields

        private IChatClient _currentChatClient;
        private Dictionary<string, IMcpClient> _mcpClients;
        private ServiceProvider _serviceProvider;
        private ILoggerFactory _loggerFactory;

        #endregion

        #endregion

        #region JSON Models for MCP Configuration

        public class McpServerConfig
        {
            public Dictionary<string, McpServerInfo> mcpServers { get; set; }
        }

        public class McpServerInfo
        {
            public string command { get; set; }
            public string[] args { get; set; }
            public Dictionary<string, string> env { get; set; }
        }

        #endregion


        #region Methods

        #region LLM Methods
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

            _serviceProvider = services.BuildServiceProvider();
            _loggerFactory = _serviceProvider.GetService<ILoggerFactory>();

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
                _currentChatClient = _serviceProvider.GetService<IChatClient>();
                IsConnected = _currentChatClient != null;
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
            if (_serviceProvider == null)
                InitializeAIClients();

            if (_currentChatClient == null)
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

                var response = await _currentChatClient.GetResponseAsync(testMessage);
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

        #region MCP Methods
        [ACMethodCommand("", "en{'Read MCP-Config from file'}de{'MCP-Konfiguration von Datei lesen'}", 106)]
        public void ReadMCPServerConfigFromFile()
        {
            try
            {

                ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
                string filePath = mediaController.OpenFileDialog(
                    false,
                    "",
                    false,
                    null,
                    new Dictionary<string, string>()
                    {
                    {
                        "Select MCP Server Config File",
                        "*.json"
                    }
                    });

                if (filePath != null && File.Exists(filePath))
                {
                    string jsonContent = File.ReadAllText(filePath);
                    MCPServerConfigFromFile = jsonContent;
                }
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "SelectLogFile", ex);
                Root.Messages.Error(this, "Error selecting log file: " + ex.Message, true);
            }
        }

        [ACMethodInfo("PingMcpServer", "en{'Ping MCP Server'}de{'MCP Server anpingen'}", 105)]
        public async Task PingMcpServer()
        {
            if (_mcpClients == null || _mcpClients.Count == 0)
            {
                ChatOutput = "No MCP clients connected";
                return;
            }

            var results = new List<string>();
            foreach (var serverEntry in _mcpClients)
            {
                try
                {
                    await serverEntry.Value.PingAsync();
                    results.Add($"{serverEntry.Key}: OK");
                }
                catch (Exception ex)
                {
                    results.Add($"{serverEntry.Key}: Failed - {ex.Message}");
                }
            }

            ChatOutput = $"MCP server ping results:\n{string.Join("\n", results)}";
        }

        [ACMethodInfo("ConnectMCP", "en{'Connect MCP'}de{'MCP verbinden'}", 102)]
        public async Task ConnectMCP()
        {
            try
            {
                // Initialize MCP clients dictionary if needed
                if (_mcpClients == null)
                    _mcpClients = new Dictionary<string, IMcpClient>();

                // Parse the JSON configuration
                McpServerConfig config;
                try
                {
                    config = JsonSerializer.Deserialize<McpServerConfig>(!string.IsNullOrEmpty(MCPServerConfigFromFile) ? MCPServerConfigFromFile : MCPServerConfig);
                    if (config?.mcpServers == null || config.mcpServers.Count == 0)
                    {
                        ChatOutput = "No MCP servers configured in JSON config";
                        return;
                    }
                }
                catch (JsonException ex)
                {
                    ChatOutput = $"Error parsing MCP server configuration JSON: {ex.Message}";
                    return;
                }

                // Clear existing connections
                await DisconnectMCP();

                int totalTools = 0;
                var connectedServers = new List<string>();

                // Create MCP client with sampling capability
                var samplingHandler = _currentChatClient?.CreateSamplingHandler();
                var clientOptions = new McpClientOptions
                {
                    Capabilities = new ClientCapabilities
                    {
                        Sampling = samplingHandler != null ? new SamplingCapability { SamplingHandler = samplingHandler } : null
                    }
                };

                // Connect to each MCP server
                foreach (var serverEntry in config.mcpServers)
                {
                    string serverName = serverEntry.Key;
                    McpServerInfo serverInfo = serverEntry.Value;

                    try
                    {
                        // Create stdio transport for MCP server
                        var transport = new StdioClientTransport(new StdioClientTransportOptions
                        {
                            Command = serverInfo.command,
                            Arguments = serverInfo.args ?? new string[0],
                            Name = serverName,
                        }, _loggerFactory);

                        // Connect to MCP server
                        var mcpClient = await McpClientFactory.CreateAsync(
                            transport,
                            clientOptions,
                            _loggerFactory);

                        _mcpClients[serverName] = mcpClient;

                        // Get available tools from this server
                        var tools = await mcpClient.ListToolsAsync();
                        AvailableTools.AddRange(tools);
                        totalTools += tools.Count;
                        connectedServers.Add($"{serverName} ({tools.Count} tools)");
                    }
                    catch (Exception ex)
                    {
                        ChatOutput += $"Failed to connect to MCP server '{serverName}': {ex.Message}\n";
                    }
                }

                if (_mcpClients.Count > 0)
                {
                    McpConnected = true;
                    ChatOutput = $"Connected to {_mcpClients.Count} MCP server(s): {string.Join(", ", connectedServers)}. Total {totalTools} tools available.";
                }
                else
                {
                    McpConnected = false;
                    ChatOutput = "Failed to connect to any MCP servers.";
                }

                OnPropertyChanged(nameof(AvailableTools));
                OnPropertyChanged(nameof(AvailableToolsNames));
            }
            catch (Exception ex)
            {
                McpConnected = false;
                ChatOutput = $"Error connecting MCP clients: {ex.Message}";
            }
        }

        [ACMethodInfo("DisconnectMCP", "en{'Disconnect MCP'}de{'MCP trennen'}", 103)]
        public async Task DisconnectMCP()
        {
            try
            {
                if (_mcpClients != null)
                {
                    foreach (var mcpClient in _mcpClients.Values)
                    {
                        if (mcpClient != null)
                            await mcpClient.DisposeAsync();
                    }
                    _mcpClients.Clear();
                }

                AvailableTools.Clear();
                McpConnected = false;
                ChatOutput = "All MCP clients disconnected";
                OnPropertyChanged(nameof(AvailableTools));
            }
            catch (Exception ex)
            {
                ChatOutput = $"Error disconnecting MCP clients: {ex.Message}";
            }
        }
        #endregion

        #region Chat Methods
        [ACMethodInfo("SendMessage", "en{'Send Message'}de{'Nachricht senden'}", 100)]
        public async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(ChatInput) || _currentChatClient == null)
                return;

            try
            {
                // Add user message to history and observable collection
                string userMessage = ChatInput;
                var userMessageWrapper = new ChatMessageWrapper("User", userMessage);

                ChatHistory.Add($"User: {userMessage}");
                ChatMessagesObservable.Add(userMessageWrapper);

                // Clear current updates for new AI response
                CurrentChatUpdates.Clear();

                // Create AI message wrapper for the response
                var aiMessageWrapper = new ChatMessageWrapper("AI", "");
                ChatMessagesObservable.Add(aiMessageWrapper);

                // Prepare chat messages
                var messages = new List<ChatMessage>
                {
                    new ChatMessage(ChatRole.User, ChatInput)
                };

                // Create chat options with available tools from MCP
                var chatOptions = new ChatOptions();
                if (_mcpClients != null && _mcpClients.Count > 0 && McpConnected)
                {
                    chatOptions.Tools = AvailableTools;
                }

                // Send to AI client and update UI in real-time
                List<ChatResponseUpdate> updates = [];
                await foreach (var update in _currentChatClient.GetStreamingResponseAsync(messages, chatOptions))
                {
                    updates.Add(update);

                    // Create wrapper and add to observable collections
                    var updateWrapper = new ChatResponseUpdateWrapper(update);
                    CurrentChatUpdates.Add(updateWrapper);
                    aiMessageWrapper.Updates.Add(updateWrapper);

                    // Update the AI message content
                    aiMessageWrapper.Content = string.Join("", aiMessageWrapper.Updates.Select(u => u.DisplayText));

                    // Trigger property change notifications
                    aiMessageWrapper.Refresh();
                }

                // Add final AI response to history
                string finalAiResponse = string.Join("", updates.Select(u => u.Text ?? ""));
                ChatHistory.Add($"AI: {finalAiResponse}");

                // Update output with token usage information
                var tokenUsage = aiMessageWrapper.TokenUsageSummary;
                ChatOutput = !string.IsNullOrEmpty(tokenUsage)
                    ? $"{finalAiResponse}\n\n{tokenUsage}"
                    : finalAiResponse;

                // Clear input
                ChatInput = "";

                OnPropertyChanged(nameof(ChatHistory));
            }
            catch (Exception ex)
            {
                ChatOutput = $"Error sending message: {ex.Message}";

                // Add error message to observable collection
                var errorMessageWrapper = new ChatMessageWrapper("System", $"Error: {ex.Message}");
                ChatMessagesObservable.Add(errorMessageWrapper);
            }
        }

        [ACMethodCommand("ClearHistory", "en{'Clear History'}de{'Verlauf löschen'}", 101)]
        public void ClearHistory()
        {
            ChatHistory.Clear();
            ChatOutput = "";
            ChatMessagesObservable.Clear();
            CurrentChatUpdates.Clear();
            OnPropertyChanged(nameof(ChatHistory));
        }
        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(SendMessage):
                    _ = SendMessage();
                    return true;
                case nameof(ClearHistory):
                    ClearHistory();
                    return true;
                case nameof(ConnectMCP):
                    _ = ConnectMCP();
                    return true;
                case nameof(DisconnectMCP):
                    _ = DisconnectMCP();
                    return true;
                //case nameof(TestConnection):
                //    _ = TestConnection();
                //    return true;
                case nameof(PingMcpServer):
                    _ = PingMcpServer();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #endregion

    }
}