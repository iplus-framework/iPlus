using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a Windows menu control that enables you to hierarchically organize elements associated with commands and event handlers.
    /// </summary>
    public class VBMenu : Menu
    {
        public VBMenu() : base()
        {
        }

        #region Mobile

        //public void SwitchActiveMenuItems()
        //{
        //    bool activeMenuExpanded = false;

        //    foreach (VBMenuItemMobile menuItem in this.Items)
        //    {
        //        if (menuItem.IsExpanded)
        //        {
        //            menuItem.SwitchActiveMenu();
        //            menuItem.IsExpanded = false;
        //            activeMenuExpanded = true;
        //        }
        //    }

        //    if (!activeMenuExpanded)
        //    {
        //        foreach (VBMenuItemMobile menuItem in this.Items)
        //        {
        //            foreach (var submenuItem in menuItem.subMenu.Items)
        //            {
        //                if (submenuItem is VBMenuItemMobile submenuVBItem && submenuVBItem.subMenuExpanded)
        //                {
        //                    submenuVBItem.SwitchItems();
        //                    submenuVBItem.IsExpanded = false;
        //                    submenuVBItem.subMenuExpanded = false;
        //                }
        //            }
        //        }
        //    }
        //}

        //public void SwitchMainMenuItems()
        //{
        //    if (Parent is VBDockPanel dockPanel)
        //    {
        //        foreach (var uiElement in dockPanel.Children)
        //        {
        //            if (uiElement is VBMenu menu && menu.Name == "MainMenu")
        //                menu.SwitchActiveMenuItems();
        //        }
        //    }
        //}

        //private bool isMenuOpen = false;

        //public void ToggleMenu()
        //{
        //    ThicknessAnimation animation = new ThicknessAnimation
        //    {
        //        Duration = TimeSpan.FromSeconds(0.3)
        //    };

        //    if (!isMenuOpen)
        //    {
        //        animation.From = new Thickness(-this.ActualWidth, 0, 0, 0);
        //        animation.To = new Thickness(0, 0, 0, 0);
        //        this.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        animation.From = new Thickness(0, 0, 0, 0);
        //        animation.To = new Thickness(-this.ActualWidth, 0, 0, 0);
        //        animation.Completed += (s, e) =>
        //        {
        //            this.Visibility = Visibility.Hidden;
        //            this.Margin = new Thickness(-this.ActualWidth, 0, 0, 0);
        //        };
        //    }

        //    this.BeginAnimation(FrameworkElement.MarginProperty, animation);

        //    isMenuOpen = !isMenuOpen;
        //}

        //public void ToggleMenuDelay(bool delayAnimation)
        //{
        //    if (delayAnimation)
        //    {
        //        DispatcherTimer timer = new DispatcherTimer();
        //        timer.Interval = TimeSpan.FromMilliseconds(150);
        //        timer.Tick += (sender, args) =>
        //        {
        //            timer.Stop();
        //            ToggleMenu();
        //        };
        //        timer.Start();
        //    }
        //    else
        //    {
        //        ToggleMenu();
        //    }
        //}

        #endregion
    }
}
