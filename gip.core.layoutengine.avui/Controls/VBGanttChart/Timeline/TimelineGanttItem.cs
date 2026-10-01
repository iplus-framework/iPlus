using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.layoutengine.avui.timeline;

namespace gip.core.layoutengine.avui.ganttchart
{

    [TemplatePart(Name = "PART_Left", Type = typeof(Connector))]
    [TemplatePart(Name = "PART_Right", Type = typeof(Connector))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'TimelineItem'}de{'TimelineItem'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class TimelineGanttItem : TimelineItemBase
    {

        #region Loaded-Event

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            _LeftConnector = e.NameScope.Find<Connector>("PART_Left");
            _RightConnector = e.NameScope.Find<Connector>("PART_Right");
            base.OnApplyTemplate(e);
        }

        //StackPanel _ToolTipStatus;
        ContentControl contentControl;
        Connector _LeftConnector;
        Connector _RightConnector;

        public override void InitVBControl()
        {
            base.InitVBControl();

            ToolTipContent = VBGanttView.ToolTipItems;

            if (VBTimelineChart.container.Children.Count == 0)
            {
                // TODO Avalonia: Must be solved in XAML
                //if (this.ContentTemplate != null)
                //{
                //    _ToolTipStatus = this.ContentTemplate.Resources["ToolTipStatus"] as StackPanel;
                //    if (_ToolTipStatus != null)
                //    {
                //        VBTimelineChart.container.Children.Add(_ToolTipStatus);
                //        VBTimelineChart.container.Children.Add(new Separator() { Height = 5, Background = Brushes.Transparent });
                //    }
                //}
            }
            if (TimelinePanel.GetRowIndex(this) == 0)
            {
                Binding bind = new Binding();
                bind.Source = this;
                bind.Path = nameof(ToolTipContent);
                contentControl = new ContentControl();
                contentControl.Bind(ContentControl.ContentProperty, bind);
                // Compare against toolTipChildrenCount (0 in the Avalonia port, the
                // ToolTipStatus branch is commented out) - NOT against a hardcoded 2,
                // otherwise the shared tooltip content host is never added and every
                // tooltip shows up empty.
                if (VBTimelineChart.container.Children.Count == 0)
                    VBTimelineChart.container.Children.Add(contentControl);
            }

            SetSharedToolTip(this, VBTimelineChart.container);

            if (VBTreeListViewItemMap == null)
            {
                VBTreeListViewItemMap = VBGanttView.PART_TreeListView.ContainerFromItem(this.ContextACObject) as VBTreeListViewItem;

                if (VBTreeListViewItemMap != null)
                {
                    VBTreeListViewItemMap.TimelineItemMap = this;
                    // Sync with the tree's EFFECTIVE visibility (a child of a collapsed
                    // ancestor must stay hidden even though its own IsVisible is true).
                    this.IsCollapsed = !VBTreeListViewItemMap.IsEffectivelyVisible();
                }
                else
                {
                    // No tree counterpart realized yet (e.g. root collapsed -> children
                    // not realized). Hide the row until the tree item maps in and
                    // confirms visibility, otherwise ALL rows show initially.
                    this.IsCollapsed = true;
                }
            }
        }

        #endregion

        #region Properties

        //private Connector leftConnector;    
        //private Connector rightConnector;

        private VBGanttChartView _VBGanttView;
        public VBGanttChartView VBGanttView
        {
            get 
            {
                if (_VBGanttView == null)
                    _VBGanttView = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBGanttChartView)) as VBGanttChartView;
                return _VBGanttView;
            }
        }

        private VBGanttChart _VBGantt;
        public VBGanttChart VBGantt
        {
            get
            {
                if (_VBGantt == null)
                    _VBGantt = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBGanttChart)) as VBGanttChart;
                return _VBGantt;
            }
        }

        public Connector LeftConnector
        {
            get
            {
                return _LeftConnector;
            }
        }

        public Connector RightConnector
        {
            get
            {
                return _RightConnector;
            }
        }

        #endregion

        #region Methods

        public override bool DeInitVBControl()
        {
            if (contentControl != null)
                contentControl.ClearAllBindings();
            this.ClearAllBindings();
            VBTreeListViewItemMap = null;
            base.DeInitVBControl();
            return true;
        }

        #endregion
    }
}
