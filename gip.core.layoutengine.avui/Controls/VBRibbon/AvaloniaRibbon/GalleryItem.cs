using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace gip.core.layoutengine.avui.AvaloniaRibbon;

public class GalleryItem : ListBoxItem
{
    public static readonly StyledProperty<object> IconProperty =
        RibbonButton.IconProperty.AddOwner<GalleryItem>();

    public static readonly StyledProperty<object> LargeIconProperty =
        RibbonButton.LargeIconProperty.AddOwner<GalleryItem>();

    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public object LargeIcon
    {
        get => GetValue(LargeIconProperty);
        set => SetValue(LargeIconProperty, value);
    }
}