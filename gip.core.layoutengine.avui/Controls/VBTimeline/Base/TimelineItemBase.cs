using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.VisualTree;
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
using Avalonia.Controls.Presenters;


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
            if (!IsToolTipEnabled)
            {
                // Do NOT clear the Tip here: ToolTipOpening fires while IsOpen is
                // already true, and clearing the Tip would trigger ToolTipService
                // to close the popup permanently for this control. Just cancel the
                // opening - Avalonia resets IsOpen back to false.
                e.Cancel = true;
                return;
            }

            // The chart shares a single StackPanel (VBTimelineChart.container) as ToolTip
            // content between all timeline items. The ToolTip of the previously hovered
            // item stays alive (cached in its control's ToolTipProperty) and still hosts
            // the shared panel inside its popup's ContentPresenter. Detach it here -
            // ToolTipOpening fires before the new ToolTip is created - otherwise
            // ContentPresenter.UpdateChild throws "already has a visual parent".
            // IMPORTANT: clear the owning ToolTip's Content, NOT the presenter's.
            // The presenter's Content is template-bound ({TemplateBinding Content});
            // setting a local value on it would break that binding permanently and
            // produce an EMPTY tooltip when the same item is hovered again.
            var container = VBTimelineChart.container;
            DetachSharedContainer(container);

            // WPF-Parity: re-target the shared tooltip content to THIS item. Without
            // this the tooltip keeps showing the data of the first / previously
            // hovered item, because the shared VB-Controls cache their bindings.
            if (ToolTipContent != null)
            {
                foreach (var element in ToolTipContent.Children)
                {
                    element.DataContext = DataContext;
                    if (element is IVBContentReInitializable reInit)
                        reInit.ReInitVBContent();
                }
            }
        }

        /// <summary>
        /// The chart shares a single StackPanel instance as ToolTip content between all
        /// timeline items. If a previous ToolTip is still hosting that panel inside its
        /// ContentPresenter, re-using it for another ToolTip throws
        /// "The control ... already has a visual parent ContentPresenter". Detach it from
        /// any previous host before assigning it as Tip again.
        /// </summary>
        protected static void SetSharedToolTip(Control item, StackPanel sharedContainer)
        {
            if (sharedContainer == null)
                return;

            DetachSharedContainer(sharedContainer);

            ToolTip.SetTip(item, sharedContainer);
        }

        /// <summary>
        /// Detaches the shared tooltip container from a ToolTip that currently hosts it.
        /// Clears the owning ToolTip's Content (the template-bound ContentPresenter
        /// follows automatically) instead of setting the presenter's Content directly,
        /// which would break its TemplateBinding and yield an empty tooltip on reuse.
        /// </summary>
        internal static void DetachSharedContainer(StackPanel sharedContainer)
        {
            if (sharedContainer == null)
                return;

            if (sharedContainer.GetVisualParent() is ContentPresenter oldPresenter)
            {
                var owningToolTip = oldPresenter.TemplatedParent as ToolTip;
                if (owningToolTip != null)
                {
                    if (ReferenceEquals(owningToolTip.Content, sharedContainer))
                        owningToolTip.Content = null;
                }
                else
                {
                    // Fallback: presenter not owned by a ToolTip template
                    oldPresenter.Content = null;
                }
            }
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
            AvaloniaProperty.Register<TimelineItemBase, bool>(nameof(IsCollapsed), false);

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
            if (change.Property == IsSelectedProperty)
            {
                if ((bool)change.NewValue)
                {
                    // Enforce single selection: clicking another item in the chart
                    // must deselect the previously selected one.
                    DeselectOtherItems();
                    VBTimelineChart.SelectedItem = DataContext;
                    if (_isPointerPressed)
                        ScrollToItemStart();
                }
                // WPF-Parity: highlight ALL elements of the selected row with a red
                // border. The consumer templates rely on DataTriggerBehavior/
                // ChangePropertyAction for this, which does not fire reliably in the
                // Avalonia port - apply it directly on the Borders of the item's
                // visual tree instead.
                ApplySelectionHighlight((bool)change.NewValue);
            }

            base.OnPropertyChanged(change);
        }

        // Red selection-frame overlay, hosted in the AdornerLayer so it renders
        // ABOVE the item content (setting BorderBrush on the template's Borders
        // is not enough: the opaque bar rectangle painted by the child content
        // covers the border stroke).
        private Avalonia.Controls.Primitives.AdornerLayer _selectionAdornerLayer;
        private Border _selectionOverlay;

        // Row siblings highlighted together with this item (timeline view draws
        // several rectangles in one row - the selection border must span all of
        // them, not only the clicked one).
        private readonly List<TimelineItemBase> _highlightedRowSiblings = new List<TimelineItemBase>();

        private void ApplySelectionHighlight(bool selected)
        {
            // Remove highlights of row siblings from a previous selection.
            foreach (var sibling in _highlightedRowSiblings)
                sibling.HighlightSelf(false);
            _highlightedRowSiblings.Clear();

            HighlightSelf(selected);

            if (selected && this.GetVisualParent() is Panel panel)
            {
                // WPF-Parity: highlight ALL elements of the selected row. Several
                // timeline items can share the same RowIndex - frame them all.
                int rowIndex = TimelinePanel.GetRowIndex(this);
                foreach (var sibling in panel.Children.OfType<TimelineItemBase>())
                {
                    if (!ReferenceEquals(sibling, this) && TimelinePanel.GetRowIndex(sibling) == rowIndex)
                    {
                        sibling.HighlightSelf(true);
                        _highlightedRowSiblings.Add(sibling);
                    }
                }
            }
        }

        private void HighlightSelf(bool selected)
        {
            if (selected)
            {
                if (_selectionOverlay == null)
                {
                    _selectionOverlay = new Border
                    {
                        BorderBrush = Avalonia.Media.Brushes.Red,
                        BorderThickness = new Thickness(1),
                        Background = null,
                        IsHitTestVisible = false
                    };
                }
                if (_selectionAdornerLayer == null)
                    _selectionAdornerLayer = Avalonia.Controls.Primitives.AdornerLayer.GetAdornerLayer(this);
                if (_selectionAdornerLayer != null && _selectionOverlay.Parent == null)
                {
                    _selectionAdornerLayer.Children.Add(_selectionOverlay);
                    _selectionOverlay.SetValue(Avalonia.Controls.Primitives.AdornerLayer.AdornedElementProperty, this);
                }
                else if (_selectionAdornerLayer == null)
                {
                    // No AdornerLayer in this visual tree (e.g. VBGanttChart hosts no
                    // AdornerDecorator) - fall back to highlighting the template's
                    // Borders directly. The bar content may partially cover the
                    // stroke there, but the frame remains visible.
                    foreach (var border in this.GetVisualDescendants().OfType<Avalonia.Controls.Border>())
                    {
                        border.SetValue(Avalonia.Controls.Border.BorderBrushProperty, Avalonia.Media.Brushes.Red);
                        border.SetValue(Avalonia.Controls.Border.BorderThicknessProperty, new Thickness(1));
                    }
                }
            }
            else
            {
                if (_selectionAdornerLayer != null && _selectionOverlay != null && _selectionOverlay.Parent != null)
                {
                    _selectionAdornerLayer.Children.Remove(_selectionOverlay);
                    _selectionOverlay.ClearValue(Avalonia.Controls.Primitives.AdornerLayer.AdornedElementProperty);
                }
                foreach (var border in this.GetVisualDescendants().OfType<Avalonia.Controls.Border>())
                {
                    border.ClearValue(Avalonia.Controls.Border.BorderBrushProperty);
                    border.ClearValue(Avalonia.Controls.Border.BorderThicknessProperty);
                }
            }
        }

        private void DeselectOtherItems()
        {
            if (this.GetVisualParent() is Panel panel)
            {
                foreach (var item in panel.Children.OfType<TimelineItemBase>())
                {
                    if (!ReferenceEquals(item, this) && item.IsSelected)
                        item.SetCurrentValue(IsSelectedProperty, false);
                }
            }
        }

        #endregion

        #region Methods

        public virtual void SelectTreeItem()
        {
            // The map is normally assigned by VBTreeListViewItem.InitVBControl.
            // If it is not set yet (e.g. tree container realized after the timeline
            // item, or TimelineView scenario), resolve it on demand.
            // NOTE: ContainerFromItem on the TreeListView only resolves TOP-LEVEL
            // items (children live in nested ItemsControls), so we search the
            // visual tree instead - works for every realized level.
            if (VBTreeListViewItemMap == null && ContextACObject != null)
            {
                var treeListView = VBTimelineView?.PART_TreeListView;
                if (treeListView != null)
                {
                    var candidates = treeListView
                        .GetVisualDescendants()
                        .OfType<VBTreeListViewItem>()
                        .ToList();
                    VBTreeListViewItemMap = ResolveTreeItem(candidates, ContextACObject);
                    if (VBTreeListViewItemMap == null)
                    {
                        // The clicked item is a CHILD log entry whose tree container
                        // is not realized (parent collapsed). Select + expand the
                        // nearest realized ANCESTOR, then select the child after
                        // the tree has realized its containers.
                        IACObject ancestor = (ContextACObject as IACObject)?.ParentACObject;
                        VBTreeListViewItem ancestorContainer = null;
                        while (ancestor != null && ancestorContainer == null)
                        {
                            ancestorContainer = ResolveTreeItem(candidates, ancestor);
                            if (ancestorContainer == null)
                                ancestor = ancestor.ParentACObject;
                        }
                        if (ancestorContainer != null)
                        {
                            ancestorContainer.IsSelected = true;
                            ancestorContainer.IsExpanded = true;
                            var childContext = ContextACObject;
                            // Re-resolve after the expansion has realized the child containers.
                            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                            {
                                var refreshed = treeListView
                                    .GetVisualDescendants()
                                    .OfType<VBTreeListViewItem>()
                                    .ToList();
                                var childContainer = ResolveTreeItem(refreshed, childContext);
                                if (childContainer != null)
                                {
                                    VBTreeListViewItemMap = childContainer;
                                    childContainer.IsSelected = true;
                                }
                            }, Avalonia.Threading.DispatcherPriority.Background);
                        }
                    }
                }
            }
            if (VBTreeListViewItemMap != null)
                VBTreeListViewItemMap.IsSelected = true;
        }

        /// <summary>
        /// Resolves the tree container for a timeline item. The timeline chart often
        /// holds COPIES of the model objects (e.g. ACPropertyLogModel created from
        /// ACPropertyLogInfo), so reference equality on DataContext fails. Match on
        /// the shared underlying PropertyLog entity first, then on identical
        /// StartDate/EndDate/PropertyValue tuples as fallback.
        /// </summary>
        private static VBTreeListViewItem ResolveTreeItem(List<VBTreeListViewItem> candidates, object context)
        {
            var ctxInfo = context as gip.core.datamodel.ACPropertyLogInfo;
            foreach (var c in candidates)
            {
                if (ReferenceEquals(c.DataContext, context))
                    return c;
            }
            if (ctxInfo != null)
            {
                foreach (var c in candidates)
                {
                    var candInfo = c.DataContext as gip.core.datamodel.ACPropertyLogInfo;
                    if (candInfo == null)
                        continue;
                    // Same underlying entity instance?
                    if (ctxInfo.PropertyLog != null && ReferenceEquals(candInfo.PropertyLog, ctxInfo.PropertyLog))
                        return c;
                    // Same time span + value (copies of the same log entry)?
                    if (ctxInfo.PropertyLog == null && candInfo.PropertyLog == null
                        && ctxInfo.StartDate == candInfo.StartDate
                        && ctxInfo.EndDate == candInfo.EndDate
                        && Equals(ctxInfo.PropertyValue, candInfo.PropertyValue))
                        return c;
                }
            }
            return null;
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
                {
                    // WPF-Parity: clicking the item on the timeline/chart selects it
                    // (red frame via IsSelected) AND the corresponding tree row.
                    this.IsSelected = true;
                    SelectTreeItem();
                }
            }
            else if (e.InitialPressMouseButton == MouseButton.Right)
            {
                // NOTE: In Avalonia the right-button PointerReleased arrives with
                // Route == Bubble (unlike WPF's tunneling preview events), so do
                // NOT gate on e.Route here - the menu would never open.
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
                vbContextMenu.Closed += (s, args) => { this.ContextMenu = null; };
                ContextMenu.Open();
                // Mark as handled so the event does not bubble up to VBDesign,
                // which would otherwise build and open a SECOND context menu
                // (its own category instances without the timeline commands).
                e.Handled = true;
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
