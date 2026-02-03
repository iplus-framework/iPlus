// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using gip.core.datamodel;
using static gip.core.datamodel.Global;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'AI Decision'}de{'AI Entscheidung'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeDecisionMsgAI : PWNodeDecisionMsg
    {
        #region Constructors 
        public const string C_InitialPrompt = "You are currently being used within a workflow decision node (class PWNodeDecisionMsgAI and the ACUrl is {this}). Therefore, perform the subsequent task or question without prompting, because the user cannot answer you until you have completed it. If the result is positive, call the 'Complete' method via execute_acurl_command with '{this}!Complete'. If the result is negative, call '{this}!Repeat'. Here is your task:";
        public const string C_StandardBotBSOName = "BSOChatBot";
        static PWNodeDecisionMsgAI()
        {
            List<ACMethodWrapper> wrappers = ACMethod.OverrideFromBase(typeof(PWNodeDecisionMsgAI), ACStateConst.SMStarting);
            if (wrappers != null)
            {
                foreach (ACMethodWrapper wrapper in wrappers)
                {
                    wrapper.Method.ParameterValueList.Add(new ACValue("ChatBot", typeof(string), C_StandardBotBSOName, Global.ParamOption.Optional));
                    wrapper.ParameterTranslation.Add("ChatBot", "en{'Chatbot BSO ACIdentifier'}de{'Chatbot BSO ACIdentifier'}");
                    wrapper.Method.ParameterValueList.Add(new ACValue("ModelName", typeof(string), "", Global.ParamOption.Optional));
                    wrapper.ParameterTranslation.Add("ModelName", "en{'AI Model name'}de{'AI Modellname'}");
                    wrapper.Method.ParameterValueList.Add(new ACValue("ChatImages", typeof(string), "", Global.ParamOption.Optional));
                    wrapper.ParameterTranslation.Add("ChatImages", "en{'Semicolon-separated list of paths or http-URL'}de{'Semikolon-getrennte Liste von Pfaden oder http-URL'}");
                    wrapper.Method.ParameterValueList.Add(new ACValue("InitialPrompt", typeof(string), C_InitialPrompt, Global.ParamOption.Required));
                    wrapper.ParameterTranslation.Add("InitialPrompt", "en{'Initial Prompt'}de{'Initial prompt'}");
                    Dictionary<string, string> resultTranslation = new Dictionary<string, string>();
                    wrapper.Method.ResultValueList.Add(new ACValue("ChatOutput", typeof(string), "", Global.ParamOption.Optional));
                    wrapper.ResultTranslation.Add("ChatOutput", "en{'Chat Output'}de{'Chat Ausgabe'}");

                }
            }
            RegisterExecuteHandler(typeof(PWNodeDecisionMsgAI), HandleExecuteACMethod_PWNodeDecisionMsgAI);
        }

        public PWNodeDecisionMsgAI(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier) 
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
           if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            ShutdownAIAgent();
            return await base.ACDeInit(deleteACClassTask);
        }

        #endregion


        #region Properties
        protected string ModelName
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("ModelName");
                    if (acValue != null)
                    {
                        return acValue.ParamAsString;
                    }
                }
                return "";
            }
        }

        protected string ChatImages
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("ChatImages");
                    if (acValue != null)
                    {
                        return acValue.ParamAsString;
                    }
                }
                return "";
            }
        }

        protected string InitialPrompt
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("InitialPrompt");
                    if (acValue != null)
                    {
                        return string.IsNullOrEmpty(acValue.ParamAsString) ? C_InitialPrompt : acValue.ParamAsString;
                    }
                }
                return C_InitialPrompt;
            }
        }

        protected string FormattedInitialPrompt
        {
            get
            {
                return InitialPrompt.Replace("{this}", this.ACUrl);
            }
        }

        protected string ChatBotBSOName
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("ChatBot");
                    if (acValue != null)
                    {
                        return string.IsNullOrEmpty(acValue.ParamAsString) ? C_StandardBotBSOName : acValue.ParamAsString;
                    }
                }
                return C_StandardBotBSOName;
            }
        }

        protected ACRef<IACAIAgent> _AIAgent = null;
        public IACAIAgent AIAgent
        {
            get
            {
                return _AIAgent?.ValueT;
            }
        }

        public void StartAIAgent()
        {
            if (_AIAgent == null || _AIAgent.ValueT == null)
            {
                var aiAgent = this.Root.Businessobjects.StartComponent(ChatBotBSOName, null, new object[] { }) as IACAIAgent;
                if (aiAgent != null)
                    _AIAgent = new ACRef<IACAIAgent>(aiAgent, this);
            }
        }

        public void ShutdownAIAgent()
        {
            if (_AIAgent != null && _AIAgent.ValueT != null)
            {
                _AIAgent.ValueT.Stop();
                _AIAgent.Detach();
                _AIAgent = null; // Clear the reference to allow garbage collection
            }
        }

        [ACPropertyBindingSource(400, "", "en{'Chat Output'}de{'Chat Ausgabe'}", "", false, false, Description = "Last answer from the agent after work is completed.")]
        public IACContainerTNet<string> ChatOutput { get; set; }

        [ACPropertyBindingSource(401, "", "en{'Is Agent Running?'}de{'Wird der Agent ausgeführt?'}", "", false, false)]
        public IACContainerTNet<bool> IsAgentRunning { get; set; }
        #endregion


        #region State
        public override void SMIdle()
        {
            // If LLM has called the desicion methods via MCP-API, than don't shutdown the agent until OnChatBotMessageProcessed has not been called back
            if (!IsAgentRunning.ValueT)
                ShutdownAIAgent();
            base.SMIdle();
        }

        protected async Task SendMessageToChatBot()
        {
            StartAIAgent();
            IACAIAgent agent = AIAgent;
            if (agent != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(ModelName))
                    {
                        (bool success, string message) result = agent.SelectChatClientSettingsByModelName(ModelName);
                        if (!result.success)
                            this.Messages.LogError(this.GetACUrl(), nameof(SendMessageToChatBot), $"Failed to select chat client settings for model '{ModelName}': {result.message}");
                    }
                    agent.ImagePaths.AddRange(ResolveChatImages());
                    agent.ChatInput = FormattedInitialPrompt + "\r\n" + this.CMessageText;
                    IsAgentRunning.ValueT = true;
                    agent.PropertyChanged += Agent_PropertyChanged;
                    await agent.SendMessage();
                }
                catch (Exception ex)
                {
                    this.Messages.LogError(this.GetACUrl(), nameof(SendMessageToChatBot), $"Error while sending message to chat bot: {ex.Message}");
                    IsAgentRunning.ValueT = false;
                }
                finally
                {
                    if (agent != null)
                        agent.PropertyChanged -= Agent_PropertyChanged;
                    OnChatBotMessageProcessed();
                    IsAgentRunning.ValueT = false;
                    ShutdownAIAgent();
                }
            }
        }

        private void Agent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IACAIAgent.ChatOutput))
            {
                IACAIAgent agent = AIAgent;
                if (agent != null)
                {
                    if (ExecutingACMethod != null)
                        ExecutingACMethod.ResultValueList["ChatOutput"] = agent.ChatOutput;
                    ChatOutput.ValueT = agent.ChatOutput;
                }
            }
        }

        protected virtual void OnChatBotMessageProcessed()
        {
            IACAIAgent agent = AIAgent;
            if (agent != null)
            {
                if (!string.IsNullOrEmpty(agent.ChatOutput))
                {
                    ChatOutput.ValueT = agent.ChatOutput;
                    if (ExecutingACMethod != null)
                        ExecutingACMethod.ResultValueList["ChatOutput"] = agent.ChatOutput;
                    // TODO: Log chat output afterwards
                    //else if (PreviousACMethod != null)
                    //    PreviousACMethod.ResultValueList["ChatOutput"] = agent.ChatOutput;
                }
                if (CurrentACState >= ACStateEnum.SMStarting && CurrentACState < ACStateEnum.SMCompleted)
                {
                    // Agent did not make a decision. Please investigate and complete the node manually.
                    Msg msg = new Msg(this, eMsgLevel.Error, nameof(PWNodeDecisionMsgAI), "OnChatBotMessageProcessed()", 1000, "Error50716");
                    OnNewAlarmOccurred(ProcessAlarm, msg, true);
                }
            }
        }

        [ACMethodState("en{'Executing'}de{'Ausführend'}", 20, true)]
        public override void SMStarting()
        {
            if (!string.IsNullOrEmpty(CMessageText) && ForceEventPoint <= 0 && !IsAgentRunning.ValueT)
                _ = Task.Run(async () => await SendMessageToChatBot());
            base.SMStarting();
        }

        public override void SMRunning()
        {
            base.SMRunning();
            if (string.IsNullOrEmpty(CMessageText))
                Complete();
        }

        protected virtual List<string> ResolveChatImages()
        {
            return PWNodeUserAckAI.ResolveChatImages(this, ChatImages);
        }

        #endregion


        #region Planning and Testing
        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList, ref DumpStats dumpStats)
        {
            base.DumpPropertyList(doc, xmlACPropertyList, ref dumpStats);

            XmlElement xmlChild = xmlACPropertyList["ModelName"];
            if (xmlChild == null)
            {
                xmlChild = doc.CreateElement("ModelName");
                if (xmlChild != null)
                    xmlChild.InnerText = CMessageText;
                xmlACPropertyList.AppendChild(xmlChild);
            }

            xmlChild = xmlACPropertyList["ChatImages"];
            if (xmlChild == null)
            {
                xmlChild = doc.CreateElement("ChatImages");
                if (xmlChild != null)
                    xmlChild.InnerText = ChatImages;
                xmlACPropertyList.AppendChild(xmlChild);
            }

            xmlChild = xmlACPropertyList["InitialPrompt"];
            if (xmlChild == null)
            {
                xmlChild = doc.CreateElement("InitialPrompt");
                if (xmlChild != null)
                    xmlChild.InnerText = InitialPrompt;
                xmlACPropertyList.AppendChild(xmlChild);
            }

        }
        #endregion


        #region Execute-Helper-Handlers
        public static bool HandleExecuteACMethod_PWNodeDecisionMsgAI(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            return HandleExecuteACMethod_PWNodeDecisionMsg(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

    }
}
