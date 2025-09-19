using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace gip.ext.design.avui.UIExtensions
{

    public static class TextBoxExtensions
    {
        public static Rect GetRectFromCharacterIndex(this TextBox tb, int charIndex)
        {
            try
            {
                // Get the Avalonia TextBox type
                var textBoxType = typeof(Avalonia.Controls.TextBox);

                // Get the internal GetCursorRectangle method
                var method = textBoxType.GetMethod("GetCursorRectangle",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (method == null)
                    return new Rect();

                // Invoke the method with the character index
                object result = method.Invoke(tb, null);
                if (result is Rect rect)
                    return rect;
            }
            catch (Exception)
            {
                // Return empty rect if reflection fails
            }

            return new Rect();
        }
    }
}