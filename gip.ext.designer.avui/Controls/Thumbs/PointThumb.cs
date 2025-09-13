// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System.Windows;
using System.Windows.Media;
using gip.ext.design.avui.Adorners;
using System.Windows.Data;

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

		// Using a DependencyProperty as the backing store for InnerRenderTransform.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty InnerRenderTransformProperty =
			DependencyProperty.Register("InnerRenderTransform", typeof(Transform), typeof(PointThumb), new PropertyMetadata(null));

		public bool IsEllipse
		{
			get { return (bool)GetValue(IsEllipseProperty); }
			set { SetValue(IsEllipseProperty, value); }
		}

		// Using a DependencyProperty as the backing store for IsEllipse.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty IsEllipseProperty =
			DependencyProperty.Register("IsEllipse", typeof(bool), typeof(PointThumb), new PropertyMetadata(false));

		public Point Point
		{
			get { return (Point)GetValue(PointProperty); }
			set { SetValue(PointProperty, value); }
		}

		// Using a DependencyProperty as the backing store for Point.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty PointProperty =
			DependencyProperty.Register("Point", typeof(Point), typeof(PointThumb), new PropertyMetadata(OnPointChanged));

		private static void OnPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var pt = (PointThumb)d;
			((PointPlacementSupport)pt.AdornerPlacement).p = (Point)e.NewValue;
			var bndExpr = pt.GetBindingExpression(PointThumb.RelativeToPointProperty);
			if (bndExpr != null)
				bndExpr.UpdateTarget();
			((PointThumb)d).ReDraw();
		}
		
		public Point? RelativeToPoint
		{
			get { return (Point?)GetValue(RelativeToPointProperty); }
			set { SetValue(RelativeToPointProperty, value); }
		}

		// Using a DependencyProperty as the backing store for RelativeToPoint.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty RelativeToPointProperty =
			DependencyProperty.Register("RelativeToPoint", typeof(Point?), typeof(PointThumb), new PropertyMetadata(null));
		
		static PointThumb()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			DefaultStyleKeyProperty.OverrideMetadata(typeof(PointThumb), new FrameworkPropertyMetadata(typeof(PointThumb)));
		}
		
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

			public override void Arrange(AdornerPanel panel, UIElement adorner, Size adornedElementSize)
			{
				adorner.Arrange(new Rect(p.X, p.Y, adornedElementSize.Width, adornedElementSize.Height));
			}
		}
	}
}
