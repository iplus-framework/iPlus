// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Defines the different roles of GridViewColumnHeaders
    /// </summary>
    public enum GridViewColumnHeaderRole
    {
        /// <summary>
        /// The normal header
        /// </summary>
        Normal,
        /// <summary>
        /// The floating header (when dragging a header)
        /// </summary>
        Floating,
        /// <summary>
        /// The padding header (the very last header in header bar)
        /// </summary>
        Padding
    }

    /// <summary>
    /// column header of GridView
    /// </summary>
    [TemplatePart(Name = "PART_HeaderGripper", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_FloatingHeaderCanvas", Type = typeof(Canvas))]
    public class GridViewColumnHeader : Button
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        #region Constructor

        static GridViewColumnHeader()
        {
            FocusableProperty.OverrideDefaultValue<GridViewColumnHeader>(false);

            // hookup property change event.
            ThemeProperty.Changed.AddClassHandler<GridViewColumnHeader>(LocalPropertyChanged);
            ContentTemplateProperty.Changed.AddClassHandler<GridViewColumnHeader>(LocalPropertyChanged);
            ContextMenuProperty.Changed.AddClassHandler<GridViewColumnHeader>(LocalPropertyChanged);
            ToolTip.TipProperty.Changed.AddClassHandler<GridViewColumnHeader>(LocalPropertyChanged);
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Methods
        //
        //-------------------------------------------------------------------

        #region Public Methods

        /// <summary>
        /// Called when the Template's tree has been generated
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            GridViewColumnHeaderRole role = Role;

            if (role == GridViewColumnHeaderRole.Normal)
            {
                HookupGripperEvents();
            }
            else if (role == GridViewColumnHeaderRole.Floating)
            {
                // if this is a floating header, try to find the FloatingHeaderCanvas,
                // and copy source header's visual to it
                _floatingHeaderCanvas = e.NameScope.Find(FloatingHeaderCanvasTemplateName) as Canvas;

                UpdateFloatingHeaderCanvas();
            }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------

        #region Public Properties

        /// <summary>
        /// The StyledProperty for Column (read-only property)
        /// </summary>
        public static readonly StyledProperty<GridViewColumn> ColumnPropertyKey =
                AvaloniaProperty.Register<GridViewColumnHeader, GridViewColumn>(
                        nameof(Column),
                        null);

        /// <summary>
        /// The StyledProperty for the Column property.
        /// </summary>
        public static readonly StyledProperty<GridViewColumn> ColumnProperty =
                ColumnPropertyKey;

        /// <summary>
        /// Column associated with this header
        /// </summary>
        public GridViewColumn Column
        {
            get { return GetValue(ColumnProperty); }
        }

        /// <summary>
        /// The StyledProperty for Role (read-only property)
        /// </summary>
        public static readonly StyledProperty<GridViewColumnHeaderRole> RolePropertyKey =
                AvaloniaProperty.Register<GridViewColumnHeader, GridViewColumnHeaderRole>(
                        nameof(Role),
                        GridViewColumnHeaderRole.Normal);

        /// <summary>
        /// The StyledProperty for the Role property.
        /// </summary>
        public static readonly StyledProperty<GridViewColumnHeaderRole> RoleProperty =
                RolePropertyKey;

        /// <summary>
        /// What the role of the header is: Normal, Floating, Padding.
        /// </summary>
        [Category("Behavior")]
        public GridViewColumnHeaderRole Role
        {
            get { return GetValue(RoleProperty); }
        }

        #endregion Public Properties

        //-------------------------------------------------------------------
        //
        //  Protected Methods
        //
        //-------------------------------------------------------------------

        #region Protected Methods

        /// <summary>
        /// This is the method that responds to the PointerReleased event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            // give parent a chance to handle PointerEvent (for GridViewHeaderRowPresenter by default)
            e.Handled = false;

            if (ClickMode == ClickMode.Press && _capturedPointer != null)
            {
                _capturedPointer.Capture(null);
                _capturedPointer = null;
            }
        }

        /// <summary>
        /// This is the method that responds to the PointerPressed event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            // give parent a chance to handle PointerEvent (for GridViewHeaderRowPresenter by default)
            e.Handled = false;

            //If ClickMode is Press, we must capture pointer in order to let column reorder work correctly
            if (ClickMode == ClickMode.Press && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _capturedPointer = e.Pointer;
                e.Pointer.Capture(this);
            }
        }

        /// <summary>
        /// This is the method that responds to the PointerMoved event.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);

            // Override base method: if left pointer is pressed, always set IsPressed as true
            if ((ClickMode != ClickMode.Press) &&
                (_capturedPointer != null && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed))
            {
                SetValue(IsPressedProperty, true);
            }

            e.Handled = false;
        }


        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == BoundsProperty)
            {
                // when render size is changed, check to hide the previous header's right half gripper
                CheckWidthForPreviousHeaderGripper();
            }
        }

        /// <summary>
        /// Override base method: raises the Click event only when not re-ordering
        /// </summary>
        protected override void OnClick()
        {
            // if not suppress click event
            if (!SuppressClickEvent)
            {
                // if is clicked by access key or automation,
                // otherwise should be clicked by pointer
                if (IsAccessKeyOrAutomation || !IsPointerOutside())
                {
                    IsAccessKeyOrAutomation = false;
                    ClickImplement();
                    MakeParentGotFocus();
                }
            }
        }

        /// <summary>
        /// The Access key for this control was invoked.
        /// </summary>
        protected override void OnAccessKey(RoutedEventArgs e)
        {
            IsAccessKeyOrAutomation = true;

            base.OnAccessKey(e);
        }


        /// <summary>
        /// An event reporting the pointer entered this element.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            
            if (HandleIsPointerOverChanged())
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// An event reporting the pointer left this element.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            
            if (HandleIsPointerOverChanged())
            {
                e.Handled = true;
            }
        }

        /// <summary>
        ///     An event announcing that the keyboard is no longer focused
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (ClickMode == ClickMode.Press && _capturedPointer != null)
            {
                _capturedPointer.Capture(null);
                _capturedPointer = null;
            }
        }

        #endregion Protected Methods

        //-------------------------------------------------------------------
        //
        //  Internal Methods
        //
        //-------------------------------------------------------------------

        #region Internal Methods

        /// <summary>
        /// This method is called when column header is clicked via automation.
        /// </summary>
        internal void AutomationClick()
        {
            IsAccessKeyOrAutomation = true;
            OnClick();
        }

        // cancel resizing if Escape key down.
        internal void OnColumnHeaderKeyDown(object sender, KeyEventArgs e)
        {
            // Avalonia: Not cancellable in Thumb
            //if (e.Key == Key.Escape && _headerGripper != null && _headerGripper.Classes.Contains(":pressed"))
            //{
            //    // NOTE: this will cause Thumb to complete the dragging and fire drag
            //    // complete event with the Canceled property as 'True'. Handler
            //    // OnColumnHeaderGripperDragCompleted will restore the width.
            //    _headerGripper.CancelDrag();
            //    e.Handled = true;
            //}
        }

        // Check to see if hide previous header's right half gripper
        internal void CheckWidthForPreviousHeaderGripper()
        {
            bool hideGripperRightHalf = false;

            if (_headerGripper != null)
            {
                // when header's width is less than gripper's width,
                // hide the right half of the left header's gripper
                hideGripperRightHalf = DoubleUtil.LessThan(Bounds.Width, _headerGripper.Width);
            }

            _previousHeader?.HideGripperRightHalf(hideGripperRightHalf);

            UpdateGripperCursor();
        }

        // Reset the background visual brush ref to
        // avoid keeping it alive.  Keeping a VisualBrush alive causes us to assume that the
        // entire Visual tree is a graph, preventing an optimized render walk of only
        // the dirty subtree.  We would end up rendering all of our realizations on each
        // frame, causing high CPU consumption when a large realization tree is present.
        internal void ResetFloatingHeaderCanvasBackground()
        {
            if (_floatingHeaderCanvas != null)
                _floatingHeaderCanvas.Background = null;
        }

        /// <summary>
        /// This method is called iff related properties are passed from GirdView/GridViewColumn to header.
        /// And must use this method to update property from GirdView/GridViewColumn to header.
        ///
        /// If this header is instantiated by user, before actually update the property,
        /// this method will turn on the IgnoreXXX flag. And the PropertyChangeCallBack
        /// will check this flag, and know that this update is an internal operation. By
        /// doing this, we can distinguish {the property change by user} from {the change
        /// by HeaderRowPresenter}.
        /// </summary>
        /// <param name="dp">the property you want to update</param>
        /// <param name="value">a null value will result in ClearValue operation</param>
        internal void UpdateProperty(AvaloniaProperty dp, object value)
        {
            Flags ignoreFlag = Flags.None;

            if (!IsInternalGenerated)
            {
                Flags flag;
                PropertyToFlags(dp, out flag, out ignoreFlag);
                Debug.Assert(flag != Flags.None && ignoreFlag != Flags.None, "Invalid parameter dp.");

                if (GetFlag(flag)) /* user has provided value for the property */
                {
                    return;
                }
                else
                {
                    SetFlag(ignoreFlag, true);
                }
            }

            if (value != null)
            {
                SetValue(dp, value);
            }
            else
            {
                ClearValue(dp);
            }

            SetFlag(ignoreFlag, false);
        }

        // Set column header width and associated column width
        internal void UpdateColumnHeaderWidth(double width)
        {
            if (Column != null)
            {
                Column.Width = width;
            }
            else
            {
                Width = width;
            }
        }

        #endregion Internal Methods

        //-------------------------------------------------------------------
        //
        //  Internal Properties
        //
        //-------------------------------------------------------------------

        #region Internal Properties

        #region PreviousVisualHeader

        // Link to the previous visual column header, the value is filled by GridViewHeaderRowPresenter
        internal GridViewColumnHeader PreviousVisualHeader
        {
            get { return _previousHeader; }
            set { _previousHeader = value; }
        }

        private GridViewColumnHeader _previousHeader;

        #endregion PreviousVisualHeader

        #region SuppressClickEvent

        // indicating whether to fire click event
        internal bool SuppressClickEvent
        {
            get { return GetFlag(Flags.SuppressClickEvent); }
            set { SetFlag(Flags.SuppressClickEvent, value); }
        }

        #endregion SuppressClickEvent

        // the source header for floating
        // This property is only used to create VisualBrush for floating header,
        // and will be set to null when VisualBrush is created. Set to null for GC.
        internal GridViewColumnHeader FloatSourceHeader
        {
            get { return _srcHeader; }
            set { _srcHeader = value; }
        }

        // whether this header is generated by GVHeaderRowPresenter or user
        internal bool IsInternalGenerated
        {
            get { return GetFlag(Flags.IsInternalGenerated); }
            set { SetFlag(Flags.IsInternalGenerated, value); }
        }

        #endregion Internal Properties

        //-------------------------------------------------------------------
        //
        //  Accessibility
        //
        //-------------------------------------------------------------------

        #region Accessibility

        /// <summary>
        /// Creates AutomationPeer (<see cref="UIElement.OnCreateAutomationPeer"/>)
        /// </summary>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GridViewColumnHeaderAutomationPeer(this);
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Private Methods
        //
        //-------------------------------------------------------------------

        #region Private Methods

        private static void LocalPropertyChanged(GridViewColumnHeader sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (!sender.IsInternalGenerated)
            {
                Flags flag, ignoreFlag;
                PropertyToFlags(e.Property, out flag, out ignoreFlag);

                if (!sender.GetFlag(ignoreFlag)) // value is updated by user
                {
                    if (e.Priority == BindingPriority.LocalValue)
                    {
                        sender.SetFlag(flag, true);
                    }
                    else
                    {
                        sender.SetFlag(flag, false);

                        GridViewHeaderRowPresenter headerRowPresenter = sender.Parent as GridViewHeaderRowPresenter;
                        headerRowPresenter?.UpdateHeaderProperty(sender, e.Property);
                    }
                }
            }
        }

        private static void PropertyToFlags(AvaloniaProperty dp, out Flags flag, out Flags ignoreFlag)
        {
            if (dp == GridViewColumnHeader.ThemeProperty)
            {
                flag = Flags.StyleSetByUser;
                ignoreFlag = Flags.IgnoreStyle;
            }
            else if (dp == GridViewColumnHeader.ContentTemplateProperty)
            {
                flag = Flags.ContentTemplateSetByUser;
                ignoreFlag = Flags.IgnoreContentTemplate;
            }
            else if (dp == GridViewColumnHeader.ContextMenuProperty)
            {
                flag = Flags.ContextMenuSetByUser;
                ignoreFlag = Flags.IgnoreContextMenu;
            }
            else if (dp == ToolTip.TipProperty)
            {
                flag = Flags.ToolTipSetByUser;
                ignoreFlag = Flags.IgnoreToolTip;
            }
            else
            {
                flag = ignoreFlag = Flags.None;
            }
        }

        /// <summary>
        /// Hide the right half of gripper
        /// +-----------------+
        /// +            +----+
        /// +  Header    + Re +
        /// +            +    +
        /// +            +----+
        /// +-----------------+
        /// </summary>
        /// <param name="hide"></param>
        private void HideGripperRightHalf(bool hide)
        {
            if (_headerGripper != null)
            {
                // hide gripper's right half by setting Parent.ClipToBounds=true
                Control gripperContainer = _headerGripper.Parent as Control;
                if (gripperContainer != null)
                    gripperContainer.ClipToBounds = hide;
            }
        }

        // Save the original width before header resize
        private void OnColumnHeaderGripperDragStarted(object sender, VectorEventArgs e)
        {
            MakeParentGotFocus();
            _originalWidth = ColumnActualWidth;
            e.Handled = true;
        }

        //Because ColumnHeader isn't focusable, we must forward focus to ListView when user invoke the header by access key
        private void MakeParentGotFocus()
        {
            GridViewHeaderRowPresenter headerRP = this.Parent as GridViewHeaderRowPresenter;
            headerRP?.MakeParentItemsControlGotFocus();
        }

        // Resize the header
        private void OnColumnHeaderResize(object sender, VectorEventArgs e)
        {
            double width = ColumnActualWidth + e.Vector.X;
            if (DoubleUtil.LessThanOrClose(width, 0.0))
            {
                width = 0.0;
            }

            UpdateColumnHeaderWidth(width);
            e.Handled = true;
        }

        private void OnColumnHeaderGripperDragCompleted(object sender, VectorEventArgs e)
        {
            if (e.Vector == default(Vector)) // Check if drag was canceled
            {
                // restore to original width
                UpdateColumnHeaderWidth(_originalWidth);
            }

            UpdateGripperCursor();
            e.Handled = true;
        }

        /// <summary>
        /// Find gripper and register drag event
        ///
        /// The default style for GridViewColumnHeader is
        /// +-----------------+
        /// +            +----------+
        /// +  Header    + Gripper  +
        /// +            +          +
        /// +            +----------+
        /// +-----------------+
        /// </summary>
        private void HookupGripperEvents()
        {
            UnhookGripperEvents();

            _headerGripper = this.FindNameScope()?.Find(HeaderGripperTemplateName) as Thumb;

            if (_headerGripper != null)
            {
                _headerGripper.DragStarted += OnColumnHeaderGripperDragStarted;
                _headerGripper.DragDelta += OnColumnHeaderResize;
                _headerGripper.DragCompleted += OnColumnHeaderGripperDragCompleted;
                _headerGripper.DoubleTapped += OnGripperDoubleTapped;
                _headerGripper.PointerEntered += OnGripperPointerEnterLeave;
                _headerGripper.PointerExited += OnGripperPointerEnterLeave;

                _headerGripper.Cursor = SplitCursor;
            }
        }

        private void OnGripperDoubleTapped(object sender, TappedEventArgs e)
        {
            if (Column != null)
            {
                if (Double.IsNaN(Column.Width))
                {
                    // force update will be triggered
                    Column.Width = Column.ActualWidth;
                }

                Column.Width = Double.NaN;

                e.Handled = true;
            }
        }

        /// <summary>
        /// Clear gripper event
        /// </summary>
        private void UnhookGripperEvents()
        {
            if (_headerGripper != null)
            {
                _headerGripper.DragStarted -= OnColumnHeaderGripperDragStarted;
                _headerGripper.DragDelta -= OnColumnHeaderResize;
                _headerGripper.DragCompleted -= OnColumnHeaderGripperDragCompleted;
                _headerGripper.DoubleTapped -= OnGripperDoubleTapped;
                _headerGripper.PointerEntered -= OnGripperPointerEnterLeave;
                _headerGripper.PointerExited -= OnGripperPointerEnterLeave;
                _headerGripper = null;
            }
        }

        private void UpdateGripperCursor()
        {
            if (_headerGripper != null && !_headerGripper.Classes.Contains(":pressed"))
            {
                Cursor gripperCursor;

                if (DoubleUtil.IsZero(Bounds.Width))
                {
                    gripperCursor = SplitOpenCursor;
                }
                else
                {
                    gripperCursor = SplitCursor;
                }

                Debug.Assert(gripperCursor != null, "gripper cursor is null");
                if (gripperCursor != null)
                {
                    _headerGripper.Cursor = gripperCursor;
                }
            }
        }

        private bool IsPointerOutside()
        {
            // In Avalonia, we can use hit testing or bounds checking
            var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            // For now, we'll return false as we don't have direct access to pointer position
            // This would need to be implemented with proper hit testing if needed
            return false;
        }

        private void ClickImplement()
        {
            base.OnClick();
        }

        private bool GetFlag(Flags flag)
        {
            return (_flags & flag) == flag;
        }

        private void SetFlag(Flags flag, bool set)
        {
            if (set)
            {
                _flags |= flag;
            }
            else
            {
                _flags &= (~flag);
            }
        }

        // update the background visual brush
        private void UpdateFloatingHeaderCanvas()
        {
            if (_floatingHeaderCanvas != null
                && FloatSourceHeader != null)
            {
                // because the gripper is partially positioned out of the header, we need to
                // map the appropriate area(viewbox) in the source header to visual brush
                // to avoid a distorted image on the floating header.
                var offsetVector = FloatSourceHeader.Bounds.Position;
                VisualBrush visualBrush = new VisualBrush(FloatSourceHeader)
                {
                    // set visual brush's mapping
                    DestinationRect = new RelativeRect(0, 0, 1, 1, RelativeUnit.Relative),
                    SourceRect = new RelativeRect(offsetVector.X, offsetVector.Y, 
                                                  FloatSourceHeader.Bounds.Width, 
                                                  FloatSourceHeader.Bounds.Height, 
                                                  RelativeUnit.Absolute)
                };

                _floatingHeaderCanvas.Background = visualBrush;
                FloatSourceHeader = null;
            }
        }

        /// <summary>
        /// Handle IsPointerOverChanged when ClickMode is Press
        /// </summary>
        // Note: When ClickMode is Press, ColumnHeader will be click when pointer is over it
        // Here are 2 cases:
        // 1) Pointer is over column header
        //    OnClick will be called
        // 2) Pointer is over gripper
        //    OnClick won't be called, only when the pointer leaves the gripper and move to header, OnClick will be called.
        private bool HandleIsPointerOverChanged()
        {
            if (ClickMode == ClickMode.Press)
            {
                if (IsPointerOver &&
                    //1) Gripper doesn't exist; 2) Gripper exists and Pointer isn't on Gripper;
                    (_headerGripper == null || !_headerGripper.IsPointerOver))
                {
                    // Pressing over the button will click in the OnPress click mode
                    SetValue(IsPressedProperty, true);
                    OnClick();
                }
                else
                {
                    ClearValue(IsPressedProperty);
                }
                return true;
            }
            return false;
        }

        // When pointer enters/leaves gripper, recall HandleIsPointerOverChanged to verify is pointer over header or not
        private void OnGripperPointerEnterLeave(object sender, PointerEventArgs e)
        {
            HandleIsPointerOverChanged();
        }

        #endregion Private Methods

        //-------------------------------------------------------------------
        //
        //  Private Properties
        //
        //-------------------------------------------------------------------

        #region Private Properties

        #region SplitCursor

        private Cursor SplitCursor
        {
            get
            {
                if (_splitCursorCache == null)
                {
                    _splitCursorCache = new Cursor(StandardCursorType.SizeWestEast);
                }
                return _splitCursorCache;
            }
        }

        private static Cursor _splitCursorCache = null;

        #endregion SplitCursor

        #region SplitOpenCursor

        private Cursor SplitOpenCursor
        {
            get
            {
                if (_splitOpenCursorCache == null)
                {
                    _splitOpenCursorCache = new Cursor(StandardCursorType.SizeWestEast);
                }
                return _splitOpenCursorCache;
            }
        }

        private static Cursor _splitOpenCursorCache = null;

        #endregion SplitOpenCursor

        // is clicked by access key or automation
        private bool IsAccessKeyOrAutomation
        {
            get { return GetFlag(Flags.IsAccessKeyOrAutomation); }
            set { SetFlag(Flags.IsAccessKeyOrAutomation, value); }
        }

        private double ColumnActualWidth
        {
            get { return (Column != null ? Column.ActualWidth : Bounds.Width); }
        }

        #endregion Private Properties

        //-------------------------------------------------------------------
        //
        //  Private Fields
        //
        //-------------------------------------------------------------------

        #region Private Fields

        /// <summary>
        /// StyleSetByUser: the value of Style property is set by user.
        /// IgnoreStyle: the OnStyleChanged is triggered by HeaderRowPresenter,
        /// not by user. Don't turn on the StyleSetByUser flag.
        /// And so on
        /// (Only for user provided header. Ignored for internal generated header)
        ///
        /// Go to UpdateProperty and OnPropetyChanged for how these flags work.
        /// </summary>
        [Flags]
        private enum Flags
        {
            // IgnoreXXX can't be combined into one flag.
            // Reason:
            // Define a Style with ContentTemplate and assign it to GridViewColumn.HeaderContainerStyle property. GridViewColumnHeader.OnPropertyChanged method will be called twice.
            // The first call is for ContentTemplate property. In this call, IgnoreContentTemplate is false.
            // The second call is for Style property. In this call, IgnoreStyle is true.
            // One flag cant distinguish them.
            None                                = 0,
            StyleSetByUser                      = 0x00000001,
            IgnoreStyle                         = 0x00000002,
            ContentTemplateSetByUser            = 0x00000004,
            IgnoreContentTemplate               = 0x00000008,
            ContentTemplateSelectorSetByUser    = 0x00000010,
            IgnoreContentTemplateSelector       = 0x00000020,
            ContextMenuSetByUser                = 0x00000040,
            IgnoreContextMenu                   = 0x00000080,
            ToolTipSetByUser                    = 0x00000100,
            IgnoreToolTip                       = 0x00000200,

            SuppressClickEvent                  = 0x00000400,
            IsInternalGenerated                 = 0x00000800,
            IsAccessKeyOrAutomation             = 0x00001000,

            ContentStringFormatSetByUser        = 0x00002000,
            IgnoreContentStringFormat           = 0x00004000,
        }

        private Flags _flags;

        private Thumb _headerGripper;
        private IPointer _capturedPointer;

        private double _originalWidth;

        // canvas for floating header
        private Canvas _floatingHeaderCanvas;

        private GridViewColumnHeader _srcHeader;

        // Part name used in the style. The class TemplatePartAttribute should use the same name
        private const string HeaderGripperTemplateName = "PART_HeaderGripper";
        private const string FloatingHeaderCanvasTemplateName = "PART_FloatingHeaderCanvas";

        #endregion Private Fields
    }
}
