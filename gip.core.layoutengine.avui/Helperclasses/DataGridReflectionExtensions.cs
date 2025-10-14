using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Reflection;

namespace gip.core.layoutengine.avui.Helperclasses
{
    /// <summary>
    /// Extension class providing access to internal properties of DataGridCell, DataGridRow, DataGridColumnHeader and DataGrid via reflection.
    /// </summary>
    public static class DataGridReflectionExtensions
    {
        private static readonly Lazy<PropertyInfo> _cellRowIndexProperty = new Lazy<PropertyInfo>(() =>
            typeof(DataGridCell).GetProperty("RowIndex", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<PropertyInfo> _cellColumnIndexProperty = new Lazy<PropertyInfo>(() =>
            typeof(DataGridCell).GetProperty("ColumnIndex", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<PropertyInfo> _cellIsEditedProperty = new Lazy<PropertyInfo>(() =>
            typeof(DataGridCell).GetProperty("IsEdited", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<PropertyInfo> _cellOwningRowProperty = new Lazy<PropertyInfo>(() =>
            typeof(DataGridCell).GetProperty("OwningRow", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<PropertyInfo> _columnCellOwningColumnProperty = new Lazy<PropertyInfo>(() =>
            typeof(DataGridCell).GetProperty("OwningColumn", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<PropertyInfo> _rowSlotProperty = new Lazy<PropertyInfo>(() =>
            typeof(DataGridRow).GetProperty("Slot", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<PropertyInfo> _rowIsEditingProperty = new Lazy<PropertyInfo>(() =>
            typeof(DataGridRow).GetProperty("IsEditing", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<PropertyInfo> _columnHeaderOwningColumnProperty = new Lazy<PropertyInfo>(() =>
            typeof(DataGridColumnHeader).GetProperty("OwningColumn", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<PropertyInfo> _gridCurrentSlotProperty = new Lazy<PropertyInfo>(() =>
            typeof(DataGrid).GetProperty("CurrentSlot", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<PropertyInfo> _gridCurrentColumnIndexProperty = new Lazy<PropertyInfo>(() =>
            typeof(DataGrid).GetProperty("CurrentColumnIndex", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<PropertyInfo> _gridDataConnectionProperty = new Lazy<PropertyInfo>(() =>
            typeof(DataGrid).GetProperty("DataConnection", BindingFlags.NonPublic | BindingFlags.Instance));

        private static readonly Lazy<PropertyInfo> _dataConnectionCollectionViewProperty = new Lazy<PropertyInfo>(() =>
        {
            var dataConnectionType = typeof(DataGrid).Assembly.GetType("Avalonia.Controls.DataGridDataConnection");
            return dataConnectionType?.GetProperty("CollectionView", BindingFlags.Public | BindingFlags.Instance);
        });

        private static readonly Lazy<MethodInfo> _gridGetRowFromItemMethod = new Lazy<MethodInfo>(() =>
            typeof(DataGrid).GetMethod("GetRowFromItem", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(object) }, null));

        private static readonly Lazy<MethodInfo> _gridProcessCopyKeyMethod = new Lazy<MethodInfo>(() =>
            typeof(DataGrid).GetMethod("ProcessCopyKey", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(KeyModifiers) }, null));

        /// <summary>
        /// Gets the internal RowIndex property of the DataGridCell via reflection.
        /// </summary>
        /// <param name="cell">The DataGridCell instance.</param>
        /// <returns>The row index, or -1 if the property cannot be accessed.</returns>
        public static int GetRowIndexViaReflection(this DataGridCell cell)
        {
            try
            {
                var property = _cellRowIndexProperty.Value;
                if (property != null)
                {
                    var result = property.GetValue(cell);
                    return result is int index ? index : -1;
                }
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the internal ColumnIndex property of the DataGridCell via reflection.
        /// </summary>
        /// <param name="cell">The DataGridCell instance.</param>
        /// <returns>The column index, or -1 if the property cannot be accessed.</returns>
        public static int GetColumnIndexViaReflection(this DataGridCell cell)
        {
            try
            {
                var property = _cellColumnIndexProperty.Value;
                if (property != null)
                {
                    var result = property.GetValue(cell);
                    return result is int index ? index : -1;
                }
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static bool GetIsEditedViaReflection(this DataGridCell cell)
        {
            try
            {
                var property = _cellIsEditedProperty.Value;
                if (property != null)
                {
                    var result = property.GetValue(cell);
                    return result is bool index ? index : false;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the internal OwningRow property of the DataGridCell via reflection.
        /// </summary>
        /// <param name="cell">The DataGridCell instance.</param>
        /// <returns>The owning DataGridRow, or null if the property cannot be accessed.</returns>
        public static DataGridRow GetOwningRowViaReflection(this DataGridCell cell)
        {
            try
            {
                var property = _cellOwningRowProperty.Value;
                if (property != null)
                {
                    return property.GetValue(cell) as DataGridRow;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static DataGridColumn GetOwningColumnViaReflection(this DataGridCell cell)
        {
            try
            {
                var property = _columnCellOwningColumnProperty.Value;
                if (property != null)
                {
                    return property.GetValue(cell) as DataGridColumn;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the internal Slot property of the DataGridRow via reflection.
        /// </summary>
        /// <param name="row">The DataGridRow instance.</param>
        /// <returns>The slot index, or -1 if the property cannot be accessed.</returns>
        public static int GetSlotViaReflection(this DataGridRow row)
        {
            try
            {
                var property = _rowSlotProperty.Value;
                if (property != null)
                {
                    var result = property.GetValue(row);
                    return result is int slot ? slot : -1;
                }
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public static bool GetIsEditingViaReflection(this DataGridRow row)
        {
            try
            {
                var property = _rowIsEditingProperty.Value;
                if (property != null)
                {
                    var result = property.GetValue(row);
                    return result is bool slot ? slot : false;
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Gets the internal OwningColumn property of the DataGridColumnHeader via reflection.
        /// </summary>
        /// <param name="columnHeader">The DataGridColumnHeader instance.</param>
        /// <returns>The owning DataGridColumn, or null if the property cannot be accessed.</returns>
        public static DataGridColumn GetOwningColumnViaReflection(this DataGridColumnHeader columnHeader)
        {
            try
            {
                var property = _columnHeaderOwningColumnProperty.Value;
                if (property != null)
                {
                    return property.GetValue(columnHeader) as DataGridColumn;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the internal CurrentSlot property of the DataGrid via reflection.
        /// </summary>
        /// <param name="dataGrid">The DataGrid instance.</param>
        /// <returns>The current slot index, or -1 if the property cannot be accessed.</returns>
        public static int GetCurrentSlotViaReflection(this DataGrid dataGrid)
        {
            try
            {
                var property = _gridCurrentSlotProperty.Value;
                if (property != null)
                {
                    var result = property.GetValue(dataGrid);
                    return result is int slot ? slot : -1;
                }
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the internal CurrentColumnIndex property of the DataGrid via reflection.
        /// </summary>
        /// <param name="dataGrid">The DataGrid instance.</param>
        /// <returns>The current column index, or -1 if the property cannot be accessed.</returns>
        public static int GetCurrentColumnIndexViaReflection(this DataGrid dataGrid)
        {
            try
            {
                var property = _gridCurrentColumnIndexProperty.Value;
                if (property != null)
                {
                    var result = property.GetValue(dataGrid);
                    return result is int index ? index : -1;
                }
                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        /// <summary>
        /// Gets the internal CollectionView property from DataGrid's DataConnection via reflection.
        /// </summary>
        /// <param name="dataGrid">The DataGrid instance.</param>
        /// <returns>The IDataGridCollectionView, or null if the property cannot be accessed.</returns>
        public static IDataGridCollectionView GetDataGridCollectionViewViaReflection(this DataGrid dataGrid)
        {
            try
            {
                var dataConnectionProperty = _gridDataConnectionProperty.Value;
                if (dataConnectionProperty != null)
                {
                    var dataConnection = dataConnectionProperty.GetValue(dataGrid);
                    if (dataConnection != null)
                    {
                        var collectionViewProperty = _dataConnectionCollectionViewProperty.Value;
                        if (collectionViewProperty != null)
                        {
                            return collectionViewProperty.GetValue(dataConnection) as IDataGridCollectionView;
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the DataGridRow associated to the provided backend data item via reflection.
        /// </summary>
        /// <param name="dataGrid">The DataGrid instance.</param>
        /// <param name="dataItem">The backend data item.</param>
        /// <returns>The associated DataGridRow, or null if the DataSource is null, the provided item is not in the source, or the item is not displayed.</returns>
        public static DataGridRow GetRowFromItemViaReflection(this DataGrid dataGrid, object dataItem)
        {
            try
            {
                var method = _gridGetRowFromItemMethod.Value;
                if (method != null)
                {
                    return method.Invoke(dataGrid, new object[] { dataItem }) as DataGridRow;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Invokes the internal ProcessCopyKey method of the DataGrid via reflection.
        /// </summary>
        /// <param name="dataGrid">The DataGrid instance.</param>
        /// <param name="modifiers">The key modifiers (Ctrl, Shift, Alt) that were pressed.</param>
        /// <returns>True if the DataGrid handled the copy operation, false otherwise.</returns>
        public static bool ProcessCopyKeyViaReflection(this DataGrid dataGrid, KeyModifiers modifiers)
        {
            try
            {
                var method = _gridProcessCopyKeyMethod.Value;
                if (method != null)
                {
                    var result = method.Invoke(dataGrid, new object[] { modifiers });
                    return result is bool handled ? handled : false;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}