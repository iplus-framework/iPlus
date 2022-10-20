using System;
using System.Data;

namespace gip.core.reporthandler.Flowdoc
{
    /// <summary>
    /// Interface for a chart object
    /// </summary>
    public interface IChart : ICloneable
    {
        /// <summary>
        /// Gets or sets the table columns which are used to draw the chart
        /// </summary>
        string TableColumns { get; set; }

        /// <summary>
        /// Gets or sets the table name containing the data to be drawn
        /// </summary>
        string TableName { get; set; }

        /// <summary>
        /// Gets or sets the data columns which are used to draw the chart
        /// </summary>
        string[] DataColumns { get; set; }

        /// <summary>
        /// Data view to be used to draw the data
        /// </summary>
        DataView DataView { get; set; }

        /// <summary>
        /// Updates the chart to use the chart data
        /// </summary>
        void UpdateChart();
    }
}
