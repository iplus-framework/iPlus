// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Data;

namespace gip.ext.design.avui.PropertyGrid
{
    /// <summary>
    /// Manages extension creation for a design context.
    /// </summary>
    public static class ConverterManager
    {
        #region Manage ExtensionEntries
        /// <summary>
        /// 
        /// </summary>
        public sealed class ConverterEntry
        {
            public readonly Type ConverterType;
            public readonly Type TargetPropertyType;
            public readonly Type SourcePropertyType;
            public readonly bool IsMultiValueConverter;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="extensionType"></param>
            /// <param name="targetPropertyType"></param>
            /// <param name="sourcePropertyType"></param>
            public ConverterEntry(Type extensionType, Type targetPropertyType, Type sourcePropertyType)
            {
                this.ConverterType = extensionType;
                this.TargetPropertyType = targetPropertyType;
                this.SourcePropertyType = sourcePropertyType;
                IsMultiValueConverter = this.ConverterType.GetInterface("IMultiValueConverter") != null;
            }

            public string ConverterName
            {
                get
                {
                    return ConverterType.Name;
                }
            }
        }

        private static Dictionary<Type, List<ConverterEntry>> _converters = new Dictionary<Type, List<ConverterEntry>>();

        public static IEnumerable<ConverterEntry> GetConverterList(bool forMultiBinding)
        {
            List<ConverterEntry> converterList = new List<ConverterEntry>();
            foreach (List<ConverterEntry> entry in _converters.Values)
            {
                foreach (ConverterEntry converter in entry)
                {
                    if ((forMultiBinding && converter.IsMultiValueConverter)
                        || (!forMultiBinding && !converter.IsMultiValueConverter))
                        converterList.Add(converter);
                }
            }
            return converterList;
        }

        private static void AddExtensionConverterEntry(Type targetPropertyType, ConverterEntry entry)
        {
            List<ConverterEntry> list;
            if (!_converters.TryGetValue(targetPropertyType, out list))
            {
                list = _converters[targetPropertyType] = new List<ConverterEntry>();
            }
            if (list.Where(c => c.ConverterType == entry.ConverterType).Any())
                return;
            list.Add(entry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPropertyType"></param>
        /// <param name="multiValueConverter"></param>
        /// <returns></returns>
        public static ConverterEntry GetConverterEntry(Type targetPropertyType, bool multiValueConverter)
        {
            List<ConverterEntry> list;
            if (_converters.TryGetValue(targetPropertyType, out list))
            {
                return list.Where(c => c.IsMultiValueConverter == multiValueConverter).FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPropertyType"></param>
        /// <returns></returns>
        public static List<ConverterEntry> GetConverters(Type targetPropertyType)
        {
            List<ConverterEntry> result;
            if (targetPropertyType.BaseType != null)
                result = GetConverters(targetPropertyType.BaseType);
            else
                result = new List<ConverterEntry>();

            List<ConverterEntry> list;
            if (_converters.TryGetValue(targetPropertyType, out list))
            {
                foreach (ConverterEntry entry in list)
                {
                    if (entry.TargetPropertyType != null)
                    {
                        result.RemoveAll(delegate(ConverterEntry oldEntry)
                        {
                            return oldEntry.ConverterType == entry.TargetPropertyType;
                        });
                    }
                    result.Add(entry);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets all the types of all extensions that are applied to the specified item type.
        /// </summary>
        public static IEnumerable<Type> GetConverterTypes(Type targetPropertyType)
        {
            if (targetPropertyType == null)
                throw new ArgumentNullException("extendedItemType");
            foreach (ConverterEntry entry in GetConverters(targetPropertyType))
            {
                yield return entry.ConverterType;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPropertyType"></param>
        /// <returns></returns>
        public static IValueConverter CreateValueConverter(Type targetPropertyType)
        {
            ConverterEntry entry = GetConverterEntry(targetPropertyType, false);
            if (entry == null)
                return null;
            return Activator.CreateInstance(entry.ConverterType) as IValueConverter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPropertyType"></param>
        /// <returns></returns>
        public static IMultiValueConverter CreateMultiValueConverter(Type targetPropertyType)
        {
            ConverterEntry entry = GetConverterEntry(targetPropertyType, true);
            if (entry == null)
                return null;
            return Activator.CreateInstance(entry.ConverterType) as IMultiValueConverter;
        }

        #endregion


        #region RegisterAssembly
        /// <summary>
        /// Registers extensions from the specified assembly.
        /// </summary>
        public static void RegisterAssembly(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            foreach (Type type in assembly.GetTypes())
            {
                object[] extensionForAttributes = type.GetCustomAttributes(typeof(ConverterAttribute), true);
                if (extensionForAttributes.Length == 0)
                    continue;

                foreach (ConverterAttribute designerFor in extensionForAttributes)
                {
                    AddExtensionConverterEntry(designerFor.TargetType, new ConverterEntry(type, designerFor.TargetType, designerFor.SourceType));
                }
            }
        }
        #endregion

    }
}
