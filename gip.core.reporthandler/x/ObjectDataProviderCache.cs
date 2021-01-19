using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using combit.ListLabel17;

namespace gip.core.reporthandler
{
	internal static class ObjectDataProviderCache
	{
		private static Dictionary<Type, PropertyDescriptorCollection> _propertyDescriptorCache = new Dictionary<Type, PropertyDescriptorCollection>();
		private static Dictionary<Type, Type> _tableTypeCache = new Dictionary<Type, Type>();
		private static Dictionary<Type, Type> _enumTypeCache = new Dictionary<Type, Type>();
		internal static PropertyDescriptorCollection GetPropertyDescriptorCollection(Type t)
		{
			Dictionary<Type, PropertyDescriptorCollection> propertyDescriptorCache;
			Monitor.Enter(propertyDescriptorCache = ObjectDataProviderCache._propertyDescriptorCache);
			PropertyDescriptorCollection properties;
			try
			{
				if (!ObjectDataProviderCache._propertyDescriptorCache.TryGetValue(t, out properties))
				{
					properties = TypeDescriptor.GetProperties(t, new Attribute[]
					{
						new BrowsableAttribute(true)
					});
					ObjectDataProviderCache._propertyDescriptorCache.Add(t, properties);
				}
			}
			finally
			{
				Monitor.Exit(propertyDescriptorCache);
			}
			return properties;
		}
		internal static Type GetGenericObjectTableType(Type t)
		{
			Dictionary<Type, Type> tableTypeCache;
			Monitor.Enter(tableTypeCache = ObjectDataProviderCache._tableTypeCache);
			Type type;
			try
			{
				if (!ObjectDataProviderCache._tableTypeCache.TryGetValue(t, out type))
				{
					type = typeof(ObjectTable<>).MakeGenericType(new Type[]
					{
						t
					});
					ObjectDataProviderCache._tableTypeCache.Add(t, type);
				}
			}
			finally
			{
				Monitor.Exit(tableTypeCache);
			}
			return type;
		}
		internal static Type GetGenericEnumerableType(Type t)
		{
			Dictionary<Type, Type> enumTypeCache;
			Monitor.Enter(enumTypeCache = ObjectDataProviderCache._enumTypeCache);
			Type type;
			try
			{
				if (!ObjectDataProviderCache._enumTypeCache.TryGetValue(t, out type))
				{
					type = typeof(IEnumerable<>).MakeGenericType(new Type[]
					{
						t
					});
					ObjectDataProviderCache._enumTypeCache.Add(t, type);
				}
			}
			finally
			{
				Monitor.Exit(enumTypeCache);
			}
			return type;
		}
	}
}
