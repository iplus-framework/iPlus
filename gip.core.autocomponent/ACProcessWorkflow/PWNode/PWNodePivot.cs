// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using gip.core.datamodel;
using static gip.core.datamodel.Global;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Pivot desicion'}de{'Drehpunkt Entscheidung'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodePivot : PWNodeDecisionFunc
    {
        #region Constructors 

        static PWNodePivot()
        {
            ACMethod method;
            method = new ACMethod(ACStateConst.SMStarting);
            Dictionary<string, string> paramTranslation = new Dictionary<string, string>();
            method.ParameterValueList.Add(new ACValue("ForceEventPoint", typeof(ushort), 0, Global.ParamOption.Required));
            paramTranslation.Add("ForceEventPoint", "en{'Raise Event [Off=0], [Below=1], [Sideward=2]'}de{'Erzwinge Ereignis [Aus=0], [Unten=1], [Seitlich/Else=2]'}");
            method.ParameterValueList.Add(new ACValue("Repeats", typeof(UInt32), 0, Global.ParamOption.Optional));
            paramTranslation.Add("Repeats", "en{'Repeats'}de{'Wiederholungen'}");
            method.ParameterValueList.Add(new ACValue("PivotMode", typeof(ushort), 0, Global.ParamOption.Required));
            paramTranslation.Add("PivotMode", "en{'[Not B if A: 0]'}de{'[Nicht B wenn A: 0]'}");
            var wrapper = new ACMethodWrapper(method, "en{'Configuration'}de{'Konfiguration'}", typeof(PWNodePivot), paramTranslation, null);
            ACMethod.RegisterVirtualMethod(typeof(PWNodePivot), ACStateConst.SMStarting, wrapper);

            RegisterExecuteHandler(typeof(PWNodePivot), HandleExecuteACMethod_PWNodePivot);
        }

        public PWNodePivot(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier) 
        {
            _PWPointIn2 = new PWPointIn(this, nameof(PWPointIn2), 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
           if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACPostInit()
        {
            PWPointIn2.SubscribeACClassWFEdgeEvents(PWPointIn2Callback);
            return base.ACPostInit();
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            return await base.ACDeInit(deleteACClassTask);
        }

        public override bool ACPreDeInit(bool deleteACClassTask = false)
        {
            bool result = base.ACPreDeInit(deleteACClassTask);
            if (!result)
                return result;
            if (deleteACClassTask)
                PWPointIn2.UnSubscribeAllEvents();
            return true;
        }
        #endregion


        #region Properties
        PWPointIn _PWPointIn2;
        [ACPropertyEventPointSubscr(9999, true, "PWPointIn2Callback")]
        public PWPointIn PWPointIn2
        {
            get
            {
                return _PWPointIn2;
            }
        }

        protected ushort PivotMode
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("PivotMode");
                    if (acValue != null)
                    {
                        return acValue.ParamAsUInt16;
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// 0 = Idle
        /// 1 = InPoint1 has raised
        /// 2 = InPoint2 has raised
        /// 3 = InPoint2 has raised first, then InPoint1
        /// 4 = InPoint1 has raised first, then InPoint2
        /// </summary>
        [ACPropertyBindingSource(500, "", "en{'Last input point who raised'}de{'Letzter Eingangspunkt der gefeuert hat'}", "", false, true)]
        public IACContainerTNet<Int16> InPointsOrder { get; set; }

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
            if (ParentPWGroup != null)
            {
                var processModule = ParentPWGroup.AccessedProcessModule;
                if (processModule != null)
                    processModule.RefreshPWNodeInfo();
            }

            if (InPointsOrder.ValueT == 1)
            {
                if (ForceEventPoint == 2)
                    RaiseElseEventAndComplete();
                else
                    RaiseOutEventAndComplete();
            }
            else if (InPointsOrder.ValueT == 2)
            {
                if (ForceEventPoint == 2)
                    RaiseOutEventAndComplete();
                else
                    RaiseElseEventAndComplete();
            }
            else if (InPointsOrder.ValueT == 3)
            {
                InPointsOrder.ValueT = 0;
                Reset();
            }
            else if (InPointsOrder.ValueT == 4)
            {
                InPointsOrder.ValueT = 0;
                Reset();
            }

            UInt32 repeats = Repeats;
            if (ForceEventPoint == 2)
            {
                //if (repeats > 0 && IterationCount.ValueT % repeats != 0)
                //    RaiseOutEventAndComplete();
                //else
                //    RaiseElseEventAndComplete();
            }
            else //if (ForceEventPoint <= 1)
            {
                //if (repeats > 0 && IterationCount.ValueT % repeats != 0)
                //    RaiseElseEventAndComplete();
                //else
                //    RaiseOutEventAndComplete();
            }
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

        public override void PWPointInCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                // Status so setzen, das Event als empfangen gekennzeichnet ist
                PWPointIn.UpdateActiveState(wrapObject);
                // Wenn alle Vorgänger ihre Events gefeuert haben, dann kann der 
                // Class aktiviert werden
                if (PWPointIn.IsActive)
                {
                    if (InPointsOrder.ValueT <= 0 || InPointsOrder.ValueT > 3)
                        InPointsOrder.ValueT = 1;
                    else if (InPointsOrder.ValueT == 2 || InPointsOrder.ValueT == 3)
                        InPointsOrder.ValueT = 3;

                    PWPointIn.ResetActiveStates();
                    Start();
                }
            }
        }

        [ACMethodInfo("Function", "en{'PWPointIn2Callback'}de{'PWPointIn2Callback'}", 9999)]
        public virtual void PWPointIn2Callback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                // Status so setzen, das Event als empfangen gekennzeichnet ist
                PWPointIn2.UpdateActiveState(wrapObject);
                // Wenn alle Vorgänger ihre Events gefeuert haben, dann kann der 
                // Class aktiviert werden
                if (PWPointIn2.IsActive)
                {
                    if (InPointsOrder.ValueT <= 0 || InPointsOrder.ValueT == 2)
                        InPointsOrder.ValueT = 2;
                    else if (InPointsOrder.ValueT == 1 || InPointsOrder.ValueT >= 4)
                        InPointsOrder.ValueT = 4;
                    //else if (InPointsOrder.ValueT == 3)
                    //    InPointsOrder.ValueT = 3;

                    PWPointIn2.ResetActiveStates();
                    Start();
                }
            }
        }
        #endregion

        #region Planning and Testing
        protected override void DumpPropertyList(XmlDocument doc, XmlElement xmlACPropertyList, ref DumpStats dumpStats)
        {
            base.DumpPropertyList(doc, xmlACPropertyList, ref dumpStats);

            XmlElement xmlChild = xmlACPropertyList["PivotMode"];
            if (xmlChild == null)
            {
                xmlChild = doc.CreateElement("PivotMode");
                if (xmlChild != null)
                    xmlChild.InnerText = PivotMode.ToString();
                xmlACPropertyList.AppendChild(xmlChild);
            }
        }
        #endregion


        #region Interaction-Methods

        /// <summary>Resets all received Events (sets ACPointEventSubscrWrap&lt;ACComponent&gt;.IsActive false at all event-subscriptions in the connectionlist)</summary>
        [ACMethodInteraction("Process", "en{'Reset input events of flag'}de{'Eingangs-Ereignisse des Merkers zurücksetzen'}", 2000, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void ResetIn2Events()
        {
            if (!IsEnabledResetIn2Events())
                return;
            PWPointIn2.ResetActiveStates();
            InPointsOrder.ValueT = 0;
        }

        public virtual bool IsEnabledResetIn2Events()
        {
            return true;
        }

        #endregion


        #region Execute-Helper-Handlers
        public static bool HandleExecuteACMethod_PWNodePivot(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            //result = null;
            //switch (acMethodName)
            //{
            //    //case Const.AskUserPrefix + "Repeat":
            //    //    result = AskUserRepeat(acComponent);
            //    //    return true;
            //    //case Const.AskUserPrefix + "Complete":
            //    //    result = AskUserComplete(acComponent);
            //    //    return true;
            //}
            return HandleExecuteACMethod_PWNodeDecisionFunc(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(PWPointIn2Callback):
                    PWPointIn2Callback(acParameter[0] as IACPointNetBase, acParameter[1] as ACEventArgs, acParameter[2] as IACObject);
                    return true;
                case nameof(ResetIn2Events):
                    ResetIn2Events();
                    return true;
                case nameof(IsEnabledResetIn2Events):
                    result = IsEnabledResetIn2Events();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

    }
}
