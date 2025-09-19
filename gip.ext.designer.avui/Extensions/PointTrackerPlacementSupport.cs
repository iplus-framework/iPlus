// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using gip.ext.design.avui;
using gip.ext.design.avui.Adorners;

namespace gip.ext.designer.avui.Extensions
{
	public class PointTrackerPlacementSupport : AdornerPlacement
	{
		private Shape shape;
		private PlacementAlignment alignment;

		public int Index
		{
			get; set;
		}
		
		public PointTrackerPlacementSupport(Shape s, PlacementAlignment align, int index)
		{
			shape = s;
			alignment = align;
			Index = index;
		}

		/// <summary>
		/// Arranges the adorner element on the specified adorner panel.
		/// </summary>
		public override void Arrange(AdornerPanel panel, Control adorner, Size adornedElementSize)
		{
			Point p = new Point(0, 0);
			if (shape is Line)
			{
				var s = shape as Line;
				double x, y;
				
				if (alignment == PlacementAlignment.BottomRight)
				{
					x = s.EndPoint.X;
					y = s.EndPoint.X;
				}
				else
				{
					x = s.StartPoint.X;
					y = s.StartPoint.Y;
				}
				p = new Point(x, y);
			} else if (shape is Polygon) {
				var pg = shape as Polygon;
				p = pg.Points[Index];
			} else if (shape is Polyline) {
				var pg = shape as Polyline;
				p = pg.Points[Index];
			}

			var transform = shape.RenderedGeometry.Transform;
            if (transform != null)
                p = p.Transform(transform.Value);
			adorner.Arrange(new Rect(p.X - 3.5, p.Y - 3.5, 7, 7));
		}
	}
}
