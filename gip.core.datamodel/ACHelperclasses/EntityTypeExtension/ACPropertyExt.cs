// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-07-2012
// ***********************************************************************
// <copyright file="ACPropertyExt.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.IO;
using System.ComponentModel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACPropertyExt
    /// </summary>
    [DataContract]
    public class ACPropertyExt //: IACRef
    {
        #region ctor's

        public ACPropertyExt()
        {
            InitLock();
        }

        public ACPropertyExt(IACType acTypeInfo)
        {
            InitLock();

            ACIdentifier = acTypeInfo.ACIdentifier;
            Type objectType = acTypeInfo.ObjectFullType;
            ObjectType = objectType;

            if (objectType == typeof(System.String))
            {
                String s = "";
                Value = s;
            }
            else if (objectType.IsValueType)
            {
                Value = Activator.CreateInstance(objectType);
            }
            // Objekte müssen vom Benutzer erzeugt werden
            else if (!objectType.IsValueType)
            {
                //acPropertyExt.Value = Activator.CreateInstance(objectType);
                Value = null;
            }
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            InitLock();
        }

        private void InitLock()
        {
            if (_Lock == null)
                _Lock = new object();
        }
        #endregion

        #region IACRef
        private object _Lock;
        private object Lock
        {
            get
            {
                if (_Lock == null)
                    InitLock();
                return _Lock;
            }
        }

        /// <summary>
        /// The _ value
        /// </summary>
        private object _Value = null;
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        [IgnoreDataMember]
        public object Value
        {
            set
            {
                object newValue = null;
                ACPropertyManager propertyManager = ParentACObject as ACPropertyManager;
                if (propertyManager != null)
                {
                    bool notNullStringObjectForInvariantCulture = value != null && value is String && this.ObjectType != typeof(string);
                    if (propertyManager.EntityObject != null)
                        newValue = ACConvert.ChangeType(value, ObjectType, !notNullStringObjectForInvariantCulture, propertyManager.DatabaseEntity);
                    else
                        newValue = ACConvert.ChangeType(value, ObjectType, true, null);
                }
                else
                    newValue = ACConvert.ChangeType(value, ObjectType, true, null);
                lock (Lock)
                {
                    _Value = newValue;
                }
                SerializeValueToXML();
            }
            get
            {
                lock (Lock)
                {
                    if (_Value != null)
                        return _Value;
                }

                string xmlValue = XMLValue;
                if (   xmlValue == null 
                    && (   objectType == null
                        || objectType.Name == Const.TNameNullable))
                    return null;

                object newValue = null;
                ACPropertyManager propertyManager = ParentACObject as ACPropertyManager;
                if (propertyManager != null)
                    newValue = ACConvert.XMLToObject(objectType, xmlValue, true, propertyManager.DatabaseEntity);
                else
                    newValue = ACConvert.XMLToObject(objectType, xmlValue, true, null);
                lock (Lock)
                {
                    _Value = newValue;
                    return _Value;
                }
            }
        }

        internal void RepairInvariantType()
        {
            var objectType = ObjectType;
            string xmlValue = XMLValue;
            if (xmlValue == null && objectType == null)
                return;

            object newValue = null;
            ACPropertyManager propertyManager = ParentACObject as ACPropertyManager;
            if (propertyManager != null)
                newValue = ACConvert.XMLToObject(objectType, xmlValue, false, propertyManager.DatabaseEntity);
            else
                newValue = ACConvert.XMLToObject(objectType, xmlValue, false, null);
            lock (Lock)
            {
                _Value = newValue;
            }

            SerializeValueToXML();
        }

        /// <summary>
        /// Attaches to.
        /// </summary>
        /// <param name="parentACObject">The parent AC object.</param>
        public void AttachTo(IACObject parentACObject)
        {
            lock (Lock)
            {
                _ParentACObject = parentACObject;
            }
        }

        /// <summary>
        /// Detaches this instance.
        /// </summary>
        public void Detach()
        {
        }

        /// <summary>
        /// Serializes the value to XML.
        /// </summary>
        public void SerializeValueToXML()
        {
            object value = null;
            lock (Lock)
            {
                value = _Value;
            }
            string valueXML = ACConvert.ObjectToXML(value, true);

            if (XMLValue != valueXML)
                XMLValue = valueXML;
        }
        #endregion

        #region Properties

        private string _ACIdentifier;
        /// <summary>
        /// Gets or sets the AC identifier.
        /// </summary>
        /// <value>The AC identifier.</value>
        [DataMember]
        public string ACIdentifier
        {
            get
            {
                lock (Lock)
                {
                    return _ACIdentifier;
                }
            }
            set
            {
                lock (Lock)
                {
                    _ACIdentifier = value;
                }
            }
        }

        private string _XMLValue;
        /// <summary>
        /// Gets or sets the XML value.
        /// </summary>
        /// <value>The XML value.</value>
        [DataMember]
        public string XMLValue
        {
            get
            {
                lock (Lock)
                {
                    return _XMLValue;
                }
            }
            set
            {
                lock (Lock)
                {
                    _XMLValue = value;
                }
            }
        }

        private Type _ObjectType;
        /// <summary>
        /// Gets or sets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        [IgnoreDataMember]
        public Type ObjectType
        {
            get
            {
                lock (Lock)
                {
                    return _ObjectType;
                }
            }
            set
            {
                lock (Lock)
                {
                    _ObjectType = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the type string.
        /// </summary>
        /// <value>The type string.</value>
        [DataMember]
        public string TypeString
        {
            get
            {
                var objectType = ObjectType;
                if (objectType == null)
                    return null;
                return objectType.FullName;
            }
            set
            {
                if (value == null)
                    ObjectType = null;
                else
                {
                    // TODO: 
                    ObjectType = TypeAnalyser.GetTypeInAssembly(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [property changed subscribed].
        /// </summary>
        /// <value><c>true</c> if [property changed subscribed]; otherwise, <c>false</c>.</value>
        [IgnoreDataMember]
        internal bool PropertyChangedSubscribed
        {
            get;
            set;
        }
        #endregion

        #region Attach
        /// <summary>
        /// The _ parent AC object
        /// </summary>
        [IgnoreDataMember]
        IACObject _ParentACObject = null;
        /// <summary>
        /// Gets the parent AC object.
        /// </summary>
        /// <value>The parent AC object.</value>
        public virtual IACObject ParentACObject
        {
            get
            {
                lock (Lock)
                {
                    return _ParentACObject;
                }
            }
        }

        #endregion
    }
}
