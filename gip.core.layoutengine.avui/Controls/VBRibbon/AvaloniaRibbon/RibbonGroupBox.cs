using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using gip.core.layoutengine.avui.AvaloniaRibbon.Models;

namespace gip.core.layoutengine.avui.AvaloniaRibbon;

public class RibbonGroupBox : HeaderedItemsControl
{
    #region Fields

    private ICommand _command;

    #endregion

    static RibbonGroupBox()
    {
        AffectsArrange<RibbonGroupBox>(DisplayModeProperty);
        AffectsMeasure<RibbonGroupBox>(DisplayModeProperty);
        AffectsRender<RibbonGroupBox>(DisplayModeProperty);

        CommandProperty = AvaloniaProperty.RegisterDirect<RibbonGroupBox, ICommand>(nameof(Command),
            button => button.Command, (button, command) => button.Command = command, enableDataValidation: true);

        // Listen for parent changes to update the attached property
        ParentProperty.Changed.AddClassHandler<RibbonGroupBox>((sender, e) =>
        {
            sender.UpdateParentRibbonOrientation();
        });
    }

    #region Static Properties

    public static readonly StyledProperty<object> CommandParameterProperty =
        AvaloniaProperty.Register<RibbonGroupBox, object>(nameof(CommandParameter));

    public static readonly DirectProperty<RibbonGroupBox, ICommand> CommandProperty;

    public static readonly StyledProperty<GroupDisplayMode> DisplayModeProperty =
        StyledProperty<RibbonGroupBox>.Register<RibbonGroupBox, GroupDisplayMode>(nameof(DisplayMode),
            GroupDisplayMode.Small);

    // Attached property for parent ribbon orientation
    public static readonly AttachedProperty<Avalonia.Layout.Orientation> ParentRibbonOrientationProperty =
        AvaloniaProperty.RegisterAttached<RibbonGroupBox, RibbonGroupBox, Avalonia.Layout.Orientation>(
            "ParentRibbonOrientation",
            Avalonia.Layout.Orientation.Horizontal);

    #endregion Static Properties

    #region Properties

    public event EventHandler Rearranged;

    public event EventHandler Remeasured;

    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        UpdateParentRibbonOrientation();

        // Subscribe to parent ribbon orientation changes if we find a ribbon
        if (FindParentRibbon() is Ribbon ribbon)
        {
            ribbon.GetObservable(Ribbon.OrientationProperty).Subscribe(_ => UpdateParentRibbonOrientation());
        }
    }

    private void UpdateParentRibbonOrientation()
    {
        var ribbon = FindParentRibbon();
        if (ribbon != null)
        {
            SetValue(ParentRibbonOrientationProperty, ribbon.Orientation);
        }
    }

    private Ribbon FindParentRibbon()
    {
        var parent = this.Parent;
        while (parent != null && parent is not Ribbon)
        {
            parent = parent.Parent;
        }
        return parent as Ribbon;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Rearranged?.Invoke(this, null);
        return base.ArrangeOverride(finalSize);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        Remeasured?.Invoke(this, null);
        return base.MeasureOverride(availableSize);
    }

    protected override Type StyleKeyOverride => typeof(RibbonGroupBox);

    public ICommand Command
    {
        get => _command;
        set => SetAndRaise(CommandProperty, ref _command, value);
    }

    public object CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public GroupDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public Ribbon ParentRibbon
    {
        get
        {
            var parent = this.Parent;
            while (parent != null && parent is not Ribbon)
            {
                parent = parent.Parent;
            }
            return parent as Ribbon;
        }
    }

    // Helper methods for attached property
    public static Avalonia.Layout.Orientation GetParentRibbonOrientation(RibbonGroupBox element)
    {
        return element.GetValue(ParentRibbonOrientationProperty);
    }

    public static void SetParentRibbonOrientation(RibbonGroupBox element, Avalonia.Layout.Orientation value)
    {
        element.SetValue(ParentRibbonOrientationProperty, value);
    }

    #endregion
}