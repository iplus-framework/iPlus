using Avalonia;
using Avalonia.Controls;

namespace gip.core.layoutengine.avui
{
    public class VBButtonSpinner : ButtonSpinner
    {
        /// <summary>
        /// When set, the spinner body (BgElement) is not painted by the theme's
        /// disabled/pointerover/focus/error state styles. Used when the spinner hosts
        /// a control (e.g. VBTextBox) that draws its own border and hover visuals.
        /// </summary>
        public static readonly StyledProperty<bool> TransparentBodyProperty =
            AvaloniaProperty.Register<VBButtonSpinner, bool>(nameof(TransparentBody));

        public bool TransparentBody
        {
            get => GetValue(TransparentBodyProperty);
            set => SetValue(TransparentBodyProperty, value);
        }
    }
}
