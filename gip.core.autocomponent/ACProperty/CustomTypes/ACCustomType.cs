using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACCustomTypeBase'}de{'ACCustomTypeBase'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public abstract class ACCustomTypeBase : IACContainer, INotifyPropertyChanged
    {
        #region Events and Delegates
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public ACCustomTypeBase()
        {
        }

        public ACCustomTypeBase(IACType acValueType)
        {
            _ACTypeInfo = acValueType;
        }

        private IACType _ACTypeInfo;
        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        [ACPropertyInfo(9999)]
        public IACType ACType
        {
            get
            {
                return _ACTypeInfo;
            }
        }

        public virtual void CloneCustomProperties(ACCustomTypeBase Target)
        {
        }

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        [ACPropertyInfo(9999)]
        public abstract object Value { get; set; }

        /// <summary>Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!</summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        public abstract ACClass ValueTypeACClass { get; }

        public abstract Type TypeOfValueT { get; }

        protected void OnPropertyChanged(string propertyName, ACPropertyChangedEventArgs serverChangedArgs = null)
        {
            if (PropertyChanged != null)
            {
                if (serverChangedArgs != null)
                    PropertyChanged(this, serverChangedArgs);
                else
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public abstract void ChangeValueServer(object newValue, bool forceSend, object invokerInfo = null);

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public abstract string ACIdentifier { get; }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public abstract string ACCaption { get; }
    }

    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACCustomType'}de{'ACCustomType'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public abstract class ACCustomType<T> : ACCustomTypeBase, IACContainerT<T>, IComparable<T> where T : IComparable<T>
    {
        #region c'tors
        public ACCustomType()
        {
        }

        public ACCustomType(IACType ACValueType) : base(ACValueType)
        {
        }

        #endregion

        #region Properties

        /// <summary>Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!</summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        public override ACClass ValueTypeACClass
        {
            get
            {
                IACObject iObject = ValueT as IACObject;
                return iObject == null ? null : iObject.ACType as ACClass;
            }
        }

        public override Type TypeOfValueT 
        {
            get
            {
                if (ValueT != null)
                    return ValueT.GetType();
                else
                    return typeof(T);
            }
        }

        [DataMember]
        protected T _valueT;
        /// <summary>Gets or sets the encapsulated value of the generic type T</summary>
        /// <value>The Value-Property as generic type</value>
        [IgnoreDataMember]
        public virtual T ValueT
        {
            get
            {
                return _valueT;
            }

            set
            {
                _valueT = value;
                OnValueTChanged();
            }
        }

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        public override object Value
        {
            get
            {
                return (object)this.ValueT;
            }

            set
            {
                if (value != null)
                {
                    if (value is T)
                        this.ValueT = (T)value;
                    else if ((this.ValueT != null) && (this.ValueT is IConvertible) && (value is IConvertible))
                        this.ValueT = (T)Convert.ChangeType(value, typeof(T));
                }
                else
                    this.ValueT = default(T);
            }
        }

        public override void ChangeValueServer(object newValue, bool forceSend, object invokerInfo = null)
        {
            if (newValue != null)
            {
                if (newValue is T)
                    this._valueT = (T)newValue;
                else if ((this.ValueT != null) && (this.ValueT is IConvertible) && (newValue is IConvertible))
                    this._valueT = (T)Convert.ChangeType(newValue, typeof(T));
            }
            else
                this._valueT = default(T);

            ACPropertyValueEvent<T> valueEvent = new ACPropertyValueEvent<T>(EventTypes.ValueChangedInSource, EventRaiser.Source, null, null, invokerInfo);
            valueEvent.ForceBroadcast = forceSend;
            OnValueTChanged(valueEvent);
        }

        #endregion

        #region EventHandling Methods
        protected virtual void OnValueTChanged(IACPropertyNetValueEvent valueEvent = null)
        {
            ACPropertyChangedEventArgs eventArgs = null;
            if (valueEvent != null)
                eventArgs = new ACPropertyChangedEventArgs(Const.ValueT, valueEvent);
            OnPropertyChanged(Const.ValueT, eventArgs);
            if (valueEvent != null)
                eventArgs = new ACPropertyChangedEventArgs(Const.Value, valueEvent);
            OnPropertyChanged(Const.Value, eventArgs);
        }

        #endregion

        #region IComparable<T> Member

        public virtual int CompareTo(T other)
        {
            if ((_valueT == null) && (other != null))
                return -1;
            else if ((_valueT != null) && (other == null))
                return -1;
            return _valueT.CompareTo(other);
        }
        #endregion
    }
}
