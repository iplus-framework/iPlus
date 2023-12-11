using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Timers;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data.Objects;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Unter-BSO für VBBSOControlDialog
    /// Wird verwendet für PABase (Modelwelt) und Ableitungen
    /// 
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Control for Processmodule'}de{'Prozessmodul Steuerung'}", Global.ACKinds.TACBSOGlobal, Global.ACStorableTypes.NotStorable, false, false)]
    public class VBBSOControlPAM : ACBSO
    {
        #region c'tors
        public VBBSOControlPAM(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            _RoutingService = ACRoutingService.ACRefToServiceInstance(this);

            return true;
        }


        public override bool ACPostInit()
        {
            if (SelectionManager != null)
            {
                SelectionManager.PropertyChanged += SelectionManager_PropertyChanged;
                if ((this.SelectionManager as VBBSOSelectionManager).ShowACObjectForSelection != null)
                {
                    CurrentACComponent = (this.SelectionManager as VBBSOSelectionManager).ShowACObjectForSelection;
                }
            }

            // TODO: Init-Query-Request
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_SelectionManager != null)
            {
                _SelectionManager.Detach();
                _SelectionManager.ObjectDetaching -= _SelectionManager_ObjectDetaching;
                _SelectionManager.ObjectAttached -= _SelectionManager_ObjectAttached;
                _SelectionManager = null;
            }

            if (_CurrentACComponent != null)
            {
                _CurrentACComponent.Detach();
                _CurrentACComponent = null;
            }

            ACSaveOrUndoChanges();
            this._CurrentACComponent = null;

            ACRoutingService.DetachACRefFromServiceInstance(this, _RoutingService);
            _RoutingService = null;

            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Selection-Manager
        private ACRef<VBBSOSelectionManager> _SelectionManager;
        public VBBSOSelectionManager SelectionManager
        {
            get
            {
                if (_SelectionManager != null)
                    return _SelectionManager.ValueT;
                if (ParentACComponent != null)
                {
                    VBBSOSelectionManager subACComponent = ParentACComponent.GetChildComponent(SelectionManagerACName) as VBBSOSelectionManager;
                    if (subACComponent == null)
                    {
                        if (ParentACComponent is VBBSOSelectionDependentDialog)
                        {
                            subACComponent = (ParentACComponent as VBBSOSelectionDependentDialog).SelectionManager;
                        }
                        else
                            subACComponent = ParentACComponent.StartComponent(SelectionManagerACName, null, null) as VBBSOSelectionManager;
                    }
                    if (subACComponent != null)
                    {
                        _SelectionManager = new ACRef<VBBSOSelectionManager>(subACComponent, this);
                        _SelectionManager.ObjectDetaching += new EventHandler(_SelectionManager_ObjectDetaching);
                        _SelectionManager.ObjectAttached += new EventHandler(_SelectionManager_ObjectAttached);
                    }
                }
                if (_SelectionManager == null)
                    return null;
                return _SelectionManager.ValueT;
            }
        }

        void _SelectionManager_ObjectAttached(object sender, EventArgs e)
        {
            if (SelectionManager != null)
                SelectionManager.PropertyChanged += SelectionManager_PropertyChanged;
        }

        void _SelectionManager_ObjectDetaching(object sender, EventArgs e)
        {
            if (SelectionManager != null)
                SelectionManager.PropertyChanged -= SelectionManager_PropertyChanged;
        }

        private string SelectionManagerACName
        {
            get
            {
                string acInstance = ACUrlHelper.ExtractInstanceName(this.ACIdentifier);
                if (String.IsNullOrEmpty(acInstance))
                    return "VBBSOSelectionManager";
                else
                    return "VBBSOSelectionManager(" + acInstance + ")";
            }
        }

        void SelectionManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowACObjectForSelection")
            {
                CurrentACComponent = SelectionManager.ShowACObjectForSelection;
            }

            //if (e.PropertyName == "SelectedVBControl")
            //{
            //    ACComponent tempComp = null;
            //    if ((this.SelectionManager as SelectionManager).SelectedVBControl != null)
            //        tempComp = (this.SelectionManager as SelectionManager).SelectedVBControl.ContextACObject as ACComponent;
            //    if (tempComp != null && ShowModuleOfPWGroup)
            //    {
            //        ACComponent refModule = GetReferencedModuleOfPWGroup(tempComp);
            //        if (refModule != null)
            //            tempComp = refModule;
            //    }
            //    CurrentACComponent = tempComp;
            //}
        }
        #endregion

        #region Properties

        #region Selected Processmodule
        public bool _ShowModuleOfPWGroup;
        [ACPropertyInfo(300, "", "en{'Show referenced Processmodule'}de{'Zeige verbundenes Prozessmodul'}")]
        public bool ShowModuleOfPWGroup
        {
            get
            {
                return _ShowModuleOfPWGroup;
            }
            set
            {
                _ShowModuleOfPWGroup = value;
                OnPropertyChanged("ShowModuleOfPWGroup");
            }
        }

        ACRef<IACObject> _CurrentACComponent;
        [ACPropertyInfo(9999)]
        public IACObject CurrentACComponent
        {
            get
            {
                if (_CurrentACComponent == null)
                    return null;
                return _CurrentACComponent.ValueT;
            }
            set
            {
                bool objectSwapped = true;
                if (_CurrentACComponent != null)
                {
                    if (_CurrentACComponent != value)
                    {
                        _CurrentACComponent.Detach();
                    }
                    else
                        objectSwapped = false;
                }
                if (value == null)
                    _CurrentACComponent = null;
                else
                    _CurrentACComponent = new ACRef<IACObject>(value, this);
                if (_CurrentACComponent != null)
                {
                    if (objectSwapped)
                    {
                        LoadChildFunctions();
                    }
                }
                else
                {
                    LoadChildFunctions();
                }
                OnPropertyChanged("CurrentACComponent");
            }
        }

        #endregion

        #region Child PAFFunctions
        private IList<IACComponent> _FunctionList = null;
        [ACPropertyList(9999, "Function")]
        public IList<IACComponent> FunctionList
        {
            get
            {
                return _FunctionList;
            }
            protected set
            {
                _FunctionList = value;
                OnPropertyChanged("FunctionList");
            }
        }

        IACComponent _SelectedFunction;
        [ACPropertySelected(9999, "Function")]
        public IACComponent SelectedFunction
        {
            get
            {
                return _SelectedFunction;
            }
            set
            {
                _SelectedFunction = value;
                OnPropertyChanged("SelectedFunction");
                OnPropertyChanged("LayoutFunction");
                OnPropertyChanged("LayoutOfControl");
                OnPropertyChanged("LayoutOfControlDialog");
                LoadVirtualMethods();
            }
        }
        #endregion

        #region Virtual Methods of Function
        private IList<ACClassMethod> _VirtualMethodList = null;
        [ACPropertyList(9999, "ACClassMethod")]
        public IList<ACClassMethod> VirtualMethodList
        {
            get
            {
                return _VirtualMethodList;
            }
            protected set
            {
                _VirtualMethodList = value;
                OnPropertyChanged("VirtualMethodList");
            }
        }

        ACClassMethod _SelectedVirtualMethod;
        [ACPropertySelected(9999, "ACClassMethod")]
        public ACClassMethod SelectedVirtualMethod
        {
            get
            {
                return _SelectedVirtualMethod;
            }
            set
            {
                _SelectedVirtualMethod = value;
                OnPropertyChanged("SelectedVirtualMethod");
                if (_SelectedVirtualMethod != null)
                {
                    CurrentACMethod = _SelectedVirtualMethod.TypeACSignature();
                }
                else
                    CurrentACMethod = null;
            }
        }
        #endregion

        #region ACMethod

        private ACMethod _CurrentACMethod;
        [ACPropertyInfo(9999)]
        public ACMethod CurrentACMethod
        {
            get
            {
                return _CurrentACMethod;
            }
            set
            {
                _CurrentACMethod = value;
                OnPropertyChanged("CurrentACMethod");
                OnPropertyChanged("ACMethodParamList");
                OnPropertyChanged("ACMethodResultParamList");
            }
        }

        [ACPropertyList(9999, "ParamACMethod")]
        public IEnumerable<ACValue> ACMethodParamList
        {
            get
            {
                if (CurrentACMethod != null)
                    return CurrentACMethod.ParameterValueList;
                return null;
            }
        }

        private ACValue _SelectedACMethodParam;
        [ACPropertySelected(9999, "ParamACMethod")]
        public ACValue SelectedACMethodParam
        {
            get
            {
                return _SelectedACMethodParam;
            }
            set
            {
                _SelectedACMethodParam = value;
                OnPropertyChanged("SelectedACMethodParam");
            }
        }


        [ACPropertyList(9999, "ResultACMethod")]
        public IEnumerable<ACValue> ACMethodResultParamList
        {
            get
            {
                if (CurrentACMethod != null)
                    return CurrentACMethod.ResultValueList;
                return null;
            }
        }

        private ACValue _SelectedACMethodResultParam;
        [ACPropertySelected(9999, "ResultACMethod")]
        public ACValue SelectedACMethodResultParam
        {
            get
            {
                return _SelectedACMethodResultParam;
            }
            set
            {
                _SelectedACMethodResultParam = value;
                OnPropertyChanged("SelectedACMethodResultParam");
            }
        }

        #endregion

        #region Design
        string _Update2 = "";
        [ACPropertyInfo(9999)]
        public string LayoutOfControl
        {
            get
            {
                if (SelectedFunction == null)
                    return null;

                string layoutXAML = "";
                ACClassDesign acClassDesign = SelectedFunction.ACType.GetDesign(SelectedFunction, Global.ACUsages.DUControl, Global.ACKinds.DSDesignLayout);
                if (acClassDesign != null)
                    layoutXAML += acClassDesign.XMLDesign + _Update2;

                // Sonst reagiert das Steuerelement nicht aufs PropertyChanged
                _Update2 = _Update2 == "" ? " " : "";
                return layoutXAML;
            }
        }

        string _Update = "";
        [ACPropertyInfo(9999)]
        public string LayoutOfControlDialog
        {
            get
            {
                if (SelectedFunction == null)
                    return null;

                string layoutXAML = "";
                ACClassDesign acClassDesign = SelectedFunction.ACType.GetDesign(SelectedFunction, Global.ACUsages.DUControlDialog, Global.ACKinds.DSDesignLayout);
                if (acClassDesign != null)
                    layoutXAML = acClassDesign.XMLDesign + _Update;

                // Sonst reagiert das Steuerelement nicht aufs PropertyChanged
                _Update = _Update == "" ? " " : "";
                return layoutXAML;
            }
        }
        #endregion

        #region Routing

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

        private bool _IsRouteServiceActive = false;

        private ACValueItem _CurrentSourceOrTarget;

        IEnumerable<ACClass> _InPointList;
        [ACPropertyList(9999, "InPoints", "en{'Sources'}de{'Quellen'}")]
        public IEnumerable<ACClass> InPointList
        {
            get
            {
                if (CurrentACComponent == null || CurrentSourceOrTargetComponent == null || CurrentSourceOrTargetComponent.Value.ToString() != "Source")
                    return null;

                if (_InPointList != null)
                    return _InPointList;

                ACClass _CurrentInPoint = (CurrentACComponent.ACType as ACClass);

                using (ACMonitor.Lock(Database.QueryLock_1X000))
                {
                    _InPointList = _CurrentInPoint.ACClassPropertyRelation_TargetACClass.Where(c => c.ConnectionTypeIndex == (short)Global.ConnectionTypes.ConnectionPhysical || c.ConnectionTypeIndex == (short)Global.ConnectionTypes.DynamicConnection)
                        .Join(Database.ContextIPlus.ACClass, x => x.SourceACClassID, y => y.ACClassID, (x, y) => new { y })
                        .Select(x => x.y)
                        .AsEnumerable()
                        .Where(c => c.ACKind == Global.ACKinds.TPAProcessModule)
                        .OrderBy(k => k.ACIdentifier);
                }
                return _InPointList;
            }
        }

        private ACClass _SelectedInPoint;
        [ACPropertySelected(9999, "InPoints", "en{'Source'}de{'Quelle'}")]
        public ACClass SelectedInPoint
        {
            get
            {
                return _SelectedInPoint;
            }
            set
            {
                if (SelectedInPoint != value)
                {
                    _SelectedInPoint = value;
                    OnPropertyChanged("SelectedInPoint");
                }
            }
        }

        IEnumerable<ACClass> _OutPointList;
        [ACPropertyList(9999, "OutPoints", "en{'Targets'}de{'Ziele'}")]
        public IEnumerable<ACClass> OutPointList
        {
            get
            {
                if (CurrentACComponent == null || CurrentSourceOrTargetComponent == null || CurrentSourceOrTargetComponent.Value.ToString() != "Target")
                    return null;

                if (_OutPointList != null)
                    return _OutPointList;

                ACClass _CurrentOutPoint = (CurrentACComponent.ACType as ACClass);

                using (ACMonitor.Lock(Database.QueryLock_1X000))
                {
                    _OutPointList = _CurrentOutPoint.ACClassPropertyRelation_SourceACClass
                        .Where(c => c.ConnectionTypeIndex == (short)Global.ConnectionTypes.ConnectionPhysical || c.ConnectionTypeIndex == (short)Global.ConnectionTypes.DynamicConnection)
                        .Join(Database.ContextIPlus.ACClass, x => x.TargetACClassID, y => y.ACClassID, (x, y) => new { y })
                        .Select(x => x.y)
                        .AsEnumerable()
                        .Where(c => c.ACKind == Global.ACKinds.TPAProcessModule)
                        .OrderBy(k => k.ACIdentifier);
                }

                return _OutPointList;
            }
        }

        private ACClass _SelectedOutPoint;
        [ACPropertySelected(9999, "OutPoints", "en{'Target'}de{'Ziel'}")]
        public ACClass SelectedOutPoint
        {
            get
            {
                return _SelectedOutPoint;
            }
            set
            {
                if (SelectedOutPoint != value)
                {
                    _SelectedOutPoint = value;
                    OnPropertyChanged("SelectedOutPoint");
                }
            }
        }

        private ACValueItem _CurrentSourceOrTargetComponent;
        [ACPropertyCurrent(999, "SourceOrTarget", "en{'Component'}de{'Component'}")]
        public ACValueItem CurrentSourceOrTargetComponent
        {
            get
            {
                return _CurrentSourceOrTargetComponent;
            }
            set
            {
                _CurrentSourceOrTargetComponent = value;
                OnPropertyChanged("CurrentSourceOrTargetComponent");
                if (_IsRouteServiceActive)
                    return;

                OnPropertyChanged("SelectedInPoint");
                OnPropertyChanged("SelectedOutPoint");
                OnPropertyChanged("InPointList");
                OnPropertyChanged("OutPointList");
            }
        }

        private ACValueItemList _SourceOrTargetComponentList;
        [ACPropertyList(999, "SourceOrTarget")]
        public ACValueItemList SourceOrTargetComponentList
        {
            get
            {
                if (_SourceOrTargetComponentList == null)
                {
                    _SourceOrTargetComponentList = new ACValueItemList("");
                    _SourceOrTargetComponentList.AddEntry("Source", "en{'Source'}de{'Quelle'}");
                    _SourceOrTargetComponentList.AddEntry("Target", "en{'Target'}de{'Ziel'}");
                }
                return _SourceOrTargetComponentList;
            }
        }

        #endregion  

        #endregion

        #region Methods

        #region User

        [ACMethodInfo("Start", "en{'Start function'}de{'Funktion starten'}", 500, true)]
        public void Start()
        {
            if (!IsEnabledStart())
                return;

            ACComponent processModule = CurrentACComponent as ACComponent;
            if (processModule == null)
                return;

            processModule.ACUrlCommand("!" + CurrentACMethod.ACIdentifier, CurrentACMethod);
        }

        public bool IsEnabledStart()
        {
            if (SelectedFunction == null || CurrentACMethod == null)
                return false;
            if (!CurrentACMethod.IsValid())
                return false;
            ACStateEnum acState = (ACStateEnum)SelectedFunction.ACUrlCommand(Const.ACState);
            if (acState != ACStateEnum.SMIdle)
                return false;
            return true;
        }

        [ACMethodInfo("Start", "en{'Edit komplex value'}de{'Editiere komplexen Wert'}", 500, true)]
        public void EditValue()
        {
            if (!IsEnabledEditValue())
                return;

            if (typeof(Route).IsAssignableFrom(SelectedACMethodParam.ObjectType))
            {
                if (SelectedFunction == null || SelectedFunction.ComponentClass == null)
                    return;

                IACVBBSORouteSelector routeSelector = ParentACComponent.GetChildComponent("VBBSORouteSelector_Child") as IACVBBSORouteSelector;
                if (routeSelector != null && IsRoutingServiceAvailable)
                {
                    string currentCompACUrl = CurrentACComponent.GetACUrl();
                    RoutingResult sources = null;
                    RoutingResult targets = null;
                    bool receive = false;
                    bool deliver = false;

                    using (Database db = new datamodel.Database())
                    {
                        receive = typeof(IPAFuncReceiveMaterial).IsAssignableFrom(SelectedFunction.ComponentClass.ObjectType);
                        deliver = typeof(IPAFuncDeliverMaterial).IsAssignableFrom(SelectedFunction.ComponentClass.ObjectType);

                        if (   (!receive && !deliver)
                            || (receive && deliver))
                        {
                            ShowDialog(this, "SourceOrTarget");
                            receive = _CurrentSourceOrTarget != null ? _CurrentSourceOrTarget.Value.ToString() == "Source" : false;
                            deliver = !receive ? true : false;
                        }

                        ACClass funcClass = SelectedFunction.ComponentClass.FromIPlusContext<ACClass>(db);
                        if (funcClass == null)
                            return;

                        if (deliver)
                        {
                            ACClassProperty paPointOut = funcClass.GetProperty(Const.PAPointMatOut1);
                            if (paPointOut != null)
                            {
                                IReadOnlyList<Route> routeToRelatedOutPointOfPM = null;
                                ACClassProperty paModulePointOut = null;
                                try
                                {
                                    routeToRelatedOutPointOfPM = ACRoutingService.DbSelectRoutesFromPoint(db, funcClass, paPointOut,
                                                                                                (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule, null,
                                                                                                            RouteDirections.Forwards, true);
                                }
                                catch (Exception ex)
                                {
                                    this.Messages.Exception(this, ex.Message, true);
                                }
                                if (routeToRelatedOutPointOfPM != null)
                                {
                                    Guid targetPropertyGuid = Guid.Empty;
                                    
                                    RouteItem routeTarget = routeToRelatedOutPointOfPM.LastOrDefault()?.GetRouteTarget();
                                    if (routeTarget != null)
                                    {
                                        paModulePointOut = routeTarget.TargetProperty;
                                        targetPropertyGuid = paModulePointOut == null ? Guid.Empty : paModulePointOut.ACClassPropertyID;
                                    }
                                    if (targetPropertyGuid != Guid.Empty)
                                        targets = ACRoutingService.MemFindSuccessorsFromPoint(RoutingService, db, currentCompACUrl, targetPropertyGuid, PAProcessModule.SelRuleID_ProcessModule, RouteDirections.Forwards, 1, true, true,
                                                                                              null, false, true);
                                }
                                if (targets == null)
                                    targets = ACRoutingService.MemFindSuccessors(RoutingService, db, currentCompACUrl, PAProcessModule.SelRuleID_ProcessModule, RouteDirections.Forwards, 1, true, true,
                                                                                 null, false, true);
                                if (targets != null)
                                {
                                    if (targets.Message != null)
                                        Messages.Msg(targets.Message);
                                    else
                                    {
                                        _IsRouteServiceActive = true;
                                        var targetItems = targets.Routes.Select(c => c.LastOrDefault().Target)?.Distinct().Select(c => new Tuple<ACClass, ACClassProperty>(c, null));
                                        ShowAvailableRoutesAndSelect(new Tuple<ACClass, ACClassProperty>[] { new Tuple<ACClass, ACClassProperty>(CurrentACComponent.ACType as ACClass, paModulePointOut) }, targetItems,
                                                                     PAProcessModule.SelRuleID_ProcessModule_Deselector, null, false);
                                    }
                                    _IsRouteServiceActive = false;
                                    return;
                                }
                            }
                        }
                        else // receive
                        {
                            ACClassProperty paPointIn = funcClass.GetProperty(Const.PAPointMatIn1);
                            ACClassProperty paModulePointIn = null;
                            if (paPointIn != null)
                            {
                                IReadOnlyList<Route> routeToRelatedInPointOfPM = null;
                                try
                                {
                                    routeToRelatedInPointOfPM = ACRoutingService.DbSelectRoutesFromPoint(db, funcClass, paPointIn,
                                                                                            (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule, null,
                                                                                                        RouteDirections.Backwards, true);
                                }
                                catch (Exception ex)
                                {
                                    this.Messages.Exception(this, ex.Message, true);
                                }

                                if (routeToRelatedInPointOfPM != null)
                                {
                                    Guid sourcePropertyGuid = Guid.Empty;
                                    RouteItem routeSource = routeToRelatedInPointOfPM.FirstOrDefault()?.GetRouteSource();
                                    if (routeSource != null)
                                    {
                                        paModulePointIn = routeSource.SourceProperty;
                                        sourcePropertyGuid = paModulePointIn == null ? Guid.Empty : paModulePointIn.ACClassPropertyID;
                                    }
                                    if (sourcePropertyGuid != Guid.Empty)
                                        sources = ACRoutingService.MemFindSuccessorsFromPoint(RoutingService, db, currentCompACUrl, sourcePropertyGuid, PAProcessModule.SelRuleID_ProcessModule, RouteDirections.Backwards, 1, true, true,
                                                                                              null, false, true);
                                }
                            }
                            if (sources == null)
                                sources = ACRoutingService.MemFindSuccessors(RoutingService, db, currentCompACUrl, PAProcessModule.SelRuleID_ProcessModule, RouteDirections.Backwards, 1, true, true,
                                                                             null, false, true);
                            if (sources != null)
                            {
                                if (sources.Message != null)
                                    Messages.Msg(sources.Message);
                                else
                                {
                                    _IsRouteServiceActive = true;
                                    var sourceItems = sources.Routes.Select(c => c.FirstOrDefault().Source)?.Distinct().Select(c => new Tuple<ACClass, ACClassProperty>(c, null));
                                    ShowAvailableRoutesAndSelect(sourceItems, new Tuple<ACClass, ACClassProperty>[] { new Tuple<ACClass, ACClassProperty>(CurrentACComponent.ACType as ACClass, paModulePointIn) }, 
                                                                 PAProcessModule.SelRuleID_ProcessModule_Deselector, null, false);
                                }
                                _IsRouteServiceActive = false;
                                return;
                            }
                        }
                    }
                }
                ShowDialog(this, "EditComplexValueDialog");
            }
            else if (SelectedACMethodParam.Value == null)
            {
                Type typeToCreate = SelectedACMethodParam.ObjectType;
                System.Reflection.ConstructorInfo constructor = typeToCreate.GetConstructor(Type.EmptyTypes);
                if (constructor != null)
                {
                    try
                    {
                        object value = Activator.CreateInstance(typeToCreate);
                        if (value != null)
                        {
                            SelectedACMethodParam.Value = value;
                            var currentTemp = SelectedACMethodParam;
                            SelectedACMethodParam = null;
                            SelectedACMethodParam = currentTemp;
                            //OnPropertyChanged("CurrentPAFunctionParamValue");
                        }
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Messages.LogException("VBBSOControlPAM", "EditValue", msg);
                    }
                }
            }
        }

        public bool IsEnabledEditValue()
        {
            if (SelectedFunction == null || CurrentACMethod == null || SelectedACMethodParam == null)
                return false;
            if (SelectedACMethodParam.ObjectType == null)
                return false;
            if (SelectedACMethodParam.ObjectType.IsValueType)
                return false;
            //if (typeof(Route).IsAssignableFrom(SelectedACMethodParam.ObjectType) && !IsRoutingServiceAvailable)
            //    return false;
            return true;
        }

        private void ShowAvailableRoutesAndSelect(IEnumerable<ACClass> start, IEnumerable<ACClass> end)
        {
            IACVBBSORouteSelector routeSelector = ParentACComponent.GetChildComponent("VBBSORouteSelector_Child") as IACVBBSORouteSelector;
            routeSelector.ShowAvailableRoutes(start, end);

            if (routeSelector.RouteResult == null)
                return;

            var routes = routeSelector.RouteResult;
            Route route = routes.FirstOrDefault();
            SelectedACMethodParam.Value = route;
        }

        private void ShowAvailableRoutesAndSelect(IEnumerable<Tuple<ACClass, ACClassProperty>> start, IEnumerable<Tuple<ACClass, ACClassProperty>> end, string selectionRuleID = null, object[] selectionRuleParams = null, bool allowProcessModuleInRoute = true)
        {
            IACVBBSORouteSelector routeSelector = ParentACComponent.GetChildComponent("VBBSORouteSelector_Child") as IACVBBSORouteSelector;
            routeSelector.ShowAvailableRoutes(start, end, selectionRuleID, selectionRuleParams, allowProcessModuleInRoute);

            if (routeSelector.RouteResult == null)
                return;

            var routes = routeSelector.RouteResult;
            Route route = Route.MergeRoutes(routes);
            SelectedACMethodParam.Value = route;
        }

        public override datamodel.Global.ControlModes OnGetControlModes(datamodel.IVBContent vbControl)
        {
            if (vbControl == null && string.IsNullOrEmpty(vbControl.VBContent))
                return base.OnGetControlModes(vbControl);

            if (vbControl.VBContent == nameof(SelectedInPoint))
            {
                if (CurrentSourceOrTargetComponent != null && CurrentSourceOrTargetComponent.Value.ToString() == "Source")
                    return datamodel.Global.ControlModes.Enabled;
                else
                    return datamodel.Global.ControlModes.Collapsed;
            }
            else if (vbControl.VBContent == nameof(SelectedOutPoint))
            {
                if (CurrentSourceOrTargetComponent != null && CurrentSourceOrTargetComponent.Value.ToString() == "Target")
                    return datamodel.Global.ControlModes.Enabled;
                else
                    return datamodel.Global.ControlModes.Collapsed;
            }
            return base.OnGetControlModes(vbControl);
        }

        [ACMethodInfo("", "en{'Ok'}de{'Ok'}", 999)]
        public void EditComplexValueOk()
        {
            string sourceACIdentifier = null;
            if (SelectedInPoint != null)
                sourceACIdentifier = SelectedInPoint.ACIdentifier;
            string targetACIdentifier = null;
            if (SelectedOutPoint != null)
                targetACIdentifier = SelectedOutPoint.ACIdentifier;
            IReadOnlyList<Route> routes = null;
            using (Database db = new datamodel.Database())
            {
                if (!String.IsNullOrEmpty(sourceACIdentifier))
                {
                    ACClass targetACClass = db.ACClass.Where(c => c.ACClassID == CurrentACComponent.ACType.ACTypeID).FirstOrDefault();
                    routes = ACRoutingService.DbSelectRoutes(db, targetACClass,
                        (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule && c.ACIdentifier == sourceACIdentifier,
                        (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule && c.ACClassID != targetACClass.ACClassID, // Breche Suche ab sobald man bei einem Vorgänger der ein Silo oder Waage angelangt ist
                        RouteDirections.Backwards, false, false, 10);
                }
                else if (!String.IsNullOrEmpty(targetACIdentifier))
                {
                    ACClass sourceACClass = db.ACClass.Where(c => c.ACClassID == CurrentACComponent.ACType.ACTypeID).FirstOrDefault();
                    routes = ACRoutingService.DbSelectRoutes(db, sourceACClass,
                                (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule && c.ACIdentifier == targetACIdentifier,
                                (c, p, r) => c.ACKind == Global.ACKinds.TPAProcessModule && c.ACClassID != sourceACClass.ACClassID,
                                RouteDirections.Forwards, false, false);
                }
                if (routes == null || !routes.Any())
                    return;

                Route route = Route.MergeRoutes(routes);
                route.DetachEntitesFromDbContext();
                SelectedACMethodParam.Value = route;
            }
            CloseTopDialog();
        }

        [ACMethodInfo("", "en{'Cancel'}de{'Cancel'}", 999)]
        public void EditComplexValueCancel()
        {
            CloseTopDialog();
        }

        [ACMethodInfo("", "en{'Select'}de{'Select'}", 999, true)]
        public void SelectSourceOrTarget()
        {
            _CurrentSourceOrTarget = CurrentSourceOrTargetComponent;
            CloseTopDialog();
        }

        #endregion

        #region Functions and ACMethod

        protected void LoadChildFunctions()
        {
            if (CurrentACComponent == null || !(CurrentACComponent is ACComponent))
            {
                EmptyChildFunctions();
                return;
            }
            Type typeOfFunction = typeof(PAProcessFunction);
            FunctionList = (CurrentACComponent as ACComponent).ACComponentChildsOnServer.Where(c => c.ACType != null && typeOfFunction.IsAssignableFrom(c.ACType.ObjectType)).ToArray();
            if (FunctionList != null && FunctionList.Any())
            {
                SelectedFunction = FunctionList.FirstOrDefault();
            }
        }


        protected void EmptyChildFunctions()
        {
            if (FunctionList != null)
                FunctionList = null;
            SelectedFunction = null;
        }

        protected void LoadVirtualMethods()
        {
            ACComponent pamModule = CurrentACComponent as ACComponent;
            if (SelectedFunction == null || pamModule == null)
            {
                EmptyVirtualMethods();
                return;
            }

            ACClass acClass = SelectedFunction.ACType as ACClass;
            if (acClass == null)
            {
                EmptyVirtualMethods();
                return;
            }

            IEnumerable<ACClassMethod> virtualMethodsAtFunction = acClass.MethodsCached.Where(c => c.ParentACClassMethodID.HasValue && c.ACClassMethod1_ParentACClassMethod.ACIdentifier == ACStateConst.TMStart).ToArray();
            // Retrieve attached methods from Process-Module:
            IEnumerable<ACClassMethod> attachedMethods = pamModule.ComponentClass.MethodsCached.Where(c => virtualMethodsAtFunction.Where(d => d.ACClassMethodID == c.ParentACClassMethodID).Any()).ToArray();
            VirtualMethodList = attachedMethods.Where(c => !c.AttachedFromACClassID.HasValue // Support to older V4-Versions (prev. 09/2020) 
                                           || c.AttachedFromACClass == acClass
                                           || acClass.IsDerivedClassFrom(c.AttachedFromACClass)).ToArray();
            //VirtualMethodList = acClass.MethodsCached.Where(c => c.ParentACClassMethodID.HasValue && c.ACClassMethod1_ParentACClassMethod.ACIdentifier == ACStateConst.TMStart).ToArray();
            if (VirtualMethodList != null && VirtualMethodList.Any())
                SelectedVirtualMethod = VirtualMethodList.FirstOrDefault();
        }

        protected void EmptyVirtualMethods()
        {
            if (VirtualMethodList != null)
                VirtualMethodList = null;
            SelectedVirtualMethod = null;
        }

        public ACComponent GetReferencedModuleOfPWGroup(ACComponent pwGroup)
        {
            IACPointNetClientObject<ACComponent> _semaphoreProp = pwGroup.GetPointNet("TrySemaphore") as IACPointNetClientObject<ACComponent>;
            if (_semaphoreProp == null)
                return null;

            if (_semaphoreProp.ConnectionList == null || !_semaphoreProp.ConnectionList.Any())
                return null;

            string acUrlOfComponent = _semaphoreProp.ConnectionList.FirstOrDefault().ACUrl;
            ACComponent paProcessModule = pwGroup.ACUrlCommand(acUrlOfComponent) as ACComponent;
            return paProcessModule;
        }

        #endregion

        #region Execute-Helper-Handlers

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "Start":
                    Start();
                    return true;
                case "EditValue":
                    EditValue();
                    return true;
                case "EditComplexValueOk":
                    EditComplexValueOk();
                    return true;
                case "EditComplexValueCancel":
                    EditComplexValueCancel();
                    return true;
                case Const.IsEnabledPrefix + "Start":
                    result = IsEnabledStart();
                    return true;
                case Const.IsEnabledPrefix + "EditValue":
                    result = IsEnabledEditValue();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #endregion
    }
}
