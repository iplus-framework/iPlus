using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACSession'}de{'ACSession'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class ACSession : PAClassAlarmingBase, IACCommSession
    {
        #region c´tors
        public ACSession(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _EventSubscr = new ACPointEventSubscr(this, "EventSubscr", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _ConnectedAlarmChanged = PAAlarmChangeState.NoChange;
            if (!base.ACInit(startChildMode))
                return false;
            IsConnected.PropertyChanged += IsConnected_PropertyChanged;
            EventSubscr.SubscribeEvent(Root, "ACPostInitEvent", EventCallback);

            foreach (IACObject child in this.ACComponentChilds)
            {
                if (child is ACSubscription)
                {
                    ACSubscription acSubscription = child as ACSubscription;
                    acSubscription.PropertyChanged += OnSubscription_PropertyChanged;
                }
            }

            return true;
        }

        public override bool ACPostInit()
        {
            InitSession();
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            foreach (IACComponent child in this.ACComponentChilds)
            {
                if (child is ACSubscription)
                {
                    ACSubscription acSubscription = child as ACSubscription;
                    acSubscription.PropertyChanged -= OnSubscription_PropertyChanged;
                }
            }
            if (!DeInitSession())
                return false;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties

        public ACService CommService
        {
            get
            {
                return ParentACComponent as ACService;
            }
        }

        [ACPropertyBindingSource(200, "Read from PLC", "en{'Connection Alarm'}de{'Connection Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsConnectedAlarm { get; set; }

        [ACPropertyBindingSource]
        public IACContainerTNet<bool> IsConnected { get; set; }
        public void OnSetIsConnected(IACPropertyNetValueEvent valueEvent)
        {
            bool newIsConnectedValue = (valueEvent as ACPropertyValueEvent<bool>).Value;
            if (newIsConnectedValue != IsConnected.ValueT)
            {
                if (!newIsConnectedValue)
                {
                    _ConnectedAlarmChanged = PAAlarmChangeState.NewAlarmOccurred;
                    if (IsConnected != null)
                        IsConnectedAlarm.ValueT = PANotifyState.AlarmOrFault;
                }
                else
                {
                    _ConnectedAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
                    if(IsConnected != null)
                        IsConnectedAlarm.ValueT = PANotifyState.Off;
                }
            }
        }

        protected PAAlarmChangeState _ConnectedAlarmChanged = PAAlarmChangeState.NoChange;
        protected virtual void IsConnected_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Const.ValueT)
            {
                if (_ConnectedAlarmChanged != PAAlarmChangeState.NoChange)
                {
                    if (_ConnectedAlarmChanged == PAAlarmChangeState.NewAlarmOccurred)
                    {
                        OnNewAlarmOccurred(IsConnectedAlarm);
                        DeactivateAutoBackup();
                    }
                    else
                        OnAlarmDisappeared(IsConnectedAlarm);
                    _ConnectedAlarmChanged = PAAlarmChangeState.NoChange;
                }
            }
        }

        protected void DeactivateAutoBackup()
        {
            foreach (IACComponent child in this.ACComponentChilds)
            {
                ACSubscription subscription = child as ACSubscription;
                if (subscription != null)
                    subscription.ConnLostBackupIsOff = true;
            }
        }

        private bool _IsReadyForWriting = false;
        /// <summary>
        /// Signals if all Items are read the first time
        /// </summary>
        [ACPropertyBindingSource()]
        public bool IsReadyForWriting
        {
            get
            {
                return _IsReadyForWriting;
            }
            set
            {
                _IsReadyForWriting = value;
                OnPropertyChanged("IsReadyForWriting");
            }
        }

        [ACPropertyInfo(true, 9999, DefaultValue = true)]
        public bool AutoReconnect
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 9999, DefaultValue = false)]
        public bool WithVerification
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 9999, DefaultValue = false)]
        public bool ConnectionDisabled
        {
            get;
            set;
        }
        #endregion

        #region Methods Range: 200
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(InitSession):
                    result = InitSession();
                    return true;
                case nameof(IsEnabledInitSession):
                    result = IsEnabledInitSession();
                    return true;
                case nameof(DeInitSession):
                    result = DeInitSession();
                    return true;
                case nameof(IsEnabledDeInitSession):
                    result = IsEnabledDeInitSession();
                    return true;
                case nameof(Connect):
                    result = Connect();
                    return true;
                case nameof(IsEnabledConnect):
                    result = IsEnabledConnect();
                    return true;
                case nameof(DisConnect):
                    result = DisConnect();
                    return true;
                case nameof(IsEnabledDisConnect):
                    result = IsEnabledDisConnect();
                    return true;
                case nameof(SendObject):
                    result = SendObject(acParameter[0], acParameter[1], Convert.ToInt32(acParameter[2]), Convert.ToInt32(acParameter[3]), acParameter[4] != null ? Convert.ToInt32(acParameter[4]) : default(Int32?), acParameter.Count() > 5 ? acParameter[5] : null);
                    return true;
                case nameof(ReadObject):
                    result = ReadObject(acParameter[0], Convert.ToInt32(acParameter[1]), Convert.ToInt32(acParameter[2]), acParameter.Count() > 3 ? acParameter[3] : null);
                    return true;
                case nameof(EventCallback):
                    EventCallback((gip.core.datamodel.IACPointNetBase)acParameter[0], (gip.core.datamodel.ACEventArgs)acParameter[1], (gip.core.datamodel.IACObject)acParameter[2]);
                    return true;
                case nameof(ActivateAutoBackup):
                    ActivateAutoBackup();
                    return true;
                case nameof(IsEnabledActivateAutoBackup):
                    result = IsEnabledActivateAutoBackup();
                    return true;
                case nameof(DeActivateAutoBackup):
                    DeActivateAutoBackup();
                    return true;
                case nameof(IsEnabledDeActivateAutoBackup):
                    result = IsEnabledDeActivateAutoBackup();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        [ACMethodInfo("xxx", "en{'Init'}de{'Initialisiere'}", 9999)]
        public abstract bool InitSession();
        public abstract bool IsEnabledInitSession();


        [ACMethodInfo("xxx", "en{'Deinit'}de{'Deinitialisiere'}", 9999)]
        public abstract bool DeInitSession();
        public abstract bool IsEnabledDeInitSession();


        [ACMethodInteraction("xxx", "en{'Connect'}de{'Verbinden'}", 200, true)]
        public abstract bool Connect();
        public virtual bool IsEnabledConnect()
        {
            return ACOperationMode == ACOperationModes.Live && !ConnectionDisabled;
        }


        [ACMethodInteraction("xxx", "en{'Disconnect'}de{'Verbindung trennen'}", 201, true)]
        public abstract bool DisConnect();
        public abstract bool IsEnabledDisConnect();

        protected abstract void StartReconnection();


        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (IsConnectedAlarm.ValueT == PANotifyState.AlarmOrFault)
            {
                IsConnectedAlarm.ValueT = PANotifyState.Off;
                _ConnectedAlarmChanged = PAAlarmChangeState.NoChange;
                OnAlarmDisappeared(IsConnectedAlarm);
            }
            base.AcknowledgeAlarms();
        }

        /// <summary>
        /// Sends the provided complex Object to the device over the underlaying driver
        /// This method searches for a child Component which is a derivation of ACComplexObjConverter
        /// If nothing is found a virtual method will be called. The Name of the virtual method must have the following structure:
        /// "Send_" + typeName
        /// if the passed complexObj is a ACMethod the ACIdentifer will be used as typeName
        /// else the FullName of the .NET-Type will be used. The namespace-separating points will be replaced by a underscore "_"
        /// e.g. "gip.core.TestObj" =&gt; "gip_gore_TestObj" therefore the Methodname must be "Send_gip_gore_TestObj";
        /// </summary>
        /// <param name="complexObj">The complexObj can be wether a ACMethod or any serializable Object.</param>
        /// <param name="dbNo">Datablock-Number</param>
        /// <param name="offset">Offset in Datablock</param>
        /// <param name="miscParams"></param>
        /// <returns>true if succeed</returns>
        [ACMethodInfo("Exchange", "en{'Send complex object'}de{'Sende komplexes objekt'}", 202)]
        public virtual bool SendObject(object complexObj, object prevComplexObj, int dbNo, int offset, int? routeOffset, object miscParams)
        {
            if (complexObj == null)
                return false;
            string typeName = complexObj.GetType().FullName;
            ACMethod acMethod = complexObj as ACMethod;
            if (acMethod != null)
                typeName = acMethod.ACIdentifier;
            bool succ = false;
            ACSessionObjSerializer converter = FindChildComponents<ACSessionObjSerializer>(c => c is ACSessionObjSerializer 
                                                                                              && (c as ACSessionObjSerializer).IsSerializerFor(typeName))
                                                                                            .FirstOrDefault();
            if (converter != null)
            {
                succ = converter.SendObject(complexObj, prevComplexObj, dbNo, offset, routeOffset, miscParams);
            }
            else
            {
                string virtualMethodName = "Send_" + typeName.Replace('.','_');
                object result = ExecuteMethod(virtualMethodName, complexObj, prevComplexObj, dbNo, offset, routeOffset, miscParams);
                if (result == null)
                    succ = false;
                else
                    succ = (bool) result;
            }

            if (succ && WithVerification)
            {
                complexObj = ReadObject(complexObj, dbNo, offset, miscParams);
                if (complexObj == null)
                    succ = false;
            }
            return succ;
        }

        /// <summary>
        /// Reads from the device over the underlaying driver and returns the complex object
        /// This method searches for a child Component which is a derivation of ACComplexObjConverter
        /// If nothing is found a virtual method will be called. The Name of the virtual method must have the following structure:
        /// "Read_" + typeName
        /// if the passed complexObj is a ACMethod the ACIdentifer will be used as typeName
        /// else the FullName of the .NET-Type will be used. The namespace-separating points will be replaced by a underscore "_"
        /// e.g. "gip.core.TestObj" =&gt; "gip_core_TestObj" therefore the Methodname must be "Read_gip_core_TestObj";
        /// </summary>
        /// <param name="complexObj">The complexObj can be wether a ACMethod or any serializable Object. The complexObject should be empty</param>
        /// <param name="dbNo">Datablock-Number</param>
        /// <param name="offset">Offset in Datablock</param>
        /// <param name="miscParams"></param>
        /// <returns>The passed complexObj with filled out properties. If read error the result is null.</returns>
        [ACMethodInfo("Exchange", "en{'Read complex object'}de{'Lese komplexes objekt'}", 203)]
        public virtual object ReadObject(object complexObj, int dbNo, int offset, object miscParams)
        {
            if (complexObj == null)
                return false;
            string typeName = complexObj.GetType().FullName;
            ACMethod acMethod = complexObj as ACMethod;
            if (acMethod != null)
                typeName = acMethod.ACIdentifier;
            ACSessionObjSerializer converter = FindChildComponents<ACSessionObjSerializer>(c => c is ACSessionObjSerializer 
                                                                                                    && (c as ACSessionObjSerializer).IsSerializerFor(typeName))
                                                                                            .FirstOrDefault();
            if (converter != null)
            {
                return converter.ReadObject(complexObj, dbNo, offset, miscParams);
            }
            else
            {
                string virtualMethodName = "Read_" + typeName.Replace('.', '_');
                return ExecuteMethod(virtualMethodName, complexObj, dbNo, offset, miscParams);
            }
        }

        public static bool IsLocalConnection(string ipAddress)
        {
            return String.IsNullOrEmpty(ipAddress)
                    || ipAddress == "localhost"
                    || ipAddress == "127.0.0.1";
        }
        #endregion

        #region Event-Handler
        ACPointEventSubscr _EventSubscr;
        [ACPropertyEventPointSubscr(9999, false)]
        public ACPointEventSubscr EventSubscr
        {
            get
            {
                return _EventSubscr;
            }
        }

        [ACMethodInfo("Function", "en{'EventCallback'}de{'EventCallback'}", 9999)]
        public void EventCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                if (sender.ACIdentifier == "ACPostInitEvent")
                {
                    // Erste wenn alle Objekte in allen Projekten intanziert worden sind, dann Connect
                    if (!Connect() && AutoReconnect)
                        StartReconnection();
                    EventSubscr.UnSubscribeAllEvents(Root);
                }
            }
        }

        protected void OnSubscription_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsReadyForWriting")
            {
                bool bAllSubscrReady = true;
                foreach (IACComponent child in this.ACComponentChilds)
                {
                    if (child is ACSubscription)
                    {
                        ACSubscription acSubscription = child as ACSubscription;
                        if (!acSubscription.IsReadyForWriting)
                        {
                            bAllSubscrReady = false;
                            break;
                        }
                    }
                }

                if (IsReadyForWriting != bAllSubscrReady)
                {
                    IsReadyForWriting = bAllSubscrReady;
                }
            }
        }
        #endregion

        #region Automatic Backup
        [ACMethodInteraction("xxx", "en{'Reactivate automatic Backup'}de{'Reaktiviere automatische Sicherung'}", 50, true)]
        public void ActivateAutoBackup()
        {
            FindChildComponents<ACSubscription>(c => c is ACSubscription && (c as ACSubscription).IsEnabledActivateAutoBackup())
                                                .ForEach(c => c.ActivateAutoBackup());
        }

        public bool IsEnabledActivateAutoBackup()
        {
            return FindChildComponents<ACSubscription>(c => c is ACSubscription).Any(c => c.IsEnabledActivateAutoBackup());
        }

        [ACMethodInteraction("xxx", "en{'Deactivate automatic Backup'}de{'Deaktiviere automatische Sicherung'}", 51, true)]
        public void DeActivateAutoBackup()
        {
            FindChildComponents<ACSubscription>(c => c is ACSubscription && (c as ACSubscription).IsEnabledDeActivateAutoBackup())
                                                .ForEach(c => c.DeActivateAutoBackup());
        }

        public bool IsEnabledDeActivateAutoBackup()
        {
            return FindChildComponents<ACSubscription>(c => c is ACSubscription).Any(c => c.IsEnabledDeActivateAutoBackup());
        }
        #endregion
    }
}
