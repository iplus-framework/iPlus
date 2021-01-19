namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for entity framework classes, that are displayed on a Items-Control (e.g. Datagrid) by a default sort-order.
    /// </summary>
    public interface ISequence
    {
        /// <summary>
        /// Database-Field taht is used as the default sort-order.
        /// </summary>
        /// <value>
        /// The sequence.
        /// </value>
        int Sequence { get; set; }
    }
}
