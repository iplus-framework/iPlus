// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using gip.ext.designer.avui.Services;
using System.Windows.Media;
using System.Windows.Shapes;
using gip.ext.design.avui;
using gip.ext.graphics.avui.shapes;
using gip.ext.designer.avui.Xaml;

namespace gip.ext.designer.avui.Controls
{
    [CLSCompliant(false)]
    public abstract class DrawPolyAdornerBase : DrawShapesAdornerBase
    {
        #region c'tors
        public DrawPolyAdornerBase(Point startPointRelativeToShapeContainer, DesignItem containerOfStartPoint, DesignItem containerForShape)
            : base(startPointRelativeToShapeContainer, containerOfStartPoint, containerForShape)
        {
        }
        #endregion

        protected Nullable<Point> _lastStartPoint;
        protected Nullable<Point> _lastEndPoint;
        protected PointCollection _PointCollection = new PointCollection();

        protected PointCollection PointCollection
        {
            get
            {
                PointCollection pointCollection = new PointCollection();
                if (_lastStartPoint.HasValue)
                {
                    bool firstPointDifferent = true;
                    if (_PointCollection.Count > 0)
                    {
                        Point pLast = _PointCollection.Last();
                        if (pLast.Equals(_lastStartPoint.Value))
                            firstPointDifferent = false;
                    }
                    if (firstPointDifferent)
                        pointCollection.Add(_lastStartPoint.Value);
                }
                foreach (Point p in _PointCollection)
                {
                    pointCollection.Add(p);
                }
                if (_lastEndPoint.HasValue)
                {
                    bool lastPointDifferent = true;
                    if (_PointCollection.Count > 0)
                    {
                        Point pLast = _PointCollection.Last();
                        if (pLast.Equals(_lastEndPoint.Value))
                            lastPointDifferent = false;
                    }
                    if (lastPointDifferent)
                        pointCollection.Add(_lastEndPoint.Value);
                }
                return pointCollection;
            }
        }

        protected PointCollection PointCollectionRelativeToBounds
        {
            get
            {
                Point startPoint;
                Point endPoint;
                return TranslatePointsToBounds(PointCollection, out startPoint, out endPoint);
            }
        }

        public override void OnIntermediateClick(Point pointRelativeToPathContainer, DependencyObject hitObject, MouseEventArgs e)
        {
            _PointCollection.Add(pointRelativeToPathContainer);
            //System.Diagnostics.Debug.WriteLine("OnIntermediateClick: " + pointRelativeToPathContainer.ToString());
        }

        public static Geometry GetGeometryForPoly(PointCollection points)
        {
            PathGeometry pathgeo = new PathGeometry();
            PathFigure pathfigLine = new PathFigure();
            PolyLineSegment polysegLine = new PolyLineSegment();
            pathfigLine.Segments.Add(polysegLine);

            // Clear out the PathGeometry.
            pathgeo.Figures.Clear();

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
            return pathgeo;
        }

    }
}
