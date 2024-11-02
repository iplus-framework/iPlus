// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.autocomponent
{
    /// <summary>
    /// PWNodeAnd is a derivative of PWBaseInOut and act as logic AND-gate. It uses PWPointIn.IsActiveAND property to switch the logical gate.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'And'}de{'Und'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeAnd : PWBaseInOut
    {
        #region c´tors
        public PWNodeAnd(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            //IsFireEdgeEvents = false;
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

        [ACMethodInteraction("Process", "en{'Reinterpret AND-Gate'}de{'UND-Gatter erneut auswerten'}", (short)MISort.Start, true)]
        public virtual void ReinterpretGate()
        {
            if (!IsEnabledReinterpretGate())
                return;
            if (PWPointIn.IsActiveAND)
            {
                PWPointIn.ResetActiveStates();
                RaiseOutEvent();
            }
        }

        public bool IsEnabledReinterpretGate()
        {
            return true;
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
