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
// <copyright file="ACSortItem.cs" company="gip mbh, Oftersheim, Germany">
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

namespace gip.core.datamodel
{
    /// <summary>
    /// ACSortItem corresponds to a sort line in the ACSortColumns list.
    /// </summary>
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACSortItem'}de{'ACSortItem'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, false)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACSortItem", "en{'ACSortItem'}de{'ACSortItem'}", typeof(ACSortItem), "ACSortItem", "PropertyName", "PropertyName")]
    public class ACSortItem : IACObject, INotifyPropertyChanged, ICloneable, IComparable
    {
        #region c'tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACSortItem"/> class.
        /// </summary>
        public ACSortItem()
        {
            //SortItemID = Guid.NewGuid();
            SortDirection = Global.SortDirections.ascending;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ACSortItem"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="sortDirection">The sort direction.</param>
        /// <param name="isConfiguration">if set to <c>true</c> [is configuration].</param>
        public ACSortItem(string propertyName, Global.SortDirections sortDirection, bool isConfiguration)
        {
            //SortItemID = Guid.NewGuid();
            PropertyName = propertyName;
            SortDirection = sortDirection;
            IsConfiguration = isConfiguration;
        }
        #endregion

        #region Properties

        #region Serializable

        private string _PropertyName;
        /// <summary>
        /// Relative path (separated by a dot) to the property (or table field) to be sorted by.
        /// </summary>
        /// <value>Relative path as string</value>
        [DataMember]
        [ACPropertyInfo(1, "", "en{'Field'}de{'Feld'}")]
        public string PropertyName
        {
            get
            {
                return _PropertyName;
            }

            set
            {
                _PropertyName = value;
                OnPropertyChanged("PropertyName");
            }
        }

        private Global.SortDirections _SortDirection;
        /// <summary>
        /// Describes the sorting direction: Ascending or descending.
        /// </summary>
        /// <value>The sort direction.</value>
        [DataMember]
        [ACPropertyInfo(2, "", "en{'Direction'}de{'Richtung'}")]
        public Global.SortDirections SortDirection
        {
            get
            {
                return _SortDirection;
            }

            set
            {
                _SortDirection = value;
                OnPropertyChanged("SortDirection");
            }
        }

        private bool _IsConfiguration;
        /// <summary>
        /// Indicates whether the sort line was created from the program code. If "False", the user created the fit line using the GUI.
        /// </summary>
        /// <value><c>true</c> if this instance is configuration; otherwise, <c>false</c>.</value>
        [DataMember]
        [ACPropertyInfo(3, "", "en{'Configuration'}de{'Konfiguration'}")]
        public bool IsConfiguration
        {
            get
            {
                return _IsConfiguration;
            }
            set
            {
                _IsConfiguration = value;
                OnPropertyChanged("IsConfiguration");
            }
        }
        #endregion

        #region IACObject Member
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return PropertyName; }
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

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return null; }
        }


        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return PropertyName; }
        }
        #endregion

        #region Static
        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        static public string KeyACIdentifier
        {
            get
            {
                return "PropertyName";
            }
        }
        #endregion

        #endregion

        #region Methods

        #region Clone
        public virtual void CopyFrom(ACSortItem from)
        {
            if (from == null)
                return;
            this.PropertyName = from.PropertyName;
            this.SortDirection = from.SortDirection;
            this.IsConfiguration = from.IsConfiguration;
        }

        public virtual object Clone()
        {
            ACSortItem clone = new ACSortItem();
            clone.CopyFrom(this);
            return clone;
        }
        #endregion

        #region IACObject Member
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
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
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

        #endregion
        
        #endregion

        #region Eventhandler
        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <param name="info">The info.</param>
        private void OnPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public int CompareTo(object obj)
        {
            ACSortItem acSortItem = obj as ACSortItem;
            if (acSortItem == null)
                return -1;
            if (this.PropertyName == acSortItem.PropertyName
                && this.SortDirection == acSortItem.SortDirection
                && this.IsConfiguration == acSortItem.IsConfiguration)
            {
                return 0;
            }
            return -1;
        }
        #endregion

    }
}
