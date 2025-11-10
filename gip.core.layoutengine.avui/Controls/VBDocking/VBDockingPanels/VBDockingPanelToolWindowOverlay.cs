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
        private VBDockingPanelToolWindow _ReferencedPane;
        public VBDockingPanelToolWindow ReferencedPane
        {
            get
            {
                return _ReferencedPane;
            }
        }

        private VBDockingContainerToolWindow _ReferencedContent;
        public VBDockingContainerToolWindow ReferencedContent
        {
            get
            {
                return _ReferencedContent;
            }
        }

        #region c´tors
        public VBDockingPanelToolWindowOverlay() : this(null, null, Avalonia.Controls.Dock.Bottom)
        {
        }

        public VBDockingPanelToolWindowOverlay(VBDockingManagerOldWPF dockManager, VBDockingContainerToolWindow content, Avalonia.Controls.Dock initialDock)
            : base(dockManager, initialDock)
        {
        }

        internal void InitWhenTemplateWasApplied(VBDockingContainerToolWindow content)
        {
            if (PART_HideButton != null)
            {
                var ltctl = PART_HideButton.Parent as LayoutTransformControl;
                if (ltctl != null && ltctl.LayoutTransform == null)
                    ltctl.LayoutTransform = new RotateTransform(90);
            }
            _ReferencedPane = content.VBDockingPanel as VBDockingPanelToolWindow;
            _ReferencedContent = content;
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
