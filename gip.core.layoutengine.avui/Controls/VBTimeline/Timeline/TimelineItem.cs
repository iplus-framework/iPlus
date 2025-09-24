using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using gip.core.datamodel;
using System.Windows.Input;
using gip.core.layoutengine.avui.Helperclasses;
using System.Windows.Media;

namespace gip.core.layoutengine.avui.timeline
{
    /// <summary>
    /// The item on timeline.
    /// </summary>
    /// <summary xml:lang="de">
    /// Das Element auf der Zeitleiste.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'TimelineItem'}de{'TimelineItem'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class TimelineItem : TimelineItemBase
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TimelineItemStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTimeline/Themes/TimelineItemStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TimelineItemStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTimeline/Themes/TimelineItemStyleAero.xaml" },
        };
        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static TimelineItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineItem),
                new FrameworkPropertyMetadata(typeof(TimelineItem)));
        }

        bool _themeApplied = false;
        public TimelineItem()
        {
            
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
            ToolTipOpening += TimelineItem_ToolTipOpening;
        }

        #endregion

        #region Loaded-Event

        ///// <summary>
        ///// The event hander for Initialized event.
        ///// </summary>
        ///// <param name="e">The event arguments.</param>
        //protected override void OnInitialized(EventArgs e)
        //{
        //    base.OnInitialized(e);
        //    ActualizeTheme(true);
        //    ToolTipOpening += TimelineItem_ToolTipOpening;
        //    Loaded += TimelineItem_Loaded;
        //}

        ////void TimelineItem_Loaded(object sender, RoutedEventArgs e)
        ////{
        ////    if (container.Children.Count == 0 || (container.Children.Count == 3 && container.Children[2] != contentControl) && contentControl != null)
        ////    {
        ////        container.Children.Clear();
        ////        if (container.Children.Count == 0)
        ////        {
        ////            if (_ToolTipStatus == null && this.ContentTemplate != null)
        ////                _ToolTipStatus = this.ContentTemplate.Resources["ToolTipStatus"] as StackPanel;

        ////            if (_ToolTipStatus != null)
        ////            {
        ////                container.Children.Add(_ToolTipStatus);
        ////                container.Children.Add(new Separator() { Height = 5, Background = Brushes.Transparent });
        ////            }
        ////        }
        ////    }
        ////    else if (contentControl == null)
        ////    {
        ////        Binding bind = new Binding();
        ////        bind.Source = this;
        ////        bind.Path = new PropertyPath("ToolTipContent");
        ////        contentControl = new ContentControl();
        ////        contentControl.SetBinding(ContentControl.ContentProperty, bind);
        ////    }
        ////    if (container.Children.Count == 2 && contentControl != null)
        ////        container.Children.Add(contentControl);
        ////}

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
            //leftConnector = Template.FindName("PART_Left", this) as Connector;
            //rightConnector = Template.FindName("PART_Right", this) as Connector;
            InitVBControl();
        }

        StackPanel _ToolTipStatus;
        ContentControl contentControl;

        public override void InitVBControl()
        {
            base.InitVBControl();

            Style timelineItemStyle = this.ContentTemplate.Resources["TimelineItemResource"] as Style;
            if (timelineItemStyle != null)
                this.Style = timelineItemStyle;

            ToolTipContent = VBTimelineView.ToolTipItems;
            int toolTipChildrenCount = 0;

            if (VBTimelineChart.container.Children.Count == 0)
            {
                if (this.ContentTemplate != null)
                {
                    _ToolTipStatus = this.ContentTemplate.Resources["ToolTipStatus"] as StackPanel;
                    if (_ToolTipStatus != null)
                    {
                        VBTimelineChart.container.Children.Add(_ToolTipStatus);
                        VBTimelineChart.container.Children.Add(new Separator() { Height = 5, Background = Brushes.Transparent });
                        toolTipChildrenCount = 2;
                    }
                }
            }
            if (TimelinePanel.GetRowIndex(this) == 0)
            {
                Binding bind = new Binding();
                bind.Source = this;
                bind.Path = new PropertyPath("ToolTipContent");
                contentControl = new ContentControl();
                contentControl.SetBinding(ContentControl.ContentProperty, bind);
                if (VBTimelineChart.container.Children.Count == toolTipChildrenCount)
                    VBTimelineChart.container.Children.Add(contentControl);
            }

            ToolTip = VBTimelineChart.container;

            if (VBTreeListViewItemMap == null)
            {
                VBTreeListViewItemMap = VBTimelineView.PART_TreeListView.ItemContainerGenerator.ContainerFromItem(this.ContextACObject) as VBTreeListViewItem;

                if (VBTreeListViewItemMap != null)
                {
                    VBTreeListViewItemMap.TimelineItemMap = this;
                    if (VBTreeListViewItemMap.IsVisible)
                        this.IsCollapsed = false;
                }
                else
                {
                    TimelineItemPanel panel = this.VisualParent as TimelineItemPanel;
                    if (panel != null)
                    {
                        var parentTimelineItem = panel.Children.OfType<TimelineItem>().FirstOrDefault(c => c.Content == this.ContextACObject.ParentACObject);
                        if(parentTimelineItem != null && parentTimelineItem.TLItemType == TimelineItemType.ContainerItem && this.TLItemType == TimelineItemType.TimelineItem)
                        {
                            _ParentConainter = parentTimelineItem;

                            Binding bind = new Binding();
                            bind.Source = parentTimelineItem;
                            bind.Path = new PropertyPath("IsCollapsed");
                            this.SetBinding(IsCollapsedProperty, bind);
                        }
                    }
                }
            }

            if (TLItemType != TimelineItemType.TimelineItem)
                IsToolTipEnabled = false;

            //_IsInitialized = true;
        }

        #endregion

        #region Properties

        private TimelineItem _ParentConainter;

        #endregion

        #region DependencyProp

        public Style TimelineItemStatus
        {
            get { return (Style)GetValue(TimelineItemStatusProperty); }
            set { SetValue(TimelineItemStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimelineItemStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimelineItemStatusProperty =
            DependencyProperty.Register("TimelineItemStatus", typeof(Style), typeof(TimelineItem));

        public TimelineItemType TLItemType
        {
            get { return (TimelineItemType)GetValue(TLItemTypeProperty); }
            set { SetValue(TLItemTypeProperty, value); }
        }

        public static readonly DependencyProperty TLItemTypeProperty =
            DependencyProperty.Register("TLItemType", typeof(TimelineItemType), typeof(TimelineItem));


        #endregion

        #region Methods

        public override bool DeInitVBControl()
        {
            if (contentControl != null)
                BindingOperations.ClearBinding(contentControl, ContentControl.ContentProperty);
            BindingOperations.ClearBinding(this, TimelinePanel.StartDateProperty);
            BindingOperations.ClearBinding(this, TimelinePanel.EndDateProperty);
            BindingOperations.ClearBinding(this, TimelinePanel.RowIndexProperty);
            BindingOperations.ClearBinding(this, ToolTipContentProperty);
            this.ClearAllBindings();
            ToolTipOpening -= TimelineItem_ToolTipOpening;
            Loaded -= TimelineItem_Loaded;
            VBTreeListViewItemMap = null;
            return true;
        }

        public override void SelectTreeItem()
        {
            if (TLItemType == TimelineItemType.TimelineItem && _ParentConainter != null)
                _ParentConainter.SelectTreeItem();

            else if (TLItemType != TimelineItemType.EmptyItem)
                base.SelectTreeItem();
        }

        #endregion

    }

    public enum TimelineItemType : short
    {
        TimelineItem = 10,
        ContainerItem = 20,
        EmptyItem = 30
    }
}
