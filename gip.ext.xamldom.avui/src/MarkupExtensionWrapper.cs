// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace gip.ext.xamldom.avui
{
	/// <summary>
	/// A wrapper for markup extensions if custom behavior of <see cref="System.Windows.Markup.MarkupExtension.ProvideValue"/> is needed in the designer.
	/// </summary>
	public abstract class MarkupExtensionWrapper
	{
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="xamlObject">The <see cref="XamlObject"/> object that represents the markup extension.</param>
		protected MarkupExtensionWrapper(XamlObject xamlObject)
		{
			if (xamlObject == null) {
				throw new ArgumentNullException("xamlObject");
			}
			
			XamlObject = xamlObject;
		}
		
		/// <summary>
		/// Gets the <see cref="XamlObject"/> object that represents the markup extension.
		/// </summary>
		public XamlObject XamlObject { get; set; }
		
		/// <summary>
		/// Returns an object that should be used as the value of the target property in the designer. 
		/// </summary>
		/// <returns>An object that should be used as the value of the target property in the designer.</returns>
		public abstract object ProvideValue();

        public abstract object GetClonedInstance();

        static readonly Dictionary<Type, Type> s_MarkupExtensionWrappers = new Dictionary<Type, Type>();

		/// <summary>
		/// Registers a markup extension wrapper.
		/// </summary>
		/// <param name="markupExtensionType">The type of the markup extension.</param>
		/// <param name="markupExtensionWrapperType">The type of the markup extension wrapper.</param>
		public static void RegisterMarkupExtensionWrapper(Type markupExtensionType, Type markupExtensionWrapperType)
		{
			if (markupExtensionType == null) {
				throw new ArgumentNullException("markupExtensionType");
			}
			
			if (!markupExtensionType.IsSubclassOf(typeof(MarkupExtension))) {
				throw new ArgumentException("The specified type must derive from MarkupExtension.", "markupExtensionType");
			}
			
			if (markupExtensionWrapperType == null) {
				throw new ArgumentNullException("markupExtensionWrapperType");
			}
			
			if (!markupExtensionWrapperType.IsSubclassOf(typeof(MarkupExtensionWrapper))) {
				throw new ArgumentException("The specified type must derive from MarkupExtensionWrapper.", "markupExtensionWrapperType");
			}
			
			s_MarkupExtensionWrappers.Add(markupExtensionType, markupExtensionWrapperType);
		}
		
		internal static MarkupExtensionWrapper CreateWrapper(Type markupExtensionWrapperType, XamlObject xamlObject)
		{
			return Activator.CreateInstance(markupExtensionWrapperType, xamlObject) as MarkupExtensionWrapper;
		}
		
		internal static MarkupExtensionWrapper TryCreateWrapper(Type markupExtensionType, XamlObject xamlObject)
		{
			Type markupExtensionWrapperType;
			if (s_MarkupExtensionWrappers.TryGetValue(markupExtensionType, out markupExtensionWrapperType)) {
				return CreateWrapper(markupExtensionWrapperType, xamlObject);
			}
			
			return null;
		}
	}
}
