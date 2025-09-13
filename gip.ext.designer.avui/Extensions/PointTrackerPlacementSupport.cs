// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Windows;
using System.Windows.Shapes;
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
		public override void Arrange(AdornerPanel panel, UIElement adorner, Size adornedElementSize)
		{
			Point p = new Point(0, 0);
			if (shape is Line)
			{
				var s = shape as Line;
				double x, y;
				
				if (alignment == PlacementAlignment.BottomRight)
				{
					x = s.X2;
					y = s.Y2;
				}
				else
				{
					x = s.X1;
					y = s.Y1;
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
			p = transform.Transform(p);
			
			adorner.Arrange(new Rect(p.X - 3.5, p.Y - 3.5, 7, 7));
		}
	}
}
