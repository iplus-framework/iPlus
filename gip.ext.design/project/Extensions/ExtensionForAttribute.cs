﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.Extensions
{
	/// <summary>
	/// Attribute to specify that the decorated class is a WPF extension for the specified item type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
	public sealed class ExtensionForAttribute : Attribute
	{
		Type _designedItemType;
		Type _overrideExtension;
		
		/// <summary>
		/// Gets the type of the item that is designed using this extension.
		/// </summary>
		public Type DesignedItemType {
			get { return _designedItemType; }
		}
		
		/// <summary>
		/// Gets/Sets the type of another extension that this extension is overriding.
		/// </summary>
		public Type OverrideExtension {
			get { return _overrideExtension; }
			set {
				_overrideExtension = value;
				if (value != null) {
					if (!typeof(Extension).IsAssignableFrom(value)) {
						throw new ArgumentException("OverrideExtension must specify the type of an Extension.");
					}
				}
			}
		}
		
		/// <summary>
		/// Create a new ExtensionForAttribute that specifies that the decorated class
		/// is a WPF extension for the specified item type.
		/// </summary>
		public ExtensionForAttribute(Type designedItemType)
		{
			if (designedItemType == null)
				throw new ArgumentNullException("designedItemType");
			if (!designedItemType.IsClass)
				throw new ArgumentException("designedItemType must be a class");
			_designedItemType = designedItemType;
		}
	}
}
