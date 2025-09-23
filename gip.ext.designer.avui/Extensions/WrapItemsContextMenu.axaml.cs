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
	public partial class WrapItemsContextMenu : ContextMenu
    {
        public WrapItemsContextMenu() : base()
        {
        }

        private DesignItem designItem;
		
		public WrapItemsContextMenu(DesignItem designItem)
		{
			this.designItem = designItem;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void Click_WrapInCanvas(object sender, RoutedEventArgs e)
		{
			ModelTools.WrapItemsNewContainer(this.designItem.Services.Selection.SelectedItems, typeof(Canvas));
		}
		
		void Click_WrapInGrid(object sender, RoutedEventArgs e)
		{
			ModelTools.WrapItemsNewContainer(this.designItem.Services.Selection.SelectedItems, typeof(Grid));
		}
	}
}
