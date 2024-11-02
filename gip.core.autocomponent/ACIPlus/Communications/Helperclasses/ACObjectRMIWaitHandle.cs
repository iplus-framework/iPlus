// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Security;

namespace gip.core.autocomponent
{
    public class ACObjectRMIWaitHandle : EventWaitHandle
    {
        [SecuritySafeCritical]
        public ACObjectRMIWaitHandle(bool initialState, EventResetMode mode, int NewRequestID)
            : base(initialState, mode)
        {
            _RequestID = NewRequestID;
            TimedOut = false;
        }

        public object RemoteMethodInvocationResult
        {
            get;
            set;
        }

        public bool TimedOut
        {
            get;
            set;
        }

        private int _RequestID = 0;
        public int RequestID
        {
            get
            {
                return _RequestID;
            }
        }
    }
}
