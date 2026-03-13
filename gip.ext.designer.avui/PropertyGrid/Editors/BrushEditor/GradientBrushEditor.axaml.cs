// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor
{
	public partial class GradientBrushEditor : UserControl
	{
		public GradientBrushEditor()
		{
			InitializeComponent();
		}

		public static readonly StyledProperty<GradientBrush> BrushProperty =
			AvaloniaProperty.Register<GradientBrushEditor, GradientBrush>(nameof(Brush), defaultBindingMode: BindingMode.TwoWay);

		public GradientBrush Brush
		{
			get { return GetValue(BrushProperty); }
			set { SetValue(BrushProperty, value); }
		}
	}
}
