using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
    /// Control for displaying a progress bar.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Anzeige eines Verlaufsbalkens.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBProgressBar'}de{'VBProgressBar'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBProgressBar : ProgressBar, IVBContent, IACObject
    {
        #region c'tors
        public VBProgressBar() : base()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Loaded += VBProgressBar_Loaded;
            this.Unloaded += VBProgressBar_Unloaded;
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

        #region StyledProperties

        #region Appearance

        /// <summary>
        /// Represents the enumeration for progress bar styles.
        /// </summary>
        [ACClassInfo(Const.PackName_VarioSystem, "en{'ProgressBarStyles'}de{'ProgressBarStyles'}", Global.ACKinds.TACEnum)]
        public enum ProgressBarStyles : short
        {
            DefaultBar = 0,
            NormalPie = 1,
            GlassyPie = 2,
            Circular = 3,
            CircularSegmented = 4,
            CircularPoints = 5,
            StopWatch = 6,
            PerformantBar = 7,
            CircularUnlimited = 8
        }
        
        /// <summary>
        /// Represents the styled property for ProgressBarStyle.
        /// </summary>
        public static readonly StyledProperty<ProgressBarStyles> ProgressBarStyleProperty =
            AvaloniaProperty.Register<VBProgressBar, ProgressBarStyles>(nameof(ProgressBarStyle), ProgressBarStyles.DefaultBar);
        
        /// <summary>
        /// Gets or sets the progress bar style.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        public ProgressBarStyles ProgressBarStyle
        {
            get { return GetValue(ProgressBarStyleProperty); }
            set { SetValue(ProgressBarStyleProperty, value); }
        }


        public static readonly StyledProperty<Visibility> ValueVisibilityProperty =
            AvaloniaProperty.Register<VBProgressBar, Visibility>(nameof(ValueVisibility), Visibility.Visible);

        public Visibility ValueVisibility
        {
            get { return GetValue(ValueVisibilityProperty); }
            set { SetValue(ValueVisibilityProperty, value); }
        }


        public static readonly StyledProperty<IBrush> PieFillProperty =
            AvaloniaProperty.Register<VBProgressBar, IBrush>(nameof(PieFill), new SolidColorBrush(Colors.Red));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public IBrush PieFill
        {
            get { return GetValue(PieFillProperty); }
            set { SetValue(PieFillProperty, value); }
        }

        public static readonly StyledProperty<IBrush> PieStrokeProperty =
            AvaloniaProperty.Register<VBProgressBar, IBrush>(nameof(PieStroke), new SolidColorBrush(Colors.DarkRed));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public IBrush PieStroke
        {
            get { return GetValue(PieStrokeProperty); }
            set { SetValue(PieStrokeProperty, value); }
        }


        public static readonly StyledProperty<IBrush> PieTextColorProperty =
            AvaloniaProperty.Register<VBProgressBar, IBrush>(nameof(PieTextColor), new SolidColorBrush(Colors.Black));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public IBrush PieTextColor
        {
            get { return GetValue(PieTextColorProperty); }
            set { SetValue(PieTextColorProperty, value); }
        }


        #endregion

        #endregion

        #region IDataField Members

        /// <summary>
        /// Represents the styled property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBProgressBar, string>(nameof(VBContent));

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
        /// Gets or sets the ACIdentifier.
        /// </summary>
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
            get;set;
        }
        #endregion

        #region CommandEvents

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
        /// Represents the attached property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBProgressBar>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBProgressBar, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
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
        /// Represents the styled property for control mode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBProgressBar, Global.ControlModes>(nameof(ControlMode));

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
            if (_Initialized || DataContext == null || ContextACObject == null)
                return;
            _Initialized = true;
            //this.Unloaded += VBProgressBar_Unloaded;

            if (String.IsNullOrEmpty(VBContent))
                return;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBProgressBar", VBContent);
                return;
            }

            RightControlMode = Global.ControlModes.Enabled;
            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            if (RightControlMode < Global.ControlModes.Disabled)
            {
                IsVisible = false;
            }

            Binding binding = new Binding();
            binding.Source = dcSource;
            binding.Path = dcPath;
            binding.Mode = BindingMode.OneWay;
            this.Bind(ProgressBar.ValueProperty, binding);

            if (BSOACComponent != null)
            {
                binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = Const.InitState;
                binding.Mode = BindingMode.OneWay;
                this.Bind(VBProgressBar.ACCompInitStateProperty, binding);
            }
        }

        bool _Loaded = false;
        void VBProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
            {
                BindingExpressionBase boundedValue = BindingOperations.GetBindingExpressionBase(this, ProgressBar.ValueProperty);
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
                        this.Root().Messages.LogDebug("VBProgressBar", "AddWPFRef", exw.Message);
                    }
                }
            }
            _Loaded = true;
        }

        void VBProgressBar_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
            {
                if (BSOACComponent != null)
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
            _VBContentPropertyInfo = null;
            this.Loaded -= VBProgressBar_Loaded;
            this.Unloaded -= VBProgressBar_Unloaded;

            try
            {
                this.ClearAllBindings();
            }
            catch 
            { 
            }
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
