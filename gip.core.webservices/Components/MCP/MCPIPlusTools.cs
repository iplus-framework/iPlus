// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using gip.core.autocomponent;
using gip.core.datamodel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Database.Isam;
using Microsoft.Isam.Esent.Interop;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using static Azure.Core.HttpHeader;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        [Description("(1) Thesaurus for the agent: " +
            "Overview of the types used in the system in order to be able to derive from the terms chosen by the user what intention he has and which objects he is talking about. " +
            "Each result contains an ACIdentifier (unique type name) and Description. " +
            "Use this tool FIRST to discover what component types exist in the system before querying specific instances or type information.")]
        public static string AppGetThesaurus(
            IACComponent mcpHost,
            [Description("Language code for localized descriptions (e.g., 'en', 'de')")]
            string i18nLangTag,
            [Description("0 = Types for static components that are instantiated during startup and added to the application tree, thus existing statically throughout runtime (Default).\r\n" +
                        "1 = Types for dynamic components that are created during runtime and automatically added to or removed from the application tree when they are no longer needed. These are usually workflow components.\r\n" +
                        "2 = Types for dynamic components (so-called business objects or apps) that are instantiated at the request of a user and used exclusively by that user to operate apps and primarily work with database data.\r\n" +
                        "3 = Types of database objects (Entity Framework) or tables.")]
            int category = 0)
        {
            return _appTree.AppGetThesaurus(mcpHost, i18nLangTag, category);
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
            "If the data type is a complex value, the value of the DataTypeClassID field can be passed recursively to AppGetPropertyInfo to analyze its structure. " +
            "This is useful, for example, for Entity Framework objects to be able to read and modify field values.\r\n " +
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
            [Description("Component address path using format: \\Root\\Parent\\Child\\...\\Target\\Operation\r\n" +
            "Examples: \r\n" +
            "- Single operation read/write property: \\Root\\Component\\PropertyName\r\n" +
            "- Bulk operation read/write property: \\Root\\Component1\\PropertyName,\\Root\\Component2\\PropertyName\r\n" +
            "- Single operation Invoke method: \\Root\\Component!MethodName\r\n" +
            "- Bulk operation invoke method: \\Root\\Component1!MethodName,\\Root\\Component2!MethodName\r\n" +
            "- Stopping components with a tilde ~: \\Root\\~Component\r\n" +
            "- Starting components (new instance) with hashtag - Prefer using AppCreateNewInstance instead: \\Root\\#Component\r\n" +
            "Use ACIdentifiers (ACId field) from AppGetInstanceInfo to construct the path. If properties are complex objects, such as Entity Framework objects, their sub-properties can be addressed by continuing the path.\r\n" +
            "Bulk-Execution: If several commands are to be executed one after the other, the ACUrls can be passed comma-separated as a list.\r\n")]
            string acUrl,
            [Description("Optional: For property writes set the value here. For bulk property writes pass values comma-separated. \r\n" +
            "For method-calls with parameters use JSON object with key-value pairs: {\"param1\": \"value1\", \"param2\": 123, \"param3\": true}")]
            string parametersJson = "")
        {
            return _appTree.ExecuteACUrlCommand(mcpHost, acUrl, parametersJson);
        }

        [McpServerTool]
        [Description("(7) Component instantiation: " +
            "Business objects/apps are dynamic instances(types from AppGetThesaurus in category 3) and must first be instantiated." +
            "Upon successful instantiation, they return the same tree structure as with AppGetInstanceInfo." +
            "The instance receives a unique ID enclosed in parentheses in the ACUrl string. " +
            "To work with the business object/app, use ExecuteACUrlCommand as with all other instances in categories 1 and 2. " +
            "Please note that instances are stateful, and only the caller is allowed to work with their own objects to avoid collisions with other users." +
            "Therefore, do not use other instances that you did not create yourself to ensure exclusive access.When the app is no longer needed, terminate the instance by calling ExecuteACUrlCommand with a tilde:" +
            "\\Root\\Businessobjects\\~ACIdentifier(InstanceID)")]
        public static string AppCreateNewInstance(
            IACComponent mcpHost,
            [Description("ClassID (values that you got from tool AppGetTypeInfos) for the component type you want to instantiate.")]
            string classID,
            [Description("Address (ACUrl) of the parent component under which the new instance should be inserted as a child in the application tree. " +
            "For business objects/apps, the address should always be \\Root\\Businessobjects. This is also the default address if the parameter is left empty.")]
            string acUrl)
        {
            return _appTree.AppCreateNewInstance(mcpHost, classID, acUrl);
        }
        #endregion


        #endregion
    }
}