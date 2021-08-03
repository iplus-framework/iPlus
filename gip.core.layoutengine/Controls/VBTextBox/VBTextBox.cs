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
using System.ComponentModel;
using System.Windows.Markup;
using System.Collections;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;
using System.Transactions;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a control that can be used to display or edit unformatted text.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein Steuerelement dar, mit dem unformatierter Text angezeigt oder bearbeitet werden kann.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTextBox'}de{'VBTextBox'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTextBox : TextBox, IVBContent, IACMenuBuilderWPFTree, IACObject, IClearVBContent
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TextBoxStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTextBox/Themes/TextBoxStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TextBoxStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTextBox/Themes/TextBoxStyleAero.xaml" },
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

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }


        static VBTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTextBox), new FrameworkPropertyMetadata(typeof(VBTextBox)));
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner(typeof(VBTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        }

        protected bool _themeApplied = false;
        public VBTextBox()
        {
            this.Focusable = true;
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true, StyleInfoList);
            GotFocus += VBTextBox_GotFocus;
            this.PreviewTextInput += TextBox_PreviewTextInput;
            this.PreviewKeyDown += TextBox_PreviewKeyDown;
            this.SourceUpdated += VBTextBox_SourceUpdated;
            this.TargetUpdated += VBTextBox_TargetUpdated;
            Loaded += VBTextBox_Loaded;
            Unloaded += VBTextBox_Unloaded;

            CmdBindingPaste = new CommandBinding(ApplicationCommands.Paste, TextBox_Paste);
            CmdBindingCut = new CommandBinding(ApplicationCommands.Cut, null, TextBox_CanCut);
            this.CommandBindings.Add(CmdBindingPaste); //handle paste
            this.CommandBindings.Add(CmdBindingCut); //surpress cut
            ResolveMaskProvider(Mask);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false, MyStyleInfoList);
            InitVBControl();
        }

        public void ActualizeTheme(bool bInitializingCall, List<CustomControlStyleInfo> CustomControlStyleInfoList)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, CustomControlStyleInfoList, bInitializingCall);
        }

        #endregion

        #region Loaded-Event
        private CommandBinding CmdBindingPaste;
        private CommandBinding CmdBindingCut;
        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized || DataContext == null)
                return;
            _Initialized = true;
            Binding binding = null;
            if (String.IsNullOrEmpty(VBContent))
            {
                if (!string.IsNullOrEmpty(ACCaption) && ContextACObject != null)
                {
                    //ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
                    ACCaptionTrans = Translator.GetTranslation(ACIdentifier, ACCaption, this.Root().Environment.VBLanguageCode);
                }
                UpdateControlMode();
                return;
            }
            else if (ContextACObject == null)
            {
                binding = new Binding();
                binding.Path = new PropertyPath(VBContent);
                binding.NotifyOnSourceUpdated = true;
                binding.NotifyOnTargetUpdated = true;
                SetBinding(TextBox.TextProperty, binding);
                UpdateControlMode();
                return;
            }

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBTextBox", VBContent);
                return;
            }

            ProxyACRefConverter refConverter = ProxyACRefConverter.IfACRefGenerateConverter(out binding, ref dcACTypeInfo, ref dcPath, ref dcSource, ref dcRightControlMode);

            _VBContentPropertyInfo = dcACTypeInfo;
            if (VBContentPropertyInfo == null)
            {
                this.Root().Messages.LogDebug("Error00003", "VBTextBox", String.Format("VBContent {0} is not  property",VBContent));
                return;
            }

            ValueSource valueSource = DependencyPropertyHelper.GetValueSource(this, VBTextBox.RightControlModeProperty);
            if ((valueSource == null)
                || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style))
                || (dcRightControlMode < RightControlMode))
            {
                RightControlMode = dcRightControlMode;
            }

            valueSource = DependencyPropertyHelper.GetValueSource(this, VBTextBox.TextAlignmentProperty);
            if ((valueSource == null)
                || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
            {
                RightControlMode = dcRightControlMode;

                Type typeOfProp = dcACTypeInfo.ObjectType;
                if ((typeOfProp.FullName == TypeAnalyser._TypeName_Byte)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_Int16)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_Int32)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_Int64)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_UInt16)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_UInt32)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_UInt64)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_Double)
                    || (typeOfProp.FullName == TypeAnalyser._TypeName_Single))
                {
                    TextAlignment = TextAlignment.Right;
                }
            }

            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            IsNullable = VBContentPropertyInfo.IsNullable;
            if (!string.IsNullOrEmpty(VBContentPropertyInfo.InputMask))
            {
                Mask = VBContentPropertyInfo.InputMask;
            }
            if (VBContentPropertyInfo.MaxLength.HasValue && VBContentPropertyInfo.MaxLength.Value > 0)
            {
                MaxLength = VBContentPropertyInfo.MaxLength.Value;
            }
            if (string.IsNullOrEmpty(ACCaption))
                ACCaptionTrans = VBContentPropertyInfo.ACCaption;
            else
                ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);

            if (refConverter == null)
            {
                binding = new Binding();
                binding.Source = dcSource;
                binding.Path = new PropertyPath(dcPath);
                binding.NotifyOnSourceUpdated = true;
                binding.NotifyOnTargetUpdated = true;
                binding.Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay;
                if (!String.IsNullOrEmpty(VBValidation))
                {
                    binding.ValidationRules.Add(new VBValidationRule(ValidationStep.ConvertedProposedValue, true, ContextACObject, VBContent, VBValidation));
                    binding.NotifyOnValidationError = true;
                    //binding.ValidatesOnExceptions = true;
                    //binding.ValidatesOnExceptions = true;
                }
                binding.Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay;
            }

            if (!String.IsNullOrEmpty(StringFormat))
                binding.StringFormat = StringFormat;
            binding.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture; //System.Globalization.CultureInfo.CurrentUICulture;

            BindingExpressionBase bExp = SetBinding(TextBox.TextProperty, binding);
            if (refConverter != null)
                refConverter.ParentBinding = bExp;

            if (BSOACComponent != null)
            {
                binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBTextBox.ACCompInitStateProperty, binding);
            }

            // DAMIR TEST
            if (ResourceKeyTestOn)
            {
                IEnumerable<object> resources = VBResourceFinder.FindResources(this);
                if (resources != null)
                {
                    foreach (object resource in resources)
                    {
                        if (resource is Style)
                        {
                            Style styleFromDictionary = resource as Style;
                            if (styleFromDictionary.Setters != null)
                            {
                                foreach (SetterBase setter in styleFromDictionary.Setters)
                                {
                                    if (setter is Setter)
                                    {
                                        if ((setter as Setter).Value is BindingBase)
                                            SetBinding((setter as Setter).Property, (setter as Setter).Value as BindingBase);
                                        else
                                            SetValue((setter as Setter).Property, (setter as Setter).Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            valueSource = DependencyPropertyHelper.GetValueSource(this, FrameworkElement.ToolTipProperty);
            if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
            {
                if (ToolTip is string)
                {
                    string toolTip = ToolTip as string;
                    if (!string.IsNullOrEmpty(toolTip))
                    {
                        ToolTip = this.Root().Environment.TranslateText(ContextACObject, toolTip);
                    }
                }
            }

            //if (IsEnabled)
            //{
            //    if (RightControlMode < Global.ControlModes.Enabled)
            //    {
            //        IsReadOnly = true;
            //    }
            //    else
            //    {
            //        UpdateControlMode();
            //    }
            //}
            if (AutoFocus)
            {
                try
                {
                    Focus();
                    //MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBTextBox", "InitVBControl", msg);
                }
            }
        }

        bool _Loaded = false;
        void VBTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
            {
                Binding boundedValue = BindingOperations.GetBinding(this, TextBox.TextProperty);
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
                        this.Root().Messages.LogDebug("VBTextBox", "AddWPFRef", exw.Message);
                    }
                }
            }
            _Loaded = true;
        }

        void VBTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
                BSOACComponent.RemoveWPFRef(this.GetHashCode());

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
            this.GotFocus -= VBTextBox_GotFocus;
            this.PreviewTextInput -= TextBox_PreviewTextInput;
            this.PreviewKeyDown -= TextBox_PreviewKeyDown;
            this.SourceUpdated -= VBTextBox_SourceUpdated;
            this.TargetUpdated -= VBTextBox_TargetUpdated;
            this.Loaded -= VBTextBox_Loaded;
            this.Unloaded -= VBTextBox_Unloaded;
            this.CommandBindings.Remove(CmdBindingPaste); //handle paste
            this.CommandBindings.Remove(CmdBindingCut); //surpress cut
            CmdBindingPaste = null;
            CmdBindingCut = null;
            _VBContentPropertyInfo = null;

            MaskProvider = null;
            BindingOperations.ClearBinding(this, TextBox.TextProperty);
            //BindingOperations.ClearBinding(this, VBTextBox.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBTextBox.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);
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
        #endregion

        #region Additional Dependency Properties

        #region ControlMode
        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBTextBox), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get { return (Global.ControlModes)GetValue(ControlModeProperty); }
            set 
            { 
                SetValue(ControlModeProperty, value);
            }
        }
        #endregion

        #region IsValueNull
        public static readonly DependencyProperty IsValueNullProperty
            = DependencyProperty.Register("IsValueNull", typeof(bool), typeof(VBTextBox), new PropertyMetadata(false));
        [Category("VBControl")]
        public bool IsValueNull
        {
            get { return (bool)GetValue(IsValueNullProperty); }
            set { SetValue(IsValueNullProperty, value); }
        }
        #endregion

        #region Layout

        /// <summary>
        /// Represents the dependency property for WidthCaption.
        /// </summary>
        public static readonly DependencyProperty WidthCaptionProperty
            = DependencyProperty.Register("WidthCaption", typeof(GridLength), typeof(VBTextBox), new PropertyMetadata(new GridLength(15, GridUnitType.Star)));
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
            = DependencyProperty.Register("WidthCaptionMax", typeof(double), typeof(VBTextBox), new PropertyMetadata((double)150));
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
            = DependencyProperty.Register("WidthContent", typeof(GridLength), typeof(VBTextBox), new PropertyMetadata(new GridLength(20, GridUnitType.Star)));
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
            = DependencyProperty.Register("WidthContentMax", typeof(double), typeof(VBTextBox), new PropertyMetadata(Double.PositiveInfinity));
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
            = DependencyProperty.Register("WidthPadding", typeof(GridLength), typeof(VBTextBox), new PropertyMetadata(new GridLength(0)));
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

        public static readonly DependencyProperty TextAlignmentCaptionProperty
            = DependencyProperty.Register("TextAlignmentCaption", typeof(TextAlignment), typeof(VBTextBox), new PropertyMetadata(TextAlignment.Left));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public TextAlignment TextAlignmentCaption
        {
            get { return (TextAlignment)GetValue(TextAlignmentCaptionProperty); }
            set { SetValue(TextAlignmentCaptionProperty, value); }
        }
        //TextAlignment
        #endregion

        #region Mask-Handling
        #region IncludePrompt

        public static readonly DependencyProperty IncludePromptProperty = DependencyProperty.Register("IncludePrompt", typeof(bool), typeof(VBTextBox), new UIPropertyMetadata(false, OnIncludePromptPropertyChanged));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool IncludePrompt
        {
            get { return (bool)GetValue(IncludePromptProperty); }
            set { SetValue(IncludePromptProperty, value); }
        }

        private static void OnIncludePromptPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBTextBox maskedTextBox = o as VBTextBox;
            if (maskedTextBox != null)
                maskedTextBox.OnIncludePromptChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIncludePromptChanged(bool oldValue, bool newValue)
        {
            ResolveMaskProvider(Mask);
        }

        #endregion //IncludePrompt

        #region IncludeLiterals

        public static readonly DependencyProperty IncludeLiteralsProperty = DependencyProperty.Register("IncludeLiterals", typeof(bool), typeof(VBTextBox), new UIPropertyMetadata(true, OnIncludeLiteralsPropertyChanged));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool IncludeLiterals
        {
            get { return (bool)GetValue(IncludeLiteralsProperty); }
            set { SetValue(IncludeLiteralsProperty, value); }
        }

        private static void OnIncludeLiteralsPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBTextBox maskedTextBox = o as VBTextBox;
            if (maskedTextBox != null)
                maskedTextBox.OnIncludeLiteralsChanged((bool)e.OldValue, (bool)e.NewValue);
        }

        protected virtual void OnIncludeLiteralsChanged(bool oldValue, bool newValue)
        {
            ResolveMaskProvider(Mask);
        }

        #endregion //IncludeLiterals
        #endregion

        #region Mask
        public static readonly DependencyProperty MaskProperty = DependencyProperty.Register("Mask", typeof(string), typeof(VBTextBox), new UIPropertyMetadata(default(String), OnMaskPropertyChanged));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string Mask
        {
            get { return (string)GetValue(MaskProperty); }
            set { SetValue(MaskProperty, value); }
        }

        private static void OnMaskPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VBTextBox maskedTextBox = o as VBTextBox;
            if (maskedTextBox != null)
                maskedTextBox.OnMaskChanged((string)e.OldValue, (string)e.NewValue);
        }

        protected virtual void OnMaskChanged(string oldValue, string newValue)
        {
            ResolveMaskProvider(newValue);
            UpdateText(MaskProvider, 0);
        }

        /// <summary>
        /// Represents the dependency property for StringFormat.
        /// </summary>
        public static readonly DependencyProperty StringFormatProperty;
        /// <summary>
        /// Gets or sets the string format for the control.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt das Stringformat für das Steuerelement.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        #endregion

        #region Caption
        /// <summary>
        /// Represents the dependency property for ACCaption.
        /// </summary>
        public static readonly DependencyProperty ACCaptionProperty
            = DependencyProperty.Register(Const.ACCaptionPrefix, typeof(string), typeof(VBTextBox), new PropertyMetadata(new PropertyChangedCallback(OnACCaptionChanged)));
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get { return (string)GetValue(ACCaptionProperty); }
            set { SetValue(ACCaptionProperty, value); }
        }

        private static void OnACCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IVBContent)
            {
                VBTextBox control = d as VBTextBox;
                if (control.ContextACObject != null)
                {
                    if (!control._Initialized)
                        return;
                    (control as VBTextBox).ACCaptionTrans = control.Root().Environment.TranslateText(control.ContextACObject, control.ACCaption);
                }
            }
        }

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty ACCaptionTransProperty
            = DependencyProperty.Register("ACCaptionTrans", typeof(string), typeof(VBTextBox));
        /// <summary>
        /// Gets or sets the ACCaption translation.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption-Übersetzung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaptionTrans
        {
            get { return (string)GetValue(ACCaptionTransProperty); }
            set { SetValue(ACCaptionTransProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ShowCaption.
        /// </summary>
        public static readonly DependencyProperty ShowCaptionProperty
            = DependencyProperty.Register("ShowCaption", typeof(bool), typeof(VBTextBox), new PropertyMetadata(true));
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

        public static readonly DependencyProperty IsNullableProperty
            = DependencyProperty.Register("IsNullable", typeof(bool), typeof(VBTextBox), new PropertyMetadata(false));
        [Category("VBControl")]
        public bool IsNullable
        {
            get { return (bool)GetValue(IsNullableProperty); }
            set { SetValue(IsNullableProperty, value); }
        }

        #endregion

        public static readonly DependencyProperty OverrideTemplateTriggerProperty = DependencyProperty.Register("OverrideTemplateTrigger", typeof(bool), typeof(VBTextBox));

        public bool OverrideTemplateTrigger
        {
            get { return (bool)GetValue(OverrideTemplateTriggerProperty); }
            set { SetValue(OverrideTemplateTriggerProperty, value); }
        }

        // DAMIR ONLY TEST:
        public bool ResourceKeyTestOn
        {
            get;
            set;
        }

        ///////////////////////////////////////////////

        ///// <summary>
        ///// Represents the dependency property for ACUrlCmdMessage.
        ///// </summary>
        ////public static readonly DependencyProperty ACUrlCmdMessageProperty =
        ////    DependencyProperty.Register("ACUrlCmdMessage",
        ////        typeof(ACUrlCmdMessage), typeof(VBTextBox),
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
                typeof(ACInitState), typeof(VBTextBox),
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
            VBTextBox thisControl = dependencyObject as VBTextBox;
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
            else if (args.Property == ControlModeProperty)
            {
                if (String.IsNullOrEmpty(thisControl.VBContent) && thisControl.ContextACObject == null)
                    thisControl.UpdateControlMode();
            }
        }

        #endregion

        #region IVBContent Member
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

        /// <summary>
        /// Represents the dependency property for VBValidation.
        /// </summary>
        public static readonly DependencyProperty VBValidationProperty = ContentPropertyHandler.VBValidationProperty.AddOwner(typeof(VBTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Name of the VBValidation property.
        /// </summary>
        /// <summary xml:lang="de">
        /// Name der Eigenschaft VBValidation.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBValidation
        {
            get { return (string)GetValue(VBValidationProperty); }
            set { SetValue(VBValidationProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly DependencyProperty DisableContextMenuProperty = ContentPropertyHandler.DisableContextMenuProperty.AddOwner(typeof(VBTextBox), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Determines is context menu disabled or enabled.
        /// </summary>
        /// <summary xml:lang="de">
        /// Ermittelt ist das Kontextmenü deaktiviert oder aktiviert
        /// </summary>
        [Category("VBControl")]
        [ACPropertyInfo(9999)]
        public bool DisableContextMenu
        {
            get { return (bool)GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
        }


        private bool Visible
        {
            get
            {
                return Visibility == System.Windows.Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (RightControlMode > Global.ControlModes.Hidden)
                    {
                        Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    Visibility = Visibility.Hidden;
                }
            }
        }

        /// <summary>
        /// Updates a control mode.
        /// </summary>
        public void UpdateControlMode()
        {
            IACComponent elementACComponent = ContextACObject as IACComponent;
            if (elementACComponent == null)
            {
                if (ReadLocalValue(IsReadOnlyProperty) == DependencyProperty.UnsetValue)
                {
                    if (ControlMode == Global.ControlModes.Disabled)
                    {
                        if (!IsReadOnly)
                            IsReadOnly = true;
                    }
                    else if (ControlMode == Global.ControlModes.Enabled)
                    {
                        if (IsReadOnly)
                            IsReadOnly = false;
                    }
                }
                return;
            }
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
                    if (!IsReadOnly)
                        IsReadOnly = true;
                }
                else
                {
                    if (IsReadOnly)
                        IsReadOnly = false;
                }
            }

            IsValueNull = controlModeInfo.IsNull;
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
        /// Represents the dependency property for RightControlMode.
        /// </summary>
        public static readonly DependencyProperty RightControlModeProperty
            = DependencyProperty.Register("RightControlMode", typeof(Global.ControlModes), typeof(VBTextBox));

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

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty DisabledModesProperty
            = DependencyProperty.Register("DisabledModes", typeof(string), typeof(VBTextBox));
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


        #region IACInteractiveObject Member
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBTextBox));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

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
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBTextBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                    if (query.Any())
                    {
                        ACCommand acCommand = query.First() as ACCommand;
                        ACUrlCommand(acCommand.GetACUrl(), null);
                    }
                    break;
            }
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    {
                        var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                        if (query.Any())
                        {
                            ACCommand acCommand = query.First() as ACCommand;
                            return this.ReflectIsEnabledACUrlCommand(acCommand.GetACUrl(), null);
                        }
                    }
                    break;
            }
            return false;
        }
        #endregion


        #region IACObject Member


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
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                List<IACObject> acContentList = new List<IACObject>();
                ACValueItem acTypedValue = new ACValueItem("", this.Text, VBContentPropertyInfo);
                acContentList.Add(acTypedValue);

                if (!String.IsNullOrEmpty(VBContent))
                {
                    int pos1 = VBContent.LastIndexOf('\\');
                    if ((pos1 != -1) && (ContextACObject != null))
                    {
                        acContentList.Add(ContextACObject.ACUrlCommand(VBContent.Substring(0, pos1)) as IACObject);
                    }
                }

                return acContentList;
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


        #region IACMenuBuilder Member
        /// <summary>
        /// Gets the context menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        ///<returns>Returns the list of ACMenu items.</returns>
        public virtual ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            AppendMenu(vbContent, vbControl, ref acMenuItemList);
            return acMenuItemList;
        }

        /// <summary>
        /// Appends the context menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        ///<param name="acMenuItemList">The acMenuItemList parameter.</param>
        public virtual void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            VBLogicalTreeHelper.AppendMenu(this, vbContent, vbControl, ref acMenuItemList);
            this.GetDesignManagerMenu(VBContent, ref acMenuItemList);
        }
        #endregion


        #region Methods
        [ACMethodInteraction("", "en{'Cut'}de{'Ausschneiden'}", (short)MISort.Cut, false)]
        public new void Cut()
        {
            base.Cut();
        }

        public bool IsEnabledCut()
        {
            return !string.IsNullOrEmpty(this.SelectedText) && !this.IsReadOnly;
        }

        [ACMethodInteraction("", "en{'Copy'}de{'Kopieren'}", (short)MISort.Copy, false)]
        public new void Copy()
        {
            base.Copy();
        }

        public bool IsEnabledCopy()
        {
            return !string.IsNullOrEmpty(this.SelectedText);
        }

        [ACMethodInteraction("", "en{'Paste'}de{'Einfügen'}", (short)MISort.Paste, false)]
        public new void Paste()
        {
            base.Paste();
        }

        public bool IsEnabledPaste()
        {
            return Clipboard.ContainsText() && !this.IsReadOnly;
        }

        [ACMethodInteraction("", "en{'Undo'}de{'Rückgängig'}", (short)MISort.Undo, false)]
        public new void Undo()
        {
            base.Undo();
        }

        public bool IsEnabledUndo()
        {
            return this.CanUndo && !this.IsReadOnly;
        }

        [ACMethodInteraction("", "en{'Redo'}de{'Wiederholen'}", (short)MISort.Redo, false)]
        public new void Redo()
        {
            base.Redo();
        }

        public bool IsEnabledRedo()
        {
            return this.CanRedo && !this.IsReadOnly;
        }

        [ACMethodInteraction("", "en{'Select all'}de{'Alles auswählen'}", (short)MISort.SelectAll, false)]
        public new void SelectAll()
        {
            base.SelectAll();
        }

        [ACMethodInteraction("", "en{'Clear'}de{'Löschen'}", (short)MISort.Clear, false)]
        public new void Clear()
        {
            if (VBContentPropertyInfo == null && TemplatedParent is IClearVBContent)
            {
                (TemplatedParent as IClearVBContent).Clear();
                return;
            }

            if (VBContentPropertyInfo != null)
            {
                if (VBContentPropertyInfo.IsNullable)
                {
                    this.Text = null;
                    var b = BindingOperations.GetBindingExpressionBase(this, TextProperty);
                    if (b != null)
                        b.UpdateSource();
                }
                else
                {
                    Type type = VBContentPropertyInfo.ObjectType;
                    if (type != null && type.IsValueType && ExpressionParser.IsNumericType(type))
                        this.Text = "0";
                    else
                        base.Clear();
                    var b = BindingOperations.GetBindingExpressionBase(this, TextProperty);
                    if (b != null)
                        b.UpdateSource();
                }
            }
            else
                base.Clear();
        }

        public bool IsEnabledClear()
        {
            return this.Visibility == System.Windows.Visibility.Visible;
        }
        #endregion


        #region Event-Handling and Mask-Handling

        protected MaskedTextProvider MaskProvider { get; set; }

        void VBTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }

        //DataObject.AddPastingHandler(control, this.OnCancelCommand);


        /// <summary>
        /// Handles the OnContextMenuOpening event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            if (DisableContextMenu)
            {
                e.Handled = true;
                return;
            }
            base.OnContextMenuOpening(e);
        }


        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (DisableContextMenu)
            {
                e.Handled = true;
                return;
            }
            if (ContextACObject != null)
            {
                Point point = e.GetPosition(this);
                ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, point.X, point.Y, Global.ElementActionType.ContextMenu);
                BSOACComponent.ACAction(actionArgs);
                if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                {
                    VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
                    this.ContextMenu = vbContextMenu;
                    //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
                    if (vbContextMenu.PlacementTarget == null)
                        vbContextMenu.PlacementTarget = this;
                    ContextMenu.IsOpen = true;
                }
                e.Handled = true;
            }
            base.OnPreviewMouseRightButtonDown(e);
        }

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            this.Focus();
            base.OnAccessKey(e);
        }

        void VBTextBox_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            e.Handled = true;
            //SetValue(VBTextBox.TextProperty, ConvertValueToText(e.));
            UpdateControlMode();
        }

        void VBTextBox_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            UpdateControlMode();
            ConvertValueToText(this.Text);
        }

        //protected void SyncTextAndValueProperties(DependencyProperty p, object newValue)
        //{
        //    //prevents recursive syncing properties
        //    if (_isSyncingTextAndValueProperties)
        //        return;

        //    _isSyncingTextAndValueProperties = true;

        //    //this only occures when the user typed in the value
        //    if (InputBase.TextProperty == p)
        //    {
        //        SetValue(VBTextBox.ValueProperty, ConvertTextToValue(newValue.ToString()));
        //    }

        //    SetValue(VBTextBox.TextProperty, ConvertValueToText(newValue));

        //    _isSyncingTextAndValueProperties = false;
        //}

        protected object ConvertTextToValue(string text)
        {
            object convertedValue = null;
            if (VBContentPropertyInfo == null)
                return null;

            Type dataType = VBContentPropertyInfo.ObjectType;

            string valueToConvert = MaskProvider.ToString();

            if (valueToConvert.GetType() == dataType || dataType.IsInstanceOfType(valueToConvert))
            {
                convertedValue = valueToConvert;
            }
#if !VS2008
            else if (String.IsNullOrWhiteSpace(valueToConvert))
            {
                convertedValue = Activator.CreateInstance(dataType);
            }
#else
            else if (String.IsNullOrEmpty(valueToConvert))
            {
                convertedValue = Activator.CreateInstance(dataType);
            }
#endif
            else if (null == convertedValue && valueToConvert is IConvertible)
            {
                convertedValue = Convert.ChangeType(valueToConvert, dataType);
            }

            return convertedValue;
        }

        protected string ConvertValueToText(object value)
        {
            if (value == null)
                value = string.Empty;

            //I have only seen this occur while in Blend, but we need it here so the Blend designer doesn't crash.
            if (MaskProvider == null)
                return value.ToString();

            if (!MaskProvider.Set(value.ToString()))
                MaskProvider.Clear();
            return MaskProvider.ToDisplayString();
        }

        void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //if the text is readonly do not add the text
            if (this.IsReadOnly)
            {
                e.Handled = true;
                return;
            }

            int position = this.SelectionStart;
            MaskedTextProvider provider = MaskProvider;
            if (provider != null)
            {
                if (position < this.Text.Length)
                {
                    position = GetNextCharacterPosition(position);

                    if (Keyboard.IsKeyToggled(Key.Insert))
                    {
                        if (provider.Replace(e.Text, position))
                            position++;
                    }
                    else
                    {
                        if (provider.InsertAt(e.Text, position))
                            position++;
                    }

                    position = GetNextCharacterPosition(position);
                }

                UpdateText(provider, position);
                e.Handled = true;
            }

            base.OnPreviewTextInput(e);
        }

        void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            MaskedTextProvider provider = MaskProvider;
            if (provider != null)
            {
                int position = this.SelectionStart;
                int selectionlength = this.SelectionLength;
                // If no selection use the start position else use end position
                int endposition = (selectionlength == 0) ? position : position + selectionlength - 1;

                if (e.Key == Key.Delete && position < this.Text.Length)//handle the delete key
                {
                    if (provider.RemoveAt(position, endposition))
                        UpdateText(provider, position);

                    e.Handled = true;
                }
                else if (e.Key == Key.Space)
                {
                    if (provider.InsertAt(" ", position))
                        UpdateText(provider, position);
                    e.Handled = true;
                }
                else if (e.Key == Key.Back)//handle the back space
                {
                    if ((position > 0) && (selectionlength == 0))
                    {
                        position--;
                        if (provider.RemoveAt(position))
                            UpdateText(provider, position);
                    }

                    if (selectionlength != 0)
                    {
                        if (provider.RemoveAt(position, endposition))
                        {
                            if (position > 0)
                                position--;

                            UpdateText(provider, position);
                        }
                    }

                    e.Handled = true;
                }
            }

            base.OnPreviewKeyDown(e);
        }

        private void UpdateText(MaskedTextProvider provider, int position)
        {
            if (provider == null)
            {
                return;
                //throw new ArgumentNullException("MaskedTextProvider", "Mask cannot be null.");
            }

            Text = provider.ToDisplayString();

            this.SelectionStart = position;
        }

        private int GetNextCharacterPosition(int startPosition)
        {
            int position = MaskProvider.FindEditPositionFrom(startPosition, true);
            return position == -1 ? startPosition : position;
        }

        private void ResolveMaskProvider(string mask)
        {
            //do not create a mask provider if the Mask is empty, which can occur if the IncludePrompt and IncludeLiterals properties
            //are set prior to the Mask.
            if (String.IsNullOrEmpty(mask))
                return;

            MaskProvider = new MaskedTextProvider(mask, System.Globalization.CultureInfo.CurrentCulture)
            {
                IncludePrompt = this.IncludePrompt,
                IncludeLiterals = this.IncludeLiterals
            };
        }


        private void TextBox_Paste(object sender, RoutedEventArgs e)
        {
            MaskedTextProvider provider = MaskProvider;
            if (provider != null)
            {
                int position = this.SelectionStart;

                object data = Clipboard.GetData(DataFormats.Text);
                if (data != null)
                {
                    string text = data.ToString().Trim();
                    if (text.Length > 0)
                    {
                        provider.Set(text);
                        UpdateText(provider, position);
                    }
                }
            }
            else
            {
                object data = Clipboard.GetData(DataFormats.Text);
                if (data != null)
                {
                    string pastText = data.ToString().Trim();
                    if (!string.IsNullOrEmpty(pastText))
                    {
                        if (string.IsNullOrEmpty(this.Text))
                        {
                            this.Text = pastText;
                        }
                        else
                        {
                            string pre = this.SelectionStart > 0 ? this.Text.Substring(0, this.SelectionStart) : "";
                            string post = this.Text.Substring(this.SelectionStart + this.SelectionLength);
                            this.Text = pre + pastText + post;
                            this.SelectionStart = (pre + pastText).Length;
                            this.SelectionLength = 0;
                        }
                    }
                }
            }
        }

        private void TextBox_CanCut(object sender, CanExecuteRoutedEventArgs e)
        {
            MaskedTextProvider provider = MaskProvider;
            if (provider != null)
            {
                e.CanExecute = false;
                e.Handled = true;
            }
        }

        #endregion

        #region WPF-Design
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var b = BindingOperations.GetBindingExpressionBase(this, TextProperty);
                if (b != null)
                {
                    b.UpdateSource();
                }
                SelectAll();
            }
            else if (e.Key == Key.Escape)
            {
                var b = BindingOperations.GetBindingExpressionBase(this, TextProperty);
                if (b != null)
                {
                    b.UpdateTarget();
                }
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
}
