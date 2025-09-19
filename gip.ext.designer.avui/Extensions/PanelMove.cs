// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using gip.ext.design.avui.Adorners;
using gip.ext.design.avui.Extensions;
using gip.ext.designer.avui.Controls;

namespace gip.ext.designer.avui.Extensions
{
    [ExtensionFor(typeof(Panel))]
    [ExtensionFor(typeof(Border))]
    [ExtensionFor(typeof(ContentControl))]
    [ExtensionFor(typeof(Viewbox))]
    public class PanelMove : PermanentAdornerProvider
    {
        protected override void OnInitialized()
        {
            base.OnInitialized();

            var adornerPanel = new AdornerPanel();
            var adorner = new PanelMoveAdorner(ExtendedItem);
            AdornerPanel.SetPlacement(adorner, AdornerPlacement.FillContent);
            adornerPanel.Children.Add(adorner);
            Adorners.Add(adornerPanel);
        }
    }
}
