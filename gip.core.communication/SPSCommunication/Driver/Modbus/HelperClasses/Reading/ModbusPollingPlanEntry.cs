using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using gip.core.datamodel;

namespace gip.core.communication
{
    public class ModbusPollingPlanEntry
    {
        #region c'tors
        public ModbusPollingPlanEntry(ModbusSubscr subscr, int dbNo)
        {
            Subscr = subscr;
            DBNo = dbNo;
        }
        #endregion

        #region Properties
        public ModbusSubscr Subscr
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
