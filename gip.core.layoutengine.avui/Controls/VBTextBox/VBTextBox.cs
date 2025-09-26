using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Labs.Input;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace gip.core.layoutengine.avui
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
        static VBTextBox()
        {
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner<VBTextBox>();
        }

        public VBTextBox()
        {
            this.Focusable = true;
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            GotFocus += VBTextBox_GotFocus;
            this.TextInput += TextBox_PreviewTextInput;
            this.KeyDown += TextBox_PreviewKeyDown;
            Loaded += VBTextBox_Loaded;
            Unloaded += VBTextBox_Unloaded;

            CmdBindingPaste = new CommandBinding();
            CmdBindingPaste.Command = ApplicationCommands.Paste;
            CmdBindingPaste.Executed += TextBox_Paste;
            CmdBindingCut = new CommandBinding();
            CmdBindingCut.Command = ApplicationCommands.Cut;
            CmdBindingCut.CanExecute += TextBox_CanCut;
            CommandManager.SetCommandBindings(this, new List<CommandBinding> { CmdBindingPaste, CmdBindingCut });
            ResolveMaskProvider(Mask);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
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
            IBinding binding = null;
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
                binding = new Binding
                {
                    Path = VBContent
                };
                this.Bind(TextBox.TextProperty, binding);
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

            ProxyACRefConverter refConverter = ProxyACRefConverter.IfACRefGenerateConverter(out var tempBinding, ref dcACTypeInfo, ref dcPath, ref dcSource, ref dcRightControlMode);
            binding = tempBinding;

            _VBContentPropertyInfo = dcACTypeInfo;
            if (VBContentPropertyInfo == null)
            {
                this.Root().Messages.LogDebug("Error00003", "VBTextBox", String.Format("VBContent {0} is not  property",VBContent));
                return;
            }

            // Check if RightControlMode is locally set
            if (   !this.IsSet(VBTextBox.RightControlModeProperty)
                || RightControlMode < dcRightControlMode)
            {
                RightControlMode = dcRightControlMode;
            }

            bool isNumericValueBound = false;
            if (dcACTypeInfo != null)
                isNumericValueBound = TypeAnalyser.IsNumericType(dcACTypeInfo.ObjectType);

            // Check if TextAlignment is locally set
            if (isNumericValueBound && !this.IsSet(TextBox.TextAlignmentProperty))
                TextAlignment = TextAlignment.Right;

            // VBContent muß im XAML gestettet sein
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
                var concreteBinding = new Binding
                {
                    Source = dcSource,
                    Path = dcPath,
                    Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay
                };
                
                if (!String.IsNullOrEmpty(VBValidation))
                    _ValidationRule = new VBValidationRule(null, true, ContextACObject, VBContent, VBValidation);

                if (!String.IsNullOrEmpty(StringFormat))
                    concreteBinding.StringFormat = StringFormat;
                
                binding = concreteBinding;
            }
            else
                isNumericValueBound = false;

            this.Bind(TextBox.TextProperty, binding);

            if (BSOACComponent != null)
            {
                var initStateBinding = new Binding
                {
                    Source = BSOACComponent,
                    Path = Const.InitState,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBTextBox.ACCompInitStateProperty, initStateBinding);
            }

            if (ContextACObject != null && ContextACObject is IACComponent)
            {
                var urlCmdBinding = new Binding
                {
                    Source = ContextACObject,
                    Path = Const.ACUrlCmdMessage,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBTextBox.ACUrlCmdMessageProperty, urlCmdBinding);
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
                                foreach (var setter in styleFromDictionary.Setters)
                                {
                                    if (setter is Setter)
                                    {
                                        var s = setter as Setter;
                                        if (s.Value is IBinding)
                                            this.Bind(s.Property, s.Value as IBinding);
                                        else
                                            SetValue(s.Property, s.Value);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Set tooltip if needed
            string tooltip = ToolTip.GetTip(this) as string;
            //if (!this.IsSet(TextBox.TextAlignmentProperty))
            if (!String.IsNullOrEmpty(tooltip))
            {
                ToolTip.SetTip(this, this.Root().Environment.TranslateText(ContextACObject, tooltip));
            }
            if (isNumericValueBound && !String.IsNullOrEmpty(this.StringFormat))
            {
                var tooltipBinding = new Binding
                {
                    Source = dcSource,
                    Path = dcPath,
                    Mode = BindingMode.OneWay
                };
                this.Bind(ToolTip.TipProperty, tooltipBinding);
            }

            if (AutoFocus)
            {
                try
                {
                    Focus();
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
                var bindingexp = BindingOperations.GetBindingExpressionBase(this, TextBox.TextProperty);
                if (bindingexp != null)
                {
                    IACObject boundToObject = bindingexp.GetSource() as IACObject;
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
            this.TextInput -= TextBox_PreviewTextInput;
            this.KeyDown -= TextBox_PreviewKeyDown;
            this.Loaded -= VBTextBox_Loaded;
            this.Unloaded -= VBTextBox_Unloaded;

            // TODO:
            //this.CommandBindings.Remove(CmdBindingPaste); //handle paste
            //this.CommandBindings.Remove(CmdBindingCut); //surpress cut

            _VBContentPropertyInfo = null;

            MaskProvider = null;
            _ValidationRule = null;
            this.ClearAllBindings();
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
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBTextBox, Global.ControlModes>(nameof(ControlMode));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get { return GetValue(ControlModeProperty); }
            set 
            { 
                SetValue(ControlModeProperty, value);
            }
        }
        #endregion

        #region IsValueNull
        public static readonly StyledProperty<bool> IsValueNullProperty =
            AvaloniaProperty.Register<VBTextBox, bool>(nameof(IsValueNull), false);
        [Category("VBControl")]
        public bool IsValueNull
        {
            get { return GetValue(IsValueNullProperty); }
            set { SetValue(IsValueNullProperty, value); }
        }
        #endregion

        #region Layout

        /// <summary>
        /// Represents the dependency property for WidthCaption.
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthCaptionProperty =
            AvaloniaProperty.Register<VBTextBox, GridLength>(nameof(WidthCaption), new GridLength(15, GridUnitType.Star));
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
            get { return GetValue(WidthCaptionProperty); }
            set { SetValue(WidthCaptionProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthCaptionMax.
        /// </summary>
        public static readonly StyledProperty<double> WidthCaptionMaxProperty =
            AvaloniaProperty.Register<VBTextBox, double>(nameof(WidthCaptionMax), 150.0);
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
            get { return GetValue(WidthCaptionMaxProperty); }
            set { SetValue(WidthCaptionMaxProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthContent.
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthContentProperty =
            AvaloniaProperty.Register<VBTextBox, GridLength>(nameof(WidthContent), new GridLength(20, GridUnitType.Star));
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
            get { return GetValue(WidthContentProperty); }
            set { SetValue(WidthContentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthContentMax.
        /// </summary>
        public static readonly StyledProperty<double> WidthContentMaxProperty =
            AvaloniaProperty.Register<VBTextBox, double>(nameof(WidthContentMax), Double.PositiveInfinity);
        /// <summary>
        /// Gets or sets the maximum width of content.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthContentMax
        {
            get { return GetValue(WidthContentMaxProperty); }
            set { SetValue(WidthContentMaxProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for WidthPadding.
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthPaddingProperty =
            AvaloniaProperty.Register<VBTextBox, GridLength>(nameof(WidthPadding), new GridLength(0));
        /// <summary>
        /// Gets or sets the width of padding.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthPadding
        {
            get { return GetValue(WidthPaddingProperty); }
            set { SetValue(WidthPaddingProperty, value); }
        }

        public static readonly StyledProperty<TextAlignment> TextAlignmentCaptionProperty =
            AvaloniaProperty.Register<VBTextBox, TextAlignment>(nameof(TextAlignmentCaption), TextAlignment.Left);
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public TextAlignment TextAlignmentCaption
        {
            get { return GetValue(TextAlignmentCaptionProperty); }
            set { SetValue(TextAlignmentCaptionProperty, value); }
        }
        #endregion

        #region Mask-Handling
        #region IncludePrompt

        public static readonly StyledProperty<bool> IncludePromptProperty = 
            AvaloniaProperty.Register<VBTextBox, bool>(nameof(IncludePrompt), false);
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool IncludePrompt
        {
            get { return GetValue(IncludePromptProperty); }
            set { SetValue(IncludePromptProperty, value); }
        }

        protected virtual void OnIncludePromptChanged(bool oldValue, bool newValue)
        {
            ResolveMaskProvider(Mask);
        }

        #endregion //IncludePrompt

        #region IncludeLiterals

        public static readonly StyledProperty<bool> IncludeLiteralsProperty = 
            AvaloniaProperty.Register<VBTextBox, bool>(nameof(IncludeLiterals), true);
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool IncludeLiterals
        {
            get { return GetValue(IncludeLiteralsProperty); }
            set { SetValue(IncludeLiteralsProperty, value); }
        }

        protected virtual void OnIncludeLiteralsChanged(bool oldValue, bool newValue)
        {
            ResolveMaskProvider(Mask);
        }

        #endregion //IncludeLiterals
        #endregion

        #region Mask
        public static readonly StyledProperty<string> MaskProperty = 
            AvaloniaProperty.Register<VBTextBox, string>(nameof(Mask), default(String));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string Mask
        {
            get { return GetValue(MaskProperty); }
            set { SetValue(MaskProperty, value); }
        }

        protected virtual void OnMaskChanged(string oldValue, string newValue)
        {
            ResolveMaskProvider(newValue);
            UpdateText(MaskProvider, 0);
        }

        /// <summary>
        /// Represents the dependency property for StringFormat.
        /// </summary>
        public static readonly AttachedProperty<string> StringFormatProperty;
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
            get { return GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        #endregion

        #region Caption
        /// <summary>
        /// Represents the dependency property for ACCaption.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionProperty =
            AvaloniaProperty.Register<VBTextBox, string>(Const.ACCaptionPrefix);
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get { return GetValue(ACCaptionProperty); }
            set { SetValue(ACCaptionProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionTransProperty =
            AvaloniaProperty.Register<VBTextBox, string>(nameof(ACCaptionTrans));
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
            get { return GetValue(ACCaptionTransProperty); }
            set { SetValue(ACCaptionTransProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ShowCaption.
        /// </summary>
        public static readonly StyledProperty<bool> ShowCaptionProperty =
            AvaloniaProperty.Register<VBTextBox, bool>(nameof(ShowCaption), true);
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
            get { return GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        public static readonly StyledProperty<bool> IsNullableProperty =
            AvaloniaProperty.Register<VBTextBox, bool>(nameof(IsNullable), false);
        [Category("VBControl")]
        public bool IsNullable
        {
            get { return GetValue(IsNullableProperty); }
            set { SetValue(IsNullableProperty, value); }
        }

        #endregion

        public static readonly StyledProperty<bool> OverrideTemplateTriggerProperty = 
            AvaloniaProperty.Register<VBTextBox, bool>(nameof(OverrideTemplateTrigger));

        public bool OverrideTemplateTrigger
        {
            get { return GetValue(OverrideTemplateTriggerProperty); }
            set { SetValue(OverrideTemplateTriggerProperty, value); }
        }

        // DAMIR ONLY TEST:
        public bool ResourceKeyTestOn
        {
            get;
            set;
        }

        ///////////////////////////////////////////////

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBTextBox, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBTextBox, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        private bool _isHandlingTextInput = false;
        protected override void OnTextInput(TextInputEventArgs e)
        {
            try
            {
                _isHandlingTextInput = true;
                base.OnTextInput(e);
            }
            finally
            {
                _isHandlingTextInput = false;
            }
            _isHandlingTextInput = true;
        }

        protected override void UpdateDataValidation(AvaloniaProperty property, BindingValueType state, Exception error)
        {

        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            VBTextBox thisControl = this;
            if (change.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        thisControl.DeInitVBControl(bso);
                }
            }
            else if (change.Property == ControlModeProperty)
            {
                if (String.IsNullOrEmpty(thisControl.VBContent) && thisControl.ContextACObject == null)
                    thisControl.UpdateControlMode();
            }
            else if (change.Property == ACUrlCmdMessageProperty)
            {
                thisControl.OnACUrlMessageReceived();
            }
            else if (change.Property == StringFormatProperty)
            {
                // Note: Avalonia handles binding updates differently
                // This would need to be implemented differently in Avalonia
            }
            else if (change.Property == IncludePromptProperty)
            {
                OnIncludePromptChanged((bool)change.OldValue, (bool)change.NewValue);
            }
            else if (change.Property == IncludeLiteralsProperty)
            {
                OnIncludeLiteralsChanged((bool)change.OldValue, (bool)change.NewValue);
            }
            else if (change.Property == MaskProperty)
            {
                OnMaskChanged(change.OldValue as string, change.NewValue as string);
            }
            else if (change.Property == ACCaptionProperty)
            {
                if (ContextACObject != null)
                {
                    if (!_Initialized)
                        return;
                    ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
                }
            }
            else if (change.Property == TextProperty)
            {
                // From Target to Source
                if (_isHandlingTextInput)
                {
                    VBTextBox_TargetUpdated(change.Sender, change);
                }
                // From Source to Target
                else
                {
                    VBTextBox_SourceUpdated(change.Sender, change);
                }
                //change.NewValue
            }
        }

        /// <summary>
        /// Handles the ACUrl message when it is received.
        /// </summary>
        public void OnACUrlMessageReceived()
        {
            if (ACUrlCmdMessage != null
                && ACUrlCmdMessage.ACUrl == Const.CmdFocusAndSelectAll
                && !String.IsNullOrEmpty(this.VBContent)
                && !String.IsNullOrEmpty(ACUrlCmdMessage.TargetVBContent)
                && (this.VBContent == ACUrlCmdMessage.TargetVBContent
                   || this.VBContent.Contains(ACUrlCmdMessage.TargetVBContent)))
            {
                Focus();
                SelectAll();
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
        public static readonly AttachedProperty<string> VBValidationProperty = 
            ContentPropertyHandler.VBValidationProperty.AddOwner<VBTextBox>();
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
            get { return GetValue(VBValidationProperty); }
            set { SetValue(VBValidationProperty, value); }
        }
        private VBValidationRule _ValidationRule = null;

        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly AttachedProperty<bool> DisableContextMenuProperty = 
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBTextBox>();
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
            get { return GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
        }


        private bool Visible
        {
            get
            {
                return IsVisible;
            }
            set
            {
                if (value)
                {
                    if (RightControlMode > Global.ControlModes.Hidden)
                    {
                        IsVisible = true;
                    }
                }
                else
                {
                    IsVisible = false;
                }
            }
        }

        /// <summary>
        /// Updates a control mode.
        /// </summary>
        public void UpdateControlMode()
        {
            if (AutoScrollToEnd)
                this.ScrollToEnd();
            IACComponent elementACComponent = ContextACObject as IACComponent;
            if (elementACComponent == null)
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
                if (!IsVisible)
                    IsVisible = false;
            }
            else if (controlMode == Global.ControlModes.Hidden)
            {
                if (IsVisible)
                    IsVisible = false;
            }
            else
            {
                if (!IsVisible)
                    IsVisible = true;
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

            // TODO: Convert this to Avalonia equivalent when available
            // RemoteCommandAdornerManager.Instance.VisualizeIfRemoteControlled(this, elementACComponent, false);
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
        public static readonly StyledProperty<Global.ControlModes> RightControlModeProperty =
            AvaloniaProperty.Register<VBTextBox, Global.ControlModes>(nameof(RightControlMode));

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
            get { return GetValue(RightControlModeProperty); }
            set { SetValue(RightControlModeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty =
            AvaloniaProperty.Register<VBTextBox, string>(nameof(DisabledModes));
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
            get { return GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
        }

        public static readonly StyledProperty<bool> AutoScrollToEndProperty = 
            AvaloniaProperty.Register<VBTextBox, bool>(nameof(AutoScrollToEnd));
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool AutoScrollToEnd
        {
            get { return GetValue(AutoScrollToEndProperty); }
            set { SetValue(AutoScrollToEndProperty, value); }
        }
        #endregion


        #region IACInteractiveObject Member
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBTextBox, string>(nameof(VBContent));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
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
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBTextBox>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
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
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            return clipboard?.GetTextAsync().Result != null && !this.IsReadOnly;
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
                    // In Avalonia, we need to update the binding differently
                    // This would need to be implemented
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
            return this.IsVisible;
        }
        #endregion


        #region Event-Handling and Mask-Handling

        protected MaskedTextProvider MaskProvider { get; set; }

        void VBTextBox_GotFocus(object sender, GotFocusEventArgs e)
        {
            SelectAll();
        }


        /// <summary>
        /// Handles the OnContextMenuOpening event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
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
                        ContextMenu = vbContextMenu;
                        //@ihrastinski NOTE: Remote desktop context menu problem - added placement target
                        if (vbContextMenu.PlacementTarget == null)
                             vbContextMenu.PlacementTarget = this;
                        ContextMenu.Open();
                    }
                    e.Handled = true;
                }
            }
            base.OnPointerReleased(e);
        }

        protected override void OnAccessKey(RoutedEventArgs e)
        {
            base.OnAccessKey(e);
            this.Focus();
        }

        void VBTextBox_SourceUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            UpdateControlMode();
        }

        void VBTextBox_TargetUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_ValidationRule != null)
                _ValidationRule.Validate(this, e.NewValue, System.Globalization.CultureInfo.CurrentUICulture);
            UpdateControlMode();
            ConvertValueToText(this.Text);
        }

        protected object ConvertTextToValue(string text)
        {
            object convertedValue = null;
            if (VBContentPropertyInfo == null)
                return null;

            Type dataType = VBContentPropertyInfo.ObjectType;

            string valueToConvert = MaskProvider?.ToString() ?? text;

            if (valueToConvert?.GetType() == dataType || dataType.IsInstanceOfType(valueToConvert))
            {
                convertedValue = valueToConvert;
            }
            else if (String.IsNullOrWhiteSpace(valueToConvert))
            {
                if (dataType.IsValueType)
                    convertedValue = Activator.CreateInstance(dataType);
            }
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

        void TextBox_PreviewTextInput(object sender, TextInputEventArgs e)
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

                    // Note: Avalonia doesn't have Insert key toggle behavior like WPF
                    if (provider.InsertAt(e.Text, position))
                        position++;

                    position = GetNextCharacterPosition(position);
                }

                UpdateText(provider, position);
                e.Handled = true;
            }
        }

        void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            MaskedTextProvider provider = MaskProvider;
            if (provider != null)
            {
                int position = this.SelectionStart;
                int selectionlength = this.SelectionEnd - this.SelectionStart;
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
        }

        private void UpdateText(MaskedTextProvider provider, int position)
        {
            if (provider == null)
            {
                return;
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


        private void TextBox_Paste(object sender, Avalonia.Labs.Input.ExecutedRoutedEventArgs e)
        {
            MaskedTextProvider provider = MaskProvider;
            if (provider != null)
            {
                int position = this.SelectionStart;

                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                var text = clipboard?.GetTextAsync().Result?.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    provider.Set(text);
                    UpdateText(provider, position);
                }
            }
            else
            {
                var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
                var pastText = clipboard?.GetTextAsync().Result?.Trim();
                if (!string.IsNullOrEmpty(pastText))
                {
                    if (string.IsNullOrEmpty(this.Text))
                    {
                        this.Text = pastText;
                    }
                    else
                    {
                        string pre = this.SelectionStart > 0 ? this.Text.Substring(0, this.SelectionStart) : "";
                        string post = this.Text.Substring(this.SelectionStart + (this.SelectionEnd - this.SelectionStart));
                        this.Text = pre + pastText + post;
                        this.SelectionStart = (pre + pastText).Length;
                        this.SelectionEnd = this.SelectionStart;
                    }
                }
            }
        }


        private void TextBox_CanCut(object sender, Avalonia.Labs.Input.CanExecuteRoutedEventArgs e)
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

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        #endregion
    }
}
