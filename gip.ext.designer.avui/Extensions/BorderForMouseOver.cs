// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;

namespace gip.ext.designer.avui.Extensions
{
	[ExtensionFor(typeof(Control))]
	[ExtensionServer(typeof(MouseOverExtensionServer))]

	public class BorderForMouseOver : AdornerProvider
	{
		readonly AdornerPanel adornerPanel;
		readonly Border border = new Border();

		public BorderForMouseOver()
		{
			adornerPanel = new AdornerPanel();
			adornerPanel.Order = AdornerOrder.Background;
			this.Adorners.Add(adornerPanel);
			border.BorderThickness = new Thickness(1);
			border.BorderBrush = Brushes.DodgerBlue;
			border.Margin = new Thickness(-2);
			AdornerPanel.SetPlacement(border, AdornerPlacement.FillContent);
			adornerPanel.Children.Add(border);
		}

		protected override void OnInitialized()
		{
			base.OnInitialized();

			if (ExtendedItem.Component is Avalonia.Controls.Shapes.Line line)
			{
				// To display border of Line in correct position.
				border.Margin = new Thickness(line.EndPoint.X < 0 ? line.EndPoint.X : 0, line.EndPoint.Y < 0 ? line.EndPoint.Y : 0);
			}
		}
	}
}
