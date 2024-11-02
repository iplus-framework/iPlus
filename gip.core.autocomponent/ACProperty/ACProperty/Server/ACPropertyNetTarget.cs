// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using System.Linq.Expressions;
using System.Reflection;

namespace gip.core.autocomponent
{
    /// <summary>
    /// A "Target-Property" is a network capable property, that doesn't contain or holds a original value. "Target-Properties" have bindings to a "Source-Property" where they get the original value from.
    /// </summary>
    /// <typeparam name="T">Type of the ValueT-Property which is serializable and registered in the KnownTypes-Property of the DataContract-Serializer.</typeparam>
    /// <seealso cref="gip.core.autocomponent.ACPropertyNetServerBase{T}" />
    /// <seealso cref="gip.core.autocomponent.IACPropertyNetTarget" />
    public class ACPropertyNetTarget<T> : ACPropertyNetServerBase<T>, IACPropertyNetTarget
    {
        #region c'tors
        public ACPropertyNetTarget(IACComponent acComponent, ACClassProperty acClassProperty)
            : base(new ACPropertyValueHolder<T>(), acComponent, acClassProperty, false, null)
        {
        }

        public ACPropertyNetTarget(IACComponent acComponent, ACClassProperty acClassProperty, ACPropertySetMethod method)
            : base(new ACPropertyValueHolder<T>(), acComponent, acClassProperty, false, method)
        {
        }

        public ACPropertyNetTarget(ACPropertyNetSource<T> PropertySource, IACComponent acComponent, ACClassProperty acClassProperty)
            : this(PropertySource, acComponent, acClassProperty, null)
        {
        }

        public ACPropertyNetTarget(ACPropertyNetSource<T> PropertySource, IACComponent ACObject, ACClassProperty acClassProperty, ACPropertySetMethod method)
            : base(PropertySource.PropertyValue, ACObject, acClassProperty, false, method)
        {
            _Source = PropertySource;
            if (_Source != null)
                _Source.Targets.Add(this);
        }

        /// <summary>
        /// Restores the stored value from the database into this persistable property
        /// </summary>
        /// <param name="IsInit">if set to <c>true</c> [is init].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ReStoreFromDB(bool IsInit)
        {
            // Target-Properties dürfen nicht während der Initialisierungsphase gesetzt werden,
            // da der Wert im Source-Property überschrieben werden würde.
            // Sie dürfen nur aktiv durch ein Kommando des Anweders ausgelöst werden
            // Beispiel: Konfigurationswerte in der SPS
            if (IsInit)
                return false;

            return base.ReStoreFromDB(IsInit);
        }

        //public override bool Persist()
        //{
        //    return base.Persist();
        //}

        public override void ACDeInit(bool deleteACClassTask = false)
        {
            base.ACDeInit(deleteACClassTask);
            if (ACRoot.SRoot.InitState != ACInitState.Destructed && ACRoot.SRoot.InitState != ACInitState.Destructing)
                UnbindSourceProperty();
        }
        #endregion

        #region Properties
        private ACPropertyNetSource<T> _Source = null;
        /// <summary>
        /// Reference to a "Source-Property" where this "Target-Property" is bound to.
        /// </summary>
        public IACPropertyNetSource Source
        {
            get
            {
                return _Source;
            }
        }

        protected override bool CanPersist
        {
            get
            {
                if (Source != null)
                    return false;
                return base.CanPersist;
            }
        }

        protected override bool CanRestoreRuntimeValue
        {
            get
            {
                if (Source != null)
                    return false;
                return base.CanRestoreRuntimeValue;
            }
        }

        /// <summary>
        /// CurrentEventArgs ist immer nur temporär gesetzt, wenn PropertyAccessor verwendet wird (Vor Aufruf gesetzt, nach Aufruf gelöscht)
        /// </summary>
        protected ACPropertyValueEvent<T> _Current_T_EventArgsForPropertyAccessor = null;
        #endregion

        #region Internal Methods
        protected override void OnCustomTypeChanged(object sender, PropertyChangedEventArgs e)
        {
            // OnCustomTypeChanged wird eigentlich nicht benötigt, da Changed-event in ACPropertyValueHolder ausgelöst wird
            // DAMIR TODO: Test ob alles noch funktioniert
            // Ansonsten wird unnötig zweimal ValueUpdatedOnReceival hintereinander ausgelöst

            //if (e.PropertyName == Const.ValueT)
            //OnPropertyChanged();
            // base.OnCustomTypeChanged(sender, e);
        }


        /// <summary>
        /// Binds this target property to the passed source-Property.
        /// Method succeeds only if both types are equal (Source and Taregt)
        /// </summary>
        /// <param name="acPropertySource">Source-Property to bind</param>
        /// <param name="bindingMode"></param>
        /// <returns>PropBindingBindingResult</returns>
        public PropBindingBindingResult BindPropertyToSource(IACPropertyNetSource acPropertySource, PropBindingMode bindingMode = PropBindingMode.BindAndBroadcast)
        {
            if ((bindingMode == PropBindingMode.BindAndBroadcast || bindingMode == PropBindingMode.BindOnly)
                && acPropertySource != null)
            {
                if (!(acPropertySource is ACPropertyNetSource<T>))
                    return PropBindingBindingResult.NotCompatibleTypes;
                _PropertyValue = null;
                _Source = acPropertySource as ACPropertyNetSource<T>;
                if (_Source != null)
                    _Source.Targets.Add(this);
                _PropertyValue = _Source.PropertyValue;
            }
            if (bindingMode == PropBindingMode.BindAndBroadcast || bindingMode == PropBindingMode.BroadcastOnly)
            {
                OnPropertyChanged(Const.SourceValue);
                ACPropertyValueEvent<T> emulatedEvent = null; 
                if (acPropertySource != null && acPropertySource.WasInitializedFromInvoker)
                {
                    emulatedEvent = new ACPropertyValueEvent<T>(EventTypes.ValueChangedInSource,
                                                                EventRaiser.Source,
                                                                this.ACRef.ValueT, this.ACType, acPropertySource);
                }
                BroadcastValueChangedInSource(EventRaiser.Source, emulatedEvent);
            }
            return PropBindingBindingResult.Succeeded;
        }


        /// <summary>
        /// Binds this target property to the passed source-Property.
        /// If types are not compatible it creates a new target property of the ACPropertyNetTargetConverter-Class that is replaced with this. In this case newTarget is not null.
        /// If a creation is not possible because the Datatype of the source-property must be replaced (e.g. the Type is another BitAccess-Wrapper) the IPlus-Service must me restarted afterwards.
        /// </summary>
        /// <param name="acPropertySource">Source-Property to bind</param>
        /// <param name="newTarget"></param>
        /// <param name="message"></param>
        /// <param name="bindInDBIfConverterNeeded">If true a binding in the database will be defined. At the next IPlus-Restart the Target-Property will be created as a ACPropertyNetTargetConverter automatically</param>
        /// <param name="bindingMode"></param>
        /// <returns>PropBindingBindingResult</returns>
        public PropBindingBindingResult BindPropertyToSource(IACPropertyNetSource acPropertySource, 
            out IACPropertyNetTarget newTarget, out string message, 
            bool bindInDBIfConverterNeeded = true, PropBindingMode bindingMode = PropBindingMode.BindAndBroadcast)
        {
            message = null;
            newTarget = null;
            PropBindingBindingResult result = BindPropertyToSource(acPropertySource, bindingMode);
            if (result.HasFlag(PropBindingBindingResult.Succeeded))
                return result;
            else if (result.HasFlag(PropBindingBindingResult.NotCompatibleTypes))
            {
                result = ACPropertyNetServerBase<T>.RepairIncompatibleTypes(this, acPropertySource, out newTarget, out message, bindInDBIfConverterNeeded);
                if (result.HasFlag(PropBindingBindingResult.TargetPropReplaced))
                {
                    result |= newTarget.BindPropertyToSource(acPropertySource, bindingMode);
                }
            }
            return result;
        }


        /// <summary>
        /// Unbinds the source property.
        /// </summary>
        public void UnbindSourceProperty()
        {
            _PropertyValue = new ACPropertyValueHolder<T>();
            if (_Source != null)
            {
                _PropertyValue.ChangeValue(this, _Source.ValueTUnrequested);

                using (ACMonitor.Lock(_Source._20035_LockTargets))
                {
                    _Source.Targets.Remove(this);
                }
            }
            _Source = null;
        }

        private void BroadcastValueChangedInSource(EventRaiser raiser = EventRaiser.Target, ACPropertyValueEvent<T> eventsArgs = null)
        {
            if (this.ACRef == null)
                return;

            object invokerInfo = null;
            if (eventsArgs != null && eventsArgs.InvokerInfo != null)
                invokerInfo = eventsArgs.InvokerInfo;
            // Broadcast (mit eigener Request-ID, beachte EventType)
            ACPropertyValueEvent<T> broadcast = new ACPropertyValueEvent<T>(EventTypes.ValueChangedInSource,
                                                                            raiser,
                                                                            this.ACRef.ValueT, this.ACType, invokerInfo);

            broadcast.ChangeValue(this, ValueTUnrequested);
            broadcast.Handled = true;
            BroadcastToProxies(broadcast);


            using (ACMonitor.Lock(this._20015_LockValue))
            {
                // Lösche interne Anfrage
                _PropertyValueRequest = null;
            }

            // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
            OnValueUpdatedOnReceival(broadcast, ACPropertyChangedPhase.BeforeBroadcast);

            // Löse PropertyChanged-Event aus
            if (_LiveLog != null)
                _LiveLog.AddValue(this.Value);
            OnPropertyChanged(Const.ValueT);
            OnPropertyChanged(Const.Value);
            OnValueUpdatedOnReceival(broadcast, ACPropertyChangedPhase.AfterBroadcast);
        }

        /// <summary>
        /// Internes Auslösen der Wertänderung durch:
        /// - Direktes setzen von ValueT (oder Oberflächenbindung)
        /// - Indirekt durch PropertyAccessorChanged-Event -&gt; Setzen von ValueT
        /// - Durch OnPropertychanged()-Aufruf von PropertyValueHolder weil ListChanged-/CollectionChanged-/PropertyChanged-Ereignis eines komplexen T-Objekts ausgelöst worden ist
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="forceSend"></param>
        /// <param name="invokerInfo"></param>
        protected override void ChangeValueRequest(T newValue, bool forceSend, object invokerInfo = null)
        {
            //base.ChangeValueRequest(newValue);
            if (_PropertyValue == null || this.ACRef == null)
                return;
            if (!PropertyInfo.IsBroadcast)
                return;
            ACPropertyValueEvent<T> newRequest;
            // Erster Aufruf der Methode bzw. 
            // NICHT indirekt aufgerufen über OnValueEventReceived->PropertyAccessorChanged
            if (_Current_T_EventArgsForPropertyAccessor == null)
            {
                newRequest = new ACPropertyValueEvent<T>(EventTypes.Request,
                                                        EventRaiser.Target,
                                                        this.ACRef.ValueT, this.ACType, invokerInfo);
                newRequest.ChangeValue(this, newValue);
                newRequest.ForceBroadcast = forceSend;

                // Callback Set-Method von ACObject
                if (SetMethod != null)
                    SetMethod(newRequest);
                else if (NetSetMethodOfScript != null)
                    NetSetMethodOfScript(newRequest, this);

                if (newRequest.Handled)
                    return;

                // Falls Wert nicht geändert
                if (!forceSend && (_PropertyValue.CompareTo(newValue, this) == 0))
                    return;


                using (ACMonitor.Lock(this._20015_LockValue))
                {
                    // Setze aktuelle Anfrage
                    _PropertyValueRequest = newRequest;
                }

                // Falls Wert mit Source verbunden, delegiere Auftrag an Source-Objekt weiter
                if (_Source != null)
                {
                    _Source.OnValueEventReceived(newRequest);
                    if (_PropertyValueRequest != null)
                    {
                        try
                        {
                            Database.Root.Messages.LogDebug(this.GetACUrlComponent(), "ChangeValueRequest(0)",
                                String.Format("PropertyValueRequest {0} not cleared. _PropertyValue is still {1}.", _PropertyValueRequest.Value, _PropertyValue.Value));
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("ACPropertyNetTarget<T>", "ChangeValueRequest", msg);
                        }
                    }
                }
                else
                    this.OnValueEventReceived(newRequest);
            }

            // Zweiter Aufruf dieser Methode
            // => Indirekt aufgerufen über OnValueEventReceived->PropertyAccessorChanged
            else //(_Current_T_EventArgsForPropertyAccessor != null)
            {
                // Falls Aufruf von Proxy-Objekt
                if (_Current_T_EventArgsForPropertyAccessor.Sender == EventRaiser.Proxy)
                {
                    // Falls Wert mit Source verbunden, delegiere Proxy-Auftrag an Source-Objekt weiter
                    if (_Source != null)
                    {
                        _Current_T_EventArgsForPropertyAccessor.ChangeValue(this, newValue);
                        _Source.OnValueEventReceived(_Current_T_EventArgsForPropertyAccessor);
                        return;
                    }
                    // Angelangt am Endpunkt, da Target nicht verbunden mit Source
                    else
                    {
                        // Änderungen, die nicht von der Source-Quelle kommen können persistiert werden
                        Persist();

                        newRequest = new ACPropertyValueEvent<T>(_Current_T_EventArgsForPropertyAccessor, EventTypes.Response,
                                                                                        EventRaiser.Target, ACRef.ValueT, this.ACType);
                        newRequest.ChangeValue(this, ValueTUnrequested);
                        BroadcastToProxies(newRequest);

                        // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
                        OnValueUpdatedOnReceival(newRequest, ACPropertyChangedPhase.BeforeBroadcast);

                        // Löse PropertyChanged-Event aus
                        if (_LiveLog != null)
                            _LiveLog.AddValue(this.Value);
                        OnPropertyChanged(Const.ValueT);
                        OnPropertyChanged(Const.Value);
                        OnValueUpdatedOnReceival(newRequest, ACPropertyChangedPhase.AfterBroadcast);
                        return;
                    }
                }
                // Falls Aufruf intern, weil nicht mit Source verbunden
                else if (_Current_T_EventArgsForPropertyAccessor.Sender == EventRaiser.Target)
                {
                    return;
                }
                // Falls Aufruf von Source-Objekt
                else //if (eventArgs.Sender == EventRaiser.Source)
                {
                    // SetSourceValue muss nicht aufgerufen werden, 
                    // da Wert sich schon im gebundenen Source-Objekt verändert hat
                    // Gebe nur Änderung bekannt
                    OnPropertyChanged(Const.SourceValue);

                    // Verwalte interne Anfrage
                    ManagePropertyValueRequestOnReceival(_Current_T_EventArgsForPropertyAccessor);

                    // Umverpackung
                    newRequest = new ACPropertyValueEvent<T>(_Current_T_EventArgsForPropertyAccessor, _Current_T_EventArgsForPropertyAccessor.EventType,
                                                                                    EventRaiser.Source,
                                                                                    ACRef.ValueT, this.ACType);
                    newRequest.ChangeValue(this, newValue);
                    newRequest.Message = _Current_T_EventArgsForPropertyAccessor.Message;

                    // Weiterleitung an alles Proxies
                    BroadcastToProxies(newRequest);
                    return;
                }
            }
        }


        /// <summary>
        /// Called when a value has changed in the "Source-Property"
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        public void OnValueEventReceivedFromSource(IACPropertyNetValueEvent eventArgs)
        {
            OnValueEventReceived((ACPropertyValueEvent<T>)eventArgs);
        }


        /// <summary>
        /// Method is called when a change-request has arrived from outside.
        /// - Either from the Network/WCF-Service through iteration of the PropertyNetValueEvent-Queue
        /// - or internaly in method ChangeValueRequest
        /// </summary>
        /// <param name="eventArgs"></param>
        internal override void OnValueEventReceived(ACPropertyValueEvent<T> eventArgs)
        {
            //base.OnValueEventRecieved(eventArgs);
            if (eventArgs == null || ACRef == null)
                return;

            // Falls Aufruf von Proxy-Objekt
            if (eventArgs.Sender == EventRaiser.Proxy)
            {
                // Callback Set-Method von ACObject
                if (SetMethod != null)
                    SetMethod(eventArgs);
                else if (NetSetMethodOfScript != null)
                    NetSetMethodOfScript(eventArgs, this);

                // Falls Setter-Methode in Objekt existiert
                if (this.PropertyAccessor != null)
                {
                    // Falls durch SetMethod keine Abweisung stattgefunden hat, dann Setter-Methode aufrufen
                    if (!eventArgs.Handled)
                    {
                        _Current_T_EventArgsForPropertyAccessor = eventArgs;
                        if (ACRef.ValueT != null)
                            this.PropertyAccessor.Set(ACRef.ValueT, eventArgs.Value);
                        // Zurueckspringen, da durch ProprtyChanged-Aufruf in Setter-Methode ein neuer Broadcast-Request generiert wurde
                        _Current_T_EventArgsForPropertyAccessor = null;
                        return;
                    }
                }

                // Falls Anfrage nicht weitergeleitet werden soll
                if (eventArgs.Handled)
                {
                    // Falls Wert mit Source verbunden
                    if (_Source != null)
                    {
                        // Ändere Wert in Source-Objekt nicht ab, 
                        // Broadcast mit Proxy-RequestID
                    }
                    else
                    {
                        // Angelangt am Endpoint
                        // Ändere lokalen Wert nicht ab, 
                        // Broadcast mit Proxy-RequestID
                    }
                    ACPropertyValueEvent<T> response = new ACPropertyValueEvent<T>(eventArgs, EventTypes.RefusedRequest,
                                                                                    EventRaiser.Target, ACRef.ValueT, this.ACType);
                    response.ChangeValue(this, ValueTUnrequested);
                    BroadcastToProxies(response);
                    return;
                }
                else
                {
                    // Falls Wert mit Source verbunden, delegiere Proxy-Auftrag an Source-Objekt weiter
                    if (_Source != null)
                    {
                        _Source.OnValueEventReceived(eventArgs);
                        return;
                    }
                    else
                    {
                        // Angelangt am Endpoint
                        eventArgs.Handled = true;

                        // Ändere Wert ab 
                        SetSourceValue(eventArgs.Value);
                        // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
                        OnValueUpdatedOnReceival(eventArgs, ACPropertyChangedPhase.BeforeBroadcast);

                        // Broadcast mit Proxy-RequestID
                        ACPropertyValueEvent<T> response = new ACPropertyValueEvent<T>(eventArgs, EventTypes.Response,
                                                                                        EventRaiser.Target, ACRef.ValueT, this.ACType);
                        response.ChangeValue(this, ValueTUnrequested);

                        // Änderungen, die nicht von der Source-Quelle kommen können persistiert werden
                        Persist();

                        BroadcastToProxies(response);

                        // Löse PropertyChanged-Event aus
                        if (_LiveLog != null)
                            _LiveLog.AddValue(this.Value);
                        OnPropertyChanged(Const.ValueT);
                        OnPropertyChanged(Const.Value);

                        OnValueUpdatedOnReceival(eventArgs, ACPropertyChangedPhase.AfterBroadcast);
                        return;
                    }
                }
            }
            // Falls Aufruf intern, weil nicht mit Source verbunden
            else if (eventArgs.Sender == EventRaiser.Target)
            {
                // Angelangt am Endpoint
                eventArgs.Handled = true;

                // Ändere Wert ab 
                SetSourceValue(eventArgs.Value);

                // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
                // OnValueUpdatedOnReceival();

                // Löse PropertyChanged-Event aus
                //if (_LiveLog != null)
                //_LiveLog.AddValue(this.Value);
                //OnPropertyChanged(Const.ValueT);
                //OnPropertyChanged(Const.Value);

                // Änderungen, die nicht von der Source-Quelle kommen können persistiert werden
                Persist();

                // Broadcast (mit eigener Request-ID, beachte EventType)
                BroadcastValueChangedInSource(EventRaiser.Target, eventArgs);

                return;
            }
            // Falls Aufruf von Source-Objekt
            else //if (eventArgs.Sender == EventRaiser.Source)
            {
                // Callback Set-Method von ACObject
                if (SetMethod != null)
                    SetMethod(eventArgs);
                else if (NetSetMethodOfScript != null)
                    NetSetMethodOfScript(eventArgs, this);

                // Falls Setter-Methode in Objekt existiert
                if (this.PropertyAccessor != null)
                {
                    // Falls durch SetMethod keine Abweisung stattgefunden hat, dann Setter-Methode aufrufen
                    if (!eventArgs.Handled)
                    {
                        _Current_T_EventArgsForPropertyAccessor = eventArgs;
                        if (ACRef.ValueT != null)
                            this.PropertyAccessor.Set(ACRef.ValueT, eventArgs.Value);
                        // Zurueckspringen, da durch ProprtyChanged-Aufruf in Setter-Methode ein neuer Broadcast-Request generiert wurde
                        _Current_T_EventArgsForPropertyAccessor = null;
                        return;
                    }
                }

                // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
                OnValueUpdatedOnReceival(eventArgs, ACPropertyChangedPhase.BeforeBroadcast);

                // SetSourceValue muss nicht aufgerufen werden, 
                // da Wert sich schon im gebundenen Source-Objekt verändert hat
                // Gebe nur Änderung bekannt
                OnPropertyChanged(Const.SourceValue);

                // Verwalte interne Anfrage
                ManagePropertyValueRequestOnReceival(eventArgs);

                // Umverpackung
                ACPropertyValueEvent<T> delegateEvent = new ACPropertyValueEvent<T>(eventArgs, eventArgs.EventType,
                                                                                EventRaiser.Source,
                                                                                ACRef.ValueT, this.ACType);
                delegateEvent.ChangeValue(this, ValueTUnrequested);
                delegateEvent.Message = eventArgs.Message;

                // Weiterleitung an alles Proxies
                BroadcastToProxies(delegateEvent);

                OnValueUpdatedOnReceival(eventArgs, ACPropertyChangedPhase.AfterBroadcast);
            }
        }


        /// <summary>
        /// Broadcast Event an alle Proxy-Objekte
        /// </summary>
        /// <param name="eventArgs"></param>
        protected override void BroadcastToProxies(ACPropertyValueEvent<T> eventArgs)
        {
            base.BroadcastToProxies(eventArgs);
        }

#endregion

#region EventHandling Methods
#endregion
    }


    /// <summary>
    /// A "Target-Property" is a network capable property, that doesn't contain or holds a original value. "Target-Properties" have bindings to a "Source-Property" where they get the original value from.
    /// This instance is created instead of a ACPropertyNetTarget when the type of the Soruce-Property is different to the Target-Property.
    /// </summary>
    /// <typeparam name="T">Type of the ValueT-Property which is serializable and registered in the KnownTypes-Property of the DataContract-Serializer.</typeparam>
    /// <typeparam name="S">Type of the Value in of the Soruce-Property.</typeparam>
    /// <seealso cref="gip.core.autocomponent.ACPropertyNetServerBase{T}" />
    /// <seealso cref="gip.core.autocomponent.IACPropertyNetTarget" />
    public class ACPropertyNetTargetConverter<T, S> : ACPropertyNetServerBase<T>, IACPropertyNetTarget
    {
#region c'tors
        public ACPropertyNetTargetConverter(IACComponent acComponent, ACClassProperty acClassProperty, ACClassPropertyRelation relation)
            : base(new ACPropertyValueHolder<T>(), acComponent, acClassProperty, false, null)
        {
            _PropRelation = relation;
        }

        public ACPropertyNetTargetConverter(IACPropertyNetBase PropertySource, IACComponent acComponent, ACClassProperty acClassProperty, ACClassPropertyRelation relation)
            : base(new ACPropertyValueHolder<T>(), acComponent, acClassProperty, false, null)
        {
            if (PropertySource is ACPropertyNetSource<S>)
                _Source = (ACPropertyNetSource<S>)PropertySource;
            if (_Source != null)
                _Source.Targets.Add(this);
            _PropRelation = relation;
        }

        /// <summary>
        /// Restores the stored value from the database into this persistable property
        /// </summary>
        /// <param name="IsInit">if set to <c>true</c> [is init].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public override bool ReStoreFromDB(bool IsInit)
        {
            // Target-Properties dürfen nicht während der Initialisierungsphase gesetzt werden,
            // da der Wert im Source-Property überschrieben werden würde.
            // Sie dürfen nur aktiv durch ein Kommando des Anweders ausgelöst werden
            // Beispiel: Konfigurationswerte in der SPS
            if (IsInit)
                return false;
            return base.ReStoreFromDB(IsInit);
        }

        //public override bool Persist()
        //{
        //    return base.Persist();
        //}

        public override void ACDeInit(bool deleteACClassTask = false)
        {
            base.ACDeInit(deleteACClassTask);
            if (ACRoot.SRoot.InitState != ACInitState.Destructed && ACRoot.SRoot.InitState != ACInitState.Destructing)
                UnbindSourceProperty();
        }

#endregion

#region Properties
        private ACPropertyNetSource<S> _Source;
        /// <summary>
        /// Reference to a "Source-Property" where this "Target-Property" is bound to.
        /// </summary>
        public IACPropertyNetSource Source
        {
            get
            {
                return _Source;
            }
        }

        protected override bool CanPersist
        {
            get
            {
                if (Source != null)
                    return false;
                return base.CanPersist;
            }
        }

        protected override bool CanRestoreRuntimeValue
        {
            get
            {
                if (Source != null)
                    return false;
                return base.CanRestoreRuntimeValue;
            }
        }

        protected ACClassPropertyRelation _PropRelation;
        public ACClassPropertyRelation PropRelation
        {
            get
            {
                return _PropRelation;
            }
        }

        private ExprParser _Parser;
        private ExprParser ExprParser
        {
            get
            {
                if (_Parser != null)
                    return _Parser;
                _Parser = new ExprParser();
                return _Parser;
            }
        }

        private LambdaExpression _ParserLambdaExprToT;
        private LambdaExpression _ParserLambdaExprToS;

        /// <summary>
        /// CurrentEventArgs ist immer nur temporär gesetzt, wenn PropertyAccessor verwendet wird (Vor Aufruf gesetzt, nach Aufruf gelöscht)
        /// </summary>
        protected ACPropertyValueEvent<T> _Current_T_EventArgsForPropertyAccessor = null;

        /// <summary>
        /// CurrentEventArgs ist immer nur temporär gesetzt, wenn PropertyAccessor verwendet wird (Vor Aufruf gesetzt, nach Aufruf gelöscht)
        /// </summary>
        protected ACPropertyValueEvent<S> _Current_S_EventArgsForPropertyAccessor = null;

#endregion

#region Internal Methods
        protected override void OnCustomTypeChanged(object sender, PropertyChangedEventArgs e)
        {
            // OnCustomTypeChanged wird eigentlich nicht benötigt, da Changed-event in ACPropertyValueHolder ausgelöst wird
            // DAMIR TODO: Test ob alles noch funktioniert
            // Ansonsten wird unnötig zweimal ValueUpdatedOnReceival hintereinander ausgelöst

            //if (e.PropertyName == Const.ValueT)
            //OnPropertyChanged();
            // base.OnCustomTypeChanged(sender, e);
        }


        /// <summary>
        /// Binds this target property to the passed source-Property.
        /// Method succeeds only if both types are equal (Source and Taregt)
        /// </summary>
        /// <param name="acPropertySource">Source-Property to bind</param>
        /// <param name="bindingMode"></param>
        /// <returns>PropBindingBindingResult</returns>
        public PropBindingBindingResult BindPropertyToSource(IACPropertyNetSource acPropertySource, PropBindingMode bindingMode = PropBindingMode.BindAndBroadcast)
        {
            if ((bindingMode == PropBindingMode.BindAndBroadcast || bindingMode == PropBindingMode.BindOnly)
                && acPropertySource != null)
            {
                if (!(acPropertySource is ACPropertyNetSource<S>))
                    return PropBindingBindingResult.NotCompatibleTypes;
                _PropertyValue = new ACPropertyValueHolder<T>();
                _PropertyValue.Owner = this;
                _Source = acPropertySource as ACPropertyNetSource<S>;
                if (_Source != null)
                {
                    _Source.Targets.Add(this);
                    T valInUnitOfT = ConvertToT(_Source.ValueTUnrequested);
                    _PropertyValue.ChangeValue(this, ApplyConvExpressionT(valInUnitOfT));
                }
            }
            if (bindingMode == PropBindingMode.BindAndBroadcast || bindingMode == PropBindingMode.BroadcastOnly)
            {
                OnPropertyChanged(Const.SourceValue);
                BroadcastValueChangedInSource(EventRaiser.Source);
            }
            return PropBindingBindingResult.Succeeded;
        }


        /// <summary>
        /// Binds this target property to the passed source-Property.
        /// If types are not compatible it creates a new target property of the ACPropertyNetTargetConverter-Class that is replaced with this. In this case newTarget is not null.
        /// If a creation is not possible because the Datatype of the source-property must be replaced (e.g. the Type is another BitAccess-Wrapper) the VarIPlusiobatch-Service must me restarted afterwards.
        /// </summary>
        /// <param name="acPropertySource">Source-Property to bind</param>
        /// <param name="newTarget"></param>
        /// <param name="message"></param>
        /// <param name="bindInDBIfConverterNeeded">If true a binding in the database will be defined. At the next IPlus-Restart the Target-Property will be created as a ACPropertyNetTargetConverter automatically</param>
        /// <param name="bindingMode"></param>
        /// <returns>PropBindingBindingResult</returns>
        public PropBindingBindingResult BindPropertyToSource(IACPropertyNetSource acPropertySource, 
            out IACPropertyNetTarget newTarget, out string message, 
            bool bindInDBIfConverterNeeded = true, PropBindingMode bindingMode = PropBindingMode.BindAndBroadcast)
        {
            message = null;
            newTarget = null;
            PropBindingBindingResult result = BindPropertyToSource(acPropertySource, bindingMode);
            if (result.HasFlag(PropBindingBindingResult.Succeeded))
                return result;
            else if (result.HasFlag(PropBindingBindingResult.NotCompatibleTypes))
            {
                result = ACPropertyNetServerBase<T>.RepairIncompatibleTypes(this, acPropertySource, out newTarget, out message, bindInDBIfConverterNeeded);
                if (result.HasFlag(PropBindingBindingResult.TargetPropReplaced))
                {
                    result |= newTarget.BindPropertyToSource(acPropertySource, bindingMode);
                }
            }
            return result;
        }


        /// <summary>
        /// Unbinds the source property.
        /// </summary>
        public void UnbindSourceProperty()
        {
            _PropertyValue = new ACPropertyValueHolder<T>();
            if (_Source != null)
            {
                T valInUnitOfT = ConvertToT(_Source.ValueTUnrequested);
                _PropertyValue.ChangeValue(this, ApplyConvExpressionT(valInUnitOfT));

                using (ACMonitor.Lock(_Source._20035_LockTargets))
                {
                    _Source.Targets.Remove(this);
                }
            }
            _Source = null;
        }

        private void BroadcastValueChangedInSource(EventRaiser raiser = EventRaiser.Target)
        {
            if (this.ACRef == null)
                return;
            // Broadcast (mit eigener Request-ID, beachte EventType)
            ACPropertyValueEvent<T> broadcast = new ACPropertyValueEvent<T>(EventTypes.ValueChangedInSource,
                                                                            raiser,
                                                                         this.ACRef.ValueT, this.ACType);
            broadcast.ChangeValue(this, ValueTUnrequested);
            broadcast.Handled = true;
            BroadcastToProxies(broadcast);


            using (ACMonitor.Lock(this._20015_LockValue))
            {
                // Lösche interne Anfrage
                _PropertyValueRequest = null;
            }

            // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
            OnValueUpdatedOnReceival(broadcast, ACPropertyChangedPhase.BeforeBroadcast);

            // Löse PropertyChanged-Event aus
            if (_LiveLog != null)
                _LiveLog.AddValue(this.Value);
            OnPropertyChanged(Const.ValueT);
            OnPropertyChanged(Const.Value);
            OnValueUpdatedOnReceival(broadcast, ACPropertyChangedPhase.AfterBroadcast);
        }

        /// <summary>
        /// Internes Auslösen der Wertänderung durch:
        /// - Direktes setzen von ValueT (oder Oberflächenbindung)
        /// - Indirekt durch PropertyAccessorChanged-Event -&gt; Setzen von ValueT
        /// - Durch OnPropertychanged()-Aufruf von PropertyValueHolder weil ListChanged-/CollectionChanged-/PropertyChanged-Ereignis eines komplexen T-Objekts ausgelöst worden ist
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="forceSend"></param>
        /// <param name="invokerInfo"></param>
        protected override void ChangeValueRequest(T newValue, bool forceSend, object invokerInfo = null)
        {
            //base.ChangeValueRequest(newValue, forceSend);
            if (_PropertyValue == null || this.ACRef == null)
                return;
            if (!PropertyInfo.IsBroadcast)
                return;
            ACPropertyValueEvent<T> newRequest;

            // Erster Aufruf der Methode bzw. 
            // NICHT indirekt aufgerufen über OnValueEventReceived->PropertyAccessorChanged
            if (_Current_T_EventArgsForPropertyAccessor == null)
            {
                newRequest = new ACPropertyValueEvent<T>(EventTypes.Request,
                                                            EventRaiser.Target,
                                                            this.ACRef.ValueT, this.ACType, invokerInfo);
                newRequest.ChangeValue(this, newValue);
                newRequest.ForceBroadcast = forceSend;

                // Callback Set-Method von ACObject
                if (SetMethod != null)
                    SetMethod(newRequest);
                else if (NetSetMethodOfScript != null)
                    NetSetMethodOfScript(newRequest, this);

                if (newRequest.Handled)
                    return;

                // Falls Wert nicht geändert
                if (!forceSend && (_PropertyValue.CompareTo(newValue, this) == 0))
                    return;


                using (ACMonitor.Lock(this._20015_LockValue))
                {
                    // Setze aktuelle Anfrage
                    _PropertyValueRequest = newRequest;
                }

                // Falls Wert mit Source verbunden, delegiere Auftrag an Source-Objekt weiter
                if (_Source != null)
                    _Source.OnValueEventReceived(ConvertToS(newRequest));
                else
                    this.OnValueEventReceived(newRequest);
            }

            // Zweiter Aufruf dieser Methode
            // => Indirekt aufgerufen über OnValueEventReceived->PropertyAccessorChanged
            else //(_Current_T_EventArgsForPropertyAccessor != null)
            {
                // Falls Aufruf von Proxy-Objekt
                if (_Current_T_EventArgsForPropertyAccessor.Sender == EventRaiser.Proxy)
                {
                    // Falls Wert mit Source verbunden, delegiere Proxy-Auftrag an Source-Objekt weiter
                    if (_Source != null)
                    {
                        _Current_T_EventArgsForPropertyAccessor.ChangeValue(this, newValue);
                        _Source.OnValueEventReceived(ConvertToS(_Current_T_EventArgsForPropertyAccessor));
                        return;
                    }
                    // Angelangt am Endpunkt, da Target nicht verbunden mit Source
                    else
                    {
                        newRequest = new ACPropertyValueEvent<T>(_Current_T_EventArgsForPropertyAccessor, EventTypes.Response,
                                                                                        EventRaiser.Target, ACRef.ValueT, this.ACType, invokerInfo);
                        newRequest.ChangeValue(this, ValueTUnrequested);
                        // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
                        OnValueUpdatedOnReceival(newRequest, ACPropertyChangedPhase.BeforeBroadcast);

                        // Änderungen, die nicht von der Source-Quelle kommen können persistiert werden
                        Persist();

                        BroadcastToProxies(newRequest);

                        // Löse PropertyChanged-Event aus
                        if (_LiveLog != null)
                            _LiveLog.AddValue(this.Value);
                        OnPropertyChanged(Const.ValueT);
                        OnPropertyChanged(Const.Value);

                        OnValueUpdatedOnReceival(newRequest, ACPropertyChangedPhase.AfterBroadcast);
                        return;
                    }
                }
                // Falls Aufruf intern, weil nicht mit Source verbunden
                else if (_Current_T_EventArgsForPropertyAccessor.Sender == EventRaiser.Target)
                {
                    return;
                }
                // Falls Aufruf von Source-Objekt
                else //if (eventArgs.Sender == EventRaiser.Source)
                {
                    // SetSourceValue muss nicht aufgerufen werden, 
                    // da Wert sich schon im gebundenen Source-Objekt verändert hat
                    // Gebe nur Änderung bekannt
                    OnPropertyChanged(Const.SourceValue);

                    // Verwalte interne Anfrage
                    ManagePropertyValueRequestOnReceival(_Current_T_EventArgsForPropertyAccessor);

                    // Umverpackung
                    newRequest = new ACPropertyValueEvent<T>(_Current_T_EventArgsForPropertyAccessor, _Current_T_EventArgsForPropertyAccessor.EventType,
                                                                                    EventRaiser.Source,
                                                                                    ACRef.ValueT, this.ACType);
                    newRequest.ChangeValue(this, newValue);
                    newRequest.Message = _Current_T_EventArgsForPropertyAccessor.Message;

                    // Weiterleitung an alles Proxies
                    BroadcastToProxies(newRequest);
                    return;
                }
            }
        }


        /// <summary>
        /// Called when a value has changed in the "Source-Property"
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        public void OnValueEventReceivedFromSource(IACPropertyNetValueEvent eventArgs)
        {
            if (eventArgs is ACPropertyValueEvent<T>)
                OnValueEventReceived((ACPropertyValueEvent<T>)eventArgs);
            else if (eventArgs is ACPropertyValueEvent<S>)
                OnValueEventReceived((ACPropertyValueEvent<S>)eventArgs);
        }

        /// <summary>
        /// Method is called when a change-request has arrived from outside.
        /// - Either from the Network/WCF-Service through iteration of the PropertyNetValueEvent-Queue
        /// - or internaly in method ChangeValueRequest
        /// </summary>
        /// <param name="eventArgs"></param>
        internal override void OnValueEventReceived(ACPropertyValueEvent<T> eventArgs)
        {
            if (eventArgs == null || this.ACRef == null)
                return;

            // Falls Aufruf von Proxy-Objekt
            if (eventArgs.Sender == EventRaiser.Proxy)
            {
                // Callback Set-Method von ACObject
                if (SetMethod != null)
                    SetMethod(eventArgs);
                else if (NetSetMethodOfScript != null)
                    NetSetMethodOfScript(eventArgs, this);

                // Falls Setter-Methode in Objekt existiert
                if (this.PropertyAccessor != null)
                {
                    // Falls durch SetMethod keine Abweisung stattgefunden hat, dann Setter-Methode aufrufen
                    if (!eventArgs.Handled)
                    {
                        _Current_T_EventArgsForPropertyAccessor = eventArgs;
                        if (ACRef.ValueT != null)
                            this.PropertyAccessor.Set(ACRef.ValueT, eventArgs.Value);
                        // Zurueckspringen, da durch ProprtyChanged-Aufruf in Setter-Methode ein neuer Broadcast-Request generiert wurde
                        _Current_T_EventArgsForPropertyAccessor = null;
                        return;
                    }
                }

                // Falls Anfrage nicht weitergeleitet werden soll
                if (eventArgs.Handled)
                {
                    // Falls Wert mit Source verbunden
                    if (_Source != null)
                    {
                        // Ändere Wert in Source-Objekt nicht ab, 
                        // Broadcast mit Proxy-RequestID
                    }
                    else
                    {
                        // Angelangt am Endpoint
                        // Ändere lokalen Wert nicht ab, 
                        // Broadcast mit Proxy-RequestID
                    }
                    ACPropertyValueEvent<T> response = new ACPropertyValueEvent<T>(eventArgs, EventTypes.RefusedRequest,
                                                                                    EventRaiser.Target, ACRef.ValueT, this.ACType);
                    response.ChangeValue(this, ValueTUnrequested);
                    BroadcastToProxies(response);
                    return;
                }
                else
                {
                    // Falls Wert mit Source verbunden, delegiere Proxy-Auftrag an Source-Objekt weiter
                    if (_Source != null)
                    {
                        _Source.OnValueEventReceived(ConvertToS(eventArgs));
                        return;
                    }
                    else
                    {
                        // Angelangt am Endpoint
                        eventArgs.Handled = true;

                        // Ändere Wert ab 
                        SetSourceValue(eventArgs.Value);

                        // Broadcast mit Proxy-RequestID
                        ACPropertyValueEvent<T> response = new ACPropertyValueEvent<T>(eventArgs, EventTypes.Response,
                                                                                        EventRaiser.Target, ACRef.ValueT, this.ACType);
                        response.ChangeValue(this, ValueTUnrequested);
                        // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
                        OnValueUpdatedOnReceival(eventArgs, ACPropertyChangedPhase.BeforeBroadcast);

                        // Änderungen, die nicht von der Source-Quelle kommen können persistiert werden
                        Persist();

                        BroadcastToProxies(response);

                        // Löse PropertyChanged-Event aus
                        if (_LiveLog != null)
                            _LiveLog.AddValue(this.Value);
                        OnPropertyChanged(Const.ValueT);
                        OnPropertyChanged(Const.Value);
                        OnValueUpdatedOnReceival(eventArgs, ACPropertyChangedPhase.AfterBroadcast);
                        return;
                    }
                }
            }
            // Falls Aufruf intern, weil nicht mit Source verbunden
            else if (eventArgs.Sender == EventRaiser.Target)
            {
                // Angelangt am Endpoint
                eventArgs.Handled = true;

                // Ändere Wert ab 
                SetSourceValue(eventArgs.Value);

                // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
                // OnValueUpdatedOnReceival();

                // Löse PropertyChanged-Event aus
                //if (_LiveLog != null)
                //_LiveLog.AddValue(this.Value);
                //OnPropertyChanged(Const.ValueT);
                //OnPropertyChanged(Const.Value);

                // Broadcast (mit eigener Request-ID, beachte EventType)
                BroadcastValueChangedInSource();
                return;
            }
        }

        /// <summary>
        /// Methode wird aufgerufen sobald ein Änderungsrequest von Außen angekommen ist
        /// - Netzwerk/WCF-Dienst durch Abarbeitung der PropertyNetValueEvent-Queue
        /// - intern durch Fall-Unterscheidung in Methode ChangeValueRequest
        /// </summary>
        /// <param name="eventArgs"></param>
        internal void OnValueEventReceived(ACPropertyValueEvent<S> eventArgs)
        {
            if (eventArgs == null || this.ACRef == null)
                return;

            // Falls Aufruf von Source-Objekt
            if (eventArgs.Sender == EventRaiser.Source)
            {
                ACPropertyValueEvent<T> eventArgsT = ConvertToT(eventArgs);

                // Callback Set-Method von ACObject
                if (SetMethod != null)
                    SetMethod(eventArgsT);
                else if (NetSetMethodOfScript != null)
                    NetSetMethodOfScript(eventArgsT, this);

                // Falls Setter-Methode in Objekt existiert
                if (this.PropertyAccessor != null)
                {
                    // Falls durch SetMethod keine Abweisung stattgefunden hat, dann Setter-Methode aufrufen
                    if (!eventArgs.Handled)
                    {
                        _Current_T_EventArgsForPropertyAccessor = eventArgsT;
                        if (ACRef.ValueT != null)
                            this.PropertyAccessor.Set(ACRef.ValueT, _Current_T_EventArgsForPropertyAccessor.Value);
                        // Zurueckspringen, da durch ProprtyChanged-Aufruf in Setter-Methode ein neuer Broadcast-Request generiert wurde
                        _Current_T_EventArgsForPropertyAccessor = null;
                        return;
                    }
                }

                int valueChanged = 0;
                if (eventArgsT.ForceBroadcast)
                    valueChanged = 1;
                else if (_PropertyValue != null)
                    valueChanged = _PropertyValue.CompareTo(eventArgsT.Value, this);
                if (valueChanged != 0)
                {
                    SetSourceValue(eventArgsT.Value);

                    // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
                    OnValueUpdatedOnReceival(eventArgs, ACPropertyChangedPhase.BeforeBroadcast);

                    // SetSourceValue muss nicht aufgerufen werden, 
                    // da Wert sich schon im gebundenen Source-Objekt verändert hat
                    // Gebe nur Änderung bekannt
                    OnPropertyChanged(Const.SourceValue);

                    // Änderungen, die nicht von der Source-Quelle kommen können persistiert werden
                    Persist();

                    // Löse PropertyChanged-Event aus
                    if (_LiveLog != null)
                        _LiveLog.AddValue(this.Value);
                    OnPropertyChanged(Const.ValueT);
                    OnPropertyChanged(Const.Value);

                    // Verwalte interne Anfrage
                    ManagePropertyValueRequestOnReceival(eventArgsT);

                    // Umverpackung
                    ACPropertyValueEvent<T> delegateEvent = new ACPropertyValueEvent<T>(eventArgsT, eventArgs.EventType,
                                                                                    EventRaiser.Source,
                                                                                    ACRef.ValueT, this.ACType);
                    delegateEvent.ChangeValue(this, ValueTUnrequested);
                    delegateEvent.Message = eventArgs.Message;

                    // Weiterleitung an alles Proxies
                    BroadcastToProxies(delegateEvent);

                    OnValueUpdatedOnReceival(eventArgs, ACPropertyChangedPhase.AfterBroadcast);
                }
            }
        }
#endregion

        public static S ConvertToS(T valueT)
        {
            Type typeS = typeof(S);
            Type typeS2Conv = typeS;
            if (typeS.IsEnum)
                typeS2Conv = Enum.GetUnderlyingType(typeS);
            else if (typeS.IsGenericType && typeS.Name == Const.TNameNullable)
                typeS2Conv = typeS.GetGenericArguments()[0];
            Type typeT = typeof(T);
            object valueTToConv = valueT;
            if (typeT.IsEnum)
            {
                Type typeEnumT = Enum.GetUnderlyingType(typeT);
                if (typeEnumT == typeof(Int16))
                    valueTToConv = (Int16)valueTToConv;
                else if (typeEnumT == typeof(Int32))
                    valueTToConv = (Int32)valueTToConv;
                else if (typeEnumT == typeof(UInt16))
                    valueTToConv = (UInt16)valueTToConv;
                else if (typeEnumT == typeof(UInt32))
                    valueTToConv = (UInt32)valueTToConv;
                else if (typeEnumT == typeof(Int64))
                    valueTToConv = (Int64)valueTToConv;
                else if (typeEnumT == typeof(UInt64))
                    valueTToConv = (UInt64)valueTToConv;
                else if (typeEnumT == typeof(Byte))
                    valueTToConv = (Byte)valueTToConv;
                else
                    valueTToConv = Enum.GetName(typeEnumT, valueTToConv);
            }
            else if (typeT.IsGenericType && typeT.Name == Const.TNameNullable)
            {
                if (valueTToConv != null)
                {
                    valueTToConv = typeT.GetValue(Const.Value);
                }
            }
            S convertedResult;
            if (typeT.IsAssignableFrom(typeof(TimeSpan)))
                convertedResult = (S)Convert.ChangeType(((TimeSpan)(object)valueT).TotalSeconds, typeS);
            else if (typeS.IsAssignableFrom(typeof(TimeSpan)))
                convertedResult = (S)(object)new TimeSpan(0, 0, Convert.ToInt32(valueTToConv));
            else if (valueTToConv != null)
            {
                if (typeS.IsGenericType && typeS.Name == Const.TNameNullable)
                {
                    object genericParamValue = Convert.ChangeType(valueTToConv, typeS2Conv);
                    convertedResult = (S)Activator.CreateInstance(typeS, new Object[] { genericParamValue });
                }
                else
                    convertedResult = (S)Convert.ChangeType(valueTToConv, typeS2Conv);
            }
            else
                convertedResult = default(S);
            if (typeS.IsEnum)
            {
                if (!Enum.IsDefined(typeS, convertedResult))
                {
                    convertedResult = (S)Enum.GetValues(typeS).GetValue(0);
                }
            }
            return convertedResult;
        }

        public static T ConvertToT(S valueS)
        {
            Type typeT = typeof(T);
            Type typeT2Conv = typeT;
            if (typeT.IsEnum)
                typeT2Conv = Enum.GetUnderlyingType(typeT);
            else if (typeT.IsGenericType && typeT.Name == Const.TNameNullable)
                typeT2Conv = typeT.GetGenericArguments()[0];
            Type typeS = typeof(S);
            object valueSToConv = valueS;
            if (typeS.IsEnum)
            {
                Type typeEnumS = Enum.GetUnderlyingType(typeS);
                if (typeEnumS == typeof(Int16))
                    valueSToConv = (Int16)valueSToConv;
                else if (typeEnumS == typeof(Int32))
                    valueSToConv = (Int32)valueSToConv;
                else if (typeEnumS == typeof(UInt16))
                    valueSToConv = (UInt16)valueSToConv;
                else if (typeEnumS == typeof(UInt32))
                    valueSToConv = (UInt32)valueSToConv;
                else if (typeEnumS == typeof(Int64))
                    valueSToConv = (Int64)valueSToConv;
                else if (typeEnumS == typeof(UInt64))
                    valueSToConv = (UInt64)valueSToConv;
                else if (typeEnumS == typeof(Byte))
                    valueSToConv = (Byte)valueSToConv;
                else
                    valueSToConv = Enum.GetName(typeEnumS, valueSToConv);
            }
            //else if (typeS.IsGenericType && typeS.Name == Const.NullableTName)
            //{
            //    if (valueSToConv != null)
            //        valueSToConv = typeS.GetValue(Const.Value);
            //}
            T convertedResult;
            if (typeT.IsAssignableFrom(typeof(TimeSpan)))
                convertedResult = (T)(object)new TimeSpan(0, 0, Convert.ToInt32(valueSToConv));
            else if (typeS.IsAssignableFrom(typeof(TimeSpan)))
                convertedResult = (T)Convert.ChangeType(((TimeSpan)(object)valueS).TotalSeconds, typeT);
            else if (valueSToConv != null)
            {
                if (typeT.IsGenericType && typeT.Name == Const.TNameNullable)
                {
                    object genericParamValue = Convert.ChangeType(valueSToConv, typeT2Conv);
                    convertedResult = (T)Activator.CreateInstance(typeT, new Object[] { genericParamValue });
                }
                else
                    convertedResult = (T)Convert.ChangeType(valueSToConv, typeT2Conv);
            }
            else
                convertedResult = default(T);
            if (typeT.IsEnum)
            {
                if (!Enum.IsDefined(typeT, convertedResult))
                {
                    convertedResult = (T)Enum.GetValues(typeT).GetValue(0);
                }
            }
            return convertedResult;
        }


        public static bool AreTypesCompatible(Type targetType, Type sourceType)
        {
            if ((targetType == null) || (sourceType == null))
                return false;
            if (targetType != sourceType)
            {
                Type typeBitAccess = typeof(IBitAccess);
                if (typeBitAccess.IsAssignableFrom(sourceType) && typeBitAccess.IsAssignableFrom(targetType))
                {
                    string underlyingType1 = BitAccess<short>.GetUnderlyingTypeOfBitAccess(sourceType);
                    string underlyingType2 = BitAccess<short>.GetUnderlyingTypeOfBitAccess(targetType);
                    if (underlyingType1 == underlyingType2)
                        return true;
                }
                else if (typeBitAccess.IsAssignableFrom(targetType))
                {
                    string underlyingType1 = sourceType.Name;
                    string underlyingType2 = BitAccess<short>.GetUnderlyingTypeOfBitAccess(targetType);
                    if (underlyingType1 == underlyingType2)
                        return true;
                }
                Type typeConvertible = typeof(IConvertible);
                Type typeTimeSpan = typeof(TimeSpan);
                if (!targetType.IsEnum
                    && !typeConvertible.IsAssignableFrom(targetType)
                    && !typeTimeSpan.IsAssignableFrom(targetType))
                    return false;
                if (!sourceType.IsEnum
                    && !typeConvertible.IsAssignableFrom(sourceType)
                    && !typeTimeSpan.IsAssignableFrom(sourceType))
                    return false;
            }
            return true;
        }

        public static object ConvertS2T(object valueS, Type typeS, Type typeT)
        {
            Type typeT2Conv = typeT;
            if (typeT.IsEnum)
                typeT2Conv = Enum.GetUnderlyingType(typeT);
            else if (typeT.IsGenericType && typeT.Name == Const.TNameNullable)
                typeT2Conv = typeT.GetGenericArguments()[0];
            object valueSToConv = valueS;
            if (typeS.IsEnum)
            {
                Type typeEnumS = Enum.GetUnderlyingType(typeS);
                if (typeEnumS == typeof(Int16))
                    valueSToConv = (Int16)valueSToConv;
                else if (typeEnumS == typeof(Int32))
                    valueSToConv = (Int32)valueSToConv;
                else if (typeEnumS == typeof(UInt16))
                    valueSToConv = (UInt16)valueSToConv;
                else if (typeEnumS == typeof(UInt32))
                    valueSToConv = (UInt32)valueSToConv;
                else if (typeEnumS == typeof(Int64))
                    valueSToConv = (Int64)valueSToConv;
                else if (typeEnumS == typeof(UInt64))
                    valueSToConv = (UInt64)valueSToConv;
                else if (typeEnumS == typeof(Byte))
                    valueSToConv = (Byte)valueSToConv;
                else
                    valueSToConv = Enum.GetName(typeEnumS, valueSToConv);
            }
            //else if (typeS.IsGenericType && typeS.Name == Const.NullableTName)
            //{
            //    if (valueSToConv != null)
            //        valueSToConv = typeS.GetValue(Const.Value);
            //}
            T convertedResult;
            if (typeT.IsAssignableFrom(typeof(TimeSpan)))
                convertedResult = (T)(object)new TimeSpan(0, 0, Convert.ToInt32(valueSToConv));
            else if (typeS.IsAssignableFrom(typeof(TimeSpan)))
                convertedResult = (T)Convert.ChangeType(((TimeSpan)(object)valueS).TotalSeconds, typeT);
            else if (valueSToConv != null)
            {
                if (typeT.IsGenericType && typeT.Name == Const.TNameNullable)
                {
                    object genericParamValue = Convert.ChangeType(valueSToConv, typeT2Conv);
                    convertedResult = (T)Activator.CreateInstance(typeT, new Object[] { genericParamValue });
                }
                else
                    convertedResult = (T)Convert.ChangeType(valueSToConv, typeT2Conv);
            }
            else
                convertedResult = default(T);
            if (typeT.IsEnum)
            {
                if (!Enum.IsDefined(typeT, convertedResult))
                {
                    convertedResult = (T)Enum.GetValues(typeT).GetValue(0);
                }
            }
            return convertedResult;
        }

        public ACPropertyValueEvent<S> ConvertToS(ACPropertyValueEvent<T> eventT)
        {
            ACPropertyValueEvent<S> eventS = new ACPropertyValueEvent<S>();
            eventS.CopyFrom(eventT);
            try
            {
                T valInUnitOfS = ApplyConvExpressionS(eventT.Value);
                eventS.Value = ConvertToS(valInUnitOfS);
            }
            catch (Exception ex)
            {
                ParentACComponent?.Messages?.LogException(ParentACComponent.GetACUrl() + ACUrlHelper.Delimiter_DirSeperator + this.ACIdentifier, "ConvertToS(10)", ex);
            }
            return eventS;
        }

        public ACPropertyValueEvent<T> ConvertToT(ACPropertyValueEvent<S> eventS)
        {
            ACPropertyValueEvent<T> eventT = new ACPropertyValueEvent<T>();
            eventT.CopyFrom(eventS);
            try
            {
                T valInUnitOfT = ConvertToT(eventS.Value);
                eventT.Value = ApplyConvExpressionT(valInUnitOfT);
            }
            catch (Exception ex)
            {
                ParentACComponent?.Messages?.LogException(ParentACComponent.GetACUrl() + ACUrlHelper.Delimiter_DirSeperator + this.ACIdentifier, "ConvertToT(10)", ex);
            }
            return eventT;
        }

        /// <summary>
        /// Methode rechnet den Source-Wert in die Einheit des Target-Wertes um
        /// </summary>
        /// <param name="origValS"></param>
        /// <returns></returns>
        public T ApplyConvExpressionT(T origValS)
        {
            if (_PropRelation == null)
                return origValS;
            if ((_PropRelation.Multiplier.HasValue || _PropRelation.Divisor.HasValue) && IsValueType)
            {
                double mult = _PropRelation.Multiplier.HasValue ? _PropRelation.Multiplier.Value : 1;
                double div = _PropRelation.Divisor.HasValue ? _PropRelation.Divisor.Value : 1;
                return (T)Convert.ChangeType((System.Convert.ToDouble(origValS) * mult) / div, typeof(T));
            }
            else if (!String.IsNullOrEmpty(_PropRelation.ConvExpressionT) && !String.IsNullOrEmpty(_PropRelation.ConvExpressionS))
            {
                try
                {
                    // ACUrlCommand
                    if (_PropRelation.ConvExpressionT[0] == '!' || _PropRelation.ConvExpressionT[0] == '\\' || _PropRelation.ConvExpressionT[0] == '.')
                    {
                        return (T)ParentACComponent.ACUrlCommand(_PropRelation.ConvExpressionT, origValS);
                    }
                    // Expression
                    else
                    {
                        if (_ParserLambdaExprToT == null)
                            _ParserLambdaExprToT = ExprParser.Parse(_PropRelation.ConvExpressionT);
                        return (T)ExprParser.Run(_ParserLambdaExprToT, origValS);
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPropertyNetConverter<T,S>", "ApplyConvExpressionT", msg);
                }
            }
            return origValS;
        }

        /// <summary>
        /// Methode rechnet den Target-Wert in die Einheit des Source-Wertes um
        /// </summary>
        /// <param name="convValT"></param>
        /// <returns></returns>
        public T ApplyConvExpressionS(T convValT)
        {
            if (_PropRelation == null)
                return convValT;
            if ((_PropRelation.Multiplier.HasValue || _PropRelation.Divisor.HasValue) && IsValueType)
            {
                double mult = _PropRelation.Multiplier.HasValue ? _PropRelation.Multiplier.Value : 1;
                double div = _PropRelation.Divisor.HasValue ? _PropRelation.Divisor.Value : 1;
                return (T)Convert.ChangeType((System.Convert.ToDouble(convValT) * div) / mult, typeof(T));
            }
            else if (!String.IsNullOrEmpty(_PropRelation.ConvExpressionT) && !String.IsNullOrEmpty(_PropRelation.ConvExpressionS))
            {
                try
                {
                    // ACUrlCommand
                    if (_PropRelation.ConvExpressionS[0] == '!' || _PropRelation.ConvExpressionS[0] == '\\' || _PropRelation.ConvExpressionS[0] == '.')
                    {
                        ParentACComponent.ACUrlCommand(_PropRelation.ConvExpressionS, convValT);
                    }
                    // Expression
                    else
                    {
                        if (_ParserLambdaExprToS == null)
                            _ParserLambdaExprToS = ExprParser.Parse(_PropRelation.ConvExpressionS);
                        return (T)ExprParser.Run(_ParserLambdaExprToS, convValT);
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("ACPropertyNetTargetConverter<T,S>", "ApplyConvExpression", msg);
                }
            }
            return convValT;
        }
    }
}
