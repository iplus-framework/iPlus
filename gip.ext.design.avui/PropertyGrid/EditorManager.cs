// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace gip.ext.design.avui.PropertyGrid
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

        static Type defaultComboboxEditor = null; //typeof(ComboBoxEditor);

        static Type defaultTextboxEditor = null; //typeof(TextBoxEditor);

        /// <summary>
        /// Creates a property editor for the specified <paramref name="property"/>
        /// </summary>
        public static Control CreateEditor(DesignItemProperty property)
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

                foreach (var t in typeEditors)
                {
                    if (t.Key.IsAssignableFrom(property.ReturnType))
                    {
                        return (Control)Activator.CreateInstance(t.Value);
                    }
                }

                if (editorType == null)
                {
                    IEnumerable standardValues = null;
                    if (property.DependencyProperty != null)
                    {
                        standardValues = Metadata.GetStandardValues(property.DependencyProperty);
                    }
                    if (standardValues == null)
                    {
                        standardValues = Metadata.GetStandardValues(property.ReturnType);
                    }

                    if (standardValues != null)
                    {
                        var itemsControl = (ItemsControl)Activator.CreateInstance(defaultComboboxEditor);
                        itemsControl.ItemsSource = standardValues;
                        if (Nullable.GetUnderlyingType(property.ReturnType) != null)
                        {
                            itemsControl.GetType().GetProperty("IsNullable").SetValue(itemsControl, true, null); //In this Class we don't know the Nullable Combo Box
                        }
                        return itemsControl;
                        //return new ComboBoxEditor() { ItemsSource = standardValues };
                    }
                    var namedStandardValues = Metadata.GetNamedStandardValues(property.ReturnType);
                    if (namedStandardValues != null)
                    {
                        SelectingItemsControl itemsControl = (SelectingItemsControl)Activator.CreateInstance(defaultComboboxEditor);
                        itemsControl.ItemsSource = namedStandardValues;
                        itemsControl.SelectedValueBinding = new Avalonia.Data.Binding("Value");
                        itemsControl.DisplayMemberBinding = new Avalonia.Data.Binding("Name");
                        //itemsControl.DisplayMemberPath = "Name";
                        //itemsControl.SelectedValuePath = "Value";
                        if (Nullable.GetUnderlyingType(property.ReturnType) != null)
                        {
                            itemsControl.GetType().GetProperty("IsNullable").SetValue(itemsControl, true, null); //In this Class we don't know the Nullable Combo Box
                        }
                        return itemsControl;
                    }
                    return (Control)Activator.CreateInstance(defaultTextboxEditor);
                }
            }
            Control instance = (Control)Activator.CreateInstance(editorType);
            if (instance is SelectingItemsControl)
            {
                var standardValues = Metadata.GetStandardValues(property.ReturnType);
                if (standardValues != null)
                {
                    (instance as SelectingItemsControl).ItemsSource = standardValues;
                }
            }
            return instance;
        }

        public static Control CreateEditor(Type propertyType)
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
                    var itemsControl = (ItemsControl)Activator.CreateInstance(defaultComboboxEditor);
                    itemsControl.ItemsSource = standardValues;
                    if (Nullable.GetUnderlyingType(propertyType) != null)
                    {
                        itemsControl.GetType().GetProperty("IsNullable").SetValue(itemsControl, true, null); //In this Class we don't know the Nullable Combo Box
                    }
                    return itemsControl;
                }
                var namedStandardValues = Metadata.GetNamedStandardValues(propertyType);
                if (namedStandardValues != null)
                {
                    SelectingItemsControl itemsControl = (SelectingItemsControl)Activator.CreateInstance(defaultComboboxEditor);
                    itemsControl.ItemsSource = namedStandardValues;
                    itemsControl.SelectedValueBinding = new Avalonia.Data.Binding("Value");
                    itemsControl.DisplayMemberBinding = new Avalonia.Data.Binding("Name");
                    //itemsControl.DisplayMemberPath = "Name";
                    //itemsControl.SelectedValuePath = "Value";
                    if (Nullable.GetUnderlyingType(propertyType) != null)
                    {
                        itemsControl.GetType().GetProperty("IsNullable").SetValue(itemsControl, true, null); //In this Class we don't know the Nullable Combo Box
                    }
                    return itemsControl;
                }
                return (Control)Activator.CreateInstance(defaultTextboxEditor);
            }
            Control instance = (Control)Activator.CreateInstance(editorType);
            if (instance is SelectingItemsControl)
            {
                var standardValues = Metadata.GetStandardValues(propertyType);
                if (standardValues != null)
                {
                    (instance as SelectingItemsControl).ItemsSource = standardValues;
                }
            }
            return instance;
        }

        /// <summary>
        /// Registers the Textbox Editor.
        /// </summary>
        public static void SetDefaultTextBoxEditorType(Type type)
        {
            defaultTextboxEditor = type;
        }

        /// <summary>
        /// Registers the Combobox Editor.
        /// </summary>
        public static void SetDefaultComboBoxEditorType(Type type)
        {
            defaultComboboxEditor = type;
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
            if (!typeof(Control).IsAssignableFrom(type))
            {
                throw new DesignerException("Editor types must derive from FrameworkElement!");
            }
        }
    }
}
