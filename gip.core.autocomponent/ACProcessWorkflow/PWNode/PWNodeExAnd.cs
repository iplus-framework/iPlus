using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// PWNodeXAnd is a derivative of PWBaseInOut and is a special iPlus logic gate. It uses PWPointIn.IsActiveExAND- and PWPointIn.IsActiveAND- property to switch the logical gate.
    /// An EXAND gate is a special feature in iPlus, which means that only one output event was received and all other output events were not fired and the group of the workflow node to which they belong is not active.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Exclusive AND'}de{'Exclusives-UND'}", Global.ACKinds.TPWNodeStatic, Global.ACStorableTypes.Optional, false, PWProcessFunction.PWClassName, true)]
    public class PWNodeXAnd : PWNodeAnd
    {
        #region c´tors
        public PWNodeXAnd(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier) 
        {
        }
        #endregion

        #region Methods


        public override void ReinterpretGate()
        {
            if (!IsEnabledReinterpretGate())
                return;
            if (PWPointIn.IsActiveExAND || PWPointIn.IsActiveAND)
            {
                PWPointIn.ResetActiveStates();
                RaiseOutEvent();
            }
        }

        #endregion

    }
}
