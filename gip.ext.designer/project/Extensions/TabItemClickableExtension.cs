// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Windows.Controls;
using gip.ext.design.Extensions;

namespace gip.ext.designer.Extensions
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
