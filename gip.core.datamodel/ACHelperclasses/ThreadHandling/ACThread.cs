// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-07-2012
// ***********************************************************************
// <copyright file="ACThread.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; 
using System.Linq;
using System.Text;
using System.Threading;
using System.Security;
using System.Security.Principal;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACThread
    /// </summary>
    public class ACThread
    {
        #region c'tors
        /// <summary>
        /// Initializes a new instance of the <see cref="ACThread"/> class.
        /// </summary>
        /// <param name="start">The start.</param>
        public ACThread(ParameterizedThreadStart start)
        {
            this._ParameterizedThreadStart = start;
            this._Thread = new Thread(this.StartThreadParameterized);
            lock (_Lock)
            {
                _ACThreadList.Add(this);
            }
            if (Database.Root != null && Database.Root.Messages != null)
                Database.Root.Messages.LogDebug("ACThread", "ACThread()", String.Format("New Thread ID:{0} Name:{1}",_Thread.ManagedThreadId,_Thread.Name));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACThread"/> class.
        /// </summary>
        /// <param name="start">The start.</param>
        public ACThread(ThreadStart start)
        {
            this._ThreadStart = start;
            this._Thread = new Thread(this.StartThread);
            lock (_Lock)
            {
                _ACThreadList.Add(this);
            }
            if (Database.Root != null && Database.Root.Messages != null)
                Database.Root.Messages.LogDebug("ACThread", "ACThread()", String.Format("New Thread ID:{0} Name:{1}", _Thread.ManagedThreadId, _Thread.Name));
        }


        /// <summary>
        /// Initializes a new instance of the ACThread class.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="parentQueue">The parentQueue.</param>
        public ACThread(ThreadStart start, ACDelegateQueue parentQueue)
            : this(start)
        {
            _ParentQueue = parentQueue;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ACThread"/> class.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="maxStackSize">Size of the max stack.</param>
        public ACThread(ParameterizedThreadStart start, int maxStackSize)
        {
            this._ParameterizedThreadStart = start;
            this._Thread = new Thread(this.StartThreadParameterized, maxStackSize);
            lock (_Lock)
            {
                _ACThreadList.Add(this);
            }
            if (Database.Root != null && Database.Root.Messages != null)
                Database.Root.Messages.LogDebug("ACThread", "ACThread()", String.Format("New Thread ID:{0} Name:{1}", _Thread.ManagedThreadId, _Thread.Name));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACThread"/> class.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="maxStackSize">Size of the max stack.</param>
        public ACThread(ThreadStart start, int maxStackSize)
        {
            this._ThreadStart = start;
            this._Thread = new Thread(this.StartThread, maxStackSize);
            lock (_Lock)
            {
                _ACThreadList.Add(this);
            }
            if (Database.Root != null && Database.Root.Messages != null)
                Database.Root.Messages.LogDebug("ACThread", "ACThread()", String.Format("New Thread ID:{0} Name:{1}", _Thread.ManagedThreadId, _Thread.Name));
        }
        #endregion

        #region Properties

        #region Private
        /// <summary>
        /// The _ lock
        /// </summary>
        private static readonly object _Lock = new object();

        /// <summary>
        /// The _ parameterized thread start
        /// </summary>
        private readonly ParameterizedThreadStart _ParameterizedThreadStart;
        /// <summary>
        /// The _ thread start
        /// </summary>
        private readonly ThreadStart _ThreadStart;

        private ACDelegateQueue _ParentQueue;

        public ACDelegateQueue ParentQueue
        {
            get
            {
                return _ParentQueue;
            }
            internal set
            {
                _ParentQueue = value;
            }
        }
        #endregion

        #region Static
        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>The count.</value>
        public static int Count
        {
            get
            {
                lock (_Lock)
                {
                    return _ACThreadList.Count;
                }
            }
        }

        /// <summary>
        /// The _ AC thread list
        /// </summary>
        private static readonly List<ACThread> _ACThreadList = new List<ACThread>();
        /// <summary>
        /// Gets the AC thread list.
        /// </summary>
        /// <value>The AC thread list.</value>
        public static IEnumerable<ACThread> ACThreadList
        {
            get
            {
                lock (_Lock)
                {
                    return new ReadOnlyCollection<ACThread>(_ACThreadList.ToArray());
                }
            }
        }

        /// <summary>
        /// Gets the thread list.
        /// </summary>
        /// <value>The thread list.</value>
        public static IEnumerable<Thread> ThreadList
        {
            get
            {
                return ACThreadList.Select(c => c.Thread);
            }
        }

        /// <summary>
        /// Gets the current thread.
        /// </summary>
        /// <value>The current thread.</value>
        public static ACThread CurrentThread
        {
            get
            {
                return ACThreadList.Where(c => c.Thread == Thread.CurrentThread).FirstOrDefault();
            }
        }

        #endregion

        #region Wrapped Properties
        /// <summary>
        /// Gets or sets the state of the apartment.
        /// </summary>
        /// <value>The state of the apartment.</value>
        public ApartmentState ApartmentState
        {
            get
            {
                return this.Thread.GetApartmentState();
            }

            set
            {
                this.Thread.TrySetApartmentState(value);
            }
        }

        /// <summary>
        /// Gets or sets the current culture.
        /// </summary>
        /// <value>The current culture.</value>
        public CultureInfo CurrentCulture 
        {
            get
            {
                return this.Thread.CurrentCulture;
            }

            set
            {
                this.Thread.CurrentCulture = value;
            }
        }

        /// <summary>
        /// Gets or sets the current UI culture.
        /// </summary>
        /// <value>The current UI culture.</value>
        public CultureInfo CurrentUICulture 
        {
            get
            {
                return this.Thread.CurrentUICulture;
            }

            set
            {
                this.Thread.CurrentUICulture = value;
            }
        }

        /// <summary>
        /// Gets the execution context.
        /// </summary>
        /// <value>The execution context.</value>
        public ExecutionContext ExecutionContext 
        {
            get
            {
                return this.Thread.ExecutionContext;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is alive.
        /// </summary>
        /// <value><c>true</c> if this instance is alive; otherwise, <c>false</c>.</value>
        public bool IsAlive 
        {
            get
            {
                return this.Thread.IsAlive;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is background.
        /// </summary>
        /// <value><c>true</c> if this instance is background; otherwise, <c>false</c>.</value>
        public bool IsBackground 
        {
            get
            {
                return this.Thread.IsBackground;
            }

            set
            {
                this.Thread.IsBackground = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is thread pool thread.
        /// </summary>
        /// <value><c>true</c> if this instance is thread pool thread; otherwise, <c>false</c>.</value>
        public bool IsThreadPoolThread 
        {
            get
            {
                return this.Thread.IsThreadPoolThread;
            }
        }

        /// <summary>
        /// Gets the managed thread id.
        /// </summary>
        /// <value>The managed thread id.</value>
        public int ManagedThreadId 
        {
            get
            {
                return this.Thread.ManagedThreadId;
            }
        }


        private Int32? _NativeThreadId;
        public Int32 NativeThreadId
        {
            get
            {
                try
                {
                    if (_NativeThreadId.HasValue)
                        return _NativeThreadId.Value;

                    // http://naveensrinivasan.github.io/2011/01/09/correlating-between-.net-and-native-thread-in-windbg/
                    // https://en.wikipedia.org/wiki/Win32_Thread_Information_Block
                    // http://www.geoffchappell.com/studies/windows/win32/ntdll/structs/teb/index.htm
                    // https://stackoverflow.com/questions/1679243/getting-the-thread-id-from-a-thread

                    var f = typeof(Thread).GetField("DONT_USE_InternalThread",
                        BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);

                    var pInternalThread = (IntPtr)f.GetValue(_Thread);
                    _NativeThreadId = Marshal.ReadInt32(pInternalThread, (IntPtr.Size == 8) ? 0x022C : 0x0160); // bei 32-bit-Systemen adresse 0x160, bei 64 bit adresse 0x22C, bei Windows 7, Windows 8, Windows 10, Windows 2012 Server
                    return _NativeThreadId.Value;
                }
                catch (Exception e)
                {
                    _NativeThreadId = - 1;

                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("ACThread", "NativeThreadId", msg);
                }
                return _NativeThreadId.Value;
            }
        }

        internal void UpdateNativeThreadID()
        {
            if (!_NativeThreadId.HasValue)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    _NativeThreadId = GetCurrentWin32ThreadId();
                else
                    _NativeThreadId = (new object()).GetHashCode();
            }
        }

        internal void ResetNativeThreadID()
        {
            _NativeThreadId = null;
        }

        [DllImport("Kernel32", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        public static extern Int32 GetCurrentWin32ThreadId();


        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name 
        {
            get
            {
                return this.Thread.Name;
            }

            set
            {
                this.Thread.Name = value;
            }
        }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>The priority.</value>
        public ThreadPriority Priority 
        {
            get
            {
                return this.Thread.Priority;
            }

            set
            {
                this.Thread.Priority = value;
            }
        }

        /// <summary>
        /// Gets the state of the thread.
        /// </summary>
        /// <value>The state of the thread.</value>
        public ThreadState ThreadState 
        {
            get
            {
                return this.Thread.ThreadState;
            }
        }
        #endregion

        #region Extended Properties
        /// <summary>
        /// The _ thread
        /// </summary>
        private readonly Thread _Thread;
        /// <summary>
        /// Gets the thread.
        /// </summary>
        /// <value>The thread.</value>
        public Thread Thread
        {
            get
            {
                return this._Thread;
            }
        }

        #endregion

        #region Statistics Properties

        private PerformanceEvent _PerfEvent;

        private static PerformanceLogger _PerfLogger = new PerformanceLogger("ACThread-Analysis");
        public static PerformanceLogger PerfLogger
        {
            get
            {
                return _PerfLogger;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Wrapped Methods
        /// <summary>
        /// Aborts this instance.
        /// </summary>
        public void Abort()
        {
            if (Database.Root != null && Database.Root.Messages != null)
                Database.Root.Messages.LogDebug("ACThread", "Abort()", String.Format("Aborting Thread ID:{0} Name:{1}", ManagedThreadId, Name));
            this.Thread.Abort();
            lock (_Lock)
            {
                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogDebug("ACThread", "Abort()", String.Format("Succeeded Thread ID:{0} Name:{1}", ManagedThreadId, Name));
                _ACThreadList.Remove(this);
            }
        }

        /// <summary>
        /// Aborts the specified state info.
        /// </summary>
        /// <param name="stateInfo">The state info.</param>
        [SecuritySafeCritical]
        public void Abort(object stateInfo)
        {
            if (Database.Root != null && Database.Root.Messages != null)
                Database.Root.Messages.LogDebug("ACThread", "Abort(object stateInfo)", String.Format("Aborting Thread ID:{0} Name:{1}", ManagedThreadId, Name));
            this.Thread.Abort(stateInfo);
            lock (_Lock)
            {
                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogDebug("ACThread", "Abort(object stateInfo)", String.Format("Succeeded Thread ID:{0} Name:{1}", ManagedThreadId, Name));
                _ACThreadList.Remove(this);
            }
        }

        /// <summary>
        /// Interrupts this instance.
        /// </summary>
        [SecuritySafeCritical]
        public void Interrupt()
        {
            this.Thread.Interrupt();
        }

        /// <summary>
        /// Joins this instance.
        /// </summary>
        [SecuritySafeCritical]
        public void Join()
        {
            if (Database.Root != null && Database.Root.Messages != null)
                Database.Root.Messages.LogDebug("ACThread", "Join()", String.Format("Joining Thread ID:{0} Name:{1}", ManagedThreadId, Name));

            Exception ex = null;
            try
            {
                this.Thread.Join();
            }
            catch (Exception e)
            {
                ex = e;
            }
            lock (_Lock)
            {
                if (Database.Root != null && Database.Root.Messages != null)
                {
                    if (ex != null)
                        Database.Root.Messages.LogDebug("ACThread", "Join()", String.Format("NOT Succeeded Thread ID:{0} Name:{1} Reason:{2}", ManagedThreadId, Name, ex.Message));
                    else
                        Database.Root.Messages.LogDebug("ACThread", "Join()", String.Format("Succeeded Thread ID:{0} Name:{1}", ManagedThreadId, Name));
                }
                _ACThreadList.Remove(this);
            }
        }

        /// <summary>
        /// Joins the specified milliseconds timeout.
        /// </summary>
        /// <param name="millisecondsTimeout">The milliseconds timeout.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        [SecuritySafeCritical]
        public bool Join(int millisecondsTimeout)
        {
            if (Database.Root != null && Database.Root.Messages != null)
                Database.Root.Messages.LogDebug("ACThread", "Join(int millisecondsTimeout)", String.Format("Joining Thread ID:{0} Name:{1}", ManagedThreadId, Name));
            Exception ex = null;
            bool result = true;
            try
            {
                result = this.Thread.Join(millisecondsTimeout);
            }
            catch (Exception e)
            {
                ex = e;
            }
            lock (_Lock)
            {
                if (Database.Root != null && Database.Root.Messages != null)
                {
                    if (ex != null)
                        Database.Root.Messages.LogDebug("ACThread", "Join(int millisecondsTimeout)", String.Format("NOT Succeeded Thread ID:{0} Name:{1} Reason:{2}", ManagedThreadId, Name, ex.Message));
                    else
                        Database.Root.Messages.LogDebug("ACThread", "Join(int millisecondsTimeout)", String.Format("Succeeded Thread ID:{0} Name:{1}", ManagedThreadId, Name));
                }
                _ACThreadList.Remove(this);
            }
            return result;
        }

        /// <summary>
        /// Joins the specified timeout.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public bool Join(TimeSpan timeout)
        {
            if (Database.Root != null && Database.Root.Messages != null)
                Database.Root.Messages.LogDebug("ACThread", "Join(TimeSpan timeout)", String.Format("Joining Thread ID:{0} Name:{1}", ManagedThreadId, Name));
            Exception ex = null;
            bool result = true;
            try
            {
                result = this.Thread.Join(timeout);
            }
            catch (Exception e)
            {
                ex = e;
            }
            lock (_Lock)
            {
                if (Database.Root != null && Database.Root.Messages != null)
                {
                    if (ex != null)
                        Database.Root.Messages.LogDebug("ACThread", "Join(TimeSpan timeout)", String.Format("NOT Succeeded Thread ID:{0} Name:{1} Reason:{2}", ManagedThreadId, Name, ex.Message));
                    else
                        Database.Root.Messages.LogDebug("ACThread", "Join(TimeSpan timeout)", String.Format("Succeeded Thread ID:{0} Name:{1}", ManagedThreadId, Name));
                }
                _ACThreadList.Remove(this);
            }
            return result;
        }

#pragma warning disable CS0618
        /// <summary>
        /// Resumes this instance.
        /// </summary>
        [SecuritySafeCritical]
        public void Resume()
        {
            this.Thread.Resume();
        }

        /// <summary>
        /// Sets the state of the apartment.
        /// </summary>
        /// <param name="state">The state.</param>
        [SecuritySafeCritical]
        public void SetApartmentState(ApartmentState state)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                this.Thread.SetApartmentState(state);
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        [SecuritySafeCritical]
        public void Start()
        {
            this.Thread.Start();
            lock (_Lock)
            {
                if (!_ACThreadList.Contains(this))
                    _ACThreadList.Add(this);
            }
        }

        /// <summary>
        /// Starts the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        [SecuritySafeCritical]
        public void Start(object parameter)
        {
            this.Thread.Start(parameter);
            lock (_Lock)
            {
                if (!_ACThreadList.Contains(this))
                    _ACThreadList.Add(this);
            }
        }

        /// <summary>
        /// Suspends this instance.
        /// </summary>
        [SecuritySafeCritical]
        public void Suspend()
        {
            this.Thread.Suspend();
        }
#pragma warning restore CS0618

        /// <summary>
        /// Tries the state of the set apartment.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        [SecuritySafeCritical]
        public bool TrySetApartmentState(ApartmentState state)
        {
            return this.Thread.TrySetApartmentState(state);
        }


        #endregion

        #region Extended Methods
        /// <summary>
        /// Starts the thread parameterized.
        /// </summary>
        /// <param name="obj">The obj.</param>
        private void StartThreadParameterized(object obj)
        {
            try
            {
                this._ParameterizedThreadStart(obj);
            }
            finally
            {
                lock (_Lock)
                {
                    _ACThreadList.Remove(this);
                }
            }
        }

        /// <summary>
        /// Starts the thread.
        /// </summary>
        private void StartThread()
        {
            try
            {
                this._ThreadStart();
            }
            catch (ThreadAbortException e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (Database.Root != null && Database.Root.Messages != null)
                    Database.Root.Messages.LogException("ACThread", "StartThread", msg);
            }
            finally
            {
                lock (_Lock)
                {
                    _ACThreadList.Remove(this);
                }
            }
        }

        #endregion

        #region Statistics Methods

        public PerformanceEvent StartReportingExeTime()
        {
            if (!PerfLogger.Active)
                return null;
            UpdateNativeThreadID();
            _PerfEvent = PerfLogger.Start(this.Name, this.ManagedThreadId);
            return _PerfEvent;
        }

        public void StopReportingExeTime()
        {
            if (_PerfEvent != null)
                PerfLogger.Stop(this.Name, this.ManagedThreadId, _PerfEvent);
        }

        public static string DumpStatisticsAndReset()
        {
            string log = ACThread.PerfLogger.DumpAndReset();
            StringBuilder sb = new StringBuilder();
            sb.Append(log);
            sb.AppendLine("");
            sb.AppendLine("<<< THREAD-STATES >>>");
            List<Tuple<Int32, Int32>> mapping = new List<Tuple<int, int>>();
            foreach (ACThread acThread in ACThreadList)
            {
                sb.AppendLine(String.Format("{0}({1}): {2}", acThread.Name, acThread.ManagedThreadId, acThread.ThreadState));
                mapping.Add(new Tuple<int, int>(acThread.ManagedThreadId, acThread.NativeThreadId));
                acThread.ResetNativeThreadID();
            }
            sb.AppendLine("");
            sb.AppendLine("<<< MAPPING MANAGED-Id to NATIVE-Id >>>");
            foreach (var item in mapping.OrderBy(c => c.Item1))
            {
                sb.AppendLine(String.Format("{0} -> {1}", item.Item1, item.Item2));
            }
            sb.AppendLine("");
            sb.AppendLine("<<< MAPPING NATIVE-Id to MANAGED-Id >>>");
            foreach (var item in mapping.OrderBy(c => c.Item2))
            {
                sb.AppendLine(String.Format("{0} -> {1}", item.Item2, item.Item1));
            }
            return sb.ToString();
        }

        #endregion

        #endregion
    } 
}
