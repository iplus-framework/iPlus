using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public interface IACCommSession : IACComponent
    {
        IACContainerTNet<bool> IsConnected { get; set; }

        bool InitSession();
        bool IsEnabledInitSession();

        bool DeInitSession();
        bool IsEnabledDeInitSession();

        bool Connect();
        bool IsEnabledConnect();

        bool DisConnect();
        bool IsEnabledDisConnect();
    }
}
