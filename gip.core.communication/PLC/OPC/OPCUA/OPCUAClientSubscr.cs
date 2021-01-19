using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Ua.Client;

namespace gip.core.communication
{
    public class OPCUAClientSubscr : Subscription
    {
        public OPCUAClientSubscr() : base()
        {

        }

        public OPCUAClientSubscr(Subscription template) : base(template)
        {

        }

        public OPCUAClientSubscr(Subscription template, bool copyEventHandlers) : base(template, copyEventHandlers)
        {

        }
    }
}
