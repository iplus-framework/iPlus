// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.avui.PropertyGrid
{
	/// <summary>
	/// Attribute to specify that the decorated class is a editor for the specified property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=false)]
	public sealed class PropertyEditorAttribute : Attribute
	{
		readonly Type propertyDeclaringType;
		readonly string propertyName;
		
		/// <summary>
		/// Creates a new PropertyEditorAttribute that specifies that the decorated class is a editor
		/// for the "<paramref name="propertyDeclaringType"/>.<paramref name="propertyName"/>".
		/// </summary>
		public PropertyEditorAttribute(Type propertyDeclaringType, string propertyName)
		{
			if (propertyDeclaringType == null)
				throw new ArgumentNullException("propertyDeclaringType");
			if (propertyName == null)
				throw new ArgumentNullException("propertyName");
			this.propertyDeclaringType = propertyDeclaringType;
			this.propertyName = propertyName;
		}
		
		/// <summary>
		/// Gets the type that declares the property that the decorated editor supports.
		/// </summary>
		public Type PropertyDeclaringType {
			get { return propertyDeclaringType; }
		}
		
		/// <summary>
		/// Gets the name of the property that the decorated editor supports.
		/// </summary>
		public string PropertyName {
			get { return propertyName; }
		}
	}
}
