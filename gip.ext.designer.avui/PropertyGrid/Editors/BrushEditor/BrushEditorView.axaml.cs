﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Globalization;

namespace gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor
{
	public partial class BrushEditorView
	{
		public BrushEditorView()
		{
			BrushEditor = new BrushEditor();
			DataContext = BrushEditor;

			InitializeComponent();

			SetBinding(HeightProperty, new Binding("Brush") {
				Converter = HeightConverter.Instance
			});
		}

		public BrushEditor BrushEditor { get; private set; }

		class HeightConverter : IValueConverter
		{
			public static HeightConverter Instance = new HeightConverter();

			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (value is GradientBrush) return double.NaN;
				return 315;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}
	}
}
