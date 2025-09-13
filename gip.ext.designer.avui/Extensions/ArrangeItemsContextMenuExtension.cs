// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;

namespace gip.ext.designer.avui.Extensions
{
	/// <summary>
	/// 
	/// </summary>
	[ExtensionServer(typeof(PrimarySelectionButOnlyWhenMultipleSelectedExtensionServer))]
	[ExtensionFor(typeof(UIElement))]
	[Extension(Order = 30)]
	public class ArrangeItemsContextMenuExtension : SelectionAdornerProvider
	{
		DesignPanel panel;
		ContextMenu contextMenu;

		protected override void OnInitialized()
		{
			base.OnInitialized();

			contextMenu = new ArrangeItemsContextMenu(ExtendedItem);
			panel = ExtendedItem.Context.Services.DesignPanel as DesignPanel;
			if (panel != null)
				panel.AddContextMenu(contextMenu, this.GetType().GetCustomAttribute<ExtensionAttribute>().Order);
		}
		
		protected override void OnRemove()
		{
			if (panel != null)
				panel.RemoveContextMenu(contextMenu);

			base.OnRemove();
		}
	}
}
