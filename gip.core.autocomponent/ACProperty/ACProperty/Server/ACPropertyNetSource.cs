using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using gip.core.datamodel;
using System.Threading;
using System.Collections.Concurrent;

namespace gip.core.autocomponent
{
    /// <summary>
    /// A "Source-Property" is a network capable property, that contains or holds a original value. "Target-Properties" can have a binding to this "Source-Property" to receive the value.
    /// </summary>
    /// <typeparam name="T">Type of the ValueT-Property which is serializable and registered in the KnownTypes-Property of the DataContract-Serializer.</typeparam>
    /// <seealso cref="gip.core.autocomponent.ACPropertyNetServerBase{T}" />
    /// <seealso cref="gip.core.autocomponent.IACPropertyNetSource" />
    public class ACPropertyNetSource<T> : ACPropertyNetServerBase<T>, IACPropertyNetSource
    {
        #region c'tors
        /// <summary>Constructor for SourceProperty as Real Object without SetValueInObject-Callback</summary>
        /// <param name="acComponent"></param>
        /// <param name="acClassProperty"></param>
        public ACPropertyNetSource(IACComponent acComponent, ACClassProperty acClassProperty)
            : this(new ACPropertyValueHolder<T>(), acComponent, acClassProperty, false, null)
        {
        }

        /// <summary>Constructor for SourceProperty as Proxy Object</summary>
        /// <param name="acComponent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="IsProxy"></param>
        public ACPropertyNetSource(IACComponent acComponent, ACClassProperty acClassProperty, bool IsProxy)
            : this(new ACPropertyValueHolder<T>(), acComponent, acClassProperty, IsProxy, null)
        {
        }

        /// <summary>SourceProperty as Real-Object =&gt; IsProxy = false;</summary>
        /// <param name="acComponent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="method"></param>
        public ACPropertyNetSource(IACComponent acComponent, ACClassProperty acClassProperty, ACPropertySetMethod method)
            : this(new ACPropertyValueHolder<T>(), acComponent, acClassProperty, false, method)
        {
        }

        /// <summary>Universal Constructor</summary>
        /// <param name="PropertyValue"></param>
        /// <param name="acComponent"></param>
        /// <param name="acClassProperty"></param>
        /// <param name="IsProxy"></param>
        /// <param name="method"></param>
        public ACPropertyNetSource(ACPropertyValueHolder<T> PropertyValue, IACComponent acComponent, ACClassProperty acClassProperty, bool IsProxy, ACPropertySetMethod method)
            : base(PropertyValue, acComponent, acClassProperty, IsProxy, method)
        {
        }

        public override void ACDeInit(bool deleteACClassTask = false)
        {
            //if (LogACState && IsACStateProperty)
            //    LogACStateChange(9999, "ACDeInit", true, true);

            base.ACDeInit(deleteACClassTask);
            if ((ACRoot.SRoot.InitState != ACInitState.Destructed && ACRoot.SRoot.InitState != ACInitState.Destructing) && Targets.Count > 0)
            {
                using (ACMonitor.Lock(_20035_LockTargets))
                {
                    foreach (IACPropertyNetTarget target in Targets.ToArray())
                    {
                        target.UnbindSourceProperty();
                    }
                }
            }
        }

#endregion

#region Properties
        public readonly ACMonitorObject _20035_LockTargets = new ACMonitorObject(20035);

        protected List<IACPropertyNetTarget> _Targets = new List<IACPropertyNetTarget>();
        /// <summary>
        /// Returns a List of "Target-Properties" that have bound it's value to this "Source-Property".
        /// NOT THREAD-SAFE! Use _20035_LockTargets to query the list!
        /// </summary>
        public List<IACPropertyNetTarget> Targets
        {
            get
            {
                return _Targets;
            }
        }


        protected List<object> _AdditionalRefs = null;
        /// <summary>
        /// Returns a List of other objects that refer this "Source-Property". e.g. OPC-Items
        /// NOT THREAD-SAFE! Use _20035_LockTargets to query the list!
        /// </summary>
        public List<object> AdditionalRefs
        {
            get
            {
                if (_AdditionalRefs == null)
                    _AdditionalRefs = new List<object>();
                return _AdditionalRefs;
            }
        }

        /// <summary>
        /// CurrentEventArgs ist immer nur temporär gesetzt, wenn PropertyAccessor verwendet wird (Vor Aufruf gesetzt, nach Aufruf gelöscht)
        /// </summary>
        protected ACPropertyValueEvent<T> _Current_T_EventArgsForPropertyAccessor = null;

        private bool _WasInitializedFromInvoker = false;
        public bool WasInitializedFromInvoker
        {
            get
            {
                return _WasInitializedFromInvoker;
            }
        }


        //private class ACStateTrace
        //{
        //    public DateTime DT { get; set; }
        //    public int TID { get; set; }
        //    public int Line { get; set; }
        //    public string Value { get; set; }
        //}

        //private List<ACStateTrace> _StateTraceList = null;
        //private static object _LockStateTraceList = new object();

        //public void LogACStateChange(int line, string value, bool dumpToFile = false, bool isACDeInit = false)
        //{
        //    if (!LogACState || InRestorePhase)
        //        return;
        //    try
        //    {
        //        lock (_LockStateTraceList)
        //        {
        //            if (_StateTraceList == null)
        //                _StateTraceList = new List<ACStateTrace>();

        //            if (dumpToFile || isACDeInit)
        //                DumpACStateLog(isACDeInit ? "ACDeInit" : "NewLog");
        //            _StateTraceList.Add(new ACStateTrace() { DT = DateTime.Now, Line = line, TID = Thread.CurrentThread.ManagedThreadId, Value = value });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        string msg = e.Message;
        //        if (e.InnerException != null && e.InnerException.Message != null)
        //            msg += " Inner:" + e.InnerException.Message;

        //        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
        //            datamodel.Database.Root.Messages.LogException("ACPropertyNetSource<T>", "LogACStateChange", msg);
        //    }
        //}

        //public void DumpACStateLog(string dumpReason)
        //{
        //    lock (_LockStateTraceList)
        //    {
        //        if (_StateTraceList != null && _StateTraceList.Any())
        //        {
        //            StringBuilder sb = new StringBuilder();
        //            sb.AppendLine(String.Format("{0} ({1}, {2})", GetACUrlComponent(), this.GetHashCode(), dumpReason));
        //            foreach (var entry in _StateTraceList)
        //            {
        //                sb.AppendLine(String.Format("{0:dd.MM.yyyy HH:mm:ss.ffff}, {1}, {2}, {3} ({4})", entry.DT, entry.TID, entry.Line, 
        //                String.IsNullOrEmpty(entry.Value) ? "Empty" : entry.Value, 
        //                    String.IsNullOrEmpty(entry.Value) ? 0 : entry.Value.GetHashCode()));
        //            }
        //            _StateTraceList = new List<ACStateTrace>();
        //            string fileName = String.Format("ACState_{0:yyyyMMdd-HH}.txt", DateTime.Now);
        //            fileName = Path.Combine(Path.GetTempPath(), fileName);
        //            File.AppendAllText(fileName, sb.ToString());
        //        }
        //    }
        //}

        //public bool HasLogACStateEntries
        //{
        //    get
        //    {
        //        lock (_LockStateTraceList)
        //        {
        //            return _StateTraceList != null && _StateTraceList.Any();
        //        }
        //    }
        //}

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
            if (_PropertyValue == null || ACRef == null || ACRef.ValueT == null)
                return;
            if (!PropertyInfo.IsBroadcast)
                return;
            if (!IsProxy)
            {
                if (!_WasInitializedFromInvoker && invokerInfo != null)
                    _WasInitializedFromInvoker = true;

                ACPropertyValueEvent<T> newRequest;
                // Falls Änderungsereignis von außen stattgefunden hat und getter-/Setter-Method exisitert
                if (_Current_T_EventArgsForPropertyAccessor != null)
                    newRequest = new ACPropertyValueEvent<T>(_Current_T_EventArgsForPropertyAccessor, EventTypes.Response,
                                                                                    EventRaiser.Source, ACRef.ValueT, this.ACType, invokerInfo);
                else
                    newRequest = new ACPropertyValueEvent<T>(EventTypes.ValueChangedInSource,
                                                                                    EventRaiser.Source,
                                                                                    this.ACRef.ValueT, this.ACType, invokerInfo);

                newRequest.ChangeValue(this, newValue);
                newRequest.ForceBroadcast = forceSend;

                // Callback Set-Method von ACObject
                if (SetMethod != null)
                    SetMethod(newRequest);
                else if (NetSetMethodOfScript != null)
                    NetSetMethodOfScript(newRequest, this);

                if (newRequest.Handled)
                {
                    //if (LogACState && IsACStateProperty)
                    //{
                    //    Database.Root.Messages.LogDebug(this.GetACUrlComponent(), "ChangeValueRequest(1)",
                    //        String.Format("Not changed to ACState-Value: {0} because Handled-Bit was set.", newValue));
                    //}
                    return;
                }

                // Falls Wert nicht geändert
                if (!forceSend && (_PropertyValue.CompareTo(newValue, this) == 0))
                {
                    //if (LogACState && IsACStateProperty)
                    //{
                    //    Database.Root.Messages.LogDebug(this.GetACUrlComponent(), "ChangeValueRequest(2)",
                    //        String.Format("ACState didn't change ACState-Value: {0}.", newValue));
                    //}
                    return;
                }

                //if (LogACState && IsACStateProperty)
                //    LogACStateChange(10, ACStateConst.ToString((ACStateEnum)(object)newValue), (ACStateEnum)(object)newValue == ACStateEnum.SMStarting);

                SetSourceValue(newValue);

                //if (LogACState && IsACStateProperty)
                //    LogACStateChange(20, ACStateConst.ToString((ACStateEnum)(object)newValue), false);
                try
                {
                    OnValueUpdatedOnReceival(newRequest, ACPropertyChangedPhase.BeforeBroadcast);

                    //if (LogACState && IsACStateProperty)
                    //    LogACStateChange(30, ACStateConst.ToString((ACStateEnum)(object)newValue), false);

                    // Löse PropertyChanged-Event aus
                    if (_LiveLog != null)
                        _LiveLog.AddValue(this.Value);
                    OnPropertyChanged(Const.ValueT);
                    OnPropertyChanged(Const.Value);

                    //if (LogACState && IsACStateProperty)
                    //    LogACStateChange(31, ACStateConst.ToString((ACStateEnum)(object)newValue), false);

                    // Benachrichtige alle gebundenen Targets
                    foreach (IACPropertyNetTarget target in _Targets)
                    {
                        target.OnValueEventReceivedFromSource(newRequest);
                    }

                    //if (LogACState && IsACStateProperty)
                    //    LogACStateChange(32, ACStateConst.ToString((ACStateEnum)(object)newValue), false);

                    Persist();

                    //if (LogACState && IsACStateProperty)
                    //    LogACStateChange(33, ACStateConst.ToString((ACStateEnum)(object)newValue), false);

                    // Benachrichtige Proxies
                    BroadcastToProxies(newRequest);

                    //if (LogACState && IsACStateProperty)
                    //    LogACStateChange(40, ACStateConst.ToString((ACStateEnum)(object)newValue), false);

                    OnValueUpdatedOnReceival(newRequest, ACPropertyChangedPhase.AfterBroadcast);

                    //if (LogACState && IsACStateProperty)
                    //    LogACStateChange(50, String.Format("{0} < {1}", ACStateConst.ToString((ACStateEnum)(object)this.ValueT), ACStateConst.ToString((ACStateEnum)(object)newValue)));

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
                    if (writer == null)
                        writer = Database.Root;
                    if (writer != null)
                    {
                        writer.Messages.LogException("\\", "ACPropertyNetSource.ChangeValueRequest(1)", message1);
                        writer.Messages.LogException("\\", "ACPropertyNetSource.ChangeValueRequest(2)", message2);
                        if (!String.IsNullOrEmpty(e.StackTrace))
                            writer.Messages.LogException("\\", "ACPropertyNetSource.ChangeValueRequest(3)", e.StackTrace);
                    }
                    return;
                    //throw e;
                }
            }
            else // Falls Property im Proxy-Objekt
            {
                // Falls Wert nicht geändert
                if (_PropertyValue.CompareTo(newValue, this) == 0)
                    return;

                ACPropertyValueEvent<T> newRequest = new ACPropertyValueEvent<T>(EventTypes.ValueChangedInSource,
                                                                                    EventRaiser.Proxy,
                                                                                    this.ACRef.ValueT, this.ACType, invokerInfo);
                newRequest.ChangeValue(this, newValue);

                using (ACMonitor.Lock(this._20015_LockValue))
                {
                    // Setze aktuelle Anfrage
                    _PropertyValueRequest = newRequest;
                }

                // Sende an Real-Source
                SendToReal(newRequest);
            }
        }


        /// <summary>
        /// Methode wird aufgerufen sobald ein Änderungsrequest von Außen angekommen ist
        /// - Netzwerk/WCF-Dienst durch Abarbeitung der PropertyNetValueEvent-Queue
        /// - intern durch Fall-Unterscheidung in Methode ChangeValueRequest
        /// </summary>
        /// <param name="eventArgs"></param>
        internal override void OnValueEventReceived(ACPropertyValueEvent<T> eventArgs)
        {
            //base.OnValueEventReceived(eventArgs);
            if (eventArgs == null)
                return;

            if (!IsProxy)
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
                        // Zurueckspringen, da durch PropertyChanged-Aufruf in Setter-Methode ein neuer Broadcast-Request generiert wurde
                        _Current_T_EventArgsForPropertyAccessor = null;
                        return;
                    }
                }

                // Falls Abgewiesen, Rückmeldung an Aufrufer
                if (eventArgs.Handled)
                {
                    ACPropertyValueEvent<T> response = new ACPropertyValueEvent<T>(eventArgs, EventTypes.RefusedRequest,
                                                                                    EventRaiser.Source, ACRef.ValueT, this.ACType);
                    response.ChangeValue(this, ValueTUnrequested);
                    BroadcastToProxies(response);
                    return;
                }
                // Sonst von Assembly-Implementierung nicht abgewiesen
                else
                {
                    int valueChanged = _PropertyValue.CompareTo(eventArgs.Value, this);
                    if (valueChanged != 0)
                    {
                        // Ändere Wert ab
                        SetSourceValue(eventArgs.Value);

                        //if (LogACState && IsACStateProperty)
                        //    Database.Root.Messages.LogDebug(this.GetACUrlComponent(), "OnValueEventReceived()", 
                        //        String.Format("Sender: {0}, EventType: {1}, eventArgs: {2}, invokerInfo: {3} ", 
                        //        eventArgs.Sender, eventArgs.EventType, eventArgs.Value, eventArgs.InvokerInfo == null ? "null" : eventArgs.InvokerInfo.ToString()));

                        // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
                        OnValueUpdatedOnReceival(eventArgs, ACPropertyChangedPhase.BeforeBroadcast);

                        // Löse PropertyChanged-Event aus
                        if (_LiveLog != null)
                            _LiveLog.AddValue(this.Value);
                        OnPropertyChanged(Const.ValueT);
                        OnPropertyChanged(Const.Value);
                        Persist();
                    }
                    else if (eventArgs.ForceBroadcast)
                    {
                        // Löse OnSourceValueUpdatedOnReceival-Event aus, damit z.B. OPC-DAItems zum Versand angetriggert werden
                        OnValueUpdatedOnReceival(eventArgs, ACPropertyChangedPhase.BeforeBroadcast);
                    }

                    ACPropertyValueEvent<T> response = new ACPropertyValueEvent<T>(eventArgs, EventTypes.Response,
                                                                                    EventRaiser.Source, ACRef.ValueT,this.ACType);
                    response.ChangeValue(this, ValueTUnrequested);

                    // Falls Wert  geändert
                    if (valueChanged != 0 || eventArgs.ForceBroadcast)
                    {
                        // Benachrichtige alle gebundenen Targets
                        foreach (IACPropertyNetTarget target in _Targets)
                        {
                            target.OnValueEventReceivedFromSource(response);
                        }
                    }

                    // Benachrichtige Proxies
                    BroadcastToProxies(response);
                    if (valueChanged != 0 || eventArgs.ForceBroadcast)
                        OnValueUpdatedOnReceival(eventArgs, ACPropertyChangedPhase.AfterBroadcast);
                }
            }
            else // IsProxy
            {
                // Falls Aufruf vom Realen Source-Objekt
                if (eventArgs.Sender == EventRaiser.Source)
                {
                    // Setze aktuellen PropertyValue
                    SetSourceValue(eventArgs.Value);

                    // Verwalte interne Anfrage
                    ManagePropertyValueRequestOnReceival(eventArgs);

                    // Benachrichtige Targets
                    foreach (IACPropertyNetTarget target in _Targets)
                    {
                        target.OnValueEventReceivedFromSource(eventArgs);
                    }
                }
                // Sonst Aufruf vom Target
                else
                {
                    // Umverpackung
                    ACPropertyValueEvent<T> delegateEvent = new ACPropertyValueEvent<T>(eventArgs, eventArgs.EventType,
                                                                                    EventRaiser.Target,
                                                                                    ACRef.ValueT, this.ACType);
                    delegateEvent.ChangeValue(this, eventArgs.Value);
                    delegateEvent.Message = eventArgs.Message;
                    // Leite weiter an Reales-Objekt
                    SendToReal(delegateEvent);
                }
            }
        }

        protected override void SendToReal(ACPropertyValueEvent<T> eventArgs)
        {
            base.SendToReal(eventArgs);
        }

        protected override void BroadcastToProxies(ACPropertyValueEvent<T> eventArgs)
        {
            base.BroadcastToProxies(eventArgs);
        }
#endregion

#region EventHandling Methods
#endregion
    }
}
