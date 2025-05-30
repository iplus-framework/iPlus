using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    /// Data structure for performance logger data export
    /// </summary>
    public class PerformanceLoggerData
    {
        public string LogName { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public List<PerformanceLoggerInstanceData> Instances { get; set; } = new List<PerformanceLoggerInstanceData>();
    }

    /// <summary>
    /// Data structure for performance logger instance data export
    /// </summary>
    public class PerformanceLoggerInstanceData
    {
        public string InstanceName { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public List<PerformanceStatisticData> Statistics { get; set; } = new List<PerformanceStatisticData>();
    }

    /// <summary>
    /// Data structure for performance statistic data export
    /// </summary>
    public class PerformanceStatisticData
    {
        public int Id { get; set; }
        public TimeSpan TotalExecutionTime { get; set; }
        public List<PerformanceEventData> Events { get; set; } = new List<PerformanceEventData>();
    }

    /// <summary>
    /// Data structure for performance event data export
    /// </summary>
    public class PerformanceEventData
    {
        public DateTime StartTime { get; set; }
        public TimeSpan Elapsed { get; set; }
        public double UsagePercent { get; set; }
    }

}
