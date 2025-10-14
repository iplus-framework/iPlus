using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Styling;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.ComponentModel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a <see cref="gip.core.layoutengine.avui.VBDataGrid"/> column that host a content of <see cref="DateTime"/> items in its cells.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine <see cref="gip.core.layoutengine.avui.VBDataGrid"/>-Spalte dar, die einen Inhalt von <see cref="DateTime"/>-Elementen in ihren Zellen enthält.
    /// </summary>
    public class VBDataGridDateTimeColumn : DataGridTextColumn, IGriColumn
    {
        private readonly Lazy<ControlTheme> _cellPickerTheme;
        private readonly Lazy<ControlTheme> _cellTextBlockTheme;
        public VBDataGridDateTimeColumn() : base()
        {
            if (this.OwningGrid != null)
            {
                _cellPickerTheme = new Lazy<ControlTheme>(() =>
                    OwningGrid.TryFindResource("DataGridCellDateTimeTheme", out var theme) ? (ControlTheme)theme : null);
                _cellTextBlockTheme = new Lazy<ControlTheme>(() =>
                    OwningGrid.TryFindResource("DataGridCellTextBlockTheme", out var theme) ? (ControlTheme)theme : null);
            }
        }

        #region VBLogic
        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty = AvaloniaProperty.Register<VBDataGridDateTimeColumn, string>(nameof(VBContent));

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
        public static readonly StyledProperty<bool> VBIsReadOnlyProperty = AvaloniaProperty.Register<VBDataGridDateTimeColumn, bool>(nameof(VBIsReadOnly));
        /// <summary>
        /// Determines is column is read only or not. The true value is for readonly.
        /// </summary>
        [Category("VBControl")]
        public bool VBIsReadOnly
        {
            get { return (bool)GetValue(VBIsReadOnlyProperty); }
            set { SetValue(VBIsReadOnlyProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for StringFormat.
        /// </summary>
        public static readonly AttachedProperty<string> StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner<VBDataGridDateTimeColumn>();

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

        public static readonly StyledProperty<DateTimeFormat> FormatDTPickerProperty = AvaloniaProperty.Register<VBDataGridDateTimeColumn, DateTimeFormat>(nameof(FormatDTPicker));
        public DateTimeFormat FormatDTPicker
        {
            get { return (DateTimeFormat)GetValue(FormatDTPickerProperty); }
            set { SetValue(FormatDTPickerProperty, value); }
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

        private bool _UpdateSourceIfIndexer = false;

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
            if (dsColPath.IndexOf('[') >= 0)
                _UpdateSourceIfIndexer = true;
            bool bIsInput = false;
            if (dsColACTypeInfo is ACClassProperty)
                bIsInput = (dsColACTypeInfo as ACClassProperty).IsInput;
            binding.Mode = bIsInput ? BindingMode.TwoWay : BindingMode.OneWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.LostFocus;

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

            if (VirtualColumnConverter != null)
            {
                binding.Converter = VirtualColumnConverter;
            }
            if (!String.IsNullOrEmpty(StringFormat))
                binding.StringFormat = StringFormat;
            binding.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture; //System.Globalization.CultureInfo.CurrentUICulture;
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

            //valueSource = AvaloniaPropertyHelper.GetValueSource(this, DataGridColumn.IsReadOnlyProperty);
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
            textBlock.IsEnabled = !this.IsReadOnly;
            ApplyBinding(Binding, textBlock, VBTextBlock.TextProperty);
            //if (this.BindingTooltip != null)
            //    ApplyBinding(BindingTooltip, textBlock, ToolTip.TipProperty);
            return textBlock;
        }

        protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
        {
            VBDateTimePicker dateTimePicker = new VBDateTimePicker()
            {
                Name = "CellTextBox"
            };
            if (_cellPickerTheme.Value is { } theme)
            {
                dateTimePicker.Theme = theme;
            }
            dateTimePicker.ShowCaption = false;
            SyncProperties(dateTimePicker);
            //ValueSource valueSource = AvaloniaPropertyHelper.GetValueSource(this, VBDataGridDateTimeColumn.FormatDTPickerProperty);
            //if ((valueSource != null) && (valueSource.BaseValueSource == BaseValueSource.Local || valueSource.BaseValueSource == BaseValueSource.Style))
            if (IsSet(FormatDTPickerProperty))
                dateTimePicker.Format = this.FormatDTPicker;
            ApplyBinding(Binding, dateTimePicker, VBDateTimePicker.SelectedDateProperty);
            if (_UpdateSourceIfIndexer)
                dateTimePicker.UpdateSourceIfIndexer = _UpdateSourceIfIndexer;
            return dateTimePicker;
        }

        private void SyncProperties(AvaloniaObject content)
        {
            DataGridHelper.SyncColumnProperty(this, content, FontFamilyProperty);
            DataGridHelper.SyncColumnProperty(this, content, FontSizeProperty);
            DataGridHelper.SyncColumnProperty(this, content, FontStyleProperty);
            DataGridHelper.SyncColumnProperty(this, content, FontWeightProperty);
            DataGridHelper.SyncColumnProperty(this, content, ForegroundProperty);
            if (content is TextBlock)
            {
                DataGridHelper.SyncColumnProperty(this, content, ContentPropertyHandler.StringFormatProperty, StringFormatProperty);
            }
            //else if (content is VBDateTimePicker)
            //{
            //    DataGridHelper.SyncColumnProperty(this, content, ContentPropertyHandler.StringFormatProperty, StringFormatProperty);
            //}
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

        protected override void RefreshCellContent(Control element, string propertyName)
        {
            DataGridCell cell = element as DataGridCell;
            if (cell != null && _ACColumnItem != null && _ColACTypeInfo != null)
            {
                VBDateTimePicker comboBox = cell.Content as VBDateTimePicker;
                if (comboBox != null && propertyName == nameof(Binding))
                    ApplyBinding(Binding, comboBox, VBDateTimePicker.SelectedDateProperty);
                else
                    base.RefreshCellContent(element, propertyName);
            }
            else
            {
                base.RefreshCellContent(element, propertyName);
            }
        }
        #endregion


        #region Editing
        protected override object PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
        {
            VBDateTimePicker dateTimePicker = editingElement as VBDateTimePicker;
            if (dateTimePicker != null)
            {
                dateTimePicker.Focus();
                return dateTimePicker.SelectedDate;
            }
            return null;
        }

        protected override void CancelCellEdit(Control editingElement, object uneditedValue)
        {
            base.CancelCellEdit(editingElement, uneditedValue);
        }

        public void OnInput(KeyEventArgs e)
        {
            throw new NotImplementedException();
            //DataGridCell cell = e.Source as DataGridCell;
            //if (OwningGrid != null
            //    && cell != null
            //    && !cell.IsEdit
            //    && e != null
            //    && e.Key >= Key.D0
            //    && e.Key < Key.Z)
            //{
            //    //OwningGrid.CurrentColumn.
            //    DataGridCellInfo currentCell = OwningGrid.CurrentCell;
            //    if (currentCell.IsValid)
            //    {
            //        BeginEdit(e, false);

            //        //
            //        // The TextEditor for the TextBox establishes contact with the IME
            //        // engine lazily at background priority. However in this case we
            //        // want to IME engine to know about the TextBox in earnest before
            //        // PostProcessing this input event. Only then will the IME key be
            //        // recorded in the TextBox. Hence the call to synchronously drain
            //        // the Dispatcher queue.
            //        //
            //        Dispatcher.Invoke(delegate () { }, System.Windows.Threading.DispatcherPriority.Background);
            //    }
            //}
        }

        //internal void BeginEdit(KeyEventArgs e, bool handled)
        //{
        //    var owner = OwningGrid;
        //    if (owner != null)
        //    {
        //        if (owner.BeginEdit(e))
        //        {
        //            e.Handled |= handled;
        //        }
        //    }
        //}

        #endregion


        /// <summary>
        /// Deinitializes this control.
        /// </summary>
        /// <param name="bso">The bso parameter.</param>
        public void DeInitVBControl(IACComponent bso)
        {
            _ACColumnItem = null;
            _ColACTypeInfo = null;
            this.ClearAllBindings();
            this.Binding = null;
        }
    }
}
