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
    public partial class BSOChatBot : ACBSO
    {
        #region c'tors

        public BSOChatBot(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _MCPServerConfig = new ACPropertyConfigValue<string>(this, nameof(MCPServerConfig), "{\r\n  \"mcpServers\": {\r\n    \"iPlus\": {\r\n      \"command\": \"npx\",\r\n      \"args\": [\r\n        \"mcp-remote\",\r\n        \"http://localhost:8750/sse\",\r\n        \"--allow-http\"\r\n      ]\r\n    },\r\n    \"github\": {\r\n      \"command\": \"npx\",\r\n      \"args\": [\r\n        \"-y\",\r\n        \"@modelcontextprotocol/server-github\"\r\n      ],\r\n      \"env\": {\r\n        \"GITHUB_PERSONAL_ACCESS_TOKEN\": \"<YOUR_TOKEN>\"\r\n      }\r\n    }\r\n  }\r\n}");
            _ChatClientConfig = new ACPropertyConfigValue<string>(this, nameof(ChatClientConfig), "[]");
            _ChatMessagesObservable = new ObservableCollection<ChatMessageWrapper>();
            _CurrentChatUpdates = new ObservableCollection<ChatResponseUpdateWrapper>();
            _AllowedTools = new ACPropertyConfigValue<string>(this, nameof(AllowedTools), "[]");
            _ToolCheckList = new List<ACObjectItemWCheckBox>();
            //Endpoint = "https://openrouter.ai/api/v1";
            //ApiKey = "";
            //ModelName = "google/gemini-2.0-flash-exp:free";
            //SelectedAIClientType = "OpenAICompatible";
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
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
                    ChatInput = "";
                    ChatOutput = "";
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
                    SelectedChatMessage = ChatMessagesObservable.LastOrDefault();
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

        private ChatMessageWrapper _SelectedChatMessage;
        [ACPropertySelected(5, "ChatMessagesObservable", "en{'Selected Chat Message'}de{'Ausgewählte Chat Nachricht'}")]
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
        [ACPropertyList(3, "ChatMessagesObservable", "en{'Chat Messages'}de{'Chat Nachrichten'}")]
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
        [ACPropertyList(4, "CurrentChatUpdates", "en{'Current Chat Updates'}de{'Aktuelle Chat Updates'}")]
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

        #endregion

        #region Methods

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

            ChatOutput = "";
            try
            {
                CurrentChatUpdates = new ObservableCollection<ChatResponseUpdateWrapper>();
                ChatMessagesObservable.AddUserMessage(this, ChatInput);
                var aiMessageWrapper = ChatMessagesObservable.CreateAndAddNewAssistentMessage(this);
                SelectedChatMessage = aiMessageWrapper;
                var messages = ChatMessagesObservable.GetConversations();
                // Create chat options with selected tools from MCP
                var chatOptions = new ChatOptions();
                if (SelectedChatClientSettings != null)
                    chatOptions = SelectedChatClientSettings.ToChatOptions();
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
                }

                if (_SelectedChatHistoryConfig != null && (string.IsNullOrEmpty(_SelectedChatHistoryConfig.Comment) || _SelectedChatHistoryConfig.Comment.StartsWith("###")))
                    _SelectedChatHistoryConfig.Comment = ChatInput + " " + ChatOutput;
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

        [ACMethodCommand("ClearHistory", "en{'Clear current Chat'}de{'Aktuellen Chat leeren'}", 101)]
        public void ClearChat()
        {
            ChatInput = "";
            ChatOutput = "";
            ChatMessagesObservable.Clear();
            CurrentChatUpdates.Clear();
            SaveCurrentChat();
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

        [ACMethodCommand("", "en{'Remove Chat'}de{'Chat löschen'}", 103)]
        public void RemoveChat()
        {
            if (!IsEnabledRemoveChat())
                return;
            _SelectedChatHistoryConfig.DeleteACObject(Db, false);
            Db.ACSaveChanges();
            ChatHistoryList?.Remove(SelectedChatHistory);
            OnPropertyChanged(nameof(ChatHistoryList));
            SelectedChatHistory = ChatHistoryList?.FirstOrDefault();
        }

        public bool IsEnabledRemoveChat()
        {
            return SelectedChatHistory != null && _SelectedChatHistoryConfig != null;
        }

        [ACMethodCommand("", "en{'Remove previous Message'}de{'Vorige Nachricht entfernen'}", 104)]
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
                case nameof(ReadMCPServerConfigFromFile):
                    ReadMCPServerConfigFromFile();
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
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #endregion

    }
}