using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using gip.core.datamodel;
using gip.core.layoutengine.avui.ganttchart;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.layoutengine.avui.timeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    [TemplatePart("PART_ItemsPresenter", typeof(TimelineGanttItemsPresenter))]
    [TemplatePart("PART_ConnectionsPresenter", typeof(ConnectionsPresenter))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBGanttChart'}de{'VBGanttChart'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBGanttChart : VBTimelineChartBase
    {
        #region c'tors
        
        /// <summary>
        /// Creates a new instance of VBGanttChart.
        /// </summary>
        public VBGanttChart()
        {
            Items = new ObservableCollection<IACTimeLog>();
            _Connections = new ObservableCollection<object>();
        }

        #endregion

        #region Loaded-Event

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _ItemsPresenter = e.NameScope.Find("PART_ItemsPresenter") as TimelineGanttItemsPresenter;
            _ConnectionsPresenter = e.NameScope.Find("PART_ConnectionsPresenter") as ConnectionsPresenter;

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
            (_ItemsPresenter as TimelineGanttItemsPresenter).DeInitControl();
            Items.All(c => ContainerFromItem(c).DeInitVBControl());
            this.ClearAllBindings();
            base.DeInitVBControl(bso);
        }

        #endregion

        #region PARTs

        private ConnectionsPresenter _ConnectionsPresenter;

        #endregion

        #region Properties

        private VBGanttChartView GanttChartView
        {
            get
            {
                return TemplatedParent as VBGanttChartView;
            }
        }

        #endregion

        #region Styled Properties

        /// <summary>
        /// Defines the Connections styled property (read-only).
        /// </summary>
        public static readonly DirectProperty<VBGanttChart, IList<object>> ConnectionsProperty =
            AvaloniaProperty.RegisterDirect<VBGanttChart, IList<object>>(
                nameof(Connections),
                o => o.Connections,
                (o, v) => o.Connections = v);

        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        public IList<object> Connections
        {
            get => _Connections;
            private set => SetAndRaise(ConnectionsProperty, ref _Connections, value);
        }
        private IList<object> _Connections;

        /// <summary>
        /// Defines the ConnectionsSource styled property.
        /// </summary>
        public static readonly StyledProperty<IEnumerable> ConnectionsSourceProperty =
            AvaloniaProperty.Register<VBGanttChart, IEnumerable>(nameof(ConnectionsSource));

        /// <summary>
        /// Gets or sets the ConnectionsSource
        /// </summary>
        public IEnumerable ConnectionsSource
        {
            get => GetValue(ConnectionsSourceProperty);
            set => SetValue(ConnectionsSourceProperty, value);
        }

        #endregion

        #region Property Changed Handling

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ConnectionsSourceProperty)
            {
                ConnectionsSourceChanged(change);
            }
        }

        private void ConnectionsSourceChanged(AvaloniaPropertyChangedEventArgs e)
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

        private void CurrentTimeChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (ignoreCurrentChanged) return;

            GoToDate((DateTime)e.NewValue);
        }

        #endregion

        #region Methods

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

        private void WireConnectionsPresenter()
        {
            if (_ConnectionsPresenter != null)
            {
                _ConnectionsPresenter.Timeline = this;
            }
        }

        internal TimelineGanttItem ContainerFromItem(object item)
        {
            if (_ItemsPresenter == null) 
                return null;

            return (_ItemsPresenter as TimelineGanttItemsPresenter).ContainerFromItem(item);
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
        public async void BringIntoView(VBGanttChartBringIntoViewMode mode, object dataItem)
        {
            TimelineGanttItem container = ContainerFromItem(dataItem);
            if (container != null)
            {
                Nullable<DateTime> start = TimelineCompactPanel.GetStartDate(container);
                Nullable<DateTime> end = TimelineCompactPanel.GetEndDate(container);

                if (IsSetZoomToFit(mode) && start.HasValue && end.HasValue)
                {
                    TimeSpan duration = end.Value - start.Value;
                    double pixelPerTick = (Bounds.Width / 2) / duration.Ticks;
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
                        if (Bounds.Width / 2 < duration.Ticks * Timeline.GetPixelsPerTick(this))
                        {
                            TickTimeSpan = newTickTimeSpan;
                        }
                    }

                    await WpfUtility.WaitForPriorityAsync(DispatcherPriority.Background);
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
