using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;


namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a <see cref="Int32"/> up or down switch control.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein <see cref="Int32"/> Up- oder Down-Steuerelement dar.
    /// </summary>
    public class VBNumericUpDown : UpDownBase, IVBContent, IACMenuBuilderWPFTree, IACObject
    {
        static VBNumericUpDown()
        {
            // In Avalonia, we don't override metadata like in WPF
            // ValueTypeProperty behavior is handled differently
        }

        /// <summary>
        /// Creates a new instance of VBNumericUpDown.
        /// </summary>
        public VBNumericUpDown() : base()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            InitVBControl();
            this.Loaded += VBNumericUpDown_Loaded;
            this.Unloaded += VBNumericUpDown_Unloaded;
        }

        private void VBNumericUpDown_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
        }

        private void VBNumericUpDown_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_Initialized)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
                BSOACComponent.RemoveWPFRef(this.GetHashCode());
        }

        /// <summary>
        /// Overrides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
        }

        #region Properties

        #region Styled-Properties (converted from Dependency-Properties)
        /// <summary>
        /// Represents the styled property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty
            = AvaloniaProperty.Register<VBNumericUpDown, string>(nameof(VBContent));

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

        /// <summary>
        /// Represents the styled property for RightControlMode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> RightControlModeProperty
            = AvaloniaProperty.Register<VBNumericUpDown, Global.ControlModes>(nameof(RightControlMode));

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
        /// Represents the styled property for ACCaption.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionProperty
            = AvaloniaProperty.Register<VBNumericUpDown, string>(nameof(ACCaption));

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
        /// Represents the styled property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionTransProperty
            = AvaloniaProperty.Register<VBNumericUpDown, string>(nameof(ACCaptionTrans));

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
        /// Represents the styled property for ShowCaption.
        /// </summary>
        public static readonly StyledProperty<bool> ShowCaptionProperty
            = AvaloniaProperty.Register<VBNumericUpDown, bool>(nameof(ShowCaption), true);

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

        public VBTextBox TextBoxVB
        {
            get
            {
                return TextBox as VBTextBox;
            }
        }

        #region Layout
        /// <summary>
        /// Represents the styled property for WidthCaption.
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthCaptionProperty
            = AvaloniaProperty.Register<VBNumericUpDown, GridLength>(nameof(WidthCaption), new GridLength(15, GridUnitType.Star));

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
        /// Represents the styled property for WidthCaptionMax.
        /// </summary>
        public static readonly StyledProperty<double> WidthCaptionMaxProperty
            = AvaloniaProperty.Register<VBNumericUpDown, double>(nameof(WidthCaptionMax), 150.0);

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
        /// Represents the styled property for WidthContent.
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthContentProperty
            = AvaloniaProperty.Register<VBNumericUpDown, GridLength>(nameof(WidthContent), new GridLength(20, GridUnitType.Star));

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
        /// Represents the styled property for WidthContentMax.
        /// </summary>
        public static readonly StyledProperty<double> WidthContentMaxProperty
            = AvaloniaProperty.Register<VBNumericUpDown, double>(nameof(WidthContentMax), Double.PositiveInfinity);

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
        /// Represents the styled property for WidthPadding.
        /// </summary>
        public static readonly StyledProperty<GridLength> WidthPaddingProperty
            = AvaloniaProperty.Register<VBNumericUpDown, GridLength>(nameof(WidthPadding), new GridLength(0));

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
        /// Represents the styled property for DisabledModes.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty
            = AvaloniaProperty.Register<VBNumericUpDown, string>(nameof(DisabledModes));

        /// <summary>
        /// Gets or sets the disabled modes.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die deaktivierten Modi.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string DisabledModes
        {
            get { return GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
        }

        /// <summary>
        /// Represents the attached property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBNumericUpDown>();

        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        #endregion

        #endregion

        #region IVBContent

        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return TextBoxVB?.VBContentPropertyInfo;
            }
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
                return TextBoxVB?.ContextACObject;
            }
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

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return TextBoxVB?.ACType;
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
                return TextBoxVB?.ACContentList;
            }
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return TextBoxVB?.ParentACObject;
            }
        }

        #endregion //FormatString

        #endregion //Properties

        #region Methods

        #region Init
        protected bool _Initialized = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized || DataContext == null || TextBoxVB == null || String.IsNullOrEmpty(VBContent))
                return;
            _Initialized = true;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBNumericUpDown", VBContent);
                return;
            }

            var newBinding = new Binding
            {
                Source = dcSource,
                Path = dcPath,
                Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay
            };
            this.Bind(ValueProperty, newBinding);
        }
        #endregion

        #region IVBContent
        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collector can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public void DeInitVBControl(IACComponent bso)
        {
            if (!_Initialized)
                return;
            _Initialized = false;
            Loaded -= VBNumericUpDown_Loaded;
            Unloaded -= VBNumericUpDown_Unloaded;
            TextBoxVB?.DeInitVBControl(bso);
            this.ClearAllBindings();
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            TextBoxVB?.ACAction(actionArgs);
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return TextBoxVB?.IsEnabledACAction(actionArgs) ?? false;
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that addresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return TextBoxVB?.ACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that addresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return TextBoxVB?.IsEnabledACUrlCommand(acUrl, acParameter) ?? false;
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a relative path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return TextBoxVB?.GetACUrl(rootACObject) ?? ACIdentifier;
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
            return TextBoxVB?.ACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode) ?? false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        public void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            TextBoxVB?.AppendMenu(vbContent, vbControl, ref acMenuItemList);
        }

        public ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            return TextBoxVB?.GetMenu(vbContent, vbControl);
        }

        #endregion //Private

        #region Abstract UpDownBase

        protected override void OnIncrement()
        {
            if (Value != null)
                UpdateNumeric(1);
        }

        protected override void OnDecrement()
        {
            if (Value != null)
                UpdateNumeric(-1);
        }

        protected override object ConvertTextToValue(string text)
        {
            if (String.IsNullOrEmpty(text))
                return Int32.MinValue;
            try
            {
                Int32 value = Int32.Parse(text, CultureInfo.CurrentCulture);
                return value;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBNumericUpDown", "ConvertTextToValue", msg);

                return Int32.MinValue; // Fixed: was returning DateTime.MinValue, should be Int32.MinValue
            }
        }

        protected override string ConvertValueToText(object value)
        {
            if (value == null)
                return string.Empty;
            if (value is string)
                return value as string;
            return value.ToString();
        }

        private void UpdateNumeric(int value)
        {
            Int32 newVal = (Int32)Value;
            newVal += value;
            Value = newVal;
        }

        #endregion //Abstract

        #endregion //Methods
    }
}
