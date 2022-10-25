// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 12-17-2012
// ***********************************************************************
// <copyright file="ACCommand.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Transactions;
using System.Runtime.CompilerServices;

namespace gip.core.datamodel
{
    /*
     * Onlinehelp: Definition von datenbankunabhängigen Entitäten 
     * 
     * Für einige Anwendungsfälle werden anwendungsspezifischen Entitäten benötigt, 
     * welche keine direkte Verbindung mit der Datenbank besitzen. In diesem Fall
     * handelt es sich um die Menüstruktur, welche im XML-Format in einem einzelnen 
     * Textfeld (ACClassMenu.Menu) in der Datenbank gespeichert wird.
     * 
     * Grundsätzlich gelten alle richtlinien, wie in "Definition von einfachen Entitäten im Datenbankmodell"
     * beschrieben, nur das es natürlich keine Historienfelder (UpdateHistoryAdded, UpdateHistoryModified) 
     * geben muss.
     * 
     * In diesem Fall werden die Daten aus dem XML-Format zu Objekten deserialisiert und 
     * vor dem Speichern wieder serialisiert. Dafür sind die Definition von [DataContract]
     * und [DataMember] notwendig.
     */


    /// <summary>
    /// Class ACCommand
    /// </summary>
    #if !EFCR
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACCommand'}de{'ACCommand'}", Global.ACKinds.TACCommand, Global.ACStorableTypes.NotStorable, true, false)]
    // 1 ACCaption
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACCommand", "en{'ACCommand'}de{'ACCommand'}", typeof(ACCommand), "ACCommand", Const.ACCaptionPrefix, Const.ACCaptionPrefix)]
    public class ACCommand : IACObject, INotifyPropertyChanged
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACCommand"/> class.
        /// </summary>
        /// <param name="acCaption">The ac caption.</param>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="parameterList">The parameter list.</param>
        /// <param name="isAutoEnabled">if set to <c>true</c> [is auto enabled].</param>
        /// <param name="handlerACElement">The handler AC element.</param>
        public ACCommand(string acCaption, string acUrl, ACValueList parameterList, bool isAutoEnabled = false, IACInteractiveObject handlerACElement = null)
        {
            ACCommandID = Guid.NewGuid();
            _ACCaptionTranslation = acCaption;
            _ACUrl = acUrl;
            _ParameterList = parameterList;
            _IsAutoEnabled = isAutoEnabled;
            HandlerACElement = handlerACElement;
        }
        #endregion

        #region IACUrl Member
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
#if !EFCR
        public object ACUrlCommand(string acUrl, params Object[] acParameter)
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
#endif
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

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(9999)]
        public string ACIdentifier
        {
            get 
            { 
                return ACCaption; 
            }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("","", 9999)]
        public string GetACUrl(IACObject rootACObject = null)
        {
            return _ACUrl; 
        }

        /// <summary>
        /// The _ AC URL
        /// </summary>
        string _ACUrl;
        /// <summary>
        /// Gets or sets the AC URL.
        /// </summary>
        /// <value>The AC URL.</value>
        [DataMember]
        [ACPropertyInfo(9999)]
        public string ACUrl
        {
            get 
            { 
                return _ACUrl; 
            }
            set
            {
                _ACUrl = value;
                OnPropertyChanged("ACUrl");
            }
        }


        /// <summary>
        /// The _ AC caption translation
        /// </summary>
        string _ACCaptionTranslation;
        /// <summary>
        /// Gets or sets the AC caption translation.
        /// </summary>
        /// <value>The AC caption translation.</value>
        [DataMember]
        public string ACCaptionTranslation
        {
            get
            {
                return _ACCaptionTranslation;
            }
            set
            {
                _ACCaptionTranslation = value;
                OnPropertyChanged("ACCaptionTranslation");
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [DataMember]
        [ACPropertyInfo(1)]
        public string ACCaption
        {
            get
            {
                return Translator.GetTranslation(_ACCaptionTranslation,_ACCaptionTranslation);
            }
            set
            {
                ACCaptionTranslation = Translator.SetTranslation(_ACCaptionTranslation, value);
                OnPropertyChanged(Const.ACCaptionPrefix);
            }
        }

        [IgnoreDataMember]
        public bool HasTranslations
        {
            get
            {
                return Translator.IsTranslationTupleValid(ACCaptionTranslation);
            }
        }


        /// <summary>
        /// The _ parameter list
        /// </summary>
        ACValueList _ParameterList;
        /// <summary>
        /// Gets or sets the parameter list.
        /// </summary>
        /// <value>The parameter list.</value>
        [DataMember]
        public ACValueList ParameterList
        {
            get
            {
                if (_ParameterList == null)
                    _ParameterList = new ACValueList();
                return _ParameterList;
            }
            set
            {
                _ParameterList = value;
            }
        }

        /// <summary>
        /// The _ is auto enabled
        /// </summary>
        bool _IsAutoEnabled;
        /// <summary>
        /// Gets a value indicating whether this instance is auto enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is auto enabled; otherwise, <c>false</c>.</value>
        public virtual bool IsAutoEnabled
        {
            get { return _IsAutoEnabled; }
        }

        /// <summary>
        /// Gets or sets the handler AC element.
        /// </summary>
        /// <value>The handler AC element.</value>
        [IgnoreDataMember]
        public virtual IACInteractiveObject HandlerACElement
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the BSO.
        /// </summary>
        /// <value>The BSO.</value>
        public IACBSO BSO
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        [ACPropertyInfo(9999)]
        public virtual IACObject ParentACObject
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
#if !EFCR
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return this.ReflectGetACContentList();
            }
        }
#endif
        #endregion

        #region IMenuItem Member

        /// <summary>
        /// Temporäre Guid, damit der Client richtig damit umgehen kann
        /// </summary>
        /// <value>The AC command ID.</value>
        [ACPropertyInfo(9999)]
        public Guid ACCommandID
        {
            get;
            set;
        }
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
                return Const.ACCaptionPrefix;
            }
        }
        #endregion

        #region IACObject Members

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public virtual IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        #endregion

        #region INotifyPropertyChanged Member
        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        public void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
#endif
}
