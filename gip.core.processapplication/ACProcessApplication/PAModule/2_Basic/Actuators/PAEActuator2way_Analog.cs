// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.Threading.Tasks;

namespace gip.core.processapplication
{
    /// <summary>
    /// Two-Way-Actuator analog (mixing valve)
    /// Zwei-Wege-Stellglied analog (Mischventil)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'2-Way Actuator analog'}de{'2-Wege Stellglied Analog'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEActuator2way_Analog : PAEActuator1way_Analog
    {
        #region c'tors

        static PAEActuator2way_Analog()
        {
            RegisterExecuteHandler(typeof(PAEActuator2way_Analog), HandleExecuteACMethod_PAEActuator2way_Analog);
        }

        public PAEActuator2way_Analog(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PAPointMatIn2 = new PAPoint(this, nameof(PAPointMatIn2));
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            return await base.ACDeInit(deleteACClassTask);
        }
        #endregion      

        #region Points
        PAPoint _PAPointMatIn2;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        public PAPoint PAPointMatIn2
        {
            get
            {
                return _PAPointMatIn2;
            }
        }
        #endregion

        #region Read-Values from PLC
        [ACPropertyBindingTarget(630, "Read from PLC", "en{'Open direction 2'}de{'Offen Richtung 2'}", "", false, false, RemotePropID = 36)]
        public IACContainerTNet<Boolean> Pos2Open { get; set; }
        public void OnSetPos2Open(IACPropertyNetValueEvent valueEvent)
        {
            bool newValue = (valueEvent as ACPropertyValueEvent<bool>).Value;
            if (newValue != Pos2Open.ValueT && this.Root.Initialized)
            {
                if (newValue)
                {
                    TurnOnInstant.ValueT = DateTime.Now;
                    SwitchingFrequency.ValueT++;
                }
                else
                {
                    TurnOffInstant.ValueT = DateTime.Now;
                    if (TurnOnInstant.ValueT > DateTime.MinValue && TurnOnInstant.ValueT < TurnOffInstant.ValueT)
                        OperatingTime.ValueT += TurnOffInstant.ValueT - TurnOnInstant.ValueT;
                }
            }
        }

        #endregion

        #region Write-Values to PLC
        [ACPropertyBindingTarget(650, "Write to PLC", "en{'open request direction 2'}de{'Öffnen Anforderung Richtung 2'}", "", false, false, RemotePropID = 37)]
        public IACContainerTNet<Boolean> ReqPos2Open { get; set; }
        #endregion


        // Properties, Range:700
        // Methods, Range: 701

        #region Handle execute helpers

        public override bool IsEnabledOpen()
        {
            if (ReqOpeningWidth.ValueT <= 0.0001)
                return false;
            return true;
        }

        public static bool HandleExecuteACMethod_PAEActuator2way_Analog(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEActuator1way_Analog(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion
    }
}
