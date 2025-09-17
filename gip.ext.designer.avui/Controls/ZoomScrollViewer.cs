// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Styling;
using Avalonia.Threading;
using System;
using System.Globalization;

namespace gip.ext.designer.avui.Controls
{
	public class ZoomScrollViewer : ScrollViewer
	{
		static ZoomScrollViewer()
		{
			// No need for DefaultStyleKeyProperty.OverrideMetadata in Avalonia
		}

        //public bool EnableHorizontalWheelSupport
        //{
        //    get { return (bool)GetValue(EnableHorizontalWheelSupportProperty); }
        //    set { SetValue(EnableHorizontalWheelSupportProperty, value); }
        //}

        //public static readonly StyledProperty<bool> EnableHorizontalWheelSupportProperty =
        //    AvaloniaProperty.Register<ZoomScrollViewer, bool>("EnableHorizontalWheelSupport", false);


        //private static void EnableHorizontalWheelSupportChanged(AvaloniaObject sender, AvaloniaPropertyChangedEventArgs e)
        //{
        //    var ctl = sender as ZoomScrollViewer;
        //    if ((bool)e.NewValue == false)
        //    {
        //        {
        //            MouseHorizontalWheelEnabler.RemoveMouseHorizontalWheelHandler(ctl, ctl.OnMouseHorizontalWheel);
        //        }
        //    }
        //    else
        //    {
        //        {
        //            MouseHorizontalWheelEnabler.AddMouseHorizontalWheelHandler(ctl, ctl.OnMouseHorizontalWheel);
        //        }
        //    }
        //}

        public static readonly StyledProperty<double> CurrentZoomProperty =
			AvaloniaProperty.Register<ZoomScrollViewer, double>("CurrentZoom", 1.0, 
				defaultBindingMode: BindingMode.TwoWay, 
				coerce: CoerceZoom);
		
		public double CurrentZoom {
			get { return GetValue(CurrentZoomProperty); }
			set { SetValue(CurrentZoomProperty, value); }
		}
		
		static double CoerceZoom(AvaloniaObject sender, double baseValue)
		{
			var zoom = baseValue;
			ZoomScrollViewer sv = (ZoomScrollViewer)sender;
			return Math.Max(sv.MinimumZoom, Math.Min(sv.MaximumZoom, zoom));
		}
		
		public static readonly StyledProperty<double> MinimumZoomProperty =
			AvaloniaProperty.Register<ZoomScrollViewer, double>("MinimumZoom", 0.2);
		
		public double MinimumZoom {
			get { return GetValue(MinimumZoomProperty); }
			set { SetValue(MinimumZoomProperty, value); }
		}
		
		public static readonly StyledProperty<double> MaximumZoomProperty =
			AvaloniaProperty.Register<ZoomScrollViewer, double>("MaximumZoom", 5.0);
		
		public double MaximumZoom {
			get { return GetValue(MaximumZoomProperty); }
			set { SetValue(MaximumZoomProperty, value); }
		}
		
		public static readonly StyledProperty<bool> MouseWheelZoomProperty =
			AvaloniaProperty.Register<ZoomScrollViewer, bool>("MouseWheelZoom", true);
		
		public bool MouseWheelZoom {
			get { return GetValue(MouseWheelZoomProperty); }
			set { SetValue(MouseWheelZoomProperty, value); }
		}
		
		public static readonly StyledProperty<bool> AlwaysShowZoomButtonsProperty =
			AvaloniaProperty.Register<ZoomScrollViewer, bool>("AlwaysShowZoomButtons", false);
		
		public bool AlwaysShowZoomButtons {
			get { return GetValue(AlwaysShowZoomButtonsProperty); }
			set { SetValue(AlwaysShowZoomButtonsProperty, value); }
		}
		
		static readonly DirectProperty<ZoomScrollViewer, bool> ComputedZoomButtonCollapsedProperty =
			AvaloniaProperty.RegisterDirect<ZoomScrollViewer, bool>("ComputedZoomButtonCollapsed",
				o => o._computedZoomButtonCollapsed,
				(o, v) => o._computedZoomButtonCollapsed = v);
		
		private bool _computedZoomButtonCollapsed = true;
		
		public bool ComputedZoomButtonCollapsed {
			get { return _computedZoomButtonCollapsed; }
			private set { SetAndRaise(ComputedZoomButtonCollapsedProperty, ref _computedZoomButtonCollapsed, value); }
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);

			if (change.Property == CurrentZoomProperty || change.Property == AlwaysShowZoomButtonsProperty)
			{
				CalculateZoomButtonCollapsed();
			}
		}
		
		void CalculateZoomButtonCollapsed()
		{
			ComputedZoomButtonCollapsed = (AlwaysShowZoomButtons == false) && (CurrentZoom == 1.0);
		}
		
		protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
		{
			if (!e.Handled && e.KeyModifiers == KeyModifiers.Control && MouseWheelZoom) {
				double oldZoom = CurrentZoom;
				double newZoom = RoundToOneIfClose(CurrentZoom * Math.Pow(1.001, e.Delta.Y * 120)); // Convert to WPF-like delta
				newZoom = Math.Max(this.MinimumZoom, Math.Min(this.MaximumZoom, newZoom));
				
				// adjust scroll position so that mouse stays over the same virtual coordinate
				ContentPresenter presenter = this.FindNameScope()?.Find("PART_Presenter") as ContentPresenter;
				Vector relMousePos;
				if (presenter != null) {
					Point mousePos = e.GetPosition(presenter);
					relMousePos = new Vector(mousePos.X / presenter.Bounds.Width, mousePos.Y / presenter.Bounds.Height);
				} else {
					relMousePos = new Vector(0.5, 0.5);
				}
				
				Point scrollOffset = new Point(this.Offset.X, this.Offset.Y);
				Vector oldHalfViewport = new Vector(this.Viewport.Width / 2, this.Viewport.Height / 2);
				Vector newHalfViewport = oldHalfViewport / newZoom * oldZoom;
				Point oldCenter = scrollOffset + oldHalfViewport;
				Point virtualMousePos = scrollOffset + new Vector(relMousePos.X * this.Viewport.Width, relMousePos.Y * this.Viewport.Height);
				
				// As newCenter, we want to choose a point between oldCenter and virtualMousePos. The more we zoom in, the closer
				// to virtualMousePos. We'll create the line x = oldCenter + lambda * (virtualMousePos-oldCenter).
				// On this line, we need to choose lambda between -1 and 1:
				// -1 = zoomed out completely
				//  0 = zoom unchanged
				// +1 = zoomed in completely
				// But the zoom factor (newZoom/oldZoom) we have is in the range [0,+Infinity].
				
				// Basically, I just played around until I found a function that maps this to [-1,1] and works well.
				// "f" is squared because otherwise the mouse simply stays over virtualMousePos, but I wanted virtualMousePos
				// to move towards the middle -> squaring f causes lambda to be closer to 1, giving virtualMousePos more weight
				// then oldCenter.
				
				double f = Math.Min(newZoom, oldZoom) / Math.Max(newZoom, oldZoom);
				double lambda = 1 - f*f;
				if (oldZoom > newZoom)
					lambda = -lambda;
				
				Point newCenter = oldCenter + lambda * (virtualMousePos - oldCenter);
				scrollOffset = newCenter - newHalfViewport;
				
				SetCurrentValue(CurrentZoomProperty, newZoom);
				
				// Use AvaloniaUI's Offset property instead of ScrollToHorizontalOffset/ScrollToVerticalOffset
				this.Offset = new Vector(scrollOffset.X, scrollOffset.Y);
				
				e.Handled = true;
			}
			base.OnPointerWheelChanged(e);
		}

        //private void OnMouseHorizontalWheel(object d, RoutedEventArgs e)
        //{
        //    if (KeyboardDevice.Instance?.KeyModifiers != KeyModifiers.Control)
        //    {
        //        var ea = e as MouseHorizontalWheelEventArgs;

        //        this.ScrollToHorizontalOffset(this.Offset.X + ea.HorizontalDelta);
        //    }
        //}

        internal static double RoundToOneIfClose(double val)
		{
			if (Math.Abs(val - 1.0) < 0.0001)
				return 1.0;
			else
				return val;
		}
	}
	
	sealed class IsNormalZoomConverter : IValueConverter
	{
		public static readonly IsNormalZoomConverter Instance = new IsNormalZoomConverter();
		
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (parameter is bool && (bool)parameter)
				return true;
			return ((double)value) == 1.0;
		}
		
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
