﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.tcShared
{
    public class MonitorObject
    {
        readonly public int LockLevel;
        public MonitorObject(int lockLevel)
        {
            LockLevel = lockLevel;
        }

        public override string ToString()
        {
            string text = base.ToString();
            return String.Format("{0}, Locklevel: {1}", text, LockLevel);
        }
    }
}