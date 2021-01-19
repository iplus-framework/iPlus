using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Print'}de{'Drucken'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodePrint : PWBaseNodeProcess
    {
        static PWNodePrint()
        {
            ACMethod method;
            method = new ACMethod(ACStateConst.SMStarting);
            Dictionary<string, string> paramTranslation = new Dictionary<string, string>();

            method.ParameterValueList.Add(new ACValue("PrintBSO", typeof(string), "", Global.ParamOption.Optional));
            paramTranslation.Add("PrintBSO", "en{'Printing BSO'}de{'Printing BSO'}");

            method.ParameterValueList.Add(new ACValue("ReportDesignName", typeof(string), "", Global.ParamOption.Optional));
            paramTranslation.Add("ReportDesignName", "en{'ReportDesignName'}de{'ReportDesignName'}");

            method.ParameterValueList.Add(new ACValue("PrinterName", typeof(string), "", Global.ParamOption.Optional));
            paramTranslation.Add("PrinterName", "en{'Printer Name'}de{'Printer Name'}");

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

        public string PrintBSO
        {
            get;
            set;
        }

        public string ReportDesignName
        {
            get;
            set;
        }

        public string PrinterName
        {
            get;
            set;
        }

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
                PrintBSO = null;
                ReportDesignName = null;
                PrinterName = null;
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

            if (string.IsNullOrEmpty(PrintBSO) || string.IsNullOrEmpty(ReportDesignName)
                                              || string.IsNullOrEmpty(PrinterName) || NumberOfCopies == 0
                                              || OrderInfo == null)
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

            ACBSO acBSO = this.GetChildComponent(PrintBSO) as ACBSO;
            if (acBSO == null)
            {
                using (Database db = new datamodel.Database())
                {
                    ACClass bsoACClass = db.ACClass.FirstOrDefault(c => c.ACIdentifier == PrintBSO);
                    if (bsoACClass == null)
                    {
                        // Can not find the PrintBSO definition according configured parameter {0}
                        Msg bsoError = new Msg(this, eMsgLevel.Error, PWClassName, "SMRunning(10)", 147, "");

                        OnNewAlarmOccurred(ProcessAlarm, bsoError, true);
                        if (IsAlarmActive(ProcessAlarm, bsoError.Message) != null)
                            Messages.LogMessageMsg(bsoError);

                        ProcessAlarm.ValueT = PANotifyState.AlarmOrFault;
                        return;
                    }

                    acBSO = this.StartComponent(bsoACClass, null, null) as ACBSO;
                }
            }

            if (acBSO == null)
            {
                // Can not start the PrintBSO component according configured parameter PrintBSO
                Msg bsoError = new Msg(this, eMsgLevel.Error, PWClassName, "SMRunning(20)", 164, "");

                OnNewAlarmOccurred(ProcessAlarm, bsoError, true);
                if (IsAlarmActive(ProcessAlarm, bsoError.Message) != null)
                    Messages.LogMessageMsg(bsoError);

                ProcessAlarm.ValueT = PANotifyState.AlarmOrFault;
                return;
            }

            Msg msg = acBSO.PrintFromWorkflowNode(ReportDesignName, PrinterName, NumberOfCopies, OrderInfo); //todo: ask - delagate to acdelagatequeue
            if (msg != null)
            {
                acBSO.Stop();
                OnNewAlarmOccurred(ProcessAlarm, msg, true);
                if (IsAlarmActive(ProcessAlarm, msg.Message) != null)
                    Messages.LogMessageMsg(msg);

                ProcessAlarm.ValueT = PANotifyState.AlarmOrFault;
                return;
            }

            acBSO.Stop();

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
                PrintBSO = bsoName;
                ReportDesignName = reportDesignName;
                PrinterName = printerName;
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
