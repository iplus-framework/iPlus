using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Metadata;
using Avalonia.Styling;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a <see cref="gip.core.layoutengine.avui.VBDataGrid"/> column that host <see cref="VBComboBox"/> control in its cells.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine <see cref="gip.core.layoutengine.avui.VBDataGrid"/>-Spalte dar, die das <see cref="VBComboBox"/>-Control in ihren Zellen hostet.
    /// </summary>
    public class VBDataGridComboBoxColumn : DataGridTextColumn, IGriColumn
    {
        private readonly Lazy<ControlTheme> _cellComboEditTheme;
        private readonly Lazy<ControlTheme> _cellComboDefaultTheme;
        public VBDataGridComboBoxColumn() : base()
        {
            _cellComboEditTheme = new Lazy<ControlTheme>(() =>
                OwningGrid.TryFindResource("DataGridCellComboEditTheme", out var theme) ? (ControlTheme)theme : null);
            _cellComboDefaultTheme = new Lazy<ControlTheme>(() =>
                OwningGrid.TryFindResource("DataGridCellComboDefaultTheme", out var theme) ? (ControlTheme)theme : null);
        }


        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty = AvaloniaProperty.Register<VBDataGridComboBoxColumn, string>(nameof(VBContent));

        /// <summary>
        /// Represents the property in which you enter the name of property that you want show in this column. Property must be in object, which is bounded to the VBDataGrid.
        /// </summary>
        [Category("VBControl")]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for VBIsReadOnlyProperty.
        /// </summary>
        public static readonly StyledProperty<bool> VBIsReadOnlyProperty = AvaloniaProperty.Register<VBDataGridComboBoxColumn, bool>(nameof(VBIsReadOnly));
        /// <summary>
        /// Determines is column is read only or not. The true value is for readonly.
        /// </summary>
        [Category("VBControl")]
        public bool VBIsReadOnly
        {
            get { return (bool)GetValue(VBIsReadOnlyProperty); }
            set { SetValue(VBIsReadOnlyProperty, value); }
        }


        public static readonly StyledProperty<bool> IsEditableProperty = AvaloniaProperty.Register<VBDataGridComboBoxColumn, bool>(nameof(IsEditable));
        [Category("VBControl")]
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        public static readonly StyledProperty<UpdateSourceTrigger> UpdateSourceTriggerProperty = AvaloniaProperty.Register<VBDataGridComboBoxColumn, UpdateSourceTrigger>(nameof(UpdateSourceTrigger), UpdateSourceTrigger.LostFocus);
        [Category("VBControl")]
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get { return (UpdateSourceTrigger)GetValue(UpdateSourceTriggerProperty); }
            set { SetValue(UpdateSourceTriggerProperty, value); }
        }

        public static readonly StyledProperty<IEnumerable> ItemsSourceProperty = AvaloniaProperty.Register<VBDataGridComboBoxColumn, IEnumerable>(nameof(ItemsSource));
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }


        public static readonly StyledProperty<IBinding> DisplayMemberBindingProperty = AvaloniaProperty.Register<VBDataGridComboBoxColumn, IBinding>(nameof(DisplayMemberBinding));
        [AssignBinding]
        [InheritDataTypeFromItems(nameof(ItemsSource))]
        public IBinding DisplayMemberBinding
        {
            get => GetValue(DisplayMemberBindingProperty);
            set => SetValue(DisplayMemberBindingProperty, value);
        }

        public static readonly StyledProperty<IBinding> SelectedValueBindingProperty = AvaloniaProperty.Register<VBDataGridComboBoxColumn, IBinding>(nameof(SelectedValueBinding));
        [AssignBinding]
        [InheritDataTypeFromItems(nameof(ItemsSource))]
        public IBinding SelectedValueBinding
        {
            get => GetValue(SelectedValueBindingProperty);
            set => SetValue(SelectedValueBindingProperty, value);
        }

        public static readonly StyledProperty<IBinding> SelectedItemBindingProperty = AvaloniaProperty.Register<VBDataGridComboBoxColumn, IBinding>(nameof(SelectedItemBinding));
        [AssignBinding]
        [InheritDataTypeFromItems(nameof(ItemsSource))]
        public IBinding SelectedItemBinding
        {
            get => GetValue(SelectedItemBindingProperty);
            set => SetValue(SelectedItemBindingProperty, value);
        }

        public static readonly StyledProperty<string> DisplayMemberPathProperty = AvaloniaProperty.Register<VBDataGridComboBoxColumn, string>(nameof(DisplayMemberPath));
        [Category("VBControl")]
        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        private IDataTemplate _ItemTemplate;
        public static readonly DirectProperty<VBDataGridComboBoxColumn, IDataTemplate> ItemTemplateProperty =
        AvaloniaProperty.RegisterDirect<VBDataGridComboBoxColumn, IDataTemplate>(
            nameof(ItemTemplate),
            o => o.ItemTemplate,
            (o, v) => o.ItemTemplate = v);


        public IDataTemplate ItemTemplate
        {
            get { return _ItemTemplate; }
            set { SetAndRaise(HeaderTemplateProperty, ref _ItemTemplate, value); }
        }

        public static readonly StyledProperty<IBinding> TextBindingProperty = AvaloniaProperty.Register<VBDataGridComboBoxColumn, IBinding>(nameof(TextBinding));
        [AssignBinding]
        [InheritDataTypeFromItems(nameof(ItemsSource))]
        public IBinding TextBinding
        {
            get => GetValue(TextBindingProperty);
            set => SetValue(TextBindingProperty, value);
        }

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        public Global.ControlModes RightControlMode
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the instance of VBDataGrid.
        /// </summary>
        public VBDataGrid VBDataGrid
        {
            get
            {
                return OwningGrid as VBDataGrid;
            }
        }

        private ACColumnItem _ACColumnItem;
        /// <summary>
        /// Gets or sets the ACColumn item.
        /// </summary>
        public ACColumnItem ACColumnItem
        {
            get
            {
                return _ACColumnItem;
            }
            set
            {
                if ((this.OwningGrid != null) && (this.OwningGrid is VBDataGrid))
                {
                    VBDataGrid dataGrid = this.OwningGrid as VBDataGrid;
                    if (dataGrid == null)
                        return;
                    if (dataGrid.ContextACObject == null)
                        return;
                    IACType dsColACTypeInfo = null;
                    object dsColSource = null;
                    string dsColPath = "";
                    Global.ControlModes dsColRightControlMode = Global.ControlModes.Hidden;
                    bool isShowColumnAMethod = false;
                    if (dataGrid.ResolveColumnItem(value, out dsColACTypeInfo, ref dsColSource, ref dsColPath, ref dsColRightControlMode, ref isShowColumnAMethod))
                    {
                        if (dsColRightControlMode != Global.ControlModes.Hidden)
                        {
                            Initialize(value, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode, isShowColumnAMethod);
                        }
                    }
                }
            }
        }

        private IACType _ColACTypeInfo = null;
        /// <summary>
        /// Gets the ACType for column.
        /// </summary>
        public ACClassProperty ColACType
        {
            get
            {
                return _ColACTypeInfo as ACClassProperty;
            }
        }

        /// <summary>
        /// Gets or sets the ACQueryDefinition.
        /// </summary>
        public ACQueryDefinition ACQueryDefinition
        {
            get;
            set;
        }

        private string _VBShowColumns;
        /// <summary>
        /// Gets or sets the VBShow columns.
        /// </summary>
        [Category("VBControl")]
        public string VBShowColumns
        {
            get
            {
                return _VBShowColumns;
            }
            set
            {
                _VBShowColumns = value;
            }
        }

        /// <summary>
        /// Gets or sets the ACCaption
        /// </summary>
        public string ACCaption
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a VBDataGridACValueColumn.
        /// </summary>
        /// <param name="acColumnItem">The ACColumn item parameter.</param>
        /// <param name="dsColACTypeInfo">The data source ACType info for column.</param>
        /// <param name="dsColSource">The data source for column.</param>
        /// <param name="dsColPath">The data source column path.</param>
        /// <param name="dsColRightControlMode">The data source right control mode for column.</param>
        public void Initialize(ACColumnItem acColumnItem, IACType dsColACTypeInfo, object dsColSource, string dsColPath, Global.ControlModes dsColRightControlMode, bool isShowColumnAMethod)
        {
            VBDataGrid dataGrid = this.OwningGrid as VBDataGrid;
            if (dataGrid == null)
                return;
            if (dataGrid.ContextACObject == null)
                return;

            _ACColumnItem = acColumnItem;
            _ColACTypeInfo = dsColACTypeInfo;
            string acAccessColumn = "";
            string dataSourceColumn;
            ACClassProperty sourceProperty = null;
            if (isShowColumnAMethod)
                dataSourceColumn = acColumnItem.PropertyName;
            else
                dataSourceColumn = dsColACTypeInfo is ACClassProperty ? (dsColACTypeInfo as ACClassProperty).GetACSource("", out acAccessColumn, out sourceProperty) : "";

            IACType dscACTypeInfo = null;
            object dscSource = null;
            string dscPath = "";
            Global.ControlModes dscRightControlMode = Global.ControlModes.Hidden;
            dataGrid.ContextACObject.ACUrlBinding(dataSourceColumn, ref dscACTypeInfo, ref dscSource, ref dscPath, ref dscRightControlMode);
            //if (!dataGrid.ContextACObject.ACUrlBinding(dataSourceColumn, ref dscACTypeInfo, ref dscSource, ref dscPath, ref dscRightControlMode))
            //{
            //    this.Root().Messages.LogDebug("Error00004", "VBDataGrid", dataGrid.VBSource + " " + VBContent + "-" + dsColACTypeInfo.ACIdentifier);
            //    //this.Root().Messages.Error(dataGrid.ContextACObject, "Error00004", "VBDataGrid", dataGrid.VBSource, VBContent + "-" + dsColACTypeInfo.ACIdentifier);
            //    return;
            //}

            if (dscACTypeInfo != null && dscACTypeInfo is ACClassMethod)
            {
                RightControlMode = dscRightControlMode;
                InitWithMethodBinding(dsColACTypeInfo, acColumnItem, dsColSource, dsColPath, dscSource, dscPath, dscACTypeInfo, dataSourceColumn, acAccessColumn);
            }
            else
            {
                RightControlMode = dsColRightControlMode;
                InitWithBinding(dsColACTypeInfo, acColumnItem, dsColSource, dsColPath, dscSource, dscPath, dscACTypeInfo, dataSourceColumn, acAccessColumn);
            }
        }

        private void InitWithBinding(IACType dsColACTypeInfo, ACColumnItem acColumnItem, object dsColSource, string dsColPath, object dscSource, string dscPath, IACType dscACTypeInfo, string dataSourceColumn, string acAccess)
        {
            VBDataGrid dataGrid = this.OwningGrid as VBDataGrid;
            if (dataGrid == null)
                return;
            if (dataGrid.ContextACObject == null)
                return;

            List<ACColumnItem> dataShowColumnsColumn = dsColACTypeInfo.GetColumns();
            IACType dsColACTypeInfoColumn = dsColACTypeInfo;
            object dsColSourceColumn = null;
            string dsColPathColumn = "";
            Global.ControlModes dscolRightControlMode = Global.ControlModes.Hidden;

            if (dsColACTypeInfo.ObjectType.IsEnum)
            {
                if (string.IsNullOrEmpty(acAccess))
                {
                    ACQueryDefinition = this.Root().Queries.CreateQueryByClass(null, dscACTypeInfo.ValueTypeACClass.PrimaryNavigationquery(), dscACTypeInfo.ValueTypeACClass.PrimaryNavigationquery().ACIdentifier, true);
                }
                // ansonsten ACQueryDefinition aus BSO verwenden
                else
                {
                    IAccess access = dataGrid.ContextACObject.ACUrlCommand(acAccess) as IAccess;
                    ACQueryDefinition = access.NavACQueryDefinition;
                }

                List<ACColumnItem> vbShowColumns = ACQueryDefinition.GetACColumns(VBShowColumns);
                if (vbShowColumns == null || !vbShowColumns.Any())
                    return;

                // Überschrift Spalte
                // ACCaption bevorzugen
                if (!string.IsNullOrEmpty(ACCaption))
                    this.Header = this.Root().Environment.TranslateText(dataGrid.ContextACObject, ACCaption);
                else
                {
                    if (!this.IsSet(DataGridColumn.HeaderProperty))
                    {
                        if (_ColACTypeInfo != null)
                            this.Header = _ColACTypeInfo.ACCaption;
                        else
                            this.Header = dsColACTypeInfo.ACCaption;
                    }
                }

                // Set Items-Source
                if (dscPath.StartsWith(gip.core.datamodel.Database.ClassName + ".") && dscSource != null)
                {
                    Type t = dscSource.GetType();
                    System.Reflection.PropertyInfo pi = t.GetProperty(Const.ContextDatabase);

                    var lastPath = dscPath.Substring(9);
                    var database = pi.GetValue(dscSource, null) as IACObject;
                    var result = database.ACSelect(ACQueryDefinition, lastPath);
                    System.Collections.ArrayList arrayList = new System.Collections.ArrayList();
                    foreach (object entry in result)
                    {
                        arrayList.Add(entry);
                    }
                    this.ItemsSource = arrayList;
                }
                else
                {
                    // Listenbereich von Combobox füllen 
                    Binding binding = new Binding();
                    if (dscSource != null)
                    {
                        binding.Source = dscSource;
                        if (!string.IsNullOrEmpty(dscPath))
                        {
                            binding.Path = dscPath;
                        }
                    }
                    else
                    {
                        binding.Source = dataGrid.ItemsSource;
                        binding.Path = dataSourceColumn;
                    }
                    Bind(ItemsSourceProperty, binding);
                }

                SetValue(SelectedValueBindingProperty, new Binding(Const.Value));

                // Anzeige des Wertes in der Zelle
                this.DisplayMemberBinding = DisplayMemberBinding = new Binding(dataShowColumnsColumn[0].PropertyName);

                Binding binding2 = new Binding();
                binding2.Path = dsColPath;
                binding2.Mode = BindingMode.TwoWay;
                binding2.UpdateSourceTrigger = UpdateSourceTrigger;
                SelectedValueBinding = binding2;

            }
            else
            {
                if (!dataShowColumnsColumn.Any() || !dsColACTypeInfo.ACUrlBinding(dataShowColumnsColumn[0].PropertyName, ref dsColACTypeInfoColumn, ref dsColSourceColumn, ref dsColPathColumn, ref dscolRightControlMode))
                    return;

                // Überschrift Spalte
                // ACCaption bevorzugen
                if (!string.IsNullOrEmpty(ACCaption))
                    this.Header = this.Root().Environment.TranslateText(dataGrid.ContextACObject, ACCaption);
                else
                {
                    if (!this.IsSet(DataGridColumn.HeaderProperty))
                        this.Header = dsColACTypeInfo.ACCaption;
                }

                // Set Selected Item
                Binding bindingEdit = new Binding();
                bindingEdit.Path = dsColPath;
                bindingEdit.Mode = BindingMode.TwoWay;
                this.SelectedItemBinding = bindingEdit;
                bindingEdit.UpdateSourceTrigger = UpdateSourceTrigger;

                // Anzeige des Wertes in der Zelle
                if (String.IsNullOrEmpty(DisplayMemberPath) && ItemTemplate == null)
                    DisplayMemberPath = dsColPathColumn;

                // Set Items-Source
                Binding bindingSource = new Binding();
                if (dscSource != null)
                {
                    bindingSource.Source = dscSource;
                    bindingSource.Path = dscPath;
                    bindingSource.Mode = BindingMode.OneWay;
                    this.Bind(ItemsSourceProperty, bindingSource);
                }
                else
                {
                    if (   _cellComboEditTheme != null
                        && !_cellComboEditTheme.Value.Setters.OfType<Setter>().Where(c => c.Property == ComboBox.ItemsSourceProperty).Any())
                    {
                        bindingSource.Path = dataSourceColumn;
                        Setter setter = new Setter();
                        setter.Property = ComboBox.ItemsSourceProperty;
                        setter.Value = bindingSource;
                        _cellComboEditTheme.Value.Setters.Add(setter);
                    }
                    if (    _cellComboDefaultTheme != null
                        && !_cellComboDefaultTheme.Value.Setters.OfType<Setter>().Where(c => c.Property == ComboBox.ItemsSourceProperty).Any())
                    {
                        Binding bindingSource2 = new Binding();
                        bindingSource2.Path = dataSourceColumn;
                        Setter setter = new Setter();
                        setter.Property = ComboBox.ItemsSourceProperty;
                        setter.Value = bindingSource2;
                        _cellComboDefaultTheme.Value.Setters.Add(setter);
                    }
                }
            }

            ACClassProperty propertyInfo = dsColACTypeInfo as ACClassProperty;
            if (propertyInfo != null && !this.IsSet(DataGridColumn.WidthProperty))
            {
                if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength > 20)
                    this.Width = new DataGridLength(200 + 20);
                else if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength.Value == 0)
                    this.Width = new DataGridLength(100 + 20);
                else if (propertyInfo.MaxLength.HasValue)
                    this.Width = new DataGridLength((propertyInfo.MaxLength.Value * 10) + 20);
                else
                    this.Width = new DataGridLength(100);
                this.MinWidth = 60;
                this.Width = new DataGridLength(100, DataGridLengthUnitType.SizeToCells);
            }

            RefreshReadOnlyProperty();
        }

        private void InitWithMethodBinding(IACType dsColACTypeInfo, ACColumnItem acColumnItem, object dsColSource, string dsColPath, object dscSource, string dscPath, IACType dscACTypeInfo, string dataSourceColumn, string acAccess)
        {
            VBDataGrid dataGrid = this.OwningGrid as VBDataGrid;
            if (dataGrid == null)
                return;
            if (dataGrid.ContextACObject == null)
                return;

            ACClassProperty propertyInfo = dsColACTypeInfo as ACClassProperty;
            if (propertyInfo != null && !this.IsSet(DataGridColumn.WidthProperty))
            {
                if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength > 20)
                    this.Width = new DataGridLength(200 + 20);
                else if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength.Value == 0)
                    this.Width = new DataGridLength(100 + 20);
                else if (propertyInfo.MaxLength.HasValue)
                    this.Width = new DataGridLength((propertyInfo.MaxLength.Value * 10) + 20);
                else
                    this.Width = new DataGridLength(100);
                this.MinWidth = 60;
                this.Width = new DataGridLength(100, DataGridLengthUnitType.SizeToCells);
            }


            List<ACColumnItem> dataShowColumnsColumn = dsColACTypeInfo.GetColumns();

            // Sonderbehandlung enum, da hier immer eine IEnumerable<ACValueItem> als Datasource generiert wird
            if (dsColACTypeInfo.ObjectType.IsEnum)
            {
                SetValue(SelectedValueBindingProperty, new Binding(Const.Value));
                Binding bindingEdit = new Binding();
                bindingEdit.Path = dsColACTypeInfo.ACIdentifier;
                bindingEdit.Mode = this.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
                bindingEdit.UpdateSourceTrigger = UpdateSourceTrigger;
                this.SelectedValueBinding = bindingEdit;
                if (!this.IsSet(DataGridColumn.HeaderProperty))
                    this.Header = dsColACTypeInfo.ACCaption;

                if (String.IsNullOrEmpty(DisplayMemberPath) && ItemTemplate == null)
                    DisplayMemberPath = Const.ACCaptionPrefix;
            }
            else
            {
                IACType dsColACTypeInfoColumn = dsColACTypeInfo;
                object dsColSourceColumn = null;
                string dsColPathColumn = "";
                Global.ControlModes dscolRightControlMode = Global.ControlModes.Hidden;

                if (!dsColACTypeInfo.ACUrlBinding(dataShowColumnsColumn[0].PropertyName, ref dsColACTypeInfoColumn, ref dsColSourceColumn, ref dsColPathColumn, ref dscolRightControlMode))
                {
                    return;
                }
                Binding bindingEdit = new Binding();
                bindingEdit.Path = dsColACTypeInfo.ACIdentifier;
                bindingEdit.Mode = BindingMode.TwoWay;
                bindingEdit.UpdateSourceTrigger = UpdateSourceTrigger;
                this.SelectedItemBinding = bindingEdit;
                if (!this.IsSet(DataGridColumn.HeaderProperty))
                    this.Header = dsColACTypeInfo.ACCaption;

                if (String.IsNullOrEmpty(DisplayMemberPath) && ItemTemplate == null)
                    DisplayMemberPath = dsColPathColumn;
            }

            ObjectDataProvider odp = new ObjectDataProvider();
            odp.ObjectInstance = dscSource;
            string param = dataSourceColumn.Substring(dataSourceColumn.IndexOf('(') + 1, dataSourceColumn.LastIndexOf(')') - (dataSourceColumn.IndexOf('(') + 1));
            if (param.StartsWith("#") && param.EndsWith("#"))
            {
                odp.MethodParameters.Add(param.Substring(1, param.Length - 2));
            }
            else
            {
                string[] paramList = param.Split(',');
                foreach (var param1 in paramList)
                {
                    odp.MethodParameters.Add(param1.Replace("\"", ""));
                }
            }
            odp.MethodName = dscPath.Substring(1);  // "!" bei Methodenname entfernen
            Binding bindingSource = new Binding();
            bindingSource.Source = odp;
            this.Bind(ItemsSourceProperty, bindingSource);

            RefreshReadOnlyProperty();
        }

        protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
        {
            VBComboBox comboBox = new VBComboBox()
            {
                Name = "CellTextBox"
            };
            if (_cellComboEditTheme.Value is { } theme)
            {
                comboBox.Theme = theme;
            }
            comboBox.PointerEntered += ComboBox_PointerEntered;
            comboBox.PointerExited += ComboBox_PointerExited;
            comboBox.IsEnabled = IsEditable;
            comboBox.ShowCaption = false;
            comboBox.VBContent = Const.Value;
            comboBox.UpdateSourceTrigger = UpdateSourceTrigger;
            if (!string.IsNullOrEmpty(this.VBContent) && !string.IsNullOrEmpty(this.VBShowColumns))
            {
                comboBox.VBContent = this.VBContent;
                comboBox.VBShowColumns = this.VBShowColumns;
            }
            ApplyColumnProperties(comboBox);
            return comboBox;
        }

        private void ComboBox_PointerEntered(object sender, PointerEventArgs e)
        {
            if (OwningGrid != null && !IsReadOnly)
            {
                //DataGridCellInfo currentCell = OwningGrid.CurrentCell;
                OwningGrid.BeginEdit();
                //System.Diagnostics.Debug.WriteLine("MouseLeave BeginEdit()");
            }
        }

        private void ComboBox_PointerExited(object sender, PointerEventArgs e)
        {
            if (OwningGrid != null && !IsReadOnly)
            {
                //DataGridCellInfo currentCell = OwningGrid.CurrentCell;
                OwningGrid.CommitEdit();
                //System.Diagnostics.Debug.WriteLine("MouseLeave CommitEdit()");
            }
        }

        /// <summary>
        /// Generates a new VBTextBlock element, then applies style and binding.
        /// </summary>
        /// <param name="cell">The DataGridCell parameter.</param>
        /// <param name="dataItem">The data item parameter.</param>
        /// <returns>Returns the VBTextBlock element.</returns>
        protected override Control GenerateElement(DataGridCell cell, object dataItem)
        {
            VBComboBox comboBox = new VBComboBox()
            {
                Name = "CellTextBlock"
            };
            if (_cellComboDefaultTheme.Value is { } theme)
            {
                comboBox.Theme = theme;
            }
            comboBox.IsEnabled = false;
            comboBox.ShowCaption = false;
            comboBox.VBContent = Const.Value;
            comboBox.UpdateSourceTrigger = UpdateSourceTrigger;
            if (!string.IsNullOrEmpty(this.VBContent) && !string.IsNullOrEmpty(this.VBShowColumns))
            {
                comboBox.VBContent = this.VBContent;
                comboBox.VBShowColumns = this.VBShowColumns;
            }
            ApplyColumnProperties(comboBox);
            return comboBox;
        }

        private void ApplyColumnProperties(VBComboBox comboBox)
        {
            if (SelectedItemBinding != null)
                comboBox.Bind(VBComboBox.SelectedItemProperty, SelectedItemBinding);
            comboBox.SetValue(VBComboBox.SelectedValueBindingProperty, SelectedValueBinding);
            comboBox.SetValue(TextSearch.TextProperty, TextBinding);
            DataGridHelper.SyncColumnProperty(this, comboBox, VBComboBox.SelectedValueBindingProperty, SelectedValueBindingProperty);
            if (string.IsNullOrEmpty(DisplayMemberPath) && ItemTemplate == null)
            {
                comboBox.ItemTemplate = new FuncDataTemplate<object>((value, namescope) =>
                                    new TextBlock
                                    {
                                        [!TextBlock.TextProperty] = new Binding(DisplayMemberPath),
                                    });
            }
            else if (ItemTemplate != null)
            {
                comboBox.ItemTemplate = ItemTemplate;
            }
            DataGridHelper.SyncColumnProperty(this, comboBox, VBComboBox.ItemsSourceProperty, ItemsSourceProperty);
        }

        private static void ApplyBinding(IBinding binding, AvaloniaObject target, AvaloniaProperty property)
        {
            if (binding != null)
            {
                target.Bind(property, binding);
            }
            else
            {
                target.ClearAllBindings();
                target.ClearBinding(property);
            }
        }

        /// <summary>
        /// Refreshes a read only properties.
        /// </summary>
        /// <param name="newReadOnlyState">The new read only state.</param>
        public void RefreshReadOnlyProperty(short newReadOnlyState = -1)
        {
            DataGridHelper.RefreshReadOnlyProperty(this, newReadOnlyState);
        }

        /// <summary>
        /// Deinitializes this control.
        /// </summary>
        /// <param name="bso">The bso parameter.</param>
        public void DeInitVBControl(IACComponent bso)
        {
            //BindingOperations.ClearBinding(this, DataGridComboBoxColumn.ItemsSourceProperty);
            //BindingOperations.ClearBinding(this, DataGridComboBoxColumn.SelectedValuePathProperty);
            this.ClearAllBindings();
            this.SelectedValueBinding = null;
            this.SelectedItemBinding = null;
            _ACColumnItem = null;
            _ColACTypeInfo = null;
        }
    }
}
