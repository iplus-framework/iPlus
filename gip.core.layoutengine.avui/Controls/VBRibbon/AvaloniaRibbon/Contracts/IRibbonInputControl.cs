using Avalonia.Controls.Templates;

namespace gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;

public interface IRibbonInputControl : IRibbonControl
{
    public object Content { get; set; }

    public object Icon { get; set; }

    public object LargeIcon { get; set; }
}