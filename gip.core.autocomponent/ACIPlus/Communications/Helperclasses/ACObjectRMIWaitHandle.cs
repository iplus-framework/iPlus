using System;
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
