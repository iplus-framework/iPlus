using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System.Transactions;
using System.Reflection;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Labs.Input;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Controls.Presenters;
using Avalonia.Input;

namespace gip.core.layoutengine.avui
{
    [TemplatePart("PART_PreviousButtonHorizontal", typeof(Button))]
    [TemplatePart("PART_NextButtonHorizontal", typeof(Button))]
    [TemplatePart("PART_PreviousButtonVertical", typeof(Button))]
    [TemplatePart("PART_NextButtonVertical", typeof(Button))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBFlipView'}de{'VBFlipView'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBFlipView : SelectingItemsControl, IVBContent, IVBSource, IACObject
    {
        #region c'tors
        string _DataSource;
        string _DataShowColumns;
        string _DataChilds;

        private static readonly FuncTemplate<Panel> DefaultPanel =
            new FuncTemplate<Panel>(() => new StackPanel()
            {
                Orientation = Orientation.Vertical
            });

        static VBFlipView()
        {
            SelectionModeProperty.OverrideDefaultValue<VBFlipView>(SelectionMode.AlwaysSelected);
            ItemsPanelProperty.OverrideDefaultValue<VBFlipView>(DefaultPanel);
            AutoScrollToSelectedItemProperty.OverrideDefaultValue<VBFlipView>(false);
        }

        /// <summary>
        /// Creates a new instance of VBFlipView.
        /// </summary>
        public VBFlipView()
        {
            CommandManager.SetCommandBindings(this, 
                new List<CommandBinding> 
                { 
                    new CommandBinding(NextCommand, this.OnNextExecuted, this.OnNextCanExecute),
                    new CommandBinding(PreviousCommand, this.OnPreviousExecuted, this.OnPreviousCanExecute) 
                });
            AddHandler(PointerWheelChangedEvent, FlipPointerWheelChanged, handledEventsToo: true);
            AddHandler(KeyDownEvent, FlipKeyDown, handledEventsToo: true);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Loaded += VBFlipView_Loaded;
            this.Unloaded += VBFlipView_Unloaded;
        }

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey) => new FlipViewItem();

        protected override bool NeedsContainerOverride(object item, int index, out object recycleKey)
        {
            return NeedsContainer<FlipViewItem>(item, out recycleKey);
        }

        protected override void PrepareContainerForItemOverride(Control element, object item, int index)
        {
            if (element is FlipViewItem viewItem)
            {
                viewItem.Content = item;
                element.Width = GetDesiredItemWidth();
                element.Height = GetDesiredItemHeight();
            }
            base.PrepareContainerForItemOverride(element, item, index);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();

            ItemsPresenterPart = e.NameScope.Get<ItemsPresenter>("PART_ItemsPresenter");

            _nextButtonHorizontal = e.NameScope.Get<Button>("PART_NextButtonHorizontal");
            _previousButtonHorizontal = e.NameScope.Get<Button>("PART_PreviousButtonHorizontal");
            _nextButtonVertical = e.NameScope.Get<Button>("PART_NextButtonVertical");
            _previousButtonVertical = e.NameScope.Get<Button>("PART_PreviousButtonVertical");

            //this.PART_PreviousItem = e.NameScope.Find("PART_PreviousItem") as ContentControl;
            //this.PART_NextItem = e.NameScope.Find("PART_NextItem") as ContentControl;
            //this.PART_CurrentItem = e.NameScope.Find("PART_CurrentItem") as ContentControl;
            this.PART_Root = e.NameScope.Find("PART_Root") as Control;
            this.PART_Container = e.NameScope.Find("PART_Container") as Control;

            if (_nextButtonHorizontal != null)
            {
                _nextButtonHorizontal.Click += NextButton_Click;
            }

            if (_nextButtonVertical != null)
            {
                _nextButtonVertical.Click += NextButton_Click;
            }

            if (_previousButtonHorizontal != null)
            {
                _previousButtonHorizontal.Click += PreviousButton_Click;
            }

            if (_previousButtonVertical != null)
            {
                _previousButtonVertical.Click += PreviousButton_Click;
            }

            if (ScrollViewerPart != null)
            {
                ScrollViewerPart.RemoveHandler(Gestures.ScrollGestureEndedEvent, ScrollEndedEventHandler);
                ScrollViewerPart.SizeChanged -= ScrollViewerPart_SizeChanged;
            }

            ScrollViewerPart = e.NameScope.Find<FlipViewScrollViewer>("PART_ScrollViewer");

            if (ScrollViewerPart != null)
            {
                ScrollViewerPart.AddHandler(Gestures.ScrollGestureEndedEvent, ScrollEndedEventHandler, handledEventsToo: true);
                ScrollViewerPart.SizeChanged += ScrollViewerPart_SizeChanged;
            }

            _isApplied = true;

            SetButtonsVisibility();
        }
        #endregion

        #region Private Fields
        private Button _previousButtonVertical;
        private Button _nextButtonHorizontal;
        private Button _previousButtonHorizontal;
        private Button _nextButtonVertical;
        private bool _isApplied;
        private bool _isHorizontal;

        private Control PART_Root;
        private Control PART_Container;
        //private double fromValue = 0.0;
        //private double elasticFactor = 1.0;
        #endregion

        #region Additional Dependenc-Properties

        /// <summary>
        /// Defines the <see cref="IsButtonsVisible"/> property.
        /// </summary>
        public static StyledProperty<bool> IsButtonsVisibleProperty = AvaloniaProperty.Register<VBFlipView, bool>(nameof(IsButtonsVisible), defaultValue: true);

        /// <summary>
        /// Defines the <see cref="TransitionDuration"/> property.
        /// </summary>
        public static readonly StyledProperty<TimeSpan?> TransitionDurationProperty =
            FlipViewScrollViewer.TransitionDurationProperty.AddOwner<VBFlipView>();

        private bool _arranged;

        internal ItemsPresenter ItemsPresenterPart { get; private set; }
        internal FlipViewScrollViewer ScrollViewerPart { get; private set; }

        /// <summary>
        /// Gets or sets whether navigation buttons are visible on the flipview.
        /// </summary>
        public bool IsButtonsVisible
        {
            get => GetValue(IsButtonsVisibleProperty);
            set => SetValue(IsButtonsVisibleProperty, value);
        }

        /// <summary>
        /// Gets or sets the duration of transitions for the flipview.
        /// </summary>
        public TimeSpan? TransitionDuration
        {
            get => GetValue(TransitionDurationProperty);
            set => SetValue(TransitionDurationProperty, value);
        }


        /// <summary>
        /// Represents the styled property for control mode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBFlipView, Global.ControlModes>(nameof(ControlMode));
        
        /// <summary>
        /// Gets or sets the Control mode.
        /// </summary>
        public Global.ControlModes ControlMode
        {
            get => GetValue(ControlModeProperty);
            set => SetValue(ControlModeProperty, value);
        }


        /// <summary>
        /// Represents the styled property for RightControlMode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> RightControlModeProperty =
            AvaloniaProperty.Register<VBFlipView, Global.ControlModes>(nameof(RightControlMode));

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
            get => GetValue(RightControlModeProperty);
            set => SetValue(RightControlModeProperty, value);
        }

        /// <summary>
        /// Represents the styled property for ACCaption.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionProperty =
            AvaloniaProperty.Register<VBFlipView, string>(Const.ACCaptionPrefix);
        
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get => GetValue(ACCaptionProperty);
            set => SetValue(ACCaptionProperty, value);
        }

        /// <summary>
        /// Represents the styled property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionTransProperty =
            AvaloniaProperty.Register<VBFlipView, string>(nameof(ACCaptionTrans));
        
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
            get => GetValue(ACCaptionTransProperty);
            set => SetValue(ACCaptionTransProperty, value);
        }


        /// <summary>
        /// Represents the styled property for ShowCaption.
        /// </summary>
        public static readonly StyledProperty<bool> ShowCaptionProperty =
            AvaloniaProperty.Register<VBFlipView, bool>(nameof(ShowCaption), defaultValue: true);
        
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
            get => GetValue(ShowCaptionProperty);
            set => SetValue(ShowCaptionProperty, value);
        }
        #endregion

        #region Loaded-Event
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
            if (String.IsNullOrEmpty(VBContent))
                return;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBFlipView", VBContent);
                return;
            }
            _ACTypeInfo = dcACTypeInfo;


            if (!this.IsSet(VBFlipView.RightControlModeProperty)
                || RightControlMode < dcRightControlMode)
            {
                RightControlMode = dcRightControlMode;
            }

            Binding binding = null;
            if (BSOACComponent != null)
            {
                binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = Const.InitState;
                binding.Mode = BindingMode.OneWay;
                this.Bind(VBFlipView.ACCompInitStateProperty, binding);
            }

            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            if (_ACTypeInfo == null)
                return;

            // Beschriftung
            if (string.IsNullOrEmpty(ACCaption))
                ACCaptionTrans = _ACTypeInfo.ACCaption;
            else
                ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);

            if (ItemTemplate == null)
            {
                this.Root().Messages.LogDebug("Error00005", "VBFlipView", VBShowColumns + " " + VBContent);
                //this.Root().Messages.Error(ContextACObject, "Error00005", "VBFlipView", VBShowColumns, VBContent);
                return;
            }


            binding = new Binding();
            binding.Source = dcSource;
            this.Bind(VBFlipView.ItemsSourceProperty, binding);

            Binding binding2 = new Binding();
            binding2.Source = dcSource;
            binding2.Path = dcPath;
            if (VBContentPropertyInfo != null)
                binding2.Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay;
            binding2.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            this.Bind(VBFlipView.SelectedValueProperty, binding2);

            if (AutoFocus)
            {
                Focus();
                //MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        bool _Loaded = false;
        void VBFlipView_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();

            if (_Loaded)
                return;

            if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
            {
                var boundedValue = BindingOperations.GetBindingExpressionBase(this, VBFlipView.ItemsSourceProperty);
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
                        this.Root().Messages.LogDebug("VBFlipView", "AddWPFRef", exw.Message);
                    }
                }
            }
            _Loaded = true;
        }

        void VBFlipView_Unloaded(object sender, RoutedEventArgs e)
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
            this.Loaded -= VBFlipView_Loaded;
            this.Unloaded -= VBFlipView_Unloaded;
            _ACTypeInfo = null;
            _ACAccess = null;

            this.ClearAllBindings();
            this.ItemsSource = null;
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

        #region IDataField Members

        /// <summary>
        /// Represents the styled property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBFlipView, string>(nameof(VBContent));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent
        {
            get => GetValue(VBContentProperty);
            set => SetValue(VBContentProperty, value);
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

        #region IVBSource Members

        /// <summary>
        /// Gets or sets the VBSource.
        /// </summary>
        [Category("VBControl")]
        public string VBSource
        {
            get
            {
                return _DataSource;
            }
            set
            {
                _DataSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the VBShowColumns.
        /// </summary>
        [Category("VBControl")]
        public string VBShowColumns
        {
            get
            {
                return _DataShowColumns;
            }
            set
            {
                _DataShowColumns = value;
            }
        }

        /// <summary>
        /// Gets or sets the VBDisabledColumns.
        /// </summary>
        [Category("VBControl")]
        public string VBDisabledColumns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the VBChilds.
        /// </summary>
        public string VBChilds
        {
            get
            {
                return _DataChilds;
            }
            set
            {
                _DataChilds = value;
            }
        }

        ACQueryDefinition _ACQueryDefinition = null;
        /// <summary>
        /// Gets the ACQueryDefinition.
        /// </summary>
        public ACQueryDefinition ACQueryDefinition
        {
            get
            {
                if (ACAccess != null)
                    return ACAccess.NavACQueryDefinition;
                return _ACQueryDefinition;
            }
        }

        IAccess _ACAccess = null;
        /// <summary>
        /// Gets ACAccess.
        /// </summary>
        public IAccess ACAccess
        {
            get
            {
                return _ACAccess;
            }
        }
        #endregion

        #region IDataContent Members
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the Control.DataContext-Property.
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
        /// Represents the styled property for BSOACComponent.
        /// </summary>
        public static readonly StyledProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBFlipView>();
        
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get => GetValue(BSOACComponentProperty);
            set => SetValue(BSOACComponentProperty, value);
        }


        /// <summary>
        /// Represents the styled property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBFlipView, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get => GetValue(ACCompInitStateProperty);
            set => SetValue(ACCompInitStateProperty, value);
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

        private void UpdateACContentList(IACObject acObject)
        {
            _ACContentList.Clear();
            // TODO Norbert:
            if (acObject != null)
            {
                _ACContentList.Add(acObject);

                int pos1 = VBContent.LastIndexOf('\\');
                if ((pos1 != -1) && (ContextACObject != null))
                {
                    _ACContentList.Add(ContextACObject.ACUrlCommand(VBContent.Substring(0, pos1)) as IACObject);
                }
            }
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

        /// <summary>
        /// Gets or sets double click.
        /// </summary>
        [Category("VBControl")]
        public string DblClick
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the styled property for VBValidation.
        /// </summary>
        public static readonly StyledProperty<string> VBValidationProperty = 
            ContentPropertyHandler.VBValidationProperty.AddOwner<VBFlipView>();
        
        /// <summary>
        /// Name of the VBValidation property.
        /// </summary>
        /// <summary xml:lang="de">
        /// Name der Eigenschaft VBValidation.
        /// </summary>
        [Category("VBControl")]
        public string VBValidation
        {
            get => GetValue(VBValidationProperty);
            set => SetValue(VBValidationProperty, value);
        }

        /// <summary>
        /// Represents the styled property for DisableContextMenu.
        /// </summary>
        public static readonly StyledProperty<bool> DisableContextMenuProperty = 
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBFlipView>();
        
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
            get => GetValue(DisableContextMenuProperty);
            set => SetValue(DisableContextMenuProperty, value);
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

            if (controlMode == Global.ControlModes.Collapsed || controlMode == Global.ControlModes.Hidden)
            {
                if (this.IsVisible)
                    this.IsVisible = false;
            }
            else
            {
                if (!this.IsVisible)
                    IsVisible = true;
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
        /// Enables or disables auto focus.
        /// </summary>
        /// <summary xml:lang="de">
        /// Aktiviert oder deaktiviert den Autofokus.
        /// </summary>
        [Category("VBControl")]
        public bool AutoFocus { get; set; }

        [Category("VBControl")]
        public DragMode DragEnabled { get; set; }

        IACType _ACTypeInfo = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _ACTypeInfo as ACClassProperty;
            }
        }

        /// <summary>
        /// Represents the styled property for DisabledModes.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty =
            AvaloniaProperty.Register<VBFlipView, string>(nameof(DisabledModes));
        
        /// <summary>
        /// Gets or sets the DisabledModes.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die deaktivierten Modi.
        /// </summary>
        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string DisabledModes
        {
            get => GetValue(DisabledModesProperty);
            set => SetValue(DisabledModesProperty, value);
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

        #region VBFlipView-Methods
        #region Private methods
        private void ScrollViewerPart_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetItemSize();

            if (ScrollViewerPart != null)
            {
                var enableTransition = ScrollViewerPart.EnableTransition;
                ScrollViewerPart.EnableTransition = false;
                this.ScrollIntoView(SelectedIndex);
                ScrollViewerPart.EnableTransition = enableTransition;
            }
        }

        private void SetItemSize()
        {
            var width = GetDesiredItemWidth();
            var height = GetDesiredItemHeight();

            var item = ContainerFromIndex(SelectedIndex);
            if (item is FlipViewItem flipViewItem)
            {
                flipViewItem.Width = width;
                flipViewItem.Height = height;
            }
        }

        private void ScrollEndedEventHandler(object sender, ScrollGestureEndedEventArgs e)
        {
            UpdateSelectedIndex();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var arrange = base.ArrangeOverride(finalSize);

            if (!_arranged)
            {
                var width = GetDesiredItemWidth();
                var height = GetDesiredItemHeight();

                for (var i = 0; i < ItemCount; i++)
                {
                    var item = ContainerFromIndex(i);
                    if (item is FlipViewItem flipViewItem)
                    {
                        flipViewItem.Width = width;
                        flipViewItem.Height = height;
                    }
                }
            }

            _arranged = true;

            return arrange;
        }

        private void UpdateSelectedIndex()
        {
            if (ItemsPresenterPart != null && ScrollViewerPart != null && ItemCount > 0)
            {
                var offset = _isHorizontal ? ScrollViewerPart.Offset.X : ScrollViewerPart.Offset.Y;
                var viewport = _isHorizontal ? ScrollViewerPart.Viewport.Width : ScrollViewerPart.Viewport.Height;
                var viewPortIndex = (long)(offset / viewport);
                var lowerBounds = viewPortIndex * viewport;
                var midPoint = lowerBounds + (viewport * 0.5);

                var index = offset > midPoint ? viewPortIndex + 1 : viewPortIndex;

                SetScrollViewerOffset((int)Math.Max(0, Math.Min(index, ItemCount)));
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            MovePrevious();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            MoveNext();
        }

        protected void FlipPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            if (e.Delta.Y < 0)
            {
                MoveNext();
            }
            else
            {
                MovePrevious();
            }
        }

        private void MoveNext()
        {
            if (ItemCount > 0)
            {
                SetScrollViewerOffset(Math.Min(ItemCount - 1, SelectedIndex + 1));
            }
        }

        private void MovePrevious()
        {
            if (ItemCount > 0)
            {
                SetScrollViewerOffset(Math.Max(0, SelectedIndex - 1));
            }
        }

        private void MoveStart()
        {
            if (ItemCount > 0)
            {
                SetScrollViewerOffset(0);
            }
        }

        private void MoveEnd()
        {
            if (ItemCount > 0)
            {
                SetScrollViewerOffset(Math.Max(0, ItemCount - 1));
            }
        }

        private void FlipKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Left:
                case Key.PageUp:
                    MovePrevious();
                    break;

                case Key.Down:
                case Key.Right:
                case Key.PageDown:
                    MoveNext();
                    break;
                case Key.Home:
                    MoveStart();
                    break;
                case Key.End:
                    MoveEnd();
                    break;
            }
        }

        internal double GetDesiredItemWidth()
        {
            double width = 0;
            if (ItemsPresenterPart is { } presenter)
            {
                if (presenter.Panel is VirtualizingPanel virtualizingPanel)
                {
                    width = virtualizingPanel.Bounds.Width;
                }

                if (width == 0)
                {
                    width = ScrollViewerPart != null ? ScrollViewerPart.Bounds.Width : Bounds.Width;
                }
            }

            if (width == 0)
            {
                width = Width;
            }

            return width;
        }

        internal double GetDesiredItemHeight()
        {
            double height = 0;
            if (ItemsPresenterPart is { } presenter)
            {
                if (presenter.Panel is VirtualizingPanel virtualizingPanel)
                {
                    height = virtualizingPanel.Bounds.Height;
                }

                if (height == 0)
                {
                    height = ScrollViewerPart != null ? ScrollViewerPart.Bounds.Height : Bounds.Height;
                }
            }

            if (height == 0)
            {
                height = Height;
            }

            return height;
        }

        private void SetButtonsVisibility()
        {
            if (!_isApplied)
            {
                return;
            }

            var panel = ItemsPanel.Build();

            if (panel is StackPanel stackPanel)
            {
                switch (stackPanel.Orientation)
                {
                    case Orientation.Horizontal:
                        _nextButtonHorizontal!.IsVisible = true && IsButtonsVisible;
                        _previousButtonHorizontal!.IsVisible = true && IsButtonsVisible;
                        _nextButtonVertical!.IsVisible = false;
                        _previousButtonVertical!.IsVisible = false;
                        _isHorizontal = true;
                        break;
                    case Orientation.Vertical:
                        _nextButtonVertical!.IsVisible = true && IsButtonsVisible;
                        _previousButtonVertical!.IsVisible = true && IsButtonsVisible;
                        _nextButtonHorizontal!.IsVisible = false;
                        _previousButtonHorizontal!.IsVisible = false;
                        _isHorizontal = false;
                        break;
                }
            }

            if (panel is VirtualizingStackPanel virtualizingStackPanel)
            {
                switch (virtualizingStackPanel.Orientation)
                {
                    case Orientation.Horizontal:
                        _nextButtonHorizontal!.IsVisible = true && IsButtonsVisible;
                        _previousButtonHorizontal!.IsVisible = true && IsButtonsVisible;
                        _nextButtonVertical!.IsVisible = false;
                        _previousButtonVertical!.IsVisible = false;
                        _isHorizontal = true;
                        break;
                    case Orientation.Vertical:
                        _nextButtonVertical!.IsVisible = true && IsButtonsVisible;
                        _previousButtonVertical!.IsVisible = true && IsButtonsVisible;
                        _nextButtonHorizontal!.IsVisible = false;
                        _previousButtonHorizontal!.IsVisible = false;
                        _isHorizontal = false;
                        break;
                }
            }
        }

        protected Vector IndexToOffset(int index)
        {
            var container = ContainerFromIndex(index);
            var panel = ItemsPanelRoot;
            var scrollViewer = ScrollViewerPart;
            if (container == null || panel == null || scrollViewer == null)
                return default;

            var bounds = container.Bounds;
            var offset = scrollViewer.Offset;

            if (bounds.Bottom > offset.Y + scrollViewer.Viewport.Height)
            {
                offset = offset.WithY((bounds.Bottom - scrollViewer.Viewport.Height) + panel.Margin.Top);
            }

            if (bounds.Y < offset.Y)
            {
                offset = offset.WithY(bounds.Y);
            }

            if (bounds.Right > offset.X + scrollViewer.Viewport.Width)
            {
                offset = offset.WithX((bounds.Right - scrollViewer.Viewport.Width) + panel.Margin.Left);
            }

            if (bounds.X < offset.X)
            {
                offset = offset.WithX(bounds.X);
            }

            return offset;
        }

        private void SetScrollViewerOffset(int index)
        {
            var offset = IndexToOffset(index);
            SetCurrentValue(SelectedIndexProperty, index);

            if (ScrollViewerPart is { } scrollViewer)
            {
                scrollViewer.SetCurrentValue(FlipViewScrollViewer.OffsetProperty, offset);
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == ACCaptionProperty)
            {
                if (!_Initialized || ContextACObject == null)
                    return;

                ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);

            }
            else if (change.Property == ACCompInitStateProperty)
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
            else if (change.Property == SelectedIndexProperty)
            {
                SetScrollViewerOffset(change.GetNewValue<int>());
            }
            else if (change.Property == ItemsPanelProperty || change.Property == IsButtonsVisibleProperty)
            {
                SetButtonsVisibility();
            }
            base.OnPropertyChanged(change);
        }

        private void OnPreviousCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SelectedIndex > 0;
        }

        private void OnPreviousExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.SelectedIndex -= 1;
        }

        private void OnNextCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.SelectedIndex < (this.Items.Count - 1);
        }

        private void OnNextExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.SelectedIndex += 1;
        }
        #endregion

        #region Commands

        /// <summary>
        /// The next command.
        /// </summary>
        public static RoutedCommand NextCommand = new RoutedCommand("Next");
        /// <summary>
        /// The previous command.
        /// </summary>
        public static RoutedCommand PreviousCommand = new RoutedCommand("Previous");

        #endregion

        #endregion
    }
}
