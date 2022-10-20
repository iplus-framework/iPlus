using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;

namespace gip.core.communication
{
    public class S7TCPPollingPlanEntry
    {
        #region c'tors
        public S7TCPPollingPlanEntry(S7TCPSubscr subscr, int dbNo)
        {
            Subscr = subscr;
            DBNo = dbNo;
        }
        #endregion

        #region Properties
        public S7TCPSubscr Subscr
        {
            get;
            set;
        }

        public int DBNo
        {
            get;
            set;
        }
        #endregion
    }
}
