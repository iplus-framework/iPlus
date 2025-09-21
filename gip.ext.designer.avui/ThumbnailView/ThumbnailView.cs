// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using gip.ext.designer.avui;
using gip.ext.designer.avui.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace gip.ext.designer.avui.ThumbnailView
{
	public class ThumbnailView : TemplatedControl
	{
		public DesignSurface DesignSurface
		{
			get { return GetValue(DesignSurfaceProperty); }
			set { SetValue(DesignSurfaceProperty, value); }
		}

		public static readonly StyledProperty<DesignSurface> DesignSurfaceProperty =
			AvaloniaProperty.Register<ThumbnailView, DesignSurface>(nameof(DesignSurface), null, coerce: OnDesignSurfaceChanged);

		private static DesignSurface OnDesignSurfaceChanged(AvaloniaObject obj, DesignSurface value)
		{
			var ctl = obj as ThumbnailView;
			
			if (ctl.oldSurface != null)
				ctl.oldSurface.LayoutUpdated -= ctl.DesignSurface_LayoutUpdated;
			
			ctl.oldSurface = value;
			ctl.SetValue(ScrollViewerProperty, null);

			if (value != null)
			{
				value.LayoutUpdated += ctl.DesignSurface_LayoutUpdated;
			}

			// Trigger update of ScrollViewer property
			ctl.UpdateScrollViewer();
			return value;
		}

		public ScrollViewer ScrollViewer
		{
			get { return GetValue(ScrollViewerProperty); }
			private set { SetValue(ScrollViewerProperty, value); }
		}

		public static readonly StyledProperty<ScrollViewer> ScrollViewerProperty =
			AvaloniaProperty.Register<ThumbnailView, ScrollViewer>(nameof(ScrollViewer));

		private void UpdateScrollViewer()
		{
			ScrollViewer newScrollViewer = null;
			if (DesignSurface != null)
			{
				// Use the ZoomControl property from DesignSurface
				newScrollViewer = DesignSurface.ZoomControl;
			}
			ScrollViewer = newScrollViewer;
		}

		void DesignSurface_LayoutUpdated(object sender, EventArgs e)
		{
			if (ScrollViewer == null)
				UpdateScrollViewer();

			if (ScrollViewer != null)
			{
				double scale, xOffset, yOffset;
				this.InvalidateScale(out scale, out xOffset, out yOffset);

				if (this.zoomThumb != null && this.ScrollViewer is ZoomControl zoomControl)
				{
					// Use Bounds instead of ViewportWidth/Height
					this.zoomThumb.Width = zoomControl.Bounds.Width * scale;
					this.zoomThumb.Height = zoomControl.Bounds.Height * scale;
					Canvas.SetLeft(this.zoomThumb, xOffset);
					Canvas.SetTop(this.zoomThumb, yOffset);
				}
			}
		}

		private DesignSurface oldSurface;
		private Canvas zoomCanvas;
		private Thumb zoomThumb;
		
		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);

			this.zoomThumb = e.NameScope.Find("PART_ZoomThumb") as Thumb;
			this.zoomCanvas = e.NameScope.Find("PART_ZoomCanvas") as Canvas;
			
			if (this.zoomThumb != null)
				this.zoomThumb.DragDelta += this.Thumb_DragDelta;
			if (this.zoomCanvas != null)
				this.zoomCanvas.PointerPressed += Canvas_PointerPressed;			
		}

		private void Canvas_PointerPressed(object sender, PointerPressedEventArgs e)
		{
			if (!e.GetCurrentPoint(zoomCanvas).Properties.IsLeftButtonPressed)
				return;

			var pos = e.GetPosition(zoomCanvas);
			var cl = Canvas.GetLeft(this.zoomThumb);
			var ct = Canvas.GetTop(this.zoomThumb);

			double scale, xOffset, yOffset;
			this.InvalidateScale(out scale, out xOffset, out yOffset);
			var dl = pos.X - cl - (zoomThumb.Width / 2);
			var dt = pos.Y - ct - (zoomThumb.Height / 2);

			// For now, just update the thumb position
			Canvas.SetLeft(this.zoomThumb, cl + dl);
			Canvas.SetTop(this.zoomThumb, ct + dt);
		}

		private void Thumb_DragDelta(object sender, VectorEventArgs e)
		{
			if (DesignSurface != null)
			{
				double scale, xOffset, yOffset;
				this.InvalidateScale(out scale, out xOffset, out yOffset);

				// Update thumb position
				var currentLeft = Canvas.GetLeft(this.zoomThumb);
				var currentTop = Canvas.GetTop(this.zoomThumb);
				Canvas.SetLeft(this.zoomThumb, currentLeft + e.Vector.X);
				Canvas.SetTop(this.zoomThumb, currentTop + e.Vector.Y);
			}
		}

		private void InvalidateScale(out double scale, out double xOffset, out double yOffset)
		{
			scale = 1;
			xOffset = 0;
			yOffset = 0;
			
			if (this.DesignSurface?.DesignContext?.RootItem != null)
			{
				var designedElement = this.DesignSurface.DesignContext.RootItem.Component as Control;

				if (designedElement != null && zoomCanvas != null)
				{
					var fac1 = designedElement.DesiredSize.Width / zoomCanvas.Bounds.Width;
					var fac2 = designedElement.DesiredSize.Height / zoomCanvas.Bounds.Height;

					// zoom canvas size
					double x = this.zoomCanvas.Bounds.Width;
					double y = this.zoomCanvas.Bounds.Height;

					if (fac1 < fac2)
					{
						x = designedElement.Bounds.Width/fac2;
						xOffset = (zoomCanvas.Bounds.Width - x)/2;
						yOffset = 0;
					}
					else
					{
						y = designedElement.Bounds.Height/fac1;
						xOffset = 0;
						yOffset = (zoomCanvas.Bounds.Height - y)/2;
					}

					double w = designedElement.DesiredSize.Width;
					double h = designedElement.DesiredSize.Height;

					double scaleX = x/w;
					double scaleY = y/h;

					scale = (scaleX < scaleY) ? scaleX : scaleY;

					xOffset += (x - scale*w)/2;
					yOffset += (y - scale*h)/2;
				}
			}
		}
	}
}
