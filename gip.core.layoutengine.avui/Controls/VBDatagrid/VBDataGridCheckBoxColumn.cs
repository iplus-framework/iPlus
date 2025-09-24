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
    /// Represents a <see cref="gip.core.layoutengine.avui.VBDataGrid"/> column that host a <see cref="VBCheckBox"/> control in its cells.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine <see cref="gip.core.layoutengine.avui.VBDataGrid"/>-Spalte dar, die ein <see cref="VBCheckBox"/>-Steuerelement in ihren Zellen enthält.
    /// </summary>
    public class VBDataGridCheckBoxColumn : DataGridCheckBoxColumn, IGriColumn
    {
        static VBDataGridCheckBoxColumn()
        {
            ElementStyleProperty.OverrideMetadata(typeof(VBDataGridCheckBoxColumn), new FrameworkPropertyMetadata(VBDefaultElementStyle));
            EditingElementStyleProperty.OverrideMetadata(typeof(VBDataGridCheckBoxColumn), new FrameworkPropertyMetadata(VBDefaultEditingElementStyle));
        }

        private static Style _defaultElementStyle;
        /// <summary>
        /// Gets the style of VBDefault element.
        /// </summary>
        public static Style VBDefaultElementStyle
        {
            get
            {
                if (_defaultElementStyle == null)
                {
                    Style style = new Style(typeof(VBCheckBox));
                    style.BasedOn = ControlManager.GetStyleOfTheme(VBCheckBox.StyleInfoList);

                    // When not in edit mode, the end-user should not be able to toggle the state 
                    style.Setters.Add(new Setter(UIElement.IsHitTestVisibleProperty, false));
                    style.Setters.Add(new Setter(UIElement.FocusableProperty, false));
                    style.Setters.Add(new Setter(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center));
                    style.Setters.Add(new Setter(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Top));

                    style.Seal();
                    _defaultElementStyle = style;
                }

                return _defaultElementStyle;
            }
        }

        private static Style _defaultEditingElementStyle;
        /// <summary>
        /// Gets the style of default editing element.
        /// </summary>
        public static Style VBDefaultEditingElementStyle
        {
            get
            {
                if (_defaultEditingElementStyle == null)
                {
                    Style style = new Style(typeof(VBCheckBox));
                    style.BasedOn = ControlManager.GetStyleOfTheme(VBCheckBox.StyleInfoList);

                    style.Setters.Add(new Setter(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center));
                    style.Setters.Add(new Setter(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Top));

                    style.Seal();
                    _defaultEditingElementStyle = style;
                }

                return _defaultEditingElementStyle;
            }
        }


        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBDataGridCheckBoxColumn));

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
        /// Represents the dependency property for VBIsReadOnly.
        /// </summary>
        public static readonly DependencyProperty VBIsReadOnlyProperty
            = DependencyProperty.Register("VBIsReadOnly", typeof(bool), typeof(VBDataGridCheckBoxColumn));

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
            bool bIsInput = false;
            if (dsColACTypeInfo is ACClassProperty)
                bIsInput = (dsColACTypeInfo as ACClassProperty).IsInput;
            binding.Mode = bIsInput ? BindingMode.TwoWay : BindingMode.OneWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

            // ACCaption bevorzugen
            if (!string.IsNullOrEmpty(ACCaption))
                this.Header = this.Root().Environment.TranslateText(dataGrid.ContextACObject, ACCaption);
            else
                this.Header = dsColACTypeInfo.ACCaption;

            this.Binding = binding;

            ValueSource valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.WidthProperty);
            if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
                this.Width = 100;

            //valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.IsReadOnlyProperty);
            //if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
            //    this.IsReadOnly = !dataGrid.IsSetInVBDisabledColumns(acColumnItem.PropertyName);
            RefreshReadOnlyProperty();
        }

        /// <summary>
        /// Generates a new VBCheckBox element.
        /// </summary>
        /// <param name="cell">The DataGridCell parameter.</param>
        /// <param name="dataItem">The data item parameter.</param>
        /// <returns>Returns the VBCheckBox element.</returns>
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            if (!String.IsNullOrEmpty(VBContent) && ACColumnItem == null)
                ACColumnItem = new ACColumnItem(VBContent);
            return GenerateCheckBox(true, cell);
        }

        /// <summary>
        /// Generates a new VBCheckBox element.
        /// </summary>
        /// <param name="cell">The DataGridCell parameter.</param>
        /// <param name="dataItem">The data item parameter.</param>
        /// <returns>Returns the VBCheckBox element.</returns>
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            if (!String.IsNullOrEmpty(VBContent) && ACColumnItem == null)
                ACColumnItem = new ACColumnItem(VBContent);
            return GenerateCheckBox(false, cell);
        }

        private CheckBox GenerateCheckBox(bool isEditing, DataGridCell cell)
        {
            VBCheckBox checkBox = (cell != null) ? (cell.Content as VBCheckBox) : null;
            if (checkBox == null)
            {
                checkBox = new VBCheckBox();
                if (!isEditing && this.IsReadOnly)
                    checkBox.IsEnabled = false;
            }

            Style style = new Style(typeof(DataGridCell), cell.Style);
            Trigger trigger = new Trigger() { Property = DataGridCell.IsMouseOverProperty, Value = true };
            trigger.Setters.Add(new Setter(DataGridCell.IsEditingProperty, true));
            style.Triggers.Add(trigger);
            cell.Style = style;

            checkBox.IsThreeState = IsThreeState;
            ApplyStyle(isEditing, /* defaultToElementStyle = */ true, checkBox);
            ApplyBinding(checkBox, CheckBox.IsCheckedProperty);
            return checkBox;
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
            this.ClearAllBindings();
            this.Binding = null;
            _ACColumnItem = null;
            _ColACTypeInfo = null;
        }
    }
}
