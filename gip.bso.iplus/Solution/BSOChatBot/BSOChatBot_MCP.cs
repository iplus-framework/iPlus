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
using System.Collections.Concurrent;

namespace gip.bso.iplus
{
    /// <summary>
    /// Partial class for BSOChatBot handling MCP integration.
    /// Node.js is only required for legacy stdio bridges (for example npx mcp-remote).
    /// Direct HTTP MCP connections do not require Node.js.
    /// </summary>
    public partial class BSOChatBot : ACBSO
    {
        #region Properties
        private bool _UseBotLocally = false;
        [ACPropertyInfo(40, "", "en{'Use Bot locally'}de{'Bot lokal verwenden'}", Description = "This setting is only for humans. Don't use this property")]
        public bool UseBotLocally
        {
            get { return _UseBotLocally; }
            set
            {
                if (_UseBotLocally != value)
                {
                    OnPropertyChanged();
                    DisconnectMCP().Wait();
                }
                _UseBotLocally = value;
            }
        }

        public virtual bool IsLocalBot
        {
            get
            {
                // If UseBotLocally is true, we are using the bot locally
                if (UseBotLocally)
                    return true;
                if (!(ParentACComponent is Businessobjects || ParentACComponent is PWBase))
                    return true;
                return false;
            }
        }

        protected ACPropertyConfigValue<string> _MCPServerConfig;
        [ACPropertyConfig("en{'MCP Server JSON config'}de{'MCP Server JSON config'}")]
        public string MCPServerConfig
        {
            get { return _MCPServerConfig.ValueT; }
            set
            {
                _MCPServerConfig.ValueT = value;
                OnPropertyChanged();
            }
        }

        [ACPropertyInfo(12, "", "en{'MCP Server JSON config'}de{'MCP Server JSON config'}")]
        public string MCPServerConfigProp
        {
            get { return MCPServerConfig; }
            set
            {
                MCPServerConfig = value;
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
        [ACPropertyConfig("en{'JSON Array with allowed toolnames}de{'JSON Array with allowed toolnames'}")]
        public string AllowedTools
        {
            get { return _AllowedTools.ValueT; }
            set
            {
                _AllowedTools.ValueT = value;
                OnPropertyChanged();
            }
        }

        private Dictionary<string, McpClient> _McpClients;

        #endregion

        #region Init/DeInit

        #endregion

        #region MCP Methods

        #region Connection

        public async Task<bool> EnsureMCPClientsInitialized()
        {
            if (_McpClients == null || _McpClients.Count == 0 && (!string.IsNullOrEmpty(MCPServerConfig) || !string.IsNullOrEmpty(MCPServerConfigFromFile)))
            {
                await ConnectMCP();
            }
            return McpConnected;
        }

        [ACMethodCommand("MCP", "en{'Read MCP-Config from file'}de{'MCP-Konfiguration von Datei lesen'}", 106)]
        public async Task ReadMCPServerConfigFromFile()
        {
            try
            {
                ACMediaController mediaController = ACMediaController.GetServiceInstance(this);
                string filePath = await mediaController.OpenFileDialog(
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
                await Root.Messages.ErrorAsync(this, "Error selecting log file: " + ex.Message, true);
            }
        }

        [ACMethodInfo("MCP", "en{'Ping MCP Server'}de{'MCP Server anpingen'}", 105)]
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

        [ACMethodInfo("MCP", "en{'Connect MCP'}de{'MCP verbinden'}", 102,
            Description = @"A connection is established to the configured MCP servers. 
    This method is asynchronous and must wait until the connections are established, which is signaled by the McpConnected property. 
    This method mustn't be called explicitly. It is called implicitly with the first call of SendMessage.")]
        public async Task ConnectMCP()
        {
            try
            {
                // Initialize MCP clients dictionary if needed
                if (_McpClients == null)
                    _McpClients = new Dictionary<string, McpClient>();

                // Parse the JSON configuration
                McpServerConfig config;
                try
                {
                    string configJson = !string.IsNullOrEmpty(MCPServerConfigFromFile) ? MCPServerConfigFromFile : MCPServerConfig;
                    config = JsonSerializer.Deserialize<McpServerConfig>(configJson);
                    if (config?.mcpServers == null || config.mcpServers.Count == 0)
                    {
                        ChatOutput = "No MCP servers configured in JSON config";
                        return;
                    }
                    if (IsLocalBot)
                    {
                        IACComponent localMCPServer = this.Root.ACUrlCommand("\\LocalServiceObjects\\LocalMCPServer") as IACComponent;
                        if (localMCPServer == null)
                        {
                            ChatOutput = "Local MCP-Server not found on address \\LocalServiceObjects\\LocalMCPServer";
                            return;
                        }
                    }

                    foreach (var serverEntry in config.mcpServers)
                    {
                        if (serverEntry.Key.Contains("iPlus", StringComparison.OrdinalIgnoreCase))
                        {
                            //config.mcpServers.TryGetValue("iPlus", out McpServerInfo iPlusServer);
                            McpServerInfo iPlusServer = serverEntry.Value;
                            if (iPlusServer != null)
                            {
                                var vbUser = this.Root.CurrentInvokingUser;
                                string authorizationHeader = vbUser != null ? String.Format("{0}#{1}", vbUser.VBUserName, vbUser.Password) : null;

                                if (!string.IsNullOrWhiteSpace(ResolveHttpEndpoint(iPlusServer)))
                                {
                                    if (iPlusServer.headers == null)
                                        iPlusServer.headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                                    if (!string.IsNullOrEmpty(authorizationHeader))
                                        iPlusServer.headers["Authorization"] = authorizationHeader;
                                    else if (iPlusServer.headers.ContainsKey("Authorization"))
                                        iPlusServer.headers.Remove("Authorization");
                                }
                                else
                                {
                                    List<string> args = iPlusServer.args != null ? iPlusServer.args.ToList() : new List<string>();
                                    if (!args.Contains("--header"))
                                        args.Add("--header");
                                    if (!args.Contains("-y"))
                                        args.Add("-y");
                                    string authorization = args.Where(c => c.StartsWith("Authorization", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                                    if (!String.IsNullOrEmpty(authorization))
                                        args.Remove(authorization);
                                    if (!string.IsNullOrEmpty(authorizationHeader))
                                        args.Add(String.Format("Authorization:{0}", authorizationHeader));
                                    iPlusServer.args = args.ToArray();
                                }
                            }
                        }
                    }

                }
                catch (JsonException ex)
                {
                    ChatOutput = $"Error parsing MCP server configuration JSON: {ex.Message}";
                    return;
                }

                // Clear existing connections properly
                await DisconnectMCP().ConfigureAwait(false);

                // Ensure AI clients are initialized
                if (_CurrentChatClient == null)
                    EnsureAIClientsInitialized();

                int totalTools = 0;
                var connectedServers = new List<string>();

                // Create MCP client with sampling capability
                var clientOptions = new McpClientOptions
                {
                    Capabilities = new ClientCapabilities
                    {
                        Sampling = _CurrentChatClient != null ? new SamplingCapability() : null
                    }
                };

                // Connect to each MCP server with proper error handling
                var connectionTasks = new List<Task>();
                var connectionResults = new ConcurrentBag<(string serverName, McpClient client, List<AITool> tools, Exception error)>();

                foreach (var serverEntry in config.mcpServers)
                {
                    if (   serverEntry.Value.ForLocalBotUsage.HasValue 
                        && (   (serverEntry.Value.ForLocalBotUsage.Value && !IsLocalBot) 
                            || (!serverEntry.Value.ForLocalBotUsage.Value && IsLocalBot)))
                    {
                        continue;
                    }

                    string serverName = serverEntry.Key;
                    McpServerInfo serverInfo = serverEntry.Value;

                    var connectionTask = ConnectToSingleServer(serverName, serverInfo, clientOptions, connectionResults);
                    connectionTasks.Add(connectionTask);
                }

                // Wait for all connections to complete
                await Task.WhenAll(connectionTasks).ConfigureAwait(false);

                // Process results (pure data operations, no UI updates)
                var connectionErrors = new List<string>();
                foreach (var result in connectionResults)
                {
                    if (result.error == null && result.client != null)
                    {
                        _McpClients[result.serverName] = result.client;
                        AvailableTools.AddRange(result.tools);
                        totalTools += result.tools.Count;
                        connectedServers.Add($"{result.serverName} ({result.tools.Count} tools)");
                    }
                    else
                    {
                        connectionErrors.Add($"Failed to connect to MCP server '{result.serverName}': {result.error?.Message}");
                    }
                }

                // Dispatch all UI updates to the UI thread synchronously so McpConnected is set
                // before ConnectMCP() returns and EnsureMCPClientsInitialized() reads it.
                void applyResults()
                {
                    foreach (var error in connectionErrors)
                        ChatOutput += error + "\n";

                    if (_McpClients.Count > 0)
                    {
                        McpConnected = true;
                        ChatOutput = $"Connected to {_McpClients.Count} MCP server(s): {string.Join(", ", connectedServers)}. Total {totalTools} tools available.";
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

                if (_MainSyncContext != null)
                    _MainSyncContext.Send(_ => applyResults(), null);
                else
                    applyResults();
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                void applyError()
                {
                    McpConnected = false;
                    ChatOutput = $"Error connecting MCP clients: {errorMsg}";
                }

                if (_MainSyncContext != null)
                    _MainSyncContext.Send(_ => applyError(), null);
                else
                    applyError();
            }
        }

        private async Task ConnectToSingleServer(
            string serverName,
            McpServerInfo serverInfo,
            McpClientOptions clientOptions,
            ConcurrentBag<(string serverName, McpClient client, List<AITool> tools, Exception error)> results)
        {
            async Task<(McpClient client, List<AITool> tools)> ConnectWithTransport(IClientTransport transport)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var mcpClient = await McpClient.CreateAsync(
                    transport,
                    clientOptions,
                    _LoggerFactory,
                    cts.Token).ConfigureAwait(false);

                var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
                return (mcpClient, tools.Cast<AITool>().ToList());
            }

            IClientTransport BuildStdioTransport(string[] arguments)
            {
                return new StdioClientTransport(new StdioClientTransportOptions
                {
                    Command = serverInfo.command,
                    Arguments = arguments,
                    Name = serverName,
                    EnvironmentVariables = serverInfo.env,
                },
                _LoggerFactory);
            }

            var endpoint = ResolveHttpEndpoint(serverInfo);
            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                try
                {
                    var httpOptions = new HttpClientTransportOptions
                    {
                        Endpoint = new Uri(endpoint, UriKind.Absolute),
                        Name = serverName,
                        TransportMode = HttpTransportMode.AutoDetect,
                        ConnectionTimeout = TimeSpan.FromSeconds(30),
                    };

                    var additionalHeaders = BuildAdditionalHeaders(serverInfo);
                    if (additionalHeaders.Count > 0)
                        httpOptions.AdditionalHeaders = additionalHeaders;

                    var transport = new HttpClientTransport(httpOptions, _LoggerFactory);
                    var connection = await ConnectWithTransport(transport).ConfigureAwait(false);
                    results.Add((serverName, connection.client, connection.tools, null));
                    return;
                }
                catch (Exception httpEx)
                {
                    results.Add((serverName, null, new List<AITool>(), httpEx));
                    return;
                }
            }

            var originalArgs = serverInfo.args ?? new string[0];
            try
            {
                var transport = BuildStdioTransport(originalArgs);
                var connection = await ConnectWithTransport(transport).ConfigureAwait(false);
                results.Add((serverName, connection.client, connection.tools, null));
            }
            catch (Exception ex)
            {
                results.Add((serverName, null, new List<AITool>(), ex));
            }
        }

        private static string ResolveHttpEndpoint(McpServerInfo serverInfo)
        {
            if (serverInfo == null)
                return null;

            if (TryNormalizeHttpEndpoint(serverInfo.url, out var normalizedUrl))
                return normalizedUrl;

            var args = serverInfo.args;
            if (args == null || args.Length == 0)
                return null;

            for (int i = 0; i < args.Length; i++)
            {
                if (TryNormalizeHttpEndpoint(args[i], out normalizedUrl))
                    return normalizedUrl;
            }

            return null;
        }

        private static bool TryNormalizeHttpEndpoint(string value, out string endpoint)
        {
            endpoint = null;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
                return false;

            bool isHttp = uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
                || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
            if (!isHttp)
                return false;

            var endpointBuilder = new UriBuilder(uri);
            if (endpointBuilder.Path.EndsWith("/sse", StringComparison.OrdinalIgnoreCase))
            {
                endpointBuilder.Path = endpointBuilder.Path.Substring(0, endpointBuilder.Path.Length - 4);
                if (string.IsNullOrEmpty(endpointBuilder.Path))
                    endpointBuilder.Path = "/";
            }

            endpoint = endpointBuilder.Uri.AbsoluteUri;
            return true;
        }

        private static Dictionary<string, string> BuildAdditionalHeaders(McpServerInfo serverInfo)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (serverInfo?.headers != null)
            {
                foreach (var pair in serverInfo.headers)
                {
                    if (!string.IsNullOrWhiteSpace(pair.Key) && pair.Value != null)
                        headers[pair.Key] = pair.Value;
                }
            }

            var args = serverInfo?.args;
            if (args == null)
                return headers;

            for (int i = 0; i < args.Length; i++)
            {
                if (TryParseHeaderArgument(args[i], out var name, out var value))
                    headers[name] = value;
            }

            return headers;
        }

        private static bool TryParseHeaderArgument(string argument, out string name, out string value)
        {
            name = null;
            value = null;

            if (string.IsNullOrWhiteSpace(argument) || argument.StartsWith("--", StringComparison.Ordinal))
                return false;

            if (Uri.TryCreate(argument, UriKind.Absolute, out _))
                return false;

            int separatorIndex = argument.IndexOf(':');
            if (separatorIndex <= 0 || separatorIndex >= argument.Length - 1)
                return false;

            var headerName = argument.Substring(0, separatorIndex).Trim();
            var headerValue = argument.Substring(separatorIndex + 1).Trim();

            if (string.IsNullOrWhiteSpace(headerName) || string.IsNullOrWhiteSpace(headerValue) || headerName.Contains(" "))
                return false;

            name = headerName;
            value = headerValue;
            return true;
        }

        [ACMethodInfo("MCP", "en{'Disconnect MCP'}de{'MCP trennen'}", 103)]
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
                _McpClients = new Dictionary<string, McpClient>();
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
        [ACMethodCommand("MCP", "en{'Save Tool Selection'}de{'Tool-Auswahl speichern'}", 107)]
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

        #endregion

    }
}