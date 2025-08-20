// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.avui.Extensions
{
	/// <summary>
	/// Attribute to specify that the decorated class is a WPF extension for the specified item type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
	public sealed class ExtensionDisabledAttribute : Attribute
	{
        Type _DisabledExtension;
		
		/// <summary>
		/// Gets/Sets the type of another extension that this extension is overriding.
		/// </summary>
		public Type DisabledExtension 
        {
			get { return _DisabledExtension; }
		}

		/// <summary>
		/// Create a new ExtensionForAttribute that specifies that the decorated class
		/// is a WPF extension for the specified item type.
		/// </summary>
        public ExtensionDisabledAttribute(Type disabledExtension)
		{
			if (disabledExtension == null)
                throw new ArgumentNullException("disabledExtension");
			if (!disabledExtension.IsClass)
                throw new ArgumentException("disabledExtension must be a class");
            _DisabledExtension = disabledExtension;
		}
	}
}
