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
    /// Represents a <see cref="gip.core.layoutengine.avui.VBDataGrid"/> column that hosts template-specified content in its cells.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt eine <see cref="gip.core.layoutengine.avui.VBDataGrid"/>-Spalte dar, die vorlagenspezifische Inhalte in ihren Zellen enthält.
    /// </summary>
    public class VBDataGridTemplateColumn : DataGridTemplateColumn, IGriColumn
    {
        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBDataGridTemplateColumn));

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
            = DependencyProperty.Register("VBIsReadOnly", typeof(bool), typeof(VBDataGridTemplateColumn));

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

        /// <summary>
        /// Gets or sets the ACCaption
        /// </summary>
        public string ACCaption
        {
            get;
            set;
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

        /// <summary>
        /// Creates a new instance of VBDataGridTemplateColumn.
        /// </summary>
        public VBDataGridTemplateColumn()
            : base()
        {
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
            RightControlMode = dsColRightControlMode;

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

            //valueSource = DependencyPropertyHelper.GetValueSource(this, DataGridColumn.IsReadOnlyProperty);
            //if ((valueSource == null) || ((valueSource.BaseValueSource != BaseValueSource.Local) && (valueSource.BaseValueSource != BaseValueSource.Style)))
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
        /// Deinitializes this control.
        /// </summary>
        /// <param name="bso">The bso parameter.</param>
        public void DeInitVBControl(IACComponent bso)
        {
            this.ClearAllBindings();
            _ACColumnItem = null;
            _ColACTypeInfo = null;
        }
    }
}
