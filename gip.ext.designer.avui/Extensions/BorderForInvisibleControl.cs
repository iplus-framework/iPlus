// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using System.Windows.Controls;
using System.Windows;
using gip.ext.designer.avui.Controls;
using System.Windows.Media;

namespace gip.ext.designer.avui.Extensions
{
	[ExtensionFor(typeof(Panel))]
	public class BorderForInvisibleControl : PermanentAdornerProvider
	{
		protected override void OnInitialized()
		{
			base.OnInitialized();

			var adornerPanel = new AdornerPanel();
			var border = new Border();
			border.BorderThickness = new Thickness(1);
			border.BorderBrush = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
			border.IsHitTestVisible = false;
			AdornerPanel.SetPlacement(border, AdornerPlacement.FillContent);
			adornerPanel.Children.Add(border);
			Adorners.Add(adornerPanel);
		}
	}
}
