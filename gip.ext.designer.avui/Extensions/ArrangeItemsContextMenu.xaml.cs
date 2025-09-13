// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;
using gip.ext.designer.avui.themes;

namespace gip.ext.designer.avui.Extensions
{
	public partial class ArrangeItemsContextMenu
	{
		private DesignItem designItem;
		
		public ArrangeItemsContextMenu(DesignItem designItem)
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
		
		void Click_ArrangeLeft(object sender, System.Windows.RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.Left);
		}

		void Click_ArrangeHorizontalCentered(object sender, System.Windows.RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.HorizontalMiddle);
		}

		void Click_ArrangeRight(object sender, System.Windows.RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.Right);
		}

		void Click_ArrangeTop(object sender, System.Windows.RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.Top);
		}

		void Click_ArrangeVerticalCentered(object sender, System.Windows.RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.VerticalMiddle);
		}

		void Click_ArrangeBottom(object sender, System.Windows.RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.Bottom);
		}
	}
}
