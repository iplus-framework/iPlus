using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui.timeline
{
    public class Timeline : AvaloniaObject
    {
        public static Nullable<DateTime> GetMinimumDate(AvaloniaObject obj)
        {
            return obj.GetValue(MinimumDateProperty);
        }

        public static void SetMinimumDate(AvaloniaObject obj, Nullable<DateTime> value)
        {
            obj.SetValue(MinimumDateProperty, value);
        }

        // Using an AttachedProperty as the backing store for MinimumDate. This enables animation, styling, binding, etc...
        public static readonly AttachedProperty<Nullable<DateTime>> MinimumDateProperty =
            AvaloniaProperty.RegisterAttached<Timeline, AvaloniaObject, Nullable<DateTime>>(
                "MinimumDate", 
                defaultValue: null, 
                inherits: true);

        public static Nullable<DateTime> GetMaximumDate(AvaloniaObject obj)
        {
            return obj.GetValue(MaximumDateProperty);
        }

        public static void SetMaximumDate(AvaloniaObject obj, Nullable<DateTime> value)
        {
            obj.SetValue(MaximumDateProperty, value);
        }

        // Using an AttachedProperty as the backing store for MaximumDate. This enables animation, styling, binding, etc...
        public static readonly AttachedProperty<Nullable<DateTime>> MaximumDateProperty =
            AvaloniaProperty.RegisterAttached<Timeline, AvaloniaObject, Nullable<DateTime>>(
                "MaximumDate", 
                defaultValue: null, 
                inherits: true);

        public static readonly TimeSpan TickTimeSpanDefaultValue = TimeSpan.FromDays(1);

        public static TimeSpan GetTickTimeSpan(AvaloniaObject obj)
        {
            return obj.GetValue(TickTimeSpanProperty);
        }

        public static void SetTickTimeSpan(AvaloniaObject obj, TimeSpan value)
        {
            obj.SetValue(TickTimeSpanProperty, value);
        }

        // Using an AttachedProperty as the backing store for TickTimeSpan. This enables animation, styling, binding, etc...
        public static readonly AttachedProperty<TimeSpan> TickTimeSpanProperty =
            AvaloniaProperty.RegisterAttached<Timeline, AvaloniaObject, TimeSpan>(
                "TickTimeSpan", 
                defaultValue: TickTimeSpanDefaultValue, 
                inherits: true);

        public static double GetPixelsPerTick(AvaloniaObject obj)
        {
            TimeSpan tickTimeSpan = GetTickTimeSpan(obj);
            if (tickTimeSpan.Ticks == 0) return 1;
            return ((double)60 / tickTimeSpan.Ticks);
        }

        public static double DateToOffset(DateTime current, AvaloniaObject owner)
        {
            Nullable<DateTime> min = GetMinimumDate(owner);
            Nullable<DateTime> max = GetMaximumDate(owner);
            double pixelsPerTick = GetPixelsPerTick(owner);

            if (min == null || max == null)
            {
                return -1;
            }

            else if (current < min || current > max)
            {
                return -1;
            }

            double offset = (current - min.Value).Ticks * pixelsPerTick;
            return offset;
        }

        public static DateTime OffsetToDate(double offset, AvaloniaObject owner)
        {
            Nullable<DateTime> min = GetMinimumDate(owner);
            Nullable<DateTime> max = GetMaximumDate(owner);
            double pixelsPerTick = GetPixelsPerTick(owner);

            if (min == null || max == null)
            {
                return DateTime.MinValue;
            }

            DateTime date = min.Value.AddTicks((long)(offset / pixelsPerTick));

            if (date < min)
            {
                return DateTime.MinValue;
            }
            if (date > max)
                return max.Value;

            return date;
        }
    }
}
