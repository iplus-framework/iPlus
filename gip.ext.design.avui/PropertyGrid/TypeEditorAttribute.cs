// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.avui.PropertyGrid
{
    public interface ITypeEditorInitItem
    {
        void LoadItemsCollection(DesignItem designObject);
    }

    public interface ITypeEditorInitCollection
    {
        void InitEditor(DesignItem designObject, DesignItemProperty collectionProperty);
    }

    /// <summary>
    /// Attribute to specify that the decorated class is a editor for properties with the specified
    /// return type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class TypeEditorAttribute : Attribute
    {
        readonly Type supportedPropertyType;

        /// <summary>
        /// Creates a new TypeEditorAttribute that specifies that the decorated class is a editor
        /// for properties with the return type "<paramref name="supportedPropertyType"/>".
        /// </summary>
        public TypeEditorAttribute(Type supportedPropertyType)
        {
            if (supportedPropertyType == null)
                throw new ArgumentNullException("supportedPropertyType");
            this.supportedPropertyType = supportedPropertyType;
        }

        /// <summary>
        /// Gets the supported property type.
        /// </summary>
        public Type SupportedPropertyType
        {
            get { return supportedPropertyType; }
        }
    }
}
