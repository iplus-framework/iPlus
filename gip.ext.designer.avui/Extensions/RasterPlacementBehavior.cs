// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls.Shapes;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.Collections;

namespace gip.ext.designer.avui.Extensions
{
	public class RasterPlacementBehavior : DefaultPlacementBehavior
	{
		Canvas surface;
		AdornerPanel adornerPanel;
		bool rasterDrawn = false;
		int raster = 5;

		public override void BeginPlacement(PlacementOperation operation)
		{
			base.BeginPlacement(operation);
			
			DesignPanel designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
			if (designPanel != null)
				raster = designPanel.RasterWidth;
			
			CreateSurface(operation);
		}

		public override void EndPlacement(PlacementOperation operation)
		{
			base.EndPlacement(operation);
			DeleteSurface();
		}

		public override void EnterContainer(PlacementOperation operation)
		{
			base.EnterContainer(operation);
			CreateSurface(operation);
		}

		public override void LeaveContainer(PlacementOperation operation)
		{
			base.LeaveContainer(operation);
			DeleteSurface();
		}

		void CreateSurface(PlacementOperation operation)
		{
			if (ExtendedItem.Services.GetService<IDesignPanel>() != null)
			{
				surface = new Canvas();
				adornerPanel = new AdornerPanel();
				adornerPanel.SetAdornedElement(ExtendedItem.View, ExtendedItem);
				AdornerPanel.SetPlacement(surface, AdornerPlacement.FillContent);
				adornerPanel.Children.Add(surface);
				ExtendedItem.Services.DesignPanel.Adorners.Add(adornerPanel);
			}
		}

		void DeleteSurface()
		{
			rasterDrawn = false;
			if (surface != null)
			{
				ExtendedItem.Services.DesignPanel.Adorners.Remove(adornerPanel);
				adornerPanel = null;
				surface = null;
			}
		}

		public override void BeforeSetPosition(PlacementOperation operation)
		{
			base.BeforeSetPosition(operation);
			if (surface == null) return;

			DesignPanel designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
			if (designPanel == null || !designPanel.UseRasterPlacement)
				return;

			if (IsKeyDown(Key.LeftCtrl))
			{
				surface.Children.Clear();
				rasterDrawn = false;
				return;
			}
			
			drawRaster();

			var bounds = operation.PlacedItems[0].Bounds;
			double y = ((int)bounds.Y/raster)*raster;
			double x = ((int)bounds.X/raster)*raster;
            double width = Convert.ToInt32((bounds.Width/raster))*raster;
            double height = Convert.ToInt32((bounds.Height/raster))*raster;
			bounds = new Rect(x, y, width, height);
            operation.PlacedItems[0].Bounds = bounds;
		}

		public override Point PlacePoint(Point point)
		{
			if (surface == null)
				return base.PlacePoint(point);

			DesignPanel designPanel = ExtendedItem.Services.DesignPanel as DesignPanel;
			if (designPanel == null || !designPanel.UseRasterPlacement)
				return base.PlacePoint(point);

			if (IsKeyDown(Key.LeftCtrl))
			{
				surface.Children.Clear();
				rasterDrawn = false;
				return base.PlacePoint(point);
			}

			drawRaster();

			point = new Point(((int)point.X / raster) * raster, ((int)point.Y / raster) * raster);

            return point;
		}

		private void drawRaster()
		{
			if (!rasterDrawn)
			{
				rasterDrawn = true;

				var w = ModelTools.GetWidth(ExtendedItem.View);
				var h = ModelTools.GetHeight(ExtendedItem.View);
				var dash = new AvaloniaList<double> {1, raster - 1};
				for (int i = 0; i <= h; i += raster)
				{
					var line = new Line()
					{
						StartPoint = new Point(0, i),
						EndPoint = new Point(w, i),
                        StrokeThickness = 1,
						Stroke = Brushes.Black,
						StrokeDashArray = dash,
					};
					surface.Children.Add(line);
				}
			}
		}
	}
}
