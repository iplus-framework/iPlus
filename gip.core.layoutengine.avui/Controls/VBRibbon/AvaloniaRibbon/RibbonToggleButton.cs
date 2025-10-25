﻿using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;
using gip.core.layoutengine.avui.AvaloniaRibbon.Helpers;
using gip.core.layoutengine.avui.AvaloniaRibbon.Models;

namespace gip.core.layoutengine.avui.AvaloniaRibbon;

public class RibbonToggleButton : ToggleButton, IRibbonControl, ICanAddToQuickAccess
{
    public static readonly AvaloniaProperty<RibbonControlSize> SizeProperty;
    public static readonly AvaloniaProperty<RibbonControlSize> MinSizeProperty;
    public static readonly AvaloniaProperty<RibbonControlSize> MaxSizeProperty;

    public static readonly StyledProperty<object> IconProperty =
        RibbonButton.IconProperty.AddOwner<RibbonToggleButton>();

    public static readonly StyledProperty<object> LargeIconProperty =
        RibbonButton.LargeIconProperty.AddOwner<RibbonToggleButton>();

    public static readonly StyledProperty<object> QuickAccessIconProperty =
        RibbonButton.QuickAccessIconProperty.AddOwner<RibbonToggleButton>();

    public static readonly StyledProperty<bool> CanAddToQuickAccessProperty =
        RibbonButton.CanAddToQuickAccessProperty.AddOwner<RibbonToggleButton>();

    public static readonly StyledProperty<IControlTemplate> QuickAccessTemplateProperty =
        AvaloniaProperty.Register<RibbonButton, IControlTemplate>(nameof(Template));

    static RibbonToggleButton()
    {
        RibbonControlHelper<RibbonToggleButton>.SetProperties(out SizeProperty, out MinSizeProperty,
            out MaxSizeProperty);
        FocusableProperty.OverrideDefaultValue<RibbonToggleButton>(false);
    }

    protected override Type StyleKeyOverride => typeof(RibbonToggleButton);

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

    public bool CanAddToQuickAccess
    {
        get => GetValue(CanAddToQuickAccessProperty);
        set => SetValue(CanAddToQuickAccessProperty, value);
    }

    public IControlTemplate QuickAccessTemplate
    {
        get => GetValue(QuickAccessTemplateProperty);
        set => SetValue(QuickAccessTemplateProperty, value);
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
}