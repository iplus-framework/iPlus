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
using System.Xml;
using gip.core.layoutengine.Helperclasses;
using System.Linq;
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// States that a dockable pane can assume.
    /// </summary>
    public enum VBDockingPanelState : short
    {
        Hidden,

        AutoHide,

        Docked,

        TabbedDocument,

        FloatingWindow,

        DockableWindow
    }


    [TemplatePart(Name = "PART_PanelHeader", Type = typeof(Border))]
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_HideButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MenuButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ContextMenu", Type = typeof(VBContextMenu))]
    [TemplatePart(Name = "PART_menuFloatingWindow", Type = typeof(VBMenuItem))]
    [TemplatePart(Name = "PART_menuDockedWindow", Type = typeof(VBMenuItem))]
    [TemplatePart(Name = "PART_menuTabbedDocument", Type = typeof(VBMenuItem))]
    [TemplatePart(Name = "PART_menuAutoHide", Type = typeof(VBMenuItem))]
    [TemplatePart(Name = "PART_menuClose", Type = typeof(VBMenuItem))]
    [TemplatePart(Name = "PART_cpClientWindowContent", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_tbcContents", Type = typeof(VBTabControl))]
    [TemplatePart(Name = "PART_tbTitle", Type = typeof(TextBlock))]

    ///// <summary>
    ///// A dockable pane is a resizable and movable window region which can host one or more dockable content
    ///// 
    ///// Verwendung:
    ///// -VBDockingContainerToolWindow
    ///// -VBDockingGroup
    ///// -VBDockingPanelBase
    ///// </summary>
    ///// <remarks>A dockable pane occupies a window region. It can be in two different states: docked to a border or hosted in a floating window.
    ///// When is docked it can be resizes only in a direction. User can switch between pane states using mouse or context menus.
    ///// Contents whitin a dockable pane are arranged through a tabcontrol.</remarks>
    partial class VBDockingPanelToolWindow : VBDockingPanelBase
    {
        #region ctors
        //public VBDockingPanelToolWindow()
        //    : base()
        //{
        //    InitVBDockingPanelToolWindow();
        //}

        public VBDockingPanelToolWindow(VBDockingManager dockManager)
            : base(dockManager)
        {
            InitializeComponent();
            IsDragable = true;
        }

        public VBDockingPanelToolWindow(VBDockingManager dockManager, Dock initialDock)
            : base(dockManager)
        {
            _dock = initialDock;
            InitializeComponent();
            IsDragable = true;
        }

        internal override void DeInitVBControl(IACComponent bso = null)
        {
            HideTabs();
            UnRegisterEvents();
            _PART_PanelHeader = null;
            _PART_CloseButton = null;
            _PART_HideButton = null;
            _PART_MenuButton = null;
            _PART_ContextMenu = null;
            _PART_menuFloatingWindow = null;
            _PART_menuDockedWindow = null;
            _PART_menuTabbedDocument = null;
            _PART_menuAutoHide = null;
            _PART_menuClose = null;
            _PART_cpClientWindowContent = null;
            if (_PART_tbcContents != null)
                _PART_tbcContents.Items.Clear();
            _PART_tbcContents = null;
            _PART_tbTitle = null;
            base.DeInitVBControl(bso);
        }

        #endregion

        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "DockingPanelToolWindowStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDocking/Themes/DockingWindowStyleGip.xaml",
                                         hasImplicitStyles = true},
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "DockingPanelToolWindowStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDocking/Themes/DockingWindowStyleAero.xaml",
                                         hasImplicitStyles = true},
        };
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

        public static List<CustomControlStyleInfo> _TemplateInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "DockingWindowAsPanelStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDocking/Themes/DockingWindowStyleGip.xaml",
                                         hasImplicitStyles = true},
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "DockingWindowAsPanelStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDocking/Themes/DockingWindowStyleAero.xaml",
                                         hasImplicitStyles = true},
        };

        /*static VBDockingPanelToolWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBDockingPanelToolWindow), new FrameworkPropertyMetadata(typeof(VBDockingPanelToolWindow)));
        }*/


        bool _themeApplied = false;



        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
            ApplyTemplate();
            RegisterEvents();
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
            RegisterEvents();
        } 

        bool _Loaded = false;
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_Loaded)
                return;
            _Loaded = true;
            //RegisterEvents();
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall, true);
            if (_themeApplied)
            {
                ControlManager.OverrideStyleWithTheme(this, StyleInfoList);
                CustomControlStyleInfo CustomControlStyleInfo = (from o in _TemplateInfoList where o.wpfTheme == ControlManager.WpfTheme select o).FirstOrDefault();
                if (CustomControlStyleInfo != null)
                {
                    object resource = this.TryFindResource(CustomControlStyleInfo.styleName);
                    if (resource == null && this.Parent is FrameworkElement)
                        resource = ((FrameworkElement)this.Parent).TryFindResource(CustomControlStyleInfo.styleName);
                    if (resource != null)
                        this.Template = resource as ControlTemplate;
                }
            }
            if (bInitializingCall)
                _themeApplied = false;
        }

        private void RegisterEvents()
        {
            //object partObject = (object)contentControl.Template.FindName("PART_PanelHeader", contentControl);
            object partObject = (object)GetTemplateChild("PART_PanelHeader");
            if ((partObject != null) && (partObject is Border))
            {
                _PART_PanelHeader = ((Border)partObject);
                _PART_PanelHeader.MouseDown += OnHeaderMouseDown;
                _PART_PanelHeader.MouseUp += OnHeaderMouseUp;
                _PART_PanelHeader.MouseMove += OnHeaderMouseMove;
            }

            //partObject = (object)contentControl.Template.FindName("PART_CloseButton", contentControl);
            partObject = (object)GetTemplateChild("PART_CloseButton");
            if ((partObject != null) && (partObject is Button))
            {
                _PART_CloseButton = ((Button)partObject);
                _PART_CloseButton.Click += OnBtnCloseMouseDown;
                _PART_CloseButton.Loaded += PART_Loaded;
            }

            //partObject = (object)contentControl.Template.FindName("PART_HideButton", contentControl);
            partObject = (object)GetTemplateChild("PART_HideButton");
            if ((partObject != null) && (partObject is Button))
            {
                _PART_HideButton = ((Button)partObject);
                _PART_HideButton.Click += OnBtnAutoHideMouseDown;
            }

            //partObject = (object)contentControl.Template.FindName("PART_MenuButton", contentControl);
            partObject = (object)GetTemplateChild("PART_MenuButton");
            if ((partObject != null) && (partObject is Button))
            {
                _PART_MenuButton = ((Button)partObject);
                _PART_MenuButton.Click += OnBtnMenuMouseDown;
            }

            //partObject = (object)contentControl.Template.FindName("PART_ContextMenu", contentControl);
            partObject = (object)GetTemplateChild("PART_ContextMenu");
            if ((partObject != null) && (partObject is VBContextMenu))
            {
                _PART_ContextMenu = ((VBContextMenu)partObject);
                _PART_ContextMenu.IsVisibleChanged += OnBtnMenuPopup;
            }

            //partObject = (object)contentControl.Template.FindName("PART_menuFloatingWindow", contentControl);
            partObject = (object)GetTemplateChild("PART_menuFloatingWindow");
            if ((partObject != null) && (partObject is VBMenuItem))
            {
                _PART_menuFloatingWindow = ((VBMenuItem)partObject);
                _PART_menuFloatingWindow.Click += OnDockingMenu;
            }

            //partObject = (object)contentControl.Template.FindName("PART_menuDockedWindow", contentControl);
            partObject = (object)GetTemplateChild("PART_menuDockedWindow");
            if ((partObject != null) && (partObject is VBMenuItem))
            {
                _PART_menuDockedWindow = ((VBMenuItem)partObject);
                _PART_menuDockedWindow.Click += OnDockingMenu;
            }

            //partObject = (object)contentControl.Template.FindName("PART_menuTabbedDocument", contentControl);
            partObject = (object)GetTemplateChild("PART_menuTabbedDocument");
            if ((partObject != null) && (partObject is VBMenuItem))
            {
                _PART_menuTabbedDocument = ((VBMenuItem)partObject);
                _PART_menuTabbedDocument.Click += OnDockingMenu;
            }

            //partObject = (object)contentControl.Template.FindName("PART_menuAutoHide", contentControl);
            partObject = (object)GetTemplateChild("PART_menuAutoHide");
            if ((partObject != null) && (partObject is VBMenuItem))
            {
                _PART_menuAutoHide = ((VBMenuItem)partObject);
                _PART_menuAutoHide.Click += OnDockingMenu;
            }

            //partObject = (object)contentControl.Template.FindName("PART_menuClose", contentControl);
            partObject = (object)GetTemplateChild("PART_menuClose");
            if ((partObject != null) && (partObject is VBMenuItem))
            {
                _PART_menuClose = ((VBMenuItem)partObject);
                _PART_menuClose.Click += OnDockingMenu;
            }

            partObject = (object)GetTemplateChild("PART_cpClientWindowContent");
            if ((partObject != null) && (partObject is ContentPresenter))
            {
                _PART_cpClientWindowContent = ((ContentPresenter)partObject);
            }

            partObject = (object)GetTemplateChild("PART_tbcContents");
            if ((partObject != null) && (partObject is VBTabControl))
            {
                _PART_tbcContents = ((VBTabControl)partObject);
            }

            partObject = (object)GetTemplateChild("PART_tbTitle");
            if ((partObject != null) && (partObject is TextBlock))
            {
                _PART_tbTitle = ((TextBlock)partObject);
            }
        }

        private void UnRegisterEvents()
        {
            if (_PART_PanelHeader != null)
            {
                _PART_PanelHeader.MouseDown -= OnHeaderMouseDown;
                _PART_PanelHeader.MouseUp -= OnHeaderMouseUp;
                _PART_PanelHeader.MouseMove -= OnHeaderMouseMove;
            }

            if (_PART_CloseButton != null)
            {
                _PART_CloseButton.Click -= OnBtnCloseMouseDown;
                _PART_CloseButton.Loaded -= PART_Loaded;
            }

            if (_PART_HideButton != null)
            {
                _PART_HideButton.Click -= OnBtnAutoHideMouseDown;
            }

            if (_PART_MenuButton != null)
            {
                _PART_MenuButton.Click -= OnBtnMenuMouseDown;
            }

            if (_PART_ContextMenu != null)
            {
                _PART_ContextMenu.IsVisibleChanged -= OnBtnMenuPopup;
            }

            if (_PART_menuFloatingWindow != null)
            {
                _PART_menuFloatingWindow.Click -= OnDockingMenu;
            }

            if (_PART_menuDockedWindow != null)
            {
                _PART_menuDockedWindow.Click -= OnDockingMenu;
            }

            if (_PART_menuTabbedDocument != null)
            {
                _PART_menuTabbedDocument.Click -= OnDockingMenu;
            }

            if (_PART_menuAutoHide != null)
            {
                _PART_menuAutoHide.Click -= OnDockingMenu;
            }

            if (_PART_menuClose != null)
            {
                _PART_menuClose.Click -= OnDockingMenu;
            }
        }

        #region TemplateParts

        private void PART_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private Border _PART_PanelHeader;
        public Border PART_PanelHeader
        {
            get
            {
                if (_PART_PanelHeader == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_PanelHeader");
                    //DependencyObject partObj = GetTemplateChild("PART_PanelHeader");
                    _PART_PanelHeader = partObj != null ? (Border)partObj : null;
                }
                return _PART_PanelHeader;
            }
        }


        private Button _PART_CloseButton;
        public Button PART_CloseButton
        {
            get
            {
                if (_PART_CloseButton == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_CloseButton");
                    _PART_CloseButton = partObj != null ? (Button)partObj : null;
                }
                return _PART_CloseButton;
            }
        }


        private Button _PART_HideButton;
        public Button PART_HideButton
        {
            get
            {
                if (_PART_HideButton == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_HideButton");
                    _PART_HideButton = partObj != null ? (Button)partObj : null;
                }
                return _PART_HideButton;
            }
        }


        private Button _PART_MenuButton;
        public Button PART_MenuButton
        {
            get
            {
                if (_PART_MenuButton == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_MenuButton");
                    _PART_MenuButton = partObj != null ? (Button)partObj : null;
                }
                return _PART_MenuButton;
            }
        }


        private VBContextMenu _PART_ContextMenu;
        public VBContextMenu PART_ContextMenu
        {
            get
            {
                if (_PART_ContextMenu == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_ContextMenu");
                    _PART_ContextMenu = partObj != null ? (VBContextMenu)partObj : null;
                }
                return _PART_ContextMenu;
            }
        }


        private VBMenuItem _PART_menuFloatingWindow;
        public VBMenuItem PART_menuFloatingWindow
        {
            get
            {
                if (_PART_menuFloatingWindow == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_menuFloatingWindow");
                    _PART_menuFloatingWindow = partObj != null ? (VBMenuItem)partObj : null;
                }
                return _PART_menuFloatingWindow;
            }
        }


        private VBMenuItem _PART_menuDockedWindow;
        public VBMenuItem PART_menuDockedWindow
        {
            get
            {
                if (_PART_menuDockedWindow == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_menuDockedWindow");
                    _PART_menuDockedWindow = partObj != null ? (VBMenuItem)partObj : null;
                }
                return _PART_menuDockedWindow;
            }
        }


        private VBMenuItem _PART_menuTabbedDocument;
        public VBMenuItem PART_menuTabbedDocument
        {
            get
            {
                if (_PART_menuTabbedDocument == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_menuTabbedDocument");
                    _PART_menuTabbedDocument = partObj != null ? (VBMenuItem)partObj : null;
                }
                return _PART_menuTabbedDocument;
            }
        }


        private VBMenuItem _PART_menuAutoHide;
        public VBMenuItem PART_menuAutoHide
        {
            get
            {
                if (_PART_menuAutoHide == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_menuAutoHide");
                    _PART_menuAutoHide = partObj != null ? (VBMenuItem)partObj : null;
                }
                return _PART_menuAutoHide;
            }
        }


        private VBMenuItem _PART_menuClose;
        public VBMenuItem PART_menuClose
        {
            get
            {
                if (_PART_menuClose == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_menuClose");
                    _PART_menuClose = partObj != null ? (VBMenuItem)partObj : null;
                }
                return _PART_menuClose;
            }
        }


        private ContentPresenter _PART_cpClientWindowContent;
        public ContentPresenter PART_cpClientWindowContent
        {
            get
            {
                if (_PART_cpClientWindowContent == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_cpClientWindowContent");
                    _PART_cpClientWindowContent = partObj != null ? (ContentPresenter)partObj : null;
                }
                return _PART_cpClientWindowContent;
            }
        }


        private VBTabControl _PART_tbcContents;
        public VBTabControl PART_tbcContents
        {
            get
            {
                if (_PART_tbcContents == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_tbcContents");
                    _PART_tbcContents = partObj != null ? (VBTabControl)partObj : null;
                }
                return _PART_tbcContents;
            }
        }


        private TextBlock _PART_tbTitle;
        public TextBlock PART_tbTitle
        {
            get
            {
                if (_PART_tbTitle == null)
                {
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_tbTitle");
                    _PART_tbTitle = partObj != null ? (TextBlock)partObj : null;
                }
                return _PART_tbTitle;
            }
        }

        #endregion


        /// <summary>
        /// When created pane is hidden
        /// </summary>
        protected VBDockingPanelState _state = VBDockingPanelState.Hidden;
        protected void SetState(VBDockingPanelState newState)
        {
            if (((_state == VBDockingPanelState.AutoHide) && (newState == VBDockingPanelState.Docked))
                || (_state == VBDockingPanelState.DockableWindow)
                || (newState == VBDockingPanelState.DockableWindow)
                || ((_state == VBDockingPanelState.Hidden) && (newState == VBDockingPanelState.Docked)))
                IsDragable = true;
            else
                IsDragable = false;
            _state = newState;
        }

        /// <summary>
        /// Get pane state
        /// </summary>
        public VBDockingPanelState State
        {
            get
            {
                return _state;
            }
        }


        public static readonly DependencyProperty IsUndockedProperty
            = DependencyProperty.Register("IsUndocked", typeof(bool), typeof(VBDockingPanelToolWindow));

        public bool IsUndocked
        {
            get
            {
                return (bool)GetValue(IsUndockedProperty);
            }
            set
            {
                SetValue(IsUndockedProperty, value);
            }
        }

        public static readonly DependencyProperty IsDragableProperty
            = DependencyProperty.Register("IsDragable", typeof(bool), typeof(VBDockingPanelToolWindow));

        public bool IsDragable
        {
            get
            {
                return (bool)GetValue(IsDragableProperty);
            }
            set
            {
                SetValue(IsDragableProperty, value);
            }
        }

        /*public bool ShowHeader
        {
            get
            {
                if (PART_PanelHeader == null)
                    return false;
                return PART_PanelHeader.Visibility == Visibility.Visible;
            }
            set
            {
                if (PART_PanelHeader == null)
                    return;
                if (value)
                    PART_PanelHeader.Visibility = Visibility.Visible;
                else
                    PART_PanelHeader.Visibility = Visibility.Collapsed;
            }
        }*/

        /// <summary>
        /// Current docking border
        /// </summary>
        Dock _dock = Dock.Right;

        /// <summary>
        /// Current docking border
        /// </summary>
        public Dock Dock
        {
            get
            {
                return _dock;
            }
        }


        /// <summary>
        /// Active visible content
        /// </summary>
        public override VBDockingContainerBase ActiveContent
        {
            get
            {
                if (VisibleContents.Count == 1)
                    return VisibleContents[0];
                else if (VisibleContents.Count > 1)
                {
                    if (PART_tbcContents.SelectedIndex <= 0)
                        return PART_tbcContents != null ? VisibleContents[0] : null;
                    else
                        return PART_tbcContents != null ? VisibleContents[PART_tbcContents.SelectedIndex] : null;
                }

                return null;
            }
            set
            {
                if ((VisibleContents.Count > 1) && PART_tbcContents != null)
                {
                    PART_tbcContents.SelectedIndex = VisibleContents.IndexOf(value);
                }
            }
        }
        /// <summary>
        /// Event raised when title is changed
        /// </summary>
        public event EventHandler OnTitleChanged;

        /// <summary>
        /// Change pane's title and fires OnTitleChanged event
        /// </summary>
        public override void RefreshTitle()
        {
            if ((ActiveContent != null) && (PART_tbTitle != null) && (PART_tbcContents != null))
            {
                PART_tbTitle.Text = Title;

                if (PART_tbcContents.Items.Count > 0)
                {
                    SetTabItemHeader(
                        PART_tbcContents.Items[VisibleContents.IndexOf(ActiveContent)] as TabItem,
                        ActiveContent);
                }
                if (OnTitleChanged != null)
                    OnTitleChanged(this, new EventArgs());
            }
        }


        /// <summary>
        /// Get pane title
        /// </summary>
        public virtual string Title
        {
            get
            {
                if (ActiveContent != null)
                    return ActiveContent.Title;

                return null;
            }
        }
        /// <summary>
        /// Get visible contents
        /// </summary>
        public readonly List<VBDockingContainerBase> VisibleContents = new List<VBDockingContainerBase>();

        /// <summary>
        /// Add a dockable content to Contents list
        /// </summary>
        /// <param name="content">Content to add</param>
        /// <remarks>Content is automatically shown.</remarks>
        public override void AddDockingContainerToolWindow(VBDockingContainerBase content)
        {
            if (ContainerToolWindowsList.Count == 0)
            {
                SaveFloatingWindowSizeAndPosition(content);
            }

            base.AddDockingContainerToolWindow(content);
        }

        /// <summary>
        /// Remove a content from pane Contents list
        /// </summary>
        /// <param name="content">Content to remove</param>
        /// <remarks>Notice that when no more contents are present in a pane, it is automatically removed</remarks>
        public override void RemoveDockingContainerToolWindow(VBDockingContainerBase content)
        {
            Hide(content);

            base.RemoveDockingContainerToolWindow(content);

            if (ContainerToolWindowsList.Count == 0)
                DockManager.RemoveDockingPanelToolWindow(this);
        }

        /// <summary>
        /// Show a content previuosly added
        /// </summary>
        /// <param name="content">DockableContent object to show</param>
        public override void Show(VBDockingContainerBase content)
        {
            AddVisibleContent(content);

            if (VisibleContents.Count == 1 && State == VBDockingPanelState.Hidden)
            {
                ChangeState(VBDockingPanelState.Docked);
                if(DockManager != null)
                    DockManager.DragPanelServices.Register(this);
            }


            base.Show(content);
        }

        /// <summary>
        /// Hide a contained dockable content
        /// </summary>
        /// <param name="content">DockableContent object to hide</param>
        /// <remarks>Pane is automatically hidden if no more visible contents are shown</remarks>
        public override void Hide(VBDockingContainerBase content)
        {
            RemoveVisibleContent(content);

            //if (VisibleContents.Count == 0 && State == VBDockingPanelState.Docked)
            //    if(ptStartDrag.X != 0 && ptStartDrag.Y != 0)
            //        Hide();
            //    else
            //        AutoHide();
            //else
                Hide();

            base.Hide(content);
        }

        /// <summary>
        /// Add a visible content
        /// </summary>
        /// <param name="content">DockableContent object to add</param>
        /// <remarks>If more then one contents are visible, this method dinamically creates a tab control and
        /// adds new content to it.</remarks>
        void AddVisibleContent(VBDockingContainerBase content)
        {
            if (VisibleContents.Contains(content))
                return;

            if (VisibleContents.Count == 0)
            {
                VisibleContents.Add(content);
                ShowSingleContent(content);
            }
            else if (VisibleContents.Count == 1)
            {
                HideSingleContent(VisibleContents[0]);
                AddItem(VisibleContents[0]);
                VisibleContents.Add(content);
                AddItem(content);
                ShowTabs();
            }
            else
            {
                VisibleContents.Add(content);
                AddItem(content);
            }
            if (this.ActiveContent != null && this.ActiveContent.VBDesignContent != null && VBDockingManager.GetCloseButtonVisibility(this.ActiveContent.VBDesignContent) == Global.ControlModes.Enabled)
            {
                PART_CloseButton.Visibility = System.Windows.Visibility.Visible;
                PART_menuClose.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                PART_CloseButton.Visibility = System.Windows.Visibility.Hidden;
                if (PART_menuClose != null)
                    PART_menuClose.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Remove a visible content from pane
        /// </summary>
        /// <param name="content">DockableContent object to remove</param>
        /// <remarks>Remove related tab item from contents tab control. if only one content is visible than hide tab control.</remarks>
        void RemoveVisibleContent(VBDockingContainerBase content)
        {
            if (!VisibleContents.Contains(content))
                return;

            if (VisibleContents.Count == 1)
            {
                VisibleContents.Remove(content);
                HideSingleContent(content);
                HideTabs();
            }
            else if (VisibleContents.Count == 2)
            {
                RemoveItem(VisibleContents[0]);
                RemoveItem(VisibleContents[1]);
                VisibleContents.Remove(content);
                ShowSingleContent(VisibleContents[0]);
                HideTabs();
            }
            else
            {
                VisibleContents.Remove(content);
                RemoveItem(content);
            }
        }

        /// <summary>
        /// Close a dockable content
        /// </summary>
        /// <param name="content">DockableContent object to close</param>
        /// <remarks>In this library version this method simply hide the content</remarks>
        public override void Close(VBDockingContainerBase content)
        {
            Hide(content);
        }

        #region Contents management
        private bool IsSingleContentVisible
        {
            get
            {
                if (PART_cpClientWindowContent == null)
                    return false;
                return PART_cpClientWindowContent.Visibility == Visibility.Visible;
            }
        }

        void ShowSingleContent(VBDockingContainerBase content)
        {
            if (PART_cpClientWindowContent == null)
                return;
            PART_cpClientWindowContent.Content = content.Content;
            PART_cpClientWindowContent.Visibility = Visibility.Visible;
            RefreshTitle();
        }

        void HideSingleContent(VBDockingContainerBase content)
        {
            if (PART_cpClientWindowContent == null)
                return;
            PART_cpClientWindowContent.Content = null;
            PART_cpClientWindowContent.Visibility = Visibility.Collapsed;
        }


        private bool IsContentsTbcVisible
        {
            get
            {
                if (PART_tbcContents == null)
                    return false;
                return PART_tbcContents.Visibility == Visibility.Visible;
            }
        }

        bool _SelectionChangedSubscr = false;
        protected void ShowTabs()
        {
            if (PART_tbcContents == null)
                return;
            if (!_SelectionChangedSubscr)
            {
                PART_tbcContents.SelectionChanged += new SelectionChangedEventHandler(_tabs_SelectionChanged);
                _SelectionChangedSubscr = true;
            }
            PART_tbcContents.Visibility = Visibility.Visible;
        }

        protected void HideTabs()
        {
            if (PART_tbcContents == null)
                return;
            if (_SelectionChangedSubscr)
            {
                PART_tbcContents.SelectionChanged -= new SelectionChangedEventHandler(_tabs_SelectionChanged);
                _SelectionChangedSubscr = false;
            }
            PART_tbcContents.Visibility = Visibility.Collapsed;
        }

        void SetTabItemHeader(TabItem item, VBDockingContainerBase content)
        {
            StackPanel spHeader = new StackPanel();
            spHeader.Orientation = Orientation.Horizontal;
            Image iconContent = new Image();
            iconContent.Source = content.Icon;
            spHeader.Children.Add(iconContent);
            TextBlock titleContent = new TextBlock();
            titleContent.Text = content.Title;
            titleContent.Margin = new Thickness(2, 0, 0, 0);
            spHeader.Children.Add(titleContent);
            item.Header = spHeader;
        }

        protected virtual void AddItem(VBDockingContainerBase content)
        {
            if (PART_tbcContents == null)
                return;

            VBTabItem vbTabItem = new VBTabItem();
            if (content.VBDesignContent != null)
            {
                vbTabItem.TabVisibilityACUrl = VBDockingManager.GetTabVisibilityACUrl(content.VBDesignContent);
                if (VBDockingManager.GetIsCloseableBSORoot(content.VBDesignContent))
                    vbTabItem.WithVisibleCloseButton = true;
                if (VBDockingManager.GetRibbonBarVisibility(content.VBDesignContent) != datamodel.Global.ControlModes.Hidden)
                    vbTabItem.ShowRibbonBar = true;
            }
            vbTabItem.Content = new ContentPresenter();
            (vbTabItem.Content as ContentPresenter).Content = content.Content;

            PART_tbcContents.Items.Add(vbTabItem);
            PART_tbcContents.SelectedItem = vbTabItem;

            RefreshTitle();
        }


        protected virtual void RemoveItem(VBDockingContainerBase content)
        {
            if (PART_tbcContents == null)
                return;
            foreach (TabItem item in PART_tbcContents.Items)
                if ((item.Content as ContentPresenter).Content == content.Content)
                {
                    //item.PreviewMouseDown -= new MouseButtonEventHandler(OnTabItemMouseDown);
                    //item.MouseMove -= new MouseEventHandler(OnTabItemMouseMove);
                    //item.MouseUp -= new MouseButtonEventHandler(OnTabItemMouseUp);

                    item.Content = null;
                    PART_tbcContents.Items.Remove(item);
                    //ChangeTitle();
                    break;
                }
        }


        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (this.ActiveContent != null && this.ActiveContent.ContextACObject != null)
            {
                this.Root().RootPageWPF.CurrentACComponent = this.ActiveContent.ContextACObject as IACComponent;
            }
            base.OnPreviewMouseDown(e);
        }

        void _tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PART_tbcContents == null)
                return;
            if (PART_tbcContents.SelectedIndex >= 0)
                RefreshTitle();
            if (this.ActiveContent != null)
            {
                if (this.ActiveContent.ContextACObject != null)
                {
                    this.Root().RootPageWPF.CurrentACComponent = this.ActiveContent.ContextACObject as IACComponent;
                }
            }
            else
            {
                if (this.DockManager != null)
                {
                    this.Root().RootPageWPF.CurrentACComponent = null;
                }
            }
        }

        #endregion

        //List<EventHandler> _list = new List<EventHandler>();

        ///// <summary>
        ///// Event fired when pane internal state is changed
        ///// </summary>
        //public event EventHandler OnStateChanged
        //{
        //    add { _list.Add(value); }
        //    remove { _list.Remove(value); }
        //}

        public EventHandler OnStateChanged;

        /// <summary>
        /// Fires OnStateChanged event
        /// </summary>
        private void FireOnOnStateChanged()
        {
            if (OnStateChanged != null)
                OnStateChanged(this, EventArgs.Empty);
            //foreach (EventHandler eh in _list)
            //    eh(this, EventArgs.Empty);

        }

        /// <summary>
        /// Change pane internal state
        /// </summary>
        /// <param name="newState">New pane state</param>
        /// <remarks>OnStateChanged event is raised only if newState is different from State.</remarks>
        public void ChangeState(VBDockingPanelState newState)
        {
            if (State != newState)
            {
                SaveSize();

                _lastState = _state;
                SetState(newState);

                FireOnOnStateChanged();
            }
        }


        /// <summary>
        /// Return true if pane is hidden, ie State is different from PaneState.Docked
        /// </summary>
        public override bool IsHidden
        {
            get
            {
                return State != VBDockingPanelState.Docked;
            }
        }

        /// <summary>
        /// Internal last pane state
        /// </summary>
        VBDockingPanelState _lastState = VBDockingPanelState.Docked;

        /// <summary>
        /// Show this pane and all contained contents
        /// </summary>
        public override void Show()
        {
            foreach (VBDockingContainerToolWindow content in ContainerToolWindowsList)
                Show(content);

            if (State == VBDockingPanelState.AutoHide || State == VBDockingPanelState.Hidden)
                ChangeState(VBDockingPanelState.Docked);

            base.Show();
        }

        /// <summary>
        /// Hide this pane and all contained contents
        /// </summary>
        public override void Hide()
        {
            foreach (VBDockingContainerToolWindow content in ContainerToolWindowsList)
            {
                RemoveVisibleContent(content);
            }

            ChangeState(VBDockingPanelState.Hidden);
            base.Hide();
        }

        /// <summary>
        /// Close this pane
        /// </summary>
        /// <remarks>Consider that in this version library this method simply hides the pane.</remarks>
        public override void Close()
        {
            Hide();

            base.Close();
        }


        public void FloatingWindow(Rect wndPos)
        {
            ptFloatingWindow = wndPos.Location;
            sizeFloatingWindow = wndPos.Size;
            FloatingWindow();
        }

        /// <summary>
        /// Create and show a floating window hosting this pane
        /// </summary>
        public virtual void FloatingWindow()
        {
            if (_lastState != VBDockingPanelState.FloatingWindow && State != VBDockingPanelState.FloatingWindow)
            {
                ChangeState(VBDockingPanelState.FloatingWindow);
                VBDockingManager parentDockManager = DockManager;
                VBWindowDockingUndocked wnd = new VBWindowDockingUndocked(this);
                SetFloatingWindowSizeAndPosition(wnd);
                
                if (DockManager.ParentWindow is VBDockingContainerToolWindow)
                {
                    VBDockingContainerToolWindow dockContainerToolW = DockManager.ParentWindow as VBDockingContainerToolWindow;
                    if (dockContainerToolW.VBDockingPanel is VBDockingPanelTabbedDoc)
                    {
                        wnd.Owner = (dockContainerToolW.VBDockingPanel as VBDockingPanelTabbedDoc).DockManager.ParentWindow;
                        parentDockManager = (dockContainerToolW.VBDockingPanel as VBDockingPanelTabbedDoc).DockManager;
                    }
                    else
                    {
                        //wnd.Owner = DockManager.ParentWindow;
                    }
                        
                }
                else
                    wnd.Owner = DockManager.ParentWindow;
                //wnd.Owner = DockManager.ParentWindow;
                ChangeState(VBDockingPanelState.FloatingWindow);
                wnd.Show();
            }
        }

        /// <summary>
        /// Create and show a dockable window hosting this pane
        /// </summary>
        public virtual void DockableWindow()
        {
            VBWindowDockingUndocked wnd = new VBWindowDockingUndocked(this);
            SetFloatingWindowSizeAndPosition(wnd);

            ChangeState(VBDockingPanelState.DockableWindow);

            if (DockManager.ParentWindow is VBDockingContainerToolWindow)
            {
                VBDockingContainerToolWindow dockContainerToolW = DockManager.ParentWindow as VBDockingContainerToolWindow;
                if (dockContainerToolW.VBDockingPanel is VBDockingPanelTabbedDoc)
                    wnd.Owner = (dockContainerToolW.VBDockingPanel as VBDockingPanelTabbedDoc).DockManager.ParentWindow;
                else
                    wnd.Owner = DockManager.ParentWindow;
            }
            else
                wnd.Owner = DockManager.ParentWindow;
            wnd.Show();
        }

        /// <summary>
        /// Show contained contents as documents and close this pane
        /// </summary>
        public virtual void TabbedDocument()
        {
            ChangeState(VBDockingPanelState.TabbedDocument);
            while (ContainerToolWindowsList.Count > 0)
            {
                VBDockingContainerBase contentToRemove = ContainerToolWindowsList[0];
                RemoveDockingContainerToolWindow(contentToRemove);
                DockManager.AddDockingContainerToolWindow_GetDockingPanel(contentToRemove);
            }
        }

        /// <summary>
        /// Dock this pane to a destination pane border
        /// </summary>
        /// <param name="destinationPane"></param>
        /// <param name="relativeDock"></param>
        internal void MoveTo(VBDockingPanelBase destinationPane, Dock relativeDock)
        {
            VBDockingPanelToolWindow dockableDestPane = destinationPane as VBDockingPanelToolWindow;
            if (dockableDestPane != null)
                ChangeDock(dockableDestPane.Dock);
            else
                ChangeDock(relativeDock);


            DockManager.MoveTo(this, destinationPane, relativeDock);
            ChangeState(VBDockingPanelState.Docked);
            //Show();
            //ChangeState(PaneState.Docked);
        }

        /// <summary>
        /// Move contained contents into a destination pane and close this one
        /// </summary>
        /// <param name="destinationPane"></param>
        internal void MoveInto(VBDockingPanelBase destinationPane)
        {
            //DockablePane dockableDestPane = destinationPane as DockablePane;
            //if (dockableDestPane != null)
            //    ChangeDock(dockableDestPane.Dock);

            DockManager.MoveInto(this, destinationPane);

            //if (destinationPane is DocumentsPane)
            //    ChangeState(PaneState.TabbedDocument);
            //else
            //    ChangeState(PaneState.Docked);
        }

        /// <summary>
        /// Event raised when Dock property is changed
        /// </summary>
        public event EventHandler OnDockChanged;

        /// <summary>
        /// Fires OnDockChanged
        /// </summary>
        private void FireOnOnDockChanged()
        {
            if (OnDockChanged != null)
                OnDockChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Change dock border
        /// </summary>
        /// <param name="dock">New dock border</param>
        public void ChangeDock(Dock dock)
        {
            //if (dock != _dock)
            {
                //SaveSize();

                _dock = dock;

                FireOnOnDockChanged();

                ChangeState(VBDockingPanelState.Docked);
                //Show();
            }
        }

        /// <summary>
        /// Auto-hide this pane 
        /// </summary>
        public void AutoHide()
        {
            foreach (VBDockingContainerToolWindow content in ContainerToolWindowsList)
                RemoveVisibleContent(content);

            ChangeState(VBDockingPanelState.AutoHide);

            if (DockManager != null)
                DockManager.DragPanelServices.Unregister(this);
        }

        /// <summary>
        /// Handles effective pane resizing 
        /// </summary>
        /// <param name="sizeInfo"></param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            //SaveSize();
            base.OnRenderSizeChanged(sizeInfo);
            SaveSize();
        }


        /// <summary>
        /// Save current pane size
        /// </summary>
        public override void SaveSize()
        {
            if (IsHidden)
                return;

            if (Dock == Dock.Left || Dock == Dock.Right)
            {
                if (ActualWidth > 20 && ActualWidth < 1500)
                    PaneWidth = ActualWidth;
                else if (PaneWidth <= 20)
                    PaneWidth = PaneDefaultWidth;
            }
            else
            {
                if (ActualHeight > 20 && ActualHeight < 1500)
                    PaneHeight = ActualHeight;
                else if (PaneHeight <= 20)
                    PaneHeight = PaneDefaultHeight;
            }

            base.SaveSize();
        }


        Point ptFloatingWindow = new Point(0, 0);
        Size sizeFloatingWindow = new Size(300,400);

        internal void SaveFloatingWindowSizeAndPosition(Window fw)
        {
            if (!double.IsNaN(fw.Left) && !double.IsNaN(fw.Top))
                ptFloatingWindow = new Point(fw.Left, fw.Top);

            if (!double.IsNaN(fw.Width) && !double.IsNaN(fw.Height))
            {
                sizeFloatingWindow.Width = fw.Width;
                sizeFloatingWindow.Height = fw.Height;
            }
            else
            {
                VBDockingContainerBase docCont = fw as VBDockingContainerBase;
                if (docCont != null)
                {
                    if (docCont.VBDesignContent != null)
                    {
                        try
                        {
                            Size size = VBDockingManager.GetWindowSize(docCont.VBDesignContent);
                            if (size != null && !double.IsNaN(size.Width) && !double.IsNaN(size.Height) && size.Width >= 0 && size.Height >= 0)
                            {
                                sizeFloatingWindow.Height = size.Height;
                                sizeFloatingWindow.Width = size.Width;
                            }
                            if (sizeFloatingWindow.Width < 50 || sizeFloatingWindow.Height < 50)
                            {
                                sizeFloatingWindow.Height = 400;
                                sizeFloatingWindow.Width = 400;
                            }
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("VBDockingPanelToolWindow", "SaveFloatingWindowSizeAndPosition", msg);
                        }
                    }
                }
            }

            if ((DockManager != null) && (ContainerToolWindowsList.Count > 0))
            {
                foreach (VBDockingContainerBase dockingContainer in ContainerToolWindowsList)
                {
                    DockManager.SaveUserFloatingWindowSize(dockingContainer, ptFloatingWindow, sizeFloatingWindow);
                }
            }
        }

        internal void SetFloatingWindowSizeAndPosition(VBWindowDockingUndocked fw)
        {
            fw.Left = ptFloatingWindow.X;
            fw.Top = ptFloatingWindow.Y;
            fw.Width = sizeFloatingWindow.Width;
            fw.Height = sizeFloatingWindow.Height;
        }

        /// <summary>
        /// Get swith options context menu
        /// </summary>
        internal ContextMenu OptionsMenu
        {
            get
            {
                return PART_ContextMenu;
            }
        }

        /// <summary>
        /// Handles user click on OptionsMenu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        protected virtual void OnDockingMenu(object sender, EventArgs e)
        {
            if (sender == PART_menuTabbedDocument)
                TabbedDocument();
            if (sender == PART_menuFloatingWindow)
                FloatingWindow();
            if (sender == PART_menuDockedWindow)
                DockableWindow();


            if (sender == PART_menuAutoHide)
            {
                if (PART_menuAutoHide.IsChecked)
                    ChangeState(VBDockingPanelState.Docked);
                else
                    AutoHide();
            }
            if (sender == PART_menuClose)
            {
                Hide();
                SwitchToCloseWindow();
            }
        }
        /// <summary>
        /// Show switch options menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnBtnMenuMouseDown(object sender, RoutedEventArgs e)
        {
            if (PART_ContextMenu == null)
                return;
            PART_ContextMenu.IsOpen = true;
            e.Handled = true;
        }

        /// <summary>
        /// Handles user click event on auto-hide button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnBtnAutoHideMouseDown(object sender, RoutedEventArgs e)
        {
            if (State == VBDockingPanelState.AutoHide)
                ChangeState(VBDockingPanelState.Docked);
            else
                AutoHide();

            e.Handled = true;
        }

        /// <summary>
        /// Handles user click event on close button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnBtnCloseMouseDown(object sender, RoutedEventArgs e)
        {
            Hide();
            SwitchToCloseWindow();
            e.Handled = true;
        }

        public void SwitchToCloseWindow()
        {
            foreach (VBDockingContainerToolWindow content in ContainerToolWindowsList)
            {
                content.OnCloseWindow();
            }
            Close();
        }

        public override bool CloseWindow()
        {
            if (ActiveContent != null)
            {
                if (!ActiveContent.OnCloseWindow())
                {
                    return false;
                }

                Close(ActiveContent);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Enable/disable switch options menu items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnBtnMenuPopup(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (PART_ContextMenu == null)
                return;
            if (PART_ContextMenu.IsOpen
                && (PART_menuFloatingWindow != null)
                && (PART_menuDockedWindow != null)
                && (PART_menuTabbedDocument != null)
                && (PART_menuAutoHide != null))
            {
                PART_menuFloatingWindow.IsEnabled = _state != VBDockingPanelState.AutoHide && _state != VBDockingPanelState.Hidden;
                PART_menuFloatingWindow.IsChecked = _state == VBDockingPanelState.FloatingWindow;
                PART_menuDockedWindow.IsEnabled = _state != VBDockingPanelState.AutoHide && _state != VBDockingPanelState.Hidden;
                PART_menuDockedWindow.IsChecked = _state == VBDockingPanelState.Docked || _state == VBDockingPanelState.DockableWindow;
                PART_menuTabbedDocument.IsEnabled = _state != VBDockingPanelState.AutoHide && _state != VBDockingPanelState.Hidden;
                PART_menuAutoHide.IsChecked = _state == VBDockingPanelState.AutoHide;
            }
        }

        /// <summary>
        /// Drag starting point
        /// </summary>
        Point ptStartDrag;

        /// <summary>
        /// Handles mouse douwn event on pane header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Save current mouse position in ptStartDrag and capture mouse event on PART_PanelHeader object.</remarks>
        void OnHeaderMouseDown(object sender, MouseEventArgs e)
        {
            if (DockManager == null)
                return;

            if (!PART_PanelHeader.IsMouseCaptured)
            {
                ptStartDrag = e.GetPosition(this);
                PART_PanelHeader.CaptureMouse();
            }
        }
        /// <summary>
        /// Handles mouse up event on pane header
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>Release any mouse capture</remarks>
        void OnHeaderMouseUp(object sender, MouseEventArgs e)
        {
            PART_PanelHeader.ReleaseMouseCapture();
        }

        /// <summary>
        /// Handles mouse move event and eventually starts draging this pane
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnHeaderMouseMove(object sender, MouseEventArgs e)
        {
            if (this.IsDragable == false)
                return;
            if (PART_PanelHeader.IsMouseCaptured && Math.Abs(ptStartDrag.X - e.GetPosition(this).X) > 4)
            {
                PART_PanelHeader.ReleaseMouseCapture();
                DragPane(DockManager.PointToScreen(e.GetPosition(DockManager)), e.GetPosition(PART_PanelHeader));
            }
        }

        /// <summary>
        /// Initiate a dragging operation of this pane, relative DockManager is also involved
        /// </summary>
        /// <param name="startDragPoint"></param>
        /// <param name="offset"></param>
        protected virtual void DragPane(Point startDragPoint, Point offset)
        {
            VBWindowDockingUndocked wnd = new VBWindowDockingUndocked(this);
            SetFloatingWindowSizeAndPosition(wnd);

            ChangeState(VBDockingPanelState.DockableWindow);
            DockManager.Drag(wnd, startDragPoint, offset);
            //ChangeState(VBDockingPanelState.DockableWindow);
        }


        /// <summary>
        /// Mouse down event on a content tab item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTabItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            //TabItem item = sender as TabItem;
            FrameworkElement senderElement = sender as FrameworkElement;
            //TabItem item = senderElement.TemplatedParent as TabItem;

            if (!senderElement.IsMouseCaptured)
            {
                ptStartDrag = e.GetPosition(this);
                senderElement.CaptureMouse();
            }

        }

        /// <summary>
        /// Mouse move event on a content tab item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>If mouse is moved when left button is pressed than this method starts a dragging content operations. Also in this case relative DockManager is involved.</remarks>
        void OnTabItemMouseMove(object sender, MouseEventArgs e)
        {
            if (PART_tbcContents == null)
                return;
            //TabItem item = sender as TabItem;
            FrameworkElement senderElement = sender as FrameworkElement;
            TabItem item = senderElement.TemplatedParent as TabItem;

            if (senderElement.IsMouseCaptured && Math.Abs(ptStartDrag.X - e.GetPosition(this).X) > 4)
            {
                senderElement.ReleaseMouseCapture();
                VBDockingContainerToolWindow contentToDrag = ContainerToolWindowsList[PART_tbcContents.Items.IndexOf(item)] as VBDockingContainerToolWindow;
                if (contentToDrag != null)
                    DragContent(contentToDrag, e.GetPosition(DockManager), e.GetPosition(item));
            }
        }

        /// <summary>
        /// Handles MouseUp event fired from a content tab item and eventually release mouse event capture 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnTabItemMouseUp(object sender, MouseButtonEventArgs e)
        {
            //TabItem item = sender as TabItem;
            FrameworkElement senderElement = sender as FrameworkElement;
            //TabItem item = senderElement.TemplatedParent as TabItem;

            senderElement.ReleaseMouseCapture();
        }


        #region persistence

        public override void Serialize(XmlDocument doc, XmlNode parentNode)
        {
            SaveSize();

            parentNode.Attributes.Append(doc.CreateAttribute("Dock"));
            parentNode.Attributes["Dock"].Value = _dock.ToString();
            parentNode.Attributes.Append(doc.CreateAttribute("State"));
            parentNode.Attributes["State"].Value = _state.ToString();
            parentNode.Attributes.Append(doc.CreateAttribute("LastState"));
            parentNode.Attributes["LastState"].Value = _lastState.ToString();


            parentNode.Attributes.Append(doc.CreateAttribute("ptFloatingWindow"));
            parentNode.Attributes["ptFloatingWindow"].Value = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Point)).ConvertToInvariantString(ptFloatingWindow);
            parentNode.Attributes.Append(doc.CreateAttribute("sizeFloatingWindow"));
            parentNode.Attributes["sizeFloatingWindow"].Value = System.ComponentModel.TypeDescriptor.GetConverter(typeof(Size)).ConvertToInvariantString(sizeFloatingWindow);

            base.Serialize(doc, parentNode);
        }

        public override void Deserialize(VBDockingManager managerToAttach, XmlNode node, GetContentFromTypeString getObjectHandler)
        {
            base.Deserialize(managerToAttach, node, getObjectHandler);

            _dock = (Dock)Enum.Parse(typeof(Dock), node.Attributes["Dock"].Value);
            SetState((VBDockingPanelState)Enum.Parse(typeof(VBDockingPanelState), node.Attributes["State"].Value));
            _lastState = (VBDockingPanelState)Enum.Parse(typeof(VBDockingPanelState), node.Attributes["LastState"].Value);

            ptFloatingWindow = (Point)System.ComponentModel.TypeDescriptor.GetConverter(typeof(Point)).ConvertFromInvariantString(node.Attributes["ptFloatingWindow"].Value);
            sizeFloatingWindow = (Size)System.ComponentModel.TypeDescriptor.GetConverter(typeof(Size)).ConvertFromInvariantString(node.Attributes["sizeFloatingWindow"].Value);



            if (State == VBDockingPanelState.FloatingWindow)
                FloatingWindow();
            else if (State == VBDockingPanelState.DockableWindow)
                DockableWindow();

            DockManager.AttachDockingPanelToolWindowEvents(this);
        }

        #endregion
    }
}
