// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace gip.core.datamodel
{
    public class PerformanceLoggerInstance
    {
        #region c'tors
        public PerformanceLoggerInstance(string acUrl)
        {
            _PerformanceLog = new Dictionary<int, PerformanceStatistic>();
            _InstanceName = acUrl;
        }
        #endregion


        #region Properties
        private string _InstanceName;
        public string InstanceName
        {
            get
            {
                return _InstanceName;
            }
        }


        private TimeSpan _TotalExecutionTime;
        public TimeSpan TotalExecutionTime
        {
            get
            {
                return _TotalExecutionTime;
            }
        }


        Dictionary<int, PerformanceStatistic> _PerformanceLog;
        object _StatLock = new object();
        #endregion


        #region Methods
        internal PerformanceEvent Start(int id, bool checkCallStack = false)
        {
            PerformanceStatistic performanceStatistic = null;
            lock (_StatLock)
            {
                if (!_PerformanceLog.TryGetValue(id, out performanceStatistic))
                {
                    performanceStatistic = new PerformanceStatistic(id, this);
                    _PerformanceLog.Add(id, performanceStatistic);
                }
            }
            if (performanceStatistic == null)
                return null;
            return performanceStatistic.Start(checkCallStack);
        }

        internal bool Stop(int id, PerformanceEvent perfEvent)
        {
            PerformanceStatistic performanceStatistic = null;
            lock (_StatLock)
            {
                if (!_PerformanceLog.TryGetValue(id, out performanceStatistic))
                {
                    return false;
                }
            }
            if (performanceStatistic == null)
                return false;
            bool added = performanceStatistic.Stop(perfEvent);
            _TotalExecutionTime += perfEvent.Elapsed;
            return added;
        }

        /// <summary>
        /// Gets performance data without resetting the internal structures
        /// </summary>
        /// <returns>Performance data for Excel export</returns>
        public PerformanceLoggerInstanceData GetPerformanceData()
        {
            lock (_StatLock)
            {
                var data = new PerformanceLoggerInstanceData
                {
                    InstanceName = this.InstanceName,
                    TotalExecutionTime = this.TotalExecutionTime,
                    Statistics = new List<PerformanceStatisticData>()
                };

                foreach (var statEntry in _PerformanceLog.OrderByDescending(c => c.Value.TotalExecutionTime))
                {
                    var statData = statEntry.Value.GetPerformanceData();
                    if (statData != null)
                        data.Statistics.Add(statData);
                }

                return data;
            }
        }

        public void DumpAndReset(StringBuilder sb, PerformanceLogger parent, TimeSpan totalExecutionTimeParent)
        {
            lock (_StatLock)
            {
                sb.AppendLine("");
                sb.AppendLine("----------------------------------");
                sb.AppendLine(String.Format("Instance: {0}, ExecTime: {1}", _InstanceName, TotalExecutionTime));
                if (!_PerformanceLog.Any())
                    return;
                foreach (var entry in _PerformanceLog.Select(c => c.Value).OrderByDescending(c => c.TotalExecutionTime))
                {
                    entry.DumpAndReset(sb, this);
                }
            }
        }
        #endregion
    }
}
