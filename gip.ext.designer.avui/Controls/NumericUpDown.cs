// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;

namespace gip.ext.designer.avui.Controls
{
	public class NumericUpDown : TemplatedControl
	{
		TextBox textBox;
		DragRepeatButton upButton;
		DragRepeatButton downButton;

		public static readonly StyledProperty<int> DecimalPlacesProperty =
			AvaloniaProperty.Register<NumericUpDown, int>(nameof(DecimalPlaces));

		public int DecimalPlaces {
			get { return GetValue(DecimalPlacesProperty); }
			set { SetValue(DecimalPlacesProperty, value); }
		}

		public static readonly StyledProperty<double> MinimumProperty =
			AvaloniaProperty.Register<NumericUpDown, double>(nameof(Minimum));

		public double Minimum {
			get { return GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}

		public static readonly StyledProperty<double> MaximumProperty =
			AvaloniaProperty.Register<NumericUpDown, double>(nameof(Maximum), 100.0);

		public double Maximum {
			get { return GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}

		public static readonly StyledProperty<double?> ValueProperty =
			AvaloniaProperty.Register<NumericUpDown, double?>(nameof(Value), 0.0, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

		public double? Value {
			get { return GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}

		public static readonly StyledProperty<double> SmallChangeProperty =
			AvaloniaProperty.Register<NumericUpDown, double>(nameof(SmallChange), 1.0);

		public double SmallChange {
			get { return GetValue(SmallChangeProperty); }
			set { SetValue(SmallChangeProperty, value); }
		}

		public static readonly StyledProperty<double> LargeChangeProperty =
			AvaloniaProperty.Register<NumericUpDown, double>(nameof(LargeChange), 10.0);

		public double LargeChange {
			get { return GetValue(LargeChangeProperty); }
			set { SetValue(LargeChangeProperty, value); }
		}

		bool IsDragging {
			get {
				return upButton?.IsDragging == true;
			}
			set {
				if (upButton != null) upButton.IsDragging = value;
				if (downButton != null) downButton.IsDragging = value;
			}
		}

		protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
		{
			base.OnApplyTemplate(e);

			upButton = e.NameScope.Find<DragRepeatButton>("PART_UpButton");
			downButton = e.NameScope.Find<DragRepeatButton>("PART_DownButton");
			textBox = e.NameScope.Find<TextBox>("PART_TextBox");

			if (upButton != null)
			{
				upButton.Click += upButton_Click;

				var upDrag = new DragListener(upButton);
				upDrag.Started += drag_Started;
				upDrag.Changed += drag_Changed;
				upDrag.Completed += drag_Completed;
			}

			if (downButton != null)
			{
				downButton.Click += downButton_Click;

				var downDrag = new DragListener(downButton);
				downDrag.Started += drag_Started;
				downDrag.Changed += drag_Changed;			
				downDrag.Completed += drag_Completed;
			}

			Print();
		}

		void drag_Started(DragListener drag)
		{
			OnDragStarted();
		}

		void drag_Changed(DragListener drag)
		{
			IsDragging = true;
			MoveValue(-drag.DeltaDelta.Y * SmallChange);
		}

		void drag_Completed(DragListener drag)
		{
			IsDragging = false;
			OnDragCompleted();
		}

		void downButton_Click(object sender, RoutedEventArgs e)
		{
			if (!IsDragging) SmallDown();
		}

		void upButton_Click(object sender, RoutedEventArgs e)
		{
			if (!IsDragging) SmallUp();
		}

		protected virtual void OnDragStarted()
		{
		}

		protected virtual void OnDragCompleted()
		{
		}

		public void SmallUp()
		{
			MoveValue(SmallChange);
		}

		public void SmallDown()
		{
			MoveValue(-SmallChange);
		}

		public void LargeUp()
		{
			MoveValue(LargeChange);
		}

		public void LargeDown()
		{
			MoveValue(-LargeChange);
		}

		void MoveValue(double delta)
		{
            if (!Value.HasValue)
                return;

            double result;
            if (double.IsNaN((double)Value) || double.IsInfinity((double)Value))
            {
                SetValueInternal(delta);
            }
            else if (textBox != null && double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                SetValueInternal(result + delta);
            }
            else
            {
                SetValueInternal((double)Value + delta);
            }
        }

		void Print()
		{
            if (textBox != null)
            {
                textBox.Text = Value?.ToString("F" + DecimalPlaces, CultureInfo.InvariantCulture);
                textBox.CaretIndex = int.MaxValue;
            }
        }

		void SetValueInternal(double? newValue)
		{
            newValue = CoerceValue(newValue);
            if (Value != newValue && !(Value.HasValue && double.IsNaN(Value.Value) && newValue.HasValue && double.IsNaN(newValue.Value)))
                Value = newValue;
        }

		double? CoerceValue(double? newValue)
		{
            if (!newValue.HasValue)
                return null;

            return Math.Max(Minimum, Math.Min((double)newValue, Maximum));
        }

		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);
			if (e.Key == Key.Enter) {
				double result;
				if (textBox != null && double.TryParse(textBox.Text, out result)) {
					SetValueInternal(result);
				}
				else {
					Print();
				}
				if (textBox != null)
					textBox.SelectAll();
				e.Handled = true;
			}
			else if (e.Key == Key.Up) {
				SmallUp();
				e.Handled = true;
			}
			else if (e.Key == Key.Down) {
				SmallDown();
				e.Handled = true;
			}
			else if (e.Key == Key.PageUp) {
				LargeUp();
				e.Handled = true;
			}
			else if (e.Key == Key.PageDown) {
				LargeDown();
				e.Handled = true;
			}
		}

        void SetInputValue()
        {
            if (textBox == null) return;
            
            double result;
            if (double.TryParse(textBox.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
            {
                SetValueInternal(result);
            }
            else
            {
                Print();
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);
			SetInputValue();
		}

		protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
		{
			if (e.Delta.Y > 0)
			{
				if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
				{
					LargeUp();
				}
				else
				{
					SmallUp();
				}
			}
			else
			{
				if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
				{
					LargeDown();
				}
				else
				{
					SmallDown();
				}
			}
			e.Handled = true;
		}

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);

            if (change.Property == ValueProperty)
            {
                var newValue = change.NewValue as double?;
                var coercedValue = CoerceValue(newValue);
                
                if (coercedValue != newValue)
                {
                    SetCurrentValue(ValueProperty, coercedValue);
                }
                Print();
            }
            else if (change.Property == SmallChangeProperty && 
                     !IsSet(LargeChangeProperty))
            {
                LargeChange = SmallChange * 10;
            }
        }
	}

	public class DragRepeatButton : RepeatButton
	{
		public static readonly StyledProperty<bool> IsDraggingProperty =
			AvaloniaProperty.Register<DragRepeatButton, bool>(nameof(IsDragging));

		public bool IsDragging {
			get { return GetValue(IsDraggingProperty); }
			set { SetValue(IsDraggingProperty, value); }
		}
	}
}
