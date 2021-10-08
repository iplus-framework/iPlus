using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.autocomponent;
using gip.core.datamodel;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Print'}de{'Drucken'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodePrint : PWBaseNodeProcess
    {
        static PWNodePrint()
        {
            ACMethod method;
            method = new ACMethod(ACStateConst.SMStarting);
            Dictionary<string, string> paramTranslation = new Dictionary<string, string>();

            method.ParameterValueList.Add(new ACValue("NumberOfCopies", typeof(short), 1, Global.ParamOption.Optional));
            paramTranslation.Add("NumberOfCopies", "en{'Number Of Copies}de{'Number Of Copies'}");

            var wrapper = new ACMethodWrapper(method, "en{'Print'}de{'Drucken'}", typeof(PWNodePrint), paramTranslation, null);
            ACMethod.RegisterVirtualMethod(typeof(PWNodePrint), ACStateConst.SMStarting, wrapper);
            RegisterExecuteHandler(typeof(PWNodePrint), HandleExecuteACMethod_PWNodePrint);
        }

        public PWNodePrint(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public const string PWClassName = "PWNodePrint";

        #region Properties


        public short NumberOfCopies
        {
            get;
            set;
        }

        public PAOrderInfo OrderInfo
        {
            get;
            set;
        }


        #endregion

        public override void SMIdle()
        {
            base.SMIdle();
            using (ACMonitor.Lock(_20015_LockValue))
            {
                NumberOfCopies = 0;
                OrderInfo = null;
            }
        }

        [ACMethodState("en{'Executing'}de{'Ausführend'}", 20, true)]
        public override void SMStarting()
        {
            var newMethod = NewACMethodWithConfiguration();
            CreateNewProgramLog(newMethod, true);

            Msg error = ReadParameters(newMethod);
            if (error != null)
            {
                OnNewAlarmOccurred(ProcessAlarm, error, true);
                if (IsAlarmActive(ProcessAlarm, error.Message) != null)
                    Messages.LogMessageMsg(error);

                ProcessAlarm.ValueT = PANotifyState.AlarmOrFault;
                return;
            }

            base.SMStarting();
        }

        public override void SMRunning()
        {
            if (!Root.Initialized)
            {
                SubscribeToProjectWorkCycle();
                return;
            }

            UnSubscribeToProjectWorkCycle();

            if (NumberOfCopies == 0 || OrderInfo == null)
            {
                var newMethod = NewACMethodWithConfiguration();
                Msg error = ReadParameters(newMethod);
                if (error != null)
                {
                    OnNewAlarmOccurred(ProcessAlarm, error, true);
                    if (IsAlarmActive(ProcessAlarm, error.Message) != null)
                        Messages.LogMessageMsg(error);

                    ProcessAlarm.ValueT = PANotifyState.AlarmOrFault;
                    return;
                }
            }

            Msg msg = null;

            ACComponent printManager = ACPrintManager.GetServiceInstance(this) as ACComponent;
            if (printManager == null)
                OnNewAlarmOccurred(ProcessAlarm, "PrintManager is not configured!", true);
            else
                msg = printManager.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + ACPrintManager.MN_Print, OrderInfo, NumberOfCopies) as Msg;


            if (msg != null)
            {
                OnNewAlarmOccurred(ProcessAlarm, msg, true);
                if (IsAlarmActive(ProcessAlarm, msg.Message) != null)
                    Messages.LogMessageMsg(msg);

                ProcessAlarm.ValueT = PANotifyState.AlarmOrFault;
                return;
            }

            CurrentACState = ACStateEnum.SMCompleted;
        }

        public override PAOrderInfo GetPAOrderInfo()
        {
            PAOrderInfo orderInfo = base.GetPAOrderInfo();

            if (RootPW != null)
            {
                PAOrderInfo rootOrderInfo = RootPW.GetPAOrderInfo();
                if (orderInfo == null)
                    orderInfo = rootOrderInfo;
                else
                    orderInfo.Append(rootOrderInfo);

                if (orderInfo != null)
                    orderInfo.Add(PWClassName, ContentACClassWF.ACClassWFID);
            }

            return orderInfo;
        }

        private Msg ReadParameters(ACMethod configMethod)
        {
            if (configMethod == null)
            {
                //Error50327 The ACMethod configuration is null!
                return new Msg(this, eMsgLevel.Error, PWClassName, "ReadParameters(10)", 215, "Error50327");
            }

            string bsoName = null, reportDesignName = null, printerName = null;
            short numberOfCopies = 0;

            ACValue bsoVal = configMethod.ParameterValueList.GetACValue("PrintBSO");
            if (bsoVal != null)
            {
                bsoName = bsoVal.ParamAsString;
            }

            if (string.IsNullOrEmpty(bsoName))
            {
                //Error50328 The configuration parameter PrintBSO is not configured!
                return new Msg(this, eMsgLevel.Error, PWClassName, "ReadParameters(20)", 230, "Error50328");
            }

            ACValue reportNameVal = configMethod.ParameterValueList.GetACValue("ReportDesignName");
            if (reportNameVal != null)
            {
                reportDesignName = reportNameVal.ParamAsString;
            }

            if (string.IsNullOrEmpty(reportDesignName))
            {
                //Error50329 The configuration parameter ReportDesignName is not configured!
                return new Msg(this, eMsgLevel.Error, PWClassName, "ReadParameters(30)", 242, "Error50329");
            }

            ACValue printerNameVal = configMethod.ParameterValueList.GetACValue("PrinterName");
            if (printerNameVal != null)
                printerName = printerNameVal.ParamAsString;

            ACValue nOCVal = configMethod.ParameterValueList.GetACValue("NumberOfCopies");
            if (nOCVal != null)
                numberOfCopies = nOCVal.ParamAsInt16;

            PAOrderInfo orderInfo = GetPAOrderInfo();
            if (orderInfo == null)
            {
                // Error50330 The order information PAOrderInfo can not be found!
                return new Msg(this, eMsgLevel.Error, PWClassName, "ReadParameters(40)", 257, "Error50330");
            }

            using (ACMonitor.Lock(_20015_LockValue))
            {
                NumberOfCopies = numberOfCopies;
                OrderInfo = orderInfo;
            }

            return null;
        }

        public static bool HandleExecuteACMethod_PWNodePrint(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_PWBaseNodeProcess(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

    }
}
