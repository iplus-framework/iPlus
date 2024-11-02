// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Runtime.CompilerServices;
// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACClassMethodInfo.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Transactions;

namespace gip.core.datamodel
{
    /// <summary>
    /// Container für ein ACClassMethod
    /// Verwendung: Für die Rechteverwaltung im BSOGroup
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Rightiteminfo Method'}de{'Rechteinfo Methode'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACClassMethodInfo", "en{'ACClassMethodInfo'}de{'ACClassMethodInfo'}", typeof(ACClassMethodInfo), "ACClassMethodInfo", "ValueT\\ACIdentifier", "ValueT\\ACIdentifier")]
    public class ACClassMethodInfo : IACContainerT<ACClassMethod>, INotifyPropertyChanged, IACObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACClassMethodInfo"/> class.
        /// </summary>
        public ACClassMethodInfo()
        {
        }

        /// <summary>
        /// The _ control mode
        /// </summary>
        Global.ControlModes _ControlMode;
        /// <summary>
        /// Gets or sets the control mode.
        /// </summary>
        /// <value>The control mode.</value>
        [ACPropertyInfo(9999, "", "en{'Control Mode'}de{'Steuerelement-Modus'}")]
        public Global.ControlModes ControlMode
        {
            get
            {
                return _ControlMode;
            }
            set
            {
                _ControlMode = value;
                OnPropertyChanged("ControlMode");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is right.
        /// </summary>
        /// <value><c>true</c> if this instance is right; otherwise, <c>false</c>.</value>
        [ACPropertyInfo(9999)]
        public bool IsRight
        {
            get
            {
                return ControlMode == Global.ControlModes.Enabled;
            }
            set
            {
                ControlMode = value ? Global.ControlModes.Enabled : Global.ControlModes.Hidden;
                OnPropertyChanged("IsRight");
            }
        }

        /// <summary>
        /// Gets or sets the control mode info.
        /// </summary>
        /// <value>The control mode info.</value>
        [ACPropertyInfo(9999, "", "", Const.ContextDatabase + "\\MethodControlModeList")]
        public ACValueItem ControlModeInfo
        {
            get
            {
                return  Global.ControlModeList.Where(c => (Global.ControlModes)c.Value == ControlMode).FirstOrDefault();
            }
            set
            {
                ControlMode = value == null ? Global.ControlModes.Hidden : (Global.ControlModes)value.Value;
            }
        }

        /// <summary>
        /// Gets the key AC identifier.
        /// </summary>
        /// <value>The key AC identifier.</value>
        static public string KeyACIdentifier
        {
            get
            {
                return "ValueT\\ACIdentifier";
            }
        }

        #region INotifyPropertyChanged Member

        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region IACValue
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
            }
        }

        /// <summary>Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!</summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        public ACClass ValueTypeACClass
        {
            get
            {
                return ValueT == null ? null : ValueT.ACType as ACClass;
            }
        }

        ACClassMethod _ValueT;
        /// <summary>Gets or sets the encapsulated value of the generic type T.
        /// T is ACClassMethod</summary>
        /// <value>The encapsulated ACClassMethod</value>
        [ACPropertyInfo(9999)]
        public ACClassMethod ValueT
        {
            get
            {
                return _ValueT;
            }
            set
            {
                _ValueT = value;
                OnPropertyChanged(Const.ValueT);
            }
        }
        #endregion

        #region IACObject Member

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return ValueT.ACIdentifier; }
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
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return null; }
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

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return ValueT.ACIdentifier; }
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
    }
}
