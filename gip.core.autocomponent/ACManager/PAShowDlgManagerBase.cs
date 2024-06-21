using gip.core.datamodel;
using System;
using System.Linq;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'PAShowDlgManagerBase'}de{'PAShowDlgManagerBase'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, false, false)]
    public abstract class PAShowDlgManagerBase : PARole
    {
        #region c´tors
        public PAShowDlgManagerBase(gip.core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _C_BSONameForShowProgramLog = new ACPropertyConfigValue<string>(this, nameof(BSONameForShowProgamLog), "");
            _C_BSONameForShowPropertyLog = new ACPropertyConfigValue<string>(this, nameof(BSONameForShowPropertyLog), "");
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            bool result = base.ACInit(startChildMode);
            _ = BSONameForShowProgamLog;
            _ = BSONameForShowPropertyLog;
            return result;
        }
        public const string C_DefaultServiceACIdentifier = "DlgManager";
        #endregion

        #region static Methods
        public static PAShowDlgManagerBase GetServiceInstance(ACComponent requester)
        {
            return GetServiceInstance<PAShowDlgManagerBase>(requester, C_DefaultServiceACIdentifier, CreationBehaviour.OnlyLocal);
        }

        public static ACRef<PAShowDlgManagerBase> ACRefToServiceInstance(ACComponent requester)
        {
            PAShowDlgManagerBase serviceInstance = GetServiceInstance(requester);
            if (serviceInstance != null)
                return new ACRef<PAShowDlgManagerBase>(serviceInstance, requester);
            return null;
        }
        #endregion

        #region Configuration
        protected ACPropertyConfigValue<string> _C_BSONameForShowProgramLog;
        [ACPropertyConfig("en{'Classname and ACIdentifier for ShowProgramLogDlg'}de{'Klassenname und ACIdentifier für ShowProgramLogDlg'}")]
        public virtual string BSONameForShowProgamLog
        {
            get
            {
                if (!String.IsNullOrEmpty(_C_BSONameForShowProgramLog.ValueT))
                    return _C_BSONameForShowProgramLog.ValueT;
                _C_BSONameForShowProgramLog.ValueT = "PresenterProgramLog";
                return _C_BSONameForShowProgramLog.ValueT;
            }
            set
            {
                _C_BSONameForShowProgramLog.ValueT = value;
            }
        }

        protected ACPropertyConfigValue<string> _C_BSONameForShowPropertyLog;
        [ACPropertyConfig("en{'Classname and ACIdentifier for ShowPropertyLogDlg'}de{'Klassenname und ACIdentifier für ShowPropertyLogDlg'}")]
        public virtual string BSONameForShowPropertyLog
        {
            get
            {
                if (!String.IsNullOrEmpty(_C_BSONameForShowPropertyLog.ValueT))
                    return _C_BSONameForShowPropertyLog.ValueT;
                _C_BSONameForShowPropertyLog.ValueT = "VBBSOPropertyLogPresenter";
                return _C_BSONameForShowPropertyLog.ValueT;
            }
            set
            {
                _C_BSONameForShowPropertyLog.ValueT = value;
            }
        }
        #endregion

        #region Public Methods

        public abstract void ShowDialogOrder(IACComponent caller, PAOrderInfo orderInfo = null);
        public abstract void ShowDialogComponents(IACComponent caller, PAOrderInfo orderInfo = null);

        public abstract bool IsEnabledShowDialogOrder(IACComponent caller);

        public virtual void ShowProgramLogViewer(IACComponent caller, ACValueList param)
        {
            if (caller == null)
                return;
            string bsoName = BSONameForShowProgamLog;
            if (String.IsNullOrEmpty(bsoName))
                return;
            ACComponent bso = caller.Root.Businessobjects.ACUrlCommand("?" + bsoName) as ACComponent;
            if (bso == null)
                bso = caller.Root.Businessobjects.StartComponent(bsoName, null, new object[] { }) as ACComponent;
            if (bso == null)
                return;
            bso.ACUrlCommand("!ShowACProgramLog", param);
            bso.Stop();
            return;
        }

        public virtual void ShowPropertyLogViewer(IACComponent caller)
        {
            if (caller == null)
                return;
            string bsoName = BSONameForShowPropertyLog;
            if (String.IsNullOrEmpty(bsoName))
                return;

            ACComponent bso = caller.Root.Businessobjects.ACUrlCommand("?" + bsoName) as ACComponent;
            if (bso == null)
                bso = caller.Root.Businessobjects.StartComponent(bsoName, null, new object[] { }) as ACComponent;
            if (bso == null)
                return;
            bso.ExecuteMethod("ShowPropertyLogsDialog", caller.ComponentClass);
            bso.Stop();
        }

        public abstract string BuildAndSetOrderInfo(PAProcessModule pm);

        public abstract string BuildOrderInfo(PWBase pw);


        public virtual string GetBSOName(string baseBSOName, string dialogName = "Dialog")
        {
            ACClass classOfBso = (Root.Database as Database).GetACType(baseBSOName);
            return GetBSOName(baseBSOName, classOfBso, dialogName);
        }

        public virtual string GetBSOName(string baseBSOName, ACClass baseBSO, string dialogName)
        {
            string bsoName = baseBSOName;
            if (baseBSO != null)
            {
                gip.core.datamodel.ACClass derivation = null;
                using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
                {
                    derivation = gip.core.datamodel.Database.GlobalDatabase.ACClass
                                            .Where(c => c.BasedOnACClassID == baseBSO.ACClassID
                                                    && !String.IsNullOrEmpty(c.AssemblyQualifiedName)
                                                    && c.AssemblyQualifiedName != baseBSO.AssemblyQualifiedName).FirstOrDefault();
                }
                if (derivation != null && !derivation.IsAbstract)
                {
                    bsoName = derivation.ACIdentifier;;
                }
            }

            if(!string.IsNullOrEmpty(dialogName))
            {
                bsoName = $"{bsoName}({dialogName})";
            }

            return bsoName;
        }

        #endregion

    }
}
