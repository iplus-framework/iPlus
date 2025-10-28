using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// A pie piece shape.
    /// </summary>
    public class VBPiePiece : Canvas
    {
        #region events

        public delegate void PiePieceClicked(VBPiePiece sender);

        public event PiePieceClicked PiePieceClickedEvent;

        #endregion

        #region StyledProperties

        /// <summary>
        /// Represents the styled property for Radius.
        /// </summary>
        public static readonly StyledProperty<double> RadiusProperty =
           AvaloniaProperty.Register<VBPiePiece, double>(nameof(Radius));

        /// <summary>
        /// The radius of this pie piece
        /// </summary>
        [Category("VBControl")]
        public double Radius
        {
            get { return GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for Fill.
        /// </summary>
        public static readonly StyledProperty<IBrush> FillProperty =
           AvaloniaProperty.Register<VBPiePiece, IBrush>(nameof(Fill));

        /// <summary>
        /// The fill color of this pie piece
        /// </summary>
        [Category("VBControl")]
        public IBrush Fill
        {
            get { return GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for Stroke.
        /// </summary>
        public static readonly StyledProperty<IBrush> StrokeProperty =
           AvaloniaProperty.Register<VBPiePiece, IBrush>(nameof(Stroke));

        /// <summary>
        /// The fill color of this pie piece
        /// </summary>
        [Category("VBControl")]
        public IBrush Stroke
        {
            get { return GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for InnerRadius.
        /// </summary>
        public static readonly StyledProperty<double> InnerRadiusProperty =
           AvaloniaProperty.Register<VBPiePiece, double>(nameof(InnerRadius));

        /// <summary>
        /// The inner radius of this pie piece
        /// </summary>
        [Category("VBControl")]
        public double InnerRadius
        {
            get { return GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for WedgeAngle.
        /// </summary>
        public static readonly StyledProperty<double> WedgeAngleProperty =
           AvaloniaProperty.Register<VBPiePiece, double>(nameof(WedgeAngle));

        /// <summary>
        /// The wedge angle of this pie piece in degrees
        /// </summary>
        [Category("VBControl")]
        public double WedgeAngle
        {
            get { return GetValue(WedgeAngleProperty); }
            set { SetValue(WedgeAngleProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for RotationAngle.
        /// </summary>
        public static readonly StyledProperty<double> RotationAngleProperty =
           AvaloniaProperty.Register<VBPiePiece, double>(nameof(RotationAngle));

        /// <summary>
        /// The rotation, in degrees, from the Y axis vector of this pie piece.
        /// </summary>
        [Category("VBControl")]
        public double RotationAngle
        {
            get { return GetValue(RotationAngleProperty); }
            set { SetValue(RotationAngleProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for CentreY.
        /// </summary>
        public static readonly StyledProperty<double> CentreYProperty =
           AvaloniaProperty.Register<VBPiePiece, double>(nameof(CentreY));

        /// <summary>
        /// The Y coordinate of centre of the circle from which this pie piece is cut.
        /// </summary>
        [Category("VBControl")]
        public double CentreY
        {
            get { return GetValue(CentreYProperty); }
            set { SetValue(CentreYProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for CentreX.
        /// </summary>
        public static readonly StyledProperty<double> CentreXProperty =
           AvaloniaProperty.Register<VBPiePiece, double>(nameof(CentreX));

        /// <summary>
        /// The X coordinate of centre of the circle from which this pie piece is cut.
        /// </summary>
        [Category("VBControl")]
        public double CentreX
        {
            get { return GetValue(CentreXProperty); }
            set { SetValue(CentreXProperty, value); }
        }

        #endregion

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == RadiusProperty ||
                change.Property == FillProperty ||
                change.Property == StrokeProperty ||
                change.Property == InnerRadiusProperty ||
                change.Property == WedgeAngleProperty ||
                change.Property == RotationAngleProperty ||
                change.Property == CentreXProperty ||
                change.Property == CentreYProperty)
            {
                AddPathToCanvas();
            }
            base.OnPropertyChanged(change);
        }

        /// <summary>
        /// Creates a new instance of VBPiePiece.
        /// </summary>
        public VBPiePiece()
        {
            AddPathToCanvas();
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left && e.Route == RoutingStrategies.Bubble)
            {
                if (PiePieceClickedEvent != null)
                {
                    PiePieceClickedEvent(this);
                }
            }
            base.OnPointerReleased(e);
        }

        private void AddPathToCanvas()
        {
            this.Children.Clear();

            String tooltip = (WedgeAngle / 360.00).ToString("#0.##%");

            Path path = ConstructPath();
            ToolTip.SetTip(path, tooltip);
            this.Children.Add(path);
        }

        /// <summary>
        /// Constructs a path that represents this pie segment
        /// </summary>
        /// <returns></returns>
        private Path ConstructPath()
        {
            if (WedgeAngle >= 360)
            {
                Path path = new Path()
                {
                    Fill = this.Fill,
                    Stroke = this.Stroke,
                    StrokeThickness = 1,
                    Data = new GeometryGroup()
                    {
                        FillRule = FillRule.EvenOdd,
                        Children = new GeometryCollection()
                        {
                          new EllipseGeometry()
                          {
                            Center = new Point(CentreX, CentreY),
                            RadiusX = Radius,
                            RadiusY = Radius
                          },
                          new EllipseGeometry()
                          {
                            Center = new Point(CentreX, CentreY),
                            RadiusX = InnerRadius,
                            RadiusY = InnerRadius
                          }
                        },
                    }
                };
                return path;
            }



            Point startPoint = new Point(CentreX, CentreY);

            Point innerArcStartPoint = VBUtils.ComputeCartesianCoordinate(RotationAngle, InnerRadius).VBOffset(CentreX, CentreY);
            Point innerArcEndPoint = VBUtils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle, InnerRadius).VBOffset(CentreX, CentreY);
            Point outerArcStartPoint = VBUtils.ComputeCartesianCoordinate(RotationAngle, Radius).VBOffset(CentreX, CentreY);
            Point outerArcEndPoint = VBUtils.ComputeCartesianCoordinate(RotationAngle + WedgeAngle, Radius).VBOffset(CentreX, CentreY);

            bool largeArc = WedgeAngle > 180.0;
            Size outerArcSize = new Size(Radius, Radius);
            Size innerArcSize = new Size(InnerRadius, InnerRadius);

            PathFigure figure = new PathFigure()
            {
                StartPoint = innerArcStartPoint,
                Segments = new PathSegments()
              {
                  new LineSegment()
                  {
                      Point = outerArcStartPoint
                  },
                  new ArcSegment()
                  {
                      Point = outerArcEndPoint,
                      Size = outerArcSize,
                      IsLargeArc = largeArc,
                      SweepDirection = SweepDirection.Clockwise,
                      RotationAngle = 0
                  },
                  new LineSegment()
                  {
                      Point = innerArcEndPoint
                  },
                  new ArcSegment()
                  {
                      Point = innerArcStartPoint,
                      Size = innerArcSize,
                      IsLargeArc = largeArc,
                      SweepDirection = SweepDirection.CounterClockwise,
                      RotationAngle = 0
                  }
              }
            };

            return new Path()
            {
                Fill = this.Fill,
                Stroke = this.Stroke,
                StrokeThickness = 1,
                Data = new PathGeometry()
                {
                    Figures = new PathFigures()
                    {
                        figure
                    }
                }
            };
        }
    }
}
