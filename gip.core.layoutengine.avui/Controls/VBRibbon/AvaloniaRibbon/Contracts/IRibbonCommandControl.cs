using System.Windows.Input;

namespace gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;

public interface IRibbonCommand
{
    public ICommand Command { get; set; }

    public object CommandParameter { get; set; }
}