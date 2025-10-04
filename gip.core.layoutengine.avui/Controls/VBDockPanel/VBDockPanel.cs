using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Transactions;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Threading;
using Avalonia;
using Avalonia.Interactivity;
using gip.core.layoutengine.avui.Helperclasses;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Control element for placing child elements using Dock positions(Top, Left, Bottom, Right)
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Platzierung von untergeordneten Elementen mittels Dockpositionen (Top, Left, Bottom, Right)
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDockPanel'}de{'VBDockPanel'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBDockPanel : DockPanel, IACInteractiveObject, IACObject, IVBContent
    {
        /// <summary>
        /// Represents the dependency property for StringFormat.
        /// </summary>
        public static readonly StyledProperty<string> StringFormatProperty;

        static VBDockPanel()
        {
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner<VBDockPanel>();
            
            ACCompInitStateProperty.Changed.AddClassHandler<VBDockPanel>((x, e) => x.InitStateChanged());
            ACUpdateControlModeProperty.Changed.AddClassHandler<VBDockPanel>((x, e) => x.UpdateControlMode());
            BSOACComponentProperty.Changed.AddClassHandler<VBDockPanel>((x, e) => x.OnBSOACComponentChanged(e));
        }

        /// <summary>
        /// Creates a new instance of VBDockPanel.
        /// </summary>
        public VBDockPanel()
        {
            this.Loaded += VBDockPanel_Loaded;
            this.Unloaded += VBDockPanel_Unloaded;
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            // ActualizeTheme method doesn't exist in Avalonia - remove or implement differently
        }

        /// <summary>
        /// Represents the dependency property for IsBackgroundPanel.
        /// </summary>
        public static readonly StyledProperty<bool> IsBackgroundPanelProperty =
            AvaloniaProperty.Register<VBDockPanel, bool>(nameof(IsBackgroundPanel));

        /// <summary>
        /// Determines is background panel or not.
        /// </summary>
        [Category("VBControl")]
        public bool IsBackgroundPanel
        {
            get { return GetValue(IsBackgroundPanelProperty); }
            set { SetValue(IsBackgroundPanelProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for CanExecuteCyclic.
        /// </summary>
        public static readonly StyledProperty<int> CanExecuteCyclicProperty = ContentPropertyHandler.CanExecuteCyclicProperty.AddOwner<VBDockPanel>();
        /// <summary>
        /// Determines is cyclic execution enabled or disabled.The value (integer) in this property determines the interval of cyclic execution in miliseconds.
        /// </summary>
        /// <summary xml:lang="de">
        /// Bestimmt, ob die zyklische Ausführung aktiviert oder deaktiviert ist. Der Wert (Integer) in dieser Eigenschaft bestimmt das Intervall der zyklischen Ausführung in Millisekunden.
        /// </summary>
        [Category("VBControl")]
        public int CanExecuteCyclic
        {
            get { return GetValue(CanExecuteCyclicProperty); }
            set { SetValue(CanExecuteCyclicProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly StyledProperty<bool> DisableContextMenuProperty = ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBDockPanel>();
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


        public static readonly StyledProperty<bool> AnimationOffProperty = ContentPropertyHandler.AnimationOffProperty.AddOwner<VBDockPanel>();
        /// <summary>
        /// Dependency property to control if animations should be switched off to save gpu/rendering performance.
        /// </summary>
        [Category("VBControl")]
        public bool AnimationOff
        {
            get { return GetValue(AnimationOffProperty); }
            set { SetValue(AnimationOffProperty, value); }
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
            if (!_Initialized)
            {
                if (!string.IsNullOrEmpty(VBContent))
                {
                    IACType dcACTypeInfo = null;
                    object dcSource = null;
                    string dcPath = "";
                    Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

                    if (ContextACObject != null && ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                    {
                        Binding binding = new Binding();
                        binding.Source = dcSource;
                        binding.Path = dcPath;
                        binding.Mode = BindingMode.OneWay;
                        this.Bind(VBDockPanel.ACUpdateControlModeProperty, binding);
                    }
                }
                if (BSOACComponent != null)
                {
                    Binding binding = new Binding();
                    binding.Source = BSOACComponent;
                    binding.Path = Const.InitState;
                    binding.Mode = BindingMode.OneWay;
                    this.Bind(VBDockPanel.ACCompInitStateProperty, binding);
                }
            }
            _Initialized = true;
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
            this.Loaded -= VBDockPanel_Loaded;
            this.Unloaded -= VBDockPanel_Unloaded;
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
                _dispTimer.Tick -= dispatcherTimer_CanExecute;
                _dispTimer = null;
            }
            this.ClearValue(VBDockPanel.ACUpdateControlModeProperty);
            this.ClearValue(VBDockPanel.ACCompInitStateProperty);
            this.ClearAllBindings();
        }

        void VBDockPanel_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            
            if (_dispTimer != null)
            {
                if (!_dispTimer.IsEnabled)
                    _dispTimer.Start();
            }
            else
            {
                if (this.IsSet(CanExecuteCyclicProperty))
                {
                    _dispTimer = new DispatcherTimer();
                    _dispTimer.Tick += new EventHandler(dispatcherTimer_CanExecute);
                    _dispTimer.Interval = new TimeSpan(0, 0, 0, 0, CanExecuteCyclic < 500 ? 500 : CanExecuteCyclic);
                    _dispTimer.Start();
                }
            }
            UpdateControlMode();
        }

        void VBDockPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
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


        #region IACInteractiveObject Member

        /// <summary>
        /// Gets or sets the VBContent value type.
        /// </summary>
        public IACType VBContentValueType
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBDockPanel, string>(nameof(VBContent));

        /// <summary>Represents the property in which you enter the name of property which you want bound with ACUpdateControlMode property.
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
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
        public static readonly StyledProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBDockPanel>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUpdateControlMode.
        /// </summary>
        public static readonly StyledProperty<object> ACUpdateControlModeProperty =
            AvaloniaProperty.Register<VBDockPanel, object>(nameof(ACUpdateControlMode));

        /// <summary>
        /// Gets or sets the AC update control mode.
        /// </summary>
        public object ACUpdateControlMode
        {
            get { return GetValue(ACUpdateControlModeProperty); }
            set { SetValue(ACUpdateControlModeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBDockPanel, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        private void OnBSOACComponentChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue == null && e.OldValue != null && !String.IsNullOrEmpty(this.VBContent))
            {
                IACBSO bso = e.OldValue as IACBSO;
                if (bso != null)
                    this.DeInitVBControl(bso);
            }
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
            IsVisible = controlMode >= Global.ControlModes.Disabled;
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

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return this.Name; }
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

        #region CanExecute Dispatcher
        private DispatcherTimer _dispTimer = null;
        private void dispatcherTimer_CanExecute(object sender, EventArgs e)
        {
            // In Avalonia, CommandManager.InvalidateRequerySuggested() doesn't exist
            // We need to use an alternative approach for command invalidation
            // This is typically handled by the command implementation itself
        }
        #endregion


        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        public Global.ControlModes RightControlMode
        {
            get { return Global.ControlModes.Enabled; }
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
    }
}
