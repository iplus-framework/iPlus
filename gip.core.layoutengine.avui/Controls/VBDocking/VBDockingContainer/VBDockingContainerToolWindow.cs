using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform;
using gip.core.datamodel;
using gip.ext.design.avui;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a content embeddable in a dockable pane or in a documents pane.
    /// </summary>
    public abstract class VBDockingContainerToolWindow : VBDockingContainerBase
    {
        #region c'tors
        public VBDockingContainerToolWindow()
        {
        }

        public VBDockingContainerToolWindow(VBDockingManager manager)
            : base(manager)
        {
        }

        public VBDockingContainerToolWindow(VBDockingManager manager, Control vbDesignContent)
            : base(manager, vbDesignContent)
        {
            RefreshTitle();
        }
        #endregion

        #region Methods Docking-Framework
        public override void ReInitDataContext()
        {
            if (VBDockingPanel != null)
                VBDockingPanel.Show(this);
            RefreshTitle();
        }

        public override bool OnCloseWindow()
        {
            //ReleaseACObjectRef();
            if (DockManager != null)
                DockManager.CloseAndRemoveVBDesign(VBDesignContent);

            RemoveFromComponentReference();
            if (VBDockingPanel is VBDockingPanelToolWindow)
            {
                VBDockingPanelToolWindow toolWindow = VBDockingPanel as VBDockingPanelToolWindow;
                if (DockManager != null && toolWindow.State == VBDockingPanelState.AutoHide)
                {
                    DockManager.RemoveDockingButton(this);
                }
            }
            DeInitVBControl();
            base.OnCloseWindow();
            return true;
        }

        /// <summary>
        /// Show this content
        /// </summary>
        /// <remarks>Show this content in a dockable pane. If no pane was previuosly created, it creates a new one with default right dock. </remarks>
        public void Show(PointerPressedEventArgs e, SettingsVBDesignWndPos wndPos = null)
        {
            if (VBDockingPanel != null)
            {
                VBDockingPanel.Show(this);
            }
            else
            {
                if (VBDesignContent != null)
                {
                    if (VBDockingManager.GetDockState(VBDesignContent) == datamodel.Global.VBDesignDockState.Docked)
                    {
                        Show(TranslateDock(VBDockingManager.GetDockPosition(VBDesignContent)));
                    }
                    else if (VBDockingManager.GetDockState(VBDesignContent) == datamodel.Global.VBDesignDockState.FloatingWindow)
                    {
                        Show(TranslateDock(VBDockingManager.GetDockPosition(VBDesignContent)));
                        if (_vbDockingPanel != null)
                        {
                            double desiredWidth = 300;
                            double desiredHeight = 400;
                            if (VBDesignContent is Control)
                            {
                                if ((VBDesignContent as Control).Width > 50 && (VBDesignContent as Control).Width < 800)
                                    desiredWidth = (VBDesignContent as Control).Width;
                                if ((VBDesignContent as Control).Height > 50 && (VBDesignContent as Control).Height < 600)
                                    desiredHeight = (VBDesignContent as Control).Height;
                            }
                            if (wndPos != null)
                            {
                                Rect wndRect = wndPos.WndRect;
                                Point maxBounds = GetMaxBoundsVirtualSreens();
                                if (wndPos.WndRect.Position.X > (maxBounds.X - 50)
                                    || wndPos.WndRect.Position.Y > (maxBounds.Y - 50))
                                    wndRect = new Rect(DockManager.PointToScreen(e != null ? e.GetPosition(DockManager) : new Point()).ToPoint(1.0), new Size(desiredWidth, desiredHeight));
                                else if (ControlManager.RestoreWindowsOnSameScreen)
                                {
                                    Window window = DockManager.TryFindParent<Window>();
                                    if (window != null)
                                    {
                                        var screenOfDockManager = Screens.ScreenFromWindow(window);
                                        var screenOfToolWindow = Screens.ScreenFromBounds(new PixelRect((int)wndRect.X, (int)wndRect.Y, (int)wndRect.Width, (int)wndRect.Height));
                                        //var screenOfToolWindow = LocatedOnScreen(wndRect);
                                        if (screenOfDockManager != screenOfToolWindow)
                                            wndRect = new Rect(DockManager.PointToScreen(e != null ? e.GetPosition(DockManager) : new Point()).ToPoint(1.0), new Size(desiredWidth, desiredHeight));
                                    }
                                }
                                (_vbDockingPanel as VBDockingPanelToolWindow).FloatingWindow(wndRect);
                            }
                            else
                            {
                                Rect wndRect;
                                if (DockManager.IsLoaded)
                                    wndRect = new Rect(DockManager.PointToScreen(e != null ? e.GetPosition(DockManager) : new Point()).ToPoint(1.0), new Size(desiredWidth, desiredHeight));
                                else
                                    wndRect = new Rect(new Point(50,50), new Size(desiredWidth, desiredHeight));
                                (_vbDockingPanel as VBDockingPanelToolWindow).FloatingWindow(wndRect);
                            }
                        }
                    }
                    else if (VBDockingManager.GetDockState(VBDesignContent) == datamodel.Global.VBDesignDockState.Tabbed)
                    {
                        ShowAsDocument();
                    }
                    else //if (this.ACDesignObject.DockState == datamodel.Global.ACDesignDockState.AutoHideButton)
                    {
                        Show(TranslateDock(VBDockingManager.GetDockPosition(VBDesignContent)));
                        if (_vbDockingPanel != null)
                            (_vbDockingPanel as VBDockingPanelToolWindow).AutoHide();
                    }
                }
                else
                {
                    Show(Dock.Right);
                }
            }
        }

        private Point GetMaxBoundsVirtualSreens()
        {
            if (this.Screens?.All == null || !this.Screens.All.Any())
                return new Point(0, 0);

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (var screen in this.Screens.All)
            {
                var bounds = screen.Bounds;
                minX = Math.Min(minX, bounds.X);
                minY = Math.Min(minY, bounds.Y);
                maxX = Math.Max(maxX, bounds.X + bounds.Width);
                maxY = Math.Max(maxY, bounds.Y + bounds.Height);
            }

            return new Point(maxX - minX, maxY - minY);
        }

        private Screen LocatedOnScreen(Rect rect)
        {
            if (this.Screens == null)
                return null;
            PixelRect pixelRect = new PixelRect((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
            foreach (var screen in this.Screens.All)
            {
                if (pixelRect.Intersects(screen.Bounds))
                {
                    return screen;
                }
            }
            return null;
        }

        /// <summary>
        /// Show this content
        /// </summary>
        /// <remarks>Show this content in a dockable pane. If no pane was previuosly created, it creates a new one with passed initial dock. </remarks>
        protected void Show(Dock dock)
        {
            if (VBDockingPanel == null)
            {
                _vbDockingPanel = new VBDockingPanelToolWindow(DockManager, dock);
                DockManager.PART_AvInvisibleInitDummy?.Children.Add(_vbDockingPanel);
                _vbDockingPanel.AddDockingContainerToolWindow(this);
                _vbDockingPanel.Show();
                DockManager.PART_AvInvisibleInitDummy?.Children.Remove(_vbDockingPanel);
                DockManager.AddDockingPanelToolWindow(_vbDockingPanel as VBDockingPanelToolWindow);
            }
            else
            {
                VBDockingPanel.Show(this);
                VBDockingPanel.Show();
            }
        }

        /// <summary>
        /// Show content into default documents pane
        /// </summary>
        protected void ShowAsDocument()
        {
            if (VBDockingPanel == null)
                _vbDockingPanel = DockManager.AddDockingContainerToolWindow_GetDockingPanel(this);

            VBDockingPanel.Show(this);
        }

        /// <summary>
        /// Hides content from container pane
        /// </summary>
        /// <remarks>If container pane doesn't contain any more content, it is automaticly hidden.</remarks>
        public virtual new void Hide()
        {
            if (VBDockingPanel == null)
                return;
            VBDockingPanel.Hide(this);
        }

        public virtual void AutoHide()
        {
            if (VBDockingPanel == null)
                return;
            VBDockingPanelToolWindow vbDockingPanel = VBDockingPanel as VBDockingPanelToolWindow;
            vbDockingPanel.ChangeState(VBDockingPanelState.Docked);

        }

        /// <summary>
        /// Set/get content title whish is shown at top of dockable panes and in tab items
        /// </summary>
        public new string Title
        {
            get { return base.Title; }
            set
            {
                base.Title = value;

                if (VBDockingPanel != null)
                    VBDockingPanel.RefreshTitle();
            }
        }
        #endregion

        #region Methods Extension
        public new void Close()
        {
            try
            {
                base.Close();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDockingContainerToolWindow", "Close", msg);
            }
            RemoveFromComponentReference();
        }
        #endregion
    }
}
