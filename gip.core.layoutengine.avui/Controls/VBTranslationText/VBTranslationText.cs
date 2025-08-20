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
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System.Transactions;
using System.ComponentModel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control for displaying a translation text.
    /// </summary>
    /// <summary>
    /// Steuerung für die Anzeige eines Übersetzungstextes.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTranslationText'}de{'VBTranslationText'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTranslationText : Control, IVBContent, IACObject
    {
        #region c'tors

        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TranslationTextStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTranslationText/Themes/TranslationTextStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TranslationTextStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBTranslationText/Themes/TranslationTextStyleAero.xaml" },
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

        static VBTranslationText()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTranslationText), new FrameworkPropertyMetadata(typeof(VBTranslationText)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Create the instance of VBTranslationText.
        /// </summary>
        public VBTranslationText()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
            //Loaded += new RoutedEventHandler(InitVBControl);
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
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }
        #endregion

        #region DependencyProperties
        /// <summary>
        /// Represents the dependency property for the BaselineOffset.
        /// </summary>
        public static readonly DependencyProperty BaselineOffsetProperty = TextBlock.BaselineOffsetProperty.AddOwner(typeof(VBTranslationText));

        /// <summary> 
        /// The BaselineOffset property provides an adjustment to baseline offset  
        /// </summary> 
        [Category("VBControl")]
        public double BaselineOffset 
        { 
            get { return (double)GetValue(BaselineOffsetProperty); }  
            set { SetValue(BaselineOffsetProperty, value); } 
        }

        /// <summary>
        /// Represents the dependency property for the Text.
        /// </summary>
        [Category("VBControl")]
        public static readonly DependencyProperty TextProperty = TextBlock.TextProperty.AddOwner(typeof(VBTranslationText));

        /// <summary> 
        /// The Text property defines the content (text) to be displayed.  
        /// </summary> 
        [Category("VBControl")]
        [Localizability(LocalizationCategory.Text)] 
        public string Text 
        {  
            get { return (string) GetValue(TextProperty); } 
            set { SetValue(TextProperty, value); }  
        }

        /// <summary>
        /// Represents the dependency property for the TextDecorations.
        /// </summary>
        public static readonly DependencyProperty TextDecorationsProperty = TextBlock.TextDecorationsProperty.AddOwner(typeof(VBTranslationText));

        /// <summary>  
        /// The TextDecorations property specifies decorations that are added to the text of an element. 
        /// </summary> 
        [Category("VBControl")]
        public TextDecorationCollection TextDecorations
        {
            get { return (TextDecorationCollection)GetValue(TextDecorationsProperty); }
            set { SetValue(TextDecorationsProperty, value); }
        }
           
        /// <summary>
        /// Represents the dependency property for the TextEffects.
        /// </summary>
        public static readonly DependencyProperty TextEffectsProperty = TextBlock.TextEffectsProperty.AddOwner(typeof(VBTranslationText));

        /// <summary> 
        /// The TextEffects property specifies effects that are added to the text of an element. 
        /// </summary> 
        [Category("VBControl")]
        public TextEffectCollection TextEffects
        {
            get { return (TextEffectCollection)GetValue(TextEffectsProperty); }
            set { SetValue(TextEffectsProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the LineHeight.
        /// </summary>
        public static readonly DependencyProperty LineHeightProperty = TextBlock.LineHeightProperty.AddOwner(typeof(VBTranslationText));

        /// <summary>  
        /// The LineHeight property specifies the height of each generated line box. 
        /// </summary> 
        [Category("VBControl")]
        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the LineStackingStrategy.
        /// </summary> 
        public static readonly DependencyProperty LineStackingStrategyProperty = TextBlock.LineStackingStrategyProperty.AddOwner(typeof(VBTranslationText));


        /// <summary> 
        /// The LineStackingStrategy property specifies how lines are placed  
        /// </summary> 
        [Category("VBControl")]
        public LineStackingStrategy LineStackingStrategy
        {
            get { return (LineStackingStrategy)GetValue(LineStackingStrategyProperty); }
            set { SetValue(LineStackingStrategyProperty, value); }
        }


        /// <summary>
        /// Represents the dependency property for the TextAlignment.
        /// </summary> 
        public static readonly DependencyProperty TextAlignmentProperty = TextBlock.TextAlignmentProperty.AddOwner(typeof(VBTranslationText));

        /// <summary> 
        /// The TextAlignment property specifies horizontal alignment of the content.  
        /// </summary> 
        [Category("VBControl")]
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }


        /// <summary>
        /// Represents the dependency property for the TextTrimming.
        /// </summary>
        public static readonly DependencyProperty TextTrimmingProperty = TextBlock.TextTrimmingProperty.AddOwner(typeof(VBTranslationText));

        /// <summary>  
        /// The TextTrimming property specifies the trimming behavior situation  
        /// in case of clipping some textual content caused by overflowing the line's box. 
        /// </summary> 
        [Category("VBControl")]
        public TextTrimming TextTrimming
        {
            get { return (TextTrimming)GetValue(TextTrimmingProperty); }
            set { SetValue(TextTrimmingProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the TextWrapping.
        /// </summary> 
        public static readonly DependencyProperty TextWrappingProperty = TextBlock.TextWrappingProperty.AddOwner(typeof(VBTranslationText));

        /// <summary> 
        /// The TextWrapping property controls whether or not text wraps  
        /// when it reaches the flow edge of its containing block box. 
        /// </summary> 
        [Category("VBControl")]
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the HyphenationEnabled.
        /// </summary>
        public static readonly DependencyProperty IsHyphenationEnabledProperty = TextBlock.IsHyphenationEnabledProperty.AddOwner(typeof(VBTranslationText));

        /// <summary> 
        /// CLR property for hyphenation 
        /// </summary> 
        [Category("VBControl")]
        public bool IsHyphenationEnabled
        {
            get { return (bool)GetValue(IsHyphenationEnabledProperty); }
            set { SetValue(IsHyphenationEnabledProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the CornerRadius.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = Border.CornerRadiusProperty.AddOwner(typeof(VBTranslationText));

        ///  <summary>
        /// The CornerRadius property allows users to control the roundness of the corners independently by 
        /// setting a radius value for each corner.  Radius values that are too large are scaled so that they
        /// smoothly blend from corner to corner. 
        /// </summary>
        [Category("VBControl")]
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius) GetValue(CornerRadiusProperty); } 
            set { SetValue(CornerRadiusProperty, value); }
 
        }

        /// <summary>
        /// Represents the dependency property for the CollapsedMode.
        /// </summary>
        public static readonly DependencyProperty CollapsedModeProperty
            = DependencyProperty.Register("CollapsedMode", typeof(Boolean), typeof(VBTranslationText));

        /// <summary>
        /// Determines is in collapsed mode or not.
        /// </summary>
        [Category("VBControl")]
        public Boolean CollapsedMode
        {
            get { return (Boolean)GetValue(CollapsedModeProperty); }
            set { SetValue(CollapsedModeProperty, value); }
        }
        #endregion

        #region IDataField Members

        /// <summary>
        /// Represents the dependency property for the VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBTranslationText));

        /// <summary>
        /// Represents the property where you enter the ACIdentifier of text to display.
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for the ShowText.
        /// </summary>
        public static readonly DependencyProperty ShowTextProperty
            = DependencyProperty.Register("ShowText", typeof(Boolean), typeof(VBTranslationText), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets the ShowText.
        /// </summary>
        [Category("VBControl")]
        public bool ShowText
        {
            get { return (bool)GetValue(ShowTextProperty); }
            set { SetValue(ShowTextProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ShowTextACUrl.
        /// </summary>
        public static readonly DependencyProperty ShowTextACUrlProperty
            = DependencyProperty.Register("ShowTextACUrl", typeof(string), typeof(VBTranslationText));

        /// <summary>
        /// Represents the property where you enter the ACUrl of text to display.
        /// </summary>
        [Category("VBControl")]
        public string ShowTextACUrl
        {
            get { return (string)GetValue(ShowTextACUrlProperty); }
            set { SetValue(ShowTextACUrlProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for Blink.
        /// </summary>
        public static readonly DependencyProperty BlinkProperty
            = DependencyProperty.Register("Blink", typeof(Boolean), typeof(VBTranslationText), new PropertyMetadata(false));

        /// <summary>
        /// Determines is text blink enabled or disabled.
        /// </summary>
        [Category("VBControl")]
        public bool Blink
        {
            get { return (bool)GetValue(BlinkProperty); }
            set { SetValue(BlinkProperty, value); }
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

        private string _Caption;
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                return _Caption;
            }
            set
            {
                _Caption = value;
            }
        }

        /*private void SetTextProperty(string value)
        {
            if (String.IsNullOrEmpty(value))
                Text = value;
            else if (value.Contains("\\n"))
            {
                Inlines.Clear();
                TextWrapping = TextWrapping.Wrap;
                string[] stringSeparators = new string[] { "\\n" };
                string[] split = ACCaption.Split(stringSeparators, StringSplitOptions.None);
                foreach (string s in split)
                {
                    Inlines.Add(new Run(s));
                    Inlines.Add(new LineBreak());
                }
            }
            else
                Text = value;
        }*/
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
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBTranslationText), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
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
        ////        typeof(ACUrlCmdMessage), typeof(VBTranslationText),
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
                typeof(ACInitState), typeof(VBTranslationText),
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
            VBTranslationText thisControl = dependencyObject as VBTranslationText;
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
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
        }
        #endregion


        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized)
                return;
            _Initialized = true;

            if (ContextACObject == null)
            {
                if (!string.IsNullOrEmpty(ACCaption))
                    this.Text = ACCaption;
                return;
            }
            if (string.IsNullOrEmpty(VBContent))
            {
                if (!string.IsNullOrEmpty(ACCaption))
                {
                    if (ACCaption.Contains("{'") && ACCaption.Contains("'}"))
                        this.Text = Translator.GetTranslation("VBTabItem", ACCaption);
                    else
                        this.Text = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
                }
                return;
            }

            RightControlMode = Global.ControlModes.Enabled;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBTranslationText", VBContent);
                return;
            }

            string text = ContextACObject.ACUrlCommand(VBContent) as String;
            if (!String.IsNullOrEmpty(text))
            {
                this.Text = text;
            }

            if (dcRightControlMode < Global.ControlModes.Enabled)
                RightControlMode = Global.ControlModes.Hidden;
            else
                RightControlMode = Global.ControlModes.Enabled;

            string propertyName = "";
            if (VBContent.IndexOf('§') != 0)
                propertyName = VBContent.Replace('§', '\\');
            else
                propertyName = VBContent.Substring(1);

            if (!String.IsNullOrEmpty(ShowTextACUrl))
            {
                propertyName = ShowTextACUrl;
            }

            if (!String.IsNullOrEmpty(propertyName))
            {
                dcACTypeInfo = null;
                dcSource = null;
                dcPath = "";
                if (!ContextACObject.ACUrlBinding(propertyName, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                {
                    if (!String.IsNullOrEmpty(ShowTextACUrl))
                        this.Root().Messages.LogDebug("Error00003", "VBTranslationText", ShowTextACUrl);
                    return;
                }

                Binding binding = new Binding();
                binding.Source = dcSource;
                binding.Path = new PropertyPath(dcPath);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBTranslationText.ShowTextProperty, binding);

                if (BSOACComponent != null)
                {
                    binding = new Binding();
                    binding.Source = BSOACComponent;
                    binding.Path = new PropertyPath(Const.InitState);
                    binding.Mode = BindingMode.OneWay;
                    SetBinding(VBTranslationText.ACCompInitStateProperty, binding);
                }
            }
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
            _VBContentPropertyInfo = null;
            BindingOperations.ClearBinding(this, VBTranslationText.ShowTextACUrlProperty);
            //BindingOperations.ClearBinding(this, VBTranslationText.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBTranslationText.ACCompInitStateProperty);
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

        //void ElementACComponent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName.StartsWith(Const.ACState))
        //    {
        //        if (e.PropertyName.Length < 8)
        //        {
        //            UpdateControlMode();
        //        }
        //        else
        //        {
        //            if (VBContent == e.PropertyName.Substring(8))
        //            {
        //                UpdateControlMode();
        //            }
        //        }
        //    }
        //}


        /// <summary>
        /// The control mode property
        /// </summary>
        public static readonly DependencyProperty RightControlModeProperty
            = DependencyProperty.Register("RightControlMode", typeof(Global.ControlModes), typeof(VBTranslationText), new PropertyMetadata(Global.ControlModes.Enabled));

        /// <summary>
        /// Gets or sets the control mode.
        /// </summary>
        /// <value>The control mode.</value>
        [Category("VBControl")]
        public Global.ControlModes RightControlMode
        {
            get { return (Global.ControlModes)GetValue(RightControlModeProperty); }
            set { SetValue(RightControlModeProperty, value); }
        }

        private string _StringFormat;
        /// <summary>
        /// Gets or sets the string format for the control.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt das Stringformat für das Steuerelement.
        /// </summary>
        [Category("VBControl")]
        public string StringFormat
        {
            get
            {
                return _StringFormat;
            }
            set
            {
                _StringFormat = value;
            }
        }

        /// <summary>
        /// Disables the control. XAML sample: DisabledModes="Disabled"
        /// </summary>
        /// <summary xml:lang="de">
        /// Deaktiviert die Steuerung. XAML-Probe: DisabledModes="Disabled"
        /// </summary>
        [Category("VBControl")]
        public string DisabledModes
        {
            get;
            set;
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

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        #endregion
    }
}
