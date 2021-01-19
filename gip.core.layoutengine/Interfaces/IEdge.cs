using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace gip.core.layoutengine
{
    public interface IEdge
    {
        Point GetSourceConnectorPointToContainer(Visual container);
        Point GetTargetConnectorPointToContainer(Visual container);
        FrameworkElement SourceElement { get;}
        FrameworkElement TargetElement { get;}
        PointCollection Points { get; set; }
        void RedrawVBEdge(bool isFromNode, bool withCalcConnPos = false);
        GeneralTransform TransformToVisual(Visual visual);
    }
}
