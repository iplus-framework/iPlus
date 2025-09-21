// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Controls;
using Avalonia.Reactive;

namespace gip.ext.designer.avui.Extensions
{
	[ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
	[ExtensionFor(typeof(Control))]
	public class RenderTransformOriginExtension : SelectionAdornerProvider
	{
		readonly AdornerPanel adornerPanel;
		RenderTransformOriginThumb renderTransformOriginThumb;
		private IDisposable _renderTransformOriginSubscription;

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
			renderTransformOriginThumb.Cursor = new Cursor(StandardCursorType.Hand);

            AdornerPanel.SetPlacement(renderTransformOriginThumb,
			                          new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top){XRelativeToContentWidth = renderTransformOrigin.X, YRelativeToContentHeight = renderTransformOrigin.Y});
			adornerPanel.Children.Add(renderTransformOriginThumb);

            renderTransformOriginThumb.DragDelta += RenderTransformOriginThumb_DragDelta;
            renderTransformOriginThumb.DragCompleted += RenderTransformOriginThumb_DragCompleted;
		}

        private void RenderTransformOriginThumb_DragCompleted(object sender, VectorEventArgs e)
        {
            ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).SetValue(new Point(Math.Round(renderTransformOrigin.X, 4), Math.Round(renderTransformOrigin.Y, 4)));
        }

        private void RenderTransformOriginThumb_DragDelta(object sender, VectorEventArgs e)
        {
            var p = AdornerPanel.GetPlacement(renderTransformOriginThumb) as RelativePlacement;
            if (p == null) 
				return;
            var pointAbs = adornerPanel.RelativeToAbsolute(new Vector(p.XRelativeToContentWidth, p.YRelativeToContentHeight));
            var pointAbsNew = pointAbs + new Vector(e.Vector.X, e.Vector.Y);
            var pRel = adornerPanel.AbsoluteToRelative(pointAbsNew);
            renderTransformOrigin = new Point(pRel.X, pRel.Y);

            this.ExtendedItem.View.SetValue(Visual.RenderTransformOriginProperty, renderTransformOrigin);
        }

        Point renderTransformOrigin = new Point(0.5, 0.5);
			
		protected override void OnInitialized()
		{
			base.OnInitialized();
			this.ExtendedItem.PropertyChanged += OnPropertyChanged;
			
			if (this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).IsSet) {
				renderTransformOrigin = this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).GetConvertedValueOnInstance<Point>();
			}
			
			AdornerPanel.SetPlacement(renderTransformOriginThumb,
			                          new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top){XRelativeToContentWidth = renderTransformOrigin.X, YRelativeToContentHeight = renderTransformOrigin.Y});

			_renderTransformOriginSubscription = (this.ExtendedItem.Component as Visual)?.GetObservable(Visual.RenderTransformOriginProperty).Subscribe(_ =>
			{
				OnRenderTransformOriginPropertyChanged(this.ExtendedItem.Component, EventArgs.Empty);
			});
		}
		
		private void OnRenderTransformOriginPropertyChanged(object sender, EventArgs e)
		{
			var pRel = renderTransformOrigin;
			if (this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).IsSet)
				pRel = this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).GetConvertedValueOnInstance<Point>();
						
			AdornerPanel.SetPlacement(renderTransformOriginThumb,
			                          new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top){ XRelativeToContentWidth = pRel.X, YRelativeToContentHeight = pRel.Y });
			
		}
		
		void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{ }
		
		protected override void OnRemove()
		{
			this.ExtendedItem.PropertyChanged -= OnPropertyChanged;
            _renderTransformOriginSubscription?.Dispose();

            base.OnRemove();
		}
	}
}
