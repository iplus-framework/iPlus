// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace gip.core.layoutengine.avui
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
        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            Loaded += CustomVBButton_Loaded;
            Unloaded += CustomVBButton_Unloaded;
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }

        bool _Initialized = false;
        private Avalonia.Labs.Input.CommandBinding _CmdBindingExecute;

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
                        //IsVisible = false;
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
                    IsVisible = false;
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
                        _CmdBindingExecute = new Avalonia.Labs.Input.CommandBinding();
                        _CmdBindingExecute.Command = this.Command;
                        _CmdBindingExecute.Executed += ucButton_Click;
                        _CmdBindingExecute.CanExecute += ucButton_IsEnabled;
                        Avalonia.Labs.Input.CommandManager.SetCommandBindings(this, new List<Avalonia.Labs.Input.CommandBinding> { _CmdBindingExecute });

                        // Falls Binding im XAML, dann keine Caption setzen
                        // TODO: Check if there's an Avalonia equivalent for IsDataBound
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

                    if (this.IsSet(CanExecuteCyclicProperty))
                    {
                        _dispTimer = new DispatcherTimer();
                        _dispTimer.Tick += dispatcherTimer_CanExecute;
                        _dispTimer.Interval = new TimeSpan(0, 0, 0, 0, CanExecuteCyclic < 500 ? 500 : CanExecuteCyclic);
                        _dispTimer.Start();
                    }

                    if (BSOACComponent != null)
                    {
                        var binding = new Binding
                        {
                            Source = BSOACComponent,
                            Path = Const.InitState,
                            Mode = BindingMode.OneWay
                        };
                        this.Bind(VBButton.ACCompInitStateProperty, binding);
                    }
                    if (ContextACObject != null && ContextACObject is IACComponent)
                    {
                        var binding = new Binding
                        {
                            Source = ContextACObject,
                            Path = Const.ACUrlCmdMessage,
                            Mode = BindingMode.OneWay
                        };
                        this.Bind(VBButton.ACUrlCmdMessageProperty, binding);
                    }

                    if (AutoFocus)
                    {
                        Focus();
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
                ToolTip.SetTip(this, this.Root().Environment.TranslateText(ContextACObject, VBToolTip));
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
                    System.Windows.Input.ICommand command = AppCommands.FindVBApplicationCommand(ACCommand.ACUrl);
                    if (command != null)
                    {
                        AppCommands.RemoveVBApplicationCommand(command);
                        // TODO: CommandBindings.RemoveCommandBinding for Avalonia
                    }
                }
            }

            // TODO: CommandBindings.Remove for Avalonia
            this.Command = null;
            _ParameterList = null;
            ACCommand = null;
            _ACContentList = null;
            _TypeMemberMouseDown = null;
            _TypeMemberMouseUp = null;
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

        #region Dependency-Properties
        /// <summary>
        /// Represents the dependency property for ContentStroke.
        /// </summary>
        public static readonly StyledProperty<IBrush> ContentStrokeProperty =
            AvaloniaProperty.Register<VBButton, IBrush>(nameof(ContentStroke));

        /// <summary>
        /// Gets or sets the stroke of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den Strich des Inhalts.
        /// </summary>
        [Category("VBControl")]
        public IBrush ContentStroke
        {
            get { return GetValue(ContentStrokeProperty); }
            set { SetValue(ContentStrokeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ContentFill.
        /// </summary>
        public static readonly StyledProperty<IBrush> ContentFillProperty =
            AvaloniaProperty.Register<VBButton, IBrush>(nameof(ContentFill));

        /// <summary>
        /// Gets or sets the fill of content.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Füllung des Inhalts.
        /// </summary>
        [Category("VBControl")]
        public IBrush ContentFill
        {
            get { return GetValue(ContentFillProperty); }
            set { SetValue(ContentFillProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBButton, Global.ControlModes>(nameof(ControlMode));

        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get { return GetValue(ControlModeProperty); }
            set { SetValue(ControlModeProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBButton, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

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
            AvaloniaProperty.Register<VBButton, ACInitState>(nameof(ACCompInitState));

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
            VBButton thisControl = this;
            if (change.Property == ACUrlCmdMessageProperty)
                thisControl.OnACUrlMessageReceived();
            else if (change.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
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
            get { return GetValue(IsTouchLeaveProperty); }
            private set { SetValue(IsTouchLeaveProperty, value); }
        }

        public static readonly StyledProperty<bool> IsTouchLeaveProperty =
            AvaloniaProperty.Register<VBButton, bool>(nameof(IsTouchLeave), false);

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
        /// Handles the OnPointerReleased event for context menu and mouse up.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == Avalonia.Input.MouseButton.Right)
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
                    ContextMenu.Open();
                }
                e.Handled = true;
            }
            else if (e.InitialPressMouseButton == Avalonia.Input.MouseButton.Left)
            {
                // Handle mouse up for VBContentMouseUp
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
                                datamodel.Database.Root.Messages.LogException("VBButton", "OnPointerReleased", msg);
                        }
                    }
                    else if (String.IsNullOrEmpty(VBContentAssignedValueMouseUp))
                    {
                        ContextACObject.ACUrlCommand(VBContentMemberMouseUp);
                    }
                }
            }
            IsTouchLeave = true;
            base.OnPointerReleased(e);
        }

        /// <summary>
        /// Handles the OnPointerPressed event for mouse down.
        /// </summary>
        /// <param name="e">The PointerPressed arguments.</param>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
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
                                datamodel.Database.Root.Messages.LogException("VBButton", "OnPointerPressed", msg);
                        }
                    }
                    else if (String.IsNullOrEmpty(VBContentAssignedValueMouseDown))
                    {
                        ContextACObject.ACUrlCommand(VBContentMemberMouseDown);
                    }
                }
            }
            IsTouchLeave = false;
            base.OnPointerPressed(e);
        }

        // TODO: Touch events for Avalonia if needed
        /*
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
        */

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
        public static readonly AttachedProperty<int> CanExecuteCyclicProperty =
            ContentPropertyHandler.CanExecuteCyclicProperty.AddOwner<VBButton>();

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
        public static readonly AttachedProperty<bool> DisableContextMenuProperty =
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBButton>();

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

            RemoteCommandAdornerManager.Instance.VisualizeIfRemoteControlled(this, elementACComponent, true);
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
                // TODO: CommandManager.InvalidateRequerySuggested equivalent for Avalonia
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
        /// Represents the dependency property for DisabledModes.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty =
            AvaloniaProperty.Register<VBButton, string>(nameof(DisabledModes));
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

        #endregion

        #region IACInteractiveObject Member
        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBButton, string>(nameof(VBContent));

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
            get { return GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }


        public static readonly StyledProperty<bool> BindToBSOProperty =
            AvaloniaProperty.Register<VBButton, bool>(nameof(BindToBSO));

        [Category("VBControl")]
        public bool BindToBSO
        {
            get { return GetValue(BindToBSOProperty); }
            set { SetValue(BindToBSOProperty, value); }
        }

        public static readonly StyledProperty<bool> BindOnLoadEventProperty =
            AvaloniaProperty.Register<VBButton, bool>(nameof(BindOnLoadEvent));

        [Category("VBControl")]
        public bool BindOnLoadEvent
        {
            get { return GetValue(BindOnLoadEventProperty); }
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
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBButton>();
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
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
        protected virtual void ucButton_Click(object sender, Avalonia.Labs.Input.ExecutedRoutedEventArgs e)
        {
            RoutedUICommandEx vbCommand = null;
            if (e != null)
                vbCommand = e.Command as RoutedUICommandEx;
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

        private void ucButton_IsEnabled(object sender, Avalonia.Labs.Input.CanExecuteRoutedEventArgs e)
        {
            if (!this.IsVisible
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

                    RemoteCommandAdornerManager.Instance.VisualizeIfRemoteControlled(this, acComponent, true);
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
            // TODO: CommandManager.InvalidateRequerySuggested equivalent for Avalonia
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
        public static readonly StyledProperty<string> VBContentMouseDownProperty =
            AvaloniaProperty.Register<VBButton, string>(nameof(VBContentMouseDown));

        /// <summary>
        /// Gets or sets VBContent for mouse down.
        /// </summary>
        [Category("VBControl")]
        public string VBContentMouseDown
        {
            get { return GetValue(VBContentMouseDownProperty); }
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
        public static readonly StyledProperty<string> VBContentMouseUpProperty =
            AvaloniaProperty.Register<VBButton, string>(nameof(VBContentMouseUp));

        /// <summary>
        /// Gets or sets the VBContent for mouse up.
        /// </summary>
        [Category("VBControl")]
        public string VBContentMouseUp
        {
            get { return GetValue(VBContentMouseUpProperty); }
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

        #endregion
    }
}
