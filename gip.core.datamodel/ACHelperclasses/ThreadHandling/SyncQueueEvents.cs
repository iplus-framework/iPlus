// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="SyncQueueEvents.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Threading;

namespace gip.core.datamodel
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
        public readonly ACMonitorObject _20010_QueueSyncLock = new ACMonitorObject(20010);
        /// <summary>
        /// The _ termination initiated
        /// </summary>
        private bool _TerminationInitiated = false;
        private DateTime? _TerminationTimeout = null;

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
            _TerminationTimeout = DateTime.Now.AddSeconds(10);
            NewItemEvent.Set();
            ExitThreadEvent.Set();
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

        public DateTime? TerminationTimeout
        {
            get
            {
                return _TerminationTimeout;
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
