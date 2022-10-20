using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace gip.ext.graphics.shapes
{

    /// <summary>
    ///     Provides a base class for ArrowLine and ArrowPolyline.
    ///     This class is abstract.
    /// </summary>
    public abstract class ArrowLineBase : Shape
    {
        protected PathGeometry _PathGeo;
        protected PathFigure _PathfigLine;
        protected PolyLineSegment _PolysegLine;

        protected PathFigure _PathfigHead1;
        protected PolyLineSegment _PolysegHead1;
        protected PathFigure _PathfigHead2;
        protected PolyLineSegment _PolysegHead2;

        /// <summary>
        ///     Identifies the ArrowAngle dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowAngleProperty =
            DependencyProperty.Register("ArrowAngle",
                typeof(double), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(45.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the angle between the two sides of the arrowhead.
        /// </summary>
        public double ArrowAngle
        {
            set { SetValue(ArrowAngleProperty, value); }
            get { return (double)GetValue(ArrowAngleProperty); }
        }

        /// <summary>
        ///     Identifies the ArrowLength dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowLengthProperty =
            DependencyProperty.Register("ArrowLength",
                typeof(double), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(12.0,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the length of the two sides of the arrowhead.
        /// </summary>
        public double ArrowLength
        {
            set { SetValue(ArrowLengthProperty, value); }
            get { return (double)GetValue(ArrowLengthProperty); }
        }

        /// <summary>
        ///     Identifies the ArrowEnds dependency property.
        /// </summary>
        public static readonly DependencyProperty ArrowEndsProperty =
            DependencyProperty.Register("ArrowEnds",
                typeof(ArrowEnds), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(ArrowEnds.None,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the property that determines which ends of the
        ///     line have arrows.
        /// </summary>
        public ArrowEnds ArrowEnds
        {
            set { SetValue(ArrowEndsProperty, value); }
            get { return (ArrowEnds)GetValue(ArrowEndsProperty); }
        }

        /// <summary>
        ///     Identifies the IsArrowClosed dependency property.
        /// </summary>
        public static readonly DependencyProperty IsArrowClosedProperty =
            DependencyProperty.Register("IsArrowClosed",
                typeof(bool), typeof(ArrowLineBase),
                new FrameworkPropertyMetadata(false,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets the property that determines if the arrow head
        ///     is closed to resemble a triangle.
        /// </summary>
        public bool IsArrowClosed

        {
            set { SetValue(IsArrowClosedProperty, value); }
            get { return (bool)GetValue(IsArrowClosedProperty); }
        }

        /// <summary>
        ///     Initializes a new instance of ArrowLineBase.
        /// </summary>
        public ArrowLineBase()
        {
            _PathGeo = new PathGeometry();

            _PathfigLine = new PathFigure();
            _PolysegLine = new PolyLineSegment();
            _PathfigLine.Segments.Add(_PolysegLine);

            _PathfigHead1 = new PathFigure();
            _PolysegHead1 = new PolyLineSegment();
            _PathfigHead1.Segments.Add(_PolysegHead1);

            _PathfigHead2 = new PathFigure();
            _PolysegHead2 = new PolyLineSegment();
            _PathfigHead2.Segments.Add(_PolysegHead2);
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowLine.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                return ArrowLineBase.GetPathGeometry(_PathGeo, _PathfigLine, _PolysegLine,
                                      _PathfigHead1, _PolysegHead1,
                                      _PathfigHead2, _PolysegHead2,
                                      ArrowEnds, ArrowLength, ArrowAngle, IsArrowClosed);
            }
        }

        private PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2)
        {
            return ArrowLineBase.CalculateArrow(pathfig, pt1, pt2, ArrowLength, ArrowAngle, IsArrowClosed);
        }

        public static PathFigure CalculateArrow(PathFigure pathfig, Point pt1, Point pt2, double arrowLength, double arrowAngle, bool isArrowClosed)
        {
            Matrix matx = new Matrix();
            Vector vect = pt1 - pt2;
            vect.Normalize();
            vect *= arrowLength;

            PolyLineSegment polyseg = pathfig.Segments[0] as PolyLineSegment;
            polyseg.Points.Clear();
            matx.Rotate(arrowAngle / 2);
            pathfig.StartPoint = pt2 + vect * matx;
            polyseg.Points.Add(pt2);

            matx.Rotate(-arrowAngle);
            polyseg.Points.Add(pt2 + vect * matx);
            pathfig.IsClosed = isArrowClosed;
            pathfig.IsFilled = isArrowClosed;

            return pathfig;
        }

        public static Geometry GetPathGeometry(PathGeometry pathgeo, PathFigure pathfigLine, PolyLineSegment polysegLine,
            PathFigure pathfigHead1, PolyLineSegment polysegHead1,
            PathFigure pathfigHead2, PolyLineSegment polysegHead2,
            ArrowEnds arrowEnds, double arrowLength, double arrowAngle, bool isArrowClosed)
        {
            int count = polysegLine.Points.Count;

            if (count > 0)
            {
                // Draw the arrow at the start of the line.
                if ((arrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
                {
                    Point pt1 = pathfigLine.StartPoint;
                    Point pt2 = polysegLine.Points[0];
                    pathgeo.Figures.Add(CalculateArrow(pathfigHead1, pt2, pt1, arrowLength, arrowAngle, isArrowClosed));
                }

                // Draw the arrow at the end of the line.
                if ((arrowEnds & ArrowEnds.End) == ArrowEnds.End)
                {
                    Point pt1 = count == 1 ? pathfigLine.StartPoint :
                                             polysegLine.Points[count - 2];
                    Point pt2 = polysegLine.Points[count - 1];
                    pathgeo.Figures.Add(CalculateArrow(pathfigHead2, pt1, pt2, arrowLength, arrowAngle, isArrowClosed));
                }
            }
            return pathgeo;
        }

    }
}
