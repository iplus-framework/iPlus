using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represent a dockable button control.
    /// </summary>
    public class VBDockingButton : VBButton
    {
        static VBDockingButton()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(VBDockingButton), new FrameworkPropertyMetadata(typeof(VBDockingButton)));

            DockableContentProperty = DependencyProperty.Register("DockableContent", typeof(VBDockingContainerToolWindow), typeof(VBDockingButton));
            DockingButtonGroupProperty = DependencyProperty.Register("DockingButtonGroup", typeof(VBDockingButtonGroup), typeof(VBDockingButton));
            DockProperty = DependencyProperty.Register("Dock", typeof(Dock), typeof(VBDockingButton));
        }

        public VBDockingButton() : base()
        {
            Click += new RoutedEventHandler(VBDockingButton_Click);
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

        public static DependencyProperty DockableContentProperty;

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

        public static DependencyProperty DockingButtonGroupProperty;

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

        public static DependencyProperty DockProperty;

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
