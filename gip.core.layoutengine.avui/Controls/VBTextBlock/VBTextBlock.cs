using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control for displaying a text.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung einer Bezeichnung.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTextBlock'}de{'VBTextBlock'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTextBlock : TextBlock, IVBContent, IACObject
    {
        #region c'tors
        static VBTextBlock()
        {
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner<VBTextBlock>();
        }

        public VBTextBlock()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        void VBTextBlock_SourceUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            UpdateControlMode();
        }

        void VBTextBlock_TargetUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            UpdateControlMode();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            VBTextBlock thisControl = this;
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
            else if (change.Property == StringFormatProperty)
            {
                // Note: Avalonia handles binding updates differently
                // This would need to be implemented differently in Avalonia
            }
            else if (change.Property == TextProperty)
            {
                VBTextBlock_SourceUpdated(change.Sender, change);
            }
        }
        #endregion

        #region IDataField Members

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBTextBlock, string>(nameof(VBContent));

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

        /// <summary>
        /// Represents the dependency property for ACCaption.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionProperty =
            AvaloniaProperty.Register<VBTextBlock, string>(Const.ACCaptionPrefix);

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
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBTextBlock>();
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
            AvaloniaProperty.Register<VBTextBlock, ACInitState>(nameof(ACCompInitState));

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

        private bool Enabled
        {
            get
            {
                return IsEnabled;
            }
            set
            {
                if (value == true)
                {
                    if (ContextACObject == null)
                    {
                        IsEnabled = true;
                    }
                    else
                    {
                        IsEnabled = RightControlMode >= Global.ControlModes.Enabled;
                    }
                }
                else
                {
                    IsEnabled = false;
                }
            }
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
            if (_Initialized || DataContext == null)
                return;
            _Initialized = true;

            IBinding binding = null;
            if (ContextACObject == null)
            {
                if (!string.IsNullOrEmpty(ACCaption))
                    this.Text = Translator.GetTranslation(ACIdentifier, ACCaption, this.Root().Environment.VBLanguageCode);
                else if (!String.IsNullOrEmpty(VBContent))
                {
                    binding = new Binding
                    {
                        Path = VBContent
                    };
                    this.Bind(TextBlock.TextProperty, binding);
                }
                return;
            }
            if (string.IsNullOrEmpty(VBContent))
            {
                if (!string.IsNullOrEmpty(ACCaption))
                {
                    if (ACCaption.Contains("{'") && ACCaption.Contains("'}"))
                        this.Text = Translator.GetTranslation(ACIdentifier, ACCaption, this.Root().Environment.VBLanguageCode);
                    if (String.IsNullOrEmpty(this.Text))
                        this.Text = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
                }
                return;
            }

            RightControlMode = Global.ControlModes.Disabled;

            if (RightControlMode < Global.ControlModes.Disabled)
            {
                IsVisible = false;
            }

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;
            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBTextBlock", VBContent);
                return;
            }

            bool isNumericValueBound = false;
            if (dcACTypeInfo != null)
                isNumericValueBound = TypeAnalyser.IsNumericType(dcACTypeInfo.ObjectType);

            // Check if TextAlignment is locally set
            if (isNumericValueBound && !this.IsSet(TextBlock.TextAlignmentProperty))
                TextAlignment = TextAlignment.Right;

            binding = null;
            if (dcSource is INotifyPropertyChanged)
            {
                ProxyACRefConverter refConverter = ProxyACRefConverter.IfACRefGenerateConverter(out var tempBinding, ref dcACTypeInfo, ref dcPath, ref dcSource, ref dcRightControlMode);
                binding = tempBinding;

                if (refConverter == null)
                {
                    var concreteBinding = new Binding
                    {
                        Source = dcSource,
                        Path = dcPath,
                        Mode = BindingMode.OneWay
                    };
                    binding = concreteBinding;
                }
                else
                    isNumericValueBound = false;

                if (!String.IsNullOrEmpty(StringFormat))
                    ((Binding)binding).StringFormat = StringFormat;
                
                this.Bind(TextBlock.TextProperty, binding);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine(String.Format("Source of VBContent {0} at VBTextBlock is not INotifyPropertyChanged",VBContent));
            }

            if (BSOACComponent != null)
            {
                var initStateBinding = new Binding
                {
                    Source = BSOACComponent,
                    Path = Const.InitState,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBTextBlock.ACCompInitStateProperty, initStateBinding);
            }

            // Set tooltip if needed
            string tooltip = ToolTip.GetTip(this) as string;
            if (!String.IsNullOrEmpty(tooltip))
            {
                ToolTip.SetTip(this, this.Root().Environment.TranslateText(ContextACObject, tooltip));
            }
            else if (isNumericValueBound && !String.IsNullOrEmpty(this.StringFormat))
            {
                var tooltipBinding = new Binding
                {
                    Source = dcSource,
                    Path = dcPath,
                    Mode = BindingMode.OneWay
                };
                this.Bind(ToolTip.TipProperty, tooltipBinding);
            }
        }

        bool _Loaded = false;
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
            {
                var bindingexp = BindingOperations.GetBindingExpressionBase(this, TextBlock.TextProperty);
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
                        this.Root().Messages.LogDebug("VBTextBlock", "AddWPFRef", exw.Message);
                    }
                }
            }
            _Loaded = true;
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
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
            _VBContentPropertyInfo = null;

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

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        [Category("VBControl")]
        public Global.ControlModes RightControlMode
        {
            get;
            set;
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
            if (controlMode != RightControlMode)
                RightControlMode = controlMode;

            if (controlMode == Global.ControlModes.Collapsed)
            {
                if (IsVisible)
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
            }
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
