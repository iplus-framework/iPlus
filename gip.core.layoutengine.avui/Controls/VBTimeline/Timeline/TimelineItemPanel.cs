using Avalonia;
using Avalonia.Controls;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace gip.core.layoutengine.avui.timeline
{
    public class TimelineItemPanel : TimelinePanel
    {
        public TimelineItemPanel()
        {
            Margin = new Thickness(0, 0, 0, 22);
        }

        private VBTimelineChart _VBTimelineChart
        {
            get
            {
                return VBVisualTreeHelper.FindParentObjectInVisualTree(this, typeof(VBTimelineChart)) as VBTimelineChart;
            }
        }
       
        protected override Size MeasureOverride(Size availableSize)
        {
            rowsCount = -1;
            List<Control> measuredChildren = new List<Control>();
            Dictionary<Control, int> logicalToActualMap = new Dictionary<Control, int>();

            //Children.OfType<Control>().All(c => c.ApplyTemplate());

            var q =
                from c in Children.OfType<Control>()
                let row = GetRowIndex(c)
                orderby row
                select new { Child = c, Row = row };


            int lastItemRowIndex = 0, nextActualRowIndex = 0;

            foreach (var childAndRow in q)
            {
                ((Control)childAndRow.Child).ApplyTemplate();
                ClearActualRowIndex(childAndRow.Child);
                childAndRow.Child.ClearValue(Control.IsVisibleProperty);

                TimelineItem tlChild = (TimelineItem)childAndRow.Child;

                tlChild.IsDisplayAsZero = false; // ShouldDisplayAsZero(tlChild);
                tlChild.ApplyTemplate();

                Rect calcChildSize = CalcChildRect(tlChild, nextActualRowIndex);
                childAndRow.Child.Measure(calcChildSize.Size);

                if (tlChild.IsCollapsed)
                    childAndRow.Child.IsVisible = false;
                else
                {
                    if(childAndRow.Row > lastItemRowIndex)
                    {
                        nextActualRowIndex++;
                        lastItemRowIndex = childAndRow.Row;
                    }
                    SetActualRowIndex(childAndRow.Child, nextActualRowIndex);
                }
            }

            TimeSpan totalTimeSpan = TimeSpan.Zero;
            if (MaximumDate.HasValue && MinimumDate.HasValue)
                totalTimeSpan = MaximumDate.Value - MinimumDate.Value;

            double totalWidth = Math.Max(0, totalTimeSpan.Ticks * PixelsPerTick);
            double totalHeight = Math.Max(0.0000001,
                nextActualRowIndex * RowHeight + nextActualRowIndex * RowVerticalMargin);

            return totalWidth <= 0 || totalHeight <= 0 ? new Size() : new Size(totalWidth, totalHeight);
        }

        private bool IsChildHasNoStartAndEndTime(Control child)
        {
            DateTime? childStartDate = GetStartDate(child);
            DateTime? childEndDate = GetEndDate(child);

            bool noTimes = childStartDate == null && childEndDate == null;
            return noTimes;
        }
    }
}
