using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;
using System.Transactions;
using System.ComponentModel;

namespace gip.core.layoutengine
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

        public VBDockingContainerBase(VBDockingManager manager, UIElement vbDesignContent)
        {
            DockManager = manager;
            this.VBDesignContent = vbDesignContent;
            if ((this.VBDesignContent is FrameworkElement) && (this.VBDesignContent is IVBContent))
            {
                if (this.VBDesignContent is VBDesign)
                    (this.VBDesignContent as VBDesign).OnContextACObjectChanged += new EventHandler(VBDockingContainerBase_OnElementACComponentChanged);
                else
                    (this.VBDesignContent as FrameworkElement).DataContextChanged += new DependencyPropertyChangedEventHandler(VBDockingContainerBase_ContentDataContextChanged);
                (this.VBDesignContent as FrameworkElement).Loaded += new RoutedEventHandler(VBDockingContainerBase_ContentLoaded);
            }
        }

        void VBDockingContainerBase_OnElementACComponentChanged(object sender, EventArgs e)
        {
            AddToComponentReference();
            RefreshTitle();
        }

        void VBDockingContainerBase_ContentDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AddToComponentReference();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
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
            if ((this.VBDesignContent is FrameworkElement) && (this.VBDesignContent is IVBContent))
            {
                if (this.VBDesignContent is VBDesign)
                    (this.VBDesignContent as VBDesign).OnContextACObjectChanged -= VBDockingContainerBase_OnElementACComponentChanged;
                else
                    (this.VBDesignContent as FrameworkElement).DataContextChanged -= VBDockingContainerBase_ContentDataContextChanged;
                (this.VBDesignContent as FrameworkElement).Loaded -= VBDockingContainerBase_ContentLoaded;
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
                    string filterFrameworkElementName = (string)ACUrlCmdMessage.ACParameter[2];
                    string filterVBContent = (string)ACUrlCmdMessage.ACParameter[3];
                    string filterACNameOfComponent = (string)ACUrlCmdMessage.ACParameter[4];
                    bool withDialogStack = (bool)ACUrlCmdMessage.ACParameter[5];

                    bool filterVBControlClassNameSet = !String.IsNullOrEmpty(filterVBControlClassName);
                    bool filterFrameworkElementNameSet = !String.IsNullOrEmpty(filterFrameworkElementName);
                    bool filterACNameOfComponentSet = !String.IsNullOrEmpty(filterACNameOfComponent);
                    bool filterVBContentSet = !String.IsNullOrEmpty(filterVBContent);
                    if (!filterVBControlClassNameSet && !filterFrameworkElementNameSet && !filterACNameOfComponentSet && !filterVBContentSet)
                        return;

                    if (ACUrlHelper.IsSearchedGUIInstance(ACIdentifier, filterVBControlClassName, filterFrameworkElementName, filterVBContent, filterACNameOfComponent))
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

        public event RoutedEventHandler VBDesignLoaded;
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
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
        }

        public UIElement VBDesignContent
        {
            get;
            set;
        }

        IACObject _ACComponent = null;
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
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
                    if (((gip.core.layoutengine.VBDesign)(VBDesignContent)) != null &&
                        ((gip.core.layoutengine.VBDesign)(VBDesignContent)).DataContext is IACComponent &&
                        !string.IsNullOrEmpty(((gip.core.layoutengine.VBDesign)(VBDesignContent)).VBContent) &&
                        ((gip.core.layoutengine.VBDesign)(VBDesignContent)).VBContent.StartsWith("*"))
                    {
                        IACComponent acComponent = ((gip.core.layoutengine.VBDesign)(VBDesignContent)).DataContext as IACComponent;
                        ACClassDesign acClassDesign = acComponent.GetDesign(((gip.core.layoutengine.VBDesign)(VBDesignContent)).VBContent.Substring(1));
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

        public VBRibbon _VBRibbon;
        public void GenerateContentLayout(VBDockPanel dockPanel)
        {
            dockPanel.DataContext = ContextACObject;
            dockPanel.BSOACComponent = ContextACObject as IACBSO;

            if ((ContextACObject == null)
                || (VBDesignContent == null)
                || (DockManager == null))
                return;

            // Ribbon-Bar
            if (VBDockingManager.GetRibbonBarVisibility(VBDesignContent) != Global.ControlModes.Hidden)
            {
                _VBRibbon = new VBRibbon();
                _VBRibbon.SetValue(DockPanel.DockProperty, Dock.Top);
                if (VBDockingManager.GetRibbonBarVisibility(VBDesignContent) == Global.ControlModes.Collapsed)
                    _VBRibbon.Visibility = System.Windows.Visibility.Collapsed;
                else
                    _VBRibbon.Visibility = System.Windows.Visibility.Visible;
                dockPanel.Children.Add(_VBRibbon);
            }

            // Content
            if (VBDesignContent is VBDesign)
            {
                ((VBDesign)VBDesignContent).DataContext = ContextACObject;
                ((VBDesign)VBDesignContent).BSOACComponent = ContextACObject as IACBSO;
            }

            dockPanel.Children.Add(VBDesignContent);
            dockPanel.DataContext = ContextACObject;
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBDockingContainerBase), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly DependencyProperty ACUrlCmdMessageProperty =
            DependencyProperty.Register("ACUrlCmdMessage",
                typeof(ACUrlCmdMessage), typeof(VBDockingContainerBase),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

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
        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBDockingContainerBase),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }


        private static void OnDepPropChanged(DependencyObject dependencyObject,
               DependencyPropertyChangedEventArgs args)
        {
            VBDockingContainerBase thisControl = dependencyObject as VBDockingContainerBase;
            if (thisControl == null)
                return;
            if (args.Property == ACUrlCmdMessageProperty)
                thisControl.OnACUrlMessageReceived();
            else if (args.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (args.Property == BSOACComponentProperty)
            {
                if (args.NewValue == null && args.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = args.OldValue as IACBSO;
                    if (bso != null)
                        thisControl.DeInitVBControl(bso);
                }
            }
        }


        public Global.ControlModes VBRibbonVisibility
        {
            get
            {
                if (_VBRibbon == null)
                    return Global.ControlModes.Hidden;
                switch (_VBRibbon.Visibility)
                {
                    case System.Windows.Visibility.Hidden:
                        return Global.ControlModes.Hidden;
                    case System.Windows.Visibility.Collapsed:
                        return Global.ControlModes.Collapsed;
                    case System.Windows.Visibility.Visible:
                        return Global.ControlModes.Enabled;
                    default:
                        return Global.ControlModes.Hidden;
                }
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

        bool _ReferenceAdded = false;
        protected void AddToComponentReference()
        {
            if ((this.VBDesignContent == null) || _ReferenceAdded)
                return;
            if (ContextACObject is IACComponent)
            {
                Binding binding = new Binding();
                binding.Source = ContextACObject;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBDockingContainerBase.ACCompInitStateProperty, binding);

                binding = new Binding();
                binding.Source = ContextACObject;
                binding.Path = new PropertyPath(Const.ACUrlCmdMessage);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBDockingContainerBase.ACUrlCmdMessageProperty, binding);
                _ReferenceAdded = true;
            }
        }

        protected void RemoveFromComponentReference()
        {
            BindingOperations.ClearBinding(this, VBDockingContainerBase.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBDockingContainerBase.ACCompInitStateProperty);
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
                    VBDockingManager.SetWindowSize(VBDesignContent, new Size((int)VBDockingPanel.ActualWidth, (int)VBDockingPanel.ActualHeight));
                    VBDockingManager.SetCloseButtonVisibility(VBDesignContent, VBDockingManager.GetCloseButtonVisibility(VBDesignContent));
                }
                else if (VBDockingPanel is VBDockingPanelTabbedDoc)
                {
                    VBDockingPanelTabbedDoc toolWindow = (VBDockingPanel as VBDockingPanelTabbedDoc);
                    VBDockingManager.SetDockState(VBDesignContent, Global.VBDesignDockState.Tabbed);
                    if (_VBRibbon != null)
                        VBDockingManager.SetRibbonBarVisibility(VBDesignContent, VBRibbonVisibility);
                    VBDockingManager.SetWindowSize(VBDesignContent, new Size((int)VBDockingPanel.ActualWidth, (int)VBDockingPanel.ActualHeight));
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _IsClosingVBWindow = true;
            base.OnClosing(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            _IsClosingVBWindow = false;
            base.OnActivated(e);
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

