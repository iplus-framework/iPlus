using Avalonia.Controls.Primitives;

using System;

namespace gip.core.layoutengine.avui.AvaloniaRibbon;

public class RibbonDropDownSeparator : TemplatedControl
{
    public RibbonDropDownSeparator()
    {
    }

    protected override Type StyleKeyOverride => typeof(RibbonDropDownSeparator);
}