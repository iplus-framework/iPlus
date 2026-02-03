// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Configuration;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    /// <summary>
    /// PWNodeAndFlag is a derivative of PWBaseInOut and act as logic AND-gate. It uses PWPointIn.IsActiveAND property to switch the logical gate.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'And 2-Inputs and Flag'}de{'Und 2-Eingänge und Merker'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeAndFlag : PWBaseExecutable
    {
        #region c´tors
        //static PWNodeAndFlag()
        //{
        //    ACMethod method;
        //    method = new ACMethod(ACStateConst.SMStarting);
        //    Dictionary<string, string> paramTranslation = new Dictionary<string, string>();
        //    method.ParameterValueList.Add(new ACValue("AndCount", typeof(UInt16), (UInt16)0, Global.ParamOption.Required));
        //    paramTranslation.Add("AndCount", "en{'Count of inputs to switch'}de{'Anzahl Eingänge zum schalten'}");
        //    var wrapper = new ACMethodWrapper(method, "en{'And Count'}de{'Und Anzahl'}", typeof(PWNodeAndFlag), paramTranslation, null);
        //    ACMethod.RegisterVirtualMethod(typeof(PWNodeAndFlag), ACStateConst.SMStarting, wrapper);
        //    RegisterExecuteHandler(typeof(PWNodeAndFlag), HandleExecuteACMethod_PWNodeAndFlag);
        //}

        public PWNodeAndFlag(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
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

        public override bool MustBeInsidePWGroup
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Methods
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case nameof(ReinterpretGate):
                    ReinterpretGate();
                    return true;
                case nameof(IsEnabledReinterpretGate):
                    result = IsEnabledReinterpretGate();
                    return true;
                case nameof(ResetIn2Events):
                    ResetIn2Events();
                    return true;
                case nameof(IsEnabledResetIn2Events):
                    result = IsEnabledResetIn2Events();
                    return true;
                case nameof(PWPointIn2Callback):
                    PWPointIn2Callback(acParameter[0] as IACPointNetBase, acParameter[1] as ACEventArgs, acParameter[2] as IACObject);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }

        public override void Start()
        {
        }

        [ACMethodState("en{'Executing'}de{'Ausführend'}", 20, true)]
        public override void SMStarting()
        {
        }

        [ACMethodInteraction("Process", "en{'Reinterpret Gate'}de{'Gatter erneut auswerten'}", (short)MISort.Start, true)]
        public virtual void ReinterpretGate()
        {
            if (!IsEnabledReinterpretGate())
                return;
            if (PWPointIn.IsActiveAND && PWPointIn2.IsActiveAND)
            {
                PWPointIn.ResetActiveStates();
                RaiseOutEvent();
            }
        }

        public bool IsEnabledReinterpretGate()
        {
            return true;
        }

        /// <summary>Resets all received Events (sets ACPointEventSubscrWrap&lt;ACComponent&gt;.IsActive false at all event-subscriptions in the connectionlist)</summary>
        [ACMethodInteraction("Process", "en{'Reset input events of flag'}de{'Eingangs-Ereignisse des Merkers zurücksetzen'}", 2000, true, "", Global.ACKinds.MSMethod, false, Global.ContextMenuCategory.ProcessCommands)]
        public virtual void ResetIn2Events()
        {
            if (!IsEnabledResetIn2Events())
                return;
            PWPointIn2.ResetActiveStates();
        }

        public virtual bool IsEnabledResetIn2Events()
        {
            return true;
        }

        public static bool HandleExecuteACMethod_PWNodeAndFlag(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
        {
            return HandleExecuteACMethod_PWBaseExecutable(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #region Callbacks
        public override void PWPointInCallback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                // Status so setzen, das Event als empfangen gekennzeichnet ist
                PWPointIn.UpdateActiveState(wrapObject);
                ReinterpretGate();
            }
        }

        [ACMethodInfo("Function", "en{'PWPointIn2Callback'}de{'PWPointIn2Callback'}", 9999)]
        public virtual void PWPointIn2Callback(IACPointNetBase sender, ACEventArgs e, IACObject wrapObject)
        {
            if (e != null)
            {
                // Status so setzen, das Event als empfangen gekennzeichnet ist
                PWPointIn2.UpdateActiveState(wrapObject);
                ReinterpretGate();
            }
        }
        #endregion

        #region Planning and Testing
        protected override TimeSpan GetPlannedDuration()
        {
            return TimeSpan.Zero;
        }
        #endregion

        #endregion
    }
}
