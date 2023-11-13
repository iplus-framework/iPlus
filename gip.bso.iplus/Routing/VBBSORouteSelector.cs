using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.manager;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace gip.bso.iplus
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Route selection'}de{'Routenauswahl'}", Global.ACKinds.TACBSO, Global.ACStorableTypes.NotStorable, false, true)]
    public class VBBSORouteSelector : ACBSO, IACVBBSORouteSelector
    {
        #region c'tors

        public VBBSORouteSelector(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACPostInit()
        {
            SelectedRouteMode = RouteModeList.FirstOrDefault();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (SelectionManager != null)
                SelectionManager.PropertyChanged -= SelectionManager_PropertyChanged;
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Properties

        private string _RoutingServiceACUrl = "\\Service\\RoutingService";
        [ACPropertyInfo(999, "", "en{'ACUrl of routing service'}de{'ACUrl of routing service'}")]
        public string RoutingServiceACUrl
        {
            get
            {
                return _RoutingServiceACUrl.StartsWith("\\") ? _RoutingServiceACUrl : "\\" + _RoutingServiceACUrl;
            }
            set
            {
                _RoutingServiceACUrl = value;
                OnPropertyChanged("RoutingServiceACUrl");
            }
        }

        private bool _CombineWithBestRoute = false;
        [ACPropertyInfo(401, "", "en{'Best route selection mode'}de{'Bester Routenauswahlmodus'}")]
        public bool CombineWithBestRoute
        {
            get { return _CombineWithBestRoute; }
            set
            {
                _CombineWithBestRoute = value;
                OnPropertyChanged("CombineWithBestRoute");
            }
        }

        private bool _IsInEdgeWeightAdjustmentMode = false;

        [ACPropertyInfo(402)]
        public List<List<ACRoutingPath>> AvailableRoutes
        {
            get;
            set;
        }

        private List<IACObject> _ActiveRouteComponents;
        [ACPropertyInfo(403)]
        public List<IACObject> ActiveRouteComponents
        {
            get { return _ActiveRouteComponents; }
            set
            {
                _ActiveRouteComponents = value;
                OnPropertyChanged("ActiveRouteComponents");
            }
        }

        private List<IACObject> _ActiveRoutePaths;
        [ACPropertyInfo(404)]
        public List<IACObject> ActiveRoutePaths
        {
            get { return _ActiveRoutePaths; }
            set
            {
                _ActiveRoutePaths = value;
                OnPropertyChanged("ActiveRoutePaths");
            }
        }

        private List<ACRoutingPath> _SelectedActiveRoutingPaths;
        private List<ACRoutingPath> SelectedActiveRoutingPaths
        {
            get
            {
                if (_SelectedActiveRoutingPaths == null)
                    _SelectedActiveRoutingPaths = new List<ACRoutingPath>();
                return _SelectedActiveRoutingPaths;
            }
            set
            {
                _SelectedActiveRoutingPaths = value;
            }
        }

        public List<ACRoutingPath> SelectedRoutingPaths
        {
            get;
            set;
        }

        private IEnumerable<string> SourceComponentsList;
        private IEnumerable<string> TargetComponentsList;

        private IEnumerable<Route> _RouteResult;
        public IEnumerable<Route> RouteResult
        {
            get { return _RouteResult; }
        }

        private Global.GraphAction _SelectedGraphAction;
        [ACPropertyInfo(405, "SelectedGraphAction", "en{'SelectedGraphAction'}de{'SelectedGraphAction'}")]
        public Global.GraphAction SelectedGraphAction
        {
            get
            {
                return _SelectedGraphAction;
            }
            set
            {
                _SelectedGraphAction = value;
                OnPropertyChanged("SelectedGraphAction");
            }
        }

        private bool _AllowProcessModuleInRoute = true;
        [ACPropertyInfo(406, "", "en{'Allow process module in route'}de{'Prozessmodul in Route zulassen'}")]
        public bool AllowProcessModuleInRoute
        {
            get => _AllowProcessModuleInRoute;
            set
            {
                _AllowProcessModuleInRoute = value;
                OnPropertyChanged();
            }
        }

        private string SelectionRuleID
        {
            get;
            set;
        }

        private object[] SelectionRuleParams
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
                case "ApplyRoute":
                    ApplyRoute();
                    return true;
                case "Relayout":
                    Relayout();
                    return true;
                case "ReturnToRouteSelector":
                    ReturnToRouteSelector();
                    return true;
                case "SetRoute":
                    SetRoute();
                    return true;
                case "SaveSettings":
                    SaveSettings();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public void ShowAvailableRoutes(IEnumerable<ACClass> startComponents, IEnumerable<ACClass> endComponents, string selectionRuleID = null, object[] selectionRuleParams = null, bool allowProcessModuleInRoute = true)
        {
            _RouteResult = null;
            List<ACClassInfoWithItems> start = new List<ACClassInfoWithItems>();
            List<ACClassInfoWithItems> end = new List<ACClassInfoWithItems>();

            if (startComponents != null && startComponents.Any())
            {
                foreach (ACClass startComp in startComponents.OrderBy(c => c.ACCaption))
                    start.Add(new ACClassInfoWithItems(startComp));
            }

            if (endComponents != null && endComponents.Any())
            {
                foreach (ACClass endComp in endComponents.OrderBy(c => c.ACCaption))
                    end.Add(new ACClassInfoWithItems(endComp));
            }

            StartComponents = start;
            EndComponents = end;

            if (!AllowProcessModuleInRoute)
            {
                SelectionRuleID = selectionRuleID;
                SelectionRuleParams = selectionRuleParams;
            }
            else
            {
                SelectionRuleID = null;
                SelectionRuleParams = null;
            }

            SelectedStartComponent = StartComponents.FirstOrDefault();
            SelectedEndComponent = EndComponents.FirstOrDefault();

            if (_CurrentRouteMode != null && SelectedRouteMode == null)
                SelectedRouteMode = _CurrentRouteMode;

            ShowDialog(this, "Mainlayout");
        }

        public void ShowAvailableRoutes(IEnumerable<Tuple<ACClass,ACClassProperty>> startPoints, IEnumerable<Tuple<ACClass, ACClassProperty>> endPoints, string selectionRuleID = null, object[] selectionRuleParams = null, bool allowProcessModuleInRoute = true)
        {
            _RouteResult = null;
            List<ACClassInfoWithItems> start = new List<ACClassInfoWithItems>();
            List<ACClassInfoWithItems> end = new List<ACClassInfoWithItems>();

            if (startPoints != null && startPoints.Any())
            {
                foreach (Tuple<ACClass, ACClassProperty> startPoint in startPoints.OrderBy(c => c.Item1.ACCaption))
                    start.Add(new ACClassInfoWithItems(startPoint.Item1));
            }

            if (endPoints != null && endPoints.Any())
            {
                foreach (Tuple<ACClass, ACClassProperty> endPoint in endPoints.OrderBy(c => c.Item1.ACCaption))
                    end.Add(new ACClassInfoWithItems(endPoint.Item1));
            }

            StartPoints = startPoints;
            EndPoints = endPoints;

            StartComponents = start;
            EndComponents = end;

            AllowProcessModuleInRoute = allowProcessModuleInRoute;
            SelectionRuleID = selectionRuleID;
            SelectionRuleParams = selectionRuleParams;

            SelectedStartComponent = StartComponents.FirstOrDefault();
            SelectedEndComponent = EndComponents.FirstOrDefault();

            if (_CurrentRouteMode != null && SelectedRouteMode == null)
                SelectedRouteMode = _CurrentRouteMode;

            ShowDialog(this, "Mainlayout");
        }

        public void EditRoutes(Route route, bool isReadOnly, bool includeReserved, bool includeAllocated)
        {
            IEnumerable<Route> splitedRoutes = Route.SplitRoutes(route);

            IEnumerable<string> sourceComponentsList = splitedRoutes.Select(x => x.FirstOrDefault().Source.ACUrlComponent).Distinct();
            IEnumerable<string> targetComponentsList = splitedRoutes.Select(x => x.LastOrDefault().Target.ACUrlComponent).Distinct();

            if (!GetRoutes(sourceComponentsList, targetComponentsList, includeReserved, includeAllocated))
                return;

            List<IACObject> components = new List<IACObject>();
            List<IACObject> routePath = new List<IACObject>();
            SelectedRoutingPaths = new List<ACRoutingPath>();

            foreach (Route r in splitedRoutes)
            {
                IEnumerable<ACRoutingPath> possiblePaths = AvailableRoutes.FirstOrDefault(c =>
                                                           c.FirstOrDefault().FirstOrDefault().SourceParentComponent.ACUrl == r.FirstOrDefault().Source.ACUrlComponent &&
                                                           c.FirstOrDefault().LastOrDefault().TargetParentComponent.ACUrl == r.LastOrDefault().Target.ACUrlComponent);

                ACRoutingPath selectedRoutePath = possiblePaths.FirstOrDefault(c => CompareRoutes(c, r));
                SetActiveRoutes(components, routePath, selectedRoutePath);
            }

            ActiveRouteComponents = components;
            ActiveRoutePaths = routePath;

            ShowRoute(isReadOnly);
        }

        public void ShowRoute(bool isReadOnly = false)
        {
            if (!isReadOnly)
                InitSelectionManger(Const.SelectionManagerCDesign_ClassName);

            CloseTopDialog();
            if (!_IsInEdgeWeightAdjustmentMode)
                ShowDialog(this, "RoutePresenter");
            else
                ShowDialog(this, "EdgeWeightsPresenter");
        }

        private bool CompareRoutes(ACRoutingPath routingPath, Route route)
        {
            if (routingPath.Count != route.Count)
                return false;

            for (int i = 0; i < routingPath.Count; i++)
            {
                PAEdge edge = routingPath[i];
                RouteItem rItem = route[i];
                if (edge.SourceParentComponent.ACUrl != rItem.Source.ACUrlComponent || edge.TargetParentComponent.ACUrl != rItem.Target.ACUrlComponent)
                    return false;
            }
            return true;
        }

        private bool GetRoutes(IEnumerable<string> sourceComponentsList, IEnumerable<string> targetComponentsList, bool includeReserved, bool includeAllocated, bool isForEditor = false, string selectionRuleID = null, object[] selectionRuleParams = null)
        {
            SourceComponentsList = sourceComponentsList;
            TargetComponentsList = targetComponentsList;

            if (SourceComponentsList == null || TargetComponentsList == null || !SourceComponentsList.Any() || !TargetComponentsList.Any())
            {
                Messages.Info(this, "Info50028");
                return false;
            }

            AvailableRoutes = new List<List<ACRoutingPath>>();
            ActiveRoutePaths = new List<IACObject>();
            ActiveRouteComponents = new List<IACObject>();
            SelectedRoutingPaths = new List<ACRoutingPath>();
            SelectedActiveRoutingPaths.Clear();

            var tempStartPoints = StartPoints?.ToList();
            var tempEndPoints = EndPoints?.ToList();

            foreach (string startComp in SourceComponentsList)
            {
                foreach (string endComp in TargetComponentsList)
                {
                    Tuple<ACClass, ACClassProperty> sourcePoint = tempStartPoints?.FirstOrDefault(c => c.Item1.ACUrlComponent == startComp);
                    Tuple<ACClass, ACClassProperty> targetPoint = tempEndPoints?.FirstOrDefault(c => c.Item1.ACUrlComponent == endComp);

                    FindRoutes(startComp, sourcePoint?.Item2?.ACClassPropertyID, endComp, targetPoint?.Item2?.ACClassPropertyID, includeReserved, includeAllocated, isForEditor, selectionRuleID, selectionRuleParams);
                }
            }

            if (!AvailableRoutes.Any())
            {
                Messages.Info(this, "Info50029");
                return false;
            }

            CheckFirstRoutePath();
            SelectActiveRoutes();

            return true;
        }

        private void FindRoutes(string startComponent, Guid? startPointID, string endComponent, Guid? endPointID, bool includeReserved, bool includeAllocated, bool isForEditor = false, string selectionRuleID = null, object[] selectionRuleParams = null)
        {
            if (startComponent == null || endComponent == null)
                return;

            int maxRouteAlt = 1;
            if (MaximumRouteAlternatives > -1 && MaximumRouteAlternatives < 11)
                maxRouteAlt = MaximumRouteAlternatives;

            var buildRouteResult = ACUrlCommand(string.Format("{0}!"+nameof(ACRoutingService.BuildAvailableRoutesFromPoints), RoutingServiceACUrl.StartsWith("\\") ? RoutingServiceACUrl : "\\" + RoutingServiceACUrl),
                                                startComponent, startPointID, endComponent, endPointID, maxRouteAlt, includeReserved, 
                                                includeAllocated, isForEditor, selectionRuleID, selectionRuleParams) as Tuple<List<ACRoutingVertex>, PriorityQueue<ST_Node>>;
            if (buildRouteResult == null)
                return;

            ACRoutingVertex startVertex = buildRouteResult.Item1.FirstOrDefault(c => c.Component.ValueT != null && c.Component.ValueT.ACUrl == startComponent);
            if (startVertex == null)
                return;

            List<ACRoutingPath> availableRoutingPaths = new List<ACRoutingPath>();

            ST_Node temp = buildRouteResult.Item2.Dequeue();
            while (temp != null)
            {
                ACRoutingPath path = ACRoutingSession.RebuildPath(temp.Sidetracks, startVertex.Component.ValueT, buildRouteResult.Item1);
                if (!temp.Sidetracks.Any())
                {
                    path.IsPrimaryRoute = true;
                }
                if (path != null && path.IsValid)
                    availableRoutingPaths.Add(path);
                temp = buildRouteResult.Item2.Dequeue();
            }
            InsertIntoAvailableRoutes(availableRoutingPaths);
        }

        private void InsertIntoAvailableRoutes(List<ACRoutingPath> routingPaths)
        {
            if ((SourceComponentsList.Count() == 1 && TargetComponentsList.Count() == 1) || !AvailableRoutes.Any())
                AvailableRoutes.Add(routingPaths);
            else
            {
                var routes = CheckRoutePath(routingPaths);
                if (routes != null)
                    AvailableRoutes.Add(new List<ACRoutingPath>(routes));
            }
        }

        private IEnumerable<ACRoutingPath> CheckRoutePath(List<ACRoutingPath> routingPaths)
        {
            List<ACRoutingPath> routes = new List<ACRoutingPath>();

            foreach (ACRoutingPath path in routingPaths)
            {
                if (AvailableRoutes.Any(c => c.Any(x => x.AllPoints.Any(k => path.AllPoints.Any(j => j.ACRef.ValueT == k.ACRef.ValueT)))))
                {
                    if (!routes.Contains(path))
                        routes.Add(path);
                }
            }
            return routes.Any() ? routes : null;
        }

        private void CheckFirstRoutePath()
        {
            if (AvailableRoutes.Count < 2)
                return;

            var routingPaths = AvailableRoutes.FirstOrDefault();
            AvailableRoutes.Remove(routingPaths);

            List<ACRoutingPath> routes = new List<ACRoutingPath>();

            foreach (ACRoutingPath path in routingPaths)
            {
                if (AvailableRoutes.Any(c => c.Any(x => x.AllPoints.Any(k => path.AllPoints.Any(j => j.ACRef.ValueT == k.ACRef.ValueT)))))
                {
                    if (!routes.Contains(path))
                        routes.Add(path);
                }
            }
            AvailableRoutes.Insert(0, routes);
        }

        private void ChangeRoute(IEnumerable<IACObject> selectedComponents)
        {
            List<IACObject> components = new List<IACObject>();
            List<IACObject> routePath = new List<IACObject>();
            SelectedRoutingPaths = new List<ACRoutingPath>();
            var tempList = new List<ACRoutingPath>();

            foreach (var component in selectedComponents)
            {
                foreach (var item in AvailableRoutes)
                {
                    if (item.Count > 1)
                    {
                        var paths = item.Where(c => c.AllPoints.Select(x => x.ACRef.ValueT).Contains(component));
                        ACRoutingPath rPath = null;

                        if (CombineWithBestRoute)
                        {
                            rPath = paths.OrderBy(c => c.RouteWeight).FirstOrDefault();
                            SetActiveRoutes(components, routePath, rPath);
                        }
                        else if (paths.Any())
                        {
                            var selected = SelectedActiveRoutingPaths.Where(c => paths.Any(x => x.Start == c.Start && x.End == c.End));
                            if (!selected.Any())
                            {
                                rPath = paths.OrderBy(c => c.RouteWeight).FirstOrDefault();
                                tempList.Add(rPath);
                                SetActiveRoutes(components, routePath, rPath);
                            }
                            else
                            {
                                foreach (var selectedPath in selected.ToList())
                                {
                                    rPath = paths.OrderByDescending(c => selectedPath.AllPoints.Select(x => x.ACRef.ValueT.ACUrl)
                                                 .Intersect(c.AllPoints.Select(k => k.ACRef.ValueT.ACUrl)).Count())
                                                 .ThenBy(t => Math.Abs(t.AllPoints.Count - selectedPath.AllPoints.Count)).FirstOrDefault();
                                    if (!tempList.Contains(rPath))
                                    {
                                        tempList.Add(rPath);
                                        SetActiveRoutes(components, routePath, rPath);
                                    }
                                }
                            }
                        }

                        //if (rPath != null)
                        //    SetActiveRoutes(components, routePath, rPath);
                        //else
                        if (rPath == null)
                        {
                            var selectedPath = item.FirstOrDefault(c => SelectedActiveRoutingPaths.Any(x => x == c));
                            if (selectedPath != null)
                                tempList.Add(selectedPath);
                            SetActiveRoutes(components, routePath, selectedPath != null ? selectedPath : item.OrderBy(k => k.RouteWeight).FirstOrDefault());
                        }
                    }
                    else if (item.Any())
                        SetActiveRoutes(components, routePath, item.FirstOrDefault());
                }
            }

            ActiveRoutePaths = routePath;
            ActiveRouteComponents = components;
            SelectedActiveRoutingPaths = tempList;
        }

        private void ChangeRouteFromEdge(IEnumerable<PAEdge> selectedEdges)
        {
            List<IACObject> components = new List<IACObject>();
            List<IACObject> routePath = new List<IACObject>();
            var tempList = new List<ACRoutingPath>();
            SelectedRoutingPaths = new List<ACRoutingPath>();

            foreach (var selectedEdge in selectedEdges)
            {
                foreach (var item in AvailableRoutes)
                {
                    if (item.Count > 1)
                    {
                        var paths = item.Where(c => c.Any(x => x == selectedEdge));
                        ACRoutingPath rPath = null;

                        if (CombineWithBestRoute)
                            rPath = paths.OrderBy(c => c.RouteWeight).FirstOrDefault();
                        else if (paths.Any())
                        {
                            var selected = SelectedActiveRoutingPaths.Where(c => paths.Any(x => x.Start == c.Start && x.End == c.End));
                            if (selected == null || !selected.Any())
                            {
                                rPath = paths.OrderBy(c => c.RouteWeight).FirstOrDefault();
                                tempList.Add(rPath);
                            }
                            else
                            {
                                foreach (var selectedPath in selected.ToList())
                                {
                                    rPath = paths.OrderByDescending(c => selectedPath.AllPoints.Select(x => x.ACRef.ValueT)
                                                 .Intersect(c.AllPoints.Select(k => k.ACRef.ValueT)).Count())
                                                 .ThenBy(t => Math.Abs(t.AllPoints.Count - selectedPath.AllPoints.Count)).FirstOrDefault();
                                    if (!tempList.Contains(rPath))
                                    {
                                        tempList.Add(rPath);
                                        SetActiveRoutes(components, routePath, rPath, selectedEdge);
                                    }
                                }
                            }
                        }

                        if (rPath != null)
                            SetActiveRoutes(components, routePath, rPath);

                        else
                        {
                            var selectedPath = item.FirstOrDefault(c => SelectedActiveRoutingPaths.Any(x => x == c));
                            if (selectedPath != null)
                                tempList.Add(selectedPath);
                            SetActiveRoutes(components, routePath, selectedPath != null ? selectedPath : item.OrderBy(k => k.RouteWeight).FirstOrDefault());
                        }
                    }
                    else if (item.Any())
                        SetActiveRoutes(components, routePath, item.FirstOrDefault());

                }
            }

            ActiveRoutePaths = routePath;
            ActiveRouteComponents = components;
            SelectedActiveRoutingPaths = tempList;
        }

        private void SetActiveRoutes(List<IACObject> components, List<IACObject> routePath, ACRoutingPath path, PAEdge selectedEdge = null)
        {
            if (path == null)
                return;

            routePath.AddRange(path);
            if (!SelectedRoutingPaths.Contains(path))
                SelectedRoutingPaths.Add(path);
            components.AddRange(path.AllPoints.Where(c => !components.Contains(c.ACRef.ValueT)).Select(c => c.ACRef.ValueT));

            if (_IsInEdgeWeightAdjustmentMode)
            {
                EdgesList = SelectedRoutingPaths.SelectMany(c => c).Distinct().ToList();
                if (selectedEdge != null)
                    SelectedEdge = selectedEdge;
            }
        }

        private void SelectActiveRoutes()
        {
            foreach (var item in AvailableRoutes)
            {
                ACRoutingPath path = null;
                if (_CurrentRouteMode != null && ((short)_CurrentRouteMode.Value) == 1)
                    path = item.OrderBy(c => c.Count).FirstOrDefault();
                else
                    path = item.OrderBy(c => c.RouteWeight).FirstOrDefault();

                SetActiveRoutes(ActiveRouteComponents, ActiveRoutePaths, path);
                SelectedActiveRoutingPaths.Add(path);

                if (_IsInEdgeWeightAdjustmentMode)
                    EdgesList = path.ToList();
            }
        }

        [ACMethodInfo("", "", 401)]
        public IEnumerable<Route> ApplyRoute()
        {
            if (SelectedRoutingPaths == null || !SelectedRoutingPaths.Any())
                return null;

            List<Route> routes = new List<Route>();
            List<RouteItem> tempList = new List<RouteItem>();


            foreach (ACRoutingPath path in SelectedRoutingPaths)
            {
                tempList.Clear();
                foreach (PAEdge edge in path)
                {
                    ACClassPropertyRelation relation = null;
                    if (edge.Relation != null)
                        relation = edge.Relation;
                    else
                        relation = Database.ContextIPlus.ACClassPropertyRelation.FirstOrDefault(c => c.ACClassPropertyRelationID == edge.RelationID.Value);

                    tempList.Add(new RouteItem(relation));
                }
                routes.Add(new Route(tempList));
            }
            _RouteResult = routes;

            ////test 
            //foreach(var item in _RouteResult)
            //{
            //    Task.Run(() => ACUrlCommand(RoutingServiceACUrl + "!SetEdgeWeight", item));
            //}

            CloseTopDialog();
            return routes;
        }

        [ACMethodInfo("", "", 402, true)]
        public void Relayout()
        {
            SelectedGraphAction = Global.GraphAction.Relayout;
        }

        [ACMethodInfo("", "", 403, true)]
        public void ReturnToRouteSelector()
        {
            CloseTopDialog();
        }

        #endregion

        #region SelectionManager

        private ACRef<VBBSOSelectionManager> _SelectionManager;
        public VBBSOSelectionManager SelectionManager
        {
            get
            {
                if (_SelectionManager != null)
                    return _SelectionManager.ValueT;
                return null;
            }
        }

        private bool _IsSelectionManagerInitialized = false;

        private void InitSelectionManger(string acIdentifier)
        {
            if (_IsSelectionManagerInitialized || SelectionManager != null)
                return;

            var childComp = GetChildComponent(acIdentifier) as VBBSOSelectionManager;
            if (childComp == null)
                childComp = StartComponent(acIdentifier, this, null) as VBBSOSelectionManager;

            if (childComp == null)
                return;

            _SelectionManager = new ACRef<VBBSOSelectionManager>(childComp, this);

            SelectionManager.PropertyChanged += SelectionManager_PropertyChanged;

            _IsSelectionManagerInitialized = true;
        }

        private void SelectionManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedVBControl" && SelectionManager.VBControlMultiselect.All(x => !(x.ContextACObject is PAEdge)))
                ChangeRoute(SelectionManager.VBControlMultiselect.Select(c => c.ContextACObject));

            else if (e.PropertyName == "SelectedVBControl" && SelectionManager.VBControlMultiselect.All(x => x.ContextACObject != null && x.ContextACObject is PAEdge))
                ChangeRouteFromEdge(SelectionManager.VBControlMultiselect.Select(c => c.ContextACObject).OfType<PAEdge>());
        }

        #endregion

        #region Components Selector

        private ACClassInfoWithItems _SelectedStartComponent;
        [ACPropertySelected(406, "StartComponent")]
        public ACClassInfoWithItems SelectedStartComponent
        {
            get { return _SelectedStartComponent; }
            set
            {
                _SelectedStartComponent = value;
                OnPropertyChanged("SelectedStartComponent");
            }
        }

        private List<ACClassInfoWithItems> _StartComponents;
        [ACPropertyList(407, "StartComponent")]
        public List<ACClassInfoWithItems> StartComponents
        {
            get { return _StartComponents; }
            set
            {
                _StartComponents = value;
                OnPropertyChanged("StartComponents");
            }
        }

        private IEnumerable<Tuple<ACClass, ACClassProperty>> StartPoints
        {
            get;
            set;
        }

        private ACClassInfoWithItems _SelectedEndComponent;
        [ACPropertySelected(408, "EndComponent")]
        public ACClassInfoWithItems SelectedEndComponent
        {
            get { return _SelectedEndComponent; }
            set
            {

                _SelectedEndComponent = value;
                OnPropertyChanged("SelectedEndComponent");
            }
        }

        private List<ACClassInfoWithItems> _EndComponents;
        [ACPropertyList(409, "EndComponent")]
        public List<ACClassInfoWithItems> EndComponents
        {
            get { return _EndComponents; }
            set
            {
                _EndComponents = value;
                OnPropertyChanged("EndComponents");
            }
        }

        private IEnumerable<Tuple<ACClass, ACClassProperty>> EndPoints
        {
            get;
            set;
        }

        private ACValueItem _CurrentRouteMode;
        private ACValueItem _SelectedRouteMode;
        [ACPropertySelected(410, "RouteMode")]
        public ACValueItem SelectedRouteMode
        {
            get { return _SelectedRouteMode; }
            set
            {
                _SelectedRouteMode = value;
                OnPropertyChanged("SelectedRouteMode");
            }
        }

        private ACValueItemList _RouteModeList;
        [ACPropertyList(411, "RouteMode")]
        public ACValueItemList RouteModeList
        {
            get
            {
                if (_RouteModeList == null)
                {
                    _RouteModeList = new ACValueItemList("");
                    _RouteModeList.Add(new ACValueItem("en{'Shortest route'}de{'Kürzeste Route'}", (short)1, null));
                    _RouteModeList.Add(new ACValueItem("en{'Most used route'}de{'Die meist verwendete Route'}", (short)2, null));
                }
                return _RouteModeList;
            }
        }

        private int _MaximumRouteAlternatives = 1;
        [ACPropertyInfo(412, "", "en{'Maximum route alternatives in loop'}de{'Maximale Routenalternativen in der Schleife'}")]
        public int MaximumRouteAlternatives
        {
            get
            {
                return _MaximumRouteAlternatives;
            }
            set
            {
                _MaximumRouteAlternatives = value;
                OnPropertyChanged("MaximumRouteAlternatives");
            }
        }

        public bool _IsMultipleSourcesTargets;
        [ACPropertyInfo(413, "", "en{'Multiple sources or/and targets mode'}de{'Mehrere Quellen oder/und Ziele Modus'}")]
        public bool IsMultipleSourcesTargets
        {
            get { return _IsMultipleSourcesTargets; }
            set
            {
                if (value)
                {
                    SelectedStartComponent = null;
                    SelectedEndComponent = null;
                    if (StartComponents.Count == 1)
                        StartComponents.FirstOrDefault().IsChecked = true;
                    if (EndComponents.Count == 1)
                        EndComponents.FirstOrDefault().IsChecked = true;
                }
                else
                {
                    SelectedStartComponent = StartComponents.FirstOrDefault();
                    SelectedEndComponent = EndComponents.FirstOrDefault();
                }
                _IsMultipleSourcesTargets = value;
                OnPropertyChanged("IsMultipleSourcesTargets");
            }
        }

        [ACMethodInfo("", "en{'Set route'}de{'Route festlegen'}", 404, true)]
        public void SetRoute()
        {
            IEnumerable<string> start = null;
            IEnumerable<string> end = null;
            if (IsMultipleSourcesTargets)
            {
                start = StartComponents.Where(c => c.IsChecked).Select(x => x.ValueT.ACUrlComponent);
                end = EndComponents.Where(c => c.IsChecked).Select(x => x.ValueT.ACUrlComponent);
            }
            else if (SelectedStartComponent != null && SelectedEndComponent != null)
            {
                start = new string[] { SelectedStartComponent.ValueT.ACUrlComponent };
                end = new string[] { SelectedEndComponent.ValueT.ACUrlComponent };
            }

            if (AllowProcessModuleInRoute)
            {
                SelectionRuleID = null;
                SelectionRuleParams = null;
            }

            _CurrentRouteMode = SelectedRouteMode;
            if (!GetRoutes(start, end, true, true, false, SelectionRuleID, SelectionRuleParams))
                return;

            ShowRoute();
        }

        #endregion

        #region AdjustmentEdgeWeight

        private PAEdge _SelectedEdge;
        [ACPropertySelected(414,"SelectedEdge")]
        public PAEdge SelectedEdge
        {
            get { return _SelectedEdge; }
            set
            {
                _SelectedEdge = value;
                OnPropertyChanged("SelectedEdge");
            }
        }

        private List<PAEdge> _EdgesList;
        [ACPropertyList(415, "SelectedEdge")]
        public List<PAEdge> EdgesList
        {
            get { return _EdgesList; }
            set
            {
                _EdgesList = value;
                OnPropertyChanged("EdgesList");
            }
        }

        public void EditEdgeWeights(IEnumerable<string> startComponentsACUrl, IEnumerable<string> endComponentsACUrl, int maxRouteAlternatives)
        {
            _IsInEdgeWeightAdjustmentMode = true;
            var routeModeTemp = _CurrentRouteMode;
            var maxRAltTemp = MaximumRouteAlternatives;
            MaximumRouteAlternatives = maxRouteAlternatives;
            _CurrentRouteMode = null;
            if (!GetRoutes(startComponentsACUrl, endComponentsACUrl, true,true,true))
                return;
            ShowRoute();
            _IsInEdgeWeightAdjustmentMode = false;
            _CurrentRouteMode = routeModeTemp;
            MaximumRouteAlternatives = maxRAltTemp;
        }

        [ACMethodInfo("", "en{'Save settings'}de{'Einstellungen speichern'}", 405)]
        public void SaveSettings()
        {
            int routeNo = 1;
            List<RouteItem> routeItems = new List<RouteItem>();
            foreach (ACRoutingPath rPath in SelectedActiveRoutingPaths)
            {
                foreach(PAEdge edge in rPath)
                {
                    RouteItem rItem = new RouteItem(edge.Relation, routeNo);
                    rItem.IsDeactivated = edge.IsDeactivated;
                    routeItems.Add(rItem);
                }
                routeNo++;
            }
            Route route = new Route(routeItems);
            ACUrlCommand(RoutingServiceACUrl + "!" + nameof(ACRoutingService.SetPriority), route);
        }

        #endregion
    }
}
