// Copyright (c) GIP. All rights reserved.
#nullable enable
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace gip.core.layoutengine.avui.Helperclasses
{
    /// <summary>
    /// Hover-to-preview behavior for pinned dockable buttons (ToolPinItemControl).
    /// <para>
    /// Mouse (desktop mode):
    /// - Hovering the pinned tool button opens the dockable preview.
    /// - Moving the mouse away collapses the preview (unless the dockable is pinned).
    /// - Clicking the button pins the dockable (locks the preview open, same as the pin icon).
    /// </para>
    /// <para>
    /// Touch (no hover state):
    /// - Tapping the button toggles the preview (existing iPlus touch behavior).
    /// - Pinning stays on the pin icon / context menu.
    /// </para>
    /// Attach in XAML: <c>helpers:PinnedToolHoverBehavior.HoverPreviewEnabled="True"</c>
    /// on the button inside the ToolPinItemControl control theme.
    /// The button must NOT set a Command; this behavior handles the click for both input types.
    /// </summary>
    public static class PinnedToolHoverBehavior
    {
        /// <summary>
        /// Attached property to enable the hover-to-preview behavior on a control
        /// whose <c>DataContext</c> is an <see cref="IDockable"/>.
        /// </summary>
        public static readonly AttachedProperty<bool> HoverPreviewEnabledProperty =
            AvaloniaProperty.RegisterAttached<Control, Control, bool>("HoverPreviewEnabled", false);

        static PinnedToolHoverBehavior()
        {
            HoverPreviewEnabledProperty.Changed.AddClassHandler<Control>(OnHoverPreviewEnabledChanged);
        }

        /// <summary>
        /// Sets the value of the HoverPreviewEnabled attached property.
        /// </summary>
        public static void SetHoverPreviewEnabled(Control control, bool value)
        {
            control.SetValue(HoverPreviewEnabledProperty, value);
        }

        /// <summary>
        /// Gets the value of the HoverPreviewEnabled attached property.
        /// </summary>
        public static bool GetHoverPreviewEnabled(Control control)
        {
            return control.GetValue(HoverPreviewEnabledProperty);
        }

        private static void OnHoverPreviewEnabledChanged(Control sender, AvaloniaPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue!)
            {
                sender.PointerEntered += OnPointerEntered;
                sender.PointerExited += OnPointerExited;
                // Button handles PointerReleased internally (to raise Click) and
                // marks it handled; subscribe with handledEventsToo to still receive it.
                sender.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Bubble, handledEventsToo: true);
            }
            else
            {
                sender.PointerEntered -= OnPointerEntered;
                sender.PointerExited -= OnPointerExited;
                sender.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
            }
        }

        private static IFactory? GetFactory(Control control)
        {
            if (control.DataContext is IDockable { Owner: IDock owner })
            {
                return owner.Factory;
            }
            return null;
        }

        private static void OnPointerEntered(object? sender, PointerEventArgs e)
        {
            if (sender is not Control { DataContext: IDockable dockable }
                || GetFactory((Control)sender) is not { } factory)
            {
                return;
            }

            // Hover-to-preview is mouse-only. Touch has no hover state and is
            // handled in OnPointerReleased (tap toggles the preview).
            // TouchScreenMode forces touch behavior even when a mouse is connected.
            if (e.Pointer.Type != PointerType.Mouse || ControlManager.TouchScreenMode)
            {
                return;
            }

            // Note: an auto-hide button IS a 'pinned' dockable in Dock terms
            // (PinDockable toggles between docked and auto-hide), so there is
            // deliberately no IsDockablePinned guard here.
            factory.PreviewPinnedDockable(dockable);
        }

        private static void OnPointerExited(object? sender, PointerEventArgs e)
        {
            if (sender is not Control { DataContext: IDockable dockable }
                || GetFactory((Control)sender) is not { } factory)
            {
                return;
            }

            if (e.Pointer.Type != PointerType.Mouse || ControlManager.TouchScreenMode)
            {
                return;
            }

            // Only touches rootDock.PinnedDock contents; if the dockable was
            // pinned open by click it is no longer in the preview dock, so this
            // is a no-op for it.
            if (factory.FindRoot(dockable, _ => true) is { } rootDock)
            {
                factory.HidePreviewingDockables(rootDock);
            }
        }

        private static void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is not Control { DataContext: IDockable dockable }
                || GetFactory((Control)sender) is not { } factory)
            {
                return;
            }

            if (e.Pointer.Type == PointerType.Mouse && !ControlManager.TouchScreenMode)
            {
                // Mouse click = same as the pin icon: PinDockable toggles between
                // auto-hide (collapsed button) and docked (locked open).
                factory.PinDockable(dockable);
            }
            else
            {
                // Touch tap = toggle the preview (existing iPlus touch behavior).
                factory.TogglePreviewPinnedDockable(dockable);
            }

            e.Handled = true;
        }
    }
}
