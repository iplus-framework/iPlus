// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.

using gip.core.autocomponent;
using gip.core.datamodel;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text;

// gip.core.webservices.shared defines DTO classes with the same names in the same namespace
// (namespace gip.core.webservices) - alias the entity types to avoid ambiguity.
using DBACClass = gip.core.datamodel.ACClass;
using DBACClassProperty = gip.core.datamodel.ACClassProperty;

namespace gip.core.webservices
{
    /// <summary>
    /// Partial extension of <see cref="MCPIPlusTools"/> with development/engineering features.
    /// These tools expose the functionality that a developer normally performs interactively
    /// via the business object <c>BSOiPlusStudio</c> and the <c>ACProjectManager</c>:
    /// creating projects (Application/AppDefinition), adding/removing classes and class
    /// compositions, and managing designs (XAML), methods and properties of classes.
    /// All operations work headless directly on the database context - no UI dialogs are needed.
    /// </summary>
    public sealed partial class MCPIPlusTools
    {
        #region Properties
        private static MCPToolDevEnvironment _devToolsServer = new MCPToolDevEnvironment(false);
        private static MCPToolDevEnvironment _devToolsLocal = new MCPToolDevEnvironment(true);

        private static MCPToolDevEnvironment GetDevTools(IACComponent mcpHost)
        {
            if (mcpHost != null && mcpHost.ParentACComponent != null && mcpHost.ParentACComponent == ACRoot.SRoot.LocalServiceObjects)
                return _devToolsLocal;
            return _devToolsServer;
        }
        #endregion

        #region MCP-Dev-Tools

        [McpServerTool(Name = "dev_create_project"), Description("(D1) Development: Creates a new project (ACProject) in the development environment, " +
            "equivalent to 'New Project' in BSOiPlusStudio. " +
            "An 'Application' project is a runnable application project. An 'AppDefinition' project serves as a template/abstraction layer " +
            "whose classes can be reused (based on) in application projects. A 'Service' project is like an application but for services. " +
            "If basedOnProjectName is given, the root classes of the base project are copied into the new project (composition inheritance). " +
            "A root class (ApplicationManager) is always created automatically with the project name as ACIdentifier. " +
            "Use get_type_infos/get_instance_info afterwards to explore the new project.")]
        public static string dev_create_project(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("Language code for localized messages (e.g., 'en', 'de')")]
            string i18nLangTag,
            [Description("(required) Unique name of the new project. Also becomes the ACIdentifier of the automatically created root class.")]
            string projectName,
            [Description("Project type: 'Application' (default), 'AppDefinition' (template/abstraction project) or 'Service'.")]
            string projectType = "Application",
            [Description("(optional) Name of an existing project on which the new project is based (e.g. an AppDefinition project). Root classes are copied from it.")]
            string basedOnProjectName = null,
            [Description("If true (default), changes are saved to the database immediately. If false, call dev_save_changes afterwards.")]
            bool saveChanges = true)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_create_project(mcpHost, userRights, projectName, projectType, basedOnProjectName, saveChanges);
        }

        [McpServerTool(Name = "dev_add_class"), Description("(D2) Development: Adds a new class (ACClass) to a project, equivalent to 'New Subclass' in BSOiPlusStudio. " +
            "The new class is derived from baseClassName (an existing class, typically from the class library). " +
            "If parentClassIdentifier is given, the class is inserted as a child of that class (class composition). " +
            "If copyChildClasses is true, all child classes of the base class are copied recursively as well " +
            "(useful to instantiate an entire 'class composition' from a library/AppDefinition project into an application project).")]
        public static string dev_add_class(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("Language code for localized messages (e.g., 'en', 'de')")]
            string i18nLangTag,
            [Description("(required) Name of the target project (ACProjectName) to which the class is added.")]
            string projectName,
            [Description("(required) Unique ACIdentifier of the new class.")]
            string acIdentifier,
            [Description("(optional) ACIdentifier of the base class to derive from. Default: 'ACGenericComponent'. Use get_type_infos to discover available base classes.")]
            string baseClassName = null,
            [Description("(optional) ACIdentifier of an existing class in the project under which the new class is inserted as a child (composition).")]
            string parentClassIdentifier = null,
            [Description("(optional) Caption (description) of the new class. If empty, the ACIdentifier is used.")]
            string accaption = null,
            [Description("If true, child classes of the base class are copied recursively (entire class composition). Default: false.")]
            bool copyChildClasses = false,
            [Description("If true (default), changes are saved to the database immediately. If false, call dev_save_changes afterwards.")]
            bool saveChanges = true)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_add_class(mcpHost, userRights, projectName, acIdentifier, baseClassName, parentClassIdentifier, accaption, copyChildClasses, saveChanges);
        }

        [McpServerTool(Name = "dev_remove_class"), Description("(D3) Development: Removes a class (ACClass) with all its child classes, designs, methods and properties " +
            "recursively from a project, equivalent to 'Delete Class' in BSOiPlusStudio.")]
        public static string dev_remove_class(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("Language code for localized messages (e.g., 'en', 'de')")]
            string i18nLangTag,
            [Description("(required) Name of the project (ACProjectName) that contains the class.")]
            string projectName,
            [Description("(required) ACIdentifier of the class to remove. Child classes are removed recursively.")]
            string acIdentifier,
            [Description("If true (default), changes are saved to the database immediately. If false, call dev_save_changes afterwards.")]
            bool saveChanges = true)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_remove_class(mcpHost, userRights, projectName, acIdentifier, saveChanges);
        }

        [McpServerTool(Name = "dev_add_design"), Description("(D4) Development: Adds a new design (ACClassDesign) with XAML content to a class, " +
            "equivalent to 'New Design' in BSOiPlusStudio. " +
            "IMPORTANT knowledge about design types (ACUsage): " +
            "DUMain = Main desktop view of a business object, loaded first when the BSO is opened (only one per class, named 'Mainlayout'). " +
            "DUMainMobile = Main view for the mobile application, loaded first on mobile devices. " +
            "DULayout = Partial view referenced via VBContent from other designs. " +
            "DUControl = XAML-Design for a control presentation (loaded by VBVisual). " +
            "DUControlDialog = Design shown when the control dialog opens. DUVisualisation = Visualisation page. " +
            "DUMainmenu = Main menu. DUIcon/DUBitmap = image resources. DULL*/DUReport* = reports. " +
            "The desktop app starts with DUMain, the mobile app with DUMainMobile; all further layouts are referenced inside the XAML. " +
            "A default XAML skeleton is generated unless xamlContent is provided. " +
            "AVAILABLE CONTROLS: The XAML uses the iPlus VB-controls from the layout engine. Discover them either via get_thesaurus/get_type_infos " +
            "(visual classes, ACKinds TACVBControl) or by reading the source of 'gip.core.layoutengine.avui' (Avalonia) and 'gip.core.layoutengine' (WPF) " +
            "in the GitHub repository org 'iplus-framework'. Typical controls: VBGrid, VBScrollViewer, VBFrame, VBHeader, VBTextBox, VBPasswordBox, VBCheckBox, " +
            "VBComboBox, VBRadioButtonGroup, VBButton, VBDataGrid, VBQueryFilterControl, VBDatePicker, VBImage. " +
            "CONVENTIONS FOR MOBILE DESIGNS (DUMainMobile and its DULayout detail pages): " +
            "(1) The main mobile design (DUMainMobile) of a list-based BSO typically contains a 'VBQueryFilterControl' (VBContent=\"AccessPrimary\\NavACQueryDefinition\") " +
            "above a 'VBDataGrid' (VBContent=<Selected property of the BSO>, VBShowColumns=comma-separated columns). " +
            "(2) Navigation to detail pages: set 'OpenDesignOnClick' on the VBDataGrid (or on a VBButton) to the ACIdentifier of a DULayout design of the same class; " +
            "the VBButton can additionally be used for deeper navigation (e.g. to master-data pages). " +
            "(3) Row heights: desktop designs use 30px control heights side-by-side; in mobile designs use 60px rows and place the field label ABOVE the control " +
            "(controls span the full width, one field per row). " +
            "(4) Wrap detail pages in a 'VBScrollViewer' (VerticalScrollBarVisibility=\"Auto\") containing a VBGrid with the fields and a bottom button row.")]
        public static string dev_add_design(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("Language code for localized messages (e.g., 'en', 'de')")]
            string i18nLangTag,
            [Description("(required) ACIdentifier of the class (ACClass) to which the design is added.")]
            string classIdentifier,
            [Description("(required) Unique ACIdentifier of the new design. For the first DUMain design of a class, 'Mainlayout' is enforced.")]
            string designIdentifier,
            [Description("(required) Design usage: DUMain, DUMainMobile, DULayout, DUControl, DUControlDialog, DUVisualisation, DUMainmenu, DUIcon, DUBitmap, DUDiagnostic, DULLReport, DULLOverview, DULLList, DULLLabel, DULLFilecard, DUReport, DUReportPrintServer (or the numeric enum value).")]
            string usage,
            [Description("(optional) Caption of the design.")]
            string accaption = null,
            [Description("If true, the design is stored as WPF-Resource style (VBBorder based). Default: false.")]
            bool isResourceStyle = false,
            [Description("(optional) Initial XAML content. If empty, a default skeleton for the usage is generated.")]
            string xamlContent = null,
            [Description("Which design column to write when xamlContent is given: 'Avalonia' (XMLDesign2, default), 'WPF' (XMLDesign) or 'Both'.")]
            string targetDesign = "Avalonia",
            [Description("If true (default), changes are saved to the database immediately. If false, call dev_save_changes afterwards.")]
            bool saveChanges = true)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_add_design(mcpHost, userRights, classIdentifier, designIdentifier, usage, accaption, isResourceStyle, xamlContent, targetDesign, saveChanges);
        }

        [McpServerTool(Name = "dev_get_design"), Description("(D5) Development: Reads the XAML content of one or more designs of a class. " +
            "Use this to analyze existing desktop designs (DUMain/DULayout) before creating equivalent mobile designs (DUMainMobile/DULayout). " +
            "Typical workflow for desktop-to-mobile conversion: read the DUMain design, identify the bound properties (VBContent paths) and columns (VBShowColumns), " +
            "then generate a DUMainMobile design with VBQueryFilterControl + VBDataGrid (OpenDesignOnClick pointing to a new DULayout detail design) " +
            "and DULayout detail designs with 60px rows, labels above the controls, wrapped in VBScrollViewer.")]
        public static string dev_get_design(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("(required) ACIdentifier of the class (ACClass) that owns the designs.")]
            string classIdentifier,
            [Description("(optional) ACIdentifier of a specific design. If empty, all designs of the class are returned (without XAML content, only metadata).")]
            string designIdentifier = null,
            [Description("Which design column to read: 'Avalonia' (XMLDesign2, default), 'WPF' (XMLDesign) or 'Both'.")]
            string targetDesign = "Avalonia")
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_get_design(mcpHost, userRights, classIdentifier, designIdentifier, targetDesign);
        }

        [McpServerTool(Name = "dev_update_design"), Description("(D6) Development: Updates the XAML content of an existing design (ACClassDesign). " +
            "Use dev_get_design first to read the current content, modify it and write it back.")]
        public static string dev_update_design(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("(required) ACIdentifier of the class (ACClass) that owns the design.")]
            string classIdentifier,
            [Description("(required) ACIdentifier of the design to update.")]
            string designIdentifier,
            [Description("(required) The complete new XAML content.")]
            string xamlContent,
            [Description("Which design column to write: 'Avalonia' (XMLDesign2, default), 'WPF' (XMLDesign) or 'Both'.")]
            string targetDesign = "Avalonia",
            [Description("(optional) New caption for the design.")]
            string accaption = null,
            [Description("If true (default), changes are saved to the database immediately. If false, call dev_save_changes afterwards.")]
            bool saveChanges = true)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_update_design(mcpHost, userRights, classIdentifier, designIdentifier, xamlContent, targetDesign, accaption, saveChanges);
        }

        [McpServerTool(Name = "dev_remove_design"), Description("(D7) Development: Removes a design (ACClassDesign) from a class.")]
        public static string dev_remove_design(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("(required) ACIdentifier of the class (ACClass) that owns the design.")]
            string classIdentifier,
            [Description("(required) ACIdentifier of the design to remove.")]
            string designIdentifier,
            [Description("If true (default), changes are saved to the database immediately. If false, call dev_save_changes afterwards.")]
            bool saveChanges = true)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_remove_design(mcpHost, userRights, classIdentifier, designIdentifier, saveChanges);
        }

        [McpServerTool(Name = "dev_add_method"), Description("(D8) Development: Adds a new method (ACClassMethod) to a class, " +
            "equivalent to 'New Method' in BSOiPlusStudio. " +
            "methodType: 'Script' = virtual method with C# source code skeleton (MSMethodExt, server-side), " +
            "'ScriptClient' = client-side trigger method (MSMethodExtClient), " +
            "'Workflow' = workflow method (MSWorkflow), " +
            "'Pre'/'Post' = pre/post method attached to an existing method (attachToMethodName required). " +
            "Script methods get a generated C# skeleton in Sourcecode which can later be edited via dev_update_method_sourcecode.")]
        public static string dev_add_method(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("Language code for localized messages (e.g., 'en', 'de')")]
            string i18nLangTag,
            [Description("(required) ACIdentifier of the class (ACClass) to which the method is added.")]
            string classIdentifier,
            [Description("(optional) Name of the new method. If empty, a unique name ('Script1', 'Script2', ...) is generated.")]
            string methodName = null,
            [Description("Method type: 'Script' (default), 'ScriptClient', 'Workflow', 'Pre' or 'Post'.")]
            string methodType = "Script",
            [Description("(optional, for 'Pre'/'Post') Name of the existing method to which the pre/post method is attached.")]
            string attachToMethodName = null,
            [Description("If true (default), changes are saved to the database immediately. If false, call dev_save_changes afterwards.")]
            bool saveChanges = true)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_add_method(mcpHost, userRights, classIdentifier, methodName, methodType, attachToMethodName, saveChanges);
        }

        [McpServerTool(Name = "dev_update_method_sourcecode"), Description("(D9) Development: Updates the C# source code of a script method (ACClassMethod with ACKind MSMethodExt/MSMethodExtClient). " +
            "Use get_property_info on the method to read the current Sourcecode first. The code is compiled by the framework's precompiler at runtime.")]
        public static string dev_update_method_sourcecode(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("(required) ACIdentifier of the class (ACClass) that owns the method.")]
            string classIdentifier,
            [Description("(required) Name of the method.")]
            string methodName,
            [Description("(required) The complete new C# source code.")]
            string sourcecode,
            [Description("If true (default), changes are saved to the database immediately. If false, call dev_save_changes afterwards.")]
            bool saveChanges = true)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_update_method_sourcecode(mcpHost, userRights, classIdentifier, methodName, sourcecode, saveChanges);
        }

        [McpServerTool(Name = "dev_remove_method"), Description("(D10) Development: Removes a method (ACClassMethod) from a class. " +
            "Note: reflection-based methods (MSMethod, MSMethodPrePost, MSMethodClient) that are defined in compiled code cannot be removed.")]
        public static string dev_remove_method(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("(required) ACIdentifier of the class (ACClass) that owns the method.")]
            string classIdentifier,
            [Description("(required) Name of the method to remove.")]
            string methodName,
            [Description("If true (default), changes are saved to the database immediately. If false, call dev_save_changes afterwards.")]
            bool saveChanges = true)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_remove_method(mcpHost, userRights, classIdentifier, methodName, saveChanges);
        }

        [McpServerTool(Name = "dev_add_property"), Description("(D11) Development: Adds a new property (DBACClassProperty) to a class, " +
            "equivalent to 'New Property' in BSOiPlusStudio. " +
            "The data type is defined by the property's configuration (ConfigACClass of its ConfigPointProperty). " +
            "If dataTypeACIdentifier is given, the config point is set to that VB-control/type class (e.g. 'VBTextBox', 'VBTextBoxList', 'IACComponent'). " +
            "Use get_property_info afterwards to verify the property structure.")]
        public static string dev_add_property(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("Language code for localized messages (e.g., 'en', 'de')")]
            string i18nLangTag,
            [Description("(required) ACIdentifier of the class (ACClass) to which the property is added.")]
            string classIdentifier,
            [Description("(optional) Name of the new property. If empty, a unique name ('Property1', ...) is generated.")]
            string propertyName = null,
            [Description("(optional) ACIdentifier of the type class that defines the data type of the property (e.g. 'VBTextBox', 'VBCheckBox', 'VBTextBoxList').")]
            string dataTypeACIdentifier = null,
            [Description("(optional) Caption of the property.")]
            string accaption = null,
            [Description("If true (default), changes are saved to the database immediately. If false, call dev_save_changes afterwards.")]
            bool saveChanges = true)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_add_property(mcpHost, userRights, classIdentifier, propertyName, dataTypeACIdentifier, accaption, saveChanges);
        }

        [McpServerTool(Name = "dev_remove_property"), Description("(D12) Development: Removes a property (DBACClassProperty) from a class.")]
        public static string dev_remove_property(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("(required) ACIdentifier of the class (ACClass) that owns the property.")]
            string classIdentifier,
            [Description("(required) Name of the property to remove.")]
            string propertyName,
            [Description("If true (default), changes are saved to the database immediately. If false, call dev_save_changes afterwards.")]
            bool saveChanges = true)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_remove_property(mcpHost, userRights, classIdentifier, propertyName, saveChanges);
        }

        [McpServerTool(Name = "dev_save_changes"), Description("(D13) Development: Saves all pending changes made with dev_* tools that were called with saveChanges=false. " +
            "Returns the result of ACSaveChanges including validation messages.")]
        public static string dev_save_changes(
            McpServer server,
            RequestContext<CallToolRequestParams> context,
            IACComponent mcpHost,
            [Description("Language code for localized messages (e.g., 'en', 'de')")]
            string i18nLangTag)
        {
            VBUserRights userRights = null;
            if (mcpHost is PAMcpServerHost host)
                userRights = host.ResolveUserForSession(server);
            return GetDevTools(mcpHost).dev_save_changes(mcpHost, userRights);
        }

        #endregion
    }


    /// <summary>
    /// Implementation of the development tools. Works headless on the database context,
    /// replicating the logic of BSOiPlusStudio / ACProjectManager without UI dialogs.
    /// </summary>
    public class MCPToolDevEnvironment : MCPToolBase
    {
        #region Properties
        private bool _LocalUsage;
        // Not used by the dev tools (they serialize their own anonymous result objects),
        // but required by the abstract MCPToolBase for the entity JSON converter factory.
        protected override Dictionary<string, gip.core.datamodel.ACClass> EntityTypes => null;
        private const string XmlnsAvalonia = "xmlns=\"https://github.com/avaloniaui\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:vb=\"http://www.iplus-framework.com/axaml\"";
        private const string XmlnsWpf = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" xmlns:vb=\"http://www.iplus-framework.com/axaml\"";

        // The dev tools work on their own database context (like BSOiPlusStudio does with its
        // own BSO context) to avoid concurrency issues with the shared global context and with
        // other components that track entities in their contexts. The context is created once
        // per MCPToolDevEnvironment instance (server usage and local usage are separated) and
        // managed by the ACObjectContextManager.
        private const string DevDatabaseContextId = "MCPDevTools";
        private Database _DevDatabase = null;
        #endregion

        #region c´tors
        public MCPToolDevEnvironment(bool localUsage)
        {
            _LocalUsage = localUsage;
        }
        #endregion

        #region Helper
        private Database GetDatabase(IACComponent mcpHost)
        {
            using (ACMonitor.Lock(_80000_Lock))
            {
                if (_DevDatabase == null)
                    _DevDatabase = ACObjectContextManager.GetOrCreateContext<Database>(DevDatabaseContextId);
                return _DevDatabase;
            }
        }

        private string Serialize(object result)
        {
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }

        private string Error(string message)
        {
            return Serialize(new { Success = false, Error = message });
        }

        private ACProject FindACProject(Database db, string projectName)
        {
            if (string.IsNullOrEmpty(projectName))
                return null;
            return db.ACProject.Where(c => c.ACProjectName == projectName).FirstOrDefault();
        }

        private DBACClass FindACClass(Database db, string acIdentifier, ACProject project = null)
        {
            if (string.IsNullOrEmpty(acIdentifier))
                return null;
            IQueryable<DBACClass> query = db.ACClass.Where(c => c.ACIdentifier == acIdentifier);
            if (project != null)
            {
                DBACClass inProject = query.Where(c => c.ACProjectID == project.ACProjectID).FirstOrDefault();
                if (inProject != null)
                    return inProject;
            }
            return query.FirstOrDefault();
        }

        private ACClassDesign FindACClassDesign(Database db, DBACClass acClass, string designIdentifier)
        {
            if (acClass == null || string.IsNullOrEmpty(designIdentifier))
                return null;
            return acClass.ACClassDesign_ACClass.Where(c => c.ACIdentifier == designIdentifier).FirstOrDefault();
        }

        private bool ParseACUsage(string usage, out Global.ACUsages acUsage, out string error)
        {
            acUsage = Global.ACUsages.DUUndefined;
            error = null;
            if (string.IsNullOrEmpty(usage))
            {
                error = "Parameter 'usage' is required.";
                return false;
            }
            usage = usage.Trim();
            if (short.TryParse(usage, out short usageIndex))
            {
                if (Enum.IsDefined(typeof(Global.ACUsages), usageIndex))
                {
                    acUsage = (Global.ACUsages)usageIndex;
                    return true;
                }
                error = $"Unknown numeric usage value '{usage}'.";
                return false;
            }
            if (Enum.TryParse<Global.ACUsages>(usage, true, out acUsage) && acUsage != Global.ACUsages.DUUndefined)
                return true;
            error = $"Unknown usage '{usage}'. Valid values: DUMain, DUMainMobile, DULayout, DUControl, DUControlDialog, DUVisualisation, DUMainmenu, DUIcon, DUBitmap, DUDiagnostic, DULLReport, DULLOverview, DULLList, DULLLabel, DULLFilecard, DUReport, DUReportPrintServer.";
            return false;
        }

        private Global.ACKinds GetACKindForUsage(Global.ACUsages acUsage)
        {
            switch (acUsage)
            {
                case Global.ACUsages.DUMainmenu:
                    return Global.ACKinds.DSDesignMenu;
                case Global.ACUsages.DUBitmap:
                case Global.ACUsages.DUIcon:
                    return Global.ACKinds.DSBitmapResource;
                case Global.ACUsages.DULLReport:
                case Global.ACUsages.DULLOverview:
                case Global.ACUsages.DULLList:
                case Global.ACUsages.DULLLabel:
                case Global.ACUsages.DULLFilecard:
                    return Global.ACKinds.DSDesignReport;
                default:
                    return Global.ACKinds.DSDesignLayout;
            }
        }

        private string BuildDefaultXAML(Global.ACUsages acUsage, DBACClass acClass, string designIdentifier, bool isAvalonia)
        {
            string xmlns = isAvalonia ? XmlnsAvalonia : XmlnsWpf;
            StringBuilder xaml = new StringBuilder();
            switch (acUsage)
            {
                case Global.ACUsages.DUControl:
                    xaml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-16\"?>");
                    xaml.AppendLine("<vb:VBViewbox " + xmlns + (isAvalonia ? " Width=\"200\" Height=\"200\"" : "") + ">");
                    xaml.AppendLine("    <vb:VBCanvas Width=\"200\" Height=\"200\" Background=\"White\" Name=\"Canvas_0\">");
                    xaml.AppendLine("    </vb:VBCanvas>");
                    xaml.Append("</vb:VBViewbox>");
                    break;
                case Global.ACUsages.DUMain:
                    {
                        string name = acClass.ACIdentifier;
                        if (name.StartsWith("BSO")) name = name.Substring(3);
                        if (name.StartsWith("MDBSO")) name = name.Substring(5);
                        xaml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-16\"?>");
                        xaml.AppendLine("<vb:VBDockingManager " + xmlns + " x:Name=\"" + acClass.ACIdentifier + "\">");
                        xaml.AppendLine("    <vb:VBDesign VBContent=\"*" + name + "\" vb:VBDockingManager.IsCloseableBSORoot=\"False\" vb:VBDockingManager.Container=\"TabItem\" vb:VBDockingManager.DockState=\"Tabbed\" vb:VBDockingManager.DockPosition=\"Bottom\" vb:VBDockingManager.RibbonBarVisibility=\"Hidden\" vb:VBDockingManager.WindowSize=\"0,0\" />");
                        xaml.AppendLine("    <vb:VBDesign VBContent=\"*Explorer\" vb:VBDockingManager.IsCloseableBSORoot=\"False\" vb:VBDockingManager.Container=\"DockableWindow\" vb:VBDockingManager.DockState=\"AutoHideButton\" vb:VBDockingManager.DockPosition=\"Bottom\" vb:VBDockingManager.RibbonBarVisibility=\"Hidden\" vb:VBDockingManager.WindowSize=\"0,0\" />");
                        xaml.Append("</vb:VBDockingManager>");
                    }
                    break;
                case Global.ACUsages.DULayout:
                case Global.ACUsages.DUMainMobile:
                    xaml.AppendLine("<?xml version=\"1.0\" encoding=\"utf-16\"?>");
                    xaml.AppendLine("<vb:VBGrid " + xmlns + ">");
                    xaml.AppendLine("    <Grid.ColumnDefinitions>");
                    xaml.AppendLine("        <ColumnDefinition></ColumnDefinition>");
                    xaml.AppendLine("        <ColumnDefinition></ColumnDefinition>");
                    xaml.AppendLine("    </Grid.ColumnDefinitions>");
                    xaml.AppendLine("    <Grid.RowDefinitions>");
                    xaml.AppendLine("        <RowDefinition Height=\"30\"></RowDefinition>");
                    xaml.AppendLine("        <RowDefinition Height=\"30\"></RowDefinition>");
                    xaml.AppendLine("    </Grid.RowDefinitions>");
                    xaml.AppendLine("    <vb:VBFrame Grid.ColumnSpan=\"2\" Grid.RowSpan=\"2\"></vb:VBFrame>");
                    xaml.Append("</vb:VBGrid>");
                    break;
            }
            return xaml.ToString();
        }

        private string WriteXAML(ACClassDesign design, string xamlContent, string targetDesign)
        {
            switch ((targetDesign ?? "").Trim())
            {
                case "WPF":
                    design.XMLDesign = xamlContent;
                    return "XMLDesign (WPF)";
                case "Both":
                    design.XMLDesign = xamlContent;
                    design.XMLDesign2 = xamlContent;
                    return "XMLDesign (WPF) and XMLDesign2 (Avalonia)";
                default:
                    design.XMLDesign2 = xamlContent;
                    return "XMLDesign2 (Avalonia)";
            }
        }

        private MsgWithDetails Save(Database db, bool saveChanges)
        {
            if (!saveChanges)
                return null;
            return db.ACSaveChanges();
        }

        private string SaveResultJson(bool success, string operation, object result, MsgWithDetails saveResult)
        {
            var payload = new Dictionary<string, object>
            {
                { "Success", success },
                { "Operation", operation }
            };
            if (result != null)
                payload["Result"] = result;
            if (saveResult != null && saveResult.MsgDetailsCount > 0)
                payload["SaveMessages"] = saveResult.MsgDetails.Select(m => m.Message).ToList();
            if (saveResult != null && !saveResult.IsSucceded())
                payload["Success"] = false;
            return Serialize(payload);
        }
        #endregion

        #region Dev-Tool implementations
        public string dev_create_project(IACComponent requester, VBUserRights userRights, string projectName, string projectType, string basedOnProjectName, bool saveChanges)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(projectName))
                        return Error("Parameter 'projectName' is required.");
                    Database db = GetDatabase(requester);

                    if (db.ACProject.Where(c => c.ACProjectName == projectName).Any())
                        return Error($"A project with name '{projectName}' already exists.");

                    Global.ACProjectTypes acProjectType = Global.ACProjectTypes.Application;
                    if (!string.IsNullOrEmpty(projectType) && !Enum.TryParse<Global.ACProjectTypes>(projectType, true, out acProjectType))
                        return Error($"Unknown projectType '{projectType}'. Valid values: Application, AppDefinition, Service.");

                    ACProject basedOnProject = null;
                    if (!string.IsNullOrEmpty(basedOnProjectName))
                    {
                        basedOnProject = FindACProject(db, basedOnProjectName);
                        if (basedOnProject == null)
                            return Error($"Base project '{basedOnProjectName}' not found.");
                    }

                    string secondaryKey = ACRoot.SRoot.NoManager.GetNewNo(db, typeof(ACProject), ACProject.NoColumnName, ACProject.FormatNewNo, null);
                    ACProject acProject = ACProject.NewACObject(db, null, secondaryKey);
                    acProject.ACProjectName = projectName;
                    acProject.ACProjectType = acProjectType;
                    acProject.ACCaption = projectName;

                    db.ACProject.Add(acProject);

                    if (basedOnProject != null)
                    {
                        acProject.ACProject1_BasedOnACProject = basedOnProject;
                        acProject.IsProduction = basedOnProject.IsProduction;
                        acProject.IsWorkflowEnabled = basedOnProject.IsWorkflowEnabled;
                        switch (acProjectType)
                        {
                            case Global.ACProjectTypes.AppDefinition:
                                acProject.IsControlCenterEnabled = false;
                                acProject.IsVisualisationEnabled = false;
                                break;
                            case Global.ACProjectTypes.Application:
                            case Global.ACProjectTypes.Service:
                                acProject.IsControlCenterEnabled = true;
                                acProject.IsVisualisationEnabled = true;
                                break;
                        }
                        // Copy root classes of the base project (composition inheritance)
                        var rootClasses = basedOnProject.ACClass_ACProject.Where(c => c.ACClass1_ParentACClass == null && c.ACIdentifier != basedOnProject.ACProjectName).ToList();
                        foreach (DBACClass rootClass in rootClasses)
                            CopyClassRecursive(db, acProject, rootClass, null);
                        DBACClass newRootClass = acProject.RootClass;
                        if (newRootClass != null)
                            newRootClass.ACIdentifier = projectName;
                    }
                    else
                    {
                        acProject.IsEnabled = true;
                        acProject.IsGlobal = false;
                        acProject.IsWorkflowEnabled = true;
                        acProject.IsProduction = true;
                        switch (acProjectType)
                        {
                            case Global.ACProjectTypes.AppDefinition:
                                acProject.IsControlCenterEnabled = false;
                                acProject.IsVisualisationEnabled = false;
                                break;
                            case Global.ACProjectTypes.Application:
                            case Global.ACProjectTypes.Service:
                                acProject.IsControlCenterEnabled = true;
                                acProject.IsVisualisationEnabled = true;
                                break;
                        }

                        // Create root ApplicationManager class like ACProjectManager.InitNewACProject
                        ACProject classLibrary = db.ACProject.Where(c => c.ACProjectType == Global.ACProjectTypes.ClassLibrary).FirstOrDefault();
                        DBACClass acClassModelServer = classLibrary?.ACClass_ACProject
                            .Where(c => c.ACIdentifier == "ApplicationManager" && c.ACKindIndex == (short)Global.ACKinds.TACApplicationManager).FirstOrDefault();
                        DBACClass acClass = DBACClass.NewACObject(db, acProject);
                        acClass.ACIdentifier = projectName;
                        if (acClassModelServer != null)
                        {
                            acClass.ACClass1_BasedOnACClass = acClassModelServer;
                            acClass.ACStorableType = acClassModelServer.ACStorableType;
                            acClass.ACPackage = acClassModelServer.ACPackage;
                            acClass.ACClass1_PWACClass = classLibrary.ACClass_ACProject
                                .Where(c => c.ACIdentifier == PWGroup.PWClassName && c.ACKindIndex == (short)Global.ACKinds.TPWGroup).FirstOrDefault();
                        }
                        if (acClass.ACStorableTypeIndex != (Int16)Global.ACStorableTypes.Required)
                            acClass.ACStorableType = Global.ACStorableTypes.Required;
                        acClass.ACKind = Global.ACKinds.TACApplicationManager;
                        db.ACClass.Add(acClass);
                    }

                    MsgWithDetails saveResult = Save(db, saveChanges);
                    return SaveResultJson(true, "dev_create_project", new
                    {
                        ACProjectID = acProject.ACProjectID,
                        ACProjectName = acProject.ACProjectName,
                        ACProjectType = acProject.ACProjectType.ToString(),
                        BasedOnProject = basedOnProject?.ACProjectName
                    }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        public string dev_add_class(IACComponent requester, VBUserRights userRights, string projectName, string acIdentifier, string baseClassName, string parentClassIdentifier, string accaption, bool copyChildClasses, bool saveChanges)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(acIdentifier))
                        return Error("Parameters 'projectName' and 'acIdentifier' are required.");
                    Database db = GetDatabase(requester);

                    ACProject acProject = FindACProject(db, projectName);
                    if (acProject == null)
                        return Error($"Project '{projectName}' not found.");

                    if (db.ACClass.Where(c => c.ACIdentifier == acIdentifier && c.ACProjectID == acProject.ACProjectID).Any())
                        return Error($"A class with ACIdentifier '{acIdentifier}' already exists in project '{projectName}'.");

                    DBACClass baseClass = FindACClass(db, string.IsNullOrEmpty(baseClassName) ? "ACGenericComponent" : baseClassName);
                    if (baseClass == null)
                        return Error($"Base class '{baseClassName}' not found.");

                    DBACClass parentClass = null;
                    if (!string.IsNullOrEmpty(parentClassIdentifier))
                    {
                        parentClass = FindACClass(db, parentClassIdentifier, acProject);
                        if (parentClass == null)
                            return Error($"Parent class '{parentClassIdentifier}' not found in project '{projectName}'.");
                    }

                    DBACClass acClass = DBACClass.NewACObject(db, acProject);
                    acClass.ACIdentifier = acIdentifier;
                    acClass.ACCaption = string.IsNullOrEmpty(accaption) ? acIdentifier : accaption;
                    acClass.ACClass1_BasedOnACClass = baseClass;
                    acClass.ACKind = baseClass.ACKind;
                    acClass.ACStorableType = baseClass.ACStorableType;
                    if (baseClass.ACPackage != null)
                        acClass.ACPackage = baseClass.ACPackage;
                    if (parentClass != null)
                    {
                        acClass.ACClass1_ParentACClass = parentClass;
                        acClass.ParentACClassID = parentClass.ACClassID;
                        if (parentClass.ACPackage != null)
                            acClass.ACPackageID = parentClass.ACPackageID;
                    }
                    db.ACClass.Add(acClass);

                    int copiedChilds = 0;
                    if (copyChildClasses)
                    {
                        foreach (DBACClass child in baseClass.Childs.ToList())
                            copiedChilds += CopyClassRecursive(db, acProject, child, acClass);
                    }

                    MsgWithDetails saveResult = Save(db, saveChanges);
                    return SaveResultJson(true, "dev_add_class", new
                    {
                        ACClassID = acClass.ACClassID,
                        ACIdentifier = acClass.ACIdentifier,
                        Project = acProject.ACProjectName,
                        BaseClass = baseClass.ACIdentifier,
                        ParentClass = parentClass?.ACIdentifier,
                        CopiedChildClasses = copiedChilds
                    }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        /// <summary>
        /// Copies a class and all its child classes recursively into the target project (class composition).
        /// Mirrors ACProjectManager.GenerateChildAppClasses.
        /// </summary>
        private int CopyClassRecursive(Database db, ACProject targetProject, DBACClass sourceClass, DBACClass newParent)
        {
            DBACClass newClass = DBACClass.NewACObject(db, targetProject);
            newClass.ACIdentifier = sourceClass.ACIdentifier;
            newClass.ACCaption = sourceClass.ACCaption;
            newClass.ACClass1_BasedOnACClass = sourceClass.ACClass1_BasedOnACClass ?? sourceClass;
            newClass.ACKind = sourceClass.ACKind;
            newClass.ACStorableType = sourceClass.ACStorableType;
            if (sourceClass.ACPackage != null)
                newClass.ACPackage = sourceClass.ACPackage;
            if (newParent != null)
            {
                newClass.ACClass1_ParentACClass = newParent;
                newClass.ParentACClassID = newParent.ACClassID;
                if (newParent.ACPackage != null)
                    newClass.ACPackageID = newParent.ACPackageID;
            }
            db.ACClass.Add(newClass);
            int count = 1;
            foreach (DBACClass child in sourceClass.Childs.ToList())
                count += CopyClassRecursive(db, targetProject, child, newClass);
            return count;
        }

        public string dev_remove_class(IACComponent requester, VBUserRights userRights, string projectName, string acIdentifier, bool saveChanges)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(acIdentifier))
                        return Error("Parameters 'projectName' and 'acIdentifier' are required.");
                    Database db = GetDatabase(requester);

                    ACProject acProject = FindACProject(db, projectName);
                    if (acProject == null)
                        return Error($"Project '{projectName}' not found.");

                    DBACClass acClass = FindACClass(db, acIdentifier, acProject);
                    if (acClass == null)
                        return Error($"Class '{acIdentifier}' not found in project '{projectName}'.");

                    if (acClass.ACIdentifier == acProject.ACProjectName)
                        return Error($"Class '{acIdentifier}' is the root class of the project and cannot be removed. Remove the project instead.");

                    Msg msg = acClass.DeleteACClassRecursive(db, true);
                    if (msg != null)
                        return Serialize(new { Success = false, Operation = "dev_remove_class", Error = msg.Message });

                    MsgWithDetails saveResult = Save(db, saveChanges);
                    return SaveResultJson(true, "dev_remove_class", new { RemovedClass = acIdentifier, Project = projectName }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        public string dev_add_design(IACComponent requester, VBUserRights userRights, string classIdentifier, string designIdentifier, string usage, string accaption, bool isResourceStyle, string xamlContent, string targetDesign, bool saveChanges)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(classIdentifier) || string.IsNullOrEmpty(designIdentifier))
                        return Error("Parameters 'classIdentifier' and 'designIdentifier' are required.");
                    if (!ParseACUsage(usage, out Global.ACUsages acUsage, out string parseError))
                        return Error(parseError);
                    Database db = GetDatabase(requester);

                    DBACClass acClass = FindACClass(db, classIdentifier);
                    if (acClass == null)
                        return Error($"Class '{classIdentifier}' not found.");

                    if (acClass.ACClassDesign_ACClass.Where(c => c.ACIdentifier == designIdentifier).Any())
                        return Error($"A design with ACIdentifier '{designIdentifier}' already exists on class '{classIdentifier}'.");

                    string secondaryKey = ACRoot.SRoot.NoManager.GetNewNo(db, typeof(ACClassDesign), ACClassDesign.NoColumnName, ACClassDesign.FormatNewNo, null);
                    ACClassDesign design = ACClassDesign.NewACObject(db.ContextIPlus, acClass, secondaryKey);
                    design.ACIdentifier = designIdentifier;
                    design.ACCaption = string.IsNullOrEmpty(accaption) ? designIdentifier : accaption;
                    design.ACUsage = acUsage;
                    design.ACKind = GetACKindForUsage(acUsage);
                    design.IsResourceStyle = isResourceStyle;

                    // Only one DUMain design per class: enforce the conventional name 'Mainlayout'
                    if (acUsage == Global.ACUsages.DUMain)
                    {
                        if (!acClass.ACClassDesign_ACClass.Where(c => c.ACUsageIndex == (Int16)Global.ACUsages.DUMain).Any())
                            design.ACIdentifier = "Mainlayout";
                    }

                    string xaml = xamlContent;
                    if (string.IsNullOrEmpty(xaml))
                        xaml = BuildDefaultXAML(acUsage, acClass, design.ACIdentifier, true);
                    if (!string.IsNullOrEmpty(xaml))
                        WriteXAML(design, xaml, targetDesign);

                    design.UpdateVBControlACClass(db.ContextIPlus);
                    db.ContextIPlus.ACClassDesign.Add(design);

                    MsgWithDetails saveResult = Save(db, saveChanges);
                    return SaveResultJson(true, "dev_add_design", new
                    {
                        ACClassDesignID = design.ACClassDesignID,
                        ACIdentifier = design.ACIdentifier,
                        Class = classIdentifier,
                        Usage = acUsage.ToString(),
                        ACKind = design.ACKind.ToString(),
                        IsResourceStyle = design.IsResourceStyle
                    }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        public string dev_get_design(IACComponent requester, VBUserRights userRights, string classIdentifier, string designIdentifier, string targetDesign)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(classIdentifier))
                        return Error("Parameter 'classIdentifier' is required.");
                    Database db = GetDatabase(requester);

                    DBACClass acClass = FindACClass(db, classIdentifier);
                    if (acClass == null)
                        return Error($"Class '{classIdentifier}' not found.");

                    string target = (targetDesign ?? "Avalonia").Trim();
                    if (!string.IsNullOrEmpty(designIdentifier))
                    {
                        ACClassDesign design = FindACClassDesign(db, acClass, designIdentifier);
                        if (design == null)
                            return Error($"Design '{designIdentifier}' not found on class '{classIdentifier}'.");
                        return Serialize(new
                        {
                            Success = true,
                            Operation = "dev_get_design",
                            Class = classIdentifier,
                            Design = new
                            {
                                design.ACClassDesignID,
                                design.ACIdentifier,
                                Usage = design.ACUsage.ToString(),
                                ACKind = design.ACKind.ToString(),
                                design.IsResourceStyle,
                                design.IsDefault,
                                XMLDesign = (target == "WPF" || target == "Both") ? design.XMLDesign : null,
                                XMLDesign2 = (target == "Avalonia" || target == "Both") ? design.XMLDesign2 : null
                            }
                        });
                    }

                    var designs = acClass.ACClassDesign_ACClass
                        .OrderBy(c => c.ACUsageIndex).ThenBy(c => c.ACIdentifier)
                        .Select(c => new
                        {
                            c.ACClassDesignID,
                            c.ACIdentifier,
                            Usage = ((Global.ACUsages)c.ACUsageIndex).ToString(),
                            ACKind = ((Global.ACKinds)c.ACKindIndex).ToString(),
                            c.IsResourceStyle,
                            c.IsDefault,
                            HasWPFDesign = !string.IsNullOrEmpty(c.XMLDesign),
                            HasAvaloniaDesign = !string.IsNullOrEmpty(c.XMLDesign2)
                        }).ToList();
                    return Serialize(new { Success = true, Operation = "dev_get_design", Class = classIdentifier, Designs = designs });
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        public string dev_update_design(IACComponent requester, VBUserRights userRights, string classIdentifier, string designIdentifier, string xamlContent, string targetDesign, string accaption, bool saveChanges)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(classIdentifier) || string.IsNullOrEmpty(designIdentifier) || xamlContent == null)
                        return Error("Parameters 'classIdentifier', 'designIdentifier' and 'xamlContent' are required.");
                    Database db = GetDatabase(requester);

                    DBACClass acClass = FindACClass(db, classIdentifier);
                    if (acClass == null)
                        return Error($"Class '{classIdentifier}' not found.");
                    ACClassDesign design = FindACClassDesign(db, acClass, designIdentifier);
                    if (design == null)
                        return Error($"Design '{designIdentifier}' not found on class '{classIdentifier}'.");

                    string writtenTo = WriteXAML(design, xamlContent, targetDesign);
                    if (!string.IsNullOrEmpty(accaption))
                        design.ACCaption = accaption;
                    design.XMLDesignUpdateDate = DateTime.Now;
                    design.XMLDesign2UpdateDate = DateTime.Now;

                    MsgWithDetails saveResult = Save(db, saveChanges);
                    return SaveResultJson(true, "dev_update_design", new
                    {
                        design.ACClassDesignID,
                        design.ACIdentifier,
                        Class = classIdentifier,
                        WrittenTo = writtenTo
                    }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        public string dev_remove_design(IACComponent requester, VBUserRights userRights, string classIdentifier, string designIdentifier, bool saveChanges)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(classIdentifier) || string.IsNullOrEmpty(designIdentifier))
                        return Error("Parameters 'classIdentifier' and 'designIdentifier' are required.");
                    Database db = GetDatabase(requester);

                    DBACClass acClass = FindACClass(db, classIdentifier);
                    if (acClass == null)
                        return Error($"Class '{classIdentifier}' not found.");
                    ACClassDesign design = FindACClassDesign(db, acClass, designIdentifier);
                    if (design == null)
                        return Error($"Design '{designIdentifier}' not found on class '{classIdentifier}'.");

                    Msg msg = design.DeleteACObject(db.ContextIPlus, true);
                    if (msg != null)
                        return Serialize(new { Success = false, Operation = "dev_remove_design", Error = msg.Message });

                    MsgWithDetails saveResult = Save(db, saveChanges);
                    return SaveResultJson(true, "dev_remove_design", new { RemovedDesign = designIdentifier, Class = classIdentifier }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        public string dev_add_method(IACComponent requester, VBUserRights userRights, string classIdentifier, string methodName, string methodType, string attachToMethodName, bool saveChanges)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(classIdentifier))
                        return Error("Parameter 'classIdentifier' is required.");
                    Database db = GetDatabase(requester);

                    DBACClass acClass = FindACClass(db, classIdentifier);
                    if (acClass == null)
                        return Error($"Class '{classIdentifier}' not found.");

                    string type = (methodType ?? "Script").Trim();
                    ACClassMethod acClassMethod = null;
                    switch (type)
                    {
                        case "Workflow":
                            acClassMethod = ACClassMethod.NewWorkACClassMethod(db.ContextIPlus, acClass);
                            break;
                        case "ScriptClient":
                            acClassMethod = ACClassMethod.NewScriptClientACClassMethod(db.ContextIPlus, acClass);
                            break;
                        case "Script":
                            acClassMethod = ACClassMethod.NewScriptACClassMethod(db.ContextIPlus, acClass, Global.ACKinds.MSMethodExt);
                            break;
                        case "Pre":
                        case "Post":
                            {
                                if (string.IsNullOrEmpty(attachToMethodName))
                                    return Error($"For methodType '{type}' the parameter 'attachToMethodName' is required.");
                                ACClassMethod attachTo = acClass.ACClassMethod_ACClass.Where(c => c.ACIdentifier == attachToMethodName).FirstOrDefault();
                                if (attachTo == null)
                                    return Error($"Method '{attachToMethodName}' not found on class '{classIdentifier}'.");
                                acClassMethod = type == "Pre"
                                    ? ACClassMethod.NewPreACClassMethod(db.ContextIPlus, acClass, attachTo)
                                    : ACClassMethod.NewPostACClassMethod(db.ContextIPlus, acClass, attachTo);
                            }
                            break;
                        default:
                            return Error($"Unknown methodType '{methodType}'. Valid values: Script, ScriptClient, Workflow, Pre, Post.");
                    }

                    if (!string.IsNullOrEmpty(methodName))
                    {
                        if (acClass.ACClassMethod_ACClass.Where(c => c.ACIdentifier == methodName).Any())
                            return Error($"A method with name '{methodName}' already exists on class '{classIdentifier}'.");
                        acClassMethod.ACIdentifier = methodName;
                    }
                    acClassMethod.SortIndex = (short)(acClass.ACClassMethod_ACClass.Count + 1);
                    acClass.AddNewACClassMethod(acClassMethod);

                    MsgWithDetails saveResult = Save(db, saveChanges);
                    return SaveResultJson(true, "dev_add_method", new
                    {
                        acClassMethod.ACClassMethodID,
                        acClassMethod.ACIdentifier,
                        Class = classIdentifier,
                        MethodType = type,
                        ACKind = acClassMethod.ACKind.ToString(),
                        HasGeneratedSourcecode = !string.IsNullOrEmpty(acClassMethod.Sourcecode)
                    }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        public string dev_update_method_sourcecode(IACComponent requester, VBUserRights userRights, string classIdentifier, string methodName, string sourcecode, bool saveChanges)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(classIdentifier) || string.IsNullOrEmpty(methodName) || sourcecode == null)
                        return Error("Parameters 'classIdentifier', 'methodName' and 'sourcecode' are required.");
                    Database db = GetDatabase(requester);

                    DBACClass acClass = FindACClass(db, classIdentifier);
                    if (acClass == null)
                        return Error($"Class '{classIdentifier}' not found.");
                    ACClassMethod acClassMethod = acClass.ACClassMethod_ACClass.Where(c => c.ACIdentifier == methodName).FirstOrDefault();
                    if (acClassMethod == null)
                        return Error($"Method '{methodName}' not found on class '{classIdentifier}'.");
                    if (acClassMethod.ACKind != Global.ACKinds.MSMethodExt && acClassMethod.ACKind != Global.ACKinds.MSMethodExtClient && acClassMethod.ACKind != Global.ACKinds.MSMethodExtTrigger)
                        return Error($"Method '{methodName}' is of kind '{acClassMethod.ACKind}' and has no editable source code. Only virtual methods (MSMethodExt/MSMethodExtClient/MSMethodExtTrigger) support source code.");

                    acClassMethod.Sourcecode = sourcecode;
                    MsgWithDetails saveResult = Save(db, saveChanges);
                    return SaveResultJson(true, "dev_update_method_sourcecode", new
                    {
                        acClassMethod.ACClassMethodID,
                        acClassMethod.ACIdentifier,
                        Class = classIdentifier
                    }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        public string dev_remove_method(IACComponent requester, VBUserRights userRights, string classIdentifier, string methodName, bool saveChanges)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(classIdentifier) || string.IsNullOrEmpty(methodName))
                        return Error("Parameters 'classIdentifier' and 'methodName' are required.");
                    Database db = GetDatabase(requester);

                    DBACClass acClass = FindACClass(db, classIdentifier);
                    if (acClass == null)
                        return Error($"Class '{classIdentifier}' not found.");
                    ACClassMethod acClassMethod = acClass.ACClassMethod_ACClass.Where(c => c.ACIdentifier == methodName).FirstOrDefault();
                    if (acClassMethod == null)
                        return Error($"Method '{methodName}' not found on class '{classIdentifier}'.");
                    if (acClassMethod.ACKind == Global.ACKinds.MSMethod || acClassMethod.ACKind == Global.ACKinds.MSMethodPrePost || acClassMethod.ACKind == Global.ACKinds.MSMethodClient)
                        return Error($"Method '{methodName}' is a reflection-based method ({acClassMethod.ACKind}) defined in compiled code and cannot be removed.");

                    Msg msg = acClassMethod.DeleteACObject(db.ContextIPlus, true);
                    if (msg != null)
                        return Serialize(new { Success = false, Operation = "dev_remove_method", Error = msg.Message });

                    MsgWithDetails saveResult = Save(db, saveChanges);
                    return SaveResultJson(true, "dev_remove_method", new { RemovedMethod = methodName, Class = classIdentifier }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        public string dev_add_property(IACComponent requester, VBUserRights userRights, string classIdentifier, string propertyName, string dataTypeACIdentifier, string accaption, bool saveChanges)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(classIdentifier))
                        return Error("Parameter 'classIdentifier' is required.");
                    Database db = GetDatabase(requester);

                    DBACClass acClass = FindACClass(db, classIdentifier);
                    if (acClass == null)
                        return Error($"Class '{classIdentifier}' not found.");

                    DBACClassProperty acClassProperty = DBACClassProperty.NewACObject(db.ContextIPlus, acClass);
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        if (acClass.ACClassProperty_ACClass.Where(c => c.ACIdentifier == propertyName).Any())
                            return Error($"A property with name '{propertyName}' already exists on class '{classIdentifier}'.");
                        acClassProperty.ACIdentifier = propertyName;
                    }
                    if (!string.IsNullOrEmpty(accaption))
                        acClassProperty.ACCaption = accaption;
                    if (!string.IsNullOrEmpty(dataTypeACIdentifier))
                    {
                        DBACClass dataTypeClass = FindACClass(db, dataTypeACIdentifier);
                        if (dataTypeClass == null)
                            return Error($"Data type class '{dataTypeACIdentifier}' not found.");
                        // The data type of a property is stored as ConfigACClass reference
                        // (see ACClassProperty.NewACObject)
                        acClassProperty.ConfigACClass = dataTypeClass;
                    }

                    acClass.ACClassProperty_ACClass.Add(acClassProperty);
                    db.ContextIPlus.ACClassProperty.Add(acClassProperty);

                    MsgWithDetails saveResult = Save(db, saveChanges);
                    return SaveResultJson(true, "dev_add_property", new
                    {
                        acClassProperty.ACClassPropertyID,
                        acClassProperty.ACIdentifier,
                        Class = classIdentifier,
                        DataType = dataTypeACIdentifier
                    }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        public string dev_remove_property(IACComponent requester, VBUserRights userRights, string classIdentifier, string propertyName, bool saveChanges)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    if (string.IsNullOrEmpty(classIdentifier) || string.IsNullOrEmpty(propertyName))
                        return Error("Parameters 'classIdentifier' and 'propertyName' are required.");
                    Database db = GetDatabase(requester);

                    DBACClass acClass = FindACClass(db, classIdentifier);
                    if (acClass == null)
                        return Error($"Class '{classIdentifier}' not found.");
                    DBACClassProperty acClassProperty = acClass.ACClassProperty_ACClass.Where(c => c.ACIdentifier == propertyName).FirstOrDefault();
                    if (acClassProperty == null)
                        return Error($"Property '{propertyName}' not found on class '{classIdentifier}'.");

                    Msg msg = acClassProperty.DeleteACObject(db.ContextIPlus, true);
                    if (msg != null)
                        return Serialize(new { Success = false, Operation = "dev_remove_property", Error = msg.Message });

                    MsgWithDetails saveResult = Save(db, saveChanges);
                    return SaveResultJson(true, "dev_remove_property", new { RemovedProperty = propertyName, Class = classIdentifier }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }

        public string dev_save_changes(IACComponent requester, VBUserRights userRights)
        {
            using (requester.Root.UsingThread(userRights != null ? userRights.VBUser : null))
            {
                try
                {
                    Database db = GetDatabase(requester);
                    MsgWithDetails saveResult = db.ACSaveChanges();
                    return SaveResultJson(saveResult.IsSucceded(), "dev_save_changes", new { ErrorCount = saveResult.MsgDetailsCount }, saveResult);
                }
                catch (Exception ex)
                {
                    return CreateExceptionResponse(ex);
                }
            }
        }
        #endregion
    }
}
