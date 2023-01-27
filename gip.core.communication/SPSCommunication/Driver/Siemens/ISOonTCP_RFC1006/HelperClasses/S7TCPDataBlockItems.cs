using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;
using gip.core.autocomponent;
using gip.core.communication.ISOonTCP;

namespace gip.core.communication
{
    public class S7TCPDataBlockItems : List<S7TCPItem>
    {
        public void DeInit()
        {
            foreach (S7TCPItem s7Item in this)
            {
                s7Item.DeInit();
            }
        }

        public int MaxLength
        {
            get
            {
                if (this.Count <= 0)
                    return 0;
                return this.Max(c => c.ItemLength);
            }
        }

        public void Reconnected()
        {
            foreach (S7TCPItem s7Item in this)
            {
                s7Item.Reconnected();
            }
        }
    }
}
