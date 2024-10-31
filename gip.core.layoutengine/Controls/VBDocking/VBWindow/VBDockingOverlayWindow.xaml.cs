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
using System.IO;
using System.Windows.Markup;

namespace gip.core.layoutengine
{
    class VBDockingOverlayWindowButton : IVBDockDropSurface
    {
        VBDockingOverlayWindow _owner;
        public readonly Button _btnDock;
        public VBDockingOverlayWindowButton(Button btnDock, VBDockingOverlayWindow owner) : this(btnDock, owner, true)
        {

        }
        public VBDockingOverlayWindowButton(Button btnDock, VBDockingOverlayWindow owner, bool enabled)
        {
            _btnDock = btnDock;
            _owner = owner;
            Enabled = enabled;
        } 
        
        /// <summary>
        /// Determines is control enabled or disabled.
        /// </summary>
        public bool Enabled = true;



        #region IDropSurface

        public Rect SurfaceRectangle
        {
            get 
            {
                if (!_owner.IsLoaded)
                    return new Rect();

                return new Rect(_btnDock.PointToScreen(new Point(0,0)), new Size(_btnDock.ActualWidth, _btnDock.ActualHeight)); 
            }
        }

        public void OnDockDragEnter(Point point)
        {
            
        }

        public void OnDockDragOver(Point point)
        {
            
        }

        public void OnDockDragLeave(Point point)
        {
            
        }

        public bool OnDockDrop(Point point)
        {
            if (!Enabled)
                return false;

            return _owner.OnDrop(_btnDock, point);
        }

        #endregion
    }

    /// <summary>
    /// Interaction logic for OverlayWindow.xaml
    /// </summary>
    public partial class VBDockingOverlayWindow : System.Windows.Window
    {
        VBDockingOverlayWindowButton owdBottom;
        VBDockingOverlayWindowButton owdTop;
        VBDockingOverlayWindowButton owdLeft;
        VBDockingOverlayWindowButton owdRight;
        VBDockingOverlayWindowButton owdInto;

        VBDockingManager _owner;

        public VBDockingManager DockManager
        {
            get { return _owner; }
        }


        public VBDockingOverlayWindow(VBDockingManager owner)
        {
            //if (!Application.Current.Resources.Contains("DockDownButtonSyle"))
            //{ 
            //    using (FileStream fs = new FileStream(@"generic.xaml", FileMode.Open, FileAccess.Read))
            //    {
            //        ResourceDictionary resources = (ResourceDictionary)XamlReader.Load(fs);
            //        Application.Current.Resources.Add("DockDownButtonSyle", resources["DockDownButtonSyle"]);
            //    }
            //}

            InitializeComponent();

            _owner = owner;

            DockManager.DragPanelServices.Register(new VBDockingOverlayWindowButton(btnDockBottom, this));
            DockManager.DragPanelServices.Register(new VBDockingOverlayWindowButton(btnDockTop, this));
            DockManager.DragPanelServices.Register(new VBDockingOverlayWindowButton(btnDockLeft, this));
            DockManager.DragPanelServices.Register(new VBDockingOverlayWindowButton(btnDockRight, this));

            owdBottom   = new VBDockingOverlayWindowButton(btnDockPaneBottom, this, false);
            owdTop      = new VBDockingOverlayWindowButton(btnDockPaneTop,    this, false);
            owdLeft     = new VBDockingOverlayWindowButton(btnDockPaneLeft,   this, false);
            owdRight    = new VBDockingOverlayWindowButton(btnDockPaneRight,  this, false);
            owdInto     = new VBDockingOverlayWindowButton(btnDockPaneInto, this, false);



            DockManager.DragPanelServices.Register(owdBottom);
            DockManager.DragPanelServices.Register(owdTop);
            DockManager.DragPanelServices.Register(owdLeft);
            DockManager.DragPanelServices.Register(owdRight);
            DockManager.DragPanelServices.Register(owdInto);

            //gridPaneRelativeDockingOptions.Width = 88;
            //gridPaneRelativeDockingOptions.Height = 88;
        }

        internal bool OnDrop(Button btnDock, Point point)
        {
            if (btnDock == btnDockBottom)
                DockManager.DragPanelServices.FloatingWindow.HostedPane.ReferencedPane.ChangeDock(Dock.Bottom);
            else if (btnDock == btnDockLeft)
                DockManager.DragPanelServices.FloatingWindow.HostedPane.ReferencedPane.ChangeDock(Dock.Left);
            else if (btnDock == btnDockRight)
                DockManager.DragPanelServices.FloatingWindow.HostedPane.ReferencedPane.ChangeDock(Dock.Right);
            else if (btnDock == btnDockTop)
                DockManager.DragPanelServices.FloatingWindow.HostedPane.ReferencedPane.ChangeDock(Dock.Top);
            else if (btnDock == btnDockPaneTop)
                DockManager.DragPanelServices.FloatingWindow.HostedPane.ReferencedPane.MoveTo(CurrentDropPane, Dock.Top);
            else if (btnDock == btnDockPaneBottom)
                DockManager.DragPanelServices.FloatingWindow.HostedPane.ReferencedPane.MoveTo(CurrentDropPane, Dock.Bottom);
            else if (btnDock == btnDockPaneLeft)
                DockManager.DragPanelServices.FloatingWindow.HostedPane.ReferencedPane.MoveTo(CurrentDropPane, Dock.Left);
            else if (btnDock == btnDockPaneRight)
                DockManager.DragPanelServices.FloatingWindow.HostedPane.ReferencedPane.MoveTo(CurrentDropPane, Dock.Right);
            else if (btnDock == btnDockPaneInto)
            {
                DockManager.DragPanelServices.FloatingWindow.HostedPane.ReferencedPane.MoveInto(CurrentDropPane);
                return true;
            }

            DockManager.DragPanelServices.FloatingWindow.HostedPane.ReferencedPane.Show();

            return true;
        }

        VBDockingPanelBase CurrentDropPane = null;

        public void ShowOverlayPaneDockingOptions(VBDockingPanelBase pane)
        {
            Rect rectPane = pane.SurfaceRectangle;

            Point myScreenTopLeft = PointToScreen(new Point(0, 0));
            rectPane.Offset(-myScreenTopLeft.X, -myScreenTopLeft.Y);//relative to me
            gridPaneRelativeDockingOptions.SetValue(Canvas.LeftProperty, rectPane.Left+rectPane.Width/2-gridPaneRelativeDockingOptions.Width/2);
            gridPaneRelativeDockingOptions.SetValue(Canvas.TopProperty, rectPane.Top + rectPane.Height/2-gridPaneRelativeDockingOptions.Height/2);
            gridPaneRelativeDockingOptions.Visibility = Visibility.Visible;

            owdBottom.Enabled = true;
            owdTop   .Enabled = true;
            owdLeft  .Enabled = true;
            owdRight .Enabled = true;
            owdInto  .Enabled = true;
            CurrentDropPane = pane;
        }

        public void HideOverlayPaneDockingOptions(VBDockingPanelBase surfaceElement)
        {
            owdBottom.Enabled = false;
            owdTop.Enabled = false;
            owdLeft.Enabled = false;
            owdRight.Enabled = false;
            owdInto.Enabled = false; 
            
            gridPaneRelativeDockingOptions.Visibility = Visibility.Collapsed;
            CurrentDropPane = null;
        }
    
    }
}
