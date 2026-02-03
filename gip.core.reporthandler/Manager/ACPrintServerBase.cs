// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using gip.core.autocomponent;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPrintServerBase'}de{'ACPrintServerBase'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, "", false)]
    public abstract class ACPrintServerBase : PAClassAlarmingBase
    {

        #region c´tors
        public const string MN_Print = nameof(Print);
        public const string MN_PrintByACUrl = nameof(PrintByACUrl);

        public ACPrintServerBase(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _IPAddress = new ACPropertyConfigValue<string>(this, nameof(IPAddress), "");
            _Port = new ACPropertyConfigValue<int>(this, nameof(Port), 0);
            _SendTimeout = new ACPropertyConfigValue<int>(this, nameof(SendTimeout), 0);
            _ReceiveTimeout = new ACPropertyConfigValue<int>(this, nameof(ReceiveTimeout), 0);
            _PrintTries = new ACPropertyConfigValue<int>(this, nameof(PrintTries), 1);
            _CodePage = new ACPropertyConfigValue<int>(this, nameof(CodePage), 0);
            _DumpToTempFolder = new ACPropertyConfigValue<bool>(this, nameof(DumpToTempFolder), false);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _DelegateQueue = new ACDispatchedDelegateQueue(GetACUrl());
            }
            _DelegateQueue.StartWorkerThreadSTA();
            //_DelegateQueue.StartWorkerThread();

            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            _DelegateQueue.StopWorkerThread();
            using (ACMonitor.Lock(_20015_LockValue))
            {
                _DelegateQueue = null;
            }

            return await base.ACDeInit(deleteACClassTask);
        }

        public override bool ACPostInit()
        {
            bool baseReturn = base.ACPostInit();

            _ = IPAddress;
            _ = Port;
            _ = SendTimeout;
            _ = ReceiveTimeout;
            _ = PrintTries;
            _ = CodePage;

            return baseReturn;
        }

        protected static IACEntityObjectContext _CommonManagerContext;
        /// <summary>
        /// Returns a seperate and shared Database-Context "StaticACComponentManager".
        /// Because Businessobjects also inherit from this class all BSO's get this shared database context.
        /// If some custom BSO's needs its own context, then they have to override this property.
        /// Application-Managers that also inherit this class should override this property an use their own context.
        /// </summary>
        /// <value>The context as IACEntityObjectContext.</value>
        public override IACEntityObjectContext Database
        {
            get
            {
                if (_CommonManagerContext == null)
                    _CommonManagerContext = ACObjectContextManager.GetOrCreateContext<Database>("StaticACComponentManager");
                return _CommonManagerContext;
            }
        }
        #endregion

        #region Properties

        private bool _CancelPrint = false;

        private ACPropertyConfigValue<string> _IPAddress;
        [ACPropertyConfig("en{'IP Address'}de{'IP Addresse'}")]
        public string IPAddress
        {
            get => _IPAddress.ValueT;
            set => _IPAddress.ValueT = value;
        }

        private ACPropertyConfigValue<int> _Port;
        [ACPropertyConfig("en{'Port'}de{'Port'}")]
        public int Port
        {
            get => _Port.ValueT;
            set => _Port.ValueT = value;
        }


        private ACPropertyConfigValue<int> _SendTimeout;
        [ACPropertyConfig("en{'SendTimeout'}de{'SendTimeout'}")]
        public int SendTimeout
        {
            get => _SendTimeout.ValueT;
            set => _SendTimeout.ValueT = value;
        }


        private ACPropertyConfigValue<int> _ReceiveTimeout;
        [ACPropertyConfig("en{'ReceiveTimeout'}de{'ReceiveTimeout'}")]
        public int ReceiveTimeout
        {
            get => _ReceiveTimeout.ValueT;
            set => _ReceiveTimeout.ValueT = value;
        }


        private ACPropertyConfigValue<int> _PrintTries;
        [ACPropertyConfig("en{'Print Tries'}de{'Print Tries'}")]
        public int PrintTries
        {
            get => _PrintTries.ValueT;
            set => _PrintTries.ValueT = value;
        }

        private ACPropertyConfigValue<int> _CodePage;
        [ACPropertyConfig("en{'Code Page'}de{'Code Page'}")]
        public int CodePage
        {
            get => _CodePage.ValueT;
            set => _CodePage.ValueT = value;
        }

        private ACPropertyConfigValue<bool> _DumpToTempFolder;
        [ACPropertyConfig("en{'Output to File in temp folder'}de{'Ausgabe in Datei im Temporären Ordner'}")]
        public bool DumpToTempFolder
        {
            get => _DumpToTempFolder.ValueT;
            set => _DumpToTempFolder.ValueT = value;
        }

        private ACDispatchedDelegateQueue _DelegateQueue = null;
        public ACDispatchedDelegateQueue DelegateQueue
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    return _DelegateQueue;
                }
            }
        }

        [ACPropertyBindingSource]
        public IACContainerTNet<bool> IsConnected { get; set; }

        #endregion

        #region Methods

        #region Methods -> General (print & provide data)

        [ACMethodInfo("Print", "en{'Print on server'}de{'Auf Server drucken'}", 200, true)]
        public virtual void Print(Guid bsoClassID, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies, bool reloadReport)
        {
            // suggestion: Use Queue
            DelegateQueue.Add(() =>
            {
                DoPrint(bsoClassID, designACIdentifier, pAOrderInfo, copies, reloadReport);
            });
        }

        [ACMethodInfo("Print", "en{'Print on server'}de{'Auf Server drucken'}", 200, true)]
        public virtual void PrintByACUrl(string acUrl, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies, bool reloadReport)
        {
            // suggestion: Use Queue
            DelegateQueue.Add(() =>
            {
                DoPrint(acUrl, designACIdentifier, pAOrderInfo, copies, reloadReport);
            });
        }

        [ACMethodInteraction("Print", "en{'Cancel print'}de{'Drucken abbrechen'}", 300, true)]
        public virtual void CancelPrintJob()
        {
            _CancelPrint = true;
        }

        public void DoPrint(Guid bsoClassID, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies, bool reloadReport)
        {
            ACBSO acBSO = null;
            try
            {
                acBSO = GetACBSO(bsoClassID, pAOrderInfo);
                if (acBSO == null)
                    return;
                DoPrint(acBSO, designACIdentifier, pAOrderInfo, copies, reloadReport);
            }

            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "DoPrint(10)", e);
            }
            finally
            {
                try
                {
                    // @aagincic: is this reqiered by IsPoolable = true?
                    // with this database context is disposed
                    // by many concurent request is exception thrown:
                    // ObjectContext instance has been disposed and can no longer be used for operations that require a connection.
                    if (acBSO != null)
                        acBSO.Stop();
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "DoPrint(20)", e);
                }
            }
        }

        public void DoPrint(string acUrl, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies, bool reloadReport)
        {
            ACBSO acBSO = null;
            try
            {
                acBSO = GetACBSO(acUrl, pAOrderInfo);
                if (acBSO == null)
                    return;
                DoPrint(acBSO, designACIdentifier, pAOrderInfo, copies, reloadReport);
            }
            catch (Exception e)
            {
                Messages.LogException(this.GetACUrl(), "DoPrint(30)", e, true);
            }
            finally
            {
                try
                {
                    // @aagincic: is this reqiered by IsPoolable = true?
                    // with this database context is disposed
                    // by many concurent request is exception thrown:
                    // ObjectContext instance has been disposed and can no longer be used for operations that require a connection.
                    if (acBSO != null)
                        acBSO.Stop();
                }
                catch (Exception e)
                {
                    Messages.LogException(this.GetACUrl(), "DoPrint(40)", e);
                }
            }
        }

        public void DoPrint(ACBSO acBSO, string designACIdentifier, PAOrderInfo pAOrderInfo, int copies, bool reloadReport)
        {
            ACClassDesign aCClassDesign = acBSO.GetDesignForPrinting(GetACUrl(), designACIdentifier, pAOrderInfo);
            if (aCClassDesign == null)
                return;
            ReportData reportData = GetReportData(acBSO, aCClassDesign);
            PrintJob printJob = OnDoPrint(aCClassDesign, CodePage, reportData);
            _CancelPrint = false;

            if (printJob != null)
            {
                SendDataBeforePrint(printJob);

                for (int i = 1; i <= copies; i++)
                {
                    if (_CancelPrint)
                        break;

                    SendDataToPrinter(printJob);
                }

                SendDataAfterPrint(printJob);
            }
        }

        protected abstract PrintJob OnDoPrint(ACClassDesign aCClassDesign, int codePage, ReportData reportData);

        /// <summary>
        /// Factiry BSI abd setzo data frin PAPrderInfo
        /// </summary>
        /// <param name="componetACUrl"></param>
        /// <param name="pAOrderInfo"></param>
        /// <returns></returns>
        public virtual ACBSO GetACBSO(Guid bsoClassID, PAOrderInfo pAOrderInfo)
        {
            ACClass bsoACClass = Root.Database.ContextIPlus.GetACType(bsoClassID);
            ACBSO acBSO = StartComponent(bsoACClass, bsoACClass,
                new ACValueList()
                {
                    new ACValue(Const.ParamSeperateContext, typeof(bool), true),
                    new ACValue(Const.SkipSearchOnStart, typeof(bool), true)
                }) as ACBSO;
            if (acBSO == null)
                return null;
            acBSO.FilterByOrderInfo(pAOrderInfo);
            return acBSO;
        }

        public virtual ACBSO GetACBSO(string acUrl, PAOrderInfo pAOrderInfo)
        {
            ACBSO acBSO = this.Root.ACUrlCommand(acUrl,
                new ACValueList()
                {
                    new ACValue(Const.ParamSeperateContext, typeof(bool), true),
                    new ACValue(Const.SkipSearchOnStart, typeof(bool), true)
                }) as ACBSO;
            if (acBSO == null)
                return null;
            acBSO.FilterByOrderInfo(pAOrderInfo);
            return acBSO;
        }


        /// <summary>
        /// From prepared ACBSO produce ReportData
        /// </summary>
        /// <param name="aCBSO"></param>
        /// <param name="aCClassDesign"></param>
        /// <returns></returns>
        public virtual ReportData GetReportData(ACBSO aCBSO, ACClassDesign aCClassDesign)
        {
            bool cloneInstantiated = false;
            ACQueryDefinition aCQueryDefinition = null;
            ReportData reportData = ReportData.BuildReportData(out cloneInstantiated, Global.CurrentOrList.Current, aCBSO, aCQueryDefinition, aCClassDesign, true);
            return reportData;
        }

        public static bool IsLocalConnection(string ipAddress)
        {
            return String.IsNullOrEmpty(ipAddress)
                    || ipAddress == "localhost"
                    || ipAddress == "127.0.0.1";
        }

        /// <summary>
        /// Convert report data to stream
        /// </summary>
        /// <param name="reportData"></param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool SendDataToPrinter(PrintJob printJob)
        {
            if (printJob == null || printJob.Main == null)
            {
                return false;
            }

            if (IsLocalConnection(IPAddress))
            {
                //IsConnected.ValueT = true;
                return true;
            }
            try
            {
                using (TcpClient tcpClient = new TcpClient(IPAddress, Port))
                {
                    NetworkStream clientStream = tcpClient.GetStream();
                    clientStream.Write(printJob.Main, 0, printJob.Main.Length);
                    clientStream.Flush();
                }
            }
            catch (Exception e)
            {
                string message = String.Format("Connection failed to {0}. See log for further details.", IPAddress);
                if (IsAlarmActive(IsConnected, message) == null)
                    Messages.LogException(GetACUrl(), "SendDataToPrinter(10)", e);
                OnNewAlarmOccurred(IsConnected, message);
                IsConnected.ValueT = false;
                return false;
            }
            if (IsAlarmActive(IsConnected) != null)
                AcknowledgeAlarms();
            //OnAlarmDisappeared(IsConnected);
            IsConnected.ValueT = true;
            return true;
        }

        public virtual void SendDataBeforePrint(PrintJob printJob)
        {

        }

        public virtual void SendDataAfterPrint(PrintJob printJob)
        {

        }
        #endregion

        #region Execute-Helper
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(Print):
                    Print((Guid)acParameter[0],
                          acParameter[1] as string,
                          acParameter[2] as PAOrderInfo,
                          (int)acParameter[3],
                          (bool)acParameter[4]);
                    return true;
                case nameof(PrintByACUrl):
                    PrintByACUrl(acParameter[0] as string,
                          acParameter[1] as string,
                          acParameter[2] as PAOrderInfo,
                          (int)acParameter[3],
                          (bool)acParameter[4]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion


        #endregion
    }
}
