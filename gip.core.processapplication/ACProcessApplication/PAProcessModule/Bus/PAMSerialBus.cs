// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Serial Bus'}de{'Serieller Bus'}", Global.ACKinds.TPAProcessModule, Global.ACStorableTypes.Required, false, PWGroup.PWClassName, true)]
    public class PAMSerialBus : PAMBus, IACPAESerialPort
    {
        public PAMSerialBus(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _AsyncRMI = new ACPointAsyncRMI(this, "AsyncRMI", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            if (String.IsNullOrEmpty(PortName))
                PortName = "COM1";
            if (BaudRate <= 0)
                BaudRate = 9600;
            if (DataBits <= 0)
                DataBits = 8;

            return true;
        }

        public override bool ACPostInit()
        {
            if (PollingInterval <= 0)
                PollingInterval = 500;
            if (ACOperationMode == ACOperationModes.Live)
            {
                _ShutdownEvent = new ManualResetEvent(false);
                _PollThread = new ACThread(Poll);
                _PollThread.Name = "ACUrl:" + this.GetACUrl() + ";Poll();";
                _PollThread.Start();
            }

            return base.ACPostInit();
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            if (_PollThread != null)
            {
                if (_ShutdownEvent != null && _ShutdownEvent.SafeWaitHandle != null && !_ShutdownEvent.SafeWaitHandle.IsClosed)
                    _ShutdownEvent.Set();
                if (!_PollThread.Join(5000))
                    _PollThread.Abort();
                _PollThread = null;
            }
            ClosePort();
            return await base.ACDeInit(deleteACClassTask);
        }

        #region Methods

        public override void SendToDeviceOnBus(string DeviceID, object[] parameter)
        {
            // Falls Synchroner Aufruf
            if (AsyncRMI.CurrentAsyncRMI == null)
            {

                using (ACMonitor.Lock(_60000_LockPort))
                {
                    try
                    {
                        if (OpenPort())
                            WriteOnBus(DeviceID, parameter);
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        Messages.LogException("PAMSerialBus", "SendToDeviceOnBus", msg);
                    }
                }
            }
        }

        protected ManualResetEvent _ShutdownEvent;
        ACThread _PollThread;
        private void Poll()
        {
            try
            {
                while (!_ShutdownEvent.WaitOne(50, false))
                {
                    _PollThread.StartReportingExeTime();
                    // Falls Asynchroner Aufruf
                    if (AsyncRMI.CurrentAsyncRMI != null)
                    {
                        if (!AsyncRMI.CurrentAsyncRMI.CallbackIsPending)
                        {
                            bool written = false;

                            using (ACMonitor.Lock(_60000_LockPort))
                            {
                                try
                                {
                                    if (!IsConnected.ValueT)
                                        OpenPort();
                                    if (IsConnected.ValueT)
                                    {
                                        WriteOnBus(AsyncRMI.CurrentAsyncRMI.Parameter.ElementAt(0).ParamAsString, (object[])AsyncRMI.CurrentAsyncRMI.Parameter.ElementAt(1).Value);
                                        written = true;
                                    }
                                }
                                catch (Exception e)
                                {
                                    string msg = e.Message;
                                    if (e.InnerException != null && e.InnerException.Message != null)
                                        msg += " Inner:" + e.InnerException.Message;

                                    Messages.LogException("PAMSerialBus", "Poll", msg);
                                }
                            }
                            if (written)
                                AsyncRMI.InvokeCallbackDelegate(new ACMethodEventArgs(AsyncRMI.CurrentAsyncRMI.RequestID, new ACValueList(), Global.ACMethodResultState.Succeeded));
                        }
                    }
                    else
                    {

                        using (ACMonitor.Lock(_60000_LockPort))
                        {
                            try
                            {
                                if (!IsConnected.ValueT)
                                    OpenPort();
                                if (IsConnected.ValueT)
                                {
                                    ReadOnBus();
                                }
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                Messages.LogException("PAMSerialBus", "Poll(10)", msg);
                            }
                        }
                        if (_ShutdownEvent.WaitOne(PollingInterval, false))
                            break;
                    }
                    _PollThread.StopReportingExeTime();
                }
            }
            catch (ThreadAbortException ex)
            {
                string msg = ex.Message;
                if (ex.InnerException != null && ex.InnerException.Message != null)
                    msg += " Inner:" + ex.InnerException.Message;

                Messages.LogException("PAMSerialBus", "Poll", msg);
            }
        }

        [ACMethodInfo("Function", "en{'Write Bus'}de{'Schreibe auf Bus'}", 9999)]
        public virtual void WriteOnBus(string DeviceID, object[] parameter)
        {
        }

        protected virtual void ReadOnBus()
        {
        }

        [ACMethodInteraction("Function", "en{'Open Port'}de{'Oeffne Port'}", 9999, false)]
        public bool OpenPort()
        {
            if (!IsEnabledOpenPort())
                return false;
            if (this.SerialPort == null)
            {
                _serialPort = new SerialPort(PortName, BaudRate, Parity, DataBits, StopBits);

                if (ReadTimeout > 0)
                    _serialPort.ReadTimeout = ReadTimeout;
                else
                    _serialPort.ReadTimeout = 5000;
                if (WriteTimeout > 0)
                    _serialPort.WriteTimeout = WriteTimeout;
                if (RtsEnable == true)
                    _serialPort.RtsEnable = true;
                if (Handshake != System.IO.Ports.Handshake.None)
                    _serialPort.Handshake = Handshake;
            }

            try
            {
                if (!this.SerialPort.IsOpen)
                    this.SerialPort.Open();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                Messages.LogException("PAMSerialBus", "OpenPort", msg);
            }
            if (IsConnected.ValueT != this.SerialPort.IsOpen)
                IsConnected.ValueT = this.SerialPort.IsOpen;
            return IsConnected.ValueT;
        }

        public bool IsEnabledOpenPort()
        {
            if (!BusEnabled || (ACOperationMode != ACOperationModes.Live))
                return false;
            if (this.SerialPort != null && this.SerialPort.IsOpen)
                return false;
            return true;
        }

        [ACMethodInteraction("Function", "en{'Close Port'}de{'Schliesse Port'}", 9999, false)]
        public void ClosePort()
        {
            if (!IsEnabledClosePort())
                return;
            if (this.SerialPort != null)
            {
                if (this.SerialPort.IsOpen)
                {
                    this.SerialPort.Close();
                    IsConnected.ValueT = false;
                }
                _serialPort = null;
            }
        }

        public bool IsEnabledClosePort()
        {
            if (ACOperationMode != ACOperationModes.Live)
                return false;
            if (this.SerialPort == null || !this.SerialPort.IsOpen)
                return false;
            return true;
        }
#endregion


#region Properties

#region Configuration
        [ACPropertyInfo(true, 200, DefaultValue = 500)]
        public int PollingInterval { get; set; }

        [ACPropertyInfo(true, 201, DefaultValue = false)]
        public bool BusEnabled { get; set; }

        private readonly ACMonitorObject _60000_LockPort = new ACMonitorObject(60000);
#endregion

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
                    IsConnectedAlarm.ValueT = PANotifyState.AlarmOrFault;
                }
                else
                {
                    _ConnectedAlarmChanged = PAAlarmChangeState.AlarmDisappeared;
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
                        OnNewAlarmOccurred(IsConnectedAlarm);
                    else
                        OnAlarmDisappeared(IsConnectedAlarm);
                    _ConnectedAlarmChanged = PAAlarmChangeState.NoChange;
                }
            }
        }

#endregion

#region Points and Events
        ACPointAsyncRMI _AsyncRMI;
        [ACPropertyAsyncMethodPoint(9999)]
        public ACPointAsyncRMI AsyncRMI
        {
            get
            {
                return _AsyncRMI;
            }
        }

        public void OnSetAsyncRMI(IACPointNetBase point)
        {
            if (point != null)
            {
                // Falls Bus nicht Aktiviert, verwerfe alle Anfragen
                if (!BusEnabled)
                {
                    if (point is IACPointAsyncRMI)
                    {
                        IACPointAsyncRMI mappingPoint = point as IACPointAsyncRMI;
                        if (mappingPoint.ConnectionListLocal != null)
                        {
                            foreach (ACPointAsyncRMIWrap<ACComponent> item in mappingPoint.ConnectionListLocal)
                            {
                                // TODO:
                                item.State = PointProcessingState.Rejected;
                            }
                        }
                    }
                }
            }
        }

#endregion

#region IACPAESerialPort Member

        protected SerialPort _serialPort = null;
        [ACPropertyInfo(9999)]
        public SerialPort SerialPort
        {
            get { return _serialPort; }
        }

        /// <summary>
        /// TODO: Configuration-Property
        /// </summary>
        [ACPropertyInfo(true, 210, DefaultValue = "COM1")]
        public string PortName { get; set; }

        /// <summary>
        /// TODO: Configuration-Property
        /// </summary>
        [ACPropertyInfo(true, 211, DefaultValue = 9600)]
        public int BaudRate { get; set; }

        /// <summary>
        /// TODO: Configuration-Property
        /// </summary>
        [ACPropertyInfo(true, 212)]
        public Parity Parity { get; set; }

        /// <summary>
        /// TODO: Configuration-Property
        /// </summary>
        [ACPropertyInfo(true, 213, DefaultValue = 8)]
        public int DataBits { get; set; }

        /// <summary>
        /// TODO: Configuration-Property
        /// </summary>
        [ACPropertyInfo(true, 214)]
        public StopBits StopBits { get; set; }

        /// <summary>
        /// TODO: Configuration-Property
        /// </summary>
        [ACPropertyInfo(true, 215, DefaultValue = -1)]
        public int ReadTimeout { get; set; }

        /// <summary>
        /// TODO: Configuration-Property
        /// </summary>
        [ACPropertyInfo(true, 216, DefaultValue = -1)]
        public int WriteTimeout { get; set; }

        /// <summary>
        /// TODO: Configuration-Property
        /// </summary>
        [ACPropertyInfo(true, 217, DefaultValue = false)]
        public bool RtsEnable { get; set; }

        /// <summary>
        /// TODO: Configuration-Property
        /// </summary>
        [ACPropertyInfo(true, 218)]
        public Handshake Handshake { get; set; }

#endregion

    }
}
