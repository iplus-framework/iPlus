using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    public abstract class ACPropertyValueBase<T>
    {
        public ACPropertyValueBase()
        {
        }

        public ACPropertyValueBase(T objectRef)
        {
            _value = objectRef;
        }

        internal virtual bool ChangeValue(ACPropertyNet<T> wrapper, T newValue)
        {
            if (wrapper == null)
                return false;
            _value = newValue;
            return true;
        }

        [DataMember]
        protected T _value;

        [IgnoreDataMember]
        public virtual T Value
        {
            get
            {
                return _value;
            }

            set
            {
                _value = value;
            }
        }


        private Nullable<bool> _IsComparableType = null;
        protected bool IsComparableType()
        {
            if (_IsComparableType.HasValue)
                return _IsComparableType.Value;
            Type typeOfProperty = this.GetType();
            Type interfaceType = null;
            if (typeOfProperty.IsGenericType)
            {
                Type genericType = typeOfProperty.GetGenericArguments()[0];
                interfaceType = genericType.GetInterface("IComparable");
            }
            else
                interfaceType = typeOfProperty.GetInterface("IComparable");
            if (interfaceType != null)
                _IsComparableType = true;
            else
                _IsComparableType = false;
            return _IsComparableType.Value;
            //return genericType.IsSubclassOf(typeof(IComparable));
        }

        private Nullable<bool> _IsConvertibleType = null;
        protected bool IsConvertibleType()
        {
            if (_IsConvertibleType.HasValue)
                return _IsConvertibleType.Value;
            Type typeOfProperty = this.GetType();
            Type interfaceType = null;
            if (typeOfProperty.IsGenericType)
            {
                Type genericType = typeOfProperty.GetGenericArguments()[0];
                interfaceType = genericType.GetInterface("IConvertible");
            }
            else
                interfaceType = typeOfProperty.GetInterface("IConvertible");
            if (interfaceType != null)
                _IsConvertibleType = true;
            else
                _IsConvertibleType = false;
            return _IsConvertibleType.Value;
        }

        public int CompareTo(ACPropertyValueBase<T> propertyValue, IACPropertyBase wrapper)
        {
            if (propertyValue == null)
                return 0;
            return CompareTo(propertyValue.Value, wrapper);
        }

        private static readonly Type _typeDouble = typeof(Double);

        public int CompareTo(T newValue, IACPropertyBase wrapper)
        {
            if (this.Value == null)
                return newValue != null ? 1 : 0;
            if (newValue == null)
                return 1;
            // Falls Wertvergleich möglich
            if (IsComparableType())
            {
                int result = (this.Value as IComparable).CompareTo(newValue as IComparable);
                if (result == 0)
                    return result;
                if (!(newValue is Double || newValue is Single) || !IsConvertibleType())
                    return result;

                int digits = wrapper.PropertyInfo.Precision.HasValue ? wrapper.PropertyInfo.Precision.Value : DefaultPrecision;
                try
                {
                    Double valueT = (Double)Convert.ChangeType(newValue, _typeDouble);
                    valueT = Math.Round(valueT, digits);
                    Double valueTLast = (Double)Convert.ChangeType(this.Value, _typeDouble);
                    Double diff = Math.Abs(valueT - valueTLast);
                    if (diff > Double.Epsilon)
                        return result;
                    else
                        return 0;
                }
                catch (OverflowException oe)
                {
                    string msg = oe.Message;
                    if (oe.InnerException != null && oe.InnerException.Message != null)
                        msg += " Inner:" + oe.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPropertyValueBase<T>", "CompareTo", msg);
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPropertyValueBase<T>", "CompareTo(10)", msg);
                }
                return result;
            }
            // Sonst Vergleich auf Identität
            else
            {
                if (this.Value.Equals(newValue))
                    return 0;
                else
                    return 1;
            }
        }

        private static short? _DefaultPrecision = null;
        public static short DefaultPrecision
        {
            get
            {
                if (_DefaultPrecision.HasValue)
                    return _DefaultPrecision.Value;
                _DefaultPrecision = 6;
                try
                {
                    CoreConfiguration coreConfig = (CoreConfiguration)CommandLineHelper.ConfigCurrentDir.GetSection("Process/CoreConfiguration");
                    if (coreConfig != null)
                    {
                        if (coreConfig.DefaultPrecision >= 0)
                            _DefaultPrecision = coreConfig.DefaultPrecision;
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPropertyValueBase<T>", "DefaultPrecision", msg);
                }
                return _DefaultPrecision.Value;
            }
        }
    }
}
