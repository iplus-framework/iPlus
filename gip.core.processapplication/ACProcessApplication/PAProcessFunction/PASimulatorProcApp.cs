// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.processapplication
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Simulator processapplication'}de{'Simulator Prozessanwendung'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, true)]
    public class PASimulatorProcApp : PAClassSimulator
    {
        #region Constructors

        public PASimulatorProcApp(core.datamodel.ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        #endregion 

        #region override

        DateTime _NextAcknowledgeSim = DateTime.Now.AddMinutes(2);

        protected override void OnSimulateItem(SimulationItem simItem)
        {
            if (simItem.ReqProperty.ACIdentifier.StartsWith("Req") 
                && (simItem.ReqProperty.ACIdentifier.Contains("Pos") || simItem.ReqProperty.ACIdentifier.Contains("Run"))
                && !ManualSimulationMode.ValueT)
            {
                PAEControlModuleBase controlModule = simItem.Component as PAEControlModuleBase;
                if (controlModule != null)
                {
                    if (simItem.ReqProperty.ACType.ObjectType == typeof(bool))
                    {
                        bool bOn = (bool)simItem.ReqProperty.Value;
                        IACCommSession session = controlModule.Session as IACCommSession;
                        if (session == null || !session.IsConnected.ValueT)
                        {
                            if (bOn)
                            {
                                controlModule.SwitchingFrequency.ValueT++;
                                int randomAlarm = new Random().Next(0, 100);
                                if (randomAlarm >= 95)
                                {
                                    if (controlModule.FaultState.ValueT == PANotifyState.Off)
                                    {
                                        controlModule.FaultState.ValueT = PANotifyState.AlarmOrFault;
                                        controlModule.TotalAlarms.ValueT++;
                                    }
                                }
                            }
                            else
                                controlModule.OperatingTime.ValueT += new TimeSpan(0, new Random().Next(0, 30), new Random().Next(0, 60));
                        }
                    }
                }
            }
        }

        protected override void objectManager_ProjectWorkCycle(object sender, EventArgs e)
        {
            base.objectManager_ProjectWorkCycle(sender, e);
            if (RunSimulation.ValueT && !ManualSimulationMode.ValueT && AppManager != null)
            {
                if (AppManager.HasAlarms.ValueT)
                {
                    if (DateTime.Now > _NextAcknowledgeSim)
                    {
                        AppManager.AcknowledgeAllAlarms();
                        _NextAcknowledgeSim = DateTime.Now.AddMinutes(1);
                    }
                }
            }
        }
        #endregion
    }
}
