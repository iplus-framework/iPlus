using System;
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
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Scale totalizing'}de{'Waage totalisierend (SWT))'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAEScaleTotalizing : PAEScaleGravimetric
    {
        #region c'tors

        static PAEScaleTotalizing()
        {
            RegisterExecuteHandler(typeof(PAEScaleTotalizing), HandleExecuteACMethod_PAEScaleTotalizing);
        }

        public PAEScaleTotalizing(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier="")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion


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
            return HandleExecuteACMethod_PAEScaleGravimetric(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

        #region overrides
        protected override void OnResetActualWeight()
        {
            base.OnResetActualWeight();
            TotalActualWeight.ValueT = 0;
        }


        public override void SimulateWeight(double increaseValue)
        {
            base.SimulateWeight(increaseValue);
        }

        protected override void RecalcActualWeight()
        {
            base.RecalcActualWeight();
            if (ActualWeight != null)
                TotalActualWeight.ValueT = ActualWeight.ValueT;
        }
        #endregion
    }

}
