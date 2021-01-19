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

        #region Public

        #region Execute-Helper-Handlers
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

        public override void SMStarting()
        {
            if (ParentPWGroup != null)
            {
                var processModule = ParentPWGroup.AccessedProcessModule;
                if (processModule != null)
                    processModule.RefreshPWNodeInfo();
            }
            base.SMStarting();
        }

        protected override TimeSpan GetPlannedDuration()
        {
            return TimeSpan.Zero;
        }

        protected void RaiseOutEventAndComplete()
        {
            RecalcTimeInfo();
            TimeInfo.ValueT.ActualTimes.ChangeTime(TimeInfo.ValueT.ActualTimes.StartTime == DateTime.MinValue ? DateTime.Now : TimeInfo.ValueT.ActualTimes.StartTime, DateTime.Now);
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("PWPointOut", VirtualEventArgs);
            eventArgs.GetACValue("TimeInfo").Value = TimeInfo.ValueT;
            if (CurrentACState == ACStateEnum.SMStarting || CurrentACState == ACStateEnum.SMRunning)
            {
                FinishProgramLog(ExecutingACMethod);
                IterationCount.ValueT++;
                Reset();
            }
            PWPointOut.Raise(eventArgs);
        }


        protected void RaiseElseEventAndComplete()
        {
            RecalcTimeInfo();
            TimeInfo.ValueT.ActualTimes.ChangeTime(TimeInfo.ValueT.ActualTimes.StartTime == DateTime.MinValue ? DateTime.Now : TimeInfo.ValueT.ActualTimes.StartTime, DateTime.Now);
            ACEventArgs eventArgs = ACEventArgs.GetVirtualEventArgs("PWPointElseOut", VirtualEventArgs);
            eventArgs.GetACValue("TimeInfo").Value = TimeInfo.ValueT;
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
