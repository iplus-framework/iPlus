using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface IMsgObserver
    {
        void SendMessage(Msg msg);
    }
}
