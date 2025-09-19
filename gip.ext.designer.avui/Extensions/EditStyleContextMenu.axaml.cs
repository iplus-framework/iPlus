// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.IO;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using gip.ext.designer.avui.Xaml;
using gip.ext.xamldom.avui;
using gip.ext.designer.avui.themes;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Extensions
{
	public partial class EditStyleContextMenu : ContextMenu
	{
		private DesignItem designItem;

		public EditStyleContextMenu(DesignItem designItem)
		{
			this.designItem = designItem;
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void Click_EditStyle(object sender, RoutedEventArgs e)
		{
			var cg = designItem.OpenGroup("Edit Style");

			var element = designItem.View;
			
			// In AvaloniaUI, we need to handle style keys differently
			object defaultStyleKey = null;
			if (element is StyledElement styledElement)
			{
				defaultStyleKey = styledElement.GetType(); // Use type as style key
			}
			
			// Try to find style from application resources
			IStyle style = null;
			if (defaultStyleKey != null && Application.Current?.Resources != null)
			{
				if (Application.Current.Resources.TryGetResource(defaultStyleKey, null, out var resource))
				{
					style = resource as IStyle;
				}
			}

			var service = ((XamlComponentService) designItem.Services.Component);

			var ms = new MemoryStream();
			XmlTextWriter writer = new XmlTextWriter(ms, System.Text.Encoding.UTF8);
			writer.Formatting = Formatting.Indented;
			
			// Note: AvaloniaUI doesn't have a direct equivalent to XamlWriter.Save
			// We'll need to serialize the style manually or use a different approach
			if (style != null)
			{
				// For now, create a basic style XAML structure
				writer.WriteStartElement("Style");
				writer.WriteAttributeString("TargetType", defaultStyleKey.ToString());
				writer.WriteEndElement();
			}

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
