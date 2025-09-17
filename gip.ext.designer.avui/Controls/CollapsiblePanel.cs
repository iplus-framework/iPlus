// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using System;
using System.Globalization;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// Allows animated collapsing of the content of this panel.
	/// </summary>
	public class CollapsiblePanel : ContentControl
	{
		static CollapsiblePanel()
		{
			//DefaultStyleKeyProperty.OverrideMetadata(typeof(CollapsiblePanel),
			                                         //new StyledPropertyMetadata<Type>(typeof(CollapsiblePanel)));
			FocusableProperty.OverrideMetadata(typeof(CollapsiblePanel),
			                                   new StyledPropertyMetadata<bool>(false));
		}
		
		public static readonly StyledProperty<bool> IsCollapsedProperty = AvaloniaProperty.Register<CollapsiblePanel, bool>(
			nameof(IsCollapsed), false);
		
		public bool IsCollapsed {
			get { return GetValue(IsCollapsedProperty); }
			set { SetValue(IsCollapsedProperty, value); }
		}
		
		public static readonly StyledProperty<Orientation> CollapseOrientationProperty =
			AvaloniaProperty.Register<CollapsiblePanel, Orientation>(nameof(CollapseOrientation), Orientation.Vertical);
		
		public Orientation CollapseOrientation {
			get { return GetValue(CollapseOrientationProperty); }
			set { SetValue(CollapseOrientationProperty, value); }
		}
		
		public static readonly StyledProperty<TimeSpan> DurationProperty = AvaloniaProperty.Register<CollapsiblePanel, TimeSpan>(
			nameof(Duration), TimeSpan.FromMilliseconds(250));
		
		/// <summary>
		/// The duration in milliseconds of the animation.
		/// </summary>
		public TimeSpan Duration {
			get { return GetValue(DurationProperty); }
			set { SetValue(DurationProperty, value); }
		}
		
		protected internal static readonly StyledProperty<double> AnimationProgressProperty = AvaloniaProperty.Register<CollapsiblePanel, double>(
			nameof(AnimationProgress), 1.0);
		
		/// <summary>
		/// Value between 0 and 1 specifying how far the animation currently is.
		/// </summary>
		protected internal double AnimationProgress {
			get { return GetValue(AnimationProgressProperty); }
			set { SetValue(AnimationProgressProperty, value); }
		}
		
		protected internal static readonly StyledProperty<double> AnimationProgressXProperty = AvaloniaProperty.Register<CollapsiblePanel, double>(
			nameof(AnimationProgressX), 1.0);
		
		/// <summary>
		/// Value between 0 and 1 specifying how far the animation currently is.
		/// </summary>
		protected internal double AnimationProgressX {
			get { return GetValue(AnimationProgressXProperty); }
			set { SetValue(AnimationProgressXProperty, value); }
		}
		
		protected internal static readonly StyledProperty<double> AnimationProgressYProperty = AvaloniaProperty.Register<CollapsiblePanel, double>(
			nameof(AnimationProgressY), 1.0);
		
		/// <summary>
		/// Value between 0 and 1 specifying how far the animation currently is.
		/// </summary>
		protected internal double AnimationProgressY {
			get { return GetValue(AnimationProgressYProperty); }
			set { SetValue(AnimationProgressYProperty, value); }
		}
		
		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == IsCollapsedProperty)
			{
				SetupAnimation(change.GetNewValue<bool>());
			}
		}
		
		void SetupAnimation(bool isCollapsed)
		{
			if (this.IsLoaded) {
				// If the animation is already running, calculate remaining portion of the time
				double currentProgress = AnimationProgress;
				if (!isCollapsed) {
					currentProgress = 1.0 - currentProgress;
				}
				
				var duration = TimeSpan.FromSeconds(Duration.TotalSeconds * currentProgress);
				var targetValue = isCollapsed ? 0.0 : 1.0;
				
				var animation = new DoubleTransition
				{
					Property = AnimationProgressProperty,
					Duration = duration,
					Easing = Easing.Parse("Linear")
				};
				
				Transitions ??= new Transitions();
				Transitions.Clear();
				Transitions.Add(animation);
				
				if (CollapseOrientation == Orientation.Horizontal) {
					var animationX = new DoubleTransition
					{
						Property = AnimationProgressXProperty,
						Duration = duration,
						Easing = Easing.Parse("Linear")
					};
					Transitions.Add(animationX);
					this.AnimationProgressY = 1.0;
					this.AnimationProgressX = targetValue;
				} else {
					this.AnimationProgressX = 1.0;
					var animationY = new DoubleTransition
					{
						Property = AnimationProgressYProperty,
						Duration = duration,
						Easing = Easing.Parse("Linear")
					};
					Transitions.Add(animationY);
					this.AnimationProgressY = targetValue;
				}
				
				this.AnimationProgress = targetValue;
			} else {
				this.AnimationProgress = isCollapsed ? 0.0 : 1.0;
				this.AnimationProgressX = (CollapseOrientation == Orientation.Horizontal) ? this.AnimationProgress : 1.0;
				this.AnimationProgressY = (CollapseOrientation == Orientation.Vertical) ? this.AnimationProgress : 1.0;
			}
		}
	}
	
	public sealed class CollapsiblePanelProgressToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is double doubleValue)
				return doubleValue > 0 ? true : false;
			else
				return true;
		}
		
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	
	public class SelfCollapsingPanel : CollapsiblePanel
	{
		public static readonly StyledProperty<bool> CanCollapseProperty =
			AvaloniaProperty.Register<SelfCollapsingPanel, bool>(nameof(CanCollapse), false);
		
		public bool CanCollapse {
			get { return GetValue(CanCollapseProperty); }
			set { SetValue(CanCollapseProperty, value); }
		}
		
		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			base.OnPropertyChanged(change);
			if (change.Property == CanCollapseProperty)
			{
				bool newValue = change.GetNewValue<bool>();
				if (newValue) {
					if (!HeldOpenByMouse)
						IsCollapsed = true;
				} else {
					IsCollapsed = false;
				}
			}
		}
		
		bool HeldOpenByMouse {
			get { return IsPointerOver; }
		}
		
		protected override void OnPointerExited(PointerEventArgs e)
		{
			base.OnPointerExited(e);
			if (CanCollapse && !HeldOpenByMouse)
				IsCollapsed = true;
		}

        //protected override void OnLostMouseCapture(MouseEventArgs e)
        //{
        //    base.OnLostMouseCapture(e);
        //    if (CanCollapse && !HeldOpenByMouse)
        //        IsCollapsed = true;
        //}
    }
}
