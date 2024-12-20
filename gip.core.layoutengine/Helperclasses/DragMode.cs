﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Mode of the WPF-Control to control the Drag-and-Drop behaviour.
    /// </summary>
    public enum DragMode
    {
        /// <summary>
        /// Drag-And-Drop is disabled
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Drag-And-Drop is enabled
        /// </summary>
        Enabled = 1,

        /// <summary>
        /// Drag-And-Drop is enabled only in combination with a pressed key
        /// </summary>
        EnabledMove = 2,
    }
}
