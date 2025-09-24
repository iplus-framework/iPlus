using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System.Reflection;

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
    }
}