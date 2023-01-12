using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.manager;
using System.Collections.ObjectModel;

namespace gip.bso.iplus
{
    /// <summary>
    /// BSO for the route test
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Route selector'}de{'Route selector'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, false, true)]
    public class BSORouteSelector : ACBSO
    {
        #region c'tors

        public BSORouteSelector(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _SourceComponentsList = new List<ACClassInfoWithItems>();
            _TargetComponentsList = new List<ACClassInfoWithItems>();
            if (!base.ACInit(startChildMode))
                return false;

            _RoutingService = ACRoutingService.ACRefToServiceInstance(this);
            return true;

        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            ACRoutingService.DetachACRefFromServiceInstance(this, _RoutingService);
            _RoutingService = null;
            _ComponentTypeFilter = null;
            _ProjectTypeFilter = null;
            bool result = base.ACDeInit(deleteACClassTask);
            if (result && _BSODatabase != null)
            {
                ACObjectContextManager.DisposeAndRemove(_BSODatabase);
                _BSODatabase = null;
            }
            return result;
        }

        #endregion

        #region DB

        private Database _BSODatabase = null;
        /// <summary>
        /// Overriden: Returns a separate database context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_BSODatabase == null)
                    _BSODatabase = ACObjectContextManager.GetOrCreateContext<Database>(this.GetACUrl());
                return _BSODatabase;
            }
        }

        public Database Db
        {
            get
            {
                return Database as Database;
            }
        }

        #endregion

        #region Properties

        protected ACRef<ACComponent> _RoutingService = null;
        protected ACComponent RoutingService
        {
            get
            {
                if (_RoutingService == null)
                    return null;
                return _RoutingService.ValueT;
            }
        }

        public bool IsRoutingServiceAvailable
        {
            get
            {
                return RoutingService != null && RoutingService.ConnectionState != ACObjectConnectionState.DisConnected;
            }
        }

        //private bool _CombineWithBestRoute = false;
        //[ACPropertyInfo(999, "", "en{'Combine with best route'}de{'Combine with best route'}")]
        //public bool CombineWithBestRoute
        //{
        //    get { return _CombineWithBestRoute; }
        //    set
        //    {
        //        _CombineWithBestRoute = value;
        //        OnPropertyChanged("CombineWithBestRoute");
        //    }
        //}

        private int _MaxRouteAlternatives = 1;
        [ACPropertyInfo(404, "", "en{'Maximum route alternatives in loop'}de{'Maximale Routenalternativen in der Schleife'}")]
        public int MaxRouteAlternatives
        {
            get
            {
                return _MaxRouteAlternatives;
            }
            set
            {
                _MaxRouteAlternatives = value;
                OnPropertyChanged("MaxRouteAlternatives");
            }
        }

        private ACValueItem _SelectedSelectionRuleID;
        [ACPropertySelected(401,"SelectedSelectionRuleID", "en{'Active selection rule query'}de{'Aktive Selektionsregelabfrage'}")]
        public ACValueItem SelectedSelectionRuleID
        {
            get
            {
                return _SelectedSelectionRuleID;
            }
            set
            {
                _SelectedSelectionRuleID = value;
                OnPropertyChanged("SelectedSelectionRuleID");
            }
        }

        private ACValueItemList _SelectionRuleIDList;
        [ACPropertyList(402, "SelectedSelectionRuleID")]
        public ACValueItemList SelectionRuleIDList
        {
            get
            {
                if (_SelectionRuleIDList == null)
                {
                    var result = RoutingService != null ? RoutingService.ExecuteMethod("GetSelectionRuleQueries", null) as IEnumerable<string> : null;
                    if(result != null && result.Any())
                    {
                        _SelectionRuleIDList = new ACValueItemList("SelectedSelectionRuleID");
                        foreach (string rule in result)
                            _SelectionRuleIDList.Add(new ACValueItem(rule, rule, null));
                    }
                }
                return _SelectionRuleIDList;
            }
        }

        private ACValueItem _SelectedRouteDirection;
        [ACPropertySelected(403, "RouteDirection")]
        public ACValueItem SelectedRouteDirection
        {
            get { return _SelectedRouteDirection; }
            set
            {
                _SelectedRouteDirection = value;
                OnPropertyChanged("SelectedRouteDirection");
            }
        }

        private ACValueItemList _RouteDirectionList;
        [ACPropertyList(404, "RouteDirection")]
        public ACValueItemList RouteDirectionList
        {
            get
            {
                if (_RouteDirectionList == null)
                {
                    _RouteDirectionList = new ACValueItemList("");
                    _RouteDirectionList.Add(new ACValueItem("Backwards", RouteDirections.Backwards, null));
                    _RouteDirectionList.Add(new ACValueItem("Forwards", RouteDirections.Forwards, null));
                }
                return _RouteDirectionList;
            }
        }

        #region Properties => Source,Target components

        public string CurrentComponent
        {
            get;
            set;
        }

        private ACClassInfoWithItems _SelectedSourceComponent;
        [ACPropertySelected(405, "SourceComponents")]
        public ACClassInfoWithItems SelectedSourceComponent
        {
            get { return _SelectedSourceComponent; }
            set
            {
                _SelectedSourceComponent = value;
                OnPropertyChanged("SelectedSourceComponent");
            }
        }

        private List<ACClassInfoWithItems> _SourceComponentsList;
        [ACPropertyList(406, "SourceComponents")]
        public List<ACClassInfoWithItems> SourceComponentsList
        {
            get;
            set;
        }

        private ACClassInfoWithItems _SelectedTargetComponent;
        [ACPropertySelected(407, "TargetComponents")]
        public ACClassInfoWithItems SelectedTargetComponent
        {
            get { return  _SelectedTargetComponent; }
            set
            {
                _SelectedTargetComponent = value;
                OnPropertyChanged("SelectedTargetComponent");
            }
        }

        private List<ACClassInfoWithItems> _TargetComponentsList;
        [ACPropertyList(408, "TargetComponents")]
        public List<ACClassInfoWithItems> TargetComponentsList
        {
            get;
            set;
        }

        #endregion

        #endregion

        #region Methods
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "GetAvailableRoutes":
                    GetAvailableRoutes();
                    return true;
                case "OpenRouteSettings":
                    OpenRouteSettings();
                    return true;
                case "TestSelectRoutesFromComponent":
                    TestSelectRoutesFromComponent();
                    return true;
                case "IsEnabledTestSelectRoutesFromComponent":
                    result = IsEnabledTestSelectRoutesFromComponent();
                    return true;
                case "TestSelectRoutes":
                    TestSelectRoutes();
                    return true;
                case "IsEnabledTestSelectRoutes":
                    result = IsEnabledTestSelectRoutes();
                    return true;
                case "TestMethod":
                    TestMethod();
                    return true;
                case "RemoveSource":
                    RemoveSource();
                    return true;
                case "RemoveTarget":
                    RemoveTarget();
                    return true;
                case "SetACClassInfoWithItems":
                    SetACClassInfoWithItems(acParameter[0] as ACClassInfoWithItems);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        //Route oldRoute;

        [ACMethodInfo("", "en{'Test routes'}de{'Test routes'}", 401, true)]
        public void GetAvailableRoutes()
        {
            if (SourceComponentsList == null || TargetComponentsList == null) //message
                return;

            VBBSORouteSelector routeSelector = GetChildComponent("VBBSORouteSelector_Child") as VBBSORouteSelector;
            if (routeSelector == null)
                return;

            routeSelector.ShowAvailableRoutes(SourceComponentsList.Select(c => c.ValueT), TargetComponentsList.Select(c => c.ValueT));

            //if (routeSelector.RouteResult != null && oldRoute != null)
            //{
            //    IACComponent diff = null;
            //    var test = Route.MergeRoutesWithoutDuplicates(routeSelector.RouteResult).TryGetFirstDifferentComponent(oldRoute, out diff);
            //    oldRoute = null;
            //}

            //oldRoute = Route.MergeRoutesWithoutDuplicates(routeSelector.RouteResult);

            //ACMethod method = new ACMethod();
            //ACValueList aCValueList = new ACValueList();
            //ACValue acValue = new ACValue("", Route.MergeRoutesWithoutDuplicates(routeSelector.RouteResult));
            //aCValueList.Add(acValue);
            //method.ParameterValueList = aCValueList;

            //ACComponent child = GetChildComponent("TestWay") as ACComponent;
            //if (child != null)
            //    child.ACUrlCommand("!SendObject", new object[] { method, 10, 10, new object() });

            //ACRoutingService.ReserveRoute(Route.MergeRoutesWithoutDuplicates(routeSelector.RouteResult));

            //if (routeSelector.RouteResult != null)
            //{
            //    string result = "";
            //    foreach (var route in routeSelector.RouteResult)
            //    {
            //        foreach (var routeItem in route.Select(x => x.Source.ACUrlComponent))
            //            result += routeItem + " -> ";
            //        result += route.LastOrDefault().Target.ACUrlComponent + System.Environment.NewLine;
            //    }
            //    Console.WriteLine(result);
            //}
        }

        [ACMethodInfo("", "en{'Open route settings'}de{'Routeneinstellungen öffnen'}", 402, true)]
        public void OpenRouteSettings()
        {
            if (SourceComponentsList == null || TargetComponentsList == null)
                return;

            VBBSORouteSelector routeSelector = GetChildComponent("VBBSORouteSelector_Child") as VBBSORouteSelector;
            if (routeSelector == null)
                return;

            routeSelector.EditEdgeWeights(SourceComponentsList.Select(x => x.ValueT.ACUrlComponent), TargetComponentsList.Select(x => x.ValueT.ACUrlComponent), MaxRouteAlternatives);
        }

        /// <summary>
        /// Called from a WPF-Control inside it's ACAction-Method when a relevant interaction-event as occured (e.g. Drag-And-Drop).
        /// </summary>
        /// <param name="targetVBDataObject">The target object that was involved in the interaction event.</param>
        /// <param name="actionArgs">Information about the type of interaction and the source.</param>
        public override void ACActionToTarget(IACInteractiveObject targetVBDataObject, ACActionArgs actionArgs)
        {
            if (targetVBDataObject is IVBContent)
            {
                switch (targetVBDataObject.VBContent)
                {
                    case "SelectedSourceComponent":
                        ACClassInfoWithItems aCClassInfo = actionArgs.DropObject.ACContentList.OfType<ACClassInfoWithItems>().FirstOrDefault();
                        if (aCClassInfo != null && IsEnabledAddSource(aCClassInfo.ValueT))
                            AddSource(aCClassInfo);
                        break;
                    case "SelectedTargetComponent":
                        ACClassInfoWithItems aCClassInfoT = actionArgs.DropObject.ACContentList.OfType<ACClassInfoWithItems>().FirstOrDefault();
                        if (aCClassInfoT != null && IsEnabledAddTarget(aCClassInfoT.ValueT))
                            AddTarget(aCClassInfoT);
                        break;
                }
            }
            base.ACActionToTarget(targetVBDataObject, actionArgs);
        }

        [ACMethodInfo("","",403)]
        public void TestSelectRoutesFromComponent()
        {
            if (!IsEnabledTestSelectRoutesFromComponent())
                return;

            string ruleID = SelectedSelectionRuleID != null && SelectedSelectionRuleID.Value != null ? SelectedSelectionRuleID.Value.ToString() : "";
            RouteDirections direction = SelectedRouteDirection != null ? (RouteDirections)SelectedRouteDirection.Value : RouteDirections.Forwards;
            var result = ACRoutingService.MemFindSuccessors(RoutingService, Db, SourceComponentsList.FirstOrDefault().ValueT.ACUrlComponent, ruleID, 
                                                            direction, 0, false, false);

            if (result.Message != null)
                Messages.Msg(result.Message);
            else
            {
                string resultMsg = "";
                foreach (var rResult in result.Routes)
                {
                    if (direction == RouteDirections.Backwards)
                        resultMsg += rResult.FirstOrDefault()?.Source.ACUrlComponent + System.Environment.NewLine;
                    else
                        resultMsg += rResult.LastOrDefault()?.Target.ACUrlComponent + System.Environment.NewLine;
                }
                Messages.Info(this, resultMsg, true);
            }
        }

        public bool IsEnabledTestSelectRoutesFromComponent()
        {
            return IsRoutingServiceAvailable;
        }

        [ACMethodInfo("","",404)]
        public void TestSelectRoutes()
        {
            if (!IsEnabledTestSelectRoutes())
                return;
            string ruleID = SelectedSelectionRuleID != null && SelectedSelectionRuleID.Value != null ? SelectedSelectionRuleID.Value.ToString() : "";
            RouteDirections direction = SelectedRouteDirection != null ? (RouteDirections)SelectedRouteDirection.Value : RouteDirections.Forwards;
            var result = ACRoutingService.MemSelectRoutes(Db, SourceComponentsList.Select(x => x.ValueT.ACUrlComponent), TargetComponentsList.Select(x => x.ValueT.ACUrlComponent),
                                                          direction, ruleID, 0, true, true, new object[] { }, RoutingService);

            //ACMethod method = new ACMethod();
            //ACValueList aCValueList = new ACValueList();
            //ACValue acValue = new ACValue("", Route.MergeRoutesWithoutDuplicates(result.Routes));
            //aCValueList.Add(acValue);
            //method.ParameterValueList = aCValueList;

            //ACComponent child = GetChildComponent("TestWay") as ACComponent;
            //if (child != null)
            //    child.ACUrlCommand("!SendObject", new object[] { method, 10, 10, new object() });
        }

        public bool IsEnabledTestSelectRoutes()
        {
            return IsRoutingServiceAvailable;
        }

        [ACMethodInfo("", "", 9999)]
        public void TestMethod()
        {
            //VBBSORouteSelector routeSelector = GetChildComponent("VBBSORouteSelector_Child") as VBBSORouteSelector;
            //if (routeSelector == null)
            //    return;

            //routeSelector.GetAvailableRoutes(SourceComponentsList.Select(c => c.Value.ToString()), TargetComponentsList.Select(x => x.Value.ToString()));

            //if (routeSelector.RouteResult != null)
            //{
            //    var result = routeSelector.RouteResult.FirstOrDefault().GetFirstDifferentRouteItem(routeSelector.RouteResult.ToList()[0]);
            //}
        }

        #region Methods => Source,Target

        public void AddSource(ACClassInfoWithItems source)
        {
            _SourceComponentsList.Add(source);
            SourceComponentsList = null;
            OnPropertyChanged("SourceComponentsList");
            SourceComponentsList = _SourceComponentsList;
            OnPropertyChanged("SourceComponentsList");
        }

        [ACMethodInfo("", "en{'Remove component from source/s'}de{'Remove component from source/s'}", 405, true)]
        public void RemoveSource()
        {
            if (SelectedSourceComponent == null && !_SourceComponentsList.Any(c => c == SelectedSourceComponent))
                return;

            _SourceComponentsList.Remove(SelectedSourceComponent);
            SourceComponentsList = null;
            OnPropertyChanged("SourceComponentsList");
            SourceComponentsList = _SourceComponentsList;
            OnPropertyChanged("SourceComponentsList");
        }

        public bool IsEnabledAddSource(ACClass sourceComponent)
        {
            if (sourceComponent != null && !_SourceComponentsList.Any(c => c.ValueT == sourceComponent))
                return true;

            return false;
        }

        public bool IsEnabledRemoveSource()
        {
            if (_SourceComponentsList.Any(c => c == SelectedSourceComponent))
                return true;
            return false;
        }

        public void AddTarget(ACClassInfoWithItems targetComponent)
        {
            _TargetComponentsList.Add(targetComponent);
            TargetComponentsList = null;
            OnPropertyChanged("TargetComponentsList");
            TargetComponentsList = _TargetComponentsList;
            OnPropertyChanged("TargetComponentsList");
        }

        [ACMethodInfo("", "en{'Remove component from target/s'}de{'Remove component from target/s'}", 406, true)]
        public void RemoveTarget()
        {
            _TargetComponentsList.Remove(SelectedTargetComponent);
            TargetComponentsList = null;
            OnPropertyChanged("TargetComponentsList");
            TargetComponentsList = _TargetComponentsList;
            OnPropertyChanged("TargetComponentsList");
        }

        public bool IsEnabledAddTarget(ACClass targetComponent)
        {
            if (targetComponent != null && !_TargetComponentsList.Any(c => c.ValueT == targetComponent))
                return true;

            return false;
        }

        public bool IsEnabledRemoveTarget()
        {
            if (_TargetComponentsList.Any(c => c == SelectedTargetComponent))
                return true;
            return false;
        }

        #endregion

        #endregion

        #region ComponentSelector

        private ACClassInfoWithItems.VisibilityFilters _ComponentTypeFilter;
        [ACPropertyInfo(999)]
        public ACClassInfoWithItems.VisibilityFilters ComponentTypeFilter
        {
            get
            {
                if (_ComponentTypeFilter == null)
                    _ComponentTypeFilter = new ACClassInfoWithItems.VisibilityFilters() { IncludeTypes = new List<Type> { typeof(PAClassPhysicalBase) } };
                return _ComponentTypeFilter;
            }
        }

        private List<Global.ACProjectTypes> _ProjectTypeFilter;
        [ACPropertyInfo(999)]
        public List<Global.ACProjectTypes> ProjectTypeFilter
        {
            get
            {
                return _ProjectTypeFilter != null ? _ProjectTypeFilter : _ProjectTypeFilter = new List<Global.ACProjectTypes>() { Global.ACProjectTypes.Application };
            }
        }

        [ACMethodInfo("", "", 999)]
        public void SetACClassInfoWithItems(ACClassInfoWithItems aCClassInfoWithItems)
        {
            CurrentComponent = aCClassInfoWithItems.ValueT.ACUrlComponent;
        }

        #endregion

    }
}
