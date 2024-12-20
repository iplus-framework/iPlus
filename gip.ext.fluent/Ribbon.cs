﻿#region Copyright and License Information
// This is a modification for iplus-framework of the Fluent Ribbon Control Suite
// https://github.com/fluentribbon/Fluent.Ribbon
// Copyright © Degtyarev Daniel, Rikker Serg. 2009-2010.  All rights reserved.
// 
// This code was originally distributed under the Microsoft Public License (Ms-PL). The modifications by gipSoft d.o.o. are now distributed under GPLv3.
// The license is available online https://github.com/fluentribbon/Fluent.Ribbonlicense
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;

namespace Fluent
{
    // TODO: improve style parts naming & using

    /// <summary>
    /// Represents the main Ribbon control which consists of multiple tabs, each of which
    /// containing groups of controls.  The Ribbon also provides improved context
    /// menus, enhanced screen tips, and keyboard shortcuts.
    /// </summary>
    [ContentProperty("Tabs")]
    [SuppressMessage("Microsoft.Maintainability", "CA1506")]
    [SuppressMessage("Microsoft.Design", "CA1001")]
    public class Ribbon : Control
    {
        #region Localization

        // Localizable properties
        static readonly RibbonLocalization localization = new RibbonLocalization();

        /// <summary>
        /// Gets localizable properties
        /// </summary>
        public static RibbonLocalization Localization
        {
            get { return localization; }
        }

        #endregion

        #region Constants

        /// <summary>
        /// Minimal width of ribbon parent window
        /// </summary>
        public const double MinimalVisibleWidth = 300;
        /// <summary>
        /// Minimal height of ribbon parent window
        /// </summary>
        public const double MinimalVisibleHeight = 250;

        #endregion

        #region ContextMenu

        private static Dictionary<int, System.Windows.Controls.ContextMenu> contextMenus = new Dictionary<int, System.Windows.Controls.ContextMenu>();

        /// <summary>
        /// Context menu for ribbon in current thread
        /// </summary>
        public static System.Windows.Controls.ContextMenu RibbonContextMenu
        {
            get
            {
                if (!contextMenus.ContainsKey(Thread.CurrentThread.ManagedThreadId)) InitRibbonContextMenu();
                return contextMenus[Thread.CurrentThread.ManagedThreadId];
            }
        }

        // Context menu owner ribbon
        private static Ribbon contextMenuOwner;

        // Context menu items
        private static Dictionary<int, System.Windows.Controls.MenuItem> addToQuickAccessMenuItemDictionary = new Dictionary<int, System.Windows.Controls.MenuItem>();
        private static System.Windows.Controls.MenuItem addToQuickAccessMenuItem
        {
            get { return addToQuickAccessMenuItemDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }
        private static Dictionary<int, System.Windows.Controls.MenuItem> addGroupToQuickAccessMenuItemDictionary = new Dictionary<int, System.Windows.Controls.MenuItem>();
        private static System.Windows.Controls.MenuItem addGroupToQuickAccessMenuItem
        {
            get { return addGroupToQuickAccessMenuItemDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }
        private static Dictionary<int, System.Windows.Controls.MenuItem> addMenuToQuickAccessMenuItemDictionary = new Dictionary<int, System.Windows.Controls.MenuItem>();
        private static System.Windows.Controls.MenuItem addMenuToQuickAccessMenuItem
        {
            get { return addMenuToQuickAccessMenuItemDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }
        private static Dictionary<int, System.Windows.Controls.MenuItem> addGalleryToQuickAccessMenuItemDictionary = new Dictionary<int, System.Windows.Controls.MenuItem>();
        private static System.Windows.Controls.MenuItem addGalleryToQuickAccessMenuItem
        {
            get { return addGalleryToQuickAccessMenuItemDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }
        private static Dictionary<int, System.Windows.Controls.MenuItem> removeFromQuickAccessMenuItemDictionary = new Dictionary<int, System.Windows.Controls.MenuItem>();
        private static System.Windows.Controls.MenuItem removeFromQuickAccessMenuItem
        {
            get { return removeFromQuickAccessMenuItemDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }
        private static Dictionary<int, System.Windows.Controls.MenuItem> showQuickAccessToolbarBelowTheRibbonMenuItemDictionary = new Dictionary<int, System.Windows.Controls.MenuItem>();
        private static System.Windows.Controls.MenuItem showQuickAccessToolbarBelowTheRibbonMenuItem
        {
            get { return showQuickAccessToolbarBelowTheRibbonMenuItemDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }
        private static Dictionary<int, System.Windows.Controls.MenuItem> showQuickAccessToolbarAboveTheRibbonMenuItemDictionary = new Dictionary<int, System.Windows.Controls.MenuItem>();
        private static System.Windows.Controls.MenuItem showQuickAccessToolbarAboveTheRibbonMenuItem
        {
            get { return showQuickAccessToolbarAboveTheRibbonMenuItemDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }
        private static Dictionary<int, System.Windows.Controls.MenuItem> minimizeTheRibbonMenuItemDictionary = new Dictionary<int, System.Windows.Controls.MenuItem>();
        private static System.Windows.Controls.MenuItem minimizeTheRibbonMenuItem
        {
            get { return minimizeTheRibbonMenuItemDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }
        private static Dictionary<int, System.Windows.Controls.MenuItem> customizeQuickAccessToolbarMenuItemDictionary = new Dictionary<int, System.Windows.Controls.MenuItem>();
        private static System.Windows.Controls.MenuItem customizeQuickAccessToolbarMenuItem
        {
            get { return customizeQuickAccessToolbarMenuItemDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }
        private static Dictionary<int, System.Windows.Controls.MenuItem> customizeTheRibbonMenuItemDictionary = new Dictionary<int, System.Windows.Controls.MenuItem>();
        private static System.Windows.Controls.MenuItem customizeTheRibbonMenuItem
        {
            get { return customizeTheRibbonMenuItemDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }
        private static Dictionary<int, Separator> firstSeparatorDictionary = new Dictionary<int, Separator>();
        private static Separator firstSeparator
        {
            get { return firstSeparatorDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }
        private static Dictionary<int, Separator> secondSeparatorDictionary = new Dictionary<int, Separator>();
        private static Separator secondSeparator
        {
            get { return secondSeparatorDictionary[Thread.CurrentThread.ManagedThreadId]; }
        }

        // Initialize ribbon context menu
        private static void InitRibbonContextMenu()
        {
            contextMenus.Add(Thread.CurrentThread.ManagedThreadId, new System.Windows.Controls.ContextMenu());
            RibbonContextMenu.Opened += OnContextMenuOpened;

            // Add to quick access toolbar
            addToQuickAccessMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, new System.Windows.Controls.MenuItem { Command = Ribbon.AddToQuickAccessCommand });
            RibbonContextMenu.Items.Add(addToQuickAccessMenuItem);
            RibbonControl.Bind(Ribbon.Localization, addToQuickAccessMenuItem, "RibbonContextMenuAddItem", MenuItem.HeaderProperty, BindingMode.OneWay);
            RibbonControl.Bind(RibbonContextMenu, addToQuickAccessMenuItem, "PlacementTarget", MenuItem.CommandParameterProperty, BindingMode.OneWay);

            // Add group to quick access toolbar
            addGroupToQuickAccessMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, new System.Windows.Controls.MenuItem { Command = Ribbon.AddToQuickAccessCommand });
            RibbonContextMenu.Items.Add(addGroupToQuickAccessMenuItem);
            RibbonControl.Bind(Ribbon.Localization, addGroupToQuickAccessMenuItem, "RibbonContextMenuAddGroup", MenuItem.HeaderProperty, BindingMode.OneWay);
            RibbonControl.Bind(RibbonContextMenu, addGroupToQuickAccessMenuItem, "PlacementTarget", MenuItem.CommandParameterProperty, BindingMode.OneWay);

            // Add menu item to quick access toolbar
            addMenuToQuickAccessMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, new System.Windows.Controls.MenuItem { Command = Ribbon.AddToQuickAccessCommand });
            RibbonContextMenu.Items.Add(addMenuToQuickAccessMenuItem);
            RibbonControl.Bind(Ribbon.Localization, addMenuToQuickAccessMenuItem, "RibbonContextMenuAddMenu", MenuItem.HeaderProperty, BindingMode.OneWay);
            RibbonControl.Bind(RibbonContextMenu, addMenuToQuickAccessMenuItem, "PlacementTarget", MenuItem.CommandParameterProperty, BindingMode.OneWay);

            // Add gallery to quick access toolbar
            addGalleryToQuickAccessMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, new System.Windows.Controls.MenuItem { Command = Ribbon.AddToQuickAccessCommand });
            RibbonContextMenu.Items.Add(addGalleryToQuickAccessMenuItem);
            RibbonControl.Bind(Ribbon.Localization, addGalleryToQuickAccessMenuItem, "RibbonContextMenuAddGallery", MenuItem.HeaderProperty, BindingMode.OneWay);
            RibbonControl.Bind(RibbonContextMenu, addGalleryToQuickAccessMenuItem, "PlacementTarget", MenuItem.CommandParameterProperty, BindingMode.OneWay);

            // Remove from quick access toolbar
            removeFromQuickAccessMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, new System.Windows.Controls.MenuItem { Command = Ribbon.RemoveFromQuickAccessCommand });
            RibbonContextMenu.Items.Add(removeFromQuickAccessMenuItem);
            RibbonControl.Bind(Ribbon.Localization, removeFromQuickAccessMenuItem, "RibbonContextMenuRemoveItem", MenuItem.HeaderProperty, BindingMode.OneWay);
            RibbonControl.Bind(RibbonContextMenu, removeFromQuickAccessMenuItem, "PlacementTarget", MenuItem.CommandParameterProperty, BindingMode.OneWay);

            // Separator
            firstSeparatorDictionary.Add(Thread.CurrentThread.ManagedThreadId, new Separator());
            RibbonContextMenu.Items.Add(firstSeparator);

            // Customize quick access toolbar
            customizeQuickAccessToolbarMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, new System.Windows.Controls.MenuItem { Command = Ribbon.CustomizeQuickAccessToolbarCommand });
            RibbonContextMenu.Items.Add(customizeQuickAccessToolbarMenuItem);
            RibbonControl.Bind(Ribbon.Localization, customizeQuickAccessToolbarMenuItem, "RibbonContextMenuCustomizeQuickAccessToolBar", MenuItem.HeaderProperty, BindingMode.OneWay);
            RibbonControl.Bind(RibbonContextMenu, customizeQuickAccessToolbarMenuItem, "PlacementTarget", MenuItem.CommandParameterProperty, BindingMode.OneWay);

            // Show quick access below the ribbon
            showQuickAccessToolbarBelowTheRibbonMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, new System.Windows.Controls.MenuItem { Command = Ribbon.ShowQuickAccessBelowCommand });
            RibbonContextMenu.Items.Add(showQuickAccessToolbarBelowTheRibbonMenuItem);
            RibbonControl.Bind(Ribbon.Localization, showQuickAccessToolbarBelowTheRibbonMenuItem, "RibbonContextMenuShowBelow", MenuItem.HeaderProperty, BindingMode.OneWay);
            RibbonControl.Bind(RibbonContextMenu, showQuickAccessToolbarBelowTheRibbonMenuItem, "PlacementTarget", MenuItem.CommandParameterProperty, BindingMode.OneWay);

            // Show quick access above the ribbon
            showQuickAccessToolbarAboveTheRibbonMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, new System.Windows.Controls.MenuItem { Command = Ribbon.ShowQuickAccessAboveCommand });
            RibbonContextMenu.Items.Add(showQuickAccessToolbarAboveTheRibbonMenuItem);
            RibbonControl.Bind(Ribbon.Localization, showQuickAccessToolbarAboveTheRibbonMenuItem, "RibbonContextMenuShowAbove", MenuItem.HeaderProperty, BindingMode.OneWay);
            RibbonControl.Bind(RibbonContextMenu, showQuickAccessToolbarAboveTheRibbonMenuItem, "PlacementTarget", MenuItem.CommandParameterProperty, BindingMode.OneWay);

            // Separator
            secondSeparatorDictionary.Add(Thread.CurrentThread.ManagedThreadId, new Separator());
            RibbonContextMenu.Items.Add(secondSeparator);

            // Customize the ribbon
            customizeTheRibbonMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, new System.Windows.Controls.MenuItem { Command = Ribbon.CustomizeTheRibbonCommand });
            RibbonContextMenu.Items.Add(customizeTheRibbonMenuItem);
            RibbonControl.Bind(Ribbon.Localization, customizeTheRibbonMenuItem, "RibbonContextMenuCustomizeRibbon", MenuItem.HeaderProperty, BindingMode.OneWay);
            RibbonControl.Bind(RibbonContextMenu, customizeTheRibbonMenuItem, "PlacementTarget", MenuItem.CommandParameterProperty, BindingMode.OneWay);

            // Minimize the ribbon
            minimizeTheRibbonMenuItemDictionary.Add(Thread.CurrentThread.ManagedThreadId, new System.Windows.Controls.MenuItem { Command = Ribbon.ToggleMinimizeTheRibbonCommand });
            RibbonContextMenu.Items.Add(minimizeTheRibbonMenuItem);
            RibbonControl.Bind(Ribbon.Localization, minimizeTheRibbonMenuItem, "RibbonContextMenuMinimizeRibbon", MenuItem.HeaderProperty, BindingMode.OneWay);
            RibbonControl.Bind(RibbonContextMenu, minimizeTheRibbonMenuItem, "PlacementTarget", MenuItem.CommandParameterProperty, BindingMode.OneWay);
        }

        /// <summary>
        /// Invoked whenever an unhandled <see cref="E:System.Windows.FrameworkElement.ContextMenuOpening"/> routed event reaches this class in its route. Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">The <see cref="T:System.Windows.RoutedEventArgs"/> that contains the event data.</param>
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            contextMenuOwner = this;
            base.OnContextMenuOpening(e);
        }

        /// <summary>
        /// Invoked whenever an unhandled <see cref="E:System.Windows.FrameworkElement.ContextMenuClosing"/> routed event reaches this class in its route. Implement this method to add class handling for this event. 
        /// </summary>
        /// <param name="e">Provides data about the event.</param>
        protected override void OnContextMenuClosing(ContextMenuEventArgs e)
        {
            contextMenuOwner = null;
            base.OnContextMenuClosing(e);
        }

        // Occurs when context menu is opening
        private static void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            Ribbon ribbon = contextMenuOwner;
            if ((RibbonContextMenu != null) && (ribbon != null))
            {
                addToQuickAccessMenuItem.CommandTarget = ribbon;
                addGroupToQuickAccessMenuItem.CommandTarget = ribbon;
                addMenuToQuickAccessMenuItem.CommandTarget = ribbon;
                addGalleryToQuickAccessMenuItem.CommandTarget = ribbon;
                removeFromQuickAccessMenuItem.CommandTarget = ribbon;
                customizeQuickAccessToolbarMenuItem.CommandTarget = ribbon;
                customizeTheRibbonMenuItem.CommandTarget = ribbon;
                minimizeTheRibbonMenuItem.CommandTarget = ribbon;
                showQuickAccessToolbarBelowTheRibbonMenuItem.CommandTarget = ribbon;
                showQuickAccessToolbarAboveTheRibbonMenuItem.CommandTarget = ribbon;
                // Hide items for ribbon controls
                addToQuickAccessMenuItem.Visibility = Visibility.Collapsed;
                addGroupToQuickAccessMenuItem.Visibility = Visibility.Collapsed;
                addMenuToQuickAccessMenuItem.Visibility = Visibility.Collapsed;
                addGalleryToQuickAccessMenuItem.Visibility = Visibility.Collapsed;
                removeFromQuickAccessMenuItem.Visibility = Visibility.Collapsed;
                firstSeparator.Visibility = Visibility.Collapsed;
                // Hide customize quick access menu item
                customizeQuickAccessToolbarMenuItem.Visibility = Visibility.Collapsed;
                secondSeparator.Visibility = Visibility.Visible;

                // Set minimize the ribbon menu item state
                minimizeTheRibbonMenuItem.IsChecked = ribbon.IsMinimized;

                // Set customize the ribbon menu item visibility
                if (ribbon.CanCustomizeRibbon) customizeTheRibbonMenuItem.Visibility = Visibility.Visible;
                else customizeTheRibbonMenuItem.Visibility = Visibility.Collapsed;

                // Hide quick access position menu items
                showQuickAccessToolbarBelowTheRibbonMenuItem.Visibility = Visibility.Collapsed;
                showQuickAccessToolbarAboveTheRibbonMenuItem.Visibility = Visibility.Collapsed;

                // If quick access toolbar is visible show 
                if (ribbon.IsQuickAccessToolBarVisible)
                {
                    // Set quick access position menu items visibility
                    if (ribbon.ShowQuickAccessToolBarAboveRibbon) showQuickAccessToolbarBelowTheRibbonMenuItem.Visibility = Visibility.Visible;
                    else showQuickAccessToolbarAboveTheRibbonMenuItem.Visibility = Visibility.Visible;
                    if (ribbon.CanCustomizeQuickAccessToolBar) customizeQuickAccessToolbarMenuItem.Visibility = Visibility.Visible;
                    secondSeparator.Visibility = Visibility.Visible;

                    // Gets control that raise menu opened
                    UIElement control = RibbonContextMenu.PlacementTarget as UIElement;
                    AddToQuickAccessCommand.CanExecute(null, control);
                    RemoveFromQuickAccessCommand.CanExecute(null, control);

                    //Debug.WriteLine("Menu opened on "+control);
                    if (control != null)
                    {
                        firstSeparator.Visibility = Visibility.Visible;
                        if (ribbon.quickAccessElements.ContainsValue(control))
                        {
                            // Control is on quick access
                            removeFromQuickAccessMenuItem.Visibility = Visibility.Visible;
                        }
                        else if (control is System.Windows.Controls.MenuItem)
                        {
                            // Control is menu item
                            addMenuToQuickAccessMenuItem.Visibility = Visibility.Visible;
                        }
                        else if ((control is Gallery) || (control is InRibbonGallery))
                        {
                            // Control is gallery
                            addGalleryToQuickAccessMenuItem.Visibility = Visibility.Visible;
                        }
                        else if (control is RibbonGroupBox)
                        {
                            // Control is group box
                            addGroupToQuickAccessMenuItem.Visibility = Visibility.Visible;
                        }
                        else if (control is IQuickAccessItemProvider)
                        {
                            // Its other control
                            addToQuickAccessMenuItem.Visibility = Visibility.Visible;
                        }
                        else firstSeparator.Visibility = Visibility.Collapsed;
                    }
                }

            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when selected tab has been changed (be aware that SelectedTab can be null)
        /// </summary>
        public event SelectionChangedEventHandler SelectedTabChanged;

        /// <summary>
        /// Occurs when customize the ribbon
        /// </summary>
        public event EventHandler CustomizeTheRibbon;

        /// <summary>
        /// Occurs when customize quick access toolbar
        /// </summary>
        public event EventHandler CustomizeQuickAccessToolbar;

        /// <summary>
        /// Occurs when IsMinimized property is changing
        /// </summary>
        public event DependencyPropertyChangedEventHandler IsMinimizedChanged;

        /// <summary>
        /// Occurs when IsCollapsed property is changing
        /// </summary>
        public event DependencyPropertyChangedEventHandler IsCollapsedChanged;

        #endregion

        #region Fields

        // Collection of contextual tab groups
        private ObservableCollection<RibbonContextualTabGroup> groups;

        // Collection of tabs
        private ObservableCollection<RibbonTabItem> tabs;

        // Collection of toolbar items
        private ObservableCollection<UIElement> toolBarItems;

        // Ribbon quick access toolbar
        private QuickAccessToolBar quickAccessToolBar;

        // Ribbon layout root
        private Panel layoutRoot;

        // Handles F10, Alt and so on
        readonly KeyTipService keyTipService;

        // Collection of quickaccess menu items
        private ObservableCollection<QuickAccessMenuItem> quickAccessItems;

        // Currently added in QAT items
        readonly Dictionary<UIElement, UIElement> quickAccessElements = new Dictionary<UIElement, UIElement>();

        // Stream to save quickaccesselements on aplytemplate
        MemoryStream quickAccessStream;

        private Window ownerWindow;

        #endregion

        #region Properties

        #region Menu

        /// <summary>
        /// Gets or sets file menu control (can be application menu button, backstage button and so on)
        /// </summary>
        public UIElement Menu
        {
            get { return (UIElement)GetValue(MenuProperty); }
            set { SetValue(MenuProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Button. 
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty MenuProperty =
            DependencyProperty.Register("Menu", typeof(UIElement), typeof(Ribbon), new UIPropertyMetadata(null, OnApplicationMenuChanged));

        static void OnApplicationMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Ribbon ribbon = (Ribbon)d;
            if (e.OldValue != null) ribbon.RemoveLogicalChild(e.OldValue);
            if (e.NewValue != null) ribbon.AddLogicalChild(e.NewValue);
        }

        #endregion

        /// <summary>
        /// Window title
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(Ribbon), new UIPropertyMetadata("", OnTitleChanged));

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((d as Ribbon).TitleBar != null) (d as Ribbon).TitleBar.InvalidateMeasure();
        }

        /// <summary>
        /// Gets or sets selected tab item
        /// </summary>
        public RibbonTabItem SelectedTabItem
        {
            get { return (RibbonTabItem)GetValue(SelectedTabItemProperty); }
            set { SetValue(SelectedTabItemProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SelectedTabItem.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SelectedTabItemProperty =
            DependencyProperty.Register("SelectedTabItem", typeof(RibbonTabItem), typeof(Ribbon), new UIPropertyMetadata(null, OnSelectedTabItemChanged));

        private static void OnSelectedTabItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ribbon = (Ribbon)d;
            if (ribbon.TabControl != null)
            {
                ribbon.TabControl.SelectedItem = e.NewValue;
            }

            var selectedItem = e.NewValue as RibbonTabItem;

            if (selectedItem != null
                && ribbon.Tabs.Contains(selectedItem))
            {
                ribbon.SelectedTabIndex = ribbon.Tabs.IndexOf(selectedItem);
            }
            else
            {
                ribbon.SelectedTabIndex = -1;
            }
        }

        /// <summary>
        /// Gets or sets selected tab index
        /// </summary>
        public int SelectedTabIndex
        {
            get { return (int)GetValue(SelectedTabIndexProperty); }
            set { SetValue(SelectedTabIndexProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for SelectedTabindex.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SelectedTabIndexProperty =
            DependencyProperty.Register("SelectedTabIndex", typeof(int), typeof(Ribbon), new UIPropertyMetadata(-1, OnSelectedTabIndexChanged));

        private static void OnSelectedTabIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ribbon = (Ribbon)d;
            var selectedIndex = (int)e.NewValue;

            if (ribbon.TabControl != null)
            {
                ribbon.TabControl.SelectedIndex = selectedIndex;
            }

            if (selectedIndex >= 0
                && selectedIndex < ribbon.Tabs.Count)
            {
                ribbon.SelectedTabItem = ribbon.Tabs[selectedIndex];
            }
            else
            {
                ribbon.SelectedTabItem = null;
            }
        }

        /// <summary>
        /// Gets the first visible TabItem
        /// </summary>
        public RibbonTabItem FirstVisibleItem
        {
            get
            {
                return this.GetFirstVisibleItem();
            }
        }

        /// <summary>
        /// Gets the last visible TabItem
        /// </summary>
        public RibbonTabItem LastVisibleItem
        {
            get
            {
                return this.GetLastVisibleItem();
            }
        }

        /// <summary>
        /// Gets ribbon titlebar
        /// </summary>
        internal RibbonTitleBar TitleBar { get; private set; }

        /// <summary>
        /// Gets the Ribbon tab control
        /// </summary>
        internal RibbonTabControl TabControl { get; private set; }

        /// <summary>
        /// Gets or sets whether quick access toolbar showes above ribbon
        /// </summary>
        public bool ShowQuickAccessToolBarAboveRibbon
        {
            get { return (bool)GetValue(ShowQuickAccessToolBarAboveRibbonProperty); }
            set { SetValue(ShowQuickAccessToolBarAboveRibbonProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for ShowAboveRibbon.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ShowQuickAccessToolBarAboveRibbonProperty =
            DependencyProperty.Register("ShowQuickAccessToolBarAboveRibbon", typeof(bool), typeof(Ribbon), new UIPropertyMetadata(true, OnShowQuickAccesToolBarAboveRibbonChanged));

        /// <summary>
        /// Handles ShowQuickAccessToolBarAboveRibbon property changed
        /// </summary>
        /// <param name="d">Object</param>
        /// <param name="e">The event data</param>
        private static void OnShowQuickAccesToolBarAboveRibbonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Ribbon ribbon = (Ribbon)d;
            if (ribbon.TitleBar != null) ribbon.TitleBar.InvalidateMeasure();
            ribbon.SaveState();
        }

        /// <summary>
        /// Gets collection of contextual tab groups
        /// </summary>
        public ObservableCollection<RibbonContextualTabGroup> ContextualGroups
        {
            get
            {
                if (groups == null)
                {
                    groups = new ObservableCollection<RibbonContextualTabGroup>();
                    groups.CollectionChanged += OnGroupsCollectionChanged;
                }
                return this.groups;
            }
        }
        /// <summary>
        /// Handles collection of contextual tab groups ghanges
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">The event data</param>
        private void OnGroupsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        if (this.TitleBar != null) this.TitleBar.Items.Insert(e.NewStartingIndex + i, e.NewItems[i]);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                    {
                        if (this.TitleBar != null) this.TitleBar.Items.Remove(item);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (object item in e.OldItems)
                    {
                        if (this.TitleBar != null) this.TitleBar.Items.Remove(item);
                    }
                    foreach (object item in e.NewItems)
                    {
                        if (this.TitleBar != null) this.TitleBar.Items.Add(item);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (this.TitleBar != null) this.TitleBar.Items.Clear();
                    break;
            }

        }

        /// <summary>
        /// gets collection of ribbon tabs
        /// </summary>
        public ObservableCollection<RibbonTabItem> Tabs
        {
            get
            {
                if (tabs == null)
                {
                    tabs = new ObservableCollection<RibbonTabItem>();
                    tabs.CollectionChanged += OnTabsCollectionChanged;
                }
                return tabs;
            }
        }

        /// <summary>
        /// Handles collection of ribbon tabs changed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">The event data</param>
        private void OnTabsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (TabControl == null)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        TabControl.Items.Insert(e.NewStartingIndex + i, e.NewItems[i]);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in e.OldItems)
                    {
                        TabControl.Items.Remove(item);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (var item in e.OldItems)
                    {
                        TabControl.Items.Remove(item);
                    }

                    foreach (var item in e.NewItems)
                    {
                        TabControl.Items.Add(item);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    TabControl.Items.Clear();
                    break;
            }
        }

        /// <summary>
        /// Gets collection of toolbar items
        /// </summary>
        public ObservableCollection<UIElement> ToolBarItems
        {
            get
            {
                if (toolBarItems == null)
                {
                    toolBarItems = new ObservableCollection<UIElement>();
                    toolBarItems.CollectionChanged += OnToolbarItemsCollectionChanged;
                }
                return toolBarItems;
            }
        }

        /// <summary>
        /// Handles collection of toolbar items changes
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">The event data</param>
        private void OnToolbarItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        if (TabControl != null) TabControl.ToolBarItems.Insert(e.NewStartingIndex + i, (UIElement)e.NewItems[i]);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                    {
                        if (TabControl != null) TabControl.ToolBarItems.Remove(item as UIElement);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (object item in e.OldItems)
                    {
                        if (TabControl != null) TabControl.ToolBarItems.Remove(item as UIElement);
                    }
                    foreach (object item in e.NewItems)
                    {
                        if (TabControl != null) TabControl.ToolBarItems.Add(item as UIElement);
                    }
                    break;
            }

        }

        /// <summary>
        /// Gets quick access toolbar associated with the ribbon
        /// </summary>
        internal QuickAccessToolBar QuickAccessToolBar
        {
            get { return quickAccessToolBar; }
        }

        /// <summary>
        /// Gets an enumerator for logical child elements of this element.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                ArrayList list = new ArrayList();
                if (layoutRoot != null) list.Add(layoutRoot);
                if (Menu != null) list.Add(Menu);
                if (quickAccessToolBar != null) list.Add(quickAccessToolBar);
                if ((TabControl != null) && (TabControl.ToolbarPanel != null)) list.Add(TabControl.ToolbarPanel);
                return list.GetEnumerator();
            }
        }

        /// <summary>
        /// Gets collection of quick access menu items
        /// </summary>
        public ObservableCollection<QuickAccessMenuItem> QuickAccessItems
        {
            get
            {
                if (quickAccessItems == null)
                {
                    quickAccessItems = new ObservableCollection<QuickAccessMenuItem>();
                    quickAccessItems.CollectionChanged += OnQuickAccessItemsCollectionChanged;
                }
                return quickAccessItems;
            }
        }
        /// <summary>
        /// Handles collection of quick access menu items changes
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">The event data</param>
        void OnQuickAccessItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        QuickAccessMenuItem menuItem = (QuickAccessMenuItem)e.NewItems[i];
                        if (quickAccessToolBar != null) quickAccessToolBar.QuickAccessItems.Insert(e.NewStartingIndex + i, menuItem);
                        menuItem.Ribbon = this;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                    {
                        QuickAccessMenuItem menuItem = (QuickAccessMenuItem)item;
                        if (quickAccessToolBar != null) quickAccessToolBar.QuickAccessItems.Remove(menuItem);
                        menuItem.Ribbon = null;
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (object item in e.OldItems)
                    {
                        QuickAccessMenuItem menuItem = (QuickAccessMenuItem)item;
                        if (quickAccessToolBar != null) quickAccessToolBar.QuickAccessItems.Remove(menuItem);
                        menuItem.Ribbon = null;
                    }
                    foreach (object item in e.NewItems)
                    {
                        QuickAccessMenuItem menuItem = (QuickAccessMenuItem)item;
                        if (quickAccessToolBar != null) quickAccessToolBar.QuickAccessItems.Add(menuItem);
                        menuItem.Ribbon = this;
                    }
                    break;
            }
        }



        /* /// <summary>
         /// Handles collection of backstage items changes
         /// </summary>
         /// <param name="sender">Sender</param>
         /// <param name="e">Th event data</param>
         private void OnBackstageItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
         {
             switch (e.Action)
             {
                 case NotifyCollectionChangedAction.Add:
                     foreach (object item in e.NewItems)
                     {
                         if (backstageButton != null) backstageButton.Backstage.Items.Add(item);
                     }
                     break;

                 case NotifyCollectionChangedAction.Remove:
                     foreach (object item in e.OldItems)
                     {
                         if (backstageButton != null) backstageButton.Backstage.Items.Remove(item);
                     }
                     break;

                 case NotifyCollectionChangedAction.Replace:
                     foreach (object item in e.OldItems)
                     {
                         if (backstageButton != null) backstageButton.Backstage.Items.Remove(item);
                     }
                     foreach (object item in e.NewItems)
                     {
                         if (backstageButton != null) backstageButton.Backstage.Items.Add(item);
                     }
                     break;
             }
         }*/


        /// <summary>
        /// Gets or set whether Customize Quick Access Toolbar menu item is shown
        /// </summary>
        public bool CanCustomizeQuickAccessToolBar
        {
            get { return (bool)GetValue(CanCustomizeQuickAccessToolBarProperty); }
            set { SetValue(CanCustomizeQuickAccessToolBarProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for CanCustomizeQuickAccessToolBar.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CanCustomizeQuickAccessToolBarProperty =
            DependencyProperty.Register("CanCustomizeQuickAccessToolBar", typeof(bool),
            typeof(Ribbon), new UIPropertyMetadata(false));


        /// <summary>
        /// Gets or set whether Customize Ribbon menu item is shown
        /// </summary>
        public bool CanCustomizeRibbon
        {
            get { return (bool)GetValue(CanCustomizeRibbonProperty); }
            set { SetValue(CanCustomizeRibbonProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for CanCustomizeRibbon. 
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CanCustomizeRibbonProperty =
            DependencyProperty.Register("CanCustomizeRibbon", typeof(bool),
            typeof(Ribbon), new UIPropertyMetadata(false));


        /// <summary>
        /// Gets or sets whether ribbon is minimized
        /// </summary>
        public bool IsMinimized
        {
            get { return (bool)GetValue(IsMinimizedProperty); }
            set { SetValue(IsMinimizedProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for IsMinimized. 
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IsMinimizedProperty =
            DependencyProperty.Register("IsMinimized", typeof(bool),
            typeof(Ribbon), new UIPropertyMetadata(false, OnIsMinimizedChanged));

        private static void OnIsMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ribbon = (Ribbon)d;

            if (ribbon.IsMinimizedChanged != null)
            {
                ribbon.IsMinimizedChanged(ribbon, e);
            }
        }

        /// <summary>
        /// Gets or sets the height of the gap between the ribbon and the content
        /// </summary>
        public double ContentGapHeight
        {
            get { return (double)GetValue(ContentGapHeightProperty); }
            set { SetValue(ContentGapHeightProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for <see cref="ContentGapHeight"/>
        /// </summary>
        public static readonly DependencyProperty ContentGapHeightProperty =
            DependencyProperty.Register("ContentGapHeight", typeof(double), typeof(Ribbon), new UIPropertyMetadata(5D));

        // todo check if IsCollapsed and IsAutomaticCollapseEnabled should be reduced to one shared property for RibbonWindow and Ribbon
        /// <summary>
        /// Gets whether ribbon is collapsed
        /// </summary>
        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for <see cref="IsCollapsed"/>
        /// </summary>
        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register("IsCollapsed", typeof(bool),
            typeof(Ribbon), new FrameworkPropertyMetadata(false, OnIsCollapsedChanged));

        private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ribbon = (Ribbon)d;
            if (ribbon.IsCollapsedChanged != null)
            {
                ribbon.IsCollapsedChanged(ribbon, e);
            }
        }

        /// <summary>
        /// Defines if the Ribbon should automatically set <see cref="IsCollapsed"/> when the width or height of the owner window drop under <see cref="MinimalVisibleWidth"/> or <see cref="MinimalVisibleHeight"/>
        /// </summary>
        public bool IsAutomaticCollapseEnabled
        {
            get { return (bool)GetValue(IsAutomaticCollapseEnabledProperty); }
            set { SetValue(IsAutomaticCollapseEnabledProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for IsCollapsed.  
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IsAutomaticCollapseEnabledProperty =
            DependencyProperty.Register("IsAutomaticCollapseEnabled", typeof(bool), typeof(Ribbon), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets whether QAT is visible
        /// </summary>
        public bool IsQuickAccessToolBarVisible
        {
            get { return (bool)GetValue(IsQuickAccessToolBarVisibleProperty); }
            set { SetValue(IsQuickAccessToolBarVisibleProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for IsQuickAccessToolBarVisible.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IsQuickAccessToolBarVisibleProperty =
            DependencyProperty.Register("IsQuickAccessToolBarVisible", typeof(bool), typeof(Ribbon), new UIPropertyMetadata(true));


        /// <summary>
        /// Gets or sets whether user can change location of QAT
        /// </summary>
        public bool CanQuickAccessLocationChanging
        {
            get { return (bool)GetValue(CanQuickAccessLocationChangingProperty); }
            set { SetValue(CanQuickAccessLocationChangingProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for CanQuickAccessLocationChanging.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CanQuickAccessLocationChangingProperty =
            DependencyProperty.Register("CanQuickAccessLocationChanging", typeof(bool), typeof(Ribbon), new UIPropertyMetadata(true));

        #endregion

        #region Commands

        /// <summary>
        /// Gets add to quick access toolbar command
        /// </summary>
        public static RoutedCommand AddToQuickAccessCommand = new RoutedCommand("AddToQuickAccessCommand", typeof(Ribbon));

        /// <summary>
        /// Gets remove from quick access command
        /// </summary>
        public static RoutedCommand RemoveFromQuickAccessCommand = new RoutedCommand("RemoveFromQuickAccessCommand", typeof(Ribbon));

        /// <summary>
        /// Gets show quick access above command
        /// </summary>
        public static RoutedCommand ShowQuickAccessAboveCommand = new RoutedCommand("ShowQuickAccessAboveCommand", typeof(Ribbon));

        /// <summary>
        /// Gets show quick access below command
        /// </summary>
        public static RoutedCommand ShowQuickAccessBelowCommand = new RoutedCommand("ShowQuickAccessBelowCommand", typeof(Ribbon));

        /// <summary>
        /// Gets toggle ribbon minimize command
        /// </summary>
        public static RoutedCommand ToggleMinimizeTheRibbonCommand = new RoutedCommand("ToggleMinimizeTheRibbonCommand", typeof(Ribbon));

        /// <summary>
        /// Gets customize quick access toolbar command
        /// </summary>
        public static RoutedCommand CustomizeQuickAccessToolbarCommand = new RoutedCommand("CustomizeQuickAccessToolbarCommand", typeof(Ribbon));

        /// <summary>
        /// Gets customize the ribbon command
        /// </summary>
        public static RoutedCommand CustomizeTheRibbonCommand = new RoutedCommand("CustomizeTheRibbonCommand", typeof(Ribbon));

        // Occurs when customize toggle minimize command can execute handles
        private static void OnToggleMinimizeTheRibbonCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var ribbon = sender as Ribbon;

            if (ribbon != null
                && ribbon.TabControl != null)
            {
                e.CanExecute = true;
            }
        }

        // Occurs when toggle minimize command executed
        private static void OnToggleMinimizeTheRibbonCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var ribbon = sender as Ribbon;
            if (ribbon != null
                && ribbon.TabControl != null)
            {
                ribbon.TabControl.IsMinimized = !ribbon.TabControl.IsMinimized;
            }
        }

        // Occurs when show quick access below command executed
        private static void OnShowQuickAccessBelowCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Ribbon ribbon = sender as Ribbon;
            ribbon.ShowQuickAccessToolBarAboveRibbon = false;
        }

        // Occurs when show quick access above command executed
        private static void OnShowQuickAccessAboveCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Ribbon ribbon = sender as Ribbon;
            ribbon.ShowQuickAccessToolBarAboveRibbon = true;
        }

        // Occurs when remove from quick access command executed
        private static void OnRemoveFromQuickAccessCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Ribbon ribbon = sender as Ribbon;
            if (ribbon.quickAccessToolBar != null)
            {
                UIElement element = ribbon.quickAccessElements.First(x => x.Value == e.Parameter).Key;
                ribbon.RemoveFromQuickAccessToolBar(element);
            }
        }

        // Occurs when add to quick access command executed
        private static void OnAddToQuickAccessCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Ribbon ribbon = sender as Ribbon;
            if (ribbon.quickAccessToolBar != null)
            {
                ribbon.AddToQuickAccessToolBar(e.Parameter as UIElement);
            }
        }

        // Occurs when customize quick access command executed
        private static void OnCustomizeQuickAccessToolbarCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Ribbon ribbon = sender as Ribbon;
            if (ribbon.CustomizeQuickAccessToolbar != null) ribbon.CustomizeQuickAccessToolbar(sender, EventArgs.Empty);
        }

        // Occurs when customize the ribbon command executed
        private static void OnCustomizeTheRibbonCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Ribbon ribbon = sender as Ribbon;
            if (ribbon.CustomizeTheRibbon != null) ribbon.CustomizeTheRibbon(sender, EventArgs.Empty);
        }

        // Occurs when customize quick access command can execute handles
        private static void OnCustomizeQuickAccessToolbarCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as Ribbon).CanCustomizeQuickAccessToolBar;
        }

        // Occurs when customize the ribbon command can execute handles
        private static void OnCustomizeTheRibbonCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (sender as Ribbon).CanCustomizeRibbon;
        }

        // Occurs when remove from quick access command can execute handles
        private static void OnRemoveFromQuickAccessCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            Ribbon ribbon = sender as Ribbon;
            if (ribbon.IsQuickAccessToolBarVisible)
            {
                e.CanExecute = ribbon.quickAccessElements.ContainsValue(e.Parameter as UIElement);
            }
            else e.CanExecute = false;
        }

        // Occurs when add to quick access command can execute handles
        private static void OnAddToQuickAccessCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            Ribbon ribbon = sender as Ribbon;
            if (ribbon.IsQuickAccessToolBarVisible)
            {
                if (e.Parameter is Gallery) e.CanExecute = !ribbon.IsInQuickAccessToolBar(FindParentRibbonControl(e.Parameter as DependencyObject) as UIElement);
                else e.CanExecute = !ribbon.IsInQuickAccessToolBar(e.Parameter as UIElement);
                Debug.WriteLine("Add to QAT - " + e.Parameter);
            }
            else e.CanExecute = false;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810")]
        static Ribbon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Ribbon), new FrameworkPropertyMetadata(typeof(Ribbon)));

            // Subscribe to menu commands
            CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(Ribbon.AddToQuickAccessCommand, OnAddToQuickAccessCommandExecuted, OnAddToQuickAccessCommandCanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(Ribbon.RemoveFromQuickAccessCommand, OnRemoveFromQuickAccessCommandExecuted, OnRemoveFromQuickAccessCommandCanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(Ribbon.ShowQuickAccessAboveCommand, OnShowQuickAccessAboveCommandExecuted));
            CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(Ribbon.ShowQuickAccessBelowCommand, OnShowQuickAccessBelowCommandExecuted));
            CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(Ribbon.ToggleMinimizeTheRibbonCommand, OnToggleMinimizeTheRibbonCommandExecuted, OnToggleMinimizeTheRibbonCommandCanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(Ribbon.CustomizeTheRibbonCommand, OnCustomizeTheRibbonCommandExecuted, OnCustomizeTheRibbonCommandCanExecute));
            CommandManager.RegisterClassCommandBinding(typeof(Ribbon), new CommandBinding(Ribbon.CustomizeQuickAccessToolbarCommand, OnCustomizeQuickAccessToolbarCommandExecuted, OnCustomizeQuickAccessToolbarCommandCanExecute));

            InitRibbonContextMenu();
            StyleProperty.OverrideMetadata(typeof(Ribbon), new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceStyle)));
        }

        // Coerce object style
        static object OnCoerceStyle(DependencyObject d, object basevalue)
        {
            if (basevalue == null)
            {
                basevalue = (d as FrameworkElement).TryFindResource(typeof(Ribbon));
            }

            return basevalue;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Ribbon()
        {
            VerticalAlignment = VerticalAlignment.Top;
            KeyboardNavigation.SetDirectionalNavigation(this, KeyboardNavigationMode.Contained);
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
            keyTipService = new KeyTipService(this);
        }

        #endregion

        #region Overrides

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.MaintainIsCollapsed();
        }

        private void MaintainIsCollapsed()
        {
            if (this.IsAutomaticCollapseEnabled == false
                || this.ownerWindow == null)
            {
                return;
            }

            if (this.ownerWindow.ActualWidth < MinimalVisibleWidth
                || this.ownerWindow.ActualHeight < MinimalVisibleHeight)
            {
                this.IsCollapsed = true;
            }
            else
            {
                this.IsCollapsed = false;
            }
        }

        /// <summary>
        /// Invoked whenever an unhandled System.Windows.UIElement.GotFocus 
        /// event reaches this element in its route.
        /// </summary>
        /// <param name="e">The System.Windows.RoutedEventArgs that contains the event data.</param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (TabControl != null)
            {
                RibbonTabItem ribbonTabItem = (RibbonTabItem)TabControl.SelectedItem;
                if (ribbonTabItem != null) ribbonTabItem.Focus();
            }
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or 
        /// internal processes call System.Windows.FrameworkElement.ApplyTemplate().
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        public override void OnApplyTemplate()
        {
            if (layoutRoot != null) RemoveLogicalChild(layoutRoot);

            layoutRoot = GetTemplateChild("PART_LayoutRoot") as Panel;

            if (layoutRoot != null) AddLogicalChild(layoutRoot);

            if ((this.TitleBar != null) && (groups != null))
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    this.TitleBar.Items.Remove(groups[i]);
                }
            }
            this.TitleBar = GetTemplateChild("PART_RibbonTitleBar") as RibbonTitleBar;
            if ((this.TitleBar != null) && (groups != null))
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    this.TitleBar.Items.Add(groups[i]);
                }
            }
            RibbonTabItem selectedTab = SelectedTabItem;
            if (TabControl != null)
            {
                TabControl.SelectionChanged -= OnTabControlSelectionChanged;
                selectedTab = TabControl.SelectedItem as RibbonTabItem;
            }
            if ((TabControl != null) && (tabs != null))
            {
                for (int i = 0; i < tabs.Count; i++)
                {
                    TabControl.Items.Remove(tabs[i]);
                }
            }

            if ((TabControl != null) && (toolBarItems != null))
            {
                for (int i = 0; i < toolBarItems.Count; i++)
                {
                    TabControl.ToolBarItems.Remove(toolBarItems[i]);
                }
            }

            TabControl = GetTemplateChild("PART_RibbonTabControl") as RibbonTabControl;

            if (TabControl != null)
            {
                TabControl.SelectionChanged += OnTabControlSelectionChanged;

                TabControl.IsMinimized = this.IsMinimized;
                TabControl.ContentGapHeight = this.ContentGapHeight;

                TabControl.SetBinding(RibbonTabControl.IsMinimizedProperty, new Binding("IsMinimized") { Source = this, Mode = BindingMode.TwoWay });
                TabControl.SetBinding(RibbonTabControl.ContentGapHeightProperty, new Binding("ContentGapHeight") { Source = this, Mode = BindingMode.OneWay });
            }

            if ((TabControl != null) && (tabs != null))
            {
                for (int i = 0; i < tabs.Count; i++)
                {
                    TabControl.Items.Add(tabs[i]);
                }
                TabControl.SelectedItem = selectedTab;
                if (TabControl.SelectedItem == null)
                {
                    //bool isBacstageOpen = IsBackstageOpen;
                    //tabControl.SelectedIndex = selectedTabIndex >= 0 ? selectedTabIndex : 0;
                    //IsBackstageOpen = isBacstageOpen;
                }
            }

            if ((TabControl != null) && (toolBarItems != null))
            {
                for (int i = 0; i < toolBarItems.Count; i++)
                {
                    TabControl.ToolBarItems.Add(toolBarItems[i]);
                }
            }


            if (quickAccessToolBar != null)
            {
                this.quickAccessStream = new MemoryStream();

                if (!this.AutomaticStateManagement
                    || this.IsStateLoaded)
                {
                    this.SaveState(this.quickAccessStream);
                }

                this.ClearQuickAccessToolBar();
            }

            if (quickAccessToolBar != null)
            {
                quickAccessToolBar.ItemsChanged -= OnQuickAccessItemsChanged;
                if (quickAccessItems != null)
                {
                    for (int i = 0; i < quickAccessItems.Count; i++)
                    {
                        quickAccessToolBar.QuickAccessItems.Remove(quickAccessItems[i]);
                    }
                }
            }

            quickAccessToolBar = GetTemplateChild("PART_QuickAccessToolBar") as QuickAccessToolBar;
            if (quickAccessToolBar != null)
            {
                if (quickAccessItems != null)
                {
                    for (int i = 0; i < quickAccessItems.Count; i++)
                    {
                        quickAccessToolBar.QuickAccessItems.Add(quickAccessItems[i]);
                    }
                }
                quickAccessToolBar.ItemsChanged += OnQuickAccessItemsChanged;

                Binding binding = new Binding("CanQuickAccessLocationChanging");
                binding.Source = this;
                binding.Mode = BindingMode.OneWay;
                quickAccessToolBar.SetBinding(Fluent.QuickAccessToolBar.CanQuickAccessLocationChangingProperty, binding);

                //quickAccessToolBar.SizeChanged += OnQATSizeChanged;
            }

            if (quickAccessToolBar != null)
            {
                if (quickAccessToolBar.Parent == null) AddLogicalChild(quickAccessToolBar);
                quickAccessToolBar.Loaded += OnFirstToolbarLoaded;
            }

            /*if (backstageButton != null)
            {
                if (backstageItems != null)
                {
                    for (int i = 0; i < backstageItems.Count; i++)
                    {
                        //backstageButton.Backstage.Items.Remove(backstageItems[i]);
                    }
                }
            }
            backstageButton = GetTemplateChild("PART_BackstageButton") as Backstage;
            adorner = null;
            if (backstageButton != null)
            {
                Binding binding = new Binding("IsBackstageOpen");
                binding.Mode = BindingMode.TwoWay;
                binding.Source = this;
                backstageButton.SetBinding(Backstage.IsOpenProperty, binding);
                if (backstageItems != null)
                {
                    for (int i = 0; i < backstageItems.Count; i++)
                    {
                        //backstageButton.Backstage.Items.Add(backstageItems[i]);
                    }
                }
            }*/
        }

        /// <summary>
        /// Called when the <see cref="ownerWindow"/> is closed, so that we set it to null and clear the <see cref="TitleProperty"/>
        /// </summary>
        private void OnOwnerWindowClosed(object sender, EventArgs e)
        {
            this.DetachFromWindow();
        }

        private void AttachToWindow()
        {
            this.DetachFromWindow();

            this.ownerWindow = Window.GetWindow(this);

            if (this.ownerWindow != null)
            {
                this.ownerWindow.Closed += this.OnOwnerWindowClosed;
                this.ownerWindow.SizeChanged += this.OnSizeChanged;
                this.ownerWindow.KeyDown += this.OnKeyDown;

                if (this.ownerWindow.Owner != null)
                {
                    var binding = new Binding("Title")
                    {
                        Mode = BindingMode.OneWay,
                        Source = this.ownerWindow
                    };
                    this.SetBinding(TitleProperty, binding);
                }
            }
        }

        private void DetachFromWindow()
        {
            if (this.ownerWindow != null)
            {
                this.SaveState();

                this.ownerWindow.Closed -= this.OnOwnerWindowClosed;
                this.ownerWindow.SizeChanged -= this.OnSizeChanged;
                this.ownerWindow.KeyDown -= this.OnKeyDown;

                BindingOperations.ClearBinding(this, TitleProperty);
            }

            this.ownerWindow = null;
        }

        private void OnFirstToolbarLoaded(object sender, RoutedEventArgs e)
        {
            this.quickAccessToolBar.Loaded -= this.OnFirstToolbarLoaded;
            if (this.quickAccessStream != null)
            {
                this.quickAccessStream.Position = 0;
                this.LoadState(this.quickAccessStream);
                this.quickAccessStream.Close();
                this.quickAccessStream = null;
            }
        }

        #endregion

        #region Quick Access Items Managment

        /// <summary>
        /// Determines whether the given element is in quick access toolbar
        /// </summary>
        /// <param name="element">Element</param>
        /// <returns>True if element in quick access toolbar</returns>
        public bool IsInQuickAccessToolBar(UIElement element)
        {
            if (element == null) return false;
            return quickAccessElements.ContainsKey(element);
        }

        /// <summary>
        /// Adds the given element to quick access toolbar
        /// </summary>
        /// <param name="element">Element</param>
        public void AddToQuickAccessToolBar(UIElement element)
        {
            if (element is Gallery) element = FindParentRibbonControl(element) as UIElement;
            if (!QuickAccessItemsProvider.IsSupported(element)) return;
            if (!IsInQuickAccessToolBar(element))
            {
                UIElement control = QuickAccessItemsProvider.GetQuickAccessItem(element);

                quickAccessElements.Add(element, control);
                quickAccessToolBar.Items.Add(control);
                quickAccessToolBar.InvalidateMeasure();
            }
        }

        private static IRibbonControl FindParentRibbonControl(DependencyObject element)
        {
            DependencyObject parent = LogicalTreeHelper.GetParent(element);
            while (parent != null)
            {
                IRibbonControl control = parent as IRibbonControl;
                if (control != null) return control;
                parent = LogicalTreeHelper.GetParent(parent);
            }
            return null;
        }

        /// <summary>
        /// Removes the given elements from quick access toolbar
        /// </summary>
        /// <param name="element">Element</param>
        public void RemoveFromQuickAccessToolBar(UIElement element)
        {
            Debug.WriteLine(element);
            if (IsInQuickAccessToolBar(element))
            {
                UIElement quickAccessItem = quickAccessElements[element];
                quickAccessElements.Remove(element);
                quickAccessToolBar.Items.Remove(quickAccessItem);
                quickAccessToolBar.InvalidateMeasure();
            }

        }

        /// <summary>
        /// Clears quick access toolbar
        /// </summary>
        public void ClearQuickAccessToolBar()
        {
            quickAccessElements.Clear();
            if (quickAccessToolBar != null) quickAccessToolBar.Items.Clear();
        }

        #endregion

        #region Event Handling

        // Handles tab control selection changed
        private void OnTabControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.TabControl != null)
            {
                this.SelectedTabItem = this.TabControl.SelectedItem as RibbonTabItem;
                this.SelectedTabIndex = this.TabControl.SelectedIndex;
            }

            if (this.SelectedTabChanged != null)
            {
                this.SelectedTabChanged(this, e);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.keyTipService.Attach();

            this.AttachToWindow();

            this.InitialLoadState();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1
                && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (this.TabControl.HasItems)
                {
                    this.IsMinimized = !this.IsMinimized;
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.SaveState();

            keyTipService.Detach();

            if (this.ownerWindow != null)
            {
                this.ownerWindow.SizeChanged -= this.OnSizeChanged;
                this.ownerWindow.KeyDown -= this.OnKeyDown;
            }
        }

        #endregion

        #region Private methods

        // Handles backstage Esc key keydown
        void OnBackstageEscapeKeyDown(object sender, KeyEventArgs e)
        {
            // if (e.Key == Key.Escape) IsBackstageOpen = false;
        }

        private RibbonTabItem GetFirstVisibleItem()
        {
            return this.Tabs.FirstOrDefault(item => item.Visibility == Visibility.Visible);
        }

        private RibbonTabItem GetLastVisibleItem()
        {
            return this.Tabs.LastOrDefault(item => item.Visibility == Visibility.Visible);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Get adorner layer for element
        /// </summary>
        /// <param name="element">Element</param>
        /// <returns>Adorner layer</returns>
        static AdornerLayer GetAdornerLayer(UIElement element)
        {
            UIElement current = element;
            while (true)
            {
                current = (UIElement)VisualTreeHelper.GetParent(current);
                if (current is AdornerDecorator) return AdornerLayer.GetAdornerLayer((UIElement)VisualTreeHelper.GetChild(current, 0));
            }
        }

        #endregion

        #region Show / Hide Backstage

        /* // We have to collapse WindowsFormsHost while Backstate is open
        Dictionary<FrameworkElement, Visibility> collapsedElements =
            new Dictionary<FrameworkElement, Visibility>();

        // Show backstage
        void ShowBackstage()
        {
            if (!IsLoaded) return;

            AdornerLayer layer = GetAdornerLayer(this);
            if (adorner == null)
            {
                if (DesignerProperties.GetIsInDesignMode(this))
                {
                    // TODO: in design mode it is required to use design time adorner
                    FrameworkElement topLevelElement = (FrameworkElement)VisualTreeHelper.GetParent(this);
                    double topOffset = backstageButton.TranslatePoint(new Point(0, backstageButton.ActualHeight), topLevelElement).Y;
                    //adorner = new BackstageAdorner(topLevelElement, backstageButton.Backstage, topOffset);
                }
                else
                {
                    FrameworkElement topLevelElement = (FrameworkElement)Window.GetWindow(this).Content;
                    double topOffset = backstageButton.TranslatePoint(new Point(0, backstageButton.ActualHeight), topLevelElement).Y;
                    //adorner = new BackstageAdorner(topLevelElement, backstageButton.Backstage, topOffset);
                }
            }
            layer.Add(adorner);
            if (tabControl != null)
            {
                savedTabItem = tabControl.SelectedItem as RibbonTabItem;
                if (savedTabItem == null && tabControl.Items.Count > 0)
                    savedTabItem = (RibbonTabItem)tabControl.Items[0];
                tabControl.SelectedItem = null;
            }
            if (quickAccessToolBar != null) quickAccessToolBar.IsEnabled = false;
            if (titleBar != null) titleBar.IsEnabled = false;

            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewKeyDown += OnBackstageEscapeKeyDown;
                savedMinWidth = window.MinWidth;
                savedMinHeight = window.MinHeight;

                SaveWindowSize(window);

                if (savedMinWidth < 500) window.MinWidth = 500;
                if (savedMinHeight < 400) window.MinHeight = 400;
                window.SizeChanged += OnWindowSizeChanged;

                // We have to collapse WindowsFormsHost while Backstate is open
                CollapseWindowsFormsHosts(window);
            }
        }

        // We have to collapse WindowsFormsHost while Backstate is open
        void CollapseWindowsFormsHosts(DependencyObject parent)
        {
            FrameworkElement frameworkElement = parent as FrameworkElement;
            if (frameworkElement != null)
            {
                if ((parent is WindowsFormsHost || parent is WebBrowser) &&
                    frameworkElement.Visibility != Visibility.Collapsed)
                {
                    collapsedElements.Add(frameworkElement, frameworkElement.Visibility);
                    frameworkElement.Visibility = Visibility.Collapsed;
                    return;
                }
            }
            // Traverse visual tree
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                CollapseWindowsFormsHosts(VisualTreeHelper.GetChild(parent, i));
            }
        }

        // Hide backstage
        void HideBackstage()
        {
            if (!IsLoaded || adorner == null) return;

            AdornerLayer layer = GetAdornerLayer(this);
            layer.Remove(adorner);
            if (tabControl != null) tabControl.SelectedItem = savedTabItem;
            if (quickAccessToolBar != null) quickAccessToolBar.IsEnabled = true;
            if (titleBar != null) titleBar.IsEnabled = true;

            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewKeyDown -= OnBackstageEscapeKeyDown;
                window.SizeChanged -= OnWindowSizeChanged;

                window.MinWidth = savedMinWidth;
                window.MinHeight = savedMinHeight;
                NativeMethods.SetWindowPos((new WindowInteropHelper(window)).Handle,
                                           new IntPtr(NativeMethods.HWND_NOTOPMOST),
                                           0, 0, savedWidth, savedHeight, NativeMethods.SWP_NOMOVE);
            }

            // Uncollapse elements
            foreach (var element in collapsedElements) element.Key.Visibility = element.Value;
            collapsedElements.Clear();

            if (SelectedTabIndex < 0)
            {
                if ((tabControl != null) && (tabControl.SelectedIndex < 0)) SelectedTabIndex = 0;
                else SelectedTabIndex = tabControl.SelectedIndex;
            }
        }*/

        #endregion

        void SaveWindowSize(Window wnd)
        {
            NativeMethods.WINDOWINFO info = new NativeMethods.WINDOWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            NativeMethods.GetWindowInfo((new WindowInteropHelper(wnd)).Handle, ref info);
            /*savedWidth = info.rcWindow.Right - info.rcWindow.Left;
            savedHeight = info.rcWindow.Bottom - info.rcWindow.Top;*/
        }

        void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Window wnd = Window.GetWindow(this);
            SaveWindowSize(wnd);
        }

        #region State Management

        #region IsolatedStorageFileName

        // Name of the isolated storage file
        string isolatedStorageFileName = null;

        /// <summary>
        /// Gets name of the isolated storage file
        /// </summary>
        string IsolatedStorageFileName
        {
            get
            {
                if (isolatedStorageFileName == null)
                {
                    string stringForHash = "";
                    Window window = Window.GetWindow(this);
                    if (window != null)
                    {
                        stringForHash += "." + window.GetType().FullName;
                        if (!String.IsNullOrEmpty(window.Name) && (window.Name.Trim().Length > 0)) stringForHash += "." + window.Name;
                    }
                    if (!String.IsNullOrEmpty(this.Name) && (this.Name.Trim().Length > 0)) stringForHash += "." + this.Name;

                    isolatedStorageFileName = "Fluent.Ribbon.State.2.0." + stringForHash.GetHashCode().ToString("X");
                }
                return isolatedStorageFileName;
            }
        }

        #endregion

        #region Initial State Loading

        // Initial state loading
        private void InitialLoadState()
        {
            this.LayoutUpdated += this.OnJustLayoutUpdated;
        }

        private void OnJustLayoutUpdated(object sender, EventArgs e)
        {
            this.LayoutUpdated -= this.OnJustLayoutUpdated;

            if (this.QuickAccessToolBar != null
                && !this.QuickAccessToolBar.IsLoaded)
            {
                this.InitialLoadState();
                return;
            }

            if (!this.IsStateLoaded)
            {
                this.LoadState();

                if (this.TabControl != null
                    && this.TabControl.SelectedIndex == -1
                    && !this.TabControl.IsMinimized)
                {
                    this.TabControl.SelectedIndex = 0;
                }
            }
        }

        #endregion

        #region Load / Save to Isolated Storage

        // Handles items changing in QAT
        void OnQuickAccessItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.SaveState();
        }

        // Saves to Isolated Storage (in user store for domain)
        private void SaveState()
        {
            // Check whether automatic save is valid now
            if (!this.AutomaticStateManagement
                || !this.IsStateLoaded)
            {
                return;
            }

            var storage = GetIsolatedStorageFile();
            using (var stream = new IsolatedStorageFileStream(this.IsolatedStorageFileName, FileMode.Create, FileAccess.Write, storage))
            {
                this.SaveState(stream);
            }
        }

        /// <summary>
        /// Loads the State from Isolated Storage (in user store for domain)
        /// </summary>
        public void LoadState()
        {
            if (!this.AutomaticStateManagement)
            {
                return;
            }

            var storage = GetIsolatedStorageFile();
            if (FileExists(storage, IsolatedStorageFileName))
            {
                using (var stream = new IsolatedStorageFileStream(this.IsolatedStorageFileName, FileMode.Open, FileAccess.Read, storage))
                {
                    this.LoadState(stream);
                }
            }

            // Now we can save states
            this.IsStateLoaded = true;
        }

        // Gets a proper isolated storage file
        private static IsolatedStorageFile GetIsolatedStorageFile()
        {
            try
            {
                return IsolatedStorageFile.GetUserStoreForDomain();
            }
            catch
            {
                return IsolatedStorageFile.GetUserStoreForAssembly();
            }
        }

        /// <summary>
        /// Resets automatically saved state
        /// </summary>
        public static void ResetState()
        {
            var storage = GetIsolatedStorageFile();
            foreach (var filename in storage.GetFileNames("*Fluent.Ribbon.State*"))
            {
                storage.DeleteFile(filename);
            }
        }

        // Determinates whether the given file exists in the given storage
        private static bool FileExists(IsolatedStorageFile storage, string fileName)
        {
            var files = storage.GetFileNames(fileName);
            return files.Length != 0;
        }

        #endregion

        #region Save to Stream

        /// <summary>
        /// Saves state to the given stream
        /// </summary>
        /// <param name="stream">Stream</param>
        public void SaveState(Stream stream)
        {
            var builder = new StringBuilder();

            var isMinimizedSaveState = this.IsMinimized;

            // Save Ribbon State
            builder.Append(isMinimizedSaveState.ToString(CultureInfo.InvariantCulture));
            builder.Append(',');
            builder.Append(ShowQuickAccessToolBarAboveRibbon.ToString(CultureInfo.InvariantCulture));
            builder.Append('|');

            // Save QAT items
            var paths = new Dictionary<FrameworkElement, string>();
            this.TraverseLogicalTree(this, "", paths);

            // Foreach items and see whether path is found for the item
            foreach (var element in quickAccessElements)
            {
                string path;
                var control = element.Key as FrameworkElement;

                if (control != null
                    && paths.TryGetValue(control, out path))
                {
                    builder.Append(path);
                    builder.Append(';');
                }
                else
                {
                    // Item is not found in logical tree, output to debug console
                    var controlName = (control != null && !String.IsNullOrEmpty(control.Name))
                        ? String.Format(CultureInfo.InvariantCulture, " (name of the control is {0})", control.Name)
                        : string.Empty;

                    Debug.WriteLine("Control " + element.Key.GetType().Name + " is not found in logical tree during QAT saving" + controlName);
                }
            }

            var writer = new StreamWriter(stream);
            writer.Write(builder.ToString());

            writer.Flush();
        }

        // Traverse logical tree and find QAT items, remember paths
        private void TraverseLogicalTree(DependencyObject item, string path, IDictionary<FrameworkElement, string> paths)
        {
            // Is this item in QAT
            var uielement = item as FrameworkElement;
            if (uielement != null
                && this.quickAccessElements.ContainsKey(uielement))
            {
                if (!paths.ContainsKey(uielement))
                {
                    paths.Add(uielement, path);
                }
            }

            var children = LogicalTreeHelper.GetChildren(item).Cast<object>().ToArray();
            for (var i = 0; i < children.Length; i++)
            {
                var child = children[i] as DependencyObject;
                if (child == null)
                {
                    continue;
                }

                this.TraverseLogicalTree(child, path + i + ",", paths);
            }
        }

        #endregion

        #region Load from Stream

        /// <summary>
        /// Loads state from the given stream
        /// </summary>
        /// <param name="stream">Stream</param>
        public void LoadState(Stream stream)
        {
            this.suppressAutomaticStateManagement = true;

            var reader = new StreamReader(stream);
            var splitted = reader.ReadToEnd().Split('|');

            if (splitted.Length != 2)
            {
                return;
            }

            // Load Ribbon State
            var ribbonProperties = splitted[0].Split(',');

            var isMinimized = Boolean.Parse(ribbonProperties[0]);

            this.IsMinimized = isMinimized;

            this.ShowQuickAccessToolBarAboveRibbon = Boolean.Parse(ribbonProperties[1]);

            // Load items
            var items = splitted[1].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            if (this.quickAccessToolBar != null)
            {
                this.quickAccessToolBar.Items.Clear();
            }

            this.quickAccessElements.Clear();

            for (var i = 0; i < items.Length; i++)
            {
                this.ParseAndAddToQuickAccessToolBar(items[i]);
            }

            // Sync QAT menu items
            foreach (var menuItem in QuickAccessItems)
            {
                menuItem.IsChecked = this.IsInQuickAccessToolBar(menuItem.Target);
            }

            this.suppressAutomaticStateManagement = false;
        }

        // Loads item and add to QAT
        private void ParseAndAddToQuickAccessToolBar(string data)
        {
            var indices = data.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Int32.Parse(x, CultureInfo.InvariantCulture)).ToArray();

            DependencyObject current = this;
            for (var i = 0; i < indices.Length; i++)
            {
                var children = LogicalTreeHelper.GetChildren(current).OfType<object>().ToArray();
                var indexIsInvalid = children.Length <= indices[i];
                var item = indexIsInvalid
                    ? null
                    : children[indices[i]] as DependencyObject;
                if (item == null)
                {
                    // Path is incorrect
                    Debug.WriteLine("Error while QAT items loading: one of the paths is invalid");
                    return;
                }
                current = item;
            }

            var result = current as UIElement;
            if ((result == null)
                || (!QuickAccessItemsProvider.IsSupported(result)))
            {
                // Item is invalid
                Debug.WriteLine("Error while QAT items loading: an item is not be able to be added to QAT");
                return;
            }

            this.AddToQuickAccessToolBar(result);
        }

        #endregion

        #region IsStateLoaded

        /// <summary>
        /// Gets or sets whether state is loaded
        /// </summary>
        bool IsStateLoaded
        {
            get;
            set;
        }

        #endregion

        #region AutomaticStateManagement Property

        // To temporary suppress automatic management
        bool suppressAutomaticStateManagement;

        /// <summary>
        /// Gets or sets whether Quick Access ToolBar can 
        /// save and load its state automatically
        /// </summary>
        public bool AutomaticStateManagement
        {
            get { return (bool)GetValue(AutomaticStateManagementProperty); }
            set { SetValue(AutomaticStateManagementProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for AutomaticStateManagement. 
        /// This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty AutomaticStateManagementProperty =
            DependencyProperty.Register("AutomaticStateManagement", typeof(bool), typeof(Ribbon), new UIPropertyMetadata(true, OnAutoStateManagement, CoerceAutoStateManagement));

        private static object CoerceAutoStateManagement(DependencyObject d, object basevalue)
        {
            Ribbon ribbon = (Ribbon)d;
            if (ribbon.suppressAutomaticStateManagement) return false;
            return basevalue;
        }

        static void OnAutoStateManagement(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Ribbon ribbon = (Ribbon)d;
            if ((bool)e.NewValue) ribbon.InitialLoadState();
        }

        #endregion

        #endregion
    }
}
