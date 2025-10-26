using Avalonia.Media;
using gip.core.datamodel;
using System;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the item in VBPropertyLogChart.
    /// </summary>
    public class VBPropertyLogChartItem : VBChartItem, IPropertyLogChartItem
    {
        /// <summary>
        /// Creates a new instance of VBPropertyLogChartItem.
        /// </summary>
        public VBPropertyLogChartItem() : base()
        {
        }

        public override ChartItem ChartItem
        {
            get
            {
                if (_ChartItem == null)
                    _ChartItem = new PropertyLogChartItem();
                return _ChartItem;
            }
        }


        /// <summary>
        /// Get the list of live log.
        /// </summary>
        /// <returns>Returns the live log list.</returns>
        public PropertyLogListInfo GetLiveLogList()
        {
            return (ChartItem as PropertyLogChartItem).GetLiveLogList();
        }

        /// <summary>
        /// Gets the list of archive log.
        /// </summary>
        /// <param name="from">The archive log from.</param>
        /// <param name="to">The archive log to.</param>
        /// <returns>Return the archive log list.</returns>
        public PropertyLogListInfo GetArchiveLogList(DateTime from, DateTime to)
        {
            return (ChartItem as PropertyLogChartItem).GetArchiveLogList(from, to);
        }
    }

    /// <summary>
    /// Represents the extension for VBPropertyLogChartItem.
    /// </summary>
    public static class VBPropertyLogChartItemExtension
    {
        /// <summary>
        /// Gets the line color.
        /// </summary>
        /// <param name="chartItem">The chart item.</param>
        /// <returns>Returns the color of line.</returns>
        public static Color GetLineColor(this IPropertyLogChartItem chartItem)
        {
            if (String.IsNullOrEmpty(chartItem.LineColor))
            {
                Color color = VBChartItemExtension.GetRandomColor();
                chartItem.LineColor = color.ToString();
                return color;
            }
            else
            {
                return (Color)Color.Parse(chartItem.LineColor);
            }
        }
    }
}
