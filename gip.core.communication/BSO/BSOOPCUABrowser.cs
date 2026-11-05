// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.autocomponent;
using gip.core.datamodel;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.communication
{
    /// <summary>
    /// Global BSO that browses the address space of an OPC UA server and lets the user select
    /// variables in a checkbox tree (checking a node automatically checks all loaded children).
    /// "Add to selected Subscription" creates for every selected variable an <see cref="ACClassProperty"/>
    /// with an <see cref="OPCItemConfig"/> (OPCAddr) on the selected subscription class - exactly like
    /// <c>BSOiPlusStudio.NewConfigACClassProperty()</c> does.
    /// <para>
    /// The BSO is opened from BSOiPlusStudio (button in TabConfigList) with the constructor parameter
    /// "AutoLoad" = ACUrl of the selected subscription ACClass (e.g. TagsPA481). The OPC UA endpoint is
    /// taken from the <see cref="OPCUAClientACSession"/> instance that owns the subscription. If that
    /// session is not connected, a temporary session is opened for browsing and closed on de-init.
    /// </para>
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPC UA Browser'}de{'OPC UA Browser'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, true, false)]
    [ACClassConstructorInfo(
        new object[]
        {
            new object[] { "AutoLoad", Global.ParamOption.Optional, typeof(String) }
        }
    )]
    public class BSOOPCUABrowser : ACBSO
    {
        #region const
        public const string ClassName = "BSOOPCUABrowser";
        public const string DummyChildCaption = "...";
        private const int MaxRecursionDepth = 32;
        private const int MaxCollectedNodes = 10000;
        #endregion

        #region c'tors
        public BSOOPCUABrowser(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            string autoLoad = ParameterValue("AutoLoad") as string;
            if (!String.IsNullOrEmpty(autoLoad))
                _CurrentSubscriptionClass = ACUrlCommand(autoLoad) as ACClass;
            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            CloseTempSession();
            _LocalAppInstance = null;
            return await base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties

        #region Properties => Subscription
        ACClass _CurrentSubscriptionClass;
        /// <summary>The subscription class (e.g. TagsPA481) to which the selected variables are added.</summary>
        [ACPropertyCurrent(501, "SubscriptionClass", "en{'Subscription'}de{'Subscription'}")]
        public ACClass CurrentSubscriptionClass
        {
            get
            {
                return _CurrentSubscriptionClass;
            }
            set
            {
                if (_CurrentSubscriptionClass != value)
                {
                    _CurrentSubscriptionClass = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Properties => Session
        List<OPCUASessionConfig> _SessionList;
        /// <summary>All OPCUAClientACSession instances that are currently running below the root.</summary>
        [ACPropertyList(502, "Session")]
        public IEnumerable<OPCUASessionConfig> SessionList
        {
            get
            {
                if (_SessionList == null)
                    RefreshSessionList();
                return _SessionList;
            }
        }

        OPCUASessionConfig _CurrentSession;
        /// <summary>The session whose endpoint is used for browsing.</summary>
        [ACPropertyCurrent(503, "Session")]
        public OPCUASessionConfig CurrentSession
        {
            get
            {
                return _CurrentSession;
            }
            set
            {
                if (_CurrentSession != value)
                {
                    CloseTempSession();
                    _CurrentSession = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Properties => Tree
        OPCUANodeItem _BrowseTreeRoot;
        /// <summary>Root item of the OPC UA address space tree.</summary>
        [ACPropertyCurrent(504, "BrowseTreeRoot")]
        public OPCUANodeItem BrowseTreeRoot
        {
            get
            {
                return _BrowseTreeRoot;
            }
            set
            {
                _BrowseTreeRoot = value;
                OnPropertyChanged();
            }
        }

        OPCUANodeItem _CurrentBrowseItem;
        /// <summary>Selected item of the OPC UA address space tree.</summary>
        [ACPropertyCurrent(505, "BrowseItem")]
        public OPCUANodeItem CurrentBrowseItem
        {
            get
            {
                return _CurrentBrowseItem;
            }
            set
            {
                _CurrentBrowseItem = value;
                OnPropertyChanged();
                UpdateNodeDataType(_CurrentBrowseItem);
            }
        }

        /// <summary>Loads the data type of an OPC UA variable lazily (on first selection) for the info panel.</summary>
        private void UpdateNodeDataType(OPCUANodeItem nodeItem)
        {
            if (nodeItem == null || !nodeItem.IsVariable || !String.IsNullOrEmpty(nodeItem.DataTypeText))
                return;
            try
            {
                Session session = GetBrowseSession(out string error);
                if (session == null)
                {
                    ShowMsg(error, eMsgLevel.Error);
                    return;
                }
                Type systemType = ReadVariableSystemType(session, nodeItem.UANodeId);
                if (systemType != null)
                    nodeItem.DataTypeText = systemType.Name;
            }
            catch (Exception e)
            {
                ShowMsg(e.Message, eMsgLevel.Error);
            }
        }

        private int _TreeSelectionVersion;
        /// <summary>
        /// Incremented whenever the checked state of a node in the browse tree changes.
        /// IsEnabled methods that depend on the checkboxes observe this property
        /// (same pattern as ProdOrderBatchPlanSelectionVersion in BSOBatchPlanScheduler).
        /// </summary>
        [ACPropertyInfo(999, nameof(TreeSelectionVersion), "en{'Tree selection version'}de{'Baum-Auswahlversion'}")]
        public int TreeSelectionVersion
        {
            get => _TreeSelectionVersion;
            private set
            {
                _TreeSelectionVersion = value;
                OnPropertyChanged();
            }
        }

        private void TreeItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e != null && e.PropertyName == ACClassInfoWithItems.IsCheckedPropName)
                TreeSelectionVersion++;
        }

        /// <summary>Subscribes a tree item so checkbox changes bump <see cref="TreeSelectionVersion"/>.</summary>
        private void HookTreeItem(OPCUANodeItem nodeItem)
        {
            if (nodeItem == null)
                return;
            nodeItem.PropertyChanged -= TreeItem_PropertyChanged;
            nodeItem.PropertyChanged += TreeItem_PropertyChanged;
        }
        #endregion

        #endregion

        #region Methods

        #region Methods => Session
        /// <summary>Searches all configured OPCUAClientACSession components below the root.
        /// Real instances (service mode) are used directly; proxies (client mode) are resolved
        /// from their <see cref="ACClass"/> configuration entries (EndpointURL, authentication).
        /// No method or live property of a proxy is ever accessed.</summary>
        [ACMethodInfo("Session", "en{'Refresh sessions'}de{'Sessions aktualisieren'}", 502, true)]
        public void RefreshSessionList()
        {
            _SessionList = FindConfiguredSessions();
            OnPropertyChanged(nameof(SessionList));

            if (_CurrentSession == null || !_SessionList.Contains(_CurrentSession))
            {
                _CurrentSession = null;
                AutoSelectSession();
            }
        }

        public bool IsEnabledRefreshSessionList()
        {
            return true;
        }

        private List<OPCUASessionConfig> FindConfiguredSessions()
        {
            Dictionary<ACClass, OPCUASessionConfig> sessionMap = new Dictionary<ACClass, OPCUASessionConfig>();

            // Search only inside the project that is currently edited in BSOiPlusStudio
            // (acProject of the subscription class), not the whole tree below the root
            ACClass projectRootClass = CurrentSubscriptionClass?.ACProject?.RootClass;
            if (projectRootClass != null)
            {
                CollectConfiguredSessionsFromClass(projectRootClass, sessionMap, 0);
            }
            else
            {
                ACComponent root = Root as ACComponent;
                if (root != null)
                    CollectConfiguredSessions(root, sessionMap, 0);
            }

            // Attach real component instances if the project runs in this process (service mode)
            if (sessionMap.Count > 0)
            {
                ACComponent root = Root as ACComponent;
                if (root != null)
                {
                    foreach (OPCUAClientACSession realSession in root.FindChildComponents<OPCUAClientACSession>())
                    {
                        ACClass realConfigClass = realSession.ACType as ACClass;
                        if (realConfigClass != null && sessionMap.TryGetValue(realConfigClass, out OPCUASessionConfig config)
                            && config.RealSession == null)
                        {
                            config.RealSession = realSession;
                        }
                    }
                }
            }
            return sessionMap.Values.ToList();
        }

        /// <summary>Metadata-only discovery: walks the ACClass tree of the project below the given
        /// root class (children are queried by ParentACClassID). Real component instances are attached
        /// afterwards, so this works in service mode as well as for proxies (client mode).</summary>
        private void CollectConfiguredSessionsFromClass(ACClass acClass, Dictionary<ACClass, OPCUASessionConfig> sessionMap, int depth)
        {
            if (acClass == null || depth > MaxSessionSearchDepth)
                return;

            if (IsSessionClassNode(acClass) && !sessionMap.ContainsKey(acClass))
                sessionMap[acClass] = CreateSessionConfig(acClass, null);

            int childDepth = depth + 1;
            if (childDepth > MaxSessionSearchDepth)
                return;
            Guid parentId = acClass.ACClassID;
            foreach (ACClass childClass in gip.core.datamodel.Database.GlobalDatabase.ACClass
                .Where(c => c.ParentACClassID == parentId).ToArray())
            {
                CollectConfiguredSessionsFromClass(childClass, sessionMap, childDepth);
            }
        }

        /// <summary>Metadata check: is the ACClass node an OPCUAClientACSession component
        /// (its class hierarchy contains the OPCUAClientACSession definition)?</summary>
        private static bool IsSessionClassNode(ACClass acClass)
        {
            if (acClass == null || acClass.ParentACClassID == null)
                return false;
            IEnumerable<ACClass> hierarchy = acClass.ClassHierarchy;
            return hierarchy != null
                && hierarchy.Any(c => c != null && c.ACIdentifier == OPCUAClientACSession.ClassName);
        }

        private void CollectConfiguredSessions(IACComponent component, Dictionary<ACClass, OPCUASessionConfig> sessionMap, int depth)
        {
            if (component == null || depth > MaxSessionSearchDepth)
                return;

            ACClass configClass = ResolveSessionConfigClass(component);
            if (configClass != null && !sessionMap.ContainsKey(configClass))
            {
                OPCUASessionConfig config = CreateSessionConfig(configClass,
                    component as OPCUAClientACSession);
                sessionMap[configClass] = config;
            }

            if (component.ACComponentChilds != null)
            {
                int childDepth = depth + 1;
                foreach (IACComponent child in component.ACComponentChilds)
                {
                    CollectConfiguredSessions(child, sessionMap, childDepth);
                }
            }
        }

        /// <summary>Returns the ACClass (app-tree node) if the component is an OPCUAClientACSession
        /// (real instance or proxy), otherwise null.</summary>
        private static ACClass ResolveSessionConfigClass(IACComponent component)
        {
            ACClass configClass = null;
            OPCUAClientACSession realSession = component as OPCUAClientACSession;
            if (realSession != null)
                configClass = realSession.ACType as ACClass;
            else if (component.IsProxy)
            {
                ACClass acType = component.ACType as ACClass;
                if (acType != null && acType.ClassHierarchy != null
                    && acType.ClassHierarchy.Any(c => c != null && c.ACIdentifier == OPCUAClientACSession.ClassName))
                    configClass = acType;
            }
            return configClass;
        }

        private static OPCUASessionConfig CreateSessionConfig(ACClass configClass, OPCUAClientACSession realSession)
        {
            OPCUASessionConfig config = new OPCUASessionConfig
            {
                ConfigClass = configClass,
                RealSession = realSession,
                EndpointURL = GetConfigValue(configClass, nameof(OPCUAClientACSession.EndpointURL), typeof(string)) as string,
                Username = GetConfigValue(configClass, nameof(OPCUAClientACSession.Username), typeof(string)) as string,
                Password = GetConfigValue(configClass, nameof(OPCUAClientACSession.Password), typeof(string)) as string,
            };

            object authModeValue = GetConfigValue(configClass, nameof(OPCUAClientACSession.UserAuthenticationMode), typeof(OPCUserAuthenticationMode));
            if (authModeValue is OPCUserAuthenticationMode authMode)
                config.AuthMode = authMode;
            return config;
        }

        /// <summary>Reads a persisted [ACPropertyInfo] value of the session component from its
        /// ACClassProperty.XMLValue (same storage from which ACProperty.RestoreValue restores
        /// the property when the real instance starts). Works without a real component instance -
        /// also on a client whose session components only exist as ACComponentProxy.</summary>
        private static object GetConfigValue(ACClass configClass, string propertyACIdentifier, Type targetType)
        {
            ACClassProperty property = FindStoredProperty(configClass, propertyACIdentifier, 0);
            if (property == null)
                return null;
            string xmlValue = property.XMLValue;
            if (String.IsNullOrEmpty(xmlValue))
                return null;
            return DeserializeStoredValue(xmlValue, targetType);
        }

        private const int MaxSessionSearchDepth = 16;
        private const int MaxPropertySearchDepth = 16;

        private static ACClassProperty FindStoredProperty(ACClass configClass, string propertyACIdentifier, int depth)
        {
            if (configClass == null || depth > MaxPropertySearchDepth)
                return null;
            ACClassProperty own = configClass.ACClassProperty_ACClass
                .Where(c => c.ACIdentifier == propertyACIdentifier && !String.IsNullOrEmpty(c.XMLValue))
                .FirstOrDefault();
            if (own != null)
                return own;
            ACClass parent = configClass.ACClass1_ParentACClass;
            return parent == null ? null : FindStoredProperty(parent, propertyACIdentifier, depth + 1);
        }

        /// <summary>Deserializes like ACProperty.RestoreValue: strings as-is, enums/convertibles via ACConvert.</summary>
        private static object DeserializeStoredValue(string xmlValue, Type targetType)
        {
            if (targetType == null)
                return xmlValue;
            if (targetType == typeof(string))
                return xmlValue;
            if (targetType.IsEnum)
            {
                if (Enum.TryParse(targetType, xmlValue, true, out object parsedEnum))
                    return parsedEnum;
                // stored as numeric value
                return Enum.ToObject(targetType, Convert.ChangeType(xmlValue, Enum.GetUnderlyingType(targetType)));
            }
            return ACConvert.ChangeType(xmlValue, targetType, true, gip.core.datamodel.Database.GlobalDatabase);
        }

        private void AutoSelectSession()
        {
            if (_SessionList == null || !_SessionList.Any())
                return;

            // Prefer the session to which the selected subscription class belongs
            if (CurrentSubscriptionClass != null)
            {
                foreach (OPCUASessionConfig sessionConfig in _SessionList)
                {
                    // Prefer the session whose live instance owns the subscription component
                    if (sessionConfig.RealSession != null && sessionConfig.RealSession.ACComponentChilds != null)
                    {
                        foreach (IACComponent child in sessionConfig.RealSession.ACComponentChilds)
                        {
                            ACClass childACClass = child.ACType as ACClass;
                            if (childACClass != null && childACClass.ACClassID == CurrentSubscriptionClass.ACClassID)
                            {
                                CurrentSession = sessionConfig;
                                return;
                            }
                        }
                    }
                    // Metadata fallback: the subscription class is configured as child class of the session class
                    if (sessionConfig.ConfigClass != null
                        && IsChildClassOf(CurrentSubscriptionClass, sessionConfig.ConfigClass))
                    {
                        CurrentSession = sessionConfig;
                        return;
                    }
                }
            }
            CurrentSession = _SessionList.FirstOrDefault();
        }

        private static bool IsChildClassOf(ACClass acClass, ACClass parentClass, int maxDepth = 64)
        {
            int depth = 0;
            ACClass current = acClass;
            while (current != null && depth < maxDepth)
            {
                if (current.ACClassID == parentClass.ACClassID)
                    return true;
                current = current.ACClass1_ParentACClass;
                depth++;
            }
            return false;
        }

        /// <summary>
        /// Returns the session to browse with. Uses the connected session of the selected
        /// OPCUAClientACSession if available, otherwise opens a temporary session which is
        /// closed at the latest on ACDeInit.
        /// </summary>
        private Session GetBrowseSession(out string error)
        {
            error = null;
            OPCUASessionConfig sessionConfig = CurrentSession;
            if (sessionConfig == null)
            {
                error = Root.Environment.TranslateMessage(this, "Error50410");
                return null;
            }
            if (String.IsNullOrEmpty(sessionConfig.EndpointURL))
            {
                error = Root.Environment.TranslateMessage(this, "Error50411");
                return null;
            }

            if (_TempSession != null && !_TempSession.Disposed)
                return _TempSession;

            OPCUAClientACSession realSession = sessionConfig.RealSession;
            // 1. If the real session is connected in this process, browse on it
            if (realSession != null && realSession.UASession != null && !realSession.UASession.Disposed
                && realSession.IsConnected != null && realSession.IsConnected.ValueT)
                return realSession.UASession;

            // 2. Configuration of a running OPCUAClientACService (also if the session is a proxy)
            ApplicationConfiguration appConfiguration = null;
            if (realSession != null && realSession.AppInstance != null)
                appConfiguration = realSession.AppInstance.ApplicationConfiguration;
            if (appConfiguration == null)
            {
                appConfiguration = EnsureLocalAppConfiguration(out error);
                if (appConfiguration == null)
                {
                    error = Root.Environment.TranslateMessage(this, "Error50412", error);
                    return null;
                }
            }

            try
            {
                bool useSecurity = appConfiguration.SecurityConfiguration?.ApplicationCertificate?.Certificate != null;
                EndpointDescription endpointDescription = CoreClientUtils
                    .SelectEndpointAsync(appConfiguration, sessionConfig.EndpointURL, useSecurity, 15000, (ITelemetryContext)null, default)
                    .GetAwaiter().GetResult();
                EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(appConfiguration);
                ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                _TempSession = OPCUAClientACSession.CreateSession(appConfiguration, endpoint, false, false);
                _TempSession.OpenAsync(ClassName, 0, BuildUserIdentity(sessionConfig), null, true, true, default).GetAwaiter().GetResult();
                return _TempSession;
            }
            catch (Exception e)
            {
                CloseTempSession();
                error = e.Message;
                if (e.InnerException != null && !String.IsNullOrEmpty(e.InnerException.Message))
                    error += " Inner: " + e.InnerException.Message;
                return null;
            }
        }

        private void CloseTempSession()
        {
            if (_TempSession == null)
                return;
            try
            {
                if (!_TempSession.Disposed && _TempSession.Connected)
                    _TempSession.CloseAsync(default).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Messages.LogException(ClassName, "CloseTempSession", e);
            }
            _TempSession.Dispose();
            _TempSession = null;
        }

        private Session _TempSession;

        ApplicationInstance _LocalAppInstance;
        /// <summary>Loads the OPC UA client application configuration in this process when no
        /// real OPCUAClientACService instance is running (client mode). Config path is read from
        /// the configured OPCUAClientACService classes (ACClassConfig), default "OPCUAClient".</summary>
        private ApplicationConfiguration EnsureLocalAppConfiguration(out string error)
        {
            error = null;
            if (_LocalAppInstance != null)
                return _LocalAppInstance.ApplicationConfiguration;

            string configSectionName = ResolveClientConfigSectionName();
            _LocalAppInstance = new ApplicationInstance((ITelemetryContext)null)
            {
                ApplicationName = ClassName,
                ApplicationType = Opc.Ua.ApplicationType.Client,
                ConfigSectionName = configSectionName,
            };
            try
            {
                ApplicationConfiguration appConfiguration = _LocalAppInstance
                    .LoadApplicationConfigurationAsync(true, default).GetAwaiter().GetResult();
                if (appConfiguration == null)
                {
                    _LocalAppInstance = null;
                    error = Root.Environment.TranslateMessage(this, "Error50413", configSectionName);
                    return null;
                }
                // Create the application certificate if it does not exist yet (needed for SignAndEncrypt)
                try
                {
                    _LocalAppInstance.CheckApplicationInstanceCertificatesAsync(true, null, default).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Messages.LogException(GetACUrl(), ClassName + ".EnsureLocalAppConfiguration", ex);
                }
                if (appConfiguration.SecurityConfiguration != null
                    && appConfiguration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    // Debug settings like in OPCUAClientACService: accept untrusted certificates
                    appConfiguration.CertificateValidator.CertificateValidation += (validator, e) =>
                    {
                        if (e.Error != null && e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
                            e.Accept = true;
                    };
                    appConfiguration.CertificateValidator.UpdateAsync(appConfiguration, default).GetAwaiter().GetResult();
                }
                return appConfiguration;
            }
            catch (Exception e)
            {
                Messages.LogException(GetACUrl(), ClassName + ".EnsureLocalAppConfiguration", e);
                _LocalAppInstance = null;
                error = "en{'" + e.Message + "'}de{'" + e.Message + "'}";
                return null;
            }
        }

        private string ResolveClientConfigSectionName()
        {
            ACComponent root = Root as ACComponent;
            if (root != null)
            {
                foreach (IACComponent component in root.FindChildComponents<IACComponent>(
                    c => IsClassOfComponent(c, OPCUAClientACService.ClassName), maxChildDepth: MaxSessionSearchDepth))
                {
                    OPCUAClientACService realService = component as OPCUAClientACService;
                    if (realService != null && !String.IsNullOrEmpty(realService.ClientApplicationConfigrationPath))
                        return realService.ClientApplicationConfigrationPath;
                    ACClass acType = component.ACType as ACClass;
                    if (acType != null)
                    {
                        string configured = GetConfigValue(acType, nameof(OPCUAClientACService.ClientApplicationConfigrationPath), typeof(string)) as string;
                        if (!String.IsNullOrEmpty(configured))
                            return configured;
                    }
                }
            }
            // example => config file name: OPCUAClient.Config.xml (relative to exe directory)
            return "OPCUAClient";
        }

        private static bool IsClassOfComponent(IACComponent component, string classACIdentifier)
        {
            ACClass acType = component?.ACType as ACClass;
            if (acType == null)
                return false;
            OPCUAClientACService realService = component as OPCUAClientACService;
            if (realService != null)
                return true;
            if (!component.IsProxy)
                return false;
            return acType.ClassHierarchy != null
                && acType.ClassHierarchy.Any(c => c != null && c.ACIdentifier == classACIdentifier);
        }

        private UserIdentity BuildUserIdentity(OPCUASessionConfig sessionConfig)
        {
            switch (sessionConfig.AuthMode)
            {
                case OPCUserAuthenticationMode.Username:
                    return new UserIdentity(sessionConfig.Username, System.Text.Encoding.UTF8.GetBytes(sessionConfig.Password ?? String.Empty));
                case OPCUserAuthenticationMode.Certificate:
                    // Certificate authentication is not supported for the temporary browse session
                    return new UserIdentity(new AnonymousIdentityToken());
                default:
                    return new UserIdentity(new AnonymousIdentityToken());
            }
        }
        #endregion

        #region Methods => Browse
        /// <summary>Connects to the OPC UA server and builds the root of the address space tree.</summary>
        [ACMethodInfo("Browse", "en{'Connect and Browse'}de{'Verbinden und Browsen'}", 510, true)]
        public void Browse()
        {
            if (!IsEnabledBrowse())
                return;
            if (!PreExecute("Browse"))
                return;
            try
            {
                Session session = GetBrowseSession(out string error);
                if (session == null)
                {
                    ShowMsg(error, eMsgLevel.Error);
                    return;
                }

                OPCUANodeItem root = new OPCUANodeItem("en{'Objects'}de{'Objekte'}");
                HookTreeItem(root);
                root.SetPresentationOptions(true, new ACClassInfoWithItems.VisibilityFilters(), new ACClassInfoWithItems.CheckHandler()
                {
                    IsCheckboxVisible = true,
                });
                root.UANodeId = ObjectIds.ObjectsFolder.ToString();
                root.UANodeClass = NodeClass.Object;
                root.Add(new OPCUANodeItem(DummyChildCaption) { IsDummy = true });

                BrowseTreeRoot = root;
                CurrentBrowseItem = root;

                ExpandNode(root, session);
            }
            catch (Exception e)
            {
                ShowMsg(e.Message, eMsgLevel.Error);
            }
            finally
            {
                PostExecute("Browse");
            }
        }

        public bool IsEnabledBrowse()
        {
            return CurrentSession != null;
        }

        /// <summary>
        /// Called by VBTreeView (VBTreeViewExpandMethod) when a tree item is expanded the first time.
        /// Loads the children of the node from the OPC UA server.
        /// </summary>
        [ACMethodInfo("Browse", "en{'Expand node'}de{'Knoten aufklappen'}", 511)]
        public void OnNodeExpand(OPCUANodeItem nodeItem)
        {
            if (nodeItem == null || nodeItem.HasBeenExpanded)
                return;
            nodeItem.HasBeenExpanded = true;

            Session session = GetBrowseSession(out string error);
            if (session == null)
            {
                ShowMsg(error, eMsgLevel.Error);
                return;
            }
            ExpandNode(nodeItem, session);
        }

        private void ExpandNode(OPCUANodeItem nodeItem, Session session)
        {
            RemoveDummyChild(nodeItem);
            try
            {
                NodeId nodeId = NodeId.Parse(nodeItem.UANodeId);
                List<ReferenceDescription> children = BrowseChildren(session, nodeId, (uint)(NodeClass.Object | NodeClass.Variable));
                foreach (ReferenceDescription reference in children)
                {
                    OPCUANodeItem childItem = CreateNodeItem(session, reference);
                    if (childItem != null)
                        nodeItem.Add(childItem);
                }
            }
            catch (Exception e)
            {
                ShowMsg(e.Message, eMsgLevel.Error);
            }
        }

        private List<ReferenceDescription> BrowseChildren(Session session, NodeId nodeId, uint nodeClassMask)
        {
            var (referenceCollections, errors) = session.ManagedBrowseAsync(
                null, null, new List<NodeId> { nodeId }, 0,
                BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, nodeClassMask, default)
                .GetAwaiter().GetResult();

            List<ReferenceDescription> result = new List<ReferenceDescription>();
            if (referenceCollections == null)
                return result;
            foreach (ServiceResult serviceResult in errors)
            {
                if (serviceResult != null && ServiceResult.IsNotGood(serviceResult.StatusCode))
                    throw new ServiceResultException(serviceResult);
            }
            foreach (ReferenceDescriptionCollection referenceCollection in referenceCollections)
            {
                if (referenceCollection == null)
                    continue;
                foreach (ReferenceDescription reference in referenceCollection)
                {
                    if (reference != null && reference.IsForward && reference.NodeId != null && !reference.NodeId.IsNull)
                        result.Add(reference);
                }
            }
            return result;
        }

        private OPCUANodeItem CreateNodeItem(Session session, ReferenceDescription reference)
        {
            string caption = reference.DisplayName != null ? reference.DisplayName.Text : null;
            if (String.IsNullOrEmpty(caption) && reference.BrowseName != null)
                caption = reference.BrowseName.Name;
            string nodeIdString = GetNodeIdString(session, reference.NodeId);
            if (String.IsNullOrEmpty(nodeIdString))
                return null;
            if (String.IsNullOrEmpty(caption))
                caption = nodeIdString;

            OPCUANodeItem item = new OPCUANodeItem(caption);
            HookTreeItem(item);
            item.UANodeId = nodeIdString;
            item.UANodeClass = reference.NodeClass;
            item.BrowseName = reference.BrowseName != null ? reference.BrowseName.Name : caption;
            if (reference.NodeClass != NodeClass.Variable)
                item.Add(new OPCUANodeItem(DummyChildCaption) { IsDummy = true });
            return item;
        }

        private string GetNodeIdString(Session session, ExpandedNodeId expandedNodeId)
        {
            if (expandedNodeId == null || expandedNodeId.IsNull)
                return null;
            if (String.IsNullOrEmpty(expandedNodeId.NamespaceUri))
                return expandedNodeId.ToString();
            try
            {
                NodeId nodeId = ExpandedNodeId.ToNodeId(expandedNodeId, session.NamespaceUris);
                return nodeId.ToString();
            }
            catch
            {
                return expandedNodeId.ToString();
            }
        }

        private static void RemoveDummyChild(OPCUANodeItem nodeItem)
        {
            OPCUANodeItem dummy = nodeItem.ItemsT.Select(c => c as OPCUANodeItem).FirstOrDefault(c => c != null && c.IsDummy);
            if (dummy != null)
                nodeItem.Remove(dummy);
        }
        #endregion

        #region Methods => Add to Subscription
        /// <summary>
        /// Creates for every checked variable an ACClassProperty with an OPCItemConfig (OPCAddr)
        /// on the selected subscription class. Checked folders are resolved recursively.
        /// </summary>
        [ACMethodInfo("Subscription", "en{'Add to selected Subscription'}de{'Zur ausgewählten Subscription hinzufügen'}", 520, true)]
        public void AddToSubscription()
        {
            if (!IsEnabledAddToSubscription())
                return;
            if (!PreExecute("AddToSubscription"))
                return;
            try
            {
                ACClassProperty pointProperty = FindOPCItemConfigPointProperty();
                if (pointProperty == null)
                {
                    ShowTranslMsg(eMsgLevel.Error, "Error50417");
                    return;
                }

                Session session = GetBrowseSession(out string error);
                if (session == null)
                {
                    ShowMsg(error, eMsgLevel.Error);
                    return;
                }

                List<OPCUANodeItem> checkedItems = new List<OPCUANodeItem>();
                BrowseTreeRoot.CallOnAllItems(delegate (ACClassInfoWithItems item)
                {
                    if (item.IsChecked && item is OPCUANodeItem nodeItem && !nodeItem.IsDummy)
                        checkedItems.Add(nodeItem);
                });

                HashSet<string> processedNodeIds = new HashSet<string>();
                int added = 0;
                int skipped = 0;
                StringBuilder skippedNames = new StringBuilder();

                foreach (OPCUANodeItem nodeItem in checkedItems)
                {
                    if (nodeItem.IsVariable)
                        AddVariable(session, nodeItem, pointProperty, processedNodeIds, ref added, ref skipped, skippedNames);
                    else
                        CollectVariablesRecursive(session, nodeItem, pointProperty, processedNodeIds, ref added, ref skipped, skippedNames, 0);
                }

                if (added > 0)
                {
                    // Same context as BSOiPlusStudio (Database.ContextIPlus). Persist immediately, so the
                    // new ACClassProperty rows (and their ACClassConfig with the OPCAddr) are visible in
                    // BSOiPlusStudio's ConfigACClassPropertyList after re-selecting the subscription class.
                    MsgWithDetails saveResult = Database.ContextIPlus.ACSaveChanges();
                    if (saveResult == null || saveResult.IsSucceded())
                        OnPropertyChanged(nameof(CurrentSubscriptionClass));
                    else
                    {
                        string saveErrors = saveResult.MsgDetails != null
                            ? String.Join(" ", saveResult.MsgDetails.Select(m => m.Message))
                            : null;
                        ShowTranslMsg(eMsgLevel.Error, "Error50418", saveErrors);
                        return;
                    }
                }

                string translId;
                if (added == 1)
                    translId = "Info50415";
                else if (added == 0)
                    translId = "Warning50414";
                else
                    translId = "Info50416";
                // Info50416 has three placeholders: {0}=count, {1}=class, {2}=skipped
                object[] formatArgs = added > 1
                    ? new object[] { added, CurrentSubscriptionClass.ACIdentifier, skipped }
                    : new object[] { CurrentSubscriptionClass.ACIdentifier, skipped };
                Msg resultMsg = new Msg(this, added > 0 ? eMsgLevel.Info : eMsgLevel.Warning, ClassName, "AddToSubscription", 0,
                    translId, formatArgs);
                if (skippedNames.Length > 0)
                    resultMsg.Message += " (" + skippedNames.ToString() + ")";
                Messages.MsgAsync(resultMsg);
            }
            catch (Exception e)
            {
                ShowMsg(e.Message, eMsgLevel.Error);
            }
            finally
            {
                PostExecute("AddToSubscription");
            }
        }

        public bool IsEnabledAddToSubscription()
        {
            if (CurrentSubscriptionClass == null || BrowseTreeRoot == null)
                return false;
            bool anyChecked = false;
            BrowseTreeRoot.CallOnAllItems(delegate (ACClassInfoWithItems item)
            {
                if (!anyChecked && item.IsChecked && item is OPCUANodeItem nodeItem && !nodeItem.IsDummy)
                    anyChecked = true;
            });
            return anyChecked;
        }

        private ACClassProperty FindOPCItemConfigPointProperty()
        {
            // Properties returns all properties over the complete class hierarchy
            return CurrentSubscriptionClass.Properties
                .Where(c => c.ACPropUsageIndex == (Int16)Global.ACPropUsages.ConfigPointProperty
                         && c.ConfigACClass != null
                         && c.ConfigACClass.ACIdentifier == nameof(OPCItemConfig))
                .FirstOrDefault();
        }

        /// <summary>
        /// Returns true if a subscription variable with the same OPC UA NodeId (OPCItemConfig.OPCAddr)
        /// is already configured in the class hierarchy of the selected subscription class.
        /// The config value of such "virtual extended properties" is serialized into the ACClassProperty
        /// row's own XMLConfig column (ACPropertyManager.Serialize), NOT into ACClassConfig rows.
        /// Only properties with a non-empty XMLConfig are probed via the indexer, because a plain
        /// indexer get could try to create a config entry as a side effect (autoCreate=true).
        /// </summary>
        private bool IsOPCAddrAlreadySubscribed(ACClassProperty pointProperty, string uaNodeId)
        {
            if (String.IsNullOrEmpty(uaNodeId) || CurrentSubscriptionClass == null)
                return false;
            string configPointName = pointProperty != null && pointProperty.ConfigACClass != null
                ? pointProperty.ConfigACClass.ACIdentifier
                : nameof(OPCItemConfig);
            foreach (ACClassProperty existingProperty in CurrentSubscriptionClass.Properties.Where(
                c => c.ACPropUsageIndex != (Int16)Global.ACPropUsages.ConfigPointProperty
                  && c.ConfigACClass != null
                  && c.ConfigACClass.ACIdentifier == configPointName
                  && !String.IsNullOrEmpty(c.XMLConfig)))
            {
                OPCItemConfig opcItemConfig = existingProperty[configPointName] as OPCItemConfig;
                if (opcItemConfig != null && String.Equals(opcItemConfig.OPCAddr, uaNodeId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void CollectVariablesRecursive(Session session, OPCUANodeItem folderItem, ACClassProperty pointProperty,
            HashSet<string> processedNodeIds, ref int added, ref int skipped, StringBuilder skippedNames, int depth)
        {
            if (depth > MaxRecursionDepth)
                return;
            try
            {
                NodeId nodeId = NodeId.Parse(folderItem.UANodeId);
                List<ReferenceDescription> children = BrowseChildren(session, nodeId, (uint)(NodeClass.Object | NodeClass.Variable));
                if (children.Count > MaxCollectedNodes)
                    children = children.Take(MaxCollectedNodes).ToList();
                foreach (ReferenceDescription reference in children)
                {
                    OPCUANodeItem childItem = CreateNodeItem(session, reference);
                    if (childItem == null)
                        continue;
                    if (childItem.IsVariable)
                        AddVariable(session, childItem, pointProperty, processedNodeIds, ref added, ref skipped, skippedNames);
                    else
                        CollectVariablesRecursive(session, childItem, pointProperty, processedNodeIds, ref added, ref skipped, skippedNames, depth + 1);
                }
            }
            catch (Exception e)
            {
                ShowMsg(e.Message, eMsgLevel.Error);
            }
        }

        private void AddVariable(Session session, OPCUANodeItem nodeItem, ACClassProperty pointProperty,
            HashSet<string> processedNodeIds, ref int added, ref int skipped, StringBuilder skippedNames)
        {
            if (String.IsNullOrEmpty(nodeItem.UANodeId) || !processedNodeIds.Add(nodeItem.UANodeId))
                return;

            string identifier = BuildIdentifierFromTree(nodeItem);
            if (String.IsNullOrEmpty(identifier))
            {
                skipped++;
                return;
            }

            // Skip if the same OPC UA variable (NodeId) is already configured on the subscription
            // class. Identifier suffixes cannot help here: they would silently create duplicate
            // subscriptions for the same variable when AddToSubscription is executed repeatedly.
            if (IsOPCAddrAlreadySubscribed(pointProperty, nodeItem.UANodeId))
            {
                skipped++;
                if (skippedNames.Length < 500)
                {
                    if (skippedNames.Length > 0)
                        skippedNames.Append(", ");
                    skippedNames.Append(identifier);
                }
                return;
            }

            // Skip if a property with this identifier already exists on the class hierarchy
            string baseIdentifier = identifier;
            int suffix = 1;            while (CurrentSubscriptionClass.Properties.Any(c => String.Compare(c.ACIdentifier, identifier, StringComparison.OrdinalIgnoreCase) == 0))
            {
                suffix++;
                if (suffix > 99)
                {
                    skipped++;
                    if (skippedNames.Length < 500)
                    {
                        if (skippedNames.Length > 0)
                            skippedNames.Append(", ");
                        skippedNames.Append(baseIdentifier);
                    }
                    return;
                }
                string suffixText = "_" + suffix;
                identifier = ShortenIdentifierWithHash(baseIdentifier + suffixText);
            }

            ACClassProperty acClassProperty = ACClassProperty.NewACObject(Database.ContextIPlus, CurrentSubscriptionClass);
            acClassProperty.ConfigACClass = pointProperty.ConfigACClass;
            acClassProperty.ACClassProperty1_ParentACClassProperty = pointProperty;
            acClassProperty.ACIdentifier = identifier;
            acClassProperty.ACCaptionTranslation = nodeItem.ACCaption;
            acClassProperty.SortIndex = NextSortIndex();

            // Set the value type from the data type of the OPC UA variable
            Type systemType = ReadVariableSystemType(session, nodeItem.UANodeId);
            if (systemType != null)
            {
                ACClass valueTypeACClass = Database.ContextIPlus.GetACType(systemType);
                if (valueTypeACClass != null)
                    acClassProperty.ValueTypeACClass = valueTypeACClass;
            }

            // Same hook as BSOiPlusStudio.NewConfigACClassProperty()
            Type configType = acClassProperty.ConfigACClass.ObjectType;
            System.Reflection.MethodInfo mi = configType.GetMethod("InitProperty", Global.bfInvokeMethodStatic);
            if (mi != null)
                mi.Invoke(configType, new object[] { Database.ContextIPlus, acClassProperty });

            Database.ContextIPlus.ACClassProperty.Add(acClassProperty);

            // Creates the Configuration-Value (serializes the OPCItemConfig with the OPCAddr into XMLConfig)
            acClassProperty[nameof(OPCItemConfig)] = new OPCItemConfig() { OPCAddr = nodeItem.UANodeId };
            added++;
        }

        private short NextSortIndex()
        {
            try
            {
                int maxSortIndex = CurrentSubscriptionClass.ACClassProperty_ACClass
                    .Where(c => c.SortIndex < 9999)
                    .Select(c => Convert.ToInt32(c.SortIndex))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToInt16(Math.Min(maxSortIndex + 1, 9998));
            }
            catch
            {
                return 100;
            }
        }

        private Type ReadVariableSystemType(Session session, string nodeIdString)
        {
            try
            {
                Node node = session.ReadNodeAsync(NodeId.Parse(nodeIdString), NodeClass.Variable, true, default).GetAwaiter().GetResult();
                VariableNode variableNode = node as VariableNode;
                if (variableNode == null || variableNode.ValueRank != ValueRanks.Scalar)
                    return null;
                BuiltInType builtInType = TypeInfo.GetBuiltInType(variableNode.DataType);
                switch (builtInType)
                {
                    case BuiltInType.Boolean:
                    case BuiltInType.SByte:
                    case BuiltInType.Byte:
                    case BuiltInType.Int16:
                    case BuiltInType.UInt16:
                    case BuiltInType.Int32:
                    case BuiltInType.UInt32:
                    case BuiltInType.Int64:
                    case BuiltInType.UInt64:
                    case BuiltInType.Float:
                    case BuiltInType.Double:
                    case BuiltInType.String:
                    case BuiltInType.DateTime:
                        return TypeInfo.GetSystemType(builtInType, variableNode.ValueRank);
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Builds the ACIdentifier from the browse-tree hierarchy of the node:
        /// captions of the parent containers (root node excluded) joined with "_" plus the
        /// node's own name, e.g. "Trockner_DK_06_RDY_TO_O". This prevents name collisions when
        /// different folders contain variables with the same browse name.</summary>
        private string BuildIdentifierFromTree(OPCUANodeItem nodeItem)
        {
            string leaf = SanitizeIdentifier(nodeItem.BrowseName) ?? SanitizeIdentifier(nodeItem.ACCaption);
            if (String.IsNullOrEmpty(leaf))
                return null;

            List<string> parts = new List<string>();
            ACClassInfoWithItems current = nodeItem.ParentContainer as ACClassInfoWithItems;
            while (current != null)
            {
                // Exclude the browse-tree root ("en{...}" caption) from the prefix
                if (current != BrowseTreeRoot)
                {
                    string part = SanitizeIdentifier(current.ACCaption);
                    if (!String.IsNullOrEmpty(part))
                        parts.Insert(0, part);
                }
                current = current.ParentContainerT;
            }
            if (parts.Count == 0)
                return leaf;
            parts.Add(leaf);
            string identifier = String.Join("_", parts);
            identifier = ShortenIdentifierWithHash(identifier);
            if (Char.IsDigit(identifier[0]))
                identifier = "_" + identifier;
            return identifier;
        }

        /// <summary>Shortens an identifier that exceeds the max length of an ACIdentifier.
        /// Instead of a blind cut (which can produce collisions) a stable SHA256 hash of the
        /// full string is appended, so two long identifiers cannot collapse into the same
        /// shortened name. GetHashCode is NOT used because it is randomized per process.</summary>
        private static string ShortenIdentifierWithHash(string identifier, int maxLength = 100)
        {
            if (String.IsNullOrEmpty(identifier) || identifier.Length <= maxLength)
                return identifier;
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                string hash = Convert.ToHexString(
                    sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(identifier))).ToLowerInvariant();
                // keep the beginning, append the stable hash - both fit into maxLength
                string hashSuffix = hash.Substring(0, 16);
                int keepCount = maxLength - 1 - hashSuffix.Length;
                if (keepCount < 1)
                    keepCount = 1;
                return identifier.Substring(0, keepCount).Trim('_') + "_" + hashSuffix;
            }
        }

        private static string SanitizeIdentifier(string name)
        {
            if (String.IsNullOrEmpty(name))
                return null;
            string sanitized = Regex.Replace(name, "[^A-Za-z0-9_]", "_");
            sanitized = ShortenIdentifierWithHash(sanitized);
            if (sanitized.Length == 0)
                return null;
            if (Char.IsDigit(sanitized[0]))
                sanitized = "_" + sanitized;
            return sanitized;
        }
        #endregion

        private void ShowMsg(string message, eMsgLevel level)
        {
            if (Messages == null)
                return;
            Messages.MsgAsync(new Msg(message, this, level, ClassName, "ShowMsg", 0));
        }

        /// <summary>
        /// Shows a translatable message stored in the database (ACClassMessage of this class, e.g. "Error50410").
        /// The text is resolved via Environment.TranslateMessage in the current UI language.
        /// </summary>
        private void ShowTranslMsg(eMsgLevel level, string translID, params object[] parameter)
        {
            if (Messages == null)
                return;
            Messages.MsgAsync(new Msg(this, level, ClassName, "ShowMsg", 0, translID, parameter));
        }

        #endregion

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(Browse):
                    Browse();
                    return true;
                case nameof(IsEnabledBrowse):
                    result = IsEnabledBrowse();
                    return true;
                case nameof(OnNodeExpand):
                    OnNodeExpand(acParameter[0] as OPCUANodeItem);
                    return true;
                case nameof(AddToSubscription):
                    AddToSubscription();
                    return true;
                case nameof(IsEnabledAddToSubscription):
                    result = IsEnabledAddToSubscription();
                    return true;
                case nameof(RefreshSessionList):
                    RefreshSessionList();
                    return true;
                case nameof(IsEnabledRefreshSessionList):
                    result = IsEnabledRefreshSessionList();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public override IEnumerable<string> GetPropsToObserveForIsEnabled(string acMethodName)
        {
            switch (acMethodName)
            {
                case nameof(Browse):
                case nameof(IsEnabledBrowse):
                case nameof(AddToSubscription):
                case nameof(IsEnabledAddToSubscription):
                    return new string[] { nameof(CurrentSession), nameof(CurrentSubscriptionClass), nameof(BrowseTreeRoot), nameof(TreeSelectionVersion) };
                case nameof(RefreshSessionList):
                case nameof(IsEnabledRefreshSessionList):
                    return new string[] { nameof(InitState) };
            }
            return base.GetPropsToObserveForIsEnabled(acMethodName);
        }
        #endregion
    }

    /// <summary>
    /// Tree item model for the OPC UA address space. Inherits the checkbox behaviour of
    /// ACClassInfoWithItems (VBTreeView renders a checkbox and cascades checks to all loaded
    /// children automatically).
    /// Declared as ACClass with ACPropertyInfo-attributes so that the layout engine can resolve
    /// VBContent/VBTextBox bindings such as <c>CurrentBrowseItem\UANodeId</c> for the info panel.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPC UA Node'}de{'OPC UA Knoten'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, true)]
    public class OPCUANodeItem : ACClassInfoWithItems
    {
        public OPCUANodeItem()
        {
        }

        public OPCUANodeItem(string acCaption)
            : base(acCaption)
        {
        }

        string _UANodeId;
        /// <summary>Node id of the OPC UA node, e.g. "ns=2;s=SPS_W4_4PA_Trockner.DK_06.RDY_TO_O".</summary>
        [ACPropertyInfo(400, "", "en{'Node id'}de{'Knoten-ID'}")]
        public string UANodeId
        {
            get { return _UANodeId; }
            set
            {
                _UANodeId = value;
                OnPropertyChanged();
            }
        }

        string _BrowseName;
        /// <summary>Browse name of the OPC UA node (used to derive the ACIdentifier).</summary>
        [ACPropertyInfo(401, "", "en{'Browse name'}de{'Browse-Name'}")]
        public string BrowseName
        {
            get { return _BrowseName; }
            set
            {
                _BrowseName = value;
                OnPropertyChanged();
            }
        }

        NodeClass _UANodeClass;
        public NodeClass UANodeClass
        {
            get { return _UANodeClass; }
            set
            {
                _UANodeClass = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UANodeClassText));
                OnPropertyChanged(nameof(IsVariable));
            }
        }

        /// <summary>Node class as display text (the Opc.Ua enum has no TACEnum declaration).</summary>
        [ACPropertyInfo(402, "", "en{'Node class'}de{'Knotenklasse'}")]
        public string UANodeClassText
        {
            get
            {
                switch (UANodeClass)
                {
                    case NodeClass.Variable:
                        return Translator.GetTranslation("en{'Variable'}de{'Variable'}");
                    case NodeClass.Object:
                        return Translator.GetTranslation("en{'Object'}de{'Objekt'}");
                    case NodeClass.Method:
                        return Translator.GetTranslation("en{'Method'}de{'Methode'}");
                    case NodeClass.ObjectType:
                        return Translator.GetTranslation("en{'Object type'}de{'Objekttyp'}");
                    case NodeClass.VariableType:
                        return Translator.GetTranslation("en{'Variable type'}de{'Variablentyp'}");
                    case NodeClass.DataType:
                        return Translator.GetTranslation("en{'Data type'}de{'Datentyp'}");
                    default:
                        return UANodeClass.ToString();
                }
            }
        }

        string _DataTypeText;
        /// <summary>Data type of an OPC UA variable ("Int16", "Boolean", ...) - lazy loaded by
        /// the browser when the node gets selected.</summary>
        [ACPropertyInfo(403, "", "en{'Data type'}de{'Datentyp'}")]
        public string DataTypeText
        {
            get { return _DataTypeText; }
            set
            {
                _DataTypeText = value;
                OnPropertyChanged();
            }
        }

        [ACPropertyInfo(404, "", "en{'Is variable'}de{'Ist Variable'}")]
        public bool IsVariable
        {
            get
            {
                return UANodeClass == NodeClass.Variable;
            }
        }

        /// <summary>True if the children of this node were already loaded from the server.</summary>
        public bool HasBeenExpanded
        {
            get;
            set;
        }

        /// <summary>True if this item is only a placeholder to make the node expandable.</summary>
        public bool IsDummy
        {
            get;
            set;
        }
    }
}
