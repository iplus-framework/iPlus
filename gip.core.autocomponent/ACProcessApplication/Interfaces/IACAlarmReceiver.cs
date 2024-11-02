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
    //AttachedAlarmHandler
    public interface IACAttachedAlarmHandler
    {
        IACContainerTNet<bool> HasAttachedAlarm
        {
            get;
            set;
        }

        SafeList<Msg> AttachedAlarms
        {
            get;
            set;
        }

        void AddAttachedAlarm(Msg msg);

        void AckAttachedAlarm(Guid msgID);

        void AckAllAttachedAlarms();

        MsgList GetAttachedAlarms();
    }
}
