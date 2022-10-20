using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO.Ports;
using gip.core.autocomponent;
using gip.core.datamodel;

namespace gip.core.processapplication
{
    public interface IACPAESerialPort : IACComponent
    {
        SerialPort SerialPort { get; }

        string PortName { get; set; }

        int BaudRate { get; set; }

        Parity Parity { get; set; }

        int DataBits { get; set; }

        StopBits StopBits { get; set; }

        int ReadTimeout { get; set; }

        int WriteTimeout { get; set; }

        bool RtsEnable { get; set; }

        Handshake Handshake { get; set; }
    }
}
