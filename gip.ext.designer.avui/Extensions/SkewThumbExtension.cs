// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui.Extensions;
using gip.ext.design.avui;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using AdornerLayer = gip.ext.designer.avui.Controls.AdornerLayer;
using Avalonia.Input;

namespace gip.ext.designer.avui.Extensions
{
	[ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
	[ExtensionFor(typeof(Control))]
	public sealed class SkewThumbExtension : SelectionAdornerProvider
	{
		readonly AdornerPanel adornerPanel;
		readonly DesignItem[] extendedItemArray = new DesignItem[1];
		
		private AdornerLayer _adornerLayer;
		
		public SkewThumbExtension()
		{
			adornerPanel = new AdornerPanel();
			adornerPanel.Order = AdornerOrder.BeforeForeground;
			this.Adorners.Add(adornerPanel);
		}
		
		#region Skew
		
		private Point startPoint;
		private Control parent;
		private SkewTransform skewTransform;
		private double skewX;
		private double skewY;
		private DesignItem rtTransform;
		private Thumb thumb1;
		private Thumb thumb2;
		PlacementOperation operation;
		
		private void dragX_Started(DragListener drag)
		{
			if (drag == null || drag.LastEventArgs == null)
				return;
            _adornerLayer = this.adornerPanel.TryFindParent<AdornerLayer>();
			
			var designerItem = this.ExtendedItem.Component as Control;
			this.parent = VisualTreeHelper.GetParent(designerItem) as Control;
			
			startPoint = drag.LastEventArgs.GetPosition(this.parent);
			
			if (this.skewTransform == null)
			{
				this.skewX = 0;
				this.skewY = 0;
			}
			else
			{
				this.skewX = this.skewTransform.AngleX;
				this.skewY = this.skewTransform.AngleY;
			}
			
			rtTransform = this.ExtendedItem.Properties[Visual.RenderTransformProperty].Value;
			
			operation = PlacementOperation.Start(extendedItemArray, PlacementType.Resize);
		}
		
		private void dragX_Changed(DragListener drag)
		{
            if (drag == null || drag.LastEventArgs == null)
                return;

            Point currentPoint = drag.LastEventArgs.GetPosition(this.parent);
			Vector deltaVector = currentPoint - this.startPoint;
			
			var destAngle = (-0.5*deltaVector.X) + skewX;
			
			if (destAngle == 0 && skewY == 0)
			{
				this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformProperty).Reset();
				rtTransform = null;
				skewTransform = null;
			}
			else
			{
				if ((rtTransform == null) || !(rtTransform.Component is SkewTransform))
				{
					if (!this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).IsSet) {
						this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).SetValue(new Point(0.5,0.5));
					}
					
					if (this.skewTransform == null)
						this.skewTransform = new SkewTransform(0, 0);
					this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformProperty).SetValue(skewTransform);
					rtTransform = this.ExtendedItem.Properties[Visual.RenderTransformProperty].Value;
				}
				rtTransform.Properties["AngleX"].SetValue(destAngle);
			}
			
			_adornerLayer.UpdateAdornersForElement(this.ExtendedItem.View, true);
		}
		
		void dragX_Completed(gip.ext.designer.avui.Controls.DragListener drag)
		{
			operation.Commit();
		}
		
		private void dragY_Started(DragListener drag)
		{
            if (drag == null || drag.LastEventArgs == null)
                return;
            _adornerLayer = this.adornerPanel.TryFindParent<AdornerLayer>();
			
			var designerItem = this.ExtendedItem.Component as Control;
			this.parent = VisualTreeHelper.GetParent(designerItem) as Control;
			
			startPoint = drag.LastEventArgs.GetPosition(this.parent);
			
			if (this.skewTransform == null)
			{
				this.skewX = 0;
				this.skewY = 0;
			}
			else
			{
				this.skewX = this.skewTransform.AngleX;
				this.skewY = this.skewTransform.AngleY;
			}
			
			rtTransform = this.ExtendedItem.Properties[Visual.RenderTransformProperty].Value;
			
			operation = PlacementOperation.Start(extendedItemArray, PlacementType.Resize);
		}
		
		private void dragY_Changed(DragListener drag)
		{
            if (drag == null || drag.LastEventArgs == null)
                return;
            Point currentPoint = drag.LastEventArgs.GetPosition(this.parent);
			Vector deltaVector = currentPoint - this.startPoint;
			
			var destAngle = (-0.5*deltaVector.Y) + skewY;
			
			if (destAngle == 0 && skewX == 0)
			{
				this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformProperty).Reset();
				rtTransform = null;
				skewTransform = null;
			}
			else
			{
				if (rtTransform == null)
				{
					if (!this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).IsSet)
					{
						this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformOriginProperty).SetValue(new Point(0.5, 0.5));
					}
					
					if (this.skewTransform == null)
						this.skewTransform = new SkewTransform(0, 0);
					this.ExtendedItem.Properties.GetProperty(Visual.RenderTransformProperty).SetValue(skewTransform);
					rtTransform = this.ExtendedItem.Properties[Visual.RenderTransformProperty].Value;
				}
				rtTransform.Properties["AngleY"].SetValue(destAngle);
			}
			
			_adornerLayer.UpdateAdornersForElement(this.ExtendedItem.View, true);
		}
		
		void dragY_Completed(gip.ext.designer.avui.Controls.DragListener drag)
		{
			operation.Commit();
		}
		
		#endregion
		
		protected override void OnInitialized()
		{
			
			if (this.ExtendedItem.Component is WindowClone)
				return;
			base.OnInitialized();
			
			extendedItemArray[0] = this.ExtendedItem;
			this.ExtendedItem.PropertyChanged += OnPropertyChanged;
			
			var designerItem = this.ExtendedItem.Component as Control;
			this.skewTransform = designerItem.RenderTransform as SkewTransform;
			
			if (skewTransform != null)
			{
				skewX = skewTransform.AngleX;
				skewY = skewTransform.AngleY;
			}

            //thumb1 = new Thumb() { Cursor = Cursors.ScrollWE, Height = 14, Width = 4, Opacity = 1 };
            //thumb2 = new Thumb() { Cursor = Cursors.ScrollNS, Width = 14, Height = 4, Opacity = 1 };
            // Scroll cursors are not available in Avalonia, using Size cursors instead
            thumb1 = new Thumb() { Cursor = new Cursor(StandardCursorType.SizeWestEast), Height = 14, Width = 4, Opacity = 1 };
            thumb2 = new Thumb() { Cursor = new Cursor(StandardCursorType.SizeNorthSouth), Width = 14, Height = 4, Opacity = 1 };


            OnPropertyChanged(null, null);
			
			adornerPanel.Children.Add(thumb1);
			adornerPanel.Children.Add(thumb2);
			
			DragListener drag1 = new DragListener(thumb1);
			drag1.Started += dragX_Started;
			drag1.Changed += dragX_Changed;
			drag1.Completed += dragX_Completed;
			DragListener drag2 = new DragListener(thumb2);
			drag2.Started += dragY_Started;
			drag2.Changed += dragY_Changed;
			drag2.Completed += dragY_Completed;
		}
		
		void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (sender == null || e.PropertyName == "Width" || e.PropertyName == "Height") {
				AdornerPanel.SetPlacement(thumb1,
				                          new RelativePlacement(HorizontalAlignment.Center, VerticalAlignment.Top) {
				                          	YOffset = 0,
				                          	XOffset = -1 * PlacementOperation.GetRealElementSize(ExtendedItem.View).Width / 4
				                          });
				
				AdornerPanel.SetPlacement(thumb2,
				                          new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Center) {
				                          	YOffset = -1 * PlacementOperation.GetRealElementSize(ExtendedItem.View).Height / 4,
				                          	XOffset = 0
				                          });

				var designPanel = this.ExtendedItem.Services.DesignPanel as DesignPanel;
				if (designPanel != null)
					designPanel.AdornerLayer.UpdateAdornersForElement(this.ExtendedItem.View, true);
			}
		}
		
		protected override void OnRemove()
		{
			this.ExtendedItem.PropertyChanged -= OnPropertyChanged;
			base.OnRemove();
		}
	}
}
