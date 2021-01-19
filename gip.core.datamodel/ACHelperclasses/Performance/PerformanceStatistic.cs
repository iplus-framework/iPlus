﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    public class PerformanceStatistic
    {
        #region c'tors
        public PerformanceStatistic(int id)
        {
            _Id = id;
        }
        #endregion


        #region Properties
        public readonly List<PerformanceEvent> Events  = new List<PerformanceEvent>();
        public readonly List<PerformanceEvent> CallStack = new List<PerformanceEvent>();

        private int _Id;
        public int Id
        {
            get
            {
                return _Id;
            }
        }

        object _LockEvents = new object();

        private TimeSpan _TotalExecutionTime;
        public TimeSpan TotalExecutionTime
        {
            get
            {
                return _TotalExecutionTime;
            }
        }

        private static int? _MaxPerfEntries = null;
        public static int MaxPerfEntries
        {
            get
            {
                if (_MaxPerfEntries.HasValue)
                    return _MaxPerfEntries.Value;
                _MaxPerfEntries = 20;
                try
                {
                    PerfLogConfiguration coreConfig = (PerfLogConfiguration)CommandLineHelper.ConfigCurrentDir.GetSection("Logging/PerfLogConfiguration");
                    if (coreConfig != null)
                    {
                        if (coreConfig.MaxPerfEntries >= 0)
                            _MaxPerfEntries = coreConfig.MaxPerfEntries;
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("PerformanceStatistic", "MaxPerfEntries", msg);
                }
                return _MaxPerfEntries.Value;
            }
        }
        #endregion


        #region Methods
        /// <summary>
        /// Start des Timers
        /// </summary>
        internal PerformanceEvent Start(bool checkCallStack)
        {
            var perfEvent = new PerformanceEvent(checkCallStack);
            perfEvent.Start();
            if (checkCallStack)
            {
                lock (_LockEvents)
                {
                    CallStack.Add(perfEvent);
                }
            }
            return perfEvent;
        }

        /// <summary>
        /// Stop des Timers
        /// </summary>
        internal bool Stop(PerformanceEvent perfEvent)
        {
            if (perfEvent == null)
                throw new ArgumentNullException();
            perfEvent.Stop();
            _TotalExecutionTime += perfEvent.Elapsed;

            lock (_LockEvents)
            {
                if (perfEvent.InCallStack)
                    CallStack.Remove(perfEvent);
                if (Events.Count < MaxPerfEntries)
                {
                    Events.Add(perfEvent);
                    return true;
                }
                var lowestEvent = Events.OrderBy(c => c.Elapsed).FirstOrDefault();
                if (lowestEvent.Elapsed > perfEvent.Elapsed)
                    return false;
                Events.Remove(lowestEvent);
                Events.Add(perfEvent);
                return true;
            }
        }

        public void DumpAndReset(StringBuilder sb, PerformanceLoggerInstance parent)
        {
            lock (_LockEvents)
            {
                double percent = (TotalExecutionTime.TotalMilliseconds / parent.TotalExecutionTime.TotalMilliseconds) * 100;
                sb.AppendLine(String.Format("  For ID: {0}, ExecTime: {1}, Usage {2:0.000} %", _Id, TotalExecutionTime, percent));
                if (Events.Any())
                {
                    foreach (var perfEvent in Events.OrderByDescending(c => c.Elapsed))
                    {
                        percent = (perfEvent.Elapsed.TotalMilliseconds / TotalExecutionTime.TotalMilliseconds) * 100;
                        sb.AppendLine(String.Format("      {0}, {1}, {2:000.000} %", perfEvent.StartTime, perfEvent.Elapsed, percent));
                    }
                }
                if (CallStack.Any())
                {
                    sb.AppendLine("!! REMAINING EVENTS IN CALLSTACK !!");
                    foreach (var perfEvent in CallStack.OrderByDescending(c => c.StartTime))
                    {
                        sb.AppendLine(String.Format("      {0}, {1}", perfEvent.StartTime, perfEvent.Elapsed));
                    }
                }
            }
        }
        #endregion
    }
}