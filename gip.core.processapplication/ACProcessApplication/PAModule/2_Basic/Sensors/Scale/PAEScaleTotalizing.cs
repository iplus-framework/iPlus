// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;
using gip.core.autocomponent;
using System.ComponentModel;

namespace gip.core.processapplication
{
    /// <summary>
    /// Totalizing gravimaterically Scale
    /// Totalisierende Waage
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Scale totalizing'}de{'Waage totalisierend (SWT)'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEScaleTotalizing : PAEScaleCalibratable
    {
        #region c'tors

        static PAEScaleTotalizing()
        {
            RegisterExecuteHandler(typeof(PAEScaleTotalizing), HandleExecuteACMethod_PAEScaleTotalizing);
        }

        public PAEScaleTotalizing(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _SWTTipWeight = new ACPropertyConfigValue<double>(this, "SWTTipWeight", 0.0);
        }

        #endregion

        private ACPropertyConfigValue<double> _SWTTipWeight;
        [ACPropertyConfig("en{'SWT tip weight'}de{'SWT Kippgewicht'}")]
        public double SWTTipWeight
        {
            get
            {
                return _SWTTipWeight.ValueT;
            }
            set
            {
                _SWTTipWeight.ValueT = value;
            }
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            _ = SWTTipWeight;
            return true;
        }

        #region Properties Range 800

        #region Read-Values from PLC
        [ACPropertyBindingTarget(830, "Read from PLC", "en{'Total desired weight'}de{'Gesamtsollgewicht'}", "", false, false, RemotePropID = 86)]
        public IACContainerTNet<Double> TotalDesiredWeight { get; set; }

        [ACPropertyBindingTarget(831, "Read from PLC", "en{'Total actual weight'}de{'Gesamtistgewichtt'}", "", false, false, RemotePropID = 87)]
        public IACContainerTNet<Double> TotalActualWeight { get; set; }

        [ACPropertyBindingTarget(832, "Read from PLC", "en{'presignal'}de{'Vorsignal'}", "", false, false, RemotePropID = 88)]
        public IACContainerTNet<Boolean> IsPresignal { get; set; }

        [ACPropertyBindingTarget(833, "Read from PLC", "en{'in flow mode'}de{'In Durchflussmodus'}", "", false, false,RemotePropID = 89)]
        public IACContainerTNet<Boolean> IsFlow { get; set; }
        #endregion

        #endregion

        // Methods, Range: 800

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAEScaleTotalizing(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEScaleCalibratable(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #region overrides
        protected override void OnResetActualWeight()
        {
            base.OnResetActualWeight();
            if (   IsInternalActualWeightCalculation 
                && IsVisibleExtActualWeight != null 
                && !IsVisibleExtActualWeight.ValueT)
                TotalActualWeight.ValueT = 0;
        }


        public override void SimulateWeight(double increaseValue)
        {
            base.SimulateWeight(increaseValue);
        }

        protected override void RecalcActualWeight()
        {
            base.RecalcActualWeight();
            if (   ActualWeight != null 
                && IsInternalActualWeightCalculation 
                && IsVisibleExtActualWeight != null 
                && !IsVisibleExtActualWeight.ValueT)
                TotalActualWeight.ValueT = ActualWeight.ValueT;
        }
        #endregion
    }

}
