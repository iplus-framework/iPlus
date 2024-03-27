using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace gip.core.layoutengine
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBMenuItemMobile'}de{'VBMenuItemMobile'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBMenuItemMobile : VBMenuItem, IACInteractiveObject, IACObject
    {
        #region c´tors
        static VBMenuItemMobile()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBMenuItemMobile), new FrameworkPropertyMetadata(typeof(VBMenuItemMobile)));
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(VBMenuItemMobile), new PropertyMetadata(false, OnIsExpandedChanged));

       
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
                CommandBindings.Add(new CommandBinding(Command, VBMenuItemMobile_Click, VBMenuItemMobile_IsEnabled));
            }
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (this.subMenu == null && this.mainSubMenu == null || IsExpanded && this.mainSubMenu.Items.Count > 0)
                VBMenuItemMobile_Click();
            else
                IsExpanded = !IsExpanded;

            e.Handled = true;
        }

        private void VBMenuItemMobile_Click()
        {
            ACActionArgs actionArgs = new ACActionArgs(this, 0, 0, Global.ElementActionType.ACCommand);
            ACAction(actionArgs);
        }

        private void VBMenuItemMobile_Click(object sender, RoutedEventArgs e)
        {
            ACActionArgs actionArgs = new ACActionArgs(this, 0, 0, Global.ElementActionType.ACCommand);
            ACAction(actionArgs);
        }

        private void VBMenuItemMobile_IsEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ACCommand.IsAutoEnabled)
            {
                e.CanExecute = true;
                return;
            }
            if (ContextACObject != null)
            {
                ACActionArgs actionArgs = new ACActionArgs(this, 0, 0, Global.ElementActionType.ACCommand);
                e.CanExecute = IsEnabledACAction(actionArgs);
            }
            else
            {
                e.CanExecute = false;
            }
        }

        #endregion

        #region Submenu

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var menuItem = d as VBMenuItemMobile;
            if (menuItem != null && menuItem.IsExpanded)
            {
                menuItem.ShowSubMenu(menuItem);
            }
        }

        public bool subMenuExpanded = false;

        public VBMenu mainSubMenu;

        public VBMenu subMenu;

        public VBMenu CreateSubmenu(VBMenu subMenuMain, ACMenuItemList aCMenuItems)
        {
            this.mainSubMenu = subMenuMain;
            this.subMenu = new VBMenu();

            if (aCMenuItems != null && aCMenuItems.Count > 0)
            {
                foreach (var subMenuItem in aCMenuItems)
                {
                    if (subMenuItem.ACCaption == "-")
                    {
                        VBMenuSeparator _Seperator = new VBMenuSeparator();
                        this.subMenu.Items.Add(_Seperator);
                        continue;
                    }
                    VBMenuItemMobile menuItem = new VBMenuItemMobile(ContextACObject, subMenuItem);
                    this.subMenu.Items.Add(menuItem);
                    if (subMenuItem.Items != null && subMenuItem.Items.Count > 0)
                    {
                        menuItem.CreateSubmenu(subMenuMain, subMenuItem.Items);
                    }
                }
            }
            return subMenu;
        }

        public async void ShowSubMenu(VBMenuItemMobile menuItem)
        {
            if (this.Parent is VBMenu parentMenu && parentMenu.Name == "Submenu")
            {
                parentMenu.SwitchMainMenuItems();
                await Task.Delay(200);
                MoveItems(this.subMenu.Items, mainSubMenu.Items);
                this.subMenuExpanded = true;
                subMenu.ToggleMenu();
            }
            else if (menuItem != null && this.Parent is VBMenu)
            {
                (this.Parent as VBMenu).ToggleMenu();
                await Task.Delay(300);
                MoveItems(this.subMenu.Items, mainSubMenu.Items);
                mainSubMenu.ToggleMenu();
            }
        }

        public void SwitchActiveMenu()
        {
            if (this.subMenu.Items.Count > 0)
            {
                this.subMenu.Items.Clear();
            }
            MoveItems(mainSubMenu.Items, this.subMenu.Items);
        }

        public void SwitchItems()
        {
            VBMenu tempMenu = new VBMenu();
            MoveItems(mainSubMenu.Items, tempMenu.Items);
            MoveItems(this.subMenu.Items, mainSubMenu.Items);
            MoveItems(tempMenu.Items, this.subMenu.Items);
        }

        private static void MoveItems(ItemCollection source, ItemCollection destination)
        {
            var items = new List<object>(source.Cast<object>());
            foreach (var item in items)
            {
                source.Remove(item);
                destination.Add(item);
            }
        }

        protected override void OnSubmenuOpened(RoutedEventArgs e)
        {
            base.OnSubmenuOpened(e);
            if (e.OriginalSource is VBMenuItemMobile vbMenuItem)
            {
                ShowSubMenu(vbMenuItem);
            }
        }

        #endregion
    }
}
