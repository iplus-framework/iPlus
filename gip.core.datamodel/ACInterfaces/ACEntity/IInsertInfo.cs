using System;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for entity framework classes, that stores information about the inserting-event.
    /// </summary>
    public interface IInsertInfo
    {
        /// <summary>
        /// Name of the user who added this record to the database.
        /// </summary>
        /// <value>
        /// Not null
        /// </value>
        string InsertName { get; set; }


        /// <summary>
        /// Date when this record was added to the database.
        /// </summary>
        /// <value>
        /// DateTime
        /// </value>
        DateTime InsertDate { get; set; }
    }
}
