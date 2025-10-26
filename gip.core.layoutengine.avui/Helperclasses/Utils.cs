using Avalonia;
using Avalonia.Input;
using gip.ext.design.avui.UIExtensions;
using System;
using System.Runtime.InteropServices;

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

        //private static Nullable<bool> _HasTouchDev;
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
        //private Cursor _previousCursor;

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


        public static class DoubleUtil
        {
            // Const values come from sdk\inc\crt\float.h
            internal const double DBL_EPSILON = 2.2204460492503131e-016; /* smallest such that 1.0+DBL_EPSILON != 1.0 */
            internal const float FLT_MIN = 1.175494351e-38F; /* Number close to zero, where float.MinValue is -float.MaxValue */

            /// 

            /// AreClose - Returns whether or not two doubles are "close".  That is, whether or 
            /// not they are within epsilon of each other.  Note that this epsilon is proportional
            /// to the numbers themselves to that AreClose survives scalar multiplication. 
            /// There are plenty of ways for this to return false even for numbers which
            /// are theoretically identical, so no code calling this should fail to work if this
            /// returns false.  This is important enough to repeat:
            /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be 
            /// used for optimizations *only*.
            /// 

            ///  
            /// bool - the result of the AreClose comparision.
            ///  
            ///  The first double to compare. 
            ///  The second double to compare. 
            public static bool AreClose(double value1, double value2)
            {
                //in case they are Infinities (then epsilon check does not work)
                if (value1 == value2) return true;
                // This computes (|value1-value2| / (|value1| + |value2| + 10.0)) < DBL_EPSILON 
                double eps = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * DBL_EPSILON;
                double delta = value1 - value2;
                return (-eps < delta) && (eps > delta);
            }

            /// 

            /// LessThan - Returns whether or not the first double is less than the second double.
            /// That is, whether or not the first is strictly less than *and* not within epsilon of 
            /// the other number.  Note that this epsilon is proportional to the numbers themselves 
            /// to that AreClose survives scalar multiplication.  Note,
            /// There are plenty of ways for this to return false even for numbers which 
            /// are theoretically identical, so no code calling this should fail to work if this
            /// returns false.  This is important enough to repeat:
            /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be
            /// used for optimizations *only*. 
            /// 

            ///  
            /// bool - the result of the LessThan comparision. 
            /// 
            ///  The first double to compare.  
            ///  The second double to compare. 
            public static bool LessThan(double value1, double value2)
            {
                return (value1 < value2) && !AreClose(value1, value2);
            }


            /// 

            /// GreaterThan - Returns whether or not the first double is greater than the second double. 
            /// That is, whether or not the first is strictly greater than *and* not within epsilon of
            /// the other number.  Note that this epsilon is proportional to the numbers themselves
            /// to that AreClose survives scalar multiplication.  Note,
            /// There are plenty of ways for this to return false even for numbers which 
            /// are theoretically identical, so no code calling this should fail to work if this
            /// returns false.  This is important enough to repeat: 
            /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be 
            /// used for optimizations *only*.
            /// 

            /// 
            /// bool - the result of the GreaterThan comparision.
            /// 
            ///  The first double to compare.  
            ///  The second double to compare. 
            public static bool GreaterThan(double value1, double value2)
            {
                return (value1 > value2) && !AreClose(value1, value2);
            }

            /// 

            /// LessThanOrClose - Returns whether or not the first double is less than or close to
            /// the second double.  That is, whether or not the first is strictly less than or within 
            /// epsilon of the other number.  Note that this epsilon is proportional to the numbers
            /// themselves to that AreClose survives scalar multiplication.  Note, 
            /// There are plenty of ways for this to return false even for numbers which 
            /// are theoretically identical, so no code calling this should fail to work if this
            /// returns false.  This is important enough to repeat: 
            /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be
            /// used for optimizations *only*.
            /// 

            ///  
            /// bool - the result of the LessThanOrClose comparision.
            ///  
            ///  The first double to compare.  
            ///  The second double to compare. 
            public static bool LessThanOrClose(double value1, double value2)
            {
                return (value1 < value2) || AreClose(value1, value2);
            }

            /// 

            /// GreaterThanOrClose - Returns whether or not the first double is greater than or close to 
            /// the second double.  That is, whether or not the first is strictly greater than or within 
            /// epsilon of the other number.  Note that this epsilon is proportional to the numbers
            /// themselves to that AreClose survives scalar multiplication.  Note, 
            /// There are plenty of ways for this to return false even for numbers which
            /// are theoretically identical, so no code calling this should fail to work if this
            /// returns false.  This is important enough to repeat:
            /// NB: NO CODE CALLING THIS FUNCTION SHOULD DEPEND ON ACCURATE RESULTS - this should be 
            /// used for optimizations *only*.
            /// 

            ///  
            /// bool - the result of the GreaterThanOrClose comparision.
            ///  
            ///  The first double to compare. 
            ///  The second double to compare. 
            public static bool GreaterThanOrClose(double value1, double value2)
            {
                return (value1 > value2) || AreClose(value1, value2);
            }

            /// 

            /// IsOne - Returns whether or not the double is "close" to 1.  Same as AreClose(double, 1), 
            /// but this is faster.
            /// 

            /// 
            /// bool - the result of the AreClose comparision. 
            /// 
            ///  The double to compare to 1.  
            public static bool IsOne(double value)
            {
                return Math.Abs(value - 1.0) < 10.0 * DBL_EPSILON;
            }

            /// 

            /// IsZero - Returns whether or not the double is "close" to 0.  Same as AreClose(double, 0), 
            /// but this is faster.
            /// 

            ///  
            /// bool - the result of the AreClose comparision.
            ///  
            ///  The double to compare to 0. 
            public static bool IsZero(double value)
            {
                return Math.Abs(value) < 10.0 * DBL_EPSILON;
            }

            // The Point, Size, Rect and Matrix class have moved to WinCorLib.  However, we provide 
            // internal AreClose methods for our own use here.

            /// 

            /// Compares two points for fuzzy equality.  This function
            /// helps compensate for the fact that double values can
            /// acquire error when operated upon 
            /// 

            /// The first point to compare 
            /// The second point to compare 
            /// Whether or not the two points are equal
            public static bool AreClose(Point point1, Point point2)
            {
                return DoubleUtil.AreClose(point1.X, point2.X) &&
                DoubleUtil.AreClose(point1.Y, point2.Y);
            }

            /// 

            /// Compares two Size instances for fuzzy equality.  This function 
            /// helps compensate for the fact that double values can
            /// acquire error when operated upon 
            /// 

            /// The first size to compare
            /// The second size to compare
            /// Whether or not the two Size instances are equal 
            public static bool AreClose(Size size1, Size size2)
            {
                return DoubleUtil.AreClose(size1.Width, size2.Width) &&
                       DoubleUtil.AreClose(size1.Height, size2.Height);
            }

            /// 

            /// Compares two Vector instances for fuzzy equality.  This function
            /// helps compensate for the fact that double values can 
            /// acquire error when operated upon
            /// 

            /// The first Vector to compare 
            /// The second Vector to compare
            /// Whether or not the two Vector instances are equal 
            public static bool AreClose(Vector vector1, Vector vector2)
            {
                return DoubleUtil.AreClose(vector1.X, vector2.X) &&
                       DoubleUtil.AreClose(vector1.Y, vector2.Y);
            }

            /// 

            /// Compares two rectangles for fuzzy equality.  This function
            /// helps compensate for the fact that double values can 
            /// acquire error when operated upon
            /// 

            /// The first rectangle to compare
            /// The second rectangle to compare 
            /// Whether or not the two rectangles are equal
            public static bool AreClose(Rect rect1, Rect rect2)
            {
                // If they're both empty, don't bother with the double logic.
                if (rect1.IsEmpty())
                {
                    return rect2.IsEmpty();
                }

                // At this point, rect1 isn't empty, so the first thing we can test is
                // rect2.IsEmpty, followed by property-wise compares. 

                return (!rect2.IsEmpty()) &&
                    DoubleUtil.AreClose(rect1.X, rect2.X) &&
                    DoubleUtil.AreClose(rect1.Y, rect2.Y) &&
                    DoubleUtil.AreClose(rect1.Height, rect2.Height) &&
                    DoubleUtil.AreClose(rect1.Width, rect2.Width);
            }

            /// 

            /// 
            /// 

            ///  
            /// 
            public static bool IsBetweenZeroAndOne(double val)
            {
                return (GreaterThanOrClose(val, 0) && LessThanOrClose(val, 1));
            }

            /// 

            ///
            /// 

            /// 
            /// 
            public static int DoubleToInt(double val)
            {
                return (0 < val) ? (int)(val + 0.5) : (int)(val - 0.5);
            }


            /// 

            /// rectHasNaN - this returns true if this rect has X, Y , Height or Width as NaN.
            /// 

            /// The rectangle to test
            /// returns whether the Rect has NaN 
            public static bool RectHasNaN(Rect r)
            {
                if (DoubleUtil.IsNaN(r.X)
                     || DoubleUtil.IsNaN(r.Y)
                     || DoubleUtil.IsNaN(r.Height)
                     || DoubleUtil.IsNaN(r.Width))
                {
                    return true;
                }
                return false;
            }


#if !PBTCOMPILER

            [StructLayout(LayoutKind.Explicit)]
            private struct NanUnion
            {
                [FieldOffset(0)] internal double DoubleValue;
                [FieldOffset(0)] internal UInt64 UintValue;
            }

            // The standard CLR double.IsNaN() function is approximately 100 times slower than our own wrapper, 
            // so please make sure to use DoubleUtil.IsNaN() in performance sensitive code.
            // PS item that tracks the CLR improvement is DevDiv Schedule : 26916.
            // IEEE 754 : If the argument is any value in the range 0x7ff0000000000001L through 0x7fffffffffffffffL
            // or in the range 0xfff0000000000001L through 0xffffffffffffffffL, the result will be NaN. 
            public static bool IsNaN(double value)
            {
                NanUnion t = new NanUnion();
                t.DoubleValue = value;

                UInt64 exp = t.UintValue & 0xfff0000000000000;
                UInt64 man = t.UintValue & 0x000fffffffffffff;

                return (exp == 0x7ff0000000000000 || exp == 0xfff0000000000000) && (man != 0);
            }
#endif
        }

    // File provided for Reference Use Only by Microsoft Corporation (c) 2007.
    // Copyright (c) Microsoft Corporation. All rights reserved.
}