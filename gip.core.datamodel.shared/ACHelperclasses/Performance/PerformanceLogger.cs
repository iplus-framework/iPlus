using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace gip.core.datamodel
{
    public class PerformanceLogger
    {
        #region c'tors
        public PerformanceLogger(string logName)
        {
            _LogName = logName;
        }
        #endregion

        #region Properties
        private string _LogName;
        public string LogName
        {
            get
            {
                return _LogName;
            }
        }

        Dictionary<string, PerformanceLoggerInstance> _Log = new Dictionary<string, PerformanceLoggerInstance>();
        List<PerformanceEvent> _ActivePerfEvents = new List<PerformanceEvent>();
        object _LogLock = new object();

        private TimeSpan _TotalExecutionTime;
        public TimeSpan TotalExecutionTime
        {
            get
            {
                return _TotalExecutionTime;
            }
        }

        private bool? _Active = null;
        public bool Active
        {
            get
            {
                if (_Active.HasValue)
                    return _Active.Value;
                _Active = false;
#if NETFRAMEWORK
                try
                {
                    PerfLogConfiguration coreConfig = (PerfLogConfiguration)CommandLineHelper.ConfigCurrentDir.GetSection("Logging/PerfLogConfiguration");
                    if (coreConfig != null)
                    {
                        if (coreConfig.Active)
                            _Active = coreConfig.Active;
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (Database.Root != null && Database.Root.Messages != null)
                        Database.Root.Messages.LogException("PerformanceLogger", "Active", msg);
                }
#endif
                return _Active.Value;
            }
            set
            {
                _Active = value;
            }
        }
#endregion

#region Methods
        public PerformanceEvent Start(string url, int id, bool checkCallStack = false)
        {
            if (!Active)
                return null;
            if (String.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException();
            PerformanceLoggerInstance performanceLoggerInstance = null;
            lock (_LogLock)
            {
                if (!_Log.TryGetValue(url, out performanceLoggerInstance))
                {
                    performanceLoggerInstance = new PerformanceLoggerInstance(url);
                    _Log.Add(url, performanceLoggerInstance);
                }
            }
            if (performanceLoggerInstance != null)
            {
                PerformanceEvent perfEvent = performanceLoggerInstance.Start(id, checkCallStack);
                if (perfEvent != null)
                {
                    lock (_LogLock)
                    {
                        _ActivePerfEvents.Add(perfEvent);
                    }
                }
                return perfEvent;
            }
            return null;
        }

        public bool Stop(string url, int id, PerformanceEvent perfEvent)
        {
            if (!Active)
                return false;
            if (perfEvent == null)
                throw new ArgumentNullException();
            if (String.IsNullOrWhiteSpace(url))
            {
                lock (_LogLock)
                {
                    if (perfEvent.IsRunning)
                        perfEvent.Stop();
                    _ActivePerfEvents.Remove(perfEvent);
                }
                return true;
            }

            PerformanceLoggerInstance performanceLoggerInstance = null;
            lock (_LogLock)
            {
                if (!_Log.TryGetValue(url, out performanceLoggerInstance))
                    return false; 
            }
            if (performanceLoggerInstance != null)
            {
                bool stopped = performanceLoggerInstance.Stop(id, perfEvent);
                _TotalExecutionTime += perfEvent.Elapsed;
                lock (_LogLock)
                {
                    if (perfEvent.IsRunning)
                        perfEvent.Stop();
                    _ActivePerfEvents.Remove(perfEvent);
                }
                return stopped;
            }
            return false;
        }

        public string DumpAndReset()
        {
            if (!Active)
                return null;
            Dictionary<string, PerformanceLoggerInstance> oldLog = null;
            TimeSpan totalExecutionTime;
            lock (_LogLock)
            {
                oldLog = _Log;
                _Log = new Dictionary<string, PerformanceLoggerInstance>();
                totalExecutionTime = TotalExecutionTime;
                _TotalExecutionTime = TimeSpan.Zero;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("========== START OF {0} ==========", this.LogName));
            sb.AppendLine("");
            sb.AppendLine("<<< SUMMARY >>>");
            sb.AppendLine("");

            if (oldLog.Any())
            {
                PerformanceLoggerInstance[] sortedByExecTime = oldLog.Select(c => c.Value).OrderByDescending(c => c.TotalExecutionTime).ToArray();
                foreach (PerformanceLoggerInstance instanceEntry in sortedByExecTime)
                {
                    double usagePercent = (instanceEntry.TotalExecutionTime.TotalMilliseconds / totalExecutionTime.TotalMilliseconds) * 100;
                    sb.AppendLine(String.Format("ExecTime: {2}, Usage: {1:000.000} %, Instance: {0}", instanceEntry.InstanceName, usagePercent, instanceEntry.TotalExecutionTime));
                }

                sb.AppendLine("");
                sb.AppendLine("");
                sb.AppendLine("<<< DETAILS >>>");
                sb.AppendLine("");
                foreach (PerformanceLoggerInstance instanceEntry in sortedByExecTime)
                {
                    instanceEntry.DumpAndReset(sb, this, totalExecutionTime);
                }
            }

            sb.AppendLine(String.Format("=========== END OF {0} ===========", this.LogName));
            return sb.ToString();
        }

#if NETFRAMEWORK
        public void MonitorActivePerfEvents(int perfTimeoutForStop, IRuntimeDump runtimeDump, string[] ignoreList, string[] includeList)
        {
            if (perfTimeoutForStop <= 0)
                return;
            IEnumerable<PerformanceEvent> activeEvents;
            lock (_LogLock)
            {
                activeEvents = _ActivePerfEvents.ToArray();
            }
            if (activeEvents != null && activeEvents.Any())
            {
                foreach (var perfEvent in activeEvents)
                {
                    perfEvent.CalculateTimeout(perfTimeoutForStop);
                }

                var queryTimeOut = activeEvents.Where(c => c.IsTimedOut
                                            && (c.InstanceName == null
                                                || (!ignoreList.Where(d => c.InstanceName.StartsWith(d)).Any()
                                                    && (!includeList.Any()
                                                        || includeList.Where(d => c.InstanceName.StartsWith(d)).Any()))
                                               )
                                      );
                if (queryTimeOut.Any())
                {
                    string timeOutText = DumpEvents(queryTimeOut.OrderByDescending(c => c.ElapsedMilliseconds).ToArray());
                    if (!String.IsNullOrEmpty(timeOutText))
                        runtimeDump.Messages.LogWarning(runtimeDump.GetACUrl(), "MonitorActivePerfEvents()", timeOutText);

//#if DEBUG
//                    if (System.Diagnostics.Debugger.IsAttached)
//                        System.Diagnostics.Debugger.Break();
//                    else
//                        runtimeDump.DumpStackTrace(Thread.CurrentThread);
//#else
                        runtimeDump.DumpStackTrace(Thread.CurrentThread, true);
//#endif
                    foreach (var perfEvent in activeEvents)
                    {
                        // If there are any Zombies, because a programer has not called Stop() in a finally-block to ensure that the Entry will be removed from watching,
                        // then remove it
                        if (perfEvent.RestartWithCounter() >= 6)
                        {
                            lock (_LogLock)
                            {
                                perfEvent.Stop();
                                _ActivePerfEvents.Remove(perfEvent);
                            }
                        }
                    }
                }
            }
        }

        public string DumpEvents(IEnumerable<PerformanceEvent> activeEvents)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var perfEvent in activeEvents)
            {
                sb.AppendLine(perfEvent.ToString());
            }
            return sb.ToString();
        }
#endif

#endregion

    }
}
