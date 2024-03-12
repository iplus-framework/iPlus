using gip.core.datamodel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static gip.core.autocomponent.PARole;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Routing Service'}de{'Routing Service'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, true)]
    public class ACRoutingService : PAJobScheduler
    {
        #region c'tors

        public ACRoutingService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _RouteItemModes = new ACPropertyConfigValue<string>(this, nameof(RouteItemModes), "");

            RestoreLastManipulationTime();
            InitRouteUsage();

            _ = RouteItemModes;

            return base.ACInit(startChildMode);
        }

        public override bool ACPostInit()
        {
            LoadRouteItemModes();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        public static ACRoutingService GetServiceInstance(ACComponent requester)
        {
            Msg msg = null;
            string routingServiceACUrl = GetRoutingServiceUrl(requester, out msg);
            return GetServiceInstance<ACRoutingService>(requester, routingServiceACUrl, CreationBehaviour.OnlyLocal);
        }

        public static ACRef<ACComponent> ACRefToServiceInstance(ACComponent bso)
        {
            IAppManager appManager = bso as IAppManager;
            if (appManager == null)
                appManager = bso.Root.FindChildComponents<IAppManager>(c => c is IAppManager, null, 1).FirstOrDefault();
            if (appManager == null)
                return null;
            return ACRefToServiceInstance(bso, appManager);
        }

        public static ACRef<ACComponent> ACRefToServiceInstance(ACComponent bso, IAppManager appManager)
        {
            Msg msg = null;
            ACComponent routingService = GetRoutingService(appManager as ACComponent, out msg) as ACComponent;
            if (routingService != null)
                return new ACRef<ACComponent>(routingService, bso);
            else
            {
                string acUrl = GetRoutingServiceUrl(appManager as ACComponent, out msg);
                if (!String.IsNullOrEmpty(acUrl))
                    return new ACRef<ACComponent>(acUrl, bso);
            }
            return null;
        }

        public static void DetachACRefFromServiceInstance(ACComponent bso, ACRef<ACComponent> acRef)
        {
            if (acRef == null)
                return;
            ACComponent manager = acRef.ValueT as ACComponent;
            acRef.Detach();
            if (manager != null)
            {
                if (manager.ParentACComponent == (bso.Root as ACRoot).LocalServiceObjects)
                {
                    if (!manager.ReferencePoint.HasStrongReferences)
                    {
                        manager.Stop();
                    }
                }
            }
        }

        #endregion

        #region Subclasses

        #endregion

        #region const
        public const string MN_SelectRoutes = nameof(SelectRoutes);
        public const string MN_FindSuccessors = "FindSuccessors";
        public const string MN_FindSuccessorsFromPoint = "FindSuccessorsFromPoint";
        public const string MN_FindLastSuccessors = "FindLastSuccessors";
        public const string MN_SetPriority = "SetPriority";
        public const string MN_IncreasePriorityStepwise = "IncreasePriorityStepwise";
        public const string MN_BuildAvailableRoutes = "BuildAvailableRoutes";
        #endregion

        #region Fields

        private static ACMonitorObject _LockObject = new ACMonitorObject(10000);

        private static Dictionary<Guid, PAEdgeInfo> _EdgeCache = new Dictionary<Guid, PAEdgeInfo>();

        private static ACMonitorObject _LockObjectRule = new ACMonitorObject(10000);

        protected static Dictionary<string, SelectionRule> _RegisteredSelectionQueries = new Dictionary<string, SelectionRule>();

        private ConcurrentDictionary<Guid, SafeList<RouteHashItem>> _UsedRoutesCache = new ConcurrentDictionary<Guid, SafeList<RouteHashItem>>();

        private ConcurrentDictionary<Guid, RouteItemModeEnum> _RouteItemsModeCache = new ConcurrentDictionary<Guid, RouteItemModeEnum>();

        #endregion

        #region Precompiled Queries
        public static readonly Func<Database, Guid, short, IQueryable<ACClassPropertyRelation>> s_cQry_TargetRoutes =
        CompiledQuery.Compile<Database, Guid, short, IQueryable<ACClassPropertyRelation>>(
            (ctx, targetID, connectionType) => from c in ctx.ACClassPropertyRelation
                                               where c.TargetACClassID == targetID
                                                      && c.ConnectionTypeIndex == connectionType
                                               select c
        );

        public static readonly Func<Database, Guid, Guid, short, IQueryable<ACClassPropertyRelation>> s_cQry_TargetRoutesFromPoint =
        CompiledQuery.Compile<Database, Guid, Guid, short, IQueryable<ACClassPropertyRelation>>(
            (ctx, targetID, targetPropertyID, connectionType) => from c in ctx.ACClassPropertyRelation
                                                                 where c.TargetACClassID == targetID
                                                                        && c.TargetACClassPropertyID == targetPropertyID
                                                                        && c.ConnectionTypeIndex == connectionType
                                                                 select c
        );

        public static readonly Func<Database, Guid, short, IQueryable<ACClassPropertyRelation>> s_cQry_SourceRoutes =
        CompiledQuery.Compile<Database, Guid, short, IQueryable<ACClassPropertyRelation>>(
            (ctx, sourceID, connectionType) => from c in ctx.ACClassPropertyRelation
                                               where c.SourceACClassID == sourceID
                                                      && c.ConnectionTypeIndex == connectionType
                                               select c
        );

        public static readonly Func<Database, Guid, Guid, short, IQueryable<ACClassPropertyRelation>> s_cQry_SourceRoutesFromPoint =
        CompiledQuery.Compile<Database, Guid, Guid, short, IQueryable<ACClassPropertyRelation>>(
            (ctx, sourceID, sourcePropertyID, connectionType) => from c in ctx.ACClassPropertyRelation
                                                                 where c.SourceACClassID == sourceID
                                                                         && c.SourceACClassPropertyID == sourcePropertyID
                                                                        && c.ConnectionTypeIndex == connectionType
                                                                 select c
        );
        #endregion


        #region Properties

        private static int _DefaultAlternatives = 3;
        public static int DefaultAlternatives
        {
            get
            {
                return _DefaultAlternatives;
            }
            set
            {
                _DefaultAlternatives = value;
            }
        }

        private int _RecalcEdgeWeightAfterDays = 15;
        [ACPropertyInfo(300, "", "en{'Recalculate edges weight after -X- days'}de{'Kantengewicht nach -X- Tagen neu berechnen'}", IsPersistable = true)]
        public int RecalcEdgeWeightAfterDays
        {
            get
            {
                return _RecalcEdgeWeightAfterDays;
            }
            set
            {
                if (value > 0)
                    _RecalcEdgeWeightAfterDays = value;
            }
        }

        [ACPropertyInfo(true, 301, "", "en{'Dump routing data to the log file'}de{'Dump routing data to the log file'}")]
        public bool DumpRoutingData
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 302, "", "en{'Ignore inactive modules'}de{'Ignoriere inaktive Module'}")]
        public bool IgnoreInactiveModules
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 302, "", "en{'Select route depending usage'}de{'Wählen Sie die Route je nach Nutzung aus'}")]
        public bool SelectRouteDependUsage
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 302, "", "en{'Max alternatives on route depending usage'}de{'Maximale Alternativen auf der Route je nach Nutzung'}")]
        public int MaxAlternativesOnRouteDependUsage
        {
            get;
            set;
        }

        private ACPropertyConfigValue<string> _RouteItemModes;
        [ACPropertyConfig("en{'Route item mode config'}de{'Route item mode config'}")]
        public string RouteItemModes
        {
            get => _RouteItemModes.ValueT;
            set
            {
                _RouteItemModes.ValueT = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(SelectRoutesInstance):
                    result = SelectRoutesInstance(acParameter[0] as string[], acParameter[1] as string[], acParameter[2] as ACRoutingParameters);
                    return true;
                case nameof(FindSuccessorsInstance):
                    result = FindSuccessorsInstance(acParameter[0] as string, acParameter[1] as ACRoutingParameters);
                    return true;
                case nameof(FindLastSuccessorsInstance):
                    result = FindLastSuccessorsInstance(acParameter[0] as string, acParameter[1] as ACRoutingParameters);
                    return true;
                case nameof(FindSuccessorsFromPointInstance):
                    result = FindSuccessorsFromPointInstance(acParameter[0] as string, (Guid) acParameter[1], acParameter[2] as ACRoutingParameters);
                    return true;
                case MN_SetPriority:
                    SetPriority(acParameter[0] as Route);
                    return true;
                case MN_IncreasePriorityStepwise:
                    IncreasePriorityStepwise(acParameter[0] as Route);
                    return true;
                case nameof(BuildAvailableRoutes):
                    result = BuildAvailableRoutes(acParameter[0] as string, acParameter[1] as string, acParameter[2] as ACRoutingParameters);
                    return true;
                case nameof(BuildAvailableRoutesFromPoints):
                    result = BuildAvailableRoutesFromPoints(acParameter[0] as string, acParameter[1] as Guid?, acParameter[2] as string, acParameter[3] as Guid?, acParameter[4] as ACRoutingParameters);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        [ACMethodInfo("", "", 300)]
        public IEnumerable<string> GetSelectionRuleQueries()
        {
            string[] result = null;
            using (ACMonitor.Lock(_LockObjectRule))
            {
                result = _RegisteredSelectionQueries.Keys.ToArray();
            }
            return result;
        }

        #region static

        #region Routing common


        public static RoutingResult FindSuccessors(ACClass from, ACRoutingParameters routingParameters)
        {
            if (routingParameters.RoutingService == null)
            {
                Msg msg = null;
                routingParameters.RoutingService = GetRoutingService(from.GetACUrlComponent(), out msg);
            }

            if (routingParameters.RoutingService != null && routingParameters.RoutingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                return MemFindSuccessors(from.GetACUrlComponent(), routingParameters);
            }
            else
            {
                return new RoutingResult(DbSelectRoutes(from, routingParameters), true, null);
            }
        }

        public static RoutingResult FindSuccessors(string fromACUrl, ACRoutingParameters routingParameters)
        {
            if (routingParameters.RoutingService == null)
            {
                Msg msg = null;
                routingParameters.RoutingService = GetRoutingService(fromACUrl, out msg);
            }

            if (routingParameters.RoutingService != null && routingParameters.RoutingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                return MemFindSuccessors(fromACUrl, routingParameters);
            }
            else
            {
                var fromClass = routingParameters.Database.ACClass.FirstOrDefault(c => c.ACURLComponentCached == fromACUrl);
                return new RoutingResult(DbSelectRoutes(fromClass, routingParameters), true, null);
            }
        }

        public static RoutingResult FindSuccessorsFromPoint(ACClass from, ACClassProperty fromPoint, ACRoutingParameters routingParameters)
        {
            if (routingParameters.RoutingService == null)
            {
                Msg msg = null;
                routingParameters.RoutingService = GetRoutingService(from.GetACUrlComponent(), out msg);
            }

            if (routingParameters.RoutingService != null && routingParameters.RoutingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                return MemFindSuccessorsFromPoint(from.GetACUrlComponent(), fromPoint.ACClassPropertyID, routingParameters);
            }
            else
            {
                return new RoutingResult(DbSelectRoutesFromPoint(from, fromPoint, routingParameters), true, null);
            }
        }

        public static RoutingResult SelectRoutes(ACClass from, ACClass to, ACRoutingParameters routingParameters)
        {
            if (routingParameters.RoutingService == null)
            {
                Msg msg = null;
                routingParameters.RoutingService = GetRoutingService(from.GetACUrlComponent(), out msg);
            }

            if (routingParameters.RoutingService != null && routingParameters.RoutingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                //return MemSelectRoutes(attachRouteItemsToContext ? database : null, from.GetACUrlComponent(), to.GetACUrlComponent(), direction, selectionRuleID,
                //                       maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, routingService, forceReattachToDatabaseContext, previousRoute);

                return MemSelectRoutes(from.GetACUrlComponent(), to.GetACUrlComponent(), routingParameters);
            }
            else
            {
                return new RoutingResult(DbSelectRoutes(from, routingParameters), true, null);
            }

        }

        public static RoutingResult SelectRoutes(ACClass from, IEnumerable<string> targets, ACRoutingParameters routingParameters)
        {
            if (routingParameters.RoutingService == null)
            {
                Msg msg = null;
                routingParameters.RoutingService = GetRoutingService(from.GetACUrlComponent(), out msg);
            }

            if (routingParameters.RoutingService != null && routingParameters.RoutingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                //return MemSelectRoutes(attachRouteItemsToContext ? database : null, new string[] { from.GetACUrlComponent() }, targets, direction, selectionRuleID,
                //                       maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, routingService, forceReattachToDatabaseContext, previousRoute);

                return MemSelectRoutes(new string[] { from.GetACUrlComponent() }, targets, routingParameters);
            }
            else
            {
                return new RoutingResult(DbSelectRoutes(from, routingParameters), true, null);
            }
        }

        public static RoutingResult SelectRoutes(ACComponent from, ACComponent to, ACRoutingParameters routingParameters)
        {
            if (routingParameters.RoutingService == null)
            {
                Msg msg = null;
                routingParameters.RoutingService = GetRoutingService(from.GetACUrl(), out msg);
            }

            if (routingParameters.RoutingService != null && routingParameters.RoutingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                //return MemSelectRoutes(attachRouteItemsToContext ? database : null, from.GetACUrl(), to.GetACUrl(), direction, selectionRuleID, maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, routingService, 
                //                       forceReattachToDatabaseContext, previousRoute);

                return MemSelectRoutes(from.GetACUrl(), to.GetACUrl(), routingParameters);
            }
            else
            {
                var fromClass = from.ComponentClass.FromIPlusContext<gip.core.datamodel.ACClass>(routingParameters.Database);
                return new RoutingResult(DbSelectRoutes(fromClass, routingParameters), true, null);
            }

        }

        #endregion

        #region Routing over memory

        public const string DefaultServiceACUrl = "\\Service\\RoutingService";

        public static ACComponent GetRoutingService(string startComponentACUrl, out Msg msg)
        {
            ACComponent startComponent = gip.core.datamodel.Database.Root.ACUrlCommand(startComponentACUrl) as ACComponent;
            if (startComponent == null)
            {
                msg = new Msg() { Message = String.Format("Component for ACUrl {0} not found!", startComponentACUrl) };
                return null;
            }
            return GetRoutingService(startComponent, out msg);
        }

        public static ACComponent GetRoutingService(ACComponent forInstance, out Msg msg)
        {
            if (forInstance == null)
                throw new ArgumentNullException("forInstance");
            string routingServiceACUrl = GetRoutingServiceUrl(forInstance, out msg);
            ACComponent routingService = null;
            if (!String.IsNullOrEmpty(routingServiceACUrl))
                routingService = forInstance.ACUrlCommand(routingServiceACUrl) as ACComponent;

            return routingService;
        }

        public static string GetRoutingServiceUrl(ACComponent forInstance, out Msg msg)
        {
            msg = null;
            if (forInstance == null)
                return null;
            IAppManager appManager = forInstance as IAppManager;
            if (appManager == null)
                appManager = forInstance.FindParentComponent<IAppManager>(c => c is IAppManager);
            if (appManager == null)
            {
                msg = new Msg() { Message = "Application manager is not found" };
                return null;
            }
            string routingServiceACUrl = appManager.RoutingServiceACUrl;
            if (String.IsNullOrEmpty(routingServiceACUrl))
                routingServiceACUrl = DefaultServiceACUrl;
            if (!routingServiceACUrl.StartsWith("\\"))
                routingServiceACUrl = "\\" + routingServiceACUrl;
            return routingServiceACUrl;
        }

        //public static RoutingResult MemFindSuccessors(ACComponent from, ACRoutingParameters routingParameters)
        //{
        //    return MemFindSuccessors(database, from.GetACUrl(), selectionRuleID, direction, maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, forceReattachToDatabaseContext);
        //}

        //public static RoutingResult MemFindSuccessors(string startComponentACUrl, ACRoutingParameters routingParameters)
        //{
        //    Msg msg = null;
        //    ACComponent routingService = GetRoutingService(startComponentACUrl, out msg);
        //    if (routingService == null)
        //        return new RoutingResult(null, false, msg);

        //    return MemFindSuccessors(routingService, database, startComponentACUrl, selectionRuleID, direction, maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, forceReattachToDatabaseContext);
        //}

        public static RoutingResult MemFindSuccessors(ACComponent from, ACRoutingParameters routingParameters)
        {
            return MemFindSuccessors(from.GetACUrl(), routingParameters);
        }

        public static RoutingResult MemFindSuccessors(string startComponentACUrl, ACRoutingParameters routingParameters)
        {
            if (routingParameters.RoutingService == null || routingParameters.RoutingService.ConnectionState == ACObjectConnectionState.DisConnected)
                return new RoutingResult(null, false, new Msg() { Message = "The routing service is unavailable!" });
            
            if (routingParameters.SelectionRuleParams == null)
                routingParameters.SelectionRuleParams = new object[] { };

            var routeResult = routingParameters.RoutingService.ExecuteMethod(nameof(FindSuccessorsInstance), startComponentACUrl, routingParameters) as RoutingResult;
            if (routeResult != null && routeResult.Message != null && routeResult.Message.MessageLevel > eMsgLevel.Warning)
                return routeResult;

            if (routeResult == null || !routeResult.Routes.Any())
                return new RoutingResult(null, false, new Msg() { Message = string.Format("Successors are not found for the component with ACUrl {0}!", startComponentACUrl) });

            if (routingParameters.Database != null)
            {
                foreach (Route item in routeResult.Routes)
                {
                    if (!item.IsAttached || routingParameters.ForceReattachToDatabaseContext)
                        item.AttachTo(routingParameters.Database);
                }
            }

            return routeResult;
        }

        public static RoutingResult MemFindSuccessorsFromPoint(string startComponentACUrl, Guid fromPointACClassPropID, ACRoutingParameters routingParameters)
        {
            if (routingParameters.RoutingService == null || routingParameters.RoutingService.ConnectionState == ACObjectConnectionState.DisConnected)
                return new RoutingResult(null, false, new Msg() { Message = "The routing service is unavailable!" });

            if (routingParameters.SelectionRuleParams == null)
                routingParameters.SelectionRuleParams = new object[] { };

            var routeResult = routingParameters.RoutingService.ExecuteMethod(nameof(FindSuccessorsFromPointInstance), startComponentACUrl, fromPointACClassPropID, routingParameters) as RoutingResult;
            if (routeResult != null && routeResult.Message != null && routeResult.Message.MessageLevel > eMsgLevel.Warning)
                return routeResult;

            if (routeResult == null || !routeResult.Routes.Any())
                return new RoutingResult(null, false, new Msg() { Message = string.Format("Successors are not found for the component with ACUrl {0}!", startComponentACUrl) });

            if (routingParameters.Database != null)
            {
                foreach (Route item in routeResult.Routes)
                {
                    if (!item.IsAttached || routingParameters.ForceReattachToDatabaseContext)
                        item.AttachTo(routingParameters.Database);
                }
            }

            return routeResult;
        }


        //find a routes between start and end components
        public static RoutingResult MemSelectRoutes(string startComponentsACUrl, string endComponentACUrl, ACRoutingParameters routingParameters)
        {
            return MemSelectRoutes(new string[] { startComponentsACUrl }, new string[] { endComponentACUrl }, routingParameters);
        }

        public static RoutingResult MemSelectRoutes(IEnumerable<string> startComponentsACUrl, IEnumerable<string> endComponentsACUrl, ACRoutingParameters routingParameters)
        {
            Msg msg = null;
            if (routingParameters.RoutingService == null)
                routingParameters.RoutingService = GetRoutingService(startComponentsACUrl.FirstOrDefault(), out msg) as ACComponent;

            if (msg != null)
                return new RoutingResult(null, false, msg);

            if (routingParameters.RoutingService == null || routingParameters.RoutingService.ConnectionState == ACObjectConnectionState.DisConnected)
                return new RoutingResult(null, false, new Msg() { Message = "Routing service is unavailable!" });

            var routeResult = routingParameters.RoutingService.ExecuteMethod(nameof(SelectRoutesInstance), startComponentsACUrl.ToArray(), endComponentsACUrl.ToArray(), routingParameters) as RoutingResult;
            if (routeResult == null || (routeResult.Routes == null && routeResult.Message == null))
                return new RoutingResult(null, false, new Msg() { Message = "Routes not found!" });
            else if (routeResult.Routes == null && routeResult.Message != null)
                return routeResult;

            if (routingParameters.Database != null)
            {
                foreach (Route item in routeResult.Routes)
                {
                    if (!item.IsAttached || routingParameters.ForceReattachToDatabaseContext)
                        item.AttachTo(routingParameters.Database);
                }
            }

            return routeResult;
        }

        #endregion

        #region Routing over database

        #region public
        public static IReadOnlyList<Route> DbSelectRoutes(ACClass start, ACRoutingParameters routingParameters)
        {
            if (routingParameters.Database == null)
            {
                throw new ArgumentNullException("database");
            }
            else if (start == null)
            {
                throw new ArgumentNullException("start");
            }
            else
            {
                List<Route> list = new List<Route>();


                using (ACMonitor.Lock(routingParameters.Database.QueryLock_1X000))
                {
                    IEnumerable<ACClassPropertyRelation> query;
                    if (routingParameters.Direction == RouteDirections.Backwards)
                    {
                        if (routingParameters.DBIncludeInternalConnections && start.ACKind == Global.ACKinds.TPAProcessFunction)
                        {
                            // Query internal connections
                            query = s_cQry_TargetRoutes(routingParameters.Database, start.ACClassID, (short)Global.ConnectionTypes.LogicalBridge);

                            if (!query.Any())
                                throw new Exception("Broken internal route in " + start.ACClassID);
                        }
                        else
                        {
                            // Query physical connections
                            query = s_cQry_TargetRoutes(routingParameters.Database, start.ACClassID, (short)Global.ConnectionTypes.ConnectionPhysical);
                        }


                        // Get routes for all start ACClass properites
                        foreach (ACClassPropertyRelation rTarget in query)
                        {
                            list.AddRange(DbSelectUpwardRoutes(routingParameters.Database, 0, new Route(new RouteItem(rTarget)), routingParameters.DBSelector, routingParameters.DBDeSelector, 
                                                               routingParameters.DBIncludeInternalConnections, routingParameters.DBRecursionLimit, routingParameters.DBIgnoreRecursionLoop));
                        }

                        // Reverse routes
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].Items.Reverse();
                        }
                    }
                    else
                    {
                        if (routingParameters.DBIncludeInternalConnections && start.ACKind == Global.ACKinds.TPAProcessFunction)
                        {
                            // Query internal connections
                            query = s_cQry_SourceRoutes(routingParameters.Database, start.ACClassID, (short)Global.ConnectionTypes.LogicalBridge);

                            if (!query.Any())
                                throw new Exception("Broken internal route in " + start.ACClassID);
                        }
                        else
                        {
                            // Query physical connections
                            query = s_cQry_SourceRoutes(routingParameters.Database, start.ACClassID, (short)Global.ConnectionTypes.ConnectionPhysical);
                        }


                        // Get routes for all start ACClass properites
                        foreach (ACClassPropertyRelation rSource in query)
                        {
                            list.AddRange(DbSelectDownwardRoutes(routingParameters.Database, 0, new Route(new RouteItem(rSource)), routingParameters.DBSelector, routingParameters.DBDeSelector,
                                                                 routingParameters.DBIncludeInternalConnections, routingParameters.DBRecursionLimit, routingParameters.DBIgnoreRecursionLoop));
                        }
                    }
                }

                //#if DEBUG
                //                if (list.Count > 0)
                //                {
                //                    StringBuilder builder = new StringBuilder();
                //                    builder.AppendLine(direction.ToString() + " routes" + (includeInternalConnections ? " with included internal connections:" : ":"));
                //                    for (int n = 0; n < list.Count; n++)
                //                    {
                //                        builder.AppendLine(n.ToString() + ": " + String.Join(" >>>> ", list[n]));
                //                    }
                //                    Database.Root.Messages.LogDebug("Route", "SelectRoutes", builder.ToString());
                //                }
                //#endif
                if (routingParameters.AutoDetachFromDBContext)
                    list.ForEach(c => c.DetachEntitesFromDbContext());
                return list;
            }
        }

        public static IReadOnlyList<Route> DbSelectRoutesFromPoint(ACClass start, ACClassProperty startPoint, ACRoutingParameters routingParameters)
        {
            if (routingParameters.Database == null)
            {
                throw new ArgumentNullException("database");
            }
            else if (start == null)
            {
                throw new ArgumentNullException("start");
            }
            else
            {
                List<Route> list = new List<Route>();


                using (ACMonitor.Lock(routingParameters.Database.QueryLock_1X000))
                {
                    IEnumerable<ACClassPropertyRelation> query;
                    if (routingParameters.Direction == RouteDirections.Backwards)
                    {
                        if (routingParameters.DBIncludeInternalConnections && start.ACKind == Global.ACKinds.TPAProcessFunction)
                        {
                            // Query internal connections
                            query = s_cQry_TargetRoutesFromPoint(routingParameters.Database, start.ACClassID, startPoint.ACClassPropertyID, (short)Global.ConnectionTypes.LogicalBridge);

                            if (!query.Any())
                                throw new Exception(String.Format("Broken internal route in {0}, {1}", start.GetACUrlComponent(), start.ACClassID));
                        }
                        else
                        {
                            // Query physical connections
                            query = s_cQry_TargetRoutesFromPoint(routingParameters.Database, start.ACClassID, startPoint.ACClassPropertyID, (short)Global.ConnectionTypes.ConnectionPhysical);
                        }


                        // Get routes for all start ACClass properites
                        foreach (ACClassPropertyRelation rTarget in query)
                        {
                            list.AddRange(DbSelectUpwardRoutes(routingParameters.Database, 0, new Route(new RouteItem(rTarget)), routingParameters.DBSelector, routingParameters.DBDeSelector, routingParameters.DBIncludeInternalConnections,
                                                               routingParameters.DBRecursionLimit, routingParameters.DBIgnoreRecursionLoop));
                        }

                        // Reverse routes
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].Items.Reverse();
                        }
                    }
                    else
                    {
                        if (routingParameters.DBIncludeInternalConnections && start.ACKind == Global.ACKinds.TPAProcessFunction)
                        {
                            // Query internal connections
                            query = s_cQry_SourceRoutesFromPoint(routingParameters.Database, start.ACClassID, startPoint.ACClassPropertyID, (short)Global.ConnectionTypes.LogicalBridge);

                            if (!query.Any())
                                throw new Exception(String.Format("Broken internal route in {0}, {1}, {2}, {3}, {4}", start.ACIdentifier, start.GetACUrlComponent(), start.ACClassID, startPoint.ACIdentifier, startPoint.ACClassPropertyID));
                        }
                        else
                        {
                            // Query physical connections
                            query = s_cQry_SourceRoutesFromPoint(routingParameters.Database, start.ACClassID, startPoint.ACClassPropertyID, (short)Global.ConnectionTypes.ConnectionPhysical);
                        }


                        // Get routes for all start ACClass properites
                        foreach (ACClassPropertyRelation rSource in query)
                        {
                            list.AddRange(DbSelectDownwardRoutes(routingParameters.Database, 0, new Route(new RouteItem(rSource)), routingParameters.DBSelector, routingParameters.DBDeSelector, routingParameters.DBIncludeInternalConnections,
                                                                 routingParameters.DBRecursionLimit, routingParameters.DBIgnoreRecursionLoop));
                        }
                    }
                }

                //#if DEBUG
                //                if (list.Count > 0)
                //                {
                //                    StringBuilder builder = new StringBuilder();
                //                    builder.AppendLine(direction.ToString() + " routes" + (includeInternalConnections ? " with included internal connections:" : ":"));
                //                    for (int n = 0; n < list.Count; n++)
                //                    {
                //                        builder.AppendLine(n.ToString() + ": " + String.Join(" >>>> ", list[n]));
                //                    }
                //                    Database.Root.Messages.LogDebug("Route", "SelectRoutes", builder.ToString());
                //                }
                //#endif
                if (routingParameters.AutoDetachFromDBContext)
                    list.ForEach(c => c.DetachEntitesFromDbContext());
                return list;
            }
        }
        #endregion

        #region Private
        private static IList<Route> DbSelectUpwardRoutes(Database database, int currentDepth,
            Route currentRoute,
            Func<ACClass, ACClassProperty, Route, bool> selector,
            Func<ACClass, ACClassProperty, Route, bool> deSelector,
            bool includeInternalConnections, int recursionLimit, bool ignoreRecursionLoop)
        {
            RouteItem currentItem = currentRoute[currentRoute.Count - 1];

            // Protection from infinite loop
            if (!ignoreRecursionLoop && currentRoute.Count > 1 && currentItem.Source.ACClassID == currentRoute[0].Target.ACClassID)
                return new Route[] { };
            if (selector != null && selector(currentItem.Source, currentItem.SourceProperty, currentRoute))
                return new Route[] { currentRoute };
            if (deSelector != null && deSelector(currentItem.Source, currentItem.SourceProperty, currentRoute))
                return new Route[] { };
            if (recursionLimit > 0 && currentDepth >= recursionLimit)
                return new Route[] { };
            else
            {
                currentDepth++;
                List<Route> list = new List<Route>();
                IEnumerable<ACClassPropertyRelation> query;

                // includeInternalConnections is ignored if currentItem is the last internal connection in process module (source is not and target is ProcessFunction).
                if (includeInternalConnections && !(currentItem.Source.ACKind != Global.ACKinds.TPAProcessFunction && currentItem.Target.ACKind == Global.ACKinds.TPAProcessFunction))
                {
                    // Query internal connections
                    query = s_cQry_TargetRoutesFromPoint(database, currentItem.Source.ACClassID, currentItem.SourceProperty.ACClassPropertyID, (short)Global.ConnectionTypes.LogicalBridge);

                    if (!query.Any())
                    {
                        if (currentItem.Source.ACKind == Global.ACKinds.TPAProcessFunction)
                            throw new Exception("Broken internal route in " + currentItem.Source);
                        else
                            query = s_cQry_TargetRoutes(database, currentItem.Source.ACClassID, (short)Global.ConnectionTypes.ConnectionPhysical);
                    }
                }
                else
                {
                    // Query physical connections
                    query = s_cQry_TargetRoutes(database, currentItem.Source.ACClassID, (short)Global.ConnectionTypes.ConnectionPhysical);
                }


                if (query.Any())
                {
                    Route currentRouteCopy = new Route(currentRoute); // Route copy is needed for branching current route

                    foreach (ACClassPropertyRelation rTarget in query)
                    {
                        // Add relation to current route as new RouteItem
                        currentRoute.Items.Add(new RouteItem(rTarget));

                        // Check current branch for upward routes
                        var sublist = DbSelectUpwardRoutes(database, currentDepth, currentRoute, selector, deSelector, includeInternalConnections, recursionLimit, ignoreRecursionLoop);
                        if (sublist != null && sublist.Any())
                            list.AddRange(sublist);

                        // Reset current route
                        currentRoute = new Route(currentRouteCopy);
                    }
                }
                else if (selector == null)
                {
                    list.Add(currentRoute);
                }

                return list;
            }
        }

        private static IList<Route> DbSelectDownwardRoutes(Database database, int currentDepth, Route currentRoute,
            Func<ACClass, ACClassProperty, Route, bool> selector,
            Func<ACClass, ACClassProperty, Route, bool> deSelector,
            bool includeInternalConnections, int recursionLimit, bool ignoreRecursionLoop)
        {
            RouteItem currentItem = currentRoute[currentRoute.Count - 1];

            // Protection from infinite loop
            if (!ignoreRecursionLoop && currentRoute.Count > 1 && currentItem.Target.ACClassID == currentRoute[0].Source.ACClassID)
                return new Route[] { };
            if (selector != null && selector(currentItem.Target, currentItem.TargetProperty, currentRoute))
                return new Route[] { currentRoute };
            if (deSelector != null && deSelector(currentItem.Target, currentItem.TargetProperty, currentRoute))
                return new Route[] { };
            if (recursionLimit > 0 && currentDepth >= recursionLimit)
                return new Route[] { };
            else
            {
                currentDepth++;
                List<Route> list = new List<Route>();
                IEnumerable<ACClassPropertyRelation> query;

                if (includeInternalConnections && !(currentItem.Target.ACKind != Global.ACKinds.TPAProcessFunction && currentItem.Source.ACKind == Global.ACKinds.TPAProcessFunction))
                {
                    // Query internal connections
                    query = s_cQry_SourceRoutesFromPoint(database, currentItem.Target.ACClassID, currentItem.TargetProperty.ACClassPropertyID, (short)Global.ConnectionTypes.LogicalBridge);

                    if (!query.Any())
                    {
                        if (currentItem.Target.ACKind == Global.ACKinds.TPAProcessFunction)
                            throw new Exception("Broken internal route in " + currentItem.Target);
                        else
                            query = s_cQry_SourceRoutes(database, currentItem.Target.ACClassID, (short)Global.ConnectionTypes.ConnectionPhysical);
                    }
                }
                else
                {
                    // Query physical connections
                    query = s_cQry_SourceRoutes(database, currentItem.Target.ACClassID, (short)Global.ConnectionTypes.ConnectionPhysical);
                }

                if (query.Any())
                {
                    Route currentRouteCopy = new Route(currentRoute); // Route copy is needed for branching current route

                    foreach (ACClassPropertyRelation rSource in query)
                    {
                        // Add relation to current route as new RouteItem
                        currentRoute.Items.Add(new RouteItem(rSource));

                        // Check current branch for downward routes
                        var sublist = DbSelectDownwardRoutes(database, currentDepth, currentRoute, selector, deSelector, includeInternalConnections, recursionLimit, ignoreRecursionLoop);
                        if (sublist != null && sublist.Any())
                            list.AddRange(sublist);

                        // Reset current route
                        currentRoute = new Route(currentRouteCopy);
                    }
                }
                else if (selector == null)
                {
                    list.Add(currentRoute);
                }

                return list;
            }
        }
        #endregion

        #endregion

        #endregion

        #region Instance-Methods
        [ACMethodInfo("", "", 301, true)]
        public Tuple<List<ACRoutingVertex>, PriorityQueue<ST_Node>> BuildAvailableRoutes(string startComponentACUrl, string endComponentACUrl, ACRoutingParameters routingParameters)
        {
            var startComponent = ACUrlCommand(startComponentACUrl) as ACComponent;
            var endComponent = ACUrlCommand(endComponentACUrl) as ACComponent;

            if (startComponent == null || endComponent == null)
                return null;

            return new ACRoutingSession(this).BuildRoutes(new ACRoutingVertex(startComponent), new ACRoutingVertex(endComponent), routingParameters.MaxRouteAlternativesInLoop, routingParameters.IncludeReserved, 
                                                          routingParameters.IncludeAllocated, routingParameters.IsForEditor);
        }

        [ACMethodInfo("", "", 301, true)]
        public Tuple<List<ACRoutingVertex>, PriorityQueue<ST_Node>> BuildAvailableRoutesFromPoints(string startComponentACUrl, Guid? startPointID, string endComponentACUrl, Guid? endPointID, ACRoutingParameters routingParameters)
        {
            var startComponent = ACUrlCommand(startComponentACUrl) as ACComponent;
            var endComponent = ACUrlCommand(endComponentACUrl) as ACComponent;

            if (startComponent == null || endComponent == null)
                return null;

            if (startPointID.HasValue && endPointID.HasValue)
            {
                return new ACRoutingSession(this).BuildRoutes(new ACRoutingVertex(startComponent, startPointID.Value), new ACRoutingVertex(endComponent, endPointID.Value), routingParameters.MaxRouteAlternativesInLoop, routingParameters.IncludeReserved,
                                                          routingParameters.IncludeAllocated, routingParameters.IsForEditor, routingParameters.SelectionRuleID, routingParameters.SelectionRuleParams);
            }
            else if (startPointID.HasValue && !endPointID.HasValue)
            {
                return new ACRoutingSession(this).BuildRoutes(new ACRoutingVertex(startComponent, startPointID.Value), new ACRoutingVertex(endComponent), routingParameters.MaxRouteAlternativesInLoop, routingParameters.IncludeReserved,
                                                          routingParameters.IncludeAllocated, routingParameters.IsForEditor, routingParameters.SelectionRuleID, routingParameters.SelectionRuleParams);
            }
            else if (!startPointID.HasValue && endPointID.HasValue)
            {
                return new ACRoutingSession(this).BuildRoutes(new ACRoutingVertex(startComponent), new ACRoutingVertex(endComponent, endPointID.Value), routingParameters.MaxRouteAlternativesInLoop, routingParameters.IncludeReserved,
                                                          routingParameters.IncludeAllocated, routingParameters.IsForEditor, routingParameters.SelectionRuleID, routingParameters.SelectionRuleParams);
            }

            return new ACRoutingSession(this).BuildRoutes(new ACRoutingVertex(startComponent), new ACRoutingVertex(endComponent), routingParameters.MaxRouteAlternativesInLoop, routingParameters.IncludeReserved,
                                                          routingParameters.IncludeAllocated, routingParameters.IsForEditor, routingParameters.SelectionRuleID, routingParameters.SelectionRuleParams);
        }

        /// <summary>Searches a route from start components to end components.</summary>
        /// <param name="startComponentsACUrl">The ACUrl array of start components.</param>
        /// <param name="endComponentsACUrl">The ACUrl array of end components.</param>
        /// <param name="routeDirection">Determines a sources and targets in route from a start and end components.</param>
        /// <param name="selectionRuleID"></param>
        /// <param name="selectionRuleParams"></param>
        /// <param name="maxRouteAlternatives"></param>
        /// <param name="includeReserved"></param>
        /// <param name="includeAllocated"></param>
        /// <returns>Available routes between start and end components.</returns>
        [ACMethodInfo("", "", 302, true)]
        public RoutingResult SelectRoutesInstance(string[] startComponentsACUrl, string[] endComponentsACUrl, ACRoutingParameters routingParameters)
        {
            Msg msg = CheckRoutingService(startComponentsACUrl, endComponentsACUrl);
            if (msg != null)
                return new RoutingResult(null, false, msg);

            List<ACComponent> startComponents;
            List<ACComponent> endComponents;

            if (routingParameters.Direction == RouteDirections.Backwards)
            {
                startComponents = FindComponent(endComponentsACUrl);
                endComponents = FindComponent(startComponentsACUrl);
                msg = CheckStartEndComponents(endComponentsACUrl, startComponentsACUrl, startComponents == null, endComponents == null);
                if (msg != null)
                    return new RoutingResult(null, false, msg);
            }
            else
            {
                startComponents = FindComponent(startComponentsACUrl);
                endComponents = FindComponent(endComponentsACUrl);
                msg = CheckStartEndComponents(startComponentsACUrl, endComponentsACUrl, startComponents == null, endComponents == null);
                if (msg != null)
                    return new RoutingResult(null, false, msg);
            }

            if (SelectRouteDependUsage)
            {
                if (routingParameters.MaxRouteAlternativesInLoop < MaxAlternativesOnRouteDependUsage)
                    routingParameters.MaxRouteAlternativesInLoop = MaxAlternativesOnRouteDependUsage;
                if (routingParameters.MaxRouteAlternativesInLoop < 2)
                    routingParameters.MaxRouteAlternativesInLoop = 2;
            }

            Tuple<ACRoutingVertex[], ACRoutingVertex[]> routeVertices = CreateRoutingVertices(startComponents, endComponents);
            RoutingResult rResult = new ACRoutingSession(this).FindRoute(routeVertices.Item1, routeVertices.Item2, routingParameters.SelectionRuleID, routingParameters.SelectionRuleParams, routingParameters.MaxRouteAlternativesInLoop, routingParameters.MaxRouteLoopDepth,
                                                                         routingParameters.IncludeReserved, routingParameters.IncludeAllocated, routingParameters.PreviousRoute);

            if (DumpRoutingData && rResult != null && rResult.Message != null && rResult.Message.MessageLevel <= eMsgLevel.Warning)
            {
                Messages.LogMessageMsg(rResult.Message);
            }

            return rResult;
        }

        public RoutingResult SelectRoutesFromPointInstance(string startComponentACUrl, Guid sourcePointID, string[] endComponentsACUrl, ACRoutingParameters routingParameters)
        {
            Msg msg = CheckRoutingService(new string[] { startComponentACUrl }, endComponentsACUrl);
            if (msg != null)
                return new RoutingResult(null, false, msg);

            List<ACComponent> startComponents;
            List<ACComponent> endComponents;

            if (routingParameters.Direction == RouteDirections.Backwards)
            {
                startComponents = FindComponent(endComponentsACUrl);
                endComponents = FindComponent(new string[] { startComponentACUrl });
                msg = CheckStartEndComponents(endComponentsACUrl, new string[] { startComponentACUrl }, startComponents == null, endComponents == null);
                if (msg != null)
                    return new RoutingResult(null, false, msg);
            }
            else
            {
                startComponents = FindComponent(new string[] { startComponentACUrl });
                endComponents = FindComponent(endComponentsACUrl);
                msg = CheckStartEndComponents(new string[] { startComponentACUrl }, endComponentsACUrl, startComponents == null, endComponents == null);
                if (msg != null)
                    return new RoutingResult(null, false, msg);
            }

            Tuple<ACRoutingVertex[], ACRoutingVertex[]> routeVertices = CreateRoutingVerticesFromPoint(startComponents.FirstOrDefault(), sourcePointID, endComponents);
            RoutingResult rResult = new ACRoutingSession(this).FindRoute(routeVertices.Item1, routeVertices.Item2, routingParameters.SelectionRuleID, routingParameters.SelectionRuleParams, routingParameters.MaxRouteAlternativesInLoop, 
                                                                         routingParameters.MaxRouteLoopDepth, routingParameters.IncludeReserved, routingParameters.IncludeAllocated, routingParameters.PreviousRoute);

            if (DumpRoutingData && rResult != null && rResult.Message != null && rResult.Message.MessageLevel <= eMsgLevel.Warning)
            {
                Messages.LogMessageMsg(rResult.Message);
            }

            return rResult;
        }

        /// <summary>Searches routes from the startComponentACUrl to the nearest component which is match to selector parameter.</summary>
        /// <param name="startComponentACUrl">The start component in searching.</param>
        /// <param name="selectionRuleID"></param>
        /// <param name="routeDirection">The search diretion.</param>
        /// <param name="selectionRuleParams"></param>
        /// <param name="maxRouteAlternatives"></param>
        /// <param name="includeReserved"></param>
        /// <param name="includeAllocated"></param>
        /// <param name="resultMode"></param>
        /// <returns>
        ///   <br />
        /// </returns>
        [ACMethodInfo("", "", 304, true)]
        public RoutingResult FindSuccessorsInstance(string startComponentACUrl, ACRoutingParameters routingParameters)
        {
            var startComp = ACUrlCommand(startComponentACUrl) as ACComponent;
            if (startComp == null)
            {
                Msg msg = new Msg(String.Format("Component for ACUrl {0} not found!", startComponentACUrl), this, eMsgLevel.Error, "ACRoutingService", "FindSuccessors(10)", 1128);
                return new RoutingResult(null, false, msg);
            }

            var result = new ACRoutingSession(this).FindSuccessors(new ACRoutingVertex(startComp), routingParameters.SelectionRuleID, routingParameters.Direction, routingParameters.SelectionRuleParams, routingParameters.MaxRouteAlternativesInLoop,
                                                                   routingParameters.MaxRouteLoopDepth, routingParameters.IncludeReserved, routingParameters.IncludeAllocated, routingParameters.ResultMode);
            if (result != null && result.Message != null)
            {
                if (result.Message.MessageLevel > eMsgLevel.Warning)
                    return result;
                else if (DumpRoutingData)
                {
                    Messages.LogMessageMsg(result.Message);
                }
            }
            if (result == null || result.Routes == null || !result.Routes.Any())
            {
                Msg msg = new Msg(String.Format("Successors are not found for the component with ACUrl {0}!", startComponentACUrl), this, eMsgLevel.Error, 
                                  "ACRoutingService", "FindSuccessors(20)", 1139);
                return new RoutingResult(null, false, msg);
            }

            return result;
        }

        [ACMethodInfo("", "", 305)]
        public RoutingResult FindLastSuccessorsInstance(string startComponentACUrl, ACRoutingParameters routingParameters)
        {
            var startComp = ACUrlCommand(startComponentACUrl) as ACComponent;
            if (startComp == null)
            {
                Msg msg = new Msg(String.Format("Component for ACUrl {0} not found!", startComponentACUrl), this, eMsgLevel.Error, "ACRoutingService", "FindLastSuccessors(10)", 1154);
                return new RoutingResult(null, false, msg);
            }

            RoutingResult rResult = new ACRoutingSession(this).FindAvailableComponents(new ACRoutingVertex(startComp), routingParameters.SelectionRuleID, routingParameters.Direction, routingParameters.SelectionRuleParams, 
                                                                                       routingParameters.IncludeReserved, routingParameters.IncludeAllocated);
            
            if (DumpRoutingData && rResult != null && rResult.Message != null && rResult.Message.MessageLevel <= eMsgLevel.Warning)
            {
                Messages.LogMessageMsg(rResult.Message);
            }

            return rResult;
        }

        [ACMethodInfo("", "", 305)]
        public RoutingResult FindSuccessorsFromPointInstance(string startComponentACUrl, Guid startPointACClassPropID, ACRoutingParameters routingParameters)
        {
            var startComp = ACUrlCommand(startComponentACUrl) as ACComponent;
            if (startComp == null)
            {
                Msg msg = new Msg(String.Format("Component for ACUrl {0} not found!", startComponentACUrl), this, eMsgLevel.Error, "ACRoutingService", "FindSuccessorsFromPoint(10)", 1168);
                return new RoutingResult(null, false, msg);
            }

            var result = new ACRoutingSession(this).FindSuccessors(new ACRoutingVertex(startComp, startPointACClassPropID), routingParameters.SelectionRuleID, routingParameters.Direction, routingParameters.SelectionRuleParams, 
                                                                   routingParameters.MaxRouteAlternativesInLoop, routingParameters.MaxRouteLoopDepth, routingParameters.IncludeReserved, routingParameters.IncludeAllocated, routingParameters.ResultMode);
            if (result != null && result.Message != null)
            {
                if (result.Message.MessageLevel > eMsgLevel.Warning)
                    return result;
                else if (DumpRoutingData)
                {
                    Messages.LogMessageMsg(result.Message);
                }
            }

            if (result == null || result.Routes == null || !result.Routes.Any())
            {
                Msg msg = new Msg(String.Format("Successors are not found for the component with ACUrl {0}!", startComponentACUrl), this, eMsgLevel.Error,
                                  "ACRoutingService", "FindSuccessorsFromPoint(20)", 1180);
                return new RoutingResult(null, false, msg);
            }

            return result;
        }

        [ACMethodInfo("", "", 307)]
        public void SetPriority(Route route)
        {
            ThreadPool.QueueUserWorkItem(c => SetPriorityInternal(route));
        }

        private void SetPriorityInternal(Route route)
        {
            if (route == null)
                return;

            using (Database db = new datamodel.Database())
            {
                foreach (RouteItem rItem in route)
                {
                    var relation = db.ContextIPlus.ACClassPropertyRelation.FirstOrDefault(c => c.ACClassPropertyRelationID == rItem.RelationID);

                    if (relation == null && rItem.RouteItemWeight.HasValue)
                        continue;


                    int weight = rItem.RouteItemWeight.Value < 1 ? 1 : rItem.RouteItemWeight.Value;
                    weight = weight > 100 ? 100 : weight;
                    using (ACMonitor.Lock(_LockObject))
                    {
                        PAEdgeInfo edgeInstance;
                        if (_EdgeCache.TryGetValue(relation.ACClassPropertyRelationID, out edgeInstance))
                        {
                            edgeInstance.Edge.Weight = weight;
                            edgeInstance.Edge.IsDeactivated = rItem.IsDeactivated;
                        }
                    }

                    relation.RelationWeight = (short)weight;
                    relation.IsDeactivated = rItem.IsDeactivated;
                    relation.LastManipulationDT = DateTime.Now;
                }
                db.ACSaveChanges();
            }
        }

        [ACMethodInfo("", "", 308)]
        public void OnRouteUsed(Route route)
        {
            if (route == null || !route.Any())
                return;

            Route clonedRoute = route.Clone() as Route;

            ApplicationManager.ApplicationQueue.Add(() => OnRouteUsedInternal(clonedRoute));
        }

        private void OnRouteUsedInternal(Route route)
        {
            if (!Root.Initialized)
                return;

            try
            {
                IEnumerable<Route> routes = Route.SplitRoute(route).ToList();
                IEnumerable<RouteItem> targetItems = route.GetRouteTargets().Distinct(new RouteItemSourceComparer());
                IEnumerable<Guid> targets = targetItems.Select(c => c.SourceGuid);

                using (Database db = new datamodel.Database())
                {
                    bool multipleTargets = targetItems.Count() > 1;
                    Guid tempGroupID = Guid.NewGuid();

                    List<RouteHashItem> hashItems = new List<RouteHashItem>();
                    List<RouteHashItem> hashItemToClear = new List<RouteHashItem>();

                    foreach (RouteItem target in targetItems)
                    {
                        var myRoutes = routes.Where(c => c.GetRouteTarget().SourceGuid == target.SourceGuid).ToList();

                        SafeList<RouteHashItem> routeHash;
                        if (!_UsedRoutesCache.TryGetValue(target.SourceGuid, out routeHash))
                        {
                            routeHash = new SafeList<RouteHashItem>();
                            _UsedRoutesCache.TryAdd(target.SourceGuid, routeHash);
                        }

                        List<int> hashCodes = new List<int>();
                        foreach (Route r in myRoutes)
                            hashCodes.Add(r.GetRouteHash());

                        hashCodes = hashCodes.OrderBy(c => c).ToList();

                        RouteHashItem item = routeHash.FirstOrDefault(c => c.RouteHashCodes.SequenceEqual(hashCodes));
                        ACClassRouteUsage acClassRouteUsage = null;
                        if (item == null)
                        {
                            acClassRouteUsage = ACClassRouteUsage.NewACObject(db);
                            acClassRouteUsage.ACClassID = target.SourceGuid;

                            item = new RouteHashItem(hashCodes);
                            routeHash.Add(item);
                            item.ACClassRouteUsageID = acClassRouteUsage.ACClassRouteUsageID;

                            foreach(int hashCode in hashCodes)
                            {
                                ACClassRouteUsagePos routeUsagePos = ACClassRouteUsagePos.NewACObject(db, acClassRouteUsage);
                                routeUsagePos.HashCode = hashCode;
                            }    
                        }

                        if (acClassRouteUsage == null && item != null)
                            acClassRouteUsage = db.ACClassRouteUsage.FirstOrDefault(c => c.ACClassRouteUsageID == item.ACClassRouteUsageID);

                        hashItems.Add(item);

                        foreach (RouteHashItem rhItem in routeHash)
                        {
                            if (rhItem.RouteHashCodes.SequenceEqual(hashCodes))
                                continue;

                            if (rhItem.UseFactor > 1)
                            {
                                rhItem.UseFactor--;

                                ACClassRouteUsage tempRouteUsage = db.ACClassRouteUsage.FirstOrDefault(c => c.ACClassRouteUsageID == rhItem.ACClassRouteUsageID);
                                if (tempRouteUsage != null)
                                    tempRouteUsage.UseFactor = rhItem.UseFactor;
                            }
                        }

                        if (item.UseFactor < 5)
                            item.UseFactor++;
                        item.LastManipulation = DateTime.Now;
                        acClassRouteUsage.UseFactor = item.UseFactor;

                        IEnumerable<PAEdge> targetSources = (target.TargetACPoint as PAPoint).ConnectionList.Where(c => c.TargetParent == target.TargetACComponent);

                        foreach (var targetSource in targetSources)
                        {
                            if (targets.Contains(targetSource.Relation.SourceACClassID))
                                continue;

                            SafeList<RouteHashItem> targetSourceHashItems;
                            if (_UsedRoutesCache.TryGetValue(targetSource.Relation.SourceACClassID, out targetSourceHashItems))
                            {
                                foreach (RouteHashItem hashItem in targetSourceHashItems)
                                {
                                    if (hashItem.UseFactor > 1)
                                    {
                                        hashItem.UseFactor--;

                                        ACClassRouteUsage tempRouteUsage = db.ACClassRouteUsage.FirstOrDefault(c => c.ACClassRouteUsageID == hashItem.ACClassRouteUsageID);
                                        if (tempRouteUsage != null)
                                            tempRouteUsage.UseFactor = hashItem.UseFactor;
                                    }

                                    hashItemToClear.Add(hashItem);
                                }
                            }
                        }
                    }

                    List<Guid> groupToRemove = new List<Guid>();

                    if (multipleTargets)
                    {
                        List<SafeList<Guid>> tempGroups = hashItems.Select(c => c.RouteUsageGroupID).ToList();

                        Guid existingGroup = Guid.Empty;

                        if (tempGroups.All(c => c != null))
                        {
                            existingGroup = tempGroups.SelectMany(x => x).Distinct()
                                                           .Where(x => tempGroups.Select(y => (y.Contains(x) ? 1 : 0))
                                                           .Sum() == tempGroups.Count).FirstOrDefault();
                        }

                        if (existingGroup == null || existingGroup == Guid.Empty)
                        {
                            foreach (RouteHashItem rhItem in hashItems)
                            {
                                ACClassRouteUsageGroup group = ACClassRouteUsageGroup.NewACObject(db);
                                group.ACClassRouteUsageID = rhItem.ACClassRouteUsageID;
                                group.GroupID = tempGroupID;

                                if (rhItem.RouteUsageGroupID == null)
                                    rhItem.RouteUsageGroupID = new SafeList<Guid>();

                                rhItem.RouteUsageGroupID.Add(tempGroupID);
                            }

                            existingGroup = tempGroupID;
                        }
                    }
                    
                    if (hashItemToClear.Any())
                    {
                        List<SafeList<Guid>> tempGroups = hashItems.Select(c => c.RouteUsageGroupID).ToList();
                        tempGroups.AddRange(hashItemToClear.Select(c => c.RouteUsageGroupID));

                        Guid existingGroup = Guid.Empty;

                        if (tempGroups.All(c => c != null))
                        {
                            existingGroup = tempGroups.SelectMany(x => x).Distinct()
                                                      .Where(x => tempGroups.Select(y => (y.Contains(x) ? 1 : 0))
                                                      .Sum() == tempGroups.Count).FirstOrDefault();
                        }

                        if (existingGroup != null && existingGroup != Guid.Empty)
                        {
                            hashItems.AddRange(hashItemToClear);

                            foreach (RouteHashItem hashItem in hashItems)
                            {
                                hashItem.RouteUsageGroupID.Remove(existingGroup);
                            }

                            ACClassRouteUsageGroup[] routeGroups = db.ACClassRouteUsageGroup.Where(c => c.GroupID == existingGroup).ToArray();

                            foreach (ACClassRouteUsageGroup routeGroup in routeGroups)
                            {
                                db.ACClassRouteUsageGroup.DeleteObject(routeGroup);
                            }
                        }
                    }

                    Msg msg = db.ACSaveChanges();
                    if (msg != null)
                    {
                        Messages.LogMessageMsg(msg);
                    }
                }
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), nameof(OnRouteUsedInternal), e);
            }
        }

        public List<RouteHashItem> GetMostUsedRouteHash(List<Guid> lastItems)
        {
            try
            {
                SafeList<RouteHashItem> hashItems;
                List<RouteHashItem> result = new List<RouteHashItem>();

                foreach (Guid target in lastItems)
                {
                    if (_UsedRoutesCache.TryGetValue(target, out hashItems))
                    {
                        result.Add(hashItems.OrderByDescending(c => c.UseFactor).ThenByDescending(c => c.LastManipulation).FirstOrDefault().Clone() as RouteHashItem);
                    }
                }

                if (result.Count > 1)
                {
                    if (result.All(c => (c.RouteUsageGroupID == null || !c.RouteUsageGroupID.Any())))
                    {
                        result = result.OrderByDescending(c => c.UseFactor).Take(1).ToList();
                    }
                    else
                    {
                        if (! result.SelectMany(c => c.RouteUsageGroupID).GroupBy(c => c).Any(c => c.Count() == result.Count))
                        {
                            result = result.OrderByDescending(c => c.UseFactor).Take(1).ToList();
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), nameof(GetMostUsedRouteHash), e);
                return null;
            }
        }

        [ACMethodInfo("","",9999, true)]
        public void ClearUsedRouteCache(GuidList targets)
        {
            if (targets == null || !targets.Any())
                return;

            foreach (Guid target in targets)
            {
                SafeList<RouteHashItem> tempList;
                if (_UsedRoutesCache.TryRemove(target, out tempList))
                {
                    tempList = null;
                }
            }

            using(Database db = new datamodel.Database())
            {
                List<ACClassRouteUsage> acClassUsageList = db.ACClassRouteUsage.Include(c => c.ACClassRouteUsagePos_ACClassRouteUsage)
                                                                               .Where(c => targets.Contains(c.ACClassID))
                                                                               .ToList();

                foreach(ACClassRouteUsage usage in acClassUsageList)
                {
                    var usagePosList = usage.ACClassRouteUsagePos_ACClassRouteUsage.ToList();

                    foreach (ACClassRouteUsagePos usagePos in usagePosList)
                        usagePos.DeleteACObject(db, false);

                    var usageGroupList = usage.ACClassRouteUsageGroup_ACClassRouteUsage.ToList();

                    foreach (ACClassRouteUsageGroup usageGroup in usageGroupList)
                        usageGroup.DeleteACObject(db, false);

                    usage.DeleteACObject(db, false);
                }

                Msg msg = db.ACSaveChanges();
                if (msg != null)
                    Messages.LogMessageMsg(msg);
            }
        }

        [ACMethodInfo("", "", 309)]
        public void IncreasePriorityStepwise(Route route)
        {
            using (Database db = new datamodel.Database())
            {
                foreach (var item in route)
                {
                    ACClassPropertyRelation relation = db.ContextIPlus.ACClassPropertyRelation
                                                         .FirstOrDefault(c => c.SourceACClassID == item.SourceGuid && c.TargetACClassID == item.TargetGuid
                                                                         /*&& c.SourceACClassPropertyID == item.SourcePropertyGuid && c.TargetACClassPropertyID == item.TargetPropertyGuid*/);
                    if (relation == null)
                        continue;

                    PAEdgeInfo edgeInfo;
                    if (_EdgeCache.TryGetValue(relation.ACClassPropertyRelationID, out edgeInfo))
                    {
                        PAEdgeInfo info;
                        using (ACMonitor.Lock(_LockObject))
                            info = edgeInfo.DecreaseEdgeWeight();

                        relation.RelationWeight = (short)info.Edge.Weight;
                        relation.UseFactor = info.UseFactor;
                        relation.LastManipulationDT = info.LastManipulationTime;
                    }
                }
                db.ACSaveChanges();
            }
        }

        [ACMethodInfo("", "", 310)]
        public Route GetAllocatedAndReserved(Route route)
        {
            if (route != null)
            {
                bool reserved = false, allocated = false;

                foreach (RouteItem rItem in route)
                {
                    PAEdgeInfo edgeInfo = null;
                    PAEdge edge = null;

                    using (ACMonitor.Lock(_LockObject))
                    {
                        if (_EdgeCache.TryGetValue(rItem.RelationID, out edgeInfo))
                            edge = edgeInfo.Edge;
                    }

                    if (edge != null)
                    {
                        var allocatedSource = edge.GetAllocationState(false);
                        var allocatedTarget = edge.GetAllocationState(true);

                        if (allocatedSource.ValueT == 0 && allocatedTarget.ValueT == 0)
                            continue;

                        if (!reserved)
                            reserved = allocatedSource.Bit00_Reserved || allocatedTarget.Bit00_Reserved;

                        if (!allocated)
                            allocated = allocatedSource.Bit01_Allocated || allocatedTarget.Bit01_Allocated;

                        if (reserved && allocated)
                            break;
                    }
                }

                route.HasAnyReserved = reserved;
                route.HasAnyAllocated = allocated;
            }

            return route;
        }

        internal Dictionary<Guid,RouteItemModeEnum> GetRouteItemsMode()
        {
            using (ACMonitor.Lock(LockMemberList_20020))
            {
                return _RouteItemsModeCache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }
        
        public static void ReserveRoute(Route route, bool reserv = true)
        {
            if (route == null)
                return;

            IEnumerable<IACComponent> sources = route.GetSourceComponentsOfRouteSources();
            foreach (IACComponent source in sources)
            {
                IACContainerT<BitAccessForAllocatedByWay> propAllocated = source.GetProperty(nameof(IRoutableModule.AllocatedByWay)) as IACContainerT<BitAccessForAllocatedByWay>;
                if (propAllocated != null)
                    propAllocated.ValueT.Bit00_Reserved = reserv;
            }

            foreach (RouteItem routeItem in route)
            {
                if (routeItem.TargetACComponent == null)
                    continue;

                IACContainerT<BitAccessForAllocatedByWay> propAllocated = routeItem.TargetACComponent.GetProperty(nameof(IRoutableModule.AllocatedByWay)) as IACContainerT<BitAccessForAllocatedByWay>;
                if (propAllocated != null)
                    propAllocated.ValueT.Bit00_Reserved = reserv;
            }
        }

        public static void AddToEdgeCache(PAEdge edge, short useFactor)
        {
            PAEdgeInfo helpInfo;
            using (ACMonitor.Lock(_LockObject))
            {
                if (!_EdgeCache.TryGetValue(edge.RelationID.Value, out helpInfo))
                    _EdgeCache.Add(edge.RelationID.Value, new PAEdgeInfo(edge, useFactor));
            }
        }

        public static void RegisterSelectionQuery(string selectionRuleID, Func<ACRoutingVertex, object[], bool> selector, Func<ACRoutingVertex, object[], bool> deSelector)
        {
            using (ACMonitor.Lock(_LockObjectRule))
            {
                SelectionRule ruleValue;
                if (!_RegisteredSelectionQueries.TryGetValue(selectionRuleID, out ruleValue))
                    _RegisteredSelectionQueries.Add(selectionRuleID, new SelectionRule() { Selector = selector, DeSelector = deSelector });
            }
        }

        public static SelectionRule GetSelectionQuery(string selectionRuleID)
        {
            SelectionRule ruleValue = null;

            if (string.IsNullOrEmpty(selectionRuleID))
                return ruleValue;

            using (ACMonitor.Lock(_LockObjectRule))
            {
                _RegisteredSelectionQueries.TryGetValue(selectionRuleID, out ruleValue);
            }
            return ruleValue;
        }

        protected override void RunJob(DateTime now, DateTime lastRun, DateTime nextRun)
        {
            Task.Run(() => RecalcEdgeWeight());
        }

        #endregion

        #endregion

        #region Private Methods

        private List<ACComponent> FindComponent(IEnumerable<string> componentsACUrl)
        {
            List<ACComponent> components = new List<ACComponent>();
            foreach (string acUrl in componentsACUrl)
            {
                if (string.IsNullOrEmpty(acUrl))
                    continue;

                ACComponent component = ACUrlCommand(acUrl) as ACComponent;
                if (component != null)
                    components.Add(component);
            }
            return components.Any() ? components : null;
        }

        private Tuple<ACRoutingVertex[], ACRoutingVertex[]> CreateRoutingVertices(List<ACComponent> startComponents, List<ACComponent> endComponents)
        {
            int startCompCount = startComponents.Count;
            int endCompCount = endComponents.Count;
            ACRoutingVertex[] startVertices = new ACRoutingVertex[startCompCount];
            ACRoutingVertex[] endVertices = new ACRoutingVertex[endCompCount];

            for (int i = 0; i < startCompCount; i++)
                startVertices[i] = new ACRoutingVertex(startComponents[i]);

            for (int i = 0; i < endCompCount; i++)
                endVertices[i] = new ACRoutingVertex(endComponents[i]);

            return new Tuple<ACRoutingVertex[], ACRoutingVertex[]>(startVertices, endVertices);
        }

        private Tuple<ACRoutingVertex[], ACRoutingVertex[]> CreateRoutingVerticesFromPoint(ACComponent startComponent, Guid startPointACClassProprertyID, List<ACComponent> endComponents)
        {
            int endCompCount = endComponents.Count;
            ACRoutingVertex[] startVertices = new ACRoutingVertex[1];
            ACRoutingVertex[] endVertices = new ACRoutingVertex[endCompCount];

            startVertices[0] = new ACRoutingVertex(startComponent, startPointACClassProprertyID);

            for (int i = 0; i < endCompCount; i++)
                endVertices[i] = new ACRoutingVertex(endComponents[i]);

            return new Tuple<ACRoutingVertex[], ACRoutingVertex[]>(startVertices, endVertices);
        }

        private static Msg CheckRoutingService(string[] startComponentsACUrl, string[] endComponentsACUrl)
        {
            Msg msg = null;
            ACComponent routingService = null;

            //Problem when starting new batch:
            foreach (string startCompACUrl in startComponentsACUrl)
            {
                ACComponent tempService = GetRoutingService(startCompACUrl, out msg) as ACComponent;
                if (msg != null)
                {
                    if (tempService == null)
                    {
                        try
                        {
                            int index = Array.IndexOf(startComponentsACUrl, startCompACUrl);
                            if (index >= 0)
                            {
                                startComponentsACUrl[index] = null;
                                return null;
                            }
                        }
                        catch (Exception e)
                        {
                            msg.Message += e.Message;
                        }
                    }

                    return msg;
                }

                if (tempService == null)
                    return new Msg() { Message = string.Format("RoutingService for {0} not found!", startCompACUrl) };

                if (routingService != null && routingService != tempService)
                    return new Msg() { Message = string.Format("RoutingServices for {0} is different to previous component!", startCompACUrl) };

                routingService = tempService;
            }

            foreach (string endCompACUrl in endComponentsACUrl)
            {
                ACComponent tempService = GetRoutingService(endCompACUrl, out msg) as ACComponent;
                if (msg != null)
                {
                    if (tempService == null)
                    {
                        try
                        {
                            int index = Array.IndexOf(endComponentsACUrl, endCompACUrl);
                            if (index >= 0)
                            {
                                endComponentsACUrl[index] = null;
                                return null;
                            }
                        }
                        catch (Exception e)
                        {
                            msg.Message += e.Message;
                        }
                    }

                    return msg;
                }
                if (tempService == null)
                    return new Msg() { Message = string.Format("RoutingService for {0} not found!", endCompACUrl) };

                if (routingService != null && routingService != tempService)
                    return new Msg() { Message = string.Format("RoutingServices for {0} is different to previous component!", endCompACUrl) };

                routingService = tempService;
            }

            return msg;
        }

        private void RecalcEdgeWeight()
        {
            try
            {
                using (Database db = new datamodel.Database())
                {
                    IEnumerable<KeyValuePair<Guid, PAEdgeInfo>> edgesForRecalc;
                    using (ACMonitor.Lock(_LockObject))
                    {
                        edgesForRecalc = _EdgeCache.Where(c => (DateTime.Now - c.Value.LastManipulationTime) > new TimeSpan(RecalcEdgeWeightAfterDays, 0, 0, 0)).ToArray();
                    }
                    foreach (var item in edgesForRecalc)
                    {
                        var relation = db.ContextIPlus.ACClassPropertyRelation.FirstOrDefault(c => c.ACClassPropertyRelationID == item.Key);
                        if (relation == null)
                            continue;

                        PAEdgeInfo info;
                        using (ACMonitor.Lock(_LockObject))
                            info = item.Value.IncreaseEdgeWeight();

                        relation.RelationWeight = (short)info.Edge.Weight;
                        relation.UseFactor = info.UseFactor;
                        relation.LastManipulationDT = info.LastManipulationTime;
                    }
                    db.ACSaveChanges();
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("ACRoutingService", "RecalcEdgeWeight", msg);
            }
        }

        private void RestoreLastManipulationTime()
        {
            try
            {
                using (Database db = new datamodel.Database())
                {
                    DateTime last = db.ACClassPropertyRelation.Where(c => c.ConnectionTypeIndex == (short)Global.ConnectionTypes.ConnectionPhysical
                                                                        || c.ConnectionTypeIndex == (short)Global.ConnectionTypes.LogicalBridge)
                                                              .Max(c => c.LastManipulationDT);
                    TimeSpan diff = DateTime.Now - last;
                    if (diff.TotalDays > 1)
                        db.udpRestoreLastManipulationDT((int)diff.TotalDays);
                }
            }
            catch (Exception e)
            {
                if ((gip.core.datamodel.Database.Root != null) && (gip.core.datamodel.Database.Root.Messages != null))
                {
                    gip.core.datamodel.Database.Root.Messages.LogException("ACRoutingService", "RestoreLastManipulationTime(1)", e.Message);
                    if (e.InnerException != null)
                        gip.core.datamodel.Database.Root.Messages.LogException("ACRoutingService", "RestoreLastManipulationTime(2)", e.InnerException.Message);
                }
            }
        }

        private Msg CheckStartEndComponents(IEnumerable<string> startComponentsACUrl, IEnumerable<string> endComponentsACUrl, bool isStartNotExist, bool isEndNotExist)
        {
            Msg msg = null;
            if (isStartNotExist && !isEndNotExist)
            {
                msg = new Msg() { Message = string.Format("Can find start components for ACUrl: {0}", string.Join(", ", startComponentsACUrl)) };
            }
            else if (!isStartNotExist && isEndNotExist)
            {
                msg = new Msg() { Message = string.Format("Can find end components for ACUrl: {0}", string.Join(", ", endComponentsACUrl)) };
            }
            else if (isStartNotExist && isEndNotExist)
            {
                msg = new Msg()
                {
                    Message = string.Format("Can find start components for ACUrl: {0} {1} and end components for ACUrl: {2}",
                                                          string.Join(", ", startComponentsACUrl), System.Environment.NewLine, string.Join(", ", endComponentsACUrl))
                };
            }
            return msg; ;
        }

        private void InitRouteUsage()
        {
            using (Database db = new datamodel.Database())
            {
                try
                {

                    List<ACClassRouteUsage> routeUsages = db.ACClassRouteUsage.Include(c => c.ACClassRouteUsagePos_ACClassRouteUsage).ToList();

                    foreach (ACClassRouteUsage routeUsage in routeUsages)
                    {
                        SafeList<RouteHashItem> hashItems;
                        if (!_UsedRoutesCache.TryGetValue(routeUsage.ACClassID, out hashItems))
                        {
                            hashItems = new SafeList<RouteHashItem>();
                            _UsedRoutesCache.TryAdd(routeUsage.ACClassID, hashItems);
                        }

                        RouteHashItem hashItem = new RouteHashItem();
                        hashItem.UseFactor = routeUsage.UseFactor;
                        hashItem.RouteHashCodes = new SafeList<int>(routeUsage.ACClassRouteUsagePos_ACClassRouteUsage.Select(c => c.HashCode));
                        hashItem.LastManipulation = routeUsage.UpdateDate;
                        hashItem.ACClassRouteUsageID = routeUsage.ACClassRouteUsageID;
                        hashItem.RouteUsageGroupID = new SafeList<Guid>(routeUsage.ACClassRouteUsageGroup_ACClassRouteUsage.Select(c => c.GroupID));
                        hashItems.Add(hashItem);
                    }
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), nameof(InitRouteUsage), e);
                }
            }
        }

        private void LoadRouteItemModes()
        {
            string xml = RouteItemModes;

            if (string.IsNullOrEmpty(xml))
                return;

            List<Tuple<Guid, short>> tempList;

            using (StringReader ms = new StringReader(xml))
            using (XmlTextReader xmlReader = new XmlTextReader(ms))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<Tuple<Guid, short>>));
                var modeList = serializer.ReadObject(xmlReader);

                tempList = modeList as List<Tuple<Guid, short>>;
            }

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                _RouteItemsModeCache = new ConcurrentDictionary<Guid, RouteItemModeEnum>(tempList.Select(c => new KeyValuePair<Guid, RouteItemModeEnum>(c.Item1, (RouteItemModeEnum)c.Item2)));
            }
        }

        [ACMethodInfo("","",9999)]
        public void SaveRouteItemModes(string xml)
        {
            ApplicationManager.ApplicationQueue.Add(() =>
            {
                RouteItemModes = xml;
                LoadRouteItemModes();
            });
        }

        #endregion
    }
}
