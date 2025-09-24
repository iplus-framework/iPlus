using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a <see cref="gip.core.layoutengine.avui.VBDataGrid"/> column that host a content of <see cref="DateTime"/> items in its cells.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine <see cref="gip.core.layoutengine.avui.VBDataGrid"/>-Spalte dar, die einen Inhalt von <see cref="DateTime"/>-Elementen in ihren Zellen enthält.
    /// </summary>
    public class VBDataGridDateTimeColumn : DataGridBoundColumn, IGriColumn
    {
        static VBDataGridDateTimeColumn()
        {
            ElementStyleProperty.OverrideMetadata(typeof(VBDataGridDateTimeColumn), new FrameworkPropertyMetadata(DefaultElementStyle));
            EditingElementStyleProperty.OverrideMetadata(typeof(VBDataGridDateTimeColumn), new FrameworkPropertyMetadata(DefaultEditingElementStyle));
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner(typeof(VBDataGridDateTimeColumn), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        }

        #region VBLogic
        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBDataGridDateTimeColumn));

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
        [Category("VBControl")]
        public static readonly DependencyProperty VBIsReadOnlyProperty
            = DependencyProperty.Register("VBIsReadOnly", typeof(bool), typeof(VBDataGridDateTimeColumn));

        /// <summary>
        /// Represents the dependency property for StringFormat.
        /// </summary>
        public static readonly DependencyProperty StringFormatProperty;
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

        public static readonly DependencyProperty FormatDTPickerProperty = DependencyProperty.Register("FormatDTPicker", typeof(DateTimeFormat), typeof(VBDataGridDateTimeColumn));
        public DateTimeFormat FormatDTPicker
        {
            get { return (DateTimeFormat)GetValue(FormatDTPickerProperty); }
            set { SetValue(FormatDTPickerProperty, value); }
        }

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
            VBDataGrid dataGrid = this.DataGridOwner as VBDataGrid;
            if (dataGrid == null)
                return;
            if (dataGrid.ContextACObject == null)
                return;
            _ACColumnItem = acColumnItem;
            _ColACTypeInfo = dsColACTypeInfo;
            RightControlMode = dsColRightControlMode;

            Binding binding = new Binding();
            binding.Path = new PropertyPath(dsColPath);
            if (dsColPath.IndexOf('[') >= 0)
                _UpdateSourceIfIndexer = true;
            bool bIsInput = false;
            if (dsColACTypeInfo is ACClassProperty)
                bIsInput = (dsColACTypeInfo as ACClassProperty).IsInput;
            binding.Mode = bIsInput ? BindingMode.TwoWay : BindingMode.OneWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.LostFocus;

            ValueSource valueSource;
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

            if (VirtualColumnConverter != null)
            {
                binding.Converter = VirtualColumnConverter;
            }
            if (!String.IsNullOrEmpty(StringFormat))
                binding.StringFormat = StringFormat;
            binding.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture; //System.Globalization.CultureInfo.CurrentUICulture;
            this.Binding = binding;

            ACClassProperty propertyInfo = dsColACTypeInfo as ACClassProperty;
            valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.WidthProperty);
            if ((propertyInfo != null) && ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style))))
            {
                if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength > 20)
                    this.Width = 200;
                else if (propertyInfo.MaxLength.HasValue && propertyInfo.MaxLength.Value == 0)
                    this.Width = 100;
                else if (propertyInfo.MaxLength.HasValue)
                    this.Width = propertyInfo.MaxLength.Value * 10;
                else
                    this.Width = 100;
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

        #region Styles
        /// <summary>
        /// Gets the default element style.
        /// </summary>
        public static Style DefaultElementStyle
        {
            get
            {
                if (_defaultElementStyle == null)
                {
                    Style style = new Style(typeof(VBTextBlock));
                    style.BasedOn = ControlManager.GetStyleOfTheme(VBTextBlock.StyleInfoList);
                    style.Setters.Add(new Setter(TextBlock.MarginProperty, new Thickness(2.0, 0.0, 2.0, 0.0)));
                    style.Seal();
                    _defaultElementStyle = style;
                }
                return _defaultElementStyle;
            }
        }

        /// <summary>
        /// Gets the default style of editing element.
        /// </summary>
        public static Style DefaultEditingElementStyle
        {
            get
            {
                if (_defaultEditingElementStyle == null)
                {
                    Style style = new Style(typeof(VBDateTimePicker));
                    style.BasedOn = ControlManager.GetStyleOfTheme(VBDateTimePicker.StyleInfoList);
                    style.Setters.Add(new Setter(VBDateTimePicker.BorderThicknessProperty, new Thickness(0.0)));
                    style.Setters.Add(new Setter(VBDateTimePicker.PaddingProperty, new Thickness(0.0)));
                    style.Setters.Add(new Setter(VBDateTimePicker.ShowCaptionProperty, false));
                    style.Seal();
                    _defaultEditingElementStyle = style;
                }
                return _defaultEditingElementStyle;
            }
        }
        #endregion

        #region Element Generation
        /// <summary>
        /// Generates a new VBTextBlock element, then applies style and binding.
        /// </summary>
        /// <param name="cell">The DataGridCell parameter.</param>
        /// <param name="dataItem">The data item parameter.</param>
        /// <returns>Returns the VBTextBlock element.</returns>
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            VBTextBlock textBlock = new VBTextBlock();
            SyncProperties(textBlock);
            ApplyStyle(false, false, textBlock);
            ApplyBinding(textBlock, TextBlock.TextProperty);
            return textBlock;
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            VBDateTimePicker dateTimePicker = new VBDateTimePicker();
            SyncProperties(dateTimePicker);
            ValueSource valueSource = DependencyPropertyHelper.GetValueSource(this, VBDataGridDateTimeColumn.FormatDTPickerProperty);
            if ((valueSource != null) && (valueSource.BaseValueSource == BaseValueSource.Local || valueSource.BaseValueSource == BaseValueSource.Style))
                dateTimePicker.Format = this.FormatDTPicker;
            if (!String.IsNullOrEmpty(this.StringFormat))
            {
                dateTimePicker.FormatString = this.StringFormat;
                dateTimePicker.Format = DateTimeFormat.Custom;
            }
            ApplyStyle(true, false, dateTimePicker);
            ApplyBinding(dateTimePicker, VBDateTimePicker.SelectedDateProperty);
            if (_UpdateSourceIfIndexer)
                dateTimePicker.UpdateSourceIfIndexer = _UpdateSourceIfIndexer;
            return dateTimePicker;
        }

        private void SyncProperties(FrameworkElement e)
        {
        }

        internal void ApplyBinding(DependencyObject target, DependencyProperty property)
        {
            BindingBase binding = Binding;
            if (binding != null)
            {
                BindingOperations.SetBinding(target, property, binding);
            }
            else
            {
                BindingOperations.ClearBinding(target, property);
            }
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

        internal void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkElement element)
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

        protected override void RefreshCellContent(FrameworkElement element, string propertyName)   
        {
            DataGridCell cell = element as DataGridCell;
            if (cell != null && _ACColumnItem != null && _ColACTypeInfo != null)
            {
                bool isCellEditing = cell.IsEditing;
                if ((string.Compare(propertyName, "ElementStyle", StringComparison.Ordinal) == 0 && !isCellEditing) ||
                    (string.Compare(propertyName, "EditingElementStyle", StringComparison.Ordinal) == 0 && isCellEditing))
                {
                    base.RefreshCellContent(element, propertyName);
                }
                else
                {
                    VBDateTimePicker comboBox = cell.Content as VBDateTimePicker;
                    switch (propertyName)
                    {
                        case "Binding":
                            ApplyBinding(Binding, comboBox, VBDateTimePicker.SelectedDateProperty);
                            break;
                        default:
                            base.RefreshCellContent(element, propertyName);
                            break;
                    }
                }
            }
            else
            {
                base.RefreshCellContent(element, propertyName);
            }
        }
        #endregion


        #region Editing
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            VBDateTimePicker dateTimePicker = editingElement as VBDateTimePicker;
            if (dateTimePicker != null)
            {
                dateTimePicker.Focus();
                return dateTimePicker.SelectedDate;
            }
            return null;
        }

        protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
        {
            base.CancelCellEdit(editingElement, uneditedValue);
        }

        public void OnInput(InputEventArgs e)
        {
            KeyEventArgs kArgs = e as KeyEventArgs;
            DataGridCell cell = e.OriginalSource as DataGridCell;
            if (DataGridOwner != null
                && cell != null
                && !cell.IsEditing
                && kArgs != null
                && kArgs.Key >= Key.D0
                && kArgs.Key < Key.Z)
            {
                DataGridCellInfo currentCell = DataGridOwner.CurrentCell;
                if (currentCell.IsValid)
                {
                    BeginEdit(e, false);

                    //
                    // The TextEditor for the TextBox establishes contact with the IME
                    // engine lazily at background priority. However in this case we
                    // want to IME engine to know about the TextBox in earnest before
                    // PostProcessing this input event. Only then will the IME key be
                    // recorded in the TextBox. Hence the call to synchronously drain
                    // the Dispatcher queue.
                    //
                    Dispatcher.Invoke(delegate () { }, System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }

        internal void BeginEdit(InputEventArgs e, bool handled)
        {
            var owner = DataGridOwner;
            if (owner != null)
            {
                if (owner.BeginEdit(e))
                {
                    e.Handled |= handled;
                }
            }
        }

        //internal void BeginEdit(FrameworkElement editingElement, RoutedEventArgs e)
        //{
        //    // This call is to ensure that the tree and its bindings have resolved
        //    // before we proceed to code that relies on the tree being ready.
        //    if (editingElement != null)
        //    {
        //        editingElement.UpdateLayout();

        //        object originalValue = PrepareCellForEdit(editingElement, e);
        //        SetOriginalValue(editingElement, originalValue);
        //    }
        //}

        //private static void SetOriginalValue(DependencyObject obj, object value)
        //{
        //    obj.SetValue(OriginalValueProperty, value);
        //}

        //private static readonly DependencyProperty OriginalValueProperty =
        //            DependencyProperty.RegisterAttached("OriginalValue", typeof(object), typeof(VBDataGridDateTimeColumn), new FrameworkPropertyMetadata(null));

        #endregion


        #region Data 

        private static Style _defaultElementStyle;
        private static Style _defaultEditingElementStyle;

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
