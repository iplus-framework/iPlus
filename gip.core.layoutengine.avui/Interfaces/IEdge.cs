using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System.Collections.Generic;

namespace gip.core.layoutengine.avui
{
    public interface IEdge
    {
        Point GetSourceConnectorPointToContainer(Visual container);
        Point GetTargetConnectorPointToContainer(Visual container);
        Control SourceElement { get;}
        Control TargetElement { get;}
        IList<Point> Points { get; set; }
        void RedrawVBEdge(bool isFromNode, bool withCalcConnPos = false);
    }
}
