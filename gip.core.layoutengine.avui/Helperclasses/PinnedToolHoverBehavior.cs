// Copyright (c) GIP. All rights reserved.
#nullable enable
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace gip.core.layoutengine.avui.Helperclasses
{
    /// <summary>
    /// Hover-to-preview behavior for pinned dockable buttons (ToolPinItemControl).
    /// <para>
    /// Mouse (desktop mode):
    /// - Hovering the pinned tool button opens the dockable preview after a short delay.
    /// - The preview stays open while the mouse is over the button OR over the preview
    ///   itself (overlay panel or pinned dock window).
    /// - It collapses only when the mouse leaves both the button and the preview.
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

        private const int ShowDelayMs = 300;
        private const int PollIntervalMs = 100;
        private const int HideGraceMs = 250;

        private sealed class HoverState
        {
            public DispatcherTimer? ShowTimer;
            public DispatcherTimer? PollTimer;
            public DateTime HideGraceUntil;
            public bool HoverOpened;
        }

        private static readonly ConditionalWeakTable<Control, HoverState> States = new();

        private static HoverState GetState(Control control) => States.GetOrCreateValue(control);

        private static void StopTimer(DispatcherTimer? timer) => timer?.Stop();

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
            if (sender is not Control { DataContext: IDockable dockable } control
                || GetFactory(control) is not { } factory)
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
            var state = GetState(control);

            // Preview is already open (pointer returning from the preview area):
            // keep it open, the poll timer handles hiding.
            if (state.HoverOpened)
            {
                return;
            }

            StopTimer(state.ShowTimer);
            var showTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ShowDelayMs) };
            showTimer.Tick += (_, _) =>
            {
                showTimer.Stop();
                if (state.ShowTimer == showTimer)
                {
                    state.ShowTimer = null;
                }

                // Control may have been recycled with a different dockable.
                if (!ReferenceEquals(control.DataContext, dockable))
                {
                    return;
                }

                factory.PreviewPinnedDockable(dockable);
                state.HoverOpened = true;
                StartPollTimer(control, factory, dockable, state);
            };
            state.ShowTimer = showTimer;
            showTimer.Start();
        }

        private static void OnPointerExited(object? sender, PointerEventArgs e)
        {
            if (sender is not Control { DataContext: IDockable dockable } control
                || GetFactory(control) is not { } factory)
            {
                return;
            }

            if (e.Pointer.Type != PointerType.Mouse || ControlManager.TouchScreenMode)
            {
                return;
            }

            var state = GetState(control);
            StopTimer(state.ShowTimer);

            if (state.HoverOpened)
            {
                // Give the pointer a short grace period to travel from the
                // button into the preview before the poll may hide it.
                state.HideGraceUntil = DateTime.UtcNow + TimeSpan.FromMilliseconds(HideGraceMs);
                StartPollTimer(control, factory, dockable, state);
            }
        }

        private static void StartPollTimer(Control control, IFactory factory, IDockable dockable, HoverState state)
        {
            if (state.PollTimer is { })
            {
                return;
            }

            var poll = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(PollIntervalMs) };
            poll.Tick += (_, _) =>
            {
                if (!state.HoverOpened)
                {
                    poll.Stop();
                    state.PollTimer = null;
                    return;
                }

                // Keep the preview open while the pointer is over the button or
                // the preview area (overlay panel or pinned dock window).
                if (control.IsPointerOver
                    || IsPointerOverPreviewArea(control)
                    || DateTime.UtcNow < state.HideGraceUntil)
                {
                    return;
                }

                poll.Stop();
                state.PollTimer = null;
                state.HoverOpened = false;

                // Only touches rootDock.PinnedDock contents; if the dockable was
                // pinned open by click it is no longer in the preview dock, so
                // this is a no-op for it.
                if (factory.FindRoot(dockable, _ => true) is { } rootDock)
                {
                    factory.HidePreviewingDockables(rootDock);
                }
            };
            state.PollTimer = poll;
            poll.Start();
        }

        private static bool IsPointerOverPreviewArea(Control control)
        {
            // Overlay mode: the preview is hosted inside a PinnedDockControl in
            // the same top-level as the button.
            if (TopLevel.GetTopLevel(control) is { } topLevel)
            {
                foreach (var pinnedDock in topLevel.GetVisualDescendants().OfType<PinnedDockControl>())
                {
                    if (pinnedDock.IsPointerOver)
                    {
                        return true;
                    }
                }
            }

            // Window mode (DockSettings.UsePinnedDockWindow): the preview is a
            // separate PinnedDockWindow.
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                foreach (var window in desktop.Windows)
                {
                    if (window is PinnedDockWindow { IsVisible: true } pinnedWindow
                        && pinnedWindow.IsPointerOver)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is not Control { DataContext: IDockable dockable } control
                || GetFactory(control) is not { } factory)
            {
                return;
            }

            var state = GetState(control);
            StopTimer(state.ShowTimer);
            StopTimer(state.PollTimer);
            state.PollTimer = null;
            state.HoverOpened = false;

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
