using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.VisualTree;
using Avalonia.Styling;
using Avalonia;

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
        #region Loaded-Event
        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }

        //StackPanel _ToolTipStatus;
        //ContentControl _ContentControl;

        public override void InitVBControl()
        {
            base.InitVBControl();

            // TODO Avalonia: Must be solved in XAML
            //Style timelineItemStyle = this.ContentTemplate.Resources["TimelineItemResource"] as Style;
            //if (timelineItemStyle != null)
            //    this.Style = timelineItemStyle;

            ToolTipContent = VBTimelineView.ToolTipItems;
            int toolTipChildrenCount = 0;

            if (VBTimelineChart.container.Children.Count == 0)
            {
                if (this.ContentTemplate != null)
                {
                    // TODO Avalonia: Must be solved in XAML
                    //_ToolTipStatus = this.ContentTemplate.Resources["ToolTipStatus"] as StackPanel;
                    //if (_ToolTipStatus != null)
                    //{
                    //    VBTimelineChart.container.Children.Add(_ToolTipStatus);
                    //    VBTimelineChart.container.Children.Add(new Separator() { Height = 5, Background = Brushes.Transparent });
                    //    toolTipChildrenCount = 2;
                    //}
                }
            }
            if (TimelinePanel.GetRowIndex(this) == 0)
            {
                Binding bind = new Binding();
                bind.Source = this;
                bind.Path = nameof(ToolTipContent);
                ContentControl contentControl = new ContentControl();
                contentControl.Bind(ContentControl.ContentProperty, bind);
                if (VBTimelineChart.container.Children.Count == toolTipChildrenCount)
                    VBTimelineChart.container.Children.Add(contentControl);
            }

            ToolTip.SetTip(this, VBTimelineChart.container);

            if (VBTreeListViewItemMap == null)
            {
                VBTreeListViewItemMap = VBTimelineView.PART_TreeListView.ContainerFromItem(this.ContextACObject) as VBTreeListViewItem;

                if (VBTreeListViewItemMap != null)
                {
                    VBTreeListViewItemMap.TimelineItemMap = this;
                    if (VBTreeListViewItemMap.IsVisible)
                        this.IsCollapsed = false;
                }
                else
                {
                    TimelineItemPanel panel = this.GetVisualParent() as TimelineItemPanel;
                    if (panel != null)
                    {
                        var parentTimelineItem = panel.Children.OfType<TimelineItem>().FirstOrDefault(c => c.Content == this.ContextACObject.ParentACObject);
                        if(parentTimelineItem != null && parentTimelineItem.TLItemType == TimelineItemType.ContainerItem && this.TLItemType == TimelineItemType.TimelineItem)
                        {
                            _ParentConainter = parentTimelineItem;

                            Binding bind = new Binding();
                            bind.Source = parentTimelineItem;
                            bind.Path = nameof(IsCollapsed);
                            this.Bind(IsCollapsedProperty, bind);
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

        public ControlTheme TimelineItemStatus
        {
            get { return (ControlTheme)GetValue(TimelineItemStatusProperty); }
            set { SetValue(TimelineItemStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimelineItemStatus.  This enables animation, styling, binding, etc...
        public static readonly StyledProperty<ControlTheme> TimelineItemStatusProperty = AvaloniaProperty.Register<TimelineItem, ControlTheme>(nameof(TimelineItemStatus));

        public TimelineItemType TLItemType
        {
            get { return (TimelineItemType)GetValue(TLItemTypeProperty); }
            set { SetValue(TLItemTypeProperty, value); }
        }

        public static readonly StyledProperty<TimelineItemType> TLItemTypeProperty = AvaloniaProperty.Register<TimelineItem, TimelineItemType>(nameof(TLItemType));
        #endregion

        #region Methods

        public override bool DeInitVBControl()
        {
            this.ClearAllBindings();
            VBTreeListViewItemMap = null;
            base.DeInitVBControl();
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
