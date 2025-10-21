using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace gip.core.layoutengine.avui.timeline
{
    /// <summary>
    /// Simple theme selector that selects themes based on item type.
    /// This replaces WPF's StyleSelector for AvaloniaUI.
    /// </summary>
    public interface IItemThemeSelector
    {
        ControlTheme SelectTheme(object item, Control container);
    }

    /// <summary>
    /// Default implementation that selects theme by item type name.
    /// </summary>
    public class ItemTypeThemeSelector : IItemThemeSelector
    {
        public ControlTheme SelectTheme(object item, Control container)
        {
            if (item == null || container == null)
                return null;

            var itemType = item.GetType();
            
            // Try to find a ControlTheme resource based on the item's type name
            if (container.TryFindResource(itemType.Name, ThemeVariant.Default, out var theme) && theme is ControlTheme controlTheme)
                return controlTheme;

            // Try with full type name
            if (container.TryFindResource(itemType.FullName, ThemeVariant.Default, out var fullNameTheme) && fullNameTheme is ControlTheme fullControlTheme)
                return fullControlTheme;

            return null;
        }
    }

    /// <summary>
    /// Backward compatibility alias for existing code.
    /// </summary>
    public class StyleSelectorByItemType : ItemTypeThemeSelector
    {
        // Inherits all functionality from ItemTypeThemeSelector
    }
}