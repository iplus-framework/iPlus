// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace gip.ext.designer.avui.Services
{
	public abstract class ChooseClassServiceBase
	{
		public Type ChooseClass()
		{
			return ChooseClass(null);
		}

		public Type ChooseClass(Window parentWindow)
		{
			var core = new ChooseClass(GetAssemblies());
			var window = new ChooseClassDialog(core);
			
			bool? result;
			if (parentWindow != null)
			{
				result = window.ShowDialog<bool?>(parentWindow).Result;
			}
			else
			{
				// Try to find the main window from the application
				var app = Avalonia.Application.Current;
				var mainWindow = app?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
					? desktop.MainWindow 
					: null;
				
				if (mainWindow != null)
				{
					result = window.ShowDialog<bool?>(mainWindow).Result;
				}
				else
				{
					// Fallback: show without parent (may not be modal)
					window.Show();
					result = true; // Assume success for non-modal case
				}
			}
			
			if (result == true) {
				return core.CurrentClass;
			}
			return null;
		}

		public abstract IEnumerable<Assembly> GetAssemblies();
	}
}
