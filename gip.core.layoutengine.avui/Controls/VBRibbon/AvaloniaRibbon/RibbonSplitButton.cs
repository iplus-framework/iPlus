using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;

namespace gip.core.layoutengine.avui.AvaloniaRibbon;

public class RibbonSplitButton : RibbonDropDownButton, IRibbonCommand
{
    public static readonly StyledProperty<ICommand> CommandProperty;
    public static readonly StyledProperty<object> CommandParameterProperty;

    static RibbonSplitButton()
    {
        CommandProperty = Button.CommandProperty.AddOwner<RibbonSplitButton>();
        CommandParameterProperty = Button.CommandParameterProperty.AddOwner<RibbonSplitButton>();

        FocusableProperty.OverrideDefaultValue<RibbonSplitButton>(false);
    }

    protected override Type StyleKeyOverride => typeof(RibbonSplitButton);

    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }
}