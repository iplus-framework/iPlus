// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.avui.Extensions
{
	/// <summary>
	/// Base class for extensions that initialize new controls with default values.
	/// </summary>
	[ExtensionServer(typeof(NeverApplyExtensionsExtensionServer))]
	public abstract class DefaultInitializer : Extension
	{
		/// <summary>
		/// Initializes the design item to default values.
		/// </summary>
		public abstract void InitializeDefaults(DesignItem item);
	}
}
