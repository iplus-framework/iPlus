using Avalonia.Controls;
using System.Collections.Generic;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a dockable button group.
    /// </summary>
    public class VBDockingButtonGroup
    {
        public readonly List<VBDockingButton> Buttons = new List<VBDockingButton>();
        public Dock Dock; 
    }
}
