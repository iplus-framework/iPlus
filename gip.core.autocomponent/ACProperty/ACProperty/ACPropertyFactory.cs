using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public class ACPropertyFactoryBase
    {
        public static List<Type> GenerateUnKnownValueEventTypes()
        {
                List<Type> _UnKnownValueEventTypes = new List<Type>();
                foreach (Type t in ACKnownTypes.UnKnownTypes)
                {
                    _UnKnownValueEventTypes.Add(t);
                    Type acPropertyType = typeof(ACPropertyValueEvent<>);
                    _UnKnownValueEventTypes.Add(acPropertyType.MakeGenericType(new Type[] { t }));
                }
                return _UnKnownValueEventTypes;
        }
    }

    public class ACPropertyFactory<T> : ACPropertyFactoryBase
    {
        public static IACContainerTNet<T> New(ACClassProperty acClassProperty, IACComponent forACComponent)
        {
            if ((acClassProperty == null) || (forACComponent == null))
                return null;
            //RegisterUnkownType();
            if (!forACComponent.IsProxy)
            {
                if (acClassProperty.IsBroadcast)
                {
                    if (acClassProperty.IsProxyProperty)
                        return new ACPropertyNetTarget<T>(forACComponent, acClassProperty);
                    else
                        return new ACPropertyNetSource<T>(forACComponent, acClassProperty);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (forACComponent.Root.HasACModelServer && (!acClassProperty.IsProxyProperty))
                    return new ACPropertyNetSource<T>(forACComponent, acClassProperty, true);
                else
                    return new ACPropertyNet<T>(forACComponent, acClassProperty);
            }
        }

    }
}
