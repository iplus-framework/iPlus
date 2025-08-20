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
using gip.core.layoutengine.avui.timeline;
using System.Windows.Media;

namespace gip.core.layoutengine.avui.ganttchart
{
    /// <summary>
    /// The item on timeline.
    /// </summary>
    /// <summary xml:lang="de">
    /// Das Element auf der Zeitleiste.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'TimelineItem'}de{'TimelineItem'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class TimelineGanttItem : TimelineItemBase
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TimelineItemStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBGanttChart/Themes/TimelineGanttItemStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TimelineItemStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBGanttChart/Themes/TimelineGanttItemStyleAero.xaml" },
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

        static TimelineGanttItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineGanttItem),
                new FrameworkPropertyMetadata(typeof(TimelineGanttItem)));
        }

        bool _themeApplied = false;
        public TimelineGanttItem()
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

        #endregion

        #region Loaded-Event

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

        //void TimelineItem_Loaded(object sender, RoutedEventArgs e)
        //{
        //    if (container.Children.Count == 0 || (container.Children.Count == 3 && container.Children[2] != contentControl) && contentControl != null)
        //    {
        //        container.Children.Clear();
        //        if (container.Children.Count == 0)
        //        {
        //            if (_ToolTipStatus == null && this.ContentTemplate != null)
        //                _ToolTipStatus = this.ContentTemplate.Resources["ToolTipStatus"] as StackPanel;

        //            if (_ToolTipStatus != null)
        //            {
        //                container.Children.Add(_ToolTipStatus);
        //                container.Children.Add(new Separator() { Height = 5, Background = Brushes.Transparent });
        //            }
        //        }
        //    }
        //    else if (contentControl == null)
        //    {
        //        Binding bind = new Binding();
        //        bind.Source = this;
        //        bind.Path = new PropertyPath("ToolTipContent");
        //        contentControl = new ContentControl();
        //        contentControl.SetBinding(ContentControl.ContentProperty, bind);
        //    }
        //    if (container.Children.Count == 2 && contentControl != null)
        //        container.Children.Add(contentControl);
        //}

        ///// <summary>
        ///// Overides the OnApplyTemplate method and run VBControl initialization.
        ///// </summary>
        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();
        //    if (!_themeApplied)
        //        ActualizeTheme(false);
        //    //leftConnector = Template.FindName("PART_Left", this) as Connector;
        //    //rightConnector = Template.FindName("PART_Right", this) as Connector;
        //    InitVBControl();
        //}

        //bool _IsInitialized = false;
        //internal static StackPanel container = new StackPanel();
        StackPanel _ToolTipStatus;
        ContentControl contentControl;

        public override void InitVBControl()
        {
            base.InitVBControl();

            ToolTipContent = VBGanttView.ToolTipItems;

            if(VBTimelineChart.container.Children.Count == 0)
            {
                if (this.ContentTemplate != null)
                {
                    _ToolTipStatus = this.ContentTemplate.Resources["ToolTipStatus"] as StackPanel;
                    if (_ToolTipStatus != null)
                    {
                        VBTimelineChart.container.Children.Add(_ToolTipStatus);
                        VBTimelineChart.container.Children.Add(new Separator() { Height = 5, Background = Brushes.Transparent });
                    }
                }
            }
            if(TimelinePanel.GetRowIndex(this) == 0)
            {
                Binding bind = new Binding();
                bind.Source = this;
                bind.Path = new PropertyPath("ToolTipContent");
                contentControl = new ContentControl();
                contentControl.SetBinding(ContentControl.ContentProperty, bind);
                if (VBTimelineChart.container.Children.Count == 2)
                    VBTimelineChart.container.Children.Add(contentControl);
            }

            ToolTip = VBTimelineChart.container;

            if (VBTreeListViewItemMap == null)
            {
                VBTreeListViewItemMap = VBGanttView.PART_TreeListView.ItemContainerGenerator.ContainerFromItem(this.ContextACObject) as VBTreeListViewItem;

                if (VBTreeListViewItemMap != null)
                {
                    VBTreeListViewItemMap.TimelineItemMap = this;
                    if (VBTreeListViewItemMap.IsVisible)
                        this.IsCollapsed = false;
                }
            }
        }

        #endregion

        #region Properties

        private Connector leftConnector;    
        private Connector rightConnector;

        private VBGanttChartView _VBGanttView;
        public VBGanttChartView VBGanttView
        {
            get 
            {
                if (_VBGanttView == null)
                    _VBGanttView = Helperclasses.VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBGanttChartView)) as VBGanttChartView;
                return _VBGanttView;
            }
        }

        private VBGanttChart _VBGantt;
        public VBGanttChart VBGantt
        {
            get
            {
                if (_VBGantt == null)
                    _VBGantt = WpfUtility.FindVisualParent<VBGanttChart>(this);
                return _VBGantt;
            }
        }

        public Connector LeftConnector
        {
            get
            {
                if (leftConnector == null)
                {
                    ApplyTemplate();
                    leftConnector = Template.FindName("PART_Left", this) as Connector;
                }
                return leftConnector;
            }
        }

        public Connector RightConnector
        {
            get
            {
                if (rightConnector == null)
                {
                    ApplyTemplate();
                    rightConnector = Template.FindName("PART_Right", this) as Connector;
                }
                return rightConnector;
            }
        }

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
            BindingOperations.ClearAllBindings(this);
            ToolTipOpening -= TimelineItem_ToolTipOpening;
            Loaded -= TimelineItem_Loaded;
            VBTreeListViewItemMap = null;
            return true;
        }

        #endregion
    }
}
