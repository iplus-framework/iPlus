using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using Softing.OPCToolbox.Client;
using Softing.OPCToolbox;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.communication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPCClientSACService'}de{'OPCClientSACService'}", Global.ACKinds.TACDAClass, Global.ACStorableTypes.Required, false, false)]
    public class OPCClientSACService : OPCClientACService
    {
        #region c´tors
        public OPCClientSACService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _syncSoftingMTA = new SyncQueueEvents();
            _SoftingMTAThread = new ACThread(ManageLifeOfSoftingApp);
            _SoftingMTAThread.Name = "ACUrl:" + this.GetACUrl() + ";ManageLifeOfSoftingApp();";
            _SoftingMTAThread.Start();

            if (!base.ACInit(startChildMode))
                return false;           
            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            if (_SoftingMTAThread != null)
            {
                _syncSoftingMTA.TerminateThread();
                _SoftingMTAThread.Join();
                _SoftingMTAThread = null;
            }
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties
        SyncQueueEvents _syncSoftingMTA;
        ACThread _SoftingMTAThread = null;


        private Softing.OPCToolbox.Client.Application _SoftingApp = null;
        public Softing.OPCToolbox.Client.Application SoftingApp
        {
            get
            {
                return _SoftingApp;
            }
        }

        #endregion

        #region methods

        private void ManageLifeOfSoftingApp()
        {
            InitSoftingApp();
            _syncSoftingMTA.ExitThreadEvent.WaitOne();
            DeIntSoftingApp();
            _syncSoftingMTA.ThreadTerminated();
        }

        private bool InitSoftingApp()
        {
            _SoftingApp = Softing.OPCToolbox.Client.Application.Instance;
            _SoftingApp.EnableTracing(EnumTraceGroup.ALL, EnumTraceGroup.ALL, EnumTraceGroup.ALL, EnumTraceGroup.ALL, "C:\\TEMP\\Softing.txt", 100000, 1);
            _SoftingApp.Activate(EnumFeature.DA_CLIENT, "09c0-0000-0325-9bd7-f5d9");
            //_SoftingApp.Activate(EnumFeature.XMLDA_CLIENT, "09c0-0000-0325-9bd7-f5d9");
            //_SoftingApp.Activate(EnumFeature.AE_CLIENT, "09c0-0000-0325-9bd7-f5d9");
            if (ResultCode.FAILED(_SoftingApp.Initialize()))
                return false;
            return true;
        }

        private bool DeIntSoftingApp()
        {
            if (_SoftingApp != null)
            {
                if (ResultCode.FAILED(_SoftingApp.Terminate()))
                    return false;
                _SoftingApp.ReleaseApplication();
                _SoftingApp = null;
            }
            return true;
        }

        #endregion
    }
}
