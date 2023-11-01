using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace gip.core.layoutengine
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBMenuItemMobile'}de{'VBMenuItemMobile'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBMenuItemMobile : VBMenuItem, IACInteractiveObject, IACObject
    {
        static VBMenuItemMobile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBMenuItemMobile), new FrameworkPropertyMetadata(typeof(VBMenuItemMobile)));
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(VBMenuItemMobile), new PropertyMetadata(false, OnIsExpandedChanged));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ToggleSubmenuVisibility();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            IsExpanded = !IsExpanded;

            e.Handled = true;
        }

        /// <summary>
        /// Creates a new instance of VBMenuItem.
        /// </summary>
        public VBMenuItemMobile()
        {
        }

        /// <summary>
        /// Creates a new instance of VBMenuItem.
        /// </summary>
        /// <param name="acComponent">The acComponent parameter.</param>
        /// <param name="acCommand">The acCommand parameter.</param>
        public VBMenuItemMobile(IACObject acComponent, ACCommand acCommand)
        {
            ContextACObject = acComponent;
            ACCommand = acCommand;

            if (acCommand is ACMenuItem)
            {
                ACMenuItem menuItem = acCommand as ACMenuItem;
                if (!string.IsNullOrEmpty(menuItem.IconACUrl))
                    LoadIcon(menuItem.IconACUrl);
            }

            if (!string.IsNullOrEmpty(acCommand.GetACUrl()))
            {
                Command = AppCommands.AddApplicationCommand(ACCommand);
                CommandBindings.Add(new CommandBinding(Command, VBMenuItem_Click, VBMenuItem_IsEnabled));
            }
        }

        #region Expand

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var menuItem = d as VBMenuItemMobile;
            if (menuItem != null)
            {
                menuItem.ToggleSubmenuVisibility();
            }
        }

        private bool isSubmenuOpen = false;

        private void ToggleSubmenuVisibility()
        {
            if (IsExpanded)
            {
                if (!isSubmenuOpen)
                {
                    var popup = GetTemplateChild("PART_Popup") as Popup;
                    if (popup != null)
                    {
                        double submenuWidth = ActualWidth - Margin.Left - Margin.Right;

                        var submenuBorder = GetTemplateChild("SubMenuBorder") as FrameworkElement;
                        double availableSpace = submenuBorder.ActualWidth;

                        if (submenuWidth > availableSpace)
                        {
                            popup.Placement = PlacementMode.Left;
                        }
                        else
                        {
                            popup.Placement = PlacementMode.Right;
                        }

                        popup.IsOpen = true;
                        isSubmenuOpen = true;
                    }
                }
            }
            else
            {
                isSubmenuOpen = false;
                var popup = GetTemplateChild("PART_Popup") as Popup;
                if (popup != null)
                {
                    popup.IsOpen = false;
                }
            }
        }



        protected override void OnSubmenuOpened(RoutedEventArgs e)
        {
            base.OnSubmenuOpened(e);
            ToggleSubmenuVisibility();
        }

        protected override void OnSubmenuClosed(RoutedEventArgs e)
        {
            base.OnSubmenuClosed(e);
            ToggleSubmenuVisibility();
        }

        #endregion
    }
}
