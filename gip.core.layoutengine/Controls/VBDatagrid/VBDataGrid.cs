using System.Runtime.CompilerServices;
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
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;
using System.Transactions;
using System.Collections;
using System.Runtime.Serialization;
using gip.core.layoutengine.Helperclasses;
using gip.core.datamodel;
using System.IO;
using System.Xml;
using System.Data.Objects.DataClasses;
using System.Reflection;
using System.Data;
using ClosedXML.Excel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Interface for VBDataGrid columns.
    /// </summary>
    public interface IGriColumn
    {
        /// <summary>
        /// Represents the name of a bounded object(ACPropertySelected[...]) property which is marked with [ACPropertyInfo(...)] attribute.
        /// </summary>
        string VBContent { get; }

        /// <summary>
        /// The ACColumn item.
        /// </summary>
        ACColumnItem ACColumnItem { get; set; }
        /// <summary>
        /// Defines is column read only or not.
        /// </summary>
        bool VBIsReadOnly { get; set; }

        /// <summary>
        /// The column right control mode.
        /// </summary>
        Global.ControlModes RightControlMode { get; }

        /// <summary>
        /// The ACType of column.
        /// </summary>
        ACClassProperty ColACType { get; }

        /// <summary>
        /// The VBDataGrid.
        /// </summary>
        VBDataGrid VBDataGrid { get; }

        /// <summary>
        /// WPF-Property
        /// </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// Refreshes the IsReadOnly-Property
        /// </summary>
        /// <param name="newReadOnlyState">-1 default, 0 = Unset Readonly  from BSO, 1 = Set Readonly from BSO</param>
        void RefreshReadOnlyProperty(short newReadOnlyState = -1);

        /// <summary>
        /// Represents the Deinitialization method for VBControl.
        /// </summary>
        /// <param name="bso">The BSO parameter.</param>
        void DeInitVBControl(IACComponent bso);
    }

    /// <summary>
    /// Represents the list of column sizes.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt die Liste der Spaltengrößen dar.
    /// </summary>
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ColumnSize'}de{'ColumnSize'}", Global.ACKinds.TACSimpleClass)]
    public class ColumnSizeList : List<ColumnSize>
    {
    }

    /// <summary>
    /// Represents the size of column.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt die Größe der Spalte dar.
    /// </summary>
    [ACSerializeableInfo]
    [DataContract]
    public class ColumnSize
    {
        /// <summary>
        /// The ACIdentifier.
        /// </summary>
        [DataMember]
        public string ACIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Represents the column width.
        /// </summary>
        [DataMember]
        public double Width
        {
            get;
            set;
        }
    }

    ///<summary>
    /// Control element for displaying data in a table form.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von Daten in Tabellenform.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDataGrid'}de{'VBDataGrid'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBDataGrid : DataGrid, IVBContent, IVBSource, IACMenuBuilderWPFTree, IACObject
    {
        string _Caption;
        string _DataShowColumns;
        string _DataDisabledColumns;
        List<string> _DisabledColumnList;
        string _DataChilds;
        bool _Enabled = false;
        List<DataGridColumn> _ColumnsFromXAML = new List<DataGridColumn>();

        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> {
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip,
                                         styleName = "DataGridStyleGip",
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDataGrid/Themes/DataGridStyleGip.xaml",
                                         hasImplicitStyles = true},
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero,
                                         styleName = "DataGridStyleAero",
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDataGrid/Themes/DataGridStyleAero.xaml",
                                         hasImplicitStyles = true},
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

        static VBDataGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBDataGrid), new FrameworkPropertyMetadata(typeof(VBDataGrid)));
        }

        bool _themeApplied = false;
        /// <summary>
        /// Creates a new instace of VBDataGrid.
        /// </summary>
        public VBDataGrid()
        {
            DragEnabled = DragMode.Disabled;
            AllowDrop = false;
            AutoLoad = true;
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
            _ColumnsFromXAML = Columns.ToList();
            Loaded += VBDataGrid_Loaded;
            Unloaded += VBDataGrid_Unloaded;
            TargetUpdated += VBDataGrid_TargetUpdated;
            SourceUpdated += VBDataGrid_SourceUpdated;
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
            _themeApplied = ControlManager.RegisterImplicitStyle(this, StyleInfoList, bInitializingCall);
        }
        #endregion

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
        /// Represents the dependency property for control mode.
        /// </summary>
        public static readonly DependencyProperty ControlModeProperty
            = DependencyProperty.Register("ControlMode", typeof(Global.ControlModes), typeof(VBDataGrid));

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
                return (Global.ControlModes)GetValue(ControlModeProperty);
            }
            set
            {
                SetValue(ControlModeProperty, value);
            }
        }

        /// <summary>
        /// [Open]Represents the dependency property for VerticalRows.
        /// </summary>
        public static readonly DependencyProperty VerticalRowsProperty
            = DependencyProperty.Register("VerticalRows", typeof(bool), typeof(VBDataGrid));

        /// <summary>
        /// [Open]Determines is rows vertical or not in VBDataGrid. 
        /// </summary>
        [Category("VBControl")]
        public bool VerticalRows
        {
            get { return (bool)GetValue(VerticalRowsProperty); }
            set { SetValue(VerticalRowsProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for IsSumEnabled.
        /// </summary>
        public static readonly DependencyProperty IsSumEnabledProperty
            = DependencyProperty.Register("IsSumEnabled", typeof(bool), typeof(VBDataGrid), new PropertyMetadata(false));

        /// <summary>
        /// This propety enable or disable sum row in datagrid.
        /// </summary>
        [Category("VBControl")]
        public bool IsSumEnabled
        {
            get
            {
                return (bool)GetValue(IsSumEnabledProperty);
            }
            set
            {
                SetValue(IsSumEnabledProperty, value);
            }
        }

        /// <summary>
        /// Represents the dependency property for VBSumColumns.
        /// </summary>
        public static readonly DependencyProperty VBSumColumnsProperty = DependencyProperty.Register("VBSumColumns", typeof(string), typeof(VBDataGrid));

        /// <summary>
        /// Represents the property in which you define sum columns. XAML Sample: VBSumColumns="SumCol1,SumCol2,SumCol3"
        /// </summary>
        [Category("VBControl")]
        public string VBSumColumns
        {
            get
            {
                return (string)GetValue(VBSumColumnsProperty);
            }
            set
            {
                SetValue(VBSumColumnsProperty, value);
            }
        }
        #endregion

        #region Loaded Event
        bool _Initialized = false;
        bool _Loaded = false;
        void VBDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null)
            {
                Binding boundedValue = BindingOperations.GetBinding(this, DataGrid.ItemsSourceProperty);
                if (boundedValue != null)
                {
                    IACObject boundToObject = boundedValue.Source as IACObject;
                    try
                    {
                        if (boundToObject != null)
                            BSOACComponent.AddWPFRef(this.GetHashCode(), boundToObject);
                    }
                    catch (Exception exw)
                    {
                        this.Root().Messages.LogDebug("VBDataGrid", "AddWPFRef", exw.Message);
                    }
                }
            }
            _Loaded = true;
        }

        void VBDataGrid_Unloaded(object sender, RoutedEventArgs e)
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
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            if (_Initialized || DataContext == null || ContextACObject == null)
                return;
            _Initialized = true;
            if (String.IsNullOrEmpty(VBContent))
                return;
            // VBContent muß im XAML gestetzt sein
            System.Diagnostics.Debug.Assert(VBContent != "");

            _PropertyInfoOfACPropertySelected = null; // Selected Type Info
            object sourceOfBindingForSelItm = null; // Für das Binding der SelectedItem-Property, die das Source-Objekt das auf die Source-Eigenschaft gesetzt wird
            string pathOfBindingForSelItm = ""; // Für das Binding der SelectedItem-Property, der Pfad relativ zum Source-Objekt
            Global.ControlModes rightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref _PropertyInfoOfACPropertySelected, ref sourceOfBindingForSelItm, ref pathOfBindingForSelItm, ref rightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBDataGrid", VBContent);
                return;
            }
            if (_PropertyInfoOfACPropertySelected == null)
                return;
            if (((ACClassProperty)_PropertyInfoOfACPropertySelected).ACPropUsage == Global.ACPropUsages.Current)
                AutoLoad = false;
            RightControlMode = rightControlMode;

            ValueSource valueSource = DependencyPropertyHelper.GetValueSource(this, DataGrid.SelectionUnitProperty);
            if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
            {
                if (SelectionUnit != DataGridSelectionUnit.FullRow)
                    SelectionUnit = DataGridSelectionUnit.FullRow;
            }

            valueSource = DependencyPropertyHelper.GetValueSource(this, DataGrid.SelectionModeProperty);
            // Plausibilitätsprüfung: Falls VBContent eine Liste ist, dann dürfen mehrere Zeilen ausgewählt werden: DataGridSelectionMode.Extended
            // Sonst muss SelectionMode DataGridSelectionMode.Single sein
            if (VBContentPropertyInfo != null)
            {
                if ((SelectionMode == DataGridSelectionMode.Extended) && !VBContentPropertyInfo.IsEnumerable)
                {
                    if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
                        SelectionMode = DataGridSelectionMode.Single;
                }
                else if ((SelectionMode == DataGridSelectionMode.Single) && VBContentPropertyInfo.IsEnumerable)
                {
                    if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
                        SelectionMode = DataGridSelectionMode.Extended;
                }
            }

            if (RightControlMode < Global.ControlModes.Disabled)
            {
                Visibility = Visibility.Collapsed;
            }
            string acAccess = "";
            ACClassProperty sourceProperty = null;
            _ACURLOfPropertyForItemsSource = VBContentPropertyInfo != null ? VBContentPropertyInfo.GetACSource(_ACURLOfPropertyForItemsSource, out acAccess, out sourceProperty) : _ACURLOfPropertyForItemsSource;
            _ItemsSourceACTypeInfo = null;
            object sourceOfBindingForItmSrc = null; // Für das Binding der ItemsSource, die das Source-Objekt das auf die Source-Eigenschaft gesetzt wird
            string pathOfBindingForItmSrc = ""; // Für das Binding der ItemsSource, die das Source-Objekt das auf die Source-Eigenschaft gesetzt wird
            Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(_ACURLOfPropertyForItemsSource, ref _ItemsSourceACTypeInfo, ref sourceOfBindingForItmSrc, ref pathOfBindingForItmSrc, ref dsRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00004", "VBDataGrid", _ACURLOfPropertyForItemsSource + " " + VBContent);
                //this.Root().Messages.Error(ContextACObject, "Error00004", "VBDataGrid", _ACURLOfPropertyForItemsSource, VBContent);
                return;
            }

            if (!String.IsNullOrEmpty(VBAccess))
                acAccess = VBAccess;
            // Falls kein Access (IAccess) im BSO definiert ist, dann die globale ACQueryDefinition verwenden
            if (string.IsNullOrEmpty(acAccess))
            {
                ACQueryDefinition queryDef = this.Root().Queries.CreateQueryByClass(null, _ItemsSourceACTypeInfo.ValueTypeACClass.PrimaryNavigationquery(), _ItemsSourceACTypeInfo.ValueTypeACClass.ACIdentifier, true);
                if (queryDef != null)
                {
                    IACObjectWithInit initObject = ContextACObject as IACObjectWithInit;
                    if (initObject == null)
                        initObject = BSOACComponent;
                    bool isObjectSet = ((pathOfBindingForItmSrc.StartsWith("Database.") || (sourceOfBindingForItmSrc != null && sourceOfBindingForItmSrc is IACEntityObjectContext))
                                        && (_ItemsSourceACTypeInfo is ACClassProperty && (_ItemsSourceACTypeInfo as ACClassProperty).GenericType == typeof(System.Data.Objects.ObjectSet<>).FullName));
                    if (initObject != null && isObjectSet)
                        _ACAccess = queryDef.NewAccess("VBDataGrid", initObject);
                    else
                        _ACQueryDefinition = queryDef;
                }
            }
            // ansonsten ACQueryDefinition aus BSO verwenden
            else
            {
                _ACAccess = ContextACObject.ACUrlCommand(acAccess) as IAccess;
            }

            UpdateColumns();

            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBDataGrid.ACCompInitStateProperty, binding);
            }
            if (ContextACObject is IACComponent)
            {
                Binding binding = new Binding();
                binding.Source = ContextACObject;
                binding.Path = new PropertyPath(Const.ACUrlCmdMessage);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBDataGrid.ACUrlCmdMessageProperty, binding);
            }

            // dsSource = Reference auf BSOCompany-Instanz
            // dsPath = "AddressList"
            // Liefert gefiltertes (ACQueryDefinition.ACFilterColumns) und 
            // sortiertes (ACQueryDefinition.ACSortColumns) Ergebnis: 
            // IEnumerable list = BSOCompany.AddressList.ACSelect(ACQueryDefinition);

            if (pathOfBindingForItmSrc.StartsWith(Const.ContextDatabase + "."))
            {
                //Type t = dsSource.GetType();
                //PropertyInfo pi = t.GetProperty(Const.ContextDatabase);

                //var lastPath = dsPath.Substring(9);
                //var database = pi.GetValue(dsSource, null) as IACObject;
                //var result = database.ACSelect(ACQueryDefinition, lastPath);
                //this.ItemsSource = result;
                if (NavSearchOnACAccess())
                {
                    Binding binding = new Binding();
                    binding.Source = ACAccess;
                    binding.Path = new PropertyPath("NavObjectList");
                    binding.Mode = BindingMode.OneWay;
                    SetBinding(DataGrid.ItemsSourceProperty, binding);
                }
            }
            else
            {
                // 1. Binding ItemsSource-Property:
                // Listenbereich vom Datagrid füllen 
                Binding bindingItemSource = new Binding();
                bindingItemSource.Source = sourceOfBindingForItmSrc;
                if (!string.IsNullOrEmpty(pathOfBindingForItmSrc))
                {
                    bindingItemSource.Path = new PropertyPath(pathOfBindingForItmSrc);
                }
                bindingItemSource.NotifyOnSourceUpdated = true;
                bindingItemSource.NotifyOnTargetUpdated = true;
                bindingItemSource.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                SetBinding(DataGrid.ItemsSourceProperty, bindingItemSource);
                //ICollectionView xTest = CollectionViewSource.GetDefaultView(this.ItemsSource);
            }


            // 2. Binding SelectedItem-Property:
            if (VBContentPropertyInfo != null)
            {
                if (!VBContentPropertyInfo.IsEnumerable)
                {
                    Binding binding2 = new Binding();
                    binding2.Source = sourceOfBindingForSelItm;
                    binding2.Path = new PropertyPath(pathOfBindingForSelItm);
                    binding2.Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay;
                    binding2.NotifyOnSourceUpdated = true;
                    binding2.NotifyOnTargetUpdated = true;
                    SetBinding(DataGrid.SelectedItemProperty, binding2);
                }
                else
                {
                    this.CanSelectMultipleItems = true;
                }
            }
            LoadDataGridConfig();

            UpdateControlMode();

            MouseDoubleClick += new MouseButtonEventHandler(VBDataGrid_MouseDoubleClick);
            // TODO: Workaround. Momentan wird der Wert beim ersten Mal nicht richtig angezeigt
            // Passiert wohl hauptsächlich bei Untertabpages
            //UpdateDataContent();
            if (AutoFocus)
            {
                Focus();
                //MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            // @aagincic: disabled becouse all logic every time handles Selected item - in conflict with custom logic executed before grid init
            //if (SelectedItem == null && Items.Count > 0)
            //{
            //    SelectedItem = Items[0];
            //}
            //SetSelectedItemToValue(SelectedItem, true);

            if (IsSumEnabled)
                CreateSumProperties();
            else if (!IsSumEnabled && !string.IsNullOrEmpty(VBSumColumns))
            {
                IsSumEnabled = true;
                CreateSumProperties();
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
            Loaded -= VBDataGrid_Loaded;
            Unloaded -= VBDataGrid_Unloaded;

            _PropertyInfoOfACPropertySelected = null;
            _ACAccess = null;

            foreach (DataGridColumn column in this.Columns)
            {
                IGriColumn iCol = column as IGriColumn;
                if (iCol != null)
                {
                    iCol.DeInitVBControl(bso);
                }
                else
                    BindingOperations.ClearAllBindings(column);
            }

            BindingOperations.ClearBinding(this, DataGrid.SelectedItemProperty);
            BindingOperations.ClearBinding(this, DataGrid.ItemsSourceProperty);
            BindingOperations.ClearBinding(this, VBDataGrid.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBDataGrid.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);
            this.ItemsSource = null;
        }

        internal bool ResolveColumnItem(ACColumnItem dataShowColumn, out IACType dsColACTypeInfo, ref object dsColSource, ref string dsColPath, ref Global.ControlModes dsColRightControlMode, ref bool isShowColumnAMethod)
        {
            dsColACTypeInfo = ItemsSourceACTypeInfo;
            //object dsColSource = null;
            //string dsColPath = "";
            //Global.ControlModes dsColRightControlMode = Global.ControlModes.Hidden;
            isShowColumnAMethod = dataShowColumn.PropertyName.StartsWith("!");
            if (isShowColumnAMethod)
            {
                if (!ContextACObject.ACUrlBinding(dataShowColumn.PropertyName, ref dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
                {
                    this.Root().Messages.LogDebug("Error00005", "VBDataGrid", dataShowColumn.PropertyName + " " + VBContent);
                    //this.Root().Messages.Error(ContextACObject, "Error00005", "VBDataGrid", dataShowColumn.PropertyName, VBContent);
                    return false;
                }
            }
            else if (!ItemsSourceACTypeInfo.ACUrlBinding(dataShowColumn.PropertyName, ref dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00005", "VBDataGrid", dataShowColumn.PropertyName + " " + VBContent);
                //this.Root().Messages.Error(ContextACObject, "Error00005", "VBDataGrid", dataShowColumn.PropertyName, VBContent);
                return false;
            }
            return true;
        }

        private Nullable<bool> ColumnsSetInXAML;
        void UpdateColumns()
        {
            if (!ColumnsSetInXAML.HasValue)
            {
                if (_ColumnsFromXAML.Count > 0)
                    ColumnsSetInXAML = true;
                else
                    ColumnsSetInXAML = false;
            }

            if (ColumnsSetInXAML == true)
            {
                foreach (DataGridColumn col in Columns.ToList())
                {
                    if (!_ColumnsFromXAML.Contains(col))
                        Columns.Remove(col);
                }
            }
            else
                Columns.Clear();

            bool generateVBColumns = true;
            if (String.IsNullOrEmpty(VBShowColumns) && (ColumnsSetInXAML == true))
                generateVBColumns = false;

            List<ACColumnItem> vbShowColumns = null;
            if (ACQueryDefinition != null)
                vbShowColumns = ACQueryDefinition.GetACColumns(this.VBShowColumns);
            else
                vbShowColumns = ACQueryDefinition.BuildACColumnsFromVBSource(this.VBShowColumns);
            if (vbShowColumns == null)
            {
                this.Root().Messages.LogDebug("Error00005", "VBDataGrid", VBShowColumns + " " + VBContent);
                //this.Root().Messages.Error(ContextACObject, "Error00005", "VBDataGrid", VBShowColumns, VBContent);
                if (ColumnsSetInXAML == false)
                    AutoGenerateColumns = true;
            }
            else if (generateVBColumns == true)
            {
                foreach (ACColumnItem dataShowColumn in vbShowColumns)
                {
                    IACType dsColACTypeInfo;
                    object dsColSource = null;
                    string dsColPath = "";
                    Global.ControlModes dsColRightControlMode = Global.ControlModes.Hidden;
                    bool isShowColumnAMethod = false;
                    if (!ResolveColumnItem(dataShowColumn, out dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode, ref isShowColumnAMethod))
                        continue;

                    if (dsColRightControlMode == Global.ControlModes.Hidden)
                        continue;

                    bool isDisabledCol = IsSetInVBDisabledColumns(dataShowColumn.PropertyName);
                    if (!isDisabledCol)
                        isDisabledCol = DisabledModes == "Disabled" || DisabledModes == "disabled";

                    if (!SimpleCellElements
                        && dsColACTypeInfo.ObjectType == typeof(bool))
                    {
                        VBDataGridCheckBoxColumn dataGridCheckBoxColumn = new VBDataGridCheckBoxColumn();
                        Columns.Add(dataGridCheckBoxColumn);
                        dataGridCheckBoxColumn.Initialize(dataShowColumn, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode);
                    }
                    else if (dsColACTypeInfo.ObjectType == typeof(DateTime))
                    {
                        VBDataGridDateTimeColumn dataGridDateTimeColumn = new VBDataGridDateTimeColumn();
                        Columns.Add(dataGridDateTimeColumn);
                        dataGridDateTimeColumn.Initialize(dataShowColumn, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode);
                    }
                    else
                    {
                        string acAccessColumn = "";
                        string dataSourceColumn;
                        ACClassProperty sourceProperty = null;
                        if (isShowColumnAMethod)
                            dataSourceColumn = dataShowColumn.PropertyName;
                        else
                        {
                            // Finde heraus ob Spalte, eine Liste ist (z.B. Enum)
                            dataSourceColumn = dsColACTypeInfo is ACClassProperty ? (dsColACTypeInfo as ACClassProperty).GetACSource("", out acAccessColumn, out sourceProperty) : "";
                        }

                        // Spalte ist keine Liste
                        if (string.IsNullOrEmpty(dataSourceColumn))
                        {
                            VBDataGridTextColumn dataGridTextColumn = new VBDataGridTextColumn();
                            Columns.Add(dataGridTextColumn);
                            dataGridTextColumn.Initialize(dataShowColumn, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode, isShowColumnAMethod);
                            dataGridTextColumn.VBContent = dsColPath;
                        }
                        // Sonst ist Spalte eine Liste
                        else
                        {
                            if (isShowColumnAMethod /*|| isDisabledCol*/)
                            {
                                VBDataGridTextColumn dataGridTextColumn = new VBDataGridTextColumn();
                                Columns.Add(dataGridTextColumn);
                                dataGridTextColumn.Initialize(dataShowColumn, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode, isShowColumnAMethod);
                            }
                            else
                            {
                                VBDataGridComboBoxColumn dataGridComboColumn = new VBDataGridComboBoxColumn();
                                Columns.Add(dataGridComboColumn);
                                dataGridComboColumn.Initialize(dataShowColumn, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode, isShowColumnAMethod);
                            }
                        }
                    }
                }
            }

            foreach (DataGridColumn col in _ColumnsFromXAML)
            {
                IGriColumn vbDataGridCol = col as IGriColumn;
                if (vbDataGridCol == null)
                    continue;
                // Falls kein VBContent gesetz oder DataGrid-Columne bereits initialisiert, dann gehe zur nächsten Spalte
                if (String.IsNullOrEmpty(vbDataGridCol.VBContent) || (vbDataGridCol.ACColumnItem != null))
                    continue;
                vbDataGridCol.ACColumnItem = new ACColumnItem(vbDataGridCol.VBContent);
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

        /// <summary>
        /// Handles the ACUrl message when it is received.
        /// </summary>
        public void OnACUrlMessageReceived()
        {
            if (ACUrlCmdMessage != null
                && (ACUrlCmdMessage.ACUrl == Const.CmdUpdateControlMode || ACUrlCmdMessage.ACUrl == Const.CmdUpdateVBContent)
                && !String.IsNullOrEmpty(this.VBContent)
                && !String.IsNullOrEmpty(ACUrlCmdMessage.TargetVBContent)
                && (this.VBContent == ACUrlCmdMessage.TargetVBContent
                   || this.VBContent.Contains(ACUrlCmdMessage.TargetVBContent)))
            {
                if (ACUrlCmdMessage.ACUrl == Const.CmdUpdateControlMode)
                    UpdateControlMode();
                else if (ACUrlCmdMessage.ACUrl == Const.CmdUpdateVBContent)
                    UpdateColumns();
            }
        }
        #endregion

        #region Event Hanndling
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
            if (DisableContextMenu)
            {
                e.Handled = true;
                return;
            }
            if (ContextACObject == null)
            {
                base.OnMouseRightButtonDown(e);
                return;
            }

            DataGridCell cell = null;
            // Wähle Zeile aus wenn rechte Maustaste geklickt wird
            if (SelectionUnit == DataGridSelectionUnit.FullRow)
            {
                DependencyObject dep = (DependencyObject)e.OriginalSource;

                while ((dep != null) && !(dep is DataGridCell) && !(dep is DataGridColumnHeader))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                if (dep == null)
                    return;

                if (dep is DataGridCell)
                {
                    cell = dep as DataGridCell;
                    while ((dep != null) && !(dep is DataGridRow))
                    {
                        dep = VisualTreeHelper.GetParent(dep);
                    }
                    DataGridRow row = dep as DataGridRow;
                    if (row != null)
                        this.SelectedIndex = FindRowIndex(row);
                }
            }

            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Handled = true;
                base.OnMouseRightButtonDown(e);
                return;
            }

            string vbContent = null;
            UpdateACContentList(GetNearestContainer(uiElement, ref vbContent), vbContent);
            if (cell != null)
            {
                UpdateACContentList(cell, vbContent);
            }
            //ACElementHelper.UpdateACElement(this);


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

        /// <summary>
        /// Handles the OnKeyDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            if (dep is DataGridCell)
            {
                DataGridCell cell = dep as DataGridCell;
                if (cell.Column is VBDataGridDateTimeColumn)
                {
                    (cell.Column as VBDataGridDateTimeColumn).OnInput(e);
                }
            }
        }

        //protected override void OnKeyUp(KeyEventArgs e)
        //{
        //    if (e.Key == Key.F3)
        //    {
        //        Filter();
        //    }
        //    base.OnKeyUp(e);
        //}

        private int FindRowIndex(DataGridRow row)
        {
            DataGrid dataGrid =
                ItemsControl.ItemsControlFromItemContainer(row)
                as DataGrid;

            int index = dataGrid.ItemContainerGenerator.
                IndexFromContainer(row);

            return index;
        }

        /// <summary>
        /// Handles the OnSelectedCellsChanged event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
        {
            base.OnSelectedCellsChanged(e);
        }

        /// <summary>
        /// Handles the OnRowEditEnding event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnRowEditEnding(DataGridRowEditEndingEventArgs e)
        {
            base.OnRowEditEnding(e);
        }

        /// <summary>
        /// Handles the OnSelectionChanged event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (BSOACComponent == null || _CopyToClipboard)
            {
                base.OnSelectionChanged(e);
                return;
            }

            if ((ItemsSourceACTypeInfo != null) && this.CanSelectMultipleItems)
            {
                Type genericType = Type.GetType("System.Collections.Generic.List`1");
                Type listType = genericType.MakeGenericType(new Type[] { ItemsSourceACTypeInfo.ObjectType });
                IList list = Activator.CreateInstance(listType) as IList;
                foreach (var item in SelectedItems)
                {
                    list.Add(item);
                }
                BSOACComponent.ACUrlCommand(VBContent, list);

            }
            // 1. Wenn "Enabled", dann automatisch auch Current... bestücken
            // 2. Wenn "AutoLoad", dann auch automatisch auch Current... bestücken
            // Wenn AutoLoad = True, dann den Datensatz automatisch laden, 
            // TODO: Bei Kombination mit CanUserAddRows=True, taucht Exception auf.
            // TODO: Austomatisches Anlegen von Zeilen muss noch implementiert werden
            if ((_PropertyInfoOfACPropertySelected != null) && (Enabled || AutoLoad))
            {
                var query = BSOACComponent.ACClassMethods.Where(c => c.InteractionVBContent == VBContent && c.SortIndex == (short)MISort.Load);
                if (query.Any())
                {
                    BSOACComponent.ACUrlCommand("!" + query.First().ACIdentifier);
                }
                else if (VBContentPropertyInfo != null)
                {
                    string dataCurrent = VBContentPropertyInfo.GetACCurrent(DataCurrent);
                    if (!string.IsNullOrEmpty(dataCurrent))
                    {
                        BSOACComponent.ACUrlCommand(dataCurrent, SelectedItem);
                    }
                }
            }
            base.OnSelectionChanged(e);
        }

        /// <summary>
        /// Handles the OnExecutedBeginEdit event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnExecutedBeginEdit(ExecutedRoutedEventArgs e)
        {
            try
            {
                base.OnExecutedBeginEdit(e);
            }
            catch (InvalidOperationException ex)
            {
                // wird von System.Windows.Controls.ItemCollection.EditItem wenn Cell editiert werden soll aber sich hinter dem gebunden IEnumerable keine Liste verbirgt sondern
                // 
                this.Root().Messages.LogDebug("Error00006", "VBDataGrid", ex.Message);
            }
        }

        /// <summary>
        /// Handles the OnCellEditEnding event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnCellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            base.OnCellEditEnding(e);
            //DataRowView rowView = e.Row.Item as DataRowView;
            //rowBeingEdited = rowView;
        }

        /// <summary>
        /// Handles the OnCurrentCellChanged event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnCurrentCellChanged(EventArgs e)
        {
            base.OnCurrentCellChanged(e);
        }

        /// <summary>
        /// Handles the OnPreparingCellForEdit event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreparingCellForEdit(DataGridPreparingCellForEditEventArgs e)
        {
            //if (BSOACComponent == null)
            //{
            //    base.OnPreparingCellForEdit(e);
            //    return;
            //}
            //bool isEnabled = true;
            //if (e.Column is System.Windows.Controls.DataGridBoundColumn)
            //{
            //    string column = ((System.Windows.Data.Binding)(((System.Windows.Controls.DataGridBoundColumn)(e.Column)).Binding)).Path.Path;
            //    // TODO:
            //    //isEnabled = BSOACComponent.GetControlModes(_ACURLOfPropertyForItemsSource + "." + column, DisabledModes) >= Global.ControlModes.Enabled;
            //    isEnabled = true;
            //}
            //else
            //{
            //    isEnabled = true;
            //}
            base.OnPreparingCellForEdit(e);
        }

        void VBDataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (_CopyToClipboard)
                return;
            if (sender is VBDataGrid)
            {
                VBDataGrid vbDataGrid = sender as VBDataGrid;

                try
                {
                    // Falls keine ObservableCollection, dann aktualisiere ITems
                    if (vbDataGrid.ItemsSource != null && !(vbDataGrid.ItemsSource is System.Collections.Specialized.INotifyCollectionChanged || vbDataGrid.ItemsSource is IRaiseItemChangedEvents))
                        vbDataGrid.Items.Refresh();
                }
                catch (Exception)
                {
                }
            }
            //if (e.Property == DataGrid.ItemsSourceProperty)
            //{
            //    // @aagincic: disabled becouse all logic every time handles Selected item - in conflict with custom logic executed before grid init
            //    //if (SelectedItem == null && Items.Count > 0)
            //    //{
            //    //    SelectedItem = Items[0];
            //    //}
            //}
            if (IsSumEnabled && SumVisibility == System.Windows.Visibility.Visible)
                CalculateSum();
        }

        void VBDataGrid_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            if (e.Property == DataGrid.SelectedItemProperty)
            {
                if (IsSumEnabled && SumVisibility == System.Windows.Visibility.Visible)
                    CalculateSum();
            }
        }

        void VBDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ContextACObject == null)
                return;
            if (DblClick != null && DblClick != "")
            {
                ContextACObject.ACUrlCommand(DblClick);
            }
        }

        /// <summary>
        /// The event handler for PreviewTextInput.
        /// </summary>
        /// <param name="e">The TextComposition event arugmnets.</param>
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
        }

        /// <summary>
        /// The event hanlder for TextInput.
        /// </summary>
        /// <param name="e">The TextComposition event arugmnets.</param>
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
        }

        #endregion

        #region IDataField Members

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBDataGrid));


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
            get { return (string)GetValue(VBContentProperty); }
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
        /// this property is not necessary to be setted. Only if you want to use different list instead, you must set this property to name of that list which must be marked with ACPropertyList attribute.
        /// </summary>
        /// <summary xml:lang="de">
        /// Name der List-Eigenschaft des BSO's, das mit dem Attribut [ACPropertyList(..,..)] gekennzeichnet ist.
        /// Diese Eigenschaft muss nicht gesetzt werden, da Sie über die VBContent-Eigenschaft und der zusammenghörenden ACGroup bei der VBDataGrid-Initialisierung automatisch ermittelt werden kann
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
                _Caption = "";
            }
        }

        /// <summary>
        /// Represents the dependency property for VBAccess.
        /// </summary>
        public static readonly DependencyProperty VBAccessProperty
            = DependencyProperty.Register("VBAccess", typeof(string), typeof(VBDataGrid));

        /// <summary>
        /// Represents the property in which you enter the name of BSO's list property marked with [ACPropertyList(...)] attribute. In usage with ACPropertySelected with same ACGroup,
        /// this property is not necessary to be setted. Only if you want to use different list instead, you must set this property to name of that list which must be marked with ACPropertyList attribute.
        /// </summary>
        [Category("VBControl")]
        public string VBAccess
        {
            get { return (string)GetValue(VBAccessProperty); }
            set { SetValue(VBAccessProperty, value); }
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
                _DisabledColumnList = value.Split(',').ToList();

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
        /// Determines is enabled auto load or not.
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
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBDataGrid), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly DependencyProperty ACUrlCmdMessageProperty =
            DependencyProperty.Register("ACUrlCmdMessage",
                typeof(ACUrlCmdMessage), typeof(VBDataGrid),
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
                typeof(ACInitState), typeof(VBDataGrid),
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
            VBDataGrid thisControl = dependencyObject as VBDataGrid;
            if (thisControl == null)
                return;
            if (args.Property == ACUrlCmdMessageProperty)
                thisControl.OnACUrlMessageReceived();
            else if (args.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (args.Property == BSOACComponentProperty)
            {
                if (args.NewValue == null && args.OldValue != null)
                {
                    IACBSO bso = args.OldValue as IACBSO;
                    if (bso != null)
                        thisControl.DeInitVBControl(bso);
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

        private void UpdateACContentList(Control control, string vbContent)
        {
            if (control is DataGridCellsPresenter)
            {
                IACObject acObject = (control as DataGridCellsPresenter).Item as IACObject;
                UpdateACContentList(acObject, vbContent);
            }
            else if (control is DataGridColumnHeader)
            {
                _ACContentList.Clear();
                var currentHeader = control as DataGridColumnHeader;
                if (currentHeader.Column is IGriColumn)
                {
                    _ACContentList.Add(((IGriColumn)(currentHeader.Column)).ACColumnItem);
                }
            }
            else if (control is DataGridCell)
            {
                DataGridCell cell = control as DataGridCell;
                if (cell.Column is IGriColumn)
                {
                    _ACContentList.Add(((IGriColumn)(cell.Column)).ACColumnItem);
                }
            }
        }

        private void UpdateACContentList(IACObject acObject, string vbContent)
        {
            _ACContentList.Clear();

            // TODO Norbert:
            if (acObject != null)
            {
                if (!string.IsNullOrEmpty(vbContent))
                {
                    var content = acObject.ACUrlCommand(vbContent, null);
                    if (content != null)
                    {
                        if (content is IACObject)
                        {
                            _ACContentList.Add(content as IACObject);
                        }
                        else
                        {
                            IACType dsACTypeInfo = this._PropertyInfoOfACPropertySelected;
                            object dsSource = null;
                            string dsPath = "";
                            Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;

                            if (acObject.ACUrlBinding(vbContent, ref dsACTypeInfo, ref dsSource, ref dsPath, ref dsRightControlMode))
                            {

                                ACValueItem acTypedValue = new ACValueItem("", content, dsACTypeInfo);
                                _ACContentList.Add(acTypedValue);
                            }
                        }
                    }
                }
                _ACContentList.Add(acObject);
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
            if (ContextACObject == null)
                return;
            BSOACComponent.ACActionToTarget(this, actionArgs);
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

            if (ContextACObject == null)
                return false;
            return BSOACComponent.IsEnabledACActionToTarget(this, actionArgs);
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
        /// Represents the dependency property for VBValidation.
        /// </summary>
        public static readonly DependencyProperty VBValidationProperty = ContentPropertyHandler.VBValidationProperty.AddOwner(typeof(VBDataGrid), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Name of the VBValidation property.
        /// </summary>
        /// <summary xml:lang="de">
        /// Name der Eigenschaft VBValidation.
        /// </summary>
        [Category("VBControl")]
        public string VBValidation
        {
            get { return (string)GetValue(VBValidationProperty); }
            set { SetValue(VBValidationProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for DisableContextMenu.
        /// </summary>
        public static readonly DependencyProperty DisableContextMenuProperty = ContentPropertyHandler.DisableContextMenuProperty.AddOwner(typeof(VBDataGrid), new FrameworkPropertyMetadata((bool)false, FrameworkPropertyMetadataOptions.Inherits));
        /// <summary>
        /// Determines is context menu disabled or enabled.
        /// </summary>
        /// <summary xml:lang="de">
        /// Ermittelt ist das Kontextmenü deaktiviert oder aktiviert.
        /// </summary>
        [Category("VBControl")]
        [ACPropertyInfo(9999)]
        public bool DisableContextMenu
        {
            get { return (bool)GetValue(DisableContextMenuProperty); }
            set { SetValue(DisableContextMenuProperty, value); }
        }

        private bool Visible
        {
            get
            {
                return Visibility == System.Windows.Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    if (RightControlMode > Global.ControlModes.Hidden)
                    {
                        Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    Visibility = Visibility.Hidden;
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

        /// <summary>
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
            if (Enabled)
            {
                if (_PropertyInfoOfACPropertySelected is ACClassProperty)
                {
                    if ((_PropertyInfoOfACPropertySelected as ACClassProperty).IsNullable)
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
        /// Represents the dependency property for ACCaptionTrans.
        /// </summary>
        public static readonly DependencyProperty DisabledModesProperty
            = DependencyProperty.Register("DisabledModes", typeof(string), typeof(VBDataGrid));
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

        #region private methods
        internal bool IsSetInVBDisabledColumns(string column)
        {
            // Wenn Grid als ganzes nicht Editierbar ist
            //if (!_Enabled)
            //return false;
            // Wenn die DataEnabledColumns nicht explizit angegeben wurden, dann sind alle Enabled
            if (string.IsNullOrEmpty(VBDisabledColumns))
                return false;
            return _DisabledColumnList.Contains(column);
        }

        /// <summary>
        /// Finds the parent of the type defined in parameter type.
        /// </summary>
        /// <param name="forObject">The forObject paramter.</param>
        /// <param name="type">The type parameter.</param>
        /// <returns></returns>
        public static DependencyObject FindParentOfType(DependencyObject forObject, Type type)
        {
            if (forObject == null || type == null)
                return null;
            for (DependencyObject curObj = VisualTreeHelper.GetParent(forObject);
                curObj != null; curObj = VisualTreeHelper.GetParent(curObj))
            {
                if (type.IsAssignableFrom(curObj.GetType()))
                    return curObj;
            }
            return null;
        }

        #endregion

        #region DragAndDrop
        /// <summary>
        /// Gets or sets DragEnabled.
        /// </summary>
        public DragMode DragEnabled { get; set; }

        /// <summary>
        /// Handles the OnMouseLeftButtonDown event.
        /// </summary>
        /// <param name="e">The MouseButtonEvent arguments.</param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (DragEnabled == DragMode.Enabled)
            {
                object dragItem = SelectedItem;
                if (dragItem is IACObject)
                {
                    string vbContent = null;
                    UpdateACContentList(dragItem as IACObject, vbContent);
                    VBDragDrop.VBDoDragDrop(this);
                }
            }
            else if (IsEnabledMoveRows)
            {
                var row = InputHitTest(e.GetPosition(this)) as DataGridRow;
                _DraggedItem = row?.Item;
            }
            base.OnMouseLeftButtonDown(e);
        }

        /// <summary>
        /// Handles the OnDragEnter event.
        /// </summary>
        /// <param name="e">The DragEvent arguments.</param>
        protected override void OnDragEnter(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                base.OnDragEnter(e);
                return;
            }
#if DEBUG
            System.Diagnostics.Debug.WriteLine(e.OriginalSource.ToString()); // as UIElement
#endif
            //HandleDragOver(e.Source, 0, 0, e);
            HandleDragOver(this, 0, 0, e);
            base.OnDragEnter(e);
        }

        /// <summary>
        /// Handles the OnDragLeave event.
        /// </summary>
        /// <param name="e">The DragEvents arguments.</param>
        protected override void OnDragLeave(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                base.OnDragLeave(e);
                return;
            }
            //HandleDragOver(e.Source, 0, 0, e);
            HandleDragOver(this, 0, 0, e);
            base.OnDragLeave(e);
        }

        /// <summary>
        /// Handles the OnDragOver event.
        /// </summary>
        /// <param name="e">The DragEvent arguments.</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                base.OnDragOver(e);
                return;
            }
            //HandleDragOver(e.Source, 0, 0, e);
            HandleDragOver(this, 0, 0, e);
            base.OnDragOver(e);
        }

        /// <summary>
        /// Handles the OnDrop event.
        /// </summary>
        /// <param name="e">The DragEvent arguments.</param>
        protected override void OnDrop(DragEventArgs e)
        {
            if (this.GetVBDesign().IsDesignerActive)
            {
                base.OnDrop(e);
                return;
            }
            //HandleDrop(e.Source, 0, 0, e);
            HandleDrop(this, 0, 0, e);
            base.OnDrop(e);
        }

        /// <summary>
        /// Handles the DragOver event.
        /// </summary>
        /// <param name="sender">The sender paramter.</param>
        /// <param name="x">The x-cordinate paramter.</param>
        /// <param name="y">The y-cordinate paramter.</param>
        /// <param name="e">The DragEvents arguments.</param>
        public void HandleDragOver(object sender, double x, double y, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            switch (e.AllowedEffects)
            {
                case DragDropEffects.Move: // Vorhandene Elemente verschieben
                    HandleDragOver_Move(sender, x, y, e);
                    return;
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move:
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    HandleDragOver_Copy(sender, x, y, e);
                    return;
                default:
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
            }
        }

        private void HandleDragOver_Move(object sender, double x, double y, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            string vbContent = null;
            UpdateACContentList(GetNearestContainer(uiElement, ref vbContent), vbContent);

            // Wenn alle generellen Vorbedingungen erfüllt sind, dann wird
            // noch das BSO gefragt, ob das verschieben erlaubt ist
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);

            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
            if (IsEnabledACAction(actionArgs))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void HandleDragOver_Copy(object sender, double x, double y, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            string vbContent = null;
            UpdateACContentList(GetNearestContainer(uiElement, ref vbContent), vbContent);

            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Wenn alle generellen Vorbedingungen erfüllt sind, dann wird
            // noch das BSO gefragt, ob das kopieren (einfügen) erlaubt ist
            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
            if (IsEnabledACAction(actionArgs))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handles the Drop event.
        /// </summary>
        /// <param name="sender">The sender parameter.</param>
        /// <param name="x">The x-cordinate paramter.</param>
        /// <param name="y">The y-cordinate paramter.</param>
        /// <param name="e">The DragEvents arguments.</param>
        public void HandleDrop(object sender, double x, double y, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            string vbContent = null;
            UpdateACContentList(GetNearestContainer(uiElement, ref vbContent), vbContent);
            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            switch (e.AllowedEffects)
            {
                case DragDropEffects.Move: // Vorhandene Elemente verschieben
                    {
                        if (e.KeyStates != DragDropKeyStates.ControlKey)
                        {
                            e.Effects = DragDropEffects.None;
                            return;
                        }
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Move);
                        ACAction(actionArgs);
                        e.Handled = true;
                        return;
                    }
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    {
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Drop);
                        ACAction(actionArgs);
                        e.Handled = true;
                    }
                    return;
                default:
                    e.Effects = DragDropEffects.None;
                    return;
            }
        }

        private Control GetNearestContainer(UIElement element, ref string vbContent)
        {
            if (element == null)
                return null;

            if (element is DataGridCellsPresenter)
            {
                DataGridCellsPresenter presenter = element as DataGridCellsPresenter;
                return presenter;
            }
            else //if (element is Border)
            {
                int count = 0;
                while (element != null && count < 10)
                {
                    if (vbContent == null)
                    {
                        if (element is TextBox)
                        {
                            TextBox textBox = element as TextBox;
                            BindingExpression be = textBox.GetBindingExpression(TextBox.TextProperty);
                            vbContent = be.ParentBinding.Path.Path;
                        }
                        if (element is ContentPresenter)
                        {
                            ContentPresenter contentPresenter = element as ContentPresenter;
                            if (contentPresenter.Content is VBCheckBox)
                            {
                                VBCheckBox vbCheckbox = contentPresenter.Content as VBCheckBox;
                                DataGridBoundColumn dataGridColumn = ((System.Windows.Controls.DataGridCell)(vbCheckbox.Parent)).Column as DataGridBoundColumn;
                                vbContent = ((System.Windows.Data.Binding)(dataGridColumn.Binding)).Path.Path;
                            }
                        }
                        if (element is TextBlock)
                        {
                            TextBlock textBlock = element as TextBlock;
                            BindingExpression be = textBlock.GetBindingExpression(TextBlock.TextProperty);
                            if (be != null)
                            {
                                vbContent = be.ParentBinding.Path.Path;
                            }
                            else
                            {
                                vbContent = textBlock.Text;
                            }
                        }
                        if (element is ComboBox)
                        {
                            ComboBox comboBox = element as ComboBox;
                            BindingExpression be = comboBox.GetBindingExpression(ComboBox.SelectedItemProperty);
                            if (be != null)
                            {
                                vbContent = be.ParentBinding.Path.Path;
                            }
                            else
                            {
                                BindingExpression be2 = comboBox.GetBindingExpression(ComboBox.SelectedValueProperty);
                                vbContent = be2.ParentBinding.Path.Path;
                            }
                        }
                    }
                    element = VisualTreeHelper.GetParent(element) as UIElement;
                    if ((element != null) && (element is DataGridCellsPresenter || element is DataGridColumnHeader))
                        return element as Control;
                    count++;
                }
            }
            return null;
        }
        #endregion

        #region MovableRows(DragAndDrop)

        public bool IsEnabledMoveRows
        {
            get { return (bool)GetValue(IsEnabledMoveRowsProperty); }
            set { SetValue(IsEnabledMoveRowsProperty, value); }
        }

        public static readonly DependencyProperty IsEnabledMoveRowsProperty =
            DependencyProperty.Register("IsEnabledMoveRows", typeof(bool), typeof(VBDataGrid), new PropertyMetadata(false));

        private object _DraggedItem = null;

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (IsEnabledMoveRows)
            {
                var rowItem = InputHitTest(e.GetPosition(this));
                var row = Helperclasses.VBVisualTreeHelper.FindParentObjectInVisualTree(rowItem as DependencyObject, typeof(DataGridRow)) as DataGridRow;
                _DraggedItem = row?.Item;
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (_DraggedItem == null) return;
            var rowItem = InputHitTest(e.GetPosition(this));
            var row = Helperclasses.VBVisualTreeHelper.FindParentObjectInVisualTree(rowItem as DependencyObject, typeof(DataGridRow)) as DataGridRow;
            if (row == null || row.IsEditing) return;
            ExchangeItems(row.Item);

            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_DraggedItem == null) return;
            ExchangeItems(SelectedItem);
            //select the dropped item
            SelectedItem = _DraggedItem;
            //reset
            _DraggedItem = null;

            if (!string.IsNullOrEmpty(_VBOnDrag) && (ContextACObject != null))
            {
                string[] vbDragData = _VBOnDrag.Split(',');
                Dictionary<int, string> newOrderInfo = new Dictionary<int, string>();
                int nr = 0;
                foreach (var item in Items)
                {
                    nr++;
                    object itemObject = item.GetValue(vbDragData[1]);
                    if (itemObject == null)
                    {
                        itemObject = item.GetType().GetProperty(vbDragData[1]).GetValue(item);
                    }
                    string itemValue = itemObject.ToString();
                    newOrderInfo.Add(nr, itemValue);
                }
                ContextACObject.ACUrlCommand(vbDragData[0], new object[] { newOrderInfo });
            }

            base.OnPreviewMouseLeftButtonUp(e);
        }

        private void ExchangeItems(object targetItem)
        {
            try
            {
                if (_DraggedItem == null)
                    return;

                foreach (var col in Columns)
                {
                    col.SortDirection = null;
                }

                if (targetItem != null && !ReferenceEquals(_DraggedItem, targetItem))
                {
                    var list = this.ItemsSource as IList;
                    if (list == null)
                        throw new ApplicationException("EnableRowsMoveProperty requires the ItemsSource property of DataGrid to be at least IList inherited collection. Use ObservableCollection to have movements reflected in UI.");
                    //get target index
                    var targetIndex = list.IndexOf(targetItem);
                    //remove the source from the list
                    if (targetIndex >= 0)
                        list.Remove(_DraggedItem);

                    //move source at the target's location
                    if (targetIndex >= 0)
                        list.Insert(targetIndex, _DraggedItem);
                }
            }
            catch (Exception e)
            {
                Database.Root.Messages.LogException(BSOACComponent != null ? BSOACComponent.GetACUrl() : "VBDataGrid", "VBDataGrid.ExchangeItems(10)", e);
            }
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
        /// <param name="acUrl">The acUrl parameter.</param>
        /// <param name="acParameter">The acParameter.</param>
        /// <returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
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
        /// <returns>Returns the list of ACMenu items.</returns>
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
        /// <param name="acMenuItemList">The acMenuItemList parameter.</param>
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
        #endregion

        #region ACMethod

        /// <summary>
        /// Modifies the content in VBDataGrid.
        /// </summary>
        [ACMethodInteraction("", "en{'Modify'}de{'Bearbeiten'}", 100, false)]
        public virtual void Modify()
        {
            if (!ACContentList.Any())
                return;
            ACClass acClass = GetValueTypeACClass();
            if (acClass == null)
                return;

            if (ContextACObject.ACType.ACKind == Global.ACKinds.TACBSO || ContextACObject.ACType.ACKind == Global.ACKinds.TACBSOGlobal)
            {
                string filterColumn = acClass.ACFilterColumns.Split(',').First();
                IACObject value = GetValueTypeACClass(acClass);

                if (value != null && !string.IsNullOrEmpty(filterColumn))
                {
                    if (Database.Root.Environment.License.MayUserDevelop)
                    {
                        if (value is ACClass)   // Falls ACClass, dann im BSOiPlusStudio öffnen
                        {
                            ACMethod acMethod = Database.Root.ACType.ACType.ACUrlACTypeSignature(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio);
                            acMethod.ParameterValueList["AutoLoad"] = (value as ACClass).GetACUrl();

                            this.Root().RootPageWPF.StartBusinessobject(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio, acMethod.ParameterValueList);
                        }
                        else
                        {
                            ACMethod acMethod = Database.Root.ACType.ACType.ACUrlACTypeSignature(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio);
                            acMethod.ParameterValueList["AutoFilter"] = value.ACUrlCommand(filterColumn).ToString();

                            this.Root().RootPageWPF.StartBusinessobject(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + acClass.ManagingBSO.ACIdentifier, acMethod.ParameterValueList);
                        }
                    }
                }
                else
                {
                    this.Root().RootPageWPF.StartBusinessobject(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + acClass.ManagingBSO.ACIdentifier, null);
                }
            }
        }

        /// <summary>
        /// Determines is enabled modify.
        /// </summary>
        /// <returns>Returns true if modify enabled otherwise false.</returns>
        public virtual bool IsEnabledModify()
        {
            if (!ACContentList.Any())
                return false;
            ACClass acClass = GetValueTypeACClass();

            if (acClass == null || acClass.ManagingBSO == null)
                return false;
            if (acClass.ManagingBSO.GetRight(acClass.ManagingBSO) != Global.ControlModes.Enabled)
                return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        [ACMethodInteraction("", "en{'Group'}de{'Gruppieren'}", 900, false)]
        public void Group()
        {
        }

        /// <summary>
        /// Determines is Group enabled or disabled.
        /// </summary>
        /// <returns>Returns true if is Group enabled otherwise false.</returns>
        public bool IsEnabledGroup()
        {
            ACColumnItem acColumnItem = ACContentList.FirstOrDefault() as ACColumnItem;
            if (acColumnItem == null)
                return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        [ACMethodInteraction("", "en{'Ungroup'}de{'Gruppierung aufheben'}", 901, false)]
        public void UnGroup()
        {
        }

        /// <summary>
        /// Determines is UnGroup is enabled or disabled.
        /// </summary>
        /// <returns>Returns true if is UnGroup enabled otherwise false.</returns>
        public bool IsEnabledUnGroup()
        {
            ACColumnItem acColumnItem = ACContentList.FirstOrDefault() as ACColumnItem;
            if (acColumnItem == null)
                return false;

            return true;
        }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        /// <summary xml:lang="de">
        /// Lädt die Konfiguration.
        /// </summary>
        [ACMethodInteraction("", "en{'Load Configuration...'}de{'Konfiguration laden...'}", 902, false)]
        public void LoadConfig()
        {
            if ((bool)BSOACComponent.ACUrlCommand("VBBSOQueryDialog!QueryLoadDlg", new object[] { ACQueryDefinition.RootACQueryDefinition }))
            {
                UpdateColumns();
                LoadDataGridConfig();
            }
        }

        /// <summary>
        /// Determines is LoadConfig enabled or disabled.
        /// </summary>
        /// <returns>Returns true if is LoadConfig enabled otherwise false.</returns>
        public bool IsEnabledLoadConfig()
        {
            if (!ACContentList.Any() || ACQueryDefinition == null)
                return false;
            ACColumnItem acColumnItem = ACContentList.Where(c => c is ACColumnItem).FirstOrDefault() as ACColumnItem;
            if (acColumnItem == null)
                return false;
            return true;
        }

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        /// <summary xml:lang="de">
        /// Speichert die Konfiguration.
        /// </summary>
        [ACMethodInteraction("", "en{'Save Configuration'}de{'Konfiguration speichern'}", 903, false)]
        public void SaveConfig()
        {
            UpdateACQueryDefinition();
            if (ACQueryDefinition.RootACQueryDefinition.SaveConfig(false, Global.ConfigSaveModes.User))
            {
                SaveDataGridConfig();
            }
        }

        /// <summary>
        /// Determines is SaveConfig enabled or disabled.
        /// </summary>
        /// <returns>Returns true if is SaveCofing enabled, otherwise false.</returns>
        public bool IsEnabledSaveConfig()
        {
            if (!ACContentList.Any() || ACQueryDefinition == null)
                return false;
            ACColumnItem acColumnItem = ACContentList.Where(c => c is ACColumnItem).FirstOrDefault() as ACColumnItem;
            if (acColumnItem == null)
                return false;
            return true;
        }

        /// <summary>
        /// Saves the configuration as.
        /// </summary>
        /// <summary xml:lang="de">
        /// Speichert die Konfiguration als.
        /// </summary>
        [ACMethodInteraction("", "en{'Save Configuration as...'}de{'Konfiguration speichern unter...'}", 904, false)]
        public void SaveConfigAs()
        {
            UpdateACQueryDefinition();
            var fileName = BSOACComponent.ACUrlCommand("VBBSOQueryDialog!QuerySaveDlg", new object[] { ACQueryDefinition.RootACQueryDefinition }) as string;
            if (!string.IsNullOrEmpty(fileName))
            {
                SaveDataGridConfig();
            }
        }

        /// <summary>
        /// Determines is SaveConfigAs enabled or disabled.
        /// </summary>
        /// <returns>Returns true is if SaveConfigAs enabled otherwise false.</returns>
        public bool IsEnabledSaveConfigAs()
        {
            if (!ACContentList.Any() || ACQueryDefinition == null)
                return false;
            ACColumnItem acColumnItem = ACContentList.Where(c => c is ACColumnItem).FirstOrDefault() as ACColumnItem;
            if (acColumnItem == null)
                return false;
            return true;
        }

        /// <summary>
        /// Saves DataGrid configuration.
        /// </summary>
        public void SaveDataGridConfig()
        {
            string fileName = GetFileName();
            try
            {
                ColumnSizeList columnSizeList = new ColumnSizeList();
                foreach (var gridColumn in this.Columns.OrderBy(c => c.DisplayIndex))
                {
                    IGriColumn iGridColumn = gridColumn as IGriColumn;
                    if (iGridColumn == null || iGridColumn.ACColumnItem == null)
                        continue;

                    columnSizeList.Add(new ColumnSize { ACIdentifier = (gridColumn as IGriColumn).ACColumnItem.ACIdentifier, Width = gridColumn.ActualWidth });
                }
                DataContractSerializer serializer = new DataContractSerializer(typeof(List<ColumnSize>));

                using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000)) // ACType ist immer vom globalen Datenbankkontext
                {
                    Database database = Database.GlobalDatabase;

                    ACClass typeACClass = ACQueryDefinition.RootACQueryDefinition.TypeACClass as ACClass;
                    var query = typeACClass.ConfigurationEntries.Where(c => c.LocalConfigACUrl == fileName);

                    IACConfig acConfig;
                    if (!query.Any())
                    {
                        acConfig = typeACClass.NewACConfig(null, database.GetACType(typeof(ColumnSizeList)));
                        acConfig.LocalConfigACUrl = fileName;
                        acConfig[Const.Value] = columnSizeList;
                    }
                    else
                    {
                        acConfig = query.First();
                        acConfig[Const.Value] = columnSizeList;
                    }
                    database.ACSaveChanges();
                }
            }
            catch (Exception ex)
            {
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDataGrid", "SaveDataGridConfig", ex.Message);
            }
        }

        /// <summary>
        /// Loads DataGrid configuration.
        /// </summary>
        public void LoadDataGridConfig()
        {
            try
            {
                string fileName = GetFileName();
                if (ACQueryDefinition == null || String.IsNullOrEmpty(fileName))
                    return;

                ACClass typeACClass = ACQueryDefinition.RootACQueryDefinition.TypeACClass as ACClass;
                var query = typeACClass.ConfigurationEntries.Where(c => c.LocalConfigACUrl == fileName);
                if (!query.Any())
                    return;
                var acConfig = query.First();
                ColumnSizeList columnSizeList = acConfig[Const.Value] as ColumnSizeList;
                if (columnSizeList == null)
                    return;

                int gridColumnCount = this.Columns.Count;
                foreach (var gridColumn in this.Columns)
                {
                    IGriColumn iGridColumn = gridColumn as IGriColumn;
                    if (iGridColumn == null || iGridColumn.ACColumnItem == null)
                        continue;
                    ColumnSize colSize = columnSizeList.Where(c => c.ACIdentifier == iGridColumn.ACColumnItem.ACIdentifier).FirstOrDefault();
                    if (colSize == null)
                        continue;
                    gridColumn.Width = colSize.Width;
                    int displayIndex = columnSizeList.IndexOf(colSize);
                    if (gridColumn.DisplayIndex != displayIndex)
                    {
                        var maxIndex = (gridColumnCount == 0) ? 0 : gridColumnCount - 1;
                        gridColumn.DisplayIndex = (displayIndex <= maxIndex) ? displayIndex : maxIndex;
                    }
                }
            }
            catch (Exception ex)
            {
                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDataGrid", "LoadDataGridConfig", ex.Message);
            }
        }

        private string GetFileName()
        {
            if (ACQueryDefinition == null)
                return "";

            string fileName = "VBDataGrid_" + ACQueryDefinition.RootACQueryDefinition.LocalConfigACUrl;

            if (ACQueryDefinition != ACQueryDefinition.RootACQueryDefinition)
            {
                ACQueryDefinition acQueryDefinition = ACQueryDefinition;
                string postFix = "";
                while (acQueryDefinition != ACQueryDefinition.RootACQueryDefinition && acQueryDefinition != null)
                {
                    postFix = "_" + acQueryDefinition.ACIdentifier + postFix;
                    acQueryDefinition = acQueryDefinition.ParentACObject as ACQueryDefinition;
                }
                fileName += postFix;
            }
            return fileName;
        }

        /// <summary>
        /// Updates ACQuery definition.
        /// </summary>
        /// <param name="configName">The configuration name.</param>
        public void UpdateACQueryDefinition(string configName = "")
        {
            int i = 0;
            // ACColumns
            List<ACColumnItem> acColumnItemList = ACQueryDefinition.ACColumns.ToList();
            ACQueryDefinition.ACColumns.Clear();
            foreach (var gridColumn in this.Columns.OrderBy(c => c.DisplayIndex).Select(c => c as IGriColumn))
            {
#if DEBUG

                System.Diagnostics.Debug.WriteLine(gridColumn.VBContent);
#endif
                var query2 = acColumnItemList.Where(c => c.ACIdentifier == gridColumn.ACColumnItem.ACIdentifier);
                if (query2.Any())
                {
                    ACQueryDefinition.ACColumns.Add(query2.First());
                    acColumnItemList.Remove(query2.First());
                }
                else
                {
                    ACColumnItem acColumn = new ACColumnItem(gridColumn.ACColumnItem.ACIdentifier);
                    ACQueryDefinition.ACColumns.Add(acColumn);
                }
                i++;
            }

            bool sortColumnsCleared = false;
            // ACSortColumns
            var query = this.Columns.Where(c => c.SortDirection != null).OrderBy(c => c.DisplayIndex);
            if (query.Any())
            {
                foreach (var gridColumn in this.Columns.Where(c => c.SortDirection != null).OrderBy(c => c.DisplayIndex))
                {
                    IGriColumn iGridColumn = gridColumn as IGriColumn;

                    // Überprüfe ob das eine Eigenschaft ist die in der Datenbank bzw. des EntityObjektes ist, nur dann darf sortiert werden
                    // weil sonst ACQuery.SearchWithEntitySQL eine Abfrage baut auf ein Feld das nicht in der Datenbank existiert
                    if (iGridColumn != null
                        && iGridColumn.ColACType != null
                        && iGridColumn.ColACType.ACKind == Global.ACKinds.PSProperty
                        && !String.IsNullOrEmpty(iGridColumn.VBContent))
                    {
                        Type entityType = iGridColumn.ColACType.ACClass.ObjectType;
                        if (entityType != null && typeof(EntityObject).IsAssignableFrom(entityType))
                        {
                            PropertyInfo propInfo = entityType.GetProperty(iGridColumn.ColACType.ACIdentifier);
                            if (propInfo != null && propInfo.GetCustomAttributes(typeof(EdmScalarPropertyAttribute), false).Any())
                            {
                                if (!sortColumnsCleared)
                                {
                                    ACQueryDefinition.ACSortColumns.Clear();
                                    sortColumnsCleared = true;
                                }
                                ACSortItem acSortItem = new ACSortItem(
                                    iGridColumn.VBContent,
                                    gridColumn.SortDirection.Value == ListSortDirection.Ascending ? Global.SortDirections.ascending : Global.SortDirections.descending,
                                    true);
                                ACQueryDefinition.ACSortColumns.Add(acSortItem);
#if DEBUG
                                System.Diagnostics.Debug.WriteLine(gridColumn.SortMemberPath + " => " + gridColumn.SortDirection);
#endif
                            }
                        }
                    }
                }
            }
        }

        private ACClass GetValueTypeACClass()
        {
            foreach (var content in ACContentList)
            {
                var tempContent = content;
                while (tempContent != null)
                {
                    if (tempContent.ACType != null && tempContent.ACType.ValueTypeACClass != null && tempContent.ACType.ValueTypeACClass.ManagingBSO != null)
                    {
                        if (!_ACContentList.Contains(tempContent))
                            _ACContentList.Add(tempContent);
                        return tempContent.ACType.ValueTypeACClass;
                    }
                    tempContent = tempContent.ParentACObject;
                }
            }
            return null;
        }

        private IACObject GetValueTypeACClass(ACClass valueTypeACClass)
        {
            foreach (var content in ACContentList)
            {
                if (content.ACType.ValueTypeACClass == valueTypeACClass)
                {
                    if (content is ACValueItem)
                    {
                        return (content as ACValueItem).Value as IACObject;
                    }
                    else
                    {
                        return content;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Displays ACUrl.
        /// </summary>
        /// <summary xml:lang="de">
        /// Zeigt ACUrl.
        /// </summary>
        [ACMethodInteraction("", "en{'Display ACUrl'}de{'Display ACUrl'}", (short)MISort.DisplayACUrl, false)]
        public virtual void DisplayACUrl()
        {
            if (!ACContentList.Any())
                return;
            IACObject content = ACContentList.Where(c => c is IACEntityProperty).FirstOrDefault();
            if (content == null)
                return;

            try
            {
                string acURL = content.GetACUrl();
                Clipboard.Clear();
                Clipboard.SetText(acURL);

                VBWindowDialogMsg vbMessagebox = new VBWindowDialogMsg(new Msg() { Message = acURL, MessageLevel = eMsgLevel.Info }, eMsgButton.OK, this);
                vbMessagebox.ShowMessageBox();
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDataGrid", "DisplayACUrl", msg);
            }
        }

        /// <summary>
        /// Determines is DisplayACUrl enabled or disabled.
        /// </summary>
        /// <returns>Returns true if is DisplayACUrl enabled, otherwise false.</returns>
        public virtual bool IsEnabledDisplayACUrl()
        {
            if (!Database.Root.Environment.License.MayUserDevelop)
                return false;

            if (!ACContentList.Any())
                return false;

            if (ACContentList.First() is ACColumnItem)
                return false;

            var query = ACContentList.Where(c => c is IACEntityProperty);
            if (!query.Any())
                return false;

            using (ACMonitor.Lock(gip.core.datamodel.Database.GlobalDatabase.QueryLock_1X000))
            {
                ACClass acClassBSOStudio = Database.GlobalDatabase.ACClass.Where(c => c.ACIdentifier == "BSOiPlusStudio").FirstOrDefault();
                if (acClassBSOStudio == null)
                    return false;

                if (acClassBSOStudio.GetRight(acClassBSOStudio) != Global.ControlModes.Enabled)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Opens the filter in ACQueryDialog.
        /// </summary>
        /// <summary xml:lang="de">
        /// Öffnet den Filter im ACQueryDialog.
        /// </summary>
        [ACMethodInteraction("", "en{'Filter'}de{'Filter'}", 101, false)]
        public void Filter()
        {
            if (ACAccess == null)
                return;
            if (ACAccess.ShowACQueryDialog())
            {
                if (NavSearchOnACAccess())
                {
                    //BindingExpression expression = this.GetBindingExpression(ItemsSourceProperty);
                    //if (expression != null)
                    //    expression.UpdateTarget();
                    //else
                    //    ItemsSource = ACAccess.NavObjectList;
                }
            }
        }

        /// <summary>
        /// Determines is Filter enabled or disabled.
        /// </summary>
        /// <returns>Returns true if is Filter enabled, otherwise false.</returns>
        public bool IsEnabledFilter()
        {
            return ACAccess != null && BSOACComponent != null;
        }

        /// <summary>
        /// Opens the dialog for change values in column.
        /// </summary>
        /// <summary xml:lang="de">
        /// Öffnet den Dialog zum Ändern von Werten in der Spalte.
        /// </summary>
        [ACMethodInteraction("", "en{'Change values in column'}de{'Ändere Werte in Spalte'}", 101, false)]
        public void ChangeColumnValues()
        {
            if (ACAccess == null || !this.ACContentList.Any())
                return;
            ACColumnItem columnItem = ACContentList.Where(c => c is ACColumnItem).FirstOrDefault() as ACColumnItem;
            if (columnItem == null)
                return;
            if (ACAccess.ShowChangeColumnValuesDialog(columnItem))
            {
            }
        }

        /// <summary>
        /// Determines is ChangeColumnValues enabled or disabled.
        /// </summary>
        /// <returns>Returns true if is ChangeColumnValues enabled, otherwise false.</returns>
        public bool IsChangeColumnValues()
        {
            return ACAccess != null && BSOACComponent != null;
        }

        private bool NavSearchOnACAccess()
        {
            if (ACAccess == null)
                return false;
            bool navSearchExecuted = false;
            IACBSO acComponent = ContextACObject as IACBSO;
            if (acComponent == null)
                acComponent = BSOACComponent;
            if (acComponent != null)
                navSearchExecuted = acComponent.ExecuteNavSearch(ACAccess);
            if (!navSearchExecuted)
                navSearchExecuted = ACAccess.NavSearch();
            return navSearchExecuted;
        }

        private bool _CopyToClipboard = false;
        /// <summary>
        /// Copies the content of selected area to clipboard.
        /// </summary>
        /// <summary xml:lang="de">
        /// Kopiert den Inhalt des ausgewählten Bereichs in die Zwischenablage.
        /// </summary>
        [ACMethodInteraction("", "en{'Copy to clipboard'}de{'In Zwischenablage kopieren'}", (short)102, false)]
        public virtual void CopyToClipboard()
        {
            _CopyToClipboard = true;
            try
            {
                bool canSelect = CanSelectMultipleItems;
                if (!canSelect)
                    CanSelectMultipleItems = true;
                SelectAllCells();
                DataGridClipboardCopyMode lastMode = ClipboardCopyMode;
                ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                ApplicationCommands.Copy.Execute(null, this);
                ClipboardCopyMode = lastMode;
                UnselectAllCells();
                CanSelectMultipleItems = canSelect;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDataGrid", "CopyToClipboard", msg);
            }
            _CopyToClipboard = false;
        }

        /// <summary>
        /// Copies the content of selected row to clipboard.
        /// </summary>
        /// <summary xml:lang="de">
        /// Kopiert den Inhalt des ausgewählte Reihe in die Zwischenablage.
        /// </summary>
        [ACMethodInteraction("", "en{'Copy row to clipboard'}de{'Reihe in Zwischenablage kopieren'}", (short)102, false)]
        public virtual void CopyRowToClipboard()
        {
            _CopyToClipboard = true;
            try
            {
                DataGridClipboardCopyMode lastMode = ClipboardCopyMode;
                ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                ApplicationCommands.Copy.Execute(null, this);
                ClipboardCopyMode = lastMode;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDataGrid", "CopyToClipboard", msg);
            }
            _CopyToClipboard = false;
        }

        private class ExportColumn
        {
            public Type _Type;
            public string _VBContent;
            public IGriColumn _Column;
        }

        [ACMethodInteraction("", "en{'Export to excel file'}de{'In Excel-Datei exportieren'}", (short)103, true)]
        public virtual void Export2Excel()
        {
            try
            {
                CollectionView collectionView = (CollectionView)CollectionViewSource.GetDefaultView(ItemsSource);
                if (collectionView == null)
                    return;

                List<ExportColumn> exportColumns = new List<ExportColumn>();
                DataTable dt = new DataTable();
                var exportableCols = Columns.Where(c => ((IGriColumn)c).ColACType != null);
                int i = 0;
                foreach (IGriColumn column in exportableCols)
                {
                    ExportColumn exportColumn = new ExportColumn() { _Column = column, _Type = column.ColACType.ObjectType, _VBContent = column.VBContent };
                    if (exportColumn._Type == null)
                        continue;
                    if (!IsExcelType(exportColumn._Type))
                    {
                        DataGridComboBoxColumn dcb = column as DataGridComboBoxColumn;
                        if (dcb == null || String.IsNullOrEmpty(dcb.DisplayMemberPath))
                            continue;
                        IACType subACType = column.ColACType.GetMember(dcb.DisplayMemberPath);
                        if (subACType == null)
                            continue;
                        Type subType = subACType.ObjectType;
                        if (!IsExcelType(subType))
                            continue;
                        exportColumn._Type = subType;
                        exportColumn._VBContent = exportColumn._VBContent + "\\" + dcb.DisplayMemberPath;
                    }
                    dt.Columns.Add(i.ToString() + " " + column.ColACType.ACCaption, exportColumn._Type);
                    exportColumns.Add(exportColumn);
                    i++;
                }
                foreach (var colViewEntry in collectionView)
                {
                    var row = dt.NewRow();
                    i = 0;
                    foreach (ExportColumn column in exportColumns)
                    {
                        object value = colViewEntry.GetValue(column._VBContent);
                        row[i] = value == null ? DBNull.Value : value;
                        i++;
                    }
                    dt.Rows.Add(row);
                }

                System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                dlg.Filter = "Excel Files (*.xlsx)|*.xlsx";
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    XLWorkbook workBook = new XLWorkbook();
                    var worksheet = workBook.Worksheets.Add(dt, BSOACComponent.ACCaption);
                    if (worksheet != null)
                        worksheet.Columns().AdjustToContents();
                    workBook.SaveAs(dlg.FileName);
                }
            }
            catch (Exception e)
            {
                this.Root().Messages.LogException("VBDataGrid", "Export2Excel()", e);
                this.Root().Messages.Exception(this, e.Message, true);
            }
        }

        private static Type[] _ExcelTypes = new Type[] { typeof(string), typeof(DateTime), typeof(TimeSpan) };
        public bool IsExcelType(Type type)
        {
            if (type == null)
                return false;
            if (type.IsPrimitive)
                return true;
            return _ExcelTypes.Where(c => c.IsAssignableFrom(type)).Any();
        }

        #endregion

        #region Sum

        /// <summary>
        /// Represents the dependency property for SumVisibility.
        /// </summary>
        public static readonly DependencyProperty SumVisibilityProperty = DependencyProperty.Register("SumVisibility", typeof(Visibility), typeof(VBDataGrid), new PropertyMetadata(Visibility.Collapsed));
        /// <summary>
        /// Gets or sets the SumVisility.
        /// </summary>
        [Category("VBControl")]
        public Visibility SumVisibility
        {
            get { return (Visibility)GetValue(SumVisibilityProperty); }
            set { SetValue(SumVisibilityProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for DictionarySumProperties.
        /// </summary>
        public static readonly DependencyProperty DictionarySumPropertiesProperty = DependencyProperty.Register("DictionarySumProperties", typeof(VBDataGridSumDictionary), typeof(VBDataGrid));

        /// <summary>
        /// Dictionary for temp save summary values.
        /// </summary>
        public VBDataGridSumDictionary DictionarySumProperties
        {
            get { return (VBDataGridSumDictionary)GetValue(DictionarySumPropertiesProperty); }
            set { SetValue(DictionarySumPropertiesProperty, value); }
        }


        /// <summary>
        /// Shows or hides summary row in datagrid.
        /// </summary>
        [ACMethodInteraction("", "en{'Sum'}de{'Summe'}", (short)100, false)]
        public void ShowSum()
        {
            if (SumVisibility != System.Windows.Visibility.Visible)
            {
                SumVisibility = System.Windows.Visibility.Visible;
                CalculateSum();
            }
            else
                SumVisibility = System.Windows.Visibility.Collapsed;
        }

        /// <summary>
        /// Show or hide Sum in context menu.
        /// </summary>
        public bool IsEnabledShowSum()
        {
            if (IsSumEnabled || !string.IsNullOrEmpty(VBSumColumns))
                return true;
            else
                return false;
        }

        String[] _SumColumns;
        /// <summary>
        /// This method creates properties for temporarily save summary values. 
        /// </summary>
        private void CreateSumProperties()
        {
            DictionarySumProperties = new VBDataGridSumDictionary();

            if (string.IsNullOrEmpty(VBSumColumns) || string.IsNullOrWhiteSpace(VBSumColumns))
            {
                foreach (IGriColumn column in Columns)
                    if (column.VBContent != null && !DictionarySumProperties.ContainsKey(column.VBContent))
                        DictionarySumProperties.Add(column.VBContent, "0");
            }
            else
            {
                _SumColumns = VBSumColumns.Split(new char[] { ',' });
                foreach (string column in _SumColumns)
                {
                    DictionarySumProperties.Add(column, "0");
                }
            }
        }

        /// <summary>
        /// This method calculate sum for each column and put it in dictionary.
        /// </summary>
        private void CalculateSum()
        {
            CollectionView collectionView = null;
            if (CollectionViewSource.GetDefaultView(ItemsSource) == null)
            {
                foreach (IGriColumn column in Columns)
                {
                    if (_SumColumns != null)
                    {
                        foreach (string columnName in _SumColumns)
                        {
                            if (column.VBContent == columnName)
                            {
                                DictionarySumProperties[columnName] = "";
                            }
                            else
                            {
                                string header = ((DataGridColumn)column)?.Header?.ToString();
                                if (header == columnName)
                                    DictionarySumProperties[header] = "";
                            }
                        }
                    }
                }
                return;
            }
            else
                collectionView = (CollectionView)CollectionViewSource.GetDefaultView(ItemsSource);

            if (collectionView == null)
                return;

            if (string.IsNullOrEmpty(VBSumColumns) || string.IsNullOrWhiteSpace(VBSumColumns))
                foreach (IGriColumn column in Columns)
                    SumAndCheckType(column, collectionView);
            else
                foreach (IGriColumn column in Columns)
                    if (_SumColumns != null)
                        foreach (string columnName in _SumColumns)
                        {
                            if (column.VBContent == columnName)
                            {
                                SumAndCheckType(column, collectionView);
                            }
                            else if (((DataGridColumn)column)?.Header as string == columnName)
                            {
                                DictionarySumProperties[((DataGridColumn)column).Header.ToString()] = collectionView.Count.ToString();
                            }
                        }
        }

        private void SumAndCheckType(IGriColumn column, CollectionView collectionView)
        {
            if (column == null || collectionView == null)
                return;

            string stringFormat = "";
            if (column.VBContent != null && column.ColACType != null)
            {
                if (DictionarySumProperties == null)
                    return;
                if (column is VBDataGridTextColumn)
                    stringFormat = ((VBDataGridTextColumn)column).StringFormat;

                if (column.ColACType.ObjectType == typeof(double))
                {
                    double _Total = 0;
                    foreach (var item in collectionView)
                    {
                        object itemValue = item.GetValue(column.VBContent);
                        if (itemValue != null && itemValue is double)
                            _Total += (double)itemValue;
                    }
                    if (DictionarySumProperties.ContainsKey(column.VBContent))
                        DictionarySumProperties[column.VBContent] = _Total.ToString(stringFormat);
                }
                else if (column.ColACType.ObjectType == typeof(float))
                {
                    float _Total = 0;
                    foreach (var item in collectionView)
                    {
                        object itemValue = item.GetValue(column.VBContent);
                        if (itemValue != null && itemValue is float)
                            _Total += (float)itemValue;
                    }
                    if (DictionarySumProperties.ContainsKey(column.VBContent))
                        DictionarySumProperties[column.VBContent] = _Total.ToString(stringFormat);
                }
                else if (column.ColACType.ObjectType == typeof(decimal))
                {
                    decimal _Total = 0;
                    foreach (var item in collectionView)
                    {
                        object itemValue = item.GetValue(column.VBContent);
                        if (itemValue != null && itemValue is decimal)
                            _Total += (decimal)itemValue;
                        if (DictionarySumProperties.ContainsKey(column.VBContent))
                            DictionarySumProperties[column.VBContent] = _Total.ToString(stringFormat);
                    }
                }
                else if (column.ColACType.ObjectType == typeof(int))
                {
                    int _Total = 0;
                    foreach (var item in collectionView)
                    {
                        object itemValue = item.GetValue(column.VBContent);
                        if (itemValue != null && itemValue is int)
                            _Total += (int)itemValue;
                    }
                    if (DictionarySumProperties.ContainsKey(column.VBContent))
                        DictionarySumProperties[column.VBContent] = _Total.ToString(stringFormat);
                }
                else if (DictionarySumProperties.ContainsKey(column.VBContent))
                    DictionarySumProperties[column.VBContent] = "";
            }
        }


        #endregion

    }

    public class VBDataGridSumDictionary : Dictionary<string, string>, INotifyPropertyChanged
    {
        public new string this[string key]
        {
            get
            {
                string val = "";
                base.TryGetValue(key, out val);
                return val;
            }
            set
            {
                if (base[key] != value)
                {
                    base[key] = value;
                    OnPropertyChanged(Binding.IndexerName);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
