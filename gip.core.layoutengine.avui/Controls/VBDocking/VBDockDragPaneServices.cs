using Avalonia;
using System;
using System.Collections.Generic;
using System.Text;

namespace gip.core.layoutengine.avui
{
    class VBDockDragPanelServices
    {
        List<IVBDockDropSurface> Surfaces = new List<IVBDockDropSurface>();
        List<IVBDockDropSurface> SurfacesWithDragOver = new List<IVBDockDropSurface>();

        VBDockingManager _owner;

        public VBDockingManager DockManager
        {
            get { return _owner; }
        }

        public VBDockDragPanelServices(VBDockingManager owner)
        {
            _owner = owner;
        }

        public void Register(IVBDockDropSurface surface)
        {
            if (!Surfaces.Contains(surface))
                Surfaces.Add(surface);
        }

        public void Unregister(IVBDockDropSurface surface)
        {
            Surfaces.Remove(surface);
        }

        //public static void StartDrag(DockablePane pane, Point point)
        //{
        //    StartDrag(new FloatingWindow(_pane), point);
        //}

        Point Offset;

        public void StartDrag(VBWindowDockingUndocked wnd, Point point, Point offset)
        {
            _pane = wnd.HostedPane;
            Offset = offset;
            
            _wnd = wnd;

            if (Offset.X >= _wnd.Width)
                Offset = new Point(_wnd.Width / 2, Offset.Y);
            
            PixelPoint wndPosition = new PixelPoint((int)(point.X - Offset.X), (int)(point.Y - Offset.Y));
            _wnd.Position = wndPosition;
            _wnd.Show();

            foreach (IVBDockDropSurface surface in Surfaces)
            {
                if (surface.SurfaceRectangle.Contains(point))
                {
                    SurfacesWithDragOver.Add(surface);
                    surface.OnDockDragEnter(point);
                }
            }
        }

        public void MoveDrag(Point point)
        {
            if (_wnd == null)
                return;

            PixelPoint wndPosition = new PixelPoint((int)(point.X - Offset.X), (int)(point.Y - Offset.Y));
            _wnd.Position = wndPosition;

            List<IVBDockDropSurface> enteringSurfaces = new List<IVBDockDropSurface>();
            foreach (IVBDockDropSurface surface in Surfaces)
            {
                if (surface.SurfaceRectangle.Contains(point))
                {
                    if (!SurfacesWithDragOver.Contains(surface))
                        enteringSurfaces.Add(surface);
                    else
                        surface.OnDockDragOver(point);
                }
                else if (SurfacesWithDragOver.Contains(surface))
                {
                    SurfacesWithDragOver.Remove(surface);
                    surface.OnDockDragLeave(point);
                }
            }

            foreach (IVBDockDropSurface surface in enteringSurfaces)
            {
                SurfacesWithDragOver.Add(surface);
                surface.OnDockDragEnter(point);
            }
        }

        public void EndDrag(Point point)
        {
            IVBDockDropSurface dropSufrace = null;
            foreach (IVBDockDropSurface surface in Surfaces)
            {
                if (surface.SurfaceRectangle.Contains(point))
                {
                    if (surface.OnDockDrop(point))
                    {
                        dropSufrace = surface;
                        break;
                    }
                }
            }

            foreach (IVBDockDropSurface surface in SurfacesWithDragOver)
            {
                if (surface != dropSufrace)
                {
                    surface.OnDockDragLeave(point);
                }
            }

            SurfacesWithDragOver.Clear();
            
            if (dropSufrace != null)
                _wnd.Close();

            _wnd = null;
            _pane = null;
        }

        VBWindowDockingUndocked _wnd;
        public VBWindowDockingUndocked FloatingWindow
        {
            get { return _wnd; }
        }
        
        
        VBDockingPanelToolWindow _pane;
        public VBDockingPanelToolWindow DockablePane
        {
            get { return _pane; }
        }
    }
}
