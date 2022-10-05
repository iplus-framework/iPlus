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
                return performanceLoggerInstance.Start(id, checkCallStack);
            return null;
        }

        public bool Stop(string url, int id, PerformanceEvent perfEvent)
        {
            if (!Active)
                return false;
            if (String.IsNullOrWhiteSpace(url) || perfEvent == null)
                throw new ArgumentNullException();
            PerformanceLoggerInstance performanceLoggerInstance = null;
            lock (_LogLock)
            {
                if (!_Log.TryGetValue(url, out performanceLoggerInstance))
                    return false; 
            }
            if (performanceLoggerInstance != null)
            {
                bool added = performanceLoggerInstance.Stop(id, perfEvent);
                _TotalExecutionTime += perfEvent.Elapsed;
                return added;
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
#endregion

    }
}
