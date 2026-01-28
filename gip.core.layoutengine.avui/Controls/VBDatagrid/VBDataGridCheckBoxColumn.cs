using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Styling;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.ComponentModel;

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
        private readonly Lazy<ControlTheme> _cellCheckBoxEditTheme;
        private readonly Lazy<ControlTheme> _cellCheckBoxDefaultTheme;
        public VBDataGridCheckBoxColumn() : base()
        {
            if (this.OwningGrid != null)
            {
                _cellCheckBoxEditTheme = new Lazy<ControlTheme>(() =>
                    OwningGrid.TryFindResource("DataGridCheckBoxEditTheme", out var theme) ? (ControlTheme)theme : null);
                _cellCheckBoxDefaultTheme = new Lazy<ControlTheme>(() =>
                    OwningGrid.TryFindResource("DataGridCheckBoxDefaultTheme", out var theme) ? (ControlTheme)theme : null);
            }
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly StyledProperty<string> VBContentProperty = AvaloniaProperty.Register<VBDataGridCheckBoxColumn, string>(nameof(VBContent));

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
        public static readonly StyledProperty<bool> VBIsReadOnlyProperty = AvaloniaProperty.Register<VBDataGridCheckBoxColumn, bool>(nameof(VBIsReadOnly));
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

            if (!this.IsSet(DataGridColumn.WidthProperty))
                this.Width = new DataGridLength(100);

            RefreshReadOnlyProperty();
        }

        /// <summary>
        /// Generates a new VBCheckBox element.
        /// </summary>
        /// <param name="cell">The DataGridCell parameter.</param>
        /// <param name="dataItem">The data item parameter.</param>
        /// <returns>Returns the VBCheckBox element.</returns>
        protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
        {
            if (!String.IsNullOrEmpty(VBContent) && ACColumnItem == null)
                ACColumnItem = new ACColumnItem(VBContent);
            VBCheckBox checkBox = GenerateCheckBox(true, cell);
            checkBox.Name = "CellTextBlock";
            if (_cellCheckBoxEditTheme.Value is { } theme)
            {
                checkBox.Theme = theme;
            }
            checkBox.ShowCaption = false;
            return checkBox;
        }

        /// <summary>
        /// Generates a new VBCheckBox element.
        /// </summary>
        /// <param name="cell">The DataGridCell parameter.</param>
        /// <param name="dataItem">The data item parameter.</param>
        /// <returns>Returns the VBCheckBox element.</returns>
        protected override Control GenerateElement(DataGridCell cell, object dataItem)
        {
            bool isEnabled = false;
            if (!String.IsNullOrEmpty(VBContent) && ACColumnItem == null)
                ACColumnItem = new ACColumnItem(VBContent);
            VBCheckBox checkBox = GenerateCheckBox(false, cell);
            checkBox.Name = "CellTextBox";
            if (_cellCheckBoxDefaultTheme.Value is { } theme)
            {
                checkBox.Theme = theme;
            }
            if (EnsureOwningGridViaReflection())
            {
                if (cell.GetRowIndexViaReflection() != -1 
                    && cell.GetColumnIndexViaReflection() != -1
                    && cell.GetOwningRowViaReflection() != null
                    && cell.GetOwningRowViaReflection().GetSlotViaReflection() == this.OwningGrid.GetCurrentSlotViaReflection()
                    && cell.GetColumnIndexViaReflection() == this.OwningGrid.GetCurrentColumnIndexViaReflection())
                {
                    isEnabled = true;
                    if (CurrentCheckBox != null)
                    {
                        CurrentCheckBox.IsEnabled = false;
                    }
                    CurrentCheckBox = checkBox;
                }
            }
            checkBox.IsEnabled = isEnabled;
            checkBox.IsHitTestVisible = false;
            //ConfigureCheckBox(checkBoxElement);
            //if (Binding != null)
            //{
            //    checkBoxElement.Bind(BindingTarget, Binding);
            //}
            return checkBox;
        }



        private VBCheckBox GenerateCheckBox(bool isEditing, DataGridCell cell)
        {
            VBCheckBox checkBox = (cell != null) ? (cell.Content as VBCheckBox) : null;
            if (checkBox == null)
            {
                checkBox = new VBCheckBox();
                if (!isEditing && this.IsReadOnly)
                    checkBox.IsEnabled = false;
            }

            //Style style = new Style(typeof(DataGridCell), cell.Style);
            //Trigger trigger = new Trigger() { Property = DataGridCell.IsMouseOverProperty, Value = true };
            //trigger.Setters.Add(new Setter(DataGridCell.IsEditingProperty, true));
            //style.Triggers.Add(trigger);
            //cell.Style = style;

            ApplyBinding(checkBox, CheckBox.IsCheckedProperty);
            return checkBox;
        }

        private void ConfigureCheckBox(CheckBox checkBox)
        {
            DataGridHelper.SyncColumnProperty(this, checkBox, IsThreeStateProperty);
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

        /// <summary>
        /// Calls the private EnsureOwningGrid method from the base class via reflection.
        /// </summary>
        /// <returns>Returns true if the owning grid is valid, false otherwise.</returns>
        private bool EnsureOwningGridViaReflection()
        {
            try
            {
                var method = this.GetType().BaseType?.GetMethod("EnsureOwningGrid", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (method != null)
                {
                    var result = method.Invoke(this, null);
                    return result is bool boolResult ? boolResult : false;
                }
                
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets or sets the current checkbox using reflection to access the private field from the base class.
        /// </summary>
        private VBCheckBox CurrentCheckBox
        {
            get
            {
                try
                {
                    var @field = this.GetType().BaseType?.GetField("_currentCheckBox", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (@field != null)
                    {
                        return @field.GetValue(this) as VBCheckBox;
                    }
                    
                    return null;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    var @field = this.GetType().BaseType?.GetField("_currentCheckBox", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (@field != null)
                    {
                        @field.SetValue(this, value);
                    }
                }
                catch (Exception)
                {
                    // Silently ignore reflection errors
                }
            }
        }
    }
}
