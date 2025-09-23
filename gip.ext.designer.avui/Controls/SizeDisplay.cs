// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace gip.ext.designer.avui.Controls
{
    /// <summary>
    /// Display height of the element.
    /// </summary>
    public class HeightDisplay : TemplatedControl
    {
        static HeightDisplay()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(HeightDisplay), new FrameworkPropertyMetadata(typeof(HeightDisplay)));
        }
    }

    /// <summary>
    /// Display width of the element.
    /// </summary>
    public class WidthDisplay : TemplatedControl
    {
        static WidthDisplay()
        {
            //DefaultStyleKeyProperty.OverrideMetadata(typeof(WidthDisplay), new FrameworkPropertyMetadata(typeof(WidthDisplay)));
        }
    }
}
