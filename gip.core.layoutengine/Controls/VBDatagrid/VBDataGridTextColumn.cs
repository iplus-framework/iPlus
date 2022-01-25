using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
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

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a <see cref="layoutengine.VBDataGrid"/> column that hosts textual content in its cells.
    /// </summary>
    /// <summary>
    /// Stellt eine <see cref="layoutengine.VBDataGrid"/>-Spalte dar, die Textinhalte in ihren Zellen enthält.
    /// </summary>
    public class VBDataGridTextColumn : DataGridTextColumn, IGriColumn
    {
        static VBDataGridTextColumn()
        {
            ElementStyleProperty.OverrideMetadata(typeof(VBDataGridTextColumn), new FrameworkPropertyMetadata(VBDefaultElementStyle));
            EditingElementStyleProperty.OverrideMetadata(typeof(VBDataGridTextColumn), new FrameworkPropertyMetadata(VBDefaultEditingElementStyle));
            StringFormatProperty = ContentPropertyHandler.StringFormatProperty.AddOwner(typeof(VBDataGridTextColumn), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        }

        private static Style _defaultElementStyle;
        /// <summary>
        /// Gets the default element style.
        /// </summary>
        public static Style VBDefaultElementStyle
        {
            get
            {
                if (_defaultElementStyle == null)
                {
                    Style style = new Style(typeof(VBTextBlock));
                    style.BasedOn = ControlManager.GetStyleOfTheme(VBTextBlock.StyleInfoList);

                    style.Setters.Add(new Setter(VBTextBlock.MarginProperty, new Thickness(2.0, 0.0, 2.0, 0.0)));

                    style.Seal();
                    _defaultElementStyle = style;
                }

                return _defaultElementStyle;
            }
        }

        private static Style _defaultEditingElementStyle;
        /// <summary>
        /// Gets the default style of editing element.
        /// </summary>
        public static Style VBDefaultEditingElementStyle
        {
            get
            {
                if (_defaultEditingElementStyle == null)
                {
                    Style style = new Style(typeof(VBTextBox));
                    style.BasedOn = ControlManager.GetStyleOfTheme(VBTextBox.StyleInfoList);

                    style.Setters.Add(new Setter(VBTextBox.BorderThicknessProperty, new Thickness(0.0)));
                    style.Setters.Add(new Setter(VBTextBox.PaddingProperty, new Thickness(0.0)));

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
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBDataGridTextColumn));

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
            = DependencyProperty.Register("VBIsReadOnly", typeof(bool), typeof(VBDataGridTextColumn));

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
        /// Gets or sets the virutal column converter.
        /// </summary>
        public ConverterBaseTypesBaseSingle VirtualColumnConverter
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of VBDataGridTextColumn.
        /// </summary>
        public VBDataGridTextColumn() : base()
        {
            if (this.DataGridOwner != null)
            {
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

        private BindingBase BindingTooltip { get; set; }
        private TextAlignment? TBAlignment { get; set; }


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
            if (_ColACTypeInfo != null)
            {
                if (TypeAnalyser.IsNumericType(ColACType.ObjectType))
                    TBAlignment = TextAlignment.Right;
            }

            if (isShowColumnAMethod)
            {
                IACType dscACTypeInfo = null;
                object dscSource = null;
                string dscPath = "";
                Global.ControlModes dscRightControlMode = Global.ControlModes.Hidden;
                if (!dataGrid.ContextACObject.ACUrlBinding(_ACColumnItem.PropertyName, ref dscACTypeInfo, ref dscSource, ref dscPath, ref dscRightControlMode))
                {
                    this.Root().Messages.LogDebug("Error00004", "VBDataGrid", dataGrid.VBSource + " " + VBContent + "-" + dsColACTypeInfo.ACIdentifier);
                    //this.Root().Messages.Error(dataGrid.ContextACObject, "Error00004", "VBDataGrid", dataGrid.VBSource, VBContent + "-" + dsColACTypeInfo.ACIdentifier);
                    return;
                }
                RightControlMode = dscRightControlMode;
                InitWithMethodBinding(_ACColumnItem, dsColPath);
            }
            else
            {
                RightControlMode = dsColRightControlMode;
                InitWithBinding(dsColACTypeInfo, _ACColumnItem, dsColSource, dsColPath);
            }
        }

        private void InitWithBinding(IACType dsColACTypeInfo, ACColumnItem acColumnItem, object dsColSource, string dsColPath, string methodName = "")
        {
            VBDataGrid dataGrid = this.DataGridOwner as VBDataGrid;
            if (dataGrid == null)
                return;
            if (dataGrid.ContextACObject == null)
                return;

            List<ACColumnItem> dataShowColumnsColumn = dsColACTypeInfo.GetColumns();
            IACType dsColACTypeInfoColumn = dsColACTypeInfo;
            //object dsColSourceColumn = null;
            //string dsColPathColumn = "";
            //Global.ControlModes dscolRightControlMode = Global.ControlModes.Hidden;

            //if (dsColACTypeInfo.ObjectType.IsEnum)
            //{
            //    //ACQueryDefinition = this.Root().Queries.CreateQuerybyClass(null, dscACTypeInfo.ValueTypeACClass.PrimaryNavigationquery(), dscACTypeInfo.ValueTypeACClass.PrimaryNavigationquery().ACIdentifier, Global.ACStartTypes.None);
            //}
            //else
            //{
            Binding binding = new Binding();
            binding.Path = new PropertyPath(dsColPath);
            ACClassProperty propertyInfo = dsColACTypeInfo as ACClassProperty;
            if (propertyInfo != null)
                binding.Mode = propertyInfo.IsInput ? BindingMode.TwoWay : BindingMode.OneWay;
            if (!String.IsNullOrEmpty(StringFormat))
                binding.StringFormat = StringFormat;
            binding.ConverterCulture = System.Globalization.CultureInfo.CurrentCulture; //System.Globalization.CultureInfo.CurrentUICulture;
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

            if (!String.IsNullOrEmpty(methodName))
            {
                if (VirtualColumnConverter != null)
                {
                    VirtualColumnConverter.ACUrlCommand = methodName;
                    binding.Converter = VirtualColumnConverter;
                }
                else
                {
                    ConverterString converter = new ConverterString();
                    converter.ACUrlCommand = methodName;
                    binding.Converter = converter;
                }
            }
            else
            {
                if (VirtualColumnConverter != null)
                {
                    binding.Converter = VirtualColumnConverter;
                }
            }
            this.Binding = binding;
            //}

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

            if (TBAlignment.HasValue 
                && TBAlignment.Value == TextAlignment.Right 
                && !String.IsNullOrEmpty(this.StringFormat))
            {
                binding = new Binding();
                binding.Path = new PropertyPath(dsColPath);
                binding.Mode = BindingMode.OneWay;
                this.BindingTooltip = binding;
            }

            //valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.IsReadOnlyProperty);
            //if ((valueSource == null) || (valueSource.BaseValueSource != BaseValueSource.Local))
            //    this.IsReadOnly = !dataGrid.IsSetInVBDisabledColumns(acColumnItem.PropertyName);
            RefreshReadOnlyProperty();
        }

        private bool InitWithMethodBinding(ACColumnItem acColumnItem, string dsColPath)
        {
            VBDataGrid dataGrid = this.DataGridOwner as VBDataGrid;
            if (dataGrid == null)
                return false;
            if (dataGrid.ContextACObject == null)
                return false;

            string param = acColumnItem.PropertyName.Substring(acColumnItem.PropertyName.IndexOf('(') + 1, acColumnItem.PropertyName.LastIndexOf(')') - (acColumnItem.PropertyName.IndexOf('(') + 1));
            string propertyNameOfParam = param;
            if (param.StartsWith("#") && param.EndsWith("#"))
                propertyNameOfParam = param.Substring(1, param.Length - 2);

            IACType dsParamACTypeInfo = dataGrid.ItemsSourceACTypeInfo;
            object dsParamSource = null;
            string dsParamPath = "";
            Global.ControlModes dsParamRightControlMode = Global.ControlModes.Hidden;
            if (!dataGrid.ItemsSourceACTypeInfo.ACUrlBinding(propertyNameOfParam, ref dsParamACTypeInfo, ref dsParamSource, ref dsParamPath, ref dsParamRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00005", "VBDataGrid", propertyNameOfParam + " " + VBContent);
                //this.Root().Messages.Error(dataGrid.ContextACObject, "Error00005", "VBDataGrid", propertyNameOfParam, VBContent);
                return false;
            }

            ACColumnItem colProperty = new ACColumnItem(propertyNameOfParam);
            InitWithBinding(dsParamACTypeInfo, colProperty, null, dsParamPath, dsColPath);

            return true;
        }


        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            VBTextBox textBox = new VBTextBox();
            textBox.ShowCaption = false;
            SyncProperties(textBox);
            ApplyStyle(true, false, textBox);
            ApplyBinding(Binding, textBox, TextBox.TextProperty);
            if (this.BindingTooltip != null)
                ApplyBinding(BindingTooltip, textBox, TextBox.ToolTipProperty);
            return textBox;
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
            //return base.GenerateElement(cell, dataItem);

            VBTextBlock textBlock = new VBTextBlock();
            SyncProperties(textBlock);
            //if (BindingTooltip == null)
            textBlock.IsEnabled = !this.IsReadOnly;
            ApplyStyle(/* isEditing = */ false, /* defaultToElementStyle = */ false, textBlock);
            ApplyBinding(Binding, textBlock, VBTextBlock.TextProperty);
            if (this.BindingTooltip != null)
                ApplyBinding(BindingTooltip, textBlock, VBTextBlock.ToolTipProperty);
            return textBlock;
        }

        private void SyncProperties(FrameworkElement e)
        {
            if (e is TextBlock)
            {
                DataGridHelper.SyncColumnProperty(this, e, TextBlock.FontFamilyProperty, FontFamilyProperty);
                DataGridHelper.SyncColumnProperty(this, e, TextBlock.FontSizeProperty, FontSizeProperty);
                DataGridHelper.SyncColumnProperty(this, e, TextBlock.FontStyleProperty, FontStyleProperty);
                DataGridHelper.SyncColumnProperty(this, e, TextBlock.FontWeightProperty, FontWeightProperty);
                DataGridHelper.SyncColumnProperty(this, e, TextBlock.ForegroundProperty, ForegroundProperty);
                DataGridHelper.SyncColumnProperty(this, e, ContentPropertyHandler.StringFormatProperty, StringFormatProperty);
            }
            else if (e is TextBox)
            {
                DataGridHelper.SyncColumnProperty(this, e, TextElement.FontFamilyProperty, FontFamilyProperty);
                DataGridHelper.SyncColumnProperty(this, e, TextElement.FontSizeProperty, FontSizeProperty);
                DataGridHelper.SyncColumnProperty(this, e, TextElement.FontStyleProperty, FontStyleProperty);
                DataGridHelper.SyncColumnProperty(this, e, TextElement.FontWeightProperty, FontWeightProperty);
                DataGridHelper.SyncColumnProperty(this, e, TextElement.ForegroundProperty, ForegroundProperty);
                DataGridHelper.SyncColumnProperty(this, e, ContentPropertyHandler.StringFormatProperty, StringFormatProperty);
            }
        }

        internal void ApplyBinding(BindingBase binding, DependencyObject target, DependencyProperty property)
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
                try
                {
                    element.Style = style;
                    if ((style == VBDefaultElementStyle || style == VBDefaultEditingElementStyle) && TBAlignment.HasValue)
                    {
                        if (style == VBDefaultElementStyle)
                            (element as TextBlock).TextAlignment = TBAlignment.Value;
                        else if (style == VBDefaultEditingElementStyle)
                            (element as TextBox).TextAlignment = TBAlignment.Value;
                    }
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    if (e.InnerException != null && e.InnerException.Message != null)
                        msg += " Inner:" + e.InnerException.Message;

                    if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                        datamodel.Database.Root.Messages.LogException("VBDataGridTextColumn", "ApplyStyle", msg);
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
            BindingOperations.ClearAllBindings(this);
            this.Binding = null;
            _ACColumnItem = null;
            _ColACTypeInfo = null;
        }
    }
}
