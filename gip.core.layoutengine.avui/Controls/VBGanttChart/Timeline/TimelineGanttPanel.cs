using Avalonia;
using Avalonia.Controls;
using gip.core.layoutengine.avui.ganttchart;
using gip.core.layoutengine.avui.timeline;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui
{

    /// <summary>
    /// Timeline Panel which arrange the items like Gantt chart -
    /// each item in a separated row.
    /// </summary>
    public class TimelineGanttPanel : TimelinePanel
    {
        public TimelineGanttPanel()
        {
        }

        private VBGanttChart _VBGanttChart
        {
            get 
            {
                return Helperclasses.VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBGanttChart)) as VBGanttChart;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // The inherited Timeline.MinimumDate/MaximumDate attached properties do NOT
            // reliably propagate through the nested ItemsControl/ItemsPresenter chain -
            // the panel keeps stale values (min == max -> totalTimeSpan = 0 ->
            // totalWidth = 0 -> nothing rendered). Like TimelineItemPanel, pull the
            // authoritative values from the owning chart on every measure pass.
            var chart = _VBGanttChart;
            if (chart != null)
            {
                if (chart.MinimumDate != MinimumDate)
                    MinimumDate = chart.MinimumDate;
                if (chart.MaximumDate != MaximumDate)
                    MaximumDate = chart.MaximumDate;
                if (chart.TickTimeSpan != TickTimeSpan)
                    TickTimeSpan = chart.TickTimeSpan;
            }

            // The item's StartDate/EndDate/RowIndex are bound to its Content (DataContext)
            // and typically resolve AFTER the first measure pass. Without observing the
            // children, the panel would keep measuring with NULL dates -> empty rects ->
            // invisible bars. ObserveChildren re-invalidates measure when a binding
            // delivers a value (same mechanism as TimelineItemPanel).
            ObserveChildren();

            rowsCount = -1;
            List<Control> measuredChildren = new List<Control>();
            Dictionary<Control, int> logicalToActualMap = new Dictionary<Control, int>();

            //Children.OfType<Control>().All(c => c.ApplyTemplate());

            var q =
                from c in Children.OfType<Control>()
                let row = GetRowIndex(c)
                orderby row
                select new { Child = c, Row = row };

            var test = q.Select(c => new Tuple<object, int>(((TimelineItem)c.Child).Content, c.Row));

            int nextActualRowIndex = 0;
            foreach (var childAndRow in q)
            {
                ((Control)childAndRow.Child).ApplyTemplate();
                ClearActualRowIndex(childAndRow.Child);
                childAndRow.Child.ClearValue(Control.IsVisibleProperty);

                TimelineGanttItem tlChild = (TimelineGanttItem)childAndRow.Child;

                tlChild.IsDisplayAsZero = ShouldDisplayAsZero(tlChild);
                tlChild.ApplyTemplate();

                Rect calcChildSize = CalcChildRect(tlChild, nextActualRowIndex);
                childAndRow.Child.Measure(calcChildSize.Size);

                if (tlChild.IsCollapsed)
                {
                    childAndRow.Child.IsVisible = false;
                }
                else
                {
                    int actualRowIndex;
                    if (!logicalToActualMap.TryGetValue(childAndRow.Child, out actualRowIndex))
                    {
                        actualRowIndex = nextActualRowIndex++;
                        logicalToActualMap.Add(childAndRow.Child, actualRowIndex);
                    }

                    SetActualRowIndex(childAndRow.Child, actualRowIndex);

                    //if (IsChildHasNoStartAndEndTime(childAndRow.Child))
                    //{
                    //    //childAndRow.Child.Visibility = Visibility.Hidden;
                    //}
                }
            }

            TimeSpan totalTimeSpan = TimeSpan.Zero;
            if (MaximumDate.HasValue && MinimumDate.HasValue)
            {
                totalTimeSpan = MaximumDate.Value - MinimumDate.Value;
            }
            double totalWidth = Math.Max(0, totalTimeSpan.Ticks * PixelsPerTick);
            double totalHeight = Math.Max(0, nextActualRowIndex * RowHeight + nextActualRowIndex * RowVerticalMargin);

            return totalWidth <= 0 || totalHeight <= 0 ? new Size() : new Size(totalWidth, totalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);
            return size;
        }

        private bool IsChildHasNoStartAndEndTime(Control child)
        {
            DateTime? childStartDate = GetStartDate(child);
            DateTime? childEndDate = GetEndDate(child);

            bool noTimes = childStartDate == null && childEndDate == null;
            return noTimes;
        }

        //protected virtual void SetChildRow(Control child, List<Control> measuredChildren)
        //{
        //}
    }
}
