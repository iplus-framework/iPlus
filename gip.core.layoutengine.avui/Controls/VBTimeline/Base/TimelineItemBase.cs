using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using gip.core.datamodel;
using gip.core.layoutengine.avui.ganttchart;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;


namespace gip.core.layoutengine.avui.timeline
{
    public abstract class TimelineItemBase : ContentControl, IACInteractiveObject, IACObject, IACMenuBuilder
    {
        #region Loaded-Event

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            DoubleTapped += TimelineItemBase_DoubleTapped;
            ToolTip.AddToolTipOpeningHandler(this, TimelineItem_ToolTipOpening);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }

        bool _IsInitialized = false;

        ContentControl contentControl;

        public virtual void InitVBControl()
        {
            if (_IsInitialized)
                return;

            Binding binding = new Binding();
            binding.Source = this.Content;
            binding.Path = VBTimelineView.GanttStart.Replace("\\", ".");
            this.Bind(TimelinePanel.StartDateProperty, binding);

            Binding binding2 = new Binding();
            binding2.Source = this.Content;
            binding2.Path = VBTimelineView.GanttEnd.Replace("\\", ".");
            this.Bind(TimelinePanel.EndDateProperty, binding2);

            Binding binding3 = new Binding();
            binding3.Source = this.Content;
            binding3.Path = "DisplayOrder";
            this.Bind(TimelinePanel.RowIndexProperty, binding3);

            _IsInitialized = true;
        }

        public virtual bool DeInitVBControl()
        {
            VBTimelineChart.container.Children.Clear();
            if (contentControl != null)
                contentControl.ClearAllBindings();
            this.ClearAllBindings();
            DoubleTapped -= TimelineItemBase_DoubleTapped;
            ToolTip.RemoveToolTipOpeningHandler(this, TimelineItem_ToolTipOpening);
            VBTreeListViewItemMap = null;
            _IsInitialized = false;
            return true;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            int toolTipChildrenCount = 0;

            if (VBTimelineChart.container.Children.Count == 0 || (VBTimelineChart.container.Children.Count == 3 && VBTimelineChart.container.Children[2] != contentControl) && contentControl != null)
            {
                // TODO Avalonia: Change XAML in iPlus Designs of PeoprtyLogPresenter. ToolTip can be set directliy witohout this wrong Tooltip ContentTemplate-Resource-Approach.
                //VBTimelineChart.container.Children.Clear();
                //if (VBTimelineChart.container.Children.Count == 0)
                //{
                //    if (_ToolTipStatus == null && this.ContentTemplate != null)
                //    {
                //        _ToolTipStatus = this.ContentTemplate.Resources["ToolTipStatus"] as StackPanel;
                //    }

                //    if (_ToolTipStatus != null)
                //    {
                //        VBTimelineChart.container.Children.Add(_ToolTipStatus);
                //        VBTimelineChart.container.Children.Add(new Separator() { Height = 5, Background = Brushes.Transparent });
                //        toolTipChildrenCount = 2;
                //    }
                //}
            }
            else if (contentControl == null)
            {
                Binding bind = new Binding();
                bind.Source = this;
                bind.Path = nameof(ToolTipContent);
                contentControl = new ContentControl();
                contentControl.Bind(ContentControl.ContentProperty, bind);
            }
            if (VBTimelineChart.container.Children.Count == toolTipChildrenCount && contentControl != null)
                VBTimelineChart.container.Children.Add(contentControl);
        }

        internal virtual void TimelineItem_ToolTipOpening(object sender, CancelRoutedEventArgs e)
        {
            // Avalonia Change Designs in iPlus
            //if (!IsToolTipEnabled)
            //{
            //    ToolTip.SetTip(this, null);
            //    return;
            //}

            //if (ToolTipContent != null)
            //{
            //    foreach (TemplatedControl element in ToolTipContent.Children)
            //    {
            //        element.DataContext = DataContext;
            //        //element.OnApplyTemplate();
            //    }
            //    ToolTip.SetTip(this, VBTimelineChart.container);
            //}
        }


        #endregion

        #region Properties

        private VBTimelineViewBase _VBTimelineView;
        public VBTimelineViewBase VBTimelineView
        {
            get
            {
                if (_VBTimelineView == null)
                    _VBTimelineView = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBTimelineViewBase)) as VBTimelineViewBase;
                return _VBTimelineView;
            }
        }

        private VBTimelineChartBase _VBTimelineChart;
        public VBTimelineChartBase VBTimelineChart
        {
            get
            {
                if (_VBTimelineChart == null)
                    _VBTimelineChart = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBTimelineChartBase)) as VBTimelineChartBase;
                return _VBTimelineChart;
            }
        }

        public VBTreeListViewItem VBTreeListViewItemMap;

        private bool _IsToolTipEnabled = true;
        public bool IsToolTipEnabled
        {
            get => _IsToolTipEnabled;
            set => _IsToolTipEnabled = value;
        }

        #endregion

        #region StyledProperties

        /// <summary>
        /// Represents the styled property for IsCollapsed.
        /// </summary>
        public static readonly StyledProperty<bool> IsCollapsedProperty =
            AvaloniaProperty.Register<TimelineItemBase, bool>(nameof(IsCollapsed), true);

        /// <summary>
        /// Gets or sets whether this timeline item is collapsed.
        /// </summary>
        public bool IsCollapsed
        {
            get { return GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for IsDisplayAsZero.
        /// </summary>
        public static readonly StyledProperty<bool> IsDisplayAsZeroProperty =
            AvaloniaProperty.Register<TimelineItemBase, bool>(nameof(IsDisplayAsZero));

        /// <summary>
        /// Get or set indication that this timeline item should be display
        /// as zero length.
        /// This can be either because the item length is really zero or
        /// because its length fall below the zoom factor.
        /// </summary>
        public bool IsDisplayAsZero
        {
            get { return GetValue(IsDisplayAsZeroProperty); }
            set { SetValue(IsDisplayAsZeroProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for IsSelected.
        /// </summary>
        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<TimelineItemBase, bool>(nameof(IsSelected));

        /// <summary>
        /// Gets or sets whether this timeline item is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for ToolTipContent.
        /// </summary>
        public static readonly StyledProperty<StackPanel> ToolTipContentProperty =
            AvaloniaProperty.Register<TimelineItemBase, StackPanel>(nameof(ToolTipContent));

        /// <summary>
        /// Gets or sets the tooltip content.
        /// </summary>
        internal StackPanel ToolTipContent
        {
            get { return GetValue(ToolTipContentProperty); }
            set { SetValue(ToolTipContentProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == IsSelectedProperty && (bool)change.NewValue)
            {
                VBTimelineChart.SelectedItem = DataContext;
                if (_isPointerPressed)
                    ScrollToItemStart();
            }

            base.OnPropertyChanged(change);
        }

        #endregion

        #region Methods

        public virtual void SelectTreeItem()
        {
            if (VBTreeListViewItemMap != null)
                VBTreeListViewItemMap.IsSelected = true;
        }

        private void ScrollToItemStart()
        {
            //VBTimelineChart.ScrollViewer.ScrollToHome();
            if (VBTimelineChart.IsAncestorOf(this))
            {
                var relativePoint = this.TranslatePoint(new Point(0, 0), VBTimelineChart);
                if (relativePoint.HasValue)
                {
                    VBTimelineChart.PART_AxesPanel._IsScrollFromZoom = false;
                    var offset = VBTimelineChart.ScrollViewer.Offset;
                    // Fix: Vector + double is not allowed, so add to X and keep Y
                    var newOffset = new Vector(
                        offset.X + relativePoint.Value.X - VBTimelineChart.Bounds.Width / 2,
                        offset.Y
                    );
                    VBTimelineChart.ScrollViewer.SetCurrentValue(
                        ScrollViewer.OffsetProperty,
                        newOffset
                    );
                }
            }
        }

        private void ScrollToItemEnd()
        {
            //VBTimelineChart.ScrollViewer.ScrollToEnd();
            if (VBTimelineChart.IsAncestorOf(this))
            {
                var relativePoint = this.TranslatePoint(new Point(0, 0), VBTimelineChart);
                if (relativePoint.HasValue)
                {
                    VBTimelineChart.PART_AxesPanel._IsScrollFromZoom = false;
                    var offset = VBTimelineChart.ScrollViewer.Offset;
                    // Fix: Vector + double is not allowed, so add to X and keep Y
                    var newOffset = new Vector(
                        offset.X + relativePoint.Value.X + Bounds.Width - VBTimelineChart.Bounds.Width / 2,
                        offset.Y
                    );
                    VBTimelineChart.ScrollViewer.SetCurrentValue(
                        ScrollViewer.OffsetProperty,
                        newOffset
                    );
                }
            }
        }

        private bool _isPointerPressed = false;
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.Properties.IsLeftButtonPressed)
            {
                _isPointerPressed = true;
            }
            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            _isPointerPressed = false;
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                if (e.KeyModifiers != KeyModifiers.Control && e.KeyModifiers != KeyModifiers.Shift)
                    SelectTreeItem();
            }
            else if (e.InitialPressMouseButton == MouseButton.Right)
            {
                if (e.Route == RoutingStrategies.Tunnel)
                {
                    if (VBTreeListViewItemMap != null)
                        VBTreeListViewItemMap.IsSelected = true;
                    Point point = e.GetPosition(this);
                    ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, point.X, point.Y, Global.ElementActionType.ContextMenu);
                    VBTimelineChart.BSOACComponent.ACAction(actionArgs);
                    VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
                    this.ContextMenu = vbContextMenu;
                    //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
                    if (vbContextMenu.PlacementTarget == null)
                        vbContextMenu.PlacementTarget = this;
                    ContextMenu.Open();
                }
            }
            base.OnPointerReleased(e);
        }

        private void TimelineItemBase_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (e.KeyModifiers != KeyModifiers.Control && e.KeyModifiers != KeyModifiers.Shift && VBTreeListViewItemMap != null)
            {
                VBTreeListViewItemMap.IsSelected = true;
                VBTreeListViewItemMap.IsExpanded = !VBTreeListViewItemMap.IsExpanded;
            }
        }

        public ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            AppendMenu(vbContent, "TimelineItem", ref acMenuItemList);
            return acMenuItemList;
        }

        /// <summary>
        /// Appends the context menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        ///<param name="acMenuItemList">The acMenuItemList parameter.</param>
        public virtual void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            VBLogicalTreeHelper.AppendMenu(this, vbContent, vbControl, ref acMenuItemList);
            this.GetDesignManagerMenu(vbContent, ref acMenuItemList);
        }

        [ACMethodInteraction("", "en{'Go to start'}de{'Zum Start gehen'}", 998, false)]
        public void GoToStart()
        {
            ScrollToItemStart();
        }

        [ACMethodInteraction("", "en{'Go to end'}de{'Zum Ende gehen'}", 999, false)]
        public void GoToEnd()
        {
            ScrollToItemEnd();
        }


        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return VBTimelineChart.VBContent; }
        }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get { return this.DataContext as IACObject; }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            if (actionArgs.ElementAction == Global.ElementActionType.ACCommand)
            {
                var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                if (query.Any())
                {
                    ACCommand acCommand = query.First() as ACCommand;
                    if (!acCommand.ParameterList.Any())
                    {
                        ACUrlCommand(acCommand.GetACUrl());
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return true;
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return this.Name; }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return ACIdentifier; }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return this.ReflectACType(); }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return this.ReflectGetACContentList(); }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return this.ReflectGetACUrl(rootACObject);
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        #endregion
    }
}
