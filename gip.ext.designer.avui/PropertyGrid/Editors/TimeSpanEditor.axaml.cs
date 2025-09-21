// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using gip.ext.design.avui.PropertyGrid;
using gip.ext.designer.avui.themes;


namespace gip.ext.designer.avui.PropertyGrid.Editors
{
	[TypeEditor(typeof(TimeSpan))]
	public partial class TimeSpanEditor : UserControl
	{
		public TimeSpanEditor()
		{
			InitializeComponent();
			DataContextChanged += NumberEditor_DataContextChanged;
		}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public PropertyNode PropertyNode
		{
			get { return DataContext as PropertyNode; }
		}

		void NumberEditor_DataContextChanged(object sender, EventArgs e)
		{
			if (PropertyNode == null)
				return;

            var designerValue = PropertyNode.DesignerValue;
            if (designerValue == null)
                return;

            var value = (TimeSpan)designerValue;


			if (value < TimeSpan.Zero)
			{
				this.Negative = true;
				value = value.Negate();
			}
			this.Days = value.Days;
			this.Hours = value.Hours;
			this.Minutes = value.Minutes;
			this.Seconds = value.Seconds;
			this.Milliseconds = value.Milliseconds;
		}

		private void UpdateValue()
		{
			var ts = new TimeSpan(this.Days, this.Hours, this.Minutes, this.Seconds, this.Milliseconds);
			if (this.Negative)
				ts = ts.Negate();
			PropertyNode.DesignerValue = ts;
		}

		public bool Negative
		{
			get { return GetValue(NegativeProperty); }
			set { SetValue(NegativeProperty, value); }
		}
		 
		// Using a StyledProperty as the backing store for Negative.  This enables animation, styling, binding, etc...
		public static readonly StyledProperty<bool> NegativeProperty =
			AvaloniaProperty.Register<TimeSpanEditor, bool>(nameof(Negative), false);

		private static void OnNegativePropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
		{
			var ctl = (TimeSpanEditor)d;
			ctl.UpdateValue();
		}


		public int Days
		{
			get { return GetValue(DaysProperty); }
			set { SetValue(DaysProperty, value); }
		}

		// Using a StyledProperty as the backing store for Days.  This enables animation, styling, binding, etc...
		public static readonly StyledProperty<int> DaysProperty =
			AvaloniaProperty.Register<TimeSpanEditor, int>(nameof(Days), 0);

		private static void OnDaysPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
		{
			var ctl = (TimeSpanEditor)d;
			
			ctl.UpdateValue();
		}

		public int Hours
		{
			get { return GetValue(HoursProperty); }
			set { SetValue(HoursProperty, value); }
		}

		// Using a StyledProperty as the backing store for Hours.  This enables animation, styling, binding, etc...
		public static readonly StyledProperty<int> HoursProperty =
			AvaloniaProperty.Register<TimeSpanEditor, int>(nameof(Hours), 0);

		private static void OnHoursPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
		{
			var ctl = (TimeSpanEditor) d;
			if (ctl.Hours > 23)
			{
				ctl.Days++;
				ctl.Hours = 0;
			}
			else if (ctl.Hours < 0)
			{
				ctl.Days--;
				ctl.Hours = 23;
			}

			ctl.UpdateValue();
		}

		public int Minutes
		{
			get { return GetValue(MinutesProperty); }
			set { SetValue(MinutesProperty, value); }
		}

		// Using a StyledProperty as the backing store for Minutes.  This enables animation, styling, binding, etc...
		public static readonly StyledProperty<int> MinutesProperty =
			AvaloniaProperty.Register<TimeSpanEditor, int>(nameof(Minutes), 0);

		private static void OnMinutesPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
		{
			var ctl = (TimeSpanEditor)d;
			if (ctl.Minutes > 59)
			{
				ctl.Hours++;
				ctl.Minutes = 0;
			}
			else if (ctl.Minutes < 0)
			{
				ctl.Hours--;
				ctl.Minutes = 59;
			}

			ctl.UpdateValue();
		}
		 
		public int Seconds
		{
			get { return GetValue(SecondsProperty); }
			set { SetValue(SecondsProperty, value); }
		}

		// Using a StyledProperty as the backing store for Seconds.  This enables animation, styling, binding, etc...
		public static readonly StyledProperty<int> SecondsProperty =
			AvaloniaProperty.Register<TimeSpanEditor, int>(nameof(Seconds), 0);

		private static void OnSecondsPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
		{
			var ctl = (TimeSpanEditor)d;
			if (ctl.Seconds > 59)
			{
				ctl.Minutes++;
				ctl.Seconds = 0;
			}
			else if (ctl.Seconds < 0)
			{
				ctl.Minutes--;
				ctl.Seconds = 59;
			}

			ctl.UpdateValue();
		}
		public int Milliseconds
		{
			get { return GetValue(MillisecondsProperty); }
			set { SetValue(MillisecondsProperty, value); }
		}

		// Using a StyledProperty as the backing store for Milliseconds.  This enables animation, styling, binding, etc...
		public static readonly StyledProperty<int> MillisecondsProperty =
			AvaloniaProperty.Register<TimeSpanEditor, int>(nameof(Milliseconds), 0);

		private static void OnMillisecondsPropertyChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
		{
			var ctl = (TimeSpanEditor)d;
			if (ctl.Milliseconds > 999)
			{
				ctl.Seconds++;
				ctl.Milliseconds = 0;
			}
			else if (ctl.Milliseconds < 0)
			{
				ctl.Seconds--;
				ctl.Milliseconds = 999;
			}

			ctl.UpdateValue();
		}

		static TimeSpanEditor()
		{
			NegativeProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => OnNegativePropertyChanged(x, e));
			DaysProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => OnDaysPropertyChanged(x, e));
			HoursProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => OnHoursPropertyChanged(x, e));
			MinutesProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => OnMinutesPropertyChanged(x, e));
			SecondsProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => OnSecondsPropertyChanged(x, e));
			MillisecondsProperty.Changed.AddClassHandler<TimeSpanEditor>((x, e) => OnMillisecondsPropertyChanged(x, e));
		}
	}
}