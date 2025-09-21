// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections;
using gip.ext.designer.avui.themes;
using gip.ext.design.avui.PropertyGrid;
using gip.ext.designer.avui.PropertyGrid.Editors;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace gip.ext.designer.avui.Editors
{
	[TypeEditor(typeof(ICollection))]
	public partial class OpenCollectionEditor : UserControl
	{
		public OpenCollectionEditor()
		{
			SpecialInitializeComponent();
		}
		
		/// <summary>
		/// Fixes InitializeComponent with multiple Versions of same Assembly loaded
		/// </summary>
		public void SpecialInitializeComponent()
		{
			// For specialized resource loading, we'll use the standard pattern and let VersionedAssemblyResourceDictionary handle it
			this.InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
		
		void open_Click(object sender, RoutedEventArgs e)
		{
			var node = this.DataContext as PropertyNode;
			
			var editor = new FlatCollectionEditor(TopLevel.GetTopLevel(this) as Window);
			editor.LoadItemsCollection(node.FirstProperty);
			editor.ShowDialog(TopLevel.GetTopLevel(this) as Window);
		}
	}
}