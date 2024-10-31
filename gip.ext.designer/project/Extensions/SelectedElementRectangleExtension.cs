// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

using gip.ext.design.Adorners;
using gip.ext.design.Extensions;

namespace gip.ext.designer.Extensions
{
	/// <summary>
	/// Draws a dotted line around selected UIElements.
	/// </summary>
	[ExtensionFor(typeof(UIElement))]
	public sealed class SelectedElementRectangleExtension : SelectionAdornerProvider
	{
		/// <summary>
		/// Creates a new SelectedElementRectangleExtension instance.
		/// </summary>
		public SelectedElementRectangleExtension()
		{
			Rectangle selectionRect = new Rectangle();
            selectionRect.SnapsToDevicePixels = true;
            selectionRect.Stroke = new SolidColorBrush(Color.FromRgb(0x47, 0x47, 0x47));
            selectionRect.StrokeThickness = 1.5;
            selectionRect.IsHitTestVisible = false;

            RelativePlacement placement = new RelativePlacement(HorizontalAlignment.Stretch, VerticalAlignment.Stretch);
            placement.XOffset = -1;
            placement.YOffset = -1;
            placement.WidthOffset = 2;
            placement.HeightOffset = 2;

            this.AddAdorners(placement, selectionRect);
		}
	}
}
