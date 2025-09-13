// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using gip.ext.designer.avui;
using gip.ext.designer.avui.Controls;

namespace gip.ext.designer.avui.ThumbnailView
{
	public class ThumbnailView : Control, INotifyPropertyChanged
	{
		public DesignSurface DesignSurface
		{
			get { return (DesignSurface)GetValue(DesignSurfaceProperty); }
			set { SetValue(DesignSurfaceProperty, value); }
		}

		public static readonly DependencyProperty DesignSurfaceProperty =
			DependencyProperty.Register("DesignSurface", typeof(DesignSurface), typeof(ThumbnailView), new PropertyMetadata(OnDesignSurfaceChanged));

		private static void OnDesignSurfaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ctl = d as ThumbnailView;
			
			
			if (ctl.oldSurface != null)
				ctl.oldSurface.LayoutUpdated -= ctl.DesignSurface_LayoutUpdated;
			
			ctl.oldSurface = ctl.DesignSurface;
			ctl.scrollViewer = null;

			if (ctl.DesignSurface != null)
			{
				ctl.DesignSurface.LayoutUpdated += ctl.DesignSurface_LayoutUpdated;
			}

			ctl.OnPropertyChanged("ScrollViewer");
		}
		
		static ThumbnailView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ThumbnailView), new FrameworkPropertyMetadata(typeof(ThumbnailView)));
		}

		public ScrollViewer ScrollViewer
		{
			get
			{
				if (DesignSurface != null && scrollViewer == null)
					scrollViewer = DesignSurface.TryFindChild<ZoomControl>();

				return scrollViewer;
			}
		}


		void DesignSurface_LayoutUpdated(object sender, EventArgs e)
		{
			if (this.scrollViewer == null)
				OnPropertyChanged("ScrollViewer");

			if (this.scrollViewer != null)
			{
				double scale, xOffset, yOffset;
				this.InvalidateScale(out scale, out xOffset, out yOffset);

				this.zoomThumb.Width = scrollViewer.ViewportWidth * scale;
				this.zoomThumb.Height = scrollViewer.ViewportHeight * scale;
				Canvas.SetLeft(this.zoomThumb, xOffset + this.ScrollViewer.HorizontalOffset * scale);
				Canvas.SetTop(this.zoomThumb, yOffset + this.ScrollViewer.VerticalOffset * scale);
			}
		}

		private DesignSurface oldSurface;
		private ZoomControl scrollViewer;
		private Canvas zoomCanvas;
		private Thumb zoomThumb;
		
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.zoomThumb = Template.FindName("PART_ZoomThumb", this) as Thumb;
			this.zoomCanvas = Template.FindName("PART_ZoomCanvas", this) as Canvas;
			
			this.zoomThumb.DragDelta += this.Thumb_DragDelta;
			this.zoomCanvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;			
		}

		private void Canvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var pos = e.GetPosition(zoomCanvas);
			var cl = Canvas.GetLeft(this.zoomThumb);
			var ct = Canvas.GetTop(this.zoomThumb);

			double scale, xOffset, yOffset;
			this.InvalidateScale(out scale, out xOffset, out yOffset);
			var dl = pos.X - cl - (zoomThumb.Width / 2);
			var dt = pos.Y - ct - (zoomThumb.Height / 2);

			scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + dl / scale);
			scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + dt / scale);
		}

		private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			if (DesignSurface != null)
			{
				if (scrollViewer != null)
				{
					double scale, xOffset, yOffset;
					this.InvalidateScale(out scale, out xOffset, out yOffset);

					scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + e.HorizontalChange / scale);
					scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + e.VerticalChange / scale);
				}
			}
		}

		private void InvalidateScale(out double scale, out double xOffset, out double yOffset)
		{
			scale = 1;
			xOffset = 0;
			yOffset = 0;
			
			if (this.DesignSurface.DesignContext != null && this.DesignSurface.DesignContext.RootItem != null)
			{
				var designedElement = this.DesignSurface.DesignContext.RootItem.Component as FrameworkElement;

				if (designedElement != null)
				{
					var fac1 = designedElement.DesiredSize.Width / zoomCanvas.ActualWidth;
					var fac2 = designedElement.DesiredSize.Height / zoomCanvas.ActualHeight;

					// zoom canvas size
					double x = this.zoomCanvas.ActualWidth;
					double y = this.zoomCanvas.ActualHeight;

					if (fac1 < fac2)
					{
						x = designedElement.ActualWidth/fac2;
						xOffset = (zoomCanvas.ActualWidth - x)/2;
						yOffset = 0;
					}
					else
					{
						y = designedElement.ActualHeight/fac1;
						xOffset = 0;
						yOffset = (zoomCanvas.ActualHeight - y)/2;
					}

					double w = designedElement.DesiredSize.Width;
					double h = designedElement.DesiredSize.Height;

					double scaleX = x/w;
					double scaleY = y/h;

					scale = (scaleX < scaleY) ? scaleX : scaleY;

					if (scrollViewer.ViewportHeight > h) {
						yOffset -= ((scrollViewer.ViewportHeight - h) / 2) * scale;
					}
					if (scrollViewer.ViewportWidth > w) {
						xOffset -= ((scrollViewer.ViewportWidth - w) / 2) * scale;
					}

					xOffset += (x - scale*w)/2;
					yOffset += (y - scale*h)/2;
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
