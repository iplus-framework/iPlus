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
    public partial class BSOChatBot : ACBSO
    {
        #region Properties

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

        private Dictionary<string, IMcpClient> _McpClients;

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
                    _McpClients = new Dictionary<string, IMcpClient>();

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
                var samplingHandler = _CurrentChatClient?.CreateSamplingHandler();
                var clientOptions = new McpClientOptions
                {
                    Capabilities = new ClientCapabilities
                    {
                        Sampling = samplingHandler != null ? new SamplingCapability { SamplingHandler = samplingHandler } : null
                    }
                };

                // Connect to each MCP server with proper error handling
                var connectionTasks = new List<Task>();
                var connectionResults = new ConcurrentBag<(string serverName, IMcpClient client, List<AITool> tools, Exception error)>();

                foreach (var serverEntry in config.mcpServers)
                {
                    string serverName = serverEntry.Key;
                    McpServerInfo serverInfo = serverEntry.Value;

                    var connectionTask = ConnectToSingleServer(serverName, serverInfo, clientOptions, connectionResults);
                    connectionTasks.Add(connectionTask);
                }

                // Wait for all connections to complete
                await Task.WhenAll(connectionTasks).ConfigureAwait(false);

                // Process results
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
                        ChatOutput += $"Failed to connect to MCP server '{result.serverName}': {result.error?.Message}\n";
                    }
                }

                // Update connection status
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

        private async Task ConnectToSingleServer(
            string serverName,
            McpServerInfo serverInfo,
            McpClientOptions clientOptions,
            ConcurrentBag<(string serverName, IMcpClient client, List<AITool> tools, Exception error)> results)
        {
            try
            {
                // Create stdio transport for MCP server
                var transport = new StdioClientTransport(new StdioClientTransportOptions
                {
                    Command = serverInfo.command,
                    Arguments = serverInfo.args ?? new string[0],
                    Name = serverName,
                    EnvironmentVariables = serverInfo.env,
                }, _LoggerFactory);

                // Connect to MCP server with timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30-second timeout
                var mcpClient = await McpClientFactory.CreateAsync(
                    transport,
                    clientOptions,
                    _LoggerFactory,
                    cts.Token).ConfigureAwait(false);

                // Get available tools from this server
                var tools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
                results.Add((serverName, mcpClient, tools.Cast<AITool>().ToList(), null));
            }
            catch (Exception ex)
            {
                results.Add((serverName, null, new List<AITool>(), ex));
            }
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