using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Styling;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.ComponentModel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a <see cref="gip.core.layoutengine.avui.VBDataGrid"/> column that hosts a content of <see cref="ACValue"/> items in its cells.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine <see cref="gip.core.layoutengine.avui.VBDataGrid"/>-Spalte dar, die einen Inhalt von <see cref="ACValue"/>-Elementen in ihren Zellen enthält.
    /// </summary>
    public class VBDataGridACValueColumn : DataGridBoundColumn, IGriColumn
    {
        private readonly Lazy<ControlTheme> _cellACValueTheme;
        private readonly Lazy<ControlTheme> _cellTextBlockTheme;

        public VBDataGridACValueColumn() : base()
        {
            _cellACValueTheme = new Lazy<ControlTheme>(() =>
               OwningGrid.TryFindResource("DataGridCellACValueTheme", out var theme) ? (ControlTheme)theme : null);
            _cellTextBlockTheme = new Lazy<ControlTheme>(() =>
               OwningGrid.TryFindResource("DataGridCellTextBlockTheme", out var theme) ? (ControlTheme)theme : null);
        }

        #region VBLogic
        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty = AvaloniaProperty.Register<VBDataGridACValueColumn, string>(nameof(VBContent));

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
        public static readonly StyledProperty<bool> VBIsReadOnlyProperty = AvaloniaProperty.Register<VBDataGridACValueColumn, bool>(nameof(VBIsReadOnly));
        /// <summary>
        /// Determines is column is read only or not. The true value is for readonly.
        /// </summary>
        [Category("VBControl")]
        public bool VBIsReadOnly
        {
            get { return (bool)GetValue(VBIsReadOnlyProperty); }
            set { SetValue(VBIsReadOnlyProperty, value); }
        }

        public static readonly StyledProperty<string> VBEditorContentProperty = AvaloniaProperty.Register<VBDataGridACValueColumn, string>(nameof(VBEditorContent));

        /// <summary>
        /// Gets or sets the content for VBEditor.
        /// </summary>
        [Category("VBControl")]
        public string VBEditorContent
        {
            get { return (string)GetValue(VBEditorContentProperty); }
            set { SetValue(VBEditorContentProperty, value); }
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
                            Initialize(value, dsColACTypeInfo, dsColSource, dsColPath, dsColRightControlMode);
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
        /// Gets or sets the virtual column converter. 
        /// </summary>
        public ConverterBaseTypesBaseSingle VirtualColumnConverter
        {
            get;
            set;
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
        public void Initialize(ACColumnItem acColumnItem, IACType dsColACTypeInfo, object dsColSource, string dsColPath, Global.ControlModes dsColRightControlMode)
        {
            VBDataGrid dataGrid = this.OwningGrid as VBDataGrid;
            if (dataGrid == null)
                return;
            if (dataGrid.ContextACObject == null)
                return;
            _ACColumnItem = acColumnItem;
            _ColACTypeInfo = dsColACTypeInfo;
            RightControlMode = dsColRightControlMode;

            Binding binding = new Binding();
            binding.Path = dsColPath;
            binding.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture;
            bool bIsInput = false;
            if (dsColACTypeInfo is ACClassProperty)
                bIsInput = (dsColACTypeInfo as ACClassProperty).IsInput;
            binding.Mode = bIsInput ? BindingMode.TwoWay : BindingMode.OneWay;

            // ACCaption bevorzugen
            if (!string.IsNullOrEmpty(ACCaption))
                this.Header = this.Root().Environment.TranslateText(dataGrid.ContextACObject, ACCaption);
            else
            {
                if (!this.IsSet(DataGridColumn.HeaderProperty))
                    this.Header = dsColACTypeInfo.ACCaption;
            }

            if (VirtualColumnConverter != null)
            {
                binding.Converter = VirtualColumnConverter;
            }
            this.Binding = binding;

            ACClassProperty propertyInfo = dsColACTypeInfo as ACClassProperty;
            if (propertyInfo != null && !this.IsSet(DataGridColumn.WidthProperty))
            {
                if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength > 20)
                    this.Width = new DataGridLength(200);
                else if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength.Value == 0)
                    this.Width = new DataGridLength(100);
                else if (propertyInfo.MaxLength.HasValue)
                    this.Width = new DataGridLength(propertyInfo.MaxLength.Value * 10);
                else
                    this.Width = new DataGridLength(100);
                this.MinWidth = 60;
                this.Width = new DataGridLength(100, DataGridLengthUnitType.SizeToCells);
            }

            //valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.IsReadOnlyProperty);
            //if ((valueSource == null) || (valueSource.BaseValueSource != BaseValueSource.Local))
            //    this.IsReadOnly = !dataGrid.IsSetInVBDisabledColumns(acColumnItem.PropertyName);
            RefreshReadOnlyProperty();
        }

        /// <summary>
        /// Refreshes a read only properties.
        /// </summary>
        /// <param name="newReadOnlyState">The new read only state.</param>
        public void RefreshReadOnlyProperty(short newReadOnlyState = -1)
        {
            DataGridHelper.RefreshReadOnlyProperty(this, newReadOnlyState);
        }

        #endregion

        #region Element Generation
        /// <summary>
        /// Generates a new VBTextBlock element, then applies style and binding.
        /// </summary>
        /// <param name="cell">The DataGridCell parameter.</param>
        /// <param name="dataItem">The data item parameter.</param>
        /// <returns>Returns the VBTextBlock element.</returns>
        protected override Control GenerateElement(DataGridCell cell, object dataItem)
        {
            VBTextBlock textBlock = new VBTextBlock()
            {
                Name = "CellTextBlock"
            };
            if (_cellTextBlockTheme.Value is { } theme)
            {
                textBlock.Theme = theme;
            }
            SyncProperties(textBlock);
            ApplyBinding(textBlock, TextBlock.TextProperty);
            return textBlock;
        }

        /// <summary>
        /// Generates a new VBValueEditor element.
        /// </summary>
        /// <param name="cell">The DataGridCell parameter.</param>
        /// <param name="dataItem">The data item parameter.</param>
        /// <returns>Returns the VBValueEditor element.</returns>
        protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
        {
            VBDataGrid dataGrid = this.OwningGrid as VBDataGrid;
            if (dataGrid == null)
                return null;
            //if (dataGrid.ContextACObject == null)
            //return null;

            VBValueEditor valueEditor = new VBValueEditor(VBDataGrid.ItemsSource as ACValueList, dataItem as IACContainer, ColACType, dataGrid.ContextACObject);

            if (!(dataItem is IACContainer) && !string.IsNullOrEmpty(VBEditorContent))
            {
                IACObject item = dataItem as IACObject;
                if (item != null)
                {
                    IACContainer acValue = item.GetValue(VBEditorContent) as IACContainer;
                    valueEditor = new VBValueEditor(VBDataGrid.ItemsSource as ACValueList, acValue, ColACType, dataGrid.ContextACObject);
                }
            }
            //else
            //{
            //    IACObject test = dataItem as IACObject;
            //    IACContainer cont = test.GetValue(ColACType.ACIdentifier) as IACContainer;

            //    if (cont != null)
            //    {
            //        valueEditor = new VBValueEditor(VBDataGrid.ItemsSource as ACValueList, cont, ColACType, dataGrid.ContextACObject);
            //    }
            //}

            return valueEditor;
        }

        private void SyncProperties(AvaloniaObject content)
        {
        }

        internal void ApplyBinding(AvaloniaObject target, AvaloniaProperty property)
        {
            IBinding binding = Binding;
            ApplyBinding(binding, target, property);
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
        /// Refreshes the content of a cell.
        /// </summary>
        /// <param name="element">The element parameter.</param>
        /// <param name="propertyName">The property name parameter.</param>
        protected override void RefreshCellContent(Control element, string propertyName)
        {
            DataGridCell cell = element as DataGridCell;
            if (cell != null)
            {
                VBValueEditor comboBox = cell.Content as VBValueEditor;
                switch (propertyName)
                {
                    case "Binding":
                        //ApplyBinding(Binding, comboBox, VBValueEditor.SelectedDateProperty);
                        break;
                    default:
                        base.RefreshCellContent(element, propertyName);
                        break;
                }
            }
            else
            {
                base.RefreshCellContent(element, propertyName);
            }
        }
        #endregion


        #region Editing
        /// <summary>
        /// Prepares a cell for edit.
        /// </summary>
        /// <param name="editingElement">The editing element.</param>
        /// <param name="editingEventArgs">The editing event arguments.</param>
        /// <returns></returns>
        protected override object PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
        {
            VBValueEditor dateTimePicker = editingElement as VBValueEditor;
            if (dateTimePicker != null)
            {
                dateTimePicker.Focus();
                return dateTimePicker.Content;
            }
            return null;
        }
        #endregion


        #region Data 

        //private static Style _defaultElementStyle;
        //private static Style _defaultEditingElementStyle;

        #endregion

        /// <summary>
        /// Deinitializes this control.
        /// </summary>
        /// <param name="bso">The bso parameter.</param>
        public void DeInitVBControl(IACComponent bso)
        {
            this.ClearAllBindings();
            this.Binding = null;
            _ACColumnItem = null;
            _ColACTypeInfo = null;
        }
    }
}
