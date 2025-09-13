// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;

using gip.ext.designer.avui.Services;
using gip.ext.designer.avui.Controls;

namespace gip.ext.designer.avui.Extensions
{
	
	/// <summary>
	/// The drag handle displayed for Framework Elements
	/// </summary>
	[ExtensionServer(typeof(PrimarySelectionButOnlyWhenMultipleSelectedExtensionServer))]
	[ExtensionFor(typeof(FrameworkElement))]
	public class TopLeftContainerDragHandleMultipleItems : AdornerProvider
	{
		/// <summary/>
		public TopLeftContainerDragHandleMultipleItems()
		{ }
		
		protected override void OnInitialized()
		{
			base.OnInitialized();
			
			ContainerDragHandle rect = new ContainerDragHandle();
			
			rect.PreviewMouseDown += delegate(object sender, MouseButtonEventArgs e) {
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
				Point relativeLocation = di.View.TranslatePoint(new Point(0, 0), this.ExtendedItem.View);
				
				minX = minX < relativeLocation.X ? minX : relativeLocation.X;
				minY = minY < relativeLocation.Y ? minY : relativeLocation.Y;
				maxX = maxX > relativeLocation.X + ((FrameworkElement)di.View).ActualWidth ? maxX : relativeLocation.X + ((FrameworkElement)di.View).ActualWidth;
				maxY = maxY > relativeLocation.Y + ((FrameworkElement)di.View).ActualHeight ? maxY : relativeLocation.Y + ((FrameworkElement)di.View).ActualHeight;
			}
			
			Rectangle rect2 = new Rectangle() {
				Width = (maxX - minX) + 4,
				Height = (maxY - minY) + 4,
				Stroke = Brushes.Black,
				StrokeThickness = 2,
				StrokeDashArray = new DoubleCollection(){ 2, 2 },
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
