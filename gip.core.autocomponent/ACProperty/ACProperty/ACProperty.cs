using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using gip.core.datamodel;
using System.Threading;
using System.Globalization;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Signature of the Delegate-Method for a Callback of Script-Setter-Methods
    /// </summary>
    /// <param name="value"></param>
    /// <param name="callingProperty"></param>
    public delegate void ACPropertySetMethodScript(object value, IACMember callingProperty);


    /// <summary>
    /// Baseclass for all Properties in a ACComponent, that are published with the [ACPropertyInfo]-Attribute to the iPlus-Framework.
    /// It Implements IACContainerT and IACPropertyBase. IACContainerT is the Generic version of IACContainer. A Container is a object that encapsulates a value which is from type IACObject.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="gip.core.datamodel.IACContainerT{T}" />
    /// <seealso cref="gip.core.datamodel.IACPropertyBase" />
    public class ACProperty<T> : IACContainerT<T>, IACPropertyBase
    {
        #region Events and Delegates
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region c'tors
        public ACProperty(IACComponent acComponent, ACClassProperty acClassProperty, bool IsProxy)
        {
            _IsProxy = IsProxy;
            _ACTypeInfo = acClassProperty;
            _ACRefParent = new ACRef<IACComponent>(acComponent, true);

            if (!IsProxy
                || (IsProxy && !acClassProperty.IsBroadcast)) // Bei ObjectProxy müssen lokale Memeber view z.B. ACClassInfo zugreifbar sein
            {
                // Weise Property der Member-Property der Klasse zu
                try
                {
                    Type typeOfThis = acComponent.GetType();
                    PropertyInfo propertyInfo = typeOfThis.GetProperty(acClassProperty.ACIdentifier);
                    //if (propertyInfo != null && !propertyInfo.PropertyType.IsGenericType && !propertyInfo.PropertyType.IsInterface)
                    if (propertyInfo != null && !propertyInfo.PropertyType.Name.StartsWith(Const.IACPropertyPrefix) && !propertyInfo.PropertyType.Name.StartsWith(Const.IACContainerTNetPrefix))
                    {
                        PropertyAccessor = new DynamicPropertyAccessor(propertyInfo);
                        acComponent.PropertyChanged += OnPropertyAccessorChanged;
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACProperty<T>", "ACProperty", msg);
                }
                if (!IsProxy && !IsSetMethodAssigned)
                {
                    if ((acComponent as ACComponent).ScriptEngine != null)
                    {
                        if ((acComponent as ACComponent).ScriptEngine.ExistsScript(ScriptTrigger.Type.OnSetACProperty, acClassProperty.ACIdentifier))
                            SetMethodOfScript = ((ACComponent)acComponent).OnSetACProperty;
                    }
                }
            }
        }

        public virtual void ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            Type typeT = typeof(T);
            if (typeT != null)
            {
                if (typeof(ACCustomTypeBase).IsAssignableFrom(typeT))
                {
                    this.ValueT = (T)Activator.CreateInstance(typeT, new Object[] { _ACTypeInfo });
                    (this.ValueT as ACCustomTypeBase).PropertyChanged += OnCustomTypeChanged;
                }
            }
            ReStoreFromDB(true);
        }

        public virtual void ACDeInit(bool deleteACClassTask = false)
        {
            var parentComponent = ParentACComponent;
            if (_ACRefParent != null)
            {
                if (_ACRefParent.ValueT != null && _ACRefParent.ValueT is ACComponent)
                {
                    ACComponent acComponent = _ACRefParent.ValueT as ACComponent;
                    if (acComponent.InitState == ACInitState.Destructing)
                    {
                        acComponent.PropertyChanged -= OnPropertyAccessorChanged;
                        _ACRefParent.Detach();
                        _ACRefParent = null;
                    }
                }
            }
            if (_ValueT != null && _ValueT is ACCustomTypeBase)
            {
                (_ValueT as ACCustomTypeBase).PropertyChanged -= OnCustomTypeChanged;
            }
            if (parentComponent != null)
            {
                if (parentComponent.InitState != ACInitState.DisposingToPool)
                {
                    _PropertyAccessor = null;
                }
                else
                {
                    // Falls Netzwerkproperty und GarbageCollector legt die Komponente im Pool ab, dann Reset
                    if (parentComponent.IsProxy && _PropertyAccessor == null)
                    {
                        SetDefaultValue();
                    }
                }
            }

            _LiveLog = null;
            if (deleteACClassTask)
            {
                ACClassTaskValue taskValue = null;

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    taskValue = _ACClassTaskValue;
                    _ACClassTaskValue = null;
                }

                if (taskValue != null)
                {
                    ACClassTaskQueue.TaskQueue.Add(() =>
                    {
                        try 
                        { 
                            if (taskValue.EntityState != System.Data.EntityState.Deleted
                                && taskValue.EntityState != System.Data.EntityState.Detached)
                                ACClassTaskQueue.TaskQueue.Context.Detach(taskValue);
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("ACProperty<T>", "ACDeInit", msg);
                        }
                    }
                    );
                }

                try
                {
                    _RestoreRuntimeActive = true;
                    SetDefaultValue();
                }
                finally
                {
                    _RestoreRuntimeActive = false;
                }
            }
        }


        /// <summary>
        /// Implizite Zuweisung:
        /// int x = 5;
        /// ACProperty<int> y;
        /// x = y;
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static implicit operator T(ACProperty<T> value)
        {
            return value.ValueT;
        }
        #endregion

        #region Properties

        #region Local


        private ACRef<IACComponent> _ACRefParent;
        /// <summary>Smart-Pointer to the Parent ACComponent where this instance belongs to.</summary>
        /// <value>The parent ac component.</value>
        public ACRef<IACComponent> ACRef
        {
            get
            {
                return _ACRefParent;
            }
        }

        protected IACType _ACTypeInfo;
        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return _ACTypeInfo;
            }
        }

        /// <summary>iPlus-Type (Metadata) of this property.</summary>
        /// <value>ACClassProperty</value>
        public ACClassProperty PropertyInfo
        {
            get
            {
                return ACType as ACClassProperty;
            }
        }


        /// <summary>Parent ACComponent where this instance belongs to.</summary>
        /// <value>The parent ac component.</value>
        public IACComponent ParentACComponent
        {
            get
            {
                if (_ACRefParent == null)
                    return null;
                return _ACRefParent.ValueT;
            }
        }


        /// <summary>
        /// This method is called from the iPlus-Framework for each member of a ACComponent when a component was recycled from the component-pool (ACInitState.RecycledFromPool) instead of a new creation.
        /// </summary>
        /// <param name="recycledComponent">The recycled component.</param>
        public void RecycleMemberAndAttachTo(IACComponent recycledComponent)
        {
            if (recycledComponent != null && _ACRefParent != null)
            {
                _ACRefParent.Value = recycledComponent;
                SetDefaultValue();
            }
        }

        protected virtual void SetDefaultValue()
        {
            this._ValueT = default(T);
        }

        /// <summary>
        /// Resets the Value-Property to a default value.
        /// </summary>
        public void ResetToDefaultValue()
        {
            ValueT = default(T);
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [ACPropertyInfo(9999)]
        public string ACIdentifier
        {
            get
            {
                return _ACTypeInfo.ACIdentifier;
            }
        }

        protected bool _IsProxy;
        /// <summary>
        /// Falls Property einem Proxy-Objekt angehört
        /// </summary>
        public virtual bool IsProxy
        {
            get
            {
                if (_IsProxy && IsSetMethodAssigned)
                    _IsProxy = false;
                return _IsProxy;
            }
        }

        protected DynamicPropertyAccessor _PropertyAccessor = null;
        public DynamicPropertyAccessor PropertyAccessor
        {
            get
            {
                return _PropertyAccessor;
            }
            set
            {
                if (_PropertyAccessor == null)
                    _PropertyAccessor = value;
            }
        }

        public virtual void OnPropertyAccessorChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyAccessor == null)
                return;
            if ((e.PropertyName == this.ACType.ACIdentifier) && (ACRef.ValueT != null))
            {
                Persist();
                OnMemberChanged();
            }
        }


        /// <summary>
        /// Delegat-Methode für Callbacks auf Skript-Setter-Methoden
        /// </summary>
        private ACPropertySetMethodScript _SetMethodACObject = null;
        public ACPropertySetMethodScript SetMethodOfScript
        {
            get
            {
                return _SetMethodACObject;
            }

            set
            {
                _SetMethodACObject = value;
                // Referenz zu Set-Methode im Objekt hat zur Folge, dass es sich um ein Realse Objekt handelt => _IsProxy = false
                if ((_SetMethodACObject != null) && (IsProxy))
                    _IsProxy = false;
            }
        }

        public virtual bool IsSetMethodAssigned
        {
            get
            {
                if (_SetMethodACObject == null)
                    return false;
                return true;
            }
        }

        #endregion

        #region Public
        public readonly ACMonitorObject _20015_LockValue = new ACMonitorObject(20015);

        //static Type _TypeOfACState = typeof(ACStateEnum);
        //bool? _IsACStateProperty = null;
        //public bool IsACStateProperty
        //{
        //    get
        //    {

        //        using (ACMonitor.Lock(_20015_LockValue))
        //        {
        //            if (_IsACStateProperty.HasValue)
        //                return _IsACStateProperty.Value;
        //            _IsACStateProperty = typeof(T) == _TypeOfACState && this.ACIdentifier == Const.ACState;
        //            return _IsACStateProperty.Value;
        //        }
        //    }
        //}

        //private static bool _LogACState = false;
        //public static bool LogACState
        //{
        //    get
        //    {
        //        return _LogACState;
        //    }
        //    set
        //    {
        //        _LogACState = value;
        //    }
        //}

        bool _ValueReadFromServer = false;
        protected T _ValueT;
        /// <summary>Gets or sets the encapsulated value of the generic type T</summary>
        /// <value>The Value-Property as generic type</value>
        public virtual T ValueT
        {
            get
            {
                // Falls Assembly-Getter/Setter
                if (PropertyAccessor != null)
                    return (T)PropertyAccessor.Get(ACRef.ValueT);
                if (IsProxy)
                {
                    if (ACRef.ValueT is ACComponentProxy)
                    {
                        if (!ACKnownTypes.IsTypeBroadcastable(typeof(T)))
                            return default(T);
                        if (_ValueReadFromServer && PropertyInfo.ForceBroadcast)
                            return _ValueT;
                        object result = (ACRef.ValueT as ACComponentProxy).InvokeACUrlCommand(this.GetACUrlComponent());
                        if (result == null)
                        {
                            _ValueT = default(T);
                            return _ValueT;
                        }
                        _ValueReadFromServer = true;
                        _ValueT = (T)result;
                        return _ValueT;
                    }
                }
                else
                {
                    // Sonst hinzugefügte Property Serverseitig
                    return _ValueT;
                }
                return default(T);
            }

            set
            {
                // Falls Assembly-Getter/Setter im Realen Objekt
                if (this.PropertyAccessor != null)
                {
                    this.PropertyAccessor.Set(ACRef.ValueT, value);
                    Persist();
                    // Property-Changed wird in der Setter-Methode ausgelöst
                }
                // Sonst Falls Property im Proxy-Objekt
                else if (IsProxy)
                {
                    if (ACRef.ValueT is ACComponentProxy)
                    {
                        if (!ACKnownTypes.IsTypeBroadcastable(typeof(T)))
                            return;
                        (ACRef.ValueT as ACComponentProxy).InvokeACUrlCommand(this.GetACUrlComponent(), value);
                        OnMemberChanged();
                    }
                }
                // Sonst manuell erweiterte Eigenschaft im Realen objekt
                // Oder Eigenschaft, die mit ACProperty-Interface-Deklaration definiert wurde aber nicht Broadcastable ist
                else
                {
                    if (ACRef.ValueT != null)
                    {
                        _ValueT = value;
                        Persist();
                        OnMemberChanged();
                    }
                }
            }
        }

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        public object Value
        {
            get
            {
                return this.ValueT;
            }

            set
            {
                if (value != null)
                {
                    try
                    {
                        this.ValueT = (T)ACConvert.ChangeType(value, (object)this.ValueT, this.ACType.ObjectFullType, true, (this.ACType as ACClassProperty).Database, true, false);
                    }
                    catch (Exception e)
                    {
                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        {
                            datamodel.Database.Root.Messages.LogException("ACProperty<T>", "Value.Set(1)", e.Message);
                            if (e.InnerException != null)
                                datamodel.Database.Root.Messages.LogException("ACProperty<T>", "Value.Set(2)", e.InnerException.Message);
                        }
                    }
                }
                else
                    this.ValueT = default(T);
            }
        }

        /// <summary>Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!</summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        public ACClass ValueTypeACClass
        {
            get
            {
                return this.ACType.ValueTypeACClass as ACClass;
            }
        }


        /// <summary>Returns the Value-Property of this insntance as a serialized string (XML).</summary>
        /// <param name="xmlIndented">if set to <c>true</c> the XML is indented.</param>
        /// <returns>XML</returns>
        public string ValueSerialized(bool xmlIndented = false)
        {
            string valueXML = "";
            try
            {
                if (this.ValueT != null)
                {
                    if (this.ValueT is String)
                        valueXML = this.ValueT as String;
                    else if (this.ValueT is DateTime)
                    {
                        DateTime utcDate = ((DateTime)(object)this.ValueT).ToUniversalTime();
                        valueXML = utcDate.ToString("o");
                    }
                    else if (this.ValueT is TimeSpan)
                    {
                        valueXML = ((TimeSpan)(object)this.ValueT).ToString("c");
                    }
                    else if (/*(this.ValueT is IFormattable) && */(this.ValueT is IConvertible))
                    {
                        valueXML = ACConvert.ObjectToXML(this.ValueT, true);
                        //valueXML = this.ValueT.ToString();
                    }
                    else if (this.IsSerializable)
                    {
                        StringBuilder sb = new StringBuilder();
                        using (StringWriter sw = new StringWriter(sb))
                        using (XmlTextWriter xmlWriter = new XmlTextWriter(sw))
                        {
                            if (xmlIndented)
                                xmlWriter.Formatting = Formatting.Indented;
                            DataContractSerializer serializer = new DataContractSerializer(typeof(T), ACKnownTypes.GetKnownType(), 99999999, true, true, null, ACConvert.MyDataContractResolver);
                            serializer.WriteObject(xmlWriter, this.ValueT);
                            valueXML = sw.ToString();
                        }
                    }
                    else
                        return null;
                }
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("ACProperty<T>", "ValueSerialized", msg);
            }
            return valueXML;
        }
        #endregion

        #region Persist
        private Nullable<bool> _IsSerializable;
        private bool IsSerializable
        {
            get
            {
                if (_IsSerializable.HasValue)
                    return _IsSerializable.Value;
                _IsSerializable = ACKnownTypes.IsKnownType(typeof(T));
                return _IsSerializable.Value;
            }
        }

        protected ACClassTaskValue _ACClassTaskValue = null;
        /// <summary>
        /// Thread-Safe Access to _ACClassTaskValue
        /// </summary>
        public ACClassTaskValue ACClassTaskValue
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _ACClassTaskValue;
                }
            }
            internal set
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    _ACClassTaskValue = value;
                }
            }
        }

        /// <summary>
        /// Restores the stored value from the database into this persistable property
        /// </summary>
        /// <param name="IsInit">if set to <c>true</c> [is init].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public virtual bool ReStoreFromDB(bool IsInit)
        {
            bool restored = RestoreRuntimeValue();
            ACClassTaskValue acClassTaskValue = ACClassTaskValue;
            if (!restored && acClassTaskValue == null)
                RestoreDefaultValue();
            return restored;
        }

        protected virtual bool RestoreDefaultValue()
        {
            if (IsProxy)
                return false;
            // Falls Default-Value in Datenbank hinterlegt, setzte Variable
            // PropertyInfo is Intity-Objekt vom Globalen Datebase-Context jedoch ist er nur lesend, daher nicht nötig für den Einsatz einer Lesse-Sperre
            if (!String.IsNullOrEmpty(PropertyInfo.Value as string))
            {
                try
                {
                    _RestoreRuntimeActive = true;
                    if (this.ValueT != null)
                    {
                        if (this.ValueT is String)
                            this.ValueT = (T)(object)PropertyInfo.Value;
                        else if (this.ValueT is DateTime)
                        {
                            string defaultValue = PropertyInfo.Value as string;
                            this.ValueT = (T)(object)DateTime.ParseExact(defaultValue, "o", CultureInfo.InvariantCulture, DateTimeStyles.None);
                        }
                        else if (this.ValueT is TimeSpan)
                        {
                            string defaultValue = PropertyInfo.Value as string;
                            this.ValueT = (T)(object)TimeSpan.ParseExact(defaultValue, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None);
                        }
                        else if (/*(this.ValueT is IFormattable) && */(this.ValueT is IConvertible))
                        {
                            Type typeT = typeof(T);
                            if (typeT.IsEnum)
                            {
                                string defaultValue = PropertyInfo.Value as string;
                                this.ValueT = (T)Enum.Parse(typeT, defaultValue);
                            }
                            else
                            {
                                this.ValueT = (T)ACConvert.ChangeType(PropertyInfo.Value as string, this.ValueT, typeof(T), true, gip.core.datamodel.Database.GlobalDatabase);
                                //this.ValueT = (T)Convert.ChangeType(PropertyInfo.Value, typeT);
                            }
                        }
                    }
                    else
                    {
                        Type typeT = typeof(T);
                        if (typeT == typeof(String))
                            this.ValueT = (T)(object)PropertyInfo.Value;
                        else if (typeT == typeof(DateTime))
                        {
                            string defaultValue = PropertyInfo.Value as string;
                            this.ValueT = (T)(object)DateTime.ParseExact(defaultValue, "o", CultureInfo.InvariantCulture, DateTimeStyles.None);
                        }
                        else if (typeT == typeof(TimeSpan))
                        {
                            string defaultValue = PropertyInfo.Value as string;
                            this.ValueT = (T)(object)TimeSpan.ParseExact(defaultValue, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None);
                        }
                        else if ((typeT.GetInterface("IConvertible") != null)/* && (typeT.GetInterface("IFormattable") != null)*/)
                        {
                            if (typeT.IsEnum)
                                this.ValueT = (T)Enum.Parse(typeT, PropertyInfo.Value as string);
                            else
                            {
                                //this.ValueT = (T)Convert.ChangeType(PropertyInfo.Value, typeT);
                                this.ValueT = (T)ACConvert.ChangeType(PropertyInfo.Value as string, typeof(T), true, gip.core.datamodel.Database.GlobalDatabase);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (ACRef.IsObjLoaded)
                    {
                        ACRef.ValueT.Messages.LogException(this.GetACUrl(), "ACProperty.RestoreDefaultValue()", e.Message);
                    }
                }
                finally
                {
                    _RestoreRuntimeActive = false;
                }
            }
            // Sonst normales Property Source/Target/Lokal oder erweitertes Property für das es keinen Datenbankwert gibt
            // => Setze default-Wert?
            //else if (_PropertyAccessor == null)
            //this.ValueT = default(T);
            return true;
        }



        private bool _RestoreRuntimeActive = false;
        /// <summary>
        /// Inidcates if this property is currently in the Init-Phase and the persisted value isn't still read from the database.
        /// </summary>
        /// <value><c>true</c> if [in restore phase]; otherwise, <c>false</c>.</value>
        public bool InRestorePhase
        {
            get
            {
                return _RestoreRuntimeActive;
            }
        }

        protected virtual bool RestoreRuntimeValue()
        {
            if (IsProxy || !PropertyInfo.IsPersistable || !ACRef.IsObjLoaded || (ACRef.ValueT.ContentTask == null) || (ACRef.ValueT.ACOperationMode == ACOperationModes.Test))
                return false;

            bool restored = false;
            try
            {
                string valueXML = null;
                ACClassTaskValue acClassTaskValue = null;

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    acClassTaskValue = _ACClassTaskValue;
                }

                if (acClassTaskValue == null)
                {
                    if (ACClassTaskQueue.TaskQueue.MassLoadPropertyValuesOff)
                    {
                        ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                            {
                                acClassTaskValue = ACRef.ValueT.ContentTask.ACClassTaskValue_ACClassTask.
                                            Where(c => (c.ACClassPropertyID == ACType.ACTypeID)).FirstOrDefault();
                            }
                        );
                    }
                    else
                    {
                        acClassTaskValue = ACClassTaskQueue.TaskQueue.GetFromAllPropValues(ACRef.ValueT.ContentTask.ACClassTaskID,
                                                                                        ACType.ACTypeID,
                                                                                        null);
                    }

                    using (ACMonitor.Lock(_20015_LockValue))
                    {
                        if (_ACClassTaskValue == null && acClassTaskValue != null)
                            _ACClassTaskValue = acClassTaskValue;
                    }

                }

                if (acClassTaskValue != null)
                    ACClassTaskQueue.TaskQueue.ProcessAction(() => { valueXML = acClassTaskValue.XMLValue; });

                _RestoreRuntimeActive = true;
                if (!String.IsNullOrEmpty(valueXML))
                {
                    if (this.ValueT != null)
                    {
                        if (this.ValueT is String)
                        {
                            this.ValueT = (T)(object)valueXML;
                            restored = true;
                        }
                        else if (this.ValueT is DateTime)
                        {
                            this.ValueT = (T)(object)DateTime.ParseExact(valueXML, "o", CultureInfo.InvariantCulture, DateTimeStyles.None);
                            restored = true;
                        }
                        else if (this.ValueT is TimeSpan)
                        {
                            this.ValueT = (T)(object)TimeSpan.ParseExact(valueXML, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None);
                            restored = true;
                        }
                        else if (this.ValueT is IConvertible)
                        {
                            this.ValueT = (T)ACConvert.ChangeType(valueXML, this.ValueT, typeof(T), true, gip.core.datamodel.Database.GlobalDatabase);
                            restored = true;
                        }
                    }
                    else
                    {
                        Type typeT = typeof(T);
                        if (typeT == typeof(String))
                        {
                            this.ValueT = (T)(object)valueXML;
                            restored = true;
                        }
                        else if (typeT == typeof(DateTime))
                        {
                            this.ValueT = (T)(object)DateTime.ParseExact(valueXML, "o", CultureInfo.InvariantCulture, DateTimeStyles.None);
                            restored = true;
                        }
                        else if (typeT == typeof(TimeSpan))
                        {
                            this.ValueT = (T)(object)TimeSpan.ParseExact(valueXML, "c", CultureInfo.InvariantCulture, TimeSpanStyles.None);
                            restored = true;
                        }
                        else if (typeT.GetInterface("IConvertible") != null)
                        {
                            this.ValueT = (T)ACConvert.ChangeType(valueXML, typeof(T), true, gip.core.datamodel.Database.GlobalDatabase);
                            restored = true;
                        }
                    }
                    if (!restored && this.IsSerializable)
                    {
                        using (StringReader ms = new StringReader(valueXML))
                        using (XmlTextReader xmlReader = new XmlTextReader(ms))
                        {
                            DataContractSerializer serializer = new DataContractSerializer(typeof(T), ACKnownTypes.GetKnownType(), 99999999, true, true, null, ACConvert.MyDataContractResolver);
                            this.ValueT = (T)serializer.ReadObject(xmlReader);
                        }
                        restored = true;
                    }
                }
            }
            catch (Exception e)
            {
                if (ACRef.IsObjLoaded)
                {
                    ACRef.ValueT.Messages.LogException(this.GetACUrl(), "ACProperty.RestoreRuntimeValue()", e.Message);
                }
            }
            finally
            {
                _RestoreRuntimeActive = false;
            }
            return restored;
        }


        /// <summary>Writes the current value to the database.
        /// It this property is persistable, then the current value is serialized (invokes ValueSerialized()) and set to the XMLValue-Property.</summary>
        /// <returns>
        ///   <c>true</c> if sucessful</returns>
        public virtual bool Persist()
        {
            if (InRestorePhase
                || IsProxy
                || !PropertyInfo.IsPersistable
                || !ACRef.IsObjLoaded
                || (ACRef.ValueT.ContentTask == null)
                || ACRef.ValueT.Root.PropPersistenceOff
                || (ACRef.ValueT.ACOperationMode == ACOperationModes.Test))
                return false;

            string valueXML = null;
            try
            {
                valueXML = ValueSerialized(false);
                if (valueXML == null)
                    return false;

                ACClassTaskValue acClassTaskValue = null;

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    acClassTaskValue = _ACClassTaskValue;
                }

                if (acClassTaskValue == null)
                {
                    ACClassTaskQueue.TaskQueue.ProcessAction(() =>
                    {
                        acClassTaskValue = ACRef.ValueT.ContentTask.ACClassTaskValue_ACClassTask.
                                    Where(c => (c.ACClassPropertyID == ACType.ACTypeID)).FirstOrDefault();
                        if (acClassTaskValue == null)
                        {
                            ACClassProperty acClassProperty = ACClassTaskQueue.TaskQueue.GetACClassPropertyFromTaskQueueCache(ACType.ACTypeID);
                            //var queryClass = RootDbOpQueue.ACClassTaskQueue.Context.ACClass.Where(c => c.ACClassID == ((ACClass)ACRef.ValueT.ACType).ACClassID);
                            acClassTaskValue = ACClassTaskValue.NewACClassTaskValue(ACClassTaskQueue.TaskQueue.Context, ACRef.ValueT.ContentTask, acClassProperty);
                            ACRef.ValueT.ContentTask.ACClassTaskValue_ACClassTask.Add(acClassTaskValue);
                        }
                    }
                    );
                }


                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_ACClassTaskValue == null && acClassTaskValue != null)
                        _ACClassTaskValue = acClassTaskValue;
                }

                if (acClassTaskValue != null)
                {
                    ACClassTaskQueue.TaskQueue.Add(() =>
                        {
                            if (acClassTaskValue != null
                                && acClassTaskValue.EntityState != System.Data.EntityState.Deleted
                                && acClassTaskValue.EntityState != System.Data.EntityState.Detached)
                                acClassTaskValue.XMLValue = valueXML;
                        }
                    );
                }
            }
            catch (Exception e)
            {
                string message1 = e.Message;
                string message2 = "";
                if (e.InnerException != null)
                    message2 = e.InnerException.Message;

                IACComponent writer = null;
                if (ACRef != null && ACRef.IsObjLoaded)
                    writer = ACRef.ValueT;
                else
                    writer = Database.Root;
                if (writer != null)
                {
                    writer.Messages.LogException("\\", "ACProperty.Persist(1)", message1);
                    writer.Messages.LogException("\\", "ACProperty.Persist(2)", message2);
                    writer.Messages.LogException("\\", "ACProperty.Persist(3)", e.StackTrace);
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if Property is in valid Range
        /// </summary>
        public Global.ControlModesInfo IsMinMaxValid
        {
            get
            {
                if (ParentACComponent == null)
                {
                    Global.ControlModesInfo minMax = Global.ControlModesInfo.Enabled;
                    if (this.Value == null)
                        minMax.IsNull = true;
                }
                return (ParentACComponent as ACComponent).CheckPropertyMinMax(PropertyInfo, this.Value, this);
            }
        }

        #endregion

        #endregion

        #region EventHandling Methods
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
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
        public virtual object ACUrlCommand(string acUrl, params Object[] acParameter)
        {
            if (IsProxy && (_PropertyAccessor == null))
            {
                if (ACRef.ValueT is ACComponentProxy)
                {
                    return (T)(ACRef.ValueT as ACComponentProxy).InvokeACUrlCommand(this.GetACUrlComponent());
                }
                return null;
            }
            else
            {
                if (this.ValueT is IACObject)
                {
                    if (acParameter == null || !acParameter.Any())
                    {
                        return ((IACObject)ValueT).ACUrlCommand(acUrl);
                    }
                    else
                    {
                        return ((IACObject)ValueT).ACUrlCommand(acUrl, acParameter);
                    }
                }
                else
                {

                    if (acParameter == null || !acParameter.Any())
                    {
                        return ACUrlCommandIntern(acUrl);
                    }
                    else
                    {
                        return ACUrlCommandIntern(acUrl, acParameter);
                    }
                }
            }
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public virtual bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            if (IsProxy)
            {
                if (ACRef.ValueT is ACComponentProxy)
                {
                    return (ACRef.ValueT as ACComponentProxy).IsEnabledExecuteACMethod(acUrl, acParameter);
                }
                return false;
            }
            else
            {
                if (this.ValueT is IACObject)
                {
                    if (acParameter == null || !acParameter.Any())
                    {
                        return ((IACObject)ValueT).IsEnabledACUrlCommand(acUrl);
                    }
                    else
                    {
                        return ((IACObject)ValueT).IsEnabledACUrlCommand(acUrl, acParameter);
                    }
                }
                else
                {
                    return true;
                    //if (acParameter == null || !acParameter.Any())
                    //{
                    //    return ACUrlCommandIntern(acUrl);
                    //}
                    //else
                    //{
                    //    return ACUrlCommandIntern(acUrl, acParameter);
                    //}
                }
            }
        }

        protected object ACUrlCommandIntern(string acUrl, params object[] acParameter)
        {
            if (string.IsNullOrEmpty(acUrl))
                return null;
            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    if (ParentACComponent == null)
                        return null;
                    return ParentACComponent.ACUrlCommand(acUrl, acParameter);
                case ACUrlHelper.UrlKeys.Child:
                    {

                        IACType acTypeInfo = ACType.GetMember(acUrlHelper.ACUrlPart);
                        if (acTypeInfo == null)
                        {
                            PropertyInfo pi = GetType().GetProperty(acUrlHelper.ACUrlPart);
                            if (pi == null)
                                return null;
                            if (acParameter == null || !acParameter.Any())
                                return pi.GetValue(this, null);
                            return null;
                        }

                        switch (acTypeInfo.ACKind)
                        {
                            case Global.ACKinds.PSProperty:
                                {
                                    if (ValueT == null)
                                        return null;
                                    Type t = ValueT.GetType();
                                    PropertyInfo valueObject = t.GetProperty(acUrlHelper.ACUrlPart);
                                    if (valueObject == null)
                                        return null;
                                    if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                    {
                                        if (acParameter == null || !acParameter.Any())
                                        {
                                            return valueObject.GetValue(ValueT, null);
                                        }
                                        else
                                        {
                                            valueObject.SetValue(ParentACComponent, acParameter[0], null);
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        if (valueObject is IACObject)
                                        {
                                            return ((IACObject)valueObject).ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                                        }
                                        else
                                        {
                                            return ACUrlCommandAssembly(valueObject.GetValue(ValueT), acTypeInfo, acUrlHelper.NextACUrl, acParameter);
                                        }
                                    }
                                }
                            case Global.ACKinds.PSPropertyExt:
                                {
                                    if (!(ParentACComponent is IACEntityProperty))
                                        return null;
                                    IACEntityProperty entityProperty = ParentACComponent as IACEntityProperty;
                                    if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                                    {
                                        if (acParameter == null || !acParameter.Any())
                                        {
                                            return entityProperty[acUrlHelper.ACUrlPart];
                                        }
                                        else
                                        {
                                            entityProperty[acUrlHelper.ACUrlPart] = acParameter[0];
                                            return null;
                                        }
                                    }
                                    else
                                    {
                                        object valueObject = entityProperty[acUrlHelper.ACUrlPart];
                                        if (valueObject == null)
                                            return null;
                                        if (valueObject is IACObject)
                                        {
                                            return ((IACObject)valueObject).ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                                        }
                                        else
                                        {
                                            return ACUrlCommandAssembly(valueObject, acTypeInfo, acUrlHelper.NextACUrl, acParameter);
                                        }
                                    }
                                }
                            default:
                                return null;
                        }
                    }
                case ACUrlHelper.UrlKeys.Parent:
                    return ParentACComponent.ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                default:
                    return null; // TODO: Fehlerbehandlung
            }
        }

        public object ACUrlCommandAssembly(object entity, IACType entityACTypeInfo, string acUrl, params Object[] acParameter)
        {
            if (entity == null) return null;
            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Child:
                    {
                        Type t = entity.GetType();
                        PropertyInfo valueObject = t.GetProperty(acUrlHelper.ACUrlPart);
                        if (valueObject == null)
                            return null;
                        if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
                        {
                            if (acParameter == null || !acParameter.Any())
                            {
                                return valueObject.GetValue(entity);
                            }
                            else
                            {
                                valueObject.SetValue(entity, acParameter[0], null);
                                return null;
                            }
                        }
                        else
                        {
                            if (valueObject is IACObject)
                            {
                                return ((IACObject)valueObject).ACUrlCommand(acUrlHelper.NextACUrl, acParameter);
                            }
                            else
                            {
                                IACType acTypeInfo = entityACTypeInfo.GetMember(acUrlHelper.ACUrlPart);
                                if (acTypeInfo == null)
                                    return null;
                                return ACUrlCommandAssembly(valueObject, acTypeInfo, acUrlHelper.NextACUrl, acParameter);
                            }
                        }
                    }
                default:
                    return null;
            }
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
            acTypeInfo = ACType;
            if (typeof(IACComponent).IsAssignableFrom(typeof(T)))
            {
                if ((Value != null) && !string.IsNullOrEmpty(acUrl))
                {
                    return ((IACComponent)Value).ACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                }
                else
                {
                    source = ParentACComponent;
                    path = ACType.GetACPath(true);
                }
                if (!string.IsNullOrEmpty(acUrl))
                    return false;
            }
            else if (typeof(IACEntityObjectContext).IsAssignableFrom(typeof(T)) && Value != null)
            {
                if (!string.IsNullOrEmpty(acUrl))
                {
                    source = ParentACComponent;
                    path = ACType.GetACPath(true);
                    return ((IACEntityObjectContext)Value).ACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                }
                else
                {
                    source = Value;
                    path = ACType.GetACPath(true);
                }
                return true;
            }
            else
            {
                if (!string.IsNullOrEmpty(acUrl) && (acUrl.Length > 7) && acUrl.StartsWith("typeof?"))
                {
                    string acTypeMembername = acUrl.Substring(7);
                    source = _ACTypeInfo;
                    path = acTypeMembername;
                    acUrl = "";
                }
                // Falls Proxy-Objekt, dann muss wert über die Value-Eigenschaft (Netzwerkzugriff)
                else if (IsProxy)
                {
                    source = this;
                    path = Const.ValueT;
                    if (!String.IsNullOrEmpty(this.PropertyInfo.GenericType))
                    {
                        if (this.PropertyInfo.GenericType == typeof(ACRef<>).FullName
                            || this.PropertyInfo.GenericType == typeof(IACContainerT<>).FullName)
                            path = "ValueT.ValueT";
                    }
                }
                // Sonst reales Objekt
                // Falls dem reales Objekt eine zusätzlich virtuelle Eigenschaft in der virtuellen Abtleitung hinzugefügt worden ist
                // oder die Property-Deklaration im realen Objekt eine IACProperty-Deklaration ist und keine normale getter/setter-Property ist,
                // dann Zugriff auch per Value
                else if ((ACType.ACKind == Global.ACKinds.PSPropertyExt)
                        || (PropertyAccessor == null))
                {
                    source = this;
                    path = Const.ValueT;
                    if (!String.IsNullOrEmpty(this.PropertyInfo.GenericType))
                    {
                        if (this.PropertyInfo.GenericType == typeof(ACRef<>).FullName
                            || this.PropertyInfo.GenericType == typeof(IACContainerT<>).FullName)
                            path = "ValueT.ValueT";
                    }
                }
                // Sonst Zugriff per getter/setter-Methode im realen Objekt
                else
                {
                    source = ParentACComponent;
                    path = ACType.GetACPath(true);
                }
            }
            if (string.IsNullOrEmpty(acUrl))
            {
                ACClassProperty cp = acTypeInfo as ACClassProperty;
                ACClass acClassRight = cp.ACClass; // Basisklasse in der die PRoperty definiert ist
                rightControlMode = acClassRight.RightManager.GetControlMode(cp);
                if ((ParentACComponent != null) && (ParentACComponent.ACType != null))
                {
                    acClassRight = ParentACComponent.ACType as ACClass; // Am weitest unten abgeleitetet Klasse
                    rightControlMode = acClassRight.RightManager.GetControlMode(cp);
                }
                return true;
            }

            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrl);
            switch (acUrlHelper.UrlKey)
            {
                case ACUrlHelper.UrlKeys.Root:
                    if (ParentACComponent == null)
                        return false;
                    return ParentACComponent.ACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                case ACUrlHelper.UrlKeys.Child:
                    {
                        string[] parts = acUrl.Split('\\');
                        foreach (var part in parts)
                        {
                            ACClassProperty acp = null;
                            IACType memberTypeInfo = acTypeInfo.GetMember(part);
                            // Falls Property(memberTypeInfo) nicht exisitiert, dann ist es ein Generischer Typ z.B. ACRef<>
                            if (memberTypeInfo == null && acTypeInfo is ACClassProperty &&  !String.IsNullOrEmpty((acTypeInfo as ACClassProperty).GenericType))
                            {
                                acp = acTypeInfo as ACClassProperty;
                                if (acp != null)
                                {
                                    memberTypeInfo = Database.GlobalDatabase.GetACType(acp.ObjectGenericType);
                                    if (memberTypeInfo != null)
                                        memberTypeInfo = memberTypeInfo.GetMember(part);
                                }
                            }
                            else if (memberTypeInfo == null && Value != null && Value is IACObject)
                            {
                                IACObject sourceACObject = Value as IACObject;
                                if (sourceACObject.ACType != null)
                                    memberTypeInfo = sourceACObject.ACType.GetMember(part);
                            }
                            acTypeInfo = memberTypeInfo;

                            //ACClassMethod MyClassMethod(string acMethodName)
                            if (acTypeInfo == null)
                            {
                                try
                                {
                                    if (path != Const.Value && path != Const.ValueT && path != "ValueT.ValueT")
                                    {
                                        PropertyInfo pi;
                                        object sourcePrev = source;
                                        source = TypeAnalyser.GetPropertyPathValue(sourcePrev, path, out pi);

                                        path = acUrl;
                                        rightControlMode = Global.ControlModes.Enabled;

                                        if (source != null)
                                        {
                                            if (source is IACObject)
                                            {
                                                IACObject sourceACObject = source as IACObject;
                                                ACClass acClassOfObject = sourceACObject.ACType as ACClass;
                                                if (acClassOfObject != null)
                                                    acTypeInfo = acClassOfObject.GetProperty(path);
                                            }
                                            else
                                            {
                                                PropertyInfo pi1 = null;
                                                object source1 = TypeAnalyser.GetPropertyPathValue(source, path, out pi1);

                                                //Type sourceType1 = source.GetType();
                                                //PropertyInfo pi1 = sourceType1.GetProperty(path);

                                                if (pi1 == null)
                                                    return false;


                                                using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                                                {
                                                    acTypeInfo = gip.core.datamodel.Database.GlobalDatabase.GetACType(pi1.PropertyType);
                                                }
                                            }
                                        }
                                        if (acTypeInfo == null)
                                            return false;
                                        return true;
                                    }
                                    // Sonst ACRef
                                    else
                                    {
                                        string dotNetPath = acUrl.Replace("\\", ".");
                                        //Type sourceType = null;
                                        object source1 = Value;
                                        PropertyInfo pi = null;
                                        // Fall wenn: ACComponent ein IACProperty hat, die ACRef<ACComponent> ist: IACProperty<ACRef<ACComponent>>
                                        // Dann darf als source-Object für Bindung nicht das ZielObjekt sein, sondern die Root-ACProperty, die die ACRef hält
                                        if (path == "ValueT.ValueT")
                                        {
                                            // Falls ACRef null ist
                                            if (source1 != null)
                                            {
                                                object valueTVal = TypeAnalyser.GetPropertyPathValue(source1, Const.ValueT, out pi);
                                                IACObject valueTAsIACObject = valueTVal as IACObject;
                                                if (valueTAsIACObject != null)
                                                {
                                                    IACType acRefTypeInfo = null;
                                                    object acRefSource = null;
                                                    string acRefPath = "";
                                                    if (valueTAsIACObject.ACUrlBinding(dotNetPath, ref acRefTypeInfo, ref acRefSource, ref acRefPath, ref rightControlMode))
                                                    {
                                                        if (acRefPath.IndexOf(Const.ValueT) >= 0)
                                                        {
                                                            dotNetPath = dotNetPath + ".ValueT";
                                                        }
                                                        acTypeInfo = acRefTypeInfo;
                                                    }
                                                    else
                                                    {
                                                        ACClass acClassOfObject = valueTAsIACObject.ACType as ACClass;
                                                        if (acClassOfObject != null)
                                                            acTypeInfo = acClassOfObject.GetProperty(dotNetPath);
                                                        source1 = valueTAsIACObject.ACUrlCommand(dotNetPath);
                                                        rightControlMode = Global.ControlModes.Enabled;
                                                    }
                                                }
                                                else if (valueTVal != null)
                                                {
                                                    source1 = TypeAnalyser.GetPropertyPathValue(valueTVal, dotNetPath, out pi);
                                                    if (pi == null)
                                                        return false;
                                                    if (pi != null)
                                                    {

                                                        using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                                                        {
                                                            acTypeInfo = gip.core.datamodel.Database.GlobalDatabase.GetACType(pi.PropertyType);
                                                        }
                                                    }
                                                    rightControlMode = Global.ControlModes.Enabled;
                                                }
                                                if (acTypeInfo == null)
                                                    return false;


                                                path = path + "." + dotNetPath;
                                                // Source-Objekt darf nicht verändert werden, da ACRef-Änderungen bemerkt werden müssen
                                                //source = Value;
                                            }
                                            // Wenn ACRef null ist, dann hole Beschreibung aus Datenbank
                                            else if (acp != null)
                                            {
                                                // Eine ACRef hat meist als Inhalt ine IACComponent und keinen konkreten Typ damit Proxy-Objekte drin sein können
                                                // => Der Pfad kann nicht verifiziert werden und ist evtl. auch dynamisch
                                                // daher muss der Pfad so akzeptiert werden
                                                // TODO: PRopertyInfo-Attributklasse werweitern, dass der Assembly-Typ drin steht obwohl als INstanz im ACRef ein ACComponent deklariert ist
                                                // Dann kann man ermitteln welche Typ die PRoperty ist
                                                if (typeof(IACObject).IsAssignableFrom(acp.ObjectType))
                                                {
                                                    path = path + "." + dotNetPath + ".ValueT";
                                                }
                                                else
                                                    return false;
                                            }
                                        }
                                        else
                                        {
                                            source1 = TypeAnalyser.GetPropertyPathValue(source1, path, out pi);

                                            if (pi == null)
                                                return false;

                                            rightControlMode = Global.ControlModes.Enabled;
                                            path = dotNetPath;
                                            source = Value;


                                            using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                                            {
                                                acTypeInfo = gip.core.datamodel.Database.GlobalDatabase.GetACType(pi.PropertyType);
                                            }
                                        }

                                        return true;
                                    }
                                }
                                catch (Exception e)
                                {
                                    string msg = e.Message;
                                    if (e.InnerException != null && e.InnerException.Message != null)
                                        msg += " Inner:" + e.InnerException.Message;

                                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                        datamodel.Database.Root.Messages.LogException("ACProperty<T>", "ACUrlBinding", msg);
                                }

                                return false;
                            }
                            switch (acTypeInfo.ACKind)
                            {
                                case Global.ACKinds.MSMethod:
                                case Global.ACKinds.MSMethodClient:
                                case Global.ACKinds.MSMethodPrePost:
                                    {
                                        PropertyInfo pi = null;
                                        source = TypeAnalyser.GetPropertyPathValue(source, path, out pi);

                                        path = acUrlHelper.ACUrlPart;
                                        ACClassMethod cm = acTypeInfo as ACClassMethod;
                                        ACClass acClassRight = cm.Safe_ACClass; // Basisklasse in der die PRoperty definiert ist
                                        rightControlMode = acClassRight.RightManager.GetControlMode(cm);
                                        if ((ParentACComponent != null) && (ParentACComponent.ACType != null) && rightControlMode == Global.ControlModes.Hidden)
                                        { 
                                            acClassRight = ParentACComponent.ACType as ACClass; // Am weitest unten abgeleitetet Klasse
                                            rightControlMode = acClassRight.RightManager.GetControlMode(cm);
                                        }

                                        return true;
                                    }
                                default:
                                    path += acTypeInfo.GetACPath(false);
                                    break;
                            }
                        }
                        ACClassProperty cp = acTypeInfo as ACClassProperty;
                        ACClass acClassRight2 = cp.ACClass; // Basisklasse in der die PRoperty definiert ist
                        rightControlMode = acClassRight2.RightManager.GetControlMode(cp);
                        if ((ParentACComponent != null) && (ParentACComponent.ACType != null) && rightControlMode == Global.ControlModes.Hidden)
                        {
                            acClassRight2 = ParentACComponent.ACType as ACClass; // Am weitest unten abgeleitetet Klasse
                            rightControlMode = acClassRight2.RightManager.GetControlMode(cp);
                        }
                        if (rightControlMode == Global.ControlModes.Enabled)
                        {
                            if (!cp.IsNullable)
                                rightControlMode = Global.ControlModes.EnabledRequired;
                        }

                        return true;
                    }
                case ACUrlHelper.UrlKeys.InvokeMethod:
                    {
                        int pos = acUrlHelper.ACUrlPart.IndexOf('(');
                        string methodName;
                        if (pos == -1)
                        {
                            methodName = acUrlHelper.ACUrlPart;
                        }
                        else
                        {
                            methodName = acUrlHelper.ACUrlPart.Substring(0, pos);
                        }
                        // acTypeInfo = this.ACType.GetACType(methodName);
                        ACClassMethod cm = (this.Value as IACObject).ACType.GetMember(methodName) as ACClassMethod;
                        ACClass acClassRight = cm.Safe_ACClass; // Basisklasse in der die PRoperty definiert ist
                        rightControlMode = acClassRight.RightManager.GetControlMode(cm);
                        if ((ParentACComponent != null) && (ParentACComponent.ACType != null) && rightControlMode == Global.ControlModes.Hidden)
                        {
                            acClassRight = ParentACComponent.ACType as ACClass; // Am weitest unten abgeleitetet Klasse
                            rightControlMode = acClassRight.RightManager.GetControlMode(cm);
                        }
                        acTypeInfo = cm;
                        source = this.Value;
                        path = "!" + methodName;
                    }
                    return true;
                case ACUrlHelper.UrlKeys.Parent:
                    return ParentACComponent.ACUrlBinding(acUrlHelper.NextACUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
                default:
                    return false; // TODO: Fehlerbehandlung
            }
        }

        public string GetACUrlComponent(IACObject rootACObject = null)
        {
            return ParentACComponent.GetACUrl(rootACObject) + "\\" + ACIdentifier;
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return ParentACComponent;
            }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        [ACMethodInfo("", "", 9999)]
        public string GetACUrl(IACObject rootACObject = null)
        {
            return GetACUrlComponent(rootACObject);
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get
            {
                return _ACTypeInfo.ACCaption;
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
                return null;
            }
        }
#endregion

#region IACMember Member


        protected virtual void OnCustomTypeChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Const.ValueT)
                OnMemberChanged();
        }

        /// <summary>
        /// Must be called inside the class that implements IACMember every time when the the encapsulated value-Property has changed.
        /// If the implementation implements INotifyPropertyChanged also then OnPropertyChanged() must be called inside the implementation of OnMemberChanged().
        /// </summary>
        /// <param name="e">
        /// The <see cref="T:System.EventArgs"/> instance containing the event data. Is not null if the change of the encapsulated value was detected by a callback of the PropertyChangedEvent or CollectionChanged-Event. Then the EventArgs will be passed.
        /// </param>
        public virtual void OnMemberChanged(EventArgs e = null)
        {
            if (_LiveLog != null)
                _LiveLog.AddValue(this.Value);
            OnPropertyChanged(Const.ValueT);
            OnPropertyChanged(Const.Value);
        }

#endregion

#region IACPropertyBase Member


        protected PropertyLogListInfo _LiveLog;
        /// <summary>
        /// The first time you access LiveLog, a property logging mechanism is automatically activated. Every time the property value changes, the new value is written to a ring buffer. You access the values ​​of this ring buffer using the LiveLogList property  . The standard capacity of the ring buffer is 500 values. However, you can change this value in the iPlus development environment in the "Size of the log buffer" field.
        /// </summary>
        /// <value>The live log.</value>
        [ACPropertyInfo(9999)]
        public PropertyLogListInfo LiveLog
        {
            get
            {
                if (_LiveLog != null)
                    return _LiveLog;
                if (this.ValueT != null)
                {
                    if (!(this.ValueT is IConvertible))
                        return null;
                }
                else
                {
                    Type typeT = typeof(T);
                    if (typeT.GetInterface("IConvertible") == null)
                        return null;
                }
                ACClassProperty acClassProperty = ACType as ACClassProperty;
                _LiveLog = new PropertyLogListInfo(acClassProperty.LogRefreshRate, logBufferSize:acClassProperty.LogBufferSize);
                _LiveLog.ACCaption = this.ACCaption;
                return _LiveLog;
            }
        }


        /// <summary>.NET-Type of the Value of this property.</summary>
        public Type PropertyType
        {
            get { return typeof(T); }
        }

        #endregion

    }
}
