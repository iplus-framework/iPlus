using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.tcShared.WCF
{
    public enum ConnectionState : byte
    {
        Connect = 0,
        ReadyForEvents = 2,
        Disconnect = 3,
        DisconnectPLC = 4
    }
}
