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
using gip.core.layoutengine.Helperclasses;
using System.Transactions;
using System.Windows.Threading;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a iPlus button control, which reacts to a Click event.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt ein iPlus-Schaltflächenelement dar, das auf ein Click-Ereignis reagiert.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBButton'}de{'VBButton'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBButton : Button, IVBDynamicIcon, IVBContent, IACObject
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> {
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip,
                                         styleName = "ButtonStyleGip",
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBButton/Themes/ButtonStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero,
                                         styleName = "ButtonStyleAero",
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBButton/Themes/ButtonStyleAero.xaml" },
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

        static VBButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBButton), new FrameworkPropertyMetadata(typeof(VBButton)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instance of VBButton.
        /// </summary>
        public VBButton()
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
            Loaded += new RoutedEventHandler(CustomVBButton_Loaded);
            Unloaded += new RoutedEventHandler(CustomVBButton_Unloaded);
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

        bool _Initialized = false;
        private CommandBinding _CmdBindingExecute;

        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl(bool onLoad = false)
        {
            if (_Initialized)
                return;
            if (!BindToBSO && ContextACObject == null)
                return;
            if (!onLoad && BindOnLoadEvent)
                return;

            _Initialized = true;
            if (!String.IsNullOrEmpty(VBContent))
            {
                IACType dcACTypeInfo = null;
                object dcSource = null;
                string dcPath = "";
                Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

                ACMenuItem menuItem = null;

                bool bound = false;
                if (ContextACObject != null)
                    bound = ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode);
                if (!bound && BSOACComponent != null)
                    bound = BSOACComponent.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode);
                if (!bound)
                {
                    ACMenuItemList acMenuItemList = new ACMenuItemList();
                    VBLogicalTreeHelper.AppendMenu(this, VBContent, "", ref acMenuItemList);
                    menuItem = acMenuItemList.Where(c => c.ACUrlCommandString == VBContent).FirstOrDefault();
                    if (menuItem == null)
                    {
                        this.Root().Messages.LogDebug("Error00003", "VBButton", VBContent);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("VBButton ACUrlBinding failed. VBContent: " + this.VBContent);
#endif
                        //Visibility = Visibility.Collapsed;
                        return;
                    }
                    else
                    {
                        dcACTypeInfo = menuItem.HandlerACElement.ACType;
                        dcRightControlMode = Global.ControlModes.Enabled;
                    }
                }

                _VBContentTypeInfo = dcACTypeInfo;
                RightControlMode = dcRightControlMode;

                if (RightControlMode < Global.ControlModes.Disabled)
                {
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    if (VBContentMethodInfo != null || menuItem != null)
                    {
                        // Wurde ein VBButton per Datatemplate erzeugt, dann weiß man nicht zu welchem Datenobjekt (z.B. ausgewähltes Item in der Listbox)
                        // der Button gehört. Durch ein Binding vom Datenobjekt zum CommandParameter (CommandParameter={Binding }), wird der Wert im Commandparameter gesetzt.
                        // Damit nun per ACUrlCommand dieser Parameter mit gegeben werden kann wird der Wert in die ValueList eingetragen.
                        if (CommandParameter != null)
                        {
                            if (ParameterList == null)
                                ParameterList = new ACValueList();
                            ParameterList.Add(new ACValue("CommandParameter", CommandParameter));
                        }

                        if (menuItem != null)
                            ACCommand = menuItem;
                        else
                            ACCommand = new ACCommand(VBContentMethodInfo.ACCaption, VBContent, ParameterList);
                        if (AppCommands.FindVBApplicationCommand(ACCommand.ACUrl) == null)
                            _RemoveACCommand = true;

                        this.Command = AppCommands.AddApplicationCommand(ACCommand);
                        _CmdBindingExecute = new CommandBinding(this.Command, ucButton_Click, ucButton_IsEnabled);
                        CommandBindings.Add(_CmdBindingExecute);

                        // Falls Binding im XAML, dann keine Caption setzen
                        if (!BindingOperations.IsDataBound(this, ContentProperty))
                        {
                            if (this.Content == null)
                            {
                                if (menuItem != null)
                                {
                                    this.Content = menuItem.ACCaption;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(ACCaption))
                                        this.Content = VBContentMethodInfo.ACCaption;
                                    else
                                        this.Content = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
                                }
                            }
                        }
                    }

                    if (ReadLocalValue(CanExecuteCyclicProperty) != DependencyProperty.UnsetValue)
                    {
                        _dispTimer = new DispatcherTimer();
                        _dispTimer.Tick += new EventHandler(dispatcherTimer_CanExecute);
                        _dispTimer.Interval = new TimeSpan(0, 0, 0, 0, CanExecuteCyclic < 500 ? 500 : CanExecuteCyclic);
                        _dispTimer.Start();
                    }

                    if (BSOACComponent != null)
                    {
                        Binding binding = new Binding();
                        binding.Source = BSOACComponent;
                        binding.Path = new PropertyPath(Const.InitState);
                        binding.Mode = BindingMode.OneWay;
                        SetBinding(VBButton.ACCompInitStateProperty, binding);
                    }
                    if (ContextACObject != null && ContextACObject is IACComponent)
                    {
                        Binding binding = new Binding();
                        binding.Source = ContextACObject;
                        binding.Path = new PropertyPath(Const.ACUrlCmdMessage);
                        binding.Mode = BindingMode.OneWay;
                        SetBinding(VBButton.ACUrlCmdMessageProperty, binding);
                    }

                    if (AutoFocus)
                    {
                        Focus();
                        //MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    }
                }
            }
            else if (!String.IsNullOrEmpty(VBContentMouseDown) || !String.IsNullOrEmpty(VBContentMouseUp))
            {
                if (!String.IsNullOrEmpty(VBContentMouseDown))
                    CheckBinding(VBContentMemberMouseDown, ref _TypeMemberMouseDown);
                if (!String.IsNullOrEmpty(VBContentMouseUp))
                    CheckBinding(VBContentMemberMouseUp, ref _TypeMemberMouseUp);
            }

            if (!string.IsNullOrEmpty(VBToolTip) && ContextACObject != null)
            {
                ToolTip = this.Root().Environment.TranslateText(ContextACObject, VBToolTip);
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
            if (bso != null && bso is IACBSO)
                (bso as IACBSO).RemoveWPFRef(this.GetHashCode());
            this.Loaded -= CustomVBButton_Loaded;
            this.Unloaded -= CustomVBButton_Unloaded;
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
                _dispTimer.Tick -= dispatcherTimer_CanExecute;
                _dispTimer = null;
            }
            _VBContentTypeInfo = null;


            if (_RemoveACCommand)
            {
                if (ACCommand != null)
                {
                    ICommand command = AppCommands.FindVBApplicationCommand(ACCommand.ACUrl);
                    if (command != null)
                    {
                        AppCommands.RemoveVBApplicationCommand(command);
                        CommandBindings.RemoveCommandBinding(command as RoutedUICommandEx);
                    }
                }
            }

            if (_CmdBindingExecute != null)
                this.CommandBindings.Remove(_CmdBindingExecute); //handle paste
            this.Command = null;
            _ParameterList = null;
            ACCommand = null;
            _ACContentList = null;
            _TypeMemberMouseDown = null;
            _TypeMemberMouseUp = null;
            BindingOperations.ClearBinding(this, VBButton.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBButton.ACCompInitStateProperty);
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

        #region Dependcy-Properties
        /// <summary>
        /// Represents the dependency property for ContentStroke.
        /// </summary>
        public static readonly DependencyProperty ContentStrokeProperty
            = DependencyProperty.Register("ContentStroke", typeof(Brush), typeof(VBButton));

        /// <summary>
        /// Gets or sets the stroke of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Strich des Inhalts.
        /// </summary>
        [Category("VBControl")]
        public Brush ContentStroke
        {
            get { return (Brush)GetValue(ContentStrokeProperty); }
            set { SetValue(ContentStrokeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ContentFill.
        /// </summary>
        public static readonly DependencyProperty ContentFillProperty
            = DependencyProperty.Register("ContentFill", typeof(Brush), typeof(VBButton));

        /// <summary>
        /// Gets or sets the fill of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Füllung des Inhalts.
        /// </summary>
        [Category("VBControl")]
        public Brush ContentFill
        {
            get { return (Brush)GetValue(ContentFillProperty); }
            set { SetValue(ContentFillProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBButton));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get { return (Global.ControlModes)GetValue(ControlModeProperty); }
            set { SetValue(ControlModeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly DependencyProperty ACUrlCmdMessageProperty =
            DependencyProperty.Register("ACUrlCmdMessage",
                typeof(ACUrlCmdMessage), typeof(VBButton),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBButton),
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
            VBButton thisControl = dependencyObject as VBButton;
            if (thisControl == null)
                return;
            if (args.Property == ACUrlCmdMessageProperty)
                thisControl.OnACUrlMessageReceived();
            else if (args.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (args.Property == BSOACComponentProperty)
            {
                if (args.NewValue == null && args.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = args.OldValue as IACBSO;
                    if (bso != null && thisControl.CommandParameter == null)
                        thisControl.DeInitVBControl(bso);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of ACText, which will be shown in tool tip.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Namen von ACText, der im Tooltip angezeigt wird.
        /// </summary>
        [Category("VBControl")]
        public string VBToolTip
        {
            get;
            set;
        }

        /// <summary>
        /// Get the touch inside or outside of this element. Return true if is outside, otherwise returns false.
        /// </summary>
        public bool IsTouchLeave
        {
            get { return (bool)GetValue(IsTouchLeaveProperty); }
            private set { SetValue(IsTouchLeaveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsTouchActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsTouchLeaveProperty =
            DependencyProperty.Register("IsTouchLeave", typeof(bool), typeof(VBButton), new PropertyMetadata(false));



        #endregion

        #region Loaded Event
        bool _Loaded;
        void CustomVBButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (_dispTimer != null)
            {
                if (!_dispTimer.IsEnabled)
                    _dispTimer.Start();
            }
            if (_Loaded)
                return;
            if (!_Initialized && BindOnLoadEvent)
                InitVBControl(true);

            if (BSOACComponent != null)
            {
                IACObject source = null;
                if (this.ACCommand != null && this.ACCommand.HandlerACElement != null)
                    source = this.ACCommand.HandlerACElement;
                if (source == null)
                    source = this.ContextACObject;
                try
                {
                    if (source != null)
                        BSOACComponent.AddWPFRef(this.GetHashCode(), source);
                }
                catch (Exception exw)
                {
                    this.Root().Messages.LogDebug("VBButton", "AddWPFRef", exw.Message);
                }
            }
            _Loaded = true;

        }

        void CustomVBButton_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
            }
            if (!_Loaded)
                return;

            try
            {
                if (BSOACComponent != null)
                    BSOACComponent.RemoveWPFRef(this.GetHashCode());
            }
            catch (Exception exw)
            {
                this.Root().Messages.LogDebug("VBButton", "RemoveWPFRef", exw.Message);
            }

            _Loaded = false;
        }

        private bool CheckBinding(string url, ref IACType dcACTypeInfo)
        {
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(url, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBButton", VBContent);
                return false;
            }
            return true;
        }


        private bool _RemoveACCommand = false;

        #endregion

        #region Event-Handling

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

        /// <summary>
        /// Handles the OnMouseRightButtonDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (BSOACComponent == null)
                return;
            if (DisableContextMenu)
            {
                e.Handled = true;
                return;
            }
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
            base.OnMouseRightButtonDown(e);
        }

        protected override void OnTouchEnter(TouchEventArgs e)
        {
            IsTouchLeave = false;
            base.OnTouchEnter(e);
        }

        protected override void OnTouchLeave(TouchEventArgs e)
        {
            IsTouchLeave = true;
            base.OnTouchLeave(e);
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
        /// Gets the ACMethod name.
        /// </summary>
        public string ACMethodName
        {
            get
            {
                int pos = VBContent.IndexOf('!');
                if (pos == 0)
                    return VBContent.Substring(1);
                else
                    return VBContent;
            }
        }

        ACValueList _ParameterList;
        /// <summary>
        /// Gets or sets the list of parameters.
        /// </summary>
        public ACValueList ParameterList
        {
            get
            {
                return _ParameterList;
            }
            set
            {
                _ParameterList = value;
            }
        }

        /// <summary>
        /// Represents the dependency property for CanExecuteCyclic.
        /// </summary>
        public static readonly DependencyProperty CanExecuteCyclicProperty =
            ContentPropertyHandler.CanExecuteCyclicProperty.AddOwner(typeof(VBButton), new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Determines is cyclic execution enabled or disabled.The value (integer) in this property determines the interval of cyclic execution in miliseconds.   
        /// </summary>
        /// <summary xml:lang="de">
        /// Bestimmt, ob die zyklische Ausführung aktiviert oder deaktiviert ist. Der Wert (Integer) in dieser Eigenschaft bestimmt das Intervall der zyklischen Ausführung in Millisekunden.
        /// </summary>
        [Category("VBControl")]
        public int CanExecuteCyclic
        {
            get { return (int)GetValue(CanExecuteCyclicProperty); }
            set { SetValue(CanExecuteCyclicProperty, value); }
        }


        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly DependencyProperty DisableContextMenuProperty =
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner(typeof(VBButton), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));

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


        /// <summary>
        /// Updates a control mode.
        /// </summary>
        public void UpdateControlMode()
        {
            IACComponent elementACComponent = ContextACObject as IACComponent;
            // Bei Datatemplates ist Button an IACObjects gebunden, die keine ControlModes handeln können
            if (elementACComponent == null)
                elementACComponent = BSOACComponent;
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
        /// Handles the ACUrl message when it is received.
        /// </summary>
        public void OnACUrlMessageReceived()
        {
            if (ACUrlCmdMessage != null
                && ACUrlCmdMessage.ACUrl == Const.CmdInvalidateRequerySuggested
                && !String.IsNullOrEmpty(this.VBContent)
                && !String.IsNullOrEmpty(ACUrlCmdMessage.TargetVBContent)
                && (this.VBContent == ACUrlCmdMessage.TargetVBContent
                   || this.VBContent.Contains(ACUrlCmdMessage.TargetVBContent)))
            {
                UpdateControlMode();
                CommandManager.InvalidateRequerySuggested();
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

        IACType _VBContentTypeInfo = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _VBContentTypeInfo as ACClassProperty;
            }
        }

        /// <summary>
        /// Gets the ACClassMethod which is bounded by VBContent.
        /// </summary>
        public ACClassMethod VBContentMethodInfo
        {
            get
            {
                return _VBContentTypeInfo as ACClassMethod;
            }
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
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty DisabledModesProperty
            = DependencyProperty.Register("DisabledModes", typeof(string), typeof(VBButton));
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
            get { return (string)GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
        }

        #endregion

        #region IACInteractiveObject Member
        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBButton));

        /// <summary>
        /// Represents the property in which you enter the name of method which will be invoked on button click. In front of method name must be an exclamation mark.
        /// The method must be marked with attribute [ACMethodInfo(...)]. XAML sample: VBContent="!MyMethod"
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


        public static readonly DependencyProperty BindToBSOProperty
            = DependencyProperty.Register("BindToBSO", typeof(bool), typeof(VBButton));

        [Category("VBControl")]
        public bool BindToBSO
        {
            get { return (bool)GetValue(BindToBSOProperty); }
            set { SetValue(BindToBSOProperty, value); }
        }

        public static readonly DependencyProperty BindOnLoadEventProperty
            = DependencyProperty.Register("BindOnLoadEvent", typeof(bool), typeof(VBButton));

        [Category("VBControl")]
        public bool BindOnLoadEvent
        {
            get { return (bool)GetValue(BindOnLoadEventProperty); }
            set { SetValue(BindOnLoadEventProperty, value); }
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
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBButton), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        ACCommand _ACCommand;
        /// <summary>
        /// Gets or sets the ACCommand.
        /// </summary>
        public ACCommand ACCommand
        {
            get
            {
                return _ACCommand;
            }
            set
            {
                _ACCommand = value;
                if (_ACContentList != null)
                {
                    _ACContentList.Clear();
                    if (_ACCommand != null)
                        _ACContentList.Add(value);
                }
            }
        }

        List<IACObject> _ACContentList = new List<IACObject>();
        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return _ACContentList;
            }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            IACComponent acComponent = ContextACObject as IACComponent;
            if (acComponent == null)
                acComponent = BSOACComponent;
            if (acComponent == null)
                return;
            actionArgs.DropObject = this;
            acComponent.ACAction(actionArgs);
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            IACComponent acComponent = ContextACObject as IACComponent;
            if (acComponent == null)
                acComponent = BSOACComponent;
            if (acComponent == null)
                return false;
            actionArgs.DropObject = this;
            return acComponent.IsEnabledACAction(actionArgs);
        }
        #endregion

        #region IACObject Member
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get;
            set;
        }

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

        #region Methods
        /// <summary>
        /// Handles the Button click event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="e">The RoutedEvent agruments.</param>
        protected virtual void ucButton_Click(object sender, RoutedEventArgs e)
        {
            RoutedUICommandEx vbCommand = null;
            ExecutedRoutedEventArgs eEx = e as ExecutedRoutedEventArgs;
            if (eEx != null)
                vbCommand = eEx.Command as RoutedUICommandEx;
            ACActionArgs actionArgs = null;
            if (vbCommand != null)
            {
                if (vbCommand.ACCommand.HandlerACElement != null && vbCommand.ACCommand.HandlerACElement != this)
                {
                    actionArgs = new ACActionArgs(this, 0, 0, Global.ElementActionType.ACCommand);
                    vbCommand.ACCommand.HandlerACElement.ACAction(actionArgs);
                    return;
                }
            }
            if (actionArgs == null)
            {
                #region @aagincic: Temp fix with grid button without action parameters
                if (ParameterList == null)
                    ParameterList = new ACValueList();

                if (CommandParameter != null)
                {
                    foreach (ACValue valueItem in ParameterList.ToArray())
                    {
                        if (valueItem.ACIdentifier == "CommandParameter")
                            ParameterList.Remove(valueItem);
                    }
                    ParameterList.Add(new ACValue("CommandParameter", CommandParameter));

                    ACCommand acCommand = ACContentList.Where(c => c is ACCommand).FirstOrDefault() as ACCommand;
                    if (acCommand != null)
                        acCommand.ParameterList = ParameterList;
                }
                #endregion

                actionArgs = new ACActionArgs(this, 0, 0, Global.ElementActionType.ACCommand);
                ACAction(actionArgs);
            }
        }

        private void ucButton_IsEnabled(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.Visibility == System.Windows.Visibility.Collapsed
                || this.Visibility == System.Windows.Visibility.Hidden
                || this.RightControlMode <= Global.ControlModes.Disabled)
            {
                e.CanExecute = false;
                return;
            }

            IACComponent acComponent = ContextACObject as IACComponent;
            if (acComponent == null)
                acComponent = BSOACComponent;
            if (acComponent != null)
            {
                RoutedUICommandEx vbCommand = e.Command as RoutedUICommandEx;
                ACActionArgs actionArgs = null;
                if (vbCommand != null)
                {
                    if (vbCommand.ACCommand.HandlerACElement != null && vbCommand.ACCommand.HandlerACElement != this)
                    {
                        actionArgs = new ACActionArgs(this, 0, 0, Global.ElementActionType.ACCommand);
                        if (this.IsLoaded)
                            e.CanExecute = vbCommand.ACCommand.HandlerACElement.IsEnabledACAction(actionArgs);
                    }
                }
                if (actionArgs == null)
                {
                    #region @aagincic: Temp fix with grid button without action parameters
                    if (ParameterList == null)
                        ParameterList = new ACValueList();

                    if (CommandParameter != null)
                    {
                        foreach (ACValue valueItem in ParameterList.ToArray())
                        {
                            if (valueItem.ACIdentifier == "CommandParameter")
                                ParameterList.Remove(valueItem);
                        }
                        ParameterList.Add(new ACValue("CommandParameter", CommandParameter));

                        ACCommand acCommand = ACContentList.Where(c => c is ACCommand).FirstOrDefault() as ACCommand;
                        if (acCommand != null)
                            acCommand.ParameterList = ParameterList;
                    }
                    #endregion

                    actionArgs = new ACActionArgs(this, 0, 0, Global.ElementActionType.ACCommand);
                    if (this.IsLoaded)
                        e.CanExecute = IsEnabledACAction(actionArgs);
                }
            }
            else
            {
                e.CanExecute = false;
            }
        }

        private DispatcherTimer _dispTimer = null;
        private void dispatcherTimer_CanExecute(object sender, EventArgs e)
        {
            //this.CommandBindings[0].Command.Execute("test");
            //ApplicationCommands.New.Execute("test", this);
            //ACActionArgs actionArgs = new ACActionArgs(this, 0, 0, ElementActionType.ACCommand);
            //this.IsEnabled = IsEnabledACAction(actionArgs);
            //(Command as RoutedCommand).CanExecute(null,this);
            CommandManager.InvalidateRequerySuggested();
            //InvalidateVisual();
            //this.Command.CanExecute

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

        #region Mouse-Up-Down
        /// <summary>
        /// Represents the dependency property for VBContentMouseDown.
        /// </summary>
        public static readonly DependencyProperty VBContentMouseDownProperty
            = DependencyProperty.Register("VBContentMouseDown", typeof(string), typeof(VBButton));

        /// <summary>
        /// Gets or sets VBContent for mouse down.
        /// </summary>
        [Category("VBControl")]
        public string VBContentMouseDown
        {
            get { return (string)GetValue(VBContentMouseDownProperty); }
            set { SetValue(VBContentMouseDownProperty, value); }
        }

        /// <summary>
        /// Gets the VBContent member for mouse down.
        /// </summary>
        protected string VBContentMemberMouseDown
        {
            get
            {
                if (String.IsNullOrEmpty(VBContentMouseDown))
                    return "";
                string[] split = VBContentMouseDown.Split('=');
                return split[0];
            }
        }

        /// <summary>
        /// Gets the VBContent of assigned value on mouse down.
        /// </summary>
        protected string VBContentAssignedValueMouseDown
        {
            get
            {
                if (String.IsNullOrEmpty(VBContentMouseDown))
                    return "";
                string[] split = VBContentMouseDown.Split('=');
                if (split.Count() <= 1)
                    return "";
                return split[1];
            }
        }

        IACType _TypeMemberMouseDown = null;
        /// <summary>
        /// Gets the type of member mouse down.
        /// </summary>
        public IACType TypeMemberMouseDown
        {
            get
            {
                return _TypeMemberMouseDown;
            }
        }

        /// <summary>
        /// Represents the dependency property for VBContentMouseUp.
        /// </summary>
        public static readonly DependencyProperty VBContentMouseUpProperty
            = DependencyProperty.Register("VBContentMouseUp", typeof(string), typeof(VBButton));

        /// <summary>
        /// Gets or sets the VBContent for mouse up.
        /// </summary>
        [Category("VBControl")]
        public string VBContentMouseUp
        {
            get { return (string)GetValue(VBContentMouseUpProperty); }
            set { SetValue(VBContentMouseUpProperty, value); }
        }

        /// <summary>
        /// Gets the VBContent member for mouse up.
        /// </summary>
        protected string VBContentMemberMouseUp
        {
            get
            {
                if (String.IsNullOrEmpty(VBContentMouseUp))
                    return "";
                string[] split = VBContentMouseUp.Split('=');
                return split[0];
            }
        }

        /// <summary>
        /// Gets the VBContent of assigned value on mouse up.
        /// </summary>
        protected string VBContentAssignedValueMouseUp
        {
            get
            {
                if (String.IsNullOrEmpty(VBContentMouseUp))
                    return "";
                string[] split = VBContentMouseUp.Split('=');
                if (split.Count() <= 1)
                    return "";
                return split[1];
            }
        }

        IACType _TypeMemberMouseUp = null;
        /// <summary>
        /// Gets the type of member mouse up.
        /// </summary>
        public IACType TypeMemberMouseUp
        {
            get
            {
                return _TypeMemberMouseUp;
            }
        }


        /// <summary>
        /// Handles the OnMouseLeftButtonDown event.
        /// </summary>
        /// <param name="e">The MouseButtonEvent arguments.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!String.IsNullOrEmpty(VBContentMemberMouseDown) && ContextACObject != null)
            {
                if (!String.IsNullOrEmpty(VBContentAssignedValueMouseDown) && TypeMemberMouseDown != null)
                {
                    try
                    {
                        object value = Convert.ChangeType(VBContentAssignedValueMouseDown, TypeMemberMouseDown.ObjectType);
                        ContextACObject.ACUrlCommand(VBContentMemberMouseDown, value);
                    }
                    catch (Exception ec)
                    {
                        string msg = ec.Message;
                        if (ec.InnerException != null && ec.InnerException.Message != null)
                            msg += " Inner:" + ec.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("VBButton", "OnMouseLeftButtonDown", msg);
                    }
                }
                else if (String.IsNullOrEmpty(VBContentAssignedValueMouseDown))
                {
                    ContextACObject.ACUrlCommand(VBContentMemberMouseDown);
                }
            }
            base.OnMouseLeftButtonDown(e);
        }

        /// <summary>
        /// Handles the OnMouseLeftButtonUp event.
        /// </summary>
        /// <param name="e">The MouseButtonEvent arguments.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (!String.IsNullOrEmpty(VBContentMemberMouseUp) && ContextACObject != null)
            {
                if (!String.IsNullOrEmpty(VBContentAssignedValueMouseUp) && TypeMemberMouseUp != null)
                {
                    try
                    {
                        object value = Convert.ChangeType(VBContentAssignedValueMouseUp, TypeMemberMouseUp.ObjectType);
                        ContextACObject.ACUrlCommand(VBContentMemberMouseUp, value);
                    }
                    catch (Exception ec)
                    {
                        string msg = ec.Message;
                        if (ec.InnerException != null && ec.InnerException.Message != null)
                            msg += " Inner:" + ec.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("VBButton", "OnMouseLeftButtonUp", msg);
                    }
                }
                else if (String.IsNullOrEmpty(VBContentAssignedValueMouseUp))
                {
                    ContextACObject.ACUrlCommand(VBContentMemberMouseUp);
                }
            }
            base.OnMouseLeftButtonUp(e);
        }

        #endregion
    }
}
