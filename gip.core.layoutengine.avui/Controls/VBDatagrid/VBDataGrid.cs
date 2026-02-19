using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Collections;
using System.Runtime.Serialization;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using System.Reflection;
using System.Data;
using ClosedXML.Excel;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace gip.core.layoutengine.avui
{
    ///<summary>
    /// Control element for displaying data in a table form.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Darstellung von Daten in Tabellenform.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBDataGrid'}de{'VBDataGrid'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public partial class VBDataGrid : DataGrid, IVBContent, IVBSource, IACMenuBuilderWPFTree, IACObject
    {
        #region c'tors

        /// <summary>
        /// Creates a new instance of VBDataGrid.
        /// </summary>
        public VBDataGrid()
        {
            //DragEnabled = DragMode.Disabled;
            AutoLoad = true;
        }

        /// <summary>
        /// The event handler for Initialized event.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            _ColumnsFromXAML = Columns.ToList();
            // Set up drag and drop event handlers for Avalonia
            if (DragEnabled == DragMode.Enabled)
            {
                DragDrop.SetAllowDrop(this, true);
                AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
                AddHandler(DragDrop.DragOverEvent, OnDragOver);
                AddHandler(DragDrop.DropEvent, OnDrop);
                AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            }
        }

        /// <summary>
        /// Overrides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            InitVBControl();
        }

        #endregion

        #region Loaded Event

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            InitVBControl();
            if (_Loaded)
                return;

            if (BSOACComponent != null)
            {
                try
                {
                    // Handle reference tracking if needed
                }
                catch (Exception exw)
                {
                    this.Root().Messages.LogDebug("VBDataGrid", "AddWPFRef", exw.Message);
                }
            }
            _Loaded = true;
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            if (!_Loaded)
                return;

            if (BSOACComponent != null)
            {
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
            if (DisableContextMenu)
                ContextFlyout = null;

            if (String.IsNullOrEmpty(VBContent))
                return;
            System.Diagnostics.Debug.Assert(VBContent != "");

            _PropertyInfoOfACPropertySelected = null;
            object sourceOfBindingForSelItm = null;
            string pathOfBindingForSelItm = "";
            Global.ControlModes rightControlMode = Global.ControlModes.Hidden;

            if (double.IsNaN(RowHeight) && Database.Root.IsSingleViewApp)
                RowHeight = 45;

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

            // In Avalonia, we check if property is locally set using IsSet extension method
            if (!this.IsSet(SelectionModeProperty))
            {
                // Set default selection mode for DataGrid
            }

            // Plausibility check: If VBContent is a list, then multiple rows can be selected
            // Otherwise SelectionMode must be Single
            if (VBContentPropertyInfo != null)
            {
                if ((SelectionMode == DataGridSelectionMode.Extended) && !VBContentPropertyInfo.IsEnumerable)
                {
                    SelectionMode = DataGridSelectionMode.Single;
                }
                else if ((SelectionMode == DataGridSelectionMode.Single) && VBContentPropertyInfo.IsEnumerable)
                {
                    SelectionMode = DataGridSelectionMode.Extended;
                }
            }

            if (RightControlMode < Global.ControlModes.Disabled)
            {
                IsVisible = false;
            }
            string acAccess = "";
            ACClassProperty sourceProperty = null;
            _ACURLOfPropertyForItemsSource = VBContentPropertyInfo != null ? VBContentPropertyInfo.GetACSource(_ACURLOfPropertyForItemsSource, out acAccess, out sourceProperty) : _ACURLOfPropertyForItemsSource;
            _ItemsSourceACTypeInfo = null;
            object sourceOfBindingForItmSrc = null;
            string pathOfBindingForItmSrc = "";
            Global.ControlModes dsRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(_ACURLOfPropertyForItemsSource, ref _ItemsSourceACTypeInfo, ref sourceOfBindingForItmSrc, ref pathOfBindingForItmSrc, ref dsRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00004", "VBDataGrid", _ACURLOfPropertyForItemsSource + " " + VBContent);
                return;
            }

            if (!String.IsNullOrEmpty(VBAccess))
                acAccess = VBAccess;
            
            if (string.IsNullOrEmpty(acAccess))
            {
                ACQueryDefinition queryDef = this.Root().Queries.CreateQueryByClass(null, _ItemsSourceACTypeInfo.ValueTypeACClass.PrimaryNavigationquery(), _ItemsSourceACTypeInfo.ValueTypeACClass.ACIdentifier, true);
                if (queryDef != null)
                {
                    IACObjectWithInit initObject = ContextACObject as IACObjectWithInit;
                    if (initObject == null)
                        initObject = BSOACComponent;
                    bool isObjectSet = ((pathOfBindingForItmSrc.StartsWith("Database.") || (sourceOfBindingForItmSrc != null && sourceOfBindingForItmSrc is IACEntityObjectContext))
                                        && (_ItemsSourceACTypeInfo is ACClassProperty && (_ItemsSourceACTypeInfo as ACClassProperty).GenericType == typeof(Microsoft.EntityFrameworkCore.DbSet<>).FullName));
                    if (initObject != null && isObjectSet)
                        _ACAccess = queryDef.NewAccess("VBDataGrid", initObject);
                    else
                        _ACQueryDefinition = queryDef;
                }
            }
            else
            {
                _ACAccess = ContextACObject.ACUrlCommand(acAccess) as IAccess;
            }

            UpdateColumns();

            if (BSOACComponent != null)
            {
                var binding = new Binding
                {
                    Source = BSOACComponent,
                    Path = Const.InitState,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBDataGrid.ACCompInitStateProperty, binding);
            }
            if (ContextACObject is IACComponent)
            {
                var binding = new Binding
                {
                    Source = ContextACObject,
                    Path = Const.ACUrlCmdMessage,
                    Mode = BindingMode.OneWay
                };
                this.Bind(VBDataGrid.ACUrlCmdMessageProperty, binding);
            }

            if (pathOfBindingForItmSrc.StartsWith(Const.ContextDatabase + "."))
            {
                if (NavSearchOnACAccess())
                {
                    var binding = new Binding
                    {
                        Source = ACAccess,
                        Path = "NavObjectList",
                        Mode = BindingMode.OneWay
                    };
                    this.Bind(ItemsSourceProperty, binding);
                }
            }
            else
            {
                var bindingItemSource = new Binding();
                if (!BindItemsSourceToContext)
                    bindingItemSource.Source = sourceOfBindingForItmSrc;
                if (!string.IsNullOrEmpty(pathOfBindingForItmSrc))
                {
                    bindingItemSource.Path = pathOfBindingForItmSrc;
                }
                else
                    bindingItemSource.Source = sourceOfBindingForItmSrc;
                this.Bind(ItemsSourceProperty, bindingItemSource);
            }

            if (VBContentPropertyInfo != null)
            {
                if (!VBContentPropertyInfo.IsEnumerable)
                {
                    var binding2 = new Binding();
                    if (!BindSelectedItemToContext)
                        binding2.Source = sourceOfBindingForSelItm;
                    binding2.Path = pathOfBindingForSelItm;
                    binding2.Mode = VBContentPropertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay;
                    this.Bind(SelectedItemProperty, binding2);
                }
                else
                {
                    // Enable multiple selection for list properties
                    SelectionMode = DataGridSelectionMode.Extended;
                }
            }
            LoadDataGridConfig();

            UpdateControlMode();

            DoubleTapped += VBDataGrid_DoubleTapped;
            
            if (AutoFocus)
            {
                Focus();
            }

            if (IsSumEnabled)
                CreateSumProperties();
            else if (!IsSumEnabled && !string.IsNullOrEmpty(VBSumColumns))
            {
                IsSumEnabled = true;
                CreateSumProperties();
            }

            if (this.IsSet(CyclicDataRefreshProperty))
            {
                if (CyclicDataRefresh > 0)
                {
                    _cyclickDataRefreshDispTimer = new DispatcherTimer();
                    _cyclickDataRefreshDispTimer.Tick += new EventHandler(cyclickDataRefreshDispTimer_CanExecute);
                    _cyclickDataRefreshDispTimer.Interval = new TimeSpan(0, 0, 0, 0, CyclicDataRefresh);
                    _cyclickDataRefreshDispTimer.Start();
                }
            }
        }

        private void cyclickDataRefreshDispTimer_CanExecute(object sender, EventArgs e)
        {
            if (ItemsSource != null && ItemsSource is ICyclicRefreshableCollection)
            {
                (ItemsSource as ICyclicRefreshableCollection).Refresh();
            }
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a Control refers to.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public virtual void DeInitVBControl(IACComponent bso)
        {
            if (!_Initialized)
                return;
            _Initialized = false;
            if (bso != null && bso is IACBSO)
                (bso as IACBSO).RemoveWPFRef(this.GetHashCode());

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
                {
                    // Clear bindings for column if needed
                }
            }

            this.ClearAllBindings();
            this.ItemsSource = null;
        }

        internal bool ResolveColumnItem(ACColumnItem dataShowColumn, out IACType dsColACTypeInfo, ref object dsColSource, ref string dsColPath, ref Global.ControlModes dsColRightControlMode, ref bool isShowColumnAMethod)
        {
            dsColACTypeInfo = ItemsSourceACTypeInfo;
            isShowColumnAMethod = dataShowColumn.PropertyName.StartsWith("!");
            if (isShowColumnAMethod)
            {
                if (!ContextACObject.ACUrlBinding(dataShowColumn.PropertyName, ref dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
                {
                    this.Root().Messages.LogDebug("Error00005", "VBDataGrid", dataShowColumn.PropertyName + " " + VBContent);
                    return false;
                }
            }
            else if (!ItemsSourceACTypeInfo.ACUrlBinding(dataShowColumn.PropertyName, ref dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00005", "VBDataGrid", dataShowColumn.PropertyName + " " + VBContent);
                return false;
            }
            return true;
        }

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

                    if (!SimpleCellElements && dsColACTypeInfo.ObjectType == typeof(bool))
                    {
                        VBDataGridCheckBoxColumn dataGridCheckBoxColumn = new VBDataGridCheckBoxColumn();
                        Columns.Add(dataGridCheckBoxColumn);
                        dataGridCheckBoxColumn.Initialize(dataShowColumn, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode);
                        dataGridCheckBoxColumn.VBContent = dsColPath;
                    }
                    else if (dsColACTypeInfo.ObjectType == typeof(DateTime))
                    {
                        VBDataGridDateTimeColumn dataGridDateTimeColumn = new VBDataGridDateTimeColumn();
                        Columns.Add(dataGridDateTimeColumn);
                        dataGridDateTimeColumn.Initialize(dataShowColumn, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode);
                        dataGridDateTimeColumn.VBContent = dsColPath;
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
                            dataSourceColumn = dsColACTypeInfo is ACClassProperty ? (dsColACTypeInfo as ACClassProperty).GetACSource("", out acAccessColumn, out sourceProperty) : "";
                        }

                        if (string.IsNullOrEmpty(dataSourceColumn))
                        {
                            VBDataGridTextColumn dataGridTextColumn = new VBDataGridTextColumn();
                            Columns.Add(dataGridTextColumn);
                            dataGridTextColumn.Initialize(dataShowColumn, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode, isShowColumnAMethod);
                            dataGridTextColumn.VBContent = dsColPath;
                        }
                        else
                        {
                            if (isShowColumnAMethod)
                            {
                                VBDataGridTextColumn dataGridTextColumn = new VBDataGridTextColumn();
                                Columns.Add(dataGridTextColumn);
                                dataGridTextColumn.Initialize(dataShowColumn, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode, isShowColumnAMethod);
                                dataGridTextColumn.VBContent = dsColPath;
                            }
                            else
                            {
                                VBDataGridComboBoxColumn dataGridComboColumn = new VBDataGridComboBoxColumn();
                                Columns.Add(dataGridComboColumn);
                                dataGridComboColumn.Initialize(dataShowColumn, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode, isShowColumnAMethod);
                                dataGridComboColumn.VBContent = dsColPath;
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

        #region Event Handling
        /// <summary>
        /// Handles the OnMouseLeftButtonDown event.
        /// </summary>
        /// <param name="e">The MouseButtonEvent arguments.</param>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.Properties.IsLeftButtonPressed)
            {
                OnLeftButtonDown(e);
            }
            base.OnPointerPressed(e);
        }

        /// <summary>
        /// Handles the OnPointerReleased event for context menu.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Right)
            {
                if (DisableContextMenu)
                {
                    e.Handled = true;
                    return;
                }
                if (ContextACObject == null)
                {
                    base.OnPointerReleased(e);
                    return;
                }

                DataGridCell cell = null;
                var visual = e.Source as Visual;

                if (visual == null)
                {
                    e.Handled = true;
                    base.OnPointerReleased(e);
                    return;
                }

                string vbContent = null;
                UpdateACContentList(GetNearestContainer(visual, ref vbContent), vbContent);
                if (cell != null)
                {
                    UpdateACContentList(cell, vbContent);
                }

                Point point = e.GetPosition(this);
                ACActionMenuArgs actionArgs = new ACActionMenuArgs(this, point.X, point.Y, Global.ElementActionType.ContextMenu);
                BSOACComponent.ACAction(actionArgs);
                if (actionArgs.ACMenuItemList != null && actionArgs.ACMenuItemList.Any())
                {
                    VBContextMenu vbContextMenu = new VBContextMenu(this, actionArgs.ACMenuItemList);
                    this.ContextMenu = vbContextMenu;
                    if (vbContextMenu.PlacementTarget == null)
                        vbContextMenu.PlacementTarget = this;
                    ContextMenu.Open();
                }
                e.Handled = true;
            }
            else if (e.InitialPressMouseButton == MouseButton.Left)
            {
                OnPreviewMouseLeftButtonUp(e);

                if (OpenDesignOnClick != null)
                {
                    VBDesignController controller = VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBDesignController)) as VBDesignController;
                    if (controller != null)
                    {
                        controller.ShowDesign(OpenDesignOnClick);
                    }
                }
            }
            base.OnPointerReleased(e);
        }

        /// <summary>
        /// Handles the OnKeyDown event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
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

            if ((ItemsSourceACTypeInfo != null) && this.SelectionMode == DataGridSelectionMode.Extended)
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

            //if (OpenDesignOnClick != null && firstTimeOpened == false)
            //{
            //    VBDesignController controller = FindParentOfType(this, typeof(VBDesignController)) as VBDesignController;
            //    if (controller != null)
            //    {
            //        controller.ShowDesign(OpenDesignOnClick);
            //    }


            //    //if (this.Parent is VBDesign designParent)
            //    //{
            //    //    var p = designParent.Parent as Grid;
            //    //    if (p?.TemplatedParent is VBPage page)
            //    //    {
            //    //        var frame = page.FrameController;
            //    //        frame.ShowDesign(OpenDesignOnClick, BSOACComponent, "");
            //    //    }
            //    //}
            //    //if (this.Parent is VBGrid gridParent)
            //    //{
            //    //    var designParentGrid = gridParent.Parent as VBDesign;
            //    //    var p = designParentGrid.Parent as Grid;
            //    //    if (p?.TemplatedParent is VBPage page)
            //    //    {
            //    //        var frame = page.FrameController;
            //    //        frame.ShowDesign(OpenDesignOnClick, BSOACComponent, "");
            //    //    }
            //    //}
            //}
            //firstTimeOpened = false;

            base.OnSelectionChanged(e);
        }

        /// <summary>
        /// Handles the OnCellEditEnding event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnCellEditEnding(DataGridCellEditEndingEventArgs e)
        {
            base.OnCellEditEnding(e);
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
            base.OnPreparingCellForEdit(e);
        }

        // Property change handling replaces WPF's TargetUpdated/SourceUpdated events
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            
            if (change.Property == ACCompInitStateProperty)
                InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null)
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        DeInitVBControl(bso);
                }
            }
            else if (change.Property == ACUrlCmdMessageProperty)
                OnACUrlMessageReceived();
            else if (change.Property == ItemsSourceProperty)
            {
                VBDataGrid_TargetUpdated(this, change);
            }
            else if (change.Property == SelectedItemProperty)
            {
                VBDataGrid_SourceUpdated(this, change);
            }
        }

        void VBDataGrid_TargetUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (_CopyToClipboard)
                return;
            if (sender is VBDataGrid vbDataGrid)
            {
                try
                {
                    // Handle collection refresh if needed
                }
                catch (Exception)
                {
                }
            }
            
            if (IsSumEnabled && SumVisibility)
                CalculateSum();
        }

        void VBDataGrid_SourceUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == SelectedItemProperty)
            {
                if (IsSumEnabled && SumVisibility)
                    CalculateSum();
            }
        }

        void VBDataGrid_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (ContextACObject == null)
                return;
            if (DblClick != null && DblClick != "")
            {
                ContextACObject.ACUrlCommand(DblClick);
            }
        }

        /// <summary>
        /// The event handler for TextInput.
        /// </summary>
        /// <param name="e">The TextInput event arguments.</param>
        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
        }

        #endregion

        #region IDataContent Members

        private void UpdateACContentList(Control control, string vbContent)
        {
            if (control is DataGridCellsPresenter cellsPresenter)
            {
                // In Avalonia, we need to handle this differently
                // DataGridCellsPresenter might not have direct Item access
                UpdateACContentList((IACObject)null, vbContent);
            }
            else if (control is DataGridColumnHeader currentHeader)
            {
                _ACContentList.Clear();
                // In Avalonia, DataGridColumnHeader might not have direct Column access
                // We need to implement this based on the actual Avalonia DataGrid structure
            }
            else if (control is DataGridCell cell)
            {
                // In Avalonia, DataGridCell might not have direct Column access
                // We need to implement this based on the actual Avalonia DataGrid structure
            }
        }

        private void UpdateACContentList(IACObject acObject, string vbContent)
        {
            _ACContentList.Clear();

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
            if (ContextACObject != null)
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
            RemoteCommandAdornerManager.Instance.VisualizeIfRemoteControlled(this, elementACComponent, false);
        }

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

        #endregion

        #region private methods
        internal bool IsSetInVBDisabledColumns(string column)
        {
            // If Grid as a whole is not editable
            if (string.IsNullOrEmpty(VBDisabledColumns))
                return false;
            return _DisabledColumnList.Contains(column);
        }

        ///// <summary>
        ///// Finds the parent of the type defined in parameter type.
        ///// </summary>
        ///// <param name="forObject">The forObject parameter.</param>
        ///// <param name="type">The type parameter.</param>
        ///// <returns></returns>
        //public static Visual FindParentOfType(Visual forObject, Type type)
        //{
        //    if (forObject == null || type == null)
        //        return null;
            
        //    var parent = forObject.GetVisualParent();
        //    while (parent != null)
        //    {
        //        if (type.IsAssignableFrom(parent.GetType()))
        //            return parent;
        //        parent = parent.GetVisualParent();
        //    }
        //    return null;
        //}

        private Control GetNearestContainer(Visual element, ref string vbContent)
        {
            // This needs to be implemented based on Avalonia's DataGrid structure
            // For now, return null as placeholder
            return null;
        }

        #endregion

        #region IACObject
        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that addresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Determines if ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl parameter.</param>
        /// <param name="acParameter">The acParameter.</param>
        /// <returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
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
        /// <param name="rootACObject">Object for creating a relative path to it</param>
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
                    if (value is ACClass)
                    {
                        if (Database.Root.Environment.License.MayUserDevelop)
                        {
                            ACMethod acMethod = Database.Root.ACType.ACType.ACUrlACTypeSignature(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio);
                            acMethod.ParameterValueList["AutoLoad"] = (value as ACClass).GetACUrl();

                            this.Root().RootPageWPF.StartBusinessobject(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio, acMethod.ParameterValueList);
                        }
                    }
                    else
                    {
                        ACMethod acMethod = Database.Root.ACType.ACType.ACUrlACTypeSignature(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + Const.BSOiPlusStudio);
                        acMethod.ParameterValueList["AutoFilter"] = value.ACUrlCommand(filterColumn).ToString();

                        this.Root().RootPageWPF.StartBusinessobject(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + acClass.ManagingBSO.ACIdentifier, acMethod.ParameterValueList);
                    }
                }
                else
                {
                    this.Root().RootPageWPF.StartBusinessobject(Const.BusinessobjectsACUrl + ACUrlHelper.Delimiter_Start + acClass.ManagingBSO.ACIdentifier, null);
                }
            }
        }

        /// <summary>
        /// Determines if enabled modify.
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
        /// Groups data.
        /// </summary>
        [ACMethodInteraction("", "en{'Group'}de{'Gruppieren'}", 900, false)]
        public void Group()
        {
        }

        /// <summary>
        /// Determines if Group is enabled or disabled.
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
        /// Ungroups data.
        /// </summary>
        [ACMethodInteraction("", "en{'Ungroup'}de{'Gruppierung aufheben'}", 901, false)]
        public void UnGroup()
        {
        }

        /// <summary>
        /// Determines if UnGroup is enabled or disabled.
        /// </summary>
        /// <returns>Returns true if is UnGroup enabled otherwise false.</returns>
        public bool IsEnabledUnGroup()
        {
            ACColumnItem acColumnItem = ACContentList.FirstOrDefault() as ACColumnItem;
            if (acColumnItem == null)
                return false;

            return true;
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
        /// Opens the filter in ACQueryDialog.
        /// </summary>
        /// <summary xml:lang="de">
        /// Öffnet den Filter im ACQueryDialog.
        /// </summary>
        [ACMethodInteraction("", "en{'Filter'}de{'Filter'}", 101, false)]
        public async void Filter()
        {
            if (ACAccess == null)
                return;
            if (await ACAccess.ShowACQueryDialog())
            {
                if (NavSearchOnACAccess())
                {
                    // In Avalonia, binding refresh is handled differently
                }
            }
        }

        /// <summary>
        /// Determines if Filter is enabled or disabled.
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
        /// Determines if ChangeColumnValues is enabled or disabled.
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
        #endregion
    }
}
