// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACDelegateQueue.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACDelegateQueue
    /// </summary>
    public class ACDelegateQueue
    {
        #region c'tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACDelegateQueue"/> class.
        /// </summary>
        /// <param name="instanceName">Name of the instance.</param>
        public ACDelegateQueue(string instanceName)
        {
            _InstanceName = instanceName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACDelegateQueue"/> class.
        /// </summary>
        /// <param name="instanceName">Name of the instance.</param>
        /// <param name="workerInterval_ms">The worker interval_ms.</param>
        public ACDelegateQueue(string instanceName, int workerInterval_ms)
            : this(instanceName)
        {
            _WorkerInterval_ms = workerInterval_ms;
            if (_WorkerInterval_ms < 10)
                _WorkerInterval_ms = 10;
        }
        #endregion

        #region Sync
        /// <summary>
        /// The _ instance name
        /// </summary>
        private string _InstanceName;
        public string InstanceName
        {
            get
            {
                return _InstanceName;
            }
        }
        /// <summary>
        /// The _sync queue
        /// </summary>
        SyncQueueEvents _syncQueue = null;
        /// <summary>
        /// The _worker thread
        /// </summary>
        ACThread _workerThread;
        /// <summary>
        /// The _ worker interval_ms
        /// </summary>
        protected int _WorkerInterval_ms = 100;
        public int WorkerInterval_ms
        {
            get
            {
                return _WorkerInterval_ms;
            }
        }

        private DateTime _LastRun = DateTimeUtils.NowDST;
        public DateTime LastRun
        {
            get
            {
                return _LastRun;
            }
        }

        public TimeSpan IdleTime
        {
            get
            {
                return DateTimeUtils.NowDST - LastRun;
            }
        }

        public Thread ThreadOfQueue
        {
            get
            {
                return _workerThread?.Thread;
            }
        }

        /// <summary>
        /// Starts the worker thread.
        /// </summary>
        public void StartWorkerThread()
        {
            if (_workerThread == null)
            {
                _syncQueue = new SyncQueueEvents();
                _workerThread = new ACThread(RunWorker, this);
                _workerThread.Name = _InstanceName;
                OnThreadStarted();
                _workerThread.Start();
            }
        }

        public void StartWorkerThreadSTA()
        {
            if (_workerThread == null)
            {
                _syncQueue = new SyncQueueEvents();
                _workerThread = new ACThread(RunWorker, this);
                _workerThread.Name = _InstanceName;
                _workerThread.SetApartmentState(ApartmentState.STA);
                OnThreadStarted();
                _workerThread.Start();
            }
        }

        /// <summary>
        /// Called when [thread started].
        /// </summary>
        protected virtual void OnThreadStarted()
        {
        }

        /// <summary>
        /// Stops the worker thread.
        /// </summary>
        public void StopWorkerThread(bool forceAbort = false)
        {
            if (_workerThread != null)
            {
                _syncQueue.TerminateThread();
                if (forceAbort)
                {
                    try
                    {
                        _workerThread.Abort();
                    }
                    // If Thread hangs in deadlock, then Abort() throws an exception while releasing the lock
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (Database.Root != null && Database.Root.Messages != null)
                            Database.Root.Messages.LogException("ACDelegateQueue", "StopWorkerThread", msg);
                    }
                }
                else
                    _workerThread.Join();
                _workerThread.ParentQueue = null;
                _workerThread = null;
                _syncQueue = null;
                OnThreadStopped();
            }
        }

        /// <summary>
        /// Called when [thread stopped].
        /// </summary>
        protected virtual void OnThreadStopped()
        {
        }

        public void RestartQueue(bool forceAbort)
        {
            StopWorkerThread(forceAbort);
            _DelegateQueue = new Queue<Action>();
            StartWorkerThread();
        }

        /// <summary>
        /// Runs the worker.
        /// </summary>
        private void RunWorker()
        {
            while (!_syncQueue.ExitThreadEvent.WaitOne(_WorkerInterval_ms, false))
            {
                if (CanStartWork())
                {
                    PerformanceEvent perfEvent = _workerThread.StartReportingExeTime();
                    if (perfEvent != null)
                        _LastRun = perfEvent.StartTime;

                    Work();
                    _workerThread.StopReportingExeTime();

                    // Warte darauf, dass neue Event ansteht               
                    _syncQueue.NewItemEvent.WaitOne();
                }
                // Sammle zuerst ein paar Events
                //Thread.Sleep(_WorkerInterval_ms);
            }
            Work();
            _syncQueue.ThreadTerminated();
        }

        #endregion

        #region Producer-Consumer for Sending

        protected readonly ACMonitorObject _0_LockDelegateQueue = new ACMonitorObject(0);
        /// <summary>
        /// The _ delegate queue
        /// </summary>
        private Queue<Action> _DelegateQueue = new Queue<Action>();
        /// <summary>
        /// Gets or sets the delegate queue.
        /// </summary>
        /// <value>The delegate queue.</value>
        internal Queue<Action> DelegateQueue
        {
            get { return _DelegateQueue; }
            set { _DelegateQueue = value; }
        }

        public int DelegateQueueCount
        {
            get
            {

                using (ACMonitor.Lock(_0_LockDelegateQueue))
                {
                    return _DelegateQueue.Count;
                }
            }
        }


        public int UnsafeDelegateQueueCount
        {
            get
            {
                return _DelegateQueue.Count;
            }
        }

        /// <summary>
        /// The _ count retry delegate
        /// </summary>
        private int _CountRetryDelegate = 0;
        public int CountRetryDelegate
        {
            get
            {
                return _CountRetryDelegate;
            }
        }
        public const int MaxRetries = 10;

        /// <summary>
        /// The _ retry delegate queue
        /// </summary>
        private Queue<Action> _RetryDelegateQueue = new Queue<Action>();
        /// <summary>
        /// Gets or sets the retry delegate queue.
        /// </summary>
        /// <value>The retry delegate queue.</value>
        internal Queue<Action> RetryDelegateQueue
        {
            get { return _RetryDelegateQueue; }
            set { _RetryDelegateQueue = value; }
        }

        public int RetryDelegateQueueCount
        {
            get
            {

                using (ACMonitor.Lock(_0_LockDelegateQueue))
                {
                    return _RetryDelegateQueue.Count;
                }
            }
        }

        private bool _IsBusy = false;
        public bool IsBusy
        {
            get
            {
                return _IsBusy || RetryDelegateQueueCount > 0 || DelegateQueueCount > 0;
            }
        }


        /// <summary>
        /// Producer-Method
        /// </summary>
        /// <param name="action">The action.</param>
        public void Add(Action action)
        {
            using (ACMonitor.Lock(_0_LockDelegateQueue))
            {
                DelegateQueue.Enqueue(action);
                if (_syncQueue != null && _syncQueue.NewItemsEnqueueable)
                    _syncQueue.NewItemEvent.Set();
            }
        }

        /// <summary>
        /// Consumer-Method
        /// </summary>
        public void Work()
        {
            List<Action> retryDelegateList = null;
            int actionCounter = RetryDelegateQueueCount;

            if (actionCounter > 0)
            {
                bool processable = OnStartQueueProcessing(actionCounter);
                try
                {
                    _IsBusy = true;
                    actionCounter = 0;
                    while (RetryDelegateQueueCount > 0)
                    {
                        Action action = null;

                        using (ACMonitor.Lock(_0_LockDelegateQueue))
                        {
                            if (RetryDelegateQueue.Count <= 0)
                                break;
                            action = RetryDelegateQueue.Dequeue();
                        }
                        if (processable && action != null)
                        {
                            try
                            {
                                ProcessAction(action);
                            }
                            catch (SynchronizationLockException slEx)
                            {
                                Database.Root.Messages.LogException(_InstanceName, "ACDelegateQueue.Work(0)", slEx.Message);
                                if (retryDelegateList == null)
                                    retryDelegateList = new List<Action>();
                                retryDelegateList.Add(action);
                            }
                            catch (Exception ex)
                            {
                                Database.Root.Messages.LogException(_InstanceName, "ACDelegateQueue.Work(1)", ex.Message);
                                if (ex.InnerException != null)
                                    Database.Root.Messages.LogException(_InstanceName, "ACDelegateQueue.Work(2)", ex.InnerException.Message);
                                Database.Root.Messages.LogException(_InstanceName, "ACDelegateQueue.Work(3)", ex.StackTrace);
                            }
                        }
                        actionCounter++;

                        if (_syncQueue != null
                            && !_syncQueue.NewItemsEnqueueable
                            && _syncQueue.TerminationTimeout.HasValue
                            && DateTime.Now > _syncQueue.TerminationTimeout.Value)
                            break;
                    }
                }
                finally
                {
                    OnQueueProcessed(actionCounter);
                    _IsBusy = false;
                }

                if (retryDelegateList != null)
                {
                    _CountRetryDelegate++;
                    if (_CountRetryDelegate > MaxRetries)
                    {
                        using (ACMonitor.Lock(_0_LockDelegateQueue))
                        {
                            RetryDelegateQueue.Clear();
                        }
                        Database.Root.Messages.LogError(_InstanceName, "ACDelegateQueue.Work(3)", "RetryDelegateQueue cleared. Actions could not be perforemd because of not solveable Deadlocks.");
                        _CountRetryDelegate = 0;
                    }
                    else
                    {
                        using (ACMonitor.Lock(_0_LockDelegateQueue))
                        {
                            retryDelegateList.ForEach(c => RetryDelegateQueue.Enqueue(c));
                        }
                        return;
                    }
                }
            }

            actionCounter = DelegateQueueCount;
            if (actionCounter > 0)
            {
                bool processable = OnStartQueueProcessing(actionCounter);
                try
                {
                    _IsBusy = true;
                    actionCounter = 0;
                    while (DelegateQueueCount > 0)
                    {
                        Action action = null;

                        using (ACMonitor.Lock(_0_LockDelegateQueue))
                        {
                            if (DelegateQueue.Count <= 0)
                                break;
                            action = DelegateQueue.Dequeue();
                        }
                        if (processable && action != null)
                        {
                            try
                            {
                                ProcessAction(action);
                            }
                            catch (SynchronizationLockException slEx)
                            {
                                Database.Root.Messages.LogException(_InstanceName, "ACDelegateQueue.Work(4)", slEx.Message);
                                RetryDelegateQueue.Enqueue(action);
                            }
                            catch (Exception ex)
                            {
                                Database.Root.Messages.LogException(_InstanceName, "ACDelegateQueue.Work(5)", ex.Message);
                                if (ex.InnerException != null)
                                    Database.Root.Messages.LogException(_InstanceName, "ACDelegateQueue.Work(6)", ex.InnerException.Message);
                                Database.Root.Messages.LogException(_InstanceName, "ACDelegateQueue.Work(7)", ex.StackTrace);
                            }
                        }
                        actionCounter++;
                        if (_syncQueue != null
                            && !_syncQueue.NewItemsEnqueueable
                            && _syncQueue.TerminationTimeout.HasValue
                            && DateTime.Now > _syncQueue.TerminationTimeout.Value)
                            break;
                    }
                    //if (actionCounter > 0)
                }
                finally
                {
                    OnQueueProcessed(actionCounter);
                    _IsBusy = false;
                }
            }
        }

        /// <summary>
        /// Processes the action.
        /// </summary>
        /// <param name="action">The action.</param>
        public virtual void ProcessAction(Action action)
        {
            action();
        }

        /// <summary>
        /// Called when [start queue processing].
        /// </summary>
        /// <param name="countActions">The count actions.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        protected virtual bool OnStartQueueProcessing(int countActions)
        {
            return true;
        }

        /// <summary>
        /// Called when [queue processed].
        /// </summary>
        /// <param name="countActions">The count actions.</param>
        protected virtual void OnQueueProcessed(int countActions)
        {
        }

        protected virtual bool CanStartWork()
        {
            return true;
        }
        #endregion
    }

    public class ACDispatchedDelegateQueue : ACDelegateQueue
    {
        public ACDispatchedDelegateQueue(string instanceName) : base(instanceName)
        {
        }

        public ACDispatchedDelegateQueue(string instanceName, int workerInterval_ms) : base(instanceName, workerInterval_ms)
        {
        }

        public override void ProcessAction(Action action)
        {
            // TODO: Printing of Flow-documents needs Dispatcher. Temporarily deisabled to enable Linux
#if !EFCR
            var currentDispatcher = Dispatcher.CurrentDispatcher;
            if (currentDispatcher != null)
                currentDispatcher.Invoke(DispatcherPriority.Normal, action);
#else
            base.ProcessAction(action);
#endif
        }

        protected override void OnQueueProcessed(int countActions)
        {
#if !EFCR
            //Dispatcher.ExitAllFrames();
            try
            {
                var currentDispatcher = Dispatcher.CurrentDispatcher;
                if (currentDispatcher != null)
                    currentDispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate () { Dispatcher.ExitAllFrames(); });
            }
            catch
            {
            }
            GC.Collect();
#endif
            base.OnQueueProcessed(countActions);
        }

#region Properties
        private static ACDispatchedDelegateQueue _PrintQueue = null;
        public static ACDispatchedDelegateQueue PrintQueue
        {
            get
            {
                if (_PrintQueue == null)
                    _PrintQueue = new ACDispatchedDelegateQueue(RootDbOpQueue.ClassName + "." + nameof(PrintQueue));
                return _PrintQueue;
            }
        }
#endregion
    }
}
