using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Xml;
using System.Threading;

namespace gip.core.autocomponent
{
    public interface IUserAckModule : IACComponent
    {
        IACContainerTNet<bool> KeySwitch { get; set; }
    }

    [ACClassInfo(Const.PackName_VarioSystem, "en{'User Acknowledge'}de{'Benutzerbestätigung'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeUserAck : PWBaseNodeProcess
    {
        public const string PWClassName = "PWNodeUserAck";


        #region Constructors

        static PWNodeUserAck()
        {
            RegisterExecuteHandler(typeof(PWNodeUserAck), HandleExecuteACMethod_PWNodeUserAck);

            ACMethod method;
            method = new ACMethod(ACStateConst.SMStarting);
            Dictionary<string, string> paramTranslation = new Dictionary<string, string>();
            method.ParameterValueList.Add(new ACValue("MessageText", typeof(string), "", Global.ParamOption.Required));
            paramTranslation.Add("MessageText", "en{'Question text'}de{'Abfragetext'}");
            method.ParameterValueList.Add(new ACValue("PasswordDlg", typeof(bool), false, Global.ParamOption.Required));
            paramTranslation.Add("PasswordDlg", "en{'With password dialogue'}de{'Mit Passwort-Dialog'}");
            method.ParameterValueList.Add(new ACValue("SkipMode", typeof(ushort), 0, Global.ParamOption.Optional));
            paramTranslation.Add("SkipMode", "en{'Skipmode: 1=Always, 2=From the second run'}de{'Überspringen: 1=Ständig, 2=Ab zweitem Durchlauf'}");
            method.ParameterValueList.Add(new ACValue("ACUrlCmd", typeof(string), "", Global.ParamOption.Required));
            paramTranslation.Add("ACUrlCmd", "en{'Invoke ACUrl-Command'}de{'ACUrl-Kommando ausführen'}");

            Dictionary<string, string> resultTranslation = new Dictionary<string, string>();
            method.ResultValueList.Add(new ACValue("UserName", typeof(string), "", Global.ParamOption.Optional));
            resultTranslation.Add("UserName", "en{'Username'}de{'Benutzername'}");

            var wrapper = new ACMethodWrapper(method, "en{'User Acknowledge'}de{'Benutzerbestätigung'}", typeof(PWNodeUserAck), paramTranslation, resultTranslation);
            ACMethod.RegisterVirtualMethod(typeof(PWNodeUserAck), ACStateConst.SMStarting, wrapper);
        }

        public PWNodeUserAck(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            _AckTiggeredOverSwitch = false;
            RefreshNodeInfoOnModule(true);
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion


        #region Properties
        public virtual string CMessageText
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("MessageText");
                    if (acValue != null)
                    {
                        return acValue.ParamAsString;
                    }
                }
                return "";
            }
        }

        public bool CPasswordDlg
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("PasswordDlg");
                    if (acValue != null)
                    {
                        return acValue.ParamAsBoolean;
                    }
                }
                return false;
            }
        }

        public enum SkipModeEnum : ushort
        {
            Never = 0,
            Always = 1,
            FromSecondRun = 2 
        }

        public ushort SkipMode
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("SkipMode");
                    if (acValue != null)
                    {
                        return acValue.ParamAsUInt16;
                    }
                }
                return 0;
            }
        }

        public bool IsSkippingNeeded
        {
            get
            {
                return SkipMode == (ushort)SkipModeEnum.Always
                || (SkipMode == (ushort)SkipModeEnum.FromSecondRun && RootPW != null && RootPW.InvocationCounter.HasValue && RootPW.InvocationCounter.Value > 1);
            }
        }

        public string ACUrlCmd
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("ACUrlCmd");
                    if (acValue != null)
                    {
                        return acValue.ParamAsString;
                    }
                }
                return "";
            }
        }

        private IACContainerTNet<bool> _KeySwitch = null;
        bool _AckTiggeredOverSwitch = false;

        public override bool MustBeInsidePWGroup
        {
            get
            {
                return false;
            }
        }
        #endregion


        #region Methods

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "AckStart":
                    AckStart();
                    return true;
                case Const.IsEnabledPrefix + "AckStart":
                    result = IsEnabledAckStart();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PWNodeUserAck(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(AckStartClient):
                    AckStartClient(acComponent);
                    return true;
                case nameof(IsEnabledAckStartClient):
                    result = IsEnabledAckStartClient(acComponent);
                    return true;
            }
            return HandleExecuteACMethod_PWBaseNodeProcess(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #region State
        public override void SMIdle()
        {
            AlarmsAsText.ValueT = "";
            base.SMIdle();
            RefreshNodeInfoOnModule(true);
        }

        [ACMethodState("en{'Executing'}de{'Ausführend'}", 20, true)]
        public override void SMStarting()
        {
            if (!CheckParentGroupAndHandleSkipMode())
                return;

            RecalcTimeInfo();
            if (CreateNewProgramLog(NewACMethodWithConfiguration()) <= CreateNewProgramLogResult.ErrorNoProgramFound)
                return;

            if (IsSkippingNeeded)
            {
                OnAckStart(true);
                return;
            }

            RefreshNodeInfoOnModule();
            if (!CPasswordDlg)
            {
                if (CanRaiseRunningEvent)
                    RaiseRunningEvent();
                CurrentACState = ACStateEnum.SMRunning;
            }
            AlarmsAsText.ValueT = CMessageText;
        }

        private void KeySwitch_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_KeySwitch != null && _KeySwitch.ValueT 
                && !_AckTiggeredOverSwitch
                && IsEnabledAckStart())
            {
                _AckTiggeredOverSwitch = true;
                AckStart();
                //this.ApplicationManager.ApplicationQueue.Add(() => AckStart());
            }
        }

        public override void SMRunning()
        {
            RefreshNodeInfoOnModule();
            if (AlarmsAsText.ValueT != CMessageText)
                AlarmsAsText.ValueT = CMessageText;
        }

        public override void SMCompleted()
        {
            _AckTiggeredOverSwitch = false;
            AlarmsAsText.ValueT = "";
            RefreshNodeInfoOnModule(true);
            base.SMCompleted();
        }

        protected void RefreshNodeInfoOnModule(bool detach = false)
        {
            if (ACOperationMode == ACOperationModes.Live)
            {
                if (ParentPWGroup != null)
                {
                    var processModule = ParentPWGroup.AccessedProcessModule;
                    if (processModule != null)
                    {
                        processModule.RefreshPWNodeInfo();
                        if (_KeySwitch == null && !detach)
                        {
                            IUserAckModule userAckModule = processModule as IUserAckModule;
                            if (userAckModule != null)
                            {
                                _KeySwitch = userAckModule.KeySwitch;
                                _KeySwitch.PropertyChanged += KeySwitch_PropertyChanged;
                            }
                        }
                    }
                }
            }
            if (_KeySwitch != null && detach)
            {
                _KeySwitch.PropertyChanged -= KeySwitch_PropertyChanged;
                _KeySwitch = null;
            }
        }
        #endregion

        #region Planning and Testing
        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList, ref DumpStats dumpStats)
        {
            base.DumpPropertyList(doc, xmlACPropertyList, ref dumpStats);

            XmlElement xmlChild = xmlACPropertyList["CMessageText"];
            if (xmlChild == null)
            {
                xmlChild = doc.CreateElement("CMessageText");
                if (xmlChild != null)
                    xmlChild.InnerText = CMessageText;
                xmlACPropertyList.AppendChild(xmlChild);
            }

            xmlChild = xmlACPropertyList["CPasswordDlg"];
            if (xmlChild == null)
            {
                xmlChild = doc.CreateElement("CPasswordDlg");
                if (xmlChild != null)
                    xmlChild.InnerText = CPasswordDlg.ToString();
                xmlACPropertyList.AppendChild(xmlChild);
            }
        }
        #endregion

        #region Client
        [ACMethodInteraction("", "en{'Acknowledge'}de{'Bestätigen'}", 800, true)]
        public virtual void AckStart()
        {
            if (!IsEnabledAckStart())
                return;
            OnAckStart(false);
        }

        [ACMethodInfo("", "en{'Acknowledge'}de{'Bestätigen'}", 801, true)]
        public virtual void AckStartUserName(string userName)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                if (ExecutingACMethod != null)
                    ExecutingACMethod.ResultValueList["UserName"] = userName;
            }

            AckStart();
        }

        protected virtual void OnAckStart(bool skipped)
        {
            AcknowledgeAllAlarms();
            var appManager = this.ApplicationManager;
            if (appManager != null && Thread.CurrentThread != appManager.ApplicationQueue.ThreadOfQueue)
            {
                this.ApplicationManager.ApplicationQueue.Add(() =>
                    {
                        if (!skipped)
                            RunUserACUrlCmd();
                        CurrentACState = ACStateEnum.SMCompleted;
                    });
            }
            else
            {
                if (!skipped)
                    RunUserACUrlCmd();
                CurrentACState = ACStateEnum.SMCompleted;
            }
        }

        protected virtual void RunUserACUrlCmd()
        {
            if (String.IsNullOrEmpty(ACUrlCmd))
                return;

            string[] acUrlCmds = ACUrlCmd.Split(';');
            foreach (string acUrlCmd in acUrlCmds)
            {
                ACComponent component = this;
                string cmd = acUrlCmd;
                if (acUrlCmd.StartsWith(nameof(PWGroup.AccessedProcessModule)))
                {
                    PWGroup parentGroup = ParentACComponent as PWGroup;
                    if (parentGroup == null)
                        return;
                    component = parentGroup.AccessedProcessModule;
                    cmd = acUrlCmd.Substring(nameof(PWGroup.AccessedProcessModule).Length);
                }
                if (component != null)
                    component.ACUrlCommand(cmd);
            }
        }

        public virtual bool IsEnabledAckStart()
        {
            return CurrentACState == ACStateEnum.SMStarting || CurrentACState == ACStateEnum.SMRunning;
        }

        public const string MN_AckStartClient = "AckStartClient";
        [ACMethodInteractionClient("", "en{'Acknowledge'}de{'Bestätigen'}", 450, false)]
        public static void AckStartClient(IACComponent acComponent)
        {
            ACComponent _this = acComponent as ACComponent;
            if (!IsEnabledAckStartClient(acComponent))
                return;
            ACStateEnum acState = (ACStateEnum) _this.ACUrlCommand("ACState");

            // If needs Password
            if (acState == ACStateEnum.SMStarting)
            {
                string bsoName = "BSOChangeMyPW";
                ACBSO childBSO = acComponent.Root.Businessobjects.ACUrlCommand("?" + bsoName) as ACBSO;
                if (childBSO == null)
                    childBSO = acComponent.Root.Businessobjects.StartComponent(bsoName, null, new object[] { }) as ACBSO;
                if (childBSO == null)
                    return;
                VBDialogResult dlgResult = childBSO.ACUrlCommand("!ShowCheckUserDialog") as VBDialogResult;
                if (dlgResult != null && dlgResult.SelectedCommand == eMsgButton.OK)
                {
                    string userName = "";
                    if (dlgResult.ReturnValue != null)
                        userName = (dlgResult.ReturnValue as ACValueItem)?.Value?.ToString();

                    if (string.IsNullOrEmpty(userName))
                        acComponent.ACUrlCommand("!" + nameof(AckStart));
                    else
                        acComponent.ACUrlCommand("!" + nameof(AckStartUserName), userName);
                }
                childBSO.Stop();
            }
            else
                acComponent.ACUrlCommand("!" + nameof(AckStart));
        }

        public static bool IsEnabledAckStartClient(IACComponent acComponent)
        {
            ACComponent _this = acComponent as ACComponent;
            ACStateEnum acState = (ACStateEnum) _this.ACUrlCommand("ACState");
            return acState == ACStateEnum.SMRunning || acState == ACStateEnum.SMStarting;
        }
        #endregion

        #endregion

    }
}
