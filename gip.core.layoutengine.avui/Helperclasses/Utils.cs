using Avalonia;
using Avalonia.Input;
using System;

namespace gip.core.layoutengine.avui
{
    public static class VBUtils
    {
        /// <summary>
        /// Converts a coordinate from the polar coordinate system to the cartesian coordinate system.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static Point ComputeCartesianCoordinate(double angle, double radius)
        {
            // convert to radians
            double angleRad = (Math.PI / 180.0) * (angle - 90);

            double x = radius * Math.Cos(angleRad);
            double y = radius * Math.Sin(angleRad);

            return new Point(x, y);
        }

        public static Point VBOffset(this Point point, double X, double Y)
        {
            return new Point(point.X + X, point.Y + Y);
        }

        private static Nullable<bool> _HasTouchDev;
        public static bool HasTouchDevices
        {
            get
            {
                throw new NotImplementedException("Tablet.TabletDevices is not implemented in Avalonia");
                //if (_HasTouchDev.HasValue)
                //    return _HasTouchDev.Value;
                //foreach (TabletDevice tabletDevice in Tablet.TabletDevices)
                //{
                //    //Only detect if it is a touch Screen not how many touches (i.e. Single touch or Multi-touch)
                //    if (tabletDevice.Type == TabletDeviceType.Touch)
                //    {
                //        _HasTouchDev = true;
                //        return true;
                //    }
                //}
                //_HasTouchDev = false;
                //return false;
            }
        }
    }

    public class WaitCursor : IDisposable
    {
        private Cursor _previousCursor;

        public WaitCursor()
        {
            throw new NotImplementedException("Mouse.OverrideCursor is not implemented in Avalonia");
            //_previousCursor = Mouse.OverrideCursor;

            //Mouse.OverrideCursor = Cursors.Wait;
        }

        #region IDisposable Members

        public void Dispose()
        {
            //Mouse.OverrideCursor = _previousCursor;
        }

        #endregion
    }
}