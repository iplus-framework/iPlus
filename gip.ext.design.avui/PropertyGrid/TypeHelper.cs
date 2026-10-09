// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Avalonia;

namespace gip.ext.design.avui.PropertyGrid
{
	/// <summary>
	/// Helper class with static methods to get the list of available properties/events.
	/// </summary>
	public static class TypeHelper
	{
		/// <summary>
		/// Gets the available properties common to all input types.
		/// </summary>
		/// <param name="types">List of input types. The list must have at least one element.</param>
		public static IEnumerable<PropertyDescriptor> GetCommonAvailableProperties(IEnumerable<Type> types)
		{
			foreach (var pd1 in GetAvailableProperties(types.First())) {
				bool propertyOk = true;
				foreach (var type in types.Skip(1)) {
					bool typeOk = false;
					foreach (var pd2 in GetAvailableProperties(type)) {
						if (pd1 == pd2) {
							typeOk = true;
							break;
						}
					}
					if (!typeOk) {
						propertyOk = false;
						break;
					}
				}
				if (propertyOk) yield return pd1;
			}
		}

		/// <summary>
		/// Gets the available properties for the type.
		/// </summary>
		public static IEnumerable<PropertyDescriptor> GetAvailableProperties(Type forType)
		{
			foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(forType)) {
				if (!p.IsBrowsable) continue;
				if (p.IsReadOnly) continue;
				if (p.Attributes.OfType<ObsoleteAttribute>().Count()!=0) continue;
				if (p.Name.Contains(".")) continue;
				yield return p;
			}
		}

		/// <summary>
		/// Gets the available events for the type.
		/// </summary>
		public static IEnumerable<EventDescriptor> GetAvailableEvents(Type forType)
		{
			foreach (EventDescriptor e in TypeDescriptor.GetEvents(forType)) {
				if (!e.IsBrowsable) continue;
				if (e.Attributes.OfType<ObsoleteAttribute>().Count()!=0) continue;
				if (e.Name.Contains(".")) continue;
				yield return e;
			}
		}

        private static string[] hiddenPropertiesOnWindow = new[] { "ClipToBounds" };
        /// <summary>
        /// Gets available properties for an object, includes attached properties also.
        /// If <paramref name="parentType"/> is given, only attached properties that make
        /// sense for that parent container are included (e.g. Canvas.Top only inside a Canvas).
        /// </summary>		
        public static IEnumerable<PropertyDescriptor> GetAvailableProperties(object element, bool withReadonly=false, Type parentType = null)
		{
			if (element.GetType().FullName == "gip.ext.designer.avui.Controls.WindowClone")
			{
				foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(element))
				{
					if (!p.IsBrowsable) continue;
					if (p.IsReadOnly && !typeof(ICollection).IsAssignableFrom(p.PropertyType)) continue;
					if (hiddenPropertiesOnWindow.Contains(p.Name)) continue;
					if (p.Attributes.OfType<ObsoleteAttribute>().Count() != 0) continue;
					yield return p;
				}
			}
			else
			{
				foreach (PropertyDescriptor p in TypeDescriptor.GetProperties(element))
				{
					if (!p.IsBrowsable) continue;
					if (p.IsReadOnly && !withReadonly) continue;
					if (p.Attributes.OfType<ObsoleteAttribute>().Count() != 0) continue;
					yield return p;
				}
			}

			// Unlike WPF, Avalonia's TypeDescriptor does not expose attached properties
			// (Canvas.Left, Grid.Row, ...). Add the registered attached layout properties
			// explicitly so that they can be edited in the property grid like in the WPF version.
			foreach (var attached in GetAttachedLayoutProperties(element, parentType))
				yield return attached;
		}

		/// <summary>
		/// Owner type names of the attached (layout) properties that should be shown in the property grid.
		/// </summary>
		public static readonly string[] AttachedLayoutPropertyOwners = new[]
		{
			"Canvas", "Grid", "DockPanel", "RelativePanel", "WrapPanel", "StackPanel"
		};

		static IEnumerable<PropertyDescriptor> GetAttachedLayoutProperties(object element, Type parentType)
		{
			var avaloniaObject = element as AvaloniaObject;
			if (avaloniaObject == null)
				yield break;

			var attachedProperties = AvaloniaPropertyRegistry.Instance.GetRegisteredAttached(avaloniaObject.GetType());
			foreach (var property in attachedProperties)
			{
				if (!AttachedLayoutPropertyOwners.Contains(property.OwnerType.Name))
					continue;
				if (property.IsReadOnly)
					continue;
				// Only offer attached properties whose owner matches the parent container
				// (e.g. Canvas.Top only when the element is placed inside a Canvas).
				if (parentType != null && !IsOwnerOfParentType(property.OwnerType, parentType))
					continue;
				yield return new AttachedPropertyDescriptor(property);
			}
		}

		static bool IsOwnerOfParentType(Type ownerType, Type parentType)
		{
			for (var t = parentType; t != null; t = t.BaseType)
			{
				if (t == ownerType)
					return true;
			}
			return false;
		}

		/// <summary>
		/// PropertyDescriptor for an attached Avalonia property. The name is in the form
		/// "OwnerType.PropertyName" (e.g. "Canvas.Top") like the WPF TypeDescriptor provided it.
		/// </summary>
		public class AttachedPropertyDescriptor : PropertyDescriptor
		{
			readonly AvaloniaProperty _property;

			public AttachedPropertyDescriptor(AvaloniaProperty property)
				: base(property.OwnerType.Name + "." + property.Name, null)
			{
				_property = property;
			}

			public AvaloniaProperty AvaloniaProperty
			{
				get { return _property; }
			}

			public override Type ComponentType
			{
				get { return _property.OwnerType; }
			}

			public override Type PropertyType
			{
				get { return _property.PropertyType; }
			}

			public override bool IsReadOnly
			{
				get { return _property.IsReadOnly; }
			}

			public override bool IsBrowsable
			{
				get { return true; }
			}

			public override string Category
			{
				get { return "Layout"; }
			}

			public override string DisplayName
			{
				get { return Name; }
			}

			public override bool CanResetValue(object component)
			{
				return true;
			}

			public override void ResetValue(object component)
			{
				((AvaloniaObject)component).ClearValue(_property);
			}

			public override object GetValue(object component)
			{
				return ((AvaloniaObject)component).GetValue(_property);
			}

			public override void SetValue(object component, object value)
			{
				((AvaloniaObject)component).SetValue(_property, value);
			}

			public override bool ShouldSerializeValue(object component)
			{
				return true;
			}
		}
		
		/// <summary>
		/// Gets common properties between <paramref name="elements"/>. Includes attached properties too.
		/// </summary>
		/// <param name="elements"></param>
		/// <returns></returns>
		public static IEnumerable<PropertyDescriptor> GetCommonAvailableProperties(IEnumerable<object> elements)
		{
            var properties = TypeDescriptor.GetProperties(elements.First()).Cast<PropertyDescriptor>();
            foreach (var element in elements.Skip(1))
            {
                var currentProperties = TypeDescriptor.GetProperties(element).Cast<PropertyDescriptor>();
                properties = Enumerable.Intersect(properties, currentProperties);
            }

            return properties;
			// Old code:
            //foreach (var pd1 in GetAvailableProperties(elements.First())) {
            //	bool propertyOk = true;
            //	foreach (var element in elements.Skip(1)) {
            //		bool typeOk = false;
            //		foreach (var pd2 in GetAvailableProperties(element)) {
            //			if (pd1 == pd2) {
            //				typeOk = true;
            //				break;
            //			}

            //			/* Check if it is attached property.*/
            //			if(pd1.Name.Contains(".") && pd2.Name.Contains(".")){
            //			   	if(pd1.Name==pd2.Name){
            //			   		typeOk=true;
            //			   		break;
            //			   	}		
            //			   }
            //		}
            //		if (!typeOk) {
            //			propertyOk = false;
            //			break;
            //		}
            //	}
            //	if (propertyOk) yield return pd1;
            //}
        }
		
	}
}
