using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.manager
{
    /// <summary>
    /// Abstrakte Basisklasse für alle Workflows aus Datenbanksicht
    /// Hierzu zählen :
    /// -ACClassMethod 
    /// -ACProgramm
    /// -Und später auch die Produktion,Einlagerung,Auslagerung und Umlagerung
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBPresenter'}de{'VBPresenter'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, true, true)]
    public abstract class VBPresenter : ACBSO
    {
        #region c´tors
        public VBPresenter(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            if (Root == null || Root.WPFServices == null || Root.WPFServices.DesignerService == null)
                throw new MemberAccessException("DesignerService is null");
            Root.WPFServices.DesignerService.GenerateNewRoutingLogic();
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (this.VBBSOSelectionManager != null && _SelectionManagerInitialized)
            {
                this.VBBSOSelectionManager.PropertyChanged -= SelectionManagerPropertyChangedHandler;
            }
            if (_SelectedRootWFNode != null)
                _SelectedRootWFNode.PropertyChanged -= _SelectedRootWFNode_PropertyChanged;

            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region BSO->ACProperty
        #region Presenter
        IACWorkflowContext _WFRootContext;
        /// <summary>
        /// Aktuell geladener Workflow (Entität: ACClassMethod, ACProgram, ACPartslist, ProgramACPartslist)
        /// Ist immer der Root-Workflow, also KEINE Untermethode des Root-Workflow
        /// </summary>
        [ACPropertyInfo(9999)]
        public IACWorkflowContext WFRootContext
        {
            get
            {
                return _WFRootContext;
            }
            set
            {
                _WFRootContext = value;
                OnPropertyChanged("WFRootContext");
            }
        }
        
        IACComponentPWNode _PresenterACWorkflowNode;
        /// <summary>
        /// Root-ACComponent-WorkflowNode (PWOfflineNode, PWProxy, lebende PW...)
        /// des geladenen WorkflowContext
        /// (entspricht ACComponent für WorkflowContext.RootACWorkflowNode)
        /// TODO: Bei lebenden PW... noch implementieren
        /// </summary>
        [ACPropertyInfo(9999)]
        public IACComponentPWNode PresenterACWorkflowNode
        {
            get
            {
                return _PresenterACWorkflowNode;
            }
            set
            {
                _PresenterACWorkflowNode = value;
                OnPropertyChanged("PresenterACWorkflowNode");
            }
        }
        #endregion

        #region Selection 

        /// <summary>
        /// Workflow der aktuellen Ansicht (ACClassMethod oder Partslist)
        /// </summary>
        [ACPropertyInfo(9999)]
        public IACWorkflowContext SelectedWFContext
        {
            get
            {
                if (_SelectedRootWFNode == null)
                    return null;
                return _SelectedRootWFNode.WFContext;
            }
        }

        IACComponentPWNode _SelectedRootWFNode;
        /// <summary>
        /// Root-ACComponent-WorkflowNode
        /// des gerade angezeigten Workflows in grafischer Darstellung (PWOfflineNode, PWProxy, lebende PW...)
        /// TODO: Bei lebenden PW... noch implementieren
        /// </summary>
        [ACPropertyInfo(9999)]
        public IACComponentPWNode SelectedRootWFNode
        {
            get
            {
                return _SelectedRootWFNode;
            }
            set
            {
                if (_SelectedRootWFNode != null)
                    _SelectedRootWFNode.PropertyChanged -= _SelectedRootWFNode_PropertyChanged;
                _SelectedRootWFNode = value;
                if (_SelectedRootWFNode != null)
                    _SelectedRootWFNode.PropertyChanged += _SelectedRootWFNode_PropertyChanged;
                OnPropertyChanged(Const.VBPresenter_SelectedRootWFNode);
            }
        }

        private void _SelectedRootWFNode_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e != null && e.PropertyName == Const.VBPresenter_SelectedRootWFNode)
            {
                OnSelectedRootWFNodeContextChanged();
            }
        }

        protected virtual void OnSelectedRootWFNodeContextChanged()
        {
            OnPropertyChanged(Const.VBPresenter_SelectedRootWFNode);
        }

        /// <summary>
        /// Aktuelle ausgewählter ACComponent-WorkflowNode in grafischer Darstellung (PWOfflineNode, PWProxy, lebende PW...)
        /// </summary>
        [ACPropertyInfo(9999)]
        public IACComponentPWNode SelectedWFNode
        {
            get
            {
                if (VBBSOSelectionManager == null) 
                    return null;
                else 
                    return (VBBSOSelectionManager as VBBSOSelectionManager).CurrentContentACObject as IACComponentPWNode;
            }
            set
            {
                if (VBBSOSelectionManager == null) 
                    return;
                else if (value == null)
                {
                    (VBBSOSelectionManager as VBBSOSelectionManager).SelectedVBControl = null;
                    (VBBSOSelectionManager as VBBSOSelectionManager).HighlightContentACObject(value);
                }
                else
                    (VBBSOSelectionManager as VBBSOSelectionManager).HighlightContentACObject(value);
            }
        }

       
        #endregion
        #endregion

        #region BSO->ACProperty
        //IACConfigModify _ProcessEntity;
        //[ACPropertyCurrent(9999, "ProcessEntity")]
        //public IACConfigModify CurrentProcessConfig
        //{
        //    get
        //    {
        //        return _ProcessEntity;
        //    }
        //    set
        //    {
        //        if (_ProcessEntity != value)
        //        {
        //            _ProcessEntity = value;
        //            OnPropertyChanged("CurrentProcessConfig");
        //            if (_ProcessEntity != null)
        //            {
        //                LoadPWRoot(_ProcessEntity.ACUrlCommand("ProgramACClassMethod") as ACClassMethod, null, null);
        //            }
        //        }
        //    }
        //}

        //VBDesignerWorkflow _CurrentPWRoot = null;
        //[ACPropertyCurrent(9999, "PWRoot")]
        //public VBDesignerWorkflow CurrentPWRoot
        //{
        //    get
        //    {
        //        return _CurrentPWRoot;
        //    }
        //    set
        //    {
        //        _CurrentPWRoot = value;
        //        OnPropertyChanged("CurrentPWRoot");
        //        OnPropertyChanged("PWRootACUrl");
        //    }
        //}

        //[ACPropertyCurrent(9999, "PWRoot", "en{'Caller'}de{'Aufrufer'}")]
        //public string PWRootACUrl
        //{
        //    get
        //    {
        //        if (_CurrentPWRoot.ParentPWNode != null)
        //            return _CurrentPWRoot.ParentPWNode.GetACUrl();
        //        return "";
        //    }
        //}

        //ACRef<ACComponent> _CurrentPWRootLive = null;
        //[ACPropertyCurrent(9999, "ProcessEntity")]
        //public ACRef<ACComponent> CurrentPWRootLive
        //{
        //    get
        //    {
        //        return _CurrentPWRootLive;
        //    }
        //    set
        //    {
        //        //if (CurrentPWRoot != null)
        //        //    CurrentPWRoot.ACDeInit();
        //        if (_CurrentPWRootLive != null)
        //            _CurrentPWRootLive.Detach();
        //        _CurrentPWRootLive = value;
        //        if (_CurrentPWRootLive != null && CurrentPWRoot != null)
        //        {
        //            LoadPWRoot(CurrentProcessConfig.ACUrlCommand("ProgramACClassMethod") as ACClassMethod, null, _CurrentPWRootLive.Obj);
        //        }
        //    }
        //}

        #region Designmanager
        //VBDesignerWorkflowMethod _DesignManagerWFACClassWF;
        //[ACPropertyInfo(9999)]
        //public VBDesignerWorkflowMethod DesignManagerWFACClassWF
        //{
        //    get
        //    {
        //        if (_DesignManagerWFACClassWF == null)
        //        {
        //            _DesignManagerWFACClassWF = this.ACUrlCommand("VBDesignerWorkflowMethod(CurrentPWRoot)") as VBDesignerWorkflowMethod;
        //            _CurrentPWRoot = DesignManagerWFACClassWF; 
        //        }
        //        return _DesignManagerWFACClassWF;
        //    }
        //}

        //public override string VBBSODiagnosticDialogACIdentifier
        //{
        //    get
        //    {
        //        return "VBBSODiagnosticDialog(CurrentPWRoot)";
        //    }
        //}

        #endregion

        #endregion

        #region SelectionManager

        private bool _SelectionManagerInitialized = false;

        public IACComponent VBBSOSelectionManager
        {
            get
            {
                IACComponent vbBSOSelectionManager = GetChildComponent(SelectionManagerACName);

                if (vbBSOSelectionManager == null)
                    vbBSOSelectionManager = StartComponent(SelectionManagerACName, null, null) as IACComponent;

                if (vbBSOSelectionManager != null && !_SelectionManagerInitialized)
                {
                    vbBSOSelectionManager.PropertyChanged += SelectionManagerPropertyChangedHandler;
                    _SelectionManagerInitialized = true;
                }

                return vbBSOSelectionManager;
            }
        }

        protected virtual void OnSelectionChanged()
        {
            OnPropertyChanged("SelectedWFNode");
        }

        private void SelectionManagerPropertyChangedHandler(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedVBControl") OnSelectionChanged();
        }

        //void _SelectionManager_ObjectAttached(object sender, EventArgs e)
        //{
        //    if (VBBSOSelectionManager != null)
        //        VBBSOSelectionManager.PropertyChanged += SelectionManager_PropertyChanged;
        //}

        //void _SelectionManager_ObjectDetaching(object sender, EventArgs e)
        //{
        //    if (VBBSOSelectionManager != null)
        //        VBBSOSelectionManager.PropertyChanged -= SelectionManager_PropertyChanged;
        //}

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

        //void SelectionManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == "SelectedVBControl")
        //    {
        //        OnPropertyChanged("SelectedVBControl");
        //        OnPropertyChanged("CurrentContentACObject");
        //    }
        //}
        #endregion

        public object GetRoutingLogic()
        {
            return Root?.WPFServices?.DesignerService.GetVBRoutingLogic();
        }
    }
}
