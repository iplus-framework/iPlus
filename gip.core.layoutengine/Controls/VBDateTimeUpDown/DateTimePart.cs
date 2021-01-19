using System;

namespace gip.core.layoutengine
{
    /// <summary>
    /// The date and time part.
    /// </summary>
    /// <summary>
    /// Der Datums- und Zeitteil.
    /// </summary>
    public enum DateTimePart : short
    {
        Day,
        DayName,
        AmPmDesignator,
        Millisecond,
        Hour12,
        Hour24,
        Minute,
        Month,
        MonthName,
        Other,
        Period,
        TimeZone,
        Second,
        Year
    }
}
