using System;
using System.Data;
using System.Windows.Documents;
using gip.core.datamodel;

namespace gip.core.reporthandler.Flowdoc
{
    /// <summary>
    /// Special event args for data row bound event
    /// </summary>
    public class PaginatorNextRowEventArgs : PaginatorEventArgs
    {
        /// <summary>
        /// Gets the DataRow object being processed
        /// </summary>
        public object DataRow { get; protected set; }

        public IACComponent ACComponent  { get; protected set; }

        public IACType PropertyType { get; protected set; }

        /// <summary>
        /// Gets or sets the table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the newly created table row
        /// </summary>
        public TableRow TableRow { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="report">associated report document</param>
        /// <param name="row">DataRow object being processed</param>
        public PaginatorNextRowEventArgs(IACComponent acComponent, IACType propertyType, object data, ReportPaginator paginator, TableRow tableRow)
        {
            _Paginator = paginator;
            ACComponent = acComponent;
            PropertyType = propertyType;
            this.DataRow = data;
            if (this.DataRow is DataRow)
            {
                DataRow dataRow = this.DataRow as DataRow;
                TableName = dataRow.Table.TableName;
            }
            TableRow = tableRow;
        }
    }
}
