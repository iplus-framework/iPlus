using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Media;
using Avalonia.Collections;

namespace gip.core.layoutengine.avui.ganttchart
{
    /// <summary>
    /// Represent connection between two TimelineItems
    /// </summary>
    public class Connection : Control
    {
        #region c'tors

        public Connection()
            : this(null, null)
        {
        }

        public Connection(Connector source, Connector sink) : base()
        {
            this.Source = source;
            this.Sink = sink;

            Loaded += Connection_Loaded;
            Unloaded += Connection_Unloaded;
        }
        #endregion

        #region Styled Properties

        /// <summary>
        /// Defines the SourceItem styled property.
        /// </summary>
        public static readonly StyledProperty<TimelineGanttItem> SourceItemProperty =
            AvaloniaProperty.Register<Connection, TimelineGanttItem>(nameof(SourceItem));

        /// <summary>
        /// Gets or sets the source timeline gantt item.
        /// </summary>
        public TimelineGanttItem SourceItem
        {
            get => GetValue(SourceItemProperty);
            set => SetValue(SourceItemProperty, value);
        }

        /// <summary>
        /// Defines the SinkItem styled property.
        /// </summary>
        public static readonly StyledProperty<TimelineGanttItem> SinkItemProperty =
            AvaloniaProperty.Register<Connection, TimelineGanttItem>(nameof(SinkItem));

        /// <summary>
        /// Gets or sets the sink timeline gantt item.
        /// </summary>
        public TimelineGanttItem SinkItem
        {
            get => GetValue(SinkItemProperty);
            set => SetValue(SinkItemProperty, value);
        }

        /// <summary>
        /// Defines the Source styled property.
        /// </summary>
        public static readonly StyledProperty<Connector> SourceProperty =
            AvaloniaProperty.Register<Connection, Connector>(nameof(Source));

        /// <summary>
        /// Gets or sets the source connector.
        /// </summary>
        public Connector Source
        {
            get => GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        /// <summary>
        /// Defines the Sink styled property.
        /// </summary>
        public static readonly StyledProperty<Connector> SinkProperty =
            AvaloniaProperty.Register<Connection, Connector>(nameof(Sink));

        /// <summary>
        /// Gets or sets the sink connector.
        /// </summary>
        public Connector Sink
        {
            get => GetValue(SinkProperty);
            set => SetValue(SinkProperty, value);
        }

        /// <summary>
        /// Defines the PathGeometry styled property.
        /// </summary>
        public static readonly StyledProperty<PathGeometry> PathGeometryProperty =
            AvaloniaProperty.Register<Connection, PathGeometry>(nameof(PathGeometry));

        /// <summary>
        /// Gets or sets the connection path geometry.
        /// </summary>
        public PathGeometry PathGeometry
        {
            get => GetValue(PathGeometryProperty);
            set => SetValue(PathGeometryProperty, value);
        }

        /// <summary>
        /// Defines the AnchorPositionSource styled property.
        /// </summary>
        public static readonly StyledProperty<Point> AnchorPositionSourceProperty =
            AvaloniaProperty.Register<Connection, Point>(nameof(AnchorPositionSource));

        /// <summary>
        /// Gets or sets the anchor position source.
        /// Between source connector position and the beginning of the path geometry we leave some space for visual reasons; 
        /// so the anchor position source really marks the beginning of the path geometry on the source side.
        /// </summary>
        public Point AnchorPositionSource
        {
            get => GetValue(AnchorPositionSourceProperty);
            set => SetValue(AnchorPositionSourceProperty, value);
        }

        /// <summary>
        /// Defines the AnchorAngleSource styled property.
        /// </summary>
        public static readonly StyledProperty<double> AnchorAngleSourceProperty =
            AvaloniaProperty.Register<Connection, double>(nameof(AnchorAngleSource));

        /// <summary>
        /// Gets or sets the anchor angle source.
        /// Slope of the path at the anchor position, needed for the rotation angle of the arrow.
        /// </summary>
        public double AnchorAngleSource
        {
            get => GetValue(AnchorAngleSourceProperty);
            set => SetValue(AnchorAngleSourceProperty, value);
        }

        /// <summary>
        /// Defines the AnchorPositionSink styled property.
        /// </summary>
        public static readonly StyledProperty<Point> AnchorPositionSinkProperty =
            AvaloniaProperty.Register<Connection, Point>(nameof(AnchorPositionSink));

        /// <summary>
        /// Gets or sets the anchor position sink.
        /// Analogue to source side.
        /// </summary>
        public Point AnchorPositionSink
        {
            get => GetValue(AnchorPositionSinkProperty);
            set => SetValue(AnchorPositionSinkProperty, value);
        }

        /// <summary>
        /// Defines the AnchorAngleSink styled property.
        /// </summary>
        public static readonly StyledProperty<double> AnchorAngleSinkProperty =
            AvaloniaProperty.Register<Connection, double>(nameof(AnchorAngleSink));

        /// <summary>
        /// Gets or sets the anchor angle sink.
        /// Analogue to source side.
        /// </summary>
        public double AnchorAngleSink
        {
            get => GetValue(AnchorAngleSinkProperty);
            set => SetValue(AnchorAngleSinkProperty, value);
        }

        /// <summary>
        /// Defines the SourceArrowSymbol styled property.
        /// </summary>
        public static readonly StyledProperty<ArrowSymbol> SourceArrowSymbolProperty =
            AvaloniaProperty.Register<Connection, ArrowSymbol>(nameof(SourceArrowSymbol), ArrowSymbol.None);

        /// <summary>
        /// Gets or sets the source arrow symbol.
        /// </summary>
        public ArrowSymbol SourceArrowSymbol
        {
            get => GetValue(SourceArrowSymbolProperty);
            set => SetValue(SourceArrowSymbolProperty, value);
        }

        /// <summary>
        /// Defines the SinkArrowSymbol styled property.
        /// </summary>
        public static readonly StyledProperty<ArrowSymbol> SinkArrowSymbolProperty =
            AvaloniaProperty.Register<Connection, ArrowSymbol>(nameof(SinkArrowSymbol), ArrowSymbol.Arrow);

        /// <summary>
        /// Gets or sets the sink arrow symbol.
        /// </summary>
        public ArrowSymbol SinkArrowSymbol
        {
            get => GetValue(SinkArrowSymbolProperty);
            set => SetValue(SinkArrowSymbolProperty, value);
        }

        /// <summary>
        /// Defines the LabelPosition styled property.
        /// </summary>
        public static readonly StyledProperty<Point> LabelPositionProperty =
            AvaloniaProperty.Register<Connection, Point>(nameof(LabelPosition));

        /// <summary>
        /// Gets or sets the label position.
        /// Specifies a point at half path length.
        /// </summary>
        public Point LabelPosition
        {
            get => GetValue(LabelPositionProperty);
            set => SetValue(LabelPositionProperty, value);
        }

        /// <summary>
        /// Defines the StrokeDashArray styled property.
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<double>> StrokeDashArrayProperty =
            AvaloniaProperty.Register<Connection, AvaloniaList<double>>(nameof(StrokeDashArray));

        /// <summary>
        /// Gets or sets the stroke dash array.
        /// Pattern of dashes and gaps that is used to outline the connection path.
        /// </summary>
        public AvaloniaList<double> StrokeDashArray
        {
            get => GetValue(StrokeDashArrayProperty);
            set => SetValue(StrokeDashArrayProperty, value);
        }

        /// <summary>
        /// Defines the LineStroke styled property.
        /// </summary>
        public static readonly StyledProperty<IBrush> LineStrokeProperty =
            AvaloniaProperty.Register<Connection, IBrush>(nameof(LineStroke), Brushes.Black);

        /// <summary>
        /// Gets or sets the line stroke.
        /// </summary>
        public IBrush LineStroke
        {
            get => GetValue(LineStrokeProperty);
            set => SetValue(LineStrokeProperty, value);
        }

        /// <summary>
        /// Defines the LineStrokeThickness styled property.
        /// </summary>
        public static readonly StyledProperty<double> LineStrokeThicknessProperty =
            AvaloniaProperty.Register<Connection, double>(nameof(LineStrokeThickness), 2.0);

        /// <summary>
        /// Gets or sets the line stroke thickness.
        /// </summary>
        public double LineStrokeThickness
        {
            get => GetValue(LineStrokeThicknessProperty);
            set => SetValue(LineStrokeThicknessProperty, value);
        }

        #endregion

        #region Private Fields

        // keep track of connections that link to this connector
        private bool sourcePropChngReg;
        private bool sinkPropChngReg;

        #endregion

        #region Property Changed Handling

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SourceItemProperty)
            {
                OnSourceItemChanged(change.GetOldValue<TimelineGanttItem>(), change.GetNewValue<TimelineGanttItem>());
            }
            else if (change.Property == SinkItemProperty)
            {
                OnSinkItemChanged(change.GetOldValue<TimelineGanttItem>(), change.GetNewValue<TimelineGanttItem>());
            }
            else if (change.Property == SourceProperty)
            {
                OnSourceChanged(change.GetOldValue<Connector>(), change.GetNewValue<Connector>());
            }
            else if (change.Property == SinkProperty)
            {
                OnSinkChanged(change.GetOldValue<Connector>(), change.GetNewValue<Connector>());
            }
            else if (change.Property == PathGeometryProperty)
            {
                OnPathGeometryChanged(change.GetNewValue<PathGeometry>());
            }
        }

        private void OnSourceItemChanged(TimelineGanttItem oldValue, TimelineGanttItem newValue)
        {
            if (oldValue != null && oldValue is INotifyPropertyChanged oldNotifyProp)
            {
                oldNotifyProp.PropertyChanged -= OnItemPropertyChanged;
                // Note: IsVisibleChanged event handling may need to be implemented differently in AvaloniaUI
                // This depends on how TimelineGanttItem implements visibility change notifications
                Source = null;
            }

            if (newValue != null && newValue is INotifyPropertyChanged newNotifyProp)
            {
                newNotifyProp.PropertyChanged += OnItemPropertyChanged;
                // Note: IsVisibleChanged event handling may need to be implemented differently in AvaloniaUI
                Source = newValue.RightConnector;
            }

            UpdateVisiblity();
        }

        private void OnSinkItemChanged(TimelineGanttItem oldValue, TimelineGanttItem newValue)
        {
            if (oldValue != null && oldValue is INotifyPropertyChanged oldNotifyProp)
            {
                oldNotifyProp.PropertyChanged -= OnItemPropertyChanged;
                // Note: IsVisibleChanged event handling may need to be implemented differently in AvaloniaUI
                Sink = null;
            }

            if (newValue != null && newValue is INotifyPropertyChanged newNotifyProp)
            {
                newNotifyProp.PropertyChanged += OnItemPropertyChanged;
                // Note: IsVisibleChanged event handling may need to be implemented differently in AvaloniaUI
                Sink = newValue.LeftConnector;
            }

            UpdateVisiblity();
        }

        private void OnSourceChanged(Connector oldValue, Connector newValue)
        {
            if (oldValue != null)
            {
                UnregisterSource();
                oldValue.Connections.Remove(this);
            }

            if (newValue != null)
            {
                newValue.Connections.Add(this);
                RegisterSource();
            }

            UpdatePathGeometry();
        }

        private void OnSinkChanged(Connector oldValue, Connector newValue)
        {
            if (oldValue != null)
            {
                UnregisterSink();
                oldValue.Connections.Remove(this);
            }

            if (newValue != null)
            {
                newValue.Connections.Add(this);
                RegisterSink();
            }

            UpdatePathGeometry();
        }

        private void OnPathGeometryChanged(PathGeometry newValue)
        {
            UpdateAnchorPosition();
        }

        #endregion

        #region Event Handlers

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(TimelineGanttItem.IsDisplayAsZero), StringComparison.InvariantCulture))
            {
                UpdateVisiblity();
            }
        }

        private void OnItemIsVisibleChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            UpdateVisiblity();
        }

        void OnConnectorPositionChanged(object sender, PropertyChangedEventArgs e)
        {
            // whenever the 'Position' property of the source or sink Connector 
            // changes we must update the connection path geometry
            if (e.PropertyName.Equals("Position"))
            {
                UpdatePathGeometry();
            }
        }

        private void Connection_Loaded(object sender, RoutedEventArgs e)
        {
            RegisterSource();
            RegisterSink();
        }

        void Connection_Unloaded(object sender, RoutedEventArgs e)
        {
            // do some housekeeping when Connection is unloaded

            // remove event handler
            UnregisterSource();
            UnregisterSink();
        }

        #endregion

        #region Private Methods

        private void UpdateVisiblity()
        {
            if (SourceItem == null 
                || SinkItem == null 
                || !SourceItem.IsVisible
                || !SinkItem.IsVisible)
            {
                IsVisible = false;
            }
            else
            {
                IsVisible = true;
            }
        }

        private void RegisterSource()
        {
            if (!sourcePropChngReg && Source != null && Source is INotifyPropertyChanged notifyProp)
            {
                sourcePropChngReg = true;
                notifyProp.PropertyChanged += OnConnectorPositionChanged;
            }
        }

        private void UnregisterSource()
        {
            if (sourcePropChngReg && Source != null && Source is INotifyPropertyChanged notifyProp)
            {
                sourcePropChngReg = false;
                notifyProp.PropertyChanged -= OnConnectorPositionChanged;
            }
        }

        private void RegisterSink()
        {
            if (!sinkPropChngReg && Sink != null && Sink is INotifyPropertyChanged notifyProp)
            {
                sinkPropChngReg = true;
                notifyProp.PropertyChanged += OnConnectorPositionChanged;
            }
        }

        private void UnregisterSink()
        {
            if (sinkPropChngReg && Sink != null && Sink is INotifyPropertyChanged notifyProp)
            {
                sinkPropChngReg = false;
                notifyProp.PropertyChanged -= OnConnectorPositionChanged;
            }
        }

        private void UpdatePathGeometry()
        {
            if (Source != null && Sink != null)
            {
                PathGeometry geometry = new PathGeometry();
                PathFigure figure = new PathFigure();
                figure.StartPoint = new Point(Source.Position.X + 5, Source.Position.Y);
                
                // In AvaloniaUI, LineSegment constructor only takes the Point parameter
                figure.Segments.Add(new LineSegment { Point = new Point(Source.Position.X + 10, Source.Position.Y) });
                figure.Segments.Add(new LineSegment { Point = new Point(Sink.Position.X - 10, Sink.Position.Y) });
                figure.Segments.Add(new LineSegment { Point = new Point(Sink.Position.X - 5, Sink.Position.Y) });
                
                geometry.Figures.Add(figure);
                this.PathGeometry = geometry;
            }
        }

        private void UpdateAnchorPosition()
        {
            if (PathGeometry == null)
                return;

            // Note: AvaloniaUI's PathGeometry doesn't have GetPointAtFractionLength method
            // We need to implement a simplified version or use alternative approach
            // For now, we'll use the start and end points of the path

            if (PathGeometry.Figures.Count > 0)
            {
                var figure = PathGeometry.Figures[0];
                var startPoint = figure.StartPoint;
                
                Point endPoint = startPoint;
                if (figure.Segments.Count > 0 && figure.Segments[^1] is LineSegment lastSegment)
                {
                    endPoint = lastSegment.Point;
                }

                // Calculate mid point
                var midPoint = new Point(
                    (startPoint.X + endPoint.X) / 2,
                    (startPoint.Y + endPoint.Y) / 2);

                // Set angles (simplified)
                this.AnchorAngleSource = 180;
                this.AnchorAngleSink = 0;

                // Calculate anchor positions with offsets
                var sourceOffset = new Point(startPoint.X - 5, startPoint.Y);
                var sinkOffset = new Point(endPoint.X + 5, endPoint.Y);

                this.AnchorPositionSource = sourceOffset;
                this.AnchorPositionSink = sinkOffset;
                this.LabelPosition = midPoint;
            }
        }

        #endregion

        #region Overrides

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            return base.ArrangeOverride(arrangeBounds);
        }

        #endregion
    }

    public enum ArrowSymbol
    {
        None,
        Arrow,
        Diamond
    }
}
