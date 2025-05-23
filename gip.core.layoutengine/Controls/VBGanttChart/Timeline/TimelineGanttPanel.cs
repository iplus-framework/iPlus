﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using gip.core.layoutengine.ganttchart;
using gip.core.layoutengine.timeline;

namespace gip.core.layoutengine
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
                return WpfUtility.FindVisualParent<VBGanttChart>(this);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            rowsCount = -1;
            List<UIElement> measuredChildren = new List<UIElement>();
            Dictionary<UIElement, int> logicalToActualMap = new Dictionary<UIElement, int>();

            Children.OfType<FrameworkElement>().All(c => c.ApplyTemplate());

            var q =
                from c in Children.OfType<UIElement>()
                let row = GetRowIndex(c)
                orderby row
                select new { Child = c, Row = row };

            var test = q.Select(c => new Tuple<object, int>(((TimelineItem)c.Child).Content, c.Row));

            int nextActualRowIndex = 0;
            foreach (var childAndRow in q)
            {
                ((FrameworkElement)childAndRow.Child).ApplyTemplate();
                ClearActualRowIndex(childAndRow.Child);
                childAndRow.Child.ClearValue(UIElement.VisibilityProperty);

                TimelineGanttItem tlChild = (TimelineGanttItem)childAndRow.Child;

                tlChild.IsDisplayAsZero = ShouldDisplayAsZero(tlChild);
                tlChild.ApplyTemplate();

                Rect calcChildSize = CalcChildRect(tlChild, nextActualRowIndex);
                childAndRow.Child.Measure(calcChildSize.Size);

                if (tlChild.IsCollapsed)
                {
                    childAndRow.Child.Visibility = Visibility.Collapsed;
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

                    //SetChildRow(childAndRow.Child, measuredChildren);
                    //measuredChildren.Add(childAndRow.Child);

                    if (IsChildHasNoStartAndEndTime(childAndRow.Child))
                    {
                        //childAndRow.Child.Visibility = Visibility.Hidden;
                    }
                }
            }

            TimeSpan totalTimeSpan = TimeSpan.Zero;
            if (MaximumDate.HasValue && MinimumDate.HasValue)
            {
                totalTimeSpan = MaximumDate.Value - MinimumDate.Value;
            }
            double totalWidth = Math.Max(0, totalTimeSpan.Ticks * PixelsPerTick);
            double totalHeight = Math.Max(0, nextActualRowIndex * RowHeight + nextActualRowIndex * RowVerticalMargin);

            return totalWidth <= 0 || totalHeight <= 0 ? Size.Empty : new Size(totalWidth, totalHeight);
        }

        private bool IsChildHasNoStartAndEndTime(UIElement child)
        {
            DateTime? childStartDate = GetStartDate(child);
            DateTime? childEndDate = GetEndDate(child);

            bool noTimes = childStartDate == null && childEndDate == null;
            return noTimes;
        }

        protected virtual void SetChildRow(UIElement child, List<UIElement> measuredChildren)
        {

        }
    }
}
