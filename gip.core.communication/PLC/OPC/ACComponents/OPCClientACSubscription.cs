using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Threading;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPCClientACSubscr'}de{'OPCClientACSubscr'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public abstract class OPCClientACSubscr : ACSubscription
    {
        #region c´tors
        static OPCClientACSubscr()
        {
            RegisterExecuteHandler(typeof(OPCClientACSubscr), HandleExecuteACMethod_OPCClientACSubscr);
        }

        public OPCClientACSubscr(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACPostInit()
        {
            if (this.ApplicationManager != null)
                this.ApplicationManager.ProjectWorkCycleR20sec += ApplicationManager_ProjectWorkCycle;
            return base.ACPostInit();
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            if (this.ApplicationManager != null)
                this.ApplicationManager.ProjectWorkCycleR20sec -= ApplicationManager_ProjectWorkCycle;
            return await base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties
        [ACPropertyPointProperty(9999, "", typeof(OPCItemConfig))]
        public IEnumerable<IACPropertyNetServer> OPCProperties
        {
            get
            {

                using (ACMonitor.Lock(LockMemberList_20020))
                {
                    try
                    {
                        var query = ACMemberList.Where(c => (c is IACPropertyNetServer)
                                                            && (c.ACType != null)
                                                            && (c.ACType.ACKind == Global.ACKinds.PSPropertyExt))
                                                .Select(c => c as IACPropertyNetServer);
                        return query;
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("OPCClientACSubscr", "OPCProperties", msg);
                    }
                }
                return null;
            }
        }

        [ACPropertyInfo(9999, DefaultValue = 500)]
        public Int32 RequiredUpdateRate
        {
            get;
            set;
        }
        #endregion

        #region Methods
        private void ApplicationManager_ProjectWorkCycle(object sender, EventArgs e)
        {
            RunAutomaticBackupIfInterval();
        }

        public static bool HandleExecuteACMethod_OPCClientACSubscr(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_ACSubscription(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }
        #endregion

    }
}
