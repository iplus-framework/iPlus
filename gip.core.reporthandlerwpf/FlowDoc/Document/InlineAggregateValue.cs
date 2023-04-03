using System;
using System.Collections.Generic;
using System.Windows;

namespace gip.core.reporthandlerwpf.Flowdoc
{
    /// <summary>
    /// Computes a single aggregate report value that is to be displayed on the report (e.g. report title)
    /// </summary>
    /// <summary xml:lang="de">
    /// Berechnet einen einzelnen aggregierten Berichtswert, der auf dem Bericht angezeigt werden soll (z.B. Berichtstitel).
    /// </summary>
    public class InlineAggregateValue : InlineValueBase
    {
        private string _aggregateGroup = null;
        /// <summary>
        /// Gets or sets the aggregate group
        /// </summary>
        public string AggregateGroup
        {
            get { return _aggregateGroup; }
            set { _aggregateGroup = value; }
        }

        private ReportAggregateValueType _aggregateValueType = ReportAggregateValueType.Count;
        /// <summary>
        /// Gets or sets the report value aggregate type
        /// </summary>
        public ReportAggregateValueType AggregateValueType
        {
            get { return _aggregateValueType; }
            set { _aggregateValueType = value; }
        }

        private string _emptyValue = "";
        /// <summary>
        /// Gets or sets the value which is shown if the computation has no result
        /// </summary>
        public string EmptyValue
        {
            get { return _emptyValue; }
            set { _emptyValue = value; }
        }

        private string _errorValue = "!ERROR!";
        /// <summary>
        /// Gets or sets the value which is shown if the computation fails
        /// </summary>
        public string ErrorValue
        {
            get { return _errorValue; }
            set { _errorValue = value; }
        }

        /// <summary>
        /// Computes an aggregate value and formats it
        /// </summary>
        /// <param name="values">list of values</param>
        /// <returns>calculated and formatted value</returns>
        /// <exception cref="NotSupportedException">The aggregate value type {0} is not supported yet!</exception>
        public string ComputeAndFormat(Dictionary<string, List<object>> values)
        {
            if ((values == null) || (values.Count <= 0))
                return _emptyValue;
            if (!values.ContainsKey(_aggregateGroup))
                return _emptyValue;

            decimal? result = null;
            bool isTimeSpan = false;
            long count = 0;
            foreach (object value in values[_aggregateGroup])
            {
                count++;
                if (_aggregateValueType == ReportAggregateValueType.Count)
                    continue; // count needs no real calculation

                decimal thisValue;
                if (value == null)
                    return _errorValue;
                if (value is TimeSpan)
                {
                    thisValue = Convert.ToDecimal(((TimeSpan)value).Ticks);
                    isTimeSpan = true;
                }
                else
                {
                    if (!Decimal.TryParse(value.ToString(), out thisValue))
                        return _errorValue;
                }
                switch (_aggregateValueType)
                {
                    case ReportAggregateValueType.Average:
                    case ReportAggregateValueType.Sum:
                        if (result == null)
                        {
                            result = thisValue;
                            break;
                        }
                        result += thisValue;
                        break;
                    case ReportAggregateValueType.Maximum:
                        if (result == null) { result = thisValue; break; }
                        if (thisValue > result) result = thisValue;
                        break;
                    case ReportAggregateValueType.Minimum:
                        if (result == null) { result = thisValue; break; }
                        if (thisValue < result) result = thisValue;
                        break;
                    default:
                        throw new NotSupportedException(String.Format("The aggregate value type {0} is not supported yet!", _aggregateValueType.ToString()));
                }
            }
            if (_aggregateValueType == ReportAggregateValueType.Count)
                result = count;
            if (result == null)
                return _emptyValue;

            if (_aggregateValueType == ReportAggregateValueType.Average)
                result /= count; // calculate average

            if (isTimeSpan) return TimeSpan.FromTicks(Convert.ToInt64(result)).ToString();  //for timespans

            return InlineValueBase.FormatValue(result, StringFormat, CultureInfo, MaxLength, Truncate);
        }
    }

    /// <summary>
    /// Enumeration of available aggregate types
    /// </summary>
    public enum ReportAggregateValueType
    {
        /// <summary>
        /// Computes the average value
        /// </summary>
        Average,
        /// <summary>
        /// Gets the values count
        /// </summary>
        Count,
        /// <summary>
        /// Determines the maximum value
        /// </summary>
        Maximum,
        /// <summary>
        /// Determines the minimum value
        /// </summary>
        Minimum,
        /// <summary>
        /// Computes the sum over all values
        /// </summary>
        Sum
    }
}
