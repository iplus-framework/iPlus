using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.layoutengine.avui.Helperclasses;
using System.Windows.Input;
using gip.core.datamodel;
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
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            this.Loaded += new RoutedEventHandler(OnLoaded);

            // buttons on active window and not active window differ => manage
            base.OnInitialized(e);
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
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            RegisterEvents();
        }

        /// <summary>
        /// Represents the dependency property for the ScaleX.
        /// </summary>
        public static readonly DependencyProperty ScaleXProperty
            = DependencyProperty.Register("ScaleX", typeof(double), typeof(VBWindow), new PropertyMetadata((double)1));

        /// <summary>
        /// Gets or sets the scale for X axis.
        /// </summary>
        [Category("VBControl")]
        public double ScaleX
        {
            get { return (double)GetValue(ScaleXProperty); }
            set { SetValue(ScaleXProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the ScaleY.
        /// </summary>
        public static readonly DependencyProperty ScaleYProperty
            = DependencyProperty.Register("ScaleY", typeof(double), typeof(VBWindow), new PropertyMetadata((double)1));

        /// <summary>
        /// Gets or sets the scale for Y axis.
        /// </summary>
        public double ScaleY
        {
            get { return (double)GetValue(ScaleYProperty); }
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

        private void RegisterEvents()
        {
            //object partObject = (object)this.Template.FindName("PART_PanelHeader", this);
            object partObject = (object)GetTemplateChild("PART_PanelHeader");
            if (partObject != null)
            {
                if (partObject is Border)
                {
                    _PART_PanelHeader = ((Border)partObject);
                    _PART_PanelHeader.MouseDown += OnHeaderMouseDown;
                    _PART_PanelHeader.MouseUp += OnHeaderMouseUp;
                    _PART_PanelHeader.MouseMove += OnHeaderMouseMove;
                    _PART_PanelHeader.MouseLeftButtonDown += OnHeaderMouseLeftDown;
                    _PART_PanelHeader.MouseRightButtonDown += OnHeaderMouseRightDown;
                    //_PART_PanelHeader.Loaded += PART_Loaded;
                }
            }

            //partObject = (object)this.Template.FindName("PART_TopThumb", this);
            partObject = (object)GetTemplateChild("PART_TopThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_TopThumb = ((Thumb)partObject);
                _PART_TopThumb.DragDelta += OnTopThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_BottomThumb", this);
            partObject = (object)GetTemplateChild("PART_BottomThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_BottomThumb = ((Thumb)partObject);
                _PART_BottomThumb.DragDelta += OnBottomThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_LeftThumb", this);
            partObject = (object)GetTemplateChild("PART_LeftThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_LeftThumb = ((Thumb)partObject);
                _PART_LeftThumb.DragDelta += OnLeftThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_RightThumb", this);
            partObject = (object)GetTemplateChild("PART_RightThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_RightThumb = ((Thumb)partObject);
                _PART_RightThumb.DragDelta += OnRightThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_CornerTopLeftThumb", this);
            partObject = (object)GetTemplateChild("PART_CornerTopLeftThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_CornerTopLeftThumb = ((Thumb)partObject);
                _PART_CornerTopLeftThumb.DragDelta += OnCornerTopLeftThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_CornerTopRightThumb", this);
            partObject = (object)GetTemplateChild("PART_CornerTopRightThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_CornerTopRightThumb = ((Thumb)partObject);
                _PART_CornerTopRightThumb.DragDelta += OnCornerTopRightThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_CornerBottomLeftThumb", this);
            partObject = (object)GetTemplateChild("PART_CornerBottomLeftThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_CornerBottomLeftThumb = ((Thumb)partObject);
                _PART_CornerBottomLeftThumb.DragDelta += OnCornerBottomLeftThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_CornerBottomRightThumb", this);
            partObject = (object)GetTemplateChild("PART_CornerBottomRightThumb");
            if ((partObject != null) && (partObject is Thumb))
            {
                _PART_CornerBottomRightThumb = ((Thumb)partObject);
                _PART_CornerBottomRightThumb.DragDelta += OnCornerBottomRightThumbDrag;
            }

            //partObject = (object)this.Template.FindName("PART_CloseButton", this);
            partObject = (object)GetTemplateChild("PART_CloseButton");
            if ((partObject != null) && (partObject is Button))
            {
                _PART_CloseButton = ((Button)partObject);
                _PART_CloseButton.Click += OnCloseButtonClicked;
            }

            //partObject = (object)this.Template.FindName("PART_MinimizeButton", this);
            partObject = (object)GetTemplateChild("PART_MinimizeButton");
            if ((partObject != null) && (partObject is Button))
            {
                _PART_MinimizeButton = ((Button)partObject);
                _PART_MinimizeButton.Click += OnMinimizeButtonClicked;
            }

            //partObject = (object)this.Template.FindName("PART_MaximizeButton", this);
            partObject = (object)GetTemplateChild("PART_MaximizeButton");
            if ((partObject != null) && (partObject is Button))
            {
                _PART_MaximizeButton = ((Button)partObject);
                _PART_MaximizeButton.Click += OnMaximizeButtonClicked;
            }

            //partObject = (object)this.Template.FindName("PART_RestoreButton", this);
            partObject = (object)GetTemplateChild("PART_RestoreButton");
            if ((partObject != null) && (partObject is Button))
            {
                _PART_RestoreButton = ((Button)partObject);
                _PART_RestoreButton.Click += OnRestoreButtonClicked;
            }

            partObject = (object)GetTemplateChild("PART_tbTitle");
            if ((partObject != null) && (partObject is TextBlock))
            {
                _PART_tbTitle = ((TextBlock)partObject);
            }

            partObject = (object)GetTemplateChild("PART_cpClientWindowContent");
            if ((partObject != null) && (partObject is ContentPresenter))
            {
                _PART_cpClientWindowContent = ((ContentPresenter)partObject);
            }
        }

        private void UnRegisterEvents()
        {
            if (_PART_PanelHeader != null)
            {
                _PART_PanelHeader.MouseDown -= OnHeaderMouseDown;
                _PART_PanelHeader.MouseUp -= OnHeaderMouseUp;
                _PART_PanelHeader.MouseMove -= OnHeaderMouseMove;
                _PART_PanelHeader.MouseLeftButtonDown -= OnHeaderMouseLeftDown;
                _PART_PanelHeader.MouseRightButtonDown -= OnHeaderMouseRightDown;
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_PanelHeader");
                    if ((partObj == null) && (this.Parent != null))
                    {
                        partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this.Parent, "PART_PanelHeader");
                    }
                    _PART_PanelHeader = partObj != null ? (Border)partObj : null;
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_TopThumb");
                    _PART_TopThumb = partObj != null ? (Thumb)partObj : null;
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_BottomThumb");
                    _PART_BottomThumb = partObj != null ? (Thumb)partObj : null;
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_LeftThumb");
                    _PART_LeftThumb = partObj != null ? (Thumb)partObj : null;
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_RightThumb");
                    _PART_RightThumb = partObj != null ? (Thumb)partObj : null;
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_CornerTopLeftThumb");
                    _PART_CornerTopLeftThumb = partObj != null ? (Thumb)partObj : null;
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_CornerTopRightThumb");
                    _PART_CornerTopRightThumb = partObj != null ? (Thumb)partObj : null;
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_CornerBottomLeftThumb");
                    _PART_CornerBottomLeftThumb = partObj != null ? (Thumb)partObj : null;
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_CornerBottomRightThumb");
                    _PART_CornerBottomRightThumb = partObj != null ? (Thumb)partObj : null;
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_CloseButton");
                    _PART_CloseButton = partObj != null ? (Button)partObj : null;
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
            set { CloseButtonState = value; OnWindowButtonStateChange(value, PART_CloseButton); }
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_MinimizeButton");
                    _PART_MinimizeButton = partObj != null ? (Button)partObj : null;
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
            set { MinimizeButtonState = value; OnWindowButtonStateChange(value, PART_MinimizeButton); }
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_MaximizeButton");
                    _PART_MaximizeButton = partObj != null ? (Button)partObj : null;
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
            set { MaximizeButtonState = value; OnWindowButtonStateChange(value, PART_MaximizeButton); }
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
                    DependencyObject partObj = VBVisualTreeHelper.FindObjectInLogicalAndVisualTree(this, "PART_RestoreButton");
                    _PART_RestoreButton = partObj != null ? (Button)partObj : null;
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
            set { RestoreButtonState = value; OnWindowButtonStateChange(value, PART_RestoreButton); }
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
                    DependencyObject partObj = VBVisualTreeHelper.FindChildObjectInVisualTree(this, "PART_tbTitle");
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

        protected override void OnStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                if (PART_RestoreButton != null)
                {
                    this.PART_RestoreButton.Visibility = Visibility.Collapsed;
                }
                // if Maximize button state is 'None' => do not make visible
                if (MaximizeButtonState != VBWindowButtonState.None && PART_MaximizeButton != null)
                    this.PART_MaximizeButton.Visibility = Visibility.Visible;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                if (PART_MaximizeButton != null)
                    this.PART_MaximizeButton.Visibility = Visibility.Collapsed;

                // if Maximize button state is 'None' => do not make visible
                if (MaximizeButtonState != VBWindowButtonState.None && PART_RestoreButton != null)
                    this.PART_RestoreButton.Visibility = Visibility.Visible;
            }
            base.OnStateChanged(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            /*PART_MinimizeButton.Background = PART_MinimizeButton.BackgroundDefaultValue;
            PART_MaximizeButton.Background = PART_MaximizeButton.BackgroundDefaultValue;
            PART_RestoreButton.Background = PART_RestoreButton.BackgroundDefaultValue;
            PART_CloseButton.Background = PART_CloseButton.BackgroundDefaultValue;*/
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            /*PART_MinimizeButton.Background = Brushes.Transparent;
            PART_MaximizeButton.Background = Brushes.Transparent;
            PART_RestoreButton.Background = Brushes.Transparent;
            PART_CloseButton.Background = Brushes.Transparent;*/
        }

        protected virtual void OnHeaderMouseDown(object sender, MouseEventArgs e)
        {
        }

        protected virtual void OnHeaderMouseUp(object sender, MouseEventArgs e)
        {
        }

        protected virtual void OnHeaderMouseLeftDown(object sender, MouseEventArgs e)
        {
        }

        protected virtual void OnHeaderMouseRightDown(object sender, MouseEventArgs e)
        {
        }

        protected virtual void OnHeaderMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                this.DragMove();
        }
 
        private void OnTopThumbDrag(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.MinHeight < 10)
                this.MinHeight = 10;
            if (this.Height > this.MinHeight)
            {
                this.Height -= e.VerticalChange;
                this.Top += e.VerticalChange;
            }
            else
            {
                this.Height = this.MinHeight + 4;
                PART_TopThumb.ReleaseMouseCapture();
            }
        }

        private void OnBottomThumbDrag(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            SizeToContent = System.Windows.SizeToContent.Manual;
            if (this.MinHeight < 10)
                this.MinHeight = 10;
            if (this.Height > this.MinHeight)
            {
                this.Height += e.VerticalChange;
            }
            else
            {
                this.Height = this.MinHeight + 4;
                PART_BottomThumb.ReleaseMouseCapture();
            }
        }

        private void OnLeftThumbDrag(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.MinWidth < 10)
                this.MinWidth = 10;
            if (this.Width > this.MinWidth)
            {
                this.Width -= e.HorizontalChange;
                this.Left += e.HorizontalChange;
            }
            else
            {
                this.Width = this.MinWidth + 4;
                PART_LeftThumb.ReleaseMouseCapture();
            }
        }

        private void OnRightThumbDrag(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (this.MinWidth < 10)
                this.MinWidth = 10;
            if (this.Width > this.MinWidth)
            {
                this.Width += e.HorizontalChange;
            }
            else
            {
                this.Width = this.MinWidth + 4;
                PART_RightThumb.ReleaseMouseCapture();
            }
        }

        private void OnCornerTopLeftThumbDrag(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            OnTopThumbDrag(sender,e);
            OnLeftThumbDrag(sender, e);
        }

        private void OnCornerTopRightThumbDrag(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            OnTopThumbDrag(sender, e);
            OnRightThumbDrag(sender, e);
        }

        private void OnCornerBottomLeftThumbDrag(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            OnBottomThumbDrag(sender, e);
            OnLeftThumbDrag(sender, e);
        }

        private void OnCornerBottomRightThumbDrag(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            OnBottomThumbDrag(sender, e);
            OnRightThumbDrag(sender, e);
            /*if (this.Width + e.HorizontalChange > 10)
                this.Width += e.HorizontalChange;
            if (this.Height + e.VerticalChange > 10)
                this.Height += e.VerticalChange;*/
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
                    button.Visibility = Visibility.Visible;
                    button.IsEnabled = true;
                    break;

                case VBWindowButtonState.Disabled:
                    button.Visibility = Visibility.Visible;
                    button.IsEnabled = false;
                    break;

                case VBWindowButtonState.None:
                    button.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        #endregion
    }
}
