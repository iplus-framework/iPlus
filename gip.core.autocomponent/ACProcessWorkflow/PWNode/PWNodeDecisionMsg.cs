using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Decision question'}de{'Entscheidungsfrage'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeDecisionMsg : PWNodeDecisionFunc, IACMyConfigCache
    {
        #region Constructors 

        static PWNodeDecisionMsg()
        {
            ACMethod method;
            method = new ACMethod(ACStateConst.SMStarting);
            Dictionary<string, string> paramTranslation = new Dictionary<string, string>();
            method.ParameterValueList.Add(new ACValue("OutIsRepeat", typeof(bool), false, Global.ParamOption.Required));
            paramTranslation.Add("OutIsRepeat", "en{'Raise Out-event for repetition'}de{'Ausgangsevent zum Wiederholen auslösen'}");
            method.ParameterValueList.Add(new ACValue("MessageText", typeof(string), "", Global.ParamOption.Required));
            paramTranslation.Add("MessageText", "en{'Question text'}de{'Abfragetext'}");
            method.ParameterValueList.Add(new ACValue("PasswordDlg", typeof(bool), false, Global.ParamOption.Required));
            paramTranslation.Add("PasswordDlg", "en{'With password dialogue'}de{'Mit Passwort-Dialog'}");
            var wrapper = new ACMethodWrapper(method, "en{'Configuration'}de{'Konfiguration'}", typeof(PWNodeDecisionMsg), paramTranslation, null);
            ACMethod.RegisterVirtualMethod(typeof(PWNodeDecisionMsg), ACStateConst.SMStarting, wrapper);

            RegisterExecuteHandler(typeof(PWNodeDecisionMsg), HandleExecuteACMethod_PWNodeDecisionMsg);
        }

        public PWNodeDecisionMsg(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier) 
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
           if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            ClearMyConfiguration();
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion


        #region Properties
        private ACMethod _MyConfiguration;
        public ACMethod MyConfiguration
        {
            get
            {

                using (ACMonitor.Lock(_20015_LockValue))
                {
                    if (_MyConfiguration != null)
                        return _MyConfiguration;
                }

                var myNewConfig = NewACMethodWithConfiguration();
                _MyConfiguration = myNewConfig;
                return myNewConfig;
            }
        }

        public void ClearMyConfiguration()
        {

            using (ACMonitor.Lock(_20015_LockValue))
            {
                _MyConfiguration = null;
            }
            this.HasRules.ValueT = 0;
        }

        protected bool OutIsRepeat
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("OutIsRepeat");
                    if (acValue != null)
                    {
                        return acValue.ParamAsBoolean;
                    }
                }
                return false;
            }
        }

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

        [ACPropertyBindingSource(103, "Values", "en{'Password-Dialog needed'}de{'Passwort-Dialog nötig'}", "", false, false)]
        public IACContainerTNet<bool> NeedsPasswordDlg
        {
            get; set;
        }


        #endregion


        #region State
        public override void SMIdle()
        {
            AlarmsAsText.ValueT = "";
            base.SMIdle();
            RefreshNodeInfoOnModule();
        }

        [ACMethodState("en{'Executing'}de{'Ausführend'}", 20, true)]
        public override void SMStarting()
        {
            if (NeedsPasswordDlg != null)
                NeedsPasswordDlg.ValueT = CPasswordDlg;
            RefreshNodeInfoOnModule();
            CurrentACState = ACStateEnum.SMRunning;
            AlarmsAsText.ValueT = CMessageText;
        }


        [ACMethodState("en{'Running'}de{'Läuft'}", 30, true)]
        public virtual void SMRunning()
        {
        }


        private void RefreshNodeInfoOnModule()
        {
            if (ACOperationMode == ACOperationModes.Live)
            {
                if (ParentPWGroup != null)
                {
                    var processModule = ParentPWGroup.AccessedProcessModule;
                    if (processModule != null)
                    {
                        processModule.RefreshPWNodeInfo();
                    }
                }
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

            xmlChild = xmlACPropertyList["OutIsRepeat"];
            if (xmlChild == null)
            {
                xmlChild = doc.CreateElement("OutIsRepeat");
                if (xmlChild != null)
                    xmlChild.InnerText = CPasswordDlg.ToString();
                xmlACPropertyList.AppendChild(xmlChild);
            }
        }
        #endregion


        #region Interaction-Methods
        [ACMethodInteraction("", "en{'Repeat'}de{'Wiederholen'}", 300, true)]
        public void Repeat()
        {
            if (!IsEnabledRepeat())
                return;
            if (OutIsRepeat)
                RaiseOutEventAndComplete();
            else
                RaiseElseEventAndComplete();
        }

        public bool IsEnabledRepeat()
        {
            return CurrentACState == ACStateEnum.SMRunning;
        }

        public static bool AskUserRepeat(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            object needsPWDlg = acComponent.ACUrlCommand("NeedsPasswordDlg");
            if (needsPWDlg == null)
                return true;
            bool isAllowed = true;
            if ((bool)needsPWDlg)
            {
                string bsoName = "BSOChangeMyPW";
                ACBSO childBSO = acComponent.Root.Businessobjects.ACUrlCommand("?" + bsoName) as ACBSO;
                if (childBSO == null)
                    childBSO = acComponent.Root.Businessobjects.StartComponent(bsoName, null, new object[] { }) as ACBSO;
                if (childBSO == null)
                    return false;
                VBDialogResult dlgResult = childBSO.ACUrlCommand("!ShowCheckUserDialog") as VBDialogResult;
                if (dlgResult != null && dlgResult.SelectedCommand == eMsgButton.OK)
                {
                    isAllowed = true;
                }
                childBSO.Stop();
            }
            return isAllowed;
        }

        [ACMethodInteraction("", "en{'Complete'}de{'Beenden'}", 301, true)]
        public void Complete()
        {
            if (!IsEnabledComplete())
                return;
            if (OutIsRepeat)
                RaiseElseEventAndComplete();
            else
                RaiseOutEventAndComplete();
        }

        public bool IsEnabledComplete()
        {
            return CurrentACState == ACStateEnum.SMRunning;
        }

        public static bool AskUserComplete(IACComponent acComponent)
        {
            if (acComponent == null)
                return false;
            object needsPWDlg = acComponent.ACUrlCommand("NeedsPasswordDlg");
            if (needsPWDlg == null)
                return true;
            bool isAllowed = true;
            if ((bool)needsPWDlg)
            {
                string bsoName = "BSOChangeMyPW";
                ACBSO childBSO = acComponent.Root.Businessobjects.ACUrlCommand("?" + bsoName) as ACBSO;
                if (childBSO == null)
                    childBSO = acComponent.Root.Businessobjects.StartComponent(bsoName, null, new object[] { }) as ACBSO;
                if (childBSO == null)
                    return false;
                VBDialogResult dlgResult = childBSO.ACUrlCommand("!ShowCheckUserDialog") as VBDialogResult;
                if (dlgResult != null && dlgResult.SelectedCommand == eMsgButton.OK)
                {
                    isAllowed = true;
                }
                childBSO.Stop();
            }
            return isAllowed;
        }

        #endregion


        #region Execute-Helper-Handlers
        public static bool HandleExecuteACMethod_PWNodeDecisionMsg(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case Const.AskUserPrefix + "Repeat":
                    result = AskUserRepeat(acComponent);
                    return true;
                case Const.AskUserPrefix + "Complete":
                    result = AskUserComplete(acComponent);
                    return true;
            }
            return HandleExecuteACMethod_PWNodeDecisionFunc(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case ACStateConst.SMRunning:
                    SMRunning();
                    return true;
                case "Repeat":
                    Repeat();
                    return true;
                case Const.IsEnabledPrefix + "Repeat":
                    result = IsEnabledRepeat();
                    return true;
                case "Complete":
                    Complete();
                    return true;
                case Const.IsEnabledPrefix + "Complete":
                    result = IsEnabledComplete();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

    }
}
