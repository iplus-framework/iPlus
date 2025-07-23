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
        public static string get_thesaurus(
            IACComponent mcpHost,
            [Description("Language code for localized descriptions (e.g., 'en', 'de')")]
            string i18nLangTag,
            [Description("0 = Types for static components that are instantiated during startup and added to the application tree, thus existing statically throughout runtime (Default).\r\n" +
                        "1 = Types for dynamic components that are created during runtime and automatically added to or removed from the application tree when they are no longer needed. These are usually workflow components.\r\n" +
                        "2 = Types for dynamic components 'business objects' or 'apps' that are instantiated at the request of a user and used exclusively by that user to operate apps/programs and handle business processes.\r\n" +
                        "3 = Types of database objects (Entity Framework) or tables. For each type (table), there are one or more database query components (category 4) with which database queries for this table are executed.\r\n" +
                        "4 = Types of database query components. Database query components are Instances of ACQueryDefinition and contain predefined database queries with predefined search parameters that can optionally be filled in with values by the user. " +
                        "Determine the address (ACUrl) of the component using get_instance_info and call the corresponding search method using execute_acurl_command, which you previously determined using get_method_info. " +
                        "As long as no database data is to be changed and only data is to be read, PREFER query components to business objects (category 2).")]
            int category = 0)
        {
            return _appTree.get_thesaurus(mcpHost, i18nLangTag, category);
        }


        [McpServerTool]
        [Description("(2) Component Type Information: Returns detailed information about specific component or object types, including their unique ClassID which is required for other tool operations. " +
            "Pass ACIdentifiers obtained from get_thesaurus to get the ClassIDs needed for instance queries and property/method discovery. " +
            "Use this tool BEFORE you call get_instance_info so that you can pass the correct types or ClassIds there while the types are still unknown to you. " +
            "If you have access to the github-mcp-server, use the search_code tool by passing 'org:iplus-framework' + ACIdentifier to the search query parameter. " +
            "If the search is successful, you can read the class's source code to better understand how it works and in which states you can call which methods and properties using execute_acurl_command. " +
            "Source code can only be retrieved for types with IsCodeOnGithub set to true. If false, traverse the inheritance hierarchy (BaseClassId) until a class with IsCodeOnGithub true is found. ")]
        public static string get_type_infos(
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
            return _appTree.get_type_infos(mcpHost, acIdentifiersOrClassIDs, i18nLangTag, getDerivedTypes);
        }

        [McpServerTool]
        [Description("(3) Application Tree Navigation: Explores the hierarchical tree structure of component instances, similar to a file system directory listing. " +
            "Returns the tree with ACIdentifiers (ACId field) that form the path for ACUrl addressing. " +
            "ALWAYS think hierarchically and PASS MULTIPLE ClassIDs separated by commas in a single call if possible to efficiently examine parent-child relationships (e.g. \"ParentClassID,ChildClassID\"). " +
            "Search conditions correspond positionally to ClassIDs - use partial matches like numbers or keywords rather than full identifiers (e.g., '4,' to find items with '4' in the first ClassID and any items for the second ClassID). " +
            "This single call approach reveals the complete hierarchy and relationships between components, avoiding multiple queries. " +
            "Each instance in the result also has a unique ClassId but is not a real class. Source code on GitHub can only be retrieved for base classes determined via get_type_infos. " +
            "If you pass ClassIDs of types in category 4, you get the ACUrl with which you can execute database queries using execute_acurl_command without having to use business objects (category 2) " +
            "with which you can also manipulate data and handle business processes.")]
        public static string get_instance_info(
            IACComponent mcpHost,
            [Description("Comma-separated list of ClassIDs (values that you got from tool get_type_infos) for the component types you want to find. Include both parent and child type IDs when exploring hierarchical relationships. Order by compositional logic (parent -> children).")]
            string classIDs,
            [Description("Comma-separated list of search filters for each ClassID to narrow results. Each condition corresponds to the ClassID in the same position. Use partial matches like numbers or keywords rather than full identifiers (e.g., '4,' to find items with '4' in the first ClassID and any items for the second ClassID). Leave empty positions for ClassIDs without search criteria, but maintain comma separation.")]
            string searchConditions,
            [Description("If true, only instances with a parent-child (composite) relationship, as defined by the ClassID list, are returned. If false, all instances of the specified type or its subtypes are retrieved, regardless of parent-child relationships, which may result in a large result set. For more targeted searches, set this parameter to true.\r\n" +
            "Default: `true`")]
            bool isCompositeSearch = true)
        {
            return _appTree.get_instance_info(mcpHost, classIDs, searchConditions, isCompositeSearch);
        }


        [McpServerTool]
        [Description("(4) Component Property Discovery: Lists all available properties of a component type or instance. " +
            "Use this before reading/writing property values to discover available property names and their data types. " +
            "If the data type is a complex value, the value of the DataTypeClassID field can be passed recursively to get_property_info to analyze its structure. " +
            "This is useful, for example, for Entity Framework objects to be able to read and modify field values. " +
            "Related properties that are in a master-detail relationship are marked with the same 'Group'. " +
            "For a selection of such complex objects (references) via execute_acurl_command, use the key (ID) from the list and pass the ID as a param to Current and Selected property. " +
            "The ID is then implicitly resolved into an object reference and assigned to the property. " +
            "To effect state changes, ALWAYS prefer to FIRST find a suitable method via get_method_info and execute it via execute_acurl_command instead of changing the property value directly, " +
            "because the methods validate whether a command may be executed or not. ")]
        public static string get_property_info(
            IACComponent mcpHost,
            [Description("ClassID of the component type whose properties you want to discover.")]
            string classID)
        {
            return _appTree.get_property_info(mcpHost, classID);
        }


        [McpServerTool]
        [Description("(5) Component Method Discovery: " +
            "Lists all available methods of a component type or instance with their parameters. " +
            "Use this before invoking methods via execute_acurl_command to discover available method names and their parameters. " +
            "ALWAYS check available methods FIRST when you need to PERFORM operations or CHANGE component states BEFORE you set property values directly which you have discovered via get_property_info. " +
            "Method names (ACId field) with the 'IsEnabled' prefix should be called first, before the same named method without this prefix, " +
            "so that you can check whether the method is allowed to be executed at all. " +
            "If there is no suitable IsEnabled method, you can always call the method. ")]
        public static string get_method_info(
            IACComponent mcpHost,
            [Description("ClassID of the component type whose methods you want to discover")]
            string classID)
        {
            return _appTree.get_method_info(mcpHost, classID);
        }


        [McpServerTool]
        [Description("(6) Component Interaction: Execute operations on specific component instances using ACUrl addressing (like file system paths). " +
            "SUPPORTS BULK OPERATIONS - pass multiple comma-separated ACUrls for efficient batch execution. " +
            "The ACUrl uses ACIdentifiers (ACId field) separated by backslashes to address the complete path from root to target component. " +
            "To QUERY the state of components, prefer reading properties instead of method calls. " +
            "For state CHANGES and OPERATIONS, use method invocation (!MethodName) rather than property assignment. Check available methods first with get_method_info. " +
            "Consider using the GitHub MCP server to read the source code before calling execute_acurl_command to better understand which methods can be executed in which object state. " +
            "Calling void methods without a return value does not necessarily mean that the operation was performed successfully if the response is \"Success: true\". " +
            "It simply means that the method could be called. Whether the desired operation was performed can only be verified by reading the object state or property values.")]
        public static string execute_acurl_command(
            IACComponent mcpHost,
            [Description("Component address path using format: \\Root\\Parent\\Child\\...\\Target\\Operation\r\n" +
                "Examples: \r\n" +
                "- Single operation read/write property: \\Root\\Component\\PropertyName\r\n" +
                "- Bulk operation read/write property: \\Root\\Component1\\PropertyName,\\Root\\Component2\\PropertyName\r\n" +
                "- Single operation Invoke method: \\Root\\Component!MethodName\r\n" +
                "- Bulk operation invoke method: \\Root\\Component1!MethodName,\\Root\\Component2!MethodName\r\n" +
                "- Stopping components with a tilde ~: \\Root\\~Component\r\n" +
                "- Starting components (new instance) with hashtag - Prefer using create_new_instance instead: \\Root\\#Component\r\n" +
                "Use ACIdentifiers (ACId field) from get_instance_info to construct the path. If properties are complex objects, such as Entity Framework objects, their sub-properties can be addressed by continuing the path.\r\n" +
                "Bulk-Execution: If several commands are to be executed one after the other, the ACUrls can be passed comma-separated as a list.\r\n")]
            string acUrl,
            [Description("To write properties, set this parameter to true. Otherwise, set it to false (default).")]
            bool writeProperty = false,
            [Description("Controls the level of detail in JSON serialization for complex objects and Entity Framework entities when reading properties:\r\n" +
                "0 = Minimal: Returns only basic type information and primitive values. Complex objects are represented with minimal metadata.\r\n" +
                "1 = First-degree table relationships: Includes direct foreign key relationships and immediate child collections from Entity Framework models.\r\n" +
                "2 = Complete: Full object serialization including all nested relationships and properties (may result in large JSON responses). Avoid using this option for reading lists, but primarily for reading a current record. " +
                "Use it mainly when you want to copy field values ​​from one entity object that will serve as a template to a new one. Use the ID field with the GUID to copy foreign keys.\r\n" +
                "3 = User-defined: Allows custom field selection via the parametersJson parameter. You can specify exactly which fields to include in the JSON output. Use get_property_info first to discover available field names.\r\n" +
                "Default: 0 (Minimal)")]
            ushort detailLevel = 0,
            [Description("Multi-purpose parameter with different uses depending on the operation and detailLevel:\r\n" +
                "FOR PROPERTY WRITES: Set the value here. For bulk property writes, pass values comma-separated.\r\n" +
                "FOR METHOD CALLS: Use JSON object with key-value pairs for parameters: {\"param1\": \"value1\", \"param2\": 123, \"param3\": true}\r\n" +
                "FOR USER-DEFINED SERIALIZATION (detailLevel=3): Specify which fields to include in JSON output using one of these formats:\r\n" +
                "  - JSON array: [\"FieldName1\", \"FieldName2\", \"RelatedEntity.SubField\"]\r\n" +
                "  - Comma-separated string: \"FieldName1,FieldName2,RelatedEntity.SubField\"\r\n" +
                "Use dot notation for nested properties (e.g., \"Customer.Name\", \"OrderItems.Product.Description\"). " +
                "Call get_property_info beforehand to discover available field names and their data types. " +
                "To set property values ​​that are complex objects (references), pass the key (ID) which in most cases you have taken from the associated list of the master-detail relationship with the same 'Group' name. " +
                "The passed ID is then implicitly resolved into an object reference and assigned to the property.")]
            string parametersJson = "")
        {
            return _appTree.execute_acurl_command(mcpHost, acUrl, writeProperty, detailLevel, parametersJson);
        }

        [McpServerTool]
        [Description("(7) Component instantiation: " +
            "Business objects/apps are dynamic instances (types from get_thesaurus in category 3) and must first be instantiated. " +
            "Do NOT use this method for types obtained via get_thesaurus for other categories (0,1,2,4) " +
            "Upon successful instantiation, they return the same tree structure as with get_instance_info. " +
            "The instance receives a unique ID enclosed in parentheses in the ACUrl string. " +
            "After instantiation, you can work with the business object/app via execute_acurl_command like with all other instances. " +
            "Please note that instances are stateful, and only the caller is allowed to work with their own objects to avoid collisions with other users." +
            "Therefore, do not use other instances that you did not create yourself to ensure exclusive access.When the app is no longer needed, terminate the instance by calling execute_acurl_command with a tilde: " +
            "\\Root\\Businessobjects\\~ACIdentifier(InstanceID)")]
        public static string create_new_instance(
            IACComponent mcpHost,
            [Description("ClassID (values that you got from tool get_type_infos) for the component type you want to instantiate.")]
            string classID,
            [Description("Address (ACUrl) of the parent component under which the new instance should be inserted as a child in the application tree. " +
            "For business objects/apps, the address should always be \\Root\\Businessobjects. This is also the default address if the parameter is left empty.")]
            string acUrl)
        {
            return _appTree.create_new_instance(mcpHost, classID, acUrl);
        }
        #endregion


        #endregion
    }
}