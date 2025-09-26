// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    ///     Defines an item collection, selection members, and key handling for the
    ///     selection adapter contained in the drop-down portion of an
    ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    public interface ISelectionAdapter
    {
        /// <summary>
        ///     Gets or sets the selected item.
        /// </summary>
        /// <value>The currently selected item.</value>
        object SelectedItem { get; set; }

        /// <summary>
        ///     Occurs when the
        ///     <see cref="P:System.Windows.Controls.ISelectionAdapter.SelectedItem" />
        ///     property value changes.
        /// </summary>
        event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        ///     Gets or sets a collection that is used to generate content for the
        ///     selection adapter.
        /// </summary>
        /// <value>
        ///     The collection that is used to generate content for the
        ///     selection adapter.
        /// </value>
        IEnumerable ItemsSource { get; set; }

        /// <summary>
        ///     Occurs when a selected item is not cancelled and is committed as the
        ///     selected item.
        /// </summary>
        event RoutedEventHandler Commit;

        /// <summary>
        ///     Occurs when a selection has been canceled.
        /// </summary>
        event RoutedEventHandler Cancel;

        /// <summary>
        ///     Provides handling for the
        ///     <see cref="E:System.Windows.UIElement.KeyDown" /> event that occurs
        ///     when a key is pressed while the drop-down portion of the
        ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" /> has focus.
        /// </summary>
        /// <param name="e">
        ///     A <see cref="T:System.Windows.Input.KeyEventArgs" />
        ///     that contains data about the
        ///     <see cref="E:System.Windows.UIElement.KeyDown" /> event.
        /// </param>
        void HandleKeyDown(KeyEventArgs e);

        /// <summary>
        ///     Returns an automation peer for the selection adapter, for use by the
        ///     Silverlight automation infrastructure.
        /// </summary>
        /// <returns>
        ///     An automation peer for the selection adapter, if one is
        ///     available; otherwise, null.
        /// </returns>
        AutomationPeer CreateAutomationPeer();
    }

    /// <summary>
    ///     The IUpdateVisualState interface is used to provide the
    ///     InteractionHelper with access to the type's UpdateVisualState method.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic",
        Justification = "This is not an exception class.")]
    internal interface IUpdateVisualState
    {
        /// <summary>
        ///     Update the visual state of the control.
        /// </summary>
        /// <param name="useTransitions">
        ///     A value indicating whether to automatically generate transitions to
        ///     the new state, or instantly transition to the new state.
        /// </param>
        void UpdateVisualState(bool useTransitions);
    }

    /// <summary>
    ///     Names and helpers for visual states in the controls.
    /// </summary>
    internal static class VisualStates
    {
        #region GroupCommon

        /// <summary>
        ///     Common state group.
        /// </summary>
        public const string GroupCommon = "CommonStates";

        /// <summary>
        ///     Normal state of the Common state group.
        /// </summary>
        public const string StateNormal = "Normal";

        /// <summary>
        ///     Normal state of the Common state group.
        /// </summary>
        public const string StateReadOnly = "ReadOnly";

        /// <summary>
        ///     MouseOver state of the Common state group.
        /// </summary>
        public const string StateMouseOver = "MouseOver";

        /// <summary>
        ///     Pressed state of the Common state group.
        /// </summary>
        public const string StatePressed = "Pressed";

        /// <summary>
        ///     Disabled state of the Common state group.
        /// </summary>
        public const string StateDisabled = "Disabled";

        #endregion GroupCommon

        #region GroupFocus

        /// <summary>
        ///     Focus state group.
        /// </summary>
        public const string GroupFocus = "FocusStates";

        /// <summary>
        ///     Unfocused state of the Focus state group.
        /// </summary>
        public const string StateUnfocused = "Unfocused";

        /// <summary>
        ///     Focused state of the Focus state group.
        /// </summary>
        public const string StateFocused = "Focused";

        #endregion GroupFocus

        #region GroupSelection

        /// <summary>
        ///     Selection state group.
        /// </summary>
        public const string GroupSelection = "SelectionStates";

        /// <summary>
        ///     Selected state of the Selection state group.
        /// </summary>
        public const string StateSelected = "Selected";

        /// <summary>
        ///     Unselected state of the Selection state group.
        /// </summary>
        public const string StateUnselected = "Unselected";

        /// <summary>
        ///     Selected inactive state of the Selection state group.
        /// </summary>
        public const string StateSelectedInactive = "SelectedInactive";

        #endregion GroupSelection

        #region GroupExpansion

        /// <summary>
        ///     Expansion state group.
        /// </summary>
        public const string GroupExpansion = "ExpansionStates";

        /// <summary>
        ///     Expanded state of the Expansion state group.
        /// </summary>
        public const string StateExpanded = "Expanded";

        /// <summary>
        ///     Collapsed state of the Expansion state group.
        /// </summary>
        public const string StateCollapsed = "Collapsed";

        #endregion GroupExpansion

        #region GroupPopup

        /// <summary>
        ///     Popup state group.
        /// </summary>
        public const string GroupPopup = "PopupStates";

        /// <summary>
        ///     Opened state of the Popup state group.
        /// </summary>
        public const string StatePopupOpened = "PopupOpened";

        /// <summary>
        ///     Closed state of the Popup state group.
        /// </summary>
        public const string StatePopupClosed = "PopupClosed";

        #endregion

        #region GroupValidation

        /// <summary>
        ///     ValidationStates state group.
        /// </summary>
        public const string GroupValidation = "ValidationStates";

        /// <summary>
        ///     The valid state for the ValidationStates group.
        /// </summary>
        public const string StateValid = "Valid";

        /// <summary>
        ///     Invalid, focused state for the ValidationStates group.
        /// </summary>
        public const string StateInvalidFocused = "InvalidFocused";

        /// <summary>
        ///     Invalid, unfocused state for the ValidationStates group.
        /// </summary>
        public const string StateInvalidUnfocused = "InvalidUnfocused";

        #endregion

        #region GroupExpandDirection

        /// <summary>
        ///     ExpandDirection state group.
        /// </summary>
        public const string GroupExpandDirection = "ExpandDirectionStates";

        /// <summary>
        ///     Down expand direction state of ExpandDirection state group.
        /// </summary>
        public const string StateExpandDown = "ExpandDown";

        /// <summary>
        ///     Up expand direction state of ExpandDirection state group.
        /// </summary>
        public const string StateExpandUp = "ExpandUp";

        /// <summary>
        ///     Left expand direction state of ExpandDirection state group.
        /// </summary>
        public const string StateExpandLeft = "ExpandLeft";

        /// <summary>
        ///     Right expand direction state of ExpandDirection state group.
        /// </summary>
        public const string StateExpandRight = "ExpandRight";

        #endregion

        #region GroupHasItems

        /// <summary>
        ///     HasItems state group.
        /// </summary>
        public const string GroupHasItems = "HasItemsStates";

        /// <summary>
        ///     HasItems state of the HasItems state group.
        /// </summary>
        public const string StateHasItems = "HasItems";

        /// <summary>
        ///     NoItems state of the HasItems state group.
        /// </summary>
        public const string StateNoItems = "NoItems";

        #endregion GroupHasItems

        #region GroupIncrease

        /// <summary>
        ///     Increment state group.
        /// </summary>
        public const string GroupIncrease = "IncreaseStates";

        /// <summary>
        ///     State enabled for increment group.
        /// </summary>
        public const string StateIncreaseEnabled = "IncreaseEnabled";

        /// <summary>
        ///     State disabled for increment group.
        /// </summary>
        public const string StateIncreaseDisabled = "IncreaseDisabled";

        #endregion GroupIncrease

        #region GroupDecrease

        /// <summary>
        ///     Decrement state group.
        /// </summary>
        public const string GroupDecrease = "DecreaseStates";

        /// <summary>
        ///     State enabled for decrement group.
        /// </summary>
        public const string StateDecreaseEnabled = "DecreaseEnabled";

        /// <summary>
        ///     State disabled for decrement group.
        /// </summary>
        public const string StateDecreaseDisabled = "DecreaseDisabled";

        #endregion GroupDecrease

        #region GroupIteractionMode

        /// <summary>
        ///     InteractionMode state group.
        /// </summary>
        public const string GroupInteractionMode = "InteractionModeStates";

        /// <summary>
        ///     Edit of the DisplayMode state group.
        /// </summary>
        public const string StateEdit = "Edit";

        /// <summary>
        ///     Display of the DisplayMode state group.
        /// </summary>
        public const string StateDisplay = "Display";

        #endregion GroupIteractionMode

        #region GroupLocked

        /// <summary>
        ///     DisplayMode state group.
        /// </summary>
        public const string GroupLocked = "LockedStates";

        /// <summary>
        ///     Edit of the DisplayMode state group.
        /// </summary>
        public const string StateLocked = "Locked";

        /// <summary>
        ///     Display of the DisplayMode state group.
        /// </summary>
        public const string StateUnlocked = "Unlocked";

        #endregion GroupLocked

        #region GroupActive

        /// <summary>
        ///     Active state.
        /// </summary>
        public const string StateActive = "Active";

        /// <summary>
        ///     Inactive state.
        /// </summary>
        public const string StateInactive = "Inactive";

        /// <summary>
        ///     Active state group.
        /// </summary>
        public const string GroupActive = "ActiveStates";

        #endregion GroupActive

        #region GroupWatermark

        /// <summary>
        ///     Non-watermarked state.
        /// </summary>
        public const string StateUnwatermarked = "Unwatermarked";

        /// <summary>
        ///     Watermarked state.
        /// </summary>
        public const string StateWatermarked = "Watermarked";

        /// <summary>
        ///     Watermark state group.
        /// </summary>
        public const string GroupWatermark = "WatermarkStates";

        #endregion GroupWatermark

        #region GroupCalendarButtonFocus

        /// <summary>
        ///     Unfocused state for Calendar Buttons.
        /// </summary>
        public const string StateCalendarButtonUnfocused = "CalendarButtonUnfocused";

        /// <summary>
        ///     Focused state for Calendar Buttons.
        /// </summary>
        public const string StateCalendarButtonFocused = "CalendarButtonFocused";

        /// <summary>
        ///     CalendarButtons Focus state group.
        /// </summary>
        public const string GroupCalendarButtonFocus = "CalendarButtonFocusStates";

        #endregion GroupCalendarButtonFocus

        #region GroupBusyStatus

        /// <summary>
        ///     Busy state for BusyIndicator.
        /// </summary>
        public const string StateBusy = "Busy";

        /// <summary>
        ///     Idle state for BusyIndicator.
        /// </summary>
        public const string StateIdle = "Idle";

        /// <summary>
        ///     Busyness group name.
        /// </summary>
        public const string GroupBusyStatus = "BusyStatusStates";

        #endregion

        #region GroupVisibility

        /// <summary>
        ///     Visible state name for BusyIndicator.
        /// </summary>
        public const string StateVisible = "Visible";

        /// <summary>
        ///     Hidden state name for BusyIndicator.
        /// </summary>
        public const string StateHidden = "Hidden";

        /// <summary>
        ///     BusyDisplay group.
        /// </summary>
        public const string GroupVisibility = "VisibilityStates";

        #endregion

        /// <summary>
        ///     Use VisualStateManager to change the visual state of the control.
        /// </summary>
        /// <param name="control">
        ///     Control whose visual state is being changed.
        /// </param>
        /// <param name="useTransitions">
        ///     A value indicating whether to use transitions when updating the
        ///     visual state, or to snap directly to the new visual state.
        /// </param>
        /// <param name="stateNames">
        ///     Ordered list of state names and fallback states to transition into.
        ///     Only the first state to be found will be used.
        /// </param>
        public static void GoToState(Control control, bool useTransitions, params string[] stateNames)
        {
            Debug.Assert(control != null, "control should not be null!");
            Debug.Assert(stateNames != null, "stateNames should not be null!");
            Debug.Assert(stateNames.Length > 0, "stateNames should not be empty!");

            foreach (string name in stateNames)
            {
                if (VisualStateManager.GoToState(control, name, useTransitions))
                {
                    break;
                }
            }
        }

        /// <summary>
        ///     Gets the implementation root of the Control.
        /// </summary>
        /// <param name="dependencyObject">The DependencyObject.</param>
        /// <remarks>
        ///     Implements Silverlight's corresponding internal property on Control.
        /// </remarks>
        /// <returns>Returns the implementation root or null.</returns>
        public static FrameworkElement GetImplementationRoot(DependencyObject dependencyObject)
        {
            Debug.Assert(dependencyObject != null, "DependencyObject should not be null.");
            return (1 == VisualTreeHelper.GetChildrenCount(dependencyObject))
                ? VisualTreeHelper.GetChild(dependencyObject, 0) as FrameworkElement
                : null;
        }

        /// <summary>
        ///     This method tries to get the named VisualStateGroup for the
        ///     dependency object. The provided object's ImplementationRoot will be
        ///     looked up in this call.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="groupName">The visual state group's name.</param>
        /// <returns>Returns null or the VisualStateGroup object.</returns>
        public static VisualStateGroup TryGetVisualStateGroup(DependencyObject dependencyObject, string groupName)
        {
            FrameworkElement root = GetImplementationRoot(dependencyObject);
            if (root == null)
            {
                return null;
            }

            return VisualStateManager.GetVisualStateGroups(root)
                .OfType<VisualStateGroup>()
                .Where(group => string.CompareOrdinal(groupName, group.Name) == 0)
                .FirstOrDefault();
        }
    }

    /// <summary>
    ///     The InteractionHelper provides controls with support for all of the
    ///     common interactions like mouse movement, mouse clicks, key presses,
    ///     etc., and also incorporates proper event semantics when the control is
    ///     disabled.
    /// </summary>
    internal sealed class InteractionHelper
    {
        // TODO: Consult with user experience experts to validate the double
        // click distance and time thresholds.

        /// <summary>
        ///     The threshold used to determine whether two clicks are temporally
        ///     local and considered a double click (or triple, quadruple, etc.).
        ///     500 milliseconds is the default double click value on Windows.
        ///     This value would ideally be pulled form the system settings.
        /// </summary>
        private const double SequentialClickThresholdInMilliseconds = 500.0;

        /// <summary>
        ///     The threshold used to determine whether two clicks are spatially
        ///     local and considered a double click (or triple, quadruple, etc.)
        ///     in pixels squared.  We use pixels squared so that we can compare to
        ///     the distance delta without taking a square root.
        /// </summary>
        private const double SequentialClickThresholdInPixelsSquared = 3.0 * 3.0;

        /// <summary>
        ///     Gets the control the InteractionHelper is targeting.
        /// </summary>
        public Control Control { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the control has focus.
        /// </summary>
        public bool IsFocused { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the mouse is over the control.
        /// </summary>
        public bool IsMouseOver { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the read-only property is set.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Linked file.")]
        public bool IsReadOnly { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the mouse button is pressed down
        ///     over the control.
        /// </summary>
        public bool IsPressed { get; private set; }

        /// <summary>
        ///     Gets or sets the last time the control was clicked.
        /// </summary>
        /// <remarks>
        ///     The value is stored as Utc time because it is slightly more
        ///     performant than converting to local time.
        /// </remarks>
        private DateTime LastClickTime { get; set; }

        /// <summary>
        ///     Gets or sets the mouse position of the last click.
        /// </summary>
        /// <remarks>The value is relative to the control.</remarks>
        private Point LastClickPosition { get; set; }

        /// <summary>
        ///     Gets the number of times the control was clicked.
        /// </summary>
        public int ClickCount { get; private set; }

        /// <summary>
        ///     Reference used to call UpdateVisualState on the base class.
        /// </summary>
        private readonly IUpdateVisualState _updateVisualState;

        /// <summary>
        ///     Initializes a new instance of the InteractionHelper class.
        /// </summary>
        /// <param name="control">Control receiving interaction.</param>
        public InteractionHelper(Control control)
        {
            Debug.Assert(control != null, "control should not be null!");
            Control = control;
            _updateVisualState = control as IUpdateVisualState;

            // Wire up the event handlers for events without a virtual override
            control.Loaded += OnLoaded;
            control.IsEnabledChanged += OnIsEnabledChanged;
        }

        #region UpdateVisualState

        /// <summary>
        ///     Update the visual state of the control.
        /// </summary>
        /// <param name="useTransitions">
        ///     A value indicating whether to automatically generate transitions to
        ///     the new state, or instantly transition to the new state.
        /// </param>
        /// <remarks>
        ///     UpdateVisualState works differently than the rest of the injected
        ///     functionality.  Most of the other events are overridden by the
        ///     calling class which calls Allow, does what it wants, and then calls
        ///     Base.  UpdateVisualState is the opposite because a number of the
        ///     methods in InteractionHelper need to trigger it in the calling
        ///     class.  We do this using the IUpdateVisualState internal interface.
        /// </remarks>
        private void UpdateVisualState(bool useTransitions)
        {
            if (_updateVisualState != null)
            {
                _updateVisualState.UpdateVisualState(useTransitions);
            }
        }

        /// <summary>
        ///     Update the visual state of the control.
        /// </summary>
        /// <param name="useTransitions">
        ///     A value indicating whether to automatically generate transitions to
        ///     the new state, or instantly transition to the new state.
        /// </param>
        public void UpdateVisualStateBase(bool useTransitions)
        {
            // Handle the Common states
            if (!Control.IsEnabled)
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateDisabled, VisualStates.StateNormal);
            }
            else if (IsReadOnly)
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateReadOnly, VisualStates.StateNormal);
            }
            else if (IsPressed)
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StatePressed, VisualStates.StateMouseOver,
                    VisualStates.StateNormal);
            }
            else if (IsMouseOver)
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateMouseOver, VisualStates.StateNormal);
            }
            else
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateNormal);
            }

            // Handle the Focused states
            if (IsFocused)
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateFocused, VisualStates.StateUnfocused);
            }
            else
            {
                VisualStates.GoToState(Control, useTransitions, VisualStates.StateUnfocused);
            }
        }

        #endregion UpdateVisualState

        /// <summary>
        ///     Handle the control's Loaded event.
        /// </summary>
        /// <param name="sender">The control.</param>
        /// <param name="e">Event arguments.</param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateVisualState(false);
        }

        /// <summary>
        ///     Handle changes to the control's IsEnabled property.
        /// </summary>
        /// <param name="sender">The control.</param>
        /// <param name="e">Event arguments.</param>
        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var enabled = (bool)e.NewValue;
            if (!enabled)
            {
                IsPressed = false;
                IsMouseOver = false;
                IsFocused = false;
            }

            UpdateVisualState(true);
        }

        /// <summary>
        ///     Handles changes to the control's IsReadOnly property.
        /// </summary>
        /// <param name="value">The value of the property.</param>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Linked file.")]
        public void OnIsReadOnlyChanged(bool value)
        {
            IsReadOnly = value;
            if (!value)
            {
                IsPressed = false;
                IsMouseOver = false;
                IsFocused = false;
            }

            UpdateVisualState(true);
        }

        /// <summary>
        ///     Update the visual state of the control when its template is changed.
        /// </summary>
        public void OnApplyTemplateBase()
        {
            UpdateVisualState(false);
        }

        #region GotFocus

        /// <summary>
        ///     Check if the control's GotFocus event should be handled.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>
        ///     A value indicating whether the event should be handled.
        /// </returns>
        public bool AllowGotFocus(RoutedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            bool enabled = Control.IsEnabled;
            if (enabled)
            {
                IsFocused = true;
            }
            return enabled;
        }

        /// <summary>
        ///     Base implementation of the virtual GotFocus event handler.
        /// </summary>
        public void OnGotFocusBase()
        {
            UpdateVisualState(true);
        }

        #endregion GotFocus

        #region LostFocus

        /// <summary>
        ///     Check if the control's LostFocus event should be handled.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>
        ///     A value indicating whether the event should be handled.
        /// </returns>
        public bool AllowLostFocus(RoutedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            bool enabled = Control.IsEnabled;
            if (enabled)
            {
                IsFocused = false;
            }
            return enabled;
        }

        /// <summary>
        ///     Base implementation of the virtual LostFocus event handler.
        /// </summary>
        public void OnLostFocusBase()
        {
            IsPressed = false;
            UpdateVisualState(true);
        }

        #endregion LostFocus

        #region MouseEnter

        /// <summary>
        ///     Check if the control's MouseEnter event should be handled.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>
        ///     A value indicating whether the event should be handled.
        /// </returns>
        public bool AllowMouseEnter(MouseEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            bool enabled = Control.IsEnabled;
            if (enabled)
            {
                IsMouseOver = true;
            }
            return enabled;
        }

        /// <summary>
        ///     Base implementation of the virtual MouseEnter event handler.
        /// </summary>
        public void OnMouseEnterBase()
        {
            UpdateVisualState(true);
        }

        #endregion MouseEnter

        #region MouseLeave

        /// <summary>
        ///     Check if the control's MouseLeave event should be handled.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>
        ///     A value indicating whether the event should be handled.
        /// </returns>
        public bool AllowMouseLeave(MouseEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            bool enabled = Control.IsEnabled;
            if (enabled)
            {
                IsMouseOver = false;
            }
            return enabled;
        }

        /// <summary>
        ///     Base implementation of the virtual MouseLeave event handler.
        /// </summary>
        public void OnMouseLeaveBase()
        {
            UpdateVisualState(true);
        }

        #endregion MouseLeave

        #region MouseLeftButtonDown

        /// <summary>
        ///     Check if the control's MouseLeftButtonDown event should be handled.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>
        ///     A value indicating whether the event should be handled.
        /// </returns>
        public bool AllowMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            bool enabled = Control.IsEnabled;
            if (enabled)
            {
                // Get the current position and time
                DateTime now = DateTime.UtcNow;
                Point position = e.GetPosition(Control);

                // Compute the deltas from the last click
                double timeDelta = (now - LastClickTime).TotalMilliseconds;
                Point lastPosition = LastClickPosition;
                double dx = position.X - lastPosition.X;
                double dy = position.Y - lastPosition.Y;
                double distance = dx * dx + dy * dy;

                // Check if the values fall under the sequential click temporal
                // and spatial thresholds
                if (timeDelta < SequentialClickThresholdInMilliseconds &&
                    distance < SequentialClickThresholdInPixelsSquared)
                {
                    // TODO: Does each click have to be within the single time
                    // threshold on WPF?
                    ClickCount++;
                }
                else
                {
                    ClickCount = 1;
                }

                // Set the new position and time
                LastClickTime = now;
                LastClickPosition = position;

                // Raise the event
                IsPressed = true;
            }
            else
            {
                ClickCount = 1;
            }

            return enabled;
        }

        /// <summary>
        ///     Base implementation of the virtual MouseLeftButtonDown event
        ///     handler.
        /// </summary>
        public void OnMouseLeftButtonDownBase()
        {
            UpdateVisualState(true);
        }

        #endregion MouseLeftButtonDown

        #region MouseLeftButtonUp

        /// <summary>
        ///     Check if the control's MouseLeftButtonUp event should be handled.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>
        ///     A value indicating whether the event should be handled.
        /// </returns>
        public bool AllowMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            bool enabled = Control.IsEnabled;
            if (enabled)
            {
                IsPressed = false;
            }
            return enabled;
        }

        /// <summary>
        ///     Base implementation of the virtual MouseLeftButtonUp event handler.
        /// </summary>
        public void OnMouseLeftButtonUpBase()
        {
            UpdateVisualState(true);
        }

        #endregion MouseLeftButtonUp

        #region KeyDown

        /// <summary>
        ///     Check if the control's KeyDown event should be handled.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>
        ///     A value indicating whether the event should be handled.
        /// </returns>
        public bool AllowKeyDown(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            return Control.IsEnabled;
        }

        #endregion KeyDown

        #region KeyUp

        /// <summary>
        ///     Check if the control's KeyUp event should be handled.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        /// <returns>
        ///     A value indicating whether the event should be handled.
        /// </returns>
        public bool AllowKeyUp(KeyEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }

            return Control.IsEnabled;
        }

        #endregion KeyUp
    }

    /// <summary>
    ///     Implements a weak event listener that allows the owner to be garbage
    ///     collected if its only remaining link is an event handler.
    /// </summary>
    /// <typeparam name="TInstance">Type of instance listening for the event.</typeparam>
    /// <typeparam name="TSource">Type of source for the event.</typeparam>
    /// <typeparam name="TEventArgs">Type of event arguments for the event.</typeparam>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "Used as link target in several projects.")]
    internal class WeakEventListener<TInstance, TSource, TEventArgs> where TInstance : class
    {
        /// <summary>
        ///     WeakReference to the instance listening for the event.
        /// </summary>
        private readonly WeakReference _weakInstance;

        /// <summary>
        ///     Gets or sets the method to call when the event fires.
        /// </summary>
        public Action<TInstance, TSource, TEventArgs> OnEventAction { get; set; }

        /// <summary>
        ///     Gets or sets the method to call when detaching from the event.
        /// </summary>
        public Action<WeakEventListener<TInstance, TSource, TEventArgs>> OnDetachAction { get; set; }

        /// <summary>
        ///     Initializes a new instances of the WeakEventListener class.
        /// </summary>
        /// <param name="instance">Instance subscribing to the event.</param>
        public WeakEventListener(TInstance instance)
        {
            if (null == instance)
            {
                throw new ArgumentNullException("instance");
            }
            _weakInstance = new WeakReference(instance);
        }

        /// <summary>
        ///     Handler for the subscribed event calls OnEventAction to handle it.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="eventArgs">Event arguments.</param>
        public void OnEvent(TSource source, TEventArgs eventArgs)
        {
            var target = (TInstance)_weakInstance.Target;
            if (null != target)
            {
                // Call registered action
                if (null != OnEventAction)
                {
                    OnEventAction(target, source, eventArgs);
                }
            }
            else
            {
                // Detach from event
                Detach();
            }
        }

        /// <summary>
        ///     Detaches from the subscribed event.
        /// </summary>
        public void Detach()
        {
            if (null != OnDetachAction)
            {
                OnDetachAction(this);
                OnDetachAction = null;
            }
        }
    }

    /// <summary>
    ///     A framework element that permits a binding to be evaluated in a new data
    ///     context leaf node.
    /// </summary>
    /// <typeparam name="T">The type of dynamic binding to return.</typeparam>
    internal class BindingEvaluator<T> : FrameworkElement
    {
        /// <summary>
        ///     Gets or sets the string value binding used by the control.
        /// </summary>
        private Binding _binding;

        #region public T Value

        /// <summary>
        ///     Gets or sets the data item string value.
        /// </summary>
        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        ///     Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(T),
                typeof(BindingEvaluator<T>),
                new PropertyMetadata(default(T)));

        #endregion public string Value

        /// <summary>
        ///     Gets or sets the value binding.
        /// </summary>
        public Binding ValueBinding
        {
            get { return _binding; }
            set
            {
                _binding = value;
                SetBinding(ValueProperty, _binding);
            }
        }

        /// <summary>
        ///     Initializes a new instance of the BindingEvaluator class.
        /// </summary>
        public BindingEvaluator()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the BindingEvaluator class,
        ///     setting the initial binding to the provided parameter.
        /// </summary>
        /// <param name="binding">The initial string value binding.</param>
        public BindingEvaluator(Binding binding)
        {
            SetBinding(ValueProperty, binding);
        }

        /// <summary>
        ///     Clears the data context so that the control does not keep a
        ///     reference to the last-looked up item.
        /// </summary>
        public void ClearDataContext()
        {
            DataContext = null;
        }

        /// <summary>
        ///     Updates the data context of the framework element and returns the
        ///     updated binding value.
        /// </summary>
        /// <param name="o">The object to use as the data context.</param>
        /// <param name="clearDataContext">
        ///     If set to true, this parameter will
        ///     clear the data context immediately after retrieving the value.
        /// </param>
        /// <returns>
        ///     Returns the evaluated T value of the bound dependency
        ///     property.
        /// </returns>
        public T GetDynamicValue(object o, bool clearDataContext)
        {
            DataContext = o;
            T value = Value;
            if (clearDataContext)
            {
                DataContext = null;
            }
            return value;
        }

        /// <summary>
        ///     Updates the data context of the framework element and returns the
        ///     updated binding value.
        /// </summary>
        /// <param name="o">The object to use as the data context.</param>
        /// <returns>
        ///     Returns the evaluated T value of the bound dependency
        ///     property.
        /// </returns>
        public T GetDynamicValue(object o)
        {
            DataContext = o;
            return Value;
        }
    }

    /// <summary>
    ///     Exposes AutoCompleteBox types to UI Automation.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    public sealed class AutoCompleteBoxAutomationPeer : FrameworkElementAutomationPeer, IValueProvider,
        IExpandCollapseProvider, ISelectionProvider
    {
        /// <summary>
        ///     The name reported as the core class name.
        /// </summary>
        private const string AutoCompleteBoxClassNameCore = "AutoCompleteBox";

        /// <summary>
        ///     Gets the AutoCompleteBox that owns this
        ///     AutoCompleteBoxAutomationPeer.
        /// </summary>
        private VBAutoCompleteBox OwnerAutoCompleteBox
        {
            get { return (VBAutoCompleteBox)Owner; }
        }

        /// <summary>
        ///     Gets a value indicating whether the UI automation provider allows
        ///     more than one child element to be selected concurrently.
        /// </summary>
        /// <remarks>
        ///     This API supports the .NET Framework infrastructure and is not
        ///     intended to be used directly from your code.
        /// </remarks>
        /// <value>True if multiple selection is allowed; otherwise, false.</value>
        bool ISelectionProvider.CanSelectMultiple
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets a value indicating whether the UI automation provider
        ///     requires at least one child element to be selected.
        /// </summary>
        /// <remarks>
        ///     This API supports the .NET Framework infrastructure and is not
        ///     intended to be used directly from your code.
        /// </remarks>
        /// <value>True if selection is required; otherwise, false.</value>
        bool ISelectionProvider.IsSelectionRequired
        {
            get { return false; }
        }

        /// <summary>
        ///     Initializes a new instance of the AutoCompleteBoxAutomationPeer
        ///     class.
        /// </summary>
        /// <param name="owner">
        ///     The AutoCompleteBox that is associated with this
        ///     AutoCompleteBoxAutomationPeer.
        /// </param>
        public AutoCompleteBoxAutomationPeer(VBAutoCompleteBox owner)
            : base(owner)
        {
        }

        /// <summary>
        ///     Gets the control type for the AutoCompleteBox that is associated
        ///     with this AutoCompleteBoxAutomationPeer. This method is called by
        ///     GetAutomationControlType.
        /// </summary>
        /// <returns>ComboBox AutomationControlType.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ComboBox;
        }

        /// <summary>
        ///     Gets the name of the AutoCompleteBox that is associated with this
        ///     AutoCompleteBoxAutomationPeer. This method is called by
        ///     GetClassName.
        /// </summary>
        /// <returns>The name AutoCompleteBox.</returns>
        protected override string GetClassNameCore()
        {
            return AutoCompleteBoxClassNameCore;
        }

        /// <summary>
        ///     Gets the control pattern for the AutoCompleteBox that is associated
        ///     with this AutoCompleteBoxAutomationPeer.
        /// </summary>
        /// <param name="patternInterface">The desired PatternInterface.</param>
        /// <returns>The desired AutomationPeer or null.</returns>
        public override object GetPattern(PatternInterface patternInterface)
        {
            object iface = null;
            VBAutoCompleteBox owner = OwnerAutoCompleteBox;

            if (patternInterface == PatternInterface.Value)
            {
                iface = this;
            }
            else if (patternInterface == PatternInterface.ExpandCollapse)
            {
                iface = this;
            }
            else if (owner.SelectionAdapter != null)
            {
                AutomationPeer peer = owner.SelectionAdapter.CreateAutomationPeer();
                if (peer != null)
                {
                    iface = peer.GetPattern(patternInterface);
                }
            }

            if (iface == null)
            {
                iface = base.GetPattern(patternInterface);
            }

            return iface;
        }

        #region ExpandCollapse

        /// <summary>
        ///     Blocking method that returns after the element has been expanded.
        /// </summary>
        /// <remarks>
        ///     This API supports the .NET Framework infrastructure and is not
        ///     intended to be used directly from your code.
        /// </remarks>
        void IExpandCollapseProvider.Expand()
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            OwnerAutoCompleteBox.IsDropDownOpen = true;
        }

        /// <summary>
        ///     Blocking method that returns after the element has been collapsed.
        /// </summary>
        /// <remarks>
        ///     This API supports the .NET Framework infrastructure and is not
        ///     intended to be used directly from your code.
        /// </remarks>
        void IExpandCollapseProvider.Collapse()
        {
            if (!IsEnabled())
            {
                throw new ElementNotEnabledException();
            }

            OwnerAutoCompleteBox.IsDropDownOpen = false;
        }

        /// <summary>
        ///     Gets an element's current Collapsed or Expanded state.
        /// </summary>
        /// <remarks>
        ///     This API supports the .NET Framework infrastructure and is not
        ///     intended to be used directly from your code.
        /// </remarks>
        ExpandCollapseState IExpandCollapseProvider.ExpandCollapseState
        {
            get
            {
                return OwnerAutoCompleteBox.IsDropDownOpen ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;
            }
        }

        /// <summary>
        ///     Raises the ExpandCollapse automation event.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        internal void RaiseExpandCollapseAutomationEvent(bool oldValue, bool newValue)
        {
            RaisePropertyChangedEvent(
                ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
                newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed);
        }

        #endregion ExpandCollapse

        #region ValueProvider

        /// <summary>
        ///     Sets the value of a control.
        /// </summary>
        /// <param name="value">
        ///     The value to set. The provider is responsible
        ///     for converting the value to the appropriate data type.
        /// </param>
        void IValueProvider.SetValue(string value)
        {
            OwnerAutoCompleteBox.Text = value;
        }

        /// <summary>
        ///     Gets a value indicating whether the value of a control is
        ///     read-only.
        /// </summary>
        /// <value>True if the value is read-only; false if it can be modified.</value>
        bool IValueProvider.IsReadOnly
        {
            get { return !OwnerAutoCompleteBox.IsEnabled; }
        }

        /// <summary>
        ///     Gets the value of the control.
        /// </summary>
        /// <value>The value of the control.</value>
        string IValueProvider.Value
        {
            get { return OwnerAutoCompleteBox.Text ?? string.Empty; }
        }

        #endregion

        /// <summary>
        ///     Gets the collection of child elements of the AutoCompleteBox that
        ///     are associated with this AutoCompleteBoxAutomationPeer. This method
        ///     is called by GetChildren.
        /// </summary>
        /// <returns>
        ///     A collection of automation peer elements, or an empty collection
        ///     if there are no child elements.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Required by automation")
        ]
        protected override List<AutomationPeer> GetChildrenCore()
        {
            var children = new List<AutomationPeer>();
            VBAutoCompleteBox owner = OwnerAutoCompleteBox;

            // TextBox part.
            TextBox textBox = owner.TextBox;
            if (textBox != null)
            {
                AutomationPeer peer = CreatePeerForElement(textBox);
                if (peer != null)
                {
                    children.Insert(0, peer);
                }
            }

            // Include SelectionAdapter's children.
            if (owner.SelectionAdapter != null)
            {
                AutomationPeer selectionAdapterPeer = owner.SelectionAdapter.CreateAutomationPeer();
                if (selectionAdapterPeer != null)
                {
                    List<AutomationPeer> listChildren = selectionAdapterPeer.GetChildren();
                    if (listChildren != null)
                    {
                        foreach (AutomationPeer child in listChildren)
                        {
                            children.Add(child);
                        }
                    }
                }
            }

            return children;
        }

        /// <summary>
        ///     Retrieves a UI automation provider for each child element that is
        ///     selected.
        /// </summary>
        /// <returns>An array of UI automation providers.</returns>
        /// <remarks>
        ///     This API supports the .NET Framework infrastructure and is not
        ///     intended to be used directly from your code.
        /// </remarks>
        IRawElementProviderSimple[] ISelectionProvider.GetSelection()
        {
            if (OwnerAutoCompleteBox.SelectionAdapter != null)
            {
                object selectedItem = OwnerAutoCompleteBox.SelectionAdapter.SelectedItem;
                if (selectedItem != null)
                {
                    var uie = selectedItem as UIElement;
                    if (uie != null)
                    {
                        AutomationPeer peer = CreatePeerForElement(uie);
                        if (peer != null)
                        {
                            return new[] { ProviderFromPeer(peer) };
                        }
                    }
                }
            }

            return new IRawElementProviderSimple[] { };
        }
    }

    /// <summary>
    ///     A predefined set of filter functions for the known, built-in
    ///     AutoCompleteFilterMode enumeration values.
    /// </summary>
    internal static class AutoCompleteSearch
    {
        public static bool Contains(string source, string value, StringComparison comparison)
        {
            return source.IndexOf(value, comparison) >= 0;
        }

        /// <summary>
        ///     Index function that retrieves the filter for the provided
        ///     AutoCompleteFilterMode.
        /// </summary>
        /// <param name="FilterMode">The built-in search mode.</param>
        /// <returns>Returns the string-based comparison function.</returns>
        public static AutoCompleteFilterPredicate<string> GetFilter(AutoCompleteFilterMode FilterMode)
        {
            switch (FilterMode)
            {
                case AutoCompleteFilterMode.Contains:
                    return Contains;

                case AutoCompleteFilterMode.ContainsCaseSensitive:
                    return ContainsCaseSensitive;

                case AutoCompleteFilterMode.ContainsOrdinal:
                    return ContainsOrdinal;

                case AutoCompleteFilterMode.ContainsOrdinalCaseSensitive:
                    return ContainsOrdinalCaseSensitive;

                case AutoCompleteFilterMode.Equals:
                    return Equals;

                case AutoCompleteFilterMode.EqualsCaseSensitive:
                    return EqualsCaseSensitive;

                case AutoCompleteFilterMode.EqualsOrdinal:
                    return EqualsOrdinal;

                case AutoCompleteFilterMode.EqualsOrdinalCaseSensitive:
                    return EqualsOrdinalCaseSensitive;

                case AutoCompleteFilterMode.StartsWith:
                    return StartsWith;

                case AutoCompleteFilterMode.StartsWithCaseSensitive:
                    return StartsWithCaseSensitive;

                case AutoCompleteFilterMode.StartsWithOrdinal:
                    return StartsWithOrdinal;

                case AutoCompleteFilterMode.StartsWithOrdinalCaseSensitive:
                    return StartsWithOrdinalCaseSensitive;

                case AutoCompleteFilterMode.None:
                case AutoCompleteFilterMode.Custom:
                default:
                    return null;
            }
        }

        /// <summary>
        ///     Check if the string value begins with the text.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool StartsWith(string text, string value)
        {
            return value.StartsWith(text, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        ///     Check if the string value begins with the text.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool StartsWithCaseSensitive(string text, string value)
        {
            return value.StartsWith(text, StringComparison.CurrentCulture);
        }

        /// <summary>
        ///     Check if the string value begins with the text.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool StartsWithOrdinal(string text, string value)
        {
            return value.StartsWith(text, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Check if the string value begins with the text.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool StartsWithOrdinalCaseSensitive(string text, string value)
        {
            return value.StartsWith(text, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Check if the prefix is contained in the string value. The current
        ///     culture's case insensitive string comparison operator is used.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool Contains(string text, string value)
        {
            return Contains(value, text, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        ///     Check if the prefix is contained in the string value.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool ContainsCaseSensitive(string text, string value)
        {
            return Contains(value, text, StringComparison.CurrentCulture);
        }

        /// <summary>
        ///     Check if the prefix is contained in the string value.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool ContainsOrdinal(string text, string value)
        {
            return Contains(value, text, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Check if the prefix is contained in the string value.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool ContainsOrdinalCaseSensitive(string text, string value)
        {
            return Contains(value, text, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Check if the string values are equal.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool Equals(string text, string value)
        {
            return value.Equals(text, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        ///     Check if the string values are equal.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool EqualsCaseSensitive(string text, string value)
        {
            return value.Equals(text, StringComparison.CurrentCulture);
        }

        /// <summary>
        ///     Check if the string values are equal.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool EqualsOrdinal(string text, string value)
        {
            return value.Equals(text, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///     Check if the string values are equal.
        /// </summary>
        /// <param name="text">The AutoCompleteBox prefix text.</param>
        /// <param name="value">The item's string value.</param>
        /// <returns>Returns true if the condition is met.</returns>
        public static bool EqualsOrdinalCaseSensitive(string text, string value)
        {
            return value.Equals(text, StringComparison.Ordinal);
        }
    }

    // When adding to this enum, please update the OnFilterModePropertyChanged
    // in the AutoCompleteBox class that is used for validation.
    /// <summary>
    ///     Specifies how text in the text box portion of the
    ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control is used
    ///     to filter items specified by the
    ///     <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemsSource" />
    ///     property for display in the drop-down.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    public enum AutoCompleteFilterMode
    {
        /// <summary>
        ///     Specifies that no filter is used. All items are returned.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Specifies a culture-sensitive, case-insensitive filter where the
        ///     returned items start with the specified text. The filter uses the
        ///     <see cref="M:System.String.StartsWith(System.String,System.StringComparison)" />
        ///     method, specifying
        ///     <see cref="P:System.StringComparer.CurrentCultureIgnoreCase" /> as
        ///     the string comparison criteria.
        /// </summary>
        StartsWith = 1,

        /// <summary>
        ///     Specifies a culture-sensitive, case-sensitive filter where the
        ///     returned items start with the specified text. The filter uses the
        ///     <see cref="M:System.String.StartsWith(System.String,System.StringComparison)" />
        ///     method, specifying
        ///     <see cref="P:System.StringComparer.CurrentCulture" /> as the string
        ///     comparison criteria.
        /// </summary>
        StartsWithCaseSensitive = 2,

        /// <summary>
        ///     Specifies an ordinal, case-insensitive filter where the returned
        ///     items start with the specified text. The filter uses the
        ///     <see cref="M:System.String.StartsWith(System.String,System.StringComparison)" />
        ///     method, specifying
        ///     <see cref="P:System.StringComparer.OrdinalIgnoreCase" /> as the
        ///     string comparison criteria.
        /// </summary>
        StartsWithOrdinal = 3,

        /// <summary>
        ///     Specifies an ordinal, case-sensitive filter where the returned items
        ///     start with the specified text. The filter uses the
        ///     <see cref="M:System.String.StartsWith(System.String,System.StringComparison)" />
        ///     method, specifying <see cref="P:System.StringComparer.Ordinal" /> as
        ///     the string comparison criteria.
        /// </summary>
        StartsWithOrdinalCaseSensitive = 4,

        /// <summary>
        ///     Specifies a culture-sensitive, case-insensitive filter where the
        ///     returned items contain the specified text.
        /// </summary>
        Contains = 5,

        /// <summary>
        ///     Specifies a culture-sensitive, case-sensitive filter where the
        ///     returned items contain the specified text.
        /// </summary>
        ContainsCaseSensitive = 6,

        /// <summary>
        ///     Specifies an ordinal, case-insensitive filter where the returned
        ///     items contain the specified text.
        /// </summary>
        ContainsOrdinal = 7,

        /// <summary>
        ///     Specifies an ordinal, case-sensitive filter where the returned items
        ///     contain the specified text.
        /// </summary>
        ContainsOrdinalCaseSensitive = 8,

        /// <summary>
        ///     Specifies a culture-sensitive, case-insensitive filter where the
        ///     returned items equal the specified text. The filter uses the
        ///     <see cref="M:System.String.Equals(System.String,System.StringComparison)" />
        ///     method, specifying
        ///     <see cref="P:System.StringComparer.CurrentCultureIgnoreCase" /> as
        ///     the search comparison criteria.
        /// </summary>
        Equals = 9,

        /// <summary>
        ///     Specifies a culture-sensitive, case-sensitive filter where the
        ///     returned items equal the specified text. The filter uses the
        ///     <see cref="M:System.String.Equals(System.String,System.StringComparison)" />
        ///     method, specifying
        ///     <see cref="P:System.StringComparer.CurrentCulture" /> as the string
        ///     comparison criteria.
        /// </summary>
        EqualsCaseSensitive = 10,

        /// <summary>
        ///     Specifies an ordinal, case-insensitive filter where the returned
        ///     items equal the specified text. The filter uses the
        ///     <see cref="M:System.String.Equals(System.String,System.StringComparison)" />
        ///     method, specifying
        ///     <see cref="P:System.StringComparer.OrdinalIgnoreCase" /> as the
        ///     string comparison criteria.
        /// </summary>
        EqualsOrdinal = 11,

        /// <summary>
        ///     Specifies an ordinal, case-sensitive filter where the returned items
        ///     equal the specified text. The filter uses the
        ///     <see cref="M:System.String.Equals(System.String,System.StringComparison)" />
        ///     method, specifying <see cref="P:System.StringComparer.Ordinal" /> as
        ///     the string comparison criteria.
        /// </summary>
        EqualsOrdinalCaseSensitive = 12,

        /// <summary>
        ///     Specifies that a custom filter is used. This mode is used when the
        ///     <see cref="P:System.Windows.Controls.AutoCompleteBox.TextFilter" />
        ///     or
        ///     <see cref="P:System.Windows.Controls.AutoCompleteBox.ItemFilter" />
        ///     properties are set.
        /// </summary>
        Custom = 13,
    }

    /// <summary>
    ///     Represents the filter used by the
    ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control to
    ///     determine whether an item is a possible match for the specified text.
    /// </summary>
    /// <returns>
    ///     true to indicate <paramref name="item" /> is a possible match
    ///     for <paramref name="search" />; otherwise false.
    /// </returns>
    /// <param name="search">The string used as the basis for filtering.</param>
    /// <param name="item">
    ///     The item that is compared with the
    ///     <paramref name="search" /> parameter.
    /// </param>
    /// <typeparam name="T">
    ///     The type used for filtering the
    ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" />. This type can
    ///     be either a string or an object.
    /// </typeparam>
    /// <QualityBand>Stable</QualityBand>
    public delegate bool AutoCompleteFilterPredicate<T>(string search, T item);

    /// <summary>
    ///     Provides data for the
    ///     <see cref="E:System.Windows.Controls.AutoCompleteBox.Populated" />
    ///     event.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    public class PopulatedEventArgs : RoutedEventArgs
    {
        /// <summary>
        ///     Gets the list of possible matches added to the drop-down portion of
        ///     the <see cref="T:System.Windows.Controls.AutoCompleteBox" />
        ///     control.
        /// </summary>
        /// <value>
        ///     The list of possible matches added to the
        ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" />.
        /// </value>
        public IEnumerable Data { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:System.Windows.Controls.PopulatedEventArgs" />.
        /// </summary>
        /// <param name="data">
        ///     The list of possible matches added to the
        ///     drop-down portion of the
        ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </param>
        public PopulatedEventArgs(IEnumerable data)
        {
            Data = data;
        }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:System.Windows.Controls.PopulatedEventArgs" />.
        /// </summary>
        /// <param name="data">
        ///     The list of possible matches added to the
        ///     drop-down portion of the
        ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </param>
        /// <param name="routedEvent">The routed event identifier for this instance.</param>
        public PopulatedEventArgs(IEnumerable data, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Data = data;
        }
    }

    /// <summary>
    ///     Represents the method that will handle the
    ///     <see cref="E:System.Windows.Controls.AutoCompleteBox.Populated" />
    ///     event of a <see cref="T:System.Windows.Controls.AutoCompleteBox" />
    ///     control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///     A
    ///     <see cref="T:System.Windows.Controls.PopulatedEventArgs" /> that
    ///     contains the event data.
    /// </param>
    /// <QualityBand>Stable</QualityBand>
    [SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances",
        Justification = "There is no generic RoutedEventHandler.")]
    public delegate void PopulatedEventHandler(object sender, PopulatedEventArgs e);

    /// <summary>
    ///     Provides data for the
    ///     <see cref="E:System.Windows.Controls.AutoCompleteBox.Populating" />
    ///     event.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    public class PopulatingEventArgs : RoutedEventArgs
    {
        /// <summary>
        ///     Gets the text that is used to determine which items to display in
        ///     the <see cref="T:System.Windows.Controls.AutoCompleteBox" />
        ///     control.
        /// </summary>
        /// <value>
        ///     The text that is used to determine which items to display in
        ///     the <see cref="T:System.Windows.Controls.AutoCompleteBox" />.
        /// </value>
        public string Parameter { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the
        ///     <see cref="E:System.Windows.Controls.AutoCompleteBox.Populating" />
        ///     event should be canceled.
        /// </summary>
        /// <value>
        ///     True to cancel the event, otherwise false. The default is
        ///     false.
        /// </value>
        public bool Cancel { get; set; }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:System.Windows.Controls.PopulatingEventArgs" />.
        /// </summary>
        /// <param name="parameter">
        ///     The value of the
        ///     <see cref="P:System.Windows.Controls.AutoCompleteBox.SearchText" />
        ///     property, which is used to filter items for the
        ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </param>
        public PopulatingEventArgs(string parameter)
        {
            Parameter = parameter;
        }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:System.Windows.Controls.PopulatingEventArgs" />.
        /// </summary>
        /// <param name="parameter">
        ///     The value of the
        ///     <see cref="P:System.Windows.Controls.AutoCompleteBox.SearchText" />
        ///     property, which is used to filter items for the
        ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </param>
        /// <param name="routedEvent">The routed event identifier for this instance.</param>
        public PopulatingEventArgs(string parameter, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Parameter = parameter;
        }
    }

    /// <summary>
    ///     Represents the method that will handle the
    ///     <see cref="E:System.Windows.Controls.AutoCompleteBox.Populating" />
    ///     event of a <see cref="T:System.Windows.Controls.AutoCompleteBox" />
    ///     control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">
    ///     A
    ///     <see cref="T:System.Windows.Controls.PopulatingEventArgs" /> that
    ///     contains the event data.
    /// </param>
    /// <QualityBand>Stable</QualityBand>
    [SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances",
        Justification = "There is no generic RoutedEventHandler.")]
    public delegate void PopulatingEventHandler(object sender, PopulatingEventArgs e);

    /// <summary>
    ///     PopupHelper is a simple wrapper type that helps abstract platform
    ///     differences out of the Popup.
    /// </summary>
    internal class PopupHelper
    {
        /// <summary>
        ///     Gets a value indicating whether a visual popup state is being used
        ///     in the current template for the Closed state. Setting this value to
        ///     true will delay the actual setting of Popup.IsOpen to false until
        ///     after the visual state's transition for Closed is complete.
        /// </summary>
        public bool UsesClosingVisualState { get; private set; }

        /// <summary>
        ///     Gets or sets the parent control.
        /// </summary>
        private Control Parent { get; set; }

        /// <summary>
        ///     Gets or sets the maximum drop down height value.
        /// </summary>
        public double MaxDropDownHeight { get; set; }

        /// <summary>
        ///     Gets the Popup control instance.
        /// </summary>
        public Popup Popup { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the actual Popup is open.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Provided for completeness.")]
        public bool IsOpen
        {
            get { return Popup.IsOpen; }
            set { Popup.IsOpen = value; }
        }

        /// <summary>
        ///     Gets or sets the popup child framework element. Can be used if an
        ///     assumption is made on the child type.
        /// </summary>
        private FrameworkElement PopupChild { get; set; }

        /// <summary>
        ///     The Closed event is fired after the Popup closes.
        /// </summary>
        public event EventHandler Closed;

        /// <summary>
        ///     Fired when the popup children have a focus event change, allows the
        ///     parent control to update visual states or react to the focus state.
        /// </summary>
        public event EventHandler FocusChanged;

        /// <summary>
        ///     Fired when the popup children intercept an event that may indicate
        ///     the need for a visual state update by the parent control.
        /// </summary>
        public event EventHandler UpdateVisualStates;

        /// <summary>
        ///     Initializes a new instance of the PopupHelper class.
        /// </summary>
        /// <param name="parent">The parent control.</param>
        public PopupHelper(Control parent)
        {
            Debug.Assert(parent != null, "Parent should not be null.");
            Parent = parent;
        }

        /// <summary>
        ///     Initializes a new instance of the PopupHelper class.
        /// </summary>
        /// <param name="parent">The parent control.</param>
        /// <param name="popup">The Popup template part.</param>
        public PopupHelper(Control parent, Popup popup)
            : this(parent)
        {
            Popup = popup;
        }

        /// <summary>
        ///     Arrange the popup.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "This try-catch pattern is used by other popup controls to keep the runtime up.")]
        public void Arrange()
        {
            if (Popup == null
                || PopupChild == null
                || Application.Current == null
                || false)
            {
                return;
            }

            UIElement u = Parent;
            if (Application.Current.Windows.Count > 0)
            {
                // TODO: USE THE CURRENT WINDOW INSTEAD! WALK THE TREE!
                u = Application.Current.Windows[0];
            }
            while ((u as Window) == null && u != null)
            {
                u = VisualTreeHelper.GetParent(u) as UIElement;
            }
            var w = u as Window;
            if (w == null)
            {
                return;
            }

            double rootWidth = w.ActualWidth;
            double rootHeight = w.ActualHeight;
            double popupContentWidth = PopupChild.ActualWidth;
            double popupContentHeight = PopupChild.ActualHeight;

            if (rootHeight == 0 || rootWidth == 0 || popupContentWidth == 0 || popupContentHeight == 0)
            {
                return;
            }

            double rootOffsetX = 0;
            double rootOffsetY = 0;
            double myControlHeight = Parent.ActualHeight;
            double myControlWidth = Parent.ActualWidth;

            // Use or come up with a maximum popup height.
            double popupMaxHeight = MaxDropDownHeight;
            if (double.IsInfinity(popupMaxHeight) || double.IsNaN(popupMaxHeight))
            {
                popupMaxHeight = (rootHeight - myControlHeight) * 3 / 5;
            }

            popupContentWidth = Math.Min(popupContentWidth, rootWidth);
            popupContentHeight = Math.Min(popupContentHeight, popupMaxHeight);
            popupContentWidth = Math.Max(myControlWidth, popupContentWidth);

            // We prefer to align the popup box with the left edge of the 
            // control, if it will fit.
            double popupX = rootOffsetX;
            if (rootWidth < popupX + popupContentWidth)
            {
                // Since it doesn't fit when strictly left aligned, we shift it 
                // to the left until it does fit.
                popupX = rootWidth - popupContentWidth;
                popupX = Math.Max(0, popupX);
            }

            // We prefer to put the popup below the combobox if it will fit.
            bool below = true;
            double popupY = rootOffsetY + myControlHeight;
            if (rootHeight < popupY + popupContentHeight)
            {
                below = false;
                // It doesn't fit below the combobox, lets try putting it above 
                // the combobox.
                popupY = rootOffsetY - popupContentHeight;
                if (popupY < 0)
                {
                    // doesn't really fit below either.  Now we just pick top 
                    // or bottom based on wich area is bigger.
                    if (rootOffsetY < (rootHeight - myControlHeight) / 2)
                    {
                        below = true;
                        popupY = rootOffsetY + myControlHeight;
                    }
                    else
                    {
                        below = false;
                        popupY = rootOffsetY - popupContentHeight;
                    }
                }
            }

            // Now that we have positioned the popup we may need to truncate 
            // its size.
            popupMaxHeight = below
                ? Math.Min(rootHeight - popupY, popupMaxHeight)
                : Math.Min(rootOffsetY, popupMaxHeight);

            Popup.HorizontalOffset = 0;
            Popup.VerticalOffset = 0;
            PopupChild.MinWidth = myControlWidth;
            PopupChild.MaxWidth = rootWidth;
            PopupChild.MinHeight = 0;
            PopupChild.MaxHeight = Math.Max(0, popupMaxHeight);

            PopupChild.Width = popupContentWidth;
            // PopupChild.Height = popupContentHeight;
            PopupChild.HorizontalAlignment = HorizontalAlignment.Left;
            PopupChild.VerticalAlignment = VerticalAlignment.Top;

            // Set the top left corner for the actual drop down.
            Canvas.SetLeft(PopupChild, popupX - rootOffsetX);
            Canvas.SetTop(PopupChild, popupY - rootOffsetY);
        }

        /// <summary>
        ///     Fires the Closed event.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void OnClosed(EventArgs e)
        {
            EventHandler handler = Closed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///     Actually closes the popup after the VSM state animation completes.
        /// </summary>
        /// <param name="sender">Event source.</param>
        /// <param name="e">Event arguments.</param>
        private void OnPopupClosedStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            // Delayed closing of the popup until now
            if (e != null && e.NewState != null && e.NewState.Name == VisualStates.StatePopupClosed)
            {
                if (Popup != null)
                {
                    Popup.IsOpen = false;
                }
                OnClosed(EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Should be called by the parent control before the base
        ///     OnApplyTemplate method is called.
        /// </summary>
        public void BeforeOnApplyTemplate()
        {
            if (UsesClosingVisualState)
            {
                // Unhook the event handler for the popup closed visual state group.
                // This code is used to enable visual state transitions before 
                // actually setting the underlying Popup.IsOpen property to false.
                VisualStateGroup groupPopupClosed = VisualStates.TryGetVisualStateGroup(Parent, VisualStates.GroupPopup);
                if (null != groupPopupClosed)
                {
                    groupPopupClosed.CurrentStateChanged -= OnPopupClosedStateChanged;
                    UsesClosingVisualState = false;
                }
            }

            if (Popup != null)
            {
                Popup.Closed -= Popup_Closed;
            }
        }

        /// <summary>
        ///     Should be called by the parent control after the base
        ///     OnApplyTemplate method is called.
        /// </summary>
        public void AfterOnApplyTemplate()
        {
            if (Popup != null)
            {
                Popup.Closed += Popup_Closed;
            }

            VisualStateGroup groupPopupClosed = VisualStates.TryGetVisualStateGroup(Parent, VisualStates.GroupPopup);
            if (null != groupPopupClosed)
            {
                groupPopupClosed.CurrentStateChanged += OnPopupClosedStateChanged;
                UsesClosingVisualState = true;
            }

            // TODO: Consider moving to the DropDownPopup setter
            // TODO: Although in line with other implementations, what happens 
            // when the template is swapped out?
            if (Popup != null)
            {
                PopupChild = Popup.Child as FrameworkElement;

                if (PopupChild != null)
                {
                    PopupChild.GotFocus += PopupChild_GotFocus;
                    PopupChild.LostFocus += PopupChild_LostFocus;
                    PopupChild.MouseEnter += PopupChild_MouseEnter;
                    PopupChild.MouseLeave += PopupChild_MouseLeave;
                    PopupChild.SizeChanged += PopupChild_SizeChanged;
                }
            }
        }

        /// <summary>
        ///     The size of the popup child has changed.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void PopupChild_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Arrange();
        }

        /// <summary>
        ///     The mouse has clicked outside of the popup.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OutsidePopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Popup != null)
            {
                Popup.IsOpen = false;
            }
        }

        /// <summary>
        ///     Connected to the Popup Closed event and fires the Closed event.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void Popup_Closed(object sender, EventArgs e)
        {
            OnClosed(EventArgs.Empty);
        }

        /// <summary>
        ///     Connected to several events that indicate that the FocusChanged
        ///     event should bubble up to the parent control.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void OnFocusChanged(EventArgs e)
        {
            EventHandler handler = FocusChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///     Fires the UpdateVisualStates event.
        /// </summary>
        /// <param name="e">The event data.</param>
        private void OnUpdateVisualStates(EventArgs e)
        {
            EventHandler handler = UpdateVisualStates;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        ///     The popup child has received focus.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void PopupChild_GotFocus(object sender, RoutedEventArgs e)
        {
            OnFocusChanged(EventArgs.Empty);
        }

        /// <summary>
        ///     The popup child has lost focus.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void PopupChild_LostFocus(object sender, RoutedEventArgs e)
        {
            OnFocusChanged(EventArgs.Empty);
        }

        /// <summary>
        ///     The popup child has had the mouse enter its bounds.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void PopupChild_MouseEnter(object sender, MouseEventArgs e)
        {
            OnUpdateVisualStates(EventArgs.Empty);
        }

        /// <summary>
        ///     The mouse has left the popup child's bounds.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void PopupChild_MouseLeave(object sender, MouseEventArgs e)
        {
            OnUpdateVisualStates(EventArgs.Empty);
        }
    }

    /// <summary>
    ///     Provides event data for various routed events that track property values
    ///     changing.  Typically the events denote a cancellable action.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the value for the dependency property that is changing.
    /// </typeparam>
    /// <QualityBand>Preview</QualityBand>
    public class RoutedPropertyChangingEventArgs<T> : RoutedEventArgs
    {
        /// <summary>
        ///     Gets the <see cref="T:System.Windows.DependencyProperty" />
        ///     identifier for the property that is changing.
        /// </summary>
        /// <value>
        ///     The <see cref="T:System.Windows.DependencyProperty" /> identifier
        ///     for the property that is changing.
        /// </value>
        public DependencyProperty Property { get; private set; }

        /// <summary>
        ///     Gets a value that reports the previous value of the changing
        ///     property.
        /// </summary>
        /// <value>
        ///     The previous value of the changing property.
        /// </value>
        public T OldValue { get; private set; }

        /// <summary>
        ///     Gets or sets a value that reports the new value of the changing
        ///     property, assuming that the property change is not cancelled.
        /// </summary>
        /// <value>
        ///     The new value of the changing property.
        /// </value>
        public T NewValue { get; set; }

        /// <summary>
        ///     Gets a value indicating whether the property change that originated
        ///     the RoutedPropertyChanging event is cancellable.
        /// </summary>
        /// <value>
        ///     True if the property change is cancellable. false if the property
        ///     change is not cancellable.
        /// </value>
        public bool IsCancelable { get; private set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the property change that
        ///     originated the RoutedPropertyChanging event should be cancelled.
        /// </summary>
        /// <value>
        ///     True to cancel the property change; this resets the property to
        ///     <see cref="P:System.Windows.Controls.RoutedPropertyChangingEventArgs`1.OldValue" />.
        ///     false to not cancel the property change; the value changes to
        ///     <see cref="P:System.Windows.Controls.RoutedPropertyChangingEventArgs`1.NewValue" />.
        /// </value>
        /// <exception cref="T:System.InvalidOperationException">
        ///     Attempted to cancel in an instance where
        ///     <see cref="P:System.Windows.Controls.RoutedPropertyChangingEventArgs`1.IsCancelable" />
        ///     is false.
        /// </exception>
        public bool Cancel
        {
            get { return _cancel; }
            set
            {
                if (IsCancelable)
                {
                    _cancel = value;
                }
                else if (value)
                {
                    throw new InvalidOperationException("Can not be canceled.");
                }
            }
        }

        /// <summary>
        ///     Private member variable for Cancel property.
        /// </summary>
        private bool _cancel;

        /// <summary>
        ///     Gets or sets a value indicating whether internal value coercion is
        ///     acting on the property change that originated the
        ///     RoutedPropertyChanging event.
        /// </summary>
        /// <value>
        ///     True if coercion is active. false if coercion is not active.
        /// </value>
        /// <remarks>
        ///     This is a total hack to work around the class hierarchy for Value
        ///     coercion in NumericUpDown.
        /// </remarks>
        public bool InCoercion { get; set; }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:System.Windows.Controls.RoutedPropertyChangingEventArgs`1" />
        ///     class.
        /// </summary>
        /// <param name="property">
        ///     The <see cref="T:System.Windows.DependencyProperty" /> identifier
        ///     for the property that is changing.
        /// </param>
        /// <param name="oldValue">The previous value of the property.</param>
        /// <param name="newValue">
        ///     The new value of the property, assuming that the property change is
        ///     not cancelled.
        /// </param>
        /// <param name="isCancelable">
        ///     True if the property change is cancellable by setting
        ///     <see cref="P:System.Windows.Controls.RoutedPropertyChangingEventArgs`1.Cancel" />
        ///     to true in event handling. false if the property change is not
        ///     cancellable.
        /// </param>
        public RoutedPropertyChangingEventArgs(
            DependencyProperty property,
            T oldValue,
            T newValue,
            bool isCancelable)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
            IsCancelable = isCancelable;
            Cancel = false;
        }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:System.Windows.Controls.RoutedPropertyChangingEventArgs`1" />
        ///     class.
        /// </summary>
        /// <param name="property">
        ///     The <see cref="T:System.Windows.DependencyProperty" /> identifier
        ///     for the property that is changing.
        /// </param>
        /// <param name="oldValue">The previous value of the property.</param>
        /// <param name="newValue">
        ///     The new value of the property, assuming that the property change is
        ///     not cancelled.
        /// </param>
        /// <param name="isCancelable">
        ///     True if the property change is cancellable by setting
        ///     <see cref="P:System.Windows.Controls.RoutedPropertyChangingEventArgs`1.Cancel" />
        ///     to true in event handling. false if the property change is not
        ///     cancellable.
        /// </param>
        /// <param name="routedEvent">The routed event identifier for this instance.</param>
        public RoutedPropertyChangingEventArgs(DependencyProperty property,
            T oldValue, T newValue, bool isCancelable, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
            IsCancelable = isCancelable;
            Cancel = false;
        }
    }

    /// <summary>
    ///     Represents methods that handle various routed events that track property
    ///     values changing.  Typically the events denote a cancellable action.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the value for the dependency property that is changing.
    /// </typeparam>
    /// <param name="sender">
    ///     The object where the initiating property is changing.
    /// </param>
    /// <param name="e">Event data for the event.</param>
    /// <QualityBand>Preview</QualityBand>
    [SuppressMessage("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances",
        Justification = "To match pattern of RoutedPropertyChangedEventHandler<T>")]
    public delegate void RoutedPropertyChangingEventHandler<T>(object sender, RoutedPropertyChangingEventArgs<T> e);

    /// <summary>
    ///     Represents the selection adapter contained in the drop-down portion of
    ///     an <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
    /// </summary>
    /// <QualityBand>Stable</QualityBand>
    public class SelectorSelectionAdapter : ISelectionAdapter
    {
        /// <summary>
        ///     The Selector instance.
        /// </summary>
        private Selector _selector;

        /// <summary>
        ///     Gets or sets a value indicating whether the selection change event
        ///     should not be fired.
        /// </summary>
        private bool IgnoringSelectionChanged { get; set; }

        /// <summary>
        ///     Gets or sets the underlying
        ///     <see cref="T:System.Windows.Controls.Primitives.Selector" />
        ///     control.
        /// </summary>
        /// <value>
        ///     The underlying
        ///     <see cref="T:System.Windows.Controls.Primitives.Selector" />
        ///     control.
        /// </value>
        public Selector SelectorControl
        {
            get { return _selector; }

            set
            {
                if (_selector != null)
                {
                    _selector.SelectionChanged -= OnSelectionChanged;
                    _selector.MouseLeftButtonUp -= OnSelectorMouseLeftButtonUp;
                }

                _selector = value;

                if (_selector != null)
                {
                    _selector.SelectionChanged += OnSelectionChanged;
                    _selector.MouseLeftButtonUp += OnSelectorMouseLeftButtonUp;
                }
            }
        }

        /// <summary>
        ///     Occurs when the
        ///     <see cref="P:System.Windows.Controls.SelectorSelectionAdapter.SelectedItem" />
        ///     property value changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged;

        /// <summary>
        ///     Occurs when an item is selected and is committed to the underlying
        ///     <see cref="T:System.Windows.Controls.Primitives.Selector" />
        ///     control.
        /// </summary>
        public event RoutedEventHandler Commit;

        /// <summary>
        ///     Occurs when a selection is canceled before it is committed.
        /// </summary>
        public event RoutedEventHandler Cancel;

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:System.Windows.Controls.SelectorSelectionAdapter" />
        ///     class.
        /// </summary>
        public SelectorSelectionAdapter()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="T:System.Windows.Controls.SelectorSelectionAdapter" />
        ///     class with the specified
        ///     <see cref="T:System.Windows.Controls.Primitives.Selector" />
        ///     control.
        /// </summary>
        /// <param name="selector">
        ///     The
        ///     <see cref="T:System.Windows.Controls.Primitives.Selector" /> control
        ///     to wrap as a
        ///     <see cref="T:System.Windows.Controls.SelectorSelectionAdapter" />.
        /// </param>
        public SelectorSelectionAdapter(Selector selector)
        {
            SelectorControl = selector;
        }

        /// <summary>
        ///     Gets or sets the selected item of the selection adapter.
        /// </summary>
        /// <value>The selected item of the underlying selection adapter.</value>
        public object SelectedItem
        {
            get { return SelectorControl == null ? null : SelectorControl.SelectedItem; }

            set
            {
                IgnoringSelectionChanged = true;
                if (SelectorControl != null)
                {
                    SelectorControl.SelectedItem = value;
                }

                // Attempt to reset the scroll viewer's position
                if (value == null)
                {
                    ResetScrollViewer();
                }

                IgnoringSelectionChanged = false;
            }
        }

        /// <summary>
        ///     Gets or sets a collection that is used to generate the content of
        ///     the selection adapter.
        /// </summary>
        /// <value>
        ///     The collection used to generate content for the selection
        ///     adapter.
        /// </value>
        public IEnumerable ItemsSource
        {
            get { return SelectorControl == null ? null : SelectorControl.ItemsSource; }
            set
            {
                if (SelectorControl != null)
                {
                    SelectorControl.ItemsSource = value;
                }
            }
        }

        /// <summary>
        ///     If the control contains a ScrollViewer, this will reset the viewer
        ///     to be scrolled to the top.
        /// </summary>
        private void ResetScrollViewer()
        {
            if (SelectorControl != null)
            {
                ScrollViewer sv = GetLogicalChildrenBreadthFirst(SelectorControl).OfType<ScrollViewer>().FirstOrDefault();
                if (sv != null)
                {
                    sv.ScrollToTop();
                }
            }
        }

        internal static IEnumerable<FrameworkElement> GetLogicalChildrenBreadthFirst(FrameworkElement parent)
        {
            Debug.Assert(parent != null, "The parent cannot be null.");

            var queue =
                new Queue<FrameworkElement>(GetVisualChildren(parent).OfType<FrameworkElement>());

            while (queue.Count > 0)
            {
                FrameworkElement element = queue.Dequeue();
                yield return element;

                foreach (FrameworkElement visualChild in GetVisualChildren(element).OfType<FrameworkElement>())
                {
                    queue.Enqueue(visualChild);
                }
            }
        }

        internal static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject parent)
        {
            Debug.Assert(parent != null, "The parent cannot be null.");

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int counter = 0; counter < childCount; counter++)
            {
                yield return VisualTreeHelper.GetChild(parent, counter);
            }
        }

        /// <summary>
        ///     Handles the mouse left button up event on the selector control.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnSelectorMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OnCommit();
        }

        /// <summary>
        ///     Handles the SelectionChanged event on the Selector control.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The selection changed event data.</param>
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoringSelectionChanged)
            {
                return;
            }

            SelectionChangedEventHandler handler = SelectionChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        /// <summary>
        ///     Increments the
        ///     <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedIndex" />
        ///     property of the underlying
        ///     <see cref="T:System.Windows.Controls.Primitives.Selector" />
        ///     control.
        /// </summary>
        protected void SelectedIndexIncrement()
        {
            if (SelectorControl != null)
            {
                SelectorControl.SelectedIndex = SelectorControl.SelectedIndex + 1 >= SelectorControl.Items.Count
                    ? -1
                    : SelectorControl.SelectedIndex + 1;
            }
        }

        /// <summary>
        ///     Decrements the
        ///     <see cref="P:System.Windows.Controls.Primitives.Selector.SelectedIndex" />
        ///     property of the underlying
        ///     <see cref="T:System.Windows.Controls.Primitives.Selector" />
        ///     control.
        /// </summary>
        protected void SelectedIndexDecrement()
        {
            if (SelectorControl != null)
            {
                int index = SelectorControl.SelectedIndex;
                if (index >= 0)
                {
                    SelectorControl.SelectedIndex--;
                }
                else if (index == -1)
                {
                    SelectorControl.SelectedIndex = SelectorControl.Items.Count - 1;
                }
            }
        }

        /// <summary>
        ///     Provides handling for the
        ///     <see cref="E:System.Windows.UIElement.KeyDown" /> event that occurs
        ///     when a key is pressed while the drop-down portion of the
        ///     <see cref="T:System.Windows.Controls.AutoCompleteBox" /> has focus.
        /// </summary>
        /// <param name="e">
        ///     A <see cref="T:System.Windows.Input.KeyEventArgs" />
        ///     that contains data about the
        ///     <see cref="E:System.Windows.UIElement.KeyDown" /> event.
        /// </param>
        public void HandleKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    OnCommit();
                    e.Handled = true;
                    break;

                case Key.Up:
                    SelectedIndexDecrement();
                    e.Handled = true;
                    break;

                case Key.Down:
                    if ((ModifierKeys.Alt & Keyboard.Modifiers) == ModifierKeys.None)
                    {
                        SelectedIndexIncrement();
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    OnCancel();
                    e.Handled = true;
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        ///     Raises the
        ///     <see cref="E:System.Windows.Controls.SelectorSelectionAdapter.Commit" />
        ///     event.
        /// </summary>
        protected virtual void OnCommit()
        {
            OnCommit(this, new RoutedEventArgs());
        }

        /// <summary>
        ///     Fires the Commit event.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnCommit(object sender, RoutedEventArgs e)
        {
            RoutedEventHandler handler = Commit;
            if (handler != null)
            {
                handler(sender, e);
            }

            AfterAdapterAction();
        }

        /// <summary>
        ///     Raises the
        ///     <see cref="E:System.Windows.Controls.SelectorSelectionAdapter.Cancel" />
        ///     event.
        /// </summary>
        protected virtual void OnCancel()
        {
            OnCancel(this, new RoutedEventArgs());
        }

        /// <summary>
        ///     Fires the Cancel event.
        /// </summary>
        /// <param name="sender">The source object.</param>
        /// <param name="e">The event data.</param>
        private void OnCancel(object sender, RoutedEventArgs e)
        {
            RoutedEventHandler handler = Cancel;
            if (handler != null)
            {
                handler(sender, e);
            }

            AfterAdapterAction();
        }

        /// <summary>
        ///     Change the selection after the actions are complete.
        /// </summary>
        private void AfterAdapterAction()
        {
            IgnoringSelectionChanged = true;
            if (SelectorControl != null)
            {
                SelectorControl.SelectedItem = null;
                SelectorControl.SelectedIndex = -1;
            }
            IgnoringSelectionChanged = false;
        }

        /// <summary>
        ///     Returns an automation peer for the underlying
        ///     <see cref="T:System.Windows.Controls.Primitives.Selector" />
        ///     control, for use by the Silverlight automation infrastructure.
        /// </summary>
        /// <returns>
        ///     An automation peer for use by the Silverlight automation
        ///     infrastructure.
        /// </returns>
        public AutomationPeer CreateAutomationPeer()
        {
            return _selector != null ? UIElementAutomationPeer.CreatePeerForElement(_selector) : null;
        }
    }

    public class VBAutoCompleteBoxItem
    {
        public VBAutoCompleteBoxItem(IACObject sourceItem, string propertyName)
        {
            SourceItem = sourceItem;
            CurrentItemCaption = sourceItem.ACUrlCommand(propertyName, null).ToString();
        }

        public string CurrentItemCaption
        {
            get;
            set;
        }

        public IACObject SourceItem
        {
            get;
            set;
        }
    }
}
