using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Property logging with OEE'}de{'Eigenschaftsprotokollierung mit OEE'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, true)]
    public class PAPropertyLogService : ACPropertyLogService
    {
        new public const string ClassName = nameof(PAPropertyLogService);

        #region c'tors

        public PAPropertyLogService(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "") :
            base(acType, content, parentACObject, parameter, acIdentifier)
        {

        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            return base.ACInit(startChildMode);
        }

        public override bool ACPostInit()
        {
            return base.ACPostInit();
        }

        public override bool ACDeInit(bool deleteACClassTask = false)
        {
            return base.ACDeInit(deleteACClassTask);
        }

        #endregion

        #region Methods


        protected override void OnPropertyValueChanged(object sender, ACPropertyNetSendEventArgs e)
        {
            if (IsOEERelevantProperty(sender, e))
            {
                // Determines the AvailabilityState
                IPAOEEProvider oeeProvider = (e.ForACComponent is PAProcessFunction) ? e.ForACComponent.ParentACComponent as IPAOEEProvider : e.ForACComponent as IPAOEEProvider;
                if (oeeProvider != null)
                {
                    AvailabilityState newAvailabilityState = AvailabilityState.Idle;
                    if (oeeProvider.OperatingMode.ValueT == Global.OperatingMode.Maintenance)
                        newAvailabilityState = AvailabilityState.Maintenance;
                    else if (oeeProvider.Allocated.ValueT)
                    {
                        // TODO: Program Retooling-State
                        newAvailabilityState = AvailabilityState.Standby;
                        List<PAProcessFunction> functions = oeeProvider.FindChildComponents<PAProcessFunction>(c => c is PAProcessFunction, null, 1);
                        if (functions.Any())
                        {
                            var queryActiveFunctions = functions.Where(c => c.CurrentACState != ACStateEnum.SMIdle);
                            if (queryActiveFunctions.Any())
                            {
                                newAvailabilityState = AvailabilityState.InOperation;
                                if (queryActiveFunctions.Where(c => c.Malfunction.ValueT >= PANotifyState.InfoOrActive).Any())
                                {
                                    newAvailabilityState = AvailabilityState.UnscheduledBreak;
                                }
                                else
                                {
                                    var queryPaused = queryActiveFunctions.Where(c => c.CurrentACState == ACStateEnum.SMPaused
                                                                        || c.CurrentACState == ACStateEnum.SMPausing
                                                                        || c.CurrentACState == ACStateEnum.SMHolding
                                                                        || c.CurrentACState == ACStateEnum.SMHeld);
                                    if (queryPaused.Any())
                                    {
                                        newAvailabilityState = AvailabilityState.ScheduledBreak;
                                        if (queryPaused.Where(c => c.HasAlarms.ValueT).Any())
                                            newAvailabilityState = AvailabilityState.UnscheduledBreak;
                                    }
                                }
                            }
                        }
                    }
                    newAvailabilityState = OnChangingAvailabilityState(newAvailabilityState, oeeProvider, sender, e);
                    oeeProvider.AvailabilityState.ValueT = newAvailabilityState;
                }
            }

            base.OnPropertyValueChanged(sender, e);
        }

        protected virtual bool IsOEERelevantProperty(object sender, ACPropertyNetSendEventArgs e)
        {
            return e.ForACComponent != null
                && e.NetValueEventArgs != null
                && e.NetValueEventArgs.EventType == EventTypes.ValueChangedInSource
                && (   (e.ForACComponent is PAProcessFunction && e.NetValueEventArgs.ACIdentifier == Const.ACState)
                    || (e.ForACComponent is IPAOEEProvider
                        && (   e.NetValueEventArgs.ACIdentifier == nameof(IPAOEEProvider.OperatingMode)
                            || e.NetValueEventArgs.ACIdentifier == nameof(IPAOEEProvider.Allocated)))
                    );
        }

        protected virtual AvailabilityState OnChangingAvailabilityState(AvailabilityState newAvailabilityState, IPAOEEProvider oeeProvider, object sender, ACPropertyNetSendEventArgs e)
        {
            return newAvailabilityState;
        }
        #endregion
    }
}
