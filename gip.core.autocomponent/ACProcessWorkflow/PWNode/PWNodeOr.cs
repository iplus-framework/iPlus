using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// PWNodeOr is a derivative of PWBaseInOut and act as logic OR-gate. It uses PWPointIn.IsActiveOR property to switch the logical gate.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Or'}de{'Oder'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeOr : PWBaseInOut
    {
        #region c´tors
        public PWNodeOr(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier) 
        {
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

        [ACMethodInteraction("Process", "en{'Reinterpret OR-Gate'}de{'ODER-Gatter erneut auswerten'}", (short)MISort.Start, true)]
        public virtual void ReinterpretGate()
        {
            if (!IsEnabledReinterpretGate())
                return;
            if (PWPointIn.IsActiveOR)
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
        /// <summary>
        /// Sonderbehandlung, da der ClassOr beim ersten Event schon weiter schaltet
        /// </summary>
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
