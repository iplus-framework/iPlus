// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Linq;
using System.Collections.Generic;
using gip.core.datamodel;
using gip.ext.design.avui;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Extensions;
using gip.ext.designer.avui.Services;
using Avalonia.Controls;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a adorner for <see cref="VBConnector"/>
    /// </summary>
    /// <summary xml:lang="de">
    /// Repräsentiert einen Adorner für <see cref="VBConnector"/>
    /// </summary>
    public class VBConnectorAdorner : Control
    {
        /// <summary>
        /// The VBVisualConnector item.
        /// </summary>
        public readonly DesignItem _vbVisualConnectorItem;

        /// <summary>
        /// Creates a new instance of VBConnectorAdorner.
        /// </summary>
        /// <param name="vbVisualConnectorItem">The vb visual connector item.</param>
        public VBConnectorAdorner(DesignItem vbVisualConnectorItem)
        {
            this._vbVisualConnectorItem = vbVisualConnectorItem;
        }


    }

}
