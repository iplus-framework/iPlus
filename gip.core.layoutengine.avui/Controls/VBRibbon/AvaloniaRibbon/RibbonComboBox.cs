using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;
using gip.core.layoutengine.avui.AvaloniaRibbon.Helpers;
using gip.core.layoutengine.avui.AvaloniaRibbon.Models;

namespace gip.core.layoutengine.avui.AvaloniaRibbon;

public class RibbonComboBox : ComboBox, IRibbonInputControl
{
    public static readonly AvaloniaProperty<RibbonControlSize> MaxSizeProperty;
    public static readonly AvaloniaProperty<RibbonControlSize> MinSizeProperty;
    public static readonly AvaloniaProperty<RibbonControlSize> SizeProperty;

    public static readonly StyledProperty<object> ContentProperty =
        AvaloniaProperty.Register<RibbonComboBox, object>(nameof(Content));

    public static readonly StyledProperty<object> IconProperty =
        AvaloniaProperty.Register<RibbonComboBox, object>(nameof(Icon));

    public static readonly StyledProperty<object> LargeIconProperty =
        AvaloniaProperty.Register<RibbonComboBox, object>(nameof(LargeIcon));

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

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

    public RibbonControlSize Size
    {
        get => (RibbonControlSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public RibbonControlSize MinSize
    {
        get => (RibbonControlSize)GetValue(MinSizeProperty);
        set => SetValue(MinSizeProperty, value);
    }

    public RibbonControlSize MaxSize
    {
        get => (RibbonControlSize)GetValue(MaxSizeProperty);
        set => SetValue(MaxSizeProperty, value);
    }


    static RibbonComboBox()
    {
        RibbonControlHelper<RibbonComboBox>.SetProperties(out SizeProperty, out MinSizeProperty, out MaxSizeProperty);
        FocusableProperty.OverrideDefaultValue<RibbonComboBox>(false);
    }

    protected override Type StyleKeyOverride => typeof(RibbonComboBox);
}