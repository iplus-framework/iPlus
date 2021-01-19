// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACPropertyInfo.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.ComponentModel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACPropertyBase
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ACPropertyBase : DefaultValueAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyBase"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acPropUsage">Usage of the Property. (Point, Selected, Current....)</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isBroadcast">If the property is network-capable, it is automatically distributed in the network when change events occur.</param>
        /// <param name="isProxyProperty">If the property is a "proxy" property, it gets its value from a property binding from a source property.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        /// <param name="dataType">Data type which is used for this property</param>
        public ACPropertyBase(Int16 sortIndex, Global.ACPropUsages acPropUsage, string acGroup, string acCaptionTranslation, string acSource, bool isBroadcast, bool isProxyProperty, bool isPersistable, bool isRightmanagement, Type dataType)
            : base(null)
        {
            ACPropUsage = acPropUsage;
            ACCaptionTranslation = acCaptionTranslation;
            ACGroup = acGroup;
            ACSource = acSource;
            IsBroadcast = isBroadcast;
            IsProxyProperty = isProxyProperty;
            IsPersistable = isPersistable;
            IsRightmanagement = isRightmanagement;
            SortIndex = sortIndex;
            ConfigDataType = dataType;
        }

        #region public Member

        /// <summary>
        /// Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.
        /// </summary>
        public Int16 SortIndex { get; set; }


        /// <summary>
        /// Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.
        /// </summary>
        public string ACGroup { get; set; }


        /// <summary>
        /// Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}
        /// </summary>
        public string ACCaptionTranslation { get; set; }



        /// <summary>
        /// Default value that the property should have as soon as the instance is created by the iPlus framework. If the value is empty, the property receives the .NET default value.
        /// </summary>
        public object DefaultValue
        {
            get
            {
                return this.Value;
            }
            set
            {
                SetValue(value);
            }
        }


        /// <summary>
        /// Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.
        /// </summary>
        public bool IsRightmanagement { get; set; }


        /// <summary>
        /// Usage of the Property. (Point, Selected, Current....)
        /// </summary>
        public Global.ACPropUsages ACPropUsage { get; set; }


        /// <summary>
        /// If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.
        /// </summary>
        public string ACSource { get; set; }


        /// <summary>
        /// If the property is network-capable, it is automatically distributed in the network when change events occur.
        /// </summary>
        public bool IsBroadcast { get; set; }


        /// <summary>
        /// With ForceBroadcast there is always a distribution of the property in the network. Even if you have not changed the value of a network-capable property.
        /// For network-enabled properties (destination or source): Whenever ValueT is set, a transmission to the clients takes place, even without changing the value.
        /// For local properties: If set, the proxy retrieves the value only once.There is never an update on the proxy side.
        /// </summary>
        public bool ForceBroadcast { get; set; }


        /// <summary>
        /// If the property is a "proxy" property, it gets its value from a property binding from a source property.
        /// </summary>
        public bool IsProxyProperty { get; set; }


        /// <summary>
        /// If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.
        /// </summary>
        public bool IsPersistable { get; set; }


        /// <summary>
        /// Custom-Flag, which is used for Communication-Drivers to synchronize this property with other devices.
        /// </summary>
        public bool IsRPCEnabled { get; set; }


        /// <summary>
        /// Custom ID, which is used for Communication-Drivers to synchronize this property with other devices.
        /// </summary>
        public int RemotePropID { get; set; }


        Type _ConfigDataType;
        /// <summary>
        /// If the ACPropUsage has the value ConfigPointConfig or ConfigPointProperty, the property value or the list entries are stored in the table ACClassConfig. 
        /// The data type must be specified so that the serializer recognizes which data type must be serialized and the iPlus development environment knows which editor is to be used in the "Configuration" tab.
        /// </summary>
        public Type ConfigDataType
        {
            get
            {
                return _ConfigDataType;
            }
            set
            {
                _ConfigDataType = value;
            }
        }


        private int _MinLength = -1;
        /// <summary>
        /// Minimum number of characters if the data type of the property is a string.
        /// </summary>
        public int MinLength
        {
            get
            {
                return _MinLength;
            }

            set
            {
                _MinLength = value;
            }
        }


        private int _MaxLength = -1;
        /// <summary>
        /// Maximum number of characters if the data type of the property is a string.
        /// </summary>
        public int MaxLength
        {
            get
            {
                return _MaxLength;
            }

            set
            {
                _MaxLength = value;
            }
        }

        private Double _MinValue = Double.NaN;
        /// <summary>
        /// Minimum allowed value.
        /// </summary>
        public Double MinValue
        {
            get
            {
                return _MinValue;
            }

            set
            {
                _MinValue = value;
            }
        }

        private Double _MaxValue = Double.NaN;
        /// <summary>
        /// Maximum allowed value.
        /// </summary>
        public Double MaxValue
        {
            get
            {
                return _MaxValue;
            }

            set
            {
                _MaxValue = value;
            }
        }

        private int _DataTypeLength = -1;
        /// <summary>
        /// Maximum number of characters of a string for persistance.
        /// The MaxLength is a restriction only for the logical point of view.
        /// </summary>
        public int DataTypeLength
        {
            get
            {
                return _DataTypeLength;
            }

            set
            {
                _DataTypeLength = value;
            }
        }

        String _VirtualTableType;
        public String VirtualTableType
        {
            get
            {
                return _VirtualTableType;
            }
            set
            {
                _VirtualTableType = value;
            }
        }

        #endregion
    }


    /// <summary>
    /// Class ACPropertyEntity
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = true)]
    public class ACPropertyEntity : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyEntity"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acIdentifier">Unique identifier of the property.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="dataType">Data type which is used for this property</param>
        /// <param name="comment">Single-line comment for a more detailed description of the property.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>

        public ACPropertyEntity(Int16 sortIndex, string acIdentifier, string acCaptionTranslation, Type dataType, string acSource = "", string comment = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.Property, "", acCaptionTranslation, acSource, false, false, false, isRightmanagement, dataType)
        {
            ACIdentifier = acIdentifier;
            Comment = comment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyEntity"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acIdentifier">Unique identifier of the property.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="comment">Single-line comment for a more detailed description of the property.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyEntity(Int16 sortIndex, string acIdentifier, string acCaptionTranslation, string acSource = "", string comment = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.Property, "", acCaptionTranslation, acSource, false, false, false, isRightmanagement, null)
        {
            ACIdentifier = acIdentifier;
            Comment = comment;
        }

        #region public Member
        /// <summary>
        /// Unique identifier.
        /// </summary>
        /// <value>The AC identifier.</value>
        public string ACIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>The comment.</value>
        public string Comment { get; set; }
        #endregion
    }

    /// <summary>
    /// Class ACPropertyAccessPrimary
    /// </summary>
    public class ACPropertyAccessPrimary : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyAccessPrimary"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyAccessPrimary(Int16 sortIndex, string acGroup, string acCaptionTranslation = "", string acSource = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.AccessPrimary, acGroup, acCaptionTranslation, acSource, false, false, false, isRightmanagement, null)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyAccess
    /// </summary>
    public class ACPropertyAccess : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyAccess"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyAccess(Int16 sortIndex, string acGroup, string acCaptionTranslation = "", string acSource = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.Access, acGroup, acCaptionTranslation, acSource, false, false, false, isRightmanagement, null)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyCurrent
    /// </summary>
    public class ACPropertyCurrent : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyCurrent"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyCurrent(Int16 sortIndex, string acGroup, string acCaptionTranslation = "", string acSource = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.Current, acGroup, acCaptionTranslation, acSource, false, false, false, isRightmanagement, null)
        {
        }
    }

    /// <summary>
    /// Class ACPropertySelected
    /// </summary>
    public class ACPropertySelected : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertySelected"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertySelected(Int16 sortIndex, string acGroup, string acCaptionTranslation = "", string acSource = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.Selected, acGroup, acCaptionTranslation, acSource, false, false, false, isRightmanagement, null)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyList
    /// </summary>
    public class ACPropertyList : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyList"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyList(Int16 sortIndex, string acGroup, string acCaptionTranslation = "", string acSource = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.List, acGroup, acCaptionTranslation, acSource, false, false, false, isRightmanagement, null)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyInfo
    /// </summary>
    public class ACPropertyInfo : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyInfo"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyInfo(Int16 sortIndex, string acGroup = "", string acCaptionTranslation = "", string acSource = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.Property, acGroup, acCaptionTranslation, acSource, false, false, false, isRightmanagement, null)
        {
        }

        /// <summary>
        /// Initializes a new instacne of the <see cref="ACPropertyInfo"/> class.
        /// </summary>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyInfo(bool isPersistable, Int16 sortIndex, string acGroup = "", string acCaptionTranslation = "", string acSource = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.Property, acGroup, acCaptionTranslation, acSource, false, false, isPersistable, isRightmanagement, null)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyChangeInfo
    /// </summary>
    public class ACPropertyChangeInfo : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyChangeInfo"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyChangeInfo(Int16 sortIndex = 9999, string acGroup = "", string acCaptionTranslation = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.ChangeInfo, acGroup, acCaptionTranslation, "", false, false, false, isRightmanagement, null)
        {
        }
    }


    /// <summary>
    /// Class ACPropertyBindingSource
    /// </summary>
    public class ACPropertyBindingSource : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyBindingSource"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        public ACPropertyBindingSource(Int16 sortIndex = 9999, string acGroup = "", string acCaptionTranslation = "", string acSource = "", bool isRightmanagement = false, bool isPersistable = false)
            : base(sortIndex, Global.ACPropUsages.Property, acGroup, acCaptionTranslation, acSource, true, false, isPersistable, isRightmanagement, null)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyBindingTarget
    /// </summary>
    public class ACPropertyBindingTarget : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyBindingTarget"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        public ACPropertyBindingTarget(Int16 sortIndex = 9999, string acGroup = "", string acCaptionTranslation = "", string acSource = "", bool isRightmanagement = false, bool isPersistable = false)
            : base(sortIndex, Global.ACPropUsages.Property, acGroup, acCaptionTranslation, acSource, true, true, isPersistable, isRightmanagement, null)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyConfig
    /// </summary>
    public class ACPropertyConfig : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyConfig"/> class.
        /// </summary>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        public ACPropertyConfig(string acCaptionTranslation)
            : base(9999, Global.ACPropUsages.Configuration, "", acCaptionTranslation, "", false, false, false, false, null)
        {
            //DefaultValue = defaultValue;
        }
    }

    #region Points

    /// <summary>
    /// Class ACPropertyPoint
    /// </summary>
    public class ACPropertyPoint : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyPoint"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acPropUsage">Usage of the Property. (Point, Selected, Current....)</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isBroadcast">If the property is network-capable, it is automatically distributed in the network when change events occur.</param>
        /// <param name="isProxyProperty">If the property is a "proxy" property, it gets its value from a property binding from a source property.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        /// <param name="dataType">Data type which is used for this property</param>
        /// <param name="pointCapacity">The point capacity.</param>
        public ACPropertyPoint(Int16 sortIndex, Global.ACPropUsages acPropUsage, string acGroup, string acCaptionTranslation, string acSource, bool isBroadcast, bool isProxyProperty, bool isPersistable, bool isRightmanagement, Type dataType, uint pointCapacity)
            : base(sortIndex, acPropUsage, acGroup, acCaptionTranslation, "", false, false, isPersistable, isRightmanagement, dataType)
        {
            PointCapacity = pointCapacity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyPoint"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acPropUsage">Usage of the Property. (Point, Selected, Current....)</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="pointCapacity">The point capacity.</param>
        /// <param name="isInteraction">if set to <c>true</c> [is interaction].</param>
        /// <param name="dataType">Data type which is used for this property</param>
        public ACPropertyPoint(Int16 sortIndex, Global.ACPropUsages acPropUsage, string acGroup, Type dataType, string acCaptionTranslation = "", bool isRightmanagement = false, bool isPersistable = false, uint pointCapacity = 0)
            : base(sortIndex, acPropUsage, acGroup, acCaptionTranslation, "", false, false, isPersistable, isRightmanagement, dataType)
        {
            PointCapacity = pointCapacity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyPoint"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acPropUsage">Usage of the Property. (Point, Selected, Current....)</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="pointCapacity">The point capacity.</param>
        public ACPropertyPoint(Int16 sortIndex, Global.ACPropUsages acPropUsage, string acGroup, string acCaptionTranslation = "", bool isRightmanagement = false, bool isPersistable = false, uint pointCapacity = 0)
            : base(sortIndex, acPropUsage, acGroup, acCaptionTranslation, "", false, false, isPersistable, isRightmanagement, null)
        {
            PointCapacity = pointCapacity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyPoint"/> class.
        /// </summary>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="pointCapacity">The point capacity.</param>
        public ACPropertyPoint(bool isPersistable, uint pointCapacity)
            : this(9999, Global.ACPropUsages.ConnectionPoint, "", "", false, isPersistable, pointCapacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyPoint"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="pointCapacity">The point capacity.</param>
        public ACPropertyPoint(Int16 sortIndex, string acGroup, string acCaptionTranslation = "", bool isRightmanagement = false, bool isPersistable = false, uint pointCapacity = 0)
            : this(sortIndex, Global.ACPropUsages.ConnectionPoint, acGroup, acCaptionTranslation, false, isPersistable, pointCapacity)
        {
        }

        /// <summary>
        /// Gets or sets the point capacity.
        /// </summary>
        /// <value>The point capacity.</value>
        public uint PointCapacity { get; set; }
    }


    /// <summary>
    /// Class ACPropertyPointConfig
    /// </summary>
    public class ACPropertyPointConfig : ACPropertyPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyPointConfig"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="configDataType">Type of the config data.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        /// <param name="pointCapacity">The point capacity.</param>
        public ACPropertyPointConfig(Int16 sortIndex, string acGroup, Type configDataType, string acCaptionTranslation = "", bool isRightmanagement = false, uint pointCapacity = 0)
            : base(sortIndex, Global.ACPropUsages.ConfigPointConfig, acGroup, acCaptionTranslation, "", false, false, false, isRightmanagement, configDataType, pointCapacity)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyPointProperty
    /// </summary>
    public class ACPropertyPointProperty : ACPropertyBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyPointProperty"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="configDataType">Type of the config data.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyPointProperty(Int16 sortIndex, string acGroup, Type configDataType, string acCaptionTranslation = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.ConfigPointProperty, acGroup, acCaptionTranslation, "", false, false, false, isRightmanagement, configDataType)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyConnectionPoint
    /// </summary>
    public class ACPropertyConnectionPoint : ACPropertyPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyConnectionPoint"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyConnectionPoint(Int16 sortIndex, string acGroup, string acCaptionTranslation = "", bool isRightmanagement = false)
                : base(sortIndex, Global.ACPropUsages.ConnectionPoint, acGroup, null, acCaptionTranslation, isRightmanagement, false, 0)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyEventPoint
    /// </summary>
    public class ACPropertyEventPoint : ACPropertyPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyEventPoint"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="acResults">The ac results.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyEventPoint(Int16 sortIndex, bool isPersistable, bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.EventPoint, "", null, "", isRightmanagement, isPersistable, 0)
        {
        }

    }

    /// <summary>
    /// Class ACPropertyEventPointSubscr
    /// </summary>
    public class ACPropertyEventPointSubscr : ACPropertyPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyEventPointSubscr"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="callbackMethod">The callback method.</param>
        public ACPropertyEventPointSubscr(Int16 sortIndex, bool isPersistable, string callbackMethod = "", string acGroup = "", string acCaptionTranslation = "", uint pointCapacity = 0, bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.EventPointSubscr, acGroup, acCaptionTranslation, isRightmanagement, isPersistable, pointCapacity)
        {
            CallbackMethod = callbackMethod;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyEventPointSubscr"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acPropUsage">Usage of the Property. (Point, Selected, Current....)</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="pointCapacity"></param>
        protected ACPropertyEventPointSubscr(Int16 sortIndex, Global.ACPropUsages acPropUsage, string acGroup, string acCaptionTranslation = "", bool isRightmanagement = false, bool isPersistable = false, uint pointCapacity = 0)
            : base(sortIndex, acPropUsage, acGroup, acCaptionTranslation, isRightmanagement, isPersistable, pointCapacity)
        {
        }

        /// <summary>
        /// Gets or sets the callback method.
        /// </summary>
        /// <value>The callback method.</value>
        public string CallbackMethod { get; set; }

    }

    /// <summary>
    /// Class ACPropertyRelationPoint
    /// </summary>
    public class ACPropertyRelationPoint : ACPropertyPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyRelationPoint"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="acSource">If the property can be displayed and selected in an ItemsControl, the ACUrl for an ItemsSource can be specified here.</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyRelationPoint(Int16 sortIndex, string acGroup = "", string acCaptionTranslation = "", string acSource = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.RelationPoint, acGroup, acCaptionTranslation, acSource, false, false, false, isRightmanagement, null, 0)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyAsyncMethodPoint
    /// </summary>
    public class ACPropertyAsyncMethodPoint : ACPropertyPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyAsyncMethodPoint"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="pointCapacity">The point capacity.</param>
        public ACPropertyAsyncMethodPoint(Int16 sortIndex, bool isPersistable = false, uint pointCapacity = 0, string acCaptionTranslation = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.AsyncMethodPoint, "", acCaptionTranslation, isRightmanagement, isPersistable, pointCapacity)
        {
        }
    }

    /// <summary>
    /// Class ACPropertyAsyncMethodPointSubcr
    /// </summary>
    public class ACPropertyAsyncMethodPointSubscr : ACPropertyEventPointSubscr
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyAsyncMethodPointSubscr"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="isPersistable">If the property is persistent, the value is written to the database for each change event (table ACClassTaskValue). The next time the iPlus service is started or the next time it is instantiated, the property receives the last value stored.</param>
        /// <param name="pointCapacity"></param>
        /// <param name="callbackMethod"></param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        public ACPropertyAsyncMethodPointSubscr(Int16 sortIndex, bool isPersistable = false, uint pointCapacity = 0, string callbackMethod = "", string acGroup = "", string acCaptionTranslation = "", bool isRightmanagement = false)
            : base(sortIndex, Global.ACPropUsages.AsyncMethodPointSubscr, acGroup, acCaptionTranslation, isRightmanagement, isPersistable, pointCapacity)
        {
            CallbackMethod = callbackMethod;
        }
    }

    /// <summary>
    /// Class ACPropertyReferenceListPoint
    /// </summary>
    public class ACPropertyReferenceListPoint : ACPropertyPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyReferenceListPoint"/> class.
        /// </summary>
        /// <param name="sortIndex">Sort sequence of the listing of properties in the iPlus development environment. If the SortIndex is > 9999, this property is not displayed in the tool window of the designer.</param>
        /// <param name="acGroup">Used to assign related properties (List, Selected, and Current). Use an empty string for independent properties.</param>
        /// <param name="acCaptionTranslation">Translation tuple for the translation of the property. Example: en{'My english text'}de{'Mein deutscher Text'}</param>
        /// <param name="isRightmanagement">Properties with rights management, different access rights can be assigned for individual users or groups in the Rights management module.</param>
        /// <param name="pointCapacity">The point capacity.</param>
        public ACPropertyReferenceListPoint(Int16 sortIndex, string acGroup, string acCaptionTranslation = "", bool isRightmanagement = false, uint pointCapacity = 0)
            : base(sortIndex, Global.ACPropUsages.ReferenceListPoint, acGroup, acCaptionTranslation, isRightmanagement, false, pointCapacity)
        {
        }
    }
    #endregion



    /// <summary>
    /// Class ACDeleteAction
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class ACDeleteAction : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACDeleteAction"/> class.
        /// </summary>
        /// <param name="acIdentifier">Unique identifier of the property.</param>
        /// <param name="deleteAction">The delete action.</param>
        public ACDeleteAction(string acIdentifier, Global.DeleteAction deleteAction)
        {
            ACIdentifier = acIdentifier;
            DeleteAction = deleteAction;
        }
        #region public Member
        /// <summary>
        /// Unique identifier.
        /// </summary>
        /// <value>The AC identifier.</value>
        public string ACIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the delete action.
        /// </summary>
        /// <value>The delete action.</value>
        public Global.DeleteAction DeleteAction { get; set; }
        #endregion
    }
}
