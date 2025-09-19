// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Services;
using gip.ext.designer.avui.Controls;
using Avalonia.Controls;
using Avalonia.Input;
using gip.ext.design.avui;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Collections;

namespace gip.ext.designer.avui.Extensions
{
	
	/// <summary>
	/// The drag handle displayed for Framework Elements
	/// </summary>
	[ExtensionServer(typeof(PrimarySelectionButOnlyWhenMultipleSelectedExtensionServer))]
	[ExtensionFor(typeof(Control))]
	public class TopLeftContainerDragHandleMultipleItems : AdornerProvider
	{
		/// <summary/>
		public TopLeftContainerDragHandleMultipleItems()
		{ }
		
		protected override void OnInitialized()
		{
			base.OnInitialized();
			
			ContainerDragHandle rect = new ContainerDragHandle();

            rect.PointerPressed += delegate (object sender, PointerPressedEventArgs e)
			{
                // In WPF this was a preview click event
                //if (e.Route != RoutingStrategies.Tunnel)
					//return;
                //Services.Selection.SetSelectedComponents(new DesignItem[] { this.ExtendedItem }, SelectionTypes.Auto);
                new DragMoveMouseGesture(this.ExtendedItem, false).Start(this.ExtendedItem.Services.DesignPanel,e);
				e.Handled=true;
			};
			
			var items = this.ExtendedItem.Services.Selection.SelectedItems;
			
			double minX = 0;
			double minY = 0;
			double maxX = 0;
			double maxY = 0;
			
			foreach (DesignItem di in items) {
				Point relativeLocation = di.View.TranslatePoint(new Point(0, 0), this.ExtendedItem.View as Visual).Value;
				
				minX = minX < relativeLocation.X ? minX : relativeLocation.X;
				minY = minY < relativeLocation.Y ? minY : relativeLocation.Y;
				maxX = maxX > relativeLocation.X + ((Control)di.View).Bounds.Width ? maxX : relativeLocation.X + ((Control)di.View).Bounds.Width;
				maxY = maxY > relativeLocation.Y + ((Control)di.View).Bounds.Height ? maxY : relativeLocation.Y + ((Control)di.View).Bounds.Height;
			}
			
			Rectangle rect2 = new Rectangle() {
				Width = (maxX - minX) + 4,
				Height = (maxY - minY) + 4,
				Stroke = Brushes.Black,
				StrokeThickness = 2,
				StrokeDashArray = new AvaloniaList<double>(){ 2, 2 },
			};
			
			RelativePlacement p = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top);
			p.XOffset = minX - 3;
			p.YOffset = minY - 3;
			
			RelativePlacement p2 = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top);
			p2.XOffset = (minX + rect2.Width) - 2;
			p2.YOffset = (minY + rect2.Height) - 2;
						
			AddAdorner(p, AdornerOrder.Background, rect);
			AddAdorner(p2, AdornerOrder.Background, rect2);
		}
	}	
}
