using Avalonia.Controls;

using gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;

using System;

namespace gip.core.layoutengine.avui.AvaloniaRibbon;

public class RibbonDropDownItem : MenuItem, IRibbonCommand
{
    #region Properties

    protected override Type StyleKeyOverride => typeof(RibbonDropDownItem);

    #endregion Properties
}