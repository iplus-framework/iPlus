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
// <copyright file="ACMonitor.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace gip.core.datamodel
{
    public class ACMonitorObject
    {
        readonly public int LockLevel;
        public ACMonitorObject(int lockLevel)
        {
            LockLevel = lockLevel;
        }

        public override string ToString()
        {
            string text = base.ToString();
            return String.Format("{0}, Locklevel: {1}", text, LockLevel);
        }
    }

    /// <summary>
    /// The ACMonitor is an Extension of the .NET's System.Threading.Monitor-Class
    /// It Provides a mechanism that synchronizes access to objects.
    /// ACMonitor should be used for implementing ACComponents instead of the classic System.Threading.Monitor respectively the lock-Keyword.
    /// The ACMonitor prevents that concurrent Threads falls into a deadlock. This is very critical for Automation-Components.
    /// Before a deadlock would occur, the ACMonitor throws a SynchronizationLockException instead.
    /// This Exception must be handled by the Automation-Component.
    /// Further information under: http://msdn.microsoft.com/en-us/magazine/cc163352.aspx
    /// </summary>
    public class ACMonitor
    {
        /// <summary>
        /// Lock used to protected private static state in ACMonitor.
        /// </summary>
        private static object _globalLock = new object();
        /// <summary>
        /// Maps actual monitor being waited on to internal data about that monitor.
        /// </summary>
        private static Dictionary<ACMonitorObject, MonitorState> _monitorStates = new Dictionary<ACMonitorObject, MonitorState>();

        private static bool? _UseSimpleMonitor;
        public static bool UseSimpleMonitor
        {
            get
            {
                if (!_UseSimpleMonitor.HasValue)
                    return true;
                return _UseSimpleMonitor.Value;
            }
            set
            {
                if (_UseSimpleMonitor.HasValue)
                    return;
                _UseSimpleMonitor = value;
            }
        }

        private static bool? _ValidateLockHierarchy;
        public static bool ValidateLockHierarchy
        {
            get
            {
                if (!_ValidateLockHierarchy.HasValue)
                {
                    #if DEBUG
                        _ValidateLockHierarchy = true;
                    #else
                        _ValidateLockHierarchy = false;
                    #endif
                }
                return _ValidateLockHierarchy.Value;
            }
            set
            {
                if (_ValidateLockHierarchy.HasValue)
                    return;
                _ValidateLockHierarchy = value;
            }
        }

        /// <summary>
        /// Locks the specified object and returns an IDisposable that can be used to release the lock.
        /// </summary>
        /// <param name="monitor">The object on which to acquire the monitor lock.</param>
        /// <returns>An IDisposable that can be used to release the lock.</returns>
        /// <exception cref="System.ArgumentNullException">monitor</exception>
        public static IDisposable Lock(ACMonitorObject monitor)
        {
            if (monitor == null) 
                throw new ArgumentNullException("monitor");
            IDisposable cookie = new ACMonitorCookie(monitor);
            Enter(monitor);
            return cookie;
        }

        /// <summary>
        /// Enables syntax close to that of lock(obj) {...}.  Rather,
        /// the Lock method will return an instance of this IDisposable
        /// and in its Dispose method it will release the lock, thus enabling
        /// the syntax: using(ACMonitorFirstAttempt.Lock(obj)) {...}
        /// </summary>
        private class ACMonitorCookie : IDisposable
        {
            /// <summary>
            /// The lock to be released.
            /// </summary>
            private ACMonitorObject _monitor;
            //private PerformanceEvent _perfEvent;

            /// <summary>
            /// Initializes the ACMonitorCookie.
            /// </summary>
            /// <param name="obj">The object to be released.</param>
            public ACMonitorCookie(ACMonitorObject obj) 
            { 
                _monitor = obj;
                //_perfEvent = new PerformanceEvent(false);
                //_perfEvent.Start();
            }

            /// <summary>
            /// Exit the lock.
            /// </summary>
            public void Dispose()
            {
                if (_monitor != null)
                {
                    //if (_perfEvent != null)
                    //{
                    //    _perfEvent.Stop();
                    //    if (_perfEvent.CalculateTimeout(2000))
                    //    {
//#if DEBUG
//                            if (System.Diagnostics.Debugger.IsAttached)
//                            {
//                                System.Diagnostics.Debugger.Break();
//                            }
//                            gip.core.datamodel.Database.Root.Messages.LogWarning("ACMonitorCookie", "Dispose()", String.Format("{0}: Duration {1} ms", _monitor.LockLevel, _perfEvent.ElapsedMilliseconds));
//                            string stackTrace = System.Environment.StackTrace;
//                            gip.core.datamodel.Database.Root.Messages.LogWarning("ACMonitorCookie", "Dispose()", stackTrace);
//#endif
                    //    }
                    //    _perfEvent = null;
                    //}
                    ACMonitor.Exit(_monitor);
                    _monitor = null;
                }
            }
        }

        /// <summary>
        /// Acquires an exclusive lock on the specified object.
        /// </summary>
        /// <param name="monitor">The object on which to acquire the monitor lock.</param>
        public static void Enter(ACMonitorObject monitor)
        {
            TryEnter(monitor, Timeout.Infinite);
            //TryEnter(monitor, 5000);
        }

        /// <summary>
        /// Acquires an exclusive lock on the specified object.
        /// </summary>
        /// <param name="monitor">The object on which to acquire the monitor lock.</param>
        public static void EnterExclusive(ACMonitorObject monitor)
        {
            TryEnter(monitor, Timeout.Infinite);
        }

        /// <summary>
        /// Attempts, for the specified amount of time, to acquire an exclusive lock on the specified object.
        /// </summary>
        /// <param name="monitor">The object on which to acquire the lock.</param>
        /// <returns>true if the current thread acquires the lock without blocking; otherwise, false.</returns>
        public static bool TryEnter(ACMonitorObject monitor)
        {
            return TryEnter(monitor, 0);
        }

        /// <summary>
        /// Attempts, for the specified amount of time, to acquire an exclusive lock on the specified object.
        /// </summary>
        /// <param name="monitor">The object on which to acquire the lock.</param>
        /// <param name="timeout">A TimeSpan representing the amount of time to wait for the lock. A value of –1 millisecond specifies an infinite wait.</param>
        /// <returns>true if the current thread acquires the lock without blocking; otherwise, false.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">timeout</exception>
        public static bool TryEnter(ACMonitorObject monitor, TimeSpan timeout)
        {
            long totalMilliseconds = (long)timeout.TotalMilliseconds;
            if (totalMilliseconds < -1 ||
                totalMilliseconds > Int32.MaxValue) throw new ArgumentOutOfRangeException("timeout");
            return TryEnter(monitor, (int)totalMilliseconds);
        }

        /// <summary>
        /// Attempts, for the specified amount of time, to acquire an exclusive lock on the specified object.
        /// </summary>
        /// <param name="monitor">The object on which to acquire the lock.</param>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait for the lock.</param>
        /// <returns>true if the current thread acquires the lock without blocking; otherwise, false.</returns>
        /// <exception cref="System.ArgumentNullException">monitor</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">millisecondsTimeout</exception>
        public static bool TryEnter(ACMonitorObject monitor, int millisecondsTimeout)
        {
            // Validate arguments
            if (monitor == null) 
                throw new ArgumentNullException("monitor");
            if (millisecondsTimeout < 0 && millisecondsTimeout != Timeout.Infinite) throw new ArgumentOutOfRangeException("millisecondsTimeout");

            if (UseSimpleMonitor)
            {
                return Monitor.TryEnter(monitor, millisecondsTimeout);
            }
            else
            {
                // Keep track of whether we actually acquired the monitor or not
                bool thisThreadOwnsMonitor = false;
                MonitorState ms = null;
                try
                {
                    // Register the current thread as waiting on the monitor.
                    // Take the global lock before manipulating shared state.  Note that by our lock order, 
                    // we can take _globalLock while holding an individual monitor, but we *can't* take a 
                    // monitor while holding _globalLock; otherwise, we'd risk deadlock.
                    lock (_globalLock)
                    {
                        // Get the internal data for this monitor.  If not data exists, create it.
                        if (!_monitorStates.TryGetValue(monitor, out ms))
                        {
                            _monitorStates[monitor] = ms = new MonitorState(monitor, _monitorStates.Count);
                        }

                        // If we already hold this lock, then there's no chance of deadlock by waiting on it,
                        // since monitors are reentrant.  If we don't hold the lock, register our intent to 
                        // wait on it and check for deadlock.
                        if (ms.OwningThread != Thread.CurrentThread)
                        {
                            ms.WaitingThreads.Add(Thread.CurrentThread);
                            ThrowIfDeadlockDetected(ms, ValidateLockHierarchy, monitor);
                        }
                        else if (ValidateLockHierarchy)
                        {
                            Dictionary<Thread, List<MonitorState>> locksHeldByThreads;
                            Dictionary<MonitorState, List<Thread>> threadsWaitingOnLocks;
                            CreateThreadAndLockTables(out locksHeldByThreads, out threadsWaitingOnLocks);

                            List<MonitorState> monitorsInCurrentThread;
                            if (locksHeldByThreads.TryGetValue(Thread.CurrentThread, out monitorsInCurrentThread))
                            {
                                MonitorState minMS  = monitorsInCurrentThread
                                    .Where(c => c.MonitorObject.LockLevel > 0)
                                    .OrderBy(c => c.MonitorObject.LockLevel)
                                    .FirstOrDefault();
                                if (minMS != null 
                                    && monitor.LockLevel > 0                                     
                                    && monitor.LockLevel > minMS.MonitorObject.LockLevel)
                                {
                                    if (System.Diagnostics.Debugger.IsAttached)
                                        throw new SynchronizationLockException(String.Format("The Hierarchy of locklevels was not met. The current locklevel of {0} is lower than the acquiring lock {1}. Please verify/modify your callstack that the order of locklevels is met.", minMS.MonitorObject.LockLevel, monitor.LockLevel));
                                    else
                                    {
                                        if (Database.Root != null)
                                        {
                                            try
                                            {
                                                StackTrace st = new StackTrace(true);
                                                string trace = st.ToString();
                                                Database.Root.Messages.LogException("ACMonitor.CreateDeadlockDescription()", "Deadlock-Warning!", String.Format("The Hierarchy of locklevels was not met. The current locklevel of {0} is lower than the acquiring lock {1}. Please verify/modify your callstack that the order of locklevels is met. Trace:\n{2}", minMS.MonitorObject.LockLevel, monitor.LockLevel, trace));
                                            }
                                            catch (Exception e)
                                            {
                                                string msg = e.Message;
                                                if (e.InnerException != null && e.InnerException.Message != null)
                                                    msg += " Inner:" + e.InnerException.Message;

                                                if (Database.Root != null && Database.Root.Messages != null)
                                                    Database.Root.Messages.LogException("ACMonitor", "TryEnter", msg);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

//#if DEBUG
//                    if (millisecondsTimeout == Timeout.Infinite && System.Diagnostics.Debugger.IsAttached)
//                    {
//                        millisecondsTimeout = 2345;
//                        thisThreadOwnsMonitor = Monitor.TryEnter(monitor, millisecondsTimeout);
//                        if (!thisThreadOwnsMonitor)
//                        {
//                            System.Diagnostics.Debugger.Break();
//                            thisThreadOwnsMonitor = Monitor.TryEnter(monitor, Timeout.Infinite);
//                        }
//                    }
//                    else
//                        thisThreadOwnsMonitor = Monitor.TryEnter(monitor, millisecondsTimeout);
//#else
                    thisThreadOwnsMonitor = Monitor.TryEnter(monitor, millisecondsTimeout);
//#endif


                    // At this point we now may own the monitor...
                }
                finally
                {
                    lock (_globalLock)
                    {
                        if (ms != null) // This would only be null if an exception occurred at a really weird place
                        {
                            // We're no longer waiting on the monitor, either because something went wrong
                            // in the wait or because we now own the monitor
                            ms.WaitingThreads.Remove(Thread.CurrentThread);

                            // If we did get the monitor, then note that we now own it
                            if (thisThreadOwnsMonitor)
                            {
                                if (ms.OwningThread != Thread.CurrentThread) 
                                    ms.OwningThread = Thread.CurrentThread;
                                else 
                                    ms.ReentranceCount++;
                            }
                        }
                    }
                }

                // Return whether we obtained the monitor or not
                return thisThreadOwnsMonitor;
            }
        }

        /// <summary>
        /// Releases an exclusive lock on the specified object.
        /// </summary>
        /// <param name="monitor">Releases an exclusive lock on the specified object.</param>
        /// <exception cref="System.ArgumentNullException">monitor</exception>
        /// <exception cref="System.Threading.SynchronizationLockException">
        /// </exception>
        public static void Exit(ACMonitorObject monitor)
        {
            // Validate arguments
            if (monitor == null) throw new ArgumentNullException("monitor");

            if (UseSimpleMonitor)
            {
                Monitor.Exit(monitor);
            }
            else
            {
                // Take the global lock to manipulate shared state
                lock (_globalLock)
                {
                    // Grab the MonitorState for this monitor.  
                    MonitorState ms;
                    if (!_monitorStates.TryGetValue(monitor, out ms)) 
                        throw new SynchronizationLockException();

                    // If the user is trying to release a monitor not held by this thread,
                    // that's an error.
                    if (ms.OwningThread != Thread.CurrentThread)
                    {
                        throw new SynchronizationLockException();
                    }
                    // If this thread has reentered this monitor, just decrement the count
                    else if (ms.ReentranceCount > 0)
                    {
                        ms.ReentranceCount--;
                    }
                    // Otherwise, if this thread will now be releasing the monitor,
                    // update the MonitorState accordingly.  And in addition,
                    // if there are no threads waiting on this monitor, free
                    // the MonitorState data by removing it from the mapping table.
                    else
                    {
                        ms.OwningThread = null;
                        if (ms.WaitingThreads.Count == 0) 
                            _monitorStates.Remove(monitor);
                    }

                    // Finally, exit the monitor.
                    Monitor.Exit(monitor);
                }
            }
        }

        /// <summary>
        /// Throws an exception if a deadlock would be caused by the current thread waiting on the specified lock.
        /// </summary>
        /// <param name="targetMs">The target lock data.</param>
        /// <param name="validateLockHierarchy">validateLockHierarchy</param>
        /// <param name="monitor">monitor</param>
        /// <exception cref="System.Threading.SynchronizationLockException"></exception>
        private static void ThrowIfDeadlockDetected(MonitorState targetMs, bool validateLockHierarchy, ACMonitorObject monitor)
        {
            // If no thread is holding the target lock, then this won't deadlock...
            if (targetMs.OwningThread == null) 
                return;

            // For the deadlock detection algorithm, we need to know what locks are
            // currently held by which threads as well as which threads are waiting on
            // which locks. We already have this information, but we need it in a tabular
            // form for easier use and better perf.
            Dictionary<Thread, List<MonitorState>> locksHeldByThreads;
            Dictionary<MonitorState, List<Thread>> threadsWaitingOnLocks;
            CreateThreadAndLockTables(out locksHeldByThreads, out threadsWaitingOnLocks);

            if (ValidateLockHierarchy)
            {
                List<MonitorState> monitorsInCurrentThread;
                if (locksHeldByThreads.TryGetValue(Thread.CurrentThread, out monitorsInCurrentThread))
                {
                    MonitorState minMS = monitorsInCurrentThread
                                    .Where(c => c.MonitorObject.LockLevel > 0)
                                    .OrderBy(c => c.MonitorObject.LockLevel)
                                    .FirstOrDefault();
                    if (minMS != null
                        && monitor.LockLevel > 0
                        && monitor.LockLevel > minMS.MonitorObject.LockLevel)
                    {
                        if (System.Diagnostics.Debugger.IsAttached)
                            throw new SynchronizationLockException(String.Format("The Hierarchy of locklevels was not met. The current locklevel of {0} is lower than the acquiring lock {1}. Please verify/modify your callstack that the order of locklevels is met.", minMS.MonitorObject.LockLevel, monitor.LockLevel));
                        else
                        {
                            if (Database.Root != null)
                            {
                                try
                                {
                                    StackTrace st = new StackTrace(true);
                                    string trace = st.ToString();
                                    Database.Root.Messages.LogException("ACMonitor.CreateDeadlockDescription()", "Deadlock-Warning!", String.Format("The Hierarchy of locklevels was not met. The current locklevel of {0} is lower than the acquiring lock {1}. Please verify/modify your callstack that the order of locklevels is met. Trace:\n{2}", minMS.MonitorObject.LockLevel, monitor.LockLevel, trace));
                                }
                                catch (Exception e)
                                {
                                    string msg = e.Message;
                                    if (e.InnerException != null && e.InnerException.Message != null)
                                        msg += " Inner:" + e.InnerException.Message;

                                    if (Database.Root != null && Database.Root.Messages != null)
                                        Database.Root.Messages.LogException("ACMonitor", "ThrowIfDeadlockDetected", msg);
                                }
                            }
                        }
                    }
                }
            }

            // As we iterate over the wait graph, we'll need to store the list of threads still left to examine
            Queue<CycleComponentNode> threadsToFollow = new Queue<CycleComponentNode>(locksHeldByThreads.Count);

            // But rather than just storing the thread, we also store the threads in the cycle that got us to this thread.
            // The top of the stack is the actual thread to be examined.
            threadsToFollow.Enqueue(new CycleComponentNode(Thread.CurrentThread, targetMs, null));

            while (threadsToFollow.Count > 0)
            {
                // Get the next thread to examine
                CycleComponentNode currentChain = threadsToFollow.Dequeue();
                Thread currentThread = currentChain.Thread;

                // If this thread doesn't hold any locks, no point in examining it
                List<MonitorState> locksHeldByThread;
                if (!locksHeldByThreads.TryGetValue(currentThread, out locksHeldByThread)) 
                    continue;

                // For each lock it does hold, add to the thread examination list all threads
                // waiting on it.  And for each, see if it completes a cycle that results in
                // a deadlock.
                foreach (MonitorState ms in locksHeldByThread)
                {
                    List<Thread> nextThreads;
                    if (!threadsWaitingOnLocks.TryGetValue(ms, out nextThreads))
                        continue;

                    foreach (Thread nextThread in nextThreads)
                    {
                        // If any thread waiting on this lock is in the current stack,
                        // it's completng a cycle... deadlock!
                        if (currentChain.ContainsThread(nextThread))
                            throw new SynchronizationLockException(CreateDeadlockDescription(currentChain, locksHeldByThreads));

                        // Clone the stack of threads in the possible cycle and add this to the top,
                        // then queue the stack for examination.
                        threadsToFollow.Enqueue(new CycleComponentNode(nextThread, ms, currentChain));
                    }
                }
            }
        }

#pragma warning disable CS0618
        /// <summary>
        /// Creates a textual description of the deadlock.
        /// </summary>
        /// <param name="currentChain">The deadlock cycle.</param>
        /// <param name="locksHeldByThreads">The table containing what locks are held by each thread holding locks.</param>
        /// <returns>The description of the deadlock.</returns>
        private static string CreateDeadlockDescription(
            CycleComponentNode currentChain,
            Dictionary<Thread, List<MonitorState>> locksHeldByThreads)
        {
//#if !EFCR
//            StringBuilder desc = new StringBuilder();
//            for (CycleComponentNode node = currentChain; node != null; node = node.Next)
//            {
//                desc.AppendFormat("Thread {0}, {1} waiting on {2} ({3:X}) while holding ",
//                    node.Thread.ManagedThreadId, node.Thread.Name, node.MonitorState.MonitorObject.ToString(),
//                    RuntimeHelpers.GetHashCode(node.MonitorState.MonitorObject));
//                bool needsComma = false;
//                foreach (MonitorState ms in locksHeldByThreads[node.Thread])
//                {
//                    if (needsComma) desc.Append(", ");
//                    desc.AppendFormat("{0} ({1:X})", ms.MonitorObject.ToString(),
//                        RuntimeHelpers.GetHashCode(ms.MonitorObject));
//                    needsComma = true;
//                }

//                StackTrace stackTrace = null;
//                var ready = new ManualResetEventSlim();
//                new Thread(() =>
//                {
//                    ready.Set();
//                    Thread.Sleep(200);
//                    try
//                    {
//                        node.Thread.Resume();
//                    }
//                    catch (Exception e)
//                    {
//                        string msg = e.Message;
//                        if (e.InnerException != null && e.InnerException.Message != null)
//                            msg += " Inner:" + e.InnerException.Message;

//                        if (Database.Root != null && Database.Root.Messages != null)
//                            Database.Root.Messages.LogException("ACMonitor", "CreateDeadlockDescription", msg);
//                    }
//                }
//                ).Start();

//                ready.Wait();
//                try
//                {
//                    desc.AppendLine();
//                    node.Thread.Suspend();
//                    stackTrace = new StackTrace(node.Thread, true);
//            desc.AppendFormat("Stacktrace of Thread: {0}, {1}", node.Thread.Name, node.Thread.ManagedThreadId);
//                    desc.AppendLine();
//                    for (int i = 0; i < stackTrace.FrameCount; i++)
//                    {
//                        StackFrame sf = stackTrace.GetFrame(i);
//                        desc.AppendFormat(" Method: {0}", sf.GetMethod());
//                        desc.AppendFormat(" File: {0}", sf.GetFileName());
//                        desc.AppendFormat(" Line Number: {0}", sf.GetFileLineNumber());
//                        desc.AppendLine();
//                    }
//                }
//                catch (Exception e)
//                {
//                    string msg = e.Message;
//                    if (e.InnerException != null && e.InnerException.Message != null)
//                        msg += " Inner:" + e.InnerException.Message;

//                    if (Database.Root != null && Database.Root.Messages != null)
//                        Database.Root.Messages.LogException("ACMonitor", "CreateDeadlockDescription(10)", msg);
//                }
//                finally
//                {
//                    try
//                    {
//                        node.Thread.Resume();
//                    }
//                    catch (Exception e)
//                    {
//                        stackTrace = null;

//                        string msg = e.Message;
//                        if (e.InnerException != null && e.InnerException.Message != null)
//                            msg += " Inner:" + e.InnerException.Message;

//                        if (Database.Root != null && Database.Root.Messages != null)
//                            Database.Root.Messages.LogException("ACMonitor", "CreateDeadlockDescription(20)", msg);
//                    }
//                }

//                desc.AppendLine();
//            }
//            string deadlockDesc = desc.ToString();
//            if (Database.Root != null)
//                Database.Root.Messages.LogException("ACMonitor.CreateDeadlockDescription()", "Deadlock!", deadlockDesc);
//            return deadlockDesc;
//#endif
            return "";
        }

        /// <summary>
        /// Generates mapping tables based on the data in _monitorStates.
        /// </summary>
        /// <param name="locksHeldByThreads">A table mapping locks to the threads that hold them.</param>
        /// <param name="threadsWaitingOnLocks">A table mapping threads to the locks they're waiting on.</param>
        private static void CreateThreadAndLockTables(
            out Dictionary<Thread, List<MonitorState>> locksHeldByThreads,
            out Dictionary<MonitorState, List<Thread>> threadsWaitingOnLocks)
        {
            // Create a table of all of the locks held by threads
            locksHeldByThreads = new Dictionary<Thread, List<MonitorState>>(_monitorStates.Values.Count);
            foreach (MonitorState ms in _monitorStates.Values)
            {
                if (ms.OwningThread != null)
                {
                    List<MonitorState> locksHeldByThread;
                    if (!locksHeldByThreads.TryGetValue(ms.OwningThread, out locksHeldByThread))
                    {
                        locksHeldByThread = new List<MonitorState>(1);
                        locksHeldByThreads.Add(ms.OwningThread, locksHeldByThread);
                    }
                    locksHeldByThread.Add(ms);
                }
            }

            // Create a table of all threads waiting on locks
            threadsWaitingOnLocks = new Dictionary<MonitorState, List<Thread>>(_monitorStates.Values.Count);
            foreach (MonitorState ms in _monitorStates.Values)
            {
                if (ms.WaitingThreads.Count > 0)
                {
                    List<Thread> threadsWaitingOnLock;
                    if (!threadsWaitingOnLocks.TryGetValue(ms, out threadsWaitingOnLock))
                    {
                        threadsWaitingOnLock = new List<Thread>(ms.WaitingThreads.Count);
                        threadsWaitingOnLocks.Add(ms, threadsWaitingOnLock);
                    }
                    foreach (Thread t in ms.WaitingThreads)
                    {
                        List<MonitorState> locksHeldByThread;
                        if (!locksHeldByThreads.TryGetValue(t, out locksHeldByThread) ||
                            !locksHeldByThread.Contains(ms))
                        {
                            threadsWaitingOnLock.Add(t);
                        }
                    }
                }
            }
        }
#pragma warning restore CS0618

        /// <summary>
        /// Information on an underlying monitor, the thread holding it, threads waiting on it, and so forth.
        /// </summary>
        private class MonitorState
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MonitorState"/> class.
            /// </summary>
            /// <param name="monitor">The monitor.</param>
            /// <param name="sequence">The sequence.</param>
            public MonitorState(ACMonitorObject monitor, int sequence) { MonitorObject = monitor; Sequence = sequence;  }
            /// <summary>
            /// The monitor object
            /// </summary>
            public ACMonitorObject MonitorObject;
            /// <summary>
            /// The owning thread
            /// </summary>
            public Thread OwningThread;
            /// <summary>
            /// The reentrance count
            /// </summary>
            public int ReentranceCount;

            public int Sequence;
            /// <summary>
            /// The waiting threads
            /// </summary>
            public List<Thread> WaitingThreads = new List<Thread>();
        }

        /// <summary>
        /// Represents a node in a possible deadlock cycle.
        /// </summary>
        private class CycleComponentNode
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CycleComponentNode"/> class.
            /// </summary>
            /// <param name="thread">The thread.</param>
            /// <param name="ms">The ms.</param>
            /// <param name="next">The next.</param>
            public CycleComponentNode(Thread thread, MonitorState ms, CycleComponentNode next)
            {
                Thread = thread;
                MonitorState = ms;
                Next = next;
            }

            /// <summary>
            /// The thread
            /// </summary>
            public Thread Thread;
            /// <summary>
            /// The monitor state
            /// </summary>
            public MonitorState MonitorState;
            /// <summary>
            /// The next
            /// </summary>
            public CycleComponentNode Next;

            /// <summary>
            /// Determines whether the specified t contains thread.
            /// </summary>
            /// <param name="t">The t.</param>
            /// <returns><c>true</c> if the specified t contains thread; otherwise, <c>false</c>.</returns>
            public bool ContainsThread(Thread t)
            {
                for (CycleComponentNode node = this; node != null; node = node.Next)
                {
                    if (node.Thread == t) return true;
                }
                return false;
            }
        }
    }
}
