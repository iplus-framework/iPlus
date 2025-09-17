// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor
{
	public partial class SolidBrushEditor : UserControl
    {
		public SolidBrushEditor()
		{
			InitializeComponent();
		}

		public static readonly StyledProperty<Color> ColorProperty =
			AvaloniaProperty.Register<SolidBrushEditor, Color>(nameof(Color), defaultValue: Colors.Transparent, defaultBindingMode: BindingMode.TwoWay);

		public Color Color {
			get { return GetValue(ColorProperty); }
			set { SetValue(ColorProperty, value); }
		}

        //private void InitializeComponent()
        //{
        //    AvaloniaXamlLoader.Load(this);
        //}
    }
}
