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

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Verwendung:
    /// -VBDockingManager
    /// </summary>
    class VBDockingPanelToolWindowOverlay : VBDockingPanelToolWindow
    {
        public readonly VBDockingPanelToolWindow ReferencedPane;
        public readonly VBDockingContainerToolWindow ReferencedContent;

        #region c´tors
        public VBDockingPanelToolWindowOverlay(VBDockingManager dockManager, VBDockingContainerToolWindow content, Dock initialDock)
            : base(dockManager, initialDock)
        {
            if (PART_HideButton != null)
                PART_HideButton.LayoutTransform = new RotateTransform(90);
            ReferencedPane = content.VBDockingPanel as VBDockingPanelToolWindow;
            ReferencedContent = content;
            AddDockingContainerToolWindow(ReferencedContent);
            Show(ReferencedContent);
            ReferencedContent.SetDockingPanel(ReferencedPane);

            SetState(VBDockingPanelState.AutoHide);
        }
        #endregion

        public override void Show()
        {
            ChangeState(VBDockingPanelState.Docked);
        }

        public override void Close()
        {
            ChangeState(VBDockingPanelState.Hidden);
        }

        public override void Close(VBDockingContainerBase content)
        {
            ChangeState(VBDockingPanelState.Hidden);
        }
    }
}
