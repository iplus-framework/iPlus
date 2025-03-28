﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace gip.ext.designer.Controls
{
    /// <summary>
    /// A Small icon which shows up a menu containing common properties
    /// </summary>
    public class QuickOperationMenu : Control
    {
        static QuickOperationMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(QuickOperationMenu), new FrameworkPropertyMetadata(typeof(QuickOperationMenu)));
        }

        private MenuItem _mainHeader;

        /// <summary>
        /// Contains Default values in the Sub menu for example "HorizontalAlignment" has "HorizontalAlignment.Stretch" as it's value.
        /// </summary>
        private readonly Dictionary<MenuItem, MenuItem> _defaults = new Dictionary<MenuItem, MenuItem>();

        /// <summary>
        /// Is the main header menu which brings up all the menus.
        /// </summary>
        public MenuItem MainHeader
        {
            get { return _mainHeader; }
            set { _mainHeader = value; }
        }

        protected virtual MenuItem CreateMenuItem(string header)
        {
            return new MenuItem() { Header = header };
        }


        /// <summary>
        /// Add a submenu with checkable values.
        /// </summary>
        /// <param name="parent">The parent menu under which to add.</param>
        /// <param name="enumValues">All the values of an enum to be showed in the menu</param>
        /// <param name="defaultValue">The default value out of all the enums.</param>
        /// <param name="setValue">The presently set value out of the enums</param>
        public void AddSubMenuCheckable(MenuItem parent, Array enumValues, string defaultValue, string setValue)
        {
            foreach (var enumValue in enumValues)
            {
                var menuItem = CreateMenuItem(enumValue.ToString());
                menuItem.IsCheckable = true;
                parent.Items.Add(menuItem);
                if (enumValue.ToString() == defaultValue)
                    _defaults.Add(parent, menuItem);
                if (enumValue.ToString() == setValue)
                    menuItem.IsChecked = true;
            }
        }

        /// <summary>
        /// Add a menu in the main header.
        /// </summary>
        /// <param name="menuItem">The menu to add.</param>
        public void AddSubMenuInTheHeader(MenuItem menuItem)
        {
            if (_mainHeader != null)
                _mainHeader.Items.Add(menuItem);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var mainHeader = Template.FindName("MainHeader", this) as MenuItem;
            if (mainHeader != null)
            {
                _mainHeader = mainHeader;
            }
        }

        /// <summary>
        /// Checks a menu item and making it exclusive. If the check was toggled then the default menu item is selected.
        /// </summary>
        /// <param name="parent">The parent item of the sub menu</param>
        /// <param name="clickedOn">The Item clicked on</param>
        /// <returns>Returns the Default value if the checkable menu item is toggled or otherwise the new checked menu item.</returns>
        public string UncheckChildrenAndSelectClicked(MenuItem parent, MenuItem clickedOn)
        {
            MenuItem defaultMenuItem;
            _defaults.TryGetValue(parent, out defaultMenuItem);
            if (IsAnyItemChecked(parent))
            {
                foreach (var item in parent.Items)
                {
                    var menuItem = item as MenuItem;
                    if (menuItem != null) menuItem.IsChecked = false;
                }
                clickedOn.IsChecked = true;
                return (string)clickedOn.Header;
            }
            else
            {
                if (defaultMenuItem != null)
                {
                    defaultMenuItem.IsChecked = true;
                    return (string)defaultMenuItem.Header;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks in the sub-menu whether aby items has been checked or not
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        private bool IsAnyItemChecked(MenuItem parent)
        {
            bool check = false;
            if (parent.HasItems)
            {
                foreach (var item in parent.Items)
                {
                    var menuItem = item as MenuItem;
                    if (menuItem != null && menuItem.IsChecked)
                        check = true;
                }
            }
            return check;
        }
    }
}
