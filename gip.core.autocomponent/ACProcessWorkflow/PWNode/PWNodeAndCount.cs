// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Configuration;

namespace gip.core.autocomponent
{
    /// <summary>
    /// PWNodeAndCount is a derivative of PWBaseInOut and act as logic AND-gate. It uses PWPointIn.IsActiveAND property to switch the logical gate.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'And Count'}de{'Und Anzahl'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeAndCount : PWBaseExecutable
    {
        #region c´tors
        static PWNodeAndCount()
        {
            ACMethod method;
            method = new ACMethod(ACStateConst.SMStarting);
            Dictionary<string, string> paramTranslation = new Dictionary<string, string>();
            method.ParameterValueList.Add(new ACValue("AndCount", typeof(UInt16), (UInt16)0, Global.ParamOption.Required));
            paramTranslation.Add("AndCount", "en{'Count of inputs to switch'}de{'Anzahl Eingänge zum schalten'}");
            var wrapper = new ACMethodWrapper(method, "en{'And Count'}de{'Und Anzahl'}", typeof(PWNodeAndCount), paramTranslation, null);
            ACMethod.RegisterVirtualMethod(typeof(PWNodeAndCount), ACStateConst.SMStarting, wrapper);
            RegisterExecuteHandler(typeof(PWNodeAndCount), HandleExecuteACMethod_PWNodeAndCount);
        }

        public PWNodeAndCount(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }
        #endregion

        #region Properties
        public UInt16 AndCount
        {
            get
            {
                ACMethod method = MyConfiguration;
                if (method != null)
                {
                    var acValue = method.ParameterValueList.GetACValue("AndCount");
                    if (acValue != null)
                        return acValue.ParamAsUInt16;
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
            UInt16 andCount = AndCount;
            if (   (andCount == 0 && PWPointIn.IsActiveAND)
                || (PWPointIn.ActiveEdgeCount >= andCount) )
            {
                PWPointIn.ResetActiveStates();
                RaiseOutEvent();
            }
        }

        public bool IsEnabledReinterpretGate()
        {
            return true;
        }

        public static bool HandleExecuteACMethod_PWNodeAndCount(out object result, IACComponent acComponent, string acMethodName, gip.core.datamodel.ACClassMethod acClassMethod, params object[] acParameter)
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
