using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Base class for docking container.
    /// </summary>
    public abstract class VBDockingContainerBase : Window, IACInteractiveObject
    {
        #region c'tors
        public VBDockingContainerBase()
        {
        }

        public VBDockingContainerBase(VBDockingManager manager)
        {
            DockManager = manager;
        }

        public VBDockingContainerBase(VBDockingManager manager, Control vbDesignContent)
        {
            DockManager = manager;
            this.VBDesignContent = vbDesignContent;
            if ((this.VBDesignContent is Control) && (this.VBDesignContent is IVBContent))
            {
                if (this.VBDesignContent is VBDesign)
                    (this.VBDesignContent as VBDesign).OnContextACObjectChanged += new EventHandler(VBDockingContainerBase_OnElementACComponentChanged);
                else
                    (this.VBDesignContent as Control).DataContextChanged += VBDockingContainerBase_ContentDataContextChanged;
                (this.VBDesignContent as Control).Loaded += VBDockingContainerBase_ContentLoaded;
            }
        }

        void VBDockingContainerBase_OnElementACComponentChanged(object sender, EventArgs e)
        {
            OnUpdateDockPanelContextOnVBDesignContextChange();
            AddToComponentReference();
            RefreshTitle();
        }

        void VBDockingContainerBase_ContentDataContextChanged(object sender, EventArgs e)
        {
            AddToComponentReference();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        internal virtual void DeInitVBControl(IACComponent bso=null)
        {
            VBLogicalTreeHelper.DeInitVBControls(bso, this.VBDesignContent);
            VBVisualTreeHelper.DeInitVBControls(bso, this.VBDesignContent);
            RemoveFromComponentReference();
            if (_vbDockingPanel != null)
            {
                _vbDockingPanel.DeInitVBControl();
            }
            
            _vbDockingPanel = null;
            if ((this.VBDesignContent is Control) && (this.VBDesignContent is IVBContent))
            {
                if (this.VBDesignContent is VBDesign)
                    (this.VBDesignContent as VBDesign).OnContextACObjectChanged -= VBDockingContainerBase_OnElementACComponentChanged;
                else
                    (this.VBDesignContent as Control).DataContextChanged -= VBDockingContainerBase_ContentDataContextChanged;
                (this.VBDesignContent as Control).Loaded -= VBDockingContainerBase_ContentLoaded;
            }
            if (VBDesignContent is VBDesign)
            {
                ((VBDesign)VBDesignContent).DeInitVBControl(bso);
                ((VBDesign)VBDesignContent).BSOACComponent = null;
            }
            this.VBDesignContent = null;
            _ACComponent = null;
            _VBRibbon = null;
            DockManager = null;
        }

        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (ContextACObject != null && ContextACObject is IACComponent &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
            {
                //Close();
                //RemoveFromComponentReference();
                DeInitVBControl(ContextACObject as IACComponent);
            }
        }

        /// <summary>
        /// Handles the ACUrl message when it is received.
        /// </summary>
        public void OnACUrlMessageReceived()
        {
            if (ACUrlCmdMessage != null && ACUrlCmdMessage.ACUrl == Const.CmdFindGUI)
            {
                try
                {
                    IACObject invoker = (IACObject)ACUrlCmdMessage.ACParameter[0];
                    string filterVBControlClassName = (string)ACUrlCmdMessage.ACParameter[1];
                    string filterControlName = (string)ACUrlCmdMessage.ACParameter[2];
                    string filterVBContent = (string)ACUrlCmdMessage.ACParameter[3];
                    string filterACNameOfComponent = (string)ACUrlCmdMessage.ACParameter[4];
                    bool withDialogStack = (bool)ACUrlCmdMessage.ACParameter[5];

                    bool filterVBControlClassNameSet = !String.IsNullOrEmpty(filterVBControlClassName);
                    bool filterControlNameSet = !String.IsNullOrEmpty(filterControlName);
                    bool filterACNameOfComponentSet = !String.IsNullOrEmpty(filterACNameOfComponent);
                    bool filterVBContentSet = !String.IsNullOrEmpty(filterVBContent);
                    if (!filterVBControlClassNameSet && !filterControlNameSet && !filterACNameOfComponentSet && !filterVBContentSet)
                        return;

                    if (ACUrlHelper.IsSearchedGUIInstance(ACIdentifier, filterVBControlClassName, filterControlName, filterVBContent, filterACNameOfComponent))
                    {
                        invoker.ACUrlCommand(Const.CmdFindGUIResult, this);
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBDockingContainerBase", "OnACUrlMessagereceived", msg);
                }
            }
        }

        #endregion

        #region Docking-Framework

        public event EventHandler VBDesignLoaded;
        void VBDockingContainerBase_ContentLoaded(object sender, RoutedEventArgs e)
        {
            AddToComponentReference();
            RefreshTitle();
            if (VBDesignLoaded != null)
                VBDesignLoaded(this, e);
        }
        #endregion


        #region Properties Docking-Framework

        protected VBDockingPanelBase _vbDockingPanel = null;
        public VBDockingManager DockManager;

        #endregion

        #region Methods Docking-Framework
        // Returns true if a Default Content-Presenter should be added to TabItem
        // // Neues Tab einem Parent-DockPanel unter VBDockingPanelTabbedDoc.AddDockingContainer() zuordnen!!
        public virtual bool OnAddedToPanelTabbedDoc(VBTabItem vbTabItem, VBDockingPanelBase panel)
        {
            return true;
        }

        public virtual void OnRemovedPanelTabbedDoc(VBTabItem vbTabItem, VBDockingPanelBase panel)
        {
        }

        public VBDockingPanelBase VBDockingPanel
        {
            get { return _vbDockingPanel; }
        }

        internal void SetDockingPanel(VBDockingPanelBase panel)
        {
            _vbDockingPanel = panel;
        }
        #endregion

        #region Properties Extension
        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
        }

        public Control VBDesignContent
        {
            get;
            set;
        }

        IACObject _ACComponent = null;
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the Control.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                if (_ACComponent != null)
                    return _ACComponent;
                if ((VBDesignContent != null) && (VBDesignContent is IVBContent))
                    return (VBDesignContent as IVBContent).ContextACObject;
                if (DockManager == null)
                    return null;
                return DockManager.ContextACObject;
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public virtual string ACCaption
        {
            get
            {
                if (this.VBDesignContent == null)
                {
                    return "";
                }
                string acCaption = "";

                if ((VBDesignContent != null) && (VBDesignContent is IVBContent))
                    acCaption = (VBDesignContent as IVBContent).ACCaption;

                if (VBDesignContent is VBDesign)
                {
                    if (((gip.core.layoutengine.avui.VBDesign)(VBDesignContent)) != null &&
                        ((gip.core.layoutengine.avui.VBDesign)(VBDesignContent)).DataContext is IACComponent &&
                        !string.IsNullOrEmpty(((gip.core.layoutengine.avui.VBDesign)(VBDesignContent)).VBContent) &&
                        ((gip.core.layoutengine.avui.VBDesign)(VBDesignContent)).VBContent.StartsWith("*"))
                    {
                        IACComponent acComponent = ((gip.core.layoutengine.avui.VBDesign)(VBDesignContent)).DataContext as IACComponent;
                        ACClassDesign acClassDesign = acComponent.GetDesign(((gip.core.layoutengine.avui.VBDesign)(VBDesignContent)).VBContent.Substring(1));
                        if (acClassDesign != null)
                            return acClassDesign.ACCaption;
                    }
                }


                string title = VBDockingManager.GetWindowTitle(VBDesignContent);
                if (!String.IsNullOrEmpty(title))
                    acCaption = title;
                if (String.IsNullOrEmpty(acCaption) && (ContextACObject != null))
                    acCaption = ContextACObject.ACCaption;
                return acCaption;
            }
        }

        public VBRibbonBSODefault _VBRibbon;
        public void GenerateContentLayout(VBDockPanel dockPanel)
        {
            dockPanel.DataContext = ContextACObject;
            dockPanel.BSOACComponent = ContextACObject as IACBSO;

            if ((ContextACObject == null)
                || (VBDesignContent == null)
                || (DockManager == null))
                return;

            // Standardbar
            if (VBDockingManager.GetRibbonBarVisibility(VBDesignContent) != Global.ControlModes.Hidden)
            {
                _VBRibbon = new VBRibbonBSODefault();
                _VBRibbon.SetValue(DockPanel.DockProperty, Dock.Top);
                if (VBDockingManager.GetRibbonBarVisibility(VBDesignContent) == Global.ControlModes.Collapsed)
                    _VBRibbon.IsVisible = false;
                else
                    _VBRibbon.IsVisible = true;
                dockPanel.Children.Add(_VBRibbon);
            }
            // VBRibbon has to be declared in XAML of VBDesignContent
            //else
            //{
            //}

            // Content
            if (VBDesignContent is VBDesign)
            {
                ((VBDesign)VBDesignContent).DataContext = ContextACObject;
                ((VBDesign)VBDesignContent).BSOACComponent = ContextACObject as IACBSO;
            }

            // Avalonia has a different behaviour here. In WPF when VBDesignContent is added to dockPanel.Children,
            // then VBDesignContent (VBDesign) is initialized and VBDesign.InitBinding() is called to create a new Instance of the Bussiness-Object.
            // ContextACObject returns the VBDesignContent.ContextACObject which is the BSO.
            // Then the VBRibbon can be inintialized because it find it's inherited BSOACComponent
            // Therefore _NeedRefreshOnVBDesignContextChange ist set to true that on the OnContextACObjectChanged event OnUpdateDockPanelContextOnVBDesignContextChange will be called.
            _NeedRefreshOnVBDesignContextChange = true;
            dockPanel.Children.Add(VBDesignContent);
            dockPanel.DataContext = ContextACObject;
        }

        protected virtual void OnUpdateDockPanelContextOnVBDesignContextChange()
        {
            if (!_NeedRefreshOnVBDesignContextChange)
                return;

            if (this.VBDesignContent != null && RootPanel != null)
            {
                IACBSO bso = ContextACObject as IACBSO;
                foreach (var childDockPanel in RootPanel.Children.OfType<VBDockPanel>())
                {
                    childDockPanel.DataContext = ContextACObject;
                    if (bso != null)
                        childDockPanel.BSOACComponent = bso;
                }
                _NeedRefreshOnVBDesignContextChange = false;
            }
        }

        protected abstract VBDockPanel RootPanel { get; }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBDockingContainerBase>();        
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty = AvaloniaProperty.Register<VBDockingContainerBase, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));
        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty = AvaloniaProperty.Register<VBDockingContainerBase, ACInitState>(nameof(ACCompInitState));
        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }


        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ACUrlCmdMessageProperty)
                OnACUrlMessageReceived();
            else if (change.Property == ACCompInitStateProperty)
                InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        DeInitVBControl(bso);
                }
            }
            base.OnPropertyChanged(change);
        }


        public Global.ControlModes VBRibbonVisibility
        {
            get
            {
                if (_VBRibbon == null)
                    return Global.ControlModes.Hidden;
                return _VBRibbon.IsVisible ? Global.ControlModes.Enabled : Global.ControlModes.Collapsed;
            }
        }

        public Dock TranslateDock(Global.VBDesignDockPosition dock)
        {
            switch (dock)
            {
                case Global.VBDesignDockPosition.Bottom:
                    return Dock.Bottom;
                case Global.VBDesignDockPosition.Left:
                    return Dock.Left;
                case Global.VBDesignDockPosition.Right:
                    return Dock.Right;
                case Global.VBDesignDockPosition.Top:
                    return Dock.Top;
                default:
                    return Dock.Right;
            }
        }

        public Global.VBDesignDockPosition TranslateDock(Dock dock)
        {
            switch (dock)
            {
                case Dock.Bottom:
                    return Global.VBDesignDockPosition.Bottom;
                case Dock.Left:
                    return Global.VBDesignDockPosition.Left;
                case Dock.Right:
                    return Global.VBDesignDockPosition.Right;
                case Dock.Top:
                    return Global.VBDesignDockPosition.Top;
                default:
                    return Global.VBDesignDockPosition.Right;
            }
        }

        public Global.VBDesignDockState TranslateDockingPanelState(VBDockingPanelState state)
        {
            switch (state)
            {
                case VBDockingPanelState.AutoHide:
                case VBDockingPanelState.Hidden:
                    return Global.VBDesignDockState.AutoHideButton;
                case VBDockingPanelState.Docked:
                case VBDockingPanelState.FloatingWindow:
                    return Global.VBDesignDockState.FloatingWindow;
                case VBDockingPanelState.DockableWindow:
                    return Global.VBDesignDockState.Docked;
                case VBDockingPanelState.TabbedDocument:
                    return Global.VBDesignDockState.Tabbed;
                default:
                    return Global.VBDesignDockState.Tabbed;
            }
        }
        #endregion

        #region Methods Extension
        public virtual void RefreshTitle()
        {
            Title = ACCaption;
        }

        public virtual void ReInitDataContext()
        {
        }

        protected bool _NeedRefreshOnVBDesignContextChange = false;
        bool _ReferenceAdded = false;
        protected void AddToComponentReference()
        {
            if ((this.VBDesignContent == null) || _ReferenceAdded)
                return;
            if (ContextACObject is IACComponent)
            {
                Binding binding = new Binding();
                binding.Source = ContextACObject;
                binding.Path = Const.InitState;
                binding.Mode = BindingMode.OneWay;
                this.Bind(VBDockingContainerBase.ACCompInitStateProperty, binding);

                binding = new Binding();
                binding.Source = ContextACObject;
                binding.Path = Const.ACUrlCmdMessage;
                binding.Mode = BindingMode.OneWay;
                this.Bind(VBDockingContainerBase.ACUrlCmdMessageProperty, binding);
                _ReferenceAdded = true;
            }
        }

        protected void RemoveFromComponentReference()
        {
            this.ClearAllBindings();
            _ReferenceAdded = false;
        }

        public virtual void PersistStateToVBDesignContent()
        {
            if (VBDesignContent == null)
                return;
            if (VBDockingPanel != null)
            {
                if (VBDockingPanel is VBDockingPanelToolWindow)
                {
                    VBDockingPanelToolWindow toolWindow = (VBDockingPanel as VBDockingPanelToolWindow);
                    VBDockingManager.SetDockPosition(VBDesignContent, TranslateDock(toolWindow.Dock));
                    VBDockingManager.SetDockState(VBDesignContent, TranslateDockingPanelState(toolWindow.State));
                    if (_VBRibbon != null)
                        VBDockingManager.SetRibbonBarVisibility(VBDesignContent, VBRibbonVisibility);
                    VBDockingManager.SetWindowSize(VBDesignContent, new Size((int)VBDockingPanel.Bounds.Width, (int)VBDockingPanel.Bounds.Height));
                    VBDockingManager.SetCloseButtonVisibility(VBDesignContent, VBDockingManager.GetCloseButtonVisibility(VBDesignContent));
                }
                else if (VBDockingPanel is VBDockingPanelTabbedDoc)
                {
                    VBDockingPanelTabbedDoc toolWindow = (VBDockingPanel as VBDockingPanelTabbedDoc);
                    VBDockingManager.SetDockState(VBDesignContent, Global.VBDesignDockState.Tabbed);
                    if (_VBRibbon != null)
                        VBDockingManager.SetRibbonBarVisibility(VBDesignContent, VBRibbonVisibility);
                    VBDockingManager.SetWindowSize(VBDesignContent, new Size((int)VBDockingPanel.Bounds.Width, (int)VBDockingPanel.Bounds.Height));
                }
            }
        }
        #endregion

        #region IACUrl Member
        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }


        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public virtual object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public virtual string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        [ACMethodInfo("", "", 9999)]
        public string GetACUrlComponent(IACObject rootACObject = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
            }
        }
        #endregion

        #region IACObjectWithBinding Member
        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public virtual string ACIdentifier
        {
            get { return ACUrlHelper.BuildACNameForGUI(this, Name); }
        }

        #endregion


        private bool _IsClosingVBWindow = false;
        public virtual bool OnCloseWindow()
        {
            if (_IsClosingVBWindow)
                return _IsClosingVBWindow;
            if (this.ContextACObject != null)
            {
                this.ContextACObject.ACUrlCommand(Const.CmdOnCloseWindow, null);
            }
            try
            {
                Close();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDockingContainerBase", "OnCloseWindow", msg);
            }
            return true;
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            _IsClosingVBWindow = true;
            base.OnClosing(e);
        }

        protected override void OnOpened(EventArgs e)
        {
            _IsClosingVBWindow = false;
            base.OnOpened(e);
        }

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get
            {
                if ((VBDesignContent != null) && (VBDesignContent is IVBContent))
                    return (VBDesignContent as IVBContent).VBContent;
                return "";
            }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return false;
        }
    }
}

