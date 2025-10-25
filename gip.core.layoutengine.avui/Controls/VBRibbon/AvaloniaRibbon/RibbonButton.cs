using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;
using gip.core.layoutengine.avui.AvaloniaRibbon.Helpers;
using gip.core.layoutengine.avui.AvaloniaRibbon.Models;

namespace gip.core.layoutengine.avui.AvaloniaRibbon;

public class RibbonButton : Button, IRibbonInputControl, IRibbonCommand, ICanAddToQuickAccess
{
    public static readonly StyledProperty<bool> CanAddToQuickAccessProperty =
        AvaloniaProperty.Register<RibbonButton, bool>(nameof(CanAddToQuickAccess), true);

    public static readonly StyledProperty<object> IconProperty =
        AvaloniaProperty.Register<RibbonButton, object>(nameof(Icon));

    public static readonly StyledProperty<object> LargeIconProperty =
        AvaloniaProperty.Register<RibbonButton, object>(nameof(LargeIcon));

    public static readonly AvaloniaProperty<RibbonControlSize> MaxSizeProperty;
    public static readonly AvaloniaProperty<RibbonControlSize> MinSizeProperty;

    public static readonly StyledProperty<object> QuickAccessIconProperty =
        AvaloniaProperty.Register<RibbonButton, object>(nameof(QuickAccessIcon));

    public static readonly StyledProperty<IControlTemplate> QuickAccessTemplateProperty =
        AvaloniaProperty.Register<RibbonButton, IControlTemplate>(nameof(QuickAccessTemplate));

    public static readonly AvaloniaProperty<RibbonControlSize> SizeProperty;

    static RibbonButton()
    {
        RibbonControlHelper<RibbonButton>.SetProperties(out SizeProperty, out MinSizeProperty, out MaxSizeProperty);
        FocusableProperty.OverrideDefaultValue<RibbonButton>(false);
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

    public object QuickAccessIcon
    {
        get => GetValue(QuickAccessIconProperty);
        set => SetValue(QuickAccessIconProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(RibbonButton);

    public bool CanAddToQuickAccess
    {
        get => GetValue(CanAddToQuickAccessProperty);
        set => SetValue(CanAddToQuickAccessProperty, value);
    }

    public IControlTemplate QuickAccessTemplate
    {
        get
        {
            var value = GetValue(QuickAccessTemplateProperty);
            var controlTemplate = value as ControlTemplate;
            return value;
        }
        set => SetValue(QuickAccessTemplateProperty, value);
    }

    public RibbonControlSize MaxSize
    {
        get => (RibbonControlSize)GetValue(MaxSizeProperty);
        set => SetValue(MaxSizeProperty, value);
    }

    public RibbonControlSize MinSize
    {
        get => (RibbonControlSize)GetValue(MinSizeProperty);
        set => SetValue(MinSizeProperty, value);
    }

    public RibbonControlSize Size
    {
        get => (RibbonControlSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}