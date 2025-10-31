using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Threading;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace gip.core.layoutengine.avui
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBAutoCompleteBox'}de{'VBAutoCompleteBox'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBAutoCompleteBox : AutoCompleteBox, IVBContent, IVBSource, IACObject
    {
        #region Private Fields
        private bool _IsInitialized = false;
        private List<string> _vbShowColumns;
        private DispatcherTimer _cyclickDataRefreshDispTimer;

        // VBContent Interface Properties backing fields
        string _DataChilds;
        protected bool _Initialized = false;
        
        ACQueryDefinition _ACQueryDefinition = null;
        IAccess _ACAccess = null;
        List<IACObject> _ACContentList = new List<IACObject>();
        IACType _ACTypeInfo = null;
        #endregion

        #region Avalonia Styled Properties

        // VBContent Properties
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, Global.ControlModes>(nameof(ControlMode));

        public static readonly StyledProperty<Global.ControlModes> RightControlModeProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, Global.ControlModes>(nameof(RightControlMode));

        public static readonly StyledProperty<string> ACCaptionProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, string>(nameof(ACCaption));

        public static readonly StyledProperty<string> ACCaptionTransProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, string>(nameof(ACCaptionTrans));

        public static readonly StyledProperty<bool> ShowCaptionProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, bool>(nameof(ShowCaption), defaultValue: true);

        public static readonly StyledProperty<bool> IsEnabledScrollIntoViewProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, bool>(nameof(IsEnabledScrollIntoView), defaultValue: true);

        public static readonly StyledProperty<int> CyclicDataRefreshProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, int>(nameof(CyclicDataRefresh));

        public static readonly StyledProperty<GridLength> WidthCaptionProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, GridLength>(nameof(WidthCaption), defaultValue: GridLength.Auto);

        public static readonly StyledProperty<double> WidthCaptionMaxProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, double>(nameof(WidthCaptionMax), defaultValue: double.PositiveInfinity);

        public static readonly StyledProperty<GridLength> WidthContentProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, GridLength>(nameof(WidthContent), defaultValue: new GridLength(1, GridUnitType.Star));

        public static readonly StyledProperty<double> WidthContentMaxProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, double>(nameof(WidthContentMax), defaultValue: double.PositiveInfinity);

        public static readonly StyledProperty<GridLength> WidthPaddingProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, GridLength>(nameof(WidthPadding), defaultValue: GridLength.Auto);

        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, string>(nameof(VBContent));

        public static readonly StyledProperty<string> VBSourceProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, string>(nameof(VBSource));

        public static readonly StyledProperty<string> VBAccessProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, string>(nameof(VBAccess));

        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBAutoCompleteBox>();

        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, ACInitState>(nameof(ACCompInitState));

        public static readonly AttachedProperty<string> VBValidationProperty = ContentPropertyHandler.VBValidationProperty.AddOwner<VBAutoCompleteBox>();

        public static readonly AttachedProperty<bool> DisableContextMenuProperty =
            ContentPropertyHandler.DisableContextMenuProperty.AddOwner<VBAutoCompleteBox>();

        public static readonly StyledProperty<DragMode> DragEnabledProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, DragMode>(nameof(DragEnabled));

        public static readonly StyledProperty<string> DisabledModesProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, string>(nameof(DisabledModes));

        // VBAutoCompleteBox specific properties
        public static readonly StyledProperty<IEnumerable<IACObject>> SourceItemsProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, IEnumerable<IACObject>>(nameof(SourceItems));

        public static readonly StyledProperty<IACObject> SelectedIACObjectProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, IACObject>(nameof(SelectedIACObject));

        public static readonly StyledProperty<string> OnVBAutoCompleteItemSelectedProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, string>(nameof(OnVBAutoCompleteItemSelected));

        public static readonly StyledProperty<string> VBShowColumnsProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, string>(nameof(VBShowColumns));

        public static readonly StyledProperty<string> VBDisabledColumnsProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, string>(nameof(VBDisabledColumns));

        public static readonly StyledProperty<string> DblClickProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, string>(nameof(DblClick));

        public static readonly StyledProperty<bool> AutoFocusProperty =
            AvaloniaProperty.Register<VBAutoCompleteBox, bool>(nameof(AutoFocus));

        #endregion

        #region CLR Properties

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Global.ControlModes ControlMode
        {
            get { return GetValue(ControlModeProperty); }
            set { SetValue(ControlModeProperty, value); }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public Global.ControlModes RightControlMode
        {
            get { return GetValue(RightControlModeProperty); }
            set { SetValue(RightControlModeProperty, value); }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get { return GetValue(ACCaptionProperty); }
            set { SetValue(ACCaptionProperty, value); }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string ACCaptionTrans
        {
            get { return GetValue(ACCaptionTransProperty); }
            set { SetValue(ACCaptionTransProperty, value); }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool ShowCaption
        {
            get { return GetValue(ShowCaptionProperty); }
            set { SetValue(ShowCaptionProperty, value); }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public bool IsEnabledScrollIntoView
        {
            get { return GetValue(IsEnabledScrollIntoViewProperty); }
            set { SetValue(IsEnabledScrollIntoViewProperty, value); }
        }

        [Category("VBControl")]
        public int CyclicDataRefresh
        {
            get { return GetValue(CyclicDataRefreshProperty); }
            set { SetValue(CyclicDataRefreshProperty, value); }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthCaption
        {
            get { return GetValue(WidthCaptionProperty); }
            set { SetValue(WidthCaptionProperty, value); }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthCaptionMax
        {
            get { return GetValue(WidthCaptionMaxProperty); }
            set { SetValue(WidthCaptionMaxProperty, value); }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthContent
        {
            get { return GetValue(WidthContentProperty); }
            set { SetValue(WidthContentProperty, value); }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public double WidthContentMax
        {
            get { return GetValue(WidthContentMaxProperty); }
            set { SetValue(WidthContentMaxProperty, value); }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public GridLength WidthPadding
        {
            get { return GetValue(WidthPaddingProperty); }
            set { SetValue(WidthPaddingProperty, value); }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string VBContent
        {
            get { return GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        public string ACIdentifier { get; set; }

        [Category("VBControl")]
        public string VBSource
        {
            get { return GetValue(VBSourceProperty); }
            set { SetValue(VBSourceProperty, value); }
        }

        [Category("VBControl")]
        public string VBShowColumns
        {
            get { return GetValue(VBShowColumnsProperty); }
            set { SetValue(VBShowColumnsProperty, value); }
        }

        [Category("VBControl")]
        public string VBDisabledColumns
        {
            get { return GetValue(VBDisabledColumnsProperty); }
            set { SetValue(VBDisabledColumnsProperty, value); }
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

        [Category("VBControl")]
        public string VBAccess
        {
            get { return GetValue(VBAccessProperty); }
            set { SetValue(VBAccessProperty, value); }
        }

        public ACQueryDefinition ACQueryDefinition
        {
            get
            {
                return _ACQueryDefinition;
            }
        }

        public IAccess ACAccess
        {
            get
            {
                return _ACAccess;
            }
        }

        public IACObject ContextACObject
        {
            get
            {
                return GetValue(BSOACComponentProperty);
            }
        }

        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        public ACInitState ACCompInitState
        {
            get { return GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return _ACContentList;
            }
        }

        [Category("VBControl")]
        public string DblClick
        {
            get { return GetValue(DblClickProperty); }
            set { SetValue(DblClickProperty, value); }
        }

        [Category("VBControl")]
        public string VBValidation
        {
            get { return GetValue(VBValidationProperty); }
            set { SetValue(VBValidationProperty, value); }
        }

        [Category("VBControl")]
        [ACPropertyInfo(9999)]
        public bool DisableContextMenu
        {
            get { return GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
        }

        [Category("VBControl")]
        public bool AutoFocus
        {
            get { return GetValue(AutoFocusProperty); }
            set { SetValue(AutoFocusProperty, value); }
        }

        [Category("VBControl")]
        public DragMode DragEnabled
        {
            get { return GetValue(DragEnabledProperty); }
            set { SetValue(DragEnabledProperty, value); }
        }

        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _ACTypeInfo as ACClassProperty;
            }
        }

        [Category("VBControl")]
        [Bindable(true)]
        [ACPropertyInfo(9999)]
        public string DisabledModes
        {
            get { return GetValue(DisabledModesProperty); }
            set { SetValue(DisabledModesProperty, value); }
        }

        public IACType ACType
        {
            get
            {
                return _ACTypeInfo;
            }
        }

        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }

        public IEnumerable<IACObject> SourceItems
        {
            get { return GetValue(SourceItemsProperty); }
            set { SetValue(SourceItemsProperty, value); }
        }

        public IACObject SelectedIACObject
        {
            get { return GetValue(SelectedIACObjectProperty); }
            set { SetValue(SelectedIACObjectProperty, value); }
        }

        public List<VBAutoCompleteBoxItem> VBAutoCompleteBoxItems { get; set; }

        [Category("VBControl")]
        public string OnVBAutoCompleteItemSelected
        {
            get { return GetValue(OnVBAutoCompleteItemSelectedProperty); }
            set { SetValue(OnVBAutoCompleteItemSelectedProperty, value); }
        }

        #endregion

        #region Constructor and Initialization

        static VBAutoCompleteBox()
        {
            ACCompInitStateProperty.Changed.AddClassHandler<VBAutoCompleteBox>((x, e) => x.InitStateChanged());
            SourceItemsProperty.Changed.AddClassHandler<VBAutoCompleteBox>((x, e) => x.SourceItemsChanged(e));
            SelectedIACObjectProperty.Changed.AddClassHandler<VBAutoCompleteBox>((x, e) => x.OnSelectedIACObjectChanged(e));
            CyclicDataRefreshProperty.Changed.AddClassHandler<VBAutoCompleteBox>((x, e) => x.LoadCyclicDataRefreshProperty());
        }

        public VBAutoCompleteBox()
        {
            SelectionChanged += VBAutoCompleteBox_SelectionChanged;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            
            // Get the selecting items control from the template
            _selectingItemsControl = e.NameScope.Find<ListBox>("PART_SelectingItemsControl");
            
            InitVBControl();
        }

        protected virtual void InitVBControl()
        {
            if (_IsInitialized || string.IsNullOrEmpty(VBContent) || ContextACObject == null)
                return;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;
            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBAutoCompleteBox", VBContent);
                return;
            }

            string acAccess = "";
            ACClassProperty sourceProperty = null;
            VBSource = dcACTypeInfo is ACClassProperty ? (dcACTypeInfo as ACClassProperty).GetACSource(VBSource, out acAccess, out sourceProperty) : VBSource;
            IACType dsACTypeInfo = null;
            object dsSource = null;
            string dsPath = "";
            Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBSource, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00004", "VBAutoCompleteBox", VBSource + " " + VBContent);
                return;
            }

            if (string.IsNullOrEmpty(VBShowColumns))
                _vbShowColumns = new List<string>() { "ACCaption" };
            else
            {
                if (VBShowColumns.Contains(","))
                    _vbShowColumns = VBShowColumns.Split(new char[] { ',' }).Select(c => c.Trim()).ToList();
                else
                    _vbShowColumns = new List<string>() { VBShowColumns };
            }

            Binding binding = new Binding
            {
                Source = dsSource,
                Path = dsPath
            };
            this.Bind(SourceItemsProperty, binding);

            Binding binding2 = new Binding
            {
                Source = this,
                Path = nameof(VBAutoCompleteBoxItems)
            };
            this.Bind(ItemsSourceProperty, binding2);

            Binding binding3 = new Binding
            {
                Source = dcSource,
                Path = dcPath,
                Mode = BindingMode.TwoWay
            };
            this.Bind(SelectedIACObjectProperty, binding3);

            ValueMemberBinding = new Binding("CurrentItemCaption");

            if (BSOACComponent != null)
            {
                Binding binding4 = new Binding
                {
                    Source = BSOACComponent,
                    Path = Const.InitState,
                    Mode = BindingMode.OneWay
                };
                this.Bind(ACCompInitStateProperty, binding4);
            }

            _IsInitialized = true;
        }

        public virtual void DeInitVBControl(IACComponent bso)
        {
            SelectionChanged -= VBAutoCompleteBox_SelectionChanged;
            
            // Clear bindings equivalent to WPF's ClearAllBindings
            this.ClearValue(SourceItemsProperty);
            this.ClearValue(ItemsSourceProperty);
            this.ClearValue(SelectedIACObjectProperty);
            this.ClearValue(ACCompInitStateProperty);
        }

        #endregion

        #region Event Handlers

        private void SourceItemsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is IEnumerable<IACObject> newValue)
            {
                VBAutoCompleteBoxItems = GenerateVBAutoCompleteBoxItems(newValue);
            }
        }

        private void OnSelectedIACObjectChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                OnSelectedItemChanged(null);
                Text = "";
            }
        }

        private void OnSelectedItemChanged(object newItem)
        {
            string text;

            if (newItem == null)
            {
                text = SearchText;
            }
            else
            {
                text = FormatValue(newItem, true);
            }

            // Update the Text property
            Text = text;

            // Update SelectedIACObject
            SelectedIACObject = newItem == null ? null : (newItem as VBAutoCompleteBoxItem)?.SourceItem;
        }

        private void VBAutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e != null && e.AddedItems.Count == 1)
            {
                OnSelectedItemChanged(e.AddedItems[0]);
            }
        }

        protected void InitStateChanged()
        {
            if (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool)
                DeInitVBControl(BSOACComponent);
        }

        private void LoadCyclicDataRefreshProperty()
        {
            if (_cyclickDataRefreshDispTimer != null)
            {
                _cyclickDataRefreshDispTimer.Stop();
                _cyclickDataRefreshDispTimer = null;
            }

            if (CyclicDataRefresh > 0)
            {
                _cyclickDataRefreshDispTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(CyclicDataRefresh), DispatcherPriority.Background, cyclickDataRefreshDispTimer_CanExecute);
                _cyclickDataRefreshDispTimer.Start();
            }
        }

        private void cyclickDataRefreshDispTimer_CanExecute(object sender, EventArgs e)
        {
            // Implementation for cyclic data refresh
        }

        #endregion

        #region Helper Methods

        private List<VBAutoCompleteBoxItem> GenerateVBAutoCompleteBoxItems(IEnumerable<IACObject> value)
        {
            if (value == null || _vbShowColumns == null || !_vbShowColumns.Any())
                return null;

            List<VBAutoCompleteBoxItem> result = new List<VBAutoCompleteBoxItem>();
            foreach (string showCol in _vbShowColumns)
            {
                result.AddRange(value.Select(c => new VBAutoCompleteBoxItem(c, showCol)).Where(c => !string.IsNullOrEmpty(c.CurrentItemCaption)).Distinct());
            }

            return result;
        }

        protected override string FormatValue(object value)
        {
            return base.FormatValue(value);
        }

        private string FormatValue(object value, bool clearDataContext)
        {
            string str = FormatValue(value);
            if (clearDataContext && ValueBindingEvaluator != null)
            {
                ValueBindingEvaluator.ClearDataContext();
            }
            return str;
        }

        private static FieldInfo _valueBindingEvaluatorFieldInfo;
        private static readonly object _fieldInfoLock = new object();

        protected BindingEvaluator<string> ValueBindingEvaluator
        {
            get
            {
                if (_valueBindingEvaluatorFieldInfo == null)
                {
                    lock (_fieldInfoLock)
                    {
                        if (_valueBindingEvaluatorFieldInfo == null)
                        {
                            _valueBindingEvaluatorFieldInfo = typeof(AutoCompleteBox).GetField("_valueBindingEvaluator",
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        }
                    }
                }

                return _valueBindingEvaluatorFieldInfo?.GetValue(this) as BindingEvaluator<string>;
            }
        }

        public void UpdateControlMode()
        {
            // Implementation for updating control mode
        }

        #endregion

        #region IACObject Interface

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        public virtual ACMenuItemList GetMenu(string vbContent, string vbControl)
        {
            ACMenuItemList acMenuItemList = new ACMenuItemList();
            AppendMenu(vbContent, vbControl, ref acMenuItemList);
            return acMenuItemList;
        }

        public virtual void AppendMenu(string vbContent, string vbControl, ref ACMenuItemList acMenuItemList)
        {
            VBLogicalTreeHelper.AppendMenu(this, vbContent, vbControl, ref acMenuItemList);
        }

        #endregion

        #region IVBContent Interface Methods

        public void ACAction(ACActionArgs actionArgs)
        {
            // Implementation for ACAction
        }

        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return false;
        }

        #endregion

        #region Context Menu Handling

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
            {
                if (ContextMenu == null)
                {
                    var point = e.GetCurrentPoint(this);
                    ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, point.Position.X, point.Position.Y, Global.ElementActionType.ContextMenu);
                    BSOACComponent?.ACAction(actionArgs);
                    if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                    {
                        VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
                        this.ContextMenu = vbContextMenu;
                        ContextMenu.Open(this);
                    }
                }
            }
            else
            {
                base.OnPointerReleased(e);
            }
        }

        #endregion

        #region Selection Adapter Event Handlers

        private ListBox _selectingItemsControl;

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (SelectionAdapter?.SelectedItem != null && IsEnabledScrollIntoView)
            {
                // Scroll into view logic for Avalonia
                if (_selectingItemsControl != null)
                {
                    _selectingItemsControl.ScrollIntoView(SelectionAdapter.SelectedItem);
                }
            }

            if (BSOACComponent != null && !string.IsNullOrEmpty(OnVBAutoCompleteItemSelected))
            {
                BSOACComponent.ACUrlCommand(OnVBAutoCompleteItemSelected, null);
            }
        }

        #endregion
    }

    public class VBAutoCompleteBoxItem
    {
        public VBAutoCompleteBoxItem(IACObject sourceItem, string propertyName)
        {
            SourceItem = sourceItem;
            CurrentItemCaption = sourceItem.ACUrlCommand(propertyName, null).ToString();
        }

        public string CurrentItemCaption
        {
            get;
            set;
        }

        public IACObject SourceItem
        {
            get;
            set;
        }
    }
}