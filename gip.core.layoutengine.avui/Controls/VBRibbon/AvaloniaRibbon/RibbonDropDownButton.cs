using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

using gip.core.layoutengine.avui.AvaloniaRibbon.Contracts;
using gip.core.layoutengine.avui.AvaloniaRibbon.Helpers;
using gip.core.layoutengine.avui.AvaloniaRibbon.Models;

using System;

namespace gip.core.layoutengine.avui.AvaloniaRibbon;

[TemplatePart("PART_PrimaryButton", typeof(Button))]
public class RibbonDropDownButton : ItemsControl, IRibbonControl, ICanAddToQuickAccess
{
    #region Fields

    private DispatcherTimer _flyoutMonitorTimer;

    private Flyout _flyout;
    private Button _primaryButton = null;
    private bool _isFlyoutOpen = false;

    #endregion Fields

    static RibbonDropDownButton()
    {
        RibbonControlHelper<RibbonDropDownButton>.SetProperties(out SizeProperty, out MinSizeProperty,
            out MaxSizeProperty);
    }

    #region Static Properties

    public static readonly StyledProperty<bool> CanAddToQuickAccessProperty =
        RibbonButton.CanAddToQuickAccessProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<object> ContentProperty =
        ContentControl.ContentProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<object> IconProperty =
        RibbonButton.IconProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        ComboBox.IsDropDownOpenProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<object> LargeIconProperty =
        RibbonButton.LargeIconProperty.AddOwner<RibbonDropDownButton>();

    public static readonly AvaloniaProperty<RibbonControlSize> MaxSizeProperty;
    public static readonly AvaloniaProperty<RibbonControlSize> MinSizeProperty;

    public static readonly StyledProperty<object> QuickAccessIconProperty =
        RibbonButton.QuickAccessIconProperty.AddOwner<RibbonDropDownButton>();

    public static readonly StyledProperty<IControlTemplate> QuickAccessTemplateProperty =
        RibbonButton.QuickAccessTemplateProperty.AddOwner<RibbonDropDownButton>();

    public static readonly AvaloniaProperty<RibbonControlSize> SizeProperty;

    #endregion Static Properties

    #region Properties

    public bool CanAddToQuickAccess
    {
        get => GetValue(CanAddToQuickAccessProperty);
        set => SetValue(CanAddToQuickAccessProperty, value);
    }

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

    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public object LargeIcon
    {
        get => GetValue(LargeIconProperty);
        set => SetValue(LargeIconProperty, value);
    }

    public RibbonControlSize MaxSize
    {
        get => (RibbonControlSize)(GetValue(MaxSizeProperty) ?? RibbonControlSize.Small);
        set => SetValue(MaxSizeProperty, value);
    }

    public RibbonControlSize MinSize
    {
        get => (RibbonControlSize)(GetValue(MinSizeProperty) ?? RibbonControlSize.Small);
        set => SetValue(MinSizeProperty, value);
    }

    public object QuickAccessIcon
    {
        get => GetValue(QuickAccessIconProperty);
        set => SetValue(QuickAccessIconProperty, value);
    }

    public IControlTemplate QuickAccessTemplate
    {
        get => GetValue(QuickAccessTemplateProperty);
        set => SetValue(QuickAccessTemplateProperty, value);
    }

    public RibbonControlSize Size
    {
        get => (RibbonControlSize)(GetValue(SizeProperty) ?? RibbonControlSize.Small);
        set => SetValue(SizeProperty, value);
    }

    #endregion Properties

    /// <summary>
    /// overrides the OnApplyTemplate method to set up the primary button and its flyout.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        UnregisterEvents();
        UnregisterFlyoutEvents(_flyout);

        _primaryButton = e.NameScope.Find<Button>("PART_PrimaryButton");

        if (_primaryButton != null)
        {
            _flyout = _primaryButton.Flyout as Flyout;
            if (_flyout != null)
            {
                if (_flyout.Content is Control contentControl)
                {
                    contentControl.PointerExited += (_, __) =>
                    {
                        CloseFlyout();
                    };
                }
            }
        }

        _primaryButton.Click += PrimaryButton_Click;
        _primaryButton.AddHandler(PointerPressedEvent, PrimaryButton_PreviewPointerPressed, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// Handle PreviewPointerPressed event for the primary button.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PrimaryButton_PreviewPointerPressed(object sender, PointerPressedEventArgs e)
    {
    }

    /// <summary>
    /// Handle Click event for the primary button.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PrimaryButton_Click(object sender, RoutedEventArgs e)
    {
        // Handle internal button click, so it won't bubble outside.
        e.Handled = true;
        OnClickPrimary(e);
    }

    /// <summary>
    /// onClickPrimary is called when the primary button is clicked.
    /// </summary>
    /// <param name="e"></param>
    private void OnClickPrimary(RoutedEventArgs e)
    {
        // Note: It is not currently required to check enabled status; however, this is a failsafe
        if (IsEffectivelyEnabled)
        {
            if (_isFlyoutOpen)
            {
                CloseFlyout();
            }
            else
            {
                OpenFlyout();
            }
        }
    }

    /// <summary>
    /// Opens the secondary button's flyout.
    /// </summary>
    protected void OpenFlyout()
    {
        if (_flyout != null && _primaryButton != null)
        {
            _isFlyoutOpen = true;
            _flyout.Placement = PlacementMode.Bottom;

            _flyout.ShowAt(_primaryButton);

            // Start monitoring pointer position
            _flyoutMonitorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            _flyoutMonitorTimer.Tick += (_, __) => MonitorPointerExit();
            _flyoutMonitorTimer.Start();
        }
    }

    /// <summary>
    /// Monitors the pointer exit from the primary button and flyout content.
    /// </summary>
    private void MonitorPointerExit()
    {
        if (_primaryButton == null || _flyout?.Content is not Control popupContent)
            return;

        if (!_primaryButton.IsPointerOver && !popupContent.IsPointerOver)
        {
            CloseFlyout();
        }
    }

    /// <summary>
    /// Closes the secondary button's flyout.
    /// </summary>
    protected void CloseFlyout()
    {
        _flyout?.Hide();
        _isFlyoutOpen = false;
        _flyoutMonitorTimer?.Stop();
        _flyoutMonitorTimer = null;
    }

    /// <summary>
    /// Explicitly unregisters all flyout events.
    /// </summary>
    /// <param name="flyout">The flyout to disconnect events from.</param>
    private void UnregisterFlyoutEvents(FlyoutBase flyout)
    {
    }

    /// <summary>
    /// Explicitly unregisters all events related to the two buttons in OnApplyTemplate().
    /// </summary>
    private void UnregisterEvents()
    {
        if (_primaryButton != null)
        {
            _primaryButton.Click -= PrimaryButton_Click;
            _primaryButton.RemoveHandler(PointerPressedEvent, PrimaryButton_PreviewPointerPressed);
        }
    }
}