using gip.core.datamodel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using static gip.core.autocomponent.PARole;

namespace gip.core.autocomponent
{
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'RouteDirections'}de{'RouteDirections'}", Global.ACKinds.TACEnum)]
    public enum RouteDirections : int
    {
        Backwards,
        Forwards
    }

    public class SelectionRule
    {
        public Func<ACRoutingVertex, object[], bool> Selector { get; set; }
        public Func<ACRoutingVertex, object[], bool> DeSelector { get; set; }
    }

    [ACSerializeableInfo]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'RoutingResult'}de{'RoutingResult'}", Global.ACKinds.TACSimpleClass)]
    public class RoutingResult
    {
        public RoutingResult(IEnumerable<Route> routes, bool isDbResult, Msg msg, IEnumerable<ACRef<IACComponent>> components = null)
        {
            _Routes = routes;
            _IsDbResult = isDbResult;
            _Msg = msg;
            _Components = components;
        }

        [IgnoreDataMember]
        private IEnumerable<Route> _Routes;
        [DataMember]
        public IEnumerable<Route> Routes
        {
            get
            {
                return _Routes;
            }
            set
            {
                _Routes = value;
            }
        }

        [IgnoreDataMember]
        private IEnumerable<ACRef<IACComponent>> _Components;
        [DataMember]
        public IEnumerable<ACRef<IACComponent>> Components
        {
            get
            {
                return _Components;
            }
            set
            {
                _Components = value;
            }
        }

        [IgnoreDataMember]
        private bool _IsDbResult;
        [DataMember]
        public bool IsDbResult
        {
            get
            {
                return _IsDbResult;
            }
            set
            {
                _IsDbResult = value;
            }
        }

        [IgnoreDataMember]
        private Msg _Msg;
        [DataMember]
        public Msg Message
        {
            get
            {
                return _Msg;
            }
            set
            {
                _Msg = value;
            }
        }
    }

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
            RestoreLastManipulationTime();
            return base.ACInit(startChildMode);
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
        public const string MN_SelectRoutes = "SelectRoutes";
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

        #endregion

        #region Precompiled Queries
        public static readonly Func<Database, Guid, short, IEnumerable<ACClassPropertyRelation>> s_cQry_TargetRoutes =
        EF.CompileQuery<Database, Guid, short, IEnumerable<ACClassPropertyRelation>>(
            (ctx, targetID, connectionType) => from c in ctx.ACClassPropertyRelation
                                               where c.TargetACClassID == targetID
                                                      && c.ConnectionTypeIndex == connectionType
                                               select c
        );

        public static readonly Func<Database, Guid, Guid, short, IEnumerable<ACClassPropertyRelation>> s_cQry_TargetRoutesFromPoint =
        EF.CompileQuery<Database, Guid, Guid, short, IEnumerable<ACClassPropertyRelation>>(
            (ctx, targetID, targetPropertyID, connectionType) => from c in ctx.ACClassPropertyRelation
                                                                 where c.TargetACClassID == targetID
                                                                        && c.TargetACClassPropertyID == targetPropertyID
                                                                        && c.ConnectionTypeIndex == connectionType
                                                                 select c
        );

        public static readonly Func<Database, Guid, short, IEnumerable<ACClassPropertyRelation>> s_cQry_SourceRoutes =
        EF.CompileQuery<Database, Guid, short, IEnumerable<ACClassPropertyRelation>>(
            (ctx, sourceID, connectionType) => from c in ctx.ACClassPropertyRelation
                                               where c.SourceACClassID == sourceID
                                                      && c.ConnectionTypeIndex == connectionType
                                               select c
        );

        public static readonly Func<Database, Guid, Guid, short, IEnumerable<ACClassPropertyRelation>> s_cQry_SourceRoutesFromPoint =
        EF.CompileQuery<Database, Guid, Guid, short, IEnumerable<ACClassPropertyRelation>>(
            (ctx, sourceID, sourcePropertyID, connectionType) => from c in ctx.ACClassPropertyRelation
                                                                 where c.SourceACClassID == sourceID
                                                                         && c.SourceACClassPropertyID == sourcePropertyID
                                                                        && c.ConnectionTypeIndex == connectionType
                                                                 select c
        );
        #endregion


        #region Properties

        private int _RecalcEdgeWeightAfterDays = 15;
        [ACPropertyInfo(300, "", "en{'Recalculate edges weight after -X- days'}de{'Kantengewicht nach -X- Tagen neu berechnen'}")]
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

        #endregion

        #region Methods

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case MN_SelectRoutes:
                    result = SelectRoutes(acParameter[0] as string[], acParameter[1] as string[], (RouteDirections)acParameter[2], acParameter[3] as string, acParameter[4] as object[], (int)acParameter[5], (bool)acParameter[6], (bool)acParameter[7]);
                    return true;
                case MN_FindSuccessors:
                    result = FindSuccessors(acParameter[0] as string, acParameter[1] as string, (RouteDirections)acParameter[2], acParameter[3] as object[], (int)acParameter[4], (bool)acParameter[5], (bool)acParameter[6]);
                    return true;
                case MN_FindLastSuccessors:
                    result = FindLastSuccessors(acParameter[0] as string, acParameter[1] as string, (RouteDirections)acParameter[2], acParameter[3] as object[], (bool)acParameter[4], (bool)acParameter[5]);
                    return true;
                case MN_FindSuccessorsFromPoint:
                    result = FindSuccessorsFromPoint(acParameter[0] as string, (Guid) acParameter[1], acParameter[2] as string, (RouteDirections)acParameter[3], acParameter[4] as object[], (int)acParameter[5], (bool)acParameter[6], (bool)acParameter[7]);
                    return true;
                case MN_SetPriority:
                    SetPriority(acParameter[0] as ACRoutingPath);
                    return true;
                case MN_IncreasePriorityStepwise:
                    IncreasePriorityStepwise(acParameter[0] as Route);
                    return true;
                case MN_BuildAvailableRoutes:
                    result = BuildAvailableRoutes(acParameter[0] as string, acParameter[1] as string, (int)acParameter[2], (bool)acParameter[3], (bool)acParameter[4], (bool)acParameter[5]);
                    return true;
                case nameof(BuildAvailableRoutesFromPoints):
                    result = BuildAvailableRoutesFromPoints(acParameter[0] as string, acParameter[1] as Guid?, acParameter[2] as string, acParameter[3] as Guid?, (int)acParameter[4], (bool)acParameter[5], (bool)acParameter[6], (bool)acParameter[7],
                                                            acParameter[8] as string, acParameter[9] as object[]);
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


        public static RoutingResult FindSuccessors(ACComponent routingService, Database database, bool attachRouteItemsToContext,
            ACClass from, string selectionRuleID, RouteDirections direction, object[] selectionRuleParams,
            Func<ACClass, ACClassProperty, Route, bool> dbSelector,
            Func<ACClass, ACClassProperty, Route, bool> dbDeSelector,
            int maxRouteAlternatives,
            bool includeReserved, bool includeAllocated,
            bool autoDetachFromDBContext = false,
            bool dbIncludeInternalConnections = false,
            int dbRecursionLimit = 0,
            bool dbIgnoreRecursionLoop = false,
            bool forceReattachToDatabaseContext = false)
        {
            if (routingService == null)
            {
                Msg msg = null;
                routingService = GetRoutingService(from.GetACUrlComponent(), out msg);
            }

            if (routingService != null && routingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                return MemFindSuccessors(routingService, attachRouteItemsToContext ? database : null, from.GetACUrlComponent(), selectionRuleID, direction,
                                                    maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, forceReattachToDatabaseContext);
            }
            else
            {
                return new RoutingResult(DbSelectRoutes(database, from,
                                    dbSelector,
                                    dbDeSelector,
                                    direction, dbIncludeInternalConnections, autoDetachFromDBContext, dbRecursionLimit),
                                true, null);
            }
        }

        public static RoutingResult FindSuccessors(ACComponent routingService, Database database, bool attachRouteItemsToContext,
            ACComponent from, string selectionRuleID, RouteDirections direction, object[] selectionRuleParams,
            Func<ACClass, ACClassProperty, Route, bool> dbSelector,
            Func<ACClass, ACClassProperty, Route, bool> dbDeSelector,
            int maxRouteAlternatives,
            bool includeReserved, bool includeAllocated,
            bool autoDetachFromDBContext = false,
            bool dbIncludeInternalConnections = false,
            int dbRecursionLimit = 0,
            bool dbIgnoreRecursionLoop = false,
            bool forceReattachToDatabaseContext = false)
        {
            if (routingService == null)
            {
                Msg msg = null;
                routingService = GetRoutingService(from.GetACUrl(), out msg);
            }

            if (routingService != null && routingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                return MemFindSuccessors(routingService, attachRouteItemsToContext ? database : null, from.GetACUrl(), selectionRuleID, direction, maxRouteAlternatives,
                                         includeReserved, includeAllocated, selectionRuleParams, forceReattachToDatabaseContext);
            }
            else
            {
                var fromClass = from.ComponentClass.FromIPlusContext<gip.core.datamodel.ACClass>(database);
                return new RoutingResult(DbSelectRoutes(database, fromClass,
                                    dbSelector,
                                    dbDeSelector,
                                    direction, dbIncludeInternalConnections, autoDetachFromDBContext, dbRecursionLimit),
                                true, null);
            }
        }

        public static RoutingResult FindSuccessorsFromPoint(ACComponent routingService, Database database, bool attachRouteItemsToContext,
                                                            ACClass from, ACClassProperty fromPoint, string selectionRuleID, RouteDirections direction, object[] selectionRuleParams,
                                                            Func<ACClass, ACClassProperty, Route, bool> dbSelector,
                                                            Func<ACClass, ACClassProperty, Route, bool> dbDeSelector,
                                                            int maxRouteAlternatives,
                                                            bool includeReserved, bool includeAllocated,
                                                            bool autoDetachFromDBContext = false,
                                                            bool dbIncludeInternalConnections = false,
                                                            int dbRecursionLimit = 0,
                                                            bool dbIgnoreRecursionLoop = false,
                                                            bool forceReattachToDatabaseContext = false)
        {
            if (routingService == null)
            {
                Msg msg = null;
                routingService = GetRoutingService(from.GetACUrlComponent(), out msg);
            }

            if (routingService != null && routingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                return MemFindSuccessorsFromPoint(routingService, attachRouteItemsToContext ? database : null, from.GetACUrlComponent(), fromPoint.ACClassPropertyID, selectionRuleID, 
                                                  direction, maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, forceReattachToDatabaseContext);
            }
            else
            {
                return new RoutingResult(DbSelectRoutesFromPoint(database, from, fromPoint,
                                                                 dbSelector,
                                                                 dbDeSelector,
                                                                 direction, dbIncludeInternalConnections, autoDetachFromDBContext, dbRecursionLimit),
                                         true, null);
            }
        }

        public static RoutingResult FindSuccessorsFromPoint(ACComponent routingService, Database database, bool attachRouteItemsToContext,
                                                            ACComponent from, ACClassProperty fromPoint, string selectionRuleID, RouteDirections direction, object[] selectionRuleParams,
                                                            Func<ACClass, ACClassProperty, Route, bool> dbSelector,
                                                            Func<ACClass, ACClassProperty, Route, bool> dbDeSelector,
                                                            int maxRouteAlternatives,
                                                            bool includeReserved, bool includeAllocated,
                                                            bool autoDetachFromDBContext = false,
                                                            bool dbIncludeInternalConnections = false,
                                                            int dbRecursionLimit = 0,
                                                            bool dbIgnoreRecursionLoop = false, 
                                                            bool forceReattachToDatabaseContext = false)
        {
            if (routingService == null)
            {
                Msg msg = null;
                routingService = GetRoutingService(from.GetACUrl(), out msg);
            }

            if (routingService != null && routingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                return MemFindSuccessorsFromPoint(routingService, attachRouteItemsToContext ? database : null, from.GetACUrl(), fromPoint.ACClassPropertyID, selectionRuleID, 
                                                  direction, maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, forceReattachToDatabaseContext);
            }
            else
            {
                var fromClass = from.ComponentClass.FromIPlusContext<gip.core.datamodel.ACClass>(database);
                return new RoutingResult(DbSelectRoutesFromPoint(database, fromClass, fromPoint,
                                                                 dbSelector,
                                                                 dbDeSelector,
                                                                 direction, dbIncludeInternalConnections, autoDetachFromDBContext, dbRecursionLimit),
                                         true, null);
            }
        }

        public static RoutingResult SelectRoutes(ACComponent routingService, Database database, bool attachRouteItemsToContext,
            ACClass from, ACClass to, RouteDirections direction, string selectionRuleID, object[] selectionRuleParams,
            Func<ACClass, ACClassProperty, Route, bool> dbSelector,
            Func<ACClass, ACClassProperty, Route, bool> dbDeSelector,
            int maxRouteAlternatives,
            bool includeReserved, bool includeAllocated,
            bool autoDetachFromDBContext = false,
            bool dbIncludeInternalConnections = false,
            int dbRecursionLimit = 0,
            bool dbIgnoreRecursionLoop = false, 
            bool forceReattachToDatabaseContext = false)
        {
            if (routingService == null)
            {
                Msg msg = null;
                routingService = GetRoutingService(from.GetACUrlComponent(), out msg);
            }

            if (routingService != null && routingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                return MemSelectRoutes(attachRouteItemsToContext ? database : null, from.GetACUrlComponent(), to.GetACUrlComponent(), direction, selectionRuleID,
                                       maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, routingService, forceReattachToDatabaseContext);
            }
            else
            {
                return new RoutingResult(DbSelectRoutes(database, from,
                                    dbSelector,
                                    dbDeSelector,
                                    direction, dbIncludeInternalConnections, autoDetachFromDBContext, dbRecursionLimit),
                                true, null);
            }

        }

        public static RoutingResult SelectRoutes(ACComponent routingService, Database database, bool attachRouteItemsToContext,
            ACClass from, IEnumerable<string> targets, RouteDirections direction, string selectionRuleID, object[] selectionRuleParams,
            Func<ACClass, ACClassProperty, Route, bool> dbSelector,
            Func<ACClass, ACClassProperty, Route, bool> dbDeSelector,
            int maxRouteAlternatives,
            bool includeReserved, bool includeAllocated,
            bool autoDetachFromDBContext = false,
            bool dbIncludeInternalConnections = false,
            int dbRecursionLimit = 0,
            bool dbIgnoreRecursionLoop = false, 
            bool forceReattachToDatabaseContext = false)
        {
            if (routingService == null)
            {
                Msg msg = null;
                routingService = GetRoutingService(from.GetACUrlComponent(), out msg);
            }

            if (routingService != null && routingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                return MemSelectRoutes(attachRouteItemsToContext ? database : null, new string[] { from.GetACUrlComponent() }, targets, direction, selectionRuleID,
                                       maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, routingService, forceReattachToDatabaseContext);
            }
            else
            {
                return new RoutingResult(DbSelectRoutes(database, from,
                                    dbSelector,
                                    dbDeSelector,
                                    direction, dbIncludeInternalConnections, autoDetachFromDBContext, dbRecursionLimit),
                                true, null);
            }

        }

        public static RoutingResult SelectRoutes(ACComponent routingService, Database database, bool attachRouteItemsToContext,
            ACComponent from, ACComponent to, RouteDirections direction, string selectionRuleID, object[] selectionRuleParams,
            Func<ACClass, ACClassProperty, Route, bool> dbSelector,
            Func<ACClass, ACClassProperty, Route, bool> dbDeSelector,
            int maxRouteAlternatives,
            bool includeReserved, bool includeAllocated,
            bool autoDetachFromDBContext = false,
            bool dbIncludeInternalConnections = false,
            int dbRecursionLimit = 0,
            bool dbIgnoreRecursionLoop = false, 
            bool forceReattachToDatabaseContext = false)
        {
            if (routingService == null)
            {
                Msg msg = null;
                routingService = GetRoutingService(from.GetACUrl(), out msg);
            }

            if (routingService != null && routingService.ConnectionState != ACObjectConnectionState.DisConnected)
            {
                return MemSelectRoutes(attachRouteItemsToContext ? database : null, from.GetACUrl(), to.GetACUrl(), direction, selectionRuleID, maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, routingService, forceReattachToDatabaseContext);
            }
            else
            {
                var fromClass = from.ComponentClass.FromIPlusContext<gip.core.datamodel.ACClass>(database);
                return new RoutingResult(DbSelectRoutes(database, fromClass,
                                    dbSelector,
                                    dbDeSelector,
                                    direction, dbIncludeInternalConnections, autoDetachFromDBContext, dbRecursionLimit),
                                true, null);
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

        public static RoutingResult MemFindSuccessors(Database database, ACComponent from, string selectionRuleID, RouteDirections direction, int maxRouteAlternatives,
                                                      bool includeReserved, bool includeAllocated, object[] selectionRuleParams = null, bool forceReattachToDatabaseContext = false)
        {
            return MemFindSuccessors(database, from.GetACUrl(), selectionRuleID, direction, maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, forceReattachToDatabaseContext);
        }

        public static RoutingResult MemFindSuccessors(Database database, string startComponentACUrl, string selectionRuleID, RouteDirections direction, int maxRouteAlternatives,
                                                      bool includeReserved, bool includeAllocated, object[] selectionRuleParams = null, bool forceReattachToDatabaseContext = false)
        {
            Msg msg = null;
            ACComponent routingService = GetRoutingService(startComponentACUrl, out msg);
            if (routingService == null)
                return new RoutingResult(null, false, msg);

            return MemFindSuccessors(routingService, database, startComponentACUrl, selectionRuleID, direction, maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, forceReattachToDatabaseContext);
        }

        public static RoutingResult MemFindSuccessors(ACComponent routingService, Database database, ACComponent from, string selectionRuleID, RouteDirections direction,
                                                           int maxRouteAlternatives, bool includeReserved, bool includeAllocated, object[] selectionRuleParams = null, bool forceReattachToDatabaseContext = false)
        {
            return MemFindSuccessors(routingService, database, from.GetACUrl(), selectionRuleID, direction, maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, forceReattachToDatabaseContext);
        }

        public static RoutingResult MemFindSuccessors(ACComponent routingService, Database database, string startComponentACUrl, string selectionRuleID, RouteDirections direction,
                                                           int maxRouteAlternatives, bool includeReserved, bool includeAllocated, object[] selectionRuleParams = null, bool forceReattachToDatabaseContext = false)
        {
            if (routingService == null || routingService.ConnectionState == ACObjectConnectionState.DisConnected)
                return new RoutingResult(null, false, new Msg() { Message = "The routing service is unavailable!" });
            if (selectionRuleParams == null)
                selectionRuleParams = new object[] { };

            var routeResult = routingService.ExecuteMethod(MN_FindSuccessors, startComponentACUrl, selectionRuleID, direction, selectionRuleParams, maxRouteAlternatives, includeReserved, includeAllocated) as RoutingResult;
            if (routeResult != null && routeResult.Message != null && routeResult.Message.MessageLevel > eMsgLevel.Warning)
                return routeResult;

            if (routeResult == null || !routeResult.Routes.Any())
                return new RoutingResult(null, false, new Msg() { Message = string.Format("Successors are not found for the component with ACUrl {0}!", startComponentACUrl) });

            if (database != null)
            {
                foreach (Route item in routeResult.Routes)
                {
                    if (!item.IsAttached || forceReattachToDatabaseContext)
                        item.AttachTo(database);
                }
            }

            return routeResult;
        }

        public static RoutingResult MemFindSuccessorsFromPoint(ACComponent routingService, Database database, string startComponentACUrl, Guid fromPointACClassPropID, 
                                                               string selectionRuleID, RouteDirections direction, int maxRouteAlternatives, bool includeReserved, bool includeAllocated, 
                                                               object[] selectionRuleParams = null, bool forceReattachToDatabaseContext = false)
        {
            if (routingService == null || routingService.ConnectionState == ACObjectConnectionState.DisConnected)
                return new RoutingResult(null, false, new Msg() { Message = "The routing service is unavailable!" });
            if (selectionRuleParams == null)
                selectionRuleParams = new object[] { };

            var routeResult = routingService.ExecuteMethod(MN_FindSuccessorsFromPoint, startComponentACUrl, fromPointACClassPropID, selectionRuleID, direction, selectionRuleParams, 
                                                           maxRouteAlternatives, includeReserved, includeAllocated) as RoutingResult;
            if (routeResult != null && routeResult.Message != null && routeResult.Message.MessageLevel > eMsgLevel.Warning)
                return routeResult;

            if (routeResult == null || !routeResult.Routes.Any())
                return new RoutingResult(null, false, new Msg() { Message = string.Format("Successors are not found for the component with ACUrl {0}!", startComponentACUrl) });

            if (database != null)
            {
                foreach (Route item in routeResult.Routes)
                {
                    if (!item.IsAttached || forceReattachToDatabaseContext)
                        item.AttachTo(database);
                }
            }

            return routeResult;
        }


        //find a routes between start and end components
        public static RoutingResult MemSelectRoutes(Database database, string startComponentsACUrl, string endComponentACUrl, RouteDirections direction, string selectionRuleID,
                                                         int maxRouteAlternatives, bool includeReserved, bool includeAllocated, object[] selectionRuleParams = null, ACComponent routingService = null, bool forceReattachToDatabaseContext = false)
        {
            return MemSelectRoutes(database, new string[] { startComponentsACUrl }, new string[] { endComponentACUrl }, direction, selectionRuleID, maxRouteAlternatives, includeReserved, includeAllocated, selectionRuleParams, routingService, forceReattachToDatabaseContext);
        }

        public static RoutingResult MemSelectRoutes(Database database, IEnumerable<string> startComponentsACUrl, IEnumerable<string> endComponentsACUrl, RouteDirections direction,
                                                    string selectionRuleID, int maxRouteAlternatives, bool includeReserved, bool includeAllocated, object[] selectionRuleParams = null, ACComponent routingService = null, bool forceReattachToDatabaseContext = false)
        {
            Msg msg = null;
            if (routingService == null)
                routingService = GetRoutingService(startComponentsACUrl.FirstOrDefault(), out msg) as ACComponent;

            if (msg != null)
                return new RoutingResult(null, false, msg);

            if (routingService == null || routingService.ConnectionState == ACObjectConnectionState.DisConnected)
                return new RoutingResult(null, false, new Msg() { Message = "Routing service is unavailable!" });

            var routeResult = routingService.ExecuteMethod(MN_SelectRoutes, startComponentsACUrl.ToArray(), endComponentsACUrl.ToArray(), direction, selectionRuleID, selectionRuleParams, maxRouteAlternatives, includeReserved, includeAllocated) as RoutingResult;
            if (routeResult == null || (routeResult.Routes == null && routeResult.Message == null))
                return new RoutingResult(null, false, new Msg() { Message = "Routes not found!" });
            else if (routeResult.Routes == null && routeResult.Message != null)
                return routeResult;

            if (database != null)
            {
                foreach (Route item in routeResult.Routes)
                {
                    if (!item.IsAttached || forceReattachToDatabaseContext)
                        item.AttachTo(database);
                }
            }

            return routeResult;
        }

        #endregion

        #region Routing over database

        #region public
        public static IReadOnlyList<Route> DbSelectRoutes(Database database, ACClass start,
            Func<ACClass, ACClassProperty, Route, bool> selector,
            Func<ACClass, ACClassProperty, Route, bool> deSelector,
            RouteDirections direction,
            bool includeInternalConnections = false,
            bool autoDetachFromDBContext = false,
            int recursionLimit = 0,
            bool ignoreRecursionLoop = false)
        {
            if (database == null)
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


                using (ACMonitor.Lock(database.QueryLock_1X000))
                {
                    IEnumerable<ACClassPropertyRelation> query;
                    if (direction == RouteDirections.Backwards)
                    {
                        if (includeInternalConnections && start.ACKind == Global.ACKinds.TPAProcessFunction)
                        {
                            // Query internal connections
                            query = s_cQry_TargetRoutes(database, start.ACClassID, (short)Global.ConnectionTypes.LogicalBridge);

                            if (!query.Any())
                                throw new Exception("Broken internal route in " + start.ACClassID);
                        }
                        else
                        {
                            // Query physical connections
                            query = s_cQry_TargetRoutes(database, start.ACClassID, (short)Global.ConnectionTypes.ConnectionPhysical);
                        }


                        // Get routes for all start ACClass properites
                        foreach (ACClassPropertyRelation rTarget in query)
                        {
                            list.AddRange(DbSelectUpwardRoutes(database, 0, new Route(new RouteItem(rTarget)), selector, deSelector, includeInternalConnections, recursionLimit, ignoreRecursionLoop));
                        }

                        // Reverse routes
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].Items.Reverse();
                        }
                    }
                    else
                    {
                        if (includeInternalConnections && start.ACKind == Global.ACKinds.TPAProcessFunction)
                        {
                            // Query internal connections
                            query = s_cQry_SourceRoutes(database, start.ACClassID, (short)Global.ConnectionTypes.LogicalBridge);

                            if (!query.Any())
                                throw new Exception("Broken internal route in " + start.ACClassID);
                        }
                        else
                        {
                            // Query physical connections
                            query = s_cQry_SourceRoutes(database, start.ACClassID, (short)Global.ConnectionTypes.ConnectionPhysical);
                        }


                        // Get routes for all start ACClass properites
                        foreach (ACClassPropertyRelation rSource in query)
                        {
                            list.AddRange(DbSelectDownwardRoutes(database, 0, new Route(new RouteItem(rSource)), selector, deSelector, includeInternalConnections, recursionLimit, ignoreRecursionLoop));
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
                if (autoDetachFromDBContext)
                    list.ForEach(c => c.DetachEntitesFromDbContext());
                return list;
            }
        }

        public static IReadOnlyList<Route> DbSelectRoutesFromPoint(Database database, ACClass start, ACClassProperty startPoint,
            Func<ACClass, ACClassProperty, Route, bool> selector,
            Func<ACClass, ACClassProperty, Route, bool> deSelector,
            RouteDirections direction,
            bool includeInternalConnections = false,
            bool autoDetachFromDBContext = false,
            int recursionLimit = 0,
            bool ignoreRecursionLoop = false)
        {
            if (database == null)
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


                using (ACMonitor.Lock(database.QueryLock_1X000))
                {
                    IEnumerable<ACClassPropertyRelation> query;
                    if (direction == RouteDirections.Backwards)
                    {
                        if (includeInternalConnections && start.ACKind == Global.ACKinds.TPAProcessFunction)
                        {
                            // Query internal connections
                            query = s_cQry_TargetRoutesFromPoint(database, start.ACClassID, startPoint.ACClassPropertyID, (short)Global.ConnectionTypes.LogicalBridge);

                            if (!query.Any())
                                throw new Exception(String.Format("Broken internal route in {0}, {1}", start.GetACUrlComponent(), start.ACClassID));
                        }
                        else
                        {
                            // Query physical connections
                            query = s_cQry_TargetRoutesFromPoint(database, start.ACClassID, startPoint.ACClassPropertyID, (short)Global.ConnectionTypes.ConnectionPhysical);
                        }


                        // Get routes for all start ACClass properites
                        foreach (ACClassPropertyRelation rTarget in query)
                        {
                            list.AddRange(DbSelectUpwardRoutes(database, 0, new Route(new RouteItem(rTarget)), selector, deSelector, includeInternalConnections, recursionLimit, ignoreRecursionLoop));
                        }

                        // Reverse routes
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].Items.Reverse();
                        }
                    }
                    else
                    {
                        if (includeInternalConnections && start.ACKind == Global.ACKinds.TPAProcessFunction)
                        {
                            // Query internal connections
                            query = s_cQry_SourceRoutesFromPoint(database, start.ACClassID, startPoint.ACClassPropertyID, (short)Global.ConnectionTypes.LogicalBridge);

                            if (!query.Any())
                                throw new Exception(String.Format("Broken internal route in {0}, {1}, {2}, {3}, {4}", start.ACIdentifier, start.GetACUrlComponent(), start.ACClassID, startPoint.ACIdentifier, startPoint.ACClassPropertyID));
                        }
                        else
                        {
                            // Query physical connections
                            query = s_cQry_SourceRoutesFromPoint(database, start.ACClassID, startPoint.ACClassPropertyID, (short)Global.ConnectionTypes.ConnectionPhysical);
                        }


                        // Get routes for all start ACClass properites
                        foreach (ACClassPropertyRelation rSource in query)
                        {
                            list.AddRange(DbSelectDownwardRoutes(database, 0, new Route(new RouteItem(rSource)), selector, deSelector, includeInternalConnections, recursionLimit, ignoreRecursionLoop));
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
                if (autoDetachFromDBContext)
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
        public Tuple<List<ACRoutingVertex>, PriorityQueue<ST_Node>> BuildAvailableRoutes(string startComponentACUrl, string endComponentACUrl, int maxRouteAlternatives,
                                                                                         bool includeReserved, bool includeAllocated, bool isForEditor = false)
        {
            var startComponent = ACUrlCommand(startComponentACUrl) as ACComponent;
            var endComponent = ACUrlCommand(endComponentACUrl) as ACComponent;

            if (startComponent == null || endComponent == null)
                return null;

            return new ACRoutingSession(this).BuildRoutes(new ACRoutingVertex(startComponent), new ACRoutingVertex(endComponent), maxRouteAlternatives, includeReserved, 
                                                          includeAllocated, isForEditor);
        }

        [ACMethodInfo("", "", 301, true)]
        public Tuple<List<ACRoutingVertex>, PriorityQueue<ST_Node>> BuildAvailableRoutesFromPoints(string startComponentACUrl, Guid? startPointID, string endComponentACUrl, Guid? endPointID, int maxRouteAlternatives,
                                                                                 bool includeReserved, bool includeAllocated, bool isForEditor = false, string selectionRuleID = null, object[] selectionRuleParams = null)
        {
            var startComponent = ACUrlCommand(startComponentACUrl) as ACComponent;
            var endComponent = ACUrlCommand(endComponentACUrl) as ACComponent;

            if (startComponent == null || endComponent == null)
                return null;

            if (startPointID.HasValue && endPointID.HasValue)
            {
                return new ACRoutingSession(this).BuildRoutes(new ACRoutingVertex(startComponent, startPointID.Value), new ACRoutingVertex(endComponent, endPointID.Value), maxRouteAlternatives, includeReserved,
                                                          includeAllocated, isForEditor, selectionRuleID, selectionRuleParams);
            }
            else if (startPointID.HasValue && !endPointID.HasValue)
            {
                return new ACRoutingSession(this).BuildRoutes(new ACRoutingVertex(startComponent, startPointID.Value), new ACRoutingVertex(endComponent), maxRouteAlternatives, includeReserved,
                                                          includeAllocated, isForEditor, selectionRuleID, selectionRuleParams);
            }
            else if (!startPointID.HasValue && endPointID.HasValue)
            {
                return new ACRoutingSession(this).BuildRoutes(new ACRoutingVertex(startComponent), new ACRoutingVertex(endComponent, endPointID.Value), maxRouteAlternatives, includeReserved,
                                                          includeAllocated, isForEditor, selectionRuleID, selectionRuleParams);
            }

            return new ACRoutingSession(this).BuildRoutes(new ACRoutingVertex(startComponent), new ACRoutingVertex(endComponent), maxRouteAlternatives, includeReserved,
                                                          includeAllocated, isForEditor, selectionRuleID, selectionRuleParams);
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
        public RoutingResult SelectRoutes(string[] startComponentsACUrl, string[] endComponentsACUrl, RouteDirections routeDirection, string selectionRuleID, object[] selectionRuleParams,
                                          int maxRouteAlternatives, bool includeReserved, bool includeAllocated)
        {
            Msg msg = CheckRoutingService(startComponentsACUrl, endComponentsACUrl);
            if (msg != null)
                return new RoutingResult(null, false, msg);

            List<ACComponent> startComponents;
            List<ACComponent> endComponents;

            if (routeDirection == RouteDirections.Backwards)
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

            Tuple<ACRoutingVertex[], ACRoutingVertex[]> routeVertices = CreateRoutingVertices(startComponents, endComponents);
            RoutingResult rResult = new ACRoutingSession(this).FindRoute(routeVertices.Item1, routeVertices.Item2, selectionRuleID, selectionRuleParams, maxRouteAlternatives, 
                                                                         includeReserved, includeAllocated);

            if (DumpRoutingData && rResult != null && rResult.Message != null && rResult.Message.MessageLevel <= eMsgLevel.Warning)
            {
                Messages.LogMessageMsg(rResult.Message);
            }

            return rResult;
        }

        public RoutingResult SelectRoutesFromPoint(string startComponentACUrl, Guid sourcePointID, string[] endComponentsACUrl, RouteDirections routeDirection, string selectionRuleID, 
                                                   object[] selectionRuleParams, int maxRouteAlternatives, bool includeReserved, bool includeAllocated)
        {
            Msg msg = CheckRoutingService(new string[] { startComponentACUrl }, endComponentsACUrl);
            if (msg != null)
                return new RoutingResult(null, false, msg);

            List<ACComponent> startComponents;
            List<ACComponent> endComponents;

            if (routeDirection == RouteDirections.Backwards)
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
            RoutingResult rResult = new ACRoutingSession(this).FindRoute(routeVertices.Item1, routeVertices.Item2, selectionRuleID, selectionRuleParams, maxRouteAlternatives, 
                                                                         includeReserved, includeAllocated);

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
        /// <returns>
        ///   <br />
        /// </returns>
        [ACMethodInfo("", "", 304, true)]
        public RoutingResult FindSuccessors(string startComponentACUrl, string selectionRuleID, RouteDirections routeDirection, object[] selectionRuleParams, int maxRouteAlternatives,
                                            bool includeReserved, bool includeAllocated)
        {
            var startComp = ACUrlCommand(startComponentACUrl) as ACComponent;
            if (startComp == null)
            {
                Msg msg = new Msg(String.Format("Component for ACUrl {0} not found!", startComponentACUrl), this, eMsgLevel.Error, "ACRoutingService", "FindSuccessors(10)", 1128);
                return new RoutingResult(null, false, msg);
            }

            var result = new ACRoutingSession(this).FindSuccessors(new ACRoutingVertex(startComp), selectionRuleID, routeDirection, selectionRuleParams, maxRouteAlternatives,
                                                                   includeReserved, includeAllocated);
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
        public RoutingResult FindLastSuccessors(string startComponentACUrl, string selectionRuleID, RouteDirections direction, object[] selectionRuleParams,
                                                bool includeReserved, bool includeAllocated)
        {
            var startComp = ACUrlCommand(startComponentACUrl) as ACComponent;
            if (startComp == null)
            {
                Msg msg = new Msg(String.Format("Component for ACUrl {0} not found!", startComponentACUrl), this, eMsgLevel.Error, "ACRoutingService", "FindLastSuccessors(10)", 1154);
                return new RoutingResult(null, false, msg);
            }

            RoutingResult rResult = new ACRoutingSession(this).FindAvailableComponents(new ACRoutingVertex(startComp), selectionRuleID, direction, selectionRuleParams, 
                                                                                       includeReserved, includeAllocated);
            
            if (DumpRoutingData && rResult != null && rResult.Message != null && rResult.Message.MessageLevel <= eMsgLevel.Warning)
            {
                Messages.LogMessageMsg(rResult.Message);
            }

            return rResult;
        }

        [ACMethodInfo("", "", 305)]
        public RoutingResult FindSuccessorsFromPoint(string startComponentACUrl, Guid startPointACClassPropID, string selectionRuleID, RouteDirections routeDirection, 
                                                    object[] selectionRuleParams, int maxRouteAlternatives, bool includeReserved, bool includeAllocated)
        {
            var startComp = ACUrlCommand(startComponentACUrl) as ACComponent;
            if (startComp == null)
            {
                Msg msg = new Msg(String.Format("Component for ACUrl {0} not found!", startComponentACUrl), this, eMsgLevel.Error, "ACRoutingService", "FindSuccessorsFromPoint(10)", 1168);
                return new RoutingResult(null, false, msg);
            }

            var result = new ACRoutingSession(this).FindSuccessors(new ACRoutingVertex(startComp, startPointACClassPropID), selectionRuleID, routeDirection, selectionRuleParams, 
                                                                   maxRouteAlternatives, includeReserved, includeAllocated);
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
        public void SetPriority(ACRoutingPath path)
        {
            ThreadPool.QueueUserWorkItem(c => SetPriorityInternal(path));
        }

        private void SetPriorityInternal(ACRoutingPath path)
        {
            using (Database db = new datamodel.Database())
            {
                foreach (PAEdge edge in path)
                {
                    int weight = edge.Weight < 1 ? 1 : edge.Weight;
                    weight = weight > 100 ? 100 : weight;
                    using (ACMonitor.Lock(_LockObject))
                    {
                        PAEdgeInfo edgeInstance;
                        if (_EdgeCache.TryGetValue(edge.RelationID.Value, out edgeInstance))
                        {
                            edgeInstance.Edge.Weight = weight;
                            edgeInstance.Edge.IsDeactivated = edge.IsDeactivated;
                        }
                    }
                    var relation = db.ContextIPlus.ACClassPropertyRelation.FirstOrDefault(c => c.ACClassPropertyRelationID == edge.RelationID.Value);
                    if (relation != null)
                    {
                        relation.RelationWeight = (short)weight;
                        relation.IsDeactivated = edge.IsDeactivated;
                        relation.LastManipulationDT = DateTime.Now;
                    }
                }
                db.ACSaveChanges();
            }
        }

        [ACMethodInfo("", "", 308)]
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

        public static void ReserveRoute(Route route)
        {
            if (route == null)
                return;

            IEnumerable<IACComponent> sources = route.GetSourceComponentsOfRouteSources();
            foreach (IACComponent source in sources)
            {
                IACContainerT<BitAccessForAllocatedByWay> propAllocated = source.GetProperty("AllocatedByWay") as IACContainerT<BitAccessForAllocatedByWay>;
                if (propAllocated != null)
                    propAllocated.ValueT.Bit00_Reserved = true;
            }

            foreach (RouteItem routeItem in route)
            {
                if (routeItem.TargetACComponent == null)
                    continue;

                IACContainerT<BitAccessForAllocatedByWay> propAllocated = routeItem.TargetACComponent.GetProperty("AllocatedByWay") as IACContainerT<BitAccessForAllocatedByWay>;
                if (propAllocated != null)
                    propAllocated.ValueT.Bit00_Reserved = true;
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
                        //db.udpRestoreLastManipulationDT((int)diff.TotalDays);
                        db.Database.ExecuteSql(FormattableStringFactory.Create("[dbo].[udpRestoreLastManipulationDT] @p0", (int)diff.TotalDays));
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

        #endregion
    }
}
