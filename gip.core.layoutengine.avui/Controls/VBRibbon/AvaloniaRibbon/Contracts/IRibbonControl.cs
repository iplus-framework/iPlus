using gip.core.layoutengine.avui.AvaloniaRibbon.Models;

namespace gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;

public interface IRibbonControl
{
    RibbonControlSize Size { get; set; }

    RibbonControlSize MinSize { get; set; }

    RibbonControlSize MaxSize { get; set; }
}