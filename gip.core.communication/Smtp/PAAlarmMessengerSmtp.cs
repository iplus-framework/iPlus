using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Runtime.Serialization;
using System.Threading;
using gip.core.autocomponent;

namespace gip.core.communication
{

    /// <summary>
    /// Baseclass for for Broadcasting alarms over Smtp-Protocol using ACSmtpClient
    /// Basisklasse um Alarme mit dem ACSmtpClient zu versenden
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'E-Mail-Messenger'}de{'E-Mail-Verteiler'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, true)]
    public class PAAlarmMessengerSmtp : PAAlarmMessengerBase, IACComponentTaskSubscr
    {
        #region c'tors
        public PAAlarmMessengerSmtp(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ACUrlOfSmtpClient = new ACPropertyConfigValue<string>(this, "ACUrlOfSmtpClient", "\\DataAccess\\Mail");
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            _TaskSubscriptionPoint = new ACPointAsyncRMISubscr(this, Const.TaskSubscriptionPoint, 0);
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACPostInit()
        {
            bool result = base.ACPostInit();
            _ = ACUrlOfSmtpClient;

            return result;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            _TaskSubscriptionPoint.UnSubscribe();
            _TaskSubscriptionPoint = null;
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Points
        protected ACPointAsyncRMISubscr _TaskSubscriptionPoint;
        [ACPropertyAsyncMethodPointSubscr(9999, false, 0, "TaskCallback")]
        public ACPointAsyncRMISubscr TaskSubscriptionPoint
        {
            get
            {
                return _TaskSubscriptionPoint;
            }
        }

        public ACPointNetEventDelegate TaskCallbackDelegate
        {
            get
            {
                return TaskCallback;
            }
        }

        [ACMethodInfo("Function", "en{'TaskCallback'}de{'TaskCallback'}", 9999)]
        public virtual void TaskCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                IACTask taskEntry = wrapObject as IACTask;
                if (taskEntry.State == PointProcessingState.Deleted && taskEntry.InProcess)
                {
                }
            }
        }

        #endregion

        #region Properties
        private ACPropertyConfigValue<string> _ACUrlOfSmtpClient;
        [ACPropertyConfig("en{'ACUrlOfSmtpClient'}de{'ACUrlOfSmtpClient'}", DefaultValue = "\\DataAccess\\Mail")]
        public string ACUrlOfSmtpClient
        {
            get { return _ACUrlOfSmtpClient.ValueT; }
            set { _ACUrlOfSmtpClient.ValueT = value; }
        }
        #endregion

        #region Methods
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "TaskCallback":
                    TaskCallback((gip.core.datamodel.IACPointNetBase)acParameter[0], (gip.core.datamodel.ACEventArgs)acParameter[1], (gip.core.datamodel.IACObject)acParameter[2]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public override void DistributeAlarm(string propertyName, Msg alarm, List<ACRef<ACComponent>> targetComponents)
        {
            ACComponent mailComp = this.ACUrlCommand(ACUrlOfSmtpClient) as ACComponent;
            if (mailComp == null)
                return;

            if (!DistributeAckAlarms.ValueT && alarm.IsAcknowledged)
                return;

            ACMethod acMethod = mailComp.ACUrlACTypeSignature("!SendMailToMailingListAsync", gip.core.datamodel.Database.GlobalDatabase); // Immer Globalen context um Deadlock zu vermeiden 
            acMethod.ParameterValueList["Subject"] = "Alarm: " + alarm.ACCaptionComponent + ", " + alarm.ACCaption;
            acMethod.ParameterValueList["Body"] = XMLToObjectConverter.ObjectToXML(alarm, true, false, true);

            IACPointAsyncRMI rmiInvocationPoint = mailComp.GetPoint(Const.TaskInvocationPoint) as IACPointAsyncRMI;
            if (rmiInvocationPoint != null)
                rmiInvocationPoint.AddTask(acMethod, this);
        }
        #endregion
    }
}
