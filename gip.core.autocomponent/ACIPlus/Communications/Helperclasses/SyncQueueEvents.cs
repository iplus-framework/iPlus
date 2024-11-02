// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;
using gip.core.datamodel;


namespace gip.core.autocomponent
{
    public class SyncQueueEvents
    {
        private EventWaitHandle _newItemEvent;
        private EventWaitHandle _exitThreadEvent;
        internal object QueueSync = new object();
        private bool _TerminationInitiated = false;

        public SyncQueueEvents()
        {
            _newItemEvent = new AutoResetEvent(false);
            _exitThreadEvent = new ManualResetEvent(false);
        }

        public void TerminateThread()
        {
            if ((ExitThreadEvent.SafeWaitHandle == null) || (ExitThreadEvent.SafeWaitHandle.IsClosed))
                return;
            ExitThreadEvent.Set();

            if (!NewItemsEnqueueable)
                return;
            _TerminationInitiated = true;
            NewItemEvent.Set();
        }

        public void ThreadTerminated()
        {
            if (!NewItemsEnqueueable)
                return;
            NewItemEvent.Close();
        }

        public bool NewItemsEnqueueable
        {
            get
            {
                if (_TerminationInitiated)
                    return false;
                if ((NewItemEvent.SafeWaitHandle == null) || (NewItemEvent.SafeWaitHandle.IsClosed))
                    return false;
                return true;
            }
        }

        public EventWaitHandle ExitThreadEvent
        {
            get { return _exitThreadEvent; }
        }

        public EventWaitHandle NewItemEvent
        {
            get { return _newItemEvent; }
        }
    }
}
