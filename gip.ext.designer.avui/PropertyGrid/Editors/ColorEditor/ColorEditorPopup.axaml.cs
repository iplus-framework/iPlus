// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using gip.ext.designer.avui.themes;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor;

namespace gip.ext.designer.avui.PropertyGrid.Editors.ColorEditor
{
	public partial class ColorEditorPopup : Window
	{	
		public ColorEditorPopup()
		{
            this.InitializeComponent();
        }
			
		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
		
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Escape) 
			{
				Close();
			}
			base.OnKeyDown(e);
		}
	}
}
