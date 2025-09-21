// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Controls;
using Avalonia;
using Avalonia.LogicalTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor
{
	public class NormalizedPanel : Panel
	{
		public static double GetX(AvaloniaObject obj)
		{
			return obj.GetValue(XProperty);
		}

		public static void SetX(AvaloniaObject obj, double value)
		{
			obj.SetValue(XProperty, value);
		}

		public static readonly AttachedProperty<double> XProperty =
			AvaloniaProperty.RegisterAttached<NormalizedPanel, Control, double>("X", 0.0, false, Avalonia.Data.BindingMode.TwoWay);

		public static double GetY(AvaloniaObject obj)
		{
			return obj.GetValue(YProperty);
		}

		public static void SetY(AvaloniaObject obj, double value)
		{
			obj.SetValue(YProperty, value);
		}

		public static readonly AttachedProperty<double> YProperty =
			AvaloniaProperty.RegisterAttached<NormalizedPanel, Control, double>("Y", 0.0, false, Avalonia.Data.BindingMode.TwoWay);

		static NormalizedPanel()
		{
			XProperty.Changed.AddClassHandler<Control>(OnPositioningChanged);
			YProperty.Changed.AddClassHandler<Control>(OnPositioningChanged);
		}

		static void OnPositioningChanged(Control sender, AvaloniaPropertyChangedEventArgs e)
		{
			NormalizedPanel parent = sender.GetLogicalParent() as NormalizedPanel;
			if (parent != null) {
				parent.InvalidateArrange();
			}
		}

		protected override Size MeasureOverride(Size availableSize)
		{
			foreach (Control item in Children) {
				item.Measure(availableSize);
			}
			return new Size();
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			foreach (Control item in Children) {
				Rect r = new Rect(item.DesiredSize);
				r = r.WithX(GetX(item) * finalSize.Width - item.DesiredSize.Width / 2);
				r = r.WithY(GetY(item) * finalSize.Height - item.DesiredSize.Height / 2);
				item.Arrange(r);
			}
			return finalSize;
		}
	}
}
