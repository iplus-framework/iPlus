// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.Isam.Esent.Interop.EnumeratedColumn;

namespace gip.core.autocomponent
{
    public interface IACPropertyConfigValue : IACContainer
    {
        bool IsCachedValueSet { get; set; }
        void RemoveDuplicateEntries();
    }

    public class ACPropertyConfigValue<T> : IACPropertyConfigValue, IACContainerT<T>
    {
        public ACPropertyConfigValue(ACComponent acComponent, string configACIdentifier, T defaultValue)
        {
            _ACComponent = acComponent;
            _ACIdentifier = configACIdentifier;
            _DefaultValue = defaultValue;
            using (ACMonitor.Lock(_ACComponent.LockMemberList_20020))
            {
                if (!_ACComponent.ACPropertyConfigValueList.Where(c => c.ACIdentifier == configACIdentifier).Any())
                    _ACComponent.ACPropertyConfigValueList.Add(this);
            }
        }

        private ACComponent _ACComponent;
        private T _DefaultValue;

        private string _ACIdentifier;
        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return _ACIdentifier;
            }
        }

        private T _ValueT;
        /// <summary>Gets or sets the encapsulated value of the generic type T</summary>
        /// <value>The Value-Property as generic type</value>
        public T ValueT
        {
            get
            {
                if (_IsCachedValueSet)
                    return _ValueT;
                IACConfig acConfig = _ACComponent.GetConfigurationValue(_ACIdentifier);
                //object result = _ACComponent[_ACIdentifier];
                //bool isValueNotSet = result == null; // || (typeof(T) == typeof(string) && string.IsNullOrEmpty(result.ToString()));
                if (acConfig == null)
                {
                    SetDefaultValue();
                    return _ValueT;
                }
                else
                {
                    object result = null;
                    try
                    {
                        result = acConfig.Value;
                        _ValueT = (T)result;
                        _IsCachedValueSet = true;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("ACPropertyConfigValue<T>", "ValueT", msg);
                        try
                        {
                            if (result != null && result is IConvertible)
                            {
                                _ValueT = (T)Convert.ChangeType(result, typeof(T));
                                _ACComponent[_ACIdentifier] = _ValueT;
                                _IsCachedValueSet = true;
                            }
                            else
                                SetDefaultValue();
                        }
                        catch (Exception ex)
                        {
                            SetDefaultValue();

                            msg = ex.Message;
                            if (ex.InnerException != null && ex.InnerException.Message != null)
                                msg += " Inner:" + ex.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("ACPropertyConfigValue<T>", "ValueT(10)", msg);
                        }
                    }
                }
                return _ValueT;
            }
            set
            {
                _ValueT = value;
                _IsCachedValueSet = true;
                _ACComponent[_ACIdentifier] = _ValueT;
            }
        }

        private void SetDefaultValue()
        {
            _ValueT = _DefaultValue;
            Type innerType = null;
            if (_ValueT == null)
            {
                Type typeT = typeof(T);
                if ((typeT.IsGenericType && typeT.Name == Const.TNameNullable))
                {
                    innerType = typeT;
                    //innerType = typeT.GetGenericArguments()[0];
                }
            }
            _ACComponent.SetConfigurationValue(_ACIdentifier, _ValueT, innerType);
            _IsCachedValueSet = true;
        }

        public void RemoveDuplicateEntries()
        {
            ACClass acTypeFromLiveContext = _ACComponent.ACTypeFromLiveContext;
            if (acTypeFromLiveContext == null)
                return;
            try
            {
                IACConfig[] entries = acTypeFromLiveContext.ConfigurationEntries.Where(c => c.KeyACUrl == acTypeFromLiveContext.ACConfigKeyACUrl && c.LocalConfigACUrl == _ACIdentifier).ToArray();
                if (entries != null && entries.Length > 1)
                {
                    ACClassConfig prevEntry = null;
                    foreach (ACClassConfig entry in entries)
                    {
                        if (prevEntry != null)
                        {
                            if (   entry.UpdateDate.Subtract(prevEntry.UpdateDate).TotalSeconds > 10
                                || prevEntry.Value == null
                                || (    (prevEntry.Value is IComparable) 
                                    &&  (   (_DefaultValue != null && ((IComparable)_DefaultValue).CompareTo(prevEntry.Value as IComparable) == 0)
                                         || (default(T) != null && ((IComparable)default(T)).CompareTo(prevEntry.Value as IComparable) == 0))
                                   )
                               )
                            {
                                acTypeFromLiveContext.RemoveACConfig(prevEntry);
                                prevEntry = entry;
                            }
                            else
                            {
                                acTypeFromLiveContext.RemoveACConfig(entry);
                            }
                        }
                        else
                            prevEntry = entry;
                    }
                }
            }
            catch (Exception e)
            {
                _ACComponent.Messages.LogException(_ACComponent.GetACUrl(), "RemoveDuplicateEntries()", e.Message);
                if (e.InnerException != null)
                    _ACComponent.Messages.LogException(_ACComponent.GetACUrl(), "RemoveDuplicateEntries()", e.InnerException.Message);
            }
        }

        private bool _IsCachedValueSet = false;
        /// <summary>
        /// Set value to false if Refresh from Database is needed
        /// </summary>
        public bool IsCachedValueSet
        {
            get
            {
                return _IsCachedValueSet;
            }
            set
            {
                _IsCachedValueSet = value;
            }
        }


        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        public object Value
        {
            get
            {
                return ValueT;
            }
            set
            {
                ValueT = (T)value;
            }
        }

        /// <summary>Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!</summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        public ACClass ValueTypeACClass
        {
            get
            {
                IACObject iObject = ValueT as IACObject;
                return iObject == null ? null : iObject.ACType as ACClass;
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return ACIdentifier; }
        }

    }
}
