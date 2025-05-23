﻿using System;
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
using gip.core.processapplication;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.EntityFrameworkCore;

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
            bool result = base.ACInit(startChildMode);

            _ConfigIncludeReserved = new ACPropertyConfigValue<bool>(this, nameof(ConfigIncludeReserved), false);
            _ConfigIncludeAllocated = new ACPropertyConfigValue<bool>(this, nameof(ConfigIncludeAllocated), false);
            _ConfigRouteMode = new ACPropertyConfigValue<short>(this, nameof(ConfigRouteMode), 1);
            _MaxRouteAltInLoop = new ACPropertyConfigValue<int>(this, nameof(MaxRouteAltInLoop), 1);

            _IncludeReserved = ConfigIncludeReserved;
            _IncludeAllocated = ConfigIncludeAllocated;
            _SelectedRouteMode = RouteModeList.FirstOrDefault(c => c.Value.Equals(ConfigRouteMode));
            _MaximumRouteAlternatives = MaxRouteAltInLoop;

            return result;
        }

        public override bool ACPostInit()
        {
            LoadRouteItemModes();
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

        private bool _IsInPresenterMode = false;

        private IEnumerable<string> _start = null;
        private IEnumerable<string> _end = null;

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

        private bool _IncludeAllocated;
        [ACPropertyInfo(402, "", "en{'Include allocated'}de{'Zugeteilt einbeziehen'}")]
        public bool IncludeAllocated
        {
            get => _IncludeAllocated;
            set
            {
                _IncludeAllocated = value;
                ConfigIncludeAllocated = value;
                OnPropertyChanged();

                if (_IsInPresenterMode)
                    ResetRoute();
            }
        }

        private bool _IncludeReserved;
        [ACPropertyInfo(403, "", "en{'Include reserved'}de{'Reserviert einschließen'}")]
        public bool IncludeReserved
        {
            get => _IncludeReserved;
            set
            {
                _IncludeReserved = value;
                ConfigIncludeReserved = value;
                OnPropertyChanged();

                if (_IsInPresenterMode)
                    ResetRoute();
            }
        }

        private bool _IsInEdgeWeightAdjustmentMode = false;

        public List<List<ACRoutingPath>> _AvailableRoutes;
        [ACPropertyInfo(404)]
        public List<List<ACRoutingPath>> AvailableRoutes
        {
            get => _AvailableRoutes;
            set
            {
                _AvailableRoutes = value;
                OnPropertyChanged();
            }
        }

        private List<IACObject> _ActiveRouteComponents;
        [ACPropertyInfo(405)]
        public List<IACObject> ActiveRouteComponents
        {
            get { return _ActiveRouteComponents; }
            set
            {
                _ActiveRouteComponents = value;
                OnPropertyChanged();
            }
        }

        private IACObject _SelectedComponent;
        [ACPropertyInfo(405)]
        public IACObject SelectedComponent
        {
            get { return _SelectedComponent; }
            set
            {
                _SelectedComponent = value;
                OnPropertyChanged();

                Guid? compID = (_SelectedComponent?.ACType as ACClass)?.ACClassID;
                if (compID.HasValue)
                {
                    var cacheItem = _RouteModeItemCache.FirstOrDefault(c => c.Item1 == compID.Value);
                    if (cacheItem != null)
                    {
                        SelectedRouteItemMode = RouteModeItemList.FirstOrDefault(c => (RouteItemModeEnum)c.Value == (RouteItemModeEnum)cacheItem.Item2);
                        return;
                    }
                }
                SelectedRouteItemMode = null;
            }
        }

        private List<IACObject> _ActiveRoutePaths;
        [ACPropertyInfo(406)]
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

        private ACPropertyConfigValue<bool> _ConfigIncludeReserved;
        [ACPropertyConfig("en{'Include reserved config'}de{'Include reserved config'}")]
        public bool ConfigIncludeReserved
        {
            get => _ConfigIncludeReserved.ValueT;
            set
            {
                _ConfigIncludeReserved.ValueT = value;
                OnPropertyChanged();
            }
        }

        private ACPropertyConfigValue<bool> _ConfigIncludeAllocated;
        [ACPropertyConfig("en{'Include allocated config'}de{'Include allocated config'}")]
        public bool ConfigIncludeAllocated
        {
            get => _ConfigIncludeAllocated.ValueT;
            set
            {
                _ConfigIncludeAllocated.ValueT = value;
                OnPropertyChanged();
            }
        }

        private ACPropertyConfigValue<short> _ConfigRouteMode;
        [ACPropertyConfig("en{'Route mode config'}de{'Route mode config'}")]
        public short ConfigRouteMode
        {
            get => _ConfigRouteMode.ValueT;
            set
            {
                _ConfigRouteMode.ValueT = value;
                OnPropertyChanged();
            }
        }

        private ACPropertyConfigValue<int> _MaxRouteAltInLoop;
        [ACPropertyConfig("en{'Max route alt. in loop'}de{'Max route alt. in loop'}")]
        public int MaxRouteAltInLoop
        {
            get => _MaxRouteAltInLoop.ValueT;
            set
            {
                _MaxRouteAltInLoop.ValueT = value;
                OnPropertyChanged();
            }
        }

        private ACValueItem _SelectedRouteItemMode;
        [ACPropertySelected(410, "RouteItemMode", "en{'Route item mode'}de{'Routenelementmodus'}")]
        public ACValueItem SelectedRouteItemMode
        {
            get => _SelectedRouteItemMode;
            set
            {
                _SelectedRouteItemMode = value;
                OnPropertyChanged();

                if (value != null)
                {
                    Guid? selectedComponentID = (SelectedComponent?.ACType as ACClass)?.ACClassID;
                    Tuple<Guid, short> item = _RouteModeItemCache.FirstOrDefault(c => c.Item1 == selectedComponentID.Value);

                    if (selectedComponentID.HasValue)
                    {
                        if (_SelectedRouteItemMode != null && (RouteItemModeEnum)_SelectedRouteItemMode.Value != RouteItemModeEnum.None)
                        {
                            if (item != null)
                                _RouteModeItemCache.Remove(item);

                            item = new Tuple<Guid, short>(selectedComponentID.Value, (short)_SelectedRouteItemMode.Value);
                            _RouteModeItemCache.Add(item);
                        }
                        else
                        {
                            if (item != null)
                                _RouteModeItemCache.Remove(item);
                        }
                    }
                }
            }
        }

        private ACValueItemList _RouteModeItemList;
        [ACPropertyList(411, "RouteItemMode")]
        public ACValueItemList RouteModeItemList
        {
            get
            {
                if (_RouteModeItemList == null)
                {
                    _RouteModeItemList = new ACValueItemList("");
                    _RouteModeItemList.Add(new ACValueItem("en{'None'}de{'Keine'}", RouteItemModeEnum.None, null));
                    _RouteModeItemList.Add(new ACValueItem("en{'Next route items parallel'}de{'Nächste Elemente parallel verlegen'}", RouteItemModeEnum.RouteItemsNextParallel, null));
                    _RouteModeItemList.Add(new ACValueItem("en{'Next route items nonparallel'}de{'Die nächsten Routenelemente sind nicht parallel'}", RouteItemModeEnum.RouteItemsNextNonParallel, null));
                }
                return _RouteModeItemList;
            }
        }

        private List<Tuple<Guid, short>> _RouteModeItemCache;

        #endregion

        #region Methods

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(ApplyRoute):
                    ApplyRoute();
                    return true;
                case nameof(Relayout):
                    Relayout();
                    return true;
                case nameof(ReturnToRouteSelector):
                    ReturnToRouteSelector();
                    return true;
                case nameof(SetRoute):
                    SetRoute();
                    return true;
                case nameof(SaveSettings):
                    SaveSettings();
                    return true;
                case nameof(SaveRouteItemMode):
                    SaveRouteItemMode();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public void ShowAvailableRoutes(IEnumerable<ACClass> startComponents, IEnumerable<ACClass> endComponents, string selectionRuleID = null, object[] selectionRuleParams = null, 
                                        bool allowProcessModuleInRoute = true, ACClass preselectedStart = null)
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

            if (preselectedStart != null)
                SelectedStartComponent = StartComponents.FirstOrDefault(c => c.ValueT.ACClassID == preselectedStart.ACClassID);
            
            if(SelectedStartComponent == null)
                SelectedStartComponent = StartComponents.FirstOrDefault();
            SelectedEndComponent = EndComponents.FirstOrDefault();

            ShowDialog(this, "Mainlayout");
        }

        public void ShowAvailableRoutes(IEnumerable<Tuple<ACClass, ACClassProperty>> startPoints, IEnumerable<Tuple<ACClass, ACClassProperty>> endPoints, string selectionRuleID = null, object[] selectionRuleParams = null, bool allowProcessModuleInRoute = true)
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

            ShowDialog(this, "Mainlayout");
        }

        public void EditRoutes(Route route, bool isReadOnly, bool includeReserved, bool includeAllocated)
        {
            IEnumerable<Route> splitedRoutes = Route.SplitRoute(route);

            IEnumerable<string> sourceComponentsList = splitedRoutes.Select(x => x.FirstOrDefault().Source.ACUrlComponent).Distinct();
            IEnumerable<string> targetComponentsList = splitedRoutes.Select(x => x.LastOrDefault().Target.ACUrlComponent).Distinct();

            if (!GetRoutes(sourceComponentsList, targetComponentsList, true, true))
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

        public void EditRoutesWithAttach(Route route, bool isReadOnly, bool includeReserved, bool includeAllocated)
        {
            route.AttachTo(Database.ContextIPlus);
            IEnumerable<Route> splitedRoutes = Route.SplitRoute(route);

            IEnumerable<string> sourceComponentsList = splitedRoutes.Select(x => x.FirstOrDefault().Source.ACUrlComponent).Distinct();
            IEnumerable<string> targetComponentsList = splitedRoutes.Select(x => x.LastOrDefault().Target.ACUrlComponent).Distinct();

            if (!GetRoutes(sourceComponentsList, targetComponentsList, true, true, false, null, null, true))
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

            route.Detach();
        }

        public void ShowRoute(Route route)
        {
            route.AttachTo(Database.ContextIPlus);
            IEnumerable<Route> splitedRoutes = Route.SplitRoute(route);

            List<IACObject> components = new List<IACObject>();
            List<IACObject> routePath = new List<IACObject>();
            SelectedRoutingPaths = new List<ACRoutingPath>();
            List<List<ACRoutingPath>> tempPath = new List<List<ACRoutingPath>>();

            foreach (Route r in splitedRoutes)
            {
                ACRoutingPath routingPath = new ACRoutingPath();
                
                foreach (RouteItem rItem in r)
                {
                    routingPath.Add(new PAEdge(rItem.TargetACPoint as PAPoint, rItem.SourceACPoint as PAPoint, rItem.RelationID));
                }

                tempPath.Add(new List<ACRoutingPath>() { routingPath });

                ACRoutingPath selectedRoutePath = routingPath;
                SetActiveRoutes(components, routePath, selectedRoutePath);
            }
            AvailableRoutes = tempPath;

            ActiveRouteComponents = components;
            ActiveRoutePaths = routePath;

            ShowRoute(true);

            route.Detach();

        }

        public void ShowRoute(bool isReadOnly = false)
        {
            if (!isReadOnly)
                InitSelectionManger(Const.SelectionManagerCDesign_ClassName);

            CloseTopDialog();

            var tempRouteMode = SelectedRouteMode;

            _IsInPresenterMode = true;

            if (!_IsInEdgeWeightAdjustmentMode)
                ShowDialog(this, "RoutePresenter");
            else
                ShowDialog(this, "EdgeWeightsPresenter");

            SelectedRouteMode = tempRouteMode;

            _IsInPresenterMode = false;
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

        private bool GetRoutes(IEnumerable<string> sourceComponentsList, IEnumerable<string> targetComponentsList, bool includeReserved, bool includeAllocated, bool isForEditor = false, string selectionRuleID = null, object[] selectionRuleParams = null,
                                bool attach = false)
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
            SelectActiveRoutes(attach ? Database.ContextIPlus : null);

            AvailableRoutes = AvailableRoutes.ToList();
            ActiveRoutePaths = ActiveRoutePaths.ToList();
            ActiveRouteComponents = ActiveRouteComponents.ToList();

            return true;
        }

        private void FindRoutes(string startComponent, Guid? startPointID, string endComponent, Guid? endPointID, bool includeReserved, bool includeAllocated, bool isForEditor = false, string selectionRuleID = null, object[] selectionRuleParams = null)
        {
            if (startComponent == null || endComponent == null)
                return;

            int maxRouteAlt = 1;
            if (MaximumRouteAlternatives > -1 && MaximumRouteAlternatives < 11)
                maxRouteAlt = MaximumRouteAlternatives;

            ACRoutingParameters routingParameters = new ACRoutingParameters()
            {
                MaxRouteAlternativesInLoop = maxRouteAlt,
                IncludeReserved = includeReserved,
                IncludeAllocated = includeAllocated,
                IsForEditor = isForEditor,
                SelectionRuleID = selectionRuleID,
                SelectionRuleParams = selectionRuleParams,
                MaxRouteLoopDepth = MaximumRouteLoopDepth
            };

            var buildRouteResult = ACUrlCommand(string.Format("{0}!" + nameof(ACRoutingService.BuildAvailableRoutesFromPoints), RoutingServiceACUrl.StartsWith("\\") ? RoutingServiceACUrl : "\\" + RoutingServiceACUrl),
                                                startComponent, startPointID, endComponent, endPointID, routingParameters) as Tuple<List<ACRoutingVertex>, PriorityQueue<ST_Node>>;
            if (buildRouteResult == null)
                return;

            foreach (ACRoutingVertex v in buildRouteResult.Item1)
            {
                v.ComponentRef.ChangeMode(ACRef<IACComponent>.RefInitMode.AutoStart);
                if (v.Next != null)
                    v.Next.ChangeMode(ACRef<IACComponent>.RefInitMode.AutoStart);
            }

            ACRoutingVertex startVertex = buildRouteResult.Item1.FirstOrDefault(c => c.ComponentRef.ValueT != null && c.ComponentRef.ValueT.ACUrl == startComponent);
            if (startVertex == null)
                return;

            List<ACRoutingPath> availableRoutingPaths = new List<ACRoutingPath>();

            ST_Node temp = buildRouteResult.Item2.Dequeue();
            while (temp != null)
            {
                ACRoutingPath path = ACRoutingSession.RebuildPath(temp.Sidetracks, startVertex.ComponentRef.ValueT, buildRouteResult.Item1, _RouteModeItemCache.ToDictionary(c => c.Item1, c => (RouteItemModeEnum)c.Item2));
                if (!temp.Sidetracks.Any())
                {
                    path.IsPrimaryRoute = true;
                }
                if (path != null && path.IsValid)
                {
                    availableRoutingPaths.Add(path);

                    foreach (PAEdge edge in path)
                    {
                        if (edge.Source != null && !edge.Source.ACRef.IsAttached)
                        {
                            edge.Source.ACRef.ChangeMode(ACRef<IACComponent>.RefInitMode.AutoStart);
                            //edge.Source.ACRef.Attach();
                        }

                        if (edge.Target != null && !edge.Target.ACRef.IsAttached)
                        {
                            edge.Target.ACRef.ChangeMode(ACRef<IACComponent>.RefInitMode.AutoStart);
                            //edge.Target.ACRef.Attach();
                        }
                    }
                }
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
                            var selected = SelectedActiveRoutingPaths.Where(c => paths.Any(x => x.Start.ParentACComponent.ACUrl == c.Start.ParentACComponent.ACUrl 
                                                                                              && x.End.ParentACComponent.ACUrl == c.End.ParentACComponent.ACUrl));
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
            SelectedComponent = selectedComponents.FirstOrDefault();
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

        private void SelectActiveRoutes(Database db = null)
        {
            bool shortestRoute = SelectedRouteMode != null && SelectedRouteMode.Value.Equals(1);

            foreach (List<ACRoutingPath> availableRoute in AvailableRoutes)
            {
                List<ACRoutingPath> paths = new List<ACRoutingPath>();

                List<Guid> targetIDs = null;
                List<RouteHashItem> routeHashItems = null;

                if (!shortestRoute)
                {
                    if (db != null)
                    {
                        foreach (ACRoutingPath routingPath in availableRoute)
                        {
                            foreach (PAEdge edge in routingPath)
                            {
                                if (edge.Relation == null)
                                    edge.AttachRelation(db);
                            }
                        }
                    }

                    targetIDs = availableRoute.Where(c => c.LastOrDefault().Relation != null).Select(c => c.LastOrDefault().Relation.SourceACClassID).Distinct().ToList();
                    routeHashItems = LoadRouteUsage(targetIDs);
                }

                List<int> tempHashCodeItems = new List<int>();

                if (availableRoute.Any(c => c.RouteItemsMode.Any()))
                {
                    List<ACRoutingPath> tempPaths = new List<ACRoutingPath>();
                    List<Tuple<IACObject, RouteItemModeEnum>> routeItemsMode = availableRoute.SelectMany(c => c.RouteItemsMode).Distinct().ToList();

                    int routingPathNo = 1;

                    foreach (ACRoutingPath path in availableRoute.OrderBy(x => x.DeltaWeight).Where(c => c.Any(x => routeItemsMode.Any(k => k.Item1 == x.SourceParent))))
                    {
                        ACRoutingPath newPath = new ACRoutingPath();
                        path.RoutingPathNo = routingPathNo;
                        newPath.RoutingPathNo = routingPathNo;

                        foreach (PAEdge edge in path)
                        {
                            if (!tempPaths.Any(x => x.Any(c => edge == c || (edge.RelationID.HasValue && c.RelationID.HasValue && edge.RelationID == c.RelationID))))
                                newPath.Add(edge);
                        }

                        if (newPath.Any())
                        {
                            tempPaths.Add(newPath);
                            routingPathNo++;
                        }
                    }

                    ACRoutingPath workingPath = tempPaths.FirstOrDefault();
                    paths.Add(workingPath);

                    foreach (ACRoutingPath path in tempPaths)
                    {
                        if (path == workingPath)
                            continue;

                        ACRoutingPath mainPath = path;

                        while (true)
                        {
                            mainPath = tempPaths.FirstOrDefault(c => c != path && c.Any(k => k.SourceParent == mainPath.FirstOrDefault().SourceParent));

                            if (mainPath == null)
                                break;

                            int index = mainPath.IndexWhere(c => c.SourceParent == path.Start.ParentACObject);
                            if (index < 0 || index + 1 > mainPath.Count)
                                continue;

                            var mode = routeItemsMode.Where(c => mainPath.Take(index + 1).Any(x => x.SourceParent == c.Item1)).LastOrDefault();
                            if (mode != null)
                            {
                                if (mode.Item2 == RouteItemModeEnum.RouteItemsNextParallel)
                                {
                                    ACRoutingPath pathToAdd = availableRoute.FirstOrDefault(c => c.RoutingPathNo == path.RoutingPathNo);
                                    paths.Add(pathToAdd);
                                }
                            }

                            if (mainPath == workingPath)
                                break;
                        }
                    }
                }
                else if (!paths.Any() && routeHashItems != null && routeHashItems.Any())
                {
                    List<int> hashCodes = new List<int>();

                    foreach (var itemPath in availableRoute)
                    {
                        PAEdge source = itemPath.FirstOrDefault();
                        PAEdge target = itemPath.LastOrDefault();

                        itemPath.Remove(source);
                        itemPath.Remove(target);

                        List<Guid> temp = null;
                        if (itemPath.Any())
                        {
                            temp = itemPath.Select(c => c.Relation.SourceACClassID).ToList();
                            temp.Add(itemPath.LastOrDefault().Relation.TargetACClassID);
                        }
                        else
                            temp = new List<Guid>() { target.Relation.SourceACClassID };

                        int hash = string.Join("", temp).GetHashCode();

                        itemPath.Insert(0, source);
                        itemPath.Add(target);

                        var groupedByKey = routeHashItems.GroupBy(c => c.UseFactor).OrderByDescending(c => c.Key).FirstOrDefault();

                        if (groupedByKey.Any(c => c.RouteHashCodes.Any(x => x == hash)))
                            paths.Add(itemPath);
                    }

                    if (paths == null || !paths.Any())
                        shortestRoute = true;
                }
                else
                    shortestRoute = true;

                if (shortestRoute)
                    paths = availableRoute.OrderBy(c => c.RouteWeight).Take(1).ToList();

                foreach (var path in paths)
                {
                    SetActiveRoutes(ActiveRouteComponents, ActiveRoutePaths, path);
                    SelectedActiveRoutingPaths.Add(path);
                }

                if (_IsInEdgeWeightAdjustmentMode)
                    EdgesList = paths.SelectMany(c => c).ToList();
            }

            SelectedActiveRoutingPaths = SelectedActiveRoutingPaths.ToList();
        }

        private List<RouteHashItem> LoadRouteUsage(IEnumerable<Guid> targets)
        {
            List<RouteHashItem> result = new List<RouteHashItem>();

            using (Database db = new core.datamodel.Database())
            {
                List<ACClassRouteUsage> routeUsageList = db.ACClassRouteUsage.Include(x => x.ACClassRouteUsagePos_ACClassRouteUsage)
                                                                             .Where(c => targets.Contains(c.ACClassID))
                                                                             .ToList();

                var groupedUsageList = routeUsageList.GroupBy(c => c.ACClassID);

                foreach (var group in groupedUsageList)
                {
                    ACClassRouteUsage routeUsage = group.OrderByDescending(c => c.UseFactor).ThenByDescending(c => c.UpdateDate).FirstOrDefault();
                    RouteHashItem hashItem = new RouteHashItem();
                    hashItem.RouteHashCodes = new SafeList<int>(routeUsage.ACClassRouteUsagePos_ACClassRouteUsage.Select(c => c.HashCode));
                    hashItem.RouteUsageGroupID = new SafeList<Guid>(routeUsage.ACClassRouteUsageGroup_ACClassRouteUsage.Select(c => c.GroupID));
                    hashItem.UseFactor = routeUsage.UseFactor;
                    result.Add(hashItem);
                }

                if (result.Count > 1)
                {
                    if (result.All(c => (c.RouteUsageGroupID == null || !c.RouteUsageGroupID.Any())))
                    {
                        result = result.OrderByDescending(c => c.UseFactor).Take(1).ToList();
                    }
                    else
                    {
                        if (!result.SelectMany(c => c.RouteUsageGroupID).GroupBy(c => c).Any(c => c.Count() == result.Count))
                        {
                            result = result.OrderByDescending(c => c.UseFactor).Take(1).ToList();
                        }
                    }
                }
            }

            return result;
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
                routes.Add(new Route(tempList) /*{ HasAnyReserved = path.HasAnyReserved, HasAnyAllocated = path.HasAnyAllocated }*/);
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

        [ACMethodInfo("", "en{'Save route mode settings'}de{'Routenmoduseinstellungen speichern'}", 404, true)]
        public void SaveRouteItemMode()
        {
            if (_RouteModeItemCache != null)
            {
                _RouteModeItemCache = _RouteModeItemCache.Where(c => c.Item2 > (short)RouteItemModeEnum.None).ToList();

                string xml = "";
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
                {
                    DataContractSerializer serializer = new DataContractSerializer(typeof(List<Tuple<Guid, short>>));
                    serializer.WriteObject(xmlWriter, _RouteModeItemCache);
                    xml = sw.ToString();
                }

                ACUrlCommand(RoutingServiceACUrl + "!" + nameof(ACRoutingService.SaveRouteItemModes), xml);

                //ACClassConfig routeItemModes = Database.ContextIPlus.ACClassConfig.FirstOrDefault(c => c.LocalConfigACUrl == nameof(ACRoutingService.RouteItemModes));
                //if (routeItemModes != null)
                //{
                //    routeItemModes.Value = xml;
                //}

                //Database.ACSaveChanges();
            }

        }

        public void LoadRouteItemModes()
        {

            ACClassConfig routeItemModes = Database.ContextIPlus.ACClassConfig.FirstOrDefault(c => c.LocalConfigACUrl == nameof(ACRoutingService.RouteItemModes) && c.ACClass.ACURLComponentCached == RoutingServiceACUrl);

            if (routeItemModes != null)
            {
                string xml = routeItemModes.Value as string;

                if (!string.IsNullOrEmpty(xml))
                {
                    using (StringReader ms = new StringReader(xml))
                    using (XmlTextReader xmlReader = new XmlTextReader(ms))
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(List<Tuple<Guid, short>>));
                        _RouteModeItemCache = serializer.ReadObject(xmlReader) as List<Tuple<Guid, short>>;
                    }
                }

                if (_RouteModeItemCache == null)
                    _RouteModeItemCache = new List<Tuple<Guid, short>>();
            }
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

        private ACValueItem _SelectedRouteMode;
        [ACPropertySelected(410, "RouteMode")]
        public ACValueItem SelectedRouteMode
        {
            get => _SelectedRouteMode;
            set
            {
                if (value != null)
                {
                    _SelectedRouteMode = value;
                    short? configValue = _SelectedRouteMode?.Value as short?;
                    if (configValue.HasValue)
                        ConfigRouteMode = configValue.Value;
                    OnPropertyChanged();
                }
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

        private int _MaximumRouteAlternatives;
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
                MaxRouteAltInLoop = value;
                OnPropertyChanged();
            }
        }

        private int _MaximumRouteLoopDepth = 2;
        [ACPropertyInfo(412, "", "en{'Maximum route loop depth'}de{'Maximale Routenschleifentiefe'}")]
        public int MaximumRouteLoopDepth
        {
            get
            {
                return _MaximumRouteLoopDepth;
            }
            set
            {
                _MaximumRouteLoopDepth = value;
                OnPropertyChanged();
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
            if (IsMultipleSourcesTargets)
            {
                _start = StartComponents.Where(c => c.IsChecked).Select(x => x.ValueT.ACUrlComponent);
                _end = EndComponents.Where(c => c.IsChecked).Select(x => x.ValueT.ACUrlComponent);
            }
            else if (SelectedStartComponent != null && SelectedEndComponent != null)
            {
                _start = new string[] { SelectedStartComponent.ValueT.ACUrlComponent };
                _end = new string[] { SelectedEndComponent.ValueT.ACUrlComponent };
            }
            else
            {
                _start = null;
                _end = null;
            }

            if (AllowProcessModuleInRoute)
            {
                SelectionRuleID = null;
                SelectionRuleParams = null;
            }

            if (!GetRoutes(_start, _end, IncludeReserved, IncludeAllocated, false, SelectionRuleID, SelectionRuleParams))
                return;

            ShowRoute();
        }

        public void ResetRoute()
        {
            if (!GetRoutes(_start, _end, IncludeReserved, IncludeAllocated, false, SelectionRuleID, SelectionRuleParams))
                return;
        }

        #endregion

        #region AdjustmentEdgeWeight

        private PAEdge _SelectedEdge;
        [ACPropertySelected(414, "SelectedEdge")]
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
            var maxRAltTemp = MaximumRouteAlternatives;
            MaximumRouteAlternatives = maxRouteAlternatives;
            if (!GetRoutes(startComponentsACUrl, endComponentsACUrl, IncludeReserved, IncludeAllocated, true, null, null, true))
                return;
            ShowRoute();
            _IsInEdgeWeightAdjustmentMode = false;
            MaximumRouteAlternatives = maxRAltTemp;
        }

        [ACMethodInfo("", "en{'Save settings'}de{'Einstellungen speichern'}", 405)]
        public void SaveSettings()
        {
            int routeNo = 1;
            List<RouteItem> routeItems = new List<RouteItem>();
            foreach (ACRoutingPath rPath in SelectedActiveRoutingPaths)
            {
                foreach (PAEdge edge in rPath)
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
