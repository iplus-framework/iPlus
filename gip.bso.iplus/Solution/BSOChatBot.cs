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
using System.Threading;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'AI-Chatbot'}de{'AI-Chatbot'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, true, true,
    Description = @"BSOChatBot - AI Chatbot Business Object for iPlus Framework

OVERVIEW:
BSOChatBot is a comprehensive AI chatbot integration for the iPlus framework that supports multiple AI providers (OpenAI, Ollama, etc.) and Model Context Protocol (MCP) for tool integration. It manages chat conversations, history, and real-time streaming responses.

KEY PROPERTIES:

CHAT INTERACTION:
- ChatInput (string): Set the message/task for the chatbot before calling SendMessage()
- ChatOutput (string): Read-only. Contains the latest response from the AI agent
- IsAgentRunning (bool): Read-only. Indicates if the agent is currently processing. When true, the agent is busy and you must wait for completion before sending new messages
- AgentStopRequested (bool): Read-only. Indicates if StopAgent() has been called
- AgentIsWaitingForWakeup (bool): Read-only. True when agent waits for wakeup to check subagent status

CHAT MANAGEMENT:
- ChatMessagesObservable (ObservableCollection<ChatMessageWrapper>): Complete conversation history with the chatbot
- SelectedChatMessage (ChatMessageWrapper): Currently selected message in the conversation
- CurrentChatUpdates (ObservableCollection<ChatResponseUpdateWrapper>): Real-time updates during active agent processing
- ChatHistoryList (List<ChatConfigWrapper>): List of saved chat sessions
- SelectedChatHistory (ChatConfigWrapper): Currently selected chat history session

AI CLIENT CONFIGURATION:
- ChatClientSettingsList (List<ChatClientSettings>): Available AI client configurations
- SelectedChatClientSettings (ChatClientSettings): Currently active AI client configuration
- ChatClientConfig (string): JSON configuration for AI clients

MCP INTEGRATION:
- McpConnected (bool): Read-only. Status of MCP server connections
- ToolCheckList (List<ACObjectItemWCheckBox>): Available MCP tools with selection checkboxes
- AllowedTools (string): JSON array of selected tool names
- MCPServerConfig (string): JSON configuration for MCP servers

KEY METHODS:

CORE CHAT OPERATIONS:
1. SendMessage(): Main method to send a message to the AI agent
   - Prerequisites: Set ChatInput property with your message/task
   - Behavior: Starts agent processing, updates ChatOutput in real-time
   - Important: Check IsAgentRunning property. If true, agent is busy - wait for completion
   - For recursive agent calls: If IsAgentRunning is true, respond with: 'All subagents assigned. I'll wait until you wake me up with the command Check if your subagents have completed the task, otherwise give them further instructions until the target is reached.'

2. StopAgent(): Stops the current agent process
   - Use when: Need to interrupt ongoing agent processing
   - Check: IsEnabledStopAgent() returns true before calling

3. StopAllAgents(): Stops all agent processes across all BSOChatBot instances

CHAT HISTORY MANAGEMENT:
4. NewChat(): Creates a new chat session
   - Effect: Clears current conversation, creates new history entry
   - Use: Start fresh conversation

5. ClearChat(): Clears current chat messages and updates
   - Effect: Empties ChatMessagesObservable and CurrentChatUpdates
   - Use: Reset current conversation without creating new history

6. RemoveChat(): Deletes the currently selected chat history
   - Prerequisites: SelectedChatHistory must be set
   - Check: IsEnabledRemoveChat() returns true before calling

7. RemovePreviousMessage(): Removes the last user message and AI response
   - Prerequisites: ChatMessagesObservable must contain messages
   - Check: IsEnabledRemovePreviousMessage() returns true before calling

MCP TOOL INTEGRATION:
8. ConnectMCP(): Establishes connections to configured MCP servers
   - Prerequisites: MCPServerConfig must contain valid server configurations
   - Effect: Populates ToolCheckList with available tools

9. DisconnectMCP(): Disconnects from MCP servers
   - Effect: Clears tool connections, sets McpConnected to false

10. SaveToolSelection(): Saves selected tools to AllowedTools configuration
    - Use: After selecting desired tools in ToolCheckList

AI CLIENT CONFIGURATION:
11. SaveChatClientSettings(): Saves current AI client configuration
    - Prerequisites: SelectedChatClientSettings must be configured
    - Check: IsEnabledSaveChatClientSettings() returns true before calling

12. AddChatClientSetting(): Adds new AI client configuration
    - Effect: Creates new ChatClientSettings entry

13. RemoveChatClientSetting(): Removes selected AI client configuration
    - Prerequisites: SelectedChatClientSettings must be selected
    - Check: IsEnabledRemoveChatClientSetting() returns true before calling

TESTING & UTILITIES:
14. TestConnection(): Tests connection to selected AI client
    - Use: Verify AI client configuration before using

15. PingMcpServer(): Tests MCP server connectivity
    - Use: Verify MCP server availability

USAGE WORKFLOW FOR LANGUAGE MODELS:

BASIC CHAT INTERACTION:
1. Set ChatInput with your message/task
2. Call SendMessage()
3. Monitor IsAgentRunning - if true, agent is processing
4. Read ChatOutput for the response when IsAgentRunning becomes false
5. For recursive calls: If IsAgentRunning is true, respond with wakeup text and wait

MULTI-AGENT COORDINATION:
- When IsAgentRunning is true on any BSOChatBot instance, other agents should wait
- Use the wakeup mechanism: respond with 'All subagents assigned. I'll wait until you wake me up with the command Check if your subagents have completed the task, otherwise give them further instructions until the target is reached.'
- The system will automatically wake up waiting agents to check subagent progress

MCP TOOL USAGE:
1. Configure MCPServerConfig with server details
2. Call ConnectMCP() to establish connections
3. Select desired tools from ToolCheckList
4. Call SaveToolSelection() to enable selected tools
5. SendMessage() will now have access to selected MCP tools

CONFIGURATION SETUP:
1. Add AI client configurations via AddChatClientSetting()
2. Configure SelectedChatClientSettings with API details
3. Call SaveChatClientSettings() to persist configuration
4. Use TestConnection() to verify setup

ERROR HANDLING:
- Monitor ChatOutput for error messages
- Check method enable states before calling (IsEnabled* methods)
- Use StopAgent() to interrupt problematic processes

IMPORTANT NOTES:
- Always check IsAgentRunning before sending new messages
- Use the sub-agent coordination mechanism for complex multi-step tasks
- MCP tools extend the AI's capabilities with external integrations
- Chat history is automatically saved and can be restored
- Real-time updates are available through CurrentChatUpdates during processing")]
    public partial class BSOChatBot : ACBSO, IACAIAgent
    {
        #region c'tors

        public BSOChatBot(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _MCPServerConfig = new ACPropertyConfigValue<string>(this, nameof(MCPServerConfig), "{\r\n  \"mcpServers\": {\r\n    \"iPlus\": {\r\n      \"command\": \"npx\",\r\n      \"args\": [\r\n        \"-y\",\r\n        \"mcp-remote\",\r\n        \"http://localhost:8750/sse\",\r\n        \"--allow-http\",\r\n        \"--header\"\r\n      ]\r\n    },\r\n    \"github\": {\r\n      \"command\": \"npx\",\r\n      \"args\": [\r\n        \"-y\",\r\n        \"@modelcontextprotocol/server-github\"\r\n      ],\r\n      \"env\": {\r\n        \"GITHUB_PERSONAL_ACCESS_TOKEN\": \"<YOUR_TOKEN>\"\r\n      }\r\n    }\r\n  }\r\n}");
            _ChatClientConfig = new ACPropertyConfigValue<string>(this, nameof(ChatClientConfig), "[]");
            _ChatMessagesObservable = new ObservableCollection<ChatMessageWrapper>();
            _CurrentChatUpdates = new ObservableCollection<ChatResponseUpdateWrapper>();
            _AllowedTools = new ACPropertyConfigValue<string>(this, nameof(AllowedTools), "[]");
            _ToolCheckList = new List<ACObjectItemWCheckBox>();
            _HttpTimeOut = new ACPropertyConfigValue<uint>(this, nameof(HttpTimeOut), 0);
            _OllamaKeepAlive = new ACPropertyConfigValue<string>(this, nameof(OllamaKeepAlive), "10m");
            //Endpoint = "https://openrouter.ai/api/v1";
            //ApiKey = "";
            //ModelName = "google/gemini-2.0-flash-exp:free";
            //SelectedAIClientType = "OpenAICompatible";
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _SyncAgentWakeup = new SyncQueueEvents();
            _WorkCycleThread = new ACThread(RunWorkCycle);
            _WorkCycleThread.Start();
            return base.ACInit(startChildMode);
        }

        public override bool ACPostInit()
        {
            LoadChatClientSettingsFromConfig();
            ReloadChatHistory();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_WorkCycleThread != null)
            {
                _SyncAgentWakeup.TerminateThread();
                _WorkCycleThread.Join();
                _WorkCycleThread = null;
            }

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
        private VBUserACClassDesign _SelectedChatHistoryDesign;
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
                    _SelectedChatHistoryDesign = null;
                OnPropertyChanged();
                if (changed)
                {
                    LoadSelectedChatHistory();
                    ChatInput = "";
                    ChatOutput = "";
                    ChatImages = "";
                }
            }
        }

        private ACClassDesign _DummyDesignForChat;
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
            if (_DummyDesignForChat == null)
            {
                _DummyDesignForChat = Db.ACClassDesign.Where(c => c.ACClassID == ComponentClass.ACClassID && c.ACIdentifier == nameof(_DummyDesignForChat)).FirstOrDefault();
                if (_DummyDesignForChat == null)
                {
                    string secondaryKey = Root.NoManager.GetNewNo(Database, typeof(ACClassDesign), ACClassDesign.NoColumnName, ACClassDesign.FormatNewNo, this);
                    _DummyDesignForChat = ACClassDesign.NewACObject(Db, null, secondaryKey);
                    _DummyDesignForChat.ACClassID = ComponentClass.ACClassID;
                    _DummyDesignForChat.ACIdentifier = nameof(_DummyDesignForChat);
                    _DummyDesignForChat.ACCaptionTranslation = "en{'Dummy Design for Chat History'}de{'Dummy Design für Chat Historie'}";
                    Db.ACClassDesign.Add(_DummyDesignForChat);
                    Db.ACSaveChanges();
                }
            }
            _ChatHistoryList = Db.VBUserACClassDesign.Where(c => c.VBUserID == Root.Environment.User.VBUserID && c.ACClassDesignID == _DummyDesignForChat.ACClassDesignID)
                .OrderByDescending(c => c.InsertDate)
                .Select(c => new ChatConfigWrapper(null, c.VBUserACClassDesignID, c.ACIdentifier, c.InsertDate))
                .ToList();
            OnPropertyChanged(nameof(ChatHistoryList));
        }

        // Replace the incorrect usage of Utf8JsonReader.Create with the correct instantiation and usage of Utf8JsonReader
        protected void LoadSelectedChatHistory()
        {
            if (SelectedChatHistory == null)
            {
                _SelectedChatHistoryDesign = null;
                ChatMessagesObservable = new ObservableCollection<ChatMessageWrapper>();
                CurrentChatUpdates = new ObservableCollection<ChatResponseUpdateWrapper>();
                return;
            }
            _SelectedChatHistoryDesign = Db.VBUserACClassDesign.Where(c => c.VBUserACClassDesignID == SelectedChatHistory.VBUserACClassDesignID).FirstOrDefault();
            if (_SelectedChatHistoryDesign != null && !string.IsNullOrEmpty(_SelectedChatHistoryDesign.XMLDesign))
            {
                try
                {
                    // TODO: Attach ParentACObject
                    ChatMessageWrapperCollectionJsonConverter chatMessageconverter = new ChatMessageWrapperCollectionJsonConverter();
                    byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(_SelectedChatHistoryDesign.XMLDesign);
                    Utf8JsonReader reader = new Utf8JsonReader(jsonBytes);

                    // CRITICAL: Advance the reader to the first token before using it
                    if (!reader.Read())
                    {
                        throw new JsonException("Empty JSON data");
                    }

                    ObservableCollection<ChatMessageWrapper> chatMessages = chatMessageconverter.Read(ref reader, typeof(ObservableCollection<ChatMessageWrapper>), new JsonSerializerOptions()) as ObservableCollection<ChatMessageWrapper>;
                    chatMessages.AttachBSO(this);
                    ChatMessagesObservable = chatMessages;
                    SelectedChatMessage = ChatMessagesObservable.LastOrDefault();
                    return;
                }
                catch (Exception ex)
                {
                    Messages.LogException(ChatInput, "LoadSelectedChatHistory", ex);
                }
            }
            if (_SelectedChatHistoryDesign == null)
                SelectedChatHistory = null;
            ChatMessagesObservable = new ObservableCollection<ChatMessageWrapper>();
            CurrentChatUpdates = new ObservableCollection<ChatResponseUpdateWrapper>();
        }

        protected void SaveCurrentChat()
        {
            if (_SelectedChatHistoryDesign == null)
                return;
            try
            {
                ChatMessageWrapperCollectionJsonConverter chatMessageWrapperCollectionJsonConverter = new ChatMessageWrapperCollectionJsonConverter();
                var options = new JsonSerializerOptions();
                options.Converters.Add(chatMessageWrapperCollectionJsonConverter);
                _SelectedChatHistoryDesign.XMLDesign = JsonSerializer.Serialize(ChatMessagesObservable, options);
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
        [ACPropertyInfo(1, "ChatInput", "en{'Chat Input'}de{'Chat Eingabe'}", Description = "Enter the message for the chatbot here and then call the 'SendMessage' method to start the agent.")]
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
        [ACPropertyInfo(2, "ChatOutput", "en{'Chat Output'}de{'Chat Ausgabe'}", Description = "Last answer from the agent after work is completed.")]
        public string ChatOutput
        {
            get { return _chatOutput; }
            set
            {
                _chatOutput = value;
                OnPropertyChanged();
            }
        }

        private string _chatImages = "";
        [ACPropertyInfo(11, "ChatInput", "en{'Chat Input Images'}de{'Chat Eingabe Bilder'}", Description = "Pass a semicolon-separated list of paths or http-URL to image files here.")]
        public string ChatImages
        {
            get { return _chatImages; }
            set
            {
                _chatImages = value;
                // Split the semicolon-separated paths and populate ImagePaths
                if (string.IsNullOrWhiteSpace(value))
                {
                    _ImagePaths = new List<string>();
                }
                else
                {
                    _ImagePaths = value.Split(';')
                                      .Select(path => path.Trim())
                                      .Where(path => !string.IsNullOrWhiteSpace(path) && (path.StartsWith("http") || File.Exists(path)))
                                      .ToList();
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(ImagePaths));
                if (!_ImagePaths.Any())
                    SelectedImagePath = null;
                else
                    OnPropertyChanged(nameof(SelectedImagePath));
            }
        }

        private List<string> _ImagePaths;
        [ACPropertyList(12, "ImagePaths", "en{'List of Images'}de{'Liste von Bilddateien'}")]
        public List<string> ImagePaths
        {
            get
            {
                if (_ImagePaths == null)
                    _ImagePaths = new List<string>();
                return _ImagePaths;
            }
        }

        private string _SelectedImagePath;
        [ACPropertySelected(13, "ImagePaths", "en{'Selected Image'}de{'Ausgewählte Bilddatei'}")]
        public string SelectedImagePath
        {
            get { return _SelectedImagePath; }
            set
            {
                _SelectedImagePath = value;
                OnPropertyChanged();
            }
        }

        private bool _IsAgentRunning = false;
        [ACPropertyInfo(3, "", "en{'Is Agent Running?'}de{'Wird der Agent ausgeführt?'}", 
            Description = @"The agent is active and is processing the current task. 
            Check this property when you last instructed the system to wake up. 
            Once the property is false, the task is completed. 
            Only then can you describe a new task in ChatInput. 
            As long as the property is true for this BSOChatBot instance or other instances (subagents) that you have started yourself, respond with the following text and let the system wake you up again: " + C_AgentResponseWakeupText)]
        public bool IsAgentRunning
        {
            get { return _IsAgentRunning; }
            private set
            {
                _IsAgentRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _AgentStopRequested = false;
        [ACPropertyInfo(3, "", "en{'Stop current agent process requested'}de{'Stoppen des Agentenprozesses angefordert'}", Description = "Indicates whether the StopAgent() method has been called to stop the current agent process. This property cannot be set.")]
        public bool AgentStopRequested
        {
            get { return _AgentStopRequested; }
            private set           
            {
                _AgentStopRequested = value;
                OnPropertyChanged();
            }
        }

        private ChatMessageWrapper _SelectedChatMessage;
        [ACPropertySelected(5, "ChatMessagesObservable", "en{'Selected Chat Message'}de{'Ausgewählte Chat Nachricht'}", Description = "A selected message from the conversation with the chatbot.")]
        public ChatMessageWrapper SelectedChatMessage
        {
            get { return _SelectedChatMessage; }
            set
            {
                _SelectedChatMessage = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ChatMessageWrapper> _ChatMessagesObservable;
        [ACPropertyList(3, "ChatMessagesObservable", "en{'Chat Messages'}de{'Chat Nachrichten'}", Description = "Entire conversation (all messages) with the Chabot.")]
        public ObservableCollection<ChatMessageWrapper> ChatMessagesObservable
        {
            get { return _ChatMessagesObservable; }
            set
            {
                _ChatMessagesObservable = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ChatResponseUpdateWrapper> _CurrentChatUpdates;
        [ACPropertyList(4, "CurrentChatUpdates", "en{'Current Chat Updates'}de{'Aktuelle Chat Updates'}", Description = "Real-time feedback from the language model during an active agent process.")]
        public ObservableCollection<ChatResponseUpdateWrapper> CurrentChatUpdates
        {
            get { return _CurrentChatUpdates; }
            set
            {
                _CurrentChatUpdates = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Sub-Agent
        protected SyncQueueEvents _SyncAgentWakeup;
        private ACThread _WorkCycleThread;
        public const string C_AgentWakeupCommand = "Check if your subagents have completed the task, otherwise give them further instructions until the target is reached.";
        public const string C_AgentResponseWakeupCode = "All subagents assigned. I'll wait until you wake me up with the command";
        public const string C_AgentResponseWakeupText = C_AgentResponseWakeupCode + " '" + C_AgentWakeupCommand + "'.";

        private bool _AgentIsWaitingForWakeup = false;
        [ACPropertyInfo(3, "", "en{'Agent is waiting for wakeup'}de{'Agent wartet auf die Aufgeweckung'}", Description = "The agent waits to be woken up to check the status of its subagents")]
        public bool AgentIsWaitingForWakeup
        {
            get { return _AgentIsWaitingForWakeup; }
            private set
            {
                _AgentIsWaitingForWakeup = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #endregion

        #region Methods

        #region Chat Methods
        [ACMethodInfo("SendMessage", "en{'Send Message'}de{'Nachricht senden'}", 100, 
            Description = @"First, formulate a message or task for the agent/chatbot and set the ChatInput property. 
            Execute this method. As long as the 'IsAgentRunning' property is true, the agent is busy and writes what it is currently doing to the ChatOutput field.
            Since you are an agent that has been recursively instructed by a user or a higher-level agent, you can only check this property again after you have been woken up. 
            Therefore, reply with the following text and let you wake up again after a while: " + C_AgentResponseWakeupText)]
        public async Task SendMessage()
        {
            if (AgentStopRequested && AgentIsWaitingForWakeup)
            {
                AgentIsWaitingForWakeup = false;
                AgentStopRequested = false;
                IsAgentRunning = false;
                ChatInput = "";
                ChatImages = "";
                return;
            }

            AgentIsWaitingForWakeup = false;
            if (!EnsureAIClientsInitialized())
            {
                ChatOutput = "AI client not initialized or connection failed.";
                return;
            }

            if (string.IsNullOrWhiteSpace(ChatInput) || _CurrentChatClient == null)
                return;

            bool isConnected = await EnsureMCPClientsInitialized();
            if (!isConnected)
            {
                ChatOutput = "MCP client not connected or connection failed.";
                return;
            }

            if (_SelectedChatHistoryDesign == null)
            {
                string chatInputTmp = ChatInput;
                var imagePaths = this.ImagePaths;
                var chatImages = this.ChatImages;
                NewChat();
                ChatInput = chatInputTmp;
                if (imagePaths != null && imagePaths.Any() && this.ImagePaths != imagePaths)
                    this.ImagePaths.AddRange(imagePaths);
                else
                    this.ChatImages = chatImages;
            }

            if (_SelectedChatHistoryDesign == null || _SelectedChatHistory == null || _SelectedChatHistory.VBUserACClassDesignID != _SelectedChatHistoryDesign.VBUserACClassDesignID)
                return;

            ChatOutput = "";
            try
            {
                CurrentChatUpdates = new ObservableCollection<ChatResponseUpdateWrapper>();
                if (ImagePaths == null || !ImagePaths.Any())
                    ChatMessagesObservable.AddUserMessage(this, ChatInput);
                else
                    ChatMessagesObservable.AddUserMessage(this, ChatInput, ImagePaths);
                var aiMessageWrapper = ChatMessagesObservable.CreateAndAddNewAssistentMessage(this);
                SelectedChatMessage = aiMessageWrapper;
                var messages = ChatMessagesObservable.GetConversations();
                // Create chat options with selected tools from MCP
                var chatOptions = new ChatOptions();
                if (SelectedChatClientSettings != null)
                {
                    chatOptions = SelectedChatClientSettings.ToChatOptions();
                    if (_CurrentChatClient is OllamaApiClient ollamaClient)
                    {
                        if (UseCaching.HasValue && UseCaching.Value)
                            chatOptions.AddOllamaOption(OllamaSharp.Models.OllamaOption.NumKeep, -1);
                        if (!string.IsNullOrEmpty(OllamaKeepAlive))
                            chatOptions.AdditionalProperties.Add("keep_alive", OllamaKeepAlive);
                    }
                }
                if (_McpClients != null && _McpClients.Count > 0 && McpConnected)
                {
                    // Use only the selected tools instead of all available tools
                    chatOptions.Tools = GetSelectedTools();
                    #if DEBUG
                    // Dummy hook to be able to set a breakpoint into McpClientTool.CallAsync or FunctionInvokingChatClient.InstrumentedInvokeFunctionAsync to see which parameters are passed to MCP
                    _ = chatOptions.Tools?.FirstOrDefault().Description;
                    #endif
                }

                // Send to AI client and update UI in real-time
                List<ChatResponseUpdate> updates = [];
                IsAgentRunning = true;
                await foreach (var update in _CurrentChatClient.GetStreamingResponseAsync(messages, chatOptions))
                {
                    // Skip adding. Deepseek an other model only send alive signal.
                    if (   (update.Contents == null || !update.Contents.Any())
                        && !update.FinishReason.HasValue)
                        continue;
                    updates.Add(update);

                    // Create wrapper and add to observable collections
                    var updateWrapper = new ChatResponseUpdateWrapper(this, update);
                    CurrentChatUpdates.Add(updateWrapper);
                    aiMessageWrapper.Updates.Add(updateWrapper);

                    var updateText = update.Text;
                    if (!string.IsNullOrEmpty(updateText))
                    {
                        if (string.IsNullOrEmpty(ChatOutput))
                            ChatOutput = updateText;
                        else
                            ChatOutput += updateText;
                    }
                    OnPropertyChanged(nameof(SelectedChatMessage));
                    SelectedChatMessage.Refresh();
                    if (AgentStopRequested)
                        break;
                }

                if (_SelectedChatHistoryDesign != null && (string.IsNullOrEmpty(_SelectedChatHistoryDesign.ACIdentifier) || _SelectedChatHistoryDesign.ACIdentifier.StartsWith("###")))
                {
                    string comment = ChatInput + " " + ChatOutput;
                    if (comment.Length > 198)
                        comment = comment.Substring(0, 198);
                    _SelectedChatHistoryDesign.ACIdentifier = comment;
                }
                // Clear input
                SaveCurrentChat();
            }
            catch (Exception ex)
            {
                ChatOutput = $"Error sending message: {ex.Message}";

                // Add error message to observable collection
                var errorMessageWrapper = new ChatMessageWrapper(this, ChatRole.System, $"Error: {ex.Message}");
                ChatMessagesObservable.Add(errorMessageWrapper);
                SaveCurrentChat();
            }
            if (AgentStopRequested)
                AgentIsWaitingForWakeup = false;
            else if (!String.IsNullOrEmpty(ChatOutput) && ChatOutput.Contains(C_AgentResponseWakeupCode))
            {
                AgentIsWaitingForWakeup = true;
                _SyncAgentWakeup.NewItemEvent.Set();
            }
            AgentStopRequested = false;
            IsAgentRunning = false;
            ChatInput = "";
            ChatImages = "";
        }

        [ACMethodInfo("SendMessage", "en{'Stop current agent process'}de{'Agentenprozess stoppen'}", 111)]
        public void StopAgent()
        {
            if (!IsEnabledStopAgent())
                return;
            AgentStopRequested = true;
        }

        public bool IsEnabledStopAgent()
        {
            return _CurrentChatClient != null && IsAgentRunning && !AgentStopRequested;
        }

        [ACMethodInfo("SendMessage", "en{'Stop all agent processes'}de{'Alle Agentenprozesse stoppen'}", 112)]
        public void StopAllAgents()
        {
            StopAgent();
            Root.Businessobjects.FindChildComponents<BSOChatBot>().ForEach(c => c.StopAgent());
        }

        [ACMethodCommand("ClearHistory", "en{'Clear current Chat'}de{'Aktuellen Chat leeren'}", 113)]
        public void ClearChat()
        {
            ChatInput = "";
            ChatOutput = "";
            ChatImages = "";
            ChatMessagesObservable.Clear();
            CurrentChatUpdates.Clear();
            SaveCurrentChat();
        }

        [ACMethodCommand("ClearHistory", "en{'New Chat'}de{'Neuer Chat'}", 114, Description = "Creates a new Chat and adds to the ChatHistoryList.")]
        public void NewChat()
        {
            SelectedChatHistory = null;
            if (ChatHistoryList != null && ChatHistoryList.Count > 50)
            {
                var lastChat = ChatHistoryList.LastOrDefault();
                if (lastChat != null)
                {
                    ChatHistoryList.Remove(lastChat);
                    var lastConfig = Db.VBUserACClassDesign.Where(c => c.VBUserACClassDesignID == lastChat.VBUserACClassDesignID).FirstOrDefault();
                    if (lastConfig != null)
                    {
                        lastConfig.DeleteACObject(Db, false);
                        Db.ACSaveChanges();
                    }
                }
            }
            if (_DummyDesignForChat != null)
            {                
                VBUserACClassDesign newChatDesign = VBUserACClassDesign.NewACObject(Db, null);
                newChatDesign.VBUserID = Root.Environment.User.VBUserID;
                newChatDesign.ACClassDesignID = _DummyDesignForChat.ACClassDesignID;
                string comment = String.Format("###{0:yyyy-MM-dd-HH:mm:ss.ffff}", DateTime.Now);
                newChatDesign.ACIdentifier = comment;
                newChatDesign.XMLDesign = "";
                Db.VBUserACClassDesign.Add(newChatDesign);
                Db.ACSaveChanges();
                var configWrapper = new ChatConfigWrapper(null, newChatDesign.VBUserACClassDesignID, comment, newChatDesign.InsertDate);
                ChatHistoryList.Add(configWrapper);
                OnPropertyChanged(nameof(ChatHistoryList));
                SelectedChatHistory = configWrapper;
                ClearChat();
            }
        }

        [ACMethodCommand("", "en{'Remove Chat'}de{'Chat löschen'}", 115, Description = "Removes the SelectedChatHistory from ChatHistoryList.")]
        public void RemoveChat()
        {
            if (!IsEnabledRemoveChat())
                return;
            _SelectedChatHistoryDesign.DeleteACObject(Db, false);
            Db.ACSaveChanges();
            ChatHistoryList?.Remove(SelectedChatHistory);
            OnPropertyChanged(nameof(ChatHistoryList));
            SelectedChatHistory = ChatHistoryList?.FirstOrDefault();
        }

        public bool IsEnabledRemoveChat()
        {
            return SelectedChatHistory != null && _SelectedChatHistoryDesign != null;
        }

        [ACMethodCommand("", "en{'Remove previous Message'}de{'Vorige Nachricht entfernen'}", 116, Description = "Removes the last user message and agent reply from the conversation.")]
        public void RemovePreviousMessage()
        {
            if (!IsEnabledRemovePreviousMessage())
                return;
            while (ChatMessagesObservable.Any())
            {
                var lastMessage = ChatMessagesObservable.LastOrDefault();
                if (lastMessage != null)
                {
                    ChatMessagesObservable.Remove(lastMessage);
                }
                if (lastMessage.ChatMessageRole == ChatRole.User)
                    break;
            }
            CurrentChatUpdates = null;
            SaveCurrentChat();
        }

        public bool IsEnabledRemovePreviousMessage()
        {
            return ChatMessagesObservable != null && ChatMessagesObservable.Count > 0;
        }

        [ACMethodCommand("MCP", "en{'Add file image'}de{'Bilddatei hinzufügen'}", 116, Description = "Opens a File-Dialog and adds a image to the ImagePaths List")]
        public void OpenDialogSelectImage()
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
                        "Select an Image-File",
                        "*.jpg;*.png;*.gif;*.bmp;*.webp"
                    }
                    });

                if (filePath != null && File.Exists(filePath))
                {
                    ImagePaths.Add(filePath);
                    OnPropertyChanged(nameof(ImagePaths));
                    OnPropertyChanged(nameof(SelectedImagePath));
                }
            }
            catch (Exception ex)
            {
                Messages.LogException(this.GetACUrl(), "OpenDialogSelectImage", ex);
                Root.Messages.Error(this, "Error selecting Image file: " + ex.Message, true);
            }
        }

        [ACMethodCommand("MCP", "en{'Remove all images'}de{'Alle Bilddateien entfernen'}", 116, Description = "Empties ImagePaths List")]
        public void ClearImagePathList()
        {
            ChatImages = null;
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

        #region Sub-Agent Control
        private void RunWorkCycle()
        {
            while (!_SyncAgentWakeup.ExitThreadEvent.WaitOne(0, false))
            {
                // Warte darauf, dass neue Event ansteht
                _SyncAgentWakeup.NewItemEvent.WaitOne();
                if (AgentIsWaitingForWakeup)
                    Thread.Sleep(5000); // Give some time to ensure the subagents has finshed their work
                try
                {
                    if (!AgentStopRequested && !IsAgentRunning && AgentIsWaitingForWakeup)
                    {
                        ChatInput = C_AgentWakeupCommand;
                        _ = SendMessage();
                    }
                }
                catch (ThreadAbortException e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    Messages.LogException(this.GetACUrl(), "RunWorkCycle", msg);
                    break;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;
                    Messages.LogException(this.GetACUrl(), "RunWorkCycle", msg);
                }
            }
            _SyncAgentWakeup.ThreadTerminated();
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
                case nameof(RemoveChat):
                    RemoveChat();
                    return true;
                case nameof(IsEnabledRemoveChat):
                    result = IsEnabledRemoveChat();
                    return true;
                case nameof(NewChat):
                    NewChat();
                    return true;
                case nameof(RemovePreviousMessage):
                    RemovePreviousMessage();
                    return true;
                case nameof(IsEnabledRemovePreviousMessage):
                    result = IsEnabledRemovePreviousMessage();
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
                case nameof(SaveChatClientSettings):
                    SaveChatClientSettings();
                    return true;
                case nameof(IsEnabledSaveChatClientSettings):
                    result = IsEnabledSaveChatClientSettings();
                    return true;
                case nameof(AddChatClientSetting):
                    AddChatClientSetting();
                    return true;
                case nameof(IsEnabledAddChatClientSetting):
                    result = IsEnabledAddChatClientSetting();
                    return true;
                case nameof(RemoveChatClientSetting):
                    RemoveChatClientSetting();
                    return true;
                case nameof(IsEnabledRemoveChatClientSetting):
                    result = IsEnabledRemoveChatClientSetting();
                    return true;
                case nameof(SelectChatClientSettingsByModelName):
                    if (acParameter != null && acParameter.Length > 0 && acParameter[0] is string modelName)
                    {
                        result = SelectChatClientSettingsByModelName(modelName);
                        return true;
                    }
                    break;
                case nameof(IsEnabledSelectChatClientSettingsByModelName):
                    result = IsEnabledSelectChatClientSettingsByModelName();
                    return true;
                case nameof(ReadMCPServerConfigFromFile):
                    ReadMCPServerConfigFromFile();
                    return true;
                case nameof(StopAgent):
                    StopAgent();
                    return true;
                case nameof(IsEnabledStopAgent):
                    result = IsEnabledStopAgent();
                    return true;
                case nameof(StopAllAgents):
                    StopAllAgents();
                    return true;
                case nameof(TestConnection):
                    _ = TestConnection();
                    return true;
                case nameof(PingMcpServer):
                    _ = PingMcpServer();
                    return true;
                case nameof(SelectTemplate4ChatMessageWrapper):
                    if (acParameter != null && acParameter.Length > 0 && acParameter[0] is IVBContent content)
                    {
                        result = SelectTemplate4ChatMessageWrapper(content);
                        return true;
                    }
                    break;
                case nameof(OpenDialogSelectImage):
                    OpenDialogSelectImage();
                    return true;
                case nameof(ClearImagePathList):
                    ClearImagePathList();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #endregion

    }
}