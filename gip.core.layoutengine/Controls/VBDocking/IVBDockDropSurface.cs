using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace gip.core.layoutengine
{
    interface IVBDockDropSurface
    {
        Rect SurfaceRectangle { get;}
        void OnDockDragEnter(Point point);
        void OnDockDragOver(Point point);
        void OnDockDragLeave(Point point);
        bool OnDockDrop(Point point);
    }
}
