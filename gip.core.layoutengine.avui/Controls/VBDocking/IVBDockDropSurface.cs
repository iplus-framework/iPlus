using Avalonia;
using System.Text;

namespace gip.core.layoutengine.avui
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
