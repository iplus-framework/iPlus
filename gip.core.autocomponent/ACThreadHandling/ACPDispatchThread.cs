// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    class ACPDispatchThread
    {
        #region Events and Delegates
        public delegate void InvokeDispatch();
        #endregion

        public void MainACPDispatchThread(InvokeDispatch DispatchMethod)
        {
            int threadCounter = 0;
            while (true)
            {
                threadCounter++;
                Thread.Sleep(100);
                if (DispatchMethod != null)
                {
                    DispatchMethod();
                }
            }
        }
    }
}
