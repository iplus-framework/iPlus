using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.reporthandler
{
    public enum PrintJobStateEnum : short
    {
        New = 0,
        InProcess = 1,
        InAlarm = 2,
        Done = 3
    }
}
