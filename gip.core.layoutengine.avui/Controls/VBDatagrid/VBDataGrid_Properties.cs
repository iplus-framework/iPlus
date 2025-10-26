using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace gip.core.layoutengine.avui
{

    public partial class VBDataGrid
    {
        string _Caption;
        string _DataShowColumns;
        string _DataDisabledColumns;
        List<string> _DisabledColumnList;
        string _DataChilds;
        bool _Enabled = false;
        List<DataGridColumn> _ColumnsFromXAML = new List<DataGridColumn>();
        private DispatcherTimer _cyclickDataRefreshDispTimer;

        #region Additional Dependency-Properties


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
        /// Represents the styled property for control mode.
        /// </summary>
        public static readonly StyledProperty<Global.ControlModes> ControlModeProperty
            = AvaloniaProperty.Register<VBDataGrid, Global.ControlModes>(nameof(ControlMode));

        /// <summary>
        /// Represents the control mode property.
        /// </summary>
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

        /// <summary>
        /// Represents the styled property for VerticalRows.
        /// </summary>
        public static readonly StyledProperty<bool> VerticalRowsProperty
            = AvaloniaProperty.Register<VBDataGrid, bool>(nameof(VerticalRows));

        /// <summary>
        /// Determines if rows are vertical or not in VBDataGrid. 
        /// </summary>
        [Category("VBControl")]
        public bool VerticalRows
        {
            get { return GetValue(VerticalRowsProperty); }
            set { SetValue(VerticalRowsProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for IsSumEnabled.
        /// </summary>
        public static readonly StyledProperty<bool> IsSumEnabledProperty
            = AvaloniaProperty.Register<VBDataGrid, bool>(nameof(IsSumEnabled), false);

        /// <summary>
        /// This property enables or disables sum row in datagrid.
        /// </summary>
        [Category("VBControl")]
        public bool IsSumEnabled
        {
            get
            {
                return GetValue(IsSumEnabledProperty);
            }
            set
            {
                SetValue(IsSumEnabledProperty, value);
            }
        }

        /// <summary>
        /// Represents the styled property for VBSumColumns.
        /// </summary>
        public static readonly StyledProperty<string> VBSumColumnsProperty = AvaloniaProperty.Register<VBDataGrid, string>(nameof(VBSumColumns));

        /// <summary>
        /// Represents the property in which you define sum columns. XAML Sample: VBSumColumns="SumCol1,SumCol2,SumCol3"
        /// </summary>
        [Category("VBControl")]
        public string VBSumColumns
        {
            get
            {
                return GetValue(VBSumColumnsProperty);
            }
            set
            {
                SetValue(VBSumColumnsProperty, value);
            }
        }

        #region CyclicDataRefresh

        /// <summary>
        /// Determines if cyclic refresh is enabled or disabled. The value (integer) in this property determines the interval of cyclic execution in milliseconds.   
        /// </summary>
        /// <summary xml:lang="de">
        /// Bestimmt, ob die zyklische Aktualisierung aktiviert oder deaktiviert ist. Der Wert (Integer) in dieser Eigenschaft bestimmt das Intervall der zyklischen Ausführung in Millisekunden.
        /// </summary>
        [Category("VBControl")]
        public int CyclicDataRefresh
        {
            get { return GetValue(CyclicDataRefreshProperty); }
            set { SetValue(CyclicDataRefreshProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for CyclicDataRefresh.
        /// </summary>
        public static readonly StyledProperty<int> CyclicDataRefreshProperty = AvaloniaProperty.Register<VBDataGrid, int>(nameof(CyclicDataRefresh));

        #endregion

        #endregion

        #region Loaded Event
        bool _Initialized = false;
        bool _Loaded = false;
        private Nullable<bool> ColumnsSetInXAML;
        #endregion

        #region IDataField Members

        /// <summary>
        /// Represents the styled property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty
            = AvaloniaProperty.Register<VBDataGrid, string>(nameof(VBContent));


        /// <summary>
        /// Represents the property in which you enter the name of BSO's Selected property. Selected property must be marked with [ACPropertySelected(...)] attribute.
        /// The BSO Selected property can be a single object or a list of objects if multiple rows selection is allowed in VBDataGrid.
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

        IACType _PropertyInfoOfACPropertySelected = null;
        /// <summary>
        /// ACClassProperty that describes the Selected property that is bound to the SelectedItem property of the DataGrid.
        /// These are the properties in the BSO that are assigned the attribute[ACPropertySelected(...,..)].
        /// </summary>
        /// <summary xml:lang="de">
        /// ACClassProperty die die Selected-Eigenschaft beschreibt, die an die SelectedItem-Property des DataGrids gebunden wird
        /// /Es handelt sich dabei um die Properties im BSO, die mit dem Attribut [ACPropertySelected(..,..)] versehen sind
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _PropertyInfoOfACPropertySelected as ACClassProperty;
            }
        }

        string _ACURLOfPropertyForItemsSource;

        /// <summary>
        /// Represents the property in which you enter the name of BSO's list property marked with [ACPropertyList(...)] attribute. In usage with ACPropertySelected with same ACGroup,
        /// this property is not necessary to be set. Only if you want to use different list instead, you must set this property to name of that list which must be marked with ACPropertyList attribute.
        /// </summary>
        /// <summary xml:lang="de">
        /// Name der List-Eigenschaft des BSO's, das mit dem Attribut [ACPropertyList(..,..)] gekennzeichnet ist.
        /// Diese Eigenschaft muss nicht gesetzt werden, da Sie über die VBContent-Eigenschaft und der zusammengehörenden ACGroup bei der VBDataGrid-Initialisierung automatisch ermittelt werden kann
        /// Die Eigenschaft KANN gesetzt werden, wenn eine stattdessen eine andere Liste als ItemsSource verwendet werden soll
        /// Die ACPropertyList-Eigenschaft des BSO's wird im Datagrid mit der ItemsSource-Property gebunden
        /// </summary>
        [Category("VBControl")]
        public string VBSource
        {
            get
            {
                return _ACURLOfPropertyForItemsSource;
            }
            set
            {
                _ACURLOfPropertyForItemsSource = value;
            }
        }

        private IACType _ItemsSourceACTypeInfo;
        /// <summary>
        /// ACClassProperty that describes the list that is bound to the ItemsSource property of the DataGrid
        /// These are the properties in the BSO that are assigned the attribute [ACPropertyList(..,..)]
        /// </summary>
        /// <summary xml:lang="de">
        /// ACClassProperty die, die Liste beschreibt die an die ItemsSource-Property des DataGrids gebunden wird
        /// Es handelt sich dabei um die Properties im BSO, die mit dem Attribut [ACPropertyList(..,..)] versehen sind
        /// </summary>
        internal IACType ItemsSourceACTypeInfo
        {
            get
            {
                return _ItemsSourceACTypeInfo;
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

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                return _Caption;
            }
            set
            {
                _Caption = value;
            }
        }

        /// <summary>
        /// Represents the styled property for VBAccess.
        /// </summary>
        public static readonly StyledProperty<string> VBAccessProperty
            = AvaloniaProperty.Register<VBDataGrid, string>(nameof(VBAccess));

        /// <summary>
        /// Represents the property in which you enter the name of BSO's list property marked with [ACPropertyList(...)] attribute. In usage with ACPropertySelected with same ACGroup,
        /// this property is not necessary to be set. Only if you want to use different list instead, you must set this property to name of that list which must be marked with ACPropertyList attribute.
        /// </summary>
        [Category("VBControl")]
        public string VBAccess
        {
            get { return GetValue(VBAccessProperty); }
            set { SetValue(VBAccessProperty, value); }
        }


        public static readonly StyledProperty<bool> BindItemsSourceToContextProperty
            = AvaloniaProperty.Register<VBDataGrid, bool>(nameof(BindItemsSourceToContext));

        [Category("VBControl")]
        public bool BindItemsSourceToContext
        {
            get { return GetValue(BindItemsSourceToContextProperty); }
            set { SetValue(BindItemsSourceToContextProperty, value); }
        }

        public static readonly StyledProperty<bool> BindSelectedItemToContextProperty
            = AvaloniaProperty.Register<VBDataGrid, bool>(nameof(BindSelectedItemToContext));

        [Category("VBControl")]
        public bool BindSelectedItemToContext
        {
            get { return GetValue(BindSelectedItemToContextProperty); }
            set { SetValue(BindSelectedItemToContextProperty, value); }
        }
        #endregion

        #region IVBSource Members

        string _DataCurrent = "";
        /// <summary>
        /// Gets or sets the DataCurrent.
        /// </summary>
        public string DataCurrent
        {
            get
            {
                return _DataCurrent;
            }
            set
            {
                _DataCurrent = value;
            }
        }

        /// <summary>
        /// Determines which properties of bounded object will be shown in VBDataGrid. XAML sample: VBShowColumns="PropName1,PropName2,PropName3"
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, welche Eigenschaften des gebundenen Objekts in VBDataGrid angezeigt werden. XAML-Probe: VBShowColumns="PropName1,PropName2,PropName3".
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
        /// Determines which columns will be disabled in VBDataGrid.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, welche Spalten im VBDataGrid deaktiviert werden.
        /// </summary>
        [Category("VBControl")]
        public string VBDisabledColumns
        {
            get
            {
                return _DataDisabledColumns;
            }
            set
            {
                _DataDisabledColumns = value;
                _DisabledColumnList = value?.Split(',').ToList() ?? new List<string>();
            }
        }

        [Category("VBControl")]
        public bool SimpleCellElements
        {
            get; set;
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
        /// Gets the ACAccess.
        /// </summary>
        public IAccess ACAccess
        {
            get
            {
                return _ACAccess;
            }
        }
        #endregion

        #region IDataHandling Members

        /// <summary>
        /// Determines if enabled auto load or not.
        /// </summary>
        [Category("VBControl")]
        public bool AutoLoad { get; set; }
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
        /// Represents the attached property for BSOACComponent.
        /// </summary>
        public static readonly AttachedProperty<IACBSO> BSOACComponentProperty = 
            AvaloniaProperty.RegisterAttached<VBDataGrid, Control, IACBSO>(nameof(BSOACComponent), inherits: true);

        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for ACUrlCmdMessage.
        /// </summary>
        public static readonly StyledProperty<ACUrlCmdMessage> ACUrlCmdMessageProperty =
            AvaloniaProperty.Register<VBDataGrid, ACUrlCmdMessage>(nameof(ACUrlCmdMessage));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for ACCompInitState.
        /// </summary>
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBDataGrid, ACInitState>(nameof(ACCompInitState));

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
        /// <value> A nullable list of IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return _ACContentList;
            }
        }

        string _DblClick = "";
        /// <summary>
        /// Represents the property in which you enter the name of method that is invoked on mouse double click.
        /// </summary>
        /// <summary xml:lang="de">
        /// Stellt die Eigenschaft dar, in der Sie den Namen der Methode eingeben, die beim Doppelklick mit der Maus aufgerufen wird.
        /// </summary>
        [Category("VBControl")]
        public string DblClick
        {
            get
            {
                if (string.IsNullOrEmpty(_DblClick) && (BSOACComponent != null))
                {
                    var query = BSOACComponent.ACClassMethods.Where(c => c.InteractionVBContent == VBContent && c.SortIndex == (short)MISort.Load);
                    if (query.Any())
                    {
                        return "!" + query.First().ACIdentifier;
                    }
                }
                return _DblClick;
            }
            set
            {
                _DblClick = value;
            }
        }

        string _VBOnDrag = "";
        /// <summary>
        /// Handle on drag event (MouseLeftButtonUp)
        /// </summary>
        /// <summary xml:lang="de">
        /// Handle on drag event (MouseLeftButtonUp)
        /// </summary>
        [Category("VBControl")]
        public string VBOnDrag
        {
            get
            {
                return _VBOnDrag;
            }
            set
            {
                _VBOnDrag = value;
            }
        }

        /// <summary>
        /// Represents the attached property for VBValidation.
        /// </summary>
        public static readonly AttachedProperty<string> VBValidationProperty = 
            AvaloniaProperty.RegisterAttached<VBDataGrid, Control, string>("VBValidation", inherits: true);
        
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

        /// <summary>
        /// Represents the attached property for DisableContextMenu.
        /// </summary>
        public static readonly AttachedProperty<bool> DisableContextMenuProperty = 
            AvaloniaProperty.RegisterAttached<VBDataGrid, Control, bool>("DisableContextMenu", false, inherits: true);
        
        /// <summary>
        /// Determines if context menu is disabled or enabled.
        /// </summary>
        /// <summary xml:lang="de">
        /// Ermittelt ist das Kontextmenü deaktiviert oder aktiviert.
        /// </summary>
        [Category("VBControl")]
        [ACPropertyInfo(9999)]
        public bool DisableContextMenu
        {
            get { return GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
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
                return _Enabled;
            }
            set
            {
                if (value == true)
                {
                    if (ContextACObject != null)
                    {
                        _Enabled = RightControlMode >= Global.ControlModes.Enabled;
                    }
                    else
                    {
                        _Enabled = true;
                    }
                }
                else
                {
                    _Enabled = false;
                }
                foreach (var column in Columns)
                {
                    IGriColumn iCol = column as IGriColumn;
                    if (iCol != null)
                        iCol.RefreshReadOnlyProperty(System.Convert.ToInt16(!_Enabled));
                }
                ControlModeChanged();
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

        /// <summary>
        /// Represents the styled property for DisabledModes.
        /// </summary>
        public static readonly StyledProperty<string> DisabledModesProperty
            = AvaloniaProperty.Register<VBDataGrid, string>(nameof(DisabledModes));
        
        /// <summary>
        /// Gets or sets the DisabledModes.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die DisabledModes.
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

        #region Mobile Members

        /// <summary>
        /// Represents the styled property for checking if the control is opened in the Mobile App.
        /// </summary>
        public static readonly StyledProperty<string> OpenDesignOnClickProperty
            = AvaloniaProperty.Register<VBDataGrid, string>(nameof(OpenDesignOnClick));

        public string OpenDesignOnClick
        {
            get { return GetValue(OpenDesignOnClickProperty); }
            set { SetValue(OpenDesignOnClickProperty, value); }
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

        #region DragAndDrop
        /// <summary>
        /// Gets or sets DragEnabled.
        /// </summary>
        public DragMode DragEnabled { get; set; }

        public bool IsEnabledMoveRows
        {
            get { return GetValue(IsEnabledMoveRowsProperty); }
            set { SetValue(IsEnabledMoveRowsProperty, value); }
        }

        public static readonly StyledProperty<bool> IsEnabledMoveRowsProperty =
            AvaloniaProperty.Register<VBDataGrid, bool>(nameof(IsEnabledMoveRows), false);

        private object _DraggedItem = null;
        #endregion

        #region Sum

        /// <summary>
        /// Represents the styled property for SumVisibility.
        /// </summary>
        public static readonly StyledProperty<bool> SumVisibilityProperty = 
            AvaloniaProperty.Register<VBDataGrid, bool>(nameof(SumVisibility), false);
        
        /// <summary>
        /// Gets or sets the SumVisibility.
        /// </summary>
        [Category("VBControl")]
        public bool SumVisibility
        {
            get { return GetValue(SumVisibilityProperty); }
            set { SetValue(SumVisibilityProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for DictionarySumProperties.
        /// </summary>
        public static readonly StyledProperty<VBDataGridSumDictionary> DictionarySumPropertiesProperty = 
            AvaloniaProperty.Register<VBDataGrid, VBDataGridSumDictionary>(nameof(DictionarySumProperties));

        /// <summary>
        /// Dictionary for temp save summary values.
        /// </summary>
        public VBDataGridSumDictionary DictionarySumProperties
        {
            get { return GetValue(DictionarySumPropertiesProperty); }
            set { SetValue(DictionarySumPropertiesProperty, value); }
        }

        String[] _SumColumns;

        #endregion

        #region Clipboard
        private bool _CopyToClipboard = false;
        #endregion
    }

    public class VBDataGridSumDictionary : AvaloniaList<KeyValuePair<string, string>>
    {
        public string this[string key]
        {
            get
            {
                var item = this.FirstOrDefault(kvp => kvp.Key == key);
                return item.Key != null ? item.Value : null;
            }
            set
            {
                var existingIndex = -1;
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].Key == key)
                    {
                        existingIndex = i;
                        break;
                    }
                }

                if (existingIndex >= 0)
                {
                    this[existingIndex] = new KeyValuePair<string, string>(key, value);
                }
                else
                {
                    this.Add(new KeyValuePair<string, string>(key, value));
                }
            }
        }

        public bool ContainsKey(string key)
        {
            return this.Any(kvp => kvp.Key == key);
        }

        public void Add(string key, string value)
        {
            this.Add(new KeyValuePair<string, string>(key, value));
        }
    }
}
