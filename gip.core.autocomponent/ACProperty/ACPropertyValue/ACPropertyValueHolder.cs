// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public class ACPropertyValueHolder<T> : ACPropertyValueBase<T>
    {
        public ACPropertyValueHolder()
            : base()
        {
        }


        private IACContainerTNet<T> _Owner = null;
        internal IACContainerTNet<T> Owner
        {
            get
            {
                return _Owner;
            }

            set
            {
                if (_Owner == null)
                    _Owner = value;
            }
        }

        public override T Value
        {
            get
            {
                if (Owner != null)
                {
                    using (ACMonitor.Lock((Owner as ACPropertyNet<T>)._20015_LockValue))
                    {
                        return _value;
                    }
                }
                else
                    return _value;
            }

            set
            {
                if (Owner != null)
                {

                    using (ACMonitor.Lock((Owner as ACPropertyNet<T>)._20015_LockValue))
                    {
                        _value = value;
                    }
                }
                else
                    _value = value;
            }
        }


        internal override bool ChangeValue(ACPropertyNet<T> wrapper, T newValue)
        {
            if ((wrapper == null) || (Owner == null))
                return false;
            // PropertyValueHolder kann nur vom besitzenden Wrapper durchgeführt werden
            if (Owner != wrapper)
                return false;
            if (wrapper == null)
                return false;
            // Locking durch Wrapper, der den Wert kapselt

            using (ACMonitor.Lock(wrapper._20015_LockValue))
            {
                if (_value != null)
                {
                    if (_value is IBindingList)
                        (_value as IBindingList).ListChanged -= OnBindingListChanged;
                    else if (_value is INotifyCollectionChanged)
                        (_value as INotifyCollectionChanged).CollectionChanged -= OnCollectionChanged;
                    else if (_value is INotifyPropertyChanged)
                        (_value as INotifyPropertyChanged).PropertyChanged -= OnComplexTypeChanged;
                }
                if (_value != null)
                {
                    if (_value is ACCustomTypeBase)
                        (_value as ACCustomTypeBase).CloneCustomProperties(newValue as ACCustomTypeBase);
                }

                if (wrapper.PropertyInfo.Precision.HasValue && IsConvertibleType())
                {
                    try
                    {
                        Type typeDouble = typeof(Double);
                        Double valueT = (Double)Convert.ChangeType(newValue, typeDouble);
                        valueT = Math.Round(valueT, (int)wrapper.PropertyInfo.Precision);
                        _value = (T)Convert.ChangeType(valueT, typeof(T));
                    }
                    catch (OverflowException oe)
                    {
                        _value = newValue;

                        string msg = oe.Message;
                        if (oe.InnerException != null && oe.InnerException.Message != null)
                            msg += " Inner:" + oe.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ACPropertyValueHolder<T>", "ChangeValue", msg);
                    }
                    catch (Exception e)
                    {
                        _value = newValue;

                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ACPropertyValueHolder<T>", "ChangeValue(10)", msg);
                    }
                }
                else
                    _value = newValue;
                //}

                if (_value != null)
                {
                    if (_value is IBindingList)
                        (_value as IBindingList).ListChanged += OnBindingListChanged;
                    else if (_value is INotifyCollectionChanged)
                        (_value as INotifyCollectionChanged).CollectionChanged += OnCollectionChanged;
                    else if (_value is INotifyPropertyChanged)
                        (_value as INotifyPropertyChanged).PropertyChanged += OnComplexTypeChanged;
                }
            }
            return true;
        }

        private void OnComplexTypeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Owner != null)
            {
                if ((_value != null) && (_value is ACCustomTypeBase))
                {
                    if (e.PropertyName == Const.ValueT)
                        Owner.OnMemberChanged(e);
                }
                else
                    Owner.OnMemberChanged(e);
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Owner != null)
            {
                Owner.OnMemberChanged(e);
            }
        }

        private void OnBindingListChanged(object sender, ListChangedEventArgs e)
        {
            if (Owner != null)
            {
                Owner.OnMemberChanged(e);
            }
        }
    }
}
