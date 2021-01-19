// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using gip.core.datamodel;
using gip.ext.design;
using gip.ext.design.Adorners;
using gip.ext.design.Extensions;
using gip.ext.designer.Extensions;
using gip.ext.designer.Services;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a adorner for <see cref="VBConnector"/>
    /// </summary>
    /// <summary xml:lang="de">
    /// Repräsentiert einen Adorner für <see cref="VBConnector"/>
    /// </summary>
    public class VBConnectorAdorner : Control
    {
        static VBConnectorAdorner()
		{
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBConnectorAdorner), new FrameworkPropertyMetadata(typeof(VBConnectorAdorner)));
		}

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
