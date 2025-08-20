// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Data;
using System.Windows.Documents;
using gip.core.datamodel;

namespace gip.core.reporthandler.avui.Flowdoc
{
    /// <summary>
    /// Special event args for data row bound event
    /// </summary>
    public class PaginatorNewTableCellEventArgs : PaginatorNextRowEventArgs
    {
        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        public TableCell NewCell { get; set; }

        public object FieldValue { get; set; }

        public string FieldName { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="report">associated report document</param>
        /// <param name="row">DataRow object being processed</param>
        public PaginatorNewTableCellEventArgs(IACComponent acComponent, IACType propertyType, object data, ReportPaginator paginator, TableRow tableRow, TableCell newCell, object newValue, string fieldName) 
            : base(acComponent,propertyType,data,paginator,tableRow)
        {
            NewCell = newCell;
            FieldValue = newValue;
            FieldName = fieldName;
        }
    }
}
