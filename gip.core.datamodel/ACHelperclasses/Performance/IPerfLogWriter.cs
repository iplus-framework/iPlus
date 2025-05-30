using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace gip.core.datamodel
{
    public interface IPerfLogWriter : IACComponent
    {
        void WritePerformanceData(DateTime stamp, PerformanceLoggerData perfData, PerformanceLogger perfLogger);
    }
}
