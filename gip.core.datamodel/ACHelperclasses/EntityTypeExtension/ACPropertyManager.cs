// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-07-2012
// ***********************************************************************
// <copyright file="ACPropertyManager.cs" company="gip mbh, Oftersheim, Germany">
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
using System.Xml;
using System.Data.Objects.DataClasses;
using System.Collections.Specialized;
using System.Threading;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class that handles "Virtual Properties".
    /// With "virtual properties" you can extend the Entity-Framework-Class with additional properties which are automatically serialized and deserialized from XML.
    /// The XML-Serialization is done by this class.
    /// "Virtual Properties" are declared via BSOiPlusStudio by adding new ACClassProperty-Entries to the corresponding ACClass, that represents the Entity-Class (respectively the Database-Table).
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPropertyManager'}de{'ACPropertyManager'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACPropertyManager : IACObject
    {
        #region c'tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPropertyManager"/> class.
        /// </summary>
        /// <param name="entityProperty">The entity property.</param>
        /// <param name="acClass">The ac class.</param>
        public ACPropertyManager(IACEntityProperty entityProperty, ACClass acClass)
        {
            _ACClass = acClass;
            _EntityProperty = entityProperty;
        }
        #endregion

        #region Properties
        private bool _IsSerializing = false;
        private readonly object _Lock = new object();

        /// <summary>
        /// The _ entity property
        /// </summary>
        private IACEntityProperty _EntityProperty;
        /// <summary>
        /// The _ AC class
        /// </summary>
        private ACClass _ACClass;

        /// <summary>
        /// The _ properties
        /// </summary>
        SafeList<ACPropertyExt> _Properties;
        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties.</value>
        public SafeList<ACPropertyExt> Properties
        {
            get
            {
                lock (_Lock)
                {
                    if (_Properties != null)
                        return _Properties;
                }

                Deserialize();

                lock (_Lock)
                {
                    return _Properties;
                }
            }
        }


        /// <summary>
        /// The _ serializer
        /// </summary>
        private DataContractSerializer _Serializer = null;
        /// <summary>
        /// Gets the serializer.
        /// </summary>
        /// <value>The serializer.</value>
        private DataContractSerializer Serializer
        {
            get
            {
                lock (_Lock)
                {
                    if (_Serializer != null)
                        return _Serializer;
                    if (ACKnownTypes.GetKnownType() != null)
                        _Serializer = new DataContractSerializer(typeof(List<ACPropertyExt>), ACKnownTypes.GetKnownType(), 99999999, true, true, null, ACConvert.MyDataContractResolver);
                    else
                        _Serializer = new DataContractSerializer(typeof(List<ACPropertyExt>));
                    return _Serializer;
                }
            }
        }


        /// <summary>
        /// Gets the database entity.
        /// </summary>
        /// <value>The database entity.</value>
        public IACEntityObjectContext DatabaseEntity
        {
            get
            {
                if (EntityObject == null) return null;
                return EntityObject.GetObjectContext();
            }
        }

        public EntityObject EntityObject
        {
            get
            {
                return _EntityProperty as EntityObject;
            }
        }

        #endregion

        #region Methods

        #region public

        /// <summary>
        /// Deserializes this instance.
        /// </summary>
        public void Deserialize()
        {
            string xmlConfig = null;
            lock (_Lock)
            {
                if (_IsSerializing)
                    return;
                xmlConfig = _EntityProperty.XMLConfig;
            }
            if (String.IsNullOrEmpty(xmlConfig))
            {
                lock (_Lock)
                {
                    _Properties = new SafeList<ACPropertyExt>();
                }
                return;
            }

            List<ACPropertyExt> propertyList = null;
            using (StringReader ms2 = new StringReader(xmlConfig))
            using (XmlTextReader xmlReader2 = new XmlTextReader(ms2))
            {
                try
                {
                    lock (_Lock)
                    {
                        var obj = Serializer.ReadObject(xmlReader2);
                        propertyList = obj as List<ACPropertyExt>;
                    }
                }
                catch (Exception e)
                {
                    propertyList = new List<ACPropertyExt>();

                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACPropertyManager", "Deserialize", msg);
                }
            }

            bool removed = false;
            bool attached = false;
            foreach (var acPropertyExt in propertyList.ToArray())
            {
                if (acPropertyExt == null)
                    continue;
                if ((acPropertyExt.ObjectType == null) || String.IsNullOrEmpty(acPropertyExt.ACIdentifier))
                {
                    propertyList.Remove(acPropertyExt);
                    removed = true;
                    continue;
                }
                acPropertyExt.AttachTo(this);
                attached = true;
            }

            lock (_Lock)
            {
                _Properties = new SafeList<ACPropertyExt>(propertyList);
            }

            if (removed)
            {
                Serialize();
            }
            // Nur wenn String.Empty oder Binding.IndexerName: "Item[]" gesendet wird, dann wird DependencyProperty per Binding aktualisiert - sonst gehts nicht laut Microsoft
            // Nachteil ist, dass alle im Indexer gebundenen Propertties gelesen werden
            //_EntityProperty.OnXMLConfigPropertyChanged("Item[" + _Properties.IndexOf(acPropertyExt) + "]");
            //_EntityProperty.OnXMLConfigPropertyChanged("Item[" + acPropertyExt.ACIdentifier + "]");
            if (attached)
                _EntityProperty.OnEntityPropertyChanged(String.Empty);
        }


        /// <summary>
        /// Serializes this instance.
        /// </summary>
        /// <exception cref="System.NotImplementedException">Warum ist PropertyExt-eintrag null?</exception>
        public void Serialize()
        {
            List<ACPropertyExt> propertyList = null;
            lock (_Lock)
            {
                if (_Properties != null)
                    propertyList = _Properties.ToList();
            }

            if (propertyList != null)
            {
                StringBuilder sb1 = new StringBuilder();
                using (StringWriter sw1 = new StringWriter(sb1))
                using (XmlTextWriter xmlWriter1 = new XmlTextWriter(sw1))
                {
                    foreach (var acPropertyExt in propertyList)
                    {
                        if (acPropertyExt == null)
                        {
                            throw new NotImplementedException("Warum ist PropertyExt-eintrag null?");
                        }
                    }

                    lock (_Lock)
                    {
                        Serializer.WriteObject(xmlWriter1, propertyList);
                        try
                        {
                            _IsSerializing = true;
                            _EntityProperty.XMLConfig = sw1.ToString();
                            _IsSerializing = false;
                        }
                        finally
                        {
                            _IsSerializing = false;
                        }
                    }
                }
            }
            else
            {
                lock (_Lock)
                {
                    _EntityProperty.XMLConfig = "";
                }
            }
        }


        /// <summary>
        /// Property: String
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>System.Object.</returns>
        public object this[string property]
        {
            get
            {
                ACPropertyExt acPropertyExt = GetOrCreateACPropertyExtByName(property,true,true);
                if (acPropertyExt == null)
                    return null;
                return acPropertyExt.Value;
            }
            set
            {
                SetACPropertyExtValue(GetOrCreateACPropertyExtByName(property), value);
            }
        }


        /// <summary>
        /// Gets the name of the AC property ext by.
        /// </summary>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <param name="autoCreate"></param>
        /// <param name="subscribePropChanged"></param>
        /// <returns>ACPropertyExt.</returns>
        public ACPropertyExt GetOrCreateACPropertyExtByName(string acIdentifier, bool autoCreate = true, bool subscribePropChanged = false)
        {
            ACPropertyExt typeExt = this.Properties.Where(c => c.ACIdentifier == acIdentifier).FirstOrDefault();
            if (typeExt != null && subscribePropChanged)
                SubscribePropertyChanged(typeExt);
            if (typeExt != null || !autoCreate)
                return typeExt;

            if (acIdentifier == "ACCommandMsgList")
            {
                typeExt = new ACPropertyExt();
                typeExt.ACIdentifier = acIdentifier;
                typeExt.ObjectType = typeof(ACCommandMsgList);
                typeExt.Value = new ACCommandMsgList();
                Properties.Add(typeExt);
            }
            else
            {
                typeExt = NewACPropertyExtByName(acIdentifier);
                if (typeExt != null)
                {
                    Properties.Add(typeExt);
                    Serialize();
                }
                else
                {
                    throw new MissingMemberException(String.Format("Class/Entity {0} doesn't have a virtual extended Property with Name {1}. Please extend this Entity with this Propertyname in iPlus development environment.", _ACClass.ACIdentifier, acIdentifier));
                }
            }
            return typeExt;
        }


        /// <summary>
        /// Sets the AC property ext value.
        /// </summary>
        /// <param name="acPropertyExt">The ac property ext.</param>
        /// <param name="value">The value.</param>
        public void SetACPropertyExtValue(ACPropertyExt acPropertyExt, object value)
        {
            if (acPropertyExt != null)
            {
                bool differentObject = false;
                if (acPropertyExt.Value != value)
                    differentObject = true;
                if (differentObject)
                    UnSubscribePropertyChanged(acPropertyExt);
                acPropertyExt.Value = value;
                if (differentObject)
                    SubscribePropertyChanged(acPropertyExt);
                // Nur wenn String.Empty oder Binding.IndexerName: "Item[]" gesendet wird, dann wird DependencyProperty per Binding aktualisiert - sonst gehts nicht laut Microsoft
                // Nachteil ist, dass alle im Indexer gebundenen Propertties gelesen werden
                //_EntityProperty.OnXMLConfigPropertyChanged("Item[" + _Properties.IndexOf(acPropertyExt) + "]");
                //_EntityProperty.OnXMLConfigPropertyChanged("Item[" + acPropertyExt.ACIdentifier + "]");
                _EntityProperty.OnEntityPropertyChanged(String.Empty);
                _EntityProperty.OnEntityPropertyChanged(acPropertyExt.ACIdentifier);
            }
            Serialize();
        }

        /// <summary>
        /// Refresh() deserializes the list if Properties are loaded
        /// </summary>
        public void Refresh()
        {
            lock (_Lock)
            {
                if (_Properties == null)
                    return;
            }
            Deserialize();
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
            ACCommandMsgList acCommandMsgList = this["ACCommandMsgList"] as ACCommandMsgList;
            if (acCommandMsgList == null)
            {
                acCommandMsgList = new ACCommandMsgList();
                this["ACCommandMsgList"] = acCommandMsgList;
            }
            acCommandMsgList.Add(new ACCommandMsg(acUrlInfo, acComment, acCaption, acUrl, parameterList));
            SetACPropertyExtValue(GetOrCreateACPropertyExtByName("ACCommandMsgList"), acCommandMsgList);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// News the name of the AC property ext by.
        /// </summary>
        /// <param name="acName">Name of the ac.</param>
        /// <returns>ACPropertyExt.</returns>
        private ACPropertyExt NewACPropertyExtByName(string acName)
        {
            // Schaue in ACTypeInfoExtList ob konfiguriert
            if (_ACClass == null)
                return null;

            IACType acTypeInfo = _ACClass.ExtendedProperties.Where(c => c.ACIdentifier == acName).FirstOrDefault();
            if (acTypeInfo != null)
            {
                // Falls konfiguriert, Erzeuge Config-Class und weise
                ACPropertyExt acPropertyExt = new ACPropertyExt();
                acPropertyExt.ACIdentifier = acTypeInfo.ACIdentifier;
                acPropertyExt.ObjectType = acTypeInfo.ObjectFullType;
                acPropertyExt.AttachTo(this);

                Type objectType = acPropertyExt.ObjectType;
                if (objectType == typeof(System.String))
                {
                    String s = "";
                    acPropertyExt.Value = s;
                }
                else if (objectType.IsValueType)
                {
                    try
                    {
                        acPropertyExt.Value = Activator.CreateInstance(acPropertyExt.ObjectType);
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("ACPropertyManager", "NewACPropertyExtByName", msg);
                    }
                }
                // Objekte müssen vom Benutzer erzeugt werden
                else if (!objectType.IsValueType)
                {
                    //acPropertyExt.Value = Activator.CreateInstance(acPropertyExt.ObjectType);
                    acPropertyExt.Value = null;
                }
                return acPropertyExt;
            }
            if (_EntityProperty is IACContainer)
            {
                var entityProperty = _EntityProperty as IACContainer;
                if (entityProperty.ValueTypeACClass != null /*&& entityProperty.ValueTypeACClass.ACIdentifier == acName*/)
                {
                    ACPropertyExt acPropertyExt = new ACPropertyExt();
                    acPropertyExt.ACIdentifier = acName;
                    acPropertyExt.ObjectType = entityProperty.ValueTypeACClass.ObjectFullType;
                    acPropertyExt.AttachTo(this);
                    if (acPropertyExt.ObjectType == typeof(System.String))
                    {
                        String s = "";
                        acPropertyExt.Value = s;
                    }
                    else
                    {
                        ConstructorInfo constructor = acPropertyExt.ObjectType.GetConstructor(Type.EmptyTypes);
                        if (constructor != null)
                        {
                            try
                            {
                                acPropertyExt.Value = Activator.CreateInstance(acPropertyExt.ObjectType);
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (Database.Root != null && Database.Root.Messages != null)
                                    Database.Root.Messages.LogException("ACPropertyManager", "NewACPropertyExtByName(10)", msg);
                            }
                        }
                    }
                    return acPropertyExt;
                }
            }
            if (_EntityProperty is ACClassProperty)
            {
                var entityProperty = _EntityProperty as ACClassProperty;
                if (entityProperty.ConfigACClass != null && entityProperty.ConfigACClass.ACIdentifier == acName)
                {
                    ACPropertyExt acPropertyExt = new ACPropertyExt();
                    acPropertyExt.ACIdentifier = acName;
                    acPropertyExt.ObjectType = entityProperty.ConfigACClass.ObjectFullType;
                    acPropertyExt.AttachTo(this);
                    if (acPropertyExt.ObjectType == typeof(System.String))
                    {
                        String s = "";
                        acPropertyExt.Value = s;
                    }
                    else
                    {
                        try
                        {
                            acPropertyExt.Value = Activator.CreateInstance(acPropertyExt.ObjectType);
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (Database.Root != null && Database.Root.Messages != null)
                                Database.Root.Messages.LogException("ACPropertyManager", "NewACPropertyExtByName(20)", msg);
                        }
                    }
                    return acPropertyExt;
                }

            }
            return null;
        }


        /// <summary>
        /// Called when [AC property ext changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnACPropertyExtChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var acPropertyExt in Properties)
            {
                if (acPropertyExt != null)
                {
                    if (acPropertyExt.Value == sender)
                    {
                        acPropertyExt.SerializeValueToXML();
                        break;
                    }
                }
            }
            Serialize();
        }

        private void OnACPropertyListChanged(object sender, ListChangedEventArgs e)
        {
            foreach (var acPropertyExt in Properties)
            {
                if (acPropertyExt != null)
                {
                    if (acPropertyExt.Value == sender)
                    {
                        acPropertyExt.SerializeValueToXML();
                        break;
                    }
                }
            }
            Serialize();
        }

        private void OnACPropertyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var acPropertyExt in Properties)
            {
                if (acPropertyExt != null)
                {
                    if (acPropertyExt.Value == sender)
                    {
                        acPropertyExt.SerializeValueToXML();
                        break;
                    }
                }
            }
            Serialize();
        }


        /// <summary>
        /// Subscribes the property changed.
        /// </summary>
        /// <param name="acPropertyExt">The ac property ext.</param>
        private void SubscribePropertyChanged(ACPropertyExt acPropertyExt)
        {
            if ((acPropertyExt == null) || (acPropertyExt.Value == null))
                return;
            if (acPropertyExt.PropertyChangedSubscribed)
                return;
            if (acPropertyExt.Value is INotifyPropertyChanged)
            {
                (acPropertyExt.Value as INotifyPropertyChanged).PropertyChanged += OnACPropertyExtChanged;
                acPropertyExt.PropertyChangedSubscribed = true;
            }
            else if (acPropertyExt.Value is INotifyCollectionChanged)
            {
                (acPropertyExt.Value as INotifyCollectionChanged).CollectionChanged += new NotifyCollectionChangedEventHandler(OnACPropertyCollectionChanged);
                acPropertyExt.PropertyChangedSubscribed = true;
            }
            else if (acPropertyExt.Value is IBindingList)
            {
                (acPropertyExt.Value as IBindingList).ListChanged += new ListChangedEventHandler(OnACPropertyListChanged);
                acPropertyExt.PropertyChangedSubscribed = true;
            }
        }

        /// <summary>
        /// Uns the subscribe property changed.
        /// </summary>
        /// <param name="acPropertyExt">The ac property ext.</param>
        private void UnSubscribePropertyChanged(ACPropertyExt acPropertyExt)
        {
            if ((acPropertyExt == null) || (acPropertyExt.Value == null))
                return;
            if (!acPropertyExt.PropertyChangedSubscribed)
                return;
            if (acPropertyExt.Value is INotifyPropertyChanged)
            {
                (acPropertyExt.Value as INotifyPropertyChanged).PropertyChanged -= OnACPropertyExtChanged;
                acPropertyExt.PropertyChangedSubscribed = false;
            }
            else if (acPropertyExt.Value is INotifyCollectionChanged)
            {
                (acPropertyExt.Value as INotifyCollectionChanged).CollectionChanged -= OnACPropertyCollectionChanged;
                acPropertyExt.PropertyChangedSubscribed = false;
            }
            else if (acPropertyExt.Value is IBindingList)
            {
                (acPropertyExt.Value as IBindingList).ListChanged -= OnACPropertyListChanged;
                acPropertyExt.PropertyChangedSubscribed = false;
            }
        }

        #endregion

        #endregion

        #region IACObject
        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return null; }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return null; }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return null; }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return null; }
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
            return null;
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return false;
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
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }
        #endregion
    }
}
