// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;
using gip.ext.design.avui;
using gip.ext.designer.avui.PropertyGrid;
using gip.ext.designer.avui.themes;

namespace gip.ext.designer.avui.Extensions
{
	public partial class TextBlockRightClickContextMenu
	{
		private DesignItem designItem;

		public TextBlockRightClickContextMenu(DesignItem designItem)
		{
			this.designItem = designItem;

			SpecialInitializeComponent();
		}

		/// <summary>
		/// Fixes InitializeComponent with multiple Versions of same Assembly loaded
		/// </summary>
		public void SpecialInitializeComponent()
		{
			if (!this._contentLoaded)
			{
				this._contentLoaded = true;
				Uri resourceLocator = new Uri(VersionedAssemblyResourceDictionary.GetXamlNameForType(this.GetType()), UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}

			this.InitializeComponent();
		}

		void Click_EditFormatedText(object sender, RoutedEventArgs e)
		{
			var dlg = new Window()
			{
				Content = new FormatedTextEditor(designItem),
				Width = 440,
				Height = 200,
				WindowStyle = WindowStyle.ToolWindow,
				Owner = ((DesignPanel)designItem.Context.Services.DesignPanel).TryFindParent<Window>(),
			};

			dlg.ShowDialog();
		}
	}
}
