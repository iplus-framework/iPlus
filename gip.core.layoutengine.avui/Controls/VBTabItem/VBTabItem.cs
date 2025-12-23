using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using System.ComponentModel;
using System.Transactions;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Threading;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents an item in <see cref="VBTabControl"/>.
    /// </summary>
    /// <summary>
    /// Stellt einen Eintrag in <siehe cref="VBTabControl"/> dar.
    /// </summary>
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_TabItemBorder", Type = typeof(Border))]
    [TemplatePart(Name = "PART_RibbonSwitchButton", Type = typeof(Button))]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBTabItem'}de{'VBTabItem'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBTabItem : TabItem, IVBContent, IVBSource, IACObject
    {
        static VBTabItem()
        {
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner<VBTabItem>();
        }

        protected Control _parentObject = null;

        public VBTabItem()
            : base()
        {
            ShowRibbonBar = false;
            IsDragable = false;
            LayoutOrientation = Global.eLayoutOrientation.HorizontalBottom;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.GetObservable(IsVisibleProperty).Subscribe(_ => VBTabItem_IsVisibleChanged());
        }

        VBTabControl vbTabControl = null;

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
            }

            if (vbTabControl != null)
            {
                vbTabControl.Items.CollectionChanged -= Items_CurrentChanged;
                TabItemCount = vbTabControl.Items.Count;
            }
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            
            var partObj = e.NameScope.Find("PART_CloseButton");
            if (partObj is Button closeButton)
            {
                _PART_CloseButton = closeButton;
                _PART_CloseButton.Click += closeButton_Click;
            }

            partObj = e.NameScope.Find("PART_TabItemBorder");
            if (partObj is Border border)
            {
                _PART_TabItemBorder = border;
            }

            partObj = e.NameScope.Find("PART_RibbonSwitchButton");
            if (partObj is Button ribbonButton)
            {
                _PART_RibbonSwitchButton = ribbonButton;
                _PART_RibbonSwitchButton.Click += ribbonSwitchButton_Click;
            }

            // In Avalonia, we can't use DependencyPropertyHelper.GetValueSource
            // Instead, we check if MinHeight has been set locally and apply touch mode if needed
            if (ControlManager.TouchScreenMode && !this.IsSet(MinHeightProperty))
            {
                MinHeight = 35;
            }

            InitVBControl();
        }

        protected bool _LoadedBase = false;
        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (!_LoadedBase)
            {
                if (!string.IsNullOrEmpty(TabIsActivated))
                {
                    var binding = new Binding
                    {
                        ElementName = TabIsActivated,
                        Path = "Visibility"
                    };
                    this.Bind(IsVisibleProperty, binding);
                }
                else if (!String.IsNullOrEmpty(TabVisibilityACUrl))
                {
                    if (ContextACObject != null)
                    {
                        IACType dcACTypeInfo = null;
                        object dcSource = null;
                        string dcPath = "";
                        Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

                        if (ContextACObject.ACUrlBinding(TabVisibilityACUrl, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                        {
                            var binding = new Binding
                            {
                                Source = dcSource,
                                Path = dcPath,
                                Mode = BindingMode.OneWay,
                                Converter = ConverterVisibilityBool.Current
                            };
                            this.Bind(IsVisibleProperty, binding);
                        }
                    }
                }

                if (BSOACComponent != null)
                {
                    var binding = new Binding
                    {
                        Source = BSOACComponent,
                        Path = Const.InitState,
                        Mode = BindingMode.OneWay
                    };
                    this.Bind(ACCompInitStateProperty, binding);
                }
            }
            _LoadedBase = true;
            CheckIsSelected();
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
            if (!_LoadedBase)
                return;
            _LoadedBase = false;
            
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
                _dispTimer.Tick -= dispatcherTimer_CanExecute;
                _dispTimer = null;
            }

            if (vbTabControl != null)
            {
                vbTabControl.Items.CollectionChanged -= Items_CurrentChanged;
                TabItemCount = vbTabControl.Items.Count;
            }

            this.ClearAllBindings();

            _PART_TabItemBorder = null;
            _PART_CloseButton = null;
            _PART_RibbonSwitchButton = null;
            _ACComponent = null;
        }

        private void CheckIsSelected()
        {
            List<Tuple<string, string>> resources = FindListWithLastSelectedTabs();
            if (resources != null)
            {
                if (this.ACCaption != null && resources.Any(c => c.Item1 == "ACCaption" && c.Item2 == this.ACCaption))
                {
                    this.IsSelected = true;
                    if (this.Content is Control contentControl)
                        contentControl.ApplyTemplate();
                    VBTabControl childTabControl = VBLogicalTreeHelper.FindChildObjectInLogicalTree(this, typeof(VBTabControl)) as VBTabControl;
                    if (childTabControl != null)
                        childTabControl.IsActiveLastSelectedTab = true;
                }
                else if (this.Header != null && resources.Any(c => c.Item1 == "Header" && c.Item2 == this.Header.ToString()))
                {
                    this.IsSelected = true;
                    if (this.Content is Control contentControl)
                        contentControl.ApplyTemplate();
                    VBTabControl childTabControl = VBLogicalTreeHelper.FindChildObjectInLogicalTree(this, typeof(VBTabControl)) as VBTabControl;
                    if (childTabControl != null)
                        childTabControl.IsActiveLastSelectedTab = true;
                }
            }
        }

        private List<Tuple<string,string>> FindListWithLastSelectedTabs()
        {
            ContentControl contentControl = VBLogicalTreeHelper.FindParentObjectInLogicalTree(this, typeof(VBDynamic)) as ContentControl;
            if(contentControl != null && contentControl.Content is Control control && control.Resources.ContainsKey("LastSelectedTabsList"))
                return control.Resources["LastSelectedTabsList"] as List<Tuple<string, string>>;

            else
            {
                while (contentControl != null)
                {
                    contentControl = VBLogicalTreeHelper.FindParentObjectInLogicalTree(contentControl.Parent, typeof(VBDynamic)) as ContentControl;
                    if (contentControl != null && contentControl.Content is Control parentControl && parentControl.Resources.ContainsKey("LastSelectedTabsList"))
                        return parentControl.Resources["LastSelectedTabsList"] as List<Tuple<string, string>>;
                }
            }
            return null;
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

        public static readonly StyledProperty<bool> IsDragableProperty
            = AvaloniaProperty.Register<VBTabItem, bool>(nameof(IsDragable));

        [Category("VBControl")]
        public bool IsDragable
        {
            get { return GetValue(IsDragableProperty); }
            set { SetValue(IsDragableProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for CanExecuteCyclic.
        /// </summary>
        public static readonly StyledProperty<int> CanExecuteCyclicProperty = 
            ContentPropertyHandler.CanExecuteCyclicProperty.AddOwner<VBTabItem>();

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
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBTabItem>();

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
        /// Represents the dependency property for StringFormat.
        /// </summary>
        public static readonly StyledProperty<string> StringFormatProperty =
            ContentPropertyHandler.StringFormatProperty.AddOwner<VBTabItem>();

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

        bool IsCreated = false;
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            if (_dispTimer != null)
            {
                if (!_dispTimer.IsEnabled)
                    _dispTimer.Start();
            }

            if (!IsCreated)
            {
                IsCreated = true;
                RightControlMode = Global.ControlModes.Enabled;

                if (!string.IsNullOrEmpty(ACCaption) && ContextACObject != null && !TranslationOff)
                {
                    IACObject translationContext = ContextACObject;
                    if (TranslateFromParentDesign)
                    {
                        VBDesign vbDesign = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBDesign)) as VBDesign;
                        if (vbDesign != null)
                        {
                            ACClassDesign acClassDesign = vbDesign.ContentACObject as ACClassDesign;
                            if (acClassDesign != null)
                                translationContext = acClassDesign.ACClass;
                        }
                    }

                    Header = this.Root().Environment.TranslateText(translationContext, ACCaption);
                }
                else if (!string.IsNullOrEmpty(ACCaption))
                {
                    if (ACCaption.Contains("{'") && ACCaption.Contains("'}"))
                        Header = Translator.GetTranslation("VBTabItem", ACCaption);
                    else
                        Header = ACCaption;
                }

                if (ContextACObject == null)
                    return;

                if ((_dispTimer == null) && this.IsSet(CanExecuteCyclicProperty))
                {
                    _dispTimer = new DispatcherTimer();
                    _dispTimer.Tick += new EventHandler(dispatcherTimer_CanExecute);
                    _dispTimer.Interval = new TimeSpan(0, 0, 0, 0, CanExecuteCyclic < 500 ? 500 : CanExecuteCyclic);
                    _dispTimer.Start();
                }
            }

            vbTabControl = Parent as VBTabControl;
            if (vbTabControl != null)
            {
                vbTabControl.Items.CollectionChanged += Items_CurrentChanged;
                TabItemCount = vbTabControl.Items.Count;
            }
            IACComponent acComponent = ContextACObject as IACComponent;
            if (acComponent != null)
                acComponent.ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.TabItemLoaded));
        }

        void VBTabItem_IsVisibleChanged()
        {
            if (!string.IsNullOrEmpty(this.TabIsActivated) && this.IsSelected && !this.IsVisible && Parent is VBTabControl)
            {
                VBTabControl tabControl = Parent as VBTabControl;
                int index = tabControl.Items.IndexOf(this);
                if (index - 1 > 0)
                    tabControl.SelectedIndex = index - 1;
                else
                    tabControl.SelectedIndex = 0;
            }
            else if (!string.IsNullOrEmpty(TabVisibilityACUrl) && Parent is VBTabControl && IsSelected && !IsVisible)
            {
                VBTabControl tabControl = Parent as VBTabControl;
                int currentIndex = tabControl.Items.IndexOf(this);
                
                if(currentIndex > 0)
                {
                    VBTabItem prevItem = tabControl.Items[currentIndex - 1] as VBTabItem;
                    if (prevItem != null && prevItem.IsVisible)
                    {
                        tabControl.SelectedItem = prevItem;
                        return;
                    }
                }

                if(currentIndex + 1 < tabControl.Items.Count)
                {
                    VBTabItem nextItem = tabControl.Items[currentIndex + 1] as VBTabItem;
                    if (nextItem != null && nextItem.IsVisible)
                        tabControl.SelectedItem = nextItem;
                }
            }
        }

        void Items_CurrentChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender is ItemCollection itemCollection)
            {
                TabItemCount = itemCollection.Count;
            }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == IsSelectedProperty)
            {
                OnSelected(change);
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
        }

        protected virtual void OnSelected(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.NewValue is bool && (bool)change.NewValue == true)
            {
                try
                {
                    if (!string.IsNullOrEmpty(VBContent))
                    {
                        if (ContextACObject != null)
                            (ContextACObject as IACComponent).ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.TabItemActivated));
                    }
                    this.Focus();
                    Control parentControl = Parent as Control;
                    if (parentControl != null)
                        parentControl.Focus();
                }
                catch (Exception)
                {
                }
            }
        }

        public static readonly StyledProperty<bool> WithVisibleCloseButtonProperty
            = AvaloniaProperty.Register<VBTabItem, bool>(nameof(WithVisibleCloseButton));

        public bool WithVisibleCloseButton
        {
            get { return GetValue(WithVisibleCloseButtonProperty); }
            set { SetValue(WithVisibleCloseButtonProperty, value); }
        }

        public static readonly StyledProperty<bool> ShowRibbonBarProperty
            = AvaloniaProperty.Register<VBTabItem, bool>(nameof(ShowRibbonBar));

        [Category("VBControl")]
        public bool ShowRibbonBar
        {
            get { return GetValue(ShowRibbonBarProperty); }
            set { SetValue(ShowRibbonBarProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ShowCaption.
        /// </summary>
        public static readonly StyledProperty<bool> ShowCaptionProperty
        = AvaloniaProperty.Register<VBTabItem, bool>(nameof(ShowCaption), false);

        /// <summary>
        /// Determines is control caption shown or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Steuertitel angezeigt werden oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool ShowCaption
        {
            get { return GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        public static readonly StyledProperty<string> TabIsActivatedProperty
            = AvaloniaProperty.Register<VBTabItem, string>(nameof(TabIsActivated));

        /// <summary>
        /// This property is defined for VBTabItem dynamic visibility which depends on child element visibility.
        /// First we set name on child element and then set in this property child element name.
        /// </summary>
        [Category("VBControl")]
        public string TabIsActivated
        {
            get { return GetValue(TabIsActivatedProperty); }
            set { SetValue(TabIsActivatedProperty, value); }
        }

        public static readonly StyledProperty<string> TabVisibilityACUrlProperty
            = AvaloniaProperty.Register<VBTabItem, string>(nameof(TabVisibilityACUrl));

        /// <summary>
        /// This property is defined for VBTabItem dynamic visibility which depends on child element visibility.
        /// First we set name on child element and then set in this property child element name.
        /// </summary>
        public string TabVisibilityACUrl
        {
            get { return GetValue(TabVisibilityACUrlProperty); }
            set { SetValue(TabVisibilityACUrlProperty, value); }
        }

        #region DockingManager members

        #endregion

        #region PART's
        private Border _PART_TabItemBorder;
        public Border PART_TabItemBorder
        {
            get
            {
                return _PART_TabItemBorder;
            }
        }

        private Button _PART_CloseButton;
        public Button PART_CloseButton
        {
            get
            {
                return _PART_CloseButton;
            }
        }

        private Button _PART_RibbonSwitchButton;
        public Button PART_RibbonSwitchButton
        {
            get
            {
                return _PART_RibbonSwitchButton;
            }
        }

        internal bool _TabItemBorderEventsSubscr = false;

        #endregion

        #region IBSOContext Members

        IACObject _ACComponent = null;
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the Control.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                if (_ACComponent == null)
                {
                    _ACComponent = DataContext as IACObject;
                    if (_ACComponent != null)
                    {
                        ACUrl = "Window" + _ACComponent.ACIdentifier;
                    }
                }
                return _ACComponent;
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = 
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBTabItem>();

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
            AvaloniaProperty.Register<VBTabItem, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
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

        #region Konfiguration
        List<ACClassDesignInfo> _FixACClassDesignList = null;
        public List<ACClassDesignInfo> FixACClassDesignList
        {
            get
            {
                return _FixACClassDesignList;
            }
            set
            {
                _FixACClassDesignList = value;
            }
        }

        List<ACClassDesignInfo> _PinACClassDesignList = null;
        public List<ACClassDesignInfo> PinACClassDesignList
        {
            get
            {
                return _PinACClassDesignList;
            }
            set
            {
                _PinACClassDesignList = value;
            }
        }

        string _caption;
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                return _caption;
            }
            set
            {
                _caption = value;
            }
        }

        [Category("VBControl")]
        public bool TranslationOff
        {
            get;
            set;
        }

        [Category("VBControl")]
        public bool TranslateFromParentDesign
        {
            get;
            set;
        }

        [Category("VBControl")]
        public Global.eLayoutOrientation LayoutOrientation
        {
            get;
            set;
        }

        // StyleInfoList property for theme handling
        public List<ACClassDesignInfo> StyleInfoList
        {
            get;
            set;
        }

        #endregion

        #endregion

        public static readonly RoutedEvent<RoutedEventArgs> CloseTabEvent =
            RoutedEvent.Register<VBTabItem, RoutedEventArgs>(nameof(CloseTab), RoutingStrategies.Bubble);

        public event EventHandler<RoutedEventArgs> CloseTab
        {
            add { AddHandler(CloseTabEvent, value); }
            remove { RemoveHandler(CloseTabEvent, value); }
        }

        public void closeButton_Click(object sender, RoutedEventArgs e)
        {
            var args = new RoutedEventArgs(CloseTabEvent, this);
            RaiseEvent(args);
            
            VBTabControl tabControl = this.vbTabControl;
            if (tabControl == null)
                tabControl = this.Parent as VBTabControl;

            // TODO:
            //VBDockingPanelTabbedDoc vbDockingPanelTabbedDoc = tabControl?.Parent as VBDockingPanelTabbedDoc;
            //VBDockingContainerBase vbDockingContainerBase = null;

            //if (this.Content is ContentPresenter presenter && vbDockingPanelTabbedDoc != null)
            //    vbDockingContainerBase = vbDockingPanelTabbedDoc.Documents.FirstOrDefault(c => c.Content == presenter.Content);

            //if (vbDockingContainerBase != null)
            //{
            //    vbDockingPanelTabbedDoc.RemoveDockingContainerToolWindow(vbDockingContainerBase);
            //    vbDockingContainerBase.OnCloseWindow();
            //}
        }

        public void ribbonSwitchButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private object DataContentObject
        {
            get;
            set;
        }

        #region IDataCommand Members

        string _InternalName;
        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get
            {
                return _InternalName;
            }
            set
            {
                _InternalName = value;
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

        string _Parameter;
        [Category("VBControl")]
        public string Parameter
        {
            get
            {
                return _Parameter;
            }
            set
            {
                _Parameter = value;
            }
        }
        #endregion

        #region IACObject Member

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
        public object ACUrlCommand(string acUrl, params Object[] acParameter)
        {
            switch (acUrl)
            {
                case Const.EventDeInit:
                    if (ContextACObject is IACComponent && (ContextACObject as IACComponent).ReferencePoint != null)
                        (ContextACObject as IACComponent).ReferencePoint.Remove(this);
                    break;
            }
            return null;
        }

        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            switch (acUrl)
            {
                case Const.EventDeInit:
                    return true;
            }
            return false;
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

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return _ACUrl;
        }

        string _ACUrl = "";
        public string ACUrl
        {
            get
            {
                return _ACUrl;
            }
            protected set
            {
                _ACUrl = value;
            }
        }

        [ACMethodInfo("", "", 9999)]
        public string GetACUrlComponent(IACObject rootACObject = null)
        {
            return "";
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
        #endregion

        #region IDataContent Member
        public static readonly StyledProperty<int> TabItemCountProperty
            = AvaloniaProperty.Register<VBTabItem, int>(nameof(TabItemCount));

        [Bindable(true)]
        public int TabItemCount
        {
            get { return GetValue(TabItemCountProperty); }
            set { SetValue(TabItemCountProperty, value); }
        }

        private bool Visible
        {
            get;
            set;
        }

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

        /// <summary>
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
        }
        #endregion

        #region IVBSource Member

        string _DataSource;
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

        string _DataShowColumns;
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

        public string VBDisabledColumns
        {
            get;
            set;
        }

        string _DataChilds;
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

        public ACQueryDefinition ACQueryDefinition
        {
            get;
            set;
        }
        #endregion

        #region CanExecute Dispatcher
        private DispatcherTimer _dispTimer = null;
        private void dispatcherTimer_CanExecute(object sender, EventArgs e)
        {
            // In Avalonia, we don't have CommandManager.InvalidateRequerySuggested()
            // Commands are automatically reevaluated when needed
            // We can trigger a manual reevaluation if needed by updating command state
        }
        #endregion

        #region Event overrides

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (DisableContextMenu)
            {
                e.Handled = true;
                return;
            }

            if (WithVisibleCloseButton && e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed && e.Source == PART_TabItemBorder)
                closeButton_Click(this, new RoutedEventArgs());

            if (ContextACObject != null && e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                var point = e.GetPosition(this);
                ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, point.X, point.Y, Global.ElementActionType.ContextMenu);
                BSOACComponent.ACAction(actionArgs);
                if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                {
                    VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
                    this.ContextMenu = vbContextMenu;
                    if (vbContextMenu.PlacementTarget == null)
                        vbContextMenu.PlacementTarget = this;
                    ContextMenu.Open(this);
                }
                e.Handled = true;
            }

            base.OnPointerPressed(e);
        }

        #endregion

        #region Behavior

        #region Behavior -> FocusNames

        public static readonly StyledProperty<string> FocusNamesProperty =
            AvaloniaProperty.Register<VBTabItem, string>(nameof(FocusNames));

        [Category("VBControl")]
        public string FocusNames
        {
            get { return GetValue(FocusNamesProperty); }
            set { SetValue(FocusNamesProperty, value); }
        }

        #endregion

        #endregion
    }
}
