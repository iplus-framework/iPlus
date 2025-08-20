using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace gip.core.layoutengine.avui.Controls.VBDocking.VBWindow
{


    /// <summary>
    /// View model for presenting MsgWithDetails object into grid
    /// Possible to add additional presentation membmers
    /// </summary>
    public class VBWindowDialogMsgViewModel
    {
        /// <summary>
        /// Content image - represent one message status by icon in grid
        /// </summary>
        public ContentControl ImageContent { get; set; }

        /// <summary>
        /// MsgWithDetails object - from backend
        /// </summary>
        public Msg Message { get; set; }
    }
}
