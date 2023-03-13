using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.layoutengine;
using gip.ext.design;
using gip.ext.designer;
using System.Windows.Media;
using System.Windows.Controls;

namespace gip.core.manager
{
    /// <summary>
    /// Der Designmanager dient zum verwalten beliebiger editierbarer Designs. Hierzu 
    /// zählen derzeit Maskenlayouts, Visualisierung und Workflows. Ist um beliebige
    /// weitere Designmanager zu erweitern
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Designer base class'}de{'Designer Basisklasse'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, false, false)]
    public abstract class VBDesigner : ACBSO, IACComponentDesignManager
    {
        #region c´tors
        public VBDesigner(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            ShowXMLEditor = false;
            PropertyWindowVisible = false;
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (_AvailableElementList == null)
                _AvailableElementList = new ObservableCollection<IACObject>();
            bool initialized = base.ACInit(startChildMode);
            if (initialized)
                _WPFProxy = Root?.WPFServices?.DesignerService?.GetDesignMangerProxy(this);
            return initialized;
        }

        public override bool ACPostInit()
        {
            if (_WPFProxy != null)
            {
                Root?.WPFServices?.DesignerService?.RemoveDesignMangerProxy(this);
                _WPFProxy = null;
            }
            _IsToolSelection = true;
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_SelectionManager != null)
            {
                _SelectionManager.Detach();
                _SelectionManager.ObjectDetaching -= _SelectionManager_ObjectDetaching;
                _SelectionManager.ObjectAttached -= _SelectionManager_ObjectAttached;
            }

            _SelectionManager = null;
            bool result = base.ACDeInit(deleteACClassTask);
            _CurrentDesign = null;
            _CurrentAvailableElement = null;
            _SelectedAvailableElement = null;
            _AvailableElementList = null;
            _CurrentAvailableTool = null;
            _SelectedAvailableTool = null;
            _ToolService = null;
            _VBDesignEditor = null;
            _CurrentDesignContext = null;
            return result;
        }

        #endregion

        #region IACComponentDesignManager Member
        IVBComponentDesignManagerProxy _WPFProxy;
        protected IVBComponentDesignManagerProxy WPFProxy
        {
            get 
            {
                return _WPFProxy;
            }
        }

        public virtual void InitDesignManager(string vbContentDesign)
        {
            _VBContentDesign = vbContentDesign;
            ParentPropertyChanged(vbContentDesign);
            if (ParentACComponent != null)
                this.ParentACComponent.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ParentACComponent_PropertyChanged);
            if (VBBSOSelectionManager != null)
                VBBSOSelectionManager.PropertyChanged += SelectionManager_PropertyChanged;
        }
        
        public virtual void DeInitDesignManager(string vbContentDesign)
        {
            if (ParentACComponent != null)
                this.ParentACComponent.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(ParentACComponent_PropertyChanged);
            if (VBBSOSelectionManager != null)
                VBBSOSelectionManager.PropertyChanged -= SelectionManager_PropertyChanged;
        }

        string _VBContentDesign;
        /// <summary>
        /// Property vom Type IACObjectDesign im übergeordneten BSO, welches durch den Designmanager darzustellen ist
        /// ACUrl relativ von ParentACComponent 
        /// </summary>
        public string VBContentDesign
        {
            get
            {
                return _VBContentDesign;
            }
        }


        /// <summary>
        /// Reference to the WPF-Control, that presents the XAML-Code of the CurrentDesign-Property
        /// </summary>
        /// <value>Reference to a WPF-Control that implements IVBContent (System.Windows.Controls.ContentControl)</value>
        public virtual IVBContent VBDesignControl
        {
            get;
            set;
        }


        /// <summary>
        /// Returns a reference to the tool-window (WPF-Control)
        /// </summary>
        /// <value>Reference to the tool-window</value>
        public virtual IACObject ToolWindow
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a reference to a window (WPF-Control) that shows the Dependency-Properties of the selected WFP-Control in the designer (SelectedVBControl).
        /// These dependency-properties can also be manipulated.
        /// </summary>
        /// <value>Reference to the tool-window</value>
        public virtual IACObject PropertyWindow
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Returns a reference to a window (WPF-Control) that shows the logical tree of the current design.
        /// </summary>
        /// <value>Reference to the logical tree window</value>
        public virtual IACObject LogicalTreeWindow
        {
            get
            {
                return null;
            }
        }

        public VBDesign VBDesign
        {
            get
            {
                if ((VBDesignControl != null) && (VBDesignControl is VBDesign))
                    return (VBDesignControl as VBDesign);
                return null;
            }
        }

        /// <summary>
        /// When the XAML-Code is loaded, the logical tree is assigned to the Content-Property of the VBDesignControl.
        /// When the user selects a child from this logical tree, which is a IVBContent, then this property contains a reference to it.
        /// </summary>
        /// <value>The selected WPF-Control that implements IVBContent</value>
        [ACPropertyInfo(9999)]
        public virtual IVBContent SelectedVBControl
        {
            get
            {
                if (VBBSOSelectionManager == null)
                    return null;
                return (VBBSOSelectionManager as VBBSOSelectionManager).SelectedVBControl;
            }
            set
            {
                if (VBBSOSelectionManager == null)
                    return;
                (VBBSOSelectionManager as VBBSOSelectionManager).OnVBControlClicked(SelectedVBControl);

                //OnPropertyChanged("SelectedVBControl");
            }
        }

        private bool _IsDesignerActive;
        [ACPropertyInfo(9999)]
        public bool IsDesignerActive
        {
            get
            {
                return _IsDesignerActive;
            }
            set
            {
                _IsDesignerActive = value;
                OnPropertyChanged("IsDesignerActive");
            }
        }

        [ACPropertyInfo(9999)]
        public abstract string DesignXAML { get; set; }

        /// <summary>
        /// Liefert den Tree der verfügbaren Elemente, die beim VBDesign eingefügt werden könnnen
        /// </summary>
        public abstract void UpdateAvailableElements();


        /// <summary>
        /// Creates a Edge between two points
        /// </summary>
        /// <param name="sourceVBConnector">The source VB connector.</param>
        /// <param name="targetVBConnector">The target VB connector.</param>
        public abstract void CreateEdge(IVBConnector sourceVBConnector, IVBConnector targetVBConnector);


        /// <summary>
        /// Checks if a Edge can be created between two points
        /// </summary>
        /// <param name="sourceVBConnector">The source VB connector.</param>
        /// <param name="targetVBConnector">The target VB connector.</param>
        /// <returns><c>true</c> if is enabled; otherwise, <c>false</c>.</returns>
        public abstract bool IsEnabledCreateEdge(IVBConnector sourceVBConnector, IVBConnector targetVBConnector);


        /// <summary>Asks this design manager if he can create edges</summary>
        /// <returns><c>true</c> if this instance can create edges; otherwise, <c>false</c>.</returns>
        public abstract bool CanManagerCreateEdges();


        /// <summary>
        /// Switches the this designer to design mode and the Designer-Tool (WPF-Control) appears on the gui.
        /// </summary>
        /// <param name="dockingManagerName">Name of the parent docking manager.</param>
        public virtual void ShowDesignManager(string dockingManagerName = "")
        {
            if (VBDesignControl == null || !(VBDesignControl is VBDesign))
                return;

            _VBDesignEditor = null;
            ((VBDesign)VBDesignControl).IsDesignerActive = true;
            IsDesignMode = true;
        }

        /// <summary>
        /// Switches the designer off and the Designer-Tool (WPF-Control) disappears on the gui.
        /// </summary>
        public virtual void HideDesignManager()
        {
            if (VBDesignControl == null || !(VBDesignControl is VBDesign))
                return;

            ((VBDesign)VBDesignControl).IsDesignerActive = false;
            IsDesignMode = false;
        }

        private bool _IsDesignMode;
        /// <summary>
        /// Gets a value indicating whether this instance is design mode.
        /// </summary>
        /// <value><c>true</c> if this instance is design mode; otherwise, <c>false</c>.</value>
        public bool IsDesignMode
        {
            get
            {
                return _IsDesignMode;
            }
            set
            {
                _IsDesignMode = value;
                OnPropertyChanged("IsDesignMode");
            }
        }
        #endregion

        #region Selection-Manager
        private ACRef<IACComponent> _SelectionManager;
        public IACComponent VBBSOSelectionManager
        {
            get
            {
                if (_SelectionManager != null)
                    return _SelectionManager.ValueT;
                if (ParentACComponent != null)
                {
                    IACComponent subACComponent = ParentACComponent.GetChildComponent(SelectionManagerACName);
                    if (subACComponent == null)
                    {
                        subACComponent = ParentACComponent.StartComponent(SelectionManagerACName, null, null) as IACComponent;
                    }
                    if (subACComponent != null)
                    {
                        _SelectionManager = new ACRef<IACComponent>(subACComponent, this);
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
            if (VBBSOSelectionManager != null)
                VBBSOSelectionManager.PropertyChanged += SelectionManager_PropertyChanged;
        }

        void _SelectionManager_ObjectDetaching(object sender, EventArgs e)
        {
            if (VBBSOSelectionManager != null)
                VBBSOSelectionManager.PropertyChanged -= SelectionManager_PropertyChanged;
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
            if (e.PropertyName == "SelectedVBControl")
            {
                OnPropertyChanged("SelectedVBControl");
                OnPropertyChanged("CurrentContentACObject");
            }
        }
        #endregion

        void ParentACComponent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ParentPropertyChanged(e.PropertyName);
        }

        protected virtual void ParentPropertyChanged(string propertyName)
        {
        }

        /// <summary>
        /// Controls if the "XAML-Editor"-Tab in the Designer-Control should be visible
        /// </summary>
        /// <value><c>true</c> if XAML-Editor is visible; otherwise, <c>false</c>.</value>
        public bool ShowXMLEditor
        {
            get;
            set;
        }

        /// <summary>
        /// Controls if the Property-Window in the Designer-Control should be visible
        /// </summary>
        /// <value><c>true</c> if Property-Window is visible; otherwise, <c>false</c>.</value>
        public bool PropertyWindowVisible
        {
            get;
            set;
        }


        /// <summary>
        /// Controls if the logical tree window in the Designer-Control should be visible
        /// </summary>
        /// <value><c>true</c> if Property-Window is visible; otherwise, <c>false</c>.</value>
        public bool LogicalTreeWindowVisible
        {
            get;
            set;
        }

        IACObjectDesign _CurrentDesign;
        /// <summary>
        /// Design that is currently presented in the designer (Its a class that implements IACObjectDesign: ACClassDesign, ACClassMethod, Partslist, PWOfflineNode, IACComponentPWNode etc.)
        /// </summary>
        /// <value>Reference to a instance of IACObjectDesign that propvides the XAML-Code that should be presented.</value>
        [ACPropertyInfo(9999)]
        public IACObjectDesign CurrentDesign
        {
            get
            {
                return _CurrentDesign;
            }
            set
            {
                if (_CurrentDesign != value)
                {
                    _CurrentDesign = value;
                    UpdateAvailableElements();
                    OnPropertyChanged("AvailableElementList");

                    OnPropertyChanged("CurrentDesign");
                    OnPropertyChanged("DesignXAML");
                }
            }
        }

        /// <summary>
        /// Aktuelles ausgewähltes VB-Steuerelement
        /// </summary>
        [ACPropertyInfo(9999)]
        public IACObject CurrentContentACObject
        {
            get
            {
                if (VBBSOSelectionManager == null)
                    return null;
                return (VBBSOSelectionManager as VBBSOSelectionManager).CurrentContentACObject;
            }
            set
            {
                if (VBBSOSelectionManager == null)
                    return;
                (VBBSOSelectionManager as VBBSOSelectionManager).HighlightContentACObject(value);
            }
        }

        public override ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            if (ParentACComponent != null)
            {
                return base.GetMenu("", "");
            }

            ACMenuItemList acMenuItemList = new ACMenuItemList();
            return acMenuItemList;
        }

        protected IACObject GetWindow(string vbDesignNameOfWindow)
        {
            return FindGui("", "", "*" + vbDesignNameOfWindow,this.ACIdentifier);
        }

        #region Available Elements
        bool _IsToolSelection = true;
        public bool IsToolSelection
        {
            get
            {
                return _IsToolSelection;
            }
            set
            {
                _IsToolSelection = value;
            }
        }

        ACObjectItem _CurrentAvailableElement;
        [ACPropertyCurrent(9999, "AvailableElement")]
        public ACObjectItem CurrentAvailableElement
        {
            get
            {
                return _CurrentAvailableElement;
            }
            set
            {
                _CurrentAvailableElement = value;
                OnPropertyChanged("CurrentAvailableElement");
                OnCurrentAvailableElementChanged();

                if (/*IsToolSelection && */ToolService != null)
                {
                    ITool newTool = null;
                    if (_CurrentAvailableElement is DesignManagerToolItem)
                    {
                        newTool = (_CurrentAvailableElement as DesignManagerToolItem).CreateControlTool;
                    }
                    else if (value != null && value.ACObject != null)
                    {
                        bool isTool = true;
                        if (value.ACObject is NodeInfo)
                        {
                            NodeInfo nodeInfo = value.ACObject as NodeInfo;
                            if (nodeInfo.PAACClass != null && nodeInfo.PWACClass == null && nodeInfo.PAACClass.ACKind == Global.ACKinds.TACApplicationManager)
                                isTool = false;
                        }
                        if (isTool)
                        {
                            newTool = DesignManagerControlTool.CreateNewInstance(this, value.ACObject, value.ACUrlRelative);
                        }
                    }
                    (ToolService as IToolService).CurrentTool = newTool ?? (ToolService as IToolService).PointerTool;
                }
            }
        }

        ACObjectItem _SelectedAvailableElement;
        [ACPropertySelected(9999, "AvailableElement")]
        public ACObjectItem SelectedAvailableElement
        {
            get
            {
                return _SelectedAvailableElement;
            }
            set
            {
                _SelectedAvailableElement = value;
                OnPropertyChanged("SelectedAvailableElement");
            }
        }

        protected ObservableCollection<IACObject> _AvailableElementList = new ObservableCollection<IACObject>();
        [ACPropertyList(9999, "AvailableElement")]
        public ObservableCollection<IACObject> AvailableElementList
        {
            get
            {
                if (_AvailableElementList == null)
                    _AvailableElementList = new ObservableCollection<IACObject>();
                return _AvailableElementList;
            }
        }

        public virtual void OnCurrentAvailableElementChanged()
        {
        }
        #endregion


        #region Available Tools

        ACObjectItem _CurrentAvailableTool;
        [ACPropertyCurrent(9999, "AvailableTool")]
        public ACObjectItem CurrentAvailableTool
        {
            get
            {
                return _CurrentAvailableTool;
            }
            set
            {
                _CurrentAvailableTool = value;
                OnPropertyChanged("CurrentAvailableTool");
                OnCurrentAvailableToolChanged();

                if (ToolService != null)
                {
                    ITool newTool = null;
                    if (_CurrentAvailableTool is DesignManagerToolItem)
                    {
                        newTool = (_CurrentAvailableTool as DesignManagerToolItem).CreateControlTool;
                    }
                    else if (value != null && value.ACObject != null)
                    {
                        newTool = DesignManagerControlTool.CreateNewInstance(this, value.ACObject, value.ACUrlRelative);
                    }
                    (ToolService as IToolService).CurrentTool = newTool ?? (ToolService as IToolService).PointerTool;
                }
            }
        }

        ACObjectItem _SelectedAvailableTool;
        [ACPropertySelected(9999, "AvailableTool")]
        public ACObjectItem SelectedAvailableTool
        {
            get
            {
                return _SelectedAvailableTool;
            }
            set
            {
                _SelectedAvailableTool = value;
                OnPropertyChanged("SelectedAvailableTool");
            }
        }

        [ACPropertyList(9999, "AvailableTool")]
        public IEnumerable<IACObject> AvailableToolList
        {
            get
            {
                return GetAvailableTools();
            }
        }

        public virtual void OnCurrentAvailableToolChanged()
        {
            if(DesignPanel != null)
                DesignPanel.Focus();
        }

        public abstract IEnumerable<IACObject> GetAvailableTools();
        #endregion

        #region Tool Service
        protected void ReloadToolService()
        {
            IToolService newService = null;
            if ((this.VBDesignEditor != null)
                && (this.DesignSurface != null)
                && (this.DesignSurface.DesignContext != null))
            {
                newService = this.DesignSurface.DesignContext.Services.Tool;
            }
            bool changed = false;
            if (_ToolService != newService)
            {
                if (_ToolService != null)
                    _ToolService.CurrentToolChanged -= OnCurrentToolChanged;
                changed = true;
                _ToolService = newService;
            }
            if (changed && (_ToolService != null))
            {
                _ToolService.CurrentToolChanged += OnCurrentToolChanged;
                OnCurrentToolChanged(null, null);
            }
        }

        IToolService _ToolService = null;

        /// <summary>
        /// The ToolService is the current graphical tool that the designer works with (Pointer, Line, Rectangle, Circle....)
        /// </summary>
        /// <value>Refence to the current tool</value>
        public object ToolService
        {
            get
            {
                ReloadToolService();
                return _ToolService;
            }
        }

        void OnCurrentToolChanged(object sender, EventArgs e)
        {
            object tagToFind;
            if (_ToolService.CurrentTool == _ToolService.PointerTool)
            {
                tagToFind = null;
            }
            else
            {
                tagToFind = _ToolService.CurrentTool;
            }
            /*if (sideBar.ActiveTab.ChoosedItem != null)
            {
                if (sideBar.ActiveTab.ChoosedItem.Tag == tagToFind)
                    return;
            }
            foreach (SideTabItem item in sideBar.ActiveTab.Items)
            {
                if (item.Tag == tagToFind)
                {
                    sideBar.ActiveTab.ChoosedItem = item;
                    sideBar.Refresh();
                    return;
                }
            }
            foreach (SideTab tab in sideBar.Tabs)
            {
                foreach (SideTabItem item in tab.Items)
                {
                    if (item.Tag == tagToFind)
                    {
                        sideBar.ActiveTab = tab;
                        sideBar.ActiveTab.ChoosedItem = item;
                        sideBar.Refresh();
                        return;
                    }
                }
            }
            sideBar.ActiveTab.ChoosedItem = null;
            sideBar.Refresh();*/
        }
        #endregion

        #region Design Editor
        IACInteractiveObject _VBDesignEditor;
        /// <summary>
        /// The Designer-Control that enables the graphically designing of the gui.
        /// (Manipulates also the XAML-Code)
        /// </summary>
        /// <value>Reference to the Designer-Tool (WPF-Control)</value>
        public IACInteractiveObject VBDesignEditor
        {
            get
            {
                if (_VBDesignEditor != null)
                    return _VBDesignEditor;
                _VBDesignEditor = WPFProxy?.GetVBDesignEditor(this);
                return _VBDesignEditor;
            }
        }

        public DesignSurface DesignSurface
        {
            get
            {
                return (this.VBDesignEditor as VBDesignEditor).DesignSurface;
            }
        }

        public DesignPanel DesignPanel
        {
            get
            {
                return this.VBDesignEditor != null ? (this.VBDesignEditor as VBDesignEditor).DesignSurface.DesignPanel : null;
            }
        }

#endregion

        #region CurrentDesignContext

        protected IACObject _CurrentDesignContext;
        /// <summary>The design context is the root datacontext for the XAML-Code where Bindings are related to. The Path in the VBContent is relative to this design context.</summary>
        /// <value>Datacontext for XAML-Code</value>
        public IACObject CurrentDesignContext
        {
            get
            {
                // Falls Design-Manager in Vavriobatch-Studio verwendet => DataContext des Designers ändert sich ständig durch Klassenauswahl
                if (_CurrentDesignContext != null)
                    return _CurrentDesignContext;
                if (ParentACComponent is IACComponentDesignManagerHost)
                    return (ParentACComponent as IACComponentDesignManagerHost).CurrentDesignContext;
                // Sonst ist DataContext der des instantierten BSO's
                return ParentACComponent;
            }
            set
            {
                _CurrentDesignContext = value;
                OnPropertyChanged("AvailableElementList");
                CurrentAvailableElement = null;
            }
        }
        #endregion

        //public void SetDesignPanel(object designPanel)
        //{
        //    _DesignPanel = designPanel as DesignPanel;
        //}

        //DesignPanel _DesignPanel;
        //public DesignPanel DesignPanel
        //{
        //    get
        //    {
        //        return _DesignPanel;
        //    }
        //}

        public enum LayoutActionType : short
        {
            Insert = 0,
            Delete = 1,
            Move = 2,
            InsertEdge = 3,
            DeleteEdge = 4,
        }

        public class VisualInfo
        {
            public LayoutActionType LayoutAction { get; set; }
            public IACObject VisualObject { get; set; }
            public string ACUrl { get; set; }
            public string ACUrl2 { get; set; }
            public Rect Position { get; set; }
        }

        List<VisualInfo> _VisualChangeList = new List<VisualInfo>();
        public List<VisualInfo> VisualChangeList
        {
            get
            {
                return _VisualChangeList;
            }
        }

        protected void AddToVisualChangeList(IACObject visualObject, LayoutActionType layoutAction = LayoutActionType.Insert, string acUrl = "", string acUrl2 = "", Rect position = new Rect())
        {
            _VisualChangeList.Add(new VisualInfo { VisualObject = visualObject, LayoutAction = layoutAction, ACUrl = acUrl, ACUrl2 = acUrl2, Position = position });
        }

        protected abstract void UpdateVisual();

        /// <summary>
        /// Opens the property window if it's closed.
        /// </summary>
        /// <param name="dockingManagerName">Name of the parent docking manager.</param>
        public virtual void ShowPropertyWindow(string dockingManagerName = "")
        {
        }


        /// <summary>
        /// Closes the property window.
        /// </summary>
        public virtual void ClosePropertyWindow()
        {
        }


        /// <summary>
        /// Opens the logical tree window if it's closed.
        /// </summary>
        /// <param name="dockingManagerName">Name of the parent docking manager.</param>
        public virtual void ShowLogicalTreeWindow(string dockingManagerName = "")
        {
        }


        /// <summary>
        /// Closes the logical tree window.
        /// </summary>
        public virtual void CloseLogicalTreeWindow()
        {
        }


        /// <summary>
        /// Builds the VBContent-String for the passed acUrl relatively to the CurrentDesignContext
        /// </summary>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="acObject">The ac object.</param>
        /// <returns>VBContent System.String</returns>
        public virtual string BuildVBContentFromACUrl(string acUrl, IACObject acObject)
        {
            return acUrl;
        }


        /// <summary>
        /// Gets the type for the passed acUrl
        /// </summary>
        /// <param name="acUrl">acUrl</param>
        /// <returns>IACType</returns>
        public virtual IACType GetTypeFromDBACUrl(string acUrl)
        {
            if (CurrentDesign == null)
                return null;
            ACClassDesign acClassDesign = CurrentDesign as ACClassDesign;
            if (acClassDesign == null)
                return null;
            IACType acType = acClassDesign.ACClass.GetTypeByACUrlComponent(acUrl);
            return acType;
        }


        /// <summary>
        /// Informs this design manager, that the Designer-Control (VBDesignEditor) was laoded on the gui
        /// </summary>
        /// <param name="designEditor">Reference to the Designer-Control (VBDesignEditor)</param>
        /// <param name="reverseToXaml">if set to <c>true</c> [reverse to xaml].</param>
        public virtual void OnDesignerLoaded(IVBContent designEditor, bool reverseToXaml)
        {
            if (!reverseToXaml)
            {
                VBPresenter vbPresenter = this.ParentACComponent as VBPresenter;
                if (vbPresenter != null && vbPresenter.RoutingLogic != null)
                {
                    vbPresenter.RoutingLogic.ClearVB();
                    VBDesignEditor vbDesignEditor = designEditor as VBDesignEditor;
                    if (vbDesignEditor != null)
                    {
                        DesignContext designContext = vbDesignEditor.DesignSurface.DesignContext;
                        if (designContext != null)
                        {
                            var designItemEdges = designContext.Services.Component.DesignItems.Where(c => c.View is VBEdge);
                            vbPresenter.RoutingLogic.InitWithDesignItems(designContext.RootItem,
                                designContext.Services.Component.DesignItems.Where(c => c.View is VBVisual || c.View is VBVisualGroup).Select(x => x.View).OfType<FrameworkElement>().ToList(),
                                designItemEdges.Select(c => c.View).OfType<FrameworkElement>(), designItemEdges);
                        }
                    }
                }
            }
        }

        public void CloseDockableWindow(IACObject window)
        {
            if (window is VBDockingContainerToolWindowVB)
            {
                VBDockingContainerToolWindow vbDockingContainerToolWindow = window as VBDockingContainerToolWindow;
                if (vbDockingContainerToolWindow != null)
                {
                    if (vbDockingContainerToolWindow.VBDockingPanel != null)
                    {
                        if (vbDockingContainerToolWindow.VBDockingPanel is VBDockingPanelToolWindow)
                        {
                            VBDockingPanelToolWindow VBDockingPanel = vbDockingContainerToolWindow.VBDockingPanel as VBDockingPanelToolWindow;
                            if (VBDockingPanel != null && VBDockingPanel.State != VBDockingPanelState.AutoHide)
                                VBDockingPanel.ChangeState(VBDockingPanelState.Hidden);
                            window.ACUrlCommand(Const.CmdClose);
                        }
                        else if (vbDockingContainerToolWindow.VBDockingPanel is VBDockingPanelTabbedDoc)
                        {
                            VBDockingPanelTabbedDoc VBDockingPanel = ((VBDockingContainerToolWindow)window).VBDockingPanel as VBDockingPanelTabbedDoc;
                            if (VBDockingPanel != null)
                                VBDockingPanel.RemoveDockingContainerToolWindow((VBDockingContainerBase)window);
                        }
                    }
                }
            }
            else if (window != null)
                window.ACUrlCommand(Const.CmdClose);
        }

        //TODO Ivan: routinglogic getbounds method - fix (problem with width and height)
        //add message(exception) more space between nodes, delete nodes or edges problem

        [ACMethodInfo("WF", "en{'Recalc edge routes'}de{'Verbindungslinien berechnen'}", 500, true)]
        public virtual void RecalcEdgeRouting()
        {
            OnDesignerLoaded(VBDesignEditor as VBDesignEditor, false);
            VBPresenter vbPresenter = this.ParentACComponent as VBPresenter;
            if (vbPresenter != null && vbPresenter.RoutingLogic != null)
            {
                UIElement rootCanvas = null;
                if (vbPresenter.RoutingLogic.RootDesignItem.View is VBCanvas)
                    rootCanvas = vbPresenter.RoutingLogic.RootDesignItem.View;
                else if (vbPresenter.RoutingLogic.RootDesignItem.View is VBScrollViewer)
                    rootCanvas = ((VBScrollViewer)vbPresenter.RoutingLogic.RootDesignItem.View).Content as UIElement;
                if (rootCanvas != null)
                    vbPresenter.RoutingLogic.CalculateEdgeRoute(rootCanvas);
            }
            DesignPanel.Focus();
        }

        /// <summary>
        /// Removes a WPF-Element from the design
        /// </summary>
        /// <param name="item">Item for delete.</param>
        /// <param name="isFromDesigner">If true, then call is invoked from this manager, else from gui</param>
        /// <returns><c>true</c> if is removed; otherwise, <c>false</c>.</returns>
        public virtual bool DeleteItem(object item, bool isFromDesigner=true)
        {
            return true;
        }
    }
}
