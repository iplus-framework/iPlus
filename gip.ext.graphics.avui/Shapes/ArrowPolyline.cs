using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace gip.ext.graphics.avui.shapes
{
    /// <summary>
    ///     Draws a series of connected straight lines with
    ///     optional arrows on the ends.
    /// </summary>
    public class ArrowPolyline : ArrowLineBase
    {
        /// <summary>
        ///     Identifies the Points dependency property.
        /// </summary>
        public static readonly DependencyProperty PointsProperty =
            DependencyProperty.Register("Points",
                typeof(PointCollection), typeof(ArrowPolyline),
                new FrameworkPropertyMetadata(null,
                        FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        ///     Gets or sets a collection that contains the 
        ///     vertex points of the ArrowPolyline.
        /// </summary>
        public virtual PointCollection Points
        {
            set { SetValue(PointsProperty, value); }
            get { return (PointCollection)GetValue(PointsProperty); }
        }

        /// <summary>
        ///     Initializes a new instance of the ArrowPolyline class. 
        /// </summary>
        public ArrowPolyline()
        {
            Points = new PointCollection();
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the ArrowPolyline.
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                //// Clear out the PathGeometry.
                //_PathGeo.Figures.Clear();

                //// Try to avoid unnecessary indexing exceptions.
                //if (Points.Count > 0)
                //{
                //    // Define a PathFigure containing the points.
                //    _PathfigLine.StartPoint = Points[0];
                //    _PolysegLine.Points.Clear();

                //    for (int i = 1; i < Points.Count; i++)
                //        _PolysegLine.Points.Add(Points[i]);

                //    _PathGeo.Figures.Add(_PathfigLine);
                //}

                //// Call the base property to add arrows on the ends.
                //return base.DefiningGeometry;

                return ArrowPolyline.GetPathGeometry(this.Points,
                                       _PathGeo, _PathfigLine, _PolysegLine,
                                       _PathfigHead1, _PolysegHead1,
                                       _PathfigHead2, _PolysegHead2,
                                       ArrowEnds, ArrowLength, ArrowAngle, IsArrowClosed);
            }
        }

        public static Geometry GetPathGeometry(PointCollection points,
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

            return ArrowPolyline.GetPathGeometry(points,
                pathgeo, pathfigLine, polysegLine,
                pathfigHead1, polysegHead1,
                pathfigHead2, polysegHead2,
                arrowEnds, arrowLength, arrowAngle, isArrowClosed);
        }

        public static Geometry GetPathGeometry(PointCollection points,
            PathGeometry pathgeo, PathFigure pathfigLine, PolyLineSegment polysegLine,
            PathFigure pathfigHead1, PolyLineSegment polysegHead1,
            PathFigure pathfigHead2, PolyLineSegment polysegHead2,
            ArrowEnds arrowEnds, double arrowLength, double arrowAngle, bool isArrowClosed)
        {
            // Clear out the PathGeometry.
            pathgeo.Figures.Clear();

            if (points == null)
                return null;

            // Try to avoid unnecessary indexing exceptions.
            if (points.Count > 0)
            {
                // Define a PathFigure containing the points.
                pathfigLine.StartPoint = points[0];
                polysegLine.Points.Clear();

                for (int i = 1; i < points.Count; i++)
                    polysegLine.Points.Add(points[i]);

                pathgeo.Figures.Add(pathfigLine);
            }

            // Call the base property to add arrows on the ends.
            return ArrowLineBase.GetPathGeometry(pathgeo, pathfigLine, polysegLine,
                pathfigHead1, polysegHead1,
                pathfigHead2, polysegHead2,
                arrowEnds, arrowLength, arrowAngle, isArrowClosed);
        }

        /// <summary>
        /// Get all points from line.
        /// </summary>
        /// <param name="p0">The start line point.</param>
        /// <param name="p1">The end line point.</param>
        /// <returns>Return all points from line.</returns>
        public static PointCollection GetBresenhamLine(Point p0, Point p1)
        {
            long x0 = Convert.ToInt64(p0.X);
            long x1 = Convert.ToInt64(p1.X);
            long y0 = Convert.ToInt64(p0.Y);
            long y1 = Convert.ToInt64(p1.Y);
            long dx = Math.Abs(x1 - x0);
            long dy = Math.Abs(y1 - y0);

            long sx = x0 < x1 ? 1 : -1;
            long sy = y0 < y1 ? 1 : -1;

            long err = dx - dy;

            var points = new PointCollection();

            while (true)
            {
                points.Add(new Point(x0, y0));
                if (x0 == x1 && y0 == y1) break;

                long e2 = 2 * err;
                if (e2 > -dy)
                {
                    err = err - dy;
                    x0 = x0 + sx;
                }
                if (e2 < dx)
                {
                    err = err + dx;
                    y0 = y0 + sy;
                }
            }

            return points;
        }

        /// <summary>
        /// Insert point in ArrowPolyline points.
        /// </summary>
        /// <param name="insertPoint">Point for insertion.</param>
        public void InsertPoint(Point insertPoint)
        {
            int insertIndex = 0;
            foreach (Point point in this.Points)
            {
                int pointIndex = this.Points.IndexOf(point);
                if (pointIndex + 1 < this.Points.Count)
                {
                    Point nextPoint = this.Points[pointIndex + 1];

                    if ((Math.Abs(insertPoint.X - point.X) < 10 && Math.Abs(insertPoint.Y - point.Y) < 10) ||
                        (Math.Abs(insertPoint.X - nextPoint.X) < 10 && Math.Abs(insertPoint.Y - nextPoint.Y) < 10))
                        break;

                    foreach (Point linePoint in GetBresenhamLine(point, nextPoint))
                    {
                        double diffX = Math.Round(linePoint.X, 0) - Math.Round(insertPoint.X, 0);
                        double diffY = Math.Round(linePoint.Y, 0) - Math.Round(insertPoint.Y, 0);
                        if (Math.Abs(diffX) < 4 && Math.Abs(diffY) < 4)
                        {
                            insertIndex = pointIndex + 1;
                            break;
                        }
                    }
                }
            }
            if (insertIndex > 0)
            {
                this.Points.Insert(insertIndex, insertPoint);
                this.UpdateLayout();
            }
        }
    }
}
