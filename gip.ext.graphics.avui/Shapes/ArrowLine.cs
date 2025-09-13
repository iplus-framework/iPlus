using System;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace gip.ext.graphics.avui.shapes
{
    /// <summary>
    ///     Draws a straight line between two points with 
    ///     optional arrows on the ends.
    /// </summary>
    public class ArrowLine : ArrowLineBase
    {
        /// <summary>
        ///     Identifies the X1 dependency property.
        /// </summary>
        public static readonly StyledProperty<double> X1Property = AvaloniaProperty.Register<ArrowLine, double>(nameof(X1));

        /// <summary>
        ///     Gets or sets the x-coordinate of the ArrowLine start point.
        /// </summary>
        public double X1
        {
            set { SetValue(X1Property, value); }
            get { return (double)GetValue(X1Property); }
        }

        /// <summary>
        ///     Identifies the Y1 dependency property.
        /// </summary>
        public static readonly StyledProperty<double> Y1Property = AvaloniaProperty.Register<ArrowLine, double>(nameof(Y1));

        /// <summary>
        ///     Gets or sets the y-coordinate of the ArrowLine start point.
        /// </summary>
        public double Y1
        {
            set { SetValue(Y1Property, value); }
            get { return (double)GetValue(Y1Property); }
        }

        /// <summary>
        ///     Identifies the X2 dependency property.
        /// </summary>
        public static readonly StyledProperty<double> X2Property = AvaloniaProperty.Register<ArrowLine, double>(nameof(X2));

        /// <summary>
        ///     Gets or sets the x-coordinate of the ArrowLine end point.
        /// </summary>
        public double X2
        {
            set { SetValue(X2Property, value); }
            get { return (double)GetValue(X2Property); }
        }

        /// <summary>
        ///     Identifies the Y2 dependency property.
        /// </summary>
        public static readonly StyledProperty<double> Y2Property = AvaloniaProperty.Register<ArrowLine, double>(nameof(Y2));
        /// <summary>
        ///     Gets or sets the y-coordinate of the ArrowLine end point.
        /// </summary>
        public double Y2
        {
            set { SetValue(Y2Property, value); }
            get { return (double)GetValue(Y2Property); }
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowLine.
        /// </summary>
        protected override Geometry CreateDefiningGeometry()
        {
            return ArrowLine.GetPathGeometry(new Point(X1, Y1), new Point(X2, Y2),
                    _PathGeo, _PathfigLine, _PolysegLine,
                    _PathfigHead1, _PolysegHead1,
                    _PathfigHead2, _PolysegHead2,
                    ArrowEnds, ArrowLength, ArrowAngle, IsArrowClosed);
        }

        public static Geometry GetPathGeometry(Point point1, Point point2, 
            ArrowEnds arrowEnds, double arrowLength, double arrowAngle, bool isArrowClosed)
        {
            PathGeometry pathgeo = new PathGeometry();
            PathFigure pathfigLine = new PathFigure();
            PolyLineSegment polysegLine = new PolyLineSegment();
            pathfigLine.Segments.Add(polysegLine);

            PathFigure pathfigHead1 = new PathFigure();
            PolyLineSegment polysegHead1 = new PolyLineSegment();
            pathfigHead1.Segments.Add(polysegHead1);

            PathFigure pathfigHead2 = new PathFigure();
            PolyLineSegment polysegHead2 = new PolyLineSegment();
            pathfigHead2.Segments.Add(polysegHead2);

            return ArrowLine.GetPathGeometry(point1, point2, 
                pathgeo, pathfigLine, polysegLine,
                pathfigHead1, polysegHead1,
                pathfigHead2, polysegHead2,
                arrowEnds, arrowLength, arrowAngle, isArrowClosed);
        }

        public static Geometry GetPathGeometry(Point point1, Point point2, 
            PathGeometry pathgeo, PathFigure pathfigLine, PolyLineSegment polysegLine,
            PathFigure pathfigHead1, PolyLineSegment polysegHead1, 
            PathFigure pathfigHead2, PolyLineSegment polysegHead2,
            ArrowEnds arrowEnds, double arrowLength, double arrowAngle, bool isArrowClosed)
        {
            // Clear out the PathGeometry.
            pathgeo.Figures.Clear();

            // Define a single PathFigure with the points.
            pathfigLine.StartPoint = new Point(point1.X, point1.Y);
            polysegLine.Points.Clear();
            polysegLine.Points.Add(new Point(point2.X, point2.Y));
            pathgeo.Figures.Add(pathfigLine);

            // Call the base property to add arrows on the ends.
            return ArrowLineBase.GetPathGeometry(pathgeo, pathfigLine, polysegLine,
                pathfigHead1, polysegHead1,
                pathfigHead2, polysegHead2,
                arrowEnds, arrowLength, arrowAngle, isArrowClosed);
        }
    }
}
