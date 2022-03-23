using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Xml;

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
            var wrapper = new ACMethodWrapper(method, "en{'User Acknowledge'}de{'Benutzerbestätigung'}", typeof(PWNodeUserAck), paramTranslation, null);
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
        protected string CMessageText
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

        protected bool CPasswordDlg
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

        private IACContainerTNet<bool> _KeySwitch = null;
        bool _AckTiggeredOverSwitch = false;
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
                case MN_AckStartClient:
                    AckStartClient(acComponent);
                    return true;
                case Const.IsEnabledPrefix + MN_AckStartClient:
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
            RecalcTimeInfo();
            if (CreateNewProgramLog(NewACMethodWithConfiguration()) <= CreateNewProgramLogResult.ErrorNoProgramFound)
                return;

            RefreshNodeInfoOnModule();
            if (!CPasswordDlg)
                CurrentACState = ACStateEnum.SMRunning;
            AlarmsAsText.ValueT = CMessageText;
        }

        private void KeySwitch_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_KeySwitch != null && _KeySwitch.ValueT 
                && !_AckTiggeredOverSwitch
                && IsEnabledAckStart())
            {
                _AckTiggeredOverSwitch = true;
                this.ApplicationManager.ApplicationQueue.Add(() => AckStart());
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
        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList)
        {
            base.DumpPropertyList(doc, xmlACPropertyList);

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
            AcknowledgeAllAlarms();
            CurrentACState = ACStateEnum.SMCompleted;
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
                    acComponent.ACUrlCommand("!AckStart");
                }
                childBSO.Stop();
            }
            else
                acComponent.ACUrlCommand("!AckStart");
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
