using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace gip.core.layoutengine
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
			TimeInfoProperty = DependencyProperty.Register
				( "TimeInfo", typeof( VBClockTimeInfo ), typeof( VBClockDisplay )
				, new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.AffectsRender, TimeInfo_Changed ) );

			NowProperty = DependencyProperty.Register
				( "Now", typeof( DateTime ), typeof( VBClockDisplay ) );

			ClockZoomProperty = DependencyProperty.Register
				( "ClockZoom", typeof( double ), typeof( VBClockDisplay )
				, new FrameworkPropertyMetadata( 1.0, FrameworkPropertyMetadataOptions.AffectsRender ) );

			TextZoomProperty = DependencyProperty.Register
				( "TextZoom", typeof( double ), typeof( VBClockDisplay )
				, new FrameworkPropertyMetadata( 1.0, FrameworkPropertyMetadataOptions.AffectsRender ) );
		}

        /// <summary>
        /// Gets or sets the TimeInfo.
        /// </summary>
		public VBClockTimeInfo TimeInfo
		{
			get
			{
				return (VBClockTimeInfo) GetValue( TimeInfoProperty );
			}
			set
			{
				SetValue( TimeInfoProperty, value );
			}
		}

        /// <summary>
        /// Gets or sets the Now property.
        /// </summary>
		public DateTime Now
		{
			get
			{
				return (DateTime) GetValue( NowProperty );
			}
			set
			{
				SetValue( NowProperty, value );
			}
		}

        /// <summary>
        /// Gets or sets the zoom of clock.
        /// </summary>
		public double ClockZoom
		{
			get
			{
				return (double) GetValue( ClockZoomProperty );
			}
			set
			{
				SetValue( ClockZoomProperty, value );
			}
		}

        /// <summary>
        /// Gets or sets the zoom of text.
        /// </summary>
		public double TextZoom
		{
			get
			{
				return (double) GetValue( TextZoomProperty );
			}
			set
			{
				SetValue( TextZoomProperty, value );
			}
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

		private static void TimeInfo_Changed( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			VBClockTimeInfo ti = (VBClockTimeInfo) d.GetValue( TimeInfoProperty );

			if( ti != null )
			{
				d.SetValue( NowProperty, ti.GetAdjusted( DateTime.UtcNow ) );
			}
		}

        /// <summary>
        /// Represents a dependency properties for TimeInfo, Now, ClockZoom, TextZoom.
        /// </summary>
		public static DependencyProperty TimeInfoProperty, NowProperty, ClockZoomProperty, TextZoomProperty;

		private DispatcherTimer _timer;
	}
}
