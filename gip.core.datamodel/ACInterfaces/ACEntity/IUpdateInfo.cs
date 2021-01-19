using System;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for entity framework classes, that stores information about the updateting-event.
    /// </summary>
    public interface IUpdateInfo
    {
        /// <summary>
        /// Name of the user who manipulated this record the last time.
        /// </summary>
        /// <value>
        /// Not null
        /// </value>
        string UpdateName { get; set; }

        /// <summary>
        /// Date when this record was manipulated the last time.
        /// </summary>
        /// <value>
        /// DateTime
        /// </value>
        DateTime UpdateDate { get; set; }
    }
}
