// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.IO;
using System.Diagnostics;


namespace gip.core.communication
{
    /// <summary>
    /// Exporter for Property-Logs (Root-Node)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Cyclic Export'}de{'Zyklischer Export'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class PAFileCyclicExport : PAJobScheduler
    {
        #region c´tors
        public PAFileCyclicExport(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _DelegateQueue = new ACDelegateQueue(this.GetACUrl());
            }
            _DelegateQueue.StartWorkerThread();
            return result;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            _DelegateQueue.StopWorkerThread();

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _DelegateQueue = null;
            }
            bool result = base.ACDeInit(deleteACClassTask);
            return result;
        }

        #endregion

        #region Properties
        private string _Path;
        [ACPropertyInfo(true, 301, "Configuration", "en{'Path to export'}de{'Exportpfad'}", "", true)]
        public string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
                OnPropertyChanged("Path");
            }
        }

        private string _NetUseArguments;
        private string _NetUseArgsSucc = "";
        [ACPropertyInfo(true, 310, "Configuration", "en{'Net use arguments connect'}de{'Netzlaufwerk argumente verbinden'}", "", true)]
        public string NetUseArguments
        {
            get
            {
                return _NetUseArguments;
            }
            set
            {
                _NetUseArguments = value;
                OnPropertyChanged("NetUseArguments");
            }
        }

        private string _NetUseDeleteArguments;
        [ACPropertyInfo(true, 311, "Configuration", "en{'Net use arguments disconnect'}de{'Netzlaufwerk argumente trennen'}", "", true)]
        public string NetUseDeleteArguments
        {
            get
            {
                return _NetUseDeleteArguments;
            }
            set
            {
                _NetUseDeleteArguments = value;
                OnPropertyChanged("NetUseDeleteArguments");
            }
        }

        
        [ACPropertyBindingSource(305, "Error", "en{'Export Alarm'}de{'Export Alarm'}", "", false, false)]
        public IACContainerTNet<PANotifyState> IsExportingAlarm { get; set; }

        [ACPropertyBindingSource(306, "Error", "en{'Error-text'}de{'Fehlertext'}", "", true, false)]
        public IACContainerTNet<String> ErrorText { get; set; }


        private Boolean _InWorkerThread;
        [ACPropertyInfo(true, 307, "Configuration", "en{'Create own Workerthread'}de{'Erzeuge Arbeitsthread'}", "", true)]
        public Boolean InWorkerThread
        {
            get
            {
                return _InWorkerThread;
            }
            set
            {
                _InWorkerThread = value;
                OnPropertyChanged("InWorkerThread");
            }
        }

        private int _LimitActionsInQueue;
        [ACPropertyInfo(true, 307, "Configuration", "en{'Maximum actions in queue'}de{'Max. Einträge in Warteschlange'}", "", true, DefaultValue=0)]
        public int LimitActionsInQueue
        {
            get
            {
                return _LimitActionsInQueue;
            }
            set
            {
                _LimitActionsInQueue = value;
                OnPropertyChanged("LimitActionsInQueue");
            }
        }

        private ACDelegateQueue _DelegateQueue = null;
        public ACDelegateQueue DelegateQueue
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _DelegateQueue;
                }
            }
        }

        #endregion

        #region Methods

        #region overidden methods
        protected override void OnStartScheduling()
        {
            if (String.IsNullOrEmpty(_NetUseArgsSucc) && !String.IsNullOrEmpty(NetUseArguments))
            {
                try
                {
                    Process p = Process.Start("net.exe", "use " + NetUseArguments);
                    p.WaitForExit();
                    _NetUseArgsSucc = NetUseArguments;
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "PAFileCyclicExportBase.StartExporting(Net use)", e.Message);
                }
            }
        }

        public override bool IsEnabledStartScheduling()
        {
            if (String.IsNullOrEmpty(Path))
                return false;
            return base.IsEnabledStartScheduling();
        }

        protected override void OnStopScheduling()
        {
            if (!String.IsNullOrEmpty(_NetUseArgsSucc) && !String.IsNullOrEmpty(NetUseDeleteArguments) && (_NetUseArgsSucc != NetUseArguments))
            {
                try
                {
                    Process p = Process.Start("net.exe", "use " + NetUseDeleteArguments);
                    p.WaitForExit();
                    _NetUseArgsSucc = "";
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "PAFileCyclicExportBase.StopExporting(Net use)", e.Message);
                }
            }
        }

        public override void AcknowledgeAlarms()
        {
            if (!IsEnabledAcknowledgeAlarms())
                return;
            if (IsExportingAlarm.ValueT == PANotifyState.AlarmOrFault)
            {
                IsExportingAlarm.ValueT = PANotifyState.Off;
                OnAlarmDisappeared(IsExportingAlarm);
            }
            base.AcknowledgeAlarms();
        }

        //protected override void OnNewMsgAlarmLogCreated(MsgAlarmLog newLog)
        //{
        //    if (String.IsNullOrEmpty(newLog.Message))
        //        newLog.Message = ErrorText.ValueT;
        //    base.OnNewMsgAlarmLogCreated(newLog);
        //}

        protected override void RunJob(DateTime now, DateTime lastRun, DateTime nextRun)
        {
            var delegateQueue = DelegateQueue;
            if (InWorkerThread && delegateQueue != null)
            {
                if(LimitActionsInQueue <= 0 || delegateQueue.DelegateQueueCount < LimitActionsInQueue)
                    delegateQueue.Add(() => { DoExport(now, lastRun, nextRun); });
            }
            else
                DoExport(now, lastRun, nextRun);
        }

        protected void DoExport(DateTime now, DateTime lastRun, DateTime nextRun)
        {
            if (this.InitState != ACInitState.Initialized || !this.Root.Initialized)
                return;

            int countSucc = 0;
            foreach (PAFileCyclicGroupBase exportGroup in FindChildComponents<PAFileCyclicGroupBase>(c => c is PAFileCyclicGroupBase))
            {
                Msg msg = exportGroup.DoExport(Path, lastRun, now);
                if (msg != null)
                {
                    //ErrorText.ValueT = msg.Message;
                    //OnNewAlarmOccurred(IsExportingAlarm);
                }
                else
                    countSucc++;
            }
        }
        #endregion
        
        #endregion
    }
}
