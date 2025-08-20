// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows.Controls;
using gip.ext.design.avui.Extensions;

namespace gip.ext.designer.avui.Extensions
{
	/// <summary>
	/// Makes TabItems clickable.
	/// </summary>
	[ExtensionFor(typeof(TabItem))]
	[ExtensionServer(typeof(PrimarySelectionExtensionServer))]
	public sealed class TabItemClickableExtension : DefaultExtension
	{
		/// <summary/>
		protected override void OnInitialized()
		{
			// When tab item becomes primary selection, make it the active tab page in its parent tab control.
			TabItem tabItem = (TabItem)this.ExtendedItem.Component;
			TabControl tabControl = tabItem.Parent as TabControl;
			if (tabControl != null) {
				tabControl.SelectedItem = tabItem;
			}
		}
	}
}
