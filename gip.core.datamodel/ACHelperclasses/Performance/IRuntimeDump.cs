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

        void DumpStackTrace(Thread ignoreThread = null);
    }
}
