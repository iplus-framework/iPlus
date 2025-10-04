using Avalonia.Controls;
using StatusBar.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui
{

    public class StatusBarItem : ContentControl
    {
        #region Constructors

        static StatusBarItem()
        {
            //IsTabStopProperty.OverrideMetadata(typeof(StatusBarItem), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
        }

        #endregion

    }

    /// <summary>
    /// Represents an item of a <see cref="VBStatusBar"/> control.
    /// </summary>
    /// <summary>
    /// Stellt ein Element eines <see cref="VBStatusBar"/> Controls dar.
    /// </summary>
    public class VBStatusBarItem : StatusBarItem
    {
        public VBStatusBarItem() : base()
        {
        }

    }
}
