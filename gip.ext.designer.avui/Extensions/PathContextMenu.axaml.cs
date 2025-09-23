// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using gip.ext.design.avui;
using gip.ext.designer.avui.themes;

namespace gip.ext.designer.avui.Extensions
{
    public partial class PathContextMenu : ContextMenu
    {
        public PathContextMenu() : base()
        {
        }

        private DesignItem designItem;

        public PathContextMenu(DesignItem designItem)
        {
            this.designItem = designItem;

            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void Click_ConvertToFigures(object sender, RoutedEventArgs e)
        {
            var path = this.designItem.Component as Path;

            if (path.Data is StreamGeometry)
            {
                var sg = path.Data as StreamGeometry;

                // In Avalonia, StreamGeometry doesn't have GetFlattenedPathGeometry method
                // We need to work with the StreamGeometry directly or convert differently
                var pgDes = designItem.Services.Component.RegisterComponentForDesigner(sg);
                designItem.Properties[Path.DataProperty].SetValue(pgDes);
            }
            else if (path.Data is PathGeometry)
            {
                var pg = path.Data as PathGeometry;

                var figs = pg.Figures;

                var newPg = new PathGeometry();
                var newPgDes = designItem.Services.Component.RegisterComponentForDesigner(newPg);

                foreach (var fig in figs)
                {
                    newPgDes.Properties[PathGeometry.FiguresProperty].CollectionElements.Add(FigureToDesignItem(fig));
                }

                designItem.Properties[Path.DataProperty].SetValue(newPg);
            }

        }

        private DesignItem FigureToDesignItem(PathFigure pf)
        {
            var pfDes = designItem.Services.Component.RegisterComponentForDesigner(new PathFigure());

            pfDes.Properties[PathFigure.StartPointProperty].SetValue(pf.StartPoint);
            pfDes.Properties[PathFigure.IsClosedProperty].SetValue(pf.IsClosed);

            foreach (var s in pf.Segments)
            {
                pfDes.Properties[PathFigure.SegmentsProperty].CollectionElements.Add(SegmentToDesignItem(s));
            }
            return pfDes;
        }

        private DesignItem SegmentToDesignItem(PathSegment s)
        {
            // Create a new instance instead of cloning since Avalonia PathSegment doesn't have Clone method
            PathSegment newSegment = null;

            if (s is LineSegment lineSegment)
            {
                newSegment = new LineSegment { Point = lineSegment.Point };
            }
            else if (s is QuadraticBezierSegment quadSegment)
            {
                newSegment = new QuadraticBezierSegment 
                { 
                    Point1 = quadSegment.Point1, 
                    Point2 = quadSegment.Point2 
                };
            }
            else if (s is BezierSegment bezierSegment)
            {
                newSegment = new BezierSegment 
                { 
                    Point1 = bezierSegment.Point1, 
                    Point2 = bezierSegment.Point2, 
                    Point3 = bezierSegment.Point3 
                };
            }
            else if (s is ArcSegment arcSegment)
            {
                newSegment = new ArcSegment 
                { 
                    Point = arcSegment.Point, 
                    IsLargeArc = arcSegment.IsLargeArc, 
                    RotationAngle = arcSegment.RotationAngle, 
                    Size = arcSegment.Size, 
                    SweepDirection = arcSegment.SweepDirection 
                };
            }
            else if (s is PolyLineSegment polyLineSegment)
            {
                newSegment = new PolyLineSegment { Points = polyLineSegment.Points };
            }
            else if (s is PolyBezierSegment polyBezierSegment)
            {
                newSegment = new PolyBezierSegment { Points = polyBezierSegment.Points };
            }

            if (newSegment == null)
                return null;

            var sDes = designItem.Services.Component.RegisterComponentForDesigner(newSegment);

            // Set IsStroked property if available
            if (!s.IsStroked)
                sDes.Properties[PathSegment.IsStrokedProperty].SetValue(s.IsStroked);

            // Note: IsSmoothJoin property doesn't exist in Avalonia PathSegment
            // This property was specific to WPF and has been removed

            return sDes;
        }
    }
}

