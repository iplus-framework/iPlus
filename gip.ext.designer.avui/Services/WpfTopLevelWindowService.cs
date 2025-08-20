// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Services
{
	sealed class WpfTopLevelWindowService : ITopLevelWindowService
	{
		public ITopLevelWindow GetTopLevelWindow(System.Windows.UIElement element)
		{
			Window window = Window.GetWindow(element);
			if (window != null)
				return new WpfTopLevelWindow(window);
			else
				return null;
		}
		
		sealed class WpfTopLevelWindow : ITopLevelWindow
		{
			Window window;
			
			public WpfTopLevelWindow(Window window)
			{
				this.window = window;
			}
			
			public void SetOwner(Window child)
			{
				child.Owner = window;
			}
			
			public bool Activate()
			{
				return window.Activate();
			}
		}
	}
}
