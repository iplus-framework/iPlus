// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using System.ComponentModel;
using Avalonia.Interactivity;

namespace gip.ext.designer.avui.PropertyGrid.Editors.BrushEditor
{
	public partial class GradientSlider : UserControl
	{
		//private Dragger strip;
		//private GradientItemsControl itemsControl;

		public GradientSlider()
		{
			InitializeComponent();
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
			
			//strip = this.FindControl<Dragger>("strip");
			//itemsControl = this.FindControl<GradientItemsControl>("itemsControl");

			this.Bind(SelectedStopProperty, new Binding("SelectedItem") {
				Source = itemsControl,
				Mode = BindingMode.TwoWay
			});

			if (strip != null)
			{
				strip.AddHandler(Thumb.DragStartedEvent, strip_DragStarted);
				strip.AddHandler(Thumb.DragDeltaEvent, strip_DragDelta);
			}
		}

		static GradientSlider()
		{
			Thumb.DragDeltaEvent.AddClassHandler<GradientSlider>((x, e) => x.OnThumbDragDelta(e));
			Thumb.DragStartedEvent.AddClassHandler<GradientSlider>((x, e) => x.OnThumbDragStarted(e));
			Thumb.DragCompletedEvent.AddClassHandler<GradientSlider>((x, e) => x.OnThumbDragCompleted(e));
		}

		GradientStop newStop;
		double startOffset;
		static bool isThumbDragInProgress = false;

		public static readonly StyledProperty<GradientBrush> BrushProperty =
			AvaloniaProperty.Register<GradientSlider, GradientBrush>(nameof(Brush));

		public GradientBrush Brush {
			get { return GetValue(BrushProperty); }
			set { SetValue(BrushProperty, value); }
		}

		public static readonly StyledProperty<GradientStop> SelectedStopProperty =
			AvaloniaProperty.Register<GradientSlider, GradientStop>(nameof(SelectedStop));

		public GradientStop SelectedStop {
			get { return GetValue(SelectedStopProperty); }
			set { SetValue(SelectedStopProperty, value); }
		}

		public static readonly StyledProperty<BindingList<GradientStop>> GradientStopsProperty =
			AvaloniaProperty.Register<GradientSlider, BindingList<GradientStop>>(nameof(GradientStops));

		public BindingList<GradientStop> GradientStops {
			get { return GetValue(GradientStopsProperty); }
			set { SetValue(GradientStopsProperty, value); }
		}

		public static Color GetColorAtOffset(IList<GradientStop> stops, double offset)
		{
			GradientStop s1 = stops[0], s2 = stops.Last();
			foreach (var item in stops) {
				if (item.Offset < offset && item.Offset > s1.Offset) s1 = item;
				if (item.Offset > offset && item.Offset < s2.Offset) s2 = item;
			}
			return Color.FromArgb(
				(byte)((s1.Color.A + s2.Color.A) / 2),
				(byte)((s1.Color.R + s2.Color.R) / 2),
				(byte)((s1.Color.G + s2.Color.G) / 2),
				(byte)((s1.Color.B + s2.Color.B) / 2)
			);
		}

		private void OnThumbDragDelta(VectorEventArgs e)
		{
			thumb_DragDelta(this, e);
		}

		private void OnThumbDragStarted(VectorEventArgs e)
		{
			isThumbDragInProgress = true;
		}

		private void OnThumbDragCompleted(VectorEventArgs e)
		{
			isThumbDragInProgress = false;
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			
			if (change.Property == BrushProperty && !isThumbDragInProgress)
			{
				if (Brush != null) {
					GradientStops = new BindingList<GradientStop>(Brush.GradientStops);
					if (SelectedStop == null)
						SelectedStop = GradientStops.FirstOrDefault();
				}
				else {
					GradientStops = null;
				}
			}
		}

		void strip_DragStarted(object sender, VectorEventArgs e)
		{
			if (strip != null && GradientStops != null)
			{
				startOffset = e.Vector.X / strip.Bounds.Width;
				newStop = new GradientStop(GetColorAtOffset(GradientStops, startOffset), startOffset);
				GradientStops.Add(newStop);
				SelectedStop = newStop;
				e.Handled = true;
			}
		}

		void strip_DragDelta(object sender, VectorEventArgs e)
		{
			if (newStop != null)
			{
				MoveStop(newStop, startOffset, e);
				e.Handled = true;
			}
		}

		void thumb_DragDelta(object sender, VectorEventArgs e)
		{
			var stop = (e.Source as GradientThumb)?.GradientStop;
			if (stop != null)
			{
				MoveStop(stop, stop.Offset, e);
			}
		}

		void MoveStop(GradientStop stop, double oldOffset, VectorEventArgs e)
		{
			if (e.Vector.Y > 50 && GradientStops != null && GradientStops.Count > 2) {
				GradientStops.Remove(stop);
				SelectedStop = GradientStops.FirstOrDefault();
				return;
			}
			try
			{
				if (strip != null)
				{
					double newOffset = oldOffset + e.Vector.X / strip.Bounds.Width;
					stop.Offset = Math.Max(0, Math.Min(1, newOffset));
				}
			}
			catch (Exception ec)
			{
				string msg = ec.Message;
				if (ec.InnerException != null && ec.InnerException.Message != null)
					msg += " Inner:" + ec.InnerException.Message;

				if (core.datamodel.Database.Root != null && core.datamodel.Database.Root.Messages != null
												   && core.datamodel.Database.Root.InitState == core.datamodel.ACInitState.Initialized)
					core.datamodel.Database.Root.Messages.LogException("GradientSlider", "MoveStop", msg);
			}
		}
	}

	public class GradientItemsControl : SelectingItemsControl
	{
		protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
		{
			return new GradientThumb();
		}

		protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
		{
			recycleKey = null;
			return !(item is GradientThumb);
		}
	}

	public class GradientThumb : Thumb
	{
		public GradientStop GradientStop {
			get { return DataContext as GradientStop; }
		}

		protected override void OnPointerPressed(PointerPressedEventArgs e)
		{
			base.OnPointerPressed(e);
			var itemsControl = ItemsControl.ItemsControlFromItemContainer(this) as GradientItemsControl;
			if (itemsControl != null)
				itemsControl.SelectedItem = GradientStop;
		}
	}

	public class Dragger : Thumb
	{
	}
}
