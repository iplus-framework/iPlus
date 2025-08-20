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
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Controls.Primitives;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a floating window.
    /// </summary>
    [TemplatePart(Name = "PART_HideButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MenuButton", Type = typeof(Button))]
    public class VBWindowDockingUndocked : VBWindow
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "DockFloatingWindowStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDocking/Themes/DockingWindowStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "DockFloatingWindowStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDocking/Themes/DockingWindowStyleAero.xaml" },
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

        static VBWindowDockingUndocked()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBWindowDockingUndocked), new FrameworkPropertyMetadata(typeof(VBWindowDockingUndocked)));
        }

        private const int WM_MOVE = 0x0003;
        private const int WM_SIZE = 0x0005;
        private const int WM_NCMOUSEMOVE = 0xa0;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int WM_NCLBUTTONUP = 0xA2;
        private const int WM_NCLBUTTONDBLCLK = 0xA3;
        private const int WM_NCRBUTTONDOWN = 0xA4;
        private const int WM_NCRBUTTONUP = 0xA5;
        private const int HTCAPTION = 2;
        private const int SC_MOVE = 0xF010;
        private const int WM_SYSCOMMAND = 0x0112;

        internal VBDockingPanelToolWindowUndocked HostedPane;
        //public readonly DockablePane ReferencedPane;

        bool _themeApplied = false;

        /// <summary>
        /// Creates a new instance of VBWindowDockingUndocked.
        /// </summary>
        public VBWindowDockingUndocked()
        {
            InitVBDockingUndockedWindow(null);
        }

        /// <summary>
        /// Creates a new instance of VBWindowDockingUndocked.
        /// </summary>
        /// <param name="panel">The panel parameter.</param>
        public VBWindowDockingUndocked(VBDockingPanelToolWindow panel)
        {
            InitVBDockingUndockedWindow(panel);
        }

        protected void InitVBDockingUndockedWindow(VBDockingPanelToolWindow panel)
        {
            #region Hosted Pane
            if (panel != null)
            {
                HostedPane = new VBDockingPanelToolWindowUndocked(this, panel);
                if (!_OnTitleChangedSubsc)
                {
                    HostedPane.ReferencedPane.OnTitleChanged += new EventHandler(HostedPane_OnTitleChanged);
                    _OnTitleChangedSubsc = true;
                }
                //Content = HostedPane;
                //Title = HostedPane.Title;
                //RefreshTitle();
            }
            #endregion
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            if (!_themeApplied)
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
            RegisterEvents();
            if (_PART_cpClientWindowContent != null)
                _PART_cpClientWindowContent.Content = HostedPane;
            else
                Content = HostedPane;
            Title = HostedPane.Title;
            RefreshTitle();
            if (VBDockingManager.GetCloseButtonVisibility(HostedPane.ActiveContent.VBDesignContent) == Global.ControlModes.Enabled && PART_CloseButton != null)
            {
                PART_CloseButton.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                PART_CloseButton.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        internal override void DeInitVBControl(IACComponent bso = null)
        {
            UnRegisterEvents();
            if (HostedPane != null && HostedPane.ReferencedPane != null)
            {
                HostedPane.ReferencedPane.OnTitleChanged -= HostedPane_OnTitleChanged;
                _OnTitleChangedSubsc = false;
            }
            HostedPane = null;
            _PART_HideButton = null;
            _PART_MenuButton = null;
            base.DeInitVBControl(bso);
        }


        private void RegisterEvents()
        {
            //partObject = (object)contentControl.Template.FindName("PART_HideButton", contentControl);
            object partObject = (object)GetTemplateChild("PART_HideButton");
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
        }

        private void UnRegisterEvents()
        {
            if (_PART_HideButton != null)
            {
                _PART_HideButton.Click -= OnBtnAutoHideMouseDown;
            }

            if (_PART_MenuButton != null)
            {
                _PART_MenuButton.Click -= OnBtnMenuMouseDown;
            }
        }


        private Button _PART_HideButton;
        /// <summary>
        /// Gets the PART_HideButton.
        /// </summary>
        public Button PART_HideButton
        {
            get
            {
                return _PART_HideButton;
            }
        }

        private Button _PART_MenuButton;
        /// <summary>
        /// Gets the PART_MenuButton.
        /// </summary>
        public Button PART_MenuButton
        {
            get
            {
                return _PART_MenuButton;
            }
        }

        private bool IsSingleContentVisible
        {
            get
            {
                if (PART_cpClientWindowContent == null)
                    return false;
                return PART_cpClientWindowContent.Visibility == Visibility.Visible;
            }
        }

        void ShowSingleContent(VBDockingContainerToolWindow content)
        {
            if (PART_cpClientWindowContent == null)
                return;
            PART_cpClientWindowContent.Content = content.Content;
            PART_cpClientWindowContent.Visibility = Visibility.Visible;
            RefreshTitle();
        }

        void HideSingleContent(VBDockingContainerToolWindow content)
        {
            if (PART_cpClientWindowContent == null)
                return;
            PART_cpClientWindowContent.Content = null;
            PART_cpClientWindowContent.Visibility = Visibility.Collapsed;
        }


        void OnBtnAutoHideMouseDown(object sender, RoutedEventArgs e)
        {
            HostedPane.SwitchToAutoHideWindow();
        }

        protected override void OnCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            // TODO: Rechte: Fesnter sollte nicht geschlossen werden
            //HostedPane.SwitchToAutoHideWindow();
            HostedPane.SwitchToClosedWindow();
        }

        void OnBtnMenuMouseDown(object sender, RoutedEventArgs e)
        {
            DisplayContextMenu();
        }


        bool _OnTitleChangedSubsc = false;
        void HostedPane_OnTitleChanged(object sender, EventArgs e)
        {
            Title = HostedPane.Title;
            RefreshTitle();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            HostedPane.ReferencedPane.SaveFloatingWindowSizeAndPosition(this);
            if (_OnTitleChangedSubsc)
            {
                HostedPane.ReferencedPane.OnTitleChanged -= new EventHandler(HostedPane_OnTitleChanged);
                _OnTitleChangedSubsc = false;
            }
            HostedPane.Close();

            if (_hwndSource != null)
                _hwndSource.RemoveHook(_wndProcHandler);

            base.OnClosing(e);
        }

        HwndSource _hwndSource;
        HwndSourceHook _wndProcHandler;

        protected void OnLoaded(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
            _wndProcHandler = new HwndSourceHook(HookHandler);
            _hwndSource.AddHook(_wndProcHandler);
        }

        protected override void OnHeaderMouseDown(object sender, MouseEventArgs e)
        {
            base.OnHeaderMouseDown(sender, e);
        }

        protected override void OnHeaderMouseUp(object sender, MouseEventArgs e)
        {
            base.OnHeaderMouseUp(sender, e);
        }

        protected override void OnHeaderMouseLeftDown(object sender, MouseEventArgs e)
        {
            base.OnHeaderMouseLeftDown(sender, e);
            if (HostedPane.ReferencedPane.State == VBDockingPanelState.DockableWindow)
            {
                Point pos = e.GetPosition(this);
                if(HostedPane != null && HostedPane.ReferencedPane != null && HostedPane.ReferencedPane.DockManager != null)
                    HostedPane.ReferencedPane.DockManager.Drag(this, new Point(pos.X, pos.Y), new Point(pos.X, pos.Y));
            }
        }

        protected override void OnHeaderMouseRightDown(object sender, MouseEventArgs e)
        {
            base.OnHeaderMouseRightDown(sender, e);
            DisplayContextMenu();
        }

        private void DisplayContextMenu()
        {
            Point pos = Mouse.GetPosition(this);
            ContextMenu cxMenu = HostedPane.OptionsMenu;
            cxMenu.Placement = PlacementMode.AbsolutePoint;
            cxMenu.PlacementRectangle = new Rect(new Point(Left + pos.X, Top + pos.Y), new Size(0, 0));
            cxMenu.PlacementTarget = this;
            cxMenu.IsOpen = true;
        }

        protected override void OnHeaderMouseMove(object sender, MouseEventArgs e)
        {
            base.OnHeaderMouseMove(sender, e);
            if (HostedPane != null && HostedPane.ReferencedPane != null)
                HostedPane.ReferencedPane.SaveFloatingWindowSizeAndPosition(this);
        }


        // Hook Handler wird nur dann gebraucht, wenn Fenster einen Rahmen hat
        // this.WindowStyle = WindowStyle.None;
        // this.AllowsTransparency = true;
        private IntPtr HookHandler(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled
        )
        {
            handled = false;

            switch (msg)
            {
                case WM_SIZE:
                case WM_MOVE:
                    HostedPane.ReferencedPane.SaveFloatingWindowSizeAndPosition(this);
                    break;
                case WM_NCLBUTTONDOWN:
                    if (HostedPane.ReferencedPane.State == VBDockingPanelState.DockableWindow && wParam.ToInt32() == HTCAPTION)
                    {
                        short x = (short)((lParam.ToInt32() & 0xFFFF));
                        short y = (short)((lParam.ToInt32() >> 16));


                        HostedPane.ReferencedPane.DockManager.Drag(this, new Point(x, y), new Point(x - Left, y - Top));

                        handled = true;
                    }
                    break;
                case WM_NCLBUTTONDBLCLK:
                    if (HostedPane.ReferencedPane.State == VBDockingPanelState.DockableWindow && wParam.ToInt32() == HTCAPTION)
                    {
                        //
                        HostedPane.ReferencedPane.ChangeState(VBDockingPanelState.Docked);
                        HostedPane.ReferencedPane.Show();
                        this.Close();

                        handled = true;
                    }
                    break;
                case WM_NCRBUTTONDOWN:
                    if (wParam.ToInt32() == HTCAPTION)
                    {
                        short x = (short)((lParam.ToInt32() & 0xFFFF));
                        short y = (short)((lParam.ToInt32() >> 16));

                        ContextMenu cxMenu = HostedPane.OptionsMenu;
                        cxMenu.Placement = PlacementMode.AbsolutePoint;
                        cxMenu.PlacementRectangle = new Rect(new Point(x, y), new Size(0, 0));
                        cxMenu.PlacementTarget = this;
                        cxMenu.IsOpen = true;

                        handled = true;
                    }
                    break;
                case WM_NCRBUTTONUP:
                    if (wParam.ToInt32() == HTCAPTION)
                    {

                        handled = true;
                    }
                    break;

            }


            return IntPtr.Zero;
        }
    }
}
