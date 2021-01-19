using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using gip.core.datamodel;
using gip.core.layoutengine.timeline;

namespace gip.core.layoutengine.timeline
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
                return WpfUtility.FindVisualParent<VBTimelineChart>(this);
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


            int lastItemRowIndex = 0, nextActualRowIndex = 0;

            foreach (var childAndRow in q)
            {
                ((FrameworkElement)childAndRow.Child).ApplyTemplate();
                ClearActualRowIndex(childAndRow.Child);
                childAndRow.Child.ClearValue(UIElement.VisibilityProperty);

                TimelineItem tlChild = (TimelineItem)childAndRow.Child;

                tlChild.IsDisplayAsZero = false; // ShouldDisplayAsZero(tlChild);
                tlChild.ApplyTemplate();

                Rect calcChildSize = CalcChildRect(tlChild, nextActualRowIndex);
                childAndRow.Child.Measure(calcChildSize.Size);

                if (tlChild.IsCollapsed)
                    childAndRow.Child.Visibility = Visibility.Collapsed;
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

            return totalWidth <= 0 || totalHeight <= 0 ? Size.Empty : new Size(totalWidth, totalHeight);
        }

        private bool IsChildHasNoStartAndEndTime(UIElement child)
        {
            DateTime? childStartDate = GetStartDate(child);
            DateTime? childEndDate = GetEndDate(child);

            bool noTimes = childStartDate == null && childEndDate == null;
            return noTimes;
        }
    }
}
