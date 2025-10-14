using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.Reflection;
using System.Collections.Generic;
using Avalonia.Controls.Primitives;
using System.Linq;
using gip.ext.design.avui;

namespace gip.core.layoutengine.avui.Helperclasses
{
    public static class AvaloniaHelperExtensions
    {
        public static void ClearAllBindings(this AvaloniaObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            foreach (var prop in AvaloniaPropertyRegistry.Instance.GetRegistered(obj))
            {
                if (obj.IsSet(prop))
                {
                    BindingOperations.GetBindingExpressionBase(obj, prop)?.Dispose();
                    //var binding = obj.GetBindingObservable(prop);
                    //if (binding != null)
                    //    obj.ClearValue(prop);
                }
            }
        }

        public static void ClearBinding(this AvaloniaObject obj, AvaloniaProperty property)
        {
            if (obj == null || property == null)
                throw new ArgumentNullException(nameof(obj));
            BindingOperations.GetBindingExpressionBase(obj, property)?.Dispose();
        }

        /// <summary>
        /// Gets the binding observable for a property (similar to WPF's GetBindingExpression)
        /// </summary>
        public static IObservable<object> GetBindingObservable(this AvaloniaObject obj, AvaloniaProperty property)
        {
            if (obj == null || property == null) return null;
            
            try
            {
                return obj.GetObservable(property);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Scrolls to the end of text in a TextBox (similar to WPF's ScrollToEnd)
        /// </summary>
        public static void ScrollToEnd(this TextBox textBox)
        {
            if (textBox == null) return;
            
            // Move caret to end
            textBox.CaretIndex = textBox.Text?.Length ?? 0;
        }

        public static void SetOwner(this Window window, WindowBase owner)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            try
            {
                var ownerProperty = typeof(Window).GetProperty("Owner", BindingFlags.Public | BindingFlags.Instance);
                if (ownerProperty != null && ownerProperty.CanWrite)
                {
                    ownerProperty.SetValue(window, owner);
                }
            }
            catch
            {
                // Silently ignore if reflection fails
            }
        }
    }

    public static class BindingExpressionExtensions
    {
        private static readonly FieldInfo SourceField = typeof(BindingExpressionBase)
            .Assembly
            .GetType("Avalonia.Data.Core.BindingExpression")?
            .GetField("_source", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Gets the source object from a BindingExpressionBase using reflection.
        /// </summary>
        /// <param name="bindingExpression">The BindingExpressionBase instance.</param>
        /// <returns>The source object if available, otherwise null.</returns>
        public static object GetSource(this BindingExpressionBase bindingExpression)
        {
            if (bindingExpression == null)
                throw new ArgumentNullException(nameof(bindingExpression));

            // Check if this is actually a BindingExpression instance
            var bindingExpressionType = bindingExpression.GetType();
            if (bindingExpressionType.Name != "BindingExpression")
                return null;

            if (SourceField == null)
                throw new InvalidOperationException("Unable to find _source field via reflection.");

            var sourceWeakRef = SourceField.GetValue(bindingExpression) as WeakReference<object>;

            if (sourceWeakRef != null && sourceWeakRef.TryGetTarget(out var target))
                return target;

            return null;
        }
    }

    public static class AdornerLayerExtensions
    {
        public static IEnumerable<Adorner> GetAdorners(this AdornerLayer adornerLayer, Visual element)
        {
            if (adornerLayer == null || element == null) 
                return null;
            return adornerLayer.Children.OfType<Adorner>().Where(c => c.AdornedElement == element);
        }

        public static AdornerHitTestResult AdornerHitTest(this AdornerLayer adornerLayer, Point point)
        {
            PointHitTestResult result = VisualTreeHelper.AsNearestPointHitTestResult(VisualTreeHelper.HitTest(adornerLayer, point));

            if (result != null && result.VisualHit != null)
            {
                Visual visual = result.VisualHit;

                while (visual != adornerLayer)
                {
                    if (visual is Adorner)
                        return new AdornerHitTestResult(result.VisualHit, result.PointHit, visual as Adorner);

                    // we intentionally separate adorners from spanning 3D boundaries
                    // and if the parent is ever 3D there was a mistake
                    visual = (Visual)VisualTreeHelper.GetParent(visual);
                }

                return null;
            }
            else
            {
                return null;
            }
        }
    }
}