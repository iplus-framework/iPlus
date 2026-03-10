using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Media;

namespace gip.ext.design.avui
{
    // Avalonia UI equivalents for WPF hit test functionality
    public delegate HitTestFilterBehavior HitTestFilterCallback(Visual potentialHitTestTarget);
    public delegate HitTestResultBehavior HitTestResultCallback(HitTestResult result);

    public enum HitTestFilterBehavior
    {
        Continue,
        ContinueSkipChildren,
        ContinueSkipSelf,
        ContinueSkipSelfAndChildren,
        Stop
    }

    public enum HitTestResultBehavior
    {
        Continue,
        Stop
    }

    public enum IntersectionDetail
    {
        NotCalculated,
        Empty,
        FullyInside,
        FullyContains,
        Intersects
    }

    public class HitTestResult
    {
        public HitTestResult()
        {
        }

        public HitTestResult(Visual visualHit)
        {
            VisualHit = visualHit;
        }
        public Visual VisualHit { get; set; }
    }

    public class GeometryHitTestResult : HitTestResult
    {
        public GeometryHitTestResult() : base()
        {
            IntersectionDetail = IntersectionDetail.NotCalculated;
        }

        public GeometryHitTestResult(Visual visualHit, IntersectionDetail intersectionDetail) : base(visualHit)
        {
            IntersectionDetail = intersectionDetail;
        }

        public IntersectionDetail IntersectionDetail { get; set; }
    }

    public class GeometryHitTestParameters
    {
        public Geometry HitGeometry { get; set; }

        public GeometryHitTestParameters(Geometry geometry)
        {
            HitGeometry = geometry;
        }
    }

    public abstract class HitTestParameters
    {
        // Prevent 3rd parties from extending this abstract base class.
        internal HitTestParameters() { }
    }

    public class PointHitTestParameters : HitTestParameters
    {
        /// <summary>
        /// The constructor takes the point to hit test with.
        /// </summary>
        public PointHitTestParameters(Point point) : base()
        {
            _hitPoint = point;
        }

        /// <summary>
        /// The point to hit test against.
        /// </summary>
        public Point HitPoint
        {
            get
            {
                return _hitPoint;
            }
        }

        internal void SetHitPoint(Point hitPoint)
        {
            _hitPoint = hitPoint;
        }

        private Point _hitPoint;
    }

    public class PointHitTestResult : HitTestResult
    {
        private Point _pointHit;

        /// <summary>
        /// This constructor takes a visual and point respresenting a hit.
        /// </summary>
        public PointHitTestResult(Visual visualHit, Point pointHit) : base(visualHit)
        {
            _pointHit = pointHit;
        }

        /// <summary>
        /// The point in local space of the hit visual.
        /// </summary>
        public Point PointHit
        {
            get
            {
                return _pointHit;
            }
        }
    }


    /// <summary>
    /// Helperclass for compatibility with WPF's VisualTreeHelper
    /// </summary>
    public static class VisualTreeHelper
    {
        public static AvaloniaObject GetParent(AvaloniaObject reference)
        {
            Visual visual = reference as Visual;
            if (visual == null)
                return null;
            return visual.GetVisualParent();
        }

        // Helper method
        public static bool IsDescendantOf(this Visual element, Visual ancestor, bool visualTree = false)
        {
            var current = element;
            while (current != null)
            {
                if (current == ancestor)
                    return true;
                if (visualTree)
                    current = current.GetVisualParent();
                else
                    current = current.Parent as Visual;
            }
            return false;
        }

        public static bool IsAncestorOf(this Visual ancestor, Visual element, bool visualTree = false)
        {
            return element.IsDescendantOf(ancestor, visualTree);
        }

        public static Transform TransformToAncestor(this Visual element, Visual ancestor)
        {
            ArgumentNullException.ThrowIfNull(ancestor);
            ArgumentNullException.ThrowIfNull(element);

            // Use Avalonia's native TransformToVisual which correctly accumulates
            // both layout offsets (Bounds.X/Y) and RenderTransforms at each level.
            Matrix? matrix = element.TransformToVisual(ancestor);
            if (matrix.HasValue)
                return new MatrixTransform(matrix.Value);

            throw new InvalidOperationException("Element is not a visual descendant of the specified ancestor.");
        }


        public static void HitTest(Visual reference, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, PointHitTestParameters hitTestParameters)
        {
            if (reference == null || hitTestParameters == null)
                return;

            if (resultCallback == null)
                throw new ArgumentNullException(nameof(resultCallback));

            HitTestVisual(reference, filterCallback, resultCallback, hitTestParameters);
        }

        private static bool HitTestVisual(Visual reference, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, PointHitTestParameters hitTestParameters)
        {
            if (reference == null)
                return true; // Continue

            // Apply filter callback
            var filterResult = filterCallback?.Invoke(reference) ?? HitTestFilterBehavior.Continue;

            if (filterResult == HitTestFilterBehavior.Stop)
                return false; // Stop

            if (filterResult == HitTestFilterBehavior.ContinueSkipSelfAndChildren)
                return true; // Continue

            // Hit test children first (reverse order for top-most hit) if not skipping children
            if (filterResult != HitTestFilterBehavior.ContinueSkipChildren && 
                filterResult != HitTestFilterBehavior.ContinueSkipSelfAndChildren)
            {
                var children = reference.GetVisualChildren().OfType<Visual>().ToList();
                for (int i = children.Count - 1; i >= 0; i--)
                {
                    var child = children[i];
                    if (child != null)
                    {
                        // Transform the hit point to child's coordinate system
                        var childHitPoint = hitTestParameters.HitPoint;
                        
                        // Apply child's offset if it's a control
                        if (child is Control childControl)
                        {
                            childHitPoint = new Point(
                                hitTestParameters.HitPoint.X - childControl.Bounds.X,
                                hitTestParameters.HitPoint.Y - childControl.Bounds.Y);
                        }

                        // Create new hit test parameters for the child
                        var childParams = new PointHitTestParameters(childHitPoint);
                        
                        // If child hit test returns false (Stop), stop processing immediately
                        if (!HitTestVisual(child, filterCallback, resultCallback, childParams))
                            return false; // Stop - child found a hit
                    }
                }
            }

            // Hit test this visual if not skipping self
            if (filterResult != HitTestFilterBehavior.ContinueSkipSelf && 
                filterResult != HitTestFilterBehavior.ContinueSkipSelfAndChildren)
            {
                // Check if the point is within this visual's bounds
                bool isHit = false;
                if (reference is Control control)
                {
                    var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
                    isHit = bounds.Contains(hitTestParameters.HitPoint);
                }

                if (isHit)
                {
                    var result = new PointHitTestResult(reference, hitTestParameters.HitPoint);
                    var resultBehavior = resultCallback(result);
                    
                    if (resultBehavior == HitTestResultBehavior.Stop)
                        return false; // Stop
                }
            }

            return true; // Continue
        }

        public static void HitTest(Visual reference, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, GeometryHitTestParameters hitTestParameters)
        {
            if (reference == null || hitTestParameters?.HitGeometry == null)
                return;

            HitTestVisual(reference, filterCallback, resultCallback, hitTestParameters);
        }

        private static bool HitTestVisual(Visual visual, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, GeometryHitTestParameters hitTestParameters)
        {
            if (visual == null)
                return true; // Continue

            // Apply filter callback
            var filterResult = filterCallback?.Invoke(visual) ?? HitTestFilterBehavior.Continue;

            if (filterResult == HitTestFilterBehavior.Stop)
                return false; // Stop

            if (filterResult == HitTestFilterBehavior.ContinueSkipSelfAndChildren)
                return true; // Continue

            // Hit test this visual if not skipping self
            if (filterResult != HitTestFilterBehavior.ContinueSkipSelf && 
                filterResult != HitTestFilterBehavior.ContinueSkipSelfAndChildren)
            {
                // Check if this visual intersects with the geometry
                if (visual is Control control)
                {
                    var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
                    var visualGeometry = new RectangleGeometry(bounds);

                    var intersection = GetIntersectionDetail(hitTestParameters.HitGeometry, visualGeometry, visual);

                    if (intersection != IntersectionDetail.Empty)
                    {
                        var result = new GeometryHitTestResult
                        {
                            VisualHit = visual,
                            IntersectionDetail = intersection
                        };

                        var resultBehavior = resultCallback?.Invoke(result) ?? HitTestResultBehavior.Continue;
                        if (resultBehavior == HitTestResultBehavior.Stop)
                            return false; // Stop
                    }
                }
            }

            // Continue with children if not skipping
            if (filterResult != HitTestFilterBehavior.ContinueSkipChildren && 
                filterResult != HitTestFilterBehavior.ContinueSkipSelfAndChildren)
            {
                foreach (var child in visual.GetVisualChildren().OfType<Visual>())
                {
                    // If child hit test returns false (Stop), stop processing immediately
                    if (!HitTestVisual(child, filterCallback, resultCallback, hitTestParameters))
                        return false; // Stop - child found a hit
                }
            }

            return true; // Continue
        }

        private static IntersectionDetail GetIntersectionDetail(Geometry testGeometry, Geometry visualGeometry, Visual visual)
        {
            if (testGeometry == null || visualGeometry == null)
                return IntersectionDetail.Empty;

            try
            {
                // Transform the visual geometry to the coordinate system of the test geometry
                var transform = visual.RenderTransform;
                if (transform != null)
                {
                    visualGeometry = visualGeometry.Clone();
                    visualGeometry.Transform = (Transform)transform;
                }

                // Check if the visual bounds are fully inside the test geometry
                var visualBounds = visualGeometry.Bounds;
                var testBounds = testGeometry.Bounds;

                if (testBounds.Contains(visualBounds))
                    return IntersectionDetail.FullyInside;

                if (testBounds.Intersects(visualBounds))
                    return IntersectionDetail.Intersects;

                return IntersectionDetail.Empty;
            }
            catch
            {
                return IntersectionDetail.Empty;
            }
        }

        public static int GetChildrenCount(AvaloniaObject reference)
        {
            if (reference is Visual vs)
                return vs.GetVisualChildren().Count();
            return 0;
        }

        internal class TopMostHitResult
        {
            internal HitTestResult _hitResult = null;

            internal HitTestResultBehavior HitTestResult(HitTestResult result)
            {
                _hitResult = result;

                return HitTestResultBehavior.Stop;
            }
        }

        public static HitTestResult HitTest(Visual reference, Point point)
        {
            TopMostHitResult result = new TopMostHitResult();

            VisualTreeHelper.HitTest(
                reference,
                null,
                new HitTestResultCallback(result.HitTestResult),
                new PointHitTestParameters(point));

            return result._hitResult;
        }

        public static PointHitTestResult AsNearestPointHitTestResult(HitTestResult result)
        {
            if (result == null)
            {
                return null;
            }

            PointHitTestResult resultAsPointHitTestResult = result as PointHitTestResult;

            if (resultAsPointHitTestResult != null)
            {
                return resultAsPointHitTestResult;
            }

            //RayHitTestResult resultAsRayHitTestResult = result as RayHitTestResult;

            //if (resultAsRayHitTestResult != null)
            //{
            //    Visual3D current = (Visual3D)resultAsRayHitTestResult.VisualHit;
            //    Matrix3D worldTransform = Matrix3D.Identity;

            //    while (true)
            //    {
            //        if (current.Transform != null)
            //        {
            //            worldTransform.Append(current.Transform.Value);
            //        }

            //        Visual3D parent3D = current.InternalVisualParent as Visual3D;

            //        if (parent3D == null)
            //        {
            //            break;
            //        }

            //        current = parent3D;
            //    }

            //    return null;
            //}

            return null;
        }
    }
}
