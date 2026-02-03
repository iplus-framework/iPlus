using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.bso.iplus;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACSubscription'}de{'ACSubscription'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class ACSubscription : PAClassAlarmingBase
    {
        #region c´tors
        static ACSubscription()
        {
            RegisterExecuteHandler(typeof(ACSubscription), HandleExecuteACMethod_ACSubscription);
        }

        public ACSubscription(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _IsReadyForWriting = false;
            _AutoBackupInitialized = true;
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override void Recycle(IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
        {
            _AutoBackupInitialized = false;
            base.Recycle(content, parentACObject, parameter, acIdentifier);
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            if (!DeInitSubscription())
                return false;
            return await base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties

        [ACPropertyBindingSource]
        public IACContainerTNet<bool> IsConnected { get; set; }

        private bool _IsReadyForWriting = false;
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

        [ACPropertyInfo(true, 9999, DefaultValue = false)]
        public bool IsWriteOnly
        {
            get;
            set;
        }

        [ACPropertyInfo(true, 800, "", "en{'Time interval for cyclic Backup'}de{'Zeitintervall für regelmäßige Sicherung'}")]
        public TimeSpan BackupInterval
        {
            get;
            set;
        }

        private static TimeSpan C_MinBackupInterval = new TimeSpan(0, 0, 30);
        private TimeSpan BackupIntervalValidated
        {
            get
            {
                if (BackupInterval <= TimeSpan.Zero)
                    return TimeSpan.Zero;
                if (BackupInterval <= C_MinBackupInterval)
                    return C_MinBackupInterval;
                return BackupInterval;
            }
        }

        private bool _AutoBackupInitialized = false;
        [ACPropertyInfo(true, 801, "", "en{'Last Backup'}de{'Letzte Sicherung'}")]
        public DateTime LastBackup
        {
            get;
            set;
        }

        private bool _ConnLostBackupIsOff = false;
        [ACPropertyInfo(false, 802, "", "en{'Connection is lost - Backup is off'}de{'Verbindung verloren - Backup ist aus'}")]
        public bool ConnLostBackupIsOff
        {
            get
            {
                return _ConnLostBackupIsOff;
            }
            set
            {
                _ConnLostBackupIsOff = value;
                OnPropertyChanged();
            }
        }

        protected ACSession CommSession
        {
            get
            {
                return ParentACComponent as ACSession;
            }
        }

        public ACService CommService
        {
            get
            {
                if (CommSession == null)
                    return null;
                return CommSession.CommService;
            }
        }

        #endregion

        #region Methods, Range: 200
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(InitSubscription):
                    result = InitSubscription();
                    return true;
                case nameof(IsEnabledInitSubscription):
                    result = IsEnabledInitSubscription();
                    return true;
                case nameof(DeInitSubscription):
                    result = DeInitSubscription();
                    return true;
                case nameof(IsEnabledDeInitSubscription):
                    result = IsEnabledDeInitSubscription();
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

        public static bool HandleExecuteACMethod_ACSubscription(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            //switch (acMethodName)
            //{
            //    case nameof(AutoInsertVariables):
            //        AutoInsertVariables(acComponent);
            //        return true;
            //    case Const.IsEnabledPrefix + nameof(AutoInsertVariables):
            //        result = IsEnabledAutoInsertVariables(acComponent);
            //        return true;
            //    case nameof(AutoRenameVariables):
            //        AutoRenameVariables(acComponent);
            //        return true;
            //    case Const.IsEnabledPrefix + nameof(AutoRenameVariables):
            //        result = IsEnabledAutoRenameVariables(acComponent);
            //        return true;
            //}
            return false;
        }


        [ACMethodInfo("xxx", "en{'Init'}de{'Initialisiere'}", 9999)]
        public abstract bool InitSubscription();
        public abstract bool IsEnabledInitSubscription();

        [ACMethodInfo("xxx", "en{'Deinit'}de{'Deinitialisiere'}", 9999)]
        public abstract bool DeInitSubscription();
        public abstract bool IsEnabledDeInitSubscription();

        [ACMethodInteraction("xxx", "en{'Connect'}de{'Verbinden'}", 20, true)]
        public abstract bool Connect();
        public abstract bool IsEnabledConnect();

        [ACMethodInteraction("xx", "en{'Disconnect'}de{'Verbindung Trennen'}", 21, true)]
        public abstract bool DisConnect();
        public abstract bool IsEnabledDisConnect();

        #region Configuration
        [ACMethodAttached("", "en{'Create variables autom.'}de{'Variablen autom. anlegen'}", 1000, typeof(BSOiPlusStudio), true, "", false, Global.ContextMenuCategory.NoCategory)]
        public static void AutoInsertVariables(IACComponent acComponent)
        {
            BSOiPlusStudio _this = acComponent as BSOiPlusStudio;
            if (!IsEnabledAutoInsertVariables(_this))
                return;

            object[] result = acComponent.Messages.InputBoxValues("Wertebereich", new object[] { "1", "1", "A", "", "", "0", "0", "" }, new string[] { "Von", "Bis", "Prefix", "Postfix", "SPSAddr-Prefix", "SPS-Startaddr.", "SPS-Offset to next", "Bemerkung" });

            if (result == null)
            {
                return;
            }

            int nfrom = System.Convert.ToInt32(result[0]);
            int nto = System.Convert.ToInt32(result[1]);
            if ((nfrom <= 0) || (nto <= 0) || (nto < nfrom))
            {
                return;
            }
            string prefix = System.Convert.ToString(result[2]);
            string postfix = System.Convert.ToString(result[3]);
            string spsPrefix = System.Convert.ToString(result[4]);
            int spsfrom = System.Convert.ToInt32(result[5]);
            int spsoffset = System.Convert.ToInt32(result[6]);
            string comment = System.Convert.ToString(result[7]);

            ACClass dataType = _this.CurrentConfigACClassProperty.ValueTypeACClass;
            Global.ACPropUsages acPropUsage = _this.CurrentConfigACClassProperty.ACPropUsage;

            int spsNextAddr = spsfrom;
            for (int i = nfrom; i <= nto; i++)
            {
                _this.NewConfigACClassProperty();
                _this.CurrentConfigACClassProperty.ACIdentifier = String.Format("{1}{0:D3}{2}", i, prefix, postfix);
                _this.CurrentConfigACClassProperty.ACCaption = _this.CurrentConfigACClassProperty.ACIdentifier;
                _this.CurrentConfigACClassProperty.ValueTypeACClass = dataType;
                _this.CurrentConfigACClassProperty.ACPropUsage = acPropUsage;
                _this.CurrentConfigACClassProperty.IsInput = true;
                _this.CurrentConfigACClassProperty.IsOutput = true;
                _this.CurrentConfigACClassProperty.IsBroadcast = true;
                _this.CurrentConfigACClassProperty.SortIndex = System.Convert.ToInt16(i);
                _this.CurrentConfigACClassProperty.Comment = comment;
                if (!String.IsNullOrEmpty(spsPrefix) && spsfrom > 0 && spsoffset > 0)
                {
                    string spsAddr = String.Format("{0}{1}", spsPrefix, spsNextAddr);
                    OPCItemConfig opcConfig = (OPCItemConfig)_this.CurrentConfigACClassProperty["OPCItemConfig"];
                    opcConfig.OPCAddr = spsAddr;
                    spsNextAddr += spsoffset;
                }
            }

            return;
        }

        public static bool IsEnabledAutoInsertVariables(IACComponent acComponent)
        {
            BSOiPlusStudio _this = acComponent as BSOiPlusStudio;
            if (_this == null)
                return false;
            return _this.CurrentACClass != null && _this.CurrentConfigACClassProperty != null;
        }


        [ACMethodAttached("", "en{'Rename variables'}de{'Variablen umbenennen'}", 1001, typeof(BSOiPlusStudio), true, "", false, Global.ContextMenuCategory.NoCategory)]
        public static void AutoRenameVariables(IACComponent acComponent)
        {
            BSOiPlusStudio _this = acComponent as BSOiPlusStudio;
            if (!IsEnabledAutoRenameVariables(_this))
                return;

            object[] result = acComponent.Messages.InputBoxValues("Wertebereich", new object[] { "1", "1", "A", "", "", "0", "0", "" }, new string[] { "Von", "Bis", "Prefix", "Postfix", "SPSAddr-Prefix", "SPS-Startaddr.", "SPS-Offset to next", "Bemerkung" });

            if (result == null)
            {
                return;
            }

            int nfrom = System.Convert.ToInt32(result[0]);
            int nto = System.Convert.ToInt32(result[1]);
            if ((nfrom <= 0) || (nto <= 0) || (nto < nfrom))
            {
                return;
            }
            string prefix = System.Convert.ToString(result[2]);
            string postfix = System.Convert.ToString(result[3]);
            string spsPrefix = System.Convert.ToString(result[4]);
            int spsfrom = System.Convert.ToInt32(result[5]);
            int spsoffset = System.Convert.ToInt32(result[6]);
            string comment = System.Convert.ToString(result[7]);

            ACClass dataType = _this.CurrentConfigACClassProperty.ValueTypeACClass;
            Global.ACPropUsages acPropUsage = _this.CurrentConfigACClassProperty.ACPropUsage;

            int spsNextAddr = spsfrom;
            for (int i = nfrom; i <= nto; i++)
            {
                string findID = String.Format("{1}{0:D3}{2}", i, prefix, postfix);

                var foundProperty = _this.ConfigACClassPropertyList.Where(c => c.ACIdentifier == findID).FirstOrDefault();
                if (foundProperty != null)
                {
                    foundProperty.ValueTypeACClass = dataType;
                    if (!String.IsNullOrEmpty(spsPrefix) && spsfrom > 0 && spsoffset > 0)
                    {
                        string spsAddr = String.Format("{0}{1}", spsPrefix, spsNextAddr);
                        OPCItemConfig opcConfig = (OPCItemConfig)foundProperty["OPCItemConfig"];
                        opcConfig.OPCAddr = spsAddr;
                    }
                }
                if (!String.IsNullOrEmpty(spsPrefix) && spsfrom > 0 && spsoffset > 0)
                    spsNextAddr += spsoffset;
            }
            return;
        }

        public static bool IsEnabledAutoRenameVariables(IACComponent acComponent)
        {
            BSOiPlusStudio _this = acComponent as BSOiPlusStudio;
            if (_this == null)
                return false;
            return _this.CurrentACClass != null && _this.CurrentConfigACClassProperty != null;
        }

        #endregion

        #region Automatic Backup
        protected void RunAutomaticBackupIfInterval()
        {
            if (   BackupIntervalValidated <= TimeSpan.Zero
                || CommSession == null 
                || !CommSession.IsConnected.ValueT 
                || !IsReadyForWriting
                || !Root.Initialized
                || (LastBackup > DateTime.MinValue && (LastBackup + BackupIntervalValidated) > DateTime.Now)
                || ConnLostBackupIsOff)
                return;
            LastBackup = DateTime.Now;
            // Wait one Cycle for Backup, to give time for restoring from the user.
            if (!_AutoBackupInitialized)
            {
                _AutoBackupInitialized = true;
                return;
            }
            BackupState();
        }

        public override void RestoreBackupedState()
        {
            base.RestoreBackupedState();
            ConnLostBackupIsOff = false;
        }

        [ACMethodInteraction("xxx", "en{'Reactivate automatic Backup'}de{'Reaktiviere automatische Sicherung'}", 50, true)]
        public void ActivateAutoBackup()
        {
            ConnLostBackupIsOff = false;
        }

        public bool IsEnabledActivateAutoBackup()
        {
            return ConnLostBackupIsOff 
                && CommSession != null 
                && CommSession.IsConnected.ValueT
                && IsReadyForWriting
                && Root.Initialized
                && BackupIntervalValidated > TimeSpan.Zero;
        }

        [ACMethodInteraction("xxx", "en{'Deactivate automatic Backup'}de{'Deaktiviere automatische Sicherung'}", 51, true)]
        public void DeActivateAutoBackup()
        {
            ConnLostBackupIsOff = true;
        }

        public bool IsEnabledDeActivateAutoBackup()
        {
            return !ConnLostBackupIsOff
                && CommSession != null
                && Root.Initialized
                && BackupIntervalValidated > TimeSpan.Zero;
        }
        #endregion

        #endregion
    }
}
