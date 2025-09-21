// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace gip.ext.designer.avui.OutlineView
{
	public class IconItem : Control
	{
		public static readonly StyledProperty<IImage> IconProperty =
			AvaloniaProperty.Register<IconItem, IImage>("Icon");

		public IImage Icon {
			get { return GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public static readonly StyledProperty<string> TextProperty =
			AvaloniaProperty.Register<IconItem, string>("Text");

		public string Text {
			get { return GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}
	}
}
