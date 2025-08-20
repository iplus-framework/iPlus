// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.avui.Extensions
{
	/// <summary>
	/// Attribute to specify that the decorated class is an extension using the specified extension server.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
	public sealed class ExtensionServerAttribute : Attribute
	{
		Type _extensionServerType;
		
		/// <summary>
		/// Gets the type of the item that is designed using this extension.
		/// </summary>
		public Type ExtensionServerType {
			get { return _extensionServerType; }
		}
		
		/// <summary>
		/// Create a new ExtensionServerAttribute that specifies that the decorated extension
		/// uses the specified extension server.
		/// </summary>
		public ExtensionServerAttribute(Type extensionServerType)
		{
			if (extensionServerType == null)
				throw new ArgumentNullException("extensionServerType");
			if (!typeof(ExtensionServer).IsAssignableFrom(extensionServerType))
				throw new ArgumentException("extensionServerType must derive from ExtensionServer");
			if (extensionServerType.GetConstructor(new Type[0]) == null)
				throw new ArgumentException("extensionServerType must have a parameter-less constructor");
			_extensionServerType = extensionServerType;
		}
	}
}
