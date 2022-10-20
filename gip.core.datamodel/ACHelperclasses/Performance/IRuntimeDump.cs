using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace gip.core.datamodel
{
    public interface IRuntimeDump : IACComponent
    {
        PerformanceLogger PerfLogger {  get; }

        int PerfTimeoutStackTrace { get; }

        PerformanceEvent PerfLoggerStart(string url, int id, bool checkCallStack = false);

        bool? PerfLoggerStop(string url, int id, PerformanceEvent perfEvent, int perfTimeoutStackTrace = 0);

        void DumpStackTrace(Thread ignoreThread = null);
    }
}
