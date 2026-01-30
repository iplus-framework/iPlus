using Avalonia.Controls;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine.avui
{
    public class VBMessageBoxModel
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
