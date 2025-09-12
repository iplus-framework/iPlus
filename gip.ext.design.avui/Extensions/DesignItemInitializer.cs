// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.avui.Extensions
{
	/// <summary>
	/// Instead of "DefaultInitializer" wich is only called for new objects, 
	/// this Initializer is called for every Instance of a Design Item
	/// </summary>
	[ExtensionServer(typeof(NeverApplyExtensionsExtensionServer))]
    public abstract class DesignItemInitializer : Extension
	{
		/// <summary>
		/// Initializes the design item.
		/// </summary>
		public abstract void InitializeDesignItem(DesignItem item);
	}
}