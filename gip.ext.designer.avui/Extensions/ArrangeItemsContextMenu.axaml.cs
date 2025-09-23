// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using gip.ext.design.avui;
using gip.ext.designer.avui.themes;

namespace gip.ext.designer.avui.Extensions
{
	public partial class ArrangeItemsContextMenu : ContextMenu
    {
        public ArrangeItemsContextMenu() : base()
        {
        }

        private DesignItem designItem;
		
		public ArrangeItemsContextMenu(DesignItem designItem)
		{
			this.designItem = designItem;
			InitializeComponent();
        }

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
		
		void Click_ArrangeLeft(object sender, RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.Left);
		}

		void Click_ArrangeHorizontalCentered(object sender, RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.HorizontalMiddle);
		}

		void Click_ArrangeRight(object sender, RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.Right);
		}

		void Click_ArrangeTop(object sender, RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.Top);
		}

		void Click_ArrangeVerticalCentered(object sender, RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.VerticalMiddle);
		}

		void Click_ArrangeBottom(object sender, RoutedEventArgs e)
		{
			ModelTools.ArrangeItems(this.designItem.Services.Selection.SelectedItems, ArrangeDirection.Bottom);
		}
	}
}
