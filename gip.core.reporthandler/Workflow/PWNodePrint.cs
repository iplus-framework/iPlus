using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using gip.core.autocomponent;
using gip.core.datamodel;

namespace gip.core.reporthandler
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Print'}de{'Drucken'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodePrint : PWBaseNodeProcess
    {
        #region c'tors
        static PWNodePrint()
        {
            ACMethod method;
            method = new ACMethod(ACStateConst.SMStarting);
            Dictionary<string, string> paramTranslation = new Dictionary<string, string>();

            method.ParameterValueList.Add(new ACValue("NumberOfCopies", typeof(short), 1, Global.ParamOption.Optional));
            paramTranslation.Add("NumberOfCopies", "en{'Number Of Copies'}de{'Anzahl Ausdrucke'}");
            method.ParameterValueList.Add(new ACValue("MaxPrintJobsInSpooler", typeof(int), 0, Global.ParamOption.Optional));
            paramTranslation.Add("MaxPrintJobsInSpooler", "en{'Max. print jobs in spooler'}de{'Maximale Anzahl an Druckaufträgen im Spooler'}");

            var wrapper = new ACMethodWrapper(method, "en{'Print'}de{'Drucken'}", typeof(PWNodePrint), paramTranslation, null);
            ACMethod.RegisterVirtualMethod(typeof(PWNodePrint), ACStateConst.SMStarting, wrapper);
            RegisterExecuteHandler(typeof(PWNodePrint), HandleExecuteACMethod_PWNodePrint);
        }

        public PWNodePrint(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            return await base.ACDeInit(deleteACClassTask);
        }

        public const string PWClassName = nameof(PWNodePrint);
        #endregion


        #region Properties
        public short NumberOfCopies
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("NumberOfCopies");
                    if (acValue != null)
                    {
                        return acValue.ParamAsInt16;
                    }
                }
                return 0;
            }
        }

        public int MaxPrintJobsInSpooler
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("MaxPrintJobsInSpooler");
                    if (acValue != null)
                    {
                        return acValue.ParamAsInt32;
                    }
                }
                return 0;
            }
        }

        public override bool MustBeInsidePWGroup
        {
            get
            {
                return false;
            }
        }
        #endregion


        #region Methods
        public override void Reset()
        {
            ClearMyConfiguration();
            base.Reset();
        }

        public override void SMIdle()
        {
            base.SMIdle();
        }

        [ACMethodState("en{'Executing'}de{'Ausführend'}", 20, true)]
        public override void SMStarting()
        {
            if (!CheckParentGroupAndHandleSkipMode())
                return;
            var newMethod = NewACMethodWithConfiguration();
            CreateNewProgramLog(newMethod, true);
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

            if (NumberOfCopies > 0)
            {
                Msg msg = null;
                ACPrintManager printManager = ACPrintManager.GetServiceInstance(this);
                if (printManager == null)
                    OnNewAlarmOccurred(ProcessAlarm, "PrintManager is not configured!", true);
                else
                {
                    PAOrderInfo orderInfo = GetPAOrderInfo();
                    if (orderInfo != null)
                        msg = printManager.Print(orderInfo, NumberOfCopies, null, MaxPrintJobsInSpooler);
                        //msg = printManager.ACUrlCommand(ACUrlHelper.Delimiter_InvokeMethod + ACPrintManager.MN_Print, orderInfo, NumberOfCopies) as Msg;
                }
                if (msg != null && msg.MessageLevel > eMsgLevel.Info)
                {
                    OnNewAlarmOccurred(ProcessAlarm, msg, true);
                    if (IsAlarmActive(ProcessAlarm, msg.Message) != null)
                        Messages.LogMessageMsg(msg);

                    ProcessAlarm.ValueT = PANotifyState.AlarmOrFault;
                    return;
                }
            }

            CurrentACState = ACStateEnum.SMCompleted;
        }

        public override PAOrderInfo GetPAOrderInfo()
        {
            PAOrderInfo orderInfo = base.GetPAOrderInfo();
            // If Parent is not a PWGroup, than OrderInfo is null
            if (orderInfo == null && RootPW != null)
                orderInfo = RootPW.GetPAOrderInfo();
            if (orderInfo != null)
            {
                // Add info on which machine this print takes place
                PWGroup pwGroup = FindParentComponent<PWGroup>();
                if (pwGroup != null)
                {
                    PAProcessModule module = pwGroup.AccessedProcessModule;
                    if (module == null)
                        module = pwGroup.PreviousAccessedPM;
                    if (module != null)
                        orderInfo.Add(ACClass.ClassName, module.ComponentClass.ACClassID);
                }
                orderInfo.Add(PWClassName, ContentACClassWF.ACClassWFID);
            }
            return orderInfo;
        }

        public static bool HandleExecuteACMethod_PWNodePrint(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_PWBaseNodeProcess(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList, ref DumpStats dumpStats)
        {
            base.DumpPropertyList(doc, xmlACPropertyList, ref dumpStats);

            XmlElement xmlChild = xmlACPropertyList["NumberOfCopies"];
            if (xmlChild == null)
            {
                xmlChild = doc.CreateElement("NumberOfCopies");
                if (xmlChild != null)
                    xmlChild.InnerText = NumberOfCopies.ToString();
                xmlACPropertyList.AppendChild(xmlChild);
            }

            xmlChild = xmlACPropertyList["MaxPrintJobsInSpooler"];
            if (xmlChild == null)
            {
                xmlChild = doc.CreateElement("MaxPrintJobsInSpooler");
                if (xmlChild != null)
                    xmlChild.InnerText = MaxPrintJobsInSpooler.ToString();
                xmlACPropertyList.AppendChild(xmlChild);
            }
        }
        #endregion

    }
}
