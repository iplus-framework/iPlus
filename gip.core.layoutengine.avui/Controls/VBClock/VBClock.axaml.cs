using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a control which display analog clock without dials.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine Steuerung dar, die eine analoge Uhr ohne Zifferblätter anzeigt.
    /// </summary>
	public partial class VBClock : UserControl
	{
        /// <summary>
        /// Creates a new instace of VBClock.
        /// </summary>
		public VBClock()
		{
			InitializeComponent();

			_canvas.DataContext = this;

			_timer = new DispatcherTimer();
			_timer.Interval = new TimeSpan( 0, 0, 0, 0, 100 );
			_timer.Tick += new EventHandler( _timer_Tick );
			_timer.Start();
		}

		static VBClock()
		{
			TimeInfoProperty = AvaloniaProperty.Register<VBClock, VBClockTimeInfo>
				( nameof(TimeInfo), defaultValue: null );

			HourAngleProperty = AvaloniaProperty.Register<VBClock, double>
				( nameof(HourAngle), defaultValue: 0.0 );
			
			MinuteAngleProperty = AvaloniaProperty.Register<VBClock, double>
				( nameof(MinuteAngle), defaultValue: 0.0 );
			
			SecondAngleProperty = AvaloniaProperty.Register<VBClock, double>
				( nameof(SecondAngle), defaultValue: 0.0 );
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
        /// Gets or sets the hour angle.
        /// </summary>
		public double HourAngle
		{
			get => GetValue( HourAngleProperty );
			set => SetValue( HourAngleProperty, value );
		}

        /// <summary>
        /// Gets or sets the minute angle.
        /// </summary>
		public double MinuteAngle
		{
			get => GetValue( MinuteAngleProperty );
			set => SetValue( MinuteAngleProperty, value );
		}

        /// <summary>
        /// Gets or sets the second angle.
        /// </summary>
		public double SecondAngle
		{
			get => GetValue( SecondAngleProperty );
			set => SetValue( SecondAngleProperty, value );
		}

        /// <summary>
        /// Handles a OnInitialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
		protected override void OnInitialized()
		{
			base.OnInitialized();

			for( int i = 0; i < 60; ++i )
			{
				Rectangle marker = new Rectangle();

				if( ( i % 5 ) == 0 )
				{
					marker.Width = 3;
					marker.Height = 8;
					marker.Fill = new SolidColorBrush( Color.FromArgb( 0xe0, 0xff, 0xff, 0xff ) );
					marker.Stroke = new SolidColorBrush( Color.FromArgb( 0x80, 0x33, 0x33, 0x33 ) );
					marker.StrokeThickness = 0.5;
				}
				else
				{
					marker.Width = 0.5;
					marker.Height = 3;
					marker.Fill = new SolidColorBrush( Color.FromArgb( 0x80, 0xff, 0xff, 0xff ) );
					marker.Stroke = null;
					marker.StrokeThickness = 0;
				}

				TransformGroup transforms = new TransformGroup();

				transforms.Children.Add( new TranslateTransform( -( marker.Width / 2 ), marker.Width / 2 - 40 - marker.Height ) );
				transforms.Children.Add( new RotateTransform( i * 6 ) );
				transforms.Children.Add( new TranslateTransform( 50, 50 ) );

				marker.RenderTransform = transforms;

				_markersCanvas.Children.Add( marker );
			}

			for( int i = 1; i <= 12; ++i )
			{
				TextBlock tb = new TextBlock();

				tb.Text = i.ToString();
				tb.TextAlignment = TextAlignment.Center;
				tb.Foreground = Brushes.White;
				tb.FontSize = 4;

				tb.RenderTransform = new ScaleTransform( 2, 2 );

				double r = 34;
				double angle = Math.PI * i * 30.0 / 180.0;
				double x = Math.Sin( angle ) * r + 50, y = -Math.Cos( angle ) * r + 50;

				Canvas.SetLeft( tb, x );
				Canvas.SetTop( tb, y );

				_markersCanvas.Children.Add( tb );
			}
		}

		private void _timer_Tick( object sender, EventArgs e )
		{
			VBClockTimeInfo ti = TimeInfo;

			if( ti == null )
			{
				HourAngle = MinuteAngle = SecondAngle = 0;
			}
			else
			{
				double hour = ti.GetAdjusted( DateTime.UtcNow ).Hour;
				double minute = ti.GetAdjusted( DateTime.UtcNow ).Minute;
				double second = ti.GetAdjusted( DateTime.UtcNow ).Second;

				double hourAngle = 30 * hour + minute / 2 + second / 120;
				double minuteAngle = 6 * minute + second / 10;
				double secondAngle = 6 * second;

				HourAngle = hourAngle;
				MinuteAngle = minuteAngle;
				SecondAngle = secondAngle;
			}
		}

        /// <summary>
        /// Represents styled properties for TimeInfo, HourAngle, MinuteAngle and SecondAngle.
        /// </summary>
		public static StyledProperty<VBClockTimeInfo> TimeInfoProperty;
		public static StyledProperty<double> HourAngleProperty, MinuteAngleProperty, SecondAngleProperty;

		private DispatcherTimer _timer = new DispatcherTimer();
	}
}
