﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;

namespace gip.ext.xamldom
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
            return typeof(IList).IsAssignableFrom(type)
                || type.IsArray
                || typeof(IAddChild).IsAssignableFrom(type)
                || typeof(ResourceDictionary).IsAssignableFrom(type);
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
                if (!CanCollectionAdd(col, item.GetType())) return false;
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
                    addChild.AddText((string)newElement.GetValueFor(null));
                }
                else
                {
                    addChild.AddChild(newElement.GetValueFor(null));
                }
            }
            else if (collectionInstance is ResourceDictionary)
            {
                object val = newElement.GetValueFor(null);
                object key = newElement is XamlObject ? ((XamlObject)newElement).GetXamlAttribute("Key") : null;
                if (key == null)
                {
                    if (val is Style)
                        key = ((Style)val).TargetType;
                }
                ((ResourceDictionary)collectionInstance).Add(key, val);
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
        public static void Insert(Type collectionType, object collectionInstance, XamlPropertyValue newElement, int index)
        {
            if (collectionInstance is SetterBaseCollection) // Bescause Collection is sealed
            {
                if ((collectionInstance as SetterBaseCollection).IsSealed)
                    return;
            }
            else if (collectionInstance is TriggerCollection) // Bescause Collection is sealed
            {
                if ((collectionInstance as TriggerCollection).IsSealed)
                    return;
            }
            else if (collectionInstance is ConditionCollection) // Bescause Collection is sealed
            {
                if ((collectionInstance as ConditionCollection).IsSealed)
                    return;
            }
            collectionType.InvokeMember(
                "Insert", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance,
                null, collectionInstance,
                new object[] { index, newElement.GetValueFor(null) },
                CultureInfo.InvariantCulture);
        }

        static readonly Type[] RemoveAtParameters = { typeof(int) };

        /// <summary>
        /// Removes the item at the specified index of the collection.
        /// </summary>
        /// <returns>True if the removal succeeded, false if the collection type does not support RemoveAt.</returns>
        public static bool RemoveItemAt(Type collectionType, object collectionInstance, int index)
        {
            if (collectionInstance is SetterBaseCollection) // Bescause Collection is sealed
            {
                if ((collectionInstance as SetterBaseCollection).IsSealed)
                    return true;
            }
            else if (collectionInstance is TriggerCollection) // Bescause Collection is sealed
            {
                if ((collectionInstance as TriggerCollection).IsSealed)
                    return true;
            }
            else if (collectionInstance is ConditionCollection) // Bescause Collection is sealed
            {
                if ((collectionInstance as ConditionCollection).IsSealed)
                    return true;
            }
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
            if (collectionInstance is SetterBaseCollection) // Bescause Collection is sealed
            {
                if ((collectionInstance as SetterBaseCollection).IsSealed)
                    return;
            }
            else if (collectionInstance is TriggerCollection) // Bescause Collection is sealed
            {
                if ((collectionInstance as TriggerCollection).IsSealed)
                    return;
            }
            else if (collectionInstance is ConditionCollection) // Bescause Collection is sealed
            {
                if ((collectionInstance as ConditionCollection).IsSealed)
                    return;
            }
            collectionType.InvokeMember(
                "Remove", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance,
                null, collectionInstance,
                new object[] { item },
                CultureInfo.InvariantCulture);
        }
    }
}
