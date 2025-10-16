using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a selectable item in <see cref="VBRadioButtonGroup"/>.
    /// </summary>
    /// <summary xml:lang="de">
    /// Repräsentiert ein auswählbares Element in <see cref="VBRadioButtonGroup"/>
    /// </summary>
    public class VBRadioButtonGroupItem : ListBoxItem
    {
        public VBRadioButtonGroupItem() : base()
        {
        }

        public static readonly StyledProperty<Boolean> PushButtonStyleProperty =
            AvaloniaProperty.Register<VBRadioButtonGroupItem, Boolean>(nameof(PushButtonStyle), false, false);
        
        [Category("VBControl")]
        public Boolean PushButtonStyle
        {
            get { return GetValue(PushButtonStyleProperty); }
            set { SetValue(PushButtonStyleProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == PushButtonStyleProperty)
            {
                UpdatePushButtonClass();
            }
            base.OnPropertyChanged(change);
        }

        private void UpdatePushButtonClass()
        {
            if (Classes == null) 
                return;

            if (PushButtonStyle)
            {
                Classes.Add("pushbutton");
            }
            else
            {
                Classes.Remove("pushbutton");
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            UpdatePushButtonClass();
        }
    }

    /// <summary>
    /// Control for representing a radio button group.
    /// </summary>
    /// <summary>
    /// Steuerelement zur Darstellung einer Schaltergruppe.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBRadioButtonGroup'}de{'VBRadioButtonGroup'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBRadioButtonGroup : ListBox, IVBContent, IVBSource, IACMenuBuilderWPFTree, IACObject
    {
        #region c'tors
        string _DataSource;
        string _DataShowColumns;
        string _DataChilds;

        public VBRadioButtonGroup() : base()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            this.Loaded += VBRadioButtonGroup_Loaded;
            this.Unloaded += VBRadioButtonGroup_Unloaded;
            UpdateTemplateClass();
            UpdateOrientationClass();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }

        #endregion

        #region Additional Dependenc-Properties
        /// <summary>
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, Global.ControlModes>(nameof(ControlMode));

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

        protected override Control CreateContainerForItemOverride(object item, int index, object recycleKey)
        {
            VBRadioButtonGroupItem vbItem = new VBRadioButtonGroupItem();
            vbItem.PushButtonStyle = this.PushButtonStyle;
            
            // Check if ItemsMinWidth is locally set
            if (this.IsSet(VBRadioButtonGroup.ItemsMinWidthProperty))
            {
                vbItem.MinWidth = this.ItemsMinWidth;
            }
            
            // Check if ItemsMaxWidth is locally set
            if (this.IsSet(VBRadioButtonGroup.ItemsMaxWidthProperty))
            {
                vbItem.MaxWidth = this.ItemsMaxWidth;
            }
            
            return vbItem;
        }

        public enum ItemHostType : short
        {
            StackPanel = 0,
            WrapPanel = 1,
            DockPanel = 2,
            Grid = 3,
        }

        public static readonly StyledProperty<ItemHostType> ItemsHostTypeProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, ItemHostType>(nameof(ItemsHostType), ItemHostType.StackPanel, false);
        
        [Category("VBControl")]
        public ItemHostType ItemsHostType
        {
            get { return GetValue(ItemsHostTypeProperty); }
            set { SetValue(ItemsHostTypeProperty, value); }
        }

        public static readonly StyledProperty<bool> ScrollViewerVisibilityProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, bool>(nameof(ScrollViewerVisibility), true, false);
        
        [Category("VBControl")]
        public bool ScrollViewerVisibility
        {
            get { return GetValue(ScrollViewerVisibilityProperty); }
            set { SetValue(ScrollViewerVisibilityProperty, value); }
        }

        private void UpdateTemplateClass()
        {
            if (Classes == null) return;

            // Remove all template classes
            Classes.Remove("stackpanel-noscroll");
            Classes.Remove("stackpanel-scroll");
            Classes.Remove("wrappanel-noscroll");
            Classes.Remove("wrappanel-scroll");
            Classes.Remove("dockpanel-noscroll");
            Classes.Remove("dockpanel-scroll");
            Classes.Remove("grid-noscroll");
            Classes.Remove("grid-scroll");

            // Add appropriate template class
            string templateClass = ItemsHostType.ToString().ToLower() + (ScrollViewerVisibility ? "-scroll" : "-noscroll");
            Classes.Add(templateClass);

            // Update ItemsPanel based on ItemsHostType
            UpdateItemsPanel();
        }

        private void UpdateItemsPanel()
        {
            var itemsPanelTemplate = new FuncTemplate<Panel>(() =>
            {
                Panel panel = ItemsHostType switch
                {
                    ItemHostType.StackPanel => new StackPanel 
                    { 
                        Orientation = HorizontalItems ? Orientation.Horizontal : Orientation.Vertical 
                    },
                    ItemHostType.WrapPanel => new WrapPanel 
                    { 
                        Orientation = HorizontalItems ? Orientation.Horizontal : Orientation.Vertical 
                    },
                    ItemHostType.DockPanel => new DockPanel(),
                    ItemHostType.Grid => new UniformGrid 
                    { 
                        Columns = this.Columns, 
                        Rows = this.Rows 
                    },
                    _ => new StackPanel 
                    { 
                        Orientation = HorizontalItems ? Orientation.Horizontal : Orientation.Vertical 
                    }
                };
                return panel;
            });

            ItemsPanel = itemsPanelTemplate;
        }

        public static readonly StyledProperty<Boolean> PushButtonStyleProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, Boolean>(nameof(PushButtonStyle));
        
        [Category("VBControl")]
        public Boolean PushButtonStyle
        {
            get { return GetValue(PushButtonStyleProperty); }
            set { SetValue(PushButtonStyleProperty, value); }
        }

        public static readonly StyledProperty<Double> ItemsMinWidthProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, Double>(nameof(ItemsMinWidth));
        
        [Category("VBControl")]
        public Double ItemsMinWidth
        {
            get { return GetValue(ItemsMinWidthProperty); }
            set { SetValue(ItemsMinWidthProperty, value); }
        }

        public static readonly StyledProperty<Double> ItemsMaxWidthProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, Double>(nameof(ItemsMaxWidth));
        
        [Category("VBControl")]
        public Double ItemsMaxWidth
        {
            get { return GetValue(ItemsMaxWidthProperty); }
            set { SetValue(ItemsMaxWidthProperty, value); }
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

        public static readonly StyledProperty<int> ColumnsProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, int>(nameof(Columns));
        
        [Category("VBControl")]
        public int Columns
        {
            get { return GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public static readonly StyledProperty<int> RowsProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, int>(nameof(Rows));
        
        [Category("VBControl")]
        public int Rows
        {
            get { return GetValue(RowsProperty); }
            set { SetValue(RowsProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCaption.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, string>(Const.ACCaptionPrefix);

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get { return GetValue(ACCaptionProperty); }
            set { SetValue(ACCaptionProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly StyledProperty<string> ACCaptionTransProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, string>(nameof(ACCaptionTrans));

        /// <summary>
        /// Gets or sets the ACCaption translation.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die ACCaption-Übersetzung.
        /// </summary>
        [Category("VBControl")]
        public string ACCaptionTrans
        {
            get { return GetValue(ACCaptionTransProperty); }
            set { SetValue(ACCaptionTransProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ShowCaption.
        /// </summary>
        public static readonly StyledProperty<bool> ShowCaptionProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, bool>(nameof(ShowCaption), true);
        
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

        public static readonly StyledProperty<Boolean> HorizontalItemsProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, Boolean>(nameof(HorizontalItems), false, false);

        [Category("VBControl")]
        public Boolean HorizontalItems
        {
            get
            {
                return GetValue(HorizontalItemsProperty);
            }
            set
            {
                SetValue(HorizontalItemsProperty, value);
            }
        }

        private void UpdateOrientationClass()
        {
            if (Classes == null) return;

            if (HorizontalItems)
            {
                Classes.Add("horizontal");
            }
            else
            {
                Classes.Remove("horizontal");
            }

            // Update ItemsPanel when orientation changes
            UpdateItemsPanel();
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
                this.Root().Messages.LogDebug("Error00003", "VBRadioButtonGroup", VBContent);
                return;
            }
            _VBContentPropertyInfo = dcACTypeInfo;
            RightControlMode = dcRightControlMode;

            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            if (VBContentPropertyInfo == null)
                return;

            if (RightControlMode < Global.ControlModes.Disabled)
            {
                IsVisible = false;
            }
            // Beschriftung
            if (string.IsNullOrEmpty(ACCaption))
                ACCaptionTrans = VBContentPropertyInfo.ACCaption;
            else
                ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);

            // Listenbereich füllen (VBSource, VBShowColumns)
            string acAccess = "";
            ACClassProperty sourceProperty = null;
            VBSource = VBContentPropertyInfo != null ? VBContentPropertyInfo.GetACSource(VBSource, out acAccess, out sourceProperty) : VBSource;
            IACType dsACTypeInfo = null;
            object dsSource = null;
            string dsPath = "";
            Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBSource, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00004", "VBRadioButtonGroup", VBSource + " " + VBContent);
                return;
            }

            if (string.IsNullOrEmpty(acAccess))
            {
                ACQueryDefinition = this.Root().Queries.CreateQueryByClass(null, dcACTypeInfo.ValueTypeACClass.PrimaryNavigationquery(), dsACTypeInfo.ValueTypeACClass.ACIdentifier, true);
            }
            else
            {
                IAccess access = ContextACObject.ACUrlCommand(acAccess) as IAccess;
                ACQueryDefinition = access.NavACQueryDefinition;
            }

            List<ACColumnItem> vbShowColumns = ACQueryDefinition != null ? ACQueryDefinition.ACColumns : null;

            if (vbShowColumns == null || !vbShowColumns.Any())
            {
                this.Root().Messages.LogDebug("Error00005", "VBRadioButtonGroup", VBShowColumns + " " + VBContent);
                return;
            }

            IACType dsColACTypeInfo = null;
            object dsColSource = null;
            string dsColPath = "";
            Global.ControlModes dsColRightControlMode = Global.ControlModes.Hidden;
            if (!dsACTypeInfo.ACUrlBinding(vbShowColumns[0].PropertyName, ref dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
            {
                return;
            }

            if (dsACTypeInfo is ACClassMethod)
            {
                var binding = new Binding()
                {
                    Source = dsSource
                };
                this.Bind(ItemsSourceProperty, binding);
            }
            else
            {
                var binding = new Binding()
                {
                    Source = dsSource
                };
                if (!string.IsNullOrEmpty(dsPath))
                {
                    binding.Path = dsPath;
                }
                this.Bind(ItemsSourceProperty, binding);
            }
            
            if (ItemTemplate == null)
                DisplayMemberBinding = new Binding(dsColPath);

            if (dcACTypeInfo.ObjectType.IsEnum)
            {
                SelectedValueBinding = new Binding(Const.Value);

                var binding2 = new Binding()
                {
                    Source = dcSource,
                    Path = dcPath,
                    Mode = BindingMode.TwoWay
                };
                this.Bind(SelectedValueProperty, binding2);
            }
            else
            {
                var binding2 = new Binding()
                {
                    Source = dcSource,
                    Path = dcPath,
                    Mode = VBContentPropertyInfo != null && VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay
                };
                this.Bind(SelectedValueProperty, binding2);
            }

            if (BSOACComponent != null)
            {
                var binding = new Binding()
                {
                    Source = BSOACComponent,
                    Path = Const.InitState,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBRadioButtonGroup.ACCompInitStateProperty, binding);
            }

            if (IsEnabled)
            {
                if (RightControlMode < Global.ControlModes.Enabled)
                {
                    IsEnabled = false;
                }
                else
                {
                    UpdateControlMode();
                }
            }
        }

        bool _Loaded = false;
        void VBRadioButtonGroup_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null)
            {
                var boundedValue = BindingOperations.GetBindingExpressionBase(this, ItemsSourceProperty);
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
                        this.Root().Messages.LogDebug("VBRadioButtonGroup", "AddWPFRef", exw.Message);
                    }
                }
            }
            _Loaded = true;
        }

        void VBRadioButtonGroup_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_Loaded)
                return;

            if (BSOACComponent != null)
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
            this.Loaded -= VBRadioButtonGroup_Loaded;
            this.Unloaded -= VBRadioButtonGroup_Unloaded;

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
            else if (change.Property == ACCaptionProperty)
            {
                if (ContextACObject != null)
                {
                    if (!_Initialized)
                        return;
                    ACCaptionTrans = this.Root().Environment.TranslateText(ContextACObject, ACCaption);
                }
            }
            else if ((change.Property == ItemsHostTypeProperty) 
                || (change.Property == ScrollViewerVisibilityProperty))
            {
                UpdateTemplateClass();
            }
            else if (change.Property == HorizontalItemsProperty)
            {
                UpdateOrientationClass();
            }
            VB_TargetUpdated(null, change);
        }

        void VB_SourceUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            UpdateControlMode();
        }

        void VB_TargetUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            UpdateControlMode();
        }
        #endregion

        #region Event-Handling

        #endregion

        #region Methods
        private IACObject GetNearestContainer(Visual element)
        {
            if (element == null)
                return null;

            {
                int count = 0;
                while (element != null && count < 10)
                {
                    element = element.GetVisualParent();

                    if ((element != null) && (element is VBRadioButtonGroupItem))
                    {
                        VBRadioButtonGroupItem vbListBoxItem = element as VBRadioButtonGroupItem;
                        return vbListBoxItem.Content as IACObject;
                    }
                    count++;
                }
            }
            return null;
        }
        #endregion

        #region IDataField Members

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, string>(nameof(VBContent));

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

        #endregion

        #region IVBSource Members

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
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBRadioButtonGroup>();
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
            AvaloniaProperty.Register<VBRadioButtonGroup, ACInitState>(nameof(ACCompInitState));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
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
        /// Represents the dependency property for VBValidation.
        /// </summary>
        public static readonly AttachedProperty<string> VBValidationProperty = 
            ContentPropertyHandler.VBValidationProperty.AddOwner<VBRadioButtonGroup>();
        /// <summary>
        /// Name of the VBValidation property.
        /// </summary>
        /// <summary xml:lang="de">
        /// Name der Eigenschaft VBValidation.
        /// </summary>
        [Category("VBControl")]
        public string VBValidation
        {
            get { return GetValue(VBValidationProperty); }
            set { SetValue(VBValidationProperty, value); }
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
                ControlModeChanged();
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
            Enabled = controlMode >= Global.ControlModes.Enabled;
            Visible = controlMode >= Global.ControlModes.Disabled;
        }

        /// <summary>
        /// Enables or disables auto focus.
        /// </summary>
        /// <summary xml:lang="de">
        /// Aktiviert oder deaktiviert den Autofokus.
        /// </summary>
        [Category("VBControl")]
        public bool AutoFocus { get; set; }

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
        /// Represents the dependency property for DisabledModes.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty =
            AvaloniaProperty.Register<VBRadioButtonGroup, string>(nameof(DisabledModes));
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

        #region IACMenuBuilder Member
        /// <summary>
        /// Gets the context menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        ///<returns>Returns the list of ACMenu items.</returns>
        public virtual ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            AppendMenu(vbContent, vbControl, ref acMenuItemList);
            return acMenuItemList;
        }

        /// <summary>
        /// Appends the context menu.
        /// </summary>
        /// <param name="vbContent">The vbContent parameter.</param>
        /// <param name="vbControl">The vbControl parameter.</param>
        ///<param name="acMenuItemList">The acMenuItemList parameter.</param>
        public virtual void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            VBLogicalTreeHelper.AppendMenu(this, vbContent, vbControl, ref acMenuItemList);
            this.GetDesignManagerMenu(VBContent, ref acMenuItemList);
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
