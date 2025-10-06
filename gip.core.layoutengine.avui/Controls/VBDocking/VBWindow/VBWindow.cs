using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.ComponentModel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a possible window button states.
    /// </summary>
    public enum VBWindowButtonState : short { Normal, Disabled, None }

    /// <summary>
    /// Represents a iPlus window.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein iPlus-Fenster dar.
    /// </summary>
    [TemplatePart(Name = "PART_PanelHeader", Type = typeof(Border))]

    [TemplatePart(Name = "PART_TopThumb", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_BottomThumb", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_LeftThumb", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_RightThumb", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_CornerTopLeftThumb", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_CornerTopRightThumb", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_CornerBottomLeftThumb", Type = typeof(Thumb))]
    [TemplatePart(Name = "PART_CornerBottomRightThumb", Type = typeof(Thumb))]

    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MinimizeButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MaximizeButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_RestoreButton", Type = typeof(Button))]

    [TemplatePart(Name = "PART_tbTitle", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_cpClientWindowContent", Type = typeof(ContentPresenter))]

    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBWindow'}de{'VBWindow'}", Global.ACKinds.TACVBControl)]
    public abstract class VBWindow : Window
    {

        #region c'tors
        /// <summary>
        /// Creates a new instance of VBWindow.
        /// </summary>
        public VBWindow()
        {
            this.WindowState = WindowState.Normal;
            this.SystemDecorations = SystemDecorations.None;
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized()
        {
            this.Loaded += OnLoaded;

            // buttons on active window and not active window differ => manage
            base.OnInitialized();
            IRoot root = this.Root();
            if (root != null && root.RootPageWPF != null && root.RootPageWPF.Zoom > 1)
            {
                ScaleX = root.RootPageWPF.Zoom * 0.01;
                ScaleY = root.RootPageWPF.Zoom * 0.01;
            }
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            RegisterEvents(e);
        }

        /// <summary>
        /// Represents the dependency property for the ScaleX.
        /// </summary>
        public static readonly StyledProperty<double> ScaleXProperty
            = AvaloniaProperty.Register<VBWindow, double>(nameof(ScaleX), 1.0);

        /// <summary>
        /// Gets or sets the scale for X axis.
        /// </summary>
        [Category("VBControl")]
        public double ScaleX
        {
            get { return GetValue(ScaleXProperty); }
            set { SetValue(ScaleXProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the ScaleY.
        /// </summary>
        public static readonly StyledProperty<double> ScaleYProperty
            = AvaloniaProperty.Register<VBWindow, double>(nameof(ScaleY), 1.0);

        /// <summary>
        /// Gets or sets the scale for Y axis.
        /// </summary>
        public double ScaleY
        {
            get { return GetValue(ScaleYProperty); }
            set { SetValue(ScaleYProperty, value); }
        }

        internal virtual void DeInitVBControl(IACComponent bso = null)
        {
            UnRegisterEvents();
            _PART_PanelHeader = null;
            _PART_TopThumb = null;
            _PART_BottomThumb = null;
            _PART_LeftThumb = null;
            _PART_RightThumb = null;
            _PART_CornerTopLeftThumb = null;
            _PART_CornerTopRightThumb = null;
            _PART_CornerBottomLeftThumb = null;
            _PART_CornerBottomRightThumb = null;
            _PART_CloseButton = null;
            _PART_MinimizeButton = null;
            _PART_MaximizeButton = null;
            _PART_RestoreButton = null;
            _PART_tbTitle = null;
            _PART_cpClientWindowContent = null;
            this.Loaded -= OnLoaded;
        }

        private void RegisterEvents(TemplateAppliedEventArgs e)
        {
            //object partObject = (object)this.Template.FindName("PART_PanelHeader", this);
            object partObject = (object)e.NameScope.Find("PART_PanelHeader");
            if (partObject != null)
            {
                if (partObject is Border)
                {
                    _PART_PanelHeader = ((Border)partObject);
                    _PART_PanelHeader.PointerPressed += OnHeaderPointerPressed;
                    _PART_PanelHeader.PointerReleased += OnHeaderPointerReleased;
                    _PART_PanelHeader.PointerMoved += OnHeaderPointerMoved;
                    //_PART_PanelHeader.Loaded += PART_Loaded;
                }
            }

            //partObject = (object)this.Template.FindName("PART_TopThumb", this);
            partObject = (object)e.NameScope.Find("PART_TopThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_TopThumb = ((Thumb)partObject);
                _PART_TopThumb.DragDelta += OnTopThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_BottomThumb", this);
            partObject = (object)e.NameScope.Find("PART_BottomThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_BottomThumb = ((Thumb)partObject);
                _PART_BottomThumb.DragDelta += OnBottomThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_LeftThumb", this);
            partObject = (object)e.NameScope.Find("PART_LeftThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_LeftThumb = ((Thumb)partObject);
                _PART_LeftThumb.DragDelta += OnLeftThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_RightThumb", this);
            partObject = (object)e.NameScope.Find("PART_RightThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_RightThumb = ((Thumb)partObject);
                _PART_RightThumb.DragDelta += OnRightThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_CornerTopLeftThumb", this);
            partObject = (object)e.NameScope.Find("PART_CornerTopLeftThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_CornerTopLeftThumb = ((Thumb)partObject);
                _PART_CornerTopLeftThumb.DragDelta += OnCornerTopLeftThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_CornerTopRightThumb", this);
            partObject = (object)e.NameScope.Find("PART_CornerTopRightThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_CornerTopRightThumb = ((Thumb)partObject);
                _PART_CornerTopRightThumb.DragDelta += OnCornerTopRightThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_CornerBottomLeftThumb", this);
            partObject = (object)e.NameScope.Find("PART_CornerBottomLeftThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_CornerBottomLeftThumb = ((Thumb)partObject);
                _PART_CornerBottomLeftThumb.DragDelta += OnCornerBottomLeftThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_CornerBottomRightThumb", this);
            partObject = (object)e.NameScope.Find("PART_CornerBottomRightThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_CornerBottomRightThumb = ((Thumb)partObject);
                _PART_CornerBottomRightThumb.DragDelta += OnCornerBottomRightThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_CloseButton", this);
            partObject = (object)e.NameScope.Find("PART_CloseButton");
            if ((partObject != null) && (partObject is Button))
            {
                _PART_CloseButton = ((Button)partObject);
                _PART_CloseButton.Click += OnCloseButtonClicked;
            }

            //partObject = (object)this.Template.FindName("PART_MinimizeButton", this);
            partObject = (object)e.NameScope.Find("PART_MinimizeButton");
            if ((partObject != null) && (partObject is Button))
            {
                _PART_MinimizeButton = ((Button)partObject);
                _PART_MinimizeButton.Click += OnMinimizeButtonClicked;
            }

            //partObject = (object)this.Template.FindName("PART_MaximizeButton", this);
            partObject = (object)e.NameScope.Find("PART_MaximizeButton");
            if ((partObject != null) && (partObject is Button))
            {
                _PART_MaximizeButton = ((Button)partObject);
                _PART_MaximizeButton.Click += OnMaximizeButtonClicked;
            }

            //partObject = (object)this.Template.FindName("PART_RestoreButton", this);
            partObject = (object)e.NameScope.Find("PART_RestoreButton");
            if ((partObject != null) && (partObject is Button))
            {
                _PART_RestoreButton = ((Button)partObject);
                _PART_RestoreButton.Click += OnRestoreButtonClicked;
            }

            partObject = (object)e.NameScope.Find("PART_tbTitle");
            if ((partObject != null) && (partObject is TextBlock))
            {
                _PART_tbTitle = ((TextBlock)partObject);
            }

            partObject = (object)e.NameScope.Find("PART_cpClientWindowContent");
            if ((partObject != null) && (partObject is ContentPresenter))
            {
                _PART_cpClientWindowContent = ((ContentPresenter)partObject);
            }
        }

        private void UnRegisterEvents()
        {
            if (_PART_PanelHeader != null)
            {
                _PART_PanelHeader.PointerPressed -= OnHeaderPointerPressed;
                _PART_PanelHeader.PointerReleased -= OnHeaderPointerReleased;
                _PART_PanelHeader.PointerMoved -= OnHeaderPointerMoved;
            }

            if (_PART_TopThumb != null)
            {
                _PART_TopThumb.DragDelta -= OnTopThumbDrag;
            }

            if (_PART_BottomThumb != null)
            {
                _PART_BottomThumb.DragDelta -= OnBottomThumbDrag;
            }

            if (_PART_LeftThumb != null)
            {
                _PART_LeftThumb.DragDelta -= OnLeftThumbDrag;
            }

            if (_PART_RightThumb != null)
            {
                _PART_RightThumb.DragDelta -= OnRightThumbDrag;
            }

            if (_PART_CornerTopLeftThumb != null)
            {
                _PART_CornerTopLeftThumb.DragDelta -= OnCornerTopLeftThumbDrag;
            }

            if (_PART_CornerTopRightThumb != null)
            {
                _PART_CornerTopRightThumb.DragDelta -= OnCornerTopRightThumbDrag;
            }

            if (_PART_CornerBottomLeftThumb != null)
            {
                _PART_CornerBottomLeftThumb.DragDelta -= OnCornerBottomLeftThumbDrag;
            }

            if (_PART_CornerBottomRightThumb != null)
            {
                _PART_CornerBottomRightThumb.DragDelta -= OnCornerBottomRightThumbDrag;
            }

            if (_PART_CloseButton != null)
            {
                _PART_CloseButton.Click -= OnCloseButtonClicked;
            }

            if (_PART_MinimizeButton != null)
            {
                _PART_MinimizeButton.Click -= OnMinimizeButtonClicked;
            }

            if (_PART_MaximizeButton != null)
            {
                _PART_MaximizeButton.Click -= OnMaximizeButtonClicked;
            }

            if (_PART_RestoreButton != null)
            {
                _PART_RestoreButton.Click -= OnRestoreButtonClicked;
            }
        }

        #endregion

        #region PART's
        private void PART_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private Border _PART_PanelHeader;

        /// <summary>
        /// Gets the PART_PanelHeader.
        /// </summary>
        public Border PART_PanelHeader
        {
            get
            {
                if (_PART_PanelHeader == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_PanelHeader");
                    if ((partObj == null) && (this.Parent != null))
                    {
                        partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this.Parent, "PART_PanelHeader");
                    }
                    _PART_PanelHeader = partObj as Border;
                }
                return _PART_PanelHeader;
            }
        }

        private Thumb _PART_TopThumb;
        /// <summary>
        /// Gets the PART_TopThumb.
        /// </summary>
        public Thumb PART_TopThumb
        {
            get
            {
                if (_PART_TopThumb == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_TopThumb");
                    _PART_TopThumb = partObj as Thumb;
                }
                return _PART_TopThumb;
            }
        }

        private Thumb _PART_BottomThumb;
        /// <summary>
        /// Gets the PART_BottomThumb.
        /// </summary>
        public Thumb PART_BottomThumb
        {
            get
            {
                if (_PART_BottomThumb == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_BottomThumb");
                    _PART_BottomThumb = partObj as Thumb;
                }
                return _PART_BottomThumb;
            }
        }

        private Thumb _PART_LeftThumb;
        /// <summary>
        /// Gets the PART_LeftThumb.
        /// </summary>
        public Thumb PART_LeftThumb
        {
            get
            {
                if (_PART_LeftThumb == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_LeftThumb");
                    _PART_LeftThumb = partObj as Thumb;
                }
                return _PART_LeftThumb;
            }
        }

        private Thumb _PART_RightThumb;
        /// <summary>
        /// Gets the PART_RightThumb.
        /// </summary>
        public Thumb PART_RightThumb
        {
            get
            {
                if (_PART_RightThumb == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_RightThumb");
                    _PART_RightThumb = partObj as Thumb;
                }
                return _PART_RightThumb;
            }
        }

        private Thumb _PART_CornerTopLeftThumb;
        /// <summary>
        /// Gets the PART_CornerTopLeftThumb.
        /// </summary>
        public Thumb PART_CornerTopLeftThumb
        {
            get
            {
                if (_PART_CornerTopLeftThumb == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_CornerTopLeftThumb");
                    _PART_CornerTopLeftThumb = partObj as Thumb;
                }
                return _PART_CornerTopLeftThumb;
            }
        }

        private Thumb _PART_CornerTopRightThumb;
        /// <summary>
        /// Gets the PART_CornerTopRightThumb.
        /// </summary>
        public Thumb PART_CornerTopRightThumb
        {
            get
            {
                if (_PART_CornerTopRightThumb == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_CornerTopRightThumb");
                    _PART_CornerTopRightThumb = partObj as Thumb;
                }
                return _PART_CornerTopRightThumb;
            }
        }

        private Thumb _PART_CornerBottomLeftThumb;
        /// <summary>
        /// Gets the PART_CornerBottomLeftThumb.
        /// </summary>
        public Thumb PART_CornerBottomLeftThumb
        {
            get
            {
                if (_PART_CornerBottomLeftThumb == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_CornerBottomLeftThumb");
                    _PART_CornerBottomLeftThumb = partObj as Thumb;
                }
                return _PART_CornerBottomLeftThumb;
            }
        }

        private Thumb _PART_CornerBottomRightThumb;
        /// <summary>
        /// Gets the PART_CornerBottomRightThumb.
        /// </summary>
        public Thumb PART_CornerBottomRightThumb
        {
            get
            {
                if (_PART_CornerBottomRightThumb == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_CornerBottomRightThumb");
                    _PART_CornerBottomRightThumb = partObj as Thumb;
                }
                return _PART_CornerBottomRightThumb;
            }
        }

        private Button _PART_CloseButton;
        /// <summary>
        /// Gets the PART_CloseButton.
        /// </summary>
        public Button PART_CloseButton
        {
            get
            {
                if (_PART_CloseButton == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_CloseButton");
                    _PART_CloseButton = partObj as Button;
                }
                return _PART_CloseButton;
            }
        }

        protected VBWindowButtonState _closeButtonState;
        /// <summary>
        /// Gets the PART_CloseButtonState.
        /// </summary>
        public VBWindowButtonState CloseButtonState
        {
            get { return _closeButtonState; }
            set { _closeButtonState = value; OnWindowButtonStateChange(value, PART_CloseButton); }
        }


        private Button _PART_MinimizeButton;
        /// <summary>
        /// Gets the PART_MinimizeButton.
        /// </summary>
        public Button PART_MinimizeButton
        {
            get
            {
                if (_PART_MinimizeButton == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_MinimizeButton");
                    _PART_MinimizeButton = partObj as Button;
                }
                return _PART_MinimizeButton;
            }
        }

        protected VBWindowButtonState _MinimizeButtonState;
        /// <summary>
        /// Gets the PART_MinimizeButtonState.
        /// </summary>
        public VBWindowButtonState MinimizeButtonState
        {
            get { return _MinimizeButtonState; }
            set { _MinimizeButtonState = value; OnWindowButtonStateChange(value, PART_MinimizeButton); }
        }


        private Button _PART_MaximizeButton;
        /// <summary>
        /// Gets the PART_MaximizeButton.
        /// </summary>
        public Button PART_MaximizeButton
        {
            get
            {
                if (_PART_MaximizeButton == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_MaximizeButton");
                    _PART_MaximizeButton = partObj as Button;
                }
                return _PART_MaximizeButton;
            }
        }

        protected VBWindowButtonState _MaximizeButtonState;
        /// <summary>
        /// Gets the PART_MaximizeButtonState.
        /// </summary>
        public VBWindowButtonState MaximizeButtonState
        {
            get { return _MaximizeButtonState; }
            set { _MaximizeButtonState = value; OnWindowButtonStateChange(value, PART_MaximizeButton); }
        }

        private Button _PART_RestoreButton;
        /// <summary>
        /// Gets the PART_RestoreButton.
        /// </summary>
        public Button PART_RestoreButton
        {
            get
            {
                if (_PART_RestoreButton == null)
                {
                    AvaloniaObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_RestoreButton");
                    _PART_RestoreButton = partObj as Button;
                }
                return _PART_RestoreButton;
            }
        }

        protected VBWindowButtonState _RestoreButtonState;
        /// <summary>
        /// Gets the PART_RestoreButtonState.
        /// </summary>
        public VBWindowButtonState RestoreButtonState
        {
            get { return _RestoreButtonState; }
            set { _RestoreButtonState = value; OnWindowButtonStateChange(value, PART_RestoreButton); }
        }

        private TextBlock _PART_tbTitle;
        /// <summary>
        /// Gets the PART_tbTitle.
        /// </summary>
        public TextBlock PART_tbTitle
        {
            get
            {
                /*if (_PART_tbTitle == null)
                {
                    Control partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_tbTitle");
                    _PART_tbTitle = partObj != null ? (TextBlock)partObj : null;
                }*/
                return _PART_tbTitle;
            }
        }

        public event EventHandler OnTitleChanged;

        /// <summary>
        /// Refreshes the title of window.
        /// </summary>
        public virtual void RefreshTitle()
        {
            if (PART_tbTitle == null)
                return;
            PART_tbTitle.Text = Title;
            if (OnTitleChanged != null)
                OnTitleChanged(this, new EventArgs());
        }

        protected ContentPresenter _PART_cpClientWindowContent;
        /// <summary>
        /// Gets the PART_cpClientWindowContent.
        /// </summary>
        public ContentPresenter PART_cpClientWindowContent
        {
            get
            {
                return _PART_cpClientWindowContent;
            }
        }

        #endregion

        #region Eventhandler
        /// <summary>
        /// Handles the OnClosed event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnClosed(EventArgs e)
        {
            DeInitVBControl();
            /*PART_MinimizeButton.Background = Brushes.Transparent;
            PART_MaximizeButton.Background = Brushes.Transparent;
            PART_RestoreButton.Background = Brushes.Transparent;
            PART_CloseButton.Background = Brushes.Transparent;*/
            base.OnClosed(e);
        }

        /// <summary>
        /// Handles the OnLoaded event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The event arugments.</param>
        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
            // refresh state
            // OnStateChanged(new EventArgs());
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == WindowStateProperty)
            {
                if (this.WindowState == WindowState.Normal)
                {
                    if (PART_RestoreButton != null)
                    {
                        this.PART_RestoreButton.IsVisible = false;
                    }
                    // if Maximize button state is 'None' => do not make visible
                    if (MaximizeButtonState != VBWindowButtonState.None && PART_MaximizeButton != null)
                        this.PART_MaximizeButton.IsVisible = true;
                }
                else if (this.WindowState == WindowState.Maximized)
                {
                    if (PART_MaximizeButton != null)
                        this.PART_MaximizeButton.IsVisible = false;

                    // if Maximize button state is 'None' => do not make visible
                    if (MaximizeButtonState != VBWindowButtonState.None && PART_RestoreButton != null)
                        this.PART_RestoreButton.IsVisible = true;
                }
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            /*PART_MinimizeButton.Background = PART_MinimizeButton.BackgroundDefaultValue;
            PART_MaximizeButton.Background = PART_MaximizeButton.BackgroundDefaultValue;
            PART_RestoreButton.Background = PART_RestoreButton.BackgroundDefaultValue;
            PART_CloseButton.Background = PART_CloseButton.BackgroundDefaultValue;*/
        }

        private bool _isDragging = false;
        private PointerPressedEventArgs _lastPressedEvent;

        protected virtual void OnHeaderPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                _lastPressedEvent = e;
                e.Handled = true;
            }
        }

        protected virtual void OnHeaderPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
            _lastPressedEvent = null;
        }

        protected virtual void OnHeaderPointerMoved(object sender, PointerEventArgs e)
        {
            if (_isDragging && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && _lastPressedEvent != null)
            {
                this.BeginMoveDrag(_lastPressedEvent);
                _isDragging = false; // Prevent multiple drag starts
            }
        }
 
        private void OnTopThumbDrag(object sender, VectorEventArgs e)
        {
            if (this.MinHeight < 10)
                this.MinHeight = 10;
            if (this.Height > this.MinHeight)
            {
                this.Height -= e.Vector.Y;
                // In Avalonia, Position is used instead of Top/Left
                var currentPos = this.Position;
                this.Position = new PixelPoint(currentPos.X, currentPos.Y + (int)e.Vector.Y);
            }
            else
            {
                this.Height = this.MinHeight + 4;
            }
        }

        private void OnBottomThumbDrag(object sender, VectorEventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
            if (this.MinHeight < 10)
                this.MinHeight = 10;
            if (this.Height > this.MinHeight)
            {
                this.Height += e.Vector.Y;
            }
            else
            {
                this.Height = this.MinHeight + 4;
            }
        }

        private void OnLeftThumbDrag(object sender, VectorEventArgs e)
        {
            if (this.MinWidth < 10)
                this.MinWidth = 10;
            if (this.Width > this.MinWidth)
            {
                this.Width -= e.Vector.X;
                // In Avalonia, Position is used instead of Top/Left
                var currentPos = this.Position;
                this.Position = new PixelPoint(currentPos.X + (int)e.Vector.X, currentPos.Y);
            }
            else
            {
                this.Width = this.MinWidth + 4;
            }
        }

        private void OnRightThumbDrag(object sender, VectorEventArgs e)
        {
            if (this.MinWidth < 10)
                this.MinWidth = 10;
            if (this.Width > this.MinWidth)
            {
                this.Width += e.Vector.X;
            }
            else
            {
                this.Width = this.MinWidth + 4;
            }
        }

        private void OnCornerTopLeftThumbDrag(object sender, VectorEventArgs e)
        {
            OnTopThumbDrag(sender,e);
            OnLeftThumbDrag(sender, e);
        }

        private void OnCornerTopRightThumbDrag(object sender, VectorEventArgs e)
        {
            OnTopThumbDrag(sender, e);
            OnRightThumbDrag(sender, e);
        }

        private void OnCornerBottomLeftThumbDrag(object sender, VectorEventArgs e)
        {
            OnBottomThumbDrag(sender, e);
            OnLeftThumbDrag(sender, e);
        }

        private void OnCornerBottomRightThumbDrag(object sender, VectorEventArgs e)
        {
            OnBottomThumbDrag(sender, e);
            OnRightThumbDrag(sender, e);
            /*if (this.Width + e.Vector.X > 10)
                this.Width += e.Vector.X;
            if (this.Height + e.Vector.Y > 10)
                this.Height += e.Vector.Y;*/
        }

        protected bool _ClosedFromCloseButton = false;
        protected virtual void OnCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            _ClosedFromCloseButton = true;
            this.Close();
        }

        protected virtual void OnMinimizeButtonClicked(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        protected virtual void OnMaximizeButtonClicked(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        protected virtual void OnRestoreButtonClicked(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        protected virtual void OnWindowButtonStateChange(VBWindowButtonState state, Button button)
        {
            if (button == null)
                return;

            switch (state)
            {
                case VBWindowButtonState.Normal:
                    button.IsVisible = true;
                    button.IsEnabled = true;
                    break;

                case VBWindowButtonState.Disabled:
                    button.IsVisible = true;
                    button.IsEnabled = false;
                    break;

                case VBWindowButtonState.None:
                    button.IsVisible = false;
                    break;
            }
        }
        #endregion
    }
}
