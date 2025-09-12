// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using Avalonia;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Services
{
	sealed class DefaultViewService : ViewService
	{
		readonly DesignContext context;
		
		public DefaultViewService(DesignContext context)
		{
			this.context = context;
		}
		
		public override DesignItem GetModel(AvaloniaObject view)
		{
			// In the WPF designer, we do not support having a different view for a component
			return context.Services.Component.GetDesignItem(view);
		}
	}
}
