// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.ext.designer.avui.Controls
{
	public class Picker : Grid
	{
		public Picker()
		{
			SizeChanged += delegate { UpdateValueOffset(); };
		}

		public static readonly StyledProperty<Control> MarkerProperty =
			AvaloniaProperty.Register<Picker, Control>(nameof(Marker));

		public Control Marker {
			get { return GetValue(MarkerProperty); }
			set { SetValue(MarkerProperty, value); }
		}

		public static readonly StyledProperty<double> ValueProperty =
			AvaloniaProperty.Register<Picker, double>(nameof(Value), 0.0, defaultBindingMode: BindingMode.TwoWay);

		public double Value {
			get { return GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public static readonly StyledProperty<double> ValueOffsetProperty =
			AvaloniaProperty.Register<Picker, double>(nameof(ValueOffset));

		public double ValueOffset {
			get { return GetValue(ValueOffsetProperty); }
			set { SetValue(ValueOffsetProperty, value); }
		}

		public static readonly StyledProperty<Orientation> OrientationProperty =
			AvaloniaProperty.Register<Picker, Orientation>(nameof(Orientation));

		public Orientation Orientation {
			get { return GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		public static readonly StyledProperty<double> MinimumProperty =
			AvaloniaProperty.Register<Picker, double>(nameof(Minimum));

		public double Minimum {
			get { return GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}

		public static readonly StyledProperty<double> MaximumProperty =
			AvaloniaProperty.Register<Picker, double>(nameof(Maximum), 100.0);

		public double Maximum {
			get { return GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);

			if (change.Property == MarkerProperty) {
				if (Marker != null)
				{
					TranslateTransform t = Marker.RenderTransform as TranslateTransform;
					if (t == null) {
						t = new TranslateTransform();
						Marker.RenderTransform = t;
					}
					var property = Orientation == Orientation.Horizontal ? TranslateTransform.XProperty : TranslateTransform.YProperty;
					t.Bind(property, new Binding(nameof(ValueOffset)) {
						Source = this
					});
				}
			}
			else if (change.Property == ValueProperty) {
				UpdateValueOffset();
			}
		}

		bool isMouseDown;

		protected override void OnPointerPressed(PointerPressedEventArgs e)
		{
			base.OnPointerPressed(e);
			isMouseDown = true;
			e.Pointer.Capture(this);
			UpdateValue(e);
		}

		protected override void OnPointerMoved(PointerEventArgs e)
		{
			base.OnPointerMoved(e);
			if (isMouseDown) {
				UpdateValue(e);
			}
		}

		protected override void OnPointerReleased(PointerReleasedEventArgs e)
		{
			base.OnPointerReleased(e);
			isMouseDown = false;
			e.Pointer.Capture(null);
		}

		void UpdateValue(PointerEventArgs e)
		{
			Point p = e.GetPosition(this);
			double length = 0, pos = 0;
			
			if (Orientation == Orientation.Horizontal) {
				length = Bounds.Width;
				pos = p.X;
			}
			else {
				length = Bounds.Height;
				pos = p.Y;
			}

			pos = Math.Max(0, Math.Min(length, pos));
			Value = Minimum + (Maximum - Minimum) * pos / length;
		}

		void UpdateValueOffset()
		{
			var length = Orientation == Orientation.Horizontal ? Bounds.Width : Bounds.Height;
			ValueOffset = length * (Value - Minimum) / (Maximum - Minimum);
		}
	}
}
