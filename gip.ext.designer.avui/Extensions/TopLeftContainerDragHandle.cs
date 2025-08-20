// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;

using gip.ext.designer.avui.Services;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Extensions
{
	
	/// <summary>
	/// The drag handle displayed for panels.
	/// </summary>
	[ExtensionServer(typeof(PrimarySelectionExtensionServer))]
	[ExtensionFor(typeof(Panel))]
	[ExtensionFor(typeof(Image))]
	[ExtensionFor(typeof(MediaElement))]
	[ExtensionFor(typeof(ItemsControl))]
	public class TopLeftContainerDragHandle : AdornerProvider
	{
		/// <summary/>
		public TopLeftContainerDragHandle()
		{
			ContainerDragHandle rect = new ContainerDragHandle();
			
			rect.PreviewMouseDown += delegate(object sender, MouseButtonEventArgs e) {
				Services.Selection.SetSelectedComponents(new DesignItem[] { this.ExtendedItem }, SelectionTypes.Auto);
				new DragMoveMouseGesture(this.ExtendedItem, false).Start(this.ExtendedItem.Services.DesignPanel,e);
				e.Handled=true;                                                  
			};
			
			RelativePlacement p = new RelativePlacement(HorizontalAlignment.Left, VerticalAlignment.Top);
			p.XOffset = -7;
			p.YOffset = -7;
			
			AddAdorner(p, AdornerOrder.Background, rect);
		}
	}
	
}
