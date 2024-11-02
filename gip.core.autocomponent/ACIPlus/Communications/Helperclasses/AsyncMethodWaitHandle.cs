// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Security;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public class AsyncMethodWaitHandle
    {
        public enum InvocationState
        {
            Invoked = 0,
            Waiting = 1,
            Done = 2
        }

        public AsyncMethodWaitHandle(ACMethod invokedACMethod)
        {
            _InvokedACMethod = invokedACMethod;
        }

        public void WaitOnCallback()
        {
            if (_InvocationStateOfAsycMethod != InvocationState.Done)
            {
                _InvocationStateOfAsycMethod = InvocationState.Waiting;
                _AsyncInvocationWaitHandle.WaitOne();
            }
        }

        public bool Callback(ACMethod acMethod, ACMethodEventArgs result)
        {
            if (acMethod != _InvokedACMethod)
                return false;

            if (_InvocationStateOfAsycMethod == InvocationState.Waiting)
            {
                _InvocationStateOfAsycMethod = InvocationState.Done;
                _AsyncInvocationWaitHandle.Set();
            }
            else
            {
                _InvocationStateOfAsycMethod = InvocationState.Done;
            }
            return true;
        }

        private InvocationState _InvocationStateOfAsycMethod = InvocationState.Invoked;

        private AutoResetEvent _AsyncInvocationWaitHandle = new AutoResetEvent(false);
        public AutoResetEvent AsyncInvocationWaitHandle
        {
            get
            {
                return _AsyncInvocationWaitHandle;
            }
        }

        private ACMethod _InvokedACMethod;
        public ACMethod InvokedACMethod
        {
            get
            {
                return _InvokedACMethod;
            }
        }

    }
}
