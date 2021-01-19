// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACValueItem.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Transactions;
using System.ComponentModel;
using System.Globalization;

namespace gip.core.datamodel
{
    /// <summary>
    /// Container für einfacher Werte. Serialisierbar
    /// Verwendung:
    /// -Enum-Listen zur Verwendung in VBCombobox, VBRadioButtonGroup, etc.
    /// -Hilfsliste bei der Text/Meldungs-Übersetzung (BSOiPlusStudio_5_Translation)
    /// -Sucheintrag in VBBSOFindAndReplace
    /// -Hilfsliste in VBBSOInputbox
    /// -Hilfsliste in BSOiPlusInstall für die Auswahl der Projekte (Verzeichnisse) beim Import
    /// </summary>
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACValueItem'}de{'ACValueItem'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    // 1 ACCaption
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACValueItem", "en{'ACValueItem'}de{'ACValueItem'}", typeof(ACValueItem), "ACValueItem", Const.ACCaptionPrefix, Const.ACCaptionPrefix)]
    public class ACValueItem : IACContainer, IACObject, INotifyPropertyChanged
    {
        #region c´tors
        /// <summary>
        /// Verwendung: ContentList bei Steuerelementen
        /// </summary>
        /// <param name="acCaption">The ac caption.</param>
        /// <param name="value">The value.</param>
        /// <param name="acType">Type of the ac.</param>
        public ACValueItem(string acCaption, object value, IACType acType )
        {
            _ACCaptionTranslation = acCaption;
            _Value = value;
            if (acType is ACClass)
            {
                _ValueTypeACClass = acType as ACClass;
                _ValueACType = acType;
            }
            else
            {
                if (acType != null)
                {
                    _ValueTypeACClass = acType.ValueTypeACClass;
                    _ValueACType = acType;
                }
            }
        }

        public ACValueItem(string acCaption, object value, IACType acType, IACObject parentACObject, short sortIndex)
        {
            _ACCaptionTranslation = acCaption;
            _Value = value;
            if (acType is ACClass)
            {
                _ValueTypeACClass = acType as ACClass;
                _ValueACType = acType;
            }
            else
            {
                if (acType != null)
                {
                    _ValueTypeACClass = acType.ValueTypeACClass;
                    _ValueACType = acType;
                }
            }
            if (parentACObject != null)
                ParentACObject = parentACObject;

            SortIndex = sortIndex;
        }

        #endregion

        #region IACContainer

        object _Value;
        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        [DataMember]
        [ACPropertyInfo(9999)]
        public object Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
                OnPropertyChanged(Const.Value);
            }
        }

        ACClass _ValueTypeACClass;
        /// <summary>Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!</summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        public ACClass ValueTypeACClass 
        {
            get
            {
                return _ValueTypeACClass;
            }
            set
            {
                _ValueTypeACClass = value;
            }
        }

        /// <summary>
        /// The _ value AC type
        /// </summary>
        IACType _ValueACType;
        /// <summary>
        /// Gets the type of the value AC.
        /// </summary>
        /// <value>The type of the value AC.</value>
        public IACType ValueACType
        {
            get
            {
                return _ValueACType;
            }
        }
        #endregion

        #region IACObjectWithBinding Member
        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("","", 9999)]
        public string GetACUrl(IACObject rootACObject = null)
        {
            //throw new NotImplementedException();
            return ACIdentifier;
        }

        /// <summary>
        /// The translation-Tupel for ACCaption
        /// </summary>
        [DataMember]
        string _ACCaptionTranslation;

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(1)]
        public string ACCaption
        {
            get 
            {
                return Translator.GetTranslation(ACCaptionTranslation,ACCaptionTranslation);
            }
        }

        /// <summary>
        /// Beschriftung
        /// </summary>
        /// <value>The AC caption.</value>
        [ACPropertyInfo(9999)]
        public string ACCaptionTranslation
        {
            get { return _ACCaptionTranslation; }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return ACCaption; }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        public short SortIndex { get;set; }

        #endregion

        #region static Properties
        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        static public string KeyACIdentifier
        {
            get
            {
                return "Index";
            }
        }
        #endregion

        #region IACObject Member
        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get;set;
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return this.ReflectGetACContentList();
            }
        }
        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        [DefaultValue(false)]
        public bool OnPropertyChangedNameWithACIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
            if (ParentACObject is INotifyPropertyChanged)
            {
                if(OnPropertyChangedNameWithACIdentifier)
                    ParentACObject.ACUrlCommand("!OnPropertyChanged", "ACValueItem("+ACIdentifier+")\\" + name);
                else
                    ParentACObject.ACUrlCommand("!OnPropertyChanged", "ACValueItem\\" + name);
            }
        }
        #endregion

        /// <summary>
        /// Sets the value from string.
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        public void SetValueFromString(string stringValue)
        {
            Value = ACConvert.ChangeType(stringValue, ValueACType.ObjectType, true, gip.core.datamodel.Database.GlobalDatabase);
            //if (string.IsNullOrEmpty(stringValue))
            //{
            //    SetDefaultValue();
            //}
            //else
            //{
            //    if (ValueACType.ObjectType.IsEnum)
            //        Value = Enum.Parse(ValueACType.ObjectType, stringValue);
            //    else
            //    {
            //        if (stringValue == "null")
            //        {
            //            Value = null;
            //        }
            //        else if(ValueACType.ObjectType == typeof(TimeSpan))
            //        {
            //            //int seconds = 0;
            //            //if (int.TryParse(stringValue, out seconds))
            //            //    Value = TimeSpan.FromSeconds(seconds);
            //            Value = TimeSpan.ParseExact(stringValue, "c", CultureInfo.InvariantCulture);
            //        }
            //        else
            //        {
            //            Value = Convert.ChangeType(stringValue, ValueACType.ObjectType);
            //        }
            //    }
            //}
        }

        public string GetStringValue()
        {
            return ACConvert.ChangeType(Value, typeof(string), true, gip.core.datamodel.Database.GlobalDatabase) as string;
            //string value = "";

            //if (Value == null)
            //    return value;
            ////if (ValueACType.ObjectType == typeof(TimeSpan))
            ////    value = TimeSpan.Par
            ////else
            //    value = Value.ToString();
            //return value;
        }

        //public object GetDefaultValue()
        //{
        //    if (ValueACType.ObjectType == null)
        //        return null;

        //    if (this.ValueACType.ObjectType == typeof(Byte))
        //        return (Byte)0;
        //    else if (this.ValueACType.ObjectType == typeof(Int16))
        //        return (Int16)0;
        //    else if (this.ValueACType.ObjectType == typeof(Int32))
        //        return (Int32)0;
        //    else if (this.ValueACType.ObjectType == typeof(Int64))
        //        return (Int64)0;
        //    else if (this.ValueACType.ObjectType == typeof(SByte))
        //        return (SByte)0;
        //    else if (this.ValueACType.ObjectType == typeof(UInt16))
        //        return (UInt16)0;
        //    else if (this.ValueACType.ObjectType == typeof(UInt32))
        //        return (UInt32)0;
        //    else if (this.ValueACType.ObjectType == typeof(UInt64))
        //        return (UInt64)0;
        //    else if (this.ValueACType.ObjectType == typeof(Single))
        //        return (Single)0.0;
        //    else if (this.ValueACType.ObjectType == typeof(Double))
        //        return (Double)0.0;
        //    else if (this.ValueACType.ObjectType == typeof(DateTime))
        //        return DateTime.Now;
        //    else if (this.ValueACType.ObjectType == typeof(TimeSpan))
        //        return new TimeSpan();
        //    return null;
        //}

        ///// <summary>
        ///// Sets the default value.
        ///// </summary>
        //public void SetDefaultValue()
        //{
        //    if (this.ValueACType.ObjectType == null)
        //        return;
        //    Value = GetDefaultValue();
        //}

    }

}
