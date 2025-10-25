using Avalonia.Controls.Templates;

namespace gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;

public interface ICanAddToQuickAccess
{
    IControlTemplate QuickAccessTemplate { get; set; }

    bool CanAddToQuickAccess { get; set; }

    public object QuickAccessIcon { get; set; }
}