using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.ControlScriptSync
{
    /// <summary>
    /// Simple sync message
    /// </summary>
    public class SyncMessage
    {
        public MessageLevel MessageLevel { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
    }
}
