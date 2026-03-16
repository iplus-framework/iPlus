using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Styling;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Policy;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// iPlus Avalonia TemplatedControl that dynamically generates filter input controls
    /// (VBTextBox / VBDatePicker / VBComboBox) from the ACQueryDefinition's ACFilterColumns,
    /// laid out in a Grid, with a Search VBButton at the bottom.
    /// Bind VBContent to the ACQueryDefinition property on the BSO, e.g.:
    ///   VBContent="AccessPrimary\NavACQueryDefinition"
    /// </summary>
    [TemplatePart(Name = PART_FilterGridName, Type = typeof(Grid))]
    [TemplatePart(Name = PART_SearchButtonName, Type = typeof(VBButton))]
    [ACClassInfo(Const.PackName_VarioSystem,
                 "en{'VBQueryFilterControl'}de{'VBQueryFilterControl'}",
                 Global.ACKinds.TACVBControl,
                 Global.ACStorableTypes.Required,
                 true, false)]
    public class VBQueryFilterControl : TemplatedControl, IVBContent, IACObject
    {
        #region Constants

        private const string PART_FilterGridName = "PART_FilterGrid";
        private const string PART_SearchButtonName = "PART_SearchButton";

        #endregion

        #region Private fields

        private bool _Initialized = false;
        private IACType _VBContentPropertyInfo = null;

        private Grid _PART_FilterGrid;
        private VBButton _PART_SearchButton;

        #endregion

        #region Constructors

        static VBQueryFilterControl()
        {
            // Override the default StyleKey so Avalonia looks up ControlTheme by this exact type
        }

        /// <summary>
        /// Creates a new instance of VBQueryFilterControl.
        /// </summary>
        public VBQueryFilterControl() : base()
        {
        }

        /// <summary>
        /// The event handler for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        /// <summary>
        /// Overrides OnApplyTemplate – resolves PART controls and triggers VBControl init.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            _PART_FilterGrid = e.NameScope.Find<Grid>(PART_FilterGridName);
            _PART_SearchButton = e.NameScope.Find<VBButton>(PART_SearchButtonName);

            InitVBControl();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            InitVBControl();
        }

        /// <summary>
        /// Routes StyleKeyOverride so Avalonia resolves the correct ControlTheme.
        /// </summary>
        protected override Type StyleKeyOverride => typeof(VBQueryFilterControl);

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the VBControl: resolves the ACQueryDefinition from the BSO
        /// and dynamically populates PART_FilterGrid with the appropriate input controls.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized || DataContext == null || ContextACObject == null)
                return;
            _Initialized = true;

            if (_PART_SearchButton != null)
                _PART_SearchButton.Click += _PART_SearchButton_Click;

            // Bind ACCompInitState to BSO lifecycle
            if (BSOACComponent != null)
            {
                var bsoBinding = new Binding
                {
                    Source = BSOACComponent,
                    Path = Const.InitState,
                    Mode = BindingMode.OneWay
                };
                this.Bind(ACCompInitStateProperty, bsoBinding);
            }

            ACQueryDefinition queryDef = ResolveQueryDefinition();
            if (queryDef == null)
                return;

            BuildFilterGrid(queryDef);
        }

        private void _PART_SearchButton_Click(object sender, RoutedEventArgs e)
        {
            VBDesignController controller = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBDesignController)) as VBDesignController;
            if (controller != null)
            {
                controller.ShowDesign(OpenDesignOnSearch);
            }
        }

        /// <summary>
        /// Resolves the ACQueryDefinition by executing the VBContent ACUrl on the ContextACObject.
        /// Falls back to "AccessPrimary\NavACQueryDefinition" when VBContent is empty.
        /// If the full URL navigation fails (can happen on first call before the generic
        /// ACClass type cache is warm), navigates step-by-step as a fallback.
        /// </summary>
        private ACQueryDefinition ResolveQueryDefinition()
        {
            string acUrl = !string.IsNullOrEmpty(VBContent)
                           ? VBContent
                           : @"AccessPrimary\NavACQueryDefinition";

            // Primary path: single ACUrlCommand call
            if (ContextACObject.ACUrlCommand(acUrl) is ACQueryDefinition queryDef)
                return queryDef;

            // Fallback: navigate segment by segment to bypass the generic-type ACClass
            // resolution issue on the first call (before _ACTypeCache is populated for ACAccess<>).
            int separatorIdx = acUrl.IndexOf('\\');
            if (separatorIdx > 0)
            {
                string accessSegment = acUrl[..separatorIdx];
                string remainder     = acUrl[(separatorIdx + 1)..];

                object accessObj = ContextACObject.ACUrlCommand(accessSegment);
                if (accessObj is IAccess access && string.Equals(remainder, nameof(IAccess.NavACQueryDefinition), StringComparison.Ordinal))
                    return access.NavACQueryDefinition;

                // Generic fallback for deeper paths
                if (accessObj is IACObject acObject)
                    return acObject.ACUrlCommand(remainder) as ACQueryDefinition;
            }

            return null;
        }

        /// <summary>
        /// Populates PART_FilterGrid: one row per filter item, correct input control per type.
        /// Layout: two columns (Auto caption + * input) handled internally by each VB control's
        /// ShowCaption mechanism; we therefore span both columns per control row.
        /// </summary>
        private void BuildFilterGrid(ACQueryDefinition queryDef)
        {
            if (_PART_FilterGrid == null)
                return;

            _PART_FilterGrid.ColumnDefinitions.Clear();
            _PART_FilterGrid.RowDefinitions.Clear();
            _PART_FilterGrid.Children.Clear();

            // Two shared columns: caption (Auto) + input (*)
            _PART_FilterGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
            _PART_FilterGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));

            // Only real filter conditions (skip parenthesis placeholders)
            List<ACFilterItem> filterItems = queryDef.ACFilterColumns
                .Where(f => f.FilterType == Global.FilterTypes.filter
                         && !string.IsNullOrEmpty(f.PropertyName))
                .ToList();

            if (!filterItems.Any())
                return;

            for (int row = 0; row < filterItems.Count; row++)
            {
                _PART_FilterGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

                ACFilterItem filterItem = filterItems[row];
                Type propertyType = ResolvePropertyType(filterItem.PropertyName, out ACClassProperty property);

                Control inputControl = CreateInputControl(filterItem, propertyType, property);
                if (inputControl == null)
                    continue;

                Grid.SetRow(inputControl, row);
                // VB controls render their own caption label internally (ShowCaption=true),
                // so we span both columns to give the control full width.
                Grid.SetColumnSpan(inputControl, 2);

                inputControl.Margin = new Avalonia.Thickness(2);
                _PART_FilterGrid.Children.Add(inputControl);
            }
        }

        /// <summary>
        /// Creates the correct VB input control for the given ACFilterItem:
        ///   String   → VBTextBox
        ///   DateTime → VBDatePicker
        ///   Navigation / complex entity → VBComboBox
        ///
        /// VBContent for each child control points to the SearchWord of the matching
        /// ACFilterItem inside the query definition, using the iPlus ACUrl convention:
        ///   AccessPrimary\NavACQueryDefinition\ACFilterColumns(PropertyName)\SearchWord
        /// </summary>
        private Control CreateInputControl(ACFilterItem filterItem, Type propertyType, ACClassProperty property)
        {
            if (IsNavigationProperty(propertyType))
            {
                var combo = new VBComboBox
                {
                    DataContext = filterItem,
                    VBContent = nameof(ACFilterItem.SearchWord),
                    VBSource = property != null ? property.ACSource : null,
                    ACCaption = property != null ? property.ACCaption : filterItem.ACCaption,
                    ShowCaption = true
                };
                return combo;
            }

            if (IsDateTimeType(propertyType))
            {
                var datePicker = new VBDatePicker
                {
                    DataContext = filterItem,
                    VBContent = nameof(ACFilterItem.SearchDT),
                    ACCaption = property != null ? property.ACCaption : filterItem.ACCaption,
                    ShowCaption = true,
                };
                return datePicker;
            }

            // Default — String (and any other primitive)
            var textBox = new VBTextBox
            {
                DataContext = filterItem,
                VBContent = nameof(ACFilterItem.SearchWord),
                ACCaption = property != null ? property.ACCaption : filterItem.ACCaption,
                ShowCaption = true,
            };
            return textBox;
        }

        /// <summary>
        /// Resolves the .NET Type of the entity property referenced by propertyName
        /// by looking it up in the ACClass metadata of the BSO's navigation query type.
        /// Falls back to typeof(string) on any failure.
        /// </summary>
        private Type ResolvePropertyType(string propertyName, out ACClassProperty property)
        {
            property = null;
            if (string.IsNullOrEmpty(propertyName))
                return typeof(string);

            try
            {
                if (ContextACObject is IACBSO bso)
                {
                    ACClass entityClass = ResolveQueryDefinition()?.TypeACClass;

                    ACQueryDefinition queryDef = ResolveQueryDefinition();

                    // Support dotted / backslash navigation paths
                    string[] parts = propertyName.Split(new[] { '\\', '.' },
                                                        StringSplitOptions.RemoveEmptyEntries);
                    ACClass current = entityClass;

                    foreach (string part in parts)
                    {
                        property = queryDef.QueryType?.Properties
                                       .FirstOrDefault(p =>
                                           string.Equals(p.ACIdentifier, part,
                                                         StringComparison.OrdinalIgnoreCase));

                        if (property.ACSource != null)
                            return property.ObjectType;

                        if (property == null) break;
                        current = property.ValueTypeACClass;
                    }

                    if (property?.ObjectType != null)
                        return property.ObjectType;
                }
            }
            catch
            {
                // Fall through
            }

            return typeof(string);
        }

        /// <summary>
        /// Returns true when the type is a navigation / complex entity type —
        /// i.e. not a primitive, string, DateTime, Guid, decimal, or enum.
        /// </summary>
        private static bool IsNavigationProperty(Type type)
        {
            if (type == null) return false;
            Type u = Nullable.GetUnderlyingType(type) ?? type;
            return (!u.IsPrimitive
                && u != typeof(string)
                && u != typeof(DateTime)
                && u != typeof(DateTimeOffset)
                && u != typeof(Guid)
                && u != typeof(decimal))
                || u.IsEnum;
        }

        /// <summary>
        /// Returns true when the type is DateTime or DateTimeOffset (including nullable variants).
        /// </summary>
        private static bool IsDateTimeType(Type type)
        {
            if (type == null) return false;
            Type u = Nullable.GetUnderlyingType(type) ?? type;
            return u == typeof(DateTime) || u == typeof(DateTimeOffset);
        }

        #endregion

        #region DeInit

        /// <summary>
        /// Removes all bindings, clears the filter grid children,
        /// and propagates DeInit to all child VB controls.
        /// Called when the BSOACComponent is stopped / destructed.
        /// </summary>
        public virtual void DeInitVBControl(IACComponent bso)
        {
            if (!_Initialized)
                return;
            _Initialized = false;
            _VBContentPropertyInfo = null;

            if (_PART_SearchButton != null)
                _PART_SearchButton.Click -= _PART_SearchButton_Click;

            if (_PART_FilterGrid != null)
            {
                foreach (Control child in _PART_FilterGrid.Children)
                {
                    switch (child)
                    {
                        case VBTextBox tb: tb.DeInitVBControl(bso); break;
                        case VBDatePicker dp: dp.DeInitVBControl(bso); break;
                        case VBComboBox cb: cb.DeInitVBControl(bso); break;
                    }
                }
                _PART_FilterGrid.Children.Clear();
                _PART_FilterGrid.RowDefinitions.Clear();
            }

            this.ClearAllBindings();
        }

        /// <summary>
        /// Called when ACCompInitState changes – triggers DeInit on BSO destruction.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null
                && (ACCompInitState == ACInitState.Destructed
                    || ACCompInitState == ACInitState.DisposedToPool))
            {
                DeInitVBControl(BSOACComponent);
            }
        }

        #endregion

        #region Styled / Attached Properties

        // ── BSOACComponent ───────────────────────────────────────────────────────
        /// <summary>
        /// Represents the inherited attached property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty =
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBQueryFilterControl>();

        /// <summary>Gets or sets the BSOACComponent.</summary>
        public IACBSO BSOACComponent
        {
            get => GetValue(BSOACComponentProperty);
            set => SetValue(BSOACComponentProperty, value);
        }

        // ── ACCompInitState ──────────────────────────────────────────────────────
        /// <summary>
        /// Represents the styled property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBQueryFilterControl, ACInitState>(nameof(ACCompInitState));

        /// <summary>Gets or sets the ACCompInitState.</summary>
        public ACInitState ACCompInitState
        {
            get => GetValue(ACCompInitStateProperty);
            set => SetValue(ACCompInitStateProperty, value);
        }

        // ── VBContent ────────────────────────────────────────────────────────────
        /// <summary>
        /// Represents the styled property for VBContent.
        /// Set to the ACUrl of the ACQueryDefinition on the BSO,
        /// e.g. "AccessPrimary\NavACQueryDefinition".
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty =
            AvaloniaProperty.Register<VBQueryFilterControl, string>(nameof(VBContent));

        /// <summary>Gets or sets VBContent.</summary>
        [Category("VBControl")]
        public string VBContent
        {
            get => GetValue(VBContentProperty);
            set => SetValue(VBContentProperty, value);
        }

        // ── OpenDesignOnSearch ────────────────────────────────────────────────────────────
        /// <summary>
        /// Represents the styled property for VBSource.
        /// Set the name of Current property in the BSO
        /// e.g. "CurrentUser".
        /// </summary>
        public static readonly StyledProperty<string> OpenDesignOnSearchProperty =
            AvaloniaProperty.Register<VBQueryFilterControl, string>(nameof(OpenDesignOnSearch), "ExplorerMobile");

        /// <summary>Gets or sets VBSource.</summary>
        [Category("VBControl")]
        public string OpenDesignOnSearch
        {
            get => GetValue(OpenDesignOnSearchProperty);
            set => SetValue(OpenDesignOnSearchProperty, value);
        }

        // ── RightControlMode ─────────────────────────────────────────────────────
        /// <summary>
        /// Represents the styled property for RightControlMode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> RightControlModeProperty =
            AvaloniaProperty.Register<VBQueryFilterControl, Global.ControlModes>(nameof(RightControlMode));

        /// <summary>Gets or sets RightControlMode.</summary>
        [Category("VBControl")]
        public Global.ControlModes RightControlMode
        {
            get => GetValue(RightControlModeProperty);
            set => SetValue(RightControlModeProperty, value);
        }

        #endregion

        #region Property-changed routing

        /// <summary>
        /// Routes ACCompInitState and BSOACComponent property changes.
        /// </summary>
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ACCompInitStateProperty)
            {
                InitStateChanged();
            }
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null
                    && !string.IsNullOrEmpty(VBContent))
                {
                    if (change.OldValue is IACBSO oldBso)
                        DeInitVBControl(oldBso);
                }
            }
        }

        #endregion

        #region IVBContent

        /// <inheritdoc />
        public ACClassProperty VBContentPropertyInfo => _VBContentPropertyInfo as ACClassProperty;

        /// <inheritdoc />
        [Category("VBControl")]
        public string DisabledModes { get; set; }

        /// <inheritdoc />
        public IACObject ContextACObject => DataContext as IACObject;

        /// <inheritdoc />
        [Category("VBControl")]
        public string ACCaption { get; set; }

        /// <inheritdoc />
        public string ACIdentifier
        {
            get => Name;
            set => Name = value;
        }

        #endregion

        #region IACObject

        /// <inheritdoc />
        public IACType ACType => this.ReflectACType();

        /// <inheritdoc />
        public IEnumerable<IACObject> ACContentList => Enumerable.Empty<IACObject>();

        /// <inheritdoc />
        public object ACUrlCommand(string acUrl, params object[] acParameter)
            => this.ReflectACUrlCommand(acUrl, acParameter);

        /// <inheritdoc />
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
            => this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);

        /// <inheritdoc />
        public string GetACUrl(IACObject rootACObject = null) => ACIdentifier;

        /// <inheritdoc />
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo,
                                 ref object source, ref string path,
                                 ref Global.ControlModes rightControlMode)
            => false;

        /// <inheritdoc />
        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
            => this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);

        public void ACAction(ACActionArgs actionArgs)
        {
        }

        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return true;
        }

        /// <inheritdoc />
        public IACObject ParentACObject => Parent as IACObject;

        #endregion
    }
}