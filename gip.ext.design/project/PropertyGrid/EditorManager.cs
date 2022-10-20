// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using gip.ext.design.PropertyGrid.Editors;

namespace gip.ext.design.PropertyGrid
{
    /// <summary>
    /// Manages registered type and property editors.
    /// </summary>
    public static class EditorManager
    {
        /// <summary>
        /// property return type => editor type 
        /// </summary>
        public static Dictionary<Type, Type> typeEditors = new Dictionary<Type, Type>();

        /// <summary>
        /// property full name => editor type
        /// </summary>
        public static Dictionary<string, Type> propertyEditors = new Dictionary<string, Type>();

        /// <summary>
        /// Creates a property editor for the specified <paramref name="property"/>
        /// </summary>
        public static FrameworkElement CreateEditor(DesignItemProperty property)
        {
            Type editorType;
            if (!propertyEditors.TryGetValue(property.FullName, out editorType))
            {
                var type = property.ReturnType;
                while (type != null)
                {
                    if (typeEditors.TryGetValue(type, out editorType))
                    {
                        break;
                    }
                    type = type.BaseType;
                }
                if (editorType == null)
                {
                    var standardValues = Metadata.GetStandardValues(property.ReturnType);
                    if (standardValues != null)
                    {
                        return new ComboBoxEditor() { ItemsSource = standardValues };
                    }
                    return new TextBoxEditor();
                }
            }
            FrameworkElement instance = (FrameworkElement)Activator.CreateInstance(editorType);
            if (instance is System.Windows.Controls.ComboBox)
            {
                var standardValues = Metadata.GetStandardValues(property.ReturnType);
                if (standardValues != null)
                {
                    (instance as System.Windows.Controls.ComboBox).ItemsSource = standardValues;
                }
            }
            return instance;
        }

        public static FrameworkElement CreateEditor(Type propertyType)
        {
            Type editorType = null;
            var type = propertyType;
            while (propertyType != null)
            {
                if (typeEditors.TryGetValue(propertyType, out editorType))
                {
                    break;
                }
                if (type == null)
                    break;
                type = type.BaseType;
            }
            if (editorType == null)
            {
                var standardValues = Metadata.GetStandardValues(propertyType);
                if (standardValues != null)
                {
                    return new ComboBoxEditor() { ItemsSource = standardValues };
                }
                return new TextBoxEditor();
            }
            FrameworkElement instance = (FrameworkElement)Activator.CreateInstance(editorType);
            if (instance is System.Windows.Controls.ComboBox)
            {
                var standardValues = Metadata.GetStandardValues(propertyType);
                if (standardValues != null)
                {
                    (instance as System.Windows.Controls.ComboBox).ItemsSource = standardValues;
                }
            }
            return instance;
        }


        /// <summary>
        /// Registers property editors defined in the specified assembly.
        /// </summary>
        public static void RegisterAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            foreach (Type type in assembly.GetExportedTypes())
            {
                foreach (TypeEditorAttribute editorAttribute in type.GetCustomAttributes(typeof(TypeEditorAttribute), false))
                {
                    CheckValidEditor(type);
                    typeEditors[editorAttribute.SupportedPropertyType] = type;
                }
                foreach (PropertyEditorAttribute editorAttribute in type.GetCustomAttributes(typeof(PropertyEditorAttribute), false))
                {
                    CheckValidEditor(type);
                    string propertyName = editorAttribute.PropertyDeclaringType.FullName + "." + editorAttribute.PropertyName;
                    propertyEditors[propertyName] = type;
                }
            }
        }

        static void CheckValidEditor(Type type)
        {
            if (!typeof(FrameworkElement).IsAssignableFrom(type))
            {
                throw new DesignerException("Editor types must derive from FrameworkElement!");
            }
        }
    }
}
