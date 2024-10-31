﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace gip.ext.designer.Controls
{
	/// <summary>
	/// A button with a drop-down arrow.
	/// </summary>
	public class DropDownButton : Button
	{
		static readonly Geometry triangle = (Geometry)new GeometryConverter().ConvertFromInvariantString("M0,0 L1,0 0.5,1 z");
		
		public DropDownButton()
		{
			Content = new Path {
				Fill = Brushes.Black,
				Data = triangle,
				Width = 7,
				Height = 3.5,
				Stretch = Stretch.Fill
			};
		}
	}
}
