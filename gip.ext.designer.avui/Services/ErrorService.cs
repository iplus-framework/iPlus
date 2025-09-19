// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia;
using Avalonia.Controls;
using gip.ext.designer.avui.Controls;
using gip.ext.design.avui;
using Avalonia.Input;

namespace gip.ext.designer.avui.Services
{
	sealed class DefaultErrorService : IErrorService
	{
		sealed class AttachedErrorBalloon : ErrorBalloon
		{
			Control attachTo;
			
			public AttachedErrorBalloon(Control attachTo, Control errorElement)
			{
				this.attachTo = attachTo;
				this.Content = errorElement;
			}
			
			internal void AttachEvents()
			{
				attachTo.Unloaded += OnCloseEvent;
				attachTo.KeyDown += OnCloseEvent;
				attachTo.PointerPressed += OnCloseEvent;
				attachTo.LostFocus += OnCloseEvent;
			}
			
			void OnCloseEvent(object sender, EventArgs e)
			{
				this.Close();
			}

            protected override void OnClosing(WindowClosingEventArgs e)
            {
				attachTo.Unloaded -= OnCloseEvent;
				attachTo.KeyDown -= OnCloseEvent;
				attachTo.PointerPressed -= OnCloseEvent;
				attachTo.LostFocus -= OnCloseEvent;
				base.OnClosing(e);
			}

            protected override void OnPointerPressed(PointerPressedEventArgs e)
            {
				Close();
			}
		}
		
		ServiceContainer services;
		
		public DefaultErrorService(DesignContext context)
		{
			this.services = context.Services;
		}
		
		public void ShowErrorTooltip(Control attachTo, Control errorElement)
		{
			if (attachTo == null)
				throw new ArgumentNullException("attachTo");
			if (errorElement == null)
				throw new ArgumentNullException("errorElement");
			
			AttachedErrorBalloon b = new AttachedErrorBalloon(attachTo, errorElement);
            PixelPoint pos = attachTo.PointToScreen(new Point(0, attachTo.Bounds.Height));
			pos = new PixelPoint(pos.X, pos.Y - 8);
			b.Position = pos;
			b.Focusable = false;
			ITopLevelWindowService windowService = services.GetService<ITopLevelWindowService>();
			ITopLevelWindow ownerWindow = (windowService != null) ? windowService.GetTopLevelWindow(attachTo) : null;
			if (ownerWindow != null) {
				ownerWindow.SetOwner(b);
			}
			b.Show();
			
			if (ownerWindow != null) {
				ownerWindow.Activate();
			}
			
			b.AttachEvents();
		}
	}
}
