using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control for date and time selection.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelemet für Datum- und Zeitsauswahl.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDateTimePicker'}de{'VBDateTimePicker'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBDateTimePicker : TemplatedControl, IVBContent, IACMenuBuilderWPFTree, IACObject, IClearVBContent
    {
        #region c'tors
        /// <summary>
        /// Creates a new instance of VBDateTimePicker.
        /// </summary>
        public VBDateTimePicker() : base()
        {
            AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            AddHandler(PointerReleasedEvent, OnMouseDownOutsideCapturedElement, RoutingStrategies.Tunnel);
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            Loaded += VBDateTimePicker_Loaded;
            Unloaded += VBDateTimePicker_Unloaded;
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _calendar = e.NameScope.Find<Calendar>("PART_Calendar");
            if (_calendar != null)
            {
                _calendar.SelectedDatesChanged += Calendar_SelectedDatesChanged;
                _calendar.SelectedDate = SelectedDate;
            }
            InitVBControl();
        }

        #endregion

        #region Members

        private Calendar _calendar;

        #endregion //Members

        #region Properties

        #region Format

        /// <summary>
        /// Represents the dependency property for Format.
        /// </summary>
        public static readonly StyledProperty<DateTimeFormat> FormatProperty = 
            AvaloniaProperty.Register<VBDateTimePicker, DateTimeFormat>(nameof(Format), DateTimeFormat.FullDateTime);
        /// <summary>
        /// Gets or sets the DateTime format according DateTimeFormat enumeration.
        /// </summary>
        [Category("VBControl")]
        public DateTimeFormat Format
        {
            get { return GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        protected virtual void OnFormatChanged(DateTimeFormat oldValue, DateTimeFormat newValue)
        {

        }

        #endregion //Format

        #region FormatString

        /// <summary>
        /// Represents the dependency property for FormatString.
        /// </summary>
        public static readonly StyledProperty<string> FormatStringProperty = 
            AvaloniaProperty.Register<VBDateTimePicker, string>(nameof(FormatString), default(String));

        /// <summary>
        /// Gets or sets the format string.
        /// </summary>
        [Category("VBControl")]
        public string FormatString
        {
            get { return GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        protected virtual void OnFormatStringChanged(string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
                throw new ArgumentException("CustomFormat should be specified.", FormatString);
        }

        #endregion //FormatString

        #region IsOpen

        /// <summary>
        /// Represents the dependency property for IsOpen.
        /// </summary>
        public static readonly StyledProperty<bool> IsOpenProperty = 
            AvaloniaProperty.Register<VBDateTimePicker, bool>(nameof(IsOpen), false);

        /// <summary>
        /// Determines is automatically opens the date and time selector dialog or not.
        /// </summary>
        [Category("VBControl")]
        public bool IsOpen
        {
            get { return GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        #endregion //IsOpen

        #region SelectedDate

        /// <summary>
        /// Represents the dependency property for SelectedDate.
        /// </summary>
        public static readonly StyledProperty<DateTime?> SelectedDateProperty = 
            AvaloniaProperty.Register<VBDateTimePicker, DateTime?>(nameof(SelectedDate), DateTime.Now, 
                coerce: OnCoerceSelectedDate);

        /// <summary>
        /// Gets or sets the selected date.
        /// </summary>
        public DateTime? SelectedDate
        {
            get { return GetValue(SelectedDateProperty); }
            set { SetValue(SelectedDateProperty, value); }
        }

        private static DateTime? OnCoerceSelectedDate(AvaloniaObject o, DateTime? value)
        {
            VBDateTimePicker dateTimePicker = o as VBDateTimePicker;
            if (dateTimePicker != null)
                return dateTimePicker.OnCoerceSelectedDate(value);
            else
                return value;
        }

        protected virtual DateTime? OnCoerceSelectedDate(DateTime? value)
        {
            // TODO: Keep the proposed value within the desired range.
            return value;
        }

        protected virtual void OnSelectedDateChanged(DateTime? oldValue, DateTime? newValue)
        {
            if (newValue == null || oldValue == null || _InitBinding)
                return;
            if (_calendar != null && _calendar.SelectedDate != null && _calendar.SelectedDate.Value != newValue.Value)
                _calendar.SelectedDate = newValue;
        }

        #endregion //SelectedDate

        #region DisplayDateStart
        /// <summary>
        /// Represents the dependency property for DisplayDateStart.
        /// </summary>
        public static readonly StyledProperty<DateTime?> DisplayDateStartProperty = 
            AvaloniaProperty.Register<VBDateTimePicker, DateTime?>(nameof(DisplayDateStart), new DateTime(2000, 1, 1));

        /// <summary>
        /// Gets or sets the initial date and time.
        /// </summary>
        public DateTime? DisplayDateStart
        {
            get { return GetValue(DisplayDateStartProperty); }
            set { SetValue(DisplayDateStartProperty, value); }
        }
        #endregion

        #endregion //Properties

        #region Additional Dependency-Properties
        /// <summary>
        /// Represents the dependency property for RightControlMode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> RightControlModeProperty =
            AvaloniaProperty.Register<VBDateTimePicker, Global.ControlModes>(nameof(RightControlMode));

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
        /// Represents the dependency property for ACCaption.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionProperty =
            AvaloniaProperty.Register<VBDateTimePicker, string>(Const.ACCaptionPrefix);
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
            AvaloniaProperty.Register<VBDateTimePicker, string>(nameof(ACCaptionTrans));
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
            AvaloniaProperty.Register<VBDateTimePicker, bool>(nameof(ShowCaption), true);
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

        #region Layout
        /// <summary>
        /// Represents the dependency property for WidthCaption.
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthCaptionProperty =
            AvaloniaProperty.Register<VBDateTimePicker, GridLength>(nameof(WidthCaption), new GridLength(15, GridUnitType.Star));
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
            AvaloniaProperty.Register<VBDateTimePicker, double>(nameof(WidthCaptionMax), 150.0);
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
            AvaloniaProperty.Register<VBDateTimePicker, GridLength>(nameof(WidthContent), new GridLength(20, GridUnitType.Star));
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
            AvaloniaProperty.Register<VBDateTimePicker, double>(nameof(WidthContentMax), Double.PositiveInfinity);
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
            AvaloniaProperty.Register<VBDateTimePicker, GridLength>(nameof(WidthPadding), new GridLength(0));
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

        /// <summary>
        /// Updates source if is indexer.
        /// </summary>
        public bool UpdateSourceIfIndexer { get; set; }
        #endregion
        #endregion

        #region Loaded-Event
        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;
        protected bool _InitBinding = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized || DataContext == null)
                return;
            _Initialized = true;
            if (String.IsNullOrEmpty(VBContent))
            {
                return;
            }
            else if (ContextACObject == null)
            {
                var binding = new Binding
                {
                    Path = VBContent
                };
                _InitBinding = true;
                this.Bind(VBDateTimePicker.SelectedDateProperty, binding);
                _InitBinding = false;
                return;
            }

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBDateTimePicker", VBContent);
                return;
            }
            _VBContentPropertyInfo = dcACTypeInfo;

            // Check if RightControlMode is locally set
            if (!this.IsSet(VBDateTimePicker.RightControlModeProperty) || (dcRightControlMode < RightControlMode))
            {
                RightControlMode = dcRightControlMode;
            }

            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            if (VBContentPropertyInfo != null)
            {
                if (string.IsNullOrEmpty(ACCaption))
                    ACCaptionTrans = VBContentPropertyInfo.ACCaption;
                else
                    ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);

                var binding2 = new Binding
                {
                    Source = dcSource,
                    Path = dcPath,
                    Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay
                };
                
                if (dcPath.IndexOf('[') >= 0)
                    UpdateSourceIfIndexer = true;
                
                if (!String.IsNullOrEmpty(VBValidation))
                    _ValidationRule = new VBValidationRule(null, true, ContextACObject, VBContent, VBValidation);

                _InitBinding = true;
                this.Bind(VBDateTimePicker.SelectedDateProperty, binding2);
                _InitBinding = false;
            }

            if (BSOACComponent != null)
            {
                var binding = new Binding
                {
                    Source = BSOACComponent,
                    Path = Const.InitState,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBDateTimePicker.ACCompInitStateProperty, binding);
            }

            if (AutoFocus)
            {
                Focus();
            }
        }

        bool _Loaded = false;
        void VBDateTimePicker_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null)
            {
                var boundedValue = BindingOperations.GetBindingExpressionBase(this, VBDateTimePicker.SelectedDateProperty);
                if (boundedValue != null)
                {
                    IACObject boundToObject = boundedValue.GetSource() as IACObject;
                    try
                    {
                        if (boundToObject != null)
                            BSOACComponent.AddWPFRef(this.GetHashCode(), boundToObject);
                    }
                    catch (Exception exw)
                    {
                        this.Root().Messages.LogDebug("VBDateTimePicker", "AddWPFRef", exw.Message);
                    }
                }
            }
            _Loaded = true;
        }

        void VBDateTimePicker_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_Loaded)
                return;

            if (BSOACComponent != null)
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
            
            Loaded -= VBDateTimePicker_Loaded;
            Unloaded -= VBDateTimePicker_Unloaded;
            _VBContentPropertyInfo = null;
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

        void VB_SourceUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            UpdateControlMode();
        }

        void VB_TargetUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_ValidationRule != null)
                _ValidationRule.Validate(this, e.NewValue, System.Globalization.CultureInfo.CurrentUICulture);
            UpdateControlMode();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == ACCompInitStateProperty)
                InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        DeInitVBControl(bso);
                }
            }
            else if (change.Property == RightControlModeProperty)
            {
                UpdateControlMode();
            }
            else if (change.Property == FormatProperty)
            {
                OnFormatChanged((DateTimeFormat)change.OldValue, (DateTimeFormat)change.NewValue);
            }
            else if (change.Property == FormatStringProperty)
            {
                OnFormatStringChanged(change.OldValue as string, change.NewValue as string);
            }
            else if (change.Property == SelectedDateProperty)
            {
                OnSelectedDateChanged(change.OldValue as DateTime?, change.NewValue as DateTime?);
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
            VB_TargetUpdated(null, change);
        }
        #endregion

        #region Event Handlers

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                case Key.Tab:
                    {
                        CloseDateTimePicker();
                        break;
                    }
            }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, PointerReleasedEventArgs e)
        {
            CloseDateTimePicker();
        }

        void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var newDate = (DateTime?)e.AddedItems[0];
                if (   (newDate.HasValue && SelectedDate.HasValue && newDate.Value != SelectedDate.Value)
                    || (newDate.HasValue && !SelectedDate.HasValue)
                    || (!newDate.HasValue && SelectedDate.HasValue))
                    SelectedDate = newDate.Value;
                if (UpdateSourceIfIndexer)
                {
                    var expression = BindingOperations.GetBindingExpressionBase(this, VBDateTimePicker.SelectedDateProperty);
                    if (expression != null)
                    {
                        expression.UpdateSource();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the OnPointerReleased event for context menu.
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
                        if (vbContextMenu.PlacementTarget == null)
                            vbContextMenu.PlacementTarget = this;
                        ContextMenu.Open();
                    }
                    e.Handled = true;
                }
            }
            base.OnPointerReleased(e);
        }

        /// <summary>
        /// Handles the OnPointerPressed event for double click.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                {
                    this.SelectedDate = null;
                }
                else
                {
                    this.SelectedDate = DateTime.Now;
                }
            }
            base.OnPointerPressed(e);
        }

        #endregion //Event Handlers

        #region Methods

        private void CloseDateTimePicker()
        {
            if (IsOpen)
                IsOpen = false;
            // Note: ReleaseMouseCapture doesn't exist in Avalonia - handled differently
        }


        public void Clear()
        {
            if (VBContentPropertyInfo == null)
                return;
            if (VBContentPropertyInfo.IsNullable)
                this.SelectedDate = null;
            else
                this.SelectedDate = DateTime.MinValue;
            var b = BindingOperations.GetBindingExpressionBase(this, SelectedDateProperty);
            if (b != null)
                b.UpdateSource();
        }


        #endregion //Methods

        #region IDataField Members

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBDateTimePicker, string>(nameof(VBContent));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
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
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBDateTimePicker>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }


        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBDateTimePicker, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
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
                ACValueItem acTypedValue = new ACValueItem("", SelectedDate, VBContentPropertyInfo);
                acContentList.Add(acTypedValue);

                if (!string.IsNullOrEmpty(VBContent))
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
        /// Represents the dependency property for VBValidation.
        /// </summary>
        public static readonly AttachedProperty<string> VBValidationProperty = 
            ContentPropertyHandler.VBValidationProperty.AddOwner<VBDateTimePicker>();
        /// <summary>
        /// Name of the VBValidation property.
        /// </summary>
        /// <summary xml:lang="de">
        /// Name der Eigenschaft VBValidation.
        /// </summary>
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
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBDateTimePicker>();
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
                if (this.IsVisible)
                    this.IsVisible = false;
            }
            else if (controlMode == Global.ControlModes.Hidden)
            {
                if (this.IsVisible)
                    this.IsVisible = false;
            }
            else
            {
                if (!this.IsVisible)
                    this.IsVisible = true;
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
            RemoteCommandAdornerManager.Instance.VisualizeIfRemoteControlled(this, elementACComponent, false);
        }

        /// <summary>
        /// Enables or disables auto focus.
        /// </summary>
        /// <summary xml:lang="de">
        /// Aktiviert oder deaktiviert den Autofokus.
        /// </summary>
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
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBDateTimePicker, Global.ControlModes>(nameof(ControlMode));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get
            {
                return GetValue(ControlModeProperty);
            }
            set
            {
                SetValue(ControlModeProperty, value);
            }
        }

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty =
            AvaloniaProperty.Register<VBDateTimePicker, string>(nameof(DisabledModes));
        /// <summary>
        /// Gets or sets the ACCaption translation.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption-Übersetzung.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string DisabledModes
        {
            get { return GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
        }
        #endregion


        #region IACInteractiveObject Member
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
