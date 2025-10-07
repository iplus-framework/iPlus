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

        public static Transform TransformToAncestor(this Visual element, Visual ancestor)
        {
            ArgumentNullException.ThrowIfNull(ancestor);
            ArgumentNullException.ThrowIfNull(element);
            return InternalTransformToAncestor(element, ancestor, false);
        }

        private static Transform InternalTransformToAncestor(Visual element, Visual ancestor, bool inverse)
        {
            Transform generalTransform;
            Avalonia.Matrix simpleTransform;

            bool isSimple = TrySimpleTransformToAncestor(element, ancestor,
                                                         inverse,
                                                         out generalTransform,
                                                         out simpleTransform);

            if (isSimple)
            {
                MatrixTransform matrixTransform = new MatrixTransform(simpleTransform);
                return matrixTransform;
            }
            else
            {
                return generalTransform;
            }
        }

        private static bool TrySimpleTransformToAncestor(Visual element, Visual ancestor,
                                           bool inverse,
                                           out Transform generalTransform,
                                           out Avalonia.Matrix simpleTransform)
        {
            generalTransform = null;
            simpleTransform = Avalonia.Matrix.Identity;

            if (element == null || ancestor == null)
                return false;

            Visual current = element;
            Avalonia.Matrix transform = Avalonia.Matrix.Identity;

            // Walk up the visual tree until we find the ancestor
            while (current != null && current != ancestor)
            {
                // Get the transform relative to parent
                var renderTransform = current.RenderTransform;
                if (renderTransform != null)
                {
                    transform = renderTransform.Value * transform;
                }

                // Apply render transform origin
                var transformOrigin = current.RenderTransformOrigin;
                if (transformOrigin != default)
                {
                    var bounds = current.Bounds;
                    var originX = transformOrigin.Point.X * bounds.Width;
                    var originY = transformOrigin.Point.Y * bounds.Height;
                    
                    var originTransform = Avalonia.Matrix.CreateTranslation(-originX, -originY);
                    var backTransform = Avalonia.Matrix.CreateTranslation(originX, originY);
                    transform = originTransform * transform * backTransform;
                }

                // Get parent
                current = current.GetVisualParent() as Visual;
            }

            if (current != ancestor)
            {
                throw new InvalidOperationException("Element is not a descendant of the specified ancestor.");
            }

            if (inverse)
            {
                if (transform.HasInverse)
                {
                    transform = transform.Invert();
                }
                else
                {
                    return false; // Cannot invert
                }
            }

            simpleTransform = transform;
            return true; // Simple transform succeeded
        }


        public static void HitTest(Visual reference, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, PointHitTestParameters hitTestParameters)
        {
            if (reference == null || hitTestParameters == null)
                return;

            if (resultCallback == null)
                throw new ArgumentNullException(nameof(resultCallback));

            HitTestVisual(reference, filterCallback, resultCallback, hitTestParameters);
        }

        private static void HitTestVisual(Visual reference, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, PointHitTestParameters hitTestParameters)
        {
            if (reference == null)
                return;

            // Apply filter callback
            var filterResult = filterCallback?.Invoke(reference) ?? HitTestFilterBehavior.Continue;

            if (filterResult == HitTestFilterBehavior.Stop)
                return;

            if (filterResult == HitTestFilterBehavior.ContinueSkipSelfAndChildren)
                return;

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
                        
                        HitTestVisual(child, filterCallback, resultCallback, childParams);
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
                        return;
                }
            }
        }

        public static void HitTest(Visual reference, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, GeometryHitTestParameters hitTestParameters)
        {
            if (reference == null || hitTestParameters?.HitGeometry == null)
                return;

            HitTestVisual(reference, filterCallback, resultCallback, hitTestParameters);
        }

        private static void HitTestVisual(Visual visual, HitTestFilterCallback filterCallback, HitTestResultCallback resultCallback, GeometryHitTestParameters hitTestParameters)
        {
            if (visual == null)
                return;

            // Apply filter callback
            var filterResult = filterCallback?.Invoke(visual) ?? HitTestFilterBehavior.Continue;

            if (filterResult == HitTestFilterBehavior.Stop)
                return;

            if (filterResult == HitTestFilterBehavior.ContinueSkipSelfAndChildren)
                return;

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
                            return;
                    }
                }
            }

            // Continue with children if not skipping
            if (filterResult != HitTestFilterBehavior.ContinueSkipChildren && 
                filterResult != HitTestFilterBehavior.ContinueSkipSelfAndChildren)
            {
                foreach (var child in visual.GetVisualChildren().OfType<Visual>())
                {
                    HitTestVisual(child, filterCallback, resultCallback, hitTestParameters);
                }
            }
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
