// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Threading;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Input;
using gip.ext.designer.avui.Controls;

namespace gip.ext.designer.avui.Extensions
{
	[ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
	[ExtensionFor(typeof(FrameworkElement))]
	public class RenderTransformOriginExtension : SelectionAdornerProvider
	{
		readonly AdornerPanel adornerPanel;
		RenderTransformOriginThumb renderTransformOriginThumb;

//		IPlacementBehavior resizeBehavior;
//		PlacementOperation operation;
//		ChangeGroup changeGroup;
		
		public RenderTransformOriginExtension()
		{
			adornerPanel = new AdornerPanel();
			adornerPanel.Order = AdornerOrder.Foreground;
			this.Adorners.Add(adornerPanel);
			
			CreateRenderTransformOriginThumb();
		}
		
		void CreateRenderTransformOriginThumb()
		{
			renderTransformOriginThumb = new RenderTransformOriginThumb();
			renderTransformOriginThumb.Cursor = Cursors.Hand;
			
			AdornerPanel.SetPlacement(renderTransformOriginThumb,
			                          new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top){XRelativeToContentWidth = renderTransformOrigin.X, YRelativeToContentHeight = renderTransformOrigin.Y});
			adornerPanel.Children.Add(renderTransformOriginThumb);

			renderTransformOriginThumb.DragDelta += new DragDeltaEventHandler(renderTransformOriginThumb_DragDelta);
			renderTransformOriginThumb.DragCompleted += new DragCompletedEventHandler(renderTransformOriginThumb_DragCompleted);
		}

		void renderTransformOriginThumb_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			ExtendedItem.Properties.GetProperty(UIElement.RenderTransformOriginProperty).SetValue(new Point(Math.Round(renderTransformOrigin.X, 4), Math.Round(renderTransformOrigin.Y, 4)));
		}
		
		void renderTransformOriginThumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			var p = AdornerPanel.GetPlacement(renderTransformOriginThumb) as RelativePlacement;
			if (p == null) return;
			var pointAbs = adornerPanel.RelativeToAbsolute(new Vector(p.XRelativeToContentWidth, p.YRelativeToContentHeight));
			var pointAbsNew = pointAbs + new Vector(e.HorizontalChange, e.VerticalChange);
			var pRel = adornerPanel.AbsoluteToRelative(pointAbsNew);
			renderTransformOrigin = new Point(pRel.X, pRel.Y);
			
			this.ExtendedItem.View.SetValue(UIElement.RenderTransformOriginProperty, renderTransformOrigin);
			//this.ExtendedItem.Properties.GetProperty(FrameworkElement.RenderTransformOriginProperty).SetValue(new Point(Math.Round(pRel.X, 4), Math.Round(pRel.Y, 4)));
		}
		
		Point renderTransformOrigin = new Point(0.5, 0.5);
		
		DependencyPropertyDescriptor renderTransformOriginPropertyDescriptor;
		
		protected override void OnInitialized()
		{
			base.OnInitialized();
			this.ExtendedItem.PropertyChanged += OnPropertyChanged;
			
			if (this.ExtendedItem.Properties.GetProperty(FrameworkElement.RenderTransformOriginProperty).IsSet) {
				renderTransformOrigin = this.ExtendedItem.Properties.GetProperty(FrameworkElement.RenderTransformOriginProperty).GetConvertedValueOnInstance<Point>();
			}
			
			AdornerPanel.SetPlacement(renderTransformOriginThumb,
			                          new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top){XRelativeToContentWidth = renderTransformOrigin.X, YRelativeToContentHeight = renderTransformOrigin.Y});
			
			renderTransformOriginPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(FrameworkElement.RenderTransformOriginProperty, typeof(FrameworkElement));
			renderTransformOriginPropertyDescriptor.AddValueChanged(this.ExtendedItem.Component, OnRenderTransformOriginPropertyChanged);
		}
		
		private void OnRenderTransformOriginPropertyChanged(object sender, EventArgs e)
		{
			var pRel = renderTransformOrigin;
			if (this.ExtendedItem.Properties.GetProperty(FrameworkElement.RenderTransformOriginProperty).IsSet)
				pRel = this.ExtendedItem.Properties.GetProperty(FrameworkElement.RenderTransformOriginProperty).GetConvertedValueOnInstance<Point>();
						
			AdornerPanel.SetPlacement(renderTransformOriginThumb,
			                          new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top){ XRelativeToContentWidth = pRel.X, YRelativeToContentHeight = pRel.Y });
			
		}
		
		void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{ }
		
		protected override void OnRemove()
		{
			this.ExtendedItem.PropertyChanged -= OnPropertyChanged;
			renderTransformOriginPropertyDescriptor.RemoveValueChanged(this.ExtendedItem.Component, OnRenderTransformOriginPropertyChanged);
			
			base.OnRemove();
		}
	}
}
