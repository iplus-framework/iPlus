using Avalonia;
using Avalonia.Controls;
using gip.ext.design.avui.UIExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.layoutengine.avui.timeline
{
    /// <summary>
    /// Represent the base panel for all timeline items panel.
    /// </summary>
    public abstract class TimelinePanel : Panel
    {

        static TimelinePanel()
        {
            MinimumDateProperty = Timeline.MinimumDateProperty.AddOwner<TimelinePanel>();
            MaximumDateProperty = Timeline.MaximumDateProperty.AddOwner<TimelinePanel>();
            TickTimeSpanProperty = Timeline.TickTimeSpanProperty.AddOwner<TimelinePanel>();
        }

        public TimelinePanel()
        {

        }

        #region StyledProperties and AttachedProperties

        public static readonly StyledProperty<DateTime?> MaximumDateProperty =
            AvaloniaProperty.Register<TimelinePanel, DateTime?>(nameof(MaximumDate));
        
        public static readonly StyledProperty<DateTime?> MinimumDateProperty =
            AvaloniaProperty.Register<TimelinePanel, DateTime?>(nameof(MinimumDate));
        
        public static readonly StyledProperty<TimeSpan> TickTimeSpanProperty =
            AvaloniaProperty.Register<TimelinePanel, TimeSpan>(nameof(TickTimeSpan));

        public static readonly AttachedProperty<DateTime?> StartDateProperty =
            AvaloniaProperty.RegisterAttached<TimelinePanel, AvaloniaObject, DateTime?>("StartDate");
        
        public static readonly AttachedProperty<DateTime?> EndDateProperty =
            AvaloniaProperty.RegisterAttached<TimelinePanel, AvaloniaObject, DateTime?>("EndDate");
        
        public static readonly AttachedProperty<int> RowIndexProperty =
            AvaloniaProperty.RegisterAttached<TimelinePanel, AvaloniaObject, int>("RowIndex");

        private static readonly AttachedProperty<int> ActualRowIndexPropertyKey =
            AvaloniaProperty.RegisterAttached<TimelinePanel, AvaloniaObject, int>("ActualRowIndex");

        public static readonly AttachedProperty<int> ActualRowIndexProperty = ActualRowIndexPropertyKey;

        public static int GetActualRowIndex(AvaloniaObject obj)
        {
            return obj.GetValue(ActualRowIndexProperty);
        }

        protected static void SetActualRowIndex(AvaloniaObject obj, int value)
        {
            obj.SetValue(ActualRowIndexPropertyKey, value);
        }

        protected static void ClearActualRowIndex(AvaloniaObject obj)
        {
            obj.ClearValue(ActualRowIndexPropertyKey);
        }

        public static DateTime? GetStartDate(AvaloniaObject obj)
        {
            // Return start time and if null end time.
            return obj.GetValue(StartDateProperty) ??
                   obj.GetValue(EndDateProperty);
        }

        public static void SetStartDate(AvaloniaObject obj, DateTime? value)
        {
            obj.SetValue(StartDateProperty, value);
        }

        public static DateTime? GetEndDate(AvaloniaObject obj)
        {
            // Return end time, and if null start time.
            return obj.GetValue(EndDateProperty) ??
                   obj.GetValue(StartDateProperty);
        }

        public static void SetEndDate(AvaloniaObject obj, DateTime? value)
        {
            obj.SetValue(EndDateProperty, value);
        }

        public static int GetRowIndex(AvaloniaObject obj)
        {
            return obj.GetValue(RowIndexProperty);
        }

        public static void SetRowIndex(AvaloniaObject obj, int value)
        {
            obj.SetValue(RowIndexProperty, value);
        }

        public Nullable<DateTime> MaximumDate
        {
            get { return GetValue(MaximumDateProperty); }
            set { SetValue(MaximumDateProperty, value); }
        }

        public Nullable<DateTime> MinimumDate
        {
            get { return GetValue(MinimumDateProperty); }
            set { SetValue(MinimumDateProperty, value); }
        }

        public int RowIndex
        {
            get { return GetValue(RowIndexProperty); }
            set { SetValue(RowIndexProperty, value); }
        }

        public TimeSpan TickTimeSpan
        {
            get { return GetValue(TickTimeSpanProperty); }
            set { SetValue(TickTimeSpanProperty, value); }
        }

        protected TimelineItem FindChildByDaya(object dep)
        {
            foreach (Control element in Children)
            {
                TimelineItem priorItem = element as TimelineItem;
                if (priorItem != null)
                {
                    if (object.Equals(priorItem.DataContext, dep)) return priorItem;
                }
            }

            return null;
        }

        protected double PixelsPerTick
        {
            get
            {
                return Timeline.GetPixelsPerTick(this);
            }
        }

        /// <summary>
        /// Represents the styled property for RowHeight.
        /// </summary>
        public static readonly StyledProperty<double> RowHeightProperty =
            AvaloniaProperty.Register<TimelinePanel, double>(nameof(RowHeight), 18D);

        /// <summary>
        /// Get or set the height of each row in the timeline panel
        /// </summary>
        public double RowHeight
        {
            get { return GetValue(RowHeightProperty); }
            set { SetValue(RowHeightProperty, value); }
        }

        /// <summary>
        /// Represents the styled property for RowVerticalMargin.
        /// </summary>
        public static readonly StyledProperty<double> RowVerticalMarginProperty =
            AvaloniaProperty.Register<TimelinePanel, double>(nameof(RowVerticalMargin), 5D);

        /// <summary>
        /// Get or set the vertical margin between two rows.
        /// </summary>
        public double RowVerticalMargin
        {
            get { return GetValue(RowVerticalMarginProperty); }
            set { SetValue(RowVerticalMarginProperty, value); }
        }

        protected double MinimumDisplayMultiplier
        {
            get { return 0.5; }
        }

        #endregion

        protected int rowsCount;

        protected int RowsCount
        {
            get { return rowsCount; }
            set { rowsCount = value; }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (Control child in Children)
            {
                ArrangeChild(child);
            }

            return finalSize;
        }

        protected virtual void ArrangeChild(Control child)
        {
            int rowIndex = GetActualRowIndex(child);
            ArrangeChild(child, rowIndex);
        }

        protected virtual void ArrangeChild(Control child, int rowIndex)
        {
            Rect childRect = CalcChildRect((TimelineItemBase)child, rowIndex);
            if (!childRect.IsEmpty())
            {
                child.Arrange(childRect);
            }
        }

        protected Rect CalcChildRect(TimelineItemBase child, int childRowIndex)
        {

            if (/*child.Visibility == Visibility.Collapsed || */child.IsCollapsed)
                return RectExtensions.Empty();

            Rect desiredSize;
            if (child.IsDisplayAsZero)
                desiredSize = CalcChildRectForDisplayAsZero(child, childRowIndex);
            else
                desiredSize = CalcChildRectByDuration(child, childRowIndex);

            return desiredSize;
        }

        protected bool ShouldDisplayAsZero(TimelineItemBase child)
        {
            DateTime? childStartDate = GetStartDate(child);
            DateTime? childEndDate = GetEndDate(child);

            bool displayAsZero;
            if (!(childStartDate.HasValue && childEndDate.HasValue))
            {
                displayAsZero = false;
            }
            else if (childStartDate.HasValue && childEndDate.HasValue)
            {
                TimeSpan duration = childEndDate.Value - childStartDate.Value;
                displayAsZero = (duration.Ticks <
                    TickTimeSpan.Ticks * MinimumDisplayMultiplier);
            }
            else
            {
                // Only start or end time exist, display as zero.
                displayAsZero = true;
            }

            return displayAsZero;
        }

        private Rect CalcChildRectForDisplayAsZero(TimelineItemBase child, int childRowIndex)
        {
            DateTime? childStartDate = GetStartDate(child);
            DateTime? childEndDate = GetEndDate(child);
            DateTime centerDate;

            if (MinimumDate == null || MaximumDate == null)
            {
                return RectExtensions.Empty();
            }

            if (!(childStartDate.HasValue && childEndDate.HasValue))
            {
                // Patch?
                centerDate = MinimumDate.Value;
            }
            //else if (childStartDate.HasValue && childEndDate.HasValue)
            //{
            //    TimeSpan duration = childEndDate.Value - childStartDate.Value;
            //    centerDate = childStartDate.Value.Add(
            //        TimeSpan.FromTicks(duration.Ticks / 2));
            //}
            else if (childStartDate.HasValue)
            {
                centerDate = childStartDate.Value;
            }
            else if (childEndDate.HasValue)
            {
                centerDate = childEndDate.Value;
            }
            else
            {
                throw new Exception("Invalid state of TimelineItem values");
            }

            double offset = (centerDate - MinimumDate.Value).Ticks * PixelsPerTick;
            double width = 20;
            double childTopOffset = childRowIndex * RowHeight +
                childRowIndex * RowVerticalMargin;

            return new Rect(offset, childTopOffset, width, RowHeight);

        }

        private Rect CalcChildRectByDuration(TimelineItemBase child, int childRowIndex)
        {
            if (GetStartDate(child) == null || GetEndDate(child) == null || MinimumDate == null)
            {
                return RectExtensions.Empty();
            }

            DateTime childStartDate = GetStartDate(child) ?? MinimumDate.Value;
            DateTime childEndDate = GetEndDate(child) ?? MinimumDate.Value;

            if (childEndDate < childStartDate)
            {
                childEndDate = childStartDate;
            }
            TimeSpan childDuration = childEndDate - childStartDate;

            double offset = (childStartDate - MinimumDate.Value).Ticks * PixelsPerTick;
            double width = childDuration.Ticks * PixelsPerTick;
            double childTopOffset = childRowIndex * RowHeight +
                childRowIndex * RowVerticalMargin;

            return new Rect(offset, childTopOffset, width, RowHeight);
        }

    }
}
