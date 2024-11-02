// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Default messanger'}de{'Default messanger'}", Global.ACKinds.TPABGModule, Global.ACStorableTypes.Required, false, true)]
    public class PAAlarmMessanger : PAAlarmMessengerBase
    {
        public PAAlarmMessanger(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override void DistributeAlarm(string propertyName, Msg alarm, List<ACRef<ACComponent>> targetComponents)
        {
            if (targetComponents != null && targetComponents.Any())
            {
                foreach (ACRef<ACComponent> targetComp in targetComponents)
                {
                    IACAttachedAlarmHandler alarmReceiver = targetComp.ValueT as IACAttachedAlarmHandler;
                    if (alarmReceiver != null)
                    {
                        alarmReceiver.AddAttachedAlarm(alarm);
                    }
                }
            }
        }
    }
}
