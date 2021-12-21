// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 01-17-2013
// ***********************************************************************
// <copyright file="Msg.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Universal Message-Class.
    /// Methods that returns a instance of this Msg-Class to inform the invoker about a problem otherwise they return null.
    /// Messagetexts are translated depending on the VBUser-Language.
    /// This class is serializable to be able to be send over the network.
    /// Messages can be written either to the database (Table MsgAlarmLog) or Logfile.
    /// Messages can also be displayed on th GUI e.g. in a MessageBox. Use Root.Messages to open a dialog.
    /// </summary>
    [DataContract]
#if NETFRAMEWORK
    [ACSerializeableInfo]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "Msg", "en{'Message'}de{'Meldung'}", typeof(Msg), "Msg", "Source", "Source,ACIdentifier")]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Message'}de{'Meldung'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class Msg : IACObject, IACEntityProperty, INotifyPropertyChanged
#else
    public class Msg : INotifyPropertyChanged
#endif
    {
        public static readonly DateTime MsgUnsetDateTime = new DateTime(1900,1,1);

        #region c'tors
        /// <summary>
        /// Initializes a new instance of the <see cref="Msg"/> class.
        /// </summary>
        public Msg()
        {
            _MsgId = Guid.NewGuid();
            _TimeStampOccurred = DateTime.Now;
            _TimeStampAcknowledged = MsgUnsetDateTime;
        }

        public Msg(eMsgLevel msgLevel, string message) : this()
        {
            MessageLevel = msgLevel;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Msg"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="alarmPropertyName">Name of the alarm property.</param>
        public Msg(string source, string alarmPropertyName) : this()
        {
            this.Source = source;
            this.ACIdentifier = alarmPropertyName;
        }

#if NETFRAMEWORK
        public Msg(IACComponent source, eMsgLevel msgLevel, string className, string methodName, int sourceRowID, string translID) : this()
        {
            Source = source.GetACUrl();
            SourceComponent = new ACRef<IACComponent>(source, true);
            MessageLevel = msgLevel;
            ACIdentifier = String.Format("{0}({1})", methodName, sourceRowID);
            Column = Msg.CreateHashCodeForMethod(className, methodName);
            Row = sourceRowID;
            TranslID = translID;
            Message = source.Root.Environment.TranslateMessage(source, TranslID);
        }

        public Msg(IACComponent source, eMsgLevel msgLevel, string className, string methodName, int sourceRowID, string translID, params object[] parameter) : this()
        {
            Source = source.GetACUrl();
            SourceComponent = new ACRef<IACComponent>(source, true);
            MessageLevel = msgLevel;
            ACIdentifier = String.Format("{0}({1})", methodName, sourceRowID);
            Column = Msg.CreateHashCodeForMethod(className, methodName);
            Row = sourceRowID;
            TranslID = translID;
            Message = source.Root.Environment.TranslateMessage(source, TranslID, parameter);
        }

        public Msg(string message, IACComponent source, eMsgLevel msgLevel, string className, string methodName, int sourceRowID) : this()
        {
            Source = source.GetACUrl();
            MessageLevel = msgLevel;
            ACIdentifier = String.Format("{0}({1})", methodName, sourceRowID);
            Column = Msg.CreateHashCodeForMethod(className, methodName);
            Row = sourceRowID;
            Message = message;
        }

        public Msg(IACComponent source, eMsgLevel msgLevel, string className, string methodName, int sourceRowID, string translID, eMsgButton msgButton) : this()
        {
            Source = source.GetACUrl();
            SourceComponent = new ACRef<IACComponent>(source, true);
            MessageLevel = msgLevel;
            ACIdentifier = String.Format("{0}({1})", methodName, sourceRowID);
            Column = Msg.CreateHashCodeForMethod(className, methodName);
            Row = sourceRowID;
            TranslID = translID;
            Message = source.Root.Environment.TranslateMessage(source, TranslID);
            MessageButton = msgButton;
        }

        public Msg(IACComponent source, eMsgLevel msgLevel, string className, string methodName, int sourceRowID, string translID, eMsgButton msgButton, 
                    params object[] parameter) : this()
        {
            Source = source.GetACUrl();
            SourceComponent = new ACRef<IACComponent>(source, true);
            MessageLevel = msgLevel;
            ACIdentifier = String.Format("{0}({1})", methodName, sourceRowID);
            Column = Msg.CreateHashCodeForMethod(className, methodName);
            Row = sourceRowID;
            TranslID = translID;
            Message = source.Root.Environment.TranslateMessage(source, TranslID, parameter);
            MessageButton = msgButton;
        }

        public Msg(string message, IACComponent source, eMsgLevel msgLevel, string className, string methodName, int sourceRowID, eMsgButton msgButton) : this()
        {
            Source = source.GetACUrl();
            MessageLevel = msgLevel;
            ACIdentifier = String.Format("{0}({1})", methodName, sourceRowID);
            Column = Msg.CreateHashCodeForMethod(className, methodName);
            Row = sourceRowID;
            Message = message;
            MessageButton = msgButton;
        }
#endif

        #endregion

        #region Properties

        [IgnoreDataMember]
        private Guid _MsgId;

        /// <summary>
        /// Unique message id
        /// </summary>
        /// <value>Unique message id</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(9999)]
#endif
        public Guid MsgId
        {
            get
            {
                return _MsgId;
            }
            set
            {
                _MsgId = value;
            }
        }



        [IgnoreDataMember]
        private eMsgLevel _MessageLevel;

        /// <summary>Gets or sets the message level.</summary>
        /// <value>The message level.</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(1, "", "en{'Messagelevel'}de{'Fehlerart'}" )]
#endif
        public eMsgLevel MessageLevel
        {
            get
            {
                return _MessageLevel;
            }
            set
            {
                _MessageLevel = value;
                OnPropertyChanged("MessageLevel");
            }
        }

        [IgnoreDataMember]
        private eMsgButton _MessageButton;

        [DataMember]
        public eMsgButton MessageButton
        {
            get
            {
                return _MessageButton;
            }
            set
            {
                _MessageButton = value;
                OnPropertyChanged("MessageButton");
            }
        }

        [IgnoreDataMember]
        private string _ACIdentifier;

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(2, "", "en{'Messagekey'}de{'Fehlerschlüssel'}")]
#endif
        public string ACIdentifier
        {
            get
            {
                return _ACIdentifier;
            }
            set
            {
                _ACIdentifier = value;
                OnPropertyChanged(Const.ACIdentifierPrefix);
            }
        }



        [IgnoreDataMember]
        private string _Message;

        /// <summary>Messagetext</summary>
        /// <value>Messagetext</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(3, "", "en{'Message'}de{'Meldung'}")]
#endif
        public virtual string Message
        {
            get
            {
                return _Message;
            }
            set
            {
                _Message = value;
                OnPropertyChanged("Message");
            }
        }

        [IgnoreDataMember]
        public virtual string InnerMessage
        {
            get
            {
                return Message;
            }
        }

        [IgnoreDataMember]
        private string _Source;

        /// <summary>
        /// Source who published this message
        /// </summary>
        /// <value>The source</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(4, "", "en{'Source'}de{'Quelle'}" )]
#endif
        public string Source
        {
            get
            {
                return _Source;
            }
            set
            {
                _Source = value;
                OnPropertyChanged("Source");
            }
        }


        [IgnoreDataMember]
        private int _Column;

        /// <summary>
        /// Hashcode of Class-ID and Method-ID for finding the right code place where this message was generated.
        /// </summary>
        /// <value>Hashcode</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(14, "", "en{'Hashcode Class+Method'}de{'Hashcode Klasse+Methode'}")]
#endif
        public int Column
        {
            get
            {
                return _Column;
            }
            set
            {
                _Column = value;
                OnPropertyChanged("Column");
            }
        }


        [IgnoreDataMember]
        private int _Row;

        /// <summary>
        /// Row-ID in the method where this message was generated.
        /// </summary>
        /// <value>Row-ID in Method</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(15, "", "en{'Row-ID in Method'}de{'Zeilen-ID in Methode'}")]
#endif
        public int Row
        {
            get
            {
                return _Row;
            }
            set
            {
                _Row = value;
                OnPropertyChanged("Row");
            }
        }


        [IgnoreDataMember]
        private string _TranslID;

        /// <summary>
        /// ID of the Messagetext (Translation-ID in Table ACClassMessage)
        /// </summary>
        /// <value>ID of the Translationtext</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(6, "", "en{'Messagetext-ID'}de{'Meldungstext-ID'}")]
#endif
        public string TranslID
        {
            get
            {
                return _TranslID;
            }
            set
            {
                _TranslID = value;
                OnPropertyChanged("TranslID");
            }
        }


        [DataMember]
        private DateTime _TimeStampOccurred;

        /// <summary>
        /// Time when this message was generated.
        /// </summary>
        /// <value>Time when this message was generated.</value>
        [IgnoreDataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(6, "", "en{'Occured'}de{'Aufgetaucht'}")]
#endif
        public DateTime TimeStampOccurred
        {
            get
            {
                return _TimeStampOccurred;
            }
            set
            {
                _TimeStampOccurred = value;
                OnPropertyChanged("TimeStampOccurred");
            }
        }


        [IgnoreDataMember]
        private DateTime _TimeStampAcknowledged;

        /// <summary>
        /// Time when this message was acknowledged.
        /// </summary>
        /// <value>Time when this message was acknowledged</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(7, "", "en{'Acknowledged'}de{'Quittiert'}")]
#endif
        public DateTime TimeStampAcknowledged
        {
            get
            {
                return _TimeStampAcknowledged;
            }
            set
            {
                _TimeStampAcknowledged = value;
                OnPropertyChanged("TimeStampAcknowledged");
            }
        }

        [IgnoreDataMember]
        private string _AcknowledgedBy;

        /// <summary>
        /// User who has acknowledged this message
        /// </summary>
        /// <value>VBUser</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(8, "", "en{'Acknowledged by'}de{'Quittiert von'}")]
#endif
        public string AcknowledgedBy
        {
            get
            {
                return _AcknowledgedBy;
            }
            set
            {
                _AcknowledgedBy = value;
                OnPropertyChanged("AcknowledgedBy");
            }
        }



        [IgnoreDataMember]
        private String _XMLConfig;

        /// <summary>
        /// XMLConfig
        /// </summary>
        /// <value>XMLConfig</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(5, "", "en{'Data'}de{'Daten'}")]
#endif
        public string XMLConfig
        {
            get
            {
                return _XMLConfig;
            }
            set
            {
#if NETFRAMEWORK
                OnXMLConfigChanging(value);
#endif
                _XMLConfig = value;
                OnPropertyChanged(Const.EntityXMLConfig);
#if NETFRAMEWORK
                OnXMLConfigChanged();
#endif
            }
        }


        [IgnoreDataMember]
        private short _ConfigIconStateValue;

        [DataMember]
        public short ConfigIconStateValue
        {
            get
            {
                return _ConfigIconStateValue;
            }
            set
            {
                _ConfigIconStateValue = value;
                OnPropertyChanged("ConfigIconStateValue");
                OnPropertyChanged("ConfigIconState");
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance is acknowledged.
        /// </summary>
        /// <value><c>true</c> if this instance is acknowledged; otherwise, <c>false</c>.</value>
        public bool IsAcknowledged
        {
            get
            {
                if (TimeStampAcknowledged == MsgUnsetDateTime)
                    return false;
                return true;
            }
        }

#if NETFRAMEWORK

        /// <summary>Translated Label/Description of the Property/Method that has created this message (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [IgnoreDataMember]
        [ACPropertyInfo(9, "", "en{'Caption'}de{'Bezeichnung'}")]
        public string ACCaption
        {
            get
            {
                return Translator.GetACPropertyCaption(Source, ACIdentifier);
            }
        }


        /// <summary>Translated Label/Description of the source (depends on the current logon)</summary>
        /// <value>Translated description</value>
        [IgnoreDataMember]
        [ACPropertyInfo(10, "", "en{'Caption Component'}de{'Bezeichnung Komponente'}")]
        public String ACCaptionComponent
        {
            get
            {
                return Translator.GetACComponentCaption(Source);
            }
        }


        /// <summary>Translated Label/Description of the Component-Comment (depends on the current logon)</summary>
        /// <value>Translated Comment</value>
        [IgnoreDataMember]
        [ACPropertyInfo(13, "", "en{'Comment Component'}de{'Kommentar Komponente'}")]
        public String ACCommentComponent
        {
            get
            {
                return Translator.GetACComponentComment(Source);
            }
        }


        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        [IgnoreDataMember]
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
        [IgnoreDataMember]
        public IEnumerable<IACObject> ACContentList
        {
            get { return null; }
        }


        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        [IgnoreDataMember]
        public IACObject ParentACObject
        {
            get;
            set;
        }


        [IgnoreDataMember]
        private Dictionary<string, string> _CustomerData;
        [IgnoreDataMember]
        public Dictionary<string, string> CustomerData
        {
            get
            {
                if (_CustomerData == null)
                    _CustomerData = new Dictionary<string, string>();
                return _CustomerData;
            }
            set
            {
                _CustomerData = value;
            }
        }


        [IgnoreDataMember]
        private ACRef<IACComponent> _SourceComponent;

        /// <summary>
        /// ACRef to the Source if it's a ACComponent
        /// </summary>
        /// <value>ACRef to the Source if it's a ACComponent.</value>
        [IgnoreDataMember]
        public ACRef<IACComponent> SourceComponent
        {
            get
            {
                if ((_SourceComponent == null || _SourceComponent.ValueT == null) && !String.IsNullOrEmpty(Source) && Source.StartsWith("\\"))
                    _SourceComponent = new ACRef<IACComponent>(Source, ACRef<IACComponent>.RefInitMode.NoInstantiation, null, true, true);
                return _SourceComponent;
            }
            set
            {
                _SourceComponent = value;
                if (_SourceComponent != null && _SourceComponent.ValueT != null)
                    Source = _SourceComponent.ValueT.GetACUrl();
                OnPropertyChanged("SourceComponent");
            }
        }

        [ACPropertyInfo(999)]
        [IgnoreDataMember]
        public Global.ConfigIconState ConfigIconState
        {
            get
            {
                return (Global.ConfigIconState) ConfigIconStateValue;
            }
            set
            {
                ConfigIconStateValue = (short) value;
            }
        }
#endif

        #endregion

        #region Methods

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }

        public static int CreateHashCodeForMethod(string className, string methodName)
        {
            string methodID = className + methodName;
            return methodID.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified MSG is equal.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns><c>true</c> if the specified MSG is equal; otherwise, <c>false</c>.</returns>
        public virtual bool IsEqual(Msg msg)
        {
            if (msg == null)
                return false;
            if (MessageLevel != msg.MessageLevel)
                return false;
            if (Source != msg.Source)
                return false;
            if (ACIdentifier != msg.ACIdentifier)
                return false;
            if (Message != msg.Message)
                return false;
            if (XMLConfig != msg.XMLConfig)
                return false;
            return true;
        }

#if NETFRAMEWORK
        /// <summary>
        /// Acknowledges the specified acknowledged by.
        /// </summary>
        /// <param name="acknowledgedBy">The acknowledged by.</param>
        public void Acknowledge(VBUser acknowledgedBy)
        {
            if (IsAcknowledged)
                return;
            if (acknowledgedBy == null)
            {
                _TimeStampAcknowledged = DateTime.Now;
                return;
            }
            if (AcknowledgedBy == null)
            {
                _AcknowledgedBy = acknowledgedBy.Initials;
                OnPropertyChanged("AcknowledgedBy");
            }
            _TimeStampAcknowledged = DateTime.Now;
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

        #region IACEntityProperty Member
        /// <summary>
        /// Gets or sets the <see cref="System.Object"/> with the specified property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>System.Object.</returns>
        public object this[string property]
        {
            get
            {
                return ACProperties[property];
            }
            set
            {
                ACProperties[property] = value;
            }
        }

        [IgnoreDataMember]
        bool _RefreshConfig = false;
        void OnXMLConfigChanging(global::System.String value)
        {
            _RefreshConfig = false;
            if (!(String.IsNullOrEmpty(value) && String.IsNullOrEmpty(XMLConfig)) && value != XMLConfig)
                _RefreshConfig = true;
        }

        void OnXMLConfigChanged()
        {
            if (_RefreshConfig)
                ACProperties.Refresh();
        }


        [IgnoreDataMember]
        ACPropertyManager _ACPropertyManager = null;
        /// <summary>
        /// Gets the AC properties.
        /// </summary>
        /// <value>The AC properties.</value>
        [IgnoreDataMember]
        public ACPropertyManager ACProperties
        {
            get 
            {
                if (_ACPropertyManager == null)
                {
                    _ACPropertyManager = new ACPropertyManager(this, this.ReflectACType() as ACClass);
                }
                return _ACPropertyManager;
            }
        }

        /// <summary>
        /// Adds the AC command MSG.
        /// </summary>
        /// <param name="acUrlInfo">The ac URL info.</param>
        /// <param name="acComment">The ac comment.</param>
        /// <param name="acCaption">The ac caption.</param>
        /// <param name="acUrl">The ac URL.</param>
        /// <param name="parameterList">The parameter list.</param>
        public void AddACCommandMsg(string acUrlInfo, string acComment, string acCaption = null, string acUrl = null, ACValueList parameterList = null)
        {
            ACProperties.AddACCommandMsg(acUrlInfo, acComment, acCaption, acUrl, parameterList);
        }

        public void OnEntityPropertyChanged(string property)
        {
            OnPropertyChanged(property);
        }
        #endregion
#endif

        #endregion

    }
}
