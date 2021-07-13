using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;
using System.ComponentModel;
using System.Transactions;
using System.Windows.Threading;
using System.Windows.Data;

namespace gip.core.layoutengine
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
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "TabItemStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTabItem/Themes/TabItemStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "TabItemStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBTabItem/Themes/TabItemStyleAero.xaml" },
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

        static VBTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBTabItem), new FrameworkPropertyMetadata(typeof(VBTabItem)));
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner(typeof(VBTabItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        }

        //public VBTabItem(IACObjectWithBinding acObject)
        //{
        //    ShowRibbonBar = true;
        //    WithVisibleCloseButton = true;

        //    DataContext = acObject;

        //    PinACClassDesignList = acObject.GetACLayouts(Global.ACLayoutType.Pinview);
        //    FixACClassDesignList = acObject.GetACLayouts(Global.ACLayoutType.View);

        //    InitVBTabItem(null);
        //}

        protected FrameworkElement _parentObject = null;
        bool _themeApplied = false;
        public VBTabItem()
            : base()
        {
            ShowRibbonBar = false;
            IsDragable = false;
            LayoutOrientation = Global.eLayoutOrientation.HorizontalBottom;
            this.Loaded += new RoutedEventHandler(VBTabItem_Loaded);
            this.Unloaded += new RoutedEventHandler(VBTabItem_Unloaded);
            this.IsVisibleChanged += VBTabItem_IsVisibleChanged;
        }

        VBTabControl vbTabControl = null;

        void VBTabItem_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
            }

            if (vbTabControl != null)
            {
                vbTabControl.Items.CurrentChanged -= Items_CurrentChanged;
                TabItemCount = vbTabControl.Items.Count;
            }
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
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
            object partObj = (object)GetTemplateChild("PART_CloseButton");
            if ((partObj != null) && (partObj is Button))
            {
                _PART_CloseButton = ((Button)partObj);
                _PART_CloseButton.Click += closeButton_Click;
            }

            partObj = (object)GetTemplateChild("PART_TabItemBorder");
            if ((partObj != null) && (partObj is Border))
            {
                _PART_TabItemBorder = ((Border)partObj);
            }

            partObj = (object)GetTemplateChild("PART_RibbonSwitchButton");
            if ((partObj != null) && (partObj is Button))
            {
                _PART_RibbonSwitchButton = ((Button)partObj);
                _PART_RibbonSwitchButton.Click += ribbonSwitchButton_Click;
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
                Binding binding = null;
                if (!string.IsNullOrEmpty(TabIsActivated))
                {
                    binding = new Binding();
                    binding.ElementName = TabIsActivated;
                    binding.Path = new PropertyPath("Visibility");
                    SetBinding(VisibilityProperty, binding);
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
                            binding = new Binding();
                            binding.Source = dcSource;
                            binding.Path = new PropertyPath(dcPath);
                            binding.Mode = BindingMode.OneWay;
                            binding.Converter = ConverterVisibilityBool.Current;
                            SetBinding(VisibilityProperty, binding);
                        }
                    }
                }

                if (BSOACComponent != null)
                {
                    binding = new Binding();
                    binding.Source = BSOACComponent;
                    binding.Path = new PropertyPath(Const.InitState);
                    binding.Mode = BindingMode.OneWay;
                    SetBinding(VBTabItem.ACCompInitStateProperty, binding);
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
            this.Loaded -= new RoutedEventHandler(VBTabItem_Loaded);
            this.Unloaded -= new RoutedEventHandler(VBTabItem_Unloaded);
            this.IsVisibleChanged -= VBTabItem_IsVisibleChanged;
            if (_dispTimer != null)
            {
                if (_dispTimer.IsEnabled)
                    _dispTimer.Stop();
                _dispTimer.Tick -= dispatcherTimer_CanExecute;
                _dispTimer = null;
            }

            if (vbTabControl != null)
            {
                vbTabControl.Items.CurrentChanged -= Items_CurrentChanged;
                TabItemCount = vbTabControl.Items.Count;
            }
            //BindingOperations.ClearBinding(this, VBTabItem.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBTabItem.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);

            _PART_TabItemBorder = null;
            _PART_CloseButton = null;
            _PART_RibbonSwitchButton = null;
            _ACComponent = null;
        }

        private void CheckIsSelected()
        {
            //if (Parent != null && Parent is VBTabControl && ((VBTabControl)Parent).IsActiveLastSelectedTab)
            //{
                List<Tuple<string, string>> resources = FindListWithLastSelectedTabs();
                if (resources != null)
                {
                    if (this.ACCaption != null && resources.Any(c => c.Item1 == "ACCaption" && c.Item2 == this.ACCaption))
                    {
                        this.IsSelected = true;
                        ((FrameworkElement)this.Content).OnApplyTemplate();
                        VBTabControl childTabControl = VBLogicalTreeHelper.FindChildObjectInLogicalTree(this, typeof(VBTabControl)) as VBTabControl;
                        if (childTabControl != null)
                            childTabControl.IsActiveLastSelectedTab = true;
                    }
                    else if (this.Header != null && resources.Any(c => c.Item1 == "Header" && c.Item2 == this.Header.ToString()))
                    {
                        this.IsSelected = true;
                        ((FrameworkElement)this.Content).OnApplyTemplate();
                        VBTabControl childTabControl = VBLogicalTreeHelper.FindChildObjectInLogicalTree(this, typeof(VBTabControl)) as VBTabControl;
                        if (childTabControl != null)
                            childTabControl.IsActiveLastSelectedTab = true;
                    }
                }
            //}
        }

        private List<Tuple<string,string>> FindListWithLastSelectedTabs()
        {
            ContentControl contentControl = VBLogicalTreeHelper.FindParentObjectInLogicalTree(this, typeof(VBDynamic)) as ContentControl;
            if(contentControl != null && ((FrameworkElement)contentControl.Content).Resources.Contains("LastSelectedTabsList"))
                return ((FrameworkElement)contentControl.Content).Resources["LastSelectedTabsList"] as List<Tuple<string, string>>;

            else
            {
                while (contentControl != null)
                {
                    contentControl = VBLogicalTreeHelper.FindParentObjectInLogicalTree(contentControl.Parent, typeof(VBDynamic)) as ContentControl;
                    if (contentControl != null && ((FrameworkElement)contentControl.Content).Resources.Contains("LastSelectedTabsList"))
                        return ((FrameworkElement)contentControl.Content).Resources["LastSelectedTabsList"] as List<Tuple<string, string>>;
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

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }

        public static readonly DependencyProperty IsDragableProperty
            = DependencyProperty.Register("IsDragable", typeof(bool), typeof(VBTabItem));

        /// <summary>
        /// Represents the dependency property for CanExecuteCyclic.
        /// </summary>
        public static readonly DependencyProperty CanExecuteCyclicProperty = ContentPropertyHandler.CanExecuteCyclicProperty.AddOwner(typeof(VBTabItem), new FrameworkPropertyMetadata((int)0, FrameworkPropertyMetadataOptions.Inherits));
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
        public static readonly DependencyProperty DisableContextMenuProperty = ContentPropertyHandler.DisableContextMenuProperty.AddOwner(typeof(VBTabItem), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));
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
        /// Represents the dependency property for StringFormat.
        /// </summary>
        public static readonly DependencyProperty StringFormatProperty;
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
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        bool IsCreated = false;
        void VBTabItem_Loaded(object sender, RoutedEventArgs e)
        {
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

                if ((_dispTimer == null) && ReadLocalValue(CanExecuteCyclicProperty) != DependencyProperty.UnsetValue)
                {
                    _dispTimer = new DispatcherTimer();
                    _dispTimer.Tick += new EventHandler(dispatcherTimer_CanExecute);
                    _dispTimer.Interval = new TimeSpan(0, 0, 0, 0, CanExecuteCyclic);
                    _dispTimer.Start();
                }
            }

            //this.IsVisibleChanged += VBTabItem_IsVisibleChanged;

            vbTabControl = Parent as VBTabControl;
            if (vbTabControl != null)
            {
                vbTabControl.Items.CurrentChanged += new EventHandler(Items_CurrentChanged);
                TabItemCount = vbTabControl.Items.Count;
            }
            IACComponent acComponent = ContextACObject as IACComponent;
            if (acComponent != null)
                acComponent.ACAction(new ACActionArgs(sender as IACInteractiveObject, 0, 0, Global.ElementActionType.TabItemLoaded));
        }

        void VBTabItem_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.TabIsActivated) && this.IsSelected && !this.IsVisible && Parent != null && Parent is VBTabControl)
            {
                VBTabControl tabControl = Parent as VBTabControl;
                int index = tabControl.Items.IndexOf(this);
                if (index - 1 > 0)
                    tabControl.SelectedIndex = index - 1;
                else
                    tabControl.SelectedIndex = 0;
            }

            else if (!string.IsNullOrEmpty(TabVisibilityACUrl) && Parent is VBTabControl && IsSelected && Visibility == Visibility.Collapsed)
            {
                VBTabControl tabControl = Parent as VBTabControl;
                int currentIndex = tabControl.Items.IndexOf(this);
                
                if(currentIndex > 0)
                {
                    VBTabItem prevItem = tabControl.Items[currentIndex - 1] as VBTabItem;
                    if (prevItem != null && prevItem.Visibility != Visibility.Collapsed)
                    {
                        tabControl.SelectedItem = prevItem;
                        return;
                    }
                }

                if(currentIndex + 1 < tabControl.Items.Count)
                {
                    VBTabItem nextItem = tabControl.Items[currentIndex + 1] as VBTabItem;
                    if (nextItem != null && nextItem.Visibility != Visibility.Collapsed)
                        tabControl.SelectedItem = nextItem;
                }
            }

        }

        void Items_CurrentChanged(object sender, EventArgs e)
        {
            if (sender is ItemCollection)
            {
                ItemCollection itemCollection = sender as ItemCollection;
                TabItemCount = itemCollection.Count;
            }
        }

        void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VBTabItem vbTabitemAdded = null;
            foreach (var added in e.AddedItems)
            {
                if (added is VBTabItem)
                {
                    vbTabitemAdded = added as VBTabItem;
                    break;
                }
            }

            if (vbTabitemAdded != null && ContextACObject is IACComponent)
            {
                foreach (var removed in e.RemovedItems)
                {
                    if (removed is VBTabItem)
                    {
                        VBTabItem vbTabitemRemoved = removed as VBTabItem;
                        (ContextACObject as IACComponent).ACAction(new ACActionArgs(vbTabitemRemoved, 0, 0, Global.ElementActionType.TabItemDeActivated));
                        break;
                    }
                }
                (ContextACObject as IACComponent).ACAction(new ACActionArgs(vbTabitemAdded, 0, 0, Global.ElementActionType.TabItemActivated));
            }
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);
            if (!string.IsNullOrEmpty(VBContent))
            {
                if (ContextACObject != null)
                    (ContextACObject as IACComponent).ACAction(new ACActionArgs(this, 0, 0, Global.ElementActionType.TabItemActivated));
            }
            this.Focus();
            UIElement parentUIElement = Parent as UIElement;
            if (parentUIElement != null)
                parentUIElement.Focus();
        }

        public static readonly DependencyProperty WithVisibleCloseButtonProperty
            = DependencyProperty.Register("WithVisibleCloseButton", typeof(bool), typeof(VBTabItem));

        public bool WithVisibleCloseButton
        {
            get
            {
                return (bool)GetValue(WithVisibleCloseButtonProperty);
            }
            set
            {
                SetValue(WithVisibleCloseButtonProperty, value);
            }
        }

        public static readonly DependencyProperty ShowRibbonBarProperty
            = DependencyProperty.Register("ShowRibbonBar", typeof(bool), typeof(VBTabItem));
        [Category("VBControl")]
        public bool ShowRibbonBar
        {
            get
            {
                return (bool)GetValue(ShowRibbonBarProperty);
            }

            set
            {
                SetValue(ShowRibbonBarProperty, value);
            }
        }

        /// <summary>
        /// Represents the dependency property for ShowCaption.
        /// </summary>
        public static readonly DependencyProperty ShowCaptionProperty
        = DependencyProperty.Register("ShowCaption", typeof(bool), typeof(VBTabItem), new PropertyMetadata(false));
        /// <summary>
        /// Determines is control caption shown or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Steuertitel angezeigt werden oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool ShowCaption
        {
            get { return (bool)GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        public static readonly DependencyProperty TabIsActivatedProperty
            = DependencyProperty.Register("TabIsActivated", typeof(string), typeof(VBTabItem));

        /// <summary>
        /// This property is defined for VBTabItem dynamic visibility which depends on child element visibility.
        /// First we set name on child element and then set in this property child element name.
        /// </summary>
        [Category("VBControl")]
        public string TabIsActivated
        {
            get { return (string)GetValue(TabIsActivatedProperty); }
            set { SetValue(TabIsActivatedProperty, value); }
        }



        public static readonly DependencyProperty TabVisibilityACUrlProperty
            = DependencyProperty.Register("TabVisibilityACUrl", typeof(string), typeof(VBTabItem));

        /// <summary>
        /// This property is defined for VBTabItem dynamic visibility which depends on child element visibility.
        /// First we set name on child element and then set in this property child element name.
        /// </summary>
        public string TabVisibilityACUrl
        {
            get { return (string)GetValue(TabVisibilityACUrlProperty); }
            set { SetValue(TabVisibilityACUrlProperty, value); }
        }


        #region DockingManager members
        //VBDockingContainerToolBSOExplorer _explorer;
        //public VBDockingContainerToolBSOExplorer Explorer
        //{
        //    get
        //    {
        //        return _explorer;
        //    }
        //}

        //VBDockingContainerToolBSOMsg _bsoMsg;
        //public VBDockingContainerToolBSOMsg BSOMsg
        //{
        //    get
        //    {
        //        return _bsoMsg;
        //    }
        //}

        //VBDockingContainerToolWindowFindReplace _findAndReplaceEditor;
        //public VBDockingContainerToolWindowFindReplace FindAndReplaceEditor
        //{
        //    get
        //    {
        //        return _findAndReplaceEditor;
        //    }
        //    set
        //    {
        //        _findAndReplaceEditor = value;
        //    }
        //}

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
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
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
                        //Caption = _ACComponent.ACCaption;
                        ACUrl = "Window" + _ACComponent.ACIdentifier;
                    }
                }
                return _ACComponent;
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBTabItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
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
        ////        typeof(ACUrlCmdMessage), typeof(VBTabItem),
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
                typeof(ACInitState), typeof(VBTabItem),
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
            VBTabItem thisControl = dependencyObject as VBTabItem;
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

        [Category("VBControl")]
        public bool IsDragable
        {
            get { return (bool)GetValue(IsDragableProperty); }
            set { SetValue(IsDragableProperty, value); }
        }

        #endregion

        #endregion

        public static readonly RoutedEvent CloseTabEvent =
            EventManager.RegisterRoutedEvent("CloseTab", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(VBTabItem));

        public event RoutedEventHandler CloseTab
        {
            add { AddHandler(CloseTabEvent, value); }
            remove { RemoveHandler(CloseTabEvent, value); }
        }

        public void closeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
            VBTabControl tabControl = this.vbTabControl;
            if (tabControl == null)
                tabControl = this.Parent as VBTabControl;

            VBDockingPanelTabbedDoc vbDockingPanelTabbedDoc = tabControl?.Parent as VBDockingPanelTabbedDoc;
            VBDockingContainerBase vbDockingContainerBase = null;

            if (this.Content is ContentPresenter && vbDockingPanelTabbedDoc != null)
                vbDockingContainerBase = vbDockingPanelTabbedDoc.Documents.FirstOrDefault(c => c.Content == ((ContentPresenter)this.Content).Content);

            if (vbDockingContainerBase != null)
            {
                vbDockingPanelTabbedDoc.RemoveDockingContainerToolWindow(vbDockingContainerBase);
                vbDockingContainerBase.OnCloseWindow();
            }
        }

        //VBRibbon _VBRibbon;

        public void ribbonSwitchButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            /*if (_VBRibbon != null)
            {
                if (_VBRibbon.Visibility == System.Windows.Visibility.Collapsed)
                    _VBRibbon.Visibility = System.Windows.Visibility.Visible;
                else
                    _VBRibbon.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (Content == null)
                return;
            DockPanel dockPanel = null;
            if (!(Content is DockPanel))
            {
                if (Content is ContentPresenter)
                {
                    if ((Content as ContentPresenter).Content is DockPanel)
                        dockPanel = (Content as ContentPresenter).Content as DockPanel;
                }
            }
            else
                dockPanel = Content as DockPanel;

            if (dockPanel == null)
                return;
            foreach (UIElement element in dockPanel.Children)
            {
                if (element is VBRibbon)
                {
                    VBRibbon ribbon = element as VBRibbon;
                    if (ribbon.Visibility == System.Windows.Visibility.Collapsed)
                        ribbon.Visibility = System.Windows.Visibility.Visible;
                    else
                        ribbon.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                }
            }*/
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
        public static readonly DependencyProperty TabItemCountProperty
            = DependencyProperty.Register("TabItemCount", typeof(int), typeof(VBTabItem));
        [Bindable(true)]
        public int TabItemCount
        {
            get
            {
                //VBTabControl vbTabControl = Parent as VBTabControl;
                //return vbTabControl.Items.Count;
                return (int)GetValue(TabItemCountProperty);
            }
            set
            {
                SetValue(TabItemCountProperty, value);
            }
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
            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region Event overrides

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if(WithVisibleCloseButton && e.MiddleButton == MouseButtonState.Pressed && e.OriginalSource == PART_TabItemBorder)
                closeButton_Click(this, new RoutedEventArgs());

            base.OnMouseDown(e);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (WithVisibleCloseButton && e.OriginalSource == PART_TabItemBorder)
                closeButton_Click(this, new RoutedEventArgs());
        }

        #endregion

        #region Behavior

        #region Behavior -> FocusNames

        public static readonly DependencyProperty FocusNamesProperty =
            DependencyProperty.Register("FocusNames", typeof(string), typeof(VBTabItem));

        [Category("VBControl")]
        public string FocusNames
        {
            get { return (string)GetValue(FocusNamesProperty); }
            set { SetValue(FocusNamesProperty, value); }
        }

        #endregion

        #endregion

    }

}
