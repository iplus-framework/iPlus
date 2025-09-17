// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;
using System.Windows.Controls;
using gip.ext.design.avui;
using gip.ext.designer.avui.themes;

namespace gip.ext.designer.avui.Extensions
{
	public partial class WrapItemsContextMenu
	{
		private DesignItem designItem;
		
		public WrapItemsContextMenu(DesignItem designItem)
		{
			this.designItem = designItem;
			
			SpecialInitializeComponent();
		}
		
		/// <summary>
		/// Fixes InitializeComponent with multiple Versions of same Assembly loaded
		/// </summary>
		public void SpecialInitializeComponent()
		{
			if (!this._contentLoaded) {
				this._contentLoaded = true;
				Uri resourceLocator = new Uri(VersionedAssemblyResourceDictionary.GetXamlNameForType(this.GetType()), UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
			
			this.InitializeComponent();
		}
		
		void Click_WrapInCanvas(object sender, System.Windows.RoutedEventArgs e)
		{
			ModelTools.WrapItemsNewContainer(this.designItem.Services.Selection.SelectedItems, typeof(Canvas));
		}
		
		void Click_WrapInGrid(object sender, System.Windows.RoutedEventArgs e)
		{
			ModelTools.WrapItemsNewContainer(this.designItem.Services.Selection.SelectedItems, typeof(Grid));
		}
	}
}
