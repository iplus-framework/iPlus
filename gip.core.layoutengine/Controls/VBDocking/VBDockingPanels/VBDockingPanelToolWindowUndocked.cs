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
using gip.core.datamodel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Verwendung:
    /// -VBWindowDockingUndocked
    /// </summary>
    class VBDockingPanelToolWindowUndocked : VBDockingPanelToolWindow
    {
        private VBDockingPanelToolWindow _ReferencedPane;
        public VBDockingPanelToolWindow ReferencedPane
        {
            get
            {
                return _ReferencedPane;
            }
            protected set
            {
                _ReferencedPane = value;
            }
        }
        VBWindowDockingUndocked _floatingWindow;

        #region c´tors
        public VBDockingPanelToolWindowUndocked(VBWindowDockingUndocked floatingWindow, VBDockingPanelToolWindow referencedPane) : base(referencedPane.DockManager)
        {
            ReferencedPane = referencedPane;
            _floatingWindow = floatingWindow;

            VBDockingContainerBase lastSelectedContent = ReferencedPane.ActiveContent;

            ChangeState(ReferencedPane.State);
            //ReferencedPane.Hide();

            //DockManager = ReferencedPane.DockManager;
            foreach (VBDockingContainerToolWindow content in ReferencedPane.ContainerToolWindowsList)
            {
                ReferencedPane.Hide(content);
                AddDockingContainerToolWindow(content);
                Show(content);
                content.SetDockingPanel(ReferencedPane);
            }

            ActiveContent = lastSelectedContent;
            IsUndocked = true;
            ActiveContent.Closing += ActiveContent_Closing;
            //ShowHeader = false;
        }

        void ActiveContent_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SwitchToClosedWindow();
        }

        internal override void DeInitVBControl(IACComponent bso = null)
        {
            ActiveContent.Closing -= ActiveContent_Closing;
            base.DeInitVBControl(bso);
            _floatingWindow = null;
            ReferencedPane = null;
        }
        #endregion

        public override void RemoveDockingContainerToolWindow(VBDockingContainerBase content)
        {
            ReferencedPane.RemoveDockingContainerToolWindow(content);
            base.RemoveDockingContainerToolWindow(content);
        }

        protected override void OnDockingMenu(object sender, EventArgs e)
        {
            if (sender == PART_menuFloatingWindow)
                SwitchToUndockedWindow();
            else if (sender == PART_menuDockedWindow)
                SwitchToDockedWindow();
            else if (sender == PART_menuTabbedDocument)
                SwitchToTabbedDocument();
            else if (sender == PART_menuClose)
                //SwitchToAutoHideWindow();
                SwitchToClosedWindow();
            else if (sender == PART_menuAutoHide)
                SwitchToAutoHideWindow();
        }

        public void SwitchToUndockedWindow()
        {
            ReferencedPane.ChangeState(VBDockingPanelState.FloatingWindow);
            ChangeState(ReferencedPane.State);
        }

        public void SwitchToDockedWindow()
        {
            ReferencedPane.ChangeState(VBDockingPanelState.DockableWindow);
            ChangeState(ReferencedPane.State);
        }

        public void SwitchToTabbedDocument()
        {
            foreach (VBDockingContainerToolWindow content in ContainerToolWindowsList)
            {
                content.SetDockingPanel(ReferencedPane);
            }
            Close();
            _floatingWindow.Close();
            ReferencedPane.TabbedDocument();
        }

        public void SwitchToClosedWindow()
        {
            foreach (VBDockingContainerToolWindow content in ContainerToolWindowsList)
            {
                 content.SetDockingPanel(ReferencedPane);
                 content.OnCloseWindow();
            }
            Close();
            _floatingWindow.Close();
            ReferencedPane.Close();
        }

        public void SwitchToAutoHideWindow()
        {
            if (State == VBDockingPanelState.FloatingWindow || State == VBDockingPanelState.Docked || State == VBDockingPanelState.DockableWindow)
            {
                foreach (VBDockingContainerToolWindow content in ContainerToolWindowsList)
                    content.SetDockingPanel(ReferencedPane);
                Close();
                _floatingWindow.Focus();
                _floatingWindow.Close();
                ReferencedPane.Show();
                ReferencedPane.AutoHide();
            }
        }

        //protected override void DragContent(DockableContent contentToDrag, Point startDragPoint, Point offset)
        //{
        //    Remove(contentToDrag);
        //    DockablePane pane = new DockablePane();
        //    pane = new DockablePane();
        //    pane.DockManager = DockManager;
        //    pane.Add(contentToDrag);
        //    pane.Show();
        //    DockManager.Add(pane);
        //    //DockManager.Add(contentToDrag);
        //    //pane.ChangeState(PaneState.DockableWindow);
        //    FloatingWindow wnd = new FloatingWindow(pane);
        //    pane.ChangeState(PaneState.DockableWindow);
        //    DockManager.Drag(wnd, startDragPoint, offset);            
        //}
    }
}
