// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using gip.ext.designer.avui.themes;
using gip.ext.design.avui.PropertyGrid;

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
			if (!this._contentLoaded) {
				this._contentLoaded = true;
				Uri resourceLocator = new Uri(VersionedAssemblyResourceDictionary.GetXamlNameForType(this.GetType()), UriKind.Relative);
				Application.LoadComponent(this, resourceLocator);
			}
			
			this.InitializeComponent();
		}
		
		void open_Click(object sender, RoutedEventArgs e)
		{
			var node = this.DataContext as PropertyNode;
			
			var editor = new FlatCollectionEditor(Window.GetWindow(this));
			editor.LoadItemsCollection(node.FirstProperty);
			editor.ShowDialog();
		}
	}
}