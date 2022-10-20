using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Softing.OPCToolbox.Client;
using Softing.OPCToolbox;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.communication
{
    public class OPCDataChangedEventArgs : EventArgs
    {
        public OPCDataChangedEventArgs(OPCClientSoftingDaItem item, ValueQT qtItem)
        {
            DaItem = item;
            QTItem = qtItem;
        }

        public OPCClientSoftingDaItem DaItem
        {
            get;
            private set;
        }

        public ValueQT QTItem
        {
            get;
            private set;
        }

    }

    public delegate void OPCDataChangedEventHandler(object sender, OPCDataChangedEventArgs e);


    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPCClientSACSubscr'}de{'OPCClientSACSubscr'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class OPCClientSACSubscr : OPCClientACSubscr
    {

        private static Dictionary<string, ACEventArgs> _SVirtualEventArgs;

#region Properties

        private OPCClientSoftingDaSubscr _DaSubscription = null;
        public OPCClientSoftingDaSubscr DaSubscription
        {
            get
            {
                return _DaSubscription;
            }
        }

        public OPCClientSoftingDaSession DaSession
        {
            get
            {
                if (ParentACComponent == null)
                    return null;
                if (ParentACComponent is OPCClientSACSession)
                    return (ParentACComponent as OPCClientSACSession).DaSession;
                return null;
            }
        }

        private List<IACPropertyNetSource> _RaiseChangedEventFor = null;

        private int _ItemsCount;
        private int _ItemsRead;

        public static new Dictionary<string, ACEventArgs> SVirtualEventArgs
        {
            get { return _SVirtualEventArgs; }
        }

        public override Dictionary<string, ACEventArgs> VirtualEventArgs
        {
            get
            {
                return SVirtualEventArgs;
            }
        }

        #endregion

        #region Constructors

        static OPCClientSACSubscr()
        {
            ACEventArgs TMP;

            _SVirtualEventArgs = new Dictionary<string,ACEventArgs>(OPCClientACSubscr.SVirtualEventArgs, StringComparer.OrdinalIgnoreCase);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue(Const.ACIdentifierPrefix, typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue("TimeStampValueQT", typeof(DateTime), null, Global.ParamOption.Required));
            TMP.Add(new ACValue("EnumQualityValueQT", typeof(int), null, Global.ParamOption.Optional));
            TMP.Add(new ACValue("Data", typeof(object), null, Global.ParamOption.Optional));
            _SVirtualEventArgs.Add("DataChangedEvent", TMP);
        }

        public OPCClientSACSubscr(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACDeInit(deleteACClassTask);
            _DaSubscription = null;
            return result;
        }

#endregion

#region Methods

        public override bool InitSubscription()
        {
            Messages.LogDebug(this.GetACUrl(), "OPCClientSACSubscr.InitSubscription(1)", "");
            if (_DaSubscription != null)
                return true;
            if (DaSession == null)
                return false;
            if (ACOperationMode != ACOperationModes.Live)
                return true;

            _DaSubscription = new OPCClientSoftingDaSubscr((uint)RequiredUpdateRate, DaSession);
            Messages.LogDebug(this.GetACUrl(), "OPCClientSACSubscr.InitSubscription(2)", "");

            using (ACMonitor.Lock(LockMemberList_20020))
            {
                if (OPCProperties != null)
                {
                    Messages.LogDebug(this.GetACUrl(), "OPCClientSACSubscr.InitSubscription(3)", String.Format("Count OPCProperties: {0}", OPCProperties.Count()));
                    foreach (IACPropertyNetServer opcProperty in OPCProperties)
                    {
                        // TODO: Abfrage zweite ACClass-Spalte
                        OPCItemConfig opcConfig = (OPCItemConfig)(opcProperty.ACType as ACClassProperty)["OPCItemConfig"];
                        if ((opcConfig != null) && !String.IsNullOrEmpty(opcConfig.OPCAddr))
                        {
                            //Messages.LogDebug(this.GetACUrl(), "OPCClientSACSubscr.InitSubscription(4)", String.Format("Addr: {0}", opcConfig.OPCAddr));
                            OPCClientSoftingDaItem daItem = new OPCClientSoftingDaItem(opcProperty, opcConfig.OPCAddr, _DaSubscription);
                            _ItemsCount++;
                            IACPropertyNetSource opcSourceProperty = opcProperty as IACPropertyNetSource;
                            if (opcSourceProperty != null)
                                opcSourceProperty.AdditionalRefs.Add(daItem);
                        }
                    }
                }
            }
            Messages.LogDebug(this.GetACUrl(), "OPCClientSACSubscr.InitSubscription(5)", "");
            _DaSubscription.DataChanged += new DataChangedEventHandler(OnDataChanged);
            _DaSubscription.StateChangeCompleted += OnStateChanged;
            if (IsWriteOnly)
                IsReadyForWriting = true;

            return true;
        }

        public override bool IsEnabledInitSubscription()
        {
            if (_DaSubscription != null)
                return false;
            if (DaSession == null)
                return false;
            return true;
        }

        public override bool DeInitSubscription()
        {
            if (_DaSubscription == null)
                return true;

            // 1. Disconnect
            if (!DisConnect())
                return false;

            // 2. Remove Events
            _DaSubscription.DataChanged -= OnDataChanged;
            _DaSubscription.StateChangeCompleted -= OnStateChanged;

            // 3. DeInit Items
            foreach (DaItem daItem in _DaSubscription.ItemList)
            {
                (daItem as OPCClientSoftingDaItem).DeInitOPC();
            }

            // 4. Remove Subscription from Session
            if (DaSession != null)
            {
                DaSession.RemoveDaSubscription(_DaSubscription);
                return true;
            }

            _DaSubscription = null;
            return true;
        }

        public override bool IsEnabledDeInitSubscription()
        {
            if (DaSession == null)
                return false;
            if (_DaSubscription == null)
                return false;
            return true;
        }

        public override bool Connect()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return true;
            if (_DaSubscription == null 
                || !InitSubscription())
                return false;
            if (!IsEnabledConnect())
                return false;
            int res = _DaSubscription.Connect(true, true, new ExecutionOptions(EnumExecutionType.ASYNCHRONOUS, 0));
            //int res = _DaSubscription.Connect(true, true, new ExecutionOptions(EnumExecutionType.SYNCHRONOUS, 0));
            if (!ResultCode.SUCCEEDED(res))
                return false;
            return true;
        }

        public override bool IsEnabledConnect()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return true;
            if (DaSession == null 
                || _DaSubscription == null 
                || _DaSubscription.TargetState == EnumObjectState.CONNECTED 
                || _DaSubscription.TargetState == EnumObjectState.ACTIVATED)
                return false;
            return true;
        }

        public override bool DisConnect()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return true;
            if (!IsEnabledDisConnect())
                return false;
            int res = _DaSubscription.Disconnect(new ExecutionOptions(EnumExecutionType.ASYNCHRONOUS, 0));
            if (!ResultCode.SUCCEEDED(res))
                return false;
            return true;
        }

        public override bool IsEnabledDisConnect()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return true;
            if (DaSession == null
                || _DaSubscription == null
                || (   _DaSubscription.TargetState != EnumObjectState.CONNECTED
                    && _DaSubscription.TargetState != EnumObjectState.ACTIVATED))
                return false;
            return true;
        }

        [ACMethodInfo("xxx", "en{'GetTimeStampValueQT'}de{'GetTimeStampValueQT'}", 200, false)]
        public DateTime GetTimeStampValueQT(string propertyACIdentifier)
        {
            return GetTimeStampValueQT(GetPropertyNet(propertyACIdentifier) as IACPropertyNetSource);
        }

        public DateTime GetTimeStampValueQT(IACPropertyNetSource opcProperty)
        {
            OPCClientSoftingDaItem addRef = GetOPCDaItem(opcProperty);
            if (addRef != null)
                return addRef.ValueQT.TimeStamp;
            return DateTime.MinValue;
        }

        [ACMethodInfo("xxx", "en{'GetEnumQualityValueQT'}de{'GetEnumQualityValueQT'}", 201, false)]
        public int GetEnumQualityValueQT(string propertyACIdentifier)
        {
            return GetEnumQualityValueQT(GetPropertyNet(propertyACIdentifier) as IACPropertyNetSource);
        }

        public int GetEnumQualityValueQT(IACPropertyNetSource opcProperty)
        {
            OPCClientSoftingDaItem addRef = GetOPCDaItem(opcProperty);
            if (addRef != null)
            {
                if (addRef.ValueQT == null)
                    return 0;
                return (int)addRef.ValueQT.Quality;
            }
            return 0;
        }

        public OPCClientSoftingDaItem GetOPCDaItem(string propertyACIdentifier)
        {
            return GetOPCDaItem(GetPropertyNet(propertyACIdentifier) as IACPropertyNetSource);
        }

        public OPCClientSoftingDaItem GetOPCDaItem(IACPropertyNetSource opcProperty)
        {
            if (opcProperty != null && opcProperty.AdditionalRefs.Any())
                return opcProperty.AdditionalRefs.FirstOrDefault() as OPCClientSoftingDaItem;
            return null;
        }

        public object GetValueQTData(IACPropertyNetSource opcProperty)
        {
            OPCClientSoftingDaItem addRef = GetOPCDaItem(opcProperty);
            if (addRef == null || addRef.ValueQT == null)
                return null;
            return addRef.ValueQT.Data;
        }

        [ACMethodInfo("xxx", "en{'RaiseDataChangedEventsFor'}de{'RaiseDataChangedEventsFor'}", 202, false)]
        public void RaiseDataChangedEventsFor(string propertyACIdentifier)
        {
            IACPropertyNetSource property = GetPropertyNet(propertyACIdentifier) as IACPropertyNetSource;
            if (property == null)
                return;
            if (_RaiseChangedEventFor == null)
                _RaiseChangedEventFor = new List<IACPropertyNetSource>();
            else
            {
                if (_RaiseChangedEventFor.Contains(property))
                    return;
            }
            _RaiseChangedEventFor.Add(property);
        }

#endregion

#region Event-Handler

        private void OnDataChanged(DaSubscription aDaSubscription, DaItem[] items, ValueQT[] values, int[] results)
        {
            for (int i = 0; i < items.Count(); i++)
            {
                OPCClientSoftingDaItem daItemEx = items[i] as OPCClientSoftingDaItem;
                if (daItemEx.QualitySwitchedToGood())
                {
                    _ItemsRead++;
                }
                ValueQT qtItem = values[i];
                
                OnOPCDataChanged(daItemEx, qtItem);

                // Falls Konfigurationsvariable: (Beschreiben von der SPS nicht erlaubt und Target-Property)
                if (!daItemEx.ACProperty.PropertyInfo.IsInput && daItemEx.ACProperty is IACPropertyNetTarget)
                    continue;

                daItemEx._ReSendLocked = OPCClientSoftingDaItem.ResendLock.Locked;
                if ((daItemEx.ACProperty.Value != null) && daItemEx.ACProperty.Value is ACCustomTypeBase)
                {
                    (daItemEx.ACProperty.Value as ACCustomTypeBase).Value = qtItem.Data;
                }
                else
                    daItemEx.ACProperty.ChangeValueServer(qtItem.Data, daItemEx.ACProperty.ForceBroadcast, qtItem);
                //if (daItemEx._ReSendLocked == OPCClientSoftingDaItem.ResendLock.ResendDone)
                    //results[i] = 1; // System.Convert.ToInt32(0x80000000); // ERROR_MASK;
                daItemEx._ReSendLocked = OPCClientSoftingDaItem.ResendLock.Unlocked;

                if (_RaiseChangedEventFor != null)
                {
                    IACPropertyNetSource sourceProp = daItemEx.ACProperty as IACPropertyNetSource;
                    if (sourceProp != null)
                    {
                        if (_RaiseChangedEventFor.Contains(sourceProp))
                        {
                            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("DataChangedEvent", VirtualEventArgs);

                            eventArgs.GetACValue(Const.ACIdentifierPrefix).Value = sourceProp.ACIdentifier;
                            eventArgs.GetACValue("TimeStampValueQT").Value = values[i].TimeStamp;
                            eventArgs.GetACValue("EnumQualityValueQT").Value = (int)values[i].Quality;
                            eventArgs.GetACValue("Data").Value = values[i].Data;

                            DataChangedEvent.Raise(eventArgs);
                        }
                    }
                }
            }
            if (!IsReadyForWriting && _ItemsCount == _ItemsRead)
                IsReadyForWriting = true;
        }

        private void OnStateChanged(ObjectSpaceElement obj, EnumObjectState state)
        {
            if (IsConnected != null)
            {
                if ((state == EnumObjectState.CONNECTED) || (state == EnumObjectState.ACTIVATED))
                {
                    IsConnected.ValueT = true;
                }
                else
                    IsConnected.ValueT = false;
            }
        }

        public event OPCDataChangedEventHandler OPCDataChanged;
        internal void OnOPCDataChanged(OPCClientSoftingDaItem item, ValueQT qtItem)
        {
            if (OPCDataChanged != null)
            {
                OPCDataChanged(this, new OPCDataChangedEventArgs(item, qtItem));
            }
        }


#endregion

#region Points

        protected ACPointEvent _DataChangedEvent;
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent DataChangedEvent
        {
            get
            {
                return _DataChangedEvent;
            }
            set
            {
                _DataChangedEvent = value;
            }
        }

#endregion

    }
}
