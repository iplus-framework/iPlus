using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represent a dockable button control.
    /// </summary>
    public class VBDockingButton : VBButton
    {
        public VBDockingButton() : base()
        {
            Click += VBDockingButton_Click;
        }

        private void VBDockingButton_Click(object sender, RoutedEventArgs e)
        {
            if (DockingContainerToolWindow != null)
            {
                if (VBDockingManager.GetDisableDockingOnClick(DockingContainerToolWindow.VBDesignContent))
                    return;
                DockingContainerToolWindow.AutoHide();
            }
        }

        public static readonly StyledProperty<VBDockingContainerToolWindow> DockableContentProperty = AvaloniaProperty.Register<VBDockingButton, VBDockingContainerToolWindow>(nameof(DockingContainerToolWindow));
        public VBDockingContainerToolWindow DockingContainerToolWindow
        {
            get
            {
                return GetValue(DockableContentProperty) as VBDockingContainerToolWindow;
            }
            set
            {
                SetValue(DockableContentProperty, value);
                if (value != null)
                {
                    Content = value.Title;
                }
            }
        }

        public void RefreshTitle()
        {
            var toolwindow = DockingContainerToolWindow;
            if (toolwindow != null)
            {
                Content = toolwindow.Title;
            }
        }

        public static readonly StyledProperty<VBDockingButtonGroup> DockingButtonGroupProperty = AvaloniaProperty.Register<VBDockingButton, VBDockingButtonGroup>(nameof(DockingButtonGroup));
        public VBDockingButtonGroup DockingButtonGroup
        {
            get
            {
                return GetValue(DockingButtonGroupProperty) as VBDockingButtonGroup;
            }
            set
            {
                SetValue(DockingButtonGroupProperty, value);
                Dock = value.Dock;
            }
        }

        public static readonly StyledProperty<Dock> DockProperty = AvaloniaProperty.Register<VBDockingButton, Dock>(nameof(Dock));
        public Dock Dock
        {
            get
            {
                return (Dock)GetValue(DockProperty);
            }
            set
            {
                SetValue(DockProperty, value);
            }
        }

    }
}
