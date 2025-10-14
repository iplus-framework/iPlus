using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.ComponentModel;
using System.Collections;
using System.Data;
using System.Linq;

namespace gip.core.layoutengine.avui
{

    public partial class VBDataGrid
    {
        #region Sum
        /// <summary>
        /// Shows or hides summary row in datagrid.
        /// </summary>
        [ACMethodInteraction("", "en{'Sum'}de{'Summe'}", (short)100, false)]
        public void ShowSum()
        {
            if (!SumVisibility)
            {
                SumVisibility = true;
                CalculateSum();
            }
            else
                SumVisibility = false;
        }

        /// <summary>
        /// Show or hide Sum in context menu.
        /// </summary>
        public bool IsEnabledShowSum()
        {
            if (IsSumEnabled || !string.IsNullOrEmpty(VBSumColumns))
                return true;
            else
                return false;
        }

        /// <summary>
        /// This method creates properties for temporarily save summary values. 
        /// </summary>
        private void CreateSumProperties()
        {
            DictionarySumProperties = new VBDataGridSumDictionary();

            if (string.IsNullOrEmpty(VBSumColumns) || string.IsNullOrWhiteSpace(VBSumColumns))
            {
                foreach (IGriColumn column in Columns)
                {
                    if (column.VBContent != null && !DictionarySumProperties.ContainsKey(column.VBContent))
                        DictionarySumProperties.Add(column.VBContent, "0");
                }
            }
            else
            {
                _SumColumns = VBSumColumns.Split(new char[] { ',' });
                foreach (string column in _SumColumns)
                {
                    DictionarySumProperties.Add(column, "0");
                }
            }
        }

        /// <summary>
        /// This method calculate sum for each column and put it in dictionary.
        /// </summary>
        private void CalculateSum()
        {
            IDataGridCollectionView collectionView = this.GetDataGridCollectionViewViaReflection();
            if (collectionView == null)
            {
                foreach (IGriColumn column in Columns)
                {
                    if (_SumColumns != null)
                    {
                        foreach (string columnName in _SumColumns)
                        {
                            if (column.VBContent == columnName)
                            {
                                DictionarySumProperties[columnName] = "";
                            }
                            else
                            {
                                string header = ((DataGridColumn)column)?.Header?.ToString();
                                if (header == columnName)
                                    DictionarySumProperties[header] = "";
                            }
                        }
                    }
                }
                return;
            }

            if (collectionView == null)
                return;

            if (string.IsNullOrEmpty(VBSumColumns) || string.IsNullOrWhiteSpace(VBSumColumns))
            {
                foreach (IGriColumn column in Columns)
                {
                    SumAndCheckType(column, collectionView);
                }
            }
            else
            {
                foreach (IGriColumn column in Columns)
                {
                    if (_SumColumns != null)
                    {
                        foreach (string columnName in _SumColumns)
                        {
                            if (column.VBContent == columnName)
                            {
                                SumAndCheckType(column, collectionView);
                            }
                            else if (((DataGridColumn)column)?.Header as string == columnName)
                            {
                                DictionarySumProperties[((DataGridColumn)column).Header.ToString()] = "NaN";
                            }
                        }
                    }
                }
            }
        }

        private void SumAndCheckType(IGriColumn column, IDataGridCollectionView collectionView)
        {
            if (column == null || collectionView == null)
                return;

            string stringFormat = "";
            if (column.VBContent != null && column.ColACType != null)
            {
                if (DictionarySumProperties == null)
                    return;
                if (column is VBDataGridTextColumn)
                    stringFormat = ((VBDataGridTextColumn)column).StringFormat;

                if (column.ColACType.ObjectType == typeof(double))
                {
                    double _Total = 0;
                    foreach (var item in collectionView)
                    {
                        object itemValue = item.GetValue(column.VBContent);
                        if (itemValue != null && itemValue is double)
                            _Total += (double)itemValue;
                    }
                    if (DictionarySumProperties.ContainsKey(column.VBContent))
                        DictionarySumProperties[column.VBContent] = _Total.ToString(stringFormat);
                }
                else if (column.ColACType.ObjectType == typeof(float))
                {
                    float _Total = 0;
                    foreach (var item in collectionView)
                    {
                        object itemValue = item.GetValue(column.VBContent);
                        if (itemValue != null && itemValue is float)
                            _Total += (float)itemValue;
                    }
                    if (DictionarySumProperties.ContainsKey(column.VBContent))
                        DictionarySumProperties[column.VBContent] = _Total.ToString(stringFormat);
                }
                else if (column.ColACType.ObjectType == typeof(decimal))
                {
                    decimal _Total = 0;
                    foreach (var item in collectionView)
                    {
                        object itemValue = item.GetValue(column.VBContent);
                        if (itemValue != null && itemValue is decimal)
                            _Total += (decimal)itemValue;
                        if (DictionarySumProperties.ContainsKey(column.VBContent))
                            DictionarySumProperties[column.VBContent] = _Total.ToString(stringFormat);
                    }
                }
                else if (column.ColACType.ObjectType == typeof(int))
                {
                    int _Total = 0;
                    foreach (var item in collectionView)
                    {
                        object itemValue = item.GetValue(column.VBContent);
                        if (itemValue != null && itemValue is int)
                            _Total += (int)itemValue;
                    }
                    if (DictionarySumProperties.ContainsKey(column.VBContent))
                        DictionarySumProperties[column.VBContent] = _Total.ToString(stringFormat);
                }
                else if (DictionarySumProperties.ContainsKey(column.VBContent))
                    DictionarySumProperties[column.VBContent] = "";
            }
        }


        #endregion
    }
}
