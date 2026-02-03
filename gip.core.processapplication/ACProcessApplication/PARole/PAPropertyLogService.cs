// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.autocomponent;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        public override async Task<bool> ACDeInit(bool deleteACClassTask = false)
        {
            return await base.ACDeInit(deleteACClassTask);
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
                    if (oeeProvider.AvailabilityState.ValueT == AvailabilityState.Standby && newAvailabilityState == AvailabilityState.InOperation)
                    {
                        //The property logs of state Standby and InOperation sometime has same Event-time. This causes a wrong display of the AvailabilityState in the PropertyPresenterLog.
                        Thread.Sleep(10);
                    }

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

        protected override Guid? OnGetPropertyLogMessageID(ACPropertyNetSendEventArgs args)
        {
            IPAOEEProvider oeeProvider = (args.ForACComponent is PAProcessFunction) ? args.ForACComponent.ParentACComponent as IPAOEEProvider : args.ForACComponent as IPAOEEProvider;
            if (oeeProvider != null)
            {
                return oeeProvider.OEEReason;
            }

            return base.OnGetPropertyLogMessageID(args);
        }

        protected override DateTime OnEditEventTime(DateTime eventTime, ACClass acClass, ACClassProperty acClassProperty, object value, Database db)
        {
            if (acClassProperty.ACIdentifier == nameof(IPAOEEProvider.AvailabilityState))
            {
                AvailabilityState? state = value as AvailabilityState?;
                if (state.HasValue && state.Value == AvailabilityState.Idle)
                {
                    var propertyLog = db.ACProgramLogPropertyLog.Include(c => c.ACPropertyLog.ACClassProperty)
                                                                .Include(c => c.ACPropertyLog.ACClass)
                                                                .GroupJoin(db.ACProgramLog,
                                                                           propLog => propLog.ACProgramLogID,
                                                                           programLog => programLog.ACProgramLogID,
                                                                           (propLog, programLog) => new { propLog, programLog })
                                                                .Where(c => c.propLog.ACPropertyLog.ACClassID == acClass.ACClassID
                                                                         && c.propLog.ACPropertyLog.ACClassPropertyID == acClassProperty.ACClassPropertyID)
                                                                .OrderByDescending(c => c.propLog.ACPropertyLog.EventTime)
                                                                .FirstOrDefault();

                    if (propertyLog != null)
                    {
                        DateTime? programLogEnd = propertyLog.programLog.Where(c => c.EndDate.HasValue).OrderByDescending(c => c.EndDate).FirstOrDefault()?.EndDate;
                        if (programLogEnd.HasValue)
                        {
                            TimeSpan diff = eventTime - programLogEnd.Value;
                            double totalSec = Math.Abs(diff.TotalSeconds);

                            if (totalSec > 300)
                                return programLogEnd.Value;
                        }
                    }
                }
            }

            return base.OnEditEventTime(eventTime, acClass, acClassProperty, value, db);
        }

        #endregion
    }
}
