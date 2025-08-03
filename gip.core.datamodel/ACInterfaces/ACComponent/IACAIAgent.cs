// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACAIAgent.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for AI Agents
    /// The IACAIAgent interface defines the contract for AI chatbot components within the iPlus framework.
    /// It provides a standardized way to interact with AI agents, manage chat sessions, configure AI clients,
    /// and integrate with Model Context Protocol (MCP) tools.
    /// 
    /// This interface enables other components to operate AI agents without needing to know the specific
    /// implementation details of the underlying BSOChatBot class.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'AI-Agent'}de{'AI-Agent'}", Global.ACKinds.TACInterface)]
    public interface IACAIAgent : IACComponent, ICloneable
    {
        #region Chat Input/Output Properties
        
        /// <summary>
        /// Gets or sets the input message for the AI agent.
        /// Set this property with your message/task before calling SendMessage().
        /// </summary>
        string ChatInput { get; set; }
        
        /// <summary>
        /// Gets the output response from the AI agent.
        /// Contains the latest response from the AI agent after work is completed.
        /// </summary>
        string ChatOutput { get; }
        
        /// <summary>
        /// Gets or sets semicolon-separated list of paths or HTTP URLs to image files.
        /// These images will be included with the next message sent to the AI agent.
        /// </summary>
        string ChatImages { get; set; }
        
        /// <summary>
        /// Gets the list of image paths extracted from ChatImages.
        /// </summary>
        List<string> ImagePaths { get; }
        
        /// <summary>
        /// Gets or sets the currently selected image path.
        /// </summary>
        string SelectedImagePath { get; set; }
        
        #endregion

        #region Agent Status Properties
        
        /// <summary>
        /// Gets a value indicating whether the agent is currently running and processing a task.
        /// When true, the agent is busy and you must wait for completion before sending new messages.
        /// For recursive agent calls: If IsAgentRunning is true, respond with wakeup text and wait.
        /// </summary>
        bool IsAgentRunning { get; }
        
        /// <summary>
        /// Gets a value indicating whether StopAgent() has been called to stop the current agent process.
        /// </summary>
        bool AgentStopRequested { get; }
        
        /// <summary>
        /// Gets a value indicating whether the agent is waiting for wakeup to check subagent status.
        /// True when agent waits for wakeup to check subagent status.
        /// </summary>
        bool AgentIsWaitingForWakeup { get; }
        
        #endregion

        #region Core Chat Operations
        
        /// <summary>
        /// Main method to send a message to the AI agent.
        /// Prerequisites: Set ChatInput property with your message/task.
        /// Behavior: Starts agent processing, updates ChatOutput in real-time.
        /// Important: Check IsAgentRunning property. If true, agent is busy - wait for completion.
        /// For recursive agent calls: If IsAgentRunning is true, respond with wakeup text and wait.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendMessage();
        
        /// <summary>
        /// Stops the current agent process.
        /// Use when you need to interrupt ongoing agent processing.
        /// Check IsEnabledStopAgent() returns true before calling.
        /// </summary>
        void StopAgent();
        
        /// <summary>
        /// Determines whether StopAgent() can be called.
        /// </summary>
        /// <returns>True if the agent can be stopped; otherwise, false.</returns>
        bool IsEnabledStopAgent();
        
        /// <summary>
        /// Stops all agent processes across all AI agent instances.
        /// </summary>
        void StopAllAgents();
        
        #endregion

        #region Chat History Management
        
        /// <summary>
        /// Creates a new chat session.
        /// Effect: Clears current conversation, creates new history entry.
        /// Use: Start fresh conversation.
        /// </summary>
        void NewChat();
        
        /// <summary>
        /// Clears current chat messages and updates.
        /// Effect: Empties ChatMessagesObservable and CurrentChatUpdates.
        /// Use: Reset current conversation without creating new history.
        /// </summary>
        void ClearChat();
        
        /// <summary>
        /// Deletes the currently selected chat history.
        /// Prerequisites: SelectedChatHistory must be set.
        /// Check IsEnabledRemoveChat() returns true before calling.
        /// </summary>
        void RemoveChat();
        
        /// <summary>
        /// Determines whether RemoveChat() can be called.
        /// </summary>
        /// <returns>True if a chat can be removed; otherwise, false.</returns>
        bool IsEnabledRemoveChat();
        
        /// <summary>
        /// Removes the last user message and AI response.
        /// Prerequisites: ChatMessagesObservable must contain messages.
        /// Check IsEnabledRemovePreviousMessage() returns true before calling.
        /// </summary>
        void RemovePreviousMessage();
        
        /// <summary>
        /// Determines whether RemovePreviousMessage() can be called.
        /// </summary>
        /// <returns>True if a previous message can be removed; otherwise, false.</returns>
        bool IsEnabledRemovePreviousMessage();
        
        #endregion

        #region MCP Tool Integration
        
        /// <summary>
        /// Gets a value indicating whether MCP (Model Context Protocol) servers are connected.
        /// </summary>
        bool McpConnected { get; }
               
        /// <summary>
        /// Establishes connections to configured MCP servers.
        /// Prerequisites: MCPServerConfig must contain valid server configurations.
        /// Effect: Populates ToolCheckList with available tools.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ConnectMCP();
        
        /// <summary>
        /// Disconnects from MCP servers.
        /// Effect: Clears tool connections, sets McpConnected to false.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DisconnectMCP();
               
        #endregion

        #region Testing & Utilities
        
        /// <summary>
        /// Tests connection to selected AI client.
        /// Use: Verify AI client configuration before using.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task TestConnection();
        
        /// <summary>
        /// Tests MCP server connectivity.
        /// Use: Verify MCP server availability.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task PingMcpServer();
        
       
        /// <summary>
        /// Empties the ImagePaths list.
        /// </summary>
        void ClearImagePathList();

        #endregion

        #region Chat Client Settings

        /// <summary>
        /// Gets the name of the selected model from the chat client settings.
        /// </summary>
        string SelectedModelName { get; }

        /// <summary>
        /// Selects a chat client setting by model name from the available configurations.
        /// Searches through ChatClientSettingsList for a matching model name and sets it as SelectedChatClientSettings.
        /// </summary>
        /// <param name="modelName">The name of the model to search for (case-insensitive).</param>
        /// <returns>True if the selection was successful and the model was found; otherwise, false.</returns>
        (bool success, string message) SelectChatClientSettingsByModelName(string modelName);
        
        /// <summary>
        /// Determines whether SelectChatClientSettingsByModelName() can be called.
        /// </summary>
        /// <returns>True if there are available chat client settings to search; otherwise, false.</returns>
        bool IsEnabledSelectChatClientSettingsByModelName();
        
        #endregion
    }
}
