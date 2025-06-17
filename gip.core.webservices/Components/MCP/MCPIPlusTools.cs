// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using gip.core.autocomponent;
using gip.core.datamodel;
using Microsoft.AspNetCore.Components;
using Microsoft.Database.Isam;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;
using static System.Net.Mime.MediaTypeNames;

namespace gip.core.webservices
{
    [McpServerToolType]
    public sealed class MCPIPlusTools
    {
        #region Properties
        private static MCPToolAppTree _appTree = new MCPToolAppTree();
        #endregion

        #region App-Services

        #region MCP-Tools
        [McpServerTool]
        [Description("(1) Application Tree Discovery: Returns all available component types (classes) in the current application tree. " +
            "Each result contains an ACIdentifier (unique type name) and Description. " +
            "Use this tool FIRST to discover what component types exist in the system before querying specific instances or type information.")]
        public static string AppGetThesaurus(
            IACComponent mcpHost,
            [Description("Language code for localized descriptions (e.g., 'en', 'de')")]
            string i18nLangTag)
        {
            return _appTree.AppGetThesaurus(mcpHost, i18nLangTag);
        }


        [McpServerTool]
        [Description("(2) Component Type Information: Returns detailed information about specific component types, including their unique ClassID (CId field) which is required for other tool operations. " +
            "Pass ACIdentifiers obtained from AppGetThesaurus to get the ClassIDs needed for instance queries and property/method discovery.")]
        public static string AppGetTypeInfos(
            IACComponent mcpHost,
            [Description("Comma-separated list of ACIdentifiers (type names) from the thesaurus")]
            string acIdentifiers,
            [Description("Language code for localized descriptions (e.g., 'en', 'de')")]
            string i18nLangTag,
            [Description("Optional: Also return all derived types from the thesaurus (Inheritance).")]
            bool getDerivedTypes = false)
        {
            return _appTree.AppGetTypeInfos(mcpHost, acIdentifiers, i18nLangTag, getDerivedTypes);
        }

        // (6) Component Interaction: Execute operations on specific component instances using ACUrl addressing (like file system paths). SUPPORTS BULK OPERATIONS - pass multiple comma-separated ACUrls for efficient batch execution. The ACUrl uses ACIdentifiers separated by backslashes to address the complete path from root to target component. For state changes and operations, use method invocation (!MethodName) rather than property assignment. Check available methods first with AppGetMethodInfo.
        [McpServerTool]
        [Description("(3) Application Tree Navigation: Explores the hierarchical tree structure of component instances, similar to a file system directory listing. " +
            "Returns the tree with ACIdentifiers that form the path for ACUrl addressing. " +
            "ALWAYS think hierarchically and PASS MULTIPLE ClassIDs separated by commas in a single call if possible to efficiently examine parent-child relationships (e.g. \"ParentClassID,ChildClassID\"). " +
            "Search conditions correspond positionally to ClassIDs - use partial matches like numbers or keywords rather than full identifiers (e.g., '4,' to find items with '4' in the first ClassID and any items for the second ClassID). " +
            "This single call approach reveals the complete hierarchy and relationships between components, avoiding multiple queries.")]
        public static string AppGetInstanceInfo(
            IACComponent mcpHost,
            [Description("Comma-separated list of ClassIDs (CId values from AppGetTypeInfos) for the component types you want to find. Include both parent and child type IDs when exploring hierarchical relationships. Order by compositional logic (parent -> children).")]
            string classIDs,
            [Description("Comma-separated list of search filters for each ClassID to narrow results. Each condition corresponds to the ClassID in the same position. Use text values (e.g., component names, numbers as text). Leave empty positions for ClassIDs without search criteria, but maintain comma separation.")]
            string searchConditions)
        {
            return _appTree.AppGetInstanceInfo(mcpHost, classIDs, searchConditions);
        }


        [McpServerTool]
        [Description("(4) Component Property Discovery: Lists all available properties of a component type or instance. Use this before reading/writing property values to discover available property names and their data types.")]
        public static string AppGetPropertyInfo(
            IACComponent mcpHost,
            [Description("ClassID (CId field) of the component type whose properties you want to discover.")]
            string classID)
        {
            return _appTree.AppGetPropertyInfo(mcpHost, classID);
        }


        [McpServerTool]
        [Description("(5) Component Method Discovery: " +
            "Use this before invoking methods to discover available method names and their parameters. " +
            "ALWAYS check available methods FIRST when you need to perform operations or change component states. " +
            "Many operations are performed through methods rather than direct property assignment. " +
            "Lists all available methods of a component type or instance with their parameters.")]
        public static string AppGetMethodInfo(
            IACComponent mcpHost,
            [Description("ClassID (CId field) of the component type whose methods you want to discover")]
            string classID)
        {
            return _appTree.AppGetMethodInfo(mcpHost, classID);
        }


        [McpServerTool]
        [Description("(6) Component Interaction: Execute operations on specific component instances using ACUrl addressing (like file system paths). " +
            "SUPPORTS BULK OPERATIONS - pass multiple comma-separated ACUrls for efficient batch execution. " +
            "The ACUrl uses ACIdentifiers separated by backslashes to address the complete path from root to target component. " +
            "For state changes and operations, use method invocation (!MethodName) rather than property assignment. Check available methods first with AppGetMethodInfo.")]
        public static string ExecuteACUrlCommand(
            IACComponent mcpHost,
            [Description("Component address path using format: \\Root\\Parent\\Child\\...\\Target\\Operation" +
            "Examples: " +
            "- Single operation read/write property: \\Root\\Component\\PropertyName" +
            "- Bulk operation read/write property: \\Root\\Component1\\PropertyName,\\Root\\Component2\\PropertyName" +
            "- Single operation Invoke method: \\Root\\Component!MethodName" +
            "- Bulk operation invoke method: \\Root\\Component1!MethodName,\\Root\\Component2!MethodName" +
            "Use ACIdentifiers (ACId field) from AppGetInstanceInfo to construct the path. " +
            "Bulk-Execution: If several commands are to be executed one after the other, the ACUrls can be passed comma-separated as a list.")]
            string acUrl,
            [Description("Optional: For property writes set the value here. " +
            "For bulk property writes pass values commas-separated. " +
            "For method-calls use JSON array: [\"param1\", 123, true]")]
            string parametersJson = "")
        {
            return _appTree.ExecuteACUrlCommand(mcpHost, acUrl, parametersJson);
        }
        #endregion


        #endregion
    }
}