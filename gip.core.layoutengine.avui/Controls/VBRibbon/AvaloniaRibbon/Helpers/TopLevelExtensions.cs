using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.VisualTree;

namespace gip.core.layoutengine.avui.AvaloniaRibbon.Helpers;

public static class TopLevelExtensions
{
    public static Canvas GetCanvasFromUsableArea(this TopLevel topLevel)
    {
        if (topLevel == null) return null;
        var descendants = topLevel.GetVisualDescendants();
        var titleBar = descendants.FirstOrDefault(x => x is TitleBar);


        return null;
    }
}