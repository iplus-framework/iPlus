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

        public PerformanceEvent(bool inCallStack, PerformanceStatistic statistic)
            : base()
        {
            _InCallStack = inCallStack;
            _Statistic = statistic;
        }

        private bool _InCallStack = false;
        public bool InCallStack
        {
            get
            {
                return _InCallStack;
            }
        }

        private PerformanceStatistic _Statistic;
        public PerformanceStatistic Statistic
        {
            get
            {
                return _Statistic;
            }
        }

        public readonly DateTime StartTime = DateTimeUtils.NowDST;
        public DateTime EndTime
        {
            get
            {
                return StartTime + Elapsed;
            }
        }

        private UInt16 _TimedOut = 0;
        public bool IsTimedOut
        {
            get
            {
                return (_TimedOut != 0) && (_TimedOut % 2 != 0);
            }
        }

        public string InstanceName
        {
            get
            {
                if (Statistic == null || Statistic.Instance == null)
                    return null;
                return Statistic.Instance.InstanceName;
            }
        }

        public bool CalculateTimeout(int perfTimeoutMs)
        {
            if (perfTimeoutMs > 0)
            {
                if (ElapsedMilliseconds > perfTimeoutMs)
                    _TimedOut++;
            }
            return IsTimedOut;
        }

        public UInt16 RestartWithCounter()
        {
            base.Restart();
            if (_TimedOut % 2 != 0)
                _TimedOut++;
            return _TimedOut;
        }

        public override string ToString()
        {
            string instanceName = InstanceName;
            if (String.IsNullOrEmpty(instanceName))
                instanceName = "null";
            return String.Format("Name: {0}, Elapsed: {1}, TimedOut: {2}", instanceName, ElapsedMilliseconds, IsTimedOut);
            //return base.ToString();
        }
    }
}
