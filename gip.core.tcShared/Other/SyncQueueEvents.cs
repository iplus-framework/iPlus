﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gip.core.tcShared
{
    /// <summary>
    /// Class SyncQueueEvents
    /// </summary>
    public class SyncQueueEvents
    {
        /// <summary>
        /// The _new item event
        /// </summary>
        private EventWaitHandle _newItemEvent;
        /// <summary>
        /// The _exit thread event
        /// </summary>
        private EventWaitHandle _exitThreadEvent;
        /// <summary>
        /// The queue sync
        /// </summary>
        public readonly MonitorObject _20009_MetaSyncLock = new MonitorObject(20009);

        public readonly MonitorObject _20010_QueueSyncLock = new MonitorObject(20010);

        public readonly MonitorObject _20011_QueueSyncLock = new MonitorObject(20011);

        public readonly MonitorObject _20012_QueueSyncLock = new MonitorObject(20012);
        /// <summary>
        /// The _ termination initiated
        /// </summary>
        private bool _TerminationInitiated = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="SyncQueueEvents"/> class.
        /// </summary>
        public SyncQueueEvents()
        {
            _newItemEvent = new AutoResetEvent(false);
            _exitThreadEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// Terminates the thread.
        /// </summary>
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

        /// <summary>
        /// Threads the terminated.
        /// </summary>
        public void ThreadTerminated()
        {
            if (!NewItemsEnqueueable)
                return;
            NewItemEvent.Close();
        }

        /// <summary>
        /// Gets a value indicating whether [new items enqueueable].
        /// </summary>
        /// <value><c>true</c> if [new items enqueueable]; otherwise, <c>false</c>.</value>
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

        /// <summary>
        /// Gets the exit thread event.
        /// </summary>
        /// <value>The exit thread event.</value>
        public EventWaitHandle ExitThreadEvent
        {
            get { return _exitThreadEvent; }
        }

        /// <summary>
        /// Gets the new item event.
        /// </summary>
        /// <value>The new item event.</value>
        public EventWaitHandle NewItemEvent
        {
            get { return _newItemEvent; }
        }
    }
}