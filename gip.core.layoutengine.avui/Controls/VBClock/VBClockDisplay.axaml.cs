using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a control which display analog clock.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine Steuerung dar, die eine analoge Uhr anzeigt.
    /// </summary>
    public partial class VBClockDisplay : UserControl
	{
        /// <summary>
        /// Creates a new instance of VBClockDisplay.
        /// </summary>
		public VBClockDisplay()
		{
			InitializeComponent();

			_panel.DataContext = this;

			_timer = new DispatcherTimer();
			_timer.Interval = new TimeSpan( 0, 0, 1 );
			_timer.Tick += new EventHandler( _timer_Tick );
			_timer.Start();
		}

		static VBClockDisplay()
		{
			TimeInfoProperty.Changed.AddClassHandler<VBClockDisplay>((sender, e) => TimeInfo_Changed(sender, e));
		}

        /// <summary>
        /// Gets or sets the TimeInfo.
        /// </summary>
		public VBClockTimeInfo TimeInfo
		{
			get => GetValue( TimeInfoProperty );
			set => SetValue( TimeInfoProperty, value );
		}

        /// <summary>
        /// Gets or sets the Now property.
        /// </summary>
		public DateTime Now
		{
			get => GetValue( NowProperty );
			set => SetValue( NowProperty, value );
		}

        /// <summary>
        /// Gets or sets the zoom of clock.
        /// </summary>
		public double ClockZoom
		{
			get => GetValue( ClockZoomProperty );
			set => SetValue( ClockZoomProperty, value );
		}

        /// <summary>
        /// Gets or sets the zoom of text.
        /// </summary>
		public double TextZoom
		{
			get => GetValue( TextZoomProperty );
			set => SetValue( TextZoomProperty, value );
		}

		private void _timer_Tick( object sender, EventArgs e )
		{
			VBClockTimeInfo ti = TimeInfo;
            if (ti == null)
            {
                ti = new VBClockTimeInfo(TimeZoneInfo.Local);
                TimeInfo = ti;
            }
			if( ti != null )
			{
				Now = TimeInfo.GetAdjusted( DateTime.UtcNow );
			}
		}

		private static void TimeInfo_Changed( VBClockDisplay sender, AvaloniaPropertyChangedEventArgs e )
		{
			VBClockTimeInfo ti = sender.TimeInfo;

			if( ti != null )
			{
				sender.Now = ti.GetAdjusted( DateTime.UtcNow );
			}
		}

        /// <summary>
        /// Represents styled properties for TimeInfo, Now, ClockZoom, TextZoom.
        /// </summary>
		public static readonly StyledProperty<VBClockTimeInfo> TimeInfoProperty = AvaloniaProperty.Register<VBClock, VBClockTimeInfo> (nameof(TimeInfo), defaultValue: null);
		public static readonly StyledProperty<DateTime> NowProperty = AvaloniaProperty.Register<VBClockDisplay, DateTime> (nameof(Now), defaultValue: default(DateTime));
        public static readonly StyledProperty<double> ClockZoomProperty = AvaloniaProperty.Register<VBClockDisplay, double>(nameof(ClockZoom), defaultValue: 1.0);
        public static readonly StyledProperty<double> TextZoomProperty = AvaloniaProperty.Register<VBClockDisplay, double>(nameof(TextZoom), defaultValue: 1.0 );

		private DispatcherTimer _timer;
	}
}
