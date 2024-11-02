// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Entscheidungsknoten mit zwei möglichen Ausgängen PWPointOut und PWPointElseOut
    /// Methoden zur Steuerung von außen: 
    /// -Start()    Starten des Processes
    ///
    /// Mögliche ACState:
    /// SMIdle      (Definiert in ACComponent)
    /// SMStarting (Definiert in PWNode)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Decision(Function)'}de{'Entscheidung(Funktion)'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeDecisionFunc : PWBaseExecutable
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

        static PWNodeDecisionFunc()
        {
            ACMethod method;
            method = new ACMethod(ACStateConst.SMStarting);
            Dictionary<string, string> paramTranslation = new Dictionary<string, string>();
            method.ParameterValueList.Add(new ACValue("ForceEventPoint", typeof(ushort), 0, Global.ParamOption.Required));
            paramTranslation.Add("ForceEventPoint", "en{'Raise Event [Off=0], [Below=1], [Sideward=2]'}de{'Erzwinge Ereignis [Aus=0], [Unten=1], [Seitlich/Else=2]'}");
            method.ParameterValueList.Add(new ACValue("Repeats", typeof(UInt32), 0, Global.ParamOption.Optional));
            paramTranslation.Add("Repeats", "en{'Repeats'}de{'Wiederholungen'}");
            var wrapper = new ACMethodWrapper(method, "en{'Configuration'}de{'Konfiguration'}", typeof(PWNodeDecisionFunc), paramTranslation, null);
            ACMethod.RegisterVirtualMethod(typeof(PWNodeDecisionFunc), ACStateConst.SMStarting, wrapper);


            ACEventArgs TMP;
            _SVirtualEventArgs = new Dictionary<string,ACEventArgs>(PWBaseExecutable.SVirtualEventArgs, StringComparer.OrdinalIgnoreCase);
            TMP = new ACEventArgs();
            TMP.Add(new ACValue("TimeInfo", typeof(PATimeInfo), null, Global.ParamOption.Required));
            _SVirtualEventArgs.Add("PWPointElseOut", TMP);

            RegisterExecuteHandler(typeof(PWNodeDecisionFunc), HandleExecuteACMethod_PWNodeDecisionFunc);
        }

        public PWNodeDecisionFunc(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            //IsFireEdgeEvents = false;
            PWPointElseOut = new ACPointEvent(this, "PWPointElseOut", 0);
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

#endregion

#region Points

        protected ACPointEvent _PWPointElseOut;

        [ACPropertyEventPoint(9999, true)]
        public ACPointEvent PWPointElseOut
        {
            get
            {
                return _PWPointElseOut;
            }
            set
            {
                _PWPointElseOut = value;
            }
        }

        #endregion

        #region Properties
        protected ushort ForceEventPoint
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("ForceEventPoint");
                    if (acValue != null)
                    {
                        return acValue.ParamAsUInt16;
                    }
                }
                return 0;
            }
        }

        protected UInt32 Repeats
        {
            get
            {
                var method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("Repeats");
                    if (acValue != null)
                    {
                        return acValue.ParamAsUInt16;
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

        #region Public

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(RaiseElseEventAndComplete):
                    RaiseElseEventAndComplete();
                    return true;
                //case Const.IsEnabledPrefix + "RaiseOutEvent":
                //    result = IsEnabledRaiseElseEventAndComplete();
                //    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public static bool HandleExecuteACMethod_PWNodeDecisionFunc(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_PWBaseExecutable(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        #endregion

        #region Protected
        public override void SMIdle()
        {
            if (ParentPWGroup != null)
            {
                var processModule = ParentPWGroup.AccessedProcessModule;
                if (processModule != null)
                    processModule.RefreshPWNodeInfo();
            }

            base.SMIdle();
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

            UInt32 repeats = Repeats;
            if (ForceEventPoint == 2)
            {
                if (repeats > 0 && IterationCount.ValueT % repeats != 0)
                    RaiseOutEventAndComplete();
                else
                    RaiseElseEventAndComplete();
            }
            else //if (ForceEventPoint <= 1)
            {
                if (repeats > 0 && IterationCount.ValueT % repeats != 0)
                    RaiseElseEventAndComplete();
                else
                    RaiseOutEventAndComplete();
            }
        }

        protected override TimeSpan GetPlannedDuration()
        {
            return TimeSpan.Zero;
        }

        public override void RaiseOutEvent()
        {
            RaiseOutEventAndComplete();
        }

        protected void RaiseOutEventAndComplete()
        {
            ResetInEvents();
            RecalcTimeInfo();
            TimeInfo?.ValueT?.ActualTimes.ChangeTime(TimeInfo.ValueT.ActualTimes.StartTime == DateTime.MinValue ? DateTimeUtils.NowDST : TimeInfo.ValueT.ActualTimes.StartTime, DateTimeUtils.NowDST);
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("PWPointOut", VirtualEventArgs);
            eventArgs.GetACValue("TimeInfo").Value = TimeInfo?.ValueT;
            if (CurrentACState == ACStateEnum.SMStarting || CurrentACState == ACStateEnum.SMRunning)
            {
                FinishProgramLog(ExecutingACMethod);
                IterationCount.ValueT++;
                Reset();
            }
            PWPointOut.Raise(eventArgs);
        }


        [ACMethodInteraction("Process", "en{'Raise ELSE event'}de{'SONST-Ereignis auslösen'}", 2004, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public void RaiseElseEventAndComplete()
        {
            ResetInEvents();
            RecalcTimeInfo();
            TimeInfo?.ValueT?.ActualTimes.ChangeTime(TimeInfo.ValueT.ActualTimes.StartTime == DateTime.MinValue ? DateTimeUtils.NowDST : TimeInfo.ValueT.ActualTimes.StartTime, DateTimeUtils.NowDST);
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("PWPointElseOut", VirtualEventArgs);
            eventArgs.GetACValue("TimeInfo").Value = TimeInfo?.ValueT;
            if (CurrentACState == ACStateEnum.SMStarting || CurrentACState == ACStateEnum.SMRunning)
            {
                FinishProgramLog(ExecutingACMethod);
                IterationCount.ValueT++;
                Reset();
            }
            PWPointElseOut.Raise(eventArgs);
        }

        #endregion

    }
}
