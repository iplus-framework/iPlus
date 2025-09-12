// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.avui.Extensions
{
	/// <summary>
	/// Attribute to specify Properties of the Extension.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
	public sealed class ExtensionAttribute : Attribute
	{
		/// <summary>
		/// Gets or sets the Order in wich the extensions are used.
		/// </summary>
		public int Order { get; set; }
	}
}
