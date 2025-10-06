using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System.Transactions;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control for docking in different views.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zum andocken verschiedener Ansichten.
    /// </summary>
    [TemplatePart(Name = "PART_BorderFreeze", Type = typeof(Border))]
    [TemplatePart(Name = "PART_btnPanelLeft", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_btnPanelRight", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_btnPanelTop", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_btnPanelBottom", Type = typeof(StackPanel))]
    [TemplatePart(Name = "PART_gridDocking", Type = typeof(VBDockingGrid))]
    [TemplatePart(Name = "PART_panelFront", Type = typeof(DockPanel))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDockingManager'}de{'VBDockingManager'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.NotStorable, true, false)]
    public class VBDockingManager : ContentControl, IVBDockDropSurface, IACInteractiveObject, IACObject, IVBGui
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "DockingManagerStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDocking/Themes/DockingManagerStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "DockingManagerStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDocking/Themes/DockingManagerStyleAero.xaml" },
        };

        public event EventHandler OnInitVBControlFinished;

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBDockingManager()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBDockingManager), new FrameworkPropertyMetadata(typeof(VBDockingManager)));
        }


        #region c'tors
        bool _themeApplied = false;
        public VBDockingManager()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.DataContextChanged += VBDockingManager_DataContextChanged;
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
            object partObj = (object)GetTemplateChild("PART_BorderFreeze");
            if ((partObj != null) && (partObj is Border))
            {
                _PART_BorderFreeze = ((Border)partObj);
            }

            partObj = (object)GetTemplateChild("PART_btnPanelLeft");
            if ((partObj != null) && (partObj is StackPanel))
            {
                _PART_btnPanelLeft = ((StackPanel)partObj);
            }

            partObj = (object)GetTemplateChild("PART_btnPanelRight");
            if ((partObj != null) && (partObj is StackPanel))
            {
                _PART_btnPanelRight = ((StackPanel)partObj);
            }

            partObj = (object)GetTemplateChild("PART_btnPanelTop");
            if ((partObj != null) && (partObj is StackPanel))
            {
                _PART_btnPanelTop = ((StackPanel)partObj);
            }

            partObj = (object)GetTemplateChild("PART_btnPanelBottom");
            if ((partObj != null) && (partObj is StackPanel))
            {
                _PART_btnPanelBottom = ((StackPanel)partObj);
            }

            partObj = (object)GetTemplateChild("PART_gridDocking");
            if ((partObj != null) && (partObj is VBDockingGrid))
            {
                _PART_gridDocking = ((VBDockingGrid)partObj);
                _PART_gridDocking.MouseEnter += OnHideAutoHidePane;
                _PART_gridDocking.MouseDown += new MouseButtonEventHandler(_PART_gridDocking_MouseDown);
            }

            partObj = (object)GetTemplateChild("PART_panelFront");
            if ((partObj != null) && (partObj is DockPanel))
            {
                _PART_panelFront = ((DockPanel)partObj);
            }
            InitVBControl();
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }
        #endregion

        #region PART's
        private Border _PART_BorderFreeze;
        public Border PART_BorderFreeze
        {
            get
            {
                return _PART_BorderFreeze;
            }
        }

        private StackPanel _PART_btnPanelLeft;
        public StackPanel PART_btnPanelLeft
        {
            get
            {
                return _PART_btnPanelLeft;
            }
        }

        private StackPanel _PART_btnPanelRight;
        public StackPanel PART_btnPanelRight
        {
            get
            {
                return _PART_btnPanelRight;
            }
        }

        private StackPanel _PART_btnPanelTop;
        public StackPanel PART_btnPanelTop
        {
            get
            {
                return _PART_btnPanelTop;
            }
        }

        private StackPanel _PART_btnPanelBottom;
        public StackPanel PART_btnPanelBottom
        {
            get
            {
                return _PART_btnPanelBottom;
            }
        }

        private VBDockingGrid _PART_gridDocking;
        public VBDockingGrid PART_gridDocking
        {
            get
            {
                return _PART_gridDocking;
            }
        }

        private DockPanel _PART_panelFront;
        public DockPanel PART_panelFront
        {
            get
            {
                return _PART_panelFront;
            }
        }

  

                   


        #endregion

        #region Deklarative Properties (XAML)
        public static readonly DependencyProperty ContainerProperty =
                               DependencyProperty.RegisterAttached("Container",
                                                                    typeof(Global.VBDesignContainer),
                                                                    typeof(VBDockingManager),
                                                                    new FrameworkPropertyMetadata(Global.VBDesignContainer.TabItem,
                                                                                                  FrameworkPropertyMetadataOptions.None));
        public static readonly DependencyProperty DockStateProperty =
                               DependencyProperty.RegisterAttached("DockState",
                                                                    typeof(Global.VBDesignDockState),
                                                                    typeof(VBDockingManager),
                                                                    new FrameworkPropertyMetadata(Global.VBDesignDockState.Tabbed,
                                                                                                  FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty DockPositionProperty =
                               DependencyProperty.RegisterAttached("DockPosition",
                                                                    typeof(Global.VBDesignDockPosition),
                                                                    typeof(VBDockingManager),
                                                                    new FrameworkPropertyMetadata(Global.VBDesignDockPosition.Bottom,
                                                                                                  FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty RibbonBarVisibilityProperty =
                               DependencyProperty.RegisterAttached("RibbonBarVisibility",
                                                                    typeof(Global.ControlModes),
                                                                    typeof(VBDockingManager),
                                                                    new FrameworkPropertyMetadata(Global.ControlModes.Hidden,
                                                                                                  FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty IsCloseableBSORootProperty =
                               DependencyProperty.RegisterAttached("IsCloseableBSORoot",
                                                                    typeof(bool),
                                                                    typeof(VBDockingManager),
                                                                    new FrameworkPropertyMetadata(false,
                                                                                                  FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty DisableDockingOnClickProperty =
                               DependencyProperty.RegisterAttached("DisableDockingOnClick",
                                                                    typeof(bool),
                                                                    typeof(VBDockingManager),
                                                                    new FrameworkPropertyMetadata(false,
                                                                                                  FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty WindowSizeProperty =
                               DependencyProperty.RegisterAttached("WindowSize",
                                                                    typeof(Size),
                                                                    typeof(VBDockingManager),
                                                                    new FrameworkPropertyMetadata(new Size(),
                                                                                                  FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty WindowTitleProperty =
                               DependencyProperty.RegisterAttached("WindowTitle",
                                                                    typeof(String),
                                                                    typeof(VBDockingManager));

        public static readonly DependencyProperty PART_closeButtonVisibilityProperty =
                               DependencyProperty.RegisterAttached("PART_closeButtonVisibility",
                                                                    typeof(Global.ControlModes),
                                                                    typeof(VBDockingManager),
                                                                    new FrameworkPropertyMetadata(Global.ControlModes.Hidden,
                                                                                                  FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty CloseButtonVisibilityProperty =
               DependencyProperty.RegisterAttached("CloseButtonVisibility",
                                                    typeof(Global.ControlModes),
                                                    typeof(VBDockingManager),
                                                    new FrameworkPropertyMetadata(Global.ControlModes.Hidden,
                                                                                  FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty TabVisibilityACUrlProperty =
                               DependencyProperty.RegisterAttached("TabVisibilityACUrl",
                                                                    typeof(string),
                                                                    typeof(VBDockingManager),
                                                                    new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty TabItemMinHeightProperty =
                               DependencyProperty.Register("TabItemMinHeight",
                                                                    typeof(double),
                                                                    typeof(VBDockingManager),
                                                                    new FrameworkPropertyMetadata(0.0,FrameworkPropertyMetadataOptions.None));


        public string ACClassUrl
        {
            get;
            set;
        }

        public List<Control> _VBDesignList = new List<Control>();
        [Content]
        public List<Control> VBDesignList
        {
            get
            {
                return _VBDesignList;
            }
            set
            {
                _VBDesignList = value;
            }
        }


        #endregion

        #region Methods riobatch-Extension

        #region Init and Release
        bool _Loaded = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Loaded || PART_gridDocking == null)
                return;
            _Loaded = true;

            AddToComponentReference();

            DragPanelServices.Register(this);

            _overlayWindow = new VBDockingOverlayWindow(this);

            PART_gridDocking.AttachDockManager(this);
            PART_gridDocking.vbDockingPanelTabbedDoc.Show();
            AddSelectionChangedHandler();

            if (VBDesignList != null)
            {
                int count = 0;
                VBPropertyGridView gridView = null;
                VBDesignEditor designEditor = null;
                VBDesignItemTreeView logicalTreeView = null;
                foreach (Control uiElement in VBDesignList)
                {
                    count++;
                    ShowVBDesign(uiElement);
                    //ShowWindow(ContextACObject as IACComponent, "FavBar", true, Global.VBDesignContainer.DockableWindow, Global.VBDesignDockState.AutoHideButton, Global.VBDesignDockPosition.Right, Global.ControlModes.Hidden);
                    if (uiElement is VBPropertyGridView)
                        gridView = (uiElement as VBPropertyGridView);
                    else if (uiElement is VBDesignEditor)
                        designEditor = (uiElement as VBDesignEditor);
                    else if (uiElement is VBDesignItemTreeView)
                        logicalTreeView = (uiElement as VBDesignItemTreeView);
                }

                if ((designEditor != null) && (gridView != null))
                    designEditor.PropertyGridView = gridView;
                if ((designEditor != null) && (logicalTreeView != null))
                    designEditor.DesignItemTreeView = logicalTreeView;
            }

            Binding bindingVarioWPF = new Binding();
            bindingVarioWPF.Source = this.Root().RootPageWPF;
            bindingVarioWPF.Path = new PropertyPath("VBDockingManagerFreezing");
            bindingVarioWPF.Mode = BindingMode.OneWay;
            this.SetBinding(VBDockingManager.MasterPageFreezeProperty, bindingVarioWPF);

            AddSelectionChangedHandler();
            if (OnInitVBControlFinished != null)
            {
                OnInitVBControlFinished(this, new EventArgs());
            }
        }

        protected virtual void DeInitVBControl(IACComponent bso = null)
        {
            foreach (VBDockingContainerToolWindow content in ToolWindowContainerList.ToList())
            {
                if (content.VBDockingPanel is VBDockingPanelToolWindow)
                    RemoveDockingPanelToolWindow(content.VBDockingPanel as VBDockingPanelToolWindow);
                content.Close();
                content.DeInitVBControl();
            }
            foreach (VBDockingContainerTabbedDoc docContent in TabbedDocContainerList.ToList())
            {
                docContent.CloseTab();
                //docContent.DeInitVBControl();
            }
            if (_overlayWindow != null)
                _overlayWindow.Close();

            if (PART_gridDocking != null)
            {
                PART_gridDocking.MouseEnter -= OnHideAutoHidePane;
                PART_gridDocking.MouseDown -= _PART_gridDocking_MouseDown;
                PART_gridDocking.DeInitVBControl();
            }

            if (_dockingBtnGroups != null)
            {
                foreach (VBDockingButtonGroup group in _dockingBtnGroups)
                {
                    foreach (VBDockingButton button in group.Buttons)
                    {
                        if (button.DockingContainerToolWindow != null)
                        {
                            button.DockingContainerToolWindow.DeInitVBControl();
                            button.DockingContainerToolWindow = null;
                        }
                    }
                    group.Buttons.Clear();
                }
                _dockingBtnGroups.Clear();
            }

            if (VBDesignList != null)
            {
                VBDesignList.Clear();
                //foreach (Control uiElement in VBDesignList)
                //{
                //}
            }
            RemoveSelectionChangedHandler();
            BindingOperations.ClearBinding(this, VBDockingManager.MasterPageFreezeProperty);
            this.ClearAllBindings();
            _dockingBtnGroups = null;
            _overlayWindow = null;

            _tempPane = null;
            _currentButton = null;
            _ParentWindow = null;
            _dragPanelServices = null;
            _overlayWindow = null;
            _PART_BorderFreeze = null;
            _PART_btnPanelLeft = null;
            _PART_btnPanelRight = null;
            _PART_btnPanelTop = null;
            _PART_btnPanelBottom = null;
            _PART_gridDocking = null;
            _PART_panelFront = null;
            this.DataContextChanged -= VBDockingManager_DataContextChanged;
        }

        protected void AddToComponentReference()
        {
            if (ContextACObject is IACComponent)
            {
                Binding binding = new Binding();
                binding.Source = ContextACObject;
                binding.Path = new PropertyPath(Const.ACUrlCmdMessage);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBDockingManager.ACUrlCmdMessageProperty, binding);

                binding = new Binding();
                binding.Source = ContextACObject;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBDockingManager.ACCompInitStateProperty, binding);
            }
        }

        protected void RemoveFromComponentReference()
        {
            BindingOperations.ClearBinding(this, VBDockingManager.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBDockingManager.ACCompInitStateProperty);
        }

        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (ContextACObject != null && ContextACObject is IACComponent && (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
            {
                RemoveFromComponentReference();
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
                        if (withDialogStack)
                        {
                            if (DialogStack.Any())
                                invoker.ACUrlCommand(Const.CmdFindGUIResult,this);
                        }
                        else
                            invoker.ACUrlCommand(Const.CmdFindGUIResult, this);
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBDockingManager", "OnACUrlMessagereceived", msg);
                }
            }
        }

        #endregion

        #region AccessMethods Attached Properties
        [AttachedPropertyBrowsableForChildren]
        public static void SetContainer(Control element, Global.VBDesignContainer value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.ContainerProperty, value);
        }

        [AttachedPropertyBrowsableForChildren]
        public static Global.VBDesignContainer GetContainer(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (Global.VBDesignContainer)element.GetValue(VBDockingManager.ContainerProperty);
        }


        [AttachedPropertyBrowsableForChildren]
        public static void SetDockState(Control element, Global.VBDesignDockState value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.DockStateProperty, value);
        }

        [AttachedPropertyBrowsableForChildren]
        public static Global.VBDesignDockState GetDockState(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (Global.VBDesignDockState)element.GetValue(VBDockingManager.DockStateProperty);
        }

        [AttachedPropertyBrowsableForChildren]
        public static void SetDockPosition(Control element, Global.VBDesignDockPosition value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.DockPositionProperty, value);
        }

        [AttachedPropertyBrowsableForChildren]
        public static Global.VBDesignDockPosition GetDockPosition(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (Global.VBDesignDockPosition)element.GetValue(VBDockingManager.DockPositionProperty);
        }


        [AttachedPropertyBrowsableForChildren]
        public static void SetRibbonBarVisibility(Control element, Global.ControlModes value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.RibbonBarVisibilityProperty, value);
        }

        [AttachedPropertyBrowsableForChildren]
        public static Global.ControlModes GetRibbonBarVisibility(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (Global.ControlModes)element.GetValue(VBDockingManager.RibbonBarVisibilityProperty);
        }


        [AttachedPropertyBrowsableForChildren]
        public static void SetIsCloseableBSORoot(Control element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.IsCloseableBSORootProperty, value);
        }

        [AttachedPropertyBrowsableForChildren]
        public static bool GetIsCloseableBSORoot(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (bool)element.GetValue(VBDockingManager.IsCloseableBSORootProperty);
        }

        [AttachedPropertyBrowsableForChildren]
        public static void SetDisableDockingOnClick(Control element, bool value)
        {
            if (element == null)
            {
                return;
                //throw new ArgumentNullException("element");
            }
            element.SetValue(VBDockingManager.DisableDockingOnClickProperty, value);
        }

        [AttachedPropertyBrowsableForChildren]
        public static bool GetDisableDockingOnClick(Control element)
        {
            if (element == null)
            {
                return false;
                //throw new ArgumentNullException("element");
            }
            return (bool)element.GetValue(VBDockingManager.DisableDockingOnClickProperty);
        }


        [AttachedPropertyBrowsableForChildren]
        public static void SetWindowSize(Control element, Size value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.WindowSizeProperty, value);
        }

        [AttachedPropertyBrowsableForChildren]
        public static Size GetWindowSize(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (Size)element.GetValue(VBDockingManager.WindowSizeProperty);
        }


        [AttachedPropertyBrowsableForChildren]
        public static void SetWindowTitle(Control element, String value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.WindowTitleProperty, value);
        }

        [AttachedPropertyBrowsableForChildren]
        public static String GetWindowTitle(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (String)element.GetValue(VBDockingManager.WindowTitleProperty);
        }

        [AttachedPropertyBrowsableForChildren]
        public static void SetCloseButtonVisibility(Control element, Global.ControlModes value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.CloseButtonVisibilityProperty, value);
        }

        [AttachedPropertyBrowsableForChildren]
        public static Global.ControlModes GetCloseButtonVisibility(Control element)
        {
            if (element == null)
            {
                return Global.ControlModes.Enabled;
                //throw new ArgumentNullException("element");
            }
            return (Global.ControlModes)element.GetValue(VBDockingManager.CloseButtonVisibilityProperty);
        }



        [AttachedPropertyBrowsableForChildren]
        public static void SetTabVisibilityACUrl(Control element, string value)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            element.SetValue(VBDockingManager.TabVisibilityACUrlProperty, value);
        }

        [AttachedPropertyBrowsableForChildren]
        public static string GetTabVisibilityACUrl(Control element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return (string)element.GetValue(VBDockingManager.TabVisibilityACUrlProperty);
        }

        public double TabItemMinHeight
        {
            set { SetValue(TabItemMinHeightProperty, value); }
            get { return (double)GetValue(TabItemMinHeightProperty); }
        }

        #endregion
        #endregion

        //IVBContent
        #region IACUrl Member

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return ACUrlHelper.BuildACNameForGUI(this, Name); }
        }

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
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            if (acUrl == Const.EventDeInit)
            {
                RemoveFromComponentReference();
                DeInitVBControl();
            }
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
            if (acUrl == Const.EventDeInit)
            {
                return true;
            }
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

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                return null;
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

        #region public members

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        public static readonly DependencyProperty IsBSOManagerProperty
            = DependencyProperty.Register("IsBSOManager", typeof(bool), typeof(VBDockingManager));
        [Category("VBControl")]
        public bool IsBSOManager
        {
            get { return (bool)GetValue(IsBSOManagerProperty); }
            set { SetValue(IsBSOManagerProperty, value); }
        }
        #region Show Methods for Dynamic Instances


        //Close Undocked Window :
        //VBWindowDockingUndocked.OnCloseButtonClicked()
        //->VBDockingPanelToolWindowUndocked.SwitchToClosedWindow()
        //VBDockingContainerBase.SetDockingPanel();
        //VBDockingPanelToolWindow.Close();-
        //    VBDockingPanelBase.Close();-
        //    VBDockDragPanelServices.Unregister();
        //    VBWindowDockingUndocked.OnClosing();-
        //VBDockingPanelToolWindow.Close();-


        //Drop Window:
        //VBWindowDockingUndocked.OnClosing()-
        //VBDockingPanelToolWindow.Close();-

        //AutoHide:
        //VBDockingPanelToolWindowUndocked.SwitchToAutoHideWindow();-
        //VBDockingPanelToolWindowUndocked.Close();-
        //VBDockingPanelToolWindow.Close();-
        //VBWindowDockingUndocked.OnClosing()-
        //VBDockingPanelToolWindow.Close();-
        //VBDockingPanelBase.Close();-


        //Close Docked Window:
        //-> VBDockingPanelToolWindow.Close.OnBtnCloseMouseDown();
        //VBDockingPanelToolWindow.CloseWindow();
        //->VBDockingContainerToolWindow.OnCloseWindow();
        //VBDockingPanelToolWindow.Close();-

        //Close Tabbed Window:
        //->VBDockingPanelTabbedDoc.OnTabItemMouseDown()


        //TAB:
        //-------------

        //new:
        //VBDockingPanelTabbedDoc.AddDockingContainer();

        //Tab Close:
        //VBDockingPanelTabbedDoc.OnTabItemMouseDown()

        #region Design
        [ACMethodInfo("", "en{'Modal Dialog'}de{'Modaler Dialog'}", 9999)]
        public void ShowDialog(IACComponent forObject, string acClassDesignName, string acCaption = "", bool isClosableBSORoot = false, 
            Global.ControlModes ribbonVisibility = Global.ControlModes.Hidden, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled)
        {
            VBDesign vbDesign = new VBDesign();
            vbDesign.DataContext = forObject;
            vbDesign.VBContent = "*" + acClassDesignName;
            VBDockingManager.SetContainer(vbDesign, Global.VBDesignContainer.ModalDialog);
            VBDockingManager.SetRibbonBarVisibility(vbDesign, ribbonVisibility);
            VBDockingManager.SetCloseButtonVisibility(vbDesign, closeButtonVisibility);
            ShowVBDesign(vbDesign, acCaption);
        }

        [ACMethodInfo("", "en{'Show Layout'}de{'Layout'}", 9999)]
        public void ShowWindow(IACComponent forObject, string acClassDesignName, bool isClosableBSORoot, Global.VBDesignContainer containerType, Global.VBDesignDockState dockState,
            Global.VBDesignDockPosition dockPosition, Global.ControlModes ribbonVisibility, Global.ControlModes closeButtonVisibility = Global.ControlModes.Enabled)
        {
            VBDesign vbDesign = new VBDesign();
            vbDesign.DataContext = forObject;
            vbDesign.VBContent = "*" + acClassDesignName;
            VBDockingManager.SetContainer(vbDesign, containerType);
            VBDockingManager.SetDockState(vbDesign, dockState);
            VBDockingManager.SetDockPosition(vbDesign, dockPosition);
            VBDockingManager.SetIsCloseableBSORoot(vbDesign, isClosableBSORoot);
            VBDockingManager.SetRibbonBarVisibility(vbDesign, ribbonVisibility);
            VBDockingManager.SetCloseButtonVisibility(vbDesign, closeButtonVisibility);

            //VBDockingManager.SetWindowSize(vbDesign, defaultWindowSize);
            VBDesignList.Add(vbDesign);
            ShowVBDesign(vbDesign);
        }

        public void StartBusinessobject(string acUrl, ACValueList parameterList, string acCaption, bool ribbonVisibilityOff = false, Global.VBDesignDockState dockState = Global.VBDesignDockState.Tabbed)
        {
            if (IsBSOManager && (ContextACObject != null))
            {
                if (acUrl.IndexOf('#') != -1)
                {
                    string checkACUrl = acUrl.Replace("#", "\\?");
                    var x = ContextACObject.ACUrlCommand(checkACUrl);
                    if (x != null)
                        return;
                }
                VBDesign vbDesign = new VBDesign();
                vbDesign.Name = String.Format("BSO{0}", VBDesignList.Count);
                if (!string.IsNullOrEmpty(acCaption))
                {
                    vbDesign.ACCaption = acCaption;
                    vbDesign.CustomizedACCaption = acCaption;
                }

                vbDesign.AutoStartACComponent = acUrl;
                vbDesign.AutoStartParameter = parameterList;

                VBDockingManager.SetContainer(vbDesign, Global.VBDesignContainer.DockableWindow);
                VBDockingManager.SetDockState(vbDesign, dockState);
                VBDockingManager.SetDockPosition(vbDesign, Global.VBDesignDockPosition.Right);
                VBDockingManager.SetIsCloseableBSORoot(vbDesign, true);
                if (ribbonVisibilityOff)
                    VBDockingManager.SetRibbonBarVisibility(vbDesign, Global.ControlModes.Collapsed);
                else
                    VBDockingManager.SetRibbonBarVisibility(vbDesign, Global.ControlModes.Enabled);
                VBDockingManager.SetCloseButtonVisibility(vbDesign, Global.ControlModes.Enabled);
                if (ControlManager.TouchScreenMode)
                    VBDockingManager.SetDisableDockingOnClick(vbDesign, true);
                VBDesignList.Add(vbDesign);
                ShowVBDesign(vbDesign);
            }
        }
        #endregion

        private void ShowVBDesign(Control uiElement, string acCaption = "")
        {
            if (uiElement == null)
                return;
            IVBContent uiElementAsDataContent = null;
            if (uiElement is IVBContent)
            {
                uiElementAsDataContent = (uiElement as IVBContent);
                if (uiElementAsDataContent.ContextACObject == null)
                {
                    if (uiElement is FrameworkElement)
                    {
                        if ((uiElement as FrameworkElement).DataContext == null)
                            (uiElement as FrameworkElement).DataContext = this.ContextACObject;
                    }
                }
                if (uiElementAsDataContent.ContextACObject == null)
                    return;
            }

            if (uiElement is VBDesign)
            {
                VBDesign uiElementAsDataDesign = (uiElement as VBDesign);
                // Rechtepr�fung ob Design ge�ffnet werden darf
                if (uiElementAsDataContent != null && uiElementAsDataDesign.ContentACObject != null && uiElementAsDataDesign.ContentACObject is IACType)
                {
                    if (uiElementAsDataContent.ContextACObject is IACComponent)
                    {
                        if (((ACClass)uiElementAsDataContent.ContextACObject.ACType).RightManager.GetControlMode(uiElementAsDataDesign.ContentACObject as IACType) != Global.ControlModes.Enabled)
                            return;
                    }
                }
            }

            Global.VBDesignContainer containerType = VBDockingManager.GetContainer(uiElement);

            if (containerType == Global.VBDesignContainer.TabItem)
            {
                VBDockingContainerTabbedDoc tabDoc = new VBDockingContainerTabbedDoc(this, uiElement);
                AddDockingContainerTabbedDoc_GetDockingPanel(tabDoc);
                tabDoc.Show();
            }
            else if (containerType == Global.VBDesignContainer.DockableWindow)
            {
                VBDockingContainerToolWindowVB toolWin = new VBDockingContainerToolWindowVB(this, uiElement);
                // TODO: ToolWindow wird nicht angezeigt
                // ContextACObject ist beim zweiten mal null
                if (ContextACObject != null)
                {
                    SettingsVBDesignWndPos wndPos = this.Root().RootPageWPF.ReStoreSettingsWndPos(toolWin.ACIdentifier) as SettingsVBDesignWndPos;
                    if (wndPos != null)
                    {
                        toolWin.Show(null, wndPos);
                    }
                    else
                        toolWin.Show(null, null);
                }
                else
                    toolWin.Show(null, null);
            }
            else if ((uiElementAsDataContent != null) && (containerType == Global.VBDesignContainer.ModalDialog))
            {
                VBWindowDialogRoot vbDialogRoot = new VBWindowDialogRoot(uiElementAsDataContent.ContextACObject, uiElement, this);
                vbDialogRoot.WindowStyle = System.Windows.WindowStyle.None;
                if (vbDialogRoot.Owner == null)
                {
                    DependencyObject dp = this;
                    while (dp != null)
                    {
                        dp = VBLogicalTreeHelper.FindParentObjectInLogicalTree(dp, typeof(Window));
                        if (dp != null)
                        {
                            Window ownerWindow = dp as Window;
                            if (ownerWindow.IsLoaded)
                            {
                                vbDialogRoot.Owner = ownerWindow;
                                break;
                            }
                            dp = LogicalTreeHelper.GetParent(dp);
                        }
                    }
                }
                vbDialogRoot.Resources = this.Resources;

                ACClassDesign acClassDesign = null;
                if (string.IsNullOrEmpty(uiElementAsDataContent.VBContent))
                {
                    acClassDesign = (uiElementAsDataContent.ContextACObject as IACComponent).GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMain);
                }
                else if (uiElementAsDataContent.VBContent[0] == '*')
                {
                    acClassDesign = (uiElementAsDataContent.ContextACObject as IACComponent).GetDesign(uiElementAsDataContent.VBContent.Substring(1));
                }
                if (acClassDesign == null)
                    acClassDesign = (uiElementAsDataContent.ContextACObject as IACComponent).GetDesign(Global.ACKinds.DSDesignLayout, Global.ACUsages.DUMain);

                if (acClassDesign != null)
                {
                    vbDialogRoot.Title = string.IsNullOrEmpty(acCaption) ? acClassDesign.ACCaption : acCaption;
                    if (acClassDesign.VisualHeight > 0)
                        vbDialogRoot.Height = acClassDesign.VisualHeight;
                    if (acClassDesign.VisualWidth > 0)
                        vbDialogRoot.Width = acClassDesign.VisualWidth;

                    if (acClassDesign.VisualHeight > 0 || acClassDesign.VisualWidth > 0)
                        vbDialogRoot.ResizeMode = ResizeMode.NoResize;
                }
                IRoot root = this.Root();
                if (root != null && root.RootPageWPF != null && root.RootPageWPF.InFullscreen)
                    vbDialogRoot.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                else
                    vbDialogRoot.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                if (root != null && root.RootPageWPF != null && root.RootPageWPF is Window && vbDialogRoot.Owner == null)
                    vbDialogRoot.Owner = root.RootPageWPF as Window;

                DialogStack.Add(vbDialogRoot);
                vbDialogRoot.ShowDialog();
            }
        }


        public void SaveUserFloatingWindowSize(VBDockingContainerBase dockingContainer, Point ptFloatingWindow, Size sizeFloatingWindow)
        {
            if (ContextACObject == null)
                return;
            SettingsVBDesignWndPos wndPos = new SettingsVBDesignWndPos(dockingContainer.ACIdentifier, new Rect(ptFloatingWindow, sizeFloatingWindow));
            this.Root().RootPageWPF.StoreSettingsWndPos(wndPos);
        }
        //[ACMethodInfo("", "en{'Contextmenu'}de{'Kontextmenü'}", 9999)]
        //public void ShowContextMenu(IACInteractiveObject acElement, IEnumerable<ACMenuItem> acMenuItemList)
        //{
        //    VBContextMenu contextMenu = new VBContextMenu(this);
        //    FillContextMenu(contextMenu, acElement, acMenuItemList);

        //    contextMenu.StaysOpen = true;
        //    contextMenu.IsOpen = true;
        //}

        bool _ReinitAtStartupDone = false;
        public void InitBusinessobjectsAtStartup()
        {
            if (ControlManager.TouchScreenMode && IsBSOManager)
                this.TabItemMinHeight = 35;

            if (_ReinitAtStartupDone || vbDockingPanelTabbedDoc == null)
                return;
            _ReinitAtStartupDone = true;

            foreach (VBDockingContainerToolWindow toolWin in ToolWindowContainerList)
            {
                VBDesign vbDesign = toolWin.VBDesignContent as VBDesign;
                if (vbDesign != null)
                {
                    if (ControlManager.TouchScreenMode && IsBSOManager)
                        VBDockingManager.SetDisableDockingOnClick(vbDesign, true);
                    vbDesign.InitVBControl();
                }
                toolWin.ReInitDataContext();
            }
            foreach (VBDockingContainerTabbedDoc tabbedDoc in TabbedDocContainerList)
            {
                VBDesign vbDesign = tabbedDoc.VBDesignContent as VBDesign;
                if (vbDesign != null)
                {
                    if (ControlManager.TouchScreenMode && IsBSOManager)
                        VBDockingManager.SetDisableDockingOnClick(vbDesign, true);
                    vbDesign.InitVBControl();
                }
                tabbedDoc.ReInitDataContext();
            }

            if (_dockingBtnGroups != null)
            {
                foreach (var dockgrps in this._dockingBtnGroups)
                {
                    if (dockgrps.Buttons == null)
                        continue;
                    foreach (var button in dockgrps.Buttons)
                    {
                        button.RefreshTitle();
                    }
                }
            }

            //foreach (var tab in vbDockingPanelTabbedDoc.Documents.ToList())
            //{
            //    VBDockingContainerToolWindowVB tabbedDoc = tab as VBDockingContainerToolWindowVB;
            //    if (tabbedDoc != null)
            //    {
            //        VBDesign vbDesign = tabbedDoc.VBDesignContent as VBDesign;
            //        if (vbDesign != null)
            //            vbDesign.InitVBControl();
            //        tabbedDoc.ReInitDataContext();
            //    }
            //}

        }

        private void FillContextMenu(ItemsControl itemsControl, IACInteractiveObject acElement, IEnumerable<ACMenuItem> acMenuItemList)
        {
            foreach (var acMenuItem in acMenuItemList)
            {
                VBMenuItem vbMenuItem = new VBMenuItem(acElement.ContextACObject, acMenuItem);
                itemsControl.Items.Add(vbMenuItem);
                if (acMenuItem.Items != null && acMenuItem.Items.Count > 0)
                {
                    FillContextMenu(vbMenuItem, acElement, acMenuItem.Items);
                }
            }
        }

        List<IVBDialog> _DialogStack = null;
        public List<IVBDialog> DialogStack
        {
            get
            {
                if (_DialogStack == null)
                    _DialogStack = new List<IVBDialog>();
                return _DialogStack;
            }
        }

        [ACMethodInfo("", "en{'Close'}de{'Schließen'}", 9999)]
        public void CloseTopDialog()
        {
            if (_DialogStack == null || _DialogStack.Count < 1)
                return;

            IVBDialog dialog = _DialogStack[_DialogStack.Count - 1];
            dialog.CloseDialog();
            OnCloseTopDialog(dialog);
        }

        internal void OnCloseTopDialog(IVBDialog dialog)
        {
            if (_DialogStack == null || _DialogStack.Count < 1)
                return;
            IVBDialog dialogTop = _DialogStack[_DialogStack.Count - 1];
            if (dialogTop == dialog)
            {
                _DialogStack.RemoveAt(_DialogStack.Count - 1);
            }
            else if (_DialogStack.Contains(dialog))
            {
                _DialogStack.Remove(dialog);
            }
        }

        public void CloseAndRemoveVBDesign(Control uiElement)
        {
            if (uiElement == null)
                return;
            if (!(uiElement is VBDesign))
                return;
            VBDesign uiElementAsDataDesign = (uiElement as VBDesign);
            if (VBDockingManager.GetIsCloseableBSORoot(uiElementAsDataDesign))
                uiElementAsDataDesign.StopAutoStartComponent();
            VBDesignList.Remove(uiElement);
            this.Focus();
        }
        #endregion


        /// <summary>
        /// List of managed contents (hiddens too)
        /// </summary>
        List<VBDockingContainerBase> ToolWindowContainerList = new List<VBDockingContainerBase>();

        /// <summary>
        /// Returns a documents list
        /// </summary>
        public VBDockingContainerTabbedDoc[] TabbedDocContainerList
        {
            get
            {
                if (PART_gridDocking == null)
                    return new VBDockingContainerTabbedDoc[0];
                int diff = PART_gridDocking.vbDockingPanelTabbedDoc.Documents.Count - PART_gridDocking.vbDockingPanelTabbedDoc.ContainerToolWindowsList.Count;
                if (diff <= 0)
                    return new VBDockingContainerTabbedDoc[0];
                VBDockingContainerTabbedDoc[] docs = new VBDockingContainerTabbedDoc[diff];
                int i = 0;
                foreach (VBDockingContainerBase content in PART_gridDocking.vbDockingPanelTabbedDoc.Documents)
                {
                    if (content is VBDockingContainerTabbedDoc)
                        docs[i++] = content as VBDockingContainerTabbedDoc;
                }

                return docs;
            }
        }

        /// <summary>
        /// Return active document. Return Selected Item in TabControl
        /// </summary>
        /// <remarks>If no document is present or a dockable content is active in the Documents pane return null</remarks>
        public VBDockingContainerTabbedDoc ActiveDocument
        {
            get
            {
                if (vbDockingPanelTabbedDoc == null)
                    return null;
                return vbDockingPanelTabbedDoc.ActiveDocument;
            }
        }


        #region FocusView

        public static readonly DependencyProperty FocusViewProperty =
            DependencyProperty.Register("FocusView", typeof(string), typeof(VBDockingManager), new PropertyMetadata(new PropertyChangedCallback(OnFocusViewChanged)));

        private static void OnFocusViewChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            VBDockingManager thisControl = dependencyObject as VBDockingManager;
            if (thisControl == null)
                return;
            if (thisControl.vbDockingPanelTabbedDoc != null && e.NewValue != null)
                thisControl.vbDockingPanelTabbedDoc.FocusView = e.NewValue.ToString();
        }

        [Category("VBControl")]
        public string FocusView
        {
            get { return (string)GetValue(FocusViewProperty); }
            set { SetValue(FocusViewProperty, value); }
        }

        #endregion

        /// <summary>
        /// Returns currently active documents pane (at the moment this is only one per DockManager control)
        /// </summary>
        /// <returns>The DocumentsPane</returns>
        internal VBDockingPanelTabbedDoc vbDockingPanelTabbedDoc
        {
            get
            {
                if (PART_gridDocking == null)
                    return null;
                return PART_gridDocking.vbDockingPanelTabbedDoc;
            }
        }


        private bool _SelectionChangedHandlerAdded = false;
        private void AddSelectionChangedHandler()
        {
            if (_SelectionChangedHandlerAdded)
                return;
            if (vbDockingPanelTabbedDoc_TabControl == null)
                return;
            vbDockingPanelTabbedDoc_TabControl.SelectionChanged += new SelectionChangedEventHandler(vbDockingPanelTabbedDoc_TabControl_SelectionChanged);
            _SelectionChangedHandlerAdded = true;
        }

        private void RemoveSelectionChangedHandler()
        {
            if (!_SelectionChangedHandlerAdded)
                return;
            if (vbDockingPanelTabbedDoc_TabControl == null)
                return;
            vbDockingPanelTabbedDoc_TabControl.SelectionChanged -= vbDockingPanelTabbedDoc_TabControl_SelectionChanged;
            _SelectionChangedHandlerAdded = false;
        }

        void vbDockingPanelTabbedDoc_TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabbedDocSelectionChanged != null)
            {
                TabbedDocSelectionChanged(sender, e);
            }
        }

        public event SelectionChangedEventHandler TabbedDocSelectionChanged;
        public VBTabControl vbDockingPanelTabbedDoc_TabControl
        {
            get
            {
                if (vbDockingPanelTabbedDoc == null)
                    return null;
                return vbDockingPanelTabbedDoc.TabControl;
            }
        }
        #endregion

        #region Add and Remove DockingContainerToolWindow
        /// <summary>
        /// Add dockable content to layout management
        /// </summary>
        /// <param name="container">Content to add</param>
        /// <returns></returns>
        internal void AddDockingContainerToolWindow(VBDockingContainerBase container)
        {
            if (!ToolWindowContainerList.Contains(container))
                ToolWindowContainerList.Add(container);
        }


        /// <summary>
        /// Add a docable content to default documents pane
        /// </summary>
        /// <param name="content">Dockable content to add</param>
        /// <returns>Documents pane where dockable content is added</returns>
        internal VBDockingPanelTabbedDoc AddDockingContainerToolWindow_GetDockingPanel(VBDockingContainerBase content)
        {
            System.Diagnostics.Debug.Assert(!ToolWindowContainerList.Contains(content));
            System.Diagnostics.Debug.Assert(!PART_gridDocking.vbDockingPanelTabbedDoc.ContainerToolWindowsList.Contains(content));

            if (!PART_gridDocking.vbDockingPanelTabbedDoc.ContainerToolWindowsList.Contains(content))
            {
                PART_gridDocking.vbDockingPanelTabbedDoc.AddDockingContainerToolWindow(content);
                PART_gridDocking.ArrangeLayout();
            }

            return PART_gridDocking.vbDockingPanelTabbedDoc;
        }



        /// <summary>
        /// Remove a dockable content from internal contents list
        /// </summary>
        /// <param name="container"></param>
        internal void RemoveDockingContainerToolWindow(VBDockingContainerBase container)
        {
            ToolWindowContainerList.Remove(container);
        }
        #endregion

        #region Add and Remove DockingPanelToolWindow
        /// <summary>
        /// Add a dockapble to layout management
        /// </summary>
        /// <param name="panel">Panel to manage</param>
        internal void AddDockingPanelToolWindow(VBDockingPanelToolWindow panel)
        {
            PART_gridDocking.Add(panel);
            AttachDockingPanelToolWindowEvents(panel);
        }

        /// <summary>
        /// Remove a dockable pane from layout management
        /// </summary>
        /// <param name="panel">Panel to remove</param>
        /// <remarks>Also panel event handlers are detached</remarks>
        internal void RemoveDockingPanelToolWindow(VBDockingPanelToolWindow panel)
        {
            PART_gridDocking.Remove(panel);
            DetachDockingPanelToolWindowEvents(panel);
        }
        #endregion

        #region Add and Remove DockingContainerTabbedDoc
        /// <summary>
        /// Add a document content
        /// </summary>
        /// <param name="container">Document content to adde</param>
        /// <returns>Returns DocumentsPane where document is added</returns>
        public VBDockingPanelTabbedDoc AddDockingContainerTabbedDoc_GetDockingPanel(VBDockingContainerTabbedDoc container)
        {
            //System.Diagnostics.Debug.Assert(!PART_gridDocking.vbDockingPanelTabbedDoc.Documents.Contains(container));

            if (!PART_gridDocking.vbDockingPanelTabbedDoc.Documents.Contains(container))
                PART_gridDocking.vbDockingPanelTabbedDoc.AddDockingContainerTabbedDoc(container);

            return PART_gridDocking.vbDockingPanelTabbedDoc;
        }

        public bool RemoveDockingContainerTabbedDoc(VBDockingContainerTabbedDoc container)
        {
            if (PART_gridDocking.vbDockingPanelTabbedDoc.Documents.Contains(container))
            {
                PART_gridDocking.vbDockingPanelTabbedDoc.RemoveDockingContainerTabbedDoc(container);
                return true;
            }
            return false;
        }

        public bool RemoveDockingContainerToolWindowTabbed(VBDockingContainerToolWindow toolWindow)
        {
            if (PART_gridDocking.vbDockingPanelTabbedDoc.Documents.Contains(toolWindow))
            {
                PART_gridDocking.vbDockingPanelTabbedDoc.RemoveDockingContainerToolWindow(toolWindow);
                return true;
            }
            return false;
        }

        public void CloseAllTabs()
        {
            foreach (var tab in PART_gridDocking.vbDockingPanelTabbedDoc.Documents.ToList())
            {
                VBDockingContainerToolWindowVB tabbedDoc = tab as VBDockingContainerToolWindowVB;
                if (tabbedDoc != null)
                {
                    if (VBDockingManager.GetIsCloseableBSORoot(tabbedDoc.VBDesignContent))
                    {
                        RemoveDockingContainerToolWindowTabbed(tabbedDoc);
                        tabbedDoc.OnCloseWindow();
                    }
                }
            }
        }
        #endregion

        #region Eventhandling
        /// <summary>
        /// During unolad process close active contents windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnloaded(object sender, EventArgs e)
        {
            foreach (VBDockingContainerToolWindow content in ToolWindowContainerList)
                content.Close();
            foreach (VBDockingContainerTabbedDoc docContent in TabbedDocContainerList)
                docContent.Close();

            _overlayWindow.Close();
        }

        /// <summary>
        /// Attach pane events handler
        /// </summary>
        /// <param name="panel"></param>
        internal void AttachDockingPanelToolWindowEvents(VBDockingPanelToolWindow panel)
        {
            panel.OnStateChanged += new EventHandler(DockingPanelToolWindow_OnStateChanged);

            PART_gridDocking.AttachDockingPanelToolWindowEvents(panel);
        }

        /// <summary>
        /// Detach pane events handler
        /// </summary>
        /// <param name="panel"></param>
        internal void DetachDockingPanelToolWindowEvents(VBDockingPanelToolWindow panel)
        {
            panel.OnStateChanged -= new EventHandler(DockingPanelToolWindow_OnStateChanged);

            PART_gridDocking.DetachDockingPanelToolWindowEvents(panel);
        }

        /// <summary>
        /// Handles pane state changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DockingPanelToolWindow_OnStateChanged(object sender, EventArgs e)
        {
            VBDockingPanelToolWindow pane = sender as VBDockingPanelToolWindow;
            if (pane.State == VBDockingPanelState.AutoHide)
            {
                HideDockingPanelToolWindow_AsDockingButton(pane);
                ShowTempPanel(false);
                HideTempPanel(true);
            }
            else if (pane.State == VBDockingPanelState.Docked && _currentButton != null)
            {
                this._tempPane.ChangeState(VBDockingPanelState.Docked);
            }
        }
        #endregion

        #region DockingButtons
        /// <summary>
        /// List of managed docking button groups currently shown in border stack panels
        /// </summary>
        List<VBDockingButtonGroup> _dockingBtnGroups = new List<VBDockingButtonGroup>();

        /// <summary>
        /// Add a group of docking buttons for a pane docked to a dockingmanager border
        /// </summary>
        /// <param name="panel"></param>
        private void HideDockingPanelToolWindow_AsDockingButton(VBDockingPanelToolWindow panel)
        {
            VBDockingButtonGroup buttonGroup = null;
            //bool isNewGroup = false;
            //var query = _dockingBtnGroups.Where(c => c.Dock == panel.Dock);
            //if (!query.Any())
            {
                buttonGroup = new VBDockingButtonGroup();
                buttonGroup.Dock = panel.Dock;
                //isNewGroup = true;
            }
            //else
            //buttonGroup = query.First();

            foreach (VBDockingContainerToolWindow container in panel.ContainerToolWindowsList)
            {
                VBDockingButton btn = new VBDockingButton();
                btn.DockingContainerToolWindow = container;
                btn.DockingButtonGroup = buttonGroup;

                if (_currentButton == null)
                    _currentButton = btn;
                buttonGroup.Buttons.Add(btn);
                //if (!isNewGroup)
                //{
                //    MakeNewDockingButtonVisible(buttonGroup, btn);
                //}
            }
            
            {
                _dockingBtnGroups.Add(buttonGroup);
                MakeDockingButtonsVisible(buttonGroup);
            }
        }

        private void MakeNewDockingButtonVisible(VBDockingButtonGroup group, VBDockingButton btn)
        {
            btn.MouseEnter += new MouseEventHandler(OnShowAutoHidePanel);
            Border br = new Border();
            br.Width = br.Height = 10;
            switch (group.Dock)
            {
                case Dock.Left:
                    btn.LayoutTransform = new RotateTransform(90);
                    PART_btnPanelLeft.Children.Add(btn);
                    PART_btnPanelLeft.Children.Add(br);
                    break;
                case Dock.Right:
                    btn.LayoutTransform = new RotateTransform(90);
                    PART_btnPanelRight.Children.Add(btn);
                    PART_btnPanelRight.Children.Add(br);
                    break;
                case Dock.Top:
                    PART_btnPanelTop.Children.Add(btn);
                    PART_btnPanelTop.Children.Add(br);
                    break;
                case Dock.Bottom:
                    PART_btnPanelBottom.Children.Add(btn);
                    PART_btnPanelBottom.Children.Add(br);
                    break;
            }
        }

        /// <summary>
        /// Add a group of docking buttons to the relative border stack panel
        /// </summary>
        /// <param name="group">Group to add</param>
        private void MakeDockingButtonsVisible(VBDockingButtonGroup group)
        {
            foreach (VBDockingButton btn in group.Buttons)
                btn.MouseEnter += new MouseEventHandler(OnShowAutoHidePanel);

            Border br = new Border();
            br.Width = br.Height = 10;
            switch (group.Dock)
            {
                case Dock.Left:
                    foreach (VBDockingButton btn in group.Buttons)
                    {
                        btn.LayoutTransform = new RotateTransform(90);
                        PART_btnPanelLeft.Children.Add(btn);
                    }
                    PART_btnPanelLeft.Children.Add(br);
                    break;
                case Dock.Right:
                    foreach (VBDockingButton btn in group.Buttons)
                    {
                        btn.LayoutTransform = new RotateTransform(90);
                        PART_btnPanelRight.Children.Add(btn);
                    }
                    PART_btnPanelRight.Children.Add(br);
                    break;
                case Dock.Top:
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelTop.Children.Add(btn);
                    PART_btnPanelTop.Children.Add(br);
                    break;
                case Dock.Bottom:
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelBottom.Children.Add(btn);
                    PART_btnPanelBottom.Children.Add(br);
                    break;
            }


        }

        /// <summary>
        /// Remove a group of docking buttons from the relative border stack panel
        /// </summary>
        /// <param name="group">Group to remove</param>
        private void HideDockingButtons(VBDockingButtonGroup group)
        {
            if (group.Buttons.Count <= 0)
                return;
            foreach (VBDockingButton btn in group.Buttons)
                btn.MouseEnter -= new MouseEventHandler(OnShowAutoHidePanel);

            switch (group.Dock)
            {
                case Dock.Left:
                    PART_btnPanelLeft.Children.RemoveAt(PART_btnPanelLeft.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelLeft.Children.Remove(btn);
                    break;
                case Dock.Right:
                    PART_btnPanelRight.Children.RemoveAt(PART_btnPanelRight.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelRight.Children.Remove(btn);
                    break;
                case Dock.Top:
                    PART_btnPanelTop.Children.RemoveAt(PART_btnPanelTop.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelTop.Children.Remove(btn);
                    break;
                case Dock.Bottom:
                    PART_btnPanelBottom.Children.RemoveAt(PART_btnPanelBottom.Children.IndexOf(group.Buttons[group.Buttons.Count - 1]) + 1);
                    foreach (VBDockingButton btn in group.Buttons)
                        PART_btnPanelBottom.Children.Remove(btn);
                    break;
            }
        }

        public void RemoveDockingButton(VBDockingContainerToolWindow ofWindow)
        {
            VBDockingButtonGroup groupToRemove = null;
            foreach (VBDockingButtonGroup group in _dockingBtnGroups)
            {
                var queryButton = group.Buttons.Where(c => c.DockingContainerToolWindow == ofWindow);
                if (queryButton.Any())
                {
                    VBDockingButton button = queryButton.First();
                    switch (group.Dock)
                    {
                        case Dock.Left:
                            RemoveDockingButtonFromStackPanel(PART_btnPanelLeft, button, group);
                            groupToRemove = group;
                            break;
                        case Dock.Right:
                            RemoveDockingButtonFromStackPanel(PART_btnPanelRight, button, group);
                            groupToRemove = group;
                            break;
                        case Dock.Top:
                            RemoveDockingButtonFromStackPanel(PART_btnPanelTop, button, group);
                            groupToRemove = group;
                            break;
                        case Dock.Bottom:
                            RemoveDockingButtonFromStackPanel(PART_btnPanelBottom, button, group);
                            groupToRemove = group;
                            break;
                    }
                }
            }
            if (groupToRemove != null)
                _dockingBtnGroups.Remove(groupToRemove);
        }

        private void RemoveDockingButtonFromStackPanel(StackPanel stackPanel, VBDockingButton button, VBDockingButtonGroup group)
        {
            bool found = false;
            Border br = null;
            foreach (Control child in stackPanel.Children)
            {
                if (found)
                {
                    if (child is Border)
                        br = child as Border;
                    break;
                }
                if (child == button)
                {
                    found = true;
                }
            }
            if (found)
            {
                stackPanel.Children.Remove(button);
                if (br != null)
                    stackPanel.Children.Remove(br);
                group.Buttons.Remove(button);
            }
        }
        #endregion

        #region Overlay Panel
        /// <summary>
        /// Temporary pane used to host orginal content which is autohidden
        /// </summary>
        VBDockingPanelToolWindowOverlay _tempPane = null;

        /// <summary>
        /// Current docking button attached to current temporary pane
        /// </summary>
        VBDockingButton _currentButton;

        /// <summary>
        /// Event handler which show a temporary pane with a single content attached to a docking button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnShowAutoHidePanel(object sender, MouseEventArgs e)
        {
            if (_currentButton == sender)
                return;

            HideTempPanel(true);

            _currentButton = sender as VBDockingButton;

            ShowTempPanel(true);
        }

        /// <summary>
        /// Event handler which hide temporary pane
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnHideAutoHidePane(object sender, MouseEventArgs e)
        {
            HideTempPanel(true);
        }

        /// <summary>
        /// Hide temporay pane and reset current docking button
        /// </summary>
        /// <param name="smooth">True if resize animation is enabled</param>
        private void HideTempPanel(bool smooth)
        {
            if (_tempPane != null)
            {
                VBDockingPanelToolWindow pane = PART_gridDocking.GetVBDockingPanelFromContainer(_tempPane.ContainerToolWindowsList[0]) as VBDockingPanelToolWindow;
                bool right_left = false;
                double length = 0.0;

                switch (_currentButton.DockingButtonGroup.Dock)
                {
                    case Dock.Left:
                    case Dock.Right:
                        if (_tempPanelAnimation != null)
                            pane.PaneWidth = _lengthAnimation;
                        else
                            pane.PaneWidth = _tempPane.Width;
                        length = _tempPane.Width;
                        right_left = true;
                        break;
                    case Dock.Top:
                    case Dock.Bottom:
                        if (_tempPanelAnimation != null)
                            pane.PaneHeight = _lengthAnimation;
                        else
                            pane.PaneHeight = _tempPane.Height;
                        length = _tempPane.Height;
                        right_left = false;
                        break;
                }

                _tempPane.OnStateChanged -= new EventHandler(_tempPane_OnStateChanged);

                if (smooth)
                {
                    HideOverlayPanel(length, right_left);
                }
                else
                {
                    ForceHideOverlayPanel();
                    PART_panelFront.BeginAnimation(DockPanel.OpacityProperty, null);
                    PART_panelFront.Children.Clear();
                    PART_panelFront.Opacity = 0.0;
                    _tempPane.Close();
                }

                _tempPane.DeInitVBControl();
                _currentButton = null;
                _tempPane = null;
            }

        }

        /// <summary>
        /// Show tampoary pane attached to current docking button
        /// </summary>
        /// <param name="smooth">True if resize animation is enabled</param>
        private void ShowTempPanel(bool smooth)
        {

            _tempPane = new VBDockingPanelToolWindowOverlay(this, _currentButton.DockingContainerToolWindow, _currentButton.DockingButtonGroup.Dock);
            _tempPane.OnStateChanged += new EventHandler(_tempPane_OnStateChanged);

            VBDockingPanelToolWindow pane = PART_gridDocking.GetVBDockingPanelFromContainer(_currentButton.DockingContainerToolWindow) as VBDockingPanelToolWindow;
            pane.SetDefaultWithFromVBDesign(_currentButton.DockingContainerToolWindow);
            PART_panelFront.Children.Clear();
            _tempPane.SetValue(DockPanel.DockProperty, _currentButton.DockingButtonGroup.Dock);
            PART_panelFront.Children.Add(_tempPane);
            VBDockingSplitter splitter = null;
            bool right_left = false;
            double length = 0.0;
            this.Focus();

            switch (_currentButton.DockingButtonGroup.Dock)
            {
                case Dock.Left:
                    splitter = new VBDockingSplitter(_tempPane, null, VBDockSplitOrientation.Vertical);
                    length = pane.PaneWidth;
                    right_left = true;
                    break;
                case Dock.Right:
                    splitter = new VBDockingSplitter(null, _tempPane, VBDockSplitOrientation.Vertical);
                    length = pane.PaneWidth;
                    right_left = true;
                    break;
                case Dock.Top:
                    splitter = new VBDockingSplitter(_tempPane, null, VBDockSplitOrientation.Horizontal);
                    length = pane.PaneHeight;
                    right_left = false;
                    break;
                case Dock.Bottom:
                    splitter = new VBDockingSplitter(null, _tempPane, VBDockSplitOrientation.Horizontal);
                    length = pane.PaneHeight;
                    right_left = false;
                    break;
            }

            splitter.SetValue(DockPanel.DockProperty, _currentButton.DockingButtonGroup.Dock);
            PART_panelFront.Children.Add(splitter);

            if (smooth)
                ShowOverlayPanel(length, right_left);
            else
            {
                if (right_left)
                    _tempPane.Width = length;
                else
                    _tempPane.Height = length;
                PART_panelFront.Opacity = 1.0;
            }

        }

        /// <summary>
        /// Handle AutoHide/Hide commande issued by user on temporary pane
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _tempPane_OnStateChanged(object sender, EventArgs e)
        {
            VBDockingPanelBase panel = PART_gridDocking.GetVBDockingPanelFromContainer(_currentButton.DockingContainerToolWindow);

            if (_currentButton != null)
            {
                switch (_currentButton.DockingButtonGroup.Dock)
                {
                    case Dock.Left:
                    case Dock.Right:
                        panel.PaneWidth = _tempPane.PaneWidth;
                        break;
                    case Dock.Top:
                    case Dock.Bottom:
                        panel.PaneHeight = _tempPane.PaneHeight;
                        break;
                }

                //if ((sender as VBDockingPanelToolWindow).State == VBDockingPanelState.Docked || (sender as VBDockingPanelToolWindow).State == VBDockingPanelState.TabbedDocument)
                {
                    HideDockingButtons(_currentButton.DockingButtonGroup);
                    _dockingBtnGroups.Remove(_currentButton.DockingButtonGroup);
                }
            }



            bool showOriginalPane = (_tempPane.State == VBDockingPanelState.Docked);

            HideTempPanel(false);

            if (showOriginalPane)
                panel.Show();
            else
                panel.Hide();
        }


        #region Temporary pane animation methods
        /// <summary>
        /// Current resize orientation
        /// </summary>
        bool _leftRightAnimation = false;
        /// <summary>
        /// Target size of animation
        /// </summary>
        double _lengthAnimation = 0;

        /// <summary>
        /// Temporary overaly pane used for animation
        /// </summary>
        VBDockingPanelToolWindowOverlay _tempPanelAnimation;

        /// <summary>
        /// Current animation object itself
        /// </summary>
        DoubleAnimation _animation;

        /// <summary>
        /// Show current overlay pane which hosts current auto-hiding content
        /// </summary>
        /// <param name="length">Target length</param>
        /// <param name="left_right">Resize orientaion</param>
        void ShowOverlayPanel(double length, bool left_right)
        {
            ForceHideOverlayPanel();

            _leftRightAnimation = left_right;
            _tempPanelAnimation = _tempPane;
            _lengthAnimation = length;

            _animation = new DoubleAnimation();
            _animation.From = 0.0;
            _animation.To = length;
            _animation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            _animation.Completed += new EventHandler(ShowOverlayPanel_Completed);
            if (_leftRightAnimation)
                _tempPanelAnimation.BeginAnimation(FrameworkElement.WidthProperty, _animation);
            else
                _tempPanelAnimation.BeginAnimation(FrameworkElement.HeightProperty, _animation);

            DoubleAnimation anOpacity = new DoubleAnimation();
            anOpacity.From = 0.0;
            anOpacity.To = 1.0;
            anOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            PART_panelFront.BeginAnimation(DockPanel.OpacityProperty, anOpacity);
        }

        /// <summary>
        /// Showing animation completed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Set final lenght and reset animation object on temp overlay panel</remarks>
        void ShowOverlayPanel_Completed(object sender, EventArgs e)
        {
            _animation.Completed -= new EventHandler(ShowOverlayPanel_Completed);

            if (_tempPanelAnimation != null)
            {
                if (_leftRightAnimation)
                {
                    _tempPanelAnimation.BeginAnimation(FrameworkElement.WidthProperty, null);
                    _tempPanelAnimation.Width = _lengthAnimation;
                }
                else
                {
                    _tempPanelAnimation.BeginAnimation(FrameworkElement.HeightProperty, null);
                    _tempPanelAnimation.Height = _lengthAnimation;
                }
            }

            _tempPanelAnimation = null;
            //FocusManager.SetIsFocusScope(this, true);
            this.Focus();
        }

        /// <summary>
        /// Hide current overlay panel
        /// </summary>
        /// <param name="length"></param>
        /// <param name="left_right"></param>
        void HideOverlayPanel(double length, bool left_right)
        {
            _leftRightAnimation = left_right;
            _tempPanelAnimation = _tempPane;
            if (Double.IsNaN(length))
                length = 200;
            _lengthAnimation = length;

            _animation = new DoubleAnimation();
            _animation.From = length;
            _animation.To = 0.0;
            _animation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            _animation.Completed += new EventHandler(HideOverlayPanel_Completed);

            if (left_right)
                _tempPanelAnimation.BeginAnimation(FrameworkElement.WidthProperty, _animation);
            else
                _tempPanelAnimation.BeginAnimation(FrameworkElement.HeightProperty, _animation);

            DoubleAnimation anOpacity = new DoubleAnimation();
            anOpacity.From = 1.0;
            anOpacity.To = 0.0;
            anOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            PART_panelFront.BeginAnimation(DockPanel.OpacityProperty, anOpacity);
        }

        /// <summary>
        /// Hiding animation completed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Set final lenght to 0 and reset animation object on temp overlay panel</remarks>
        void HideOverlayPanel_Completed(object sender, EventArgs e)
        {
            ForceHideOverlayPanel();
            try
            {
                if (PART_panelFront != null)
                    PART_panelFront.Children.Clear();
            }
            catch (InvalidOperationException ec)
            {
                string msg = ec.Message;
                if (ec.InnerException != null && ec.InnerException.Message != null)
                    msg += " Inner:" + ec.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDockingManager", "HideOverlayPanel_Completed", msg);
            }
            //FocusManager.SetIsFocusScope(this, true);
            this.Focus();
        }

        /// <summary>
        /// Forces to hide current overlay panel
        /// </summary>
        /// <remarks>Usually used when a second animation is about to start from a different button</remarks>
        void ForceHideOverlayPanel()
        {
            if (_tempPanelAnimation != null)
            {
                _animation.Completed -= new EventHandler(HideOverlayPanel_Completed);
                _animation.Completed -= new EventHandler(ShowOverlayPanel_Completed);
                if (_leftRightAnimation)
                {
                    _tempPanelAnimation.BeginAnimation(FrameworkElement.WidthProperty, null);
                    _tempPanelAnimation.Width = 0;
                }
                else
                {
                    _tempPanelAnimation.BeginAnimation(FrameworkElement.HeightProperty, null);
                    _tempPanelAnimation.Height = 0;
                }

                //_tempPanelAnimation.Close();
                //_tempPanelAnimation = null;
            }
        }
        #endregion

        #endregion

        #region DragDrop Operations
        /// <summary>
        /// Parent window hosting DockManager user control
        /// </summary>
        private Window _ParentWindow = null;
        public Window ParentWindow
        {
            get
            {
                if (_ParentWindow == null)
                {
                    DependencyObject rootObject = gip.core.layoutengine.avui.ControlManager.GetHighestFrameworkElementInLogicalTree(this);
                    if (rootObject is Window)
                        _ParentWindow = rootObject as Window;
                }
                return _ParentWindow;
            }
            set
            {
                _ParentWindow = value;
            }
        }


        /// <summary>
        /// Handle dockable pane layout changing
        /// </summary>
        /// <param name="sourcePanel">Source pane to move</param>
        /// <param name="destinationPanel">Relative panel</param>
        /// <param name="relativeDock"></param>
        internal void MoveTo(VBDockingPanelToolWindow sourcePanel, VBDockingPanelBase destinationPanel, Dock relativeDock)
        {
            PART_gridDocking.MoveTo(sourcePanel, destinationPanel, relativeDock);
        }


        /// <summary>
        /// Called from a pane when it's dropped into an other pane
        /// </summary>
        /// <param name="sourcePanel">Source panel which is going to be closed</param>
        /// <param name="destinationPanel">Destination panel which is about to host contents from SourcePane</param>
        internal void MoveInto(VBDockingPanelToolWindow sourcePanel, VBDockingPanelBase destinationPanel)
        {
            PART_gridDocking.MoveInto(sourcePanel, destinationPanel);
        }



        /// <summary>
        /// Begins dragging operations
        /// </summary>
        /// <param name="floatingWindow">Floating window containing pane which is dragged by user</param>
        /// <param name="point">Current mouse position</param>
        /// <param name="offset">Offset to be use to set floating window screen position</param>
        /// <returns>Retruns True is drag is completed, false otherwise</returns>
        public bool Drag(VBWindowDockingUndocked floatingWindow, Point point, Point offset)
        {
            if (!Focusable)
            {
                e.Pointer.Capture(this);
                {
                    if (ParentWindow is VBDockingContainerToolWindow)
                    {
                        VBDockingContainerToolWindow dockContainerToolW = ParentWindow as VBDockingContainerToolWindow;
                        if (dockContainerToolW.VBDockingPanel is VBDockingPanelTabbedDoc)
                            floatingWindow.Owner = (dockContainerToolW.VBDockingPanel as VBDockingPanelTabbedDoc).DockManager.ParentWindow;
                        else
                            floatingWindow.Owner = ParentWindow;
                    }
                    else
                        floatingWindow.Owner = ParentWindow;
                    DragPanelServices.StartDrag(floatingWindow, point, offset);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles mousemove event
        /// </summary>
        /// <param name="e">The event arugments.</param>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (Focusable)
                DragPanelServices.MoveDrag(PointToScreen(e.GetPosition(this)));
            base.OnPreviewMouseMove(e);
        }

        /// <summary>
        /// Handles mouseUp event
        /// </summary>
        /// <param name="e">The event arguments.</param>
        /// <remarks>Releases eventually camptured mouse events</remarks>
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (Focusable)
            {
                DragPanelServices.EndDrag(PointToScreen(e.GetPosition(this)));
                if (e.Pointer.Captured == this)
                    e.Pointer.Capture(null);
            }
            base.OnPreviewMouseUp(e);
        }

        VBDockDragPanelServices _dragPanelServices;

        internal VBDockDragPanelServices DragPanelServices
        {
            get
            {
                if (_dragPanelServices == null)
                    _dragPanelServices = new VBDockDragPanelServices(this);

                return _dragPanelServices;
            }
        }
        #endregion

        #region IDropSurface

        /// <summary>
        /// Returns a rectangle where this surface is active
        /// </summary>
        public Rect SurfaceRectangle
        {
            get
            { return new Rect(PointToScreen(new Point(0, 0)), new Size(ActualWidth, ActualHeight)); }
        }

        /// <summary>
        /// Overlay window which shows docking placeholders
        /// </summary>
        VBDockingOverlayWindow _overlayWindow;

        /// <summary>
        /// Returns current overlay window
        /// </summary>
        internal VBDockingOverlayWindow OverlayWindow
        {
            get
            {
                return _overlayWindow;
            }
        }

        /// <summary>
        /// Handles this sourface mouse entering (show current overlay window)
        /// </summary>
        /// <param name="point">Current mouse position</param>
        public void OnDockDragEnter(Point point)
        {
            OverlayWindow.Owner = DragPanelServices.FloatingWindow;
            OverlayWindow.Left = PointToScreen(new Point(0, 0)).X;
            OverlayWindow.Top = PointToScreen(new Point(0, 0)).Y;
            OverlayWindow.Width = ActualWidth;
            OverlayWindow.Height = ActualHeight;
            OverlayWindow.Show();
        }

        /// <summary>
        /// Handles mouse overing this surface
        /// </summary>
        /// <param name="point"></param>
        public void OnDockDragOver(Point point)
        {

        }

        /// <summary>
        /// Handles mouse leave event during drag (hide overlay window)
        /// </summary>
        /// <param name="point"></param>
        public void OnDockDragLeave(Point point)
        {
            _overlayWindow.Owner = null;
            _overlayWindow.Hide();
            if (ParentWindow != null)
                ParentWindow.Activate();
            if (!IsFocused)
                this.Focus();
        }

        /// <summary>
        /// Handler drop events
        /// </summary>
        /// <param name="point">Current mouse position</param>
        /// <returns>Returns alwasy false because this surface doesn't support direct drop</returns>
        public bool OnDockDrop(Point point)
        {
            return false;
        }

        #endregion

        #region Persistence
        public static readonly DependencyProperty FreezeActiveProperty = DependencyProperty.Register("FreezeActive", typeof(bool), typeof(VBDockingManager));
        public bool FreezeActive
        {
            get { return (bool)GetValue(FreezeActiveProperty); }
            set { SetValue(FreezeActiveProperty, value); }
        }

        public static readonly DependencyProperty MasterPageFreezeProperty = DependencyProperty.Register("MasterPageFreeze",
        typeof(WPFControlSelectionEventArgs), typeof(VBDockingManager), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        public WPFControlSelectionEventArgs MasterPageFreeze
        {
            get { return (WPFControlSelectionEventArgs)GetValue(MasterPageFreezeProperty); }
            set { SetValue(MasterPageFreezeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly DependencyProperty ACUrlCmdMessageProperty =
            DependencyProperty.Register("ACUrlCmdMessage",
                typeof(ACUrlCmdMessage), typeof(VBDockingManager),
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
                typeof(ACInitState), typeof(VBDockingManager),
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
            VBDockingManager thisControl = dependencyObject as VBDockingManager;
            if (thisControl == null)
                return;
            if (args.Property == MasterPageFreezeProperty)
                thisControl.RootPageWPF_VBDockingManagerFreezingEvent();
            else if (args.Property == ACUrlCmdMessageProperty)
                thisControl.OnACUrlMessageReceived();
            else if (args.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
        }

        void VBDockingManager_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null && e.OldValue != null)
            {
                IACBSO bso = e.OldValue as IACBSO;
                if (bso != null)
                    DeInitVBControl(bso);
            }
        }


        void RootPageWPF_VBDockingManagerFreezingEvent()
        {
            if (MasterPageFreeze == null)
                return;
            if (MasterPageFreeze.ControlSelectionState == ControlSelectionState.Off)
                FreezeActive = false;
            else
                FreezeActive = true;
        }

        // Double-Click for Freezing this
        void _PART_gridDocking_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!FreezeActive || (ContextACObject == null))
                return;
            if (!(e.Source is VBDockingGrid))
                return;
            if ((e.Source as VBDockingGrid).vbDockingPanelTabbedDoc.DockManager != this)
                return;
            SerializeVBDesignList();
            this.Root().RootPageWPF.DockingManagerFreezed(this);
            e.Handled = true;
        }


        private void PersistDockStateToDesignList()
        {
            PART_gridDocking.PersistStateToVBDesignContent();
            foreach (VBDockingContainerToolWindow toolWin in ToolWindowContainerList)
            {
                toolWin.PersistStateToVBDesignContent();
            }
            foreach (VBDockingContainerTabbedDoc tabbedDoc in TabbedDocContainerList)
            {
                tabbedDoc.PersistStateToVBDesignContent();
            }
        }

        public string SerializeVBDesignList()
        {
            if (VBDesignList == null)
                return "";
            //if (VBDesignList.Count <= 0)
            //return "";
            string xaml = "";

            KeyValuePair<string, ACxmlnsInfo> nsThis = Layoutgenerator.GetNamespaceInfo(this);
            if (nsThis.Value == null)
                return "";
            //KeyValuePair<string, string> nsX = Layoutgenerator.GetNamespaceInfo("http://schemas.microsoft.com/winfx/2006/xaml");
            //if (String.IsNullOrEmpty(nsX.Value))
            //    return "";

            PersistDockStateToDesignList();
            string thisTypeName = this.GetType().Name;

            #region LINQ to XML
            XNamespace xNsThis = nsThis.Value.XMLNameSpace;
            XNamespace xNsX = ACxmlnsResolver.xNamespaceWPF;
            XDocument xDoc = new XDocument();
            XElement xElementRoot = new XElement(xNsThis + thisTypeName);
            foreach (KeyValuePair<string, ACxmlnsInfo> kvp in ACxmlnsResolver.NamespacesDict)
            {
                string key = kvp.Key.Trim();
                if (!String.IsNullOrEmpty(key))
                    xElementRoot.Add(new XAttribute(XNamespace.Xmlns + key, kvp.Value.XMLNameSpace));
            }
            xElementRoot.Add(new XAttribute(xNsX + "Name", this.Name));
            xDoc.Add(xElementRoot);

            foreach (Control uiElement in VBDesignList)
            {
                KeyValuePair<string, ACxmlnsInfo> nsControl = Layoutgenerator.GetNamespaceInfo(uiElement);
                if (nsControl.Value == null)
                    continue;
                XNamespace xNsUI = nsControl.Value.XMLNameSpace;
                XElement xElement = new XElement(xNsUI + uiElement.GetType().Name,
                    new XAttribute(xNsThis + thisTypeName + ".Container", VBDockingManager.GetContainer(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".DockState", VBDockingManager.GetDockState(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".DockPosition", VBDockingManager.GetDockPosition(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".RibbonBarVisibility", VBDockingManager.GetRibbonBarVisibility(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".IsCloseableBSORoot", VBDockingManager.GetIsCloseableBSORoot(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".CloseButtonVisibility", VBDockingManager.GetCloseButtonVisibility(uiElement).ToString()),
                    new XAttribute(xNsThis + thisTypeName + ".DisableDockingOnClick", VBDockingManager.GetDisableDockingOnClick(uiElement).ToString())
                    );
                Size size = VBDockingManager.GetWindowSize(uiElement);
                xElement.Add(new XAttribute(xNsThis + thisTypeName + ".WindowSize", String.Format("{0},{1}", size.Width, size.Height)));
                if (uiElement is FrameworkElement)
                    xElement.Add(new XAttribute(xNsX + "Name", (uiElement as FrameworkElement).Name));
                if (uiElement is IVBSerialize)
                    (uiElement as IVBSerialize).AddSerializableAttributes(xElement);
                xElementRoot.Add(xElement);
            }
            VBDesign parentDesign = VBLogicalTreeHelper.FindParentObjectInLogicalTree(this, typeof(VBDesign)) as VBDesign;
            if (parentDesign != null)
            {
                parentDesign.UpdateDesignOfCurrentUser(xElementRoot, IsBSOManager);
            }
            xaml = xDoc.ToString();
            #endregion

            #region Example with XmlDocument
            //XmlDocument xmldoc = new XmlDocument();
            //XmlElement xmlElementRoot = xmldoc.CreateElement(nsThis.Key, thisTypeName, nsThis.Value);
            //foreach (KeyValuePair<string, string> kvp in Layoutgenerator.NamespacesDict)
            //{
            //    string key = kvp.Key.Trim();
            //    if (String.IsNullOrEmpty(key))
            //        xmlElementRoot.SetAttribute("xmlns", kvp.Value);
            //    else
            //        xmlElementRoot.SetAttribute("xmlns:" + key, kvp.Value);
            //}

            //xmlElementRoot.SetAttribute("Name", nsX.Value, this.Name);
            //xmldoc.AppendChild(xmlElementRoot);

            //foreach (Control uiElement in VBDesignList)
            //{
            //    XmlElement xmlElement;
            //    KeyValuePair<string, string> nsControl = Layoutgenerator.GetNamespaceInfo(uiElement);
            //    if (String.IsNullOrEmpty(nsControl.Value))
            //        continue;
            //    xmlElement = xmldoc.CreateElement(nsControl.Key, uiElement.GetType().Name, nsControl.Value);
            //    xmlElement.SetAttribute(thisTypeName + ".Container", nsThis.Value, VBDockingManager.GetContainer(uiElement).ToString());
            //    xmlElement.SetAttribute(thisTypeName + ".DockState", nsThis.Value, VBDockingManager.GetDockState(uiElement).ToString());
            //    xmlElement.SetAttribute(thisTypeName + ".DockPosition", nsThis.Value, VBDockingManager.GetDockPosition(uiElement).ToString());
            //    xmlElement.SetAttribute(thisTypeName + ".RibbonBarVisibility", nsThis.Value, VBDockingManager.GetRibbonBarVisibility(uiElement).ToString());
            //    xmlElement.SetAttribute(thisTypeName + ".IsCloseableBSORoot", nsThis.Value, VBDockingManager.GetIsCloseableBSORoot(uiElement).ToString());
            //    Size size = VBDockingManager.GetWindowSize(uiElement);
            //    xmlElement.SetAttribute(thisTypeName + ".Size", nsThis.Value, String.Format("{0},{1}", size.Width, size.Height));
            //    if (uiElement is FrameworkElement)
            //        xmlElement.SetAttribute("Name", nsX.Value, (uiElement as FrameworkElement).Name);
            //    xmlElementRoot.AppendChild(xmlElement);
            //}
            //xaml = xmldoc.InnerXml;
            #endregion
            return xaml;
        }

        /// <summary>
        /// Serialize layout state of panes and contents into a xml string
        /// </summary>
        /// <returns>Xml containing layout state</returns>
        public string GetLayoutAsXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("DockingLibrary_Layout"));
            PART_gridDocking.Serialize(doc, doc.DocumentElement);
            return doc.OuterXml;
        }

        /// <summary>
        /// Restore docking layout reading a xml string which is previously generated by a call to GetLayoutState
        /// </summary>
        /// <param name="xml">Xml containing layout state</param>
        /// <param name="getContentHandler">Delegate used by serializer to get user defined dockable contents</param>
        public void RestoreLayoutFromXml(string xml, GetContentFromTypeString getContentHandler)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            PART_gridDocking.Deserialize(this, doc.ChildNodes[0], getContentHandler);

            List<VBDockingPanelBase> addedPanes = new List<VBDockingPanelBase>();
            foreach (VBDockingContainerToolWindow content in ToolWindowContainerList)
            {
                VBDockingPanelToolWindow pane = content.VBDockingPanel as VBDockingPanelToolWindow;
                if (pane != null && !addedPanes.Contains(pane))
                {
                    if (pane.State == VBDockingPanelState.AutoHide)
                    {
                        addedPanes.Add(pane);
                        HideDockingPanelToolWindow_AsDockingButton(pane);
                    }
                }
            }

            _currentButton = null;
        }
        #endregion

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get;
            set;
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            return;
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

    /// <summary>
    /// The settings for window position of the VBDesign.
    /// </summary>
    [Serializable]
    public class SettingsVBDesignWndPos
    {
        /// <summary>
        /// Creates a new instance of SettingsWndPos.
        /// </summary>
        public SettingsVBDesignWndPos()
        {
        }

        /// <summary>
        /// Creates a new instance of SettingsWndPos.
        /// </summary>
        /// <param name="acIdentifier">The acIdentifier parameter.</param>
        /// <param name="wndRect">The widnow rectangle parameter.</param>
        public SettingsVBDesignWndPos(string acIdentifier, Rect wndRect)
        {
            ACIdentifier = acIdentifier;
            WndRect = wndRect;
        }

        /// <summary>
        /// Gets or sets the ACIdentifier.
        /// </summary>
        public string ACIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the WndRect.
        /// </summary>
        public Rect WndRect
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents the list of settings for window position of VBDesign.
    /// </summary>
    [Serializable]
    public class SettingsVBDesignWndPosList : List<SettingsVBDesignWndPos>
    {
    }

}
