using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Bus'}de{'Allgemeiner Bus'}", Global.ACKinds.TPAProcessModule, Global.ACStorableTypes.Required, false, PWGroup.PWClassName, true)]
    public class PAMBus : PAClassAlarmingBase
    {

        private static Dictionary<string, ACEventArgs> _SVirtualEventArgs;

        #region Properties

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

        static PAMBus()
        {
            ACEventArgs TMP;

            _SVirtualEventArgs = new Dictionary<string,ACEventArgs>(PAClassAlarmingBase.SVirtualEventArgs, StringComparer.OrdinalIgnoreCase);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("DeviceID", typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue("ScanData", typeof(string), null, Global.ParamOption.Required));
            _SVirtualEventArgs.Add("ReadOnBusEvent", TMP);

            TMP = new ACEventArgs();
            TMP.Add(new ACValue("DeviceID", typeof(string), null, Global.ParamOption.Required));
            TMP.Add(new ACValue("WriteData", typeof(object[]), null, Global.ParamOption.Required));
            _SVirtualEventArgs.Add("WriteOnBusEvent", TMP);

        }

        public PAMBus(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ReadOnBusEvent = new ACPointEvent(this, "ReadOnBusEvent", 0);
            _WriteOnBusEvent = new ACPointEvent(this, "WriteOnBusEvent", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Public

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "SendToDeviceOnBus":
                    SendToDeviceOnBus(acParameter[0] as string, acParameter[1] as object[]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInfo("Function", "en{'Write on Bus'}de{'Schreibe auf Bus'}", 9999)]
        public virtual void SendToDeviceOnBus(string DeviceID, object[] parameter)
        {
        }

#endregion

#region Points

        protected ACPointEvent _ReadOnBusEvent;
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent ReadOnBusEvent
        {
            get
            {
                return _ReadOnBusEvent;
            }
            set
            {
                _ReadOnBusEvent = value;
            }
        }

        protected ACPointEvent _WriteOnBusEvent;
        [ACPropertyEventPoint(9999, false)]
        public ACPointEvent WriteOnBusEvent
        {
            get
            {
                return _WriteOnBusEvent;
            }
            set
            {
                _WriteOnBusEvent = value;
            }
        }

#endregion

    }

}
