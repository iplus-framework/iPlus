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
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;
using System.Transactions;
using gip.ext.designer.PropertyGrid;

namespace gip.core.layoutengine
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
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ProgressBarStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBProgressBar/Themes/ProgressBarStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ProgressBarStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBProgressBar/Themes/ProgressBarStyleAero.xaml" },
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

        static VBProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBProgressBar), new FrameworkPropertyMetadata(typeof(VBProgressBar)));
        }

        bool _themeApplied = false;
        public VBProgressBar()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.Loaded += VBProgressBar_Loaded;
            this.Unloaded += VBProgressBar_Unloaded;
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
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }
        #endregion

        #region DependencyProperties

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
        /// Represents the dependency property for ProgressBarStyle.
        /// </summary>
        public static readonly DependencyProperty ProgressBarStyleProperty
            = DependencyProperty.Register("ProgressBarStyle", typeof(ProgressBarStyles), typeof(VBProgressBar), new PropertyMetadata(ProgressBarStyles.DefaultBar));
        
        /// <summary>
        /// Gets or sets the progress bar style.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        public ProgressBarStyles ProgressBarStyle
        {
            get { return (ProgressBarStyles)GetValue(ProgressBarStyleProperty); }
            set { SetValue(ProgressBarStyleProperty, value); }
        }


        public static readonly DependencyProperty ValueVisibilityProperty =
            DependencyProperty.Register("ValueVisibility", typeof(Visibility), typeof(VBProgressBar), new PropertyMetadata(Visibility.Visible));

        public Visibility ValueVisibility
        {
            get { return (Visibility)GetValue(ValueVisibilityProperty); }
            set { SetValue(ValueVisibilityProperty, value); }
        }


        public static readonly DependencyProperty PieFillProperty
            = DependencyProperty.Register("PieFill", typeof(SolidColorBrush), typeof(VBProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush PieFill
        {
            get { return (SolidColorBrush)GetValue(PieFillProperty); }
            set { SetValue(PieFillProperty, value); }
        }

        public static readonly DependencyProperty PieStrokeProperty
            = DependencyProperty.Register("PieStroke", typeof(SolidColorBrush), typeof(VBProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.DarkRed)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush PieStroke
        {
            get { return (SolidColorBrush)GetValue(PieStrokeProperty); }
            set { SetValue(PieStrokeProperty, value); }
        }


        public static readonly DependencyProperty PieTextColorProperty
            = DependencyProperty.Register("PieTextColor", typeof(SolidColorBrush), typeof(VBProgressBar), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public SolidColorBrush PieTextColor
        {
            get { return (SolidColorBrush)GetValue(PieTextColorProperty); }
            set { SetValue(PieTextColorProperty, value); }
        }


        #endregion

        #endregion

        #region IDataField Members

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBProgressBar));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
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
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBProgressBar), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
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
        ////        typeof(ACUrlCmdMessage), typeof(VBProgressBar),
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
                typeof(ACInitState), typeof(VBProgressBar),
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
            VBProgressBar thisControl = dependencyObject as VBProgressBar;
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

        //public string ToolTip
        //{
        //    get
        //    {
        //        return ucTextbox.ToolTip as string;
        //    }
        //    set
        //    {
        //        ucTextbox.ToolTip = value;
        //    }
        //}

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

        private bool Enabled
        {
            get
            {
                return false;
            }
            set
            {
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
            if (Enabled)
            {
                if (VBContentPropertyInfo != null)
                {
                    if (VBContentPropertyInfo.IsNullable)
                    {
                        ControlMode = Global.ControlModes.Enabled;
                    }
                    else
                    {
                        ControlMode = Global.ControlModes.EnabledRequired;
                    }
                }
                else
                    ControlMode = Global.ControlModes.Disabled;
            }
            else
            {
                ControlMode = Global.ControlModes.Disabled;
            }
        }

        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBProgressBar));

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
        #endregion

        /// <summary>
        /// Determines is accepts return or not.
        /// </summary>
        [Category("VBControl")]
        public bool AcceptsReturn
        {
            get
            {
                return false;
            }
            set
            {
                
            }
        }

        private void InitPopUp()
        {
        }

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
                Visibility = Visibility.Collapsed;
            }

            Binding binding = new Binding();
            binding.Source = dcSource;
            binding.Path = new PropertyPath(dcPath);
            binding.Mode = BindingMode.OneWay;
            binding.NotifyOnSourceUpdated = true;
            binding.NotifyOnTargetUpdated = true;
            SetBinding(ProgressBar.ValueProperty, binding);

            if (BSOACComponent != null)
            {
                binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBProgressBar.ACCompInitStateProperty, binding);
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
                Binding boundedValue = BindingOperations.GetBinding(this, ProgressBar.ValueProperty);
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

            BindingOperations.ClearBinding(this, ProgressBar.ValueProperty);
            //BindingOperations.ClearBinding(this, VBProgressBar.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBProgressBar.ACCompInitStateProperty);
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

        #endregion
    }
}
