// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;
using System.Windows.Input;

using gip.ext.design.Adorners;

namespace gip.ext.design
{
    // Interfaces for mouse interaction on the design surface.

    /// <summary>
    /// Behavior interface implemented by elements to handle the mouse down event
    /// on them.
    /// </summary>
    public interface IHandlePointerToolMouseDown
    {
        /// <summary>
        /// Called to handle the mouse down event.
        /// </summary>
        void HandleSelectionMouseDown(IDesignPanel designPanel, MouseButtonEventArgs e, DesignPanelHitTestResult result);
    }

    /// <summary>
    /// Behavior interface implemented by elements to handle the mouse down event of Draw-Tools
    /// on them.
    /// iplus Extension
    /// </summary>
    public interface IHandleDrawToolMouseDown
    {
        /// <summary>
        /// Called to handle the mouse down event.
        /// </summary>
        void HandleStartDrawingOnMouseDown(IDesignPanel designPanel, MouseButtonEventArgs e, DesignPanelHitTestResult result, IDrawingTool tool);
    }
}
