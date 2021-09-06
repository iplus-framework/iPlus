using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Base-Class that represents network capable properties which values (IACContainer&amp;lt;T&amp;gt;.ValueT) will be broadcasted over the network.
    /// ACPropertyNet-Instances are primary for ProxyACComponents (clinet side)
    /// </summary>
    /// <typeparam name="T">Type of the ValueT-Property which is serializable and registered in the KnownTypes-Property of the DataContract-Serializer.</typeparam>
    /// <seealso cref="gip.core.autocomponent.ACProperty{T}" />
    /// <seealso cref="gip.core.datamodel.IACContainerTNet{T}" />
    public class ACPropertyNet<T> : ACProperty<T>, IACContainerTNet<T>
    {
        #region c'tors
        public ACPropertyNet(IACComponent acComponent, ACClassProperty acClassProperty)
            : this(new ACPropertyValueHolder<T>(), acComponent, acClassProperty, true)
        {
        }

        public ACPropertyNet(ACPropertyValueHolder<T> PropertyValue, IACComponent acComponent, ACClassProperty acClassProperty)
            : this(PropertyValue, acComponent, acClassProperty, false)
        {
        }

        public ACPropertyNet(ACPropertyValueHolder<T> PropertyValue, IACComponent acComponent, ACClassProperty acClassProperty, bool IsProxy)
            : base(acComponent, acClassProperty, IsProxy)
        {
            _PropertyValue = PropertyValue;
            if (_PropertyValue != null)
            {
                if (_PropertyValue.Owner == null)
                    _PropertyValue.Owner = this;
            }
            this.ForceBroadcast = acClassProperty.ForceBroadcast;
        }

        public override void ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            base.ACInit(startChildMode);
        }

        #endregion

        #region Properties

        #region Local

        public override void OnPropertyAccessorChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyAccessor == null)
                return;
            if ((e.PropertyName == this.ACType.ACIdentifier) && (ACRef.ValueT != null))
                this.ValueT = (T)PropertyAccessor.Get(ACRef.ValueT);
        }
        #endregion


        /// <summary>
        /// Setter-Delegat-Methode von Assembly-ACObjekten (Callback)
        /// </summary>
        protected ACPropertySetMethod _SetMethod = null;
        public ACPropertySetMethod SetMethod
        {
            get
            {
                return _SetMethod;
            }

            set
            {
                _SetMethod = value;
                // Referenz zu Set-Methode im Objekt hat zur Folge, dass es sich um ein Realse Objekt handelt => _IsProxy = false
                if ((_SetMethod != null) && (IsProxy))
                    _IsProxy = false;
            }
        }

        #region Distributed

        /// <summary>
        /// Aktuell gültiger Wert, der bei Server- und Clientobjekt synchron ist
        /// </summary>
        protected ACPropertyValueHolder<T> _PropertyValue = null;
        internal ACPropertyValueHolder<T> PropertyValue
        {
            get
            {
                return _PropertyValue;
            }
        }


        /// <summary>
        /// The last value changed.This value is passed as a request to the data source. 
        /// If a value change has occurred, the current valid value is set.
        /// If the value change has been rejected or changed, the feedback (possibly with error description) is written to this property.
        /// </summary>
        protected ACPropertyValueEvent<T> _PropertyValueRequest = null;
        internal ACPropertyValueEvent<T> PropertyValueRequest
        {
            get
            {
                return _PropertyValueRequest;
            }
        }


        /// <summary>
        /// The last value changed.This value is passed as a request to the data source. 
        /// If a value change has occurred, the current valid value is set.
        /// If the value change has been rejected or changed, the feedback (possibly with error description) is written to this property.
        /// </summary>
        public IACPropertyNetValueEvent CurrentChangeValueRequest
        {
            get
            {
                return _PropertyValueRequest;
            }
        }

        #endregion

        #region Public
        /// <summary>Gets or sets the encapsulated value of the generic type T</summary>
        /// <value>The Value-Property as generic type</value>
        public override T ValueT
        {
            get
            {
                if (_PropertyValue == null)
                    return default(T);
                // Falls Anfrage noch aussteht und kein Wert vom Server zurückgekommen ist,
                // dann ist der aktuelle Wert gleich dem beauftragten Wert, das optimistisch angenommen wird,
                // dass angeforderte Wertänderung angenommen wird

                using (ACMonitor.Lock(this._20015_LockValue))
                {
                    if (_PropertyValueRequest != null)
                    {
                        if (_PropertyValueRequest.IsRequestedValueStillValid)
                            return _PropertyValueRequest.Value;
                    }
                }
                return _PropertyValue.Value;
            }

            set
            {
                ChangeValueRequest(value, ForceBroadcast);
            }
        }

        /// <summary>The "Unrequested" value is the last valid value sent from the real object to the proxy object. If a value change of the ValueT property has occurred, the "Unrequested" value remains unchanged until the value change in real object has occurred and the new accepted value has been returned to the proxy object over the network.</summary>
        /// <value>The last valid value on server-side (real object)</value>
        public T ValueTUnrequested
        {
            get
            {
                if (_PropertyValue == null)
                    return default(T);
                return _PropertyValue.Value;
            }
        }

        /// <summary>A change request is started whenever the ValueT-Property was changed. The change request will be sent to the server (real object) and the reponse send back. If the server refused the chsnge request the message-text can be read here.</summary>
        /// <value>Messagetext if change request was refused on server-side.</value>
        public string Message
        {
            get
            {

                using (ACMonitor.Lock(this._20015_LockValue))
                {
                    if (_PropertyValueRequest == null)
                        return "";
                    return _PropertyValueRequest.Message;
                }
            }
        }

        /// <summary>
        /// Must be called inside the class that implements IACMember every time when the the encapsulated value-Property has changed.
        /// If the implementation implements INotifyPropertyChanged also then OnPropertyChanged() must be called inside the implementation of OnMemberChanged().
        /// </summary>
        /// <param name="e">
        /// The <see cref="T:System.EventArgs"/> instance containing the event data. Is not null if the change of the encapsulated value was detected by a callback of the PropertyChangedEvent or CollectionChanged-Event. Then the EventArgs will be passed.
        /// </param>
        public override void OnMemberChanged(EventArgs e = null)
        {
            ACPropertyChangedEventArgs propChangedArgs = e as ACPropertyChangedEventArgs;
            if (propChangedArgs != null)
                ChangeValueRequest(_PropertyValue.Value, propChangedArgs.ValueEvent.ForceBroadcast, propChangedArgs.ValueEvent.InvokerInfo);
            else
                ChangeValueRequest(_PropertyValue.Value, true);
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
        public override object ACUrlCommand(string acUrl, params Object[] acParameter)
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

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public override bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return true;
        }

#endregion

#endregion

#region Internal Methods

        /// <summary>
        /// Methode löst einen Value-Change-Request über das Netzwerk auf dem entsprechenden Realen-Objekt aus
        /// 
        /// ChangeValueRequest wird aufgerufen durch:
        /// - Direktes setzen von ValueT (oder Oberflächenbindung)
        /// - Indirekt durch PropertyAccessorChanged-Event -> Setzen von ValueT
        /// - Durch OnPropertychanged()-Aufruf von PropertyValueHolder weil ListChanged-/CollectionChanged-/PropertyChanged-Ereignis eines komplexen T-Objekts ausgelöst worden ist
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="forceSend"></param>
        protected virtual void ChangeValueRequest(T newValue, bool forceSend, object invokerInfo = null)
        {
            if (_PropertyValue == null)
                return;
            if (!PropertyInfo.IsBroadcast)
                return;

            // Falls Wert nicht geändert
            if (!forceSend && (_PropertyValue.CompareTo(newValue, this) == 0))
                return;


            ACPropertyValueEvent<T> newRequest = new ACPropertyValueEvent<T>(EventTypes.Request,
                                                                               EventRaiser.Proxy,
                                                                               this.ACRef.ValueT, this.ACType, invokerInfo);
            newRequest.ChangeValue(this, newValue);

            // Setze aktuelle Anfrage

            using (ACMonitor.Lock(this._20015_LockValue))
            {
                _PropertyValueRequest = newRequest;
            }

            // Sende
            SendToReal(newRequest);
        }


        /// <summary>
        /// It's called when a new value has arrived from the real object on serverside to refresh the current value.
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        public void OnValueEventReceivedRemote(IACPropertyNetValueEvent eventArgs)
        {
            if (eventArgs == null)
                return;
            if (!ACRef.IsObjLoaded)
                return;
            ACComponent acComponent = ACRef.ValueT as ACComponent;
            if (acComponent.InitState == ACInitState.DisposingToPool || acComponent.InitState == ACInitState.DisposedToPool)
                return;
            (ACRef.ValueT as ACComponent).OnValueEventReceivedRemote(this, eventArgs);
            ACPropertyValueEvent<T> eventArgsT = eventArgs as ACPropertyValueEvent<T>;
            if (eventArgsT != null && eventArgsT.Value is IACContainerRef)
                (eventArgsT.Value as IACContainerRef).AttachTo(ACRef.ValueT);

            OnValueEventReceived(eventArgsT);
        }

        /// <summary>
        /// Methode wird aufgerufen sobald ein Änderungsrequest von Außen angekommen ist
        /// - Netzwerk/WCF-Dienst durch Abarbeitung der PropertyNetValueEvent-Queue
        /// - intern durch Fall-Unterscheidung in Methode ChangeValueRequest
        /// </summary>
        /// <param name="eventArgs"></param>
        internal virtual void OnValueEventReceived(ACPropertyValueEvent<T> eventArgs)
        {
            if (eventArgs == null)
                return;

            // Setze aktuellen PropertyValue
            SetSourceValue(eventArgs.Value);

            // Verwalte interne Anfrage
            ManagePropertyValueRequestOnReceival(eventArgs);
        }


        /// <summary>
        /// Method sends a PropertyValueEvent from this Client/Proxy-Object
        /// to the Real Object on Server-side
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        protected virtual void SendToReal(ACPropertyValueEvent<T> eventArgs)
        {
            if (this.ACRef == null)
                return;
            if (!ACRef.IsObjLoaded)
                return;
            ACComponent comp = this.ACRef.ValueT as ACComponent;
            if (comp != null)
                comp.SendPropertyValue(eventArgs);
        }

        /// <summary>
        /// Ändert den internen Source-Wert Thread-Safe
        /// </summary>
        /// <param name="newValue"></param>
        protected void SetSourceValue(T newValue)
        {
            if (_PropertyValue != null)
            {
                _PropertyValue.ChangeValue(this, newValue);
                OnPropertyChanged(Const.SourceValue);
            }
        }

        protected override void SetDefaultValue()
        {
            SetSourceValue(default(T));
        }


        /// <summary>
        /// Verwalte internen Change-Request bei Empfang der Antwort
        /// </summary>
        /// <param name="eventArgs"></param>
        protected void ManagePropertyValueRequestOnReceival(ACPropertyValueEvent<T> eventArgs)
        {
            if ((eventArgs.EventType == EventTypes.RefusedRequest)
                || (eventArgs.EventType == EventTypes.Response))
            {

                using (ACMonitor.Lock(this._20015_LockValue))
                {
                    // Es steht ein Änderungsrequest an
                    if (_PropertyValueRequest != null)
                    {
                        // Falls Antwort auf Change-Request empfangen
                        if (_PropertyValueRequest.RequestID == eventArgs.RequestID)
                        {
                            if (eventArgs.Message == null)
                                eventArgs.Message = "";
                            // Wenn Message leer, dann lösche Objekt
                            if (eventArgs.Message.Length <= 0)
                            {
                                bool PropertyChangedMessage = false;
                                if (_PropertyValueRequest.Message != eventArgs.Message)
                                    PropertyChangedMessage = true;
                                _PropertyValueRequest = null;
                                if (PropertyChangedMessage)
                                    OnPropertyChanged("Message");
                            }
                            // ansonsten weise Objekt zu
                            else
                            {
                                _PropertyValueRequest = eventArgs;
                                OnPropertyChanged("Message");
                            }
                        }
                        // Antwort auf Request eines anderen Proxys
                        else
                        {
                            // Kein Löschen des Requests
                        }
                    }
                    // Es steht kein Änderungsrequest an -> Antwort auf Request eines anderen Proxys
                    else
                    {
                    }
                }
            }
            // ValueChangedInSource: Normale interne Wertänderung im Source-Objekt
            else
            {
                // Es steht ein Änderungsrequest an
                // if (_PropertyValueRequest != null)
                //{
                // Kein Löschen des Requests
                //}
            }


            using (ACMonitor.Lock(this._20015_LockValue))
            {
                // Falls ein Änderungsrequest ansteht
                if (_PropertyValueRequest != null)
                {
                    // Markiere jedoch Request als ungültig, damit aktueller Wert wieder angezeigt wird
                    _PropertyValueRequest.InvalidateRequestedValue();
                }
            }

            // Löse PropertyChanged-Event aus
            if (_LiveLog != null)
                _LiveLog.AddValue(this.Value);
            OnPropertyChanged(Const.ValueT);
            OnPropertyChanged(Const.Value);
        }


        /// <summary>
        /// Returns the current value inside a wrapper of type IACPropertyNetValueEvent
        /// </summary>
        /// <returns>
        /// IACPropertyNetValueEvent.
        /// </returns>
        public IACPropertyNetValueEvent GetValueAsEvent()
        {
            if (PropertyInfo == null)
                return null;
            EventRaiser raiser = EventRaiser.Source;
            if (PropertyInfo.IsProxyProperty)
                raiser = EventRaiser.Target;

            ACPropertyValueEvent<T> response = new ACPropertyValueEvent<T>(EventTypes.FetchValue,
                                                                            raiser, ACRef.ValueT, this.ACType);
            response.ChangeValue(this, ValueTUnrequested);
            return response;
        }

#endregion

#region IACPropertyNetBase Member


        /// <summary>
        /// PAClassPhysicalBaseProperty values ​​can also be stored in long-term archives. To do this, the update rate must be set in the iPlus development environment. Set a threshold value in the "Log Filter" field so that not every marginal change in value is persisted. This is particularly useful for double and float values. Use this method to query these archived values.
        /// </summary>
        /// <param name="from">Filter time from</param>
        /// <param name="to">Filter time to</param>
        /// <returns>PropertyLogListInfo.</returns>
        public virtual PropertyLogListInfo GetArchiveLog(DateTime from, DateTime to)
        {
            if (IsProxy)
            {
                if (ACRef.ValueT is ACComponentProxy)
                {
                    return (ACRef.ValueT as ACComponentProxy).GetArchiveLog(this.ACIdentifier, from, to) as PropertyLogListInfo;
                }
            }
            return null;
        }
        #endregion


        /// <summary>
        /// Every time when the Value-Property is set a broadcast to all subscribed components will be performed - whether the value has changed or not.
        /// If this property is false a broadcast happens only if the value has changed.
        /// </summary>
        /// <value><c>true</c> if broadcast should be forced; otherwise, <c>false</c>.</value>
        public bool ForceBroadcast
        {
            get;
            set;
        }
    }
}
