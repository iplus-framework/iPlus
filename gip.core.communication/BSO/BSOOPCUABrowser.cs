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
        List<OPCUAClientACSession> _SessionList;
        /// <summary>All OPCUAClientACSession instances that are currently running below the root.</summary>
        [ACPropertyList(502, "Session")]
        public IEnumerable<OPCUAClientACSession> SessionList
        {
            get
            {
                if (_SessionList == null)
                    RefreshSessionList();
                return _SessionList;
            }
        }

        OPCUAClientACSession _CurrentSession;
        /// <summary>The session whose endpoint is used for browsing.</summary>
        [ACPropertyCurrent(503, "Session")]
        public OPCUAClientACSession CurrentSession
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
            }
        }
        #endregion

        #endregion

        #region Methods

        #region Methods => Session
        /// <summary>Searches all running OPCUAClientACSession instances and selects the one that owns the subscription.</summary>
        [ACMethodInfo("Session", "en{'Refresh sessions'}de{'Sessions aktualisieren'}", 502, true)]
        public void RefreshSessionList()
        {
            ACComponent root = Root as ACComponent;
            if (root != null)
                _SessionList = root.FindChildComponents<OPCUAClientACSession>();
            else
                _SessionList = new List<OPCUAClientACSession>();
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

        private void AutoSelectSession()
        {
            if (_SessionList == null || !_SessionList.Any())
                return;

            // Prefer the session that owns a subscription instance of the selected class
            if (CurrentSubscriptionClass != null)
            {
                foreach (OPCUAClientACSession session in _SessionList)
                {
                    if (session.ACComponentChilds == null)
                        continue;
                    foreach (IACComponent child in session.ACComponentChilds)
                    {
                        ACClass childACClass = child.ACType as ACClass;
                        if (childACClass != null && childACClass.ACClassID == CurrentSubscriptionClass.ACClassID)
                        {
                            CurrentSession = session;
                            return;
                        }
                    }
                }
            }
            CurrentSession = _SessionList.FirstOrDefault();
        }

        /// <summary>
        /// Returns the session to browse with. Uses the connected session of the selected
        /// OPCUAClientACSession if available, otherwise opens a temporary session which is
        /// closed at the latest on ACDeInit.
        /// </summary>
        private Session GetBrowseSession(out string error)
        {
            error = null;
            OPCUAClientACSession acSession = CurrentSession;
            if (acSession == null)
            {
                error = "en{'No OPC UA session selected.'}de{'Keine OPC-UA-Session ausgewählt.'}";
                return null;
            }

            if (acSession.UASession != null && !acSession.UASession.Disposed
                && acSession.IsConnected != null && acSession.IsConnected.ValueT)
                return acSession.UASession;

            if (_TempSession != null && !_TempSession.Disposed)
                return _TempSession;

            ApplicationInstance appInstance = acSession.AppInstance;
            if (appInstance == null || appInstance.ApplicationConfiguration == null)
            {
                error = "en{'The OPC UA client application is not started. Start the OPC UA client service first.'}de{'Die OPC-UA-Client-Anwendung ist nicht gestartet. Zuerst den OPC-UA-Client-Dienst starten.'}";
                return null;
            }
            if (String.IsNullOrEmpty(acSession.EndpointURL))
            {
                error = "en{'The session has no endpoint URL configured.'}de{'Die Session hat keine Endpoint-URL konfiguriert.'}";
                return null;
            }

            try
            {
                bool useSecurity = acSession.ParentOPCUAClientACService != null && acSession.ParentOPCUAClientACService.HasAppCertificate;
                EndpointDescription endpointDescription = CoreClientUtils
                    .SelectEndpointAsync(appInstance.ApplicationConfiguration, acSession.EndpointURL, useSecurity, 15000, (ITelemetryContext)null, default)
                    .GetAwaiter().GetResult();
                EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(appInstance.ApplicationConfiguration);
                ConfiguredEndpoint endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                _TempSession = OPCUAClientACSession.CreateSession(appInstance.ApplicationConfiguration, endpoint, false, false);

                UserIdentity userIdentity;
                switch (acSession.UserAuthenticationMode)
                {
                    case OPCUserAuthenticationMode.Username:
                        userIdentity = new UserIdentity(acSession.Username, System.Text.Encoding.UTF8.GetBytes(acSession.Password ?? String.Empty));
                        break;
                    default:
                        // Certificate authentication is not supported for the temporary browse session
                        userIdentity = new UserIdentity(new AnonymousIdentityToken());
                        break;
                }
                _TempSession.OpenAsync(ClassName, 0, userIdentity, null, true, true, default).GetAwaiter().GetResult();
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
                    ShowMsg("en{'The selected class has no configuration point of type OPCItemConfig. Drag an OPCUAClientACSubscr into the project tree and select it.'}de{'Die ausgewählte Klasse hat keinen Konfigurationspunkt vom Typ OPCItemConfig. Zuerst einen OPCUAClientACSubscr in den Projektbaum ziehen und diesen auswählen.'}", eMsgLevel.Error);
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
                    OnPropertyChanged(nameof(CurrentSubscriptionClass));

                string resultMsg;
                if (added == 1)
                    resultMsg = "en{'Added 1 variable to '" + CurrentSubscriptionClass.ACIdentifier + "'. Skipped: " + skipped + ".}de{'1 Variable zu '" + CurrentSubscriptionClass.ACIdentifier + "' hinzugefügt. Übersprungen: " + skipped + ".}";
                else if (added == 0)
                    resultMsg = "en{'No variable added to '" + CurrentSubscriptionClass.ACIdentifier + "'. Skipped: " + skipped + ".}de{'Keine Variable zu '" + CurrentSubscriptionClass.ACIdentifier + "' hinzugefügt. Übersprungen: " + skipped + ".}";
                else
                    resultMsg = "en{'Added " + added + " variables to '" + CurrentSubscriptionClass.ACIdentifier + "'. Skipped: " + skipped + ".}de{'" + added + " Variablen zu '" + CurrentSubscriptionClass.ACIdentifier + "' hinzugefügt. Übersprungen: " + skipped + ".}";
                if (skippedNames.Length > 0)
                    resultMsg += " (" + skippedNames.ToString() + ")";
                ShowMsg(resultMsg, added > 0 ? eMsgLevel.Info : eMsgLevel.Warning);
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

            string identifier = SanitizeIdentifier(nodeItem.BrowseName);
            if (String.IsNullOrEmpty(identifier))
                identifier = SanitizeIdentifier(nodeItem.ACCaption);
            if (String.IsNullOrEmpty(identifier))
            {
                skipped++;
                return;
            }

            // Skip if a property with this identifier already exists on the class hierarchy
            if (CurrentSubscriptionClass.Properties.Any(c => String.Compare(c.ACIdentifier, identifier, StringComparison.OrdinalIgnoreCase) == 0))
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

        private static string SanitizeIdentifier(string name)
        {
            if (String.IsNullOrEmpty(name))
                return null;
            string sanitized = Regex.Replace(name, "[^A-Za-z0-9_]", "_");
            if (sanitized.Length > 100)
                sanitized = sanitized.Substring(0, 100);
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
                    return new string[] { nameof(CurrentSession), nameof(CurrentSubscriptionClass), nameof(BrowseTreeRoot) };
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
    /// </summary>
    public class OPCUANodeItem : ACClassInfoWithItems
    {
        public OPCUANodeItem()
        {
        }

        public OPCUANodeItem(string acCaption)
            : base(acCaption)
        {
        }

        /// <summary>Node id of the OPC UA node, e.g. "ns=2;s=SPS_W4_4PA_Trockner.DK_06.RDY_TO_O".</summary>
        public string UANodeId
        {
            get;
            set;
        }

        /// <summary>Browse name of the OPC UA node (used to derive the ACIdentifier).</summary>
        public string BrowseName
        {
            get;
            set;
        }

        public NodeClass UANodeClass
        {
            get;
            set;
        }

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
