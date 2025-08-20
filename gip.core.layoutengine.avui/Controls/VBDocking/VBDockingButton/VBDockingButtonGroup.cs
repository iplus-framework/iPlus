using System;
using System.Collections.Generic;
using System.Text;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a dockable button group.
    /// </summary>
    public class VBDockingButtonGroup
    {
        public readonly List<VBDockingButton> Buttons = new List<VBDockingButton>();
        public System.Windows.Controls.Dock Dock; 
    }
}
