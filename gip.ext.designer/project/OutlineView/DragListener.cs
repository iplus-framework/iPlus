// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace gip.ext.designer.OutlineView
{
	public class DragListener
	{
		public DragListener(FrameworkElement target)
		{
			this.target = target;
			target.AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(MouseButtonDown), true);
			target.PreviewMouseMove += MouseMove;
			target.PreviewMouseLeftButtonUp += MouseLeftButtonUp;
		}

		public event MouseButtonEventHandler DragStarted;

		FrameworkElement target;
		Point startPoint;
		bool ready;
		MouseButtonEventArgs args;

		void MouseButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left && Mouse.Captured == null) {
				ready = true;
				startPoint = e.GetPosition(target);
				args = e;
				target.CaptureMouse();
			}
		}

		void MouseMove(object sender, MouseEventArgs e)
		{
			if (ready) {
				var currentPoint = e.GetPosition(target);
				if (Math.Abs(currentPoint.X - startPoint.X) >= SystemParameters.MinimumHorizontalDragDistance ||
					Math.Abs(currentPoint.Y - startPoint.Y) >= SystemParameters.MinimumVerticalDragDistance) {
					ready = false;
					if (DragStarted != null) {
						DragStarted(this, args);
					}
				}
			}
		}

		void MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ready = false;
			target.ReleaseMouseCapture();
		}
	}
}
