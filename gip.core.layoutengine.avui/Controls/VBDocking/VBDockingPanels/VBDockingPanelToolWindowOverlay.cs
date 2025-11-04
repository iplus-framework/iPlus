using Avalonia.Controls;
using Avalonia.Media;

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
        public VBDockingPanelToolWindowOverlay() : this(null, null, Dock.Bottom)
        {
        }

        public VBDockingPanelToolWindowOverlay(VBDockingManager dockManager, VBDockingContainerToolWindow content, Dock initialDock)
            : base(dockManager, initialDock)
        {
            if (PART_HideButton != null)
            {
                var ltctl = PART_HideButton.Parent as LayoutTransformControl;
                if (ltctl != null && ltctl.LayoutTransform == null)
                    ltctl.LayoutTransform = new RotateTransform(90);
            }
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
