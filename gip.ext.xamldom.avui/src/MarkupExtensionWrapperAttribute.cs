// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.xamldom.avui
{
	/// <summary>
	/// Apply this to markup extensions that needs custom behavior of <see cref="System.Windows.Markup.MarkupExtension.ProvideValue"/> in the designer.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class MarkupExtensionWrapperAttribute : Attribute
	{
		/// <summary>
		/// Initializes a new instance of <see cref="MarkupExtensionWrapperAttribute"/>.
		/// </summary>
		/// <param name="markupExtensionWrapperType">The wrapper type.</param>
		public MarkupExtensionWrapperAttribute(Type markupExtensionWrapperType)
		{
			MarkupExtensionWrapperType = markupExtensionWrapperType;
		}
		
		/// <summary>
		/// Gets the wrapper type.
		/// </summary>
		public Type MarkupExtensionWrapperType { get; private set; }
	}
}
