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
        [Description("(2) Component Type Information: Returns detailed information about specific component types, including their unique ClassID which is required for other tool operations. " +
            "Pass ACIdentifiers obtained from AppGetThesaurus to get the ClassIDs needed for instance queries and property/method discovery. " +
            "Use this tool BEFORE you call AppGetInstanceInfo so that you can pass the correct types or ClassIds there while the types are still unknown to you. " +
            "If you have access to the github-mcp-server, use the search_code tool by passing 'org:iplus-framework' + ACIdentifier to the search query parameter. " +
            "If the search is successful, you can read the class's source code to better understand how it works and in which states you can call which methods and properties using ExecuteACUrlCommand. " +
            "Source code can only be retrieved for types with IsCodeOnGithub set to true. If false, traverse the inheritance hierarchy (BaseClassId) until a class with IsCodeOnGithub true is found. ")]
        public static string AppGetTypeInfos(
            IACComponent mcpHost,
            [Description("(required) Comma-separated list of ACIdentifiers (type names) from the thesaurus or ClassIDs that have already been resolved using the ACIdentifier. Can be base class names to find derived types when used with `getDerivedTypes=true`")]
            string acIdentifiersOrClassIDs,
            [Description("Language code for localized descriptions (e.g., 'en', 'de')")]
            string i18nLangTag,
            [Description(" (optional) When set to `true`, returns all types that inherit from the specified type(s). This is particularly valuable for:\r\n" +
            "- Discovering all implementations of an interface or base class\r\n" +
            "- Enabling bulk operations on polymorphic components\r\n" +
            "- Understanding the complete type hierarchy\r\n" +
            "Default: `false`")]
            bool getDerivedTypes = false,
            [Description(" (optional) When set to `true`, returns all base classes/types along the inheritance hierarchy from the specified type(s). This is particularly valuable for:\r\n" +
            "- Finding the right base class to effectively perform bulk operations on polymorphic components\r\n" +
            "- Understanding the complete type hierarchy\r\n" +
            "Default: `false`")]
            bool getBaseTypes = false)
        {
            return _appTree.AppGetTypeInfos(mcpHost, acIdentifiersOrClassIDs, i18nLangTag, getDerivedTypes);
        }

        // (6) Component Interaction: Execute operations on specific component instances using ACUrl addressing (like file system paths). SUPPORTS BULK OPERATIONS - pass multiple comma-separated ACUrls for efficient batch execution. The ACUrl uses ACIdentifiers separated by backslashes to address the complete path from root to target component. For state changes and operations, use method invocation (!MethodName) rather than property assignment. Check available methods first with AppGetMethodInfo.
        [McpServerTool]
        [Description("(3) Application Tree Navigation: Explores the hierarchical tree structure of component instances, similar to a file system directory listing. " +
            "Returns the tree with ACIdentifiers (ACId field) that form the path for ACUrl addressing. " +
            "ALWAYS think hierarchically and PASS MULTIPLE ClassIDs separated by commas in a single call if possible to efficiently examine parent-child relationships (e.g. \"ParentClassID,ChildClassID\"). " +
            "Search conditions correspond positionally to ClassIDs - use partial matches like numbers or keywords rather than full identifiers (e.g., '4,' to find items with '4' in the first ClassID and any items for the second ClassID). " +
            "This single call approach reveals the complete hierarchy and relationships between components, avoiding multiple queries. " +
            "Each instance in the result also has a unique ClassId but is not a real class. Source code on GitHub can only be retrieved for base classes determined via AppGetTypeInfos.")]
        public static string AppGetInstanceInfo(
            IACComponent mcpHost,
            [Description("Comma-separated list of ClassIDs (values that you got from tool AppGetTypeInfos) for the component types you want to find. Include both parent and child type IDs when exploring hierarchical relationships. Order by compositional logic (parent -> children).")]
            string classIDs,
            [Description("Comma-separated list of search filters for each ClassID to narrow results. Each condition corresponds to the ClassID in the same position. Use partial matches like numbers or keywords rather than full identifiers (e.g., '4,' to find items with '4' in the first ClassID and any items for the second ClassID). Leave empty positions for ClassIDs without search criteria, but maintain comma separation.")]
            string searchConditions,
            [Description("If true, only instances with a parent-child (composite) relationship, as defined by the ClassID list, are returned. If false, all instances of the specified type or its subtypes are retrieved, regardless of parent-child relationships, which may result in a large result set. For more targeted searches, set this parameter to true.\r\n" +
            "Default: `true`")]
            bool isCompositeSearch = true)
        {
            return _appTree.AppGetInstanceInfo(mcpHost, classIDs, searchConditions, isCompositeSearch);
        }


        [McpServerTool]
        [Description("(4) Component Property Discovery: Lists all available properties of a component type or instance. " +
            "Use this before reading/writing property values to discover available property names and their data types. " +
            "To effect state changes, ALWAYS prefer to FIRST find a suitable method via AppGetMethodInfo and execute it via ExecuteACUrlCommand instead of changing the property value directly, " +
            "because the methods validate whether a command may be executed or not. ")]
        public static string AppGetPropertyInfo(
            IACComponent mcpHost,
            [Description("ClassID of the component type whose properties you want to discover.")]
            string classID)
        {
            return _appTree.AppGetPropertyInfo(mcpHost, classID);
        }


        [McpServerTool]
        [Description("(5) Component Method Discovery: " +
            "Lists all available methods of a component type or instance with their parameters. " +
            "Use this before invoking methods via ExecuteACUrlCommand to discover available method names and their parameters. " +
            "ALWAYS check available methods FIRST when you need to perform operations or change component states BEFORE you set property values directly which you have discovered via AppGetPropertyInfo. " +
            "Method names (ACId field) with the 'IsEnabled' prefix should be called first, before the same named method without this prefix, " +
            "so that you can check whether the method is allowed to be executed at all. " +
            "If there is no suitable IsEnabled method, you can always call the method. ")]
        public static string AppGetMethodInfo(
            IACComponent mcpHost,
            [Description("ClassID of the component type whose methods you want to discover")]
            string classID)
        {
            return _appTree.AppGetMethodInfo(mcpHost, classID);
        }


        [McpServerTool]
        [Description("(6) Component Interaction: Execute operations on specific component instances using ACUrl addressing (like file system paths). " +
            "SUPPORTS BULK OPERATIONS - pass multiple comma-separated ACUrls for efficient batch execution. " +
            "The ACUrl uses ACIdentifiers (ACId field) separated by backslashes to address the complete path from root to target component. " +
            "For state changes and operations, use method invocation (!MethodName) rather than property assignment. Check available methods first with AppGetMethodInfo. " +
            "Consider using the GitHub MCP server to read the source code before calling ExecuteACUrlCommand to better understand which methods can be executed in which object state.")]
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