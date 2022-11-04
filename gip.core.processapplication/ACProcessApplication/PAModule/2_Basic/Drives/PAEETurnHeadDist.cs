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
    /// Turn-head-distributor
    /// Drehrohverteiler
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Turn-Head Distributor'}de{'Drehrohrverteiler'}", Global.ACKinds.TPAModule, Global.ACStorableTypes.Required, false, true)]
    public class PAETurnHeadDist : PAEEMotor2D
    {
        #region c'tors

        static PAETurnHeadDist()
        {
            RegisterExecuteHandler(typeof(PAETurnHeadDist), HandleExecuteACMethod_PAETurnHeadDist);
        }

        public PAETurnHeadDist(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
            _PAPointMatIn1 = new PAPoint(this, nameof(PAPointMatIn1));
            _PAPointMatOut01 = new PAPoint(this, nameof(PAPointMatOut01));
            _PAPointMatOut02 = new PAPoint(this, nameof(PAPointMatOut02));
            _PAPointMatOut03 = new PAPoint(this, nameof(PAPointMatOut03));
            _PAPointMatOut04 = new PAPoint(this, nameof(PAPointMatOut04));
            _PAPointMatOut05 = new PAPoint(this, nameof(PAPointMatOut05));
            _PAPointMatOut06 = new PAPoint(this, nameof(PAPointMatOut06));
            _PAPointMatOut07 = new PAPoint(this, nameof(PAPointMatOut07));
            _PAPointMatOut08 = new PAPoint(this, nameof(PAPointMatOut08));
            _PAPointMatOut09 = new PAPoint(this, nameof(PAPointMatOut09));
            _PAPointMatOut10 = new PAPoint(this, nameof(PAPointMatOut10));
            _PAPointMatOut11 = new PAPoint(this, nameof(PAPointMatOut11));
            _PAPointMatOut12 = new PAPoint(this, nameof(PAPointMatOut12));
            _PAPointMatOut13 = new PAPoint(this, nameof(PAPointMatOut13));
            _PAPointMatOut14 = new PAPoint(this, nameof(PAPointMatOut14));
            _PAPointMatOut15 = new PAPoint(this, nameof(PAPointMatOut15));
            _PAPointMatOut16 = new PAPoint(this, nameof(PAPointMatOut16));
            _PAPointMatOut17 = new PAPoint(this, nameof(PAPointMatOut17));
            _PAPointMatOut18 = new PAPoint(this, nameof(PAPointMatOut18));
            _PAPointMatOut19 = new PAPoint(this, nameof(PAPointMatOut19));
            _PAPointMatOut20 = new PAPoint(this, nameof(PAPointMatOut20));
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;
            return true;
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }
        #endregion

        #region Properties, Range: 900

        #region Read-Values from PLC
        [ACPropertyBindingTarget(930, "Read from PLC", "en{'current position'}de{'aktuelle Position'}", "", false, false, RemotePropID = 55)]
        public IACContainerTNet<Int16> Position { get; set; }

        [ACPropertyBindingTarget(932, "Read from PLC", "en{'desired position'}de{'Soll Position'}", "", false, false, RemotePropID = 56)]
        public IACContainerTNet<Int16> DesiredPosition { get; set; }

        #endregion

        #region Write-Values to PLC
        [ACPropertyBindingTarget(950, "Write to PLC", "en{'requested position'}de{'angeforderte Position'}", "", false, false, RemotePropID = 57)]
        public IACContainerTNet<Int16> ReqPosition { get; set; }
        #endregion

        #endregion

        #region Methods, Range: 900

        #region Execute-Helper-Handlers
        protected override bool HandleExecuteACMethod(out object result, AsyncMethodInvocationMode invocationMode, string acMethodName, ACClassMethod acClassMethod, params object[] acParameter)
        {
            result = null;
            switch (acMethodName)
            {
                case "TurnOnLeftWithPos":
                    TurnOnLeftWithPos((Int16)acParameter[0]);
                    return true;
                case "TurnOnRightWithPos":
                    TurnOnRightWithPos((Int16)acParameter[0]);
                    return true;
            }
            return base.HandleExecuteACMethod(out result, invocationMode, acMethodName, acClassMethod, acParameter);
        }
        #endregion

        [ACMethodInfo("", "en{'turn on left pos'}de{'Einschalten links mit Pos.'}", 901, false, Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOnLeftWithPos(Int16 pos)
        {
            if (!PreExecute("TurnOnLeftWithPos"))
                return;
            ReqPosition.ValueT = pos;
            TurnOnLeft();
            PostExecute("TurnOnLeftWithPos");
        }

        [ACMethodInfo("", "en{'turn on Right pos'}de{'Einschalten rechts mit Pos.'}", 902, false, Global.ACKinds.MSMethodPrePost)]
        public virtual void TurnOnRightWithPos(Int16 pos)
        {
            if (!PreExecute("TurnOnRightWithPos"))
                return;
            ReqPosition.ValueT = pos;
            TurnOnRight();
            PostExecute("TurnOnRightWithPos");
        }
        #endregion

        #region Points
        PAPoint _PAPointMatIn1;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        public PAPoint PAPointMatIn1
        {
            get
            {
                return _PAPointMatIn1;
            }
        }

        PAPoint _PAPointMatOut01;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 1, "Position", "en{'Pos1'}de{'Pos1'}", Global.Operators.and)]
        public PAPoint PAPointMatOut01
        {
            get
            {
                return _PAPointMatOut01;
            }
        }

        PAPoint _PAPointMatOut02;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 2, "Position", "en{'Pos2'}de{'Pos2'}", Global.Operators.and)]
        public PAPoint PAPointMatOut02
        {
            get
            {
                return _PAPointMatOut02;
            }
        }

        PAPoint _PAPointMatOut03;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 3, "Position", "en{'Pos3'}de{'Pos3'}", Global.Operators.and)]
        public PAPoint PAPointMatOut03
        {
            get
            {
                return _PAPointMatOut03;
            }
        }

        PAPoint _PAPointMatOut04;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 4, "Position", "en{'Pos4'}de{'Pos4'}", Global.Operators.and)]
        public PAPoint PAPointMatOut04
        {
            get
            {
                return _PAPointMatOut04;
            }
        }

        PAPoint _PAPointMatOut05;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 5, "Position", "en{'Pos5'}de{'Pos5'}", Global.Operators.and)]
        public PAPoint PAPointMatOut05
        {
            get
            {
                return _PAPointMatOut05;
            }
        }

        PAPoint _PAPointMatOut06;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 6, "Position", "en{'Pos6'}de{'Pos6'}", Global.Operators.and)]
        public PAPoint PAPointMatOut06
        {
            get
            {
                return _PAPointMatOut06;
            }
        }

        PAPoint _PAPointMatOut07;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 7, "Position", "en{'Pos7'}de{'Pos7'}", Global.Operators.and)]
        public PAPoint PAPointMatOut07
        {
            get
            {
                return _PAPointMatOut07;
            }
        }

        PAPoint _PAPointMatOut08;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 8, "Position", "en{'Pos8'}de{'Pos8'}", Global.Operators.and)]
        public PAPoint PAPointMatOut08
        {
            get
            {
                return _PAPointMatOut08;
            }
        }

        PAPoint _PAPointMatOut09;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 9, "Position", "en{'Pos9'}de{'Pos9'}", Global.Operators.and)]
        public PAPoint PAPointMatOut09
        {
            get
            {
                return _PAPointMatOut09;
            }
        }

        PAPoint _PAPointMatOut10;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 10, "Position", "en{'Pos10'}de{'Pos10'}", Global.Operators.and)]
        public PAPoint PAPointMatOut10
        {
            get
            {
                return _PAPointMatOut10;
            }
        }

        PAPoint _PAPointMatOut11;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 11, "Position", "en{'Pos11'}de{'Pos11'}", Global.Operators.and)]
        public PAPoint PAPointMatOut11
        {
            get
            {
                return _PAPointMatOut11;
            }
        }

        PAPoint _PAPointMatOut12;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 12, "Position", "en{'Pos12'}de{'Pos12'}", Global.Operators.and)]
        public PAPoint PAPointMatOut12
        {
            get
            {
                return _PAPointMatOut12;
            }
        }

        PAPoint _PAPointMatOut13;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 13, "Position", "en{'Pos13'}de{'Pos13'}", Global.Operators.and)]
        public PAPoint PAPointMatOut13
        {
            get
            {
                return _PAPointMatOut13;
            }
        }

        PAPoint _PAPointMatOut14;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 14, "Position", "en{'Pos14'}de{'Pos14'}", Global.Operators.and)]
        public PAPoint PAPointMatOut14
        {
            get
            {
                return _PAPointMatOut14;
            }
        }

        PAPoint _PAPointMatOut15;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 15, "Position", "en{'Pos15'}de{'Pos15'}", Global.Operators.and)]
        public PAPoint PAPointMatOut15
        {
            get
            {
                return _PAPointMatOut15;
            }
        }

        PAPoint _PAPointMatOut16;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 16, "Position", "en{'Pos16'}de{'Pos16'}", Global.Operators.and)]
        public PAPoint PAPointMatOut16
        {
            get
            {
                return _PAPointMatOut16;
            }
        }

        PAPoint _PAPointMatOut17;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 17, "Position", "en{'Pos17'}de{'Pos17'}", Global.Operators.and)]
        public PAPoint PAPointMatOut17
        {
            get
            {
                return _PAPointMatOut17;
            }
        }

        PAPoint _PAPointMatOut18;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 18, "Position", "en{'Pos18'}de{'Pos18'}", Global.Operators.and)]
        public PAPoint PAPointMatOut18
        {
            get
            {
                return _PAPointMatOut18;
            }
        }

        PAPoint _PAPointMatOut19;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 19, "Position", "en{'Pos19'}de{'Pos19'}", Global.Operators.and)]
        public PAPoint PAPointMatOut19
        {
            get
            {
                return _PAPointMatOut19;
            }
        }

        PAPoint _PAPointMatOut20;
        [ACPropertyConnectionPoint(9999, "PointMaterial")]
        [ACPointStateInfo("Position", 20, "Position", "en{'Pos20'}de{'Pos20'}", Global.Operators.and)]
        public PAPoint PAPointMatOut20
        {
            get
            {
                return _PAPointMatOut20;
            }
        }

        public override void ActivateRouteItemOnSimulation(RouteItem item, bool switchOff)
        {
            base.ActivateRouteItemOnSimulation(item, switchOff);
            if (PAPointMatOut01.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 1);
            else if (PAPointMatOut02.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 2);
            else if (PAPointMatOut03.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 3);
            else if (PAPointMatOut04.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 4);
            else if (PAPointMatOut05.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 5);
            else if (PAPointMatOut06.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 6);
            else if (PAPointMatOut07.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 7);
            else if (PAPointMatOut08.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 8);
            else if (PAPointMatOut09.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 9);
            else if (PAPointMatOut10.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 10);
            else if (PAPointMatOut11.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 11);
            else if (PAPointMatOut12.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 12);
            else if (PAPointMatOut13.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 13);
            else if (PAPointMatOut14.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 14);
            else if (PAPointMatOut15.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 15);
            else if (PAPointMatOut16.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 16);
            else if (PAPointMatOut17.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 17);
            else if (PAPointMatOut18.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 18);
            else if (PAPointMatOut19.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 19);
            else if (PAPointMatOut20.ConnectionList.Any(c => c.TargetParentComponent == item.TargetACComponent))
                OnSetPositionValues(switchOff ? 0 : 20);
        }

        private void OnSetPositionValues(int pos)
        {
            DesiredPosition.ValueT = (short) pos;
            Position.ValueT = (short)pos;
        }
        #endregion

        #region Handle execute helpers

        public static bool HandleExecuteACMethod_PAETurnHeadDist(out object result, IACComponent acComponent, string acMethodName, ACClassMethod acClassMethod, object[] acParameter)
        {
            return HandleExecuteACMethod_PAEEMotor2D(out result, acComponent, acMethodName, acClassMethod, acParameter);
        }

        #endregion

    }
}
