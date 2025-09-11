// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Controls.Documents;
using System.Collections.Generic;
using Avalonia.Collections;
using System.Linq;

namespace gip.ext.xamldom.avui
{
    /// <summary>
    /// Static class containing helper methods to work with collections (like the XamlParser does)
    /// </summary>
    public static class CollectionSupport
    {
        /// <summary>
        /// Gets if the type is considered a collection in XAML.
        /// </summary>
        public static bool IsCollectionType(Type type)
        {
            return type != typeof(LineBreak) && (
                typeof(IList).IsAssignableFrom(type)
                || type.IsArray
                || typeof(IAddChild).IsAssignableFrom(type)
                || typeof(IResourceDictionary).IsAssignableFrom(type));
        }

        /// <summary>
        /// Gets if the collection type <paramref name="col"/> can accepts items of type
        /// <paramref name="item"/>.
        /// </summary>
        public static bool CanCollectionAdd(Type col, Type item)
        {
            var e = col.GetInterface("IEnumerable`1");
            if (e != null && e.IsGenericType)
            {
                var a = e.GetGenericArguments()[0];
                return a.IsAssignableFrom(item);
            }
            return true;
        }

        /// <summary>
        /// Gets if the collection type <paramref name="col"/> can accept the specified items.
        /// </summary>
        public static bool CanCollectionAdd(Type col, IEnumerable items)
        {
            foreach (var item in items)
            {
                if (!CanCollectionAdd(col, item.GetType())) 
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Adds a value to the end of a collection.
        /// </summary>
        public static void AddToCollection(Type collectionType, object collectionInstance, XamlPropertyValue newElement)
        {
            if (collectionInstance == null)
                return;
            IAddChild addChild = collectionInstance as IAddChild;
            if (addChild != null)
            {
                if (newElement is XamlTextValue)
                {
                    throw new NotSupportedException(
                        "Adding text values to a collection is not supported in Avalonia.");
                    // Avalonia Deprecated:
                    // addChild.AddText((string)newElement.GetValueFor(null));
                }
                else
                {
                    addChild.AddChild(newElement.GetValueFor(null));
                }
            }
            else if (collectionInstance is IResourceDictionary)
            {
                object val = newElement.GetValueFor(null);
                object key = newElement is XamlObject ? ((XamlObject)newElement).GetXamlAttribute("Key") : null;
                if (key == null || (key as string) == "")
                {
                    if (val is ControlTheme)
                        key = ((ControlTheme)val).TargetType;
                }
                if (key == null || (key as string) == "")
                    key = val;
                ((IDictionary)collectionInstance).Add(key, val);
            }
            else
            {
                collectionType.InvokeMember(
                    "Add", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance,
                    null, collectionInstance,
                    new object[] { newElement.GetValueFor(null) },
                    CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Adds a value at the specified index in the collection.
        /// </summary>
        public static bool Insert(Type collectionType, object collectionInstance, XamlPropertyValue newElement, int index)
        {
            object value = newElement.GetValueFor(null);

            // Using IList, with possible Add instead of Insert, was primarily added as a workaround
            // for a peculiarity (or bug) with collections inside System.Windows.Input namespace.
            // See CollectionTests.InputCollectionsPeculiarityOrBug test method for details.
            var list = collectionInstance as IList;
            if (list != null)
            {
                if (list.Count == index)
                {
                    list.Add(value);
                }
                else
                {
                    list.Insert(index, value);
                }
                return true;
            }
            else
            {
                var hasInsert = collectionType.GetMethods().Any(x => x.Name == "Insert");

                if (hasInsert)
                {
                    collectionType.InvokeMember(
                        "Insert", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance,
                        null, collectionInstance,
                        new object[] { index, value },
                        CultureInfo.InvariantCulture);

                    return true;
                }
            }

            return false;

            //if (collectionInstance is List<SetterBase>)
            //{
            //    //if ((collectionInstance as List<SetterBase>).IsSealed)
            //    //    return;
            //}
            //else if (collectionInstance is AvaloniaList<AvaloniaObject>)
            //{
            //    //if ((collectionInstance as AvaloniaList<AvaloniaObject>).IsSealed)
            //    //    return;
            //}

            //collectionType.InvokeMember(
            //    "Insert", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance,
            //    null, collectionInstance,
            //    new object[] { index, newElement.GetValueFor(null) },
            //    CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Adds a value at the specified index in the collection. A return value indicates whether the Insert succeeded.
        /// </summary>
        /// <returns>True if the Insert succeeded, false if the collection type does not support Insert.</returns>
        internal static bool TryInsert(Type collectionType, object collectionInstance, XamlPropertyValue newElement, int index)
        {
            try
            {
                return Insert(collectionType, collectionInstance, newElement, index);
            }
            catch (MissingMethodException)
            {
                return false;
            }
        }

        static readonly Type[] RemoveAtParameters = { typeof(int) };

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <returns>True if the removal succeeded, false if the collection type does not support RemoveAt.</returns>
        public static bool RemoveItemAt(Type collectionType, object collectionInstance, int index)
        {
            //if (collectionInstance is List<SetterBase>)
            //{
            //    //if ((collectionInstance as SetterBaseCollection).IsSealed)
            //    //    return true;
            //}
            //else if (collectionInstance is AvaloniaList<AvaloniaObject> Collection)
            //{
            //    //if ((collectionInstance as TriggerCollection).IsSealed)
            //    //    return true;
            //}

            MethodInfo m = collectionType.GetMethod("RemoveAt", RemoveAtParameters);
            if (m != null)
            {
                m.Invoke(collectionInstance, new object[] { index });
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Removes an item instance from the specified collection.
        /// </summary>
        public static void RemoveItem(Type collectionType, object collectionInstance, object item)
        {
            //if (collectionInstance is List<SetterBase>)
            //{
            //    //if ((collectionInstance as SetterBaseCollection).IsSealed)
            //    //    return true;
            //}
            //else if (collectionInstance is AvaloniaList<AvaloniaObject> Collection)
            //{
            //    //if ((collectionInstance as TriggerCollection).IsSealed)
            //    //    return true;
            //}
            collectionType.InvokeMember(
                "Remove", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance,
                null, collectionInstance,
                new object[] { item },
                CultureInfo.InvariantCulture);
        }


        internal static void RemoveItem(Type collectionType, object collectionInstance, object item, XamlPropertyValue element)
        {
            var dictionary = collectionInstance as IDictionary;
            var xamlObject = element as XamlObject;

            if (dictionary != null && xamlObject != null)
            {
                dictionary.Remove(xamlObject.GetXamlAttribute("Key"));
            }
            else
            {
                RemoveItem(collectionType, collectionInstance, item);
            }
        }
    }
}
