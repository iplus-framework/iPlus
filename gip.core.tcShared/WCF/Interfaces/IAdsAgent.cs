using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.tcShared.ACVariobatch;

namespace gip.core.tcShared
{
    public interface IAdsAgent
    {
        void StartReadEvent();

        byte[] ReadMemory();

        void SendParameters(byte[] parameters);

        void ReadResult(byte[] info);

        void SendValueToPLC(VBEvent vbEvent);
    }
}
