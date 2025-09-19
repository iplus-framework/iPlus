// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Reflection;
using gip.ext.design.avui.Adorners;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui.Extensions;
using Avalonia.Controls;

namespace gip.ext.designer.avui.Extensions
{
	/// <summary>
	/// 
	/// </summary>
	[ExtensionServer(typeof(OnlyOneItemSelectedExtensionServer))]
	[ExtensionFor(typeof(Control))]
	[Extension(Order = 50)]
	public class WrapItemContextMenuExtension : SelectionAdornerProvider
	{
		DesignPanel panel;
		ContextMenu contextMenu;

		protected override void OnInitialized()
		{
			base.OnInitialized();

			if (!(ExtendedItem.View is WindowClone)) {
				contextMenu = new WrapItemContextMenu(ExtendedItem);
				panel = ExtendedItem.Context.Services.DesignPanel as DesignPanel;
				if (panel != null)
					panel.AddContextMenu(contextMenu, this.GetType().GetCustomAttribute<ExtensionAttribute>().Order);
			}
		}

		protected override void OnRemove()
		{
			if (panel != null)
				panel.RemoveContextMenu(contextMenu);

			base.OnRemove();
		}
	}
}
