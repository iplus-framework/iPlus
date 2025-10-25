namespace gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;

public interface IRibbon : IKeyTipHandler
{
    public bool IsCollapsedPopupOpen { get; set; }

    public void Close();
}