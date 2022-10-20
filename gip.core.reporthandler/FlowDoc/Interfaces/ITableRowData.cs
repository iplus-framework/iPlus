namespace gip.core.reporthandler.Flowdoc
{
    /// <summary>
    /// Base-Interface for tables which are filled by binding to a datasource
    /// </summary>
    public interface ITableRowData : IDictRef
    {
        /// <summary>
        /// ACUrl to DataSource
        /// </summary>
        string VBSource { get; set; }
    }
}
