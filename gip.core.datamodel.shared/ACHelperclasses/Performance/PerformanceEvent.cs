using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public class PerformanceEvent : Stopwatch
    {
        public PerformanceEvent(bool inCallStack)
            : base()
        {
            _InCallStack = inCallStack;
        }
        private bool _InCallStack = false;
        public bool InCallStack
        {
            get
            {
                return _InCallStack;
            }
        }

        public readonly DateTime StartTime = DateTime.Now;
        public DateTime EndTime
        {
            get
            {
                return StartTime + Elapsed;
            }
        }
    }
}
