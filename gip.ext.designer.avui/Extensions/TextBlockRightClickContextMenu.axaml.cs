// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using gip.ext.design.avui;
using gip.ext.designer.avui.PropertyGrid;
using gip.ext.designer.avui.themes;

namespace gip.ext.designer.avui.Extensions
{
	public partial class TextBlockRightClickContextMenu : ContextMenu
	{
        public TextBlockRightClickContextMenu() : base()
        {
        }

        private DesignItem designItem;

		public TextBlockRightClickContextMenu(DesignItem designItem)
		{
			this.designItem = designItem;
			InitializeComponent();
        }

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		void Click_EditFormatedText(object sender, RoutedEventArgs e)
		{
			var dlg = new Window()
			{
				Content = new gip.ext.designer.avui.PropertyGrid.FormatedTextEditor(designItem),
				Width = 440,
				Height = 200,
				WindowStartupLocation = WindowStartupLocation.CenterOwner
			};

			// Find the parent window for the owner
			var designPanel = (DesignPanel)designItem.Context.Services.DesignPanel;
			var parentWindow = designPanel.TryFindParent<Window>();
			if (parentWindow != null)
			{
				// Show as dialog with parent owner
				dlg.ShowDialog(parentWindow);
			}
			else
			{
				// Fallback to showing without owner
				dlg.Show();
			}
		}
	}
}
