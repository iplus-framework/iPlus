// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Windows.Documents;
using System.Windows;
using System;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Class for fillable table row values
    /// </summary>
    public abstract class TableRowDataBase : TableRow, ITableRowData
    {
        private string _DictKey = null;
        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        public string DictKey
        {
            get { return _DictKey; }
            set { _DictKey = value; }
        }

        private string _VBSource = null;
        /// <summary>
        /// ACUrl to DataSource
        /// </summary>
        public string VBSource
        {
            get { return _VBSource; }
            set { _VBSource = value; }
        }

        private string _ColumnWidthInfos = null;
        /// <summary>
        /// 23.5;20.6;
        /// </summary>
        public string ColumnWidthInfos
        {
            get { return _ColumnWidthInfos; }
            set { _ColumnWidthInfos = value; }
        }
        private double[] _ColumnWidthValues = null;

        public double GetColumnWidth(int index)
        {
            if (_ColumnWidthValues == null)
            {
                if (String.IsNullOrEmpty(ColumnWidthInfos))
                    return -1;
                string[] values = ColumnWidthInfos.Split(';');
                if (values == null || values.Length <= 0)
                    return -1;
                _ColumnWidthValues = new double[values.Length];
                int i = 0;
                foreach (string value in values)
                {
                    try
                    {
                        _ColumnWidthValues[i] = System.Convert.ToDouble(value);
                    }
                    catch (Exception e)
                    {
                        string msg = e.Message;
                        if (e.InnerException != null && e.InnerException.Message != null)
                            msg += " Inner:" + e.InnerException.Message;

                        if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == datamodel.ACInitState.Initialized)
                            datamodel.Database.Root.Messages.LogException("TableRowDataBase", "GetColumnWidth", msg);
                    }
                    i++;
                }
            }
            if (index >= _ColumnWidthValues.Length)
                return -1;
            return _ColumnWidthValues[index];
        }


        public virtual string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public static readonly DependencyProperty StringFormatProperty = ReportDocument.StringFormatProperty.AddOwner(typeof(TableRowDataBase));

        public virtual int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }
        public static readonly DependencyProperty MaxLengthProperty = ReportDocument.MaxLengthProperty.AddOwner(typeof(TableRowDataBase));

        public virtual int Truncate
        {
            get { return (int)GetValue(TruncateProperty); }
            set { SetValue(TruncateProperty, value); }
        }
        public static readonly DependencyProperty TruncateProperty = ReportDocument.TruncateProperty.AddOwner(typeof(TableRowDataBase));

        public virtual string CultureInfo
        {
            get { return (string)GetValue(CultureInfoProperty); }
            set { SetValue(CultureInfoProperty, value); }
        }
        public static readonly DependencyProperty CultureInfoProperty = ReportDocument.CultureInfoProperty.AddOwner(typeof(TableRowDataBase));

        public virtual bool BreakPageBeforeNextRow
        {
            get { return (bool)GetValue(BreakPageBeforeNextRowProperty); }
            set { SetValue(BreakPageBeforeNextRowProperty, value); }
        }
        public static readonly DependencyProperty BreakPageBeforeNextRowProperty = DependencyProperty.Register("BreakPageBeforeNextRow", typeof(bool), typeof(TableRowDataBase));

    }
}