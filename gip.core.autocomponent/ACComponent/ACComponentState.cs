// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// Basisklasse für Statusbehaftete Komponenten
    /// 
    /// Jede ACComponentState befindet sich in einem bestimmten Status 'ACState'.
    /// Nach der Initialisierung standardmäßig im 'SMIdle'
    /// In jeder Ableitung können weitere Status definiert werden, in dem Methoden mit dem Attribute 'ACMethodState' deklariert werden.
    /// Mögliche ACState:
    /// SMIdle
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACComponent State}de{'ACComponent State'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, true, true)]
    public abstract class ACComponentState : ACComponent
    {
        #region c´tors
        public ACComponentState(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _ACStateMethod = acType.MethodsCached.Where(c => c.ACIdentifier == ACStateConst.SMIdle).First();
        }

        public async override Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            bool result = await base.ACDeInit(deleteACClassTask);
            if (InitState == ACInitState.Destructed)
            {
                _ACStateMethod = null;
            }

            return result;
        }
        #endregion

        #region ACState

        ACClassMethod _ACStateMethod;
        /// <summary>
        /// Modus der ACComponent
        /// </summary>
        public string ACState
        {
            get
            {
                return _ACStateMethod.ACIdentifier;
            }
            set
            {
                if (_ACStateMethod.ACIdentifier != value)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        ACClassMethod acClassMethod = ComponentClass.GetMethod(value);
                        // Muss immer ein [ACMethodState(..)] sein
                        if (acClassMethod == null)
                            return;
                        if (acClassMethod != null && acClassMethod.ACGroup != Const.ACState) 
                            return;
                        if (!IsEnabledExecuteACMethod(value, null))
                            return;
                        _LastACStateMethod = _ACStateMethod;
                        _ACStateMethod = acClassMethod;
                        OnSetACState(acClassMethod);
                        OnPropertyChanged(Const.ACState);
                    }
                }
            }
        }

        ACClassMethod _LastACStateMethod = null;
        /// <summary>
        /// Letzter Modus der ACComponent
        /// </summary>
        [ACPropertyInfo(9999)]
        public string LastACState
        {
            get
            {
                if (_LastACStateMethod == null)
                    return null;
                return _LastACStateMethod.ACIdentifier;
            }
        }

        /// <summary>
        /// Kann überschrieben werden, wenn nicht das Standardverhalten des Frameworks gewünscht ist
        /// </summary>
        /// <param name="acClassMethod"></param>
        public virtual void OnSetACState(ACClassMethod acClassMethod)
        {
            ExecuteMethod(acClassMethod.ACIdentifier, null);
        }


        /// <summary>
        /// Anfangszustand 
        /// Je nach Implementierung evtl. Reset auf Anfangszustand durchführen
        /// </summary>
        [ACMethodState("en{'Idle'}de{'Leerlauf'}", 10)]
        public virtual void SMIdle()
        {
            if (!PreExecute(ACStateConst.SMIdle)) 
                return;
            PostExecute(ACStateConst.SMIdle);
        }


        /// <summary>Called inside the GetControlModes-Method to get the Global.ControlModes from derivations.
        /// This method should be overriden in the derivations to dynmically control the presentation mode depending on the current value which is bound via VBContent</summary>
        /// <param name="vbControl">A WPF-Control that implements IVBContent</param>
        /// <returns>ControlModesInfo</returns>
        public override Global.ControlModes OnGetControlModes(IVBContent vbControl)
        {
            if (vbControl == null)
                return base.OnGetControlModes(vbControl);
            if (String.IsNullOrEmpty(vbControl.DisabledModes))
                return base.OnGetControlModes(vbControl);
            return vbControl.DisabledModes.IndexOf(ACState.ToString()) == -1 ? Global.ControlModes.Enabled : Global.ControlModes.Disabled;
        }

        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case ACStateConst.SMIdle:
                    SMIdle();
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }


        //protected override bool HandleIsEnabledExecuteACMethod(out bool result, string acMethodName, ACClassMethod acClassMethod, params Object[] acParameter)
        //{
        //    switch (acMethodName)
        //    {
        //    }
        //    return base.HandleIsEnabledExecuteACMethod(out result, acMethodName, acClassMethod, acParameter);
        //}

        #endregion
    }
}
