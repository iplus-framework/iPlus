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
            _AllowedTools = new ACPropertyConfigValue<string>(this, nameof(AllowedTools), "[]");
            _ToolCheckList = new List<ACObjectItemWCheckBox>();
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACPostInit()
        {
            ReloadChatHistory();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_McpClients != null)
            {
                foreach (var mcpClient in _McpClients.Values)
                {
                    mcpClient?.DisposeAsync().AsTask().Wait();
                }
                _McpClients.Clear();
            }
            _ServiceProvider?.Dispose();
            bool result = base.ACDeInit(deleteACClassTask);

            if (result && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return result;
        }

        #endregion

        #region Properties

        #region Database
        private Database _BSODatabase = null;
        /// <summary>
        /// Overriden: Returns a separate database context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_BSODatabase == null)
                    _BSODatabase = ACObjectContextManager.GetOrCreateContext<Database>(this.GetACUrl());
                return _BSODatabase;
            }
        }

        public Database Db
        {
            get
            {
                return Database as Database;
            }
        }
        #endregion

        #region History
        private ACClassConfig _SelectedChatHistoryConfig;
        private ChatConfigWrapper _SelectedChatHistory;
        [ACPropertySelected(151, "ChatHistory", "en{'Selected Chat History'}de{'Ausgewählte Chat Historie'}")]
        public ChatConfigWrapper SelectedChatHistory
        {
            get
            {
                return _SelectedChatHistory;
            }
            set
            {
                bool changed = _SelectedChatHistory != value;
                _SelectedChatHistory = value;
                if (value == null)
                    _SelectedChatHistoryConfig = null;
                OnPropertyChanged();
                if (changed)
                {
                    LoadSelectedChatHistory();
                }
            }
        }

        private ACClass _ThisACClass;
        List<ChatConfigWrapper> _ChatHistoryList;
        [ACPropertyList(150, "ChatHistory", "en{'Chat History'}de{'Chat History'}")]
        public List<ChatConfigWrapper> ChatHistoryList
        {
            get
            {
                if (_ChatHistoryList != null)
                    return _ChatHistoryList;
                ReloadChatHistory();
                return _ChatHistoryList;
            }
        }

        protected void ReloadChatHistory()
        {
            if (_ThisACClass == null)
            {
                _ThisACClass = Db.ACClass.Where(c => c.ACClassID == ComponentClass.ACClassID).FirstOrDefault();
            }
            _ChatHistoryList = Db.ACClassConfig.Where(c => c.ACClassID == ComponentClass.ACClassID && c.LocalConfigACUrl == nameof(ChatHistoryList))
                .OrderByDescending(c => c.InsertDate)
                .Select(c => new ChatConfigWrapper(null, c.ACClassConfigID, c.Comment, c.InsertDate))
                .ToList();
            OnPropertyChanged(nameof(ChatHistoryList));
        }

        // Replace the incorrect usage of Utf8JsonReader.Create with the correct instantiation and usage of Utf8JsonReader
        protected void LoadSelectedChatHistory()
        {
            if (SelectedChatHistory == null)
            {
                _SelectedChatHistoryConfig = null;
                ChatMessagesObservable = new ObservableCollection<ChatMessageWrapper>();
                CurrentChatUpdates = new ObservableCollection<ChatResponseUpdateWrapper>();
                return;
            }
            _SelectedChatHistoryConfig = Db.ACClassConfig.Where(c => c.ACClassConfigID == SelectedChatHistory.ACClassConfigID).FirstOrDefault();
            if (_SelectedChatHistoryConfig != null && !string.IsNullOrEmpty(_SelectedChatHistoryConfig.XMLConfig))
            {
                try
                {
                    // TODO: Attach ParentACObject
                    ChatMessageWrapperCollectionJsonConverter chatMessageconverter = new ChatMessageWrapperCollectionJsonConverter();
                    byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(_SelectedChatHistoryConfig.XMLConfig);
                    Utf8JsonReader reader = new Utf8JsonReader(jsonBytes);

                    // CRITICAL: Advance the reader to the first token before using it
                    if (!reader.Read())
                    {
                        throw new JsonException("Empty JSON data");
                    }

                    ObservableCollection<ChatMessageWrapper> chatMessages = chatMessageconverter.Read(ref reader, typeof(ObservableCollection<ChatMessageWrapper>), new JsonSerializerOptions()) as ObservableCollection<ChatMessageWrapper>;
                    chatMessages.AttachBSO(this);
                    ChatMessagesObservable = chatMessages;
                    return;
                }
                catch (Exception ex)
                {
                    Messages.LogException(ChatInput, "LoadSelectedChatHistory", ex);
                }
            }
            if (_SelectedChatHistoryConfig == null)
                SelectedChatHistory = null;
            ChatMessagesObservable = new ObservableCollection<ChatMessageWrapper>();
            CurrentChatUpdates = new ObservableCollection<ChatResponseUpdateWrapper>();
        }

        protected void SaveCurrentChat()
        {
            if (_SelectedChatHistoryConfig == null)
                return;
            try
            {
                ChatMessageWrapperCollectionJsonConverter chatMessageWrapperCollectionJsonConverter = new ChatMessageWrapperCollectionJsonConverter();
                var options = new JsonSerializerOptions();
                options.Converters.Add(chatMessageWrapperCollectionJsonConverter);
                _SelectedChatHistoryConfig.XMLConfig = JsonSerializer.Serialize(ChatMessagesObservable, options);
                Db.ACSaveChanges();
            }
            catch (Exception ex)
            {
                Messages.LogException(ChatInput, "SaveCurrentChat", ex);
            }
        }
        #endregion

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

        private string _endpoint = "https://openrouter.ai/api/v1";
        [ACPropertyInfo(8, "Endpoint", "en{'Endpoint'}de{'Endpunkt'}")]
        public string Endpoint
        {
            get { return _endpoint; }
            set
            {
                if (_endpoint != value)
                    _LLMPropsChanged = true;
                _endpoint = value;
                OnPropertyChanged();
            }
        }

        private string _modelName = "google/gemini-2.0-flash-exp:free";
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

        private ACObjectItemWCheckBox _SelectedToolCheck;
        [ACPropertySelected(16, "ToolsSelection", "en{'Available Tools (with selection)'}de{'Verfügbare Tools (mit Auswahl)'}")]
        public ACObjectItemWCheckBox SelectedToolCheck
        {
            get { return _SelectedToolCheck; }
            set
            {
                _SelectedToolCheck = value;
                OnPropertyChanged();
            }
        }

        private List<ACObjectItemWCheckBox> _ToolCheckList;
        [ACPropertyList(17, "ToolsSelection", "en{'Available Tools (with selection)'}de{'Verfügbare Tools (mit Auswahl)'}")]
        public List<ACObjectItemWCheckBox> ToolCheckList
        {
            get { return _ToolCheckList; }
            set
            {
                _ToolCheckList = value;
                OnPropertyChanged();
            }
        }

        protected ACPropertyConfigValue<string> _AllowedTools;
        [ACPropertyInfo(62, "McpServerCommand", "en{'JSON Array with allowed toolnames}de{'JSON Array with allowed toolnames'}")]
        public string AllowedTools
        {
            get { return _AllowedTools.ValueT; }
            set
            {
                _AllowedTools.ValueT = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Private Fields

        private IChatClient _CurrentChatClient;
        private Dictionary<string, IMcpClient> _McpClients;
        private ServiceProvider _ServiceProvider;
        private ILoggerFactory _LoggerFactory;

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
                _CurrentChatClient = _ServiceProvider.GetService<IChatClient>();
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

        #region MCP Methods
        public bool EnsureMCPClientsInitialized()
        {
            if (_McpClients == null || _McpClients.Count == 0 && (!string.IsNullOrEmpty(MCPServerConfig) || !string.IsNullOrEmpty(MCPServerConfigFromFile)))
            {
                ConnectMCP().Wait();
            }
            return McpConnected;
        }

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
            if (_McpClients == null || _McpClients.Count == 0)
            {
                ChatOutput = "No MCP clients connected";
                return;
            }

            var results = new List<string>();
            foreach (var serverEntry in _McpClients)
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
                if (_McpClients == null)
                    _McpClients = new Dictionary<string, IMcpClient>();

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
                var samplingHandler = _CurrentChatClient?.CreateSamplingHandler();
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
                        }, _LoggerFactory);

                        // Connect to MCP server
                        var mcpClient = await McpClientFactory.CreateAsync(
                            transport,
                            clientOptions,
                            _LoggerFactory);

                        _McpClients[serverName] = mcpClient;

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

                if (_McpClients.Count > 0)
                {
                    McpConnected = true;
                    ChatOutput = $"Connected to {_McpClients.Count} MCP server(s): {string.Join(", ", connectedServers)}. Total {totalTools} tools available.";
                    
                    // Populate the AvailableToolsWithSelection list after loading tools
                    PopulateToolsWithSelection();
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
                if (_McpClients != null)
                {
                    foreach (var mcpClient in _McpClients.Values)
                    {
                        if (mcpClient != null)
                            await mcpClient.DisposeAsync();
                    }
                    _McpClients.Clear();
                }

                AvailableTools.Clear();
                ToolCheckList.Clear();
                McpConnected = false;
                ChatOutput = "All MCP clients disconnected";
                _McpClients = new Dictionary<string, IMcpClient>();
                OnPropertyChanged(nameof(AvailableTools));
            }
            catch (Exception ex)
            {
                ChatOutput = $"Error disconnecting MCP clients: {ex.Message}";
            }
        }

        /// <summary>
        /// Populates the AvailableToolsWithSelection list with ACObjectItemWCheckBox instances
        /// and sets the IsChecked property based on the AllowedTools JSON configuration
        /// </summary>
        private void PopulateToolsWithSelection()
        {
            try
            {
                ToolCheckList.Clear();
                
                // Parse the allowed tools from JSON
                HashSet<string> allowedToolNames = new HashSet<string>();
                if (!string.IsNullOrEmpty(AllowedTools))
                {
                    try
                    {
                        var allowedArray = JsonSerializer.Deserialize<string[]>(AllowedTools);
                        if (allowedArray != null)
                        {
                            allowedToolNames = new HashSet<string>(allowedArray);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Messages.LogException(this.GetACUrl(), "PopulateToolsWithSelection - JSON parsing", ex);
                    }
                }

                // Create ACObjectItemWCheckBox instances for each available tool
                foreach (var tool in AvailableTools)
                {
                    bool isSelected = allowedToolNames.Contains(tool.Name);
                    var toolItem = new ACObjectItemWCheckBox(this, tool.Name, isSelected)
                    {
                        ACObject = new ACValueItem(tool.Name, tool, null)
                    };
                    toolItem.IsChecked = isSelected;
                    ToolCheckList.Add(toolItem);
                }
                OnPropertyChanged(nameof(ToolCheckList));
                OnPropertyChanged(nameof(SelectedToolCheck));
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "PopulateToolsWithSelection", ex);
            }
        }

        /// <summary>
        /// Saves the currently selected tools to the AllowedTools JSON configuration
        /// </summary>
        [ACMethodCommand("SaveToolSelection", "en{'Save Tool Selection'}de{'Tool-Auswahl speichern'}", 107)]
        public void SaveToolSelection()
        {
            try
            {
                var selectedToolNames = ToolCheckList
                    .Where(item => item.IsChecked)
                    .Select(item => item.ACCaption)
                    .ToArray();

                AllowedTools = JsonSerializer.Serialize(selectedToolNames);
                
                ChatOutput = $"Tool selection saved. {selectedToolNames.Length} tools selected.";
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "SaveToolSelection", ex);
                ChatOutput = $"Error saving tool selection: {ex.Message}";
            }
        }

        public bool IsEnabledSaveToolSelection()
        {
            // Enable SaveToolSelection only if there are available tools and at least one is selected
            return ToolCheckList != null && ToolCheckList.Any(item => item.IsChecked);
        }

        /// <summary>
        /// Gets the selected tools for use in chat
        /// </summary>
        /// <returns>List of selected AITool instances</returns>
        private List<AITool> GetSelectedTools()
        {
            if (ToolCheckList == null || !ToolCheckList.Any())
                return new List<AITool>();

            return ToolCheckList
                .Where(item => item.IsChecked && ((ACValueItem)item.ACObject).Value is AITool)
                .Select(item => (AITool)((ACValueItem)item.ACObject).Value)
                .ToList();
        }
        #endregion

        #region Chat Methods
        [ACMethodInfo("SendMessage", "en{'Send Message'}de{'Nachricht senden'}", 100)]
        public async Task SendMessage()
        {
            if (!EnsureAIClientsInitialized())
            {
                ChatOutput = "AI client not initialized or connection failed.";
                return;
            }

            if (string.IsNullOrWhiteSpace(ChatInput) || _CurrentChatClient == null)
                return;

            EnsureMCPClientsInitialized();

            if (_SelectedChatHistoryConfig == null)
                NewChat();

            if (_SelectedChatHistoryConfig == null || _SelectedChatHistory == null || _SelectedChatHistory.ACClassConfigID != _SelectedChatHistoryConfig.ACClassConfigID)
                return;

            try
            {
                CurrentChatUpdates = new ObservableCollection<ChatResponseUpdateWrapper>();
                ChatMessagesObservable.AddUserMessage(this, ChatInput);
                var aiMessageWrapper = ChatMessagesObservable.CreateAndAddNewAssistentMessage(this);
                var messages = ChatMessagesObservable.GetConversations();
                // Create chat options with selected tools from MCP
                var chatOptions = new ChatOptions();
                if (_McpClients != null && _McpClients.Count > 0 && McpConnected)
                {
                    // Use only the selected tools instead of all available tools
                    var selectedTools = GetSelectedTools();
                    chatOptions.Tools = selectedTools;
                    chatOptions.AllowMultipleToolCalls = true;
                    chatOptions.ToolMode = ChatToolMode.RequireAny;
                }

                // Send to AI client and update UI in real-time
                List<ChatResponseUpdate> updates = [];
                await foreach (var update in _CurrentChatClient.GetStreamingResponseAsync(messages, chatOptions))
                {
                    updates.Add(update);

                    // Create wrapper and add to observable collections
                    var updateWrapper = new ChatResponseUpdateWrapper(this, update);
                    CurrentChatUpdates.Add(updateWrapper);
                    aiMessageWrapper.Updates.Add(updateWrapper);

                    // Update the AI message content
                    //aiMessageWrapper.Content = string.Join("", aiMessageWrapper.Updates.Select(u => u.DisplayText));

                    // Trigger property change notifications
                    aiMessageWrapper.Refresh();
                }

                // Add final AI response to history
                string finalAiResponse = string.Join("", updates.Select(u => u.Text ?? ""));
                // Update output with token usage information
                var tokenUsage = aiMessageWrapper.TokenUsageSummary;
                ChatOutput = !string.IsNullOrEmpty(tokenUsage)
                    ? $"{finalAiResponse}\n\n{tokenUsage}"
                    : finalAiResponse;

                if (_SelectedChatHistoryConfig != null && (string.IsNullOrEmpty(_SelectedChatHistoryConfig.Comment) || _SelectedChatHistoryConfig.Comment.StartsWith("###")))
                    _SelectedChatHistoryConfig.Comment = ChatInput + " " + finalAiResponse;
                // Clear input
                SaveCurrentChat();
                ChatInput = "";
            }
            catch (Exception ex)
            {
                ChatOutput = $"Error sending message: {ex.Message}";

                // Add error message to observable collection
                var errorMessageWrapper = new ChatMessageWrapper(this, ChatRole.System, $"Error: {ex.Message}");
                ChatMessagesObservable.Add(errorMessageWrapper);
            }
        }

        [ACMethodCommand("ClearHistory", "en{'Clear Chat'}de{'Chat löschen'}", 101)]
        public void ClearChat()
        {
            ChatInput = "";
            ChatOutput = "";
            ChatMessagesObservable.Clear();
            CurrentChatUpdates.Clear();
        }

        [ACMethodCommand("ClearHistory", "en{'New Chat'}de{'Neuer Chat'}", 102)]
        public void NewChat()
        {
            ClearChat();
            SelectedChatHistory = null;
            if (ChatHistoryList != null && ChatHistoryList.Count > 50)
            {
                var lastChat = ChatHistoryList.LastOrDefault();
                if (lastChat != null)
                {
                    ChatHistoryList.Remove(lastChat);
                    var lastConfig = Db.ACClassConfig.Where(c => c.ACClassConfigID == lastChat.ACClassConfigID).FirstOrDefault();
                    if (lastConfig != null)
                    {
                        lastConfig.DeleteACObject(Db, false);
                        Db.ACSaveChanges();
                    }
                }
            }
            if (_ThisACClass != null)
            {
                ACClassConfig newChatConfig = ACClassConfig.NewACObject(Db, _ThisACClass);
                newChatConfig.LocalConfigACUrl = nameof(ChatHistoryList);
                Db.ACSaveChanges();
                var configWrapper = new ChatConfigWrapper(null, newChatConfig.ACClassConfigID, String.Format("###{0:yyyy-MM-dd-HH:mm:ss.ffff}", newChatConfig.InsertDate), newChatConfig.InsertDate);
                ChatHistoryList.Add(configWrapper);
                SelectedChatHistory = configWrapper;
                OnPropertyChanged(nameof(ChatHistoryList));
                SelectedChatHistory = configWrapper;
            }
        }
        #endregion

        #region Design

        [ACMethodInfo("", "en{'Template selector'}de{'Template selector'}", 110)]
        public string SelectTemplate4ChatMessageWrapper(object content)
        {
            if (content is ChatWrapperBase chatMessage)
            {
                ChatRole chatRole = chatMessage.ChatMessageRole ?? ChatRole.System; // Use null-coalescing operator
                if (chatRole.Equals(ChatRole.User))
                {
                    return "UserMessageTemplate";
                }
                else if (chatRole.Equals(ChatRole.Assistant) || chatRole.Equals(ChatRole.Tool))
                {
                    return "AIMessageTemplate";
                }
                else if (chatRole.Equals(ChatRole.System))
                {
                    return "SystemMessageTemplate";
                }
                else
                {
                    return "AIMessageTemplate";
                }
            }

            return "";
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
                case nameof(ClearChat):
                    ClearChat();
                    return true;
                case nameof(ConnectMCP):
                    _ = ConnectMCP();
                    return true;
                case nameof(DisconnectMCP):
                    _ = DisconnectMCP();
                    return true;
                case nameof(SaveToolSelection):
                    SaveToolSelection();
                    return true;
                case nameof(IsEnabledSaveToolSelection):
                    result = IsEnabledSaveToolSelection();
                    return true;
                case nameof(ReadMCPServerConfigFromFile):
                    ReadMCPServerConfigFromFile();
                    return true;
                case nameof(TestConnection):
                    _ = TestConnection();
                    return true;
                case nameof(PingMcpServer):
                    _ = PingMcpServer();
                    return true;
                case nameof(NewChat):
                    NewChat();
                    return true;
                case nameof(SelectTemplate4ChatMessageWrapper):
                    if (acParameter != null && acParameter.Length > 0 && acParameter[0] is IVBContent content)
                    {
                        result = SelectTemplate4ChatMessageWrapper(content);
                        return true;
                    }
                    break;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #endregion

    }
}