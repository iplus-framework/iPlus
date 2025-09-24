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
    /// Represents a <see cref="gip.core.layoutengine.avui.VBDataGrid"/> column that hosts a content of <see cref="ACValue"/> items in its cells.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine <see cref="gip.core.layoutengine.avui.VBDataGrid"/>-Spalte dar, die einen Inhalt von <see cref="ACValue"/>-Elementen in ihren Zellen enthält.
    /// </summary>
    public class VBDataGridACValueColumn : DataGridBoundColumn, IGriColumn
    {
        static VBDataGridACValueColumn()
        {
            ElementStyleProperty.OverrideMetadata(typeof(VBDataGridACValueColumn), new FrameworkPropertyMetadata(DefaultElementStyle));
            EditingElementStyleProperty.OverrideMetadata(typeof(VBDataGridACValueColumn), new FrameworkPropertyMetadata(DefaultEditingElementStyle));
        }

        #region VBLogic
        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBDataGridACValueColumn));

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
        public static readonly DependencyProperty VBIsReadOnlyProperty
            = DependencyProperty.Register("VBIsReadOnly", typeof(bool), typeof(VBDataGridACValueColumn));

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
        /// Gets or sets the content for VBEditor.
        /// </summary>
        [Category("VBControl")]
        public string VBEditorContent
        {
            get { return (string)GetValue(VBEditorContentProperty); }
            set { SetValue(VBEditorContentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for VBEditorContent.
        /// </summary>
        public static readonly DependencyProperty VBEditorContentProperty =
            DependencyProperty.Register("VBEditorContent", typeof(string), typeof(VBDataGridACValueColumn), new PropertyMetadata(""));


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
            binding.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture;
            bool bIsInput = false;
            if (dsColACTypeInfo is ACClassProperty)
                bIsInput = (dsColACTypeInfo as ACClassProperty).IsInput;
            binding.Mode = bIsInput ? BindingMode.TwoWay : BindingMode.OneWay;

            ValueSource valueSource;

            // ACCaption bevorzugen
            if (!string.IsNullOrEmpty(ACCaption))
                this.Header = this.Root().Environment.TranslateText(dataGrid.ContextACObject, ACCaption);
            else
            {
                valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.HeaderProperty);
                if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
                    this.Header = dsColACTypeInfo.ACCaption;
            }

            if (VirtualColumnConverter != null)
            {
                binding.Converter = VirtualColumnConverter;
            }
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
                    Style style = new Style(typeof(VBValueEditor));
                    style.Setters.Add(new Setter(VBValueEditor.BorderThicknessProperty, new Thickness(0.0)));
                    style.Setters.Add(new Setter(VBValueEditor.PaddingProperty, new Thickness(0.0)));
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
            ApplyStyle(false, false, textBlock, dataItem);
            ApplyBinding(textBlock, TextBlock.TextProperty);
            return textBlock;
        }

        /// <summary>
        /// Generates a new VBValueEditor element.
        /// </summary>
        /// <param name="cell">The DataGridCell parameter.</param>
        /// <param name="dataItem">The data item parameter.</param>
        /// <returns>Returns the VBValueEditor element.</returns>
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            VBDataGrid dataGrid = this.DataGridOwner as VBDataGrid;
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

            ApplyStyle(true, false, valueEditor, dataItem);
            return valueEditor;
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

        internal void ApplyStyle(bool isEditing, bool defaultToElementStyle, FrameworkElement element, object dataItem)
        {
            Style style = PickStyle(isEditing, defaultToElementStyle);
            if (style != null)
            {
                element.Style = style;
                IACContainer acValue = dataItem as IACContainer;

                if (acValue != null 
                    && acValue.ValueTypeACClass != null 
                    && (style == DefaultElementStyle || style == DefaultEditingElementStyle)
                    && TypeAnalyser.IsNumericType(acValue.ValueTypeACClass.ObjectType))
                {
                    if (style == DefaultElementStyle)
                        (element as TextBlock).TextAlignment = TextAlignment.Right;
                    else if (style == DefaultEditingElementStyle)
                        (element as VBValueEditor).TextAlignment = TextAlignment.Right;
                }
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

        /// <summary>
        /// Refreshes the content of a cell.
        /// </summary>
        /// <param name="element">The element parameter.</param>
        /// <param name="propertyName">The property name parameter.</param>
        protected override void RefreshCellContent(FrameworkElement element, string propertyName)
        {
            DataGridCell cell = element as DataGridCell;
            if (cell != null)
            {
                bool isCellEditing = cell.IsEditing;
                if ((string.Compare(propertyName, "ElementStyle", StringComparison.Ordinal) == 0 && !isCellEditing) ||
                    (string.Compare(propertyName, "EditingElementStyle", StringComparison.Ordinal) == 0 && isCellEditing))
                {
                    base.RefreshCellContent(element, propertyName);
                }
                else
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
        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
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

        private static Style _defaultElementStyle;
        private static Style _defaultEditingElementStyle;

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
