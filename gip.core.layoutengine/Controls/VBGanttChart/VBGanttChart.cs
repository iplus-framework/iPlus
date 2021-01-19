using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Windows;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;
using gip.core.datamodel;
using System.Windows.Data;
using System.Windows.Documents;
using gip.core.layoutengine.ganttchart;
using System.Windows.Media;
using gip.ext.graphics.shapes;
using gip.core.layoutengine.timeline;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents the Gantt chart control.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt die Gantt-Diagrammsteuerung dar.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBGanttChart'}de{'VBGanttChart'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBGanttChart : VBTimelineChartBase
    {
        #region c'tors
        
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "GanttChartStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBGanttChart/Themes/GanttChartStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "GanttChartStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBGanttChart/Themes/GanttChartStyleAero.xaml" },
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

        bool _themeApplied = false;

        static VBGanttChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBGanttChart), new FrameworkPropertyMetadata(typeof(VBTimelineChartBase)));
        }

        /// <summary>
        /// Creates a new instance of VBGanttChart.
        /// </summary>
        public VBGanttChart()
        {
            Items = new ObservableCollection<object>();
            Connections = new ObservableCollection<object>();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
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
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);

            itemsPresenter = Template.FindName("PART_ItemsPresenter", this) as TimelineGanttItemsPresenter;
            connectionsPresenter = Template.FindName("PART_ConnectionsPresenter", this) as ConnectionsPresenter;

            WireConnectionsPresenter();
        }

        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
        public override void InitVBControl()
        {
            base.InitVBControl();
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public override void DeInitVBControl(IACComponent bso)
        {
            (itemsPresenter as TimelineGanttItemsPresenter).DeInitControl();
            Items.All(c => ContainerFromItem(c).DeInitVBControl());
            BindingOperations.ClearBinding(this, ItemsSourceProperty);
            BindingOperations.ClearBinding(this, SelectedItemProperty);
            BindingOperations.ClearAllBindings(this);
            base.DeInitVBControl(bso);
        }

        #endregion

        #region PARTs

        private ConnectionsPresenter connectionsPresenter;

        #endregion

        #region Properties

        private VBGanttChartView _VBGanttChartView
        {
            get
            {
                return TemplatedParent as VBGanttChartView;
            }
        }

        #endregion

        #region DP

        /// <summary>
        /// Represents the dependecy property key for the Connections.
        /// </summary>
        private static readonly DependencyPropertyKey ConnectionsPropertyKey = 
            DependencyProperty.RegisterReadOnly("Connections", typeof(IList<object>), typeof(VBGanttChart), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Represents the dependecy property for the Connections.
        /// </summary>
        public static readonly DependencyProperty ConnectionsProperty = ConnectionsPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        public IList<object> Connections
        {
            get { return (IList<object>)GetValue(ConnectionsProperty); }
            private set { SetValue(ConnectionsPropertyKey, value); }
        }

        /// <summary>
        /// Represents the dependecy property for the ConnectionsSource.
        /// </summary>
        public static readonly DependencyProperty ConnectionsSourceProperty =
            DependencyProperty.Register("ConnectionsSource", typeof(IEnumerable), typeof(VBGanttChart), new FrameworkPropertyMetadata(null, ConnectionsSourceChanged));

        /// <summary>
        /// Gets or sets the ConnectionsSource
        /// </summary>
        public IEnumerable ConnectionsSource
        {
            get { return (IEnumerable)GetValue(ConnectionsSourceProperty); }
            set { SetValue(ConnectionsSourceProperty, value); }
        }

        private static void ConnectionsSourceChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBGanttChart self = (VBGanttChart)o;
            self.ConnectionsSourceChanged(e);
        }

        private void ConnectionsSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                IEnumerable items = (IEnumerable)e.NewValue;
                AddConnectionsInternal(items);

                INotifyCollectionChanged oldConnectionsSource = e.OldValue as INotifyCollectionChanged;
                if (oldConnectionsSource != null)
                {
                    oldConnectionsSource.CollectionChanged -= ConnectionsSource_CollectionChanged;
                }

                INotifyCollectionChanged collectionChanged = e.NewValue as INotifyCollectionChanged;
                if (collectionChanged != null)
                {
                    collectionChanged.CollectionChanged += ConnectionsSource_CollectionChanged;
                }
            }
        }

        void ConnectionsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddConnectionsInternal(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                RemoveConnectionsInternal(e.OldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                RemoveAllConnectionsInternal();
            }
        }

       


        /// <summary>
        /// Gets the style selector of connection.
        /// </summary>
        public StyleSelector ConnectionStyleSelector
        {
            get { return (StyleSelector)GetValue(ConnectionStyleSelectorProperty); }
            set { SetValue(ConnectionStyleSelectorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectionStyleSelector.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectionStyleSelectorProperty =
            DependencyProperty.Register("ConnectionStyleSelector", typeof(StyleSelector), typeof(VBGanttChart),
            new FrameworkPropertyMetadata(null));

       

        private bool ignoreCurrentChanged = false;

        private void CurrentTimeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (ignoreCurrentChanged) return;

            GoToDate((DateTime)e.NewValue);
        }

        #endregion

        #region Methods

        //private void CalculateDefaultTickTimeSpan()
        //{
        //    if (MinimumDate.HasValue && MaximumDate.HasValue)
        //    {
        //        TimeSpan diff = MaximumDate.Value - MinimumDate.Value;
        //        if (diff.TotalMinutes < 60)
        //            _defaultTimeTickSpanValue = TimeSpan.FromSeconds(5);
        //        else
        //            _defaultTimeTickSpanValue = TimeSpan.FromSeconds(60);
        //    }
        //    TickTimeSpan = _defaultTimeTickSpanValue;
        //}

        //private void AddItemsInternal(IEnumerable items)
        //{
        //    foreach (object item in items)
        //    {
        //        AddItemInternal(item);
        //    }
        //    SetMinMaxBounds();
        //}

        //private void AddItemInternal(object item)
        //{
        //    Items.Add(item);
        //}

        //private void RemoveItemsInternal(IEnumerable items)
        //{
        //    foreach (object item in items)
        //    {
        //        RemoveItemInternal(item);
        //    }
        //}

        //private void RemoveItemInternal(object item)
        //{
        //    Items.Remove(item);
        //}

        //private void RemoveAllItemsInternal()
        //{
        //    Items.Clear();
        //}

        private void RemoveAllConnectionsInternal()
        {
            Connections.Clear();
        }

        private void AddConnectionsInternal(IEnumerable items)
        {
            foreach (object item in items)
            {
                AddConnectionInternal(item);
            }
        }

        private void AddConnectionInternal(object item)
        {
            Connections.Add(item);
        }

        private void RemoveConnectionsInternal(IEnumerable items)
        {
            foreach (var item in items)
            {
                RemoveConnectionInternal(item);
            }
        }

        private void RemoveConnectionInternal(object item)
        {
            Connections.Remove(item);
        }

        //private void UpdateDisplayTimeSpan()
        //{
        //    if (ActualWidth > 0 && TickTimeSpan.Ticks > 0)
        //    {
        //        double pixelPerTick = Timeline.GetPixelsPerTick(this);
        //        DisplayTimeSpan = TimeSpan.FromTicks((long)(ActualWidth / pixelPerTick));
        //    }
        //    else
        //    {
        //        DisplayTimeSpan = TimeSpan.Zero;
        //    }
        //}

        private void WireConnectionsPresenter()
        {
            if (connectionsPresenter != null)
            {
                connectionsPresenter.Timeline = this;
            }
        }

        internal TimelineGanttItem ContainerFromItem(object item)
        {
            if (itemsPresenter == null) return null;

            return (itemsPresenter as TimelineGanttItemsPresenter).ContainerFromItem(item);
        }

        /// <summary>
        /// Brings item into view.
        /// </summary>
        /// <param name="item">The item parameter.</param>
        public void BringItemIntoView(object item)
        {
            TimelineGanttItem container = ContainerFromItem(item);
            if (container != null)
            {
                Nullable<DateTime> start = TimelineCompactPanel.GetStartDate(container);
                if (start.HasValue)
                {
                    GoToDate(start.Value);
                }
            }
        }

        /// <summary>
        /// Brings into view.
        /// </summary>
        /// <param name="mode">The mode parameter.</param>
        /// <param name="dataItem">The data item parameter.</param>
        public void BringIntoView(VBGanttChartBringIntoViewMode mode, object dataItem)
        {
            TimelineGanttItem container = ContainerFromItem(dataItem);
            if (container != null)
            {
                Nullable<DateTime> start = TimelineCompactPanel.GetStartDate(container);
                Nullable<DateTime> end = TimelineCompactPanel.GetEndDate(container);

                if (IsSetZoomToFit(mode) && start.HasValue && end.HasValue)
                {
                    TimeSpan duration = end.Value - start.Value;
                    double pixelPerTick = (ActualWidth / 2) / duration.Ticks;
                    TimeSpan newTickTimeSpan = TimeSpan.FromTicks((long)(1D / pixelPerTick));

                    if (newTickTimeSpan.TotalMinutes < 1)
                    {
                        newTickTimeSpan = TimeSpan.FromMinutes(1);
                    }

                    if (newTickTimeSpan < TickTimeSpan)
                    {
                        TickTimeSpan = newTickTimeSpan;
                    }
                    else
                    {
                        if (ActualWidth / 2 < duration.Ticks * Timeline.GetPixelsPerTick(this))
                        {
                            TickTimeSpan = newTickTimeSpan;
                        }
                    }

                    WpfUtility.WaitForPriority(DispatcherPriority.Background);
                }

                if (IsSetCurrentTime(mode))
                {
                    if (start.HasValue) CurrentTime = start.Value;
                    else if (end.HasValue) CurrentTime = end.Value;
                }
            }
        }

        private static bool IsSetZoomToFit(VBGanttChartBringIntoViewMode mode)
        {
            return ((mode & VBGanttChartBringIntoViewMode.SetZoomToFit) != 0);
        }

        private static bool IsSetCurrentTime(VBGanttChartBringIntoViewMode mode)
        {
            return ((mode & VBGanttChartBringIntoViewMode.SetCurrentTime) != 0);
        }

        #endregion
    }

    /// <summary>
    /// Represents the enumeration for BringIntoViewMode.
    /// </summary>
    [Flags]
    public enum VBGanttChartBringIntoViewMode
    {
        SetCurrentTime = 1,
        SetZoomToFit = 2,

        FcosOnItem = SetCurrentTime | SetZoomToFit
    }
}
