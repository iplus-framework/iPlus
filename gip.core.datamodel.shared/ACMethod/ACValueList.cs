// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACValueList.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    /// <summary>
    /// ACValueList's are used for passing values to method- or constructor-parameters or returning result-values.
    /// ACValueList's are serializable and can be send over network.
    /// The list contains entries of ACValues. 
    /// ACValue is a class that has essentially three properties:<para />
    /// string ACIdentifier: Unique ID/Name of the parameter<para />
    /// object Value: parameter value<para />
    /// Type ObjectFullType: Datatype of parameter value
    /// </summary>
#if NETFRAMEWORK
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACValueList'}de{'ACValueList'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    [NotMapped]
    public class ACValueList : SafeBindingList<ACValue>, IACEntityProperty, ICloneable
#else
    public class ACValueList : SafeBindingList<ACValue>, ICloneable
#endif
    {
        #region c'tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACValueList"/> class.
        /// </summary>
        public ACValueList() : base()
        {
        }

        public ACValueList(ACMethod parentACMethod) : base()
        {
            _ParentACMethod = parentACMethod;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACValueList"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public ACValueList(ACValueList collection)
            : base(collection)
        {
        }

        public ACValueList(ACValueList collection, ACMethod parentACMethod)
            : base(collection)
        {
            _ParentACMethod = parentACMethod;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACValueList"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        public ACValueList(IList<ACValue> collection)
            : base(collection)
        {
        }

        public ACValueList(IList<ACValue> collection, ACMethod parentACMethod)
            : base(collection)
        {
            _ParentACMethod = parentACMethod;
        }
        #endregion

        #region Properties
        private ACMethod _ParentACMethod;
        public ACMethod ParentACMethod
        {
            get
            {
                return _ParentACMethod;
            }

            internal set
            {
                _ParentACMethod = value;
            }
        }

        private bool? _UseCultureInfoForConversion;
        public bool? UseCultureInfoForConversion
        {
            get
            {
                if (_UseCultureInfoForConversion.HasValue)
                    return _UseCultureInfoForConversion;
                if (ParentACMethod != null)
                    return ParentACMethod.UseCultureInfoForConversion;
                return null;
            }
            set
            {
                _UseCultureInfoForConversion = value;
            }
        }

        /// <summary>
        /// Ruft das Element am angegebenen Index ab oder legt dieses fest.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>System.Object.</returns>
        public object this[string propertyName]
        {
            get
            {
                ACValue param = GetACValue(propertyName);
                if (param == null)
                    return null;
                return param.Value;
            }
            set
            {
                ACValue param = GetACValue(propertyName);
                if (param == null)
                    return;
                param.Value = value;
                //OnPropertyChanged(property);
            }
        }

        /// <summary>
        /// Gets the option.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Global.ParamOption.</returns>
        public Global.ParamOption GetOption(string propertyName)
        {
            ACValue param = GetACValue(propertyName);
            if (param == null)
                return Global.ParamOption.NotRequired;
            return param.Option;
        }

        /// <summary>
        /// Gets the AC value.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>ACValue.</returns>
        public ACValue GetACValue(string propertyName)
        {
            return this.Where(c => c.ACIdentifier == propertyName).FirstOrDefault();
        }
        #endregion

        #region Validation
        /// <summary>
        /// Determines whether the specified valid message is valid.
        /// </summary>
        /// <param name="ValidMessage">The valid message.</param>
        /// <returns><c>true</c> if the specified valid message is valid; otherwise, <c>false</c>.</returns>
        public virtual bool IsValid(MsgWithDetails ValidMessage)
        {
            ValidMessage.ClearMsgDetails();
            foreach (var acParameter in this.Where(c => c is ACValue).Select(c => c as ACValue))
            {
                if (acParameter.Option == Global.ParamOption.Required && acParameter.Value == null)
                {
#if NETFRAMEWORK
    ValidMessage.AddDetailMessage(new Msg { Source = "", MessageLevel = eMsgLevel.Info, ACIdentifier = "30002"/* TODO:RequiredParamsNotSet*/, Message = Database.Root.Environment.TranslateMessage(Database.Root, "Info00001", acParameter.ACIdentifier )});
#else
                    ValidMessage.AddDetailMessage(new Msg { Source = "", MessageLevel = eMsgLevel.Info, ACIdentifier = "30002"/* TODO:RequiredParamsNotSet*/, Message = "Required parameter was not set" });
#endif
                }
            }
            return ValidMessage.MsgDetailsCount == 0;
        }
#endregion

#region Attaching
#if NETFRAMEWORK
        /// <summary>
        /// Attaches to.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        public void AttachTo(IACObject acObject)
        {
            if (acObject == null)
                return;
            foreach (ACValue acValue in this)
            {
                acValue.AttachTo(acObject);
            }
        }

        public void Detach(bool detachFromDBContext = false)
        {
            foreach (ACValue acValue in this)
            {
                acValue.Detach(detachFromDBContext);
            }
        }
#endif
#endregion

#region public TypeConversion Methods
        /// <summary>
        /// Gets the boolean.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Boolean.</returns>
        public Boolean GetBoolean(string propertyName)
        {
            return GetACValue(propertyName).ParamAsBoolean;
        }

        /// <summary>
        /// Gets the S byte.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>SByte.</returns>
        public SByte GetSByte(string propertyName)
        {
            return GetACValue(propertyName).ParamAsSByte;
        }

        /// <summary>
        /// Gets the int16.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Int16.</returns>
        public Int16 GetInt16(string propertyName)
        {
            return GetACValue(propertyName).ParamAsInt16;
        }

        /// <summary>
        /// Gets the int32.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Int32.</returns>
        public Int32 GetInt32(string propertyName)
        {
            return GetACValue(propertyName).ParamAsInt32;
        }

        /// <summary>
        /// Gets the int64.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Int64.</returns>
        public Int64 GetInt64(string propertyName)
        {
            return GetACValue(propertyName).ParamAsInt64;
        }

        /// <summary>
        /// Gets the U int16.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>UInt16.</returns>
        public UInt16 GetUInt16(string propertyName)
        {
            return GetACValue(propertyName).ParamAsUInt16;
        }

        /// <summary>
        /// Gets the U int32.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>UInt32.</returns>
        public UInt32 GetUInt32(string propertyName)
        {
            return GetACValue(propertyName).ParamAsUInt32;
        }

        /// <summary>
        /// Gets the U int64.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>UInt64.</returns>
        public UInt64 GetUInt64(string propertyName)
        {
            return GetACValue(propertyName).ParamAsUInt64;
        }

        /// <summary>
        /// Gets the single.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Single.</returns>
        public Single GetSingle(string propertyName)
        {
            return GetACValue(propertyName).ParamAsSingle;
        }

        /// <summary>
        /// Gets the double.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Double.</returns>
        public Double GetDouble(string propertyName)
        {
            return GetACValue(propertyName).ParamAsDouble;
        }

        /// <summary>
        /// Gets the decimal.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Decimal.</returns>
        public Decimal GetDecimal(string propertyName)
        {
            return GetACValue(propertyName).ParamAsDecimal;
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>String.</returns>
        public String GetString(string propertyName)
        {
            return GetACValue(propertyName).ParamAsString;
        }

        /// <summary>
        /// Gets the GUID.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Guid.</returns>
        public Guid GetGuid(string propertyName)
        {
            return GetACValue(propertyName).ParamAsGuid;
        }

        /// <summary>
        /// Gets the time span.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>TimeSpan.</returns>
        public TimeSpan GetTimeSpan(string propertyName)
        {
            return GetACValue(propertyName).ParamAsTimeSpan;
        }


        /// <summary>
        /// Gets the date time.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>DateTime.</returns>
        public DateTime GetDateTime(string propertyName)
        {
            return GetACValue(propertyName).ParamAsDateTime;
        }
#endregion

#region Serializer

        private ReaderWriterLockSlim _ACVLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        protected override ReaderWriterLockSlim RWLock
        {
            get
            {
                return _ACVLock;
            }
        }


#if NETFRAMEWORK
        /// <summary>
        /// Serializes the AC value list.
        /// </summary>
        /// <param name="acValueList">The ac value list.</param>
        /// <returns>System.String.</returns>
        public static string SerializeACValueList(ACValueList acValueList)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(ACValueList), new DataContractSerializerSettings() { KnownTypes = ACKnownTypes.GetKnownType(), MaxItemsInObjectGraph = 99999999, IgnoreExtensionDataObject = true, PreserveObjectReferences = true, DataContractResolver = ACConvert.MyDataContractResolver });
            StringBuilder sb1 = new StringBuilder();
            using (StringWriter sw1 = new StringWriter(sb1))
            using (XmlTextWriter xmlWriter1 = new XmlTextWriter(sw1))
            {
                serializer.WriteObject(xmlWriter1, acValueList);
                return sw1.ToString();
            }
        }

        /// <summary>
        /// Deserializes the AC value list.
        /// </summary>
        /// <param name="acValueListXML">The ac value list XML.</param>
        /// <returns>ACValueList.</returns>
        public static ACValueList DeserializeACValueList(string acValueListXML)
        {
            using (StringReader ms = new StringReader(acValueListXML))
            using (XmlTextReader xmlReader = new XmlTextReader(ms))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ACValueList), new DataContractSerializerSettings() { KnownTypes = ACKnownTypes.GetKnownType(), MaxItemsInObjectGraph = 99999999, IgnoreExtensionDataObject = true, PreserveObjectReferences = true, DataContractResolver = ACConvert.MyDataContractResolver });
                ACValueList acValueList = (ACValueList)serializer.ReadObject(xmlReader);

                return acValueList;
            }
        }
#endif
#endregion

#region Abgleich
        /// <summary>
        /// Updates the values.
        /// </summary>
        /// <param name="acValueList">The ac value list.</param>
        public void UpdateValues(ACValueList acValueList)
        {
            foreach (var acValue in this)
            {
                acValue.Value = acValueList[acValue.ACIdentifier];
            }
        }
#endregion

        /// <summary>
        /// To the value array.
        /// </summary>
        /// <returns>System.Object[][].</returns>
        public object[] ToValueArray()
        {
            if (!this.Any())
                return null;
            else
                return (from ACValue X in this select X.Value).ToArray(); 
        }


        public string XMLConfig
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        public void OnEntityPropertyChanged(string property)
        {
            //OnPropertyChanged(property);
        }

#if NETFRAMEWORK
        public ACPropertyManager ACProperties
        {
            get { return null; }
        }
#endif

        public virtual void CloneValues(ACValueList from)
        {
            if (from == null)
                return;
            _ParentACMethod = from._ParentACMethod;
            foreach (ACValue acValue in from)
            {
                ACValue clone = acValue.Clone() as ACValue;
                if (clone != null)
                    this.Add(clone);
            }
        }

        public virtual void CopyValues(ACValueList from, bool forceConversion = false)
        {
            foreach (ACValue toValue in this)
            {
                ACValue fromValue = from.GetACValue(toValue.ACIdentifier);
                if (fromValue != null)
                {
                    toValue.CopyValue(fromValue, forceConversion);
                }
            }
        }

        public virtual object Clone()
        {
            ACValueList clone = new ACValueList();
            clone.CloneValues(this);
            return clone;
        }
    }

#if NETFRAMEWORK
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACValueWithCaptionList'}de{'ACValueWithCaptionList'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACValueWithCaptionList : BindingList<ACValueWithCaption>
    {
        public ACValueWithCaptionList() : base()
        {

        }

        public ACValueWithCaptionList(IList<ACValueWithCaption> collection) : base (collection)
        {
        }

    }
#endif
}
