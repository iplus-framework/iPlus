using Avalonia.Input;

namespace gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;

public interface IKeyTipHandler
{
    void ActivateKeyTips(IRibbon ribbon, IKeyTipHandler prev);

    bool HandleKeyTipKeyPress(Key key);
}