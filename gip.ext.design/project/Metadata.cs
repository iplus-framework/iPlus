﻿// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;

namespace gip.ext.design
{
	/// <summary>
	/// Contains helper methods for retrieving meta data.
	/// </summary>
	public static class Metadata
	{
		/// <summary>
		/// Gets the full name of a dependency property (OwnerType.FullName + "." + Name).
		/// </summary>
		public static string GetFullName(this DependencyProperty p)
		{
			return p.OwnerType.FullName + "." + p.Name;
		}

		// TODO: do we really want to store these values in a static dictionary?
		// Why not per-design context (as a service?)
		static Dictionary<Type, List<object>> standardValues = new Dictionary<Type, List<object>>();

		/// <summary>
		/// Registers a set of standard values for a <paramref name="type"/> by using the
		/// public static properties of the type <paramref name="valuesContainer"/>.
		/// </summary>
		/// <example>Metadata.AddStandardValues(typeof(Brush), typeof(Brushes));</example>
		public static void AddStandardValues(Type type, Type valuesContainer)
		{
			AddStandardValues(type,
			                  valuesContainer.GetProperties(BindingFlags.Public | BindingFlags.Static)
			                  .Select(p => p.GetValue(null, null)));
		}

		/// <summary>
		/// Registers a set of standard <paramref name="values"/> for a <paramref name="type"/>.
		/// </summary>
		/// <remarks>You can call this method multiple times to add additional standard values.</remarks>
		public static void AddStandardValues<T>(Type type, IEnumerable<T> values)
		{
			List<object> list;
			lock (standardValues) {
				if (!standardValues.TryGetValue(type, out list)) {
					list = new List<object>();
					standardValues[type] = list;
				}
				foreach (var v in values) {
					list.Add(v);
				}
			}
		}

		/// <summary>
		/// Retrieves the standard values for the specified <paramref name="type"/>.
		/// </summary>
		public static IEnumerable GetStandardValues(Type type)
		{
			if (type.IsEnum) {
				return Enum.GetValues(type);
			}
			List<object> values;
			lock (standardValues) {
				if (standardValues.TryGetValue(type, out values)) {
					return values;
				}
			}
			return null;
		}

		//static Dictionary<string, string> categories = new Dictionary<string, string>();

		//public static void AddCategory(DependencyProperty p, string category)
		//{
		//    lock (categories) {
		//        categories[p.GetFullName()] = category;
		//    }
		//}

		//public static void AddCategory(Type type, string property, string category)
		//{
		//    lock (categories) {
		//        categories[type + "." + property] = category;
		//    }
		//}

		//public static string GetCategory(DesignItemProperty p)
		//{
		//    string result;
		//    lock (categories) {
		//        if (categories.TryGetValue(p.DependencyFullName, out result)) {
		//            return result;
		//        }
		//    }
		//    return p.Category;
		//}

		//static HashSet<string> advancedProperties = new HashSet<string>();

		//public static void AddAdvancedProperty(DependencyProperty p)
		//{
		//    lock (advancedProperties) {
		//        advancedProperties.Add(p.GetFullName());
		//    }
		//}

		//public static void AddAdvancedProperty(Type type, string member)
		//{
		//    lock (advancedProperties) {
		//        advancedProperties.Add(type.FullName + "." + member);
		//    }
		//}

		//public static bool IsAdvanced(DesignItemProperty p)
		//{
		//    lock (advancedProperties) {
		//        if (advancedProperties.Contains(p.DependencyFullName)) {
		//            return true;
		//        }
		//    }
		//    return p.IsAdvanced;
		//}

		static HashSet<string> hiddenProperties = new HashSet<string>();

		/// <summary>
		/// Hides the specified property (marks it as not browsable).
		/// </summary>
		public static void HideProperty(DependencyProperty p)
		{
			lock (hiddenProperties) {
				hiddenProperties.Add(p.GetFullName());
			}
		}

		/// <summary>
		/// Hides the specified property (marks it as not browsable).
		/// </summary>
		public static void HideProperty(Type type, string member)
		{
			lock (hiddenProperties) {
				hiddenProperties.Add(type.FullName + "." + member);
			}
		}

		/// <summary>
		/// Gets whether the specified property is browsable (should be visible in property grids).
		/// </summary>
		public static bool IsBrowsable(DesignItemProperty p)
		{
			lock (hiddenProperties) {
				if (hiddenProperties.Contains(p.DependencyFullName)) {
					return false;
				}
			}
			return true;
		}

		//public static string[] CategoryOrder { get; set; }

		static HashSet<string> popularProperties = new HashSet<string>();

		/// <summary>
		/// Registers a popular property (shown first in the property grid).
		/// </summary>
		public static void AddPopularProperty(DependencyProperty p)
		{
			lock (popularProperties) {
				popularProperties.Add(p.GetFullName());
			}
		}

		/// <summary>
		/// Registers a popular property (shown first in the property grid).
		/// </summary>
		public static void AddPopularProperty(Type type, string member)
		{
			lock (popularProperties) {
				popularProperties.Add(type.FullName + "." + member);
			}
		}

		/// <summary>
		/// Gets whether the specified property was registered as popular.
		/// </summary>
		public static bool IsPopularProperty(DesignItemProperty p)
		{
			lock (popularProperties) {
				if (popularProperties.Contains(p.DependencyFullName)) {
					return true;
				}
			}
			return false;
		}

		static HashSet<Type> popularControls = new HashSet<Type>();

		/// <summary>
		/// Registers a popular control (visible in the default toolbox).
		/// </summary>
		public static void AddPopularControl(Type t)
		{
			lock (popularControls) {
				popularControls.Add(t);
			}
		}

		/// <summary>
		/// Gets the list of popular controls.
		/// </summary>
		public static IEnumerable<Type> GetPopularControls()
		{
			lock (popularControls) {
				return popularControls.ToArray();
			}
		}

		/// <summary>
		/// Gets whether the specified control was registered as popular.
		/// </summary>
		public static bool IsPopularControl(Type t)
		{
			lock (popularControls) {
				return popularControls.Contains(t);
			}
		}

		static Dictionary<string, NumberRange> ranges = new Dictionary<string, NumberRange>();

		/// <summary>
		/// Registers the value range for the property.
		/// </summary>
		public static void AddValueRange(DependencyProperty p, double min, double max)
		{
			lock (ranges) {
				ranges[p.GetFullName()] = new NumberRange() { Min = min, Max = max };
			}
		}

		/// <summary>
		/// Gets the registered value range for the property, or null if no range was registered.
		/// </summary>
		public static NumberRange GetValueRange(DesignItemProperty p)
		{
			NumberRange r;
			lock (ranges) {
				if (ranges.TryGetValue(p.DependencyFullName, out r)) {
					return r;
				}
			}
			return null;
		}

		static HashSet<Type> placementDisabled = new HashSet<Type>();

		/// <summary>
		/// Disables the default placement behaviour (setting the ContentProperty) for the type.
		/// </summary>
		public static void DisablePlacement(Type type)
		{
			lock (placementDisabled) {
				placementDisabled.Add(type);
			}
		}

		/// <summary>
		/// Gets whether thr default placement behaviour (setting the ContentProperty) was disabled for the type.
		/// </summary>
		public static bool IsPlacementDisabled(Type type)
		{
			lock (placementDisabled) {
				return placementDisabled.Contains(type);
			}
		}

		static Dictionary<Type, Size> defaultSizes = new Dictionary<Type, Size>();

		/// <summary>
		/// Registers a default size for new controls of the specified type.
		/// </summary>
		public static void AddDefaultSize(Type t, Size s)
		{
			lock (defaultSizes) {
				defaultSizes[t] = s;
			}
		}

		/// <summary>
		/// Gets the default size for new controls of the specified type,
		/// or new Size(double.NaN, double.NaN) if no default size was registered.
		/// </summary>
		public static Size GetDefaultSize(Type t)
		{
			Size s;
			lock (defaultSizes) {
				while (t != null) {
					if (defaultSizes.TryGetValue(t, out s)) {
						return s;
					}
					t = t.BaseType;
				}
			}
			return new Size(double.NaN, double.NaN);
		}
	}

	/// <summary>
	/// Represets the minimum and maximum valid value for a double property.
	/// </summary>
	public class NumberRange
	{
		/// <summary>
		/// Gets/Sets the minimum value.
		/// </summary>
		public double Min { get; set; }
		
		/// <summary>
		/// Gets/Sets the maximum value.
		/// </summary>
		public double Max { get; set; }
	}
}
