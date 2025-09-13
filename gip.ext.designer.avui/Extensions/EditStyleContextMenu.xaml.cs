// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Xml;
using gip.ext.designer.avui.Xaml;
using gip.ext.xamldom.avui;
using gip.ext.designer.avui.themes;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Extensions
{
	public partial class EditStyleContextMenu
	{
		private DesignItem designItem;

		public EditStyleContextMenu(DesignItem designItem)
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

		void Click_EditStyle(object sender, RoutedEventArgs e)
		{
			var cg = designItem.OpenGroup("Edit Style");

			var element = designItem.View;
			object defaultStyleKey = element.GetValue(FrameworkElement.DefaultStyleKeyProperty);
			Style style = Application.Current.TryFindResource(defaultStyleKey) as Style;

			var service = ((XamlComponentService) designItem.Services.Component);

			var ms = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(ms, System.Text.Encoding.UTF8);
			writer.Formatting = Formatting.Indented;
			XamlWriter.Save(style, writer);

			var rootItem = this.designItem.Context.RootItem as XamlDesignItem;

			ms.Position = 0;
			var sr = new StreamReader(ms);
			var xaml = sr.ReadToEnd();

			var xamlObject = XamlParser.ParseSnippet(rootItem.XamlObject, xaml, ((XamlDesignContext)this.designItem.Context).ParserSettings);
			
			var styleDesignItem=service.RegisterXamlComponentRecursive(xamlObject);
			try {
				designItem.Properties.GetProperty("Resources").CollectionElements.Add(styleDesignItem);
				cg.Commit();
			}
			catch (Exception) {
				cg.Abort();
			}
		}
	}
}
