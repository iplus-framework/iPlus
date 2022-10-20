using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.ComponentModel;
using System.Globalization;
using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System.Transactions;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Control for entering a password.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Eingabe eines Passwortes.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBPasswordBox'}de{'VBPasswordBox'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public partial class VBPasswordBox : UserControl, IVBContent, IACObject
    {
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "PasswordBoxStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBPasswordBox/Themes/PasswordBoxStyleGip.xaml",
                                         hasImplicitStyles = true,},
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "PasswordBoxStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBPasswordBox/Themes/ButtonStyleAero.xaml",
                                         hasImplicitStyles = true,},
        };
        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        bool _themeApplied = false;

        /// <summary>
        /// Creates a new instance of VBPasswordBox.
        /// </summary>
        public VBPasswordBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.SourceUpdated += VB_SourceUpdated;
            this.TargetUpdated += VB_TargetUpdated;
            this.Loaded += VBPasswordBox_Loaded;
            this.Unloaded += VBPasswordBox_Unloaded;
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
            InitVBControl();
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
            _themeApplied = ControlManager.OverrideStyleWithTheme(ucPasswordBox, StyleInfoList);
        }

        /// <summary>
        /// Represents the dependency property for CryptPassword.
        /// </summary>
        public static readonly DependencyProperty CryptPasswordProperty
            = DependencyProperty.Register("CryptPassword", typeof(String), typeof(VBPasswordBox), new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        /// <summary>
        /// Gets or sets the CryptPassword.
        /// </summary>
        [Bindable(true)]
        public String CryptPassword
        {
            get { return (String)GetValue(CryptPasswordProperty); }
            set { SetValue(CryptPasswordProperty, value); }
        }


        #region IDataField Members

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get;
            set;
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependency property for ShowCaption.
        /// </summary>
        public static readonly DependencyProperty ShowCaptionProperty
            = DependencyProperty.Register("ShowCaption", typeof(bool), typeof(VBPasswordBox), new PropertyMetadata(true));
        /// <summary>
        /// Determines is control caption shown or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Steuertitel angezeigt werden oder nicht.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool ShowCaption
        {
            get { return (bool)GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        #region Layout

        /// <summary>
        /// Represents the dependency property for WidthCaption.
        /// </summary>
        public static readonly DependencyProperty WidthCaptionProperty
            = DependencyProperty.Register("WidthCaption", typeof(GridLength), typeof(VBPasswordBox), new PropertyMetadata(new GridLength(15, GridUnitType.Star)));
        /// <summary>
        /// Gets or sets the width of caption.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Breite der Beschriftung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthCaption
        {   
            get { return (GridLength)GetValue(WidthCaptionProperty); }
            set { SetValue(WidthCaptionProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthCaptionMax.
        /// </summary>
        public static readonly DependencyProperty WidthCaptionMaxProperty
            = DependencyProperty.Register("WidthCaptionMax", typeof(double), typeof(VBPasswordBox), new PropertyMetadata((double)150));
        /// <summary>
        /// Gets or sets the maximum width of caption.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die maximale Breite der Beschriftung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthCaptionMax
        {
            get { return (double)GetValue(WidthCaptionMaxProperty); }
            set { SetValue(WidthCaptionMaxProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthContent.
        /// </summary>
        public static readonly DependencyProperty WidthContentProperty
            = DependencyProperty.Register("WidthContent", typeof(GridLength), typeof(VBPasswordBox), new PropertyMetadata(new GridLength(20, GridUnitType.Star)));
        /// <summary>
        /// Gets or sets the width of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die maximale Breite der Beschriftung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthContent
        {
            get { return (GridLength)GetValue(WidthContentProperty); }
            set { SetValue(WidthContentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthContentMax.
        /// </summary>
        public static readonly DependencyProperty WidthContentMaxProperty
            = DependencyProperty.Register("WidthContentMax", typeof(double), typeof(VBPasswordBox), new PropertyMetadata(Double.PositiveInfinity));
        /// <summary>
        /// Gets or sets the maximum width of content.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthContentMax
        {
            get { return (double)GetValue(WidthContentMaxProperty); }
            set { SetValue(WidthContentMaxProperty, value); }
        }


        /// <summary>
        /// Represents the dependency property for WidthPadding.
        /// </summary>
        public static readonly DependencyProperty WidthPaddingProperty
            = DependencyProperty.Register("WidthPadding", typeof(GridLength), typeof(VBPasswordBox), new PropertyMetadata(new GridLength(0)));
        /// <summary>
        /// Gets or sets the width of padding.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthPadding
        {
            get { return (GridLength)GetValue(WidthPaddingProperty); }
            set { SetValue(WidthPaddingProperty, value); }
        }
        #endregion

        #endregion

        #region CommandEvents

        private void ucPasswordBoxCanPaste(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Clipboard.ContainsText();
        }

        private void ucPasswordBoxPaste(object sender, RoutedEventArgs e)
        {
            ucPasswordBox.Paste();
        }

        private void ucPasswordBoxCanCut(object sender, CanExecuteRoutedEventArgs e)
        {
        }

        private void ucPasswordBoxCut(object sender, RoutedEventArgs e)
        {
        }

        private void ucPasswordBoxSelectAll(object sender, RoutedEventArgs e)
        {
            ucPasswordBox.SelectAll();
        }

        private void ucPasswordBoxUndo(object sender, RoutedEventArgs e)
        {
        }

        private void ucPasswordBoxRedo(object sender, RoutedEventArgs e)
        {
        }

        #endregion

        #region IDataContent Members
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBPasswordBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        ///// <summary>
        ///// Represents the dependency property for ACUrlCmdMessage.
        ///// </summary>
        ////public static readonly DependencyProperty ACUrlCmdMessageProperty =
        ////    DependencyProperty.Register("ACUrlCmdMessage",
        ////        typeof(ACUrlCmdMessage), typeof(VBPasswordBox),
        ////        new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        ///// <summary>
        ///// Gets or sets the ACUrlCmdMessage.
        ///// </summary>
        ////public ACUrlCmdMessage ACUrlCmdMessage
        ////{
        ////    get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
        ////    set { SetValue(ACUrlCmdMessageProperty, value); }
        ////}

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBPasswordBox),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        private static void OnDepPropChanged(DependencyObject dependencyObject,
               DependencyPropertyChangedEventArgs args)
        {
            VBPasswordBox thisControl = dependencyObject as VBPasswordBox;
            if (thisControl == null)
                return;
            if (args.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (args.Property == BSOACComponentProperty)
            {
                if (args.NewValue == null && args.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = args.OldValue as IACBSO;
                    if (bso != null)
                        thisControl.DeInitVBControl(bso);
                }
            }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return false;
        }

        /// <summary>
        /// Represents the dependency property for VBValidation.
        /// </summary>
        public static readonly DependencyProperty VBValidationProperty = ContentPropertyHandler.VBValidationProperty.AddOwner(typeof(VBPasswordBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Name of the VBValidation property.
        /// </summary>
        /// <summary xml:lang="de">
        /// Name der Eigenschaft VBValidation.
        /// </summary>
        public string VBValidation
        {
            get { return (string)GetValue(VBValidationProperty); }
            set { SetValue(VBValidationProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Password.
        /// </summary>
        public string Password
        {
            get { return ucPasswordBox.Password; }
            set { ucPasswordBox.Password = value; }
        }

        //public string ToolTip
        //{
        //    get
        //    {
        //        return ucPasswordBox.ToolTip as string;
        //    }
        //    set
        //    {
        //        ucPasswordBox.ToolTip = value;
        //    }
        //}

        /// <summary>
        /// Updates a control mode.
        /// </summary>
        public void UpdateControlMode()
        {
            IACComponent elementACComponent = ContextACObject as IACComponent;
            if (elementACComponent == null)
                return;
            Global.ControlModesInfo controlModeInfo = elementACComponent.GetControlModes(this);
            Global.ControlModes controlMode = controlModeInfo.Mode;
            if (controlMode != ControlMode)
                ControlMode = controlMode;

            if (controlMode >= Global.ControlModes.Enabled)
                this.IsTabStop = true;
            else
                this.IsTabStop = false;

            if (controlMode == Global.ControlModes.Collapsed)
            {
                if (this.Visibility != System.Windows.Visibility.Collapsed)
                    this.Visibility = System.Windows.Visibility.Collapsed;
            }
            else if (controlMode == Global.ControlModes.Hidden)
            {
                if (this.Visibility != System.Windows.Visibility.Hidden)
                    this.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                if (this.Visibility != System.Windows.Visibility.Visible)
                    this.Visibility = System.Windows.Visibility.Visible;
                if (controlMode == Global.ControlModes.Disabled)
                {
                    if (IsEnabled)
                        IsEnabled = false;
                }
                else
                {
                    if (!IsEnabled)
                        IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Enables or disables auto focus.
        /// </summary>
        /// <summary xml:lang="de">
        /// Aktiviert oder deaktiviert den Autofokus.
        /// </summary>
        [Category("VBControl")]
        public bool AutoFocus { get; set; }

        IACType _VBContentPropertyInfo = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _VBContentPropertyInfo as ACClassProperty;
            }
        }

        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBPasswordBox));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get
            {
                return (Global.ControlModes)GetValue(ControlModeProperty);
            }
            set
            {
                SetValue(ControlModeProperty, value);
            }
        }

        /// <summary>
        /// Represents the dependency property for DisabledModes.
        /// </summary>
        public static readonly DependencyProperty DisabledModesProperty
            = DependencyProperty.Register("DisabledModes", typeof(string), typeof(VBPasswordBox));
        /// <summary>
        /// Disables the control. XAML sample: DisabledModes="Disabled"
        /// </summary>
        /// <summary xml:lang="de">
        /// Deaktiviert die Steuerung. XAML-Probe: DisabledModes="Disabled"
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string DisabledModes
        {
            get { return (string)GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
        }
        #endregion

        bool _Initialized = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized)
                return;

            _Initialized = true;

            if (!ShowCaption)
            {
                ColCaption.Width = new GridLength(0);
                ucPasswordBox.Margin = new Thickness(0, 0, 0, 0);
            }

            if (DataContext == null || ContextACObject == null)
                return;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBPasswordBox", VBContent);
                return;
            }
            _VBContentPropertyInfo = dcACTypeInfo;

            ValueSource valueSource = DependencyPropertyHelper.GetValueSource(this, VBPasswordBox.RightControlModeProperty);
            if ((valueSource == null)
                || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style))
                || (dcRightControlMode < RightControlMode))
            {
                RightControlMode = dcRightControlMode;
            }

            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            if (VBContentPropertyInfo == null)
                return;

            if (VBContentPropertyInfo != null)
            {
                if (VBContentPropertyInfo.InputMask != "")
                {
                }
                if (VBContentPropertyInfo.MaxLength.HasValue)
                {
                    ucPasswordBox.MaxLength = VBContentPropertyInfo.MaxLength.Value;
                }

                if (string.IsNullOrEmpty(ACCaption))
                    PART_Caption.Text = VBContentPropertyInfo.ACCaption;
                else
                    PART_Caption.Text = this.Root().Environment.TranslateText(ContextACObject, ACCaption);

                ucPasswordBox.PasswordChanged += new RoutedEventHandler(ucPasswordBox_PasswordChanged);
                //PasswordHelper.SetAttach(ucPasswordBox, true);

                Binding binding = new Binding();
                binding.Source = dcSource;
                binding.Path = new PropertyPath(dcPath);
                binding.NotifyOnSourceUpdated = true;
                binding.NotifyOnTargetUpdated = true;
                binding.Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay;
                this.SetBinding(VBPasswordBox.CryptPasswordProperty, binding);

                if (BSOACComponent != null)
                {
                    binding = new Binding();
                    binding.Source = BSOACComponent;
                    binding.Path = new PropertyPath(Const.InitState);
                    binding.Mode = BindingMode.OneWay;
                    SetBinding(VBPasswordBox.ACCompInitStateProperty, binding);
                }
            }

            if (AutoFocus)
            {
                Focus();
                //MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        bool _Loaded = false;
        void VBPasswordBox_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
            {
                Binding boundedValue = BindingOperations.GetBinding(this, VBPasswordBox.CryptPasswordProperty);
                if (boundedValue != null)
                {
                    IACObject boundToObject = boundedValue.Source as IACObject;
                    try
                    {
                        if (boundToObject != null)
                            BSOACComponent.AddWPFRef(this.GetHashCode(), boundToObject);
                    }
                    catch (Exception exw)
                    {
                        this.Root().Messages.LogDebug("VBPassowrdBox", "AddWPFRef", exw.Message);
                    }
                }
            }
            _Loaded = true;
        }

        void VBPasswordBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
            {
                if (BSOACComponent != null )
                    BSOACComponent.RemoveWPFRef(this.GetHashCode());
            }

            _Loaded = false;
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public virtual void DeInitVBControl(IACComponent bso)
        {
            if (!_Initialized)
                return;
            _Initialized = false;
            if (bso != null && bso is IACBSO)
                (bso as IACBSO).RemoveWPFRef(this.GetHashCode());
            if (ucPasswordBox != null)
                ucPasswordBox.PasswordChanged -= ucPasswordBox_PasswordChanged;

            this.SourceUpdated -= VB_SourceUpdated;
            this.TargetUpdated -= VB_TargetUpdated;
            this.Loaded -= VBPasswordBox_Loaded;
            this.Unloaded -= VBPasswordBox_Unloaded;
            _VBContentPropertyInfo = null;

            BindingOperations.ClearBinding(this, VBPasswordBox.CryptPasswordProperty);
            //BindingOperations.ClearBinding(this, VBPasswordBox.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBPasswordBox.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);
        }


        private bool _IsUpdating = false;
        private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            VBPasswordBox passwordBox = sender as VBPasswordBox;
            passwordBox.OnPasswordPropertyChanged(e);
        }

        private void OnPasswordPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            ucPasswordBox.PasswordChanged -= ucPasswordBox_PasswordChanged;

            if (!_IsUpdating)
            {
                ucPasswordBox.Password = (string)e.NewValue;
            }
            ucPasswordBox.PasswordChanged += ucPasswordBox_PasswordChanged;
        }

        void ucPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _IsUpdating = true;
            this.CryptPassword = ucPasswordBox.Password;
            _IsUpdating = false;
        }

        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }

        void ClickPaste(Object sender, RoutedEventArgs args) { ucPasswordBox.Paste(); }
        void ClickCopy(Object sender, RoutedEventArgs args) { }
        void ClickCut(Object sender, RoutedEventArgs args) { }
        void ClickSelectAll(Object sender, RoutedEventArgs args) { ucPasswordBox.SelectAll(); }
        void ClickClear(Object sender, RoutedEventArgs args) { ucPasswordBox.Clear(); }
        void ClickUndo(Object sender, RoutedEventArgs args) { }
        void ClickRedo(Object sender, RoutedEventArgs args) { }

        void menuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ContextACObject != null)
            {
                ACCommand acCommand = RoutedEventHelper.GetACCommand(e);
                if (!acCommand.ParameterList.Any())
                {
                    ContextACObject.ACUrlCommand(acCommand.GetACUrl());
                }
                else
                {
                    ContextACObject.ACUrlCommand(acCommand.GetACUrl(), acCommand.ParameterList);
                }
                e.Handled = true;
            }
        }

        private void menuItem_IsEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ContextACObject != null)
            {
                ACCommand acCommand = RoutedEventHelper.GetACCommand(e);
                if (!acCommand.ParameterList.Any())
                {
                    e.CanExecute = ContextACObject.IsEnabledACUrlCommand(acCommand.GetACUrl());
                }
                else
                {
                    e.CanExecute = ContextACObject.IsEnabledACUrlCommand(acCommand.GetACUrl(), acCommand.ParameterList);
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private void ucPasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
#if !DEBUG
            // On first fucus old value is truncated on release version
            ucPasswordBox.Password = "";
#endif
        }

        /// <summary>
        /// Represents the dependency property for RightControlMode.
        /// </summary>
        public static readonly DependencyProperty RightControlModeProperty
            = DependencyProperty.Register("RightControlMode", typeof(Global.ControlModes), typeof(VBPasswordBox));

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Global.ControlModes RightControlMode
        {
            get { return (Global.ControlModes)GetValue(RightControlModeProperty); }
            set { SetValue(RightControlModeProperty, value); }
        }


        void VB_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            e.Handled = true;
            UpdateControlMode();
        }

        void VB_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateControlMode();
        }

        #region IACObject
        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }


        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }
        #endregion

        #region IACObject Member

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        #endregion

    }

    /// <summary>
    /// Represents the helper for the Password.
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Represents the dependency property for Password.
        /// </summary>
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password",
            typeof(string), typeof(PasswordHelper),
            new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        /// <summary>
        /// Represents the dependency property for Attach.
        /// </summary>
        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach",
            typeof(bool), typeof(PasswordHelper), new PropertyMetadata(false, Attach));

        /// <summary>
        /// Represents the dependency property for IsUpdating.
        /// </summary>
        private static readonly DependencyProperty IsUpdatingProperty =
           DependencyProperty.RegisterAttached("IsUpdating", typeof(bool),
           typeof(PasswordHelper));

        /// <summary>
        /// Sets the Attach.
        /// </summary>
        /// <param name="dp">The dependency object.</param>
        /// <param name="value">The value.</param>
        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        /// <summary>
        /// Gets the Attach.
        /// </summary>
        /// <param name="dp">The dependency object.</param>
        /// <returns>The attach.</returns>
        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        /// <summary>
        /// Gets the Password.
        /// </summary>
        /// <param name="dp">The dependency object.</param>
        /// <returns>The password.</returns>
        public static string GetPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(PasswordProperty);
        }

        /// <summary>
        /// Sets the Password.
        /// </summary>
        /// <param name="dp">The dependency object.</param>
        /// <param name="value">The value.</param>
        public static void SetPassword(DependencyObject dp, string value)
        {
            dp.SetValue(PasswordProperty, value);
        }

        /// <summary>
        /// Gets is updating.
        /// </summary>
        /// <param name="dp">The dependency object.</param>
        /// <returns>True if is updating, otherwise false.</returns>
        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        /// <summary>
        /// Sets the IsUpdating.
        /// </summary>
        /// <param name="dp">The dependency property.</param>
        /// <param name="value">The value.</param>
        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        private static void OnPasswordPropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            passwordBox.PasswordChanged -= PasswordChanged;

            if (!(bool)GetIsUpdating(passwordBox))
            {
                passwordBox.Password = (string)e.NewValue;
            }
            passwordBox.PasswordChanged += PasswordChanged;
        }

        private static void Attach(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;

            if (passwordBox == null)
                return;

            if ((bool)e.OldValue)
            {
                passwordBox.PasswordChanged -= PasswordChanged;
            }

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            SetIsUpdating(passwordBox, true);
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }
    }

}
