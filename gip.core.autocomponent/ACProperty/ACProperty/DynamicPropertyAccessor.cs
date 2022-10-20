using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Reflection.Emit;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public class DynamicPropertyAccessor
    {
        delegate object Getter(object target);
        delegate void Setter(object target, object value);

        Getter _getter;
        Setter _setter;

        public DynamicPropertyAccessor(PropertyInfo propertyInfo)
        {
            if (propertyInfo.CanRead)
                _getter = EmitGetter(propertyInfo);
            if (propertyInfo.CanWrite)
                _setter = EmitSetter(propertyInfo);
        }

        Getter EmitGetter(PropertyInfo propertyInfo)
        {
            DynamicMethod dynMethod = new DynamicMethod("Get",
                typeof(object),
                new Type[] { typeof(object) },
                propertyInfo.DeclaringType);

            ILGenerator il = dynMethod.GetILGenerator();
            MethodInfo getterMethod = propertyInfo.GetGetMethod();

            il.DeclareLocal(typeof(object));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            il.EmitCall(OpCodes.Callvirt, getterMethod, null);

            OpCode box = propertyInfo.PropertyType.IsValueType ?
                OpCodes.Box :
                OpCodes.Castclass;

            il.Emit(box, propertyInfo.PropertyType);

            il.Emit(OpCodes.Ret);

            return (Getter)dynMethod.CreateDelegate(typeof(Getter));
        }

        Setter EmitSetter(PropertyInfo propertyInfo)
        {
            DynamicMethod dynMethod = new DynamicMethod("Set",
                null,
                new Type[]{typeof(object),
                           typeof(object)},
                propertyInfo.DeclaringType);

            ILGenerator il = dynMethod.GetILGenerator();
            MethodInfo setter = propertyInfo.GetSetMethod();
            if (setter == null)
                return null;

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
            il.Emit(OpCodes.Ldarg_1);

            OpCode box = propertyInfo.PropertyType.IsValueType ?
                OpCodes.Unbox_Any :
                OpCodes.Castclass;

            il.Emit(box, propertyInfo.PropertyType);
            il.EmitCall(OpCodes.Callvirt, setter, null);

            il.Emit(OpCodes.Ret);

            return (Setter)dynMethod.CreateDelegate(typeof(Setter));
        }

        public void Set(object target, object value)
        {
            if ((_setter == null) || (target == null))
                return;
            //throw new NotSupportedException("Property is read-only");
            _setter(target, value);
        }

        public object Get(object target)
        {
            if ((_getter == null) || (target == null))
                return null;
            //throw new NotSupportedException("Property is write-only");
            return _getter(target);
        }
    }
}
