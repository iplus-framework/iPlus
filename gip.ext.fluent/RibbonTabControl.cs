﻿#region Copyright and License Information
// This is a modification for iplus-framework of the Fluent Ribbon Control Suite
// https://github.com/fluentribbon/Fluent.Ribbon
// Copyright © Degtyarev Daniel, Rikker Serg. 2009-2010.  All rights reserved.
// 
// This code was originally distributed under the Microsoft Public License (Ms-PL). The modifications by gipSoft d.o.o. are now distributed under GPLv3.
// The license is available online https://github.com/fluentribbon/Fluent.Ribbonlicense
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Fluent
{
    /// <summary>
    /// Represents ribbon tab control
    /// </summary>
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(RibbonTabItem))]
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_TabsContainer", Type = typeof(IScrollInfo))]
    [TemplatePart(Name = "PART_ToolbarPanel", Type = typeof(Panel))]
    public class RibbonTabControl : Selector, IDropDownControl
    {
        #region Fields

        // Collection of toolbar items
        private ObservableCollection<UIElement> toolBarItems;

        // ToolBar panel
        private Panel toolbarPanel;

        #endregion

        #region Events

        /// <summary>
        /// Event which is fired when the, maybe listening, <see cref="Backstage"/> should be closed
        /// </summary>
        public event EventHandler RequestBackstageClose;

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
            DependencyProperty.Register("Menu", typeof(UIElement),
            typeof(RibbonTabControl), new UIPropertyMetadata(null));

        #endregion

        /// <summary>
        /// Gets drop down popup
        /// </summary>
        public Popup DropDownPopup { get; private set; }

        /// <summary>
        /// Gets a value indicating whether context menu is opened
        /// </summary>
        public bool IsContextMenuOpened { get; set; }

        /// <summary>
        /// Gets content of selected tab item
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedContent
        {
            get
            {
                return this.GetValue(SelectedContentProperty);
            }
            internal set
            {
                this.SetValue(SelectedContentPropertyKey, value);
            }
        }

        // DependencyProperty key for SelectedContent
        static readonly DependencyPropertyKey SelectedContentPropertyKey = DependencyProperty.RegisterReadOnly("SelectedContent", typeof(object), typeof(RibbonTabControl), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Using a DependencyProperty as the backing store for <see cref="SelectedContent"/>.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty SelectedContentProperty = SelectedContentPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets whether ribbon is minimized
        /// </summary>
        public bool IsMinimized
        {
            get { return (bool)GetValue(IsMinimizedProperty); }
            set { SetValue(IsMinimizedProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for <see cref="IsMinimized"/>.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IsMinimizedProperty = DependencyProperty.Register("IsMinimized", typeof(bool), typeof(RibbonTabControl), new UIPropertyMetadata(false, OnMinimizedChanged));

        /// <summary>
        /// Gets or sets whether ribbon popup is opened
        /// </summary>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for <see cref="IsDropDownOpen"/>.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(RibbonTabControl), new UIPropertyMetadata(false, OnIsDropDownOpenChanged, CoerceIsDropDownOpen));

        private static object CoerceIsDropDownOpen(DependencyObject d, object basevalue)
        {
            var tabControl = d as RibbonTabControl;

            if (tabControl == null)
            {
                return basevalue;
            }

            if (!tabControl.IsMinimized)
            {
                return false;
            }

            return basevalue;
        }

        /// <summary>
        /// Defines if the currently selected item should draw it's highlight/selected borders
        /// </summary>
        public bool HighlightSelectedItem
        {
            get { return (bool)GetValue(HighlightSelectedItemProperty); }
            set { SetValue(HighlightSelectedItemProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for <see cref="HighlightSelectedItem"/>.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty HighlightSelectedItemProperty =
            DependencyProperty.RegisterAttached("HighlightSelectedItem", typeof(bool), typeof(RibbonTabControl), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Gets whether ribbon tabs can scroll
        /// </summary>
        internal bool CanScroll
        {
            get
            {
                var scrollInfo = GetTemplateChild("PART_TabsContainer") as IScrollInfo;
                if (scrollInfo != null)
                {
                    return scrollInfo.ExtentWidth > scrollInfo.ViewportWidth;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets or sets selected tab item
        /// </summary>
        internal RibbonTabItem SelectedTabItem
        {
            get { return (RibbonTabItem)GetValue(SelectedTabItemProperty); }
            private set { SetValue(SelectedTabItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedTabItem.  This enables animation, styling, binding, etc...
        internal static readonly DependencyProperty SelectedTabItemProperty =
            DependencyProperty.Register("SelectedTabItem", typeof(RibbonTabItem), typeof(RibbonTabControl), new UIPropertyMetadata(null));

        /// <summary>
        /// Gets collection of ribbon toolbar items
        /// </summary>
        public ObservableCollection<UIElement> ToolBarItems
        {
            get
            {
                if (this.toolBarItems == null)
                {
                    this.toolBarItems = new ObservableCollection<UIElement>();
                    this.toolBarItems.CollectionChanged += this.OnToolbarItemsCollectionChanged;
                }

                return this.toolBarItems;
            }
        }

        internal Panel ToolbarPanel
        {
            get { return toolbarPanel; }
        }

        // Handle toolbar iitems changes
        private void OnToolbarItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.ToolbarPanel == null)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (var i = 0; i < e.NewItems.Count; i++)
                    {
                        this.ToolbarPanel.Children.Insert(e.NewStartingIndex + i, (UIElement)e.NewItems[i]);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var obj3 in e.OldItems.OfType<UIElement>())
                    {
                        ToolbarPanel.Children.Remove(obj3);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (var obj4 in e.OldItems.OfType<UIElement>())
                    {
                        this.ToolbarPanel.Children.Remove(obj4);
                    }
                    foreach (var obj5 in e.NewItems.OfType<UIElement>())
                    {
                        this.ToolbarPanel.Children.Add(obj5);
                    }
                    break;
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
            DependencyProperty.Register("ContentGapHeight", typeof(double), typeof(RibbonTabControl), new UIPropertyMetadata(5D));

        #endregion

        #region Initializion

        /// <summary>
        /// Static constructor
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810")]
        static RibbonTabControl()
        {
            var type = typeof(RibbonTabControl);
            DefaultStyleKeyProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(typeof(RibbonTabControl)));
            ContextMenuService.Attach(type);
            PopupService.Attach(type);
            StyleProperty.OverrideMetadata(type, new FrameworkPropertyMetadata(null, OnCoerceStyle));
        }

        // Coerce object style
        private static object OnCoerceStyle(DependencyObject d, object basevalue)
        {
            if (basevalue == null)
            {
                basevalue = ((FrameworkElement)d).TryFindResource(typeof(RibbonTabControl));
            }

            return basevalue;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public RibbonTabControl()
        {
            ContextMenuService.Coerce(this);

            this.Loaded += this.OnLoaded;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Raises the System.Windows.FrameworkElement.Initialized event. 
        /// This method is invoked whenever System.Windows.
        /// FrameworkElement.IsInitialized is set to true internally.
        /// </summary>
        /// <param name="e">The System.Windows.RoutedEventArgs that contains the event data.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.ItemContainerGenerator.StatusChanged += this.OnGeneratorStatusChanged;
        }

        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>The element that is used to display the given item.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new RibbonTabItem();
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or 
        /// internal processes call System.Windows.FrameworkElement.ApplyTemplate().
        /// </summary>
        public override void OnApplyTemplate()
        {
            this.DropDownPopup = this.Template.FindName("PART_Popup", this) as Popup;
            if (this.DropDownPopup != null)
            {
                /*Binding binding = new Binding("IsOpen");
                binding.Mode = BindingMode.TwoWay;
                binding.Source = this;
                popup.SetBinding(Popup.IsOpenProperty, binding);
                */
                this.DropDownPopup.CustomPopupPlacementCallback = this.CustomPopupPlacementMethod;
            }

            if (this.ToolbarPanel != null
                && this.toolBarItems != null)
            {
                for (var i = 0; i < this.toolBarItems.Count; i++)
                {
                    this.ToolbarPanel.Children.Remove(this.toolBarItems[i]);
                }
            }

            this.toolbarPanel = this.Template.FindName("PART_ToolbarPanel", this) as Panel;

            if (this.ToolbarPanel != null
                && this.toolBarItems != null)
            {
                for (var i = 0; i < this.toolBarItems.Count; i++)
                {
                    this.ToolbarPanel.Children.Add(this.toolBarItems[i]);
                }
            }
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>true if the item is (or is eligible to be) its own container; otherwise, false.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is RibbonTabItem);
        }

        /// <summary>
        /// Updates the current selection when an item in the System.Windows.Controls.Primitives.Selector has changed
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (e.Action == NotifyCollectionChangedAction.Remove
                && this.SelectedIndex == -1)
            {
                var startIndex = e.OldStartingIndex + 1;
                if (startIndex > this.Items.Count)
                {
                    startIndex = 0;
                }

                var item = this.FindNextTabItem(startIndex, -1);
                if (item != null)
                {
                    item.IsSelected = true;
                }
                else
                {
                    this.SelectedContent = null;
                }
            }
        }

        /// <summary>
        /// Called when the selection changes.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            this.UpdateSelectedContent();

            if (e.AddedItems.Count > 0)
            {
                if (this.IsMinimized)
                {
                    this.IsDropDownOpen = true;

                    ((RibbonTabItem)e.AddedItems[0]).IsHitTestVisible = false;
                }
            }
            else
            {
                this.IsDropDownOpen = false;
            }

            if (e.RemovedItems.Count > 0)
            {
                ((RibbonTabItem)e.RemovedItems[0]).IsHitTestVisible = true;
            }

            base.OnSelectionChanged(e);
        }

        /// <summary>
        /// Invoked when an unhandled System.Windows.Input.Mouse.PreviewMouseWheel 
        /// attached event reaches an element in its route that is derived from this class. 
        /// Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The System.Windows.Input.MouseWheelEventArgs that contains the event data.</param>
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            //base.OnPreviewMouseWheel(e);
            ProcessMouseWheel(e);
        }

        #endregion

        #region Private methods

        private static bool IsRibbonAncestorOf(DependencyObject element)
        {
            while (element != null)
            {
                if (element is Ribbon)
                {
                    return true;
                }

                var parent = LogicalTreeHelper.GetParent(element) ?? VisualTreeHelper.GetParent(element);

                element = parent;
            }

            return false;
        }

        // Process mouse wheel event
        internal void ProcessMouseWheel(MouseWheelEventArgs e)
        {
            if (this.IsMinimized
                || this.SelectedItem == null)
            {
                return;
            }

            var focusedElement = Keyboard.FocusedElement as DependencyObject;

            if (focusedElement != null
                && IsRibbonAncestorOf(focusedElement))
            {
                return;
            }

            var visualItems = new List<RibbonTabItem>();
            var selectedIndex = -1;
            for (var i = 0; i < Items.Count; i++)
            {
                if (((RibbonTabItem)this.Items[i]).Visibility == Visibility.Visible)
                {
                    visualItems.Add((Items[i] as RibbonTabItem));

                    if (((RibbonTabItem)this.Items[i]).IsSelected)
                    {
                        selectedIndex = visualItems.Count - 1;
                    }
                }
            }

            if (e.Delta > 0)
            {
                if (selectedIndex > 0)
                {
                    visualItems[selectedIndex].IsSelected = false;
                    selectedIndex--;
                    visualItems[selectedIndex].IsSelected = true;
                }
            }

            if (e.Delta < 0)
            {
                if (selectedIndex < visualItems.Count - 1)
                {
                    visualItems[selectedIndex].IsSelected = false;
                    selectedIndex++;
                    visualItems[selectedIndex].IsSelected = true;
                }
            }

            e.Handled = true;
        }

        // Get selected ribbon tab item
        private RibbonTabItem GetSelectedTabItem()
        {
            var selectedItem = this.SelectedItem;
            if (selectedItem == null)
            {
                return null;
            }

            var item = selectedItem as RibbonTabItem
                ?? this.ItemContainerGenerator.ContainerFromIndex(this.SelectedIndex) as RibbonTabItem;

            return item;
        }

        // Find next tab item
        private RibbonTabItem FindNextTabItem(int startIndex, int direction)
        {
            if (direction != 0)
            {
                var index = startIndex;
                for (var i = 0; i < this.Items.Count; i++)
                {
                    index += direction;

                    if (index >= this.Items.Count)
                    {
                        index = 0;
                    }
                    else if (index < 0)
                    {
                        index = this.Items.Count - 1;
                    }

                    var nextItem = this.ItemContainerGenerator.ContainerFromIndex(index) as RibbonTabItem;
                    if (((nextItem != null) && nextItem.IsEnabled) && (nextItem.Visibility == Visibility.Visible))
                    {
                        return nextItem;
                    }
                }
            }

            return null;
        }

        // Updates selected content
        private void UpdateSelectedContent()
        {
            if (this.SelectedIndex < 0)
            {
                this.SelectedContent = null;
                this.SelectedTabItem = null;
            }
            else
            {
                var selectedTabItem = this.GetSelectedTabItem();
                if (selectedTabItem != null)
                {
                    this.SelectedContent = selectedTabItem.GroupsContainer;
                    this.UpdateLayout();
                    this.SelectedTabItem = selectedTabItem;
                }
            }
        }

        #endregion

        #region Event handling

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        // Handles GeneratorStatus changed
        private void OnGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                this.UpdateSelectedContent();
            }
        }

        // Handles IsMinimized changed
        private static void OnMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tab = (RibbonTabControl)d;

            if (!tab.IsMinimized)
            {
                tab.IsDropDownOpen = false;
            }

            if ((bool)e.NewValue == false
                && tab.SelectedIndex < 0)
            {
                var item = tab.FindNextTabItem(-1, 1);

                if (item != null)
                {
                    item.IsSelected = true;
                }
            }
        }

        // Handles ribbon popup closing
        private void OnRibbonTabPopupClosing()
        {
            var ribbonTabItem = this.SelectedItem as RibbonTabItem;

            if (ribbonTabItem != null)
            {
                ribbonTabItem.IsHitTestVisible = true;
            }

            if (Mouse.Captured == this)
            {
                Mouse.Capture(null);
            }
        }

        // handles ribbon popup opening
        private void OnRibbonTabPopupOpening()
        {
            var ribbonTabItem = this.SelectedItem as RibbonTabItem;

            if (ribbonTabItem != null)
            {
                ribbonTabItem.IsHitTestVisible = false;
            }

            Mouse.Capture(this, CaptureMode.SubTree);
        }

        /// <summary>
        /// Implements custom placement for ribbon popup
        /// </summary>
        /// <param name="popupsize"></param>
        /// <param name="targetsize"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private CustomPopupPlacement[] CustomPopupPlacementMethod(Size popupsize, Size targetsize, Point offset)
        {
            if (this.DropDownPopup != null
                && this.SelectedTabItem != null)
            {
                // Get current workarea                
                var tabItemPos = this.SelectedTabItem.PointToScreen(new Point(0, 0));
                var tabItemRect = new NativeMethods.Rect
                                      {
                                          Left = (int)tabItemPos.X,
                                          Top = (int)tabItemPos.Y,
                                          Right = (int)tabItemPos.X + (int)this.SelectedTabItem.ActualWidth,
                                          Bottom = (int)tabItemPos.Y + (int)this.SelectedTabItem.ActualHeight
                                      };

                const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

                var monitor = NativeMethods.MonitorFromRect(ref tabItemRect, MONITOR_DEFAULTTONEAREST);
                if (monitor != IntPtr.Zero)
                {
                    var monitorInfo = new NativeMethods.MonitorInfo();
                    monitorInfo.Size = Marshal.SizeOf(monitorInfo);
                    NativeMethods.GetMonitorInfo(monitor, monitorInfo);

                    var startPoint = this.PointToScreen(new Point(0, 0));
                    if (this.FlowDirection == FlowDirection.RightToLeft)
                    {
                        startPoint.X -= this.ActualWidth;
                    }

                    var inWindowRibbonWidth = monitorInfo.Work.Right - Math.Max(monitorInfo.Work.Left, startPoint.X);

                    var actualWidth = this.ActualWidth;
                    if (startPoint.X < monitorInfo.Work.Left)
                    {
                        actualWidth -= monitorInfo.Work.Left - startPoint.X;
                        startPoint.X = monitorInfo.Work.Left;
                    }

                    // Set width
                    this.DropDownPopup.Width = Math.Min(actualWidth, inWindowRibbonWidth);
                    return new[]
                               {
                                   new CustomPopupPlacement(new Point(startPoint.X - tabItemPos.X, this.SelectedTabItem.ActualHeight - ((FrameworkElement)this.DropDownPopup.Child).Margin.Top), PopupPrimaryAxis.None),
                                   new CustomPopupPlacement(new Point(startPoint.X - tabItemPos.X, -((ScrollViewer)this.SelectedContent).ActualHeight - ((FrameworkElement)this.DropDownPopup.Child).Margin.Bottom), PopupPrimaryAxis.None)
                               };
                }
            }

            return null;
        }

        // Handles IsDropDownOpen property changed
        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ribbonTabControl = (RibbonTabControl)d;

            ribbonTabControl.RaiseRequestBackstageClose();

            if (ribbonTabControl.IsDropDownOpen)
            {
                ribbonTabControl.OnRibbonTabPopupOpening();
            }
            else
            {
                ribbonTabControl.OnRibbonTabPopupClosing();
            }
        }

        /// <summary>
        /// Raises an event causing the Backstage-View to be closed
        /// </summary>
        public void RaiseRequestBackstageClose()
        {
            var handler = this.RequestBackstageClose;

            if (handler != null)
            {
                handler(this, null);
            }
        }

        #endregion
    }
}