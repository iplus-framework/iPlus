using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using combit.ListLabel17;
using combit.ListLabel17.DataProviders;
using gip.core.datamodel;

namespace gip.core.reporthandler
{
	internal static class ObjectDataProviderCache
	{
        public static readonly ACMonitorObject _90000_LockPropertyDescriptorCache = new ACMonitorObject(90000);
        private static Dictionary<Type, PropertyDescriptorCollection> _propertyDescriptorCache = new Dictionary<Type, PropertyDescriptorCollection>();
        public static readonly ACMonitorObject _90000_LockTableTypeCache = new ACMonitorObject(90000);
        private static Dictionary<Type, Type> _tableTypeCache = new Dictionary<Type, Type>();
        public static readonly ACMonitorObject _90000_LockEnumTypeCache = new ACMonitorObject(90000);
        private static Dictionary<Type, Type> _enumTypeCache = new Dictionary<Type, Type>();
		internal static PropertyDescriptorCollection GetPropertyDescriptorCollection(Type t)
		{
			PropertyDescriptorCollection properties;

            using (ACMonitor.Lock(ObjectDataProviderCache._90000_LockPropertyDescriptorCache))
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
			return properties;
		}
		internal static Type GetGenericObjectTableType(Type t)
		{
			Type type;

            using (ACMonitor.Lock(ObjectDataProviderCache._90000_LockTableTypeCache))
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
			return type;
		}
		internal static Type GetGenericEnumerableType(Type t)
		{
            Type type;

            using (ACMonitor.Lock(ObjectDataProviderCache._90000_LockEnumTypeCache))
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
			return type;
		}
	}
}
