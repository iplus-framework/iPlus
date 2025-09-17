// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Reactive;
using gip.ext.design.avui.Adorners;

namespace gip.ext.designer.avui.Controls
{
	/// <summary>
	/// Description of MultiPointThumb.
	/// </summary>
	public class PointThumb : DesignerThumb
	{
		public Transform InnerRenderTransform
		{
			get { return (Transform)GetValue(InnerRenderTransformProperty); }
			set { SetValue(InnerRenderTransformProperty, value); }
		}

		// Using an AvaloniaProperty as the backing store for InnerRenderTransform.  This enables animation, styling, binding, etc...
		public static readonly StyledProperty<Transform> InnerRenderTransformProperty =
			AvaloniaProperty.Register<PointThumb, Transform>("InnerRenderTransform", null);

		public bool IsEllipse
		{
			get { return (bool)GetValue(IsEllipseProperty); }
			set { SetValue(IsEllipseProperty, value); }
		}

		// Using an AvaloniaProperty as the backing store for IsEllipse.  This enables animation, styling, binding, etc...
		public static readonly StyledProperty<bool> IsEllipseProperty =
			AvaloniaProperty.Register<PointThumb, bool>("IsEllipse", false);

		public Point Point
		{
			get { return (Point)GetValue(PointProperty); }
			set { SetValue(PointProperty, value); }
		}

		// Using an AvaloniaProperty as the backing store for Point.  This enables animation, styling, binding, etc...
		public static readonly StyledProperty<Point> PointProperty =
			AvaloniaProperty.Register<PointThumb, Point>("Point", default(Point), false, Avalonia.Data.BindingMode.TwoWay);

		
		static PointThumb()
		{
			PointProperty.Changed.AddClassHandler<PointThumb>((x, e) => OnPointChanged(x, e));
			//This OverrideDefaultValue call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			//DefaultStyleKeyProperty.OverrideDefaultValue<PointThumb>(typeof(PointThumb));
		}

		private static void OnPointChanged(AvaloniaObject d, AvaloniaPropertyChangedEventArgs e)
		{
			var pt = (PointThumb)d;
			((PointPlacementSupport)pt.AdornerPlacement).p = (Point)e.NewValue;
            pt.GetValue(PointThumb.RelativeToPointProperty);
            BindingOperations.GetBindingExpressionBase(pt, PointThumb.RelativeToPointProperty)?.UpdateTarget();
			((PointThumb)d).ReDraw();
		}
		
		public Point? RelativeToPoint
		{
			get { return (Point?)GetValue(RelativeToPointProperty); }
			set { SetValue(RelativeToPointProperty, value); }
		}

		// Using an AvaloniaProperty as the backing store for RelativeToPoint.  This enables animation, styling, binding, etc...
		public static readonly StyledProperty<Point?> RelativeToPointProperty =
			AvaloniaProperty.Register<PointThumb, Point?>("RelativeToPoint", null);
		
		public PointThumb(Point point)
		{
			this.AdornerPlacement = new PointPlacementSupport(point);
			Point = point;
		}

		public PointThumb()
		{
			this.AdornerPlacement = new PointPlacementSupport(Point);
		}

		public AdornerPlacement AdornerPlacement { get; private set; }
		
		private class PointPlacementSupport : AdornerPlacement
		{
			public Point p;
			public PointPlacementSupport(Point point)
			{
				this.p = point;
			}

			public override void Arrange(AdornerPanel panel, Control adorner, Size adornedElementSize)
			{
				adorner.Arrange(new Rect(p.X, p.Y, adornedElementSize.Width, adornedElementSize.Height));
			}
		}
	}
}
