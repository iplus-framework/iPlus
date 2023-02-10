using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a <see cref="layoutengine.VBDataGrid"/> column that host <see cref="VBComboBox"/> control in its cells.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine <see cref="layoutengine.VBDataGrid"/>-Spalte dar, die das <see cref="VBComboBox"/>-Control in ihren Zellen hostet.
    /// </summary>
    public class VBDataGridComboBoxColumn : DataGridComboBoxColumn, IGriColumn
    {
        static VBDataGridComboBoxColumn()
        {
            ElementStyleProperty.OverrideMetadata(typeof(VBDataGridComboBoxColumn), new FrameworkPropertyMetadata(VBDefaultElementStyle));
            EditingElementStyleProperty.OverrideMetadata(typeof(VBDataGridComboBoxColumn), new FrameworkPropertyMetadata(VBDefaultEditingElementStyle));
        }

        private static Style _defaultElementStyle;
        /// <summary>
        /// Gets the default element style.
        /// </summary>
        public static Style VBDefaultElementStyle
        {
            get
            {
                return DefaultElementStyle;
            }
        }

        /// <summary>
        /// Gets the default style of editing element.
        /// </summary>
        public static Style VBDefaultEditingElementStyle
        {
            get
            {
                if (_defaultElementStyle == null)
                {
                    Style style = GetDefaultEditingElementStyle();
                    style.Seal();
                    _defaultElementStyle = style;
                }

                return _defaultElementStyle;
            }
        }

        public static Style GetDefaultEditingElementStyle()
        {
            Style style = new Style(typeof(VBComboBox));
            style.BasedOn = ControlManager.GetStyleOfTheme(VBComboBox.StyleInfoList);
            style.Setters.Add(new Setter(ComboBox.IsSynchronizedWithCurrentItemProperty, false));
            return style;
        }

        public static Style GetDefaultElementStyle()
        {
            Style style = new Style(typeof(ComboBox));
            style.Setters.Add(new Setter(ComboBox.IsSynchronizedWithCurrentItemProperty, false));
            //Style style = new Style(typeof(TextBlock));
            //style.BasedOn = ControlManager.GetStyleOfTheme(TextBlock.StyleInfoList);
            return style;
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBDataGridComboBoxColumn));

        /// <summary>
        /// Represents the property in which you enter the name of property that you want show in this column. Property must be in object, which is bounded to the VBDataGrid.
        /// </summary>
        [Category("VBControl")]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set
            {
                SetValue(VBContentProperty, value);
                this.ACColumnItem = new ACColumnItem(value);
            }
        }

        /// <summary>
        /// Represents the dependency property for VBIsReadOnlyProperty.
        /// </summary>
        public static readonly DependencyProperty VBIsReadOnlyProperty
            = DependencyProperty.Register("VBIsReadOnly", typeof(bool), typeof(VBDataGridComboBoxColumn));

        /// <summary>
        /// Determines is column is read only or not. The true value is for readonly.
        /// </summary>
        [Category("VBControl")]
        public bool VBIsReadOnly
        {
            get { return (bool)GetValue(VBIsReadOnlyProperty); }
            set { SetValue(VBIsReadOnlyProperty, value); }
        }


        public static readonly DependencyProperty IsEditableProperty
            = DependencyProperty.Register("IsEditable", typeof(bool), typeof(VBDataGridComboBoxColumn), new PropertyMetadata(true));
        [Category("VBControl")]
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        public static readonly DependencyProperty UpdateSourceTriggerProperty
                        = DependencyProperty.Register("UpdateSource", typeof(System.Windows.Data.UpdateSourceTrigger), typeof(VBDataGridComboBoxColumn), new PropertyMetadata(System.Windows.Data.UpdateSourceTrigger.LostFocus));
        [Category("VBControl")]
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get { return (UpdateSourceTrigger)GetValue(UpdateSourceTriggerProperty); }
            set { SetValue(UpdateSourceTriggerProperty, value); }
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
                return DataGridOwner as VBDataGrid;
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
                if ((this.DataGridOwner != null) && (this.DataGridOwner is VBDataGrid))
                {
                    VBDataGrid dataGrid = this.DataGridOwner as VBDataGrid;
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
            VBDataGrid dataGrid = this.DataGridOwner as VBDataGrid;
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
            VBDataGrid dataGrid = this.DataGridOwner as VBDataGrid;
            if (dataGrid == null)
                return;
            if (dataGrid.ContextACObject == null)
                return;

            List<ACColumnItem> dataShowColumnsColumn = dsColACTypeInfo.GetColumns();
            IACType dsColACTypeInfoColumn = dsColACTypeInfo;
            object dsColSourceColumn = null;
            string dsColPathColumn = "";
            Global.ControlModes dscolRightControlMode = Global.ControlModes.Hidden;

            ValueSource valueSource;
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
                    valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.HeaderProperty);
                    if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
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
                            binding.Path = new PropertyPath(dscPath);
                        }
                    }
                    else
                    {
                        binding.Source = dataGrid.Items;
                        binding.Path = new PropertyPath(dataSourceColumn);
                    }
                    BindingOperations.SetBinding(this, DataGridComboBoxColumn.ItemsSourceProperty, binding);
                }

                SetValue(DataGridComboBoxColumn.SelectedValuePathProperty, Const.Value);

                // Anzeige des Wertes in der Zelle
                this.DisplayMemberPath = dataShowColumnsColumn[0].PropertyName;

                Binding binding2 = new Binding();
                //binding2.Source = dsColSource;
                binding2.Path = new PropertyPath(dsColPath);
                binding2.Mode = BindingMode.TwoWay;
                binding2.UpdateSourceTrigger = UpdateSourceTrigger;
                //this.SelectedItemBinding = binding2;
                SelectedValueBinding = binding2;

                //Binding binding3 = new Binding();
                //binding3.Path = new PropertyPath(dataShowColumnsColumn[0].PropertyName);
                //binding3.Mode = BindingMode.OneWay;
                //TextBinding = binding3;
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
                    valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.HeaderProperty);
                    if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
                        this.Header = dsColACTypeInfo.ACCaption;
                }

                // Set Selected Item
                Binding bindingEdit = new Binding();
                bindingEdit.Path = new PropertyPath(dsColPath);
                bindingEdit.Mode = BindingMode.TwoWay;
                this.SelectedItemBinding = bindingEdit;
                bindingEdit.UpdateSourceTrigger = UpdateSourceTrigger;

                // Anzeige des Wertes in der Zelle
                if (String.IsNullOrEmpty(DisplayMemberPath))
                    DisplayMemberPath = dsColPathColumn;

                // Set Items-Source
                Binding bindingSource = new Binding();
                if (dscSource != null)
                {
                    bindingSource.Source = dscSource;
                    bindingSource.Path = new PropertyPath(dscPath);
                    bindingSource.Mode = BindingMode.OneWay;
                    BindingOperations.SetBinding(this, DataGridComboBoxColumn.ItemsSourceProperty, bindingSource);
                }
                else
                {
                    if (this.EditingElementStyle != null)
                    {
                        bindingSource.Path = new PropertyPath(dataSourceColumn);
                        Setter setter = new Setter();
                        setter.Property = ComboBox.ItemsSourceProperty;
                        setter.Value = bindingSource;
                        Style style = GetDefaultEditingElementStyle();
                        style.Setters.Add(setter);
                        this.EditingElementStyle = style;
                    }
                    if (this.ElementStyle != null)
                    {
                        Binding bindingSource2 = new Binding();
                        bindingSource2.Path = new PropertyPath(dataSourceColumn);
                        Setter setter = new Setter();
                        setter.Property = ComboBox.ItemsSourceProperty;
                        setter.Value = bindingSource2;
                        Style style = GetDefaultElementStyle();
                        style.Setters.Add(setter);
                        this.ElementStyle = style;
                    }
                }
            }

            ACClassProperty propertyInfo = dsColACTypeInfo as ACClassProperty;
            valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.WidthProperty);
            if ((propertyInfo != null) && ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style))))
            {
                if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength > 20)
                    this.Width = 200 + 20;
                else if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength.Value == 0)
                    this.Width = 100 + 20;
                else if (propertyInfo.MaxLength.HasValue)
                    this.Width = propertyInfo.MaxLength.Value * 10 + 20;
                else
                    this.Width = 100;
                this.MinWidth = 60;
                this.Width = new DataGridLength(100, DataGridLengthUnitType.SizeToCells);
            }

            //valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.IsReadOnlyProperty);
            //if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
            //    this.IsReadOnly = !dataGrid.IsSetInVBDisabledColumns(acColumnItem.PropertyName);
            RefreshReadOnlyProperty();
        }

        private void InitWithMethodBinding(IACType dsColACTypeInfo, ACColumnItem acColumnItem, object dsColSource, string dsColPath, object dscSource, string dscPath, IACType dscACTypeInfo, string dataSourceColumn, string acAccess)
        {
            VBDataGrid dataGrid = this.DataGridOwner as VBDataGrid;
            if (dataGrid == null)
                return;
            if (dataGrid.ContextACObject == null)
                return;

            ACClassProperty propertyInfo = dsColACTypeInfo as ACClassProperty;
            ValueSource valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.WidthProperty);
            if ((propertyInfo != null) && ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style))))
            {
                if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength > 20)
                    this.Width = 200 + 20;
                else if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength.Value == 0)
                    this.Width = 100 + 20;
                else if (propertyInfo.MaxLength.HasValue)
                    this.Width = propertyInfo.MaxLength.Value * 10 + 20;
                else
                    this.Width = 100;
                this.MinWidth = 60;
                this.Width = new DataGridLength(100, DataGridLengthUnitType.SizeToCells);
            }

            //valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.IsReadOnlyProperty);
            //if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
            //    this.IsReadOnly = dataGrid.IsSetInVBDisabledColumns(acColumnItem.PropertyName);

            List<ACColumnItem> dataShowColumnsColumn = dsColACTypeInfo.GetColumns();

            // Sonderbehandlung enum, da hier immer eine IEnumerable<ACValueItem> als Datasource generiert wird
            if (dsColACTypeInfo.ObjectType.IsEnum)
            {
                this.SelectedValuePath = Const.Value;
                Binding bindingEdit = new Binding();
                bindingEdit.Path = new PropertyPath(dsColACTypeInfo.ACIdentifier);
                bindingEdit.Mode = this.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
                bindingEdit.UpdateSourceTrigger = UpdateSourceTrigger;
                this.SelectedValueBinding = bindingEdit;
                valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.HeaderProperty);
                if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
                    this.Header = dsColACTypeInfo.ACCaption;

                this.DisplayMemberPath = Const.ACCaptionPrefix;
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
                bindingEdit.Path = new PropertyPath(dsColACTypeInfo.ACIdentifier);
                bindingEdit.Mode = BindingMode.TwoWay;
                bindingEdit.UpdateSourceTrigger = UpdateSourceTrigger;
                this.SelectedItemBinding = bindingEdit;
                valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.HeaderProperty);
                if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
                    this.Header = dsColACTypeInfo.ACCaption;

                this.DisplayMemberPath = dsColPathColumn;
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
            BindingOperations.SetBinding(this, DataGridComboBoxColumn.ItemsSourceProperty, bindingSource);

            RefreshReadOnlyProperty();
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            VBComboBox comboBox = new VBComboBox();
            comboBox.MouseEnter += ComboBox_MouseEnter;
            comboBox.MouseLeave += ComboBox_MouseLeave;
            comboBox.IsEditable = IsEditable;
            comboBox.ShowCaption = false;
            comboBox.VBContent = Const.Value;
            comboBox.UpdateSourceTrigger = UpdateSourceTrigger;
            if (!string.IsNullOrEmpty(this.VBContent) && !string.IsNullOrEmpty(this.VBShowColumns))
            {
                comboBox.VBContent = this.VBContent;
                comboBox.VBShowColumns = this.VBShowColumns;
            }
            ApplyStyle(true, false, comboBox);
            ApplyColumnProperties(comboBox);
            return comboBox;
        }

        private void ComboBox_MouseEnter(object sender, MouseEventArgs e)
        {
            if (DataGridOwner != null && !IsReadOnly)
            {
                DataGridCellInfo currentCell = DataGridOwner.CurrentCell;
                DataGridOwner.BeginEdit();
                System.Diagnostics.Debug.WriteLine("MouseLeave BeginEdit()");
            }
        }

        private void ComboBox_MouseLeave(object sender, MouseEventArgs e)
        {
            if (DataGridOwner != null && !IsReadOnly)
            {
                DataGridCellInfo currentCell = DataGridOwner.CurrentCell;
                DataGridOwner.CommitEdit();
                System.Diagnostics.Debug.WriteLine("MouseLeave CommitEdit()");
            }
        }

        /// <summary>
        /// Generates a new VBTextBlock element, then applies style and binding.
        /// </summary>
        /// <param name="cell">The DataGridCell parameter.</param>
        /// <param name="dataItem">The data item parameter.</param>
        /// <returns>Returns the VBTextBlock element.</returns>
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            //if (!String.IsNullOrEmpty(VBContent) && ACColumnItem == null)
            //    ACColumnItem = new ACColumnItem(VBContent);
            try
            {
                FrameworkElement element = base.GenerateElement(cell, dataItem);
                return element;
            }
            catch (Exception e)
            {
                string msg = e.Message;
                if (e.InnerException != null && e.InnerException.Message != null)
                    msg += " Inner:" + e.InnerException.Message;

                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                    datamodel.Database.Root.Messages.LogException("VBDataGridComboxBoxColumn", "GenerateElement", msg);
#if DEBUG
                System.Diagnostics.Debug.Print(e.Message);
#endif
            }
            return new TextBlock();
            //VBComboBox comboBox = new VBComboBox();
            //comboBox.ShowCaption = false;
            //comboBox.IsEnabled = false;
            //comboBox.IsEditable = false;
            //ApplyStyle(true, false, comboBox);
            //ApplyColumnProperties(comboBox);
            //return comboBox;
        }

        private void ApplyColumnProperties(VBComboBox comboBox)
        {
            ApplyBinding(SelectedItemBinding, comboBox, VBComboBox.SelectedItemProperty);
            ApplyBinding(SelectedValueBinding, comboBox, VBComboBox.SelectedValueProperty);
            ApplyBinding(TextBinding, comboBox, VBComboBox.TextProperty);

            DataGridHelper.SyncColumnProperty(this, comboBox, VBComboBox.SelectedValuePathProperty, SelectedValuePathProperty);
            DataGridHelper.SyncColumnProperty(this, comboBox, VBComboBox.DisplayMemberPathProperty, DisplayMemberPathProperty);
            DataGridHelper.SyncColumnProperty(this, comboBox, VBComboBox.ItemsSourceProperty, ItemsSourceProperty);

        }

        private void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkElement element)
        {
            Style style = PickStyle(isEditing, defaultToElementStyle);
            if (style != null)
            {
                element.Style = style;
            }
        }

        internal void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkContentElement element)
        {
            Style style = PickStyle(isEditing, defaultToElementStyle);
            if (style != null)
            {
                element.Style = style;
            }
        }

        private Style PickStyle(bool isEditing, bool defaultToElementStyle)
        {
            Style style = isEditing ? EditingElementStyle : ElementStyle;
            if (isEditing && defaultToElementStyle && (style == null))
            {
                style = ElementStyle;
            }
            return style;
        }

        private static void ApplyBinding(BindingBase binding, DependencyObject target, DependencyProperty property)
        {
            if (binding != null)
            {
                BindingOperations.SetBinding(target, property, binding);
            }
            else
            {
                BindingOperations.ClearBinding(target, property);
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
            BindingOperations.ClearBinding(this, DataGridComboBoxColumn.ItemsSourceProperty);
            BindingOperations.ClearBinding(this, DataGridComboBoxColumn.SelectedValuePathProperty);
            BindingOperations.ClearAllBindings(this);
            this.SelectedValueBinding = null;
            this.SelectedItemBinding = null;
            _ACColumnItem = null;
            _ColACTypeInfo = null;
        }
    }
}
