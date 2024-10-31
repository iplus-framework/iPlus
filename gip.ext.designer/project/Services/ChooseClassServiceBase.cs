// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace gip.ext.designer.Services
{
	public abstract class ChooseClassServiceBase
	{
		public Type ChooseClass()
		{
			var core = new ChooseClass(GetAssemblies());
			var window = new ChooseClassDialog(core);
			
			if (window.ShowDialog().Value) {
				return core.CurrentClass;
			}
			return null;
		}

		public abstract IEnumerable<Assembly> GetAssemblies();
	}
}
